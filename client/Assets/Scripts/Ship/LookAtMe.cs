using UnityEngine;
using System.Collections;

public class LookAtMe : MonoBehaviour {

    GameObject camera_ship;

	// Use this for initialization
	void Start ()
    {
        camera_ship = GameObject.FindGameObjectWithTag("camera_ship");

    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.LookAt(camera_ship.transform);
	}
}
