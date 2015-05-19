using UnityEngine;
using System.Collections;

public class PDB_mesh : MonoBehaviour {
    PDB_molecule mol;
    public GameObject other;

	// Use this for initialization
	void Start () {
	    MeshFilter f = GetComponent<MeshFilter>();
        mol = PDB_parser.get_molecule(this.name);;
        f.mesh = mol.mesh;
        f.transform.Translate(mol.pos);
	}
	
	// Update is called once per frame
	void Update () {
        if (other) {
            if (Input.GetKey("w"))
            {
                transform.Translate(0, 0.1f, 0);
            }
            if (Input.GetKey("s"))
            {
                transform.Translate(0, -0.1f, 0);
            }
            PDB_mesh other_mesh = other.GetComponent<PDB_mesh>();
    	    PDB_molecule.collide(mol, transform, other_mesh.mol, other.transform);
        }
	}
}
