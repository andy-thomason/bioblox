using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;

public class BioBlox : MonoBehaviour
{

	List<string> filenames = new List<string> ();
	int filenameIndex = 0;
	EventSystem eventSystem;
	public List<GameObject> prefabLabels;
	List<LabelScript> activeLabels = new List<LabelScript> ();
	List<Tuple<int,int>> winCondition = new List<Tuple<int,int>> ();
	bool reload = false;
	bool won = false;
	LabelScript[] selectedLabelIndex = new LabelScript[2];
	GameObject[] molecules;

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("Start");
		filenames.Add ("pdb2ptcWithTags");
		filenames.Add ("pdb2ptcWithTags");
		StartCoroutine (game_loop ());
		eventSystem = EventSystem.current;
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

	public void GetLabelPos (int atomID,Transform t)
	{
		int molNum = -1;
		Vector3 atomPos = GetAtomPos (atomID);
		int index = GetAtomIndexFromID (atomID, out molNum);

		if (molNum == -1) {
			return;
		}
		PDB_mesh molMesh = molecules [molNum].GetComponent<PDB_mesh> ();
		Vector3 atomPosW = molMesh.transform.TransformPoint (atomPos);
		if (atomPosW.z > molMesh.transform.position.z) {
			Vector3 transToAt = atomPosW - molMesh.transform.position;
			transToAt.z = 0;
			transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
			atomPosW = transToAt + molMesh.transform.position;
		} else {
			atomPosW += new Vector3 (0, 0, -10);
		}
		t.position = atomPosW;
		if (atomPosW.x < molMesh.transform.position.x) {
			t.localScale = new Vector3(- Mathf.Abs(t.localScale.x),t.localScale.y,t.localScale.z);
		} else {
			t.localScale=new Vector3(Mathf.Abs(t.localScale.x),t.localScale.y,t.localScale.z);
		}
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
    
	// Update is called once per frame
	void Update ()
	{
		if (reload) {
			filenameIndex++;
			Reset ();
			if (filenameIndex == filenames.Count) {
				Debug.Log ("End of levels");
			}
			StartCoroutine (game_loop ());
		}
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
		molecules [molNum].GetComponent<PDB_mesh> ().BringAtomToFocus (index);
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
		laSc.label = label;
		laSc.owner = this;
		laSc.labelID = activeLabels.Count;
		laSc.is3D = true;
		//GameObject foundObject = GameObject.Find ("Label" + (activeLabels.Count + 1));
		//newLabel.transform.position = foundObject.transform.position;
		GameObject canvas = GameObject.Find ("Canvas");
		newLabel.transform.SetParent (canvas.transform);
		newLabel.name = label.labelName + laSc.labelID;
		activeLabels.Add (laSc);
	}

	void Reset ()
	{
		for (int i=0; i<activeLabels.Count; ++i) {
			GameObject.Destroy (activeLabels [i].gameObject);
		}
		molecules = new GameObject[0];
		activeLabels.Clear ();
		selectedLabelIndex [0] = null;
		selectedLabelIndex [1] = null;
		winCondition.Clear ();
		reload = false;
		won = false;
	}

	public void CheckWin ()
	{
		if (winCondition.Count > 0 && selectedLabelIndex [0] && selectedLabelIndex [1]) {

			for (int i=0; i<winCondition.Count; ++i) {
				int winCon1 = winCondition [i].First;
				int winCon2 = winCondition [i].Second;

				int selected1 = selectedLabelIndex [0].label.atomIndex;
				int selected2 = selectedLabelIndex [1].label.atomIndex;

				if (!(winCon1 == selected1 && winCon2 == selected2) &&
					!(winCon1 == selected2 && winCon2 == selected1)) {
					won = false;
					return;	
				}
			}
			won = true;
			return;
		}
		won = false;
		return;
	}

	MeshRenderer make_molecule (
		string name, string proto, float xoffset
	)
	{
		GameObject obj = new GameObject ();
		obj.name = name;
		obj.AddComponent<TransformLerper> ();
	
		MeshFilter f = obj.AddComponent<MeshFilter> ();
		MeshRenderer r = obj.AddComponent<MeshRenderer> ();
		PDB_mesh p = obj.AddComponent<PDB_mesh> ();

		PDB_molecule mol = PDB_parser.get_molecule (name);
		p.mol = mol;
		f.mesh = mol.mesh;
		Debug.Log (mol.mesh.vertices.Length);
		GameObject pdb = GameObject.Find (proto);
		MeshRenderer pdbr = pdb.GetComponent<MeshRenderer> ();
		r.material = pdbr.material;
		r.enabled = false;



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

		for (int i=0; i<mol.labels.Length; ++i) {
			CreateLabel (mol.labels [i], (int)char.GetNumericValue (name [name.Length - 1]));
		}
		return r;
	}

	IEnumerator game_loop ()
	{
		if (filenameIndex >= filenames.Count) {
			Debug.LogError ("No next level");
		}
		string file = filenames [filenameIndex];
		MeshRenderer mol1 = make_molecule (file + ".1", "Proto1", -20);
		MeshRenderer mol2 = make_molecule (file + ".2", "Proto2", 20);
		molecules = new GameObject[2];
		molecules [0] = mol1.gameObject;
		molecules [1] = mol2.gameObject;
		PDB_mesh p1 = mol1.GetComponent<PDB_mesh> ();
		PDB_mesh p2 = mol2.GetComponent<PDB_mesh> ();
		p1.other = p2.gameObject;
		p2.other = p1.gameObject;
		for (int i=0; i<p1.mol.pairedAtoms.Length; ++i) {
			winCondition.Add (p1.mol.pairedAtoms [i]);
		
		}

		for (int i = 0; i < activeLabels.Count; ++i) {
			activeLabels [i].gameObject.SetActive (false);
		}
		GameObject CD1 = GameObject.Find ("CD1");
		GameObject CD2 = GameObject.Find ("CD2");
		GameObject CD3 = GameObject.Find ("CD3");
		CD1.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (1000, 0, 0);
		yield return new WaitForSeconds (1.0f);
		CD3.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (-1000, 0, 0);
		yield return new WaitForSeconds (1.0f);
		CD2.transform.Translate (1000, 0, 0);
		CD1.transform.Translate (-1000, 0, 0);
		yield return new WaitForSeconds (1.0f);
		CD1.transform.Translate (1000, 0, 0);

		mol1.enabled = true;
		yield return new WaitForSeconds (0.3f);
		mol2.enabled = true;

		for (int i = 0; i < activeLabels.Count; ++i) {
			yield return new WaitForSeconds (0.1f);
			activeLabels [i].gameObject.SetActive (true);
		}
		yield return new WaitForSeconds (0.1f);
		eventSystem.enabled = true; 
		while (true) {
			if (won) {
				Debug.Log ("We won");
				p1.AutoDock ();
				p2.AutoDock ();
				while (!p1.GetComponent<TransformLerper>().finished&&
				      !p2.GetComponent<TransformLerper>().finished) {
					yield return null;
				}
				for (int i = 0; i < activeLabels.Count; ++i) {
					activeLabels [i].gameObject.SetActive (false);
				}
				eventSystem.enabled = false;
				yield return new WaitForSeconds(1.0f);
				GameObject.Destroy (mol1.gameObject);
				GameObject.Destroy (mol2.gameObject);
				reload = true;
				Debug.Log ("Reloading");
				yield break;
			}
			yield return null;
		}
	}

}

