using UnityEngine;
using System.Collections;

public class PDB_mesh : MonoBehaviour {
    PDB_molecule mol;
    GameObject other;

	// Use this for initialization
	void Start () {
	    MeshFilter f = GetComponent<MeshFilter>();
        mol = PDB_parser.get_molecule(this.name);;
        f.mesh = mol.mesh;
        f.transform.Translate(mol.pos);
        other = GameObject.Find("pdb2ptc.1");
	}
	
	// Update is called once per frame
	void Update () {
        if (this != other) {
            PDB_mesh other_mesh = other.GetComponent<PDB_mesh>();
    	    PDB_molecule.collide(mol, transform, other_mesh.mol, other.transform);
        }
	}
}
