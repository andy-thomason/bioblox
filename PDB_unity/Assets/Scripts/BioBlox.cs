using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BioBlox : MonoBehaviour
{
	// dictaing which gametype, puzzle or museum
	bool simpleGame = true;

	// controls whether the win state should attempt to fade out the molecules
	public bool winShouldFadeMol = false;

	// score card for saving scores. 
	ScoreSheet scoreCard;

	// filenames for the levels, without the .txt
	public List<string> filenames = new List<string> ();

	// the level we are currently on, incremented at the end of a level
	int current_level = 0;

	// a holder variable for the current event system
	EventSystem eventSystem;

	// prefab of the labels used to point to atoms on the molecules, must have label script attached
	public GameObject prefabLabel;

	// the win and loose spash images
	//public GameObject winSplash;
	//public GameObject looseSplash;
	public GameObject goSplash;

	// a variable controlling the size of the area faded out during the win state
	public float shaderKVal = -0.03f;
	
	// a bool that will exit the win splash if set to true
	public bool exitWinSplash = false;

	// all labels which are currently in the scene
	public List<LabelScript> activeLabels = new List<LabelScript> ();
	// a number of selected labels, at the moment limited to 2, one from each molecule
	public LabelScript[] selectedLabel = new LabelScript[2];
	// a list of win conditions where two atoms must be paired
	List<Tuple<int,int>> winCondition = new List<Tuple<int,int>> ();

	// the molecules in the scene
	public GameObject[] molecules;

	//  NOT CURRENTLY IN USE
	//  sites are smaller regions of the molecules that can be selected and manipulated independtly from the molecules
	GameObject[] sites = new GameObject[2];
	//  wheter the player is moving the molecules, playerIsMoving[0] being molecule[0]
	bool[] playerIsMoving = new bool[2]{false,false};
	// the original positions of the molecules, used to provide a returning force during the puzzle mode
	Vector3[] originPosition = new Vector3[2];
	// game object target of the "popping" co-routines to shrink and grow the object out of and into the scene
	GameObject popTarget;

	//  score to achive to win
	public float winScore = 10.0f;
	//  torque applied to the molecule to move them when player drags
	public float uiScrollSpeed = 10.0f;

	//  force being applied to molecules to return them to their origin positions
	public float repulsiveForce = 30000.0f;
	//  force used for physics
	public float seperationForce = 10000.0f;
	//  force applied by string
	public float stringForce = 20000.0f;
	private float ScoreScaleSize;
	private float ScoreScaleValue;

	public Slider rmsScoreSlider;
	public Slider heuristicScoreSlider;
	public RectTransform heuristicScore;
	public Text GameScoreValue;
	public RectTransform GameScore;
	public Slider overrideSlider;
	public Slider cutawaySlider;
	public Text invalidDockText;
	public List<Slider> dockSliders = new List<Slider> ();
	public float dockOverrideOffset = 0.0f;
	//Animator of the tools menu
	public Animator ToolMenuAnimator;
	public GameObject OpenToolImage;
	public GameObject CloseToolImage;

	// colors of the labels and an offset that is randomly decided randomize colours
	List<Color> colorPool = new List<Color>();
	int randomColorPoolOffset;

	public Button lockButton;

	public float triangleOffset = 10.0f;
	GameObject[] featureTriangle =new GameObject[2];

	// shape scoring
	public int num_touching_0 = 0;
	public int num_touching_1 = 0;
	public int num_invalid = 0;
	public int num_connections = 0;

	public enum GameState {
		Setup,
		Waiting,
		Picking,
		Docking,
		Locked
	}

	public GameState game_state;

	// Use this for initialization
	void Start ()
	{
		//winSplash.SetActive (false);
		//looseSplash.SetActive (false);
		//goSplash.SetActive (false);

		game_state = GameState.Setup;

		Time.fixedDeltaTime = 0.033f;

		colorPool.Add (Color.red);
		colorPool.Add (Color.blue);
		colorPool.Add (Color.cyan);
		colorPool.Add (Color.green);
		colorPool.Add (Color.magenta);
		colorPool.Add (Color.yellow);
		colorPool.Add (Color.white);
		colorPool.Add (Color.gray);
		colorPool.Add (new Color (1.0f, 0.5f, 0.1f));
		randomColorPoolOffset = 0; //Random.Range (0, colorPool.Count - 1);
		Debug.Log ("Start");
		//filenames.Add ("jigsawBlue");

		//filenames.Add ("2W9G");

		//filenames.Add ("betabarrel_b");
		filenames.Add ("pdb2ptcWithTags");

		filenames.Add ("1GCQ_bWithTags");


		filenames.Add ("pdb2ptcWithTags");

		scoreCard = GameObject.Find ("ScoreCard").GetComponent<ScoreSheet> ();
		scoreCard.gameObject.SetActive (false);

		//game_loop loads the file at filenames[current_level]
		eventSystem = EventSystem.current;

		StartCoroutine (game_loop ());
	}

	public string GetCurrentLevelName ()
	{
		return filenames [current_level];
	}

	//converts an atom serial number (unique file identifier) into a index
	//also outputs the molecule index that that atom serial exists in
	int GetAtomIndexFromID (int id, out int moleculeNum)
	{
		moleculeNum = -1;
		if (molecules.Length == 2) {
			PDB_molecule mol1 = molecules [0].GetComponent<PDB_mesh> ().mol;
			PDB_molecule mol2 = molecules [1].GetComponent<PDB_mesh> ().mol;
			if (id >= mol1.serial_to_atom.Length) {
				moleculeNum = 1;
				return mol2.serial_to_atom [id];
			} else {
				moleculeNum = 0;
				return mol1.serial_to_atom [id];
			}
		}
		return -1;
	}

	//calculates a label positions within a dome infront of the molecules
	//stops labels from dissapering behind the molecules and uses the size of the largest collision sphere in the bvh
	public void GetLabelPos (List<int> atomIds, int molNum, Transform t)
	{
		Vector3 sumAtomPos = Vector3.zero;
		for (int i = 0; i < atomIds.Count; ++i) {
			sumAtomPos += GetAtomPos (atomIds [i], molNum);
		}
		sumAtomPos /= atomIds.Count;

		//if the molecule does not exist, or the atom id is bad
		if (molNum == -1 || molecules.Length == 0) {
			return;
		}

		PDB_mesh molMesh = molecules [molNum].GetComponent<PDB_mesh> ();
		Vector3 atomPosW = molMesh.transform.TransformPoint (sumAtomPos);

		Vector3 transToAt = atomPosW - molMesh.transform.position;
		//atom is behind molecule
		if (atomPosW.z > molMesh.transform.position.z) {
			//we project onto a circle on the xy plan
			transToAt.z = 0;
		}
		//project the label onto the first bvh radius
		transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
		atomPosW = transToAt + molMesh.transform.position;
		t.position = atomPosW;
		t.position += new Vector3 (0, 0, -10);
	}

	public void GetMiniLabelPos(int atomID, int molNum, Transform t)
	{
		Vector3 atomPos = GetAtomPos (atomID, molNum);
		//if the molecule does not exist, or the atom id is bad
		if (molNum == -1 || molecules.Length == 0) {
			return;
		}
		
		PDB_mesh molMesh = molecules [molNum].GetComponent<PDB_mesh> ();
		Vector3 atomPosW = molMesh.transform.TransformPoint (atomPos);
		
		Vector3 transToAt = atomPosW - molMesh.transform.position;
		//atom is behind molecule
		if (atomPosW.z > molMesh.transform.position.z) {
			//we project onto a circle on the xy plan
			transToAt.z = 0;
		}
		//project the label onto the first bvh radius
		//transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
		atomPosW = transToAt + molMesh.transform.position;
		t.position = atomPosW + new Vector3 (-3, 3, 0);
	}

	//returns an atoms local position from a atom index and molecule index	
	public Vector3 GetAtomPos (int atomID, int molNum)
	{
		if (atomID == -1 || molNum >= molecules.Length || molecules [molNum] == null) {
			Debug.LogError ("Bad index " + atomID + " " + molNum + " " + molecules.Length);
			return Vector3.zero;
		}
		return molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID];
	}

	//returns an atoms world position from atom index and molecule index
	public Vector3 GetAtomWorldPos (int atomID, int molNum)
	{
		if (atomID == -1 || molNum >= molecules.Length || molecules [molNum] == null) {
			Debug.LogError ("Bad index");
			return Vector3.zero;
		}
		return molecules [molNum].transform.TransformPoint (
			molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID]
		);
	}
    

	//co-routine to manage player interaction with molecules
	IEnumerator PlayerMoveMolecule (int molIndex)
	{
		GameObject mol = molecules [molIndex];
		Rigidbody r = mol.GetComponent<Rigidbody> ();
		r.maxAngularVelocity = 4;
		//player movement of the molecules times out after periods of no input
		float timeout = 100.0f;
		playerIsMoving [molIndex] = true;
		Vector3 lastMousePos = Input.mousePosition;
		RigidbodyConstraints old = RigidbodyConstraints.None;

		for (float t=0.0f; t<timeout; t+=Time.deltaTime) {
			if (playerIsMoving [molIndex] == false ||
				eventSystem.IsActive () == false) {
				//breakout if we the event system is deactivated or we are told to stop moving
				r.constraints = old;
				break;
			}
			if(Input.GetMouseButtonDown(0))
			{
				old = r.constraints;
				r.constraints = RigidbodyConstraints.FreezePosition;
			}
			if(Input.GetMouseButtonUp(0))
			{
				r.constraints = old;
			}

			if (Input.GetMouseButton (0)) { //left mouse button
				Camera c = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
				Ray ray = c.ScreenPointToRay (Input.mousePosition);
				//create a ray to the cursor and cast it, if it hits at all
				int atomID = PDB_molecule.collide_ray (mol, mol.GetComponent<PDB_mesh> ().mol,
				                                      mol.transform,
				                                      ray);
				if (atomID != -1) {
					if (t > 0.3f) {
						//if there is no recent input reset previous position
						lastMousePos = Input.mousePosition;
					}
					t = 0.0f;
					Vector3 mousePos = Input.mousePosition;
					Vector3 mouseDelta = mousePos - lastMousePos;
					//find the world position of the atom
					//Vector3 atomPos = GetAtomWorldPos (atomID, molIndex);

					//GameObject other = molecules[1-molIndex];

					Vector3 dirRight = Vector3.right;
					Vector3 dirUp = Vector3.up;
					//add force at the postion of the atom picked by the cursor
					//r.AddForceAtPosition(new Vector3 (mouseDelta.x, mouseDelta.y, 0) * uiScrollSpeed,
					//                     atomPos);
					r.transform.RotateAround(r.gameObject.transform.position,dirRight,mouseDelta.y);
					r.transform.RotateAround(r.gameObject.transform.position,dirUp,-mouseDelta.x);                       

					lastMousePos = mousePos;
				}
			}
			yield return null;
		}
		playerIsMoving [molIndex] = false;
		yield break;
	}

	// Update handles (badly) a few things that dont fit anywhere else.
	void Update ()
	{
		LineRenderer line_renderer = GameObject.FindObjectOfType<LineRenderer> () as LineRenderer;
		Camera camera = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		if (line_renderer && camera) {
			line_renderer.clear ();
			if (cutawaySlider && cutawaySlider.value > cutawaySlider.minValue) {
				for (int x = -50; x <= 50; x += 10) {
					//Vector3 p0 = camera.transform.TransformPoint(new Vector3( x, -50, cutawaySlider.value));
					//Vector3 p1 = camera.transform.TransformPoint(new Vector3( x,  50, cutawaySlider.value));
					Vector3 p0 = new Vector3( x, -50, cutawaySlider.value);
					Vector3 p1 = new Vector3( x, 50, cutawaySlider.value);
					line_renderer.add_line(new LineRenderer.Line(p0, p1));
				}
				for (int y = -50; y <= 50; y += 10) {
					Vector3 p0 = new Vector3( -50, y, cutawaySlider.value);
					Vector3 p1 = new Vector3( 50, y, cutawaySlider.value);
					//Vector3 p0 = camera.transform.TransformPoint(new Vector3( -50, y, cutawaySlider.value));
					//Vector3 p1 = camera.transform.TransformPoint(new Vector3(  50, y, cutawaySlider.value));
					line_renderer.add_line(new LineRenderer.Line(p0, p1));
				}
			}
		}

		GameObject cam = GameObject.Find ("Main Camera");
		Vector3 light_pos = cam.transform.TransformPoint(new Vector3(-50,0,0));
		foreach (GameObject mol in molecules) {
			MeshRenderer[] meshes = mol.GetComponentsInChildren<MeshRenderer> ();
			foreach (MeshRenderer r in meshes) {
				r.material.SetVector ("_LightPos", light_pos);
				if (cutawaySlider) {
					Vector4 plane = new Vector4(0, 0, 1, -cutawaySlider.value);
					r.material.SetVector ("_CutawayPlane", plane);
				}
			}
		}
	}

	void PopInSound (GameObject g)
	{
		this.GetComponent<AudioManager> ().Play ("Blip");
		PopIn (g);
	}

	public void PopIn (GameObject g)
	{
		popTarget = g;
		StartCoroutine ("PopInCo");
	}

	public void PopInWaitDisappear (GameObject g, float waitTime)
	{
		popTarget = g;
		StartCoroutine ("PopInWaitDisappearCo", waitTime);
	}

	public void PopOut (GameObject g)
	{
		popTarget = g;
		StartCoroutine ("PopOutCo");
	}

	IEnumerator PopInCo ()
	{
		GameObject target = popTarget;
		target.SetActive (true);
		popTarget = null;
		float scaleSpeed = 3.0f;
		Vector3 targetScale = target.transform.localScale;
		target.transform.localScale = new Vector3 (0, 0, 0);
		for (float t=0; t<1; t+=Time.deltaTime*scaleSpeed) {
			target.transform.localScale = targetScale * t;
			yield return null;
		}
		target.transform.localScale = targetScale;
		yield break;
	}

	IEnumerator PopInWaitDisappearCo (float waitTime)
	{
		GameObject target = popTarget;
		target.SetActive (true);
		popTarget = null;
		float scaleSpeed = 3.0f;
		Vector3 targetScale = target.transform.localScale;
		target.transform.localScale = new Vector3 (0, 0, 0);
		for (float t=0; t<1; t+=Time.deltaTime*scaleSpeed) {
			target.transform.localScale = targetScale * t;
			yield return null;
		}
		target.transform.localScale = targetScale;
		yield return new WaitForSeconds (waitTime);
		target.SetActive (false);
		yield break;
	}

	IEnumerator PopOutCo ()
	{
		GameObject target = popTarget;
		popTarget = null;
		float scaleSpeed = 3.0f;
		Vector3 targetScale = target.transform.localScale;
		for (float t=1; t>0; t-=Time.deltaTime*scaleSpeed) {
			target.transform.localScale = targetScale * t;
			yield return null;
		}
		target.SetActive (false);
		target.transform.localScale = targetScale;
		yield break;
	}

	//performs lignad RMSD scoring (sum of  the distance of each atom from its original position in the file)
	public float ScoreRMSD ()
	{
		if (molecules.Length == 0 || !molecules [0] || !molecules [0]) {
			return 0.0f;
		}
		float rmsd = 0.0f;
		int count = 0;
		PDB_mesh receptor = molecules [0].GetComponent<PDB_mesh> ();
		PDB_mesh ligand = molecules [1].GetComponent<PDB_mesh> ();
		Vector3 offset = ligand.mol.pos - receptor.mol.pos;
		Matrix4x4 transMat = ligand.transform.worldToLocalMatrix * receptor.transform.localToWorldMatrix;
		for (int i = 0; i < ligand.mol.atom_centres.Length; i++) {
			if (ligand.mol.names [i] == PDB_molecule.atom_C) {
				rmsd += (transMat.MultiplyPoint3x4 (ligand.mol.atom_centres [i] + offset) - ligand.mol.atom_centres [i]).sqrMagnitude;
				count++;
			}
		}
		return Mathf.Sqrt (rmsd / count);
	}

	//Handle the dock slider inputs
	public void HandleDockSlider(Slider slide)
	{
		//if we are at the max value then reveal labelsif (slide.value > slide.maxValue - 10.0f) {


	}

	public void HandleCameraSlider(Slider slide)
	{
		Transform t = GameObject.Find ("Main Camera").transform;
		GameObject cellCam = GameObject.Find ("Camera");
		float dist = t.position.magnitude;

		Vector3 dir = new Vector3 (Mathf.Cos(Mathf.Deg2Rad * slide.value),
		                          0,
		                          Mathf.Sin(Mathf.Deg2Rad * slide.value));

		if (cellCam) {
			cellCam.transform.localRotation = Quaternion.Euler(new Vector3(0,slide.value + 90,0));
		}

		Vector3 pos = dir * dist;
		t.position = pos;
		t.rotation = Quaternion.LookRotation (-dir, Vector3.up);
	}


	//changes variables in the shader to fade the molecules everywhere but two points
	//this is to emphisie the docking site
	/*IEnumerator FadeMolecules ()
	{
		//GameObject mol1 = molecules [0];
		//GameObject mol2 = molecules [1];
		MeshRenderer[] meshes1 = molecules [0].GetComponentsInChildren<MeshRenderer> ();
		MeshRenderer[] meshes2 = molecules [1].GetComponentsInChildren<MeshRenderer> ();
		//PDB_molecule molInfo1 = mol1.GetComponent<PDB_mesh> ().mol;
		//PDB_molecule molInfo2 = mol2.GetComponent<PDB_mesh> ().mol;

		//it is necessary to loop through the children as the mesh may be split into seveal sections
		//due to vertex number limitations in unity
		for (int i=0; i<meshes1.Length; ++i) {
			meshes1 [i].material.SetVector ("_CullPos", 
			                               new Vector3 (0, 0, 0));
		}
		for (int i=0; i<meshes2.Length; ++i) {
			meshes2 [i].material.SetVector ("_CullPos",
			                               new Vector3 (0, 0, 0));
		}
		float targetKVal = shaderKVal;
		float currentKVal = 0;
		//lerp a current Kval to a target Kval for a smoother fade
		for (float t=0; t<=1.0f; t+=Time.deltaTime) {
			float k = currentKVal + targetKVal * t;
			
			for (int i=0; i<meshes1.Length; ++i) {
				meshes1 [i].material.SetFloat ("_K", k);
			}
			for (int i=0; i<meshes2.Length; ++i) {
				meshes2 [i].material.SetFloat ("_K", k);
			}
			yield return null;
		}
		//ensure that it reaches the target
		for (int i=0; i<meshes1.Length; ++i) {
			meshes1 [i].material.SetFloat ("_K", targetKVal);
		}
		for (int i=0; i<meshes2.Length; ++i) {
			meshes2 [i].material.SetFloat ("_K", targetKVal);
		}
	}*/


	public void ManageSliders()
	{
		ConnectionManager conMan = this.GetComponent<ConnectionManager> ();

		bool allInPickZone = true;
		if (overrideSlider.gameObject.activeSelf && conMan.connectionMinDistances.Length == dockSliders.Count) {
			float slider_pos = overrideSlider.value - dockOverrideOffset;

			// see if the sliders are in the "sticky" region near the constrainer
			float val0 = dockSliders[0].value;
			bool all_together = true;
			for (int i = 1; i < dockSliders.Count; ++i)
			{
				all_together = all_together && dockSliders[i].value == val0;
			}

			if (true || all_together && val0 >= slider_pos - 2)
			{
				// all sliders are being moved by the master
				for (int i = 0; i < dockSliders.Count; ++i)
				{
					dockSliders[i].value = slider_pos;
					conMan.connectionMinDistances[i] = dockSliders[i].value;
				}
			} else
			{
				// sliders are individual.
				for (int i = 0; i < dockSliders.Count; ++i)
				{
					if (dockSliders[i].value >= slider_pos)
					{
						dockSliders[i].value = slider_pos;
					}
					conMan.connectionMinDistances[i] = dockSliders[i].value;
				}
			}

			// Return to pick state if all on the right.
			for (int i = 1; i < dockSliders.Count; ++i)
			{
				if (dockSliders[i].value < dockSliders[i].maxValue-5)
				{
					allInPickZone = false;
				}
			}
		}

		if (game_state == GameState.Picking || game_state == GameState.Docking) {
			if (allInPickZone) {
				if (!activeLabels [0].gameObject.activeSelf) {
					for (int i = 0; i < activeLabels.Count; ++i) {
						activeLabels [i].gameObject.SetActive (true);
					}
				}
				game_state = GameState.Picking;
			} else {
				if (activeLabels [0].gameObject.activeSelf) {
					for (int i = 0; i < activeLabels.Count; ++i) {
						activeLabels [i].gameObject.SetActive (false);
					}
				}
				game_state = GameState.Docking;
			}
		}
	}

	public void SiteClicked (GameObject labelObj)
	{
		/*


		//this is site picking code, it basically takes atoms local to the selected one
		//and creates a new mesh with that subset
		Debug.Log (labelObj.name + "'s site was selected");
		LabelScript script = labelObj.GetComponent<LabelScript> ();
		int molNum = -1;
		int index = GetAtomIndexFromID (script.label.atomIndex,out molNum);

		GameObject g;
		Transform t = GameObject.Find ("SitePosition" + molNum).transform;
		if (!sites [molNum]) {
			Debug.Log("Created site");
			GameObject primitive = GameObject.CreatePrimitive (PrimitiveType.Quad);
			Material diffuse = primitive.GetComponent<MeshRenderer> ().sharedMaterial;
			DestroyImmediate (primitive);

			g = new GameObject ();
			g.AddComponent<MeshFilter> ();
			MeshRenderer r = g.AddComponent<MeshRenderer> ();
			g.transform.position = t.position;
			r.material = diffuse;
			sites [molNum] = g;
			g.transform.rotation = Quaternion.identity;
		}

		MeshFilter f = sites [molNum].GetComponent<MeshFilter> ();
		sites [molNum].GetComponent<MeshRenderer> ();
		PDB_mesh meshy = molecules [molNum].GetComponent<PDB_mesh> ();
		PDB_molecule mol = meshy.mol;
		Vector3 spherePos = meshy.mol.atom_centres [index];
		float rad = 3.5f;

		PDB_molecule.BvhSphereCollider sphere = 
			new PDB_molecule.BvhSphereCollider (meshy.mol,
			                                    spherePos,
			                                    rad);

		List<int> mol_index_vec = new List<int> ();
		for(int i=0;i<sphere.results.Count;++i)
		{
			mol_index_vec.Add(sphere.results[i].index);
		}
		Mesh m = mol.build_section_mesh (mol_index_vec.ToArray());
		f.mesh = m;*/
	
	}

	//creates a err label. The label object contains a atom id and name used to name the instance
	void CreateLabel (PDB_molecule.Label argLabel, int molNum, string labelName, PDB_molecule mol)
	{
		GameObject newLabel = GameObject.Instantiate<GameObject> (prefabLabel);
		if (!newLabel) {
			Debug.Log ("Could not create Label");
		}

		LabelScript laSc = newLabel.GetComponent<LabelScript> ();
		newLabel.name = labelName + laSc.labelID;

		//assigned the label a new color from a random range
		newLabel.GetComponent<Image> ().color = colorPool [(argLabel.uniqueLabelID + randomColorPoolOffset) % colorPool.Count];
		newLabel.GetComponent<Light> ().color = newLabel.GetComponent<Image> ().color;
		laSc.atomIds = argLabel.atomIds;
		laSc.moleculeNumber = molNum;

		laSc.owner = this;
		laSc.labelID = argLabel.uniqueLabelID;
		//3D and 2D arrows see LabelScript
		laSc.cloudIs3D = true;
		//GameObject foundObject = GameObject.Find ("Label" + (activeLabels.Count + 1));
		//newLabel.transform.position = foundObject.transform.position;

		//we group all the labels under a single empty transform for convienence in the unity hierarchy
		GameObject canvas = GameObject.Find ("Labels");
		newLabel.transform.SetParent (canvas.transform);

		laSc.Init (mol);
		laSc.gameObject.SetActive (false);
		while (activeLabels.Count < argLabel.uniqueLabelID + 1) {
			activeLabels.Add (null);
		}

		activeLabels [argLabel.uniqueLabelID] = laSc;
	}

	//manages a left click on a label
	//has two different functions depending on the gametype eg( simple or complicated)
	public void LabelClicked (GameObject labelObj)
	{
		//Debug.Log (labelObj.name + " was clicked");
		LabelScript script = labelObj.GetComponent<LabelScript> ();
		
		int molNum = script.moleculeNumber;

		if (script.atomIds.Count == 0) {
			return;
		}

		PDB_mesh pdbMesh = molecules [molNum].GetComponent<PDB_mesh> ();
		ConnectionManager conMan = this.GetComponent<ConnectionManager> ();

		if (simpleGame) {
			Debug.Log ("simple game");
			//here we update the selected label list
			//and align the molecule so that the labeled atom is facing the other molecule
			Vector3 sumAtomPos = Vector3.zero;
			for (int i=0; i < script.atomIds.Count; ++i) {
				sumAtomPos += GetAtomPos (script.atomIds [i], molNum);
			}
			sumAtomPos /= script.atomIds.Count;
			int otherMolNum = 1 - molNum;
			Vector3 alignDir = molecules [otherMolNum].transform.position -
				molecules [molNum].transform.position;
			pdbMesh.AlignPointToVector (sumAtomPos, alignDir);
			//logic to select which labels should glow
			if (selectedLabel [0]) {
				selectedLabel [0].shouldGlow = false;
			}
			if (selectedLabel [1]) {
				selectedLabel [1].shouldGlow = false;
			}
			selectedLabel [molNum] = script;
			if (selectedLabel [0]) {
				selectedLabel [0].shouldGlow = true;
			}
			if (selectedLabel [1]) {
				selectedLabel [1].shouldGlow = true;
			}
			Vector3 atomPos1 = new Vector3 (0, 100000, 0);
			Vector3 atomPos2 = atomPos1;
			Vector3 atomPos3 = atomPos2;
			if (script.atomIds.Count > 0) {
				atomPos1 = pdbMesh.mol.atom_centres [script.atomIds [0]];
			}
			if (script.atomIds.Count > 1) {
				atomPos2 = pdbMesh.mol.atom_centres [script.atomIds [1]];
			}
			if (script.atomIds.Count > 2) {
				atomPos3 = pdbMesh.mol.atom_centres [script.atomIds [2]];
			}

			//set the sector we clicked to glow, this is done in the shader to stop us having to rebuild the mesh with new colours
			MeshRenderer[] meshes = molecules [molNum].GetComponentsInChildren<MeshRenderer> ();
			foreach (MeshRenderer r in meshes) {
				r.material.SetVector ("_GlowPoint1", atomPos1);
				r.material.SetFloat ("_GlowRadius1", 5.0f);
				r.material.SetVector ("_GlowPoint2", atomPos2);
				r.material.SetFloat ("_GlowRadius2", 5.0f);
				r.material.SetVector ("_GlowPoint3", atomPos3);
				r.material.SetFloat ("_GlowRadius3", 5.0f);
			}

			if (selectedLabel [0] != null && selectedLabel [1] != null) {
				conMan.CreateLinks (molecules [0].GetComponent<PDB_mesh> (),
				                   selectedLabel [0].atomIds.ToArray (),
				                   molecules [1].GetComponent<PDB_mesh> (),
				                   selectedLabel [1].atomIds.ToArray ());
				/*for(int i = 0; i < dockSliders.Count; ++i)
				{
					dockSliders[i].gameObject.SetActive(true);
					overrideSlider.gameObject.SetActive(true);
				}*/				
				overrideSlider.interactable = true;

				uiScrollSpeed =900;
			}
		}
	}

	// The pick button resets the sliders to the right.
	public void Pick()
	{
		if (game_state == GameState.Picking || game_state == GameState.Docking) {
			ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
		
			overrideSlider.value = conMan.maxDistance;
			for (int i = 0; i < dockSliders.Count; ++i) {
				dockSliders [i].value = conMan.maxDistance;
			}

			//conMan.Reset ();

			//selectedLabel [0] = null;
			//selectedLabel [1] = null;
			//game_state = GameState.Picking;

			ManageSliders ();
		}
	}
	
	// The dock button resets the sliders to the middle.
	public void Dock()
	{
		if (game_state == GameState.Picking || game_state == GameState.Docking) {
			ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
			
			overrideSlider.value = conMan.maxDistance * 0.5f;
			for (int i = 0; i < dockSliders.Count; ++i) {
				dockSliders [i].value = conMan.maxDistance * 0.5f;
			}
			ManageSliders ();
		}
	}
	
	// The lock button establishes the Locked state.
	public void Lock()
	{
		Debug.Log ("Lock: " + game_state);
		if (game_state == GameState.Docking) {
			Debug.Log("ok");
			game_state = GameState.Locked;
		}
	}
	
	public void SolidClicked()
	{
		make_molecules (false, MeshTopology.Triangles);
	}
	
	public void PointClicked()
	{
		make_molecules (false, MeshTopology.Points);
	}
	
	public void WireClicked()
	{
		make_molecules (false, MeshTopology.Lines);
	}


	
	void Reset ()
	{
		//clears the molecules and re-randomizes the colour range
		randomColorPoolOffset = 0; //Random.Range (0, colorPool.Count - 1);
		GameObject.Destroy (molecules [0].gameObject);
		GameObject.Destroy (molecules [1].gameObject);

		//clear the old win condition
		winCondition.Clear ();

		//clears the selected index
		selectedLabel [0] = null;
		selectedLabel [1] = null;

		game_state = GameState.Picking;

		//Clear score
		GameScore.localScale = Vector3.zero;
		heuristicScore.localScale = Vector3.zero;
		GameScoreValue.text = "0";
	}

	//since a molecule may be too large for one mesh we may have to make several
	void make_molecule_mesh (PDB_mesh mesh, Material material, int layerNum, MeshTopology mesh_type)
	{
		foreach (Transform child in mesh.transform) {
			Destroy(child.gameObject);
		}

		for (int i=0; i<mesh.mol.mesh.Length; ++i) {
			Mesh cur = mesh.mol.mesh [i];
			if (mesh_type != MeshTopology.Triangles) {
				Mesh new_mesh = new Mesh();
				new_mesh.vertices = cur.vertices;
				new_mesh.colors = cur.colors;
				new_mesh.normals = cur.normals;
				new_mesh.SetIndices(cur.GetIndices(0), mesh_type, 0);
				cur = new_mesh;
			}
			GameObject obj = new GameObject ();
			obj.name = cur.name;
			obj.layer = layerNum;
			MeshFilter f = obj.AddComponent<MeshFilter> ();
			MeshRenderer r = obj.AddComponent<MeshRenderer> ();
			r.material = material;
			f.mesh = cur;
			obj.transform.SetParent (mesh.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localRotation = Quaternion.identity;
		}
	}

	// Creates the molecule objects including the PDB_mesh script.
	GameObject make_molecule (string name, string proto, int layerNum, MeshTopology mesh_type)
	{
		GameObject obj = GameObject.Find (name);
		if (obj == null) {
			obj = new GameObject ();
		}

		obj.name = name;
		obj.SetActive (true);
		//the layer numbers are to seperate the molecules in certain cameras
		obj.layer = layerNum;

		PDB_mesh p = obj.AddComponent<PDB_mesh> ();

		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		GameObject pdb = GameObject.Find (proto);
		MeshRenderer pdbr = pdb.GetComponent<MeshRenderer> ();
		make_molecule_mesh (p, pdbr.material, layerNum, mesh_type);
		return obj;
	}

	// set the object to its initial position etc.
	void reset_molecule(GameObject obj, float xoffset, int molNum)
	{
		PDB_mesh p = obj.GetComponent<PDB_mesh> ();
		Rigidbody ri = obj.AddComponent<Rigidbody> ();
		PDB_molecule mol = p.mol;

		float mass = 1000;
		float r = mol.bvh_radii [0] * 0.5f;
		float val = 0.4f * mass * r * r;

		ri.drag = 2f;
		ri.angularDrag = 5f;
		ri.useGravity = false;
		ri.mass = mass;
		ri.inertiaTensor = new Vector3 (val, val, val);

		obj.transform.Translate ((mol.bvh_radii [0] * xoffset) * 0.7f, 0, 0);
		//obj.transform.Translate (xoffset * 30, 0, 0);
		obj.transform.Rotate (0, 0, 270);
		obj.transform.Translate (mol.pos.x, mol.pos.y, mol.pos.z);

		//obj.transform.rotation = Random.rotation;

		//if this is the simple game load in labels from the file
		if (simpleGame) {
			for (int i=0; i<mol.labels.Length; ++i) {
				if (mol.labels [i].atomIds.Count > 0) {
					CreateLabel (p.mol.labels [i], molNum, "Label" + i + "mol" + name, mol);
				}
			}
		}
	}


	//applies forces to both molecules to return them to their respective origins
	void ApplyReturnToOriginForce ()
	{
		for (int i = 0; i < molecules.Length; ++i) {
			Vector3 molToOrigin = originPosition [i] - molecules [i].transform.position;
			if (molToOrigin.sqrMagnitude > 1.0f) {
				Rigidbody rb = molecules [i].GetComponent<Rigidbody> ();
				rb.AddForce (molToOrigin.normalized * repulsiveForce);
			}
		}
	}

	public void DebugDock()
	{
		if (molecules [0] && molecules [1]) {
			molecules[0].GetComponent<PDB_mesh>().AutoDockCheap();
			molecules[1].GetComponent<PDB_mesh>().AutoDockCheap();
			repulsiveForce = 0;
			Debug.Log("Docking");
		}

	}

	// create both molecules
	void make_molecules(bool init, MeshTopology mesh_type) {
		//Debug.Log ("make_molecules");
		string file = filenames [current_level];
		
		GameObject mol1 = make_molecule (file + ".1", "Proto1", 7, mesh_type);
		GameObject mol2 = make_molecule (file + ".2", "Proto2", 7, mesh_type);

		if (init) {
			molecules = new GameObject[2];
			molecules [0] = mol1.gameObject;
			molecules [1] = mol2.gameObject;

			float offset = -1;
			int molNum = 0;
			foreach (GameObject obj in molecules)
			{
				reset_molecule(obj, offset, molNum++);
				offset += 2;
			}
		}
	}

	//main meat of the initilisation logic and level completion logic
	IEnumerator game_loop ()
	{
		// for each level
		for (;;) {
			Debug.Log ("start level " + current_level);
			//if true, we have no more levels listed in the vector
			//to be replaced with level selection. Talk to andy on PDB file selection
			if (current_level >= filenames.Count) {
				Debug.LogError ("No next level");
				current_level = 0;
			}

			if (lockButton) {
				//lockButton.gameObject.SetActive(false);
				lockButton.interactable = false;
			}

			make_molecules (true, MeshTopology.Triangles);

			// This is very grubby, must generalise.
			GameObject mol1 = molecules [0];
			GameObject mol2 = molecules [1];

			originPosition [0] = mol1.transform.position;
			originPosition [1] = mol2.transform.position;

			for (int i = 0; i < activeLabels.Count; ++i) {
				activeLabels [i].gameObject.SetActive (false);
			}

			ClockTimer playerClock = gameObject.GetComponent<ClockTimer> ();
			playerClock.ResetTimer ();
			playerClock.timeText.enabled = false;
		
			PDB_mesh p1 = mol1.GetComponent<PDB_mesh> ();
			PDB_mesh p2 = mol2.GetComponent<PDB_mesh> ();

			//create the win condition from the file specified paired atoms
			for (int i=0; i<p1.mol.pairedLabels.Length; ++i) {
				winCondition.Add (new Tuple<int,int> (p1.mol.pairedLabels [i].First,
				                 p1.mol.pairedLabels [i].Second));
			}
			//debug 3D texture
			//GameObject.Find ("Test").GetComponent<Tex3DMap> ().Build (p1.mol);
			p1.other = p2.gameObject;
			p2.other = p1.gameObject;
			p1.gameObject.SetActive (false);
			p2.gameObject.SetActive (false);
		
			//pop the molecules in for a visually pleasing effect
			PopInSound (mol1.gameObject);
			yield return new WaitForSeconds (0.2f);
			PopInSound (mol2.gameObject);

			for (int i = 0; i < activeLabels.Count; ++i) {
				yield return new WaitForSeconds (0.1f);
				PopInSound (activeLabels [i].gameObject);
			}

			//this is the connection manager for the complex game, it handles grappling between the molecules
			ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();

			for (int i = 0; i < dockSliders.Count; ++i) {
				dockSliders[i].maxValue =  conMan.maxDistance;
				dockSliders[i].minValue = conMan.minDistance;
				dockSliders[i].value =conMan.maxDistance;
				//dockSliders[i].gameObject.SetActive(false);
				//sliderConstarinedByOverride.Add(true);
			}

			overrideSlider.maxValue = conMan.maxDistance;
			overrideSlider.minValue = conMan.minDistance;
			overrideSlider.value = conMan.maxDistance;
			
			overrideSlider.interactable = false;

			mol1.transform.localScale = new Vector3 (1, 1, 1);
			mol2.transform.localScale = new Vector3 (1, 1, 1);
			yield return new WaitForSeconds (0.1f);
			eventSystem.enabled = true;


			// Enter waiting state
			game_state = GameState.Waiting;

			//any input should start the timer
			while (!Input.anyKey || !playerClock.clockStopped) {
				// start the new frame
				yield return new WaitForEndOfFrame();
			}

			/*if (goSplash) {
				PopInWaitDisappear (goSplash, 1.0f);
			}*/

			playerClock.StartPlayerTimer ();

			// Enter picking state
			game_state = GameState.Picking;
			bool prev_button = false;

			// In this loop, the game state is either Picking or Docking.
			while (game_state ==  GameState.Picking || game_state ==  GameState.Docking) {
				// start the new frame
				yield return new WaitForEndOfFrame();
				//Debug.Log ("gs = " + game_state);

				// measure the score
				float rms_distance_score = ScoreRMSD ();
				if (rmsScoreSlider) {
					rmsScoreSlider.value = rms_distance_score * 0.1f;
					float scaleGameScore = 1.0f - (rms_distance_score * 0.1f);
					if(scaleGameScore <= 1.0f && scaleGameScore > 0)
					{
						GameScore.localScale = new Vector3(scaleGameScore,scaleGameScore,scaleGameScore);						
						GameScoreValue.text = ((int)(scaleGameScore * 1250)).ToString();
					}
					else
					{
						GameScore.localScale = Vector3.zero;
						GameScoreValue.text = "0";
					}
				}

				if (lockButton) {
					//lockButton.gameObject.SetActive (rms_distance_score < winScore);
					lockButton.interactable = (rms_distance_score < winScore);
				}

				//test if we should move the molecules with quick ray casts
				if (eventSystem.IsActive ()) {
					ManageSliders ();
					bool cur_button = Input.GetMouseButton (0);
					if (cur_button && !prev_button) {
						//Using this system we dont allow the player to move the two molecules at the same time
						Camera c = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
						Ray r = c.ScreenPointToRay (Input.mousePosition);

						// Starting cooroutines is grubby, we should avoid this.
						if (PDB_molecule.collide_ray_quick (
							molecules [0],
							molecules [0].GetComponent<PDB_mesh> ().mol,
							molecules [0].transform,
							r)) {
							//uncomment this line to stop players moving two molecules at once
							//playerIsMoving[1]=false;#
							if (!playerIsMoving [0]) {
								playerIsMoving [0] = true;
								StartCoroutine ("PlayerMoveMolecule", 0);
							}
						} else {
							playerIsMoving [0] = false;
						} 

						if (PDB_molecule.collide_ray_quick (
							molecules [1],
							molecules [1].GetComponent<PDB_mesh> ().mol,
							molecules [1].transform,
							r)) {

							//uncomment this line to stop players moving two molecules at once
							//playerIsMoving[0]=false;
							if (!playerIsMoving [1]) {
								playerIsMoving [1] = true;
								StartCoroutine ("PlayerMoveMolecule", 1);
							}
						} else {
							playerIsMoving [1] = false;
						}
					}

					if (sites [0]) {
						sites [0].transform.rotation = molecules [0].transform.rotation;
						if (simpleGame) {
							sites [0].transform.Rotate (new Vector3 (0, 90, 0), Space.World);
						}
					}
					if (sites [1]) {
						sites [1].transform.rotation = molecules [1].transform.rotation;
						if (simpleGame) {
							sites [1].transform.Rotate (new Vector3 (0, -90, 0), Space.World);
						}
					}
					prev_button = cur_button;
				}

			}

			Debug.Log ("exited docking loop " + game_state);

			if (game_state == GameState.Locked) {
				Debug.Log ("locking");

				for (int i = 0; i < activeLabels.Count; ++i) {
					PopOut (activeLabels [i].gameObject);
				}

				eventSystem.enabled = false;

				//StartCoroutine("DockingOneAxis");
				if (sites [0]) {
					PopOut (sites [0]);
				}

				if (sites [1]) {
					PopOut (sites [1]);
				}

				//lockButton.gameObject.SetActive(false);
				lockButton.interactable = false;
					
				Debug.Log ("Docked");

				this.GetComponent<AudioManager> ().Play ("Win");

				GameObject parent = new GameObject ();
				Rigidbody r = parent.AddComponent<Rigidbody> ();
				molecules [0].transform.SetParent (parent.transform, true);
				molecules [1].transform.SetParent (parent.transform, true);
				
				r.angularDrag = 1.0f;
				r.constraints = RigidbodyConstraints.FreezePosition;
				r.useGravity = false;
				parent.name = "MoveableParent";

				//this is to stop the molecules rumbling around as they inherit the pearents velocity
				Component.Destroy (molecules [0].GetComponent<Rigidbody> ());
				Component.Destroy (molecules [1].GetComponent<Rigidbody> ());
					
				//StartCoroutine ("WinSplash", new Vector3 (0, 0, 0));
				GameObject.Destroy (sites [0]);
				GameObject.Destroy (sites [1]);

				Debug.Log("current_level=" + current_level);
				current_level++;
				if (current_level == filenames.Count) {
					Debug.Log ("End of levels");
					current_level = 0;
				}
				conMan.Reset();
				Reset ();
				molecules = null;
				activeLabels.Clear();
			}
		}
	}

	public int work_done = 0;

	// Physics simulation
	void FixedUpdate() {
		num_touching_0 = 0;
		num_touching_1 = 0;
		num_invalid = 0;
		num_connections = 0;
		
		if (game_state == GameState.Docking && molecules.Length >= 2) {
			// Get a list of atoms that collide.
			GameObject obj0 = molecules[0];
			GameObject obj1 = molecules[1];
			PDB_mesh mesh0 = (PDB_mesh)obj0.GetComponent<PDB_mesh>();
			PDB_mesh mesh1 = (PDB_mesh)obj1.GetComponent<PDB_mesh>();
			Rigidbody r0 = obj0.GetComponent<Rigidbody>();
			Rigidbody r1 = obj1.GetComponent<Rigidbody>();
			Transform t0 = obj0.transform;
			Transform t1 = obj1.transform;
			PDB_molecule mol0 = mesh0.mol;
			PDB_molecule mol1 = mesh1.mol;
			GridCollider b = new GridCollider(mol0, t0, mol1, t1, 0);
			work_done = b.work_done;

			BitArray ba0 = new BitArray (mol0.atom_centres.Length);
			BitArray ba1 = new BitArray (mol1.atom_centres.Length);

			// Apply forces to the rigid bodies.
			foreach (GridCollider.Result r in b.results) {
				Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
				Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
				float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
				float distance = (c1 - c0).magnitude;
				
				num_connections++;

				if (distance < min_d) {
					Vector3 normal = (c0 - c1).normalized * (min_d - distance);
					normal *= seperationForce;
					r0.AddForceAtPosition(normal,c0);
					r1.AddForceAtPosition(-normal, c1);

					if (!ba0[r.i0]) { num_touching_0++; ba0.Set(r.i0, true); }
					if (!ba1[r.i1]) { num_touching_1++; ba1.Set(r.i1, true); }
					if (distance < min_d * 0.5) {
						num_invalid++;
					}
				}
				
			}

			heuristicScoreSlider.value = num_invalid != 0 ? 1.0f : 1.0f - (num_touching_0 + num_touching_1) * 0.013f;
			
			ScoreScaleSize = (num_touching_0 + num_touching_1) * 0.013f;

			heuristicScore.localScale = num_invalid != 0 ? new Vector3(0,0,0) : new Vector3(ScoreScaleSize,ScoreScaleSize,ScoreScaleSize);

		}

		invalidDockText.enabled = num_invalid != 0;

		if (eventSystem != null && eventSystem.IsActive ()) {
			ApplyReturnToOriginForce ();
		}
	}

	public void ToogleToolMenu(bool Status)
	{
		ToolMenuAnimator.SetBool ("Open", Status);
		OpenToolImage.SetActive (!Status);
		CloseToolImage.SetActive (Status);
	}

	public Scrollbar ScrollbarAmino1;
	public Scrollbar ScrollbarAmino2;

	public void ScrollUpAmino1()
	{
		ScrollbarAmino1.value += 0.01f;
	}

	public void ScrollDownAmino1()
	{
		ScrollbarAmino1.value -= 0.01f;		
	}

	public void ScrollUpAmino2()
	{
		ScrollbarAmino2.value += 0.01f;	
	}

	public void ScrollDownAmino2()
	{
		ScrollbarAmino2.value -= 0.01f;	
	}

}

