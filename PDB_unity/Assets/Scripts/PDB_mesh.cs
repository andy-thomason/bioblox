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
	public bool shouldCollide = false;
	float t=0;

	// add atom indices to here to display them selected
	public int[] selected_atoms = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

	public bool has_rotated = false;

	//public bool hasCollided=false;

	// Use this for initialization
	void Start () {
	  
        mol = PDB_parser.get_molecule(this.name);
	}

	public void AlignPointToVector(Vector3 point, Vector3 targetDir)
	{
		Vector3 localPos = point;
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

	public void AutoDock(int[] thisAtomIndicies, int[] otherAtomIndicies)
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

		for (int i=0; i<thisAtomIndicies.Length; ++i) {
			int index1=thisAtomIndicies[i];
			int index2=otherAtomIndicies[i];

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
			j.minDistance=atomRad1 + atomRad2 -0.5f;
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

			float dist = (wpos1-wpos2).sqrMagnitude;

			if(dist > s.minDistance*s.minDistance)
			{
				return false;
			}
		}
		return true;
	}
	//at the moment very fake
	public void AutoDockCheap()
	{
		shouldCollide = false;
		this.transform.position = mol.pos;
		this.transform.rotation = Quaternion.identity;
	}


	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp (0)) {
			// click without movement selects an atom/amino acid.
			if (!has_rotated) {
				Camera c = GameObject.FindGameObjectWithTag ("MainCamera").
					GetComponent<Camera> ();
				Ray r = c.ScreenPointToRay (Input.mousePosition);
				RaycastHit info = new RaycastHit();
				int atom = PDB_molecule.collide_ray (gameObject, mol, transform, r);
				if (atom != -1) {
					SelectAtom(atom);
				}

				/*if (allowInteraction) {
					if (startRotation) {
						t += Time.deltaTime;
						transform.localRotation = Quaternion.Slerp (start, end, t);
						if (t > 1) {
							startRotation = false;
							t = 0;
						}
					}
				}*/
			}
			has_rotated = false;
		}
	}

	public Vector4[] GetSelectedAtomUniforms(Camera camera) {
		int len = selected_atoms.Length;
		Vector4[] result = new Vector4[10]; // see PDB.shader _Atom0..9
		int imax = Mathf.Min (len, result.Length);
		for (int i = 0; i != imax; ++i) {
			int sel = selected_atoms[i];
			Vector3 atom_pos = transform.TransformPoint(mol.atom_centres[sel]);
			Vector3 screen_pos = camera.WorldToViewportPoint(atom_pos);
			float size = screen_pos.z / mol.atom_radii[sel];
			result[i] = new Vector4(screen_pos.x, screen_pos.y, size * camera.aspect, size);
		}
		for (int i = len; i < result.Length; ++i) {
			result[i] = new Vector4(0, 0, 1e37f, 1e37f);
		}
		return result;
	}

	// call this to select an amino acid
	public void SelectAminoAcid(int acid_number) {
		selected_atoms = mol.aminoAcidsAtomIds[acid_number];
	}

	// call this to deselect amino acids
	public void DeselectAminoAcid() {
		selected_atoms = new int[0];
	}

	public void SelectAtom(int atom) {
		for (int i = 0; i != mol.aminoAcidsAtomIds.Count; ++i) {
			int[] ids = mol.aminoAcidsAtomIds[i];
			for (int j = 0; j != ids.Length; ++j) {
				if (ids[j] == atom) {
					SelectAminoAcid(i);
					return;
				}
			}
		}
	}
}
