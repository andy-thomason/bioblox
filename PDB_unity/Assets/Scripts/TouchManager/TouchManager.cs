using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TouchManager {
	
	struct Finger {	
		public int protein;
		public int touchId; // last touch id
		// the position of the hit transform in camera space
		// useful as holds the distance z of the transform from the camera
		public Vector3 cameraSpaceHitTransformPos; 
		// the screen touch in the hit transform space
		// or else the distance between the screen touch in world space and the hit transform
		// the z from cameraSpaceHitTransformPos used to get the screen touch in world space
		public Vector3 worldSpaceHitTransformOffset; // the offset of the touch
		public Ray r; // a ray used only for pinching
		public Vector2 screenPos; // starting screen position
		public int pending; // for pending fingers TODO: boolean in the future, now int for debugging
	};
	
	enum Gestures { None, SingleFinger, TwoFinger };
	
	enum TwoFingerModes { None, RotateXY, RotateZ, Pinch };
	
	enum GestureModes { Single, Dual };

	class TouchManager : MonoBehaviour {

		// Bioblox
		// These added to make the script more general without using any names or tags
		// and without adding functionality wrt to atoms
		// There are two cameras to be able to change the near clipping plane without
		// clipping the terrain
		public GameObject receptor, ligand;
		public Camera mainCamera, proteinCamera;
		public float ligRadius, recRadius;

		public GameObject uiCanvasPrefab, uiEventSystemPrefab, uiButtonPrefab, uiSliderPrefab;
		public Sprite gestureModeLockedImage, gestureModeUnlockedImage;

		Image gestureModeButtonImage;
		GameObject nearClipPlaneSlider;

		float centreRadius;

		Transform[] proteins = new Transform[3];
		Vector3[] centreSpaceProteinsPos = new Vector3[2];
		Quaternion[] initialProteinsRot = new Quaternion[2];
		Vector3[] initialProteinsPos = new Vector3[2];
		Vector2[] unitsPerPixel = new Vector2[3];
		float[] degreesPerUnit = new float[3];

		float minTurnAngle = 5f;
		float minDistanceDelta = 5;

		GestureModes gm = GestureModes.Single;

		bool[] updateT = new bool[2];
		int[] prevNumFingers = new int[2];
		bool[] zChanged = new bool[2];
		TwoFingerModes[] tfms = new TwoFingerModes[2];

		Finger[] fingers = new Finger[10];

		HashSet<int>[] activeFingerIds = { new HashSet<int> (), new HashSet<int> () };

		private RaycastHit hitInfo = new RaycastHit();
		private float maxPickingDistance = 1000;
		LayerMask touchInputMask;

		void visualizeVector(Vector3 pos, Color col, Transform t) {
			LineRenderer lr = t.gameObject.GetComponent<LineRenderer> ();
			if (lr == null) {
				//Shader shader = Shader.Find("MyShader/VertexColor");
				Shader shader = Shader.Find("Transparent/Diffuse");
				Material mat = new Material(shader);
				mat.color = Color.red;
				lr = t.gameObject.AddComponent<LineRenderer>();
				lr.useWorldSpace = true;
				lr.material = mat;
				lr.SetColors (col, col);
				lr.SetWidth (0.2f, 0.2f);
			}
			lr.SetVertexCount(3);
			lr.SetPosition (0, t.position);
			lr.SetPosition (1, t.position + pos);
			lr.SetPosition (2, t.position + 2*pos);
		}

		GameObject visualizePoint(Vector3 pos, Color col, string layerName) {
			GameObject o = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			o.transform.position = pos;
			o.transform.localScale = new Vector3 (6.0f, 6.0f, 6.0f);
			o.transform.GetComponent<Renderer>().materials [0].SetColor ("_Color", col);
			o.layer = LayerMask.NameToLayer (layerName);
			return o;
		}

		GameObject visualizePoint(Vector3 pos, Color col, string layerName, Transform parent) {
			GameObject o = visualizePoint (Vector3.zero, col, layerName);
			o.transform.parent = parent;
			o.transform.localPosition = pos;
			return o;
		}

		float getMaxNearClipPlaneValue() {
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(proteinCamera);
			int sign = 1;

			float recMaxVal = 0;
			Bounds recBounds;
			recBounds = proteins [0].gameObject.GetComponent<MeshCollider> ().sharedMesh.bounds;
			if (GeometryUtility.TestPlanesAABB (planes, recBounds)) {
				if (proteinCamera.transform.position.z < proteins[0].position.z) {
					recMaxVal = proteins[0].position.z + recRadius - proteinCamera.transform.position.z;
				} else {
					sign = -1;
					recMaxVal = proteinCamera.transform.position.z - (proteins[0].position.z - recRadius);
				}
			}

			float ligMaxVal = 0;
			Bounds ligBounds;
			ligBounds = proteins[1].gameObject.GetComponent<MeshCollider> ().sharedMesh.bounds;
			if (GeometryUtility.TestPlanesAABB (planes, ligBounds)) {
				if (proteinCamera.transform.position.z < proteins[1].position.z) {
					ligMaxVal = proteins[1].position.z + ligRadius - proteinCamera.transform.position.z ;
				} else {
					sign = -1;
					ligMaxVal = proteinCamera.transform.position.z - (proteins[1].position.z - ligRadius);
				}
			}

			return sign * Mathf.Max (recMaxVal, ligMaxVal);
		}

		void handleGestureMode() {
			gm = (GestureModes)((int)(gm+1) % 2);
			tfms[0] = TwoFingerModes.None;
			tfms[1] = TwoFingerModes.None;
			activeFingerIds[0].Clear();
			activeFingerIds[1].Clear();
			if (gm == GestureModes.Dual) {
				initialProteinsPos [0] = proteins [0].position;
				initialProteinsPos [1] = proteins [1].position;
				initialProteinsRot [0] = proteins [0].rotation;
				initialProteinsRot [1] = proteins [1].rotation;
				proteins[2].position = MergeSpheres(proteins [0].position, recRadius, proteins [1].position, ligRadius, out centreRadius);
				unitsPerPixel [2] = getUnitsPerPixel(proteins [2].position);
				degreesPerUnit [2] = 360 / (2 * Mathf.PI * centreRadius);
				centreSpaceProteinsPos[0] = proteins[0].position - proteins[2].position;
				centreSpaceProteinsPos[1] = proteins[1].position - proteins[2].position;
				gestureModeButtonImage.overrideSprite = gestureModeLockedImage;
				nearClipPlaneSlider.GetComponent<Slider> ().maxValue = getMaxNearClipPlaneValue ();
				nearClipPlaneSlider.SetActive (true);
			} else {
				initialProteinsPos [0] = proteins [0].position;
				initialProteinsPos [1] = proteins [1].position;
				initialProteinsRot [0] = proteins [0].rotation;
				initialProteinsRot [1] = proteins [1].rotation;
				gestureModeButtonImage.overrideSprite = null;
				nearClipPlaneSlider.SetActive (false);
			}
		}

		void handleNearClipPlane(float value) {
			proteinCamera.nearClipPlane = value;
		}

		Vector2 getUnitsPerPixel(Vector3 pos) {
			// Get the screen coordinate of some point
			Vector3 screenPos = mainCamera.WorldToScreenPoint (pos);
			// Offset by 1 pixel
			screenPos.x += (screenPos.x > 0) ? -1 : 1;
			screenPos.y += (screenPos.y > 0) ? -1 : 1;
			// Get the world coordinate once offset.
			Vector3 worldOffsetPos = mainCamera.ScreenToWorldPoint (screenPos);
			// Get the number of world units difference.
			Vector2 ratio = pos - worldOffsetPos;
			return ratio;
		}

		void initCanvas() {
			Instantiate (uiEventSystemPrefab);

			GameObject canvas = Instantiate(uiCanvasPrefab) as GameObject;
			canvas.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.GetComponent<CanvasScaler> ().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvas.layer = LayerMask.NameToLayer ("UI");
			canvas.name = "TouchManagerCanvas";

			GameObject gestureModeButton = Instantiate(uiButtonPrefab) as GameObject;
			gestureModeButton.transform.SetParent(canvas.transform, false);
			gestureModeButton.name = "GestureModeButton";
			RectTransform rt = gestureModeButton.GetComponent<RectTransform> ();
			// where are the anchors (bottom/up, left/right) 0/1
			rt.anchorMin = Vector2.one;
			rt.anchorMax = Vector2.one;
			// If the anchors are together (min == max), sizeDelta is the same as size. 
			rt.sizeDelta = new Vector2 (32, 32);
			// location of pivot expressed as fraction
			rt.pivot = Vector2.one;
			// where is the position of the pivot relative to the anchor
			rt.anchoredPosition = new Vector2 (0, 0);
			gestureModeButtonImage = gestureModeButton.GetComponent<Image> ();
			gestureModeButtonImage.sprite = gestureModeUnlockedImage;
			gestureModeButtonImage.type = Image.Type.Simple;
			gestureModeButtonImage.preserveAspect = true;
			Button button = gestureModeButton.GetComponent<Button> ();
			button.transition = Selectable.Transition.None;
			button.onClick.AddListener (handleGestureMode);
			// I don't want text
			button.transform.GetChild (0).gameObject.SetActive (false);

			nearClipPlaneSlider = Instantiate(uiSliderPrefab) as GameObject;
			nearClipPlaneSlider.transform.SetParent(canvas.transform, false);
			nearClipPlaneSlider.name = "nearClipPlaneSlider";
			nearClipPlaneSlider.SetActive (false);
			rt = nearClipPlaneSlider.GetComponent<RectTransform> ();
			// where are the anchors (bottom/up, left/right) 0/1
			rt.anchorMin = new Vector2 (1f, 0.5f);
			rt.anchorMax = new Vector2 (1f, 0.5f);
			// If the anchors are together (min == max), sizeDelta is the same as size. 
			rt.sizeDelta = new Vector2 (20, 160);
			// location of pivot expressed as fraction
			rt.pivot = new Vector2 (1f, 0.5f);
			// where is the position of the pivot relative to the anchor
			rt.anchoredPosition = new Vector2 (0, 0);
			Slider slider = nearClipPlaneSlider.GetComponent<Slider> ();
			slider.transition = Selectable.Transition.None;
			// TODO: there seems to be a problem with direction when changed from script, does not really work
			//slider.direction = Slider.Direction.BottomToTop;
			slider.value = proteinCamera.nearClipPlane;
			slider.minValue = proteinCamera.nearClipPlane;
			slider.maxValue = getMaxNearClipPlaneValue ();
			slider.onValueChanged.AddListener(handleNearClipPlane);
		}

		void Awake() {
			proteinCamera = mainCamera.transform.GetChild (0).GetComponent<Camera> ();

			// Bioblox
			// Creating this object - it exists in old Dockit project
			GameObject centre = new GameObject ("Center");
			centre.transform.position = (receptor.transform.position + ligand.transform.position) * 0.5f;

			proteins [0] = receptor.transform;
			proteins [1] = ligand.transform;
			proteins [2] = centre.transform;

			initialProteinsPos [0] = proteins [0].position;
			initialProteinsPos [1] = proteins [1].position;
			initialProteinsRot [0] = proteins [0].rotation;
			initialProteinsRot [1] = proteins [1].rotation;

			unitsPerPixel [0] = getUnitsPerPixel(proteins [0].position);
			unitsPerPixel [1] = getUnitsPerPixel(proteins [1].position);

			degreesPerUnit [0] = 360 / (2 * Mathf.PI * recRadius);
			degreesPerUnit [1] = 360 / (2 * Mathf.PI * ligRadius);

			touchInputMask = (1 << ligand.layer) | (1 << receptor.layer);

			initCanvas();
		}

		void Update() {

			// if double tap, reset proteins to the positions they had at the beginning of the gestureMode and return
			if (Input.touchCount == 1 && Input.GetTouch (0).tapCount == 2) {
				if (gm == GestureModes.Dual) {
					nearClipPlaneSlider.GetComponent<Slider> ().maxValue = getMaxNearClipPlaneValue ();
				} else {					
					activeFingerIds[1].Clear();
				}
				activeFingerIds[0].Clear();
				proteins [0].position = initialProteinsPos [0];
				proteins [1].position = initialProteinsPos [1];
				proteins [0].rotation = initialProteinsRot [0];
				proteins [1].rotation = initialProteinsRot [1];
				return;
			}

			// if single protein mode
			if (gm == GestureModes.Single) {
				updateT[0] = false;
				updateT[1] = false;
				prevNumFingers[0] = activeFingerIds[0].Count;
				prevNumFingers[1] = activeFingerIds[1].Count;
				for (int i = 0; i != Input.touchCount; ++i) {
					Touch touch = Input.GetTouch(i);
					if (touch.fingerId < fingers.Length) {
						Finger finger = fingers[touch.fingerId];
						if ((touch.phase == TouchPhase.Began) || (touch.phase == TouchPhase.Moved && finger.pending > 0)) {
							if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
								Ray ray = mainCamera.ScreenPointToRay (touch.position);
								if (Physics.Raycast (ray, out hitInfo, maxPickingDistance, touchInputMask)) {
									// Bioblox
									// Using transform name instead of tags
									int curProtein = hitInfo.transform.name.Equals(proteins[0].name)? 0 : 1;
									if ((finger.pending > 0 && curProtein == finger.protein) || finger.pending == 0) {
										finger.pending = 0;
										finger.protein = curProtein;
										finger.touchId = i;
										finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[curProtein].position);
										finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[curProtein].position;
										finger.r = ray;
										finger.screenPos = touch.position;
										activeFingerIds[finger.protein].Add (touch.fingerId);
									}
								}
							}
						} else if (activeFingerIds[finger.protein].Contains(touch.fingerId)) {
							if (touch.phase == TouchPhase.Moved) {
								updateT[finger.protein] = true;
								finger.touchId = i;
							} else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) {
								activeFingerIds[finger.protein].Remove(touch.fingerId);
							}
						}
						fingers[touch.fingerId] = finger;
					}
				}

				// For each protein
				for(int i = 0; i < activeFingerIds.Length; i++) {
					if (activeFingerIds[i].Count > 0) {
						List<int> afList = new List<int>(activeFingerIds[i]);
						// Since one finger was added or removed, we need to update all fingers that
						// correspond to touches that are Moved or Stationary
						// If the new ray does not hit the same protein then the finger becomes pending
						if (activeFingerIds[i].Count != prevNumFingers[i]) {
							tfms[i] = TwoFingerModes.None;
							for(int j = 0; j < afList.Count; j++) {							
								Finger finger = fingers[afList[j]];		
								Touch touch = Input.GetTouch(finger.touchId);
								if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
									updateT[finger.protein] = false;
									Ray ray = mainCamera.ScreenPointToRay (touch.position);
									if (Physics.Raycast (ray, out hitInfo, maxPickingDistance, touchInputMask)) {
										int curProtein = hitInfo.transform.name.Equals(proteins[0].name)? 0 : 1;
										if (curProtein == finger.protein) {
											finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[curProtein].position);
											finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[curProtein].position;
											finger.r = ray;					
											finger.screenPos = touch.position; 
											fingers[touch.fingerId] = finger;
										} else {						
											activeFingerIds[finger.protein].Remove(touch.fingerId);
											finger.pending = 1;
											fingers[touch.fingerId] = finger;
										}
									} else {						
										activeFingerIds[finger.protein].Remove(touch.fingerId);
										finger.pending = 2;
										fingers[touch.fingerId] = finger;
									}
								}								
							}
						} else if (updateT[i]) {
							if (activeFingerIds[i].Count == (int)Gestures.SingleFinger) {
								Finger finger = fingers[afList[0]];
								Touch touch = Input.GetTouch(finger.touchId);
								proteins[i].position = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z))-finger.worldSpaceHitTransformOffset;
							} else if (activeFingerIds[i].Count == (int)Gestures.TwoFinger) {
								// if we were in mode and direction changes, then reset everything
								//TODO: index out of bounds! Why?????
								//TODO: Try to add all touch info in the finger struct to solver this 
								float direction = Vector3.Dot(Input.GetTouch(fingers[afList[0]].touchId).deltaPosition, Input.GetTouch(fingers[afList[1]].touchId).deltaPosition);
								if ((tfms[i] == TwoFingerModes.RotateXY && direction < 0) || ((tfms[i] == TwoFingerModes.RotateZ || tfms[i] == TwoFingerModes.Pinch) && direction > 0)) {
									// reset fingers
									for(int j = 0; j < afList.Count; j++) {							
										Finger finger = fingers[afList[j]];		
										Touch touch = Input.GetTouch(finger.touchId);
										if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
											Vector3 touchPos = (touch.phase == TouchPhase.Moved)? (touch.position - touch.deltaPosition) : touch.position;
											Ray ray = mainCamera.ScreenPointToRay (touchPos);
											if (Physics.Raycast (ray, out hitInfo, maxPickingDistance, touchInputMask)) {
												int curProtein = hitInfo.transform.name.Equals(proteins[0].name)? 0 : 1;
												if (curProtein == finger.protein) {
													finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[curProtein].position);
													finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[curProtein].position;
													finger.r = ray;					
													finger.screenPos = touchPos; 
													fingers[touch.fingerId] = finger;
												} else {					
													activeFingerIds[finger.protein].Remove(touch.fingerId);
													finger.pending = 3;
													fingers[touch.fingerId] = finger;
												}
											} else {										
												activeFingerIds[finger.protein].Remove(touch.fingerId);
												finger.pending = 4;
												fingers[touch.fingerId] = finger;
											}
										}
									}
									afList.Clear();
									afList = new List<int>(activeFingerIds[i]);
									tfms[i] = TwoFingerModes.None;
								}

								// if we are in two finger gesture
								if (activeFingerIds[i].Count == (int)Gestures.TwoFinger) {
									Finger finger1, finger2;
									if (afList[0] < afList[1]) {
										finger1 = fingers[afList[0]];
										finger2 = fingers[afList[1]];
									} else {
										finger1 = fingers[afList[1]];
										finger2 = fingers[afList[0]];
									}
									Touch touch1 = Input.GetTouch(finger1.touchId);		
									Touch touch2 = Input.GetTouch(finger2.touchId);
									float angle = Mathf.Abs (Vector2.Angle(touch2.position - touch1.position, finger2.screenPos - finger1.screenPos));
									direction = Vector3.Dot(touch2.deltaPosition, touch1.deltaPosition);
									float distanceDelta = Mathf.Abs (Vector2.Distance (touch2.position, touch1.position) - Vector2.Distance (finger2.screenPos, finger1.screenPos));

									// if no two finger mode, then set it
									if (tfms[i] == TwoFingerModes.None) {
										// angle and distanceDelta thresholds are used only for iniating the respective modes
										// only change in the direction or in the number of fingers can take you out of the mode
										if (angle < minTurnAngle && direction >= 0) {
											tfms[i] = TwoFingerModes.RotateXY;
											if (zChanged[i]) {
												unitsPerPixel [i] = getUnitsPerPixel(proteins[i].position);
												zChanged[i] = false;
											}
										} else if (angle >= minTurnAngle && direction <= 0) {
											tfms[i] = TwoFingerModes.RotateZ;
										} else if (angle < minTurnAngle && direction <= 0 && distanceDelta >= minDistanceDelta) {
											tfms[i] = TwoFingerModes.Pinch;
											zChanged[i] = true;
											// calculate the ray based on the centre of the 2 screen touches and 
											// update the ray of both fingers as there is no guaranteed order in the hashset
											Finger finger = fingers[afList[0]];		
											Touch touch = Input.GetTouch(finger.touchId);
											Ray ray = mainCamera.ScreenPointToRay ((Input.GetTouch(fingers[afList[0]].touchId).position + Input.GetTouch(fingers[afList[1]].touchId).position)/2);
											finger.r = ray;
											fingers[touch.fingerId] = finger;
											for(int j = 0; j < afList.Count; j++) {									
												finger = fingers[afList[j]];
												finger.r = ray;									
												fingers[finger.touchId] = finger;
											}
										}
									}

									// if we have mode, do something!
									if (tfms[i] == TwoFingerModes.RotateXY) {
										float changeX = (Mathf.Abs(touch1.deltaPosition.x) > Mathf.Abs (touch2.deltaPosition.x))? touch1.deltaPosition.x : touch2.deltaPosition.x;
										float changeY = (Mathf.Abs(touch1.deltaPosition.y) > Mathf.Abs (touch2.deltaPosition.y))? touch1.deltaPosition.y : touch2.deltaPosition.y;
										proteins[i].Rotate(changeY*unitsPerPixel[i].y*degreesPerUnit[i], -changeX*unitsPerPixel[i].x*degreesPerUnit[i], 0, Space.World);
									} else if (tfms[i] == TwoFingerModes.RotateZ) {
										Vector2 curDir = touch2.position - touch1.position;
										Vector2 prevDir = touch2.position - touch2.deltaPosition - touch1.position + touch1.deltaPosition;
										float curAngle = (Mathf.Atan2 (curDir.y, curDir.x) - Mathf.Atan2 (prevDir.y, prevDir.x)) * Mathf.Rad2Deg;
										proteins[i].Rotate(0, 0, curAngle, Space.World);
									} else if (tfms[i] == TwoFingerModes.Pinch) {
										distanceDelta = Vector2.Distance (touch2.position-touch2.deltaPosition, touch1.position-touch1.deltaPosition) - Vector2.Distance (touch2.position, touch1.position); 
										if (Mathf.Abs(distanceDelta) > 0) {
											proteins[i].Translate(fingers[afList[0]].r.direction*distanceDelta, Space.World);
										}
									}
								}
							}
						}
					} else {						
						tfms[i] = TwoFingerModes.None;
					}
				} // end of GestureModes.Single
			} else if (gm == GestureModes.Dual) {
				updateT[0] = false;
				prevNumFingers[0] = activeFingerIds[0].Count;
				for (int i = 0; i != Input.touchCount; ++i) {
					Touch touch = Input.GetTouch(i);
					if (touch.fingerId < fingers.Length) {
						Finger finger = fingers[touch.fingerId];
						if (touch.phase == TouchPhase.Began) {
							if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
								Ray ray = mainCamera.ScreenPointToRay (touch.position);
								finger.pending = 0;
								finger.protein = 0;
								finger.touchId = i;
								finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[2].position);
								finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[2].position;
								finger.r = ray;
								finger.screenPos = touch.position;
								activeFingerIds[finger.protein].Add (touch.fingerId);
							}
						} else if (activeFingerIds[finger.protein].Contains(touch.fingerId)) {
							if (touch.phase == TouchPhase.Moved) {
								updateT[finger.protein] = true;
								finger.touchId = i;
							} else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) {
								activeFingerIds[finger.protein].Remove(touch.fingerId);
							}
						}
						fingers[touch.fingerId] = finger;
					}
				}

				if (activeFingerIds[0].Count > 0) {
					List<int> afList = new List<int>(activeFingerIds[0]);
					// Since one finger was added or removed, we need to update all fingers that
					// correspond to touches that are Moved or Stationary
					if (activeFingerIds[0].Count != prevNumFingers[0]) {
						// if previously we had pinch, then update the max near clip plane value
						if (tfms[0] == TwoFingerModes.Pinch) {
							nearClipPlaneSlider.GetComponent<Slider> ().maxValue = getMaxNearClipPlaneValue ();
						}
						tfms[0] = TwoFingerModes.None;
						for(int j = 0; j < afList.Count; j++) {							
							Finger finger = fingers[afList[j]];		
							Touch touch = Input.GetTouch(finger.touchId);
							if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
								updateT[finger.protein] = false;
								Ray ray = mainCamera.ScreenPointToRay (touch.position);
								finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[2].position);
								finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[2].position;
								finger.r = ray;
								finger.screenPos = touch.position;
								fingers[touch.fingerId] = finger;
							}
						}
					} else if (updateT[0]) {
						if (activeFingerIds[0].Count == (int)Gestures.SingleFinger) {
							Finger finger = fingers[afList[0]];
							Touch touch = Input.GetTouch(finger.touchId);
							proteins[2].position = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z))-finger.worldSpaceHitTransformOffset;
							proteins[0].position = proteins[2].position + centreSpaceProteinsPos[0];
							proteins[1].position = proteins[2].position + centreSpaceProteinsPos[1];
						} else if (activeFingerIds[0].Count == (int)Gestures.TwoFinger) {
							// if we were in mode and direction changes, then reset everything
							//TODO: index out of bounds! Why?????
							float direction = Vector3.Dot(Input.GetTouch(fingers[afList[0]].touchId).deltaPosition, Input.GetTouch(fingers[afList[1]].touchId).deltaPosition);
							if ((tfms[0] == TwoFingerModes.RotateXY && direction < 0) || ((tfms[0] == TwoFingerModes.RotateZ || tfms[0] == TwoFingerModes.Pinch) && direction > 0)) {
								// reset fingers
								for(int j = 0; j < afList.Count; j++) {							
									Finger finger = fingers[afList[j]];		
									Touch touch = Input.GetTouch(finger.touchId);
									if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
										Vector3 touchPos = (touch.phase == TouchPhase.Moved)? (touch.position - touch.deltaPosition) : touch.position;
										Ray ray = mainCamera.ScreenPointToRay (touchPos);
										finger.cameraSpaceHitTransformPos = mainCamera.transform.InverseTransformPoint(proteins[2].position);
										finger.worldSpaceHitTransformOffset = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, finger.cameraSpaceHitTransformPos.z)) - proteins[2].position;
										finger.r = ray;
										finger.screenPos = touchPos;
										fingers[touch.fingerId] = finger;
									}
								}
								afList.Clear();
								afList = new List<int>(activeFingerIds[0]);
								// if previously we had pinch, then update the max near clip plane value
								if (tfms[0] == TwoFingerModes.Pinch) {
									nearClipPlaneSlider.GetComponent<Slider> ().maxValue = getMaxNearClipPlaneValue ();
								}
								tfms[0] = TwoFingerModes.None;
							}

							// if we are in two finger gesture
							// We should be as number of fingers does not change but just to be sure
							if (activeFingerIds[0].Count == (int)Gestures.TwoFinger) {
								Finger finger1, finger2;
								if (afList[0] < afList[1]) {
									finger1 = fingers[afList[0]];
									finger2 = fingers[afList[1]];
								} else {
									finger1 = fingers[afList[1]];
									finger2 = fingers[afList[0]];
								}
								Touch touch1 = Input.GetTouch(finger1.touchId);		
								Touch touch2 = Input.GetTouch(finger2.touchId);
								float angle = Mathf.Abs (Vector2.Angle(touch2.position - touch1.position, finger2.screenPos - finger1.screenPos));
								direction = Vector3.Dot(touch2.deltaPosition, touch1.deltaPosition);
								float distanceDelta = Mathf.Abs (Vector2.Distance (touch2.position, touch1.position) - Vector2.Distance (finger2.screenPos, finger1.screenPos));
								
								// if no two finger mode, then set it
								if (tfms[0] == TwoFingerModes.None) {
									// angle and distanceDelta thresholds are used only for iniating the respective modes
									// only change in the direction or in the number of fingers can take you out of the mode
									if (angle < minTurnAngle && direction >= 0) {
										tfms[0] = TwoFingerModes.RotateXY;
										if (zChanged[0]) {
											unitsPerPixel [2] = getUnitsPerPixel(proteins[2].position);
											zChanged[0] = false;
										}
									} else if (angle >= minTurnAngle && direction <= 0) {
										tfms[0] = TwoFingerModes.RotateZ;
									} else if (angle < minTurnAngle && direction <= 0 && distanceDelta >= minDistanceDelta) {
										tfms[0] = TwoFingerModes.Pinch;
										zChanged[0] = true;
										// calculate the ray based on the centre of the 2 screen touches and 
										// update the ray of both fingers as there is no guaranteed order in the hashset
										Finger finger = fingers[afList[0]];		
										Touch touch = Input.GetTouch(finger.touchId);
										Ray ray = mainCamera.ScreenPointToRay ((Input.GetTouch(fingers[afList[0]].touchId).position + Input.GetTouch(fingers[afList[1]].touchId).position)/2);
										finger.r = ray;
										fingers[touch.fingerId] = finger;
										for(int j = 0; j < afList.Count; j++) {									
											finger = fingers[afList[j]];
											finger.r = ray;									
											fingers[finger.touchId] = finger;
										}
									}
								}
								
								// if we have mode, do something!
								if (tfms[0] == TwoFingerModes.RotateXY) {
									float changeX = (Mathf.Abs(touch1.deltaPosition.x) > Mathf.Abs (touch2.deltaPosition.x))? touch1.deltaPosition.x : touch2.deltaPosition.x;
									float changeY = (Mathf.Abs(touch1.deltaPosition.y) > Mathf.Abs (touch2.deltaPosition.y))? touch1.deltaPosition.y : touch2.deltaPosition.y;
									Quaternion rot = Quaternion.AngleAxis(changeY*unitsPerPixel[2].y*degreesPerUnit[2], Vector3.right) * Quaternion.AngleAxis(-changeX*unitsPerPixel[2].x*degreesPerUnit[2], Vector3.up);
									for(int j = 0; j < 2; j++) {
										Vector3 dir = proteins[j].position - proteins[2].position; // find current direction relative to center
										dir = rot * dir; // rotate the direction
										proteins[j].position = proteins[2].position + dir; // define new position
										Quaternion myRot = proteins[j].rotation;
										proteins[j].rotation *= Quaternion.Inverse(myRot) * rot * myRot;
										centreSpaceProteinsPos[j] = proteins[j].position - proteins[2].position;
									}
								} else if (tfms[0] == TwoFingerModes.RotateZ) {
									Vector2 curDir = touch2.position - touch1.position;
									Vector2 prevDir = touch2.position - touch2.deltaPosition - touch1.position + touch1.deltaPosition;
									float curAngle = (Mathf.Atan2 (curDir.y, curDir.x) - Mathf.Atan2 (prevDir.y, prevDir.x)) * Mathf.Rad2Deg;
									proteins[0].RotateAround(proteins[2].position, Vector3.forward, curAngle);
									proteins[1].RotateAround(proteins[2].position, Vector3.forward, curAngle);									
									centreSpaceProteinsPos[0] = proteins[0].position - proteins[2].position;
									centreSpaceProteinsPos[1] = proteins[1].position - proteins[2].position;
								} else if (tfms[0] == TwoFingerModes.Pinch) {
									distanceDelta = Vector2.Distance (touch2.position-touch2.deltaPosition, touch1.position-touch1.deltaPosition) - Vector2.Distance (touch2.position, touch1.position); 
									if (Mathf.Abs(distanceDelta) > 0) {
										proteins[2].Translate(fingers[afList[0]].r.direction*distanceDelta, Space.World);
										proteins[0].position = proteins[2].position + centreSpaceProteinsPos[0];
										proteins[1].position = proteins[2].position + centreSpaceProteinsPos[1];
									}
								}
							}
						}
					}
				} else {
					// if previously we had pinch, then update the max near clip plane value
					if (tfms[0] == TwoFingerModes.Pinch) {
						nearClipPlaneSlider.GetComponent<Slider> ().maxValue = getMaxNearClipPlaneValue ();
					}
					tfms[0] = TwoFingerModes.None;
				}
			} // end of GestureModes.Dual
		} // end of Update

		// It calculates the angle of the points in clockwise
		// Vector2.Angle gives the smallest angle
		static private float Angle (Vector2 pos1, Vector2 pos2) {
			Vector2 from = pos2 - pos1;
			Vector2 to = new Vector2(1, 0);
			
			float result = Vector2.Angle( from, to );
			Vector3 cross = Vector3.Cross( from, to );
			
			if (cross.z > 0) {
				result = 360f - result;
			}
			
			return result;
		}	

		// Calculates the minimum sphere bounding two spheres
		// Returns the centre of the sphere and updates the radius passed as parameter
		static private Vector3 MergeSpheres(Vector3 pos0, float rad0, Vector3 pos1, float rad1, out float rad) {
			Vector3 pos = Vector3.zero;
			Vector3 d = pos1 - pos0;
			float dist2 = d.sqrMagnitude;
			// if the distance of the centers is less than the difference of the radii, 
			// then one sphere fully encloses the other.
			if (dist2 <= (rad1 - rad0)*(rad1 - rad0)) {
				if (rad1 >= rad0) {
					rad = rad1;
					pos = pos1;
				} else {
					rad = rad0;
					pos = pos0;
				}
			} else {
				// Spheres partially overlapping or disjoint
				float dist = Mathf.Sqrt(dist2);
				rad = (dist + rad0 + rad1) * 0.5f;
				pos = pos0;
				if (dist > 0.00001) {
					pos += ((rad - rad0)/dist) * d; 
				}
			}
			return pos;
		}

	} // end of TouchManager class

}

