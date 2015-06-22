using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BioBlox : MonoBehaviour
{
	public List<string> filenames = new List<string> ();
	int filenameIndex = 0;
	EventSystem eventSystem;
	public List<GameObject> prefabLabels;
	public GameObject winSplash;
	public GameObject looseSplash;
	public float shaderKVal=-0.03f;
	List<LabelScript> activeLabels = new List<LabelScript> ();
	List<Tuple<int,int>> winCondition = new List<Tuple<int,int>> ();
	bool reload = false;
	bool won = false;
	bool loose = false;
	LabelScript[] selectedLabelIndex = new LabelScript[2];
	GameObject[] molecules;
	bool[] playerIsMoving = new bool[2]{false,false};
	GameObject popTarget;

	List<Color> colorPool=new List<Color>();
	int randomColorPoolOffset;

	// Use this for initialization
	void Start ()
	{
		colorPool.Add (Color.red);
		colorPool.Add (Color.blue);
		colorPool.Add (Color.cyan);
		colorPool.Add (Color.green);
		colorPool.Add (Color.magenta);
		colorPool.Add (Color.yellow);
		colorPool.Add (new Color (1.0f, 0.5f, 0.1f));
		randomColorPoolOffset = Random.Range (0, colorPool.Count-1);
		Debug.Log ("Start");
		//filenames.Add ("jigsawBlue");

		//filenames.Add ("2Q5R");
		filenames.Add ("pdb2ptcWithTags");
		filenames.Add ("1GCQ_bWithTags");


		filenames.Add ("pdb2ptcWithTags");



		StartCoroutine (game_loop ());
		eventSystem = EventSystem.current;

		winSplash.SetActive (false);
		looseSplash.SetActive (false);

	}

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

	public void GetLabelPos (int atomID, Transform t)
	{
		int molNum = -1;
		Vector3 atomPos = GetAtomPos (atomID);
		int index = GetAtomIndexFromID (atomID, out molNum);
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

		transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
		atomPosW = transToAt + molMesh.transform.position;
		t.position = atomPosW;
		t.position += new Vector3 (0, 0, -10);

	}

	public Vector3 GetAtomPos (int atomID)
	{
		if (molecules.Length == 2) {
			int molNum;
			int index = GetAtomIndexFromID (atomID, out molNum);
			if (index == -1) {
				Debug.LogError ("Bad index");
			}
			return  molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [index];
		}
		return new Vector3 (0, 0, 0);
	}

	public Vector3 GetAtomWorldPos( int atomID)
	{
		if (molecules.Length == 2) {
			int molNum;
			int index = GetAtomIndexFromID (atomID, out molNum);
			if (index == -1) {
				Debug.LogError ("Bad index");
			}
			return  molecules[molNum].transform.TransformPoint(
				molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [index]);
		}
		return new Vector3 (0, 0, 0);
	}
    
	IEnumerator PlayerMoveMolecule(int molIndex)
	{
		GameObject mol = molecules [molIndex];
		Rigidbody r = mol.GetComponent<Rigidbody> ();
		r.maxAngularVelocity = 4;
		float timeout = 4.0f;
		playerIsMoving [molIndex] = true;
		Vector3 lastMousePos = Input.mousePosition;
		for (float t=0.0f; t<timeout; t+=Time.deltaTime) {
			if(playerIsMoving[molIndex]==false||
			   eventSystem.IsActive()==false)
			{
				break;
			}

			if(Input.GetMouseButton(0))
			{
				if(t>0.3f)
				{
					lastMousePos=Input.mousePosition;
				}
				t=0.0f;
				Vector3 mousePos=Input.mousePosition;
				Vector3 mouseDelta=mousePos-lastMousePos;

				r.AddTorque(new Vector3(mouseDelta.y,-mouseDelta.x,0));
				lastMousePos=mousePos;
			}
			yield return null;
		}
		if (selectedLabelIndex [molIndex]&&eventSystem.IsActive()) {
			LabelClicked (selectedLabelIndex [molIndex].gameObject);
		}
		playerIsMoving[molIndex]=false;
		r.angularVelocity=new Vector3(0,0,0);
		yield break;
	}

	// Update is called once per frame
	void Update ()
	{
		if (reload) {
			filenameIndex++;
			Reset ();
			if (filenameIndex == filenames.Count) {
				Debug.Log ("End of levels");
			}
			else{
				StartCoroutine (game_loop ());
			}
		}
		if (eventSystem.IsActive()) {
			if(Input.GetMouseButton(0))
			{
				Camera c = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
				Ray r = c.ScreenPointToRay(Input.mousePosition);
				if(!playerIsMoving[0] && PDB_molecule.collide_ray(
					molecules[0],
					molecules[0].GetComponent<PDB_mesh>().mol,
					molecules[0].transform,
					r)!=-1)
				{
					Debug.Log("Started moving 1");
					playerIsMoving[1]=false;
					playerIsMoving[0]=true;
					StartCoroutine("PlayerMoveMolecule",0);
				}
				else if(!playerIsMoving[1] && PDB_molecule.collide_ray(
					molecules[1],
					molecules[1].GetComponent<PDB_mesh>().mol,
					molecules[1].transform,
					r)!=-1)
				{
					Debug.Log("Started moving 2");
					playerIsMoving[0]=false;
					playerIsMoving[1]=true;
					StartCoroutine("PlayerMoveMolecule",1);
				}
			}
		}
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

	//returns other link index or -1 on failure
//	public void LinkPair(LabelScript a, LabelScript b)
//	{
//		Debug.Log ("Link "+a.labelID+" and "+b.labelID+" was made");
//		Debug.Log (labelToLabelPairs.Count);
//		labelToLabelPairs.Add (labelToLabelPairs.Count,new Tuple<int,int> (a.labelID, b.labelID));
//		b.MakeLink(labelToLabelPairs.Count-1);
//		a.MakeLink (labelToLabelPairs.Count-1);
//	}
//
//	public void BreakLink (int linkIndex)
//	{
//			Tuple<int,int> link = labelToLabelPairs [linkIndex];
//			Debug.Log ("Link "+link.First+" and "+link.Second+" was broken");
//			
//			activeLabels [link.First].BreakLink ();
//			activeLabels [link.Second].BreakLink ();
//		labelToLabelPairs.Remove (linkIndex);
//	}

	public void LabelClicked (GameObject labelObj)
	{
		Debug.Log (labelObj.name + " was clicked");
		LabelScript script = labelObj.GetComponent<LabelScript> ();
		
		int molNum = -1;
		int index = GetAtomIndexFromID (script.label.atomIndex, out molNum);
		playerIsMoving [molNum] = false;
		//gives us the opposite molecule, 1-0=1, 1-1 =0
		int otherMolNum = 1 - molNum;
		Vector3 alignDir = molecules [otherMolNum].transform.position -
			molecules [molNum].transform.position;
		molecules [molNum].GetComponent<PDB_mesh> ().AlignAtomToVector (index, alignDir);
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

		//Handle label click, make active, focusd on atom //etc
	}

	void CreateLabel (PDB_molecule.Label label, int molNum)
	{
		int labelTypeIndex = -1;
		for (int i=0; i<prefabLabels.Count; ++i) {
			if (string.Compare (label.labelName, prefabLabels [i].name) == 0) {
				labelTypeIndex = i;
				break;
			}
		}
		if (labelTypeIndex == -1) {
			Debug.LogError ("Could not find Label with type" + label.labelName);
			return;
		}
		GameObject newLabel = GameObject.Instantiate<GameObject> (prefabLabels [labelTypeIndex]);
		if (!newLabel) {
			Debug.Log ("Could not create Label");
		}
		LabelScript laSc = newLabel.GetComponent<LabelScript> ();
		if (!laSc) {
			Debug.LogError ("Label prefab " + label.labelName + " does not have a LabelScript attached");
		}
		newLabel.GetComponent<Image> ().color = colorPool[(activeLabels.Count+randomColorPoolOffset) % colorPool.Count];
		newLabel.GetComponent<Light> ().color = newLabel.GetComponent<Image> ().color;
		laSc.label = label;
		laSc.owner = this;
		laSc.labelID = activeLabels.Count;
		laSc.is3D = true;
		//GameObject foundObject = GameObject.Find ("Label" + (activeLabels.Count + 1));
		//newLabel.transform.position = foundObject.transform.position;
		GameObject canvas = GameObject.Find ("Labels");
		newLabel.transform.SetParent (canvas.transform);
		newLabel.name = label.labelName + laSc.labelID;
		activeLabels.Add (laSc);
	}

	void Reset ()
	{
		for (int i=0; i<activeLabels.Count; ++i) {
			GameObject.Destroy (activeLabels [i].gameObject);
		}
		randomColorPoolOffset = Random.Range (0, colorPool.Count-1);
		molecules = new GameObject[0];
		activeLabels.Clear ();
		selectedLabelIndex [0] = null;
		selectedLabelIndex [1] = null;
		winCondition.Clear ();
		reload = false;
		won = false;
	}

	public void CheckPair ()
	{
		if (winCondition.Count > 0 && selectedLabelIndex [0] && selectedLabelIndex [1]) {
			bool hasWon = true;

			
			for (int i=0; i<winCondition.Count; ++i) {
				int winCon1 = winCondition [i].First;
				int winCon2 = winCondition [i].Second;
				
				int selected1 = selectedLabelIndex [0].label.atomIndex;
				int selected2 = selectedLabelIndex [1].label.atomIndex;
				
				if (!(winCon1 == selected1 && winCon2 == selected2) &&
					!(winCon1 == selected2 && winCon2 == selected1)) {
					hasWon = false;
					break;
				}
			}
			if (hasWon) {
				won = true;
			} else {
				loose = true;
			}
			this.GetComponent<AudioManager>().Play("Drum");
		}
	}

	IEnumerator LooseSplash ()
	{
		//put animation for loose splash here
		PopIn (looseSplash);
		yield return new WaitForSeconds (2.0f);
		PopOut (looseSplash);
		yield break;
	}

	IEnumerator FadeMolecules()
	{
		GameObject mol1 = molecules [0];
		GameObject mol2 = molecules [1];
		MeshRenderer[] meshes1 = molecules [0].GetComponentsInChildren<MeshRenderer> ();
		MeshRenderer[] meshes2 = molecules [1].GetComponentsInChildren<MeshRenderer> ();
		PDB_molecule molInfo1 = mol1.GetComponent<PDB_mesh> ().mol;
		PDB_molecule molInfo2 = mol2.GetComponent<PDB_mesh> ().mol;

		float targetKVal = shaderKVal;
		float currentKVal = 0;
		for (float t=0; t<=1.0f; t+=Time.deltaTime) {
			float k=currentKVal+targetKVal*t;

			for(int i=0;i<meshes1.Length;++i)
			{
			meshes1[i].material.SetFloat("_K",k);
			}
			for(int i=0;i<meshes2.Length;++i)
			{
			meshes2[i].material.SetFloat("_K",k);
			}
			yield return null;
		}
		for (int i=0; i<meshes1.Length; ++i) {
			meshes1[i].material.SetVector ("_CullPos",mol1.transform.TransformPoint( 
			    molInfo1.atom_centres[molInfo1.serial_to_atom[winCondition[0].First]]));
			meshes1[i].material.SetFloat("_K",targetKVal);
		}
		for(int i=0;i<meshes2.Length;++i)
		{
			meshes2[i].material.SetVector ("_CullPos",mol2.transform.TransformPoint(
				molInfo2.atom_centres[molInfo2.serial_to_atom[winCondition[0].Second]]));
			meshes2[i].material.SetFloat("_K",targetKVal);
		}
	}

	IEnumerator WinSplash (Vector3 focusPoint)
	{
		gameObject.GetComponent<ClockTimer> ().LogPlayerTime ();
		gameObject.GetComponent<ClockTimer> ().StopPlayerTimer ();
		//put animation for win splash here
		PopIn (winSplash);
		Camera c = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		GameObject parent = new GameObject ();
		Rigidbody r=parent.AddComponent<Rigidbody> ();
		molecules [0].transform.SetParent (parent.transform, true);
		molecules [1].transform.SetParent (parent.transform, true);
	
		r.angularDrag = 1.0f;
		r.constraints = RigidbodyConstraints.FreezePosition;
		r.useGravity = false;
		parent.name = "MoveableParent";

		StartCoroutine ("FadeMolecules");
		float zoomValue = 30.0f;
		float radRot = (3.0f * Mathf.PI) / 2;
		float camRot = 0;
		Vector3 start = c.transform.position;
		Vector3 moveDir = focusPoint+ new Vector3 (0,0, -30) - c.transform.position;
		for (float t=0; t<1.0f; t+=Time.deltaTime) {
			c.transform.position = start + moveDir * t;
			yield return null;
		}
		SpringJoint [] joints = molecules [0].GetComponents<SpringJoint> ();
		for(int i=0;i<joints.Length;++i)
		{
			Component.Destroy(joints[i]);
		}
		Component.Destroy (molecules [0].GetComponent<Rigidbody> ());
		Component.Destroy (molecules [1].GetComponent<Rigidbody> ());
 

		const float nonInteractionTimeOut = 4.0f;
		float nonInteractionTimer = 0.0f;
		bool autoRotate = true;

		Vector3 oldMousePos = Input.mousePosition;

		while(true){

			nonInteractionTimer+=Time.deltaTime;
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
			if(Input.anyKeyDown&&!Input.GetMouseButtonDown(0))
			{
				break;
			}
			yield return null;
		}
		PopOut (molecules[0].gameObject);
		PopOut (molecules[1].gameObject);
		PopOut (winSplash);
		yield return new WaitForSeconds (1.0f);
		GameObject.Destroy (molecules[0].gameObject);
		GameObject.Destroy (molecules[1].gameObject);
		Vector3 target = start;
		start = c.transform.position;
		Quaternion startQ = c.transform.rotation;
		for (float t=0; t<1.0f; t+=Time.deltaTime) {
			c.transform.position=Vector3.Lerp(start,target,t);
			c.transform.rotation=Quaternion.Slerp(startQ,Quaternion.identity,t);
			yield return null;
		}
		reload = true;
		Debug.Log ("Reloading");

		yield break;
	}



	void make_molecule_mesh(PDB_mesh mesh, string proto)
	{
		GameObject pdb = GameObject.Find (proto);
		MeshRenderer pdbr = pdb.GetComponent<MeshRenderer> ();


		for(int i=0;i<mesh.mol.mesh.Length;++i)
		{
			Mesh cur=mesh.mol.mesh[i];
			GameObject obj = new GameObject();
			obj.name=cur.name;
			MeshFilter f = obj.AddComponent<MeshFilter> ();
			MeshRenderer r = obj.AddComponent<MeshRenderer> ();
			r.material = pdbr.material;
			f.mesh=cur;
			obj.transform.SetParent(mesh.transform);
			obj.transform.position=Vector3.zero;
		}
	
	}


	GameObject make_molecule (
		string name, string proto, float xoffset
	)
	{
		GameObject obj = new GameObject ();
		obj.name = name;
		obj.AddComponent<TransformLerper> ();

		PDB_mesh p = obj.AddComponent<PDB_mesh> ();
		Rigidbody ri = obj.AddComponent<Rigidbody> ();
		//obj.AddComponent<Tex3DMap> ();
		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		make_molecule_mesh (p,proto);
		Debug.Log (mol.mesh[0].vertices.Length);


		ri.angularDrag = 1;
		ri.useGravity = false;
		ri.constraints = RigidbodyConstraints.FreezePosition;


		obj.transform.Rotate (0, 0, 270);
		Vector3 originMolPos = obj.transform.TransformPoint (mol.pos);
		Quaternion originalMolRot = obj.transform.rotation;
		obj.transform.Rotate (0, 0, -270); 
		obj.transform.Translate (xoffset, 0, 0);
		obj.transform.Rotate (0, 0, 270);
		obj.transform.Translate (mol.pos.x, mol.pos.y, mol.pos.z);
		obj.GetComponent<TransformLerper> ().AddTransformPoint (obj.transform.rotation);

		obj.GetComponent<TransformLerper> ().AddTransformPoint (originMolPos,
		                                                       originalMolRot);
		obj.GetComponent<TransformLerper> ().speed =1.0f;



		obj.transform.rotation = Random.rotation;
		for (int i=0; i<mol.labels.Length; ++i) {
			CreateLabel (mol.labels [i], (int)char.GetNumericValue (name [name.Length - 1]));
		}
		return obj;
	}

	IEnumerator game_loop ()
	{
		if (filenameIndex >= filenames.Count) {
			Debug.LogError ("No next level");
		}
		string file = filenames [filenameIndex];
		GameObject mol1 = make_molecule (file + ".1", "Proto1", -20);
		GameObject mol2 = make_molecule (file + ".2", "Proto2", 20);
		ClockTimer playerClock = gameObject.GetComponent<ClockTimer> ();
		playerClock.ResetTimer ();
	
		molecules = new GameObject[2];
		molecules [0] = mol1.gameObject;
		molecules [1] = mol2.gameObject;
		PDB_mesh p1 = mol1.GetComponent<PDB_mesh> ();
		PDB_mesh p2 = mol2.GetComponent<PDB_mesh> ();
		//debug 3D texture
		//GameObject.Find ("Test").GetComponent<Tex3DMap> ().Build (p1.mol);
		p1.other = p2.gameObject;
		p2.other = p1.gameObject;
		p1.gameObject.SetActive (false);
		p2.gameObject.SetActive (false);
		for (int i=0; i<p1.mol.pairedAtoms.Length; ++i) {
			winCondition.Add (p1.mol.pairedAtoms [i]);
		}

		for (int i = 0; i < activeLabels.Count; ++i) {
			activeLabels [i].gameObject.SetActive (false);
		}
		GameObject CD1 = GameObject.Find ("CD1");
		GameObject CD2 = GameObject.Find ("CD2");
		GameObject CD3 = GameObject.Find ("CD3");
		CD1.transform.position = new Vector3 (0, 0, 0);
		CD2.transform.position = new Vector3 (0, 0, 0);
		CD3.transform.position = new Vector3 (0, 0, 0);
		CD1.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (1000, 0, 0);
		this.GetComponent<AudioManager> ().Play ("Count");
		yield return new WaitForSeconds (1.0f);
		CD3.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (-1000, 0, 0);
		this.GetComponent<AudioManager> ().Play ("Count");
		yield return new WaitForSeconds (1.0f);
		CD2.transform.Translate (1000, 0, 0);
		CD1.transform.Translate (-1000, 0, 0);
		this.GetComponent<AudioManager> ().Play ("Count");
		yield return new WaitForSeconds (1.0f);
		CD1.transform.Translate (1000, 0, 0);

		PopInSound (mol1.gameObject);
		yield return new WaitForSeconds (0.3f);
		PopInSound (mol2.gameObject);

		for (int i = 0; i < activeLabels.Count; ++i) {
			yield return new WaitForSeconds (0.1f);
			PopInSound (activeLabels [i].gameObject);
		}
		yield return new WaitForSeconds (0.1f);
		eventSystem.enabled = true;
		playerClock.StartPlayerTimer ();
		while (true) {
			if(Input.GetKeyDown(KeyCode.L))
			{
				PopOut (molecules[0].gameObject);
				PopOut (molecules[1].gameObject);
				yield return new WaitForSeconds (1.0f);
				GameObject.Destroy (molecules[0].gameObject);
				GameObject.Destroy (molecules[1].gameObject);
				reload=true;
				yield break;
			}
			if (won) {
				Debug.Log ("We won");

				for (int i = 0; i < activeLabels.Count; ++i) {
					PopOut (activeLabels [i].gameObject);
				}
				eventSystem.enabled = false;
				//p1.AutoDockCheap();
				//p2.AutoDockCheap();
				p1.AutoDock ();
				while(!p1.HasDocked())
				{

					yield return null;
				}
				Debug.Log("Docked");
				/*
				while (!p1.GetComponent<TransformLerper>().finished&&
				      !p2.GetComponent<TransformLerper>().finished) {
					yield return null;
				}*/
				this.GetComponent<AudioManager>().Play("Win");

				StartCoroutine ("WinSplash",new Vector3(0,0,0));
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
				StartCoroutine ("LooseSplash");
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
				eventSystem.enabled = true;
				loose = false;
			}
			yield return null;
		}
	}

}

