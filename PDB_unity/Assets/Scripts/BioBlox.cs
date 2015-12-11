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



	// the win and loose spash images
	//public GameObject winSplash;
	//public GameObject looseSplash;
	public GameObject goSplash;

	// a variable controlling the size of the area faded out during the win state
	public float shaderKVal = -0.03f;
	
	// a bool that will exit the win splash if set to true
	public bool exitWinSplash = false;
	
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
	public Image heuristicScore;
	public Text GameScoreValue;
	public Image GameScore;
	//public Slider overrideSlider;
	public Slider cutawaySlider;
	public GameObject invalidDockText;
	public GameObject InvalidDockScore;
	//public List<Slider> dockSliders = new List<Slider> ();
	public float dockOverrideOffset = 0.0f;
	//Animator of the tools menu
	public Animator ToolMenuAnimator;
	public GameObject OpenToolImage;
	public GameObject CloseToolImage;
	public GameObject EndLevelMenu;

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

	public Camera MainCamera;

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

//		scoreCard = GameObject.Find ("ScoreCard").GetComponent<ScoreSheet> ();
		//scoreCard.gameObject.SetActive (false);

		//game_loop loads the file at filenames[current_level]
		eventSystem = EventSystem.current;

		StartCoroutine (game_loop ());
		GetComponent<AminoSliderController> ().init ();
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

		/*
		//camera zoom roll mouse
		if (Input.GetAxis ("Mouse ScrollWheel") != 0)
		{
			if(MainCamera.transform.position.z >= -120 && MainCamera.transform.position.z <= 0)
			{
				Vector3 temp = MainCamera.transform.position;
				temp.z += Input.GetAxis ("Mouse ScrollWheel") * 5;
				MainCamera.transform.position = temp;
			}

			if(MainCamera.fieldOfView < 20)
			{
				MainCamera.fieldOfView = 20;
			}

			if(MainCamera.fieldOfView > 60)
			{
				MainCamera.fieldOfView = 60;
			}
		}*/
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


	/*public void ManageSliders()
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
	}*/
	/*
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
	}*/

	// The pick button resets the sliders to the right.
	/*public void Pick()
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
	}*/
	
	// The lock button establishes the Locked state.
	public void Lock()
	{
		if (game_state == GameState.Docking) {
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

		game_state = GameState.Picking;

		//Clear score
		GameScore.fillAmount = 0;
		heuristicScore.fillAmount = 0;
		GameScoreValue.text = "0";
		MainCamera.fieldOfView = 60;
		
		GetComponent<AminoSliderController> ().init ();
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
	GameObject make_molecule (string name, string proto, int layerNum, MeshTopology mesh_type, int index)
	{
		GameObject obj = GameObject.Find (name);
		if (obj == null) {
			obj = new GameObject ();
		}

		obj.name = name;
		obj.SetActive (true);
		//the layer numbers are to seperate the molecules in certain cameras
		obj.layer = layerNum;

		//just the one PDB_mesh component should be sufficient...
		PDB_mesh p = obj.GetComponent<PDB_mesh>();
		if (!p) p = obj.AddComponent<PDB_mesh>();

		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		p.protein_id = index;
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
		/*if (simpleGame) {
			for (int i=0; i<mol.labels.Length; ++i) {
				if (mol.labels [i].atomIds.Count > 0) {
					CreateLabel (p.mol.labels [i], molNum, "Label" + i + "mol" + name, mol);
				}
			}
		}*/
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
		
		GameObject mol1 = make_molecule (file + ".1", "Proto1", 7, mesh_type,0);
		GameObject mol2 = make_molecule (file + ".2", "Proto2", 7, mesh_type,1);

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

			//this is the connection manager for the complex game, it handles grappling between the molecules
			ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
			/*
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
			
			overrideSlider.interactable = false;*/

			mol1.transform.localScale = new Vector3 (1, 1, 1);
			mol2.transform.localScale = new Vector3 (1, 1, 1);
			yield return new WaitForSeconds (0.1f);
			eventSystem.enabled = true;


			// Enter waiting state
			game_state = GameState.Waiting;

			/*if (goSplash) {
				PopInWaitDisappear (goSplash, 1.0f);
			}*/

			// Enter picking state
			game_state = GameState.Picking;

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
						GameScore.fillAmount = scaleGameScore;						
						GameScoreValue.text = ((int)(scaleGameScore * 1250)).ToString();
					}
					else
					{
						GameScore.fillAmount = 0;
						GameScoreValue.text = "0";
					}
				}

				if (lockButton) {
					//lockButton.gameObject.SetActive (rms_distance_score < winScore);
					lockButton.interactable = (rms_distance_score < winScore);
				}
			}

			Debug.Log ("exited docking loop " + game_state);

			if (game_state == GameState.Locked) {
				Debug.Log ("locking");

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
				//GetComponent<AminoButtonController> ().EmptyAminoSliders ();
				//EndLevelMenu.SetActive(true);
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
		//Debug.Log ("game_state=" + game_state + "molecules.Length=" + molecules.Length);
		
		if (molecules.Length >= 2) {
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

			//heuristicScoreSlider.value = num_invalid != 0 ? 1.0f : 1.0f - (num_touching_0 + num_touching_1) * 0.013f;
			
			ScoreScaleSize = (num_touching_0 + num_touching_1) * 0.013f;

			GameScore.fillAmount = num_invalid != 0 ? GameScore.fillAmount + 0.01f : GameScore.fillAmount - 0.01f;
			//Debug.Log ("num_touching_0: "+num_touching_0+" / num_touching_1: "+num_touching_1);
			//Debug.Log ("num_invalid: "+num_invalid);

		}

		invalidDockText.SetActive(num_invalid != 0);
		InvalidDockScore.SetActive(num_invalid != 0);


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

}

