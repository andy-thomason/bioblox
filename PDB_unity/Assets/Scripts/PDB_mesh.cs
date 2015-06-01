using UnityEngine;
using System.Collections;

public class PDB_mesh : MonoBehaviour {
    PDB_molecule mol;
    public GameObject other;

	Quaternion start;
	Quaternion end;
	bool startRotation=false;
	float t=0;

	// Use this for initialization
	void Start () {
	    MeshFilter f = GetComponent<MeshFilter>();
        PDB_molecule mol = PDB_parser.get_molecule(this.name);
        f.mesh = mol.mesh;
        //f.transform.Translate(mol.pos);
	}

	void BringPointToFocus(Vector3 LocalPoint,Camera c)
	{
		Vector3 startDir = LocalPoint;
		Vector3 targetDir=c.transform.position-this.transform.position;
		
		Quaternion targetQ=Quaternion.LookRotation(targetDir);
		Quaternion startQ=Quaternion.LookRotation(startDir);
		start=transform.rotation;
		
		Quaternion toFront = targetQ * Quaternion.Inverse (startQ);
		
		
		end=toFront;
		startRotation=true;
		t=0;


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
			if(Input.GetMouseButtonDown(0))
			{
				Camera c=GameObject.FindGameObjectWithTag("MainCamera").
					GetComponent<Camera>();
				Ray r=c.ScreenPointToRay(
						Input.mousePosition);

				int hit =PDB_molecule.collide_ray(gameObject,mol,
				                         transform,r);
				if(hit!=-1&&!startRotation)
				{
					Vector3 molDir=mol.atom_centres[hit];
					BringPointToFocus(molDir,c);
				}
			}
			if(startRotation)
			{
				t+=Time.deltaTime;
				transform.localRotation=Quaternion.Slerp(start,end,t);
				if(t>1)
				{
					startRotation=false;
					t=0;
				}



			}
            PDB_mesh other_mesh = other.GetComponent<PDB_mesh>();

    	    PDB_molecule.collide(
                gameObject, mol, transform,
                other, other_mesh.mol, other.transform
            );
        }
	}
}
