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
        
        do
        {
            temp.transform.position = temp.transform.position + temp.transform.up * 2;
            Debug.Log("dentro");

        } while (Physics.Raycast(temp.transform.position, Vector3.down, 15));
        Debug.Log("salio");
        temp.transform.position = temp.transform.position + temp.transform.forward * 2;
        temp.GetComponent<ShipController>().enabled = true;
        temp.AddComponent<Rigidbody>();
        temp.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        temp.GetComponent<Rigidbody>().drag = 1;
    }
}
