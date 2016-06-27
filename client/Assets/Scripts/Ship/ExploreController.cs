using UnityEngine;
using System.Collections;

public class ExploreController : MonoBehaviour {

    BioBlox bb;
    public GameObject Ship;
    public GameObject MainCamera;
    public GameObject MainCanvas;

	// Use this for initialization
	void Start () {
        bb = FindObjectOfType<BioBlox>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartExplore()
    {
        MainCamera.SetActive(false);
        MainCanvas.SetActive(false);
        GameObject temp = Instantiate(Ship);
        temp.transform.position = bb.molecules[0].transform.position;
        Ray r_temp = new Ray(temp.transform.position, -transform.up);
        do
        {
            temp.transform.position = temp.transform.position + temp.transform.up * 2;
            r_temp = new Ray(temp.transform.position, -transform.up);
            Debug.Log(PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, r_temp, temp)+"dentro");

        } while (PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, r_temp, temp) < 15.0f);
        Debug.Log("salio");
        temp.transform.position = temp.transform.position + temp.transform.forward * 2;

        temp.GetComponent<ShipController>().enabled = true;
        temp.AddComponent<Rigidbody>();
        temp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        temp.GetComponent<Rigidbody>().drag = 1;
        temp.GetComponent<Rigidbody>().useGravity = false;
    }
}
