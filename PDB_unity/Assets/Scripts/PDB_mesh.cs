using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;

public class PDB_mesh : MonoBehaviour {
    public PDB_molecule mol;
    public GameObject other;

	bool allowInteraction=true;

	Quaternion start;
	Quaternion end;
	bool startRotation=false;
	public bool shouldCollide =false;
	public float seperationForce =100.0f;
	float t=0;

	public bool hasCollided=false;

	// Use this for initialization
	void Start () {
	  
        mol = PDB_parser.get_molecule(this.name);
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

	public Vector3 GetAtomWorldPositon(int atomIndex)
	{
		return transform.TransformPoint (mol.atom_centres [atomIndex]);
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

	public void OneAxisDock()
	{
		shouldCollide = true;
		allowInteraction = false;
		startRotation = false;

		Rigidbody rigidBody1 = gameObject.GetComponent<Rigidbody> ();
		Rigidbody rigidBody2 = other.GetComponent<Rigidbody> ();

		rigidBody1.constraints = RigidbodyConstraints.FreezeRotation;
		rigidBody2.constraints = RigidbodyConstraints.None;

		rigidBody1.drag = 2.0f;
		rigidBody2.drag = 2.0f;

		Vector3 usToOth = other.transform.position - gameObject.transform.position;
		Vector3 othToUs = gameObject.transform.position - other.transform.position;


		Vector3 anchor = transform.InverseTransformPoint (transform.position + usToOth.normalized);
		Vector3 otherAnchor = other.transform.InverseTransformPoint (other.transform.position + othToUs.normalized);

		SpringJoint j = gameObject.AddComponent<SpringJoint> ();

		j.anchor = anchor;
		j.connectedBody = rigidBody2;
		j.autoConfigureConnectedAnchor = false;
		j.connectedAnchor = otherAnchor;
		j.damper = 6.0f;
		j.spring = 0.5f;
	}

	public void AutoDock()
	{
		shouldCollide = true;
		allowInteraction = false;
		startRotation = false;
		PDB_molecule otherMol = other.GetComponent<PDB_mesh> ().mol;
		Rigidbody r1 = gameObject.GetComponent<Rigidbody> ();
		Rigidbody r2 = other.GetComponent<Rigidbody> ();

		r1.constraints = RigidbodyConstraints.None;
		r2.constraints = RigidbodyConstraints.None;
		r1.drag = 2.0f;
		r2.drag = 2.0f;

		for (int i=0; i<mol.spring_pairs.Length; ++i) {
			int index1=mol.serial_to_atom[mol.spring_pairs[i].First];
			int index2=otherMol.serial_to_atom[mol.spring_pairs[i].Second];

			Vector3 atomPos1 = mol.atom_centres[index1];
			Vector3 atomPos2 =otherMol.atom_centres[index2];

			float atomRad1= mol.atom_radii[index1];
			float atomRad2=otherMol.atom_radii[index2];

			SpringJoint j=gameObject.AddComponent<SpringJoint>();

			j.anchor=atomPos1;
			j.connectedBody=r2;
			j.autoConfigureConnectedAnchor=false;
			j.connectedAnchor=atomPos2;
			j.damper=3.0f;
			j.spring=3.0f;
			j.minDistance=atomRad1+atomRad2-0.2f;
		}
	}

	public bool OneAxisHasDocked()
	{
		Tuple<int,int>[] pairs = mol.spring_pairs;
		PDB_molecule otherMol = other.GetComponent<PDB_mesh> ().mol;
		for(int i = 0; i < pairs.Length; i++)
		{
			Vector3 pos1 = transform.TransformPoint(mol.atom_centres[mol.serial_to_atom[pairs[i].First]]);
			Vector3 pos2 = other.transform.TransformPoint(otherMol.atom_centres[otherMol.serial_to_atom[pairs[i].Second]]);

			float rad=mol.atom_radii[mol.serial_to_atom[pairs[i].First]];

			float sqrdDist= (pos1-pos2).sqrMagnitude;
			float sqrdRad = (rad+rad)*(rad*rad);
			float offset = 0.4f;

			if(sqrdDist > sqrdRad + offset)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasDocked ()
	{
		for (int i=0; i<gameObject.GetComponents<SpringJoint>().Length; ++i) {
			SpringJoint s=gameObject.GetComponents<SpringJoint>()[i];

			Vector3 wpos1=transform.TransformPoint(s.anchor);
			Vector3 wpos2=other.transform.TransformPoint(s.connectedAnchor);

			if((wpos1-wpos2).sqrMagnitude>(s.minDistance+0.2f)*(s.minDistance+0.2f))
			{
				return false;
			}
		}
		return true;
	}
	//at the moment very fake
	public void AutoDockCheap()
	{
		allowInteraction = false;
		TransformLerper mover = gameObject.GetComponent<TransformLerper>();
		startRotation = false;
		t = 0;
		//mover.AddTransformPoint (autoDockStartPos, autoDockStartRotation);
		//mover.AddTransformPoint(mol.pos,Quaternion.identity);
		mover.StartTransform();
	}


	// Update is called once per frame
	void Update () {
        if (other) {
			if (Input.GetMouseButtonDown(1)) {
				Camera c = GameObject.FindGameObjectWithTag ("MainCamera").
					GetComponent<Camera> ();
				Ray r = c.ScreenPointToRay (Input.mousePosition);
				
				RaycastHit info = new RaycastHit();
				Physics.Raycast(r, out info);
				if (info.collider == null)
				{
					
					int hit = PDB_molecule.collide_ray (gameObject, mol,
					                                    transform, r);
					for(int i = 0; i < mol.serial_to_atom.Length; ++i)
					{
						if (mol.serial_to_atom[i] == hit)
						{

							break;
						}

					}
				}
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
				if(Input.GetKey("p"))
				{
					this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
					other.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
					AutoDockCheap();
				}

				if (startRotation) {
					t += Time.deltaTime;
					transform.localRotation = Quaternion.Slerp (start, end, t);
					if (t > 1) {
						startRotation = false;
						t = 0;
					}
				}
			}
			if(shouldCollide)
			{
				PDB_mesh other_mesh = other.GetComponent<PDB_mesh> ();
				
				if(PDB_molecule.pysics_collide (
					gameObject, mol, transform,
					other, other_mesh.mol, other.transform
					,seperationForce
					))
				{
					hasCollided=true;
				}
				
				
				
				
			}
		}
	}
}
