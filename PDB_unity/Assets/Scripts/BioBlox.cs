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
	public GameObject prefabLabel;
	public GameObject winSplash;
	public GameObject looseSplash;
	public float shaderKVal=-0.03f;
	bool reload = false;



	public bool exitWinSplash=false;
	List<LabelScript> activeLabels = new List<LabelScript> ();
	LabelScript[] selectedLabelIndex = new LabelScript[2];
	public GameObject[] molecules;
	GameObject[] sites = new GameObject[2];
	bool[] playerIsMoving = new bool[2]{false,false};
	Vector3[] originPosition = new Vector3[2];
	GameObject popTarget;

	public float score=10.0f;
	public float winScore=10.0f;
	public float uiScrollSpeed = 10.0f;
	public float repulsiveForce = 30.0f;


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

		transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
		atomPosW = transToAt + molMesh.transform.position;
		t.position = atomPosW;
		t.position += new Vector3 (0, 0, -10);
	}

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
    
	IEnumerator PlayerMoveMolecule(int molIndex)
	{
		GameObject mol = molecules [molIndex];
		Rigidbody r = mol.GetComponent<Rigidbody> ();
		r.maxAngularVelocity = 4;
		float timeout = 100.0f;
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
				Camera c = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
				Ray ray = c.ScreenPointToRay(Input.mousePosition);
				if(PDB_molecule.collide_ray_quick(mol,mol.GetComponent<PDB_mesh>().mol,
				                                  mol.transform,
				                                  ray))
				{
					if(t>0.3f)
					{
						lastMousePos=Input.mousePosition;
					}
					t=0.0f;
					Vector3 mousePos=Input.mousePosition;
					Vector3 mouseDelta=mousePos-lastMousePos;

					r.AddTorque(new Vector3(mouseDelta.y,-mouseDelta.x,0)*uiScrollSpeed);
					lastMousePos=mousePos;
				}
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

					playerIsMoving[0]=false;
					playerIsMoving[1]=true;
					StartCoroutine("PlayerMoveMolecule",1);
				}
			}
			if(sites[0])
			{
				sites[0].transform.rotation=molecules[0].transform.rotation;
				//sites[0].transform.Rotate(new Vector3(0,90,0),Space.World);
			}
			if(sites[1])
			{
				sites[1].transform.rotation=molecules[1].transform.rotation;
				//sites[1].transform.Rotate(new Vector3(0,-90,0),Space.World);
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
		target.transform.localScale = targetScale;
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

	IEnumerator FadeMolecules()
	{
		GameObject mol1 = molecules [0];
		GameObject mol2 = molecules [1];
		MeshRenderer[] meshes1 = molecules [0].GetComponentsInChildren<MeshRenderer> ();
		MeshRenderer[] meshes2 = molecules [1].GetComponentsInChildren<MeshRenderer> ();
		PDB_molecule molInfo1 = mol1.GetComponent<PDB_mesh> ().mol;
		PDB_molecule molInfo2 = mol2.GetComponent<PDB_mesh> ().mol;
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
		for (int i=0; i<meshes1.Length; ++i) {
			meshes1 [i].material.SetFloat ("_K", targetKVal);
		}
		for (int i=0; i<meshes2.Length; ++i) {
			meshes2 [i].material.SetFloat ("_K", targetKVal);
		}
	}


	IEnumerator WinSplash (Vector3 focusPoint)
	{
		ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
		conMan.Reset ();
		eventSystem.enabled = false;
		gameObject.GetComponent<ClockTimer> ().LogPlayerTime ();
		gameObject.GetComponent<ClockTimer> ().StopPlayerTimer ();
		//put animation for win splash here
		for(int i=0;i<activeLabels.Count;++i)
		{
			GameObject.Destroy(activeLabels[i].gameObject);
		}
		activeLabels.Clear();

		PopIn (winSplash);
		GameObject gene = GameObject.Find ("Gene");
		if (gene) {
			gene.SetActive (false);
		}
		Camera c = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		GameObject parent = new GameObject ();
		Rigidbody r=parent.AddComponent<Rigidbody> ();
		molecules [0].transform.SetParent (parent.transform, true);
		molecules [1].transform.SetParent (parent.transform, true);
		
		r.angularDrag = 1.0f;
		r.constraints = RigidbodyConstraints.FreezePosition;
		r.useGravity = false;
		parent.name = "MoveableParent";
		
		//StartCoroutine ("FadeMolecules");
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
		
		float timeoutTimer = 40.0f;
		const float nonInteractionTimeOut = 2.0f;
		float nonInteractionTimer = 0.0f;
		bool autoRotate = true;
		
		Vector3 oldMousePos = Input.mousePosition;
		
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
		PopOut (molecules[0].gameObject);
		PopOut (molecules[1].gameObject);
		PopOut (winSplash);
		yield return new WaitForSeconds (1.0f);
		GameObject.Destroy (molecules[0].gameObject);
		GameObject.Destroy (molecules[1].gameObject);
		GameObject.Destroy (parent);
		if (gene) {
			gene.SetActive (true);
		}
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


	public void SiteClicked (GameObject labelObj)
	{
		activeLabels.Remove (labelObj.GetComponent<LabelScript> ());
		GameObject.Destroy (labelObj);
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

	void CreateLabel (PDB_molecule.Label label, int molNum)
	{
		GameObject newLabel = GameObject.Instantiate<GameObject> (prefabLabel);
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
		laSc.moleculeNumber = molNum;
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

	public void LabelClicked (GameObject labelObj)
	{
		Debug.Log (labelObj.name + " was clicked");
		LabelScript script = labelObj.GetComponent<LabelScript> ();
		
		int molNum = script.moleculeNumber;
		int index = script.label.atomIndex;
	


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
		//Handle label click, make active, focusd on atom //etc
	}
	

	void Reset ()
	{

		randomColorPoolOffset = Random.Range (0, colorPool.Count - 1);
		GameObject.Destroy (molecules [0].gameObject);
		GameObject.Destroy (molecules [1].gameObject);




		selectedLabelIndex [0] = null;
		selectedLabelIndex [1] = null;
		reload = false;
	}

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


	GameObject make_molecule (
		string name, string proto, float xoffset, int layerNum
	)
	{
		GameObject obj = new GameObject ();
		obj.SetActive (true);
		obj.name = name;
		obj.layer = layerNum;
		//obj.AddComponent<TransformLerper> ();

		PDB_mesh p = obj.AddComponent<PDB_mesh> ();
		Rigidbody ri = obj.AddComponent<Rigidbody> ();
		//obj.AddComponent<Tex3DMap> ();
		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		make_molecule_mesh (p,proto,layerNum);
		Debug.Log (mol.mesh[0].vertices.Length);

		p.seperationForce = 200.0f;
		ri.drag = 0f;
		ri.angularDrag = 5f;
		ri.useGravity = false;


		obj.transform.Rotate (0, 0, 270);
		Vector3 originMolPos = obj.transform.TransformPoint (mol.pos);
		Quaternion originalMolRot = obj.transform.rotation;
		obj.transform.Rotate (0, 0, -270); 
		obj.transform.Translate ((mol.bvh_radii[0]*xoffset)*0.5f, 0, 0);
		obj.transform.Rotate (0, 0, 270);
		obj.transform.Translate (mol.pos.x, mol.pos.y, mol.pos.z);
		//obj.GetComponent<TransformLerper> ().AddTransformPoint (obj.transform.rotation);

		//obj.GetComponent<TransformLerper> ().AddTransformPoint (originMolPos,
		//                                                       originalMolRot);
		//obj.GetComponent<TransformLerper> ().speed =1.0f;



		obj.transform.rotation = Random.rotation;
		return obj;
	}

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


	IEnumerator game_loop ()
	{
		if (filenameIndex >= filenames.Count) {
			Debug.LogError ("No next level");
		}
		string file = filenames [filenameIndex];
		GameObject mol1 = make_molecule (file + ".1", "Proto1", -1,8);
		GameObject mol2 = make_molecule (file + ".2", "Proto2", 1,9);

		originPosition [0] = mol1.transform.position;
		originPosition [1] = mol2.transform.position;



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
		//debug 3D texture
		//GameObject.Find ("Test").GetComponent<Tex3DMap> ().Build (p1.mol);
		p1.other = p2.gameObject;
		p2.other = p1.gameObject;
		p1.gameObject.SetActive (false);
		p2.gameObject.SetActive (false);
	
		PopInSound (mol1.gameObject);
		yield return new WaitForSeconds (0.2f);
		PopInSound (mol2.gameObject);

		p1.shouldCollide = true;

		ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();

		mol1.transform.localScale = new Vector3 (1, 1, 1);
		mol2.transform.localScale = new Vector3 (1, 1, 1);
		yield return new WaitForSeconds (0.1f);
		eventSystem.enabled = true;
		while (true) {
			if (Input.anyKey){
			playerClock.StartPlayerTimer();
			}

			if(ScoreRMSD() < winScore || Input.GetKeyDown(KeyCode.L))
			{
				StartCoroutine("WinSplash",new Vector3(0,0,0));
				yield break;

			}

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
			if(Input.GetKeyDown(KeyCode.Space))
			{
				//gameObject.GetComponent<ConnectionManager>().Contract();
			}
			yield return null;
		}
	}

}

