using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace TouchManager {

	public class TouchDebugger : MonoBehaviour {
		#if UNITY_EDITOR

		private List<Transform> fingers = new List<Transform> ();

		// image is taken from Touchscript library and converted to sprite
		public Sprite image;

		public int numFingers = 10;
		
		void Awake() {

			GameObject canvas = new GameObject ("CanvasTouchDebugger", typeof(Canvas), typeof(CanvasScaler));
			canvas.GetComponent<Canvas> ().renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.GetComponent<CanvasScaler> ().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

			for (int i = 1; i <= numFingers; i++) {
				GameObject finger = new GameObject ("Finger"+i, typeof(RectTransform), typeof(Image));				
				finger.transform.SetParent(canvas.transform, false);
				RectTransform rt = finger.GetComponent<RectTransform> ();
				// where are the anchors (bottom/up, left/right) 0/1
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.zero;
				// If the anchors are together (min == max), sizeDelta is the same as size. 
				rt.sizeDelta = new Vector2 (100, 100);
				// where is the position relative to the anchor
				rt.anchoredPosition = new Vector2 (0, 0);
				// location of pivot expressed as fraction
				rt.pivot = new Vector2 (.5f, .5f);
				
				finger.GetComponent<Image> ().sprite = image;
				finger.SetActive (false);
				
				GameObject textObject = new GameObject("Text", typeof(Text));				
				textObject.transform.SetParent(finger.transform, false);
				Text text = textObject.GetComponent<Text> ();
				text.rectTransform.sizeDelta = new Vector2 (100, 100);
				text.rectTransform.anchorMin = new Vector2(.5f, .5f);
				text.rectTransform.anchorMax = new Vector2(.5f, .5f);
				text.rectTransform.anchoredPosition = new Vector2(.5f, .5f);
				text.text = "F:"+i;
				text.font = Resources.FindObjectsOfTypeAll<Font>()[0];
				text.fontSize = 30;
				text.color = Color.yellow;
				text.alignment = TextAnchor.MiddleCenter;

				fingers.Add (finger.transform);
			}
		}

		void Update() {
			for (int i = 0; i != Input.touchCount; ++i) {
				Touch t = Input.GetTouch(i);
				if (t.fingerId < numFingers) {
					fingers[t.fingerId].position = t.position;
					if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) {
						fingers[t.fingerId].gameObject.SetActive(false);
					} else {
						fingers[t.fingerId].gameObject.SetActive(true);
						fingers[t.fingerId].GetComponentInChildren<Text>().text = "F:"+(t.fingerId+1)+"/T:"+i;
					}
				}
			}
		}
		#endif
	}
}