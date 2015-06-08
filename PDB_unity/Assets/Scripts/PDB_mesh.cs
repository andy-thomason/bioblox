using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class PDB_mesh : MonoBehaviour {
    public PDB_molecule mol;
    public GameObject other;

	Vector3 joinedPos = new Vector3();
	Quaternion joinedRotation = Quaternion.identity;
	Vector3 autoDockStartPos;
	Quaternion autoDockStartRotation;

	bool allowInteraction=true;

	Quaternion start;
	Quaternion end;
	bool startRotation=false;
	float t=0;

	// Use this for initialization
	void Start () {
	    MeshFilter f = GetComponent<MeshFilter>();
        mol = PDB_parser.get_molecule(this.name);
        f.mesh = mol.mesh;
		autoDockStartPos = this.transform.position;
		autoDockStartRotation = this.transform.rotation;
		joinedPos = mol.pos;
	}
	public void AlignAtomToVector(int atomIndex, Vector3 targetDir)
	{
		Vector3 localPos = mol.atom_centres [atomIndex];
		Vector3 startDir = localPos;
		
		Quaternion targetQ=Quaternion.LookRotation(targetDir);
		Quaternion startQ=Quaternion.LookRotation(startDir);
		start=transform.rotation;
		
		Quaternion toFront = targetQ * Quaternion.Inverse (startQ);

		end=toFront;
		startRotation=true;
		t=0;
	}

	public void BringAtomToFocus(int atomIndex)
	{
			Camera c = GameObject.FindGameObjectWithTag ("MainCamera").
				GetComponent<Camera> ();

			Vector3 localPos = mol.atom_centres [atomIndex];
		BringPointToFocus (localPos, c);
	}
	//rotates this object to bring that point closest to the camera
	void BringPointToFocus(Vector3 localPoint,Camera c)
	{
		Vector3 startDir = localPoint;
		Vector3 targetDir=c.transform.position-this.transform.position;
		
		Quaternion targetQ=Quaternion.LookRotation(targetDir);
		Quaternion startQ=Quaternion.LookRotation(startDir);
		start=transform.rotation;
		
		Quaternion toFront = targetQ * Quaternion.Inverse (startQ);
		
		
		end=toFront;
		startRotation=true;
		t=0;
	}

	//at the moment very fake
	public void AutoDock()
	{
		allowInteraction = false;
		TransformLerper mover = gameObject.GetComponent<TransformLerper>();

		t = 0;
		//mover.AddTransformPoint (autoDockStartPos, autoDockStartRotation);
		//mover.AddTransformPoint(mol.pos,Quaternion.identity);
		mover.StartTransform();
	}


	// Update is called once per frame
	void Update () {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (other) {
			if (gameObject.GetComponent<TransformLerper> ().finished == true) {
				allowInteraction = true;
			}
			if (allowInteraction) {
//				if (Input.GetKey ("w")) {
//					rb.AddForce (new Vector3 (0, 10, 0));
//				}
//				if (Input.GetKey ("s")) {
//					rb.AddForce (new Vector3 (0, -10, 0));
//				}
//				if (Input.GetKey ("a")) {
//					rb.AddForce (new Vector3 (-10, 0, 0));
//				}
//				if (Input.GetKey ("d")) {
//					rb.AddForce (new Vector3 (10, 0, 0));
//				}
				if (Input.GetKey ("p")) {
					AutoDock ();
				}
//				if (Input.GetMouseButtonDown (0)) {
//					Camera c = GameObject.FindGameObjectWithTag ("MainCamera").
//					GetComponent<Camera> ();
//					Ray r = c.ScreenPointToRay (
//						Input.mousePosition);
//
//					RaycastHit info= new RaycastHit();
//					Physics.Raycast(r,out info);
//					if(info.collider==null)
//					{
//
//						int hit = PDB_molecule.collide_ray (gameObject, mol,
//				                         transform, r);
//						Debug.Log (hit);
//						if (hit != -1 && !startRotation) {
//							Vector3 molDir = mol.atom_centres [hit];
//							BringPointToFocus (molDir, c);
//						}
//					}
//				}
				if (startRotation) {
					t += Time.deltaTime;
					transform.localRotation = Quaternion.Slerp (start, end, t);
					if (t > 1) {
						startRotation = false;
						t = 0;
					}
				}
				PDB_mesh other_mesh = other.GetComponent<PDB_mesh> ();

				//PDB_molecule.collide (
               // gameObject, mol, transform,
               // other, other_mesh.mol, other.transform
				//);
			}
		}
	}
}
