using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledLookAt : MonoBehaviour {

    public Camera main_camera;
	
	// Update is called once per frame
	void Update ()
    {
        transform.LookAt(main_camera.transform);
	}
}
