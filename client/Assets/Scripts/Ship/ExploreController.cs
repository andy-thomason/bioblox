using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExploreController : MonoBehaviour {

    BioBlox bb;
    public GameObject Ship;
    public GameObject MainCamera;
    public GameObject MainCanvas;
    public bool exploration_status = false;
    public List<GameObject> beacon_holder;
    public Toggle toggle_beacons;
    public GameObject Flare;

    // Use this for initialization
    void Start () {
        bb = FindObjectOfType<BioBlox>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StartExplore(GameObject temp)
    {
        //MainCamera.SetActive(exploration_status);
        //MainCanvas.SetActive(exploration_status);
        //GameObject temp = Instantiate(Ship);
       // temp.tag = "active_ship";
        //temp.transform.position = new Vector3(0,70,5);
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
        //temp.transform.LookAt(new Vector3(0,0,5));
        temp.AddComponent<Rigidbody>();
        //temp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        temp.GetComponent<Rigidbody>().drag = 25;
        temp.GetComponent<Rigidbody>().useGravity = false;
        //exploration_status = !exploration_status;
        //if beacons, hide sphere
        //if (beacon_holder.Count != 0)
            //ToggleSphere();
    }

    public void EndExplore()
    {
        //ToggleSphere();
        //MainCamera.SetActive(exploration_status);
        MainCanvas.SetActive(exploration_status);
        GameObject temp = GameObject.FindGameObjectWithTag("active_ship").gameObject;
        Destroy(temp);
       // exploration_status = !exploration_status;
        //GameObject.FindGameObjectWithTag("flare_sun").SetActive(false);
        GameObject.Find("ToolPanel").GetComponent<Animator>().SetBool("Open", true);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void StoreBeacons(GameObject temp_beacon)
    {
        toggle_beacons.interactable = true;
        beacon_holder.Add(temp_beacon);
    }

    public void EmptyBeacons()
    {
        toggle_beacons.interactable = false;
        beacon_holder.Clear();
    }

    bool toggle_beacon = false;

    public void ToggleBeacon()
    {
        foreach(GameObject temp_beacon in beacon_holder)
        {
            temp_beacon.SetActive(toggle_beacon);
        }
        toggle_beacon = !toggle_beacon;
    }

    bool toggle_beacon_sphere = true;

    public void ToggleSphere()
    {
        foreach (GameObject temp_beacon in beacon_holder)
        {
            temp_beacon.transform.GetChild(1).gameObject.SetActive(toggle_beacon_sphere);
        }
        toggle_beacon_sphere = !toggle_beacon_sphere;
    }
}
