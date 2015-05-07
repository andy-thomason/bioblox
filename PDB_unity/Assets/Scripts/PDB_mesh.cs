using UnityEngine;
using System.Collections;

public class PDB_mesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    MeshFilter f = GetComponent<MeshFilter>();
        PDB_molecule mol = PDB_parser.get_molecule(this.name);;
        f.mesh = mol.mesh;
        f.transform.Translate(mol.pos);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
