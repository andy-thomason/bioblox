using UnityEngine;
using System.Collections;

public class ArponController : MonoBehaviour {

    BioBlox bb;
    AminoSliderController aminoSliderController;
    GameObject current_camera;
    int protein_to_shot;

	// Use this for initialization
	void Start () {
        bb = FindObjectOfType<BioBlox>();
        aminoSliderController = FindObjectOfType<AminoSliderController>();
        current_camera = GameObject.FindGameObjectWithTag("FirstPerson");
        if (current_camera.transform.parent.gameObject.GetComponent<PDB_mesh>().protein_id == 0)
            protein_to_shot = 1;
        else
            protein_to_shot = 0;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        BvhSphereCollider molecule = new BvhSphereCollider(bb.molecules[protein_to_shot].GetComponent<PDB_mesh>().mol, bb.molecules[protein_to_shot].transform, transform.position, 1f);

        if (molecule.results.Count > 0)
        {
            //Debug.Log(bb.molecules[0].transform.TransformPoint(bb.molecules[protein_to_shot].GetComponent<PDB_mesh>().mol.atom_centres[molecule.results[0].index]));
            bb.molecules[protein_to_shot].GetComponent<PDB_mesh>().SelectAtom(molecule.results[0].index);
            aminoSliderController.AddConnectionButton();
            transform.position = bb.molecules[protein_to_shot].transform.TransformPoint(bb.molecules[protein_to_shot].GetComponent<PDB_mesh>().mol.atom_centres[molecule.results[0].index]);
            GetComponent<TrailRenderer>().enabled = false;
            GetComponent<ArponController>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            transform.SetParent(bb.molecules[protein_to_shot].transform);
            // Destroy(gameObject);
        }

        //realistic flying
        transform.forward = Vector3.Slerp(transform.forward, GetComponent<Rigidbody>().velocity.normalized, Time.deltaTime * 15);


        //BvhSphereCollider molecule_1 = new BvhSphereCollider(bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, transform.position, 1f);
        //BvhSphereCollider molecule_2 = new BvhSphereCollider(bb.molecules[1].GetComponent<PDB_mesh>().mol, bb.molecules[1].transform, transform.position, 1f);

        //if (molecule_1.results.Count > 0)
        //{
        //    Debug.Log(bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[0].index]));
        //    bb.molecules[0].GetComponent<PDB_mesh>().SelectAtom(molecule_1.results[0].index);
        //    aminoSliderController.AddConnectionButton();
        //    transform.position = bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[0].index]);
        //    GetComponent<TrailRenderer>().enabled = false;
        //    GetComponent<ArponController>().enabled = false;
        //    GetComponent<Rigidbody>().isKinematic = true;
        //    transform.SetParent(bb.molecules[0].transform);
        //    // Destroy(gameObject);
        //}

        //if (molecule_2.results.Count > 0)
        //{
        //    Debug.Log(bb.molecules[1].transform.TransformPoint(bb.molecules[1].GetComponent<PDB_mesh>().mol.atom_centres[molecule_2.results[1].index]));
        //    bb.molecules[1].GetComponent<PDB_mesh>().SelectAtom(molecule_2.results[0].index);
        //    aminoSliderController.AddConnectionButton();
        //    transform.position = bb.molecules[1].transform.TransformPoint(bb.molecules[1].GetComponent<PDB_mesh>().mol.atom_centres[molecule_2.results[0].index]);
        //    GetComponent<TrailRenderer>().enabled = false;
        //    GetComponent<ArponController>().enabled = false;
        //    GetComponent<Rigidbody>().isKinematic = true;
        //    transform.SetParent(bb.molecules[1].transform);
        //    //Destroy(gameObject);
        //}
    }

    void OnCollisionEnter(Collision collision)
    {
        GetComponent<ArponController>().enabled = false;
    }
}
