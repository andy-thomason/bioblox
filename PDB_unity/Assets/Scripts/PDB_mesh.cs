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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (other) {
            if (Input.GetKey("w"))
            {
                rb.AddForce(new Vector3(0, 10, 0));
            }
            if (Input.GetKey("s"))
            {
                rb.AddForce(new Vector3(0, -10, 0));
            }
            if (Input.GetKey("a"))
            {
                rb.AddForce(new Vector3(-10, 0, 0));
            }
            if (Input.GetKey("d"))
            {
                rb.AddForce(new Vector3(10, 0, 0));
            }
            PDB_mesh other_mesh = other.GetComponent<PDB_mesh>();

    	    PDB_molecule.collide(
                gameObject, mol, transform,
                other, other_mesh.mol, other.transform
            );
        }
	}
}
