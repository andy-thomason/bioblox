using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour {


    Rigidbody rb;
	// Use this for initialization
	void Start () {
        //rb = GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {

        /*if (Physics.Raycast(transform.position, Vector3.down, 3))
            GetComponent<Rigidbody>().AddForce(Vector3.up * 2);*/

        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5))
        {
            float proportionalHeight = (5 - hit.distance) / 5;
            Vector3 appliedHoverForce = Vector3.up * proportionalHeight * 30;
            GetComponent<Rigidbody>().AddForce(appliedHoverForce, ForceMode.Acceleration);
        }


    }
}
