using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {


    Rigidbody rb;
    BioBlox bb;
	// Use this for initialization
	void Start () {
        //rb = GetComponent<Rigidbody>();
        bb = FindObjectOfType<BioBlox>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        /*if (Physics.Raycast(transform.position, Vector3.down, 3))
            GetComponent<Rigidbody>().AddForce(Vector3.up * 2);*/

        /* Ray ray = new Ray(transform.position, -transform.up);
         RaycastHit hit;

         if (Physics.Raycast(ray, out hit, 5))
         {
             float proportionalHeight = (5 - hit.distance) / 5;
             Vector3 appliedHoverForce = Vector3.up * proportionalHeight * 30;
             GetComponent<Rigidbody>().AddForce(appliedHoverForce, ForceMode.Acceleration);
         }*/

        float altitude = PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, new Ray(transform.position, -transform.up), gameObject);
        if (altitude < 3.0f)
        {
            Debug.Log("puja");
            GetComponent<Rigidbody>().velocity = Vector3.zero;

        }

        Debug.Log(PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, new Ray(transform.position, -transform.up), gameObject));

        if (Input.GetKey(KeyCode.Q))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 2, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.E))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.down * 2, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Rigidbody>().AddTorque(transform.up * Time.deltaTime * 10);
        }

        if (Input.GetKey(KeyCode.D))
        {
            GetComponent<Rigidbody>().AddTorque(-transform.up * Time.deltaTime * 10);
        }

        //gameObject.transform.LookAt(bb.molecules[0].transform);

    }
}
