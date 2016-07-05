using UnityEngine;
using System.Collections;

public class ExploreController : MonoBehaviour {

    BioBlox bb;
    public GameObject Ship;
    public GameObject MainCamera;
    public GameObject MainCanvas;
    public bool exploration_status = false;

    // Use this for initialization
    void Start () {
        bb = FindObjectOfType<BioBlox>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartExplore()
    {
        MainCamera.SetActive(exploration_status);
        MainCanvas.SetActive(exploration_status);
        GameObject temp = Instantiate(Ship);
        temp.tag = "active_ship";
        temp.transform.position = new Vector3(0,70,5);
        /*Ray r_temp = new Ray(temp.transform.position, -transform.up);
        do
        {
            temp.transform.position = temp.transform.position + temp.transform.up * 2;
            r_temp = new Ray(temp.transform.position, -transform.up);
            Debug.Log(PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, r_temp, temp)+"dentro");

        } while (PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, r_temp, temp) < 15.0f);
        Debug.Log("salio");
        temp.transform.position = temp.transform.position + temp.transform.forward * 2;
        */
        temp.GetComponent<ShipController>().enabled = true;
        temp.transform.LookAt(new Vector3(0,0,5));
        temp.AddComponent<Rigidbody>();
        //temp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        temp.GetComponent<Rigidbody>().drag = 25;
        temp.GetComponent<Rigidbody>().useGravity = false;
        exploration_status = !exploration_status;
    }

    public void EndExplore()
    {
        MainCamera.SetActive(exploration_status);
        MainCanvas.SetActive(exploration_status);
        GameObject temp = GameObject.FindGameObjectWithTag("active_ship").gameObject;
        Destroy(temp);
        exploration_status = !exploration_status;
    }
}
