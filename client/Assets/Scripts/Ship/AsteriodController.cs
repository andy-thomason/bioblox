using UnityEngine;
using System.Collections;

public class AsteriodController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        if (Input.GetKey(KeyCode.S))
        {
            GetComponent<Rigidbody>().AddRelativeTorque(transform.up * Time.deltaTime * 1000000);
        }

        if (Input.GetKey(KeyCode.W))
        {
            GetComponent<Rigidbody>().AddRelativeTorque(-transform.up * Time.deltaTime * 1000000);
        }
    }
}
