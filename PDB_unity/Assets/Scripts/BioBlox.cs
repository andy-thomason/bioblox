using UnityEngine;
using System.Collections;

public class BioBlox : MonoBehaviour {

	// Use this for initialization
    void Start () {
        Debug.Log ("Start");
        StartCoroutine (game_loop ());
	}
    
    // Update is called once per frame
    void Update () {
    }

	MeshRenderer make_molecule(
		string name, string proto, float xoffset
	) {
		GameObject obj = new GameObject ();
		obj.name = name;
		MeshFilter f = obj.AddComponent<MeshFilter> ();
		MeshRenderer r = obj.AddComponent<MeshRenderer> ();
		PDB_molecule mol = PDB_parser.get_molecule(name);
		f.mesh = mol.mesh;
		Debug.Log (mol.mesh.vertices.Length);
		GameObject pdb = GameObject.Find (proto);
		MeshRenderer pdbr = pdb.GetComponent<MeshRenderer>();
		r.material = pdbr.material;
		r.enabled = false;
		obj.transform.Translate(xoffset, 0, 0);
		obj.transform.Rotate(0, 0, 270);
		obj.transform.Translate(mol.pos.x, mol.pos.y, mol.pos.z);
		return r;
	}

    IEnumerator game_loop() {
		MeshRenderer mol1 = make_molecule ("pdb2ptc.1", "Proto1", -20);
		MeshRenderer mol2 = make_molecule ("pdb2ptc.2", "Proto2", 20);

		for (int i = 1; i <= 6; ++i) {
			GameObject.Find ("Label" + i).transform.Translate(1000, 0, 0);
		}
		GameObject CD1 = GameObject.Find ("CD1");
		GameObject CD2 = GameObject.Find ("CD2");
		GameObject CD3 = GameObject.Find ("CD3");
		CD1.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (1000, 0, 0);
		yield return new WaitForSeconds(1.0f);
		CD3.transform.Translate (1000, 0, 0);
		CD2.transform.Translate (-1000, 0, 0);
		yield return new WaitForSeconds(1.0f);
		CD2.transform.Translate (1000, 0, 0);
		CD1.transform.Translate (-1000, 0, 0);
		yield return new WaitForSeconds(1.0f);
		CD1.transform.Translate (1000, 0, 0);

		mol1.enabled = true;
		yield return new WaitForSeconds(0.3f);
		mol2.enabled = true;

		for (int i = 1; i <= 6; ++i) {
			yield return new WaitForSeconds(0.1f);
			GameObject.Find ("Label" + i).transform.Translate(-1000, 0, 0);
		}
		yield return new WaitForSeconds(0.1f);
	}
}
