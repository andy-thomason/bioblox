using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BioBlox : MonoBehaviour
{

	//dictaing which gametype, puzzle or museum
	bool simpleGame = true;

	//controls whether the win state should attempt to fade out the molecules
	public bool winShouldFadeMol = false;

	//score card for saving scores. 
	ScoreSheet scoreCard;

	//filenames for the levels, without the .txt
	public List<string> filenames = new List<string> ();
	//the level we are currently on, incremented at the end of a level
	int filenameIndex = 0;
	//a holder variable for the current event system
	EventSystem eventSystem;
	//prefab of the labels used to point to atoms on the molecules, must have label script attached
	public GameObject prefabLabel;
	//the win and loose spash images
	public GameObject winSplash;
	public GameObject looseSplash;
	public GameObject goSplash;
	//a variable controlling the size of the area faded out during the win state
	public float shaderKVal=-0.03f;
	
	//a bool that will exit the win splash if set to true
	public bool exitWinSplash=false;

	//all labels which are currently in the scene
	List<LabelScript> activeLabels = new List<LabelScript> ();
	//a number of selected labels, at the moment limited to 2, one from each molecule
	LabelScript[] selectedLabelIndex = new LabelScript[2];
	//a list of win conditions where two atoms must be paired
	List<Tuple<int,int>> winCondition = new List<Tuple<int,int>> ();
	//the molecules in the scene
	public GameObject[] molecules;

	// NOT CURRENT IN USE
	// sites are smaller regions of the molecules that can be selected and manipulated independtly from the molecules
	GameObject[] sites = new GameObject[2];
	// wheter the player is moving the molecules, playerIsMoving[0] being molecule[0]
	bool[] playerIsMoving = new bool[2]{false,false};
	//the original positions of the molecules, used to provide a returning force during the puzzle mode
	Vector3[] originPosition = new Vector3[2];
	//game object target of the "popping" co-routines to shrink and grow the object out of and into the scene
	GameObject popTarget;

	//current score
	public float score=10.0f;
	//score to achive to win
	public float winScore=10.0f;
	//torque applied to the molecule to move them when player drags
	public float uiScrollSpeed = 10.0f;
	//force being applied to molecules to return them to their origin positions
	public float repulsiveForce = 30.0f;

	//whether we have won or lost
	bool win = false;
	bool loose = false;

	//colors of the labels and an offset that is randomly decided randomize colours
	List<Color> colorPool=new List<Color>();
	int randomColorPoolOffset;
	
	int numLinks;



	// Use this for initialization
	void Start ()
	{
		numLinks = 0;
		colorPool.Add (Color.red);
		colorPool.Add (Color.blue);
		colorPool.Add (Color.cyan);
		colorPool.Add (Color.green);
		colorPool.Add (Color.magenta);
		colorPool.Add (Color.yellow);
		colorPool.Add (Color.white);
		colorPool.Add (Color.gray);
		colorPool.Add (new Color (1.0f, 0.5f, 0.1f));
		randomColorPoolOffset = Random.Range (0, colorPool.Count-1);
		Debug.Log ("Start");
		//filenames.Add ("jigsawBlue");

		//filenames.Add ("betabarrel_b");
		filenames.Add ("pdb2ptcWithTags");

		filenames.Add ("1GCQ_bWithTags");


		filenames.Add ("pdb2ptcWithTags");

		scoreCard = GameObject.Find ("ScoreCard").GetComponent<ScoreSheet> ();
		scoreCard.gameObject.SetActive (false);

		//game_loop loads the file at filenames[filenameIndex]
		StartCoroutine (game_loop ());
		eventSystem = EventSystem.current;

		winSplash.SetActive (false);
		looseSplash.SetActive (false);
		goSplash.SetActive (false);
	}

	public string GetCurrentLevelName()
	{
		return filenames [filenameIndex];
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
	public void GetLabelPos (int atomID,int molNum, Transform t)
	{

		Vector3 atomPos = GetAtomPos (atomID,molNum);

		//if the molecule does not exist, or the atom id is bad
		if (molNum == -1) {
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
		transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
		atomPosW = transToAt + molMesh.transform.position;
		t.position = atomPosW;
		t.position += new Vector3 (0, 0, -10);
	}

	//returns an atoms local position from a atom index and molecule index	
	public Vector3 GetAtomPos (int atomID,int molNum)
	{
		if (molecules.Length == 2) {

			if (atomID == -1) {
				Debug.LogError ("Bad index");
			}
			if(molecules [molNum] == null)
			{
				return Vector3.zero;
			}
			return  molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID];
		}
		return new Vector3 (0, 0, 0);
	}

	//returns an atoms world position from atom index and molecule index
	public Vector3 GetAtomWorldPos(int atomID, int molNum)
	{
		if (molecules.Length == 2) {
			if (atomID == -1) {
				Debug.LogError ("Bad index");
			}
			return  molecules[molNum].transform.TransformPoint(
				molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID]);
		}
		return new Vector3 (0, 0, 0);
	}
    

	//co-routine to manage player interaction with molecules
	IEnumerator PlayerMoveMolecule(int molIndex)
	{
		GameObject mol = molecules [molIndex];
		Rigidbody r = mol.GetComponent<Rigidbody> ();
		r.maxAngularVelocity = 4;
		//player movement of the molecules times out after periods of no input
		float timeout = 100.0f;
		playerIsMoving [molIndex] = true;
		Vector3 lastMousePos = Input.mousePosition;
		for (float t=0.0f; t<timeout; t+=Time.deltaTime) {
			if(playerIsMoving[molIndex]==false||
			   eventSystem.IsActive()==false)
			{
				//breakout if we the event system is deactivated or we are told to stop moving
				break;
			}
			if(Input.GetMouseButton(0)) //left mouse button
			{
				Camera c = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
				Ray ray = c.ScreenPointToRay(Input.mousePosition);
				//create a ray to the cursor and cast it, if it hits at all
				int atomID = PDB_molecule.collide_ray(mol,mol.GetComponent<PDB_mesh>().mol,
				                                      mol.transform,
				                                      ray);
				if(atomID != -1)
				{
					if(t>0.3f)
					{
						//if there is no recent input reset previous position
						lastMousePos=Input.mousePosition;
					}
					t=0.0f;
					Vector3 mousePos=Input.mousePosition;
					Vector3 mouseDelta=mousePos-lastMousePos;
					//find the world position of the atom
					Vector3 atomPos =GetAtomWorldPos(atomID,molIndex);


					//add torque based on mouse delta.
					//UPGRADE this to adding forces to the atom hit by a ray
					r.AddForceAtPosition(new Vector3(mouseDelta.x,mouseDelta.y,0)*uiScrollSpeed,
					                     atomPos);
					lastMousePos=mousePos;
				}
			}
			yield return null;
		}
		if (selectedLabelIndex [molIndex]&&eventSystem.IsActive()) {
			//if there is a selected label for our molecule return it to that orientation
			LabelClicked (selectedLabelIndex [molIndex].gameObject);
		}
		playerIsMoving[molIndex]=false;
		r.angularVelocity=new Vector3(0,0,0);
		yield break;
	}

	public void Reload()
	{
		filenameIndex++;
		Reset ();
		if (filenameIndex == filenames.Count) {
			Debug.Log ("End of levels");
		}
		else{
			StartCoroutine (game_loop ());
		}
	}

	// Update handles (badly) a few things that dont fit anywhere else.
	void Update ()
	{

	}

	void PopInSound(GameObject g)
	{
		this.GetComponent<AudioManager> ().Play ("Blip");
		PopIn (g);
	}

	public void PopIn (GameObject g)
	{
		popTarget = g;
		StartCoroutine ("PopInCo");
	}

	public void PopInWaitDisappear(GameObject g, float waitTime)
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

	IEnumerator PopInWaitDisappearCo(float waitTime)
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
	public float ScoreRMSD()
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
			if(ligand.mol.names[i] == PDB_molecule.atom_C)
			{
				rmsd += (transMat.MultiplyPoint3x4(ligand.mol.atom_centres[i] + offset) - ligand.mol.atom_centres[i]).sqrMagnitude;
				count++;
			}
		}
		return Mathf.Sqrt (rmsd / count);
	}


	//changes variables in the shader to fade the molecules everywhere but two points
	//this is to emphisie the docking site
	IEnumerator FadeMolecules()
	{
		GameObject mol1 = molecules [0];
		GameObject mol2 = molecules [1];
		MeshRenderer[] meshes1 = molecules [0].GetComponentsInChildren<MeshRenderer> ();
		MeshRenderer[] meshes2 = molecules [1].GetComponentsInChildren<MeshRenderer> ();
		PDB_molecule molInfo1 = mol1.GetComponent<PDB_mesh> ().mol;
		PDB_molecule molInfo2 = mol2.GetComponent<PDB_mesh> ().mol;

		//it is necessary to loop through the children as the mesh may be split into seveal sections
		//due to vertex number limitations in unity
		for (int i=0; i<meshes1.Length; ++i) {
			meshes1 [i].material.SetVector ("_CullPos", 
			                               new Vector3(0,0,0));
		}
		for (int i=0; i<meshes2.Length; ++i) {
			meshes2 [i].material.SetVector ("_CullPos",
			                               new Vector3(0,0,0));
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
	}


	//Meant for the simple game
	//Checks to see if the win-conditions are satisfied
	//The win conditions are order specific, .first is molecule[0] atomIndex
	public void CheckPair ()
	{
		if (simpleGame && winCondition.Count > 0 && selectedLabelIndex [0] && selectedLabelIndex [1]) {
			bool hasWon = true;
			
			
			for (int i=0; i<winCondition.Count; ++i) {
				int winCon1 = winCondition [i].First;
				int winCon2 = winCondition [i].Second;
				
				int selected1 = selectedLabelIndex [0].labelID;
				int selected2 = selectedLabelIndex [1].labelID;
				
				if (!(winCon1 == selected1 && winCon2 == selected2) &&
				    !(winCon1 == selected2 && winCon2 == selected1)) {
					hasWon = false;
					break;
				}
			}
			if (hasWon) {
				win = true;
			} else {
				loose = true;
			}
			this.GetComponent<AudioManager>().Play("Drum");
		}
	}

	//Interactive win splash state before transitioning to the next level. 
	IEnumerator WinSplash (Vector3 focusPoint)
	{
		ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
		conMan.Reset ();
		//disable label clicking and other interactions
		eventSystem.enabled = false;
		//save the players time to complete the level
		gameObject.GetComponent<ClockTimer> ().LogPlayerTime ();
		gameObject.GetComponent<ClockTimer> ().StopPlayerTimer ();
		//put animation for win splash here
		for(int i=0;i<activeLabels.Count;++i)
		{
			PopOut(activeLabels[i].gameObject);
		}
		activeLabels.Clear();


		PopIn (winSplash);
		//if gene is in the scene make him shrink away
		//this is a bit of legacy when we used to have a mascot for the game. Fun times
		/*GameObject gene = GameObject.Find ("Gene");
		if (gene) {
			gene.SetActive (false);
		}*/

		//create a pearent that is moved to rotate both molecules on player interaction
		Camera c = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		GameObject parent = new GameObject ();
		Rigidbody r=parent.AddComponent<Rigidbody> ();
		molecules [0].transform.SetParent (parent.transform, true);
		molecules [1].transform.SetParent (parent.transform, true);

		r.angularDrag = 1.0f;
		r.constraints = RigidbodyConstraints.FreezePosition;
		r.useGravity = false;
		parent.name = "MoveableParent";

		if (winShouldFadeMol) {
			StartCoroutine ("FadeMolecules");
		}
		//values to sort out camera rotation and zoom
		float zoomValue = 30.0f;
		float radRot = (3.0f * Mathf.PI) / 2;
		float camRot = 0;
		Vector3 start = c.transform.position;
		Vector3 moveDir = focusPoint+ new Vector3 (0,0, -30) - c.transform.position;
		//zoom in the camera
		for (float t=0; t<1.0f; t+=Time.deltaTime) {
			c.transform.position = start + moveDir * t;
			yield return null;
		}
		//if we just won the simple version then we need to destroy the spring joints that docked the molecules
		if (simpleGame) {
			SpringJoint [] joints = molecules [0].GetComponents<SpringJoint> ();
			for (int i=0; i<joints.Length; ++i) {
				Component.Destroy (joints [i]);
			}
		}
		//this is to stop the molecules rumbling around as they inherit the pearents velocity
		Component.Destroy (molecules [0].GetComponent<Rigidbody> ());
		Component.Destroy (molecules [1].GetComponent<Rigidbody> ());

		//the winstate expires regardless of player input
		float timeoutTimer = 40.0f;
		// and auto-rotate will resume after a period of inactivity
		const float nonInteractionTimeOut = 2.0f;
		float nonInteractionTimer = 0.0f;
		bool autoRotate = true;
		
		Vector3 oldMousePos = Input.mousePosition;

		// the molecules will be auto-rotate around untill there is some player input
		while(true){
			nonInteractionTimer += Time.deltaTime;
			timeoutTimer -= Time.deltaTime;
			if(nonInteractionTimer>nonInteractionTimeOut)
			{
				autoRotate=true;
			}
			if(Input.GetMouseButton(0))
			{
				if(nonInteractionTimer>0.3f)
				{
					oldMousePos=Input.mousePosition;
				}
				nonInteractionTimer=0;
				autoRotate=false;
				Vector3 mousePos=Input.mousePosition;
				Vector3 mouseDelta=mousePos-oldMousePos;
				oldMousePos=mousePos;
				r.AddTorque(new Vector3(mouseDelta.y,-mouseDelta.x,0));
			}
			if(autoRotate)
			{
				Vector3 dir = new Vector3(
					Mathf.Cos(radRot),
					0,
					Mathf.Sin(radRot));
				dir=dir.normalized*zoomValue;
				c.transform.position=dir;
				c.transform.rotation =Quaternion.LookRotation(focusPoint-c.transform.position);
				radRot+=Time.deltaTime;
				camRot+=Time.deltaTime;
			}
			if((Input.anyKeyDown&&!Input.GetMouseButtonDown(0))||exitWinSplash||timeoutTimer<0)
			{
				exitWinSplash=false;
				break;
			}
			yield return null;
		}

		//clearing out the scene for the reset
		PopOut (molecules[0].gameObject);
		PopOut (molecules[1].gameObject);
		PopOut (winSplash);
		yield return new WaitForSeconds (1.0f);
		GameObject.Destroy (molecules[0].gameObject);
		GameObject.Destroy (molecules[1].gameObject);
		GameObject.Destroy (parent);
		/*
		if (gene) {
			gene.SetActive (true);
		}*/

		//transform the camera to its original position
		eventSystem.enabled = true;
		scoreCard.ScorePlayer ();

		
		Vector3 target = start;
		start = c.transform.position;
		Quaternion startQ = c.transform.rotation;
		for (float t=0; t<1.0f; t+=Time.deltaTime) {
			c.transform.position=Vector3.Lerp(start,target,t);
			c.transform.rotation=Quaternion.Slerp(startQ,Quaternion.identity,t);
			yield return null;
		}


		Debug.Log ("Reloading");
		yield break;
	}

	public void SiteClicked (GameObject labelObj)
	{
		activeLabels.Remove (labelObj.GetComponent<LabelScript> ());
		GameObject.Destroy (labelObj);

		//this is site picking code, it basically takes atoms local to the selected one
		//and creates a new mesh with that subset
		/*
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
	void CreateLabel (List<int> atomIds, int molNum, string labelName)
	{
		GameObject newLabel = GameObject.Instantiate<GameObject> (prefabLabel);
		if (!newLabel) {
			Debug.Log ("Could not create Label");
		}
		LabelScript laSc = newLabel.GetComponent<LabelScript> ();
		//assigned the label a new color from a random range
		newLabel.GetComponent<Image> ().color = colorPool[(activeLabels.Count+randomColorPoolOffset) % colorPool.Count];
		newLabel.GetComponent<Light> ().color = newLabel.GetComponent<Image> ().color;
		laSc.atomIds = atomIds;
		laSc.moleculeNumber = molNum;
		if (molNum==1) {
			Vector3 scale = laSc.transform.localScale;
			scale.x *= -1;
			laSc.transform.localScale = scale;
			
		}
		laSc.owner = this;
		laSc.labelID = activeLabels.Count;
		//3D and 2D arrows see LabelScript
		laSc.is3D = true;
		//GameObject foundObject = GameObject.Find ("Label" + (activeLabels.Count + 1));
		//newLabel.transform.position = foundObject.transform.position;

		//we group all the labels under a single empty transform for convienence in the unity hierarchy
		GameObject canvas = GameObject.Find ("Labels");
		newLabel.transform.SetParent (canvas.transform);
		newLabel.name = labelName + laSc.labelID;
		activeLabels.Add (laSc);
	}

	//manages a left click on a label
	//has two different functions depending on the gametype eg( simple or complicated)
	public void LabelClicked (GameObject labelObj)
	{
		Debug.Log (labelObj.name + " was clicked");
		LabelScript script = labelObj.GetComponent<LabelScript> ();
		
		int molNum = script.moleculeNumber;
		int index = script.labelID;

		PDB_mesh pdbMesh = molecules [molNum].GetComponent<PDB_mesh> ();

		if (simpleGame) {
			//here we update the selected label list
			//and align the molecule so that the labeled atom is facing the other molecule
			Vector3 atomPos = pdbMesh.mol.atom_centres[index];
			int otherMolNum = 1 - molNum;
			Vector3 alignDir = molecules [otherMolNum].transform.position -
				molecules [molNum].transform.position;
			pdbMesh.AlignAtomToVector(index,alignDir);
			//logic to select which labels should glow
			if (selectedLabelIndex [0]) {
				selectedLabelIndex [0].shouldGlow = false;
			}
			if (selectedLabelIndex [1]) {
				selectedLabelIndex [1].shouldGlow = false;
			}
			selectedLabelIndex [molNum] = script;
			if (selectedLabelIndex [0]) {
				selectedLabelIndex [0].shouldGlow = true;
			}
			if (selectedLabelIndex [1]) {
				selectedLabelIndex [1].shouldGlow = true;
			}
			//set the sector we clicked to glow, this is done in the shader to stop us having to rebuild the mesh with new colours
			MeshRenderer[] meshes = molecules [molNum].GetComponentsInChildren<MeshRenderer> ();
			foreach (MeshRenderer r in meshes)
			{
				r.material.SetVector("_GlowPoint",atomPos);
				r.material.SetFloat("_GlowRadius",5.0f);
			}
		} else {
			ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();
			if (conMan.RegisterClick (molecules [molNum].GetComponent<PDB_mesh> (), index)) {
				if(numLinks==0)
				{
					selectedLabelIndex[numLinks]=script;
					script.shouldGlow = true;
					numLinks++;
				}
				else{
					selectedLabelIndex[0].shouldGlow=false;
					numLinks=0;
				}
			
			}
		}
		//Handle label click, make active, focusd on atom //etc
	}
	

	void Reset ()
	{
		//clears the molecules and re-randomizes the colour range
		randomColorPoolOffset = Random.Range (0, colorPool.Count - 1);
		GameObject.Destroy (molecules [0].gameObject);
		GameObject.Destroy (molecules [1].gameObject);

		//clear the old win condition
		winCondition.Clear ();

		//deactivates any states
		win = false;
		loose = false;

		//clears the selected index
		selectedLabelIndex [0] = null;
		selectedLabelIndex [1] = null;
	}

	//since a molecule may be too large for one mesh we may have to make several
	void make_molecule_mesh(PDB_mesh mesh, string proto,int layerNum)
	{
		GameObject pdb = GameObject.Find (proto);
		MeshRenderer pdbr = pdb.GetComponent<MeshRenderer> ();


		for(int i=0;i<mesh.mol.mesh.Length;++i)
		{
			Mesh cur=mesh.mol.mesh[i];
			GameObject obj = new GameObject();
			obj.name=cur.name;
			obj.layer=layerNum;
			MeshFilter f = obj.AddComponent<MeshFilter> ();
			MeshRenderer r = obj.AddComponent<MeshRenderer> ();
			r.material = pdbr.material;
			f.mesh=cur;
			obj.transform.SetParent(mesh.transform);
			obj.transform.position=Vector3.zero;
		}
	}

	//creates the molecule objects including the PDB_mesh script
	GameObject make_molecule (
		string name, string proto, float xoffset, int layerNum
	)
	{
		GameObject obj = new GameObject ();
		obj.SetActive (true);
		obj.name = name;
		//the layer numbers are to seperate the molecules in certain cameras
		obj.layer = layerNum;

		PDB_mesh p = obj.AddComponent<PDB_mesh> ();
		Rigidbody ri = obj.AddComponent<Rigidbody> ();

		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		make_molecule_mesh (p,proto,layerNum);
		Debug.Log (mol.mesh[0].vertices.Length);

		//the complex game requires a much harsher seperation force
		//the speration force is the force applied when the molecules inter-penetrate to seperate them
		if (simpleGame) {
			p.seperationForce = 15.0f;
		} else {
			p.seperationForce = 200.0f;
		}
		ri.drag = 2f;
		ri.angularDrag = 5f;
		ri.useGravity = false;

		//save their original positions them move them to oppose oneanother
		//very messy, should really clean this up, either use points in the world or do it dynamically
		obj.transform.Rotate (0, 0, 270);
		Vector3 originMolPos = obj.transform.TransformPoint (mol.pos);
		Quaternion originalMolRot = obj.transform.rotation;
		obj.transform.Rotate (0, 0, -270); 
		obj.transform.Translate ((mol.bvh_radii[0]*xoffset)*0.7f, 0, 0);
		obj.transform.Rotate (0, 0, 270);
		obj.transform.Translate (mol.pos.x, mol.pos.y, mol.pos.z);
		//obj.GetComponent<TransformLerper> ().AddTransformPoint (obj.transform.rotation);

		//obj.GetComponent<TransformLerper> ().AddTransformPoint (originMolPos,
		//                                                       originalMolRot);
		//obj.GetComponent<TransformLerper> ().speed =1.0f;

		//extract the molecule number from the name .1 = 0 .2 = 1
		int molNum = (int)char.GetNumericValue (name [name.Length - 1])-1;
		obj.transform.rotation = Random.rotation;

		//if this is the simple game load in labels from the file
		if(simpleGame)
		{
			for (int i=0; i<mol.labels.Length; ++i) {
				CreateLabel (p.mol.labels[i],molNum,"Label"+ i +"mol"+name);
			}
		}
		return obj;
	}


	//applies forces to both molecules to return them to their respective origins
	void ApplyReturnToOriginForce()
	{
		for(int i = 0; i < molecules.Length; ++i)
		{
			Vector3 molToOrigin = originPosition[i] -molecules[i].transform.position;
			if(molToOrigin.sqrMagnitude>1.0f)
			{
				Rigidbody rb = molecules[i].GetComponent<Rigidbody>();
				rb.AddForce(molToOrigin.normalized* repulsiveForce);
			}
		}
	}
	

	//main meat of the initilisation logic and level completion logic
	IEnumerator game_loop ()
	{
		//if true, we have no more levels listed in the vector
		//to be replaced with level selection. Talk to andy on PDB file selection
		if (filenameIndex >= filenames.Count) {
			Debug.LogError ("No next level");
		}
		string file = filenames [filenameIndex];
		//create both molecules
		GameObject mol1 = make_molecule (file + ".1", "Proto1", -1,8);
		GameObject mol2 = make_molecule (file + ".2", "Proto2", 1,9);

		originPosition [0] = mol1.transform.position;
		originPosition [1] = mol2.transform.position;

		for (int i = 0; i < activeLabels.Count; ++i) {
			activeLabels [i].gameObject.SetActive (false);
		}

		ClockTimer playerClock = gameObject.GetComponent<ClockTimer> ();
		playerClock.ResetTimer ();
		playerClock.timeText.enabled = false;
	
		molecules = new GameObject[2];
		molecules [0] = mol1.gameObject;
		mol1.layer = 7;
		molecules [1] = mol2.gameObject;
		mol1.layer = 8;
		PDB_mesh p1 = mol1.GetComponent<PDB_mesh> ();
		PDB_mesh p2 = mol2.GetComponent<PDB_mesh> ();

		//create the win condition from the file specified paired atoms
		for (int i=0; i<p1.mol.pairedAtoms.Length; ++i) {
			winCondition.Add (new Tuple<int,int>(p1.mol.serial_to_atom[p1.mol.pairedAtoms[i].First],
			                  p2.mol.serial_to_atom[p1.mol.pairedAtoms[i].Second]));
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

		p1.shouldCollide = true;

		//this is the connection manager for the complex game, it handles grappling between the molecules
		ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();

		mol1.transform.localScale = new Vector3 (1, 1, 1);
		mol2.transform.localScale = new Vector3 (1, 1, 1);
		yield return new WaitForSeconds (0.1f);
		eventSystem.enabled = true;
		while (true) {
			if (Input.anyKey && playerClock.clockStopped){
				//any input should start the timer
				if(goSplash)
				{
					PopInWaitDisappear(goSplash,1.0f);
				}
				playerClock.StartPlayerTimer();
			}
			if(Input.GetKeyDown(KeyCode.L))
			{
				win=true;
			}
			//test if we should move the molecules with quick ray casts
			if (eventSystem.IsActive()) {
				if(Input.GetMouseButton(0))
				{
					
					//Using this system we dont allow the player to move the two molecules at the same time
					Camera c = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
					Ray r = c.ScreenPointToRay(Input.mousePosition);
					if(!playerIsMoving[0] && PDB_molecule.collide_ray(
						molecules[0],
						molecules[0].GetComponent<PDB_mesh>().mol,
						molecules[0].transform,
						r)!=-1)
					{
						//uncomment this line to stop players moving two molecules at once
						//playerIsMoving[1]=false;
						playerIsMoving[0]=true;
						StartCoroutine("PlayerMoveMolecule",0);
					}
					else if(!playerIsMoving[1] && PDB_molecule.collide_ray(
						molecules[1],
						molecules[1].GetComponent<PDB_mesh>().mol,
						molecules[1].transform,
						r)!=-1)
					{
						//uncomment this line to stop players moving two molecules at once
						//playerIsMoving[0]=false;
						playerIsMoving[1]=true;
						StartCoroutine("PlayerMoveMolecule",1);
					}
				}
				if(sites[0])
				{
					sites[0].transform.rotation=molecules[0].transform.rotation;
					if(simpleGame)
					{
						sites[0].transform.Rotate(new Vector3(0,90,0),Space.World);
					}
				}
				if(sites[1])
				{
					sites[1].transform.rotation=molecules[1].transform.rotation;
					if(simpleGame)
					{
						sites[1].transform.Rotate(new Vector3(0,-90,0),Space.World);
					}
				}
			}
			if(!simpleGame)// the complex game uses score minimisation
			{
				if(ScoreRMSD() < winScore || Input.GetKeyDown(KeyCode.L))
				{
					StartCoroutine("WinSplash",new Vector3(0,0,0));
					yield break;
				}
				//this is where clicks to attach grapples between the molecules are performed
				if(Input.GetMouseButtonDown(1))
				{
					Camera main =  GameObject.Find ("Main Camera").GetComponent<Camera>();
					Ray r = main.ScreenPointToRay(Input.mousePosition);
					if(PDB_molecule.collide_ray_quick( mol1, p1.mol,
					                                  mol1.transform,
					                                  r))
					{
						int atomID = PDB_molecule.collide_ray(mol1, p1.mol,
						                                      mol1.transform,
						                                      r);
						if(conMan.RegisterClick(p1,atomID))
						{
							//create a label at the selected atom to allow easier selection later
							//CreateLabel(new PDB_molecule.Label(atomID,"ConnectionPoint"),0);
						}

					}
					else if(PDB_molecule.collide_ray_quick( mol2, p2.mol,
					                                       mol2.transform,
					                                       r))
					{
						int atomID = PDB_molecule.collide_ray(mol2, p2.mol,
						                                      mol2.transform,
						                                      r);

						if(conMan.RegisterClick(p2,atomID))
						{
							//CreateLabel(new PDB_molecule.Label(atomID,"ConnectionPoint"),1);

						}
					}
				}
				ApplyReturnToOriginForce();
			}
			else if (simpleGame) // the simple game just tests if the chosen labels match up to the win condition
			{
				if (win) {
					Debug.Log ("We won");
					
					for (int i = 0; i < activeLabels.Count; ++i) {
						PopOut (activeLabels [i].gameObject);
					}
					eventSystem.enabled = false;
					//StartCoroutine("DockingOneAxis");
					if(sites[0])
					{
						PopOut (sites[0]);
					}
					if(sites[1])
					{
						PopOut (sites[1]);
					}
					
					//p1.AutoDockCheap();
					//p2.AutoDockCheap();
					p1.AutoDock();
					while(!p1.HasDocked())
					{
						yield return null;
					}
					Debug.Log("Docked");
					/*
				while (!p1.GetComponent<TransformLerper>().finished&&
				      !p2.GetComponent<TransformLerper>().finished) {
					yield return null;
				}/=*/
					
					this.GetComponent<AudioManager>().Play("Win");
					
					StartCoroutine ("WinSplash",new Vector3(0,0,0));
					GameObject.Destroy(sites[0]);
					GameObject.Destroy(sites[1]);
					yield break;
				}
				if (loose) {
					Debug.Log ("Wrong");
					eventSystem.enabled = false;
					Vector3 startPos1 = p1.gameObject.transform.position;
					Vector3 startPos2 = p2.gameObject.transform.position;
					
					Vector3 moveVec1 =  - startPos1;
					Vector3 moveVec2 =  - startPos2;
					moveVec1.y = 0;
					moveVec2.y = 0;
					
					LabelClicked(selectedLabelIndex[0].gameObject);
					LabelClicked(selectedLabelIndex[1].gameObject);
					yield return new WaitForSeconds(0.9f);
					if(sites[0])
					{
						PopOut (sites[0]);
					}
					if(sites[1])
					{
					PopOut (sites[1]);
					}
					
					for (int i = 0; i < activeLabels.Count; ++i) {
						PopOut (activeLabels [i].gameObject);
					}
					for (float t=0; t<1;t+=Time.deltaTime*0.35f) {
						p1.transform.position = startPos1 + moveVec1 * t;
						p2.transform.position = startPos2 + moveVec2 * t;
						if (PDB_molecule.collide (p1.gameObject, p1.mol, p1.transform,
						                          p2.gameObject, p2.mol, p2.transform)) {
							break;
						}
						yield return null;
					}
					this.GetComponent<AudioManager>().Play("Loose");
					yield return new WaitForSeconds (2.0f);
					
					moveVec1 = startPos1 - p1.transform.position;
					moveVec2 = startPos2 - p2.transform.position;
					
					startPos1 = p1.transform.position;
					startPos2 = p2.transform.position;
					for (float t=0; t<1;t+=Time.deltaTime) {
						p1.transform.position = startPos1 + moveVec1 * t;
						p2.transform.position = startPos2 + moveVec2 * t;
						yield return null;
					}
					for (int i = 0; i < activeLabels.Count; ++i) {
						PopIn(activeLabels [i].gameObject);
					}
					if(sites[0])
					{
						PopIn (sites[0]);
					}
					if(sites[1])
					{
						PopIn (sites[1]);
					}
					eventSystem.enabled = true;
					loose = false;
				}
			}
			yield return null;
		}
	}
}

