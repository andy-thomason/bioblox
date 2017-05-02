using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateProteins : MonoBehaviour {

    // control
    private SteamVR_TrackedObject trackedObj;
    // control access
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
    public Transform cubo;


    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Use this for initialization
    void Start () {
		
	}
    Vector3 startpos = Vector3.zero;
    Vector3 lastpost;
    Vector3 current_post;

    // Update is called once per frame
    void Update ()
    {
        // press grip
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip))
        {
            
                cubo.transform.Rotate(Controller.angularVelocity);
            
            //Debug.Log(Controller.angularVelocity);
            //float rotX = transform.TransformDirection * Mathf.Deg2Rad;
            //float rotY = transform.position.y * Mathf.Deg2Rad;

            //cubo.RotateAround(Vector3.up, -rotX);
            //cubo.RotateAround(Vector3.right, rotY);
            //cubo.transform.Rotate(0, transform.position.x, 0);



            //if there is no recent input reset previous position

            //t = 0.0f;
            lastpost = transform.position;

        }

        // press grip
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
        }
    }
}
