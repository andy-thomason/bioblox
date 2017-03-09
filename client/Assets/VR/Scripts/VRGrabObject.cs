using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabObject : MonoBehaviour
{

    BioBlox bb;
    // control
    private SteamVR_TrackedObject trackedObj;
    // get the obejct to grab
    private GameObject collidingObject;
    // currebntly grabing
    private GameObject objectInHand;
    // control access
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        bb = FindObjectOfType<BioBlox>();
    }

    private void SetCollidingObject(Collider col)
    {
        // not going to grab if I have something in hand
        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }
        // 2
        collidingObject = col.gameObject;
        bb.is_grabbed = true;
    }


    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    // 2
    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    // 3
    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
        bb.is_grabbed = false;
    }

    private void GrabObject()
    {
        // 1
        objectInHand = collidingObject;
        collidingObject = null;
        // 2
        objectInHand.GetComponent<Rigidbody>().velocity = Vector3.zero;
        objectInHand.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    // 3
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject()
    {
        // 1
        if (GetComponent<FixedJoint>())
        {
            // 2
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            // 3
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity * 100;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity * 100;
            objectInHand.GetComponent<SphereCollider>().isTrigger = false;
        }
        // 4
        objectInHand = null;
    }

    // Update is called once per frame
    void Update()
    {
        // press trigger
        if (Controller.GetHairTriggerDown())
        {
            if (collidingObject)
            {
                GrabObject();
            }
        }

        // release trigger
        if (Controller.GetHairTriggerUp())
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }

        // press grip
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            if (collidingObject)
            {
                bb.ChangeProteinRenderer(collidingObject.GetComponent<PDB_mesh>().protein_id);
            }
        }

        ////position on the pad
        //if (Controller.GetAxis() != Vector2.zero)
        //{
        //    if (Controller.GetAxis().x > 0)
        //        transform.parent.Translate(Vector3.right);
        //    if (Controller.GetAxis().x < 0)
        //        transform.parent.Translate(Vector3.left);
        //    if (Controller.GetAxis().y > 0)
        //        transform.parent.Translate(Vector3.forward);
        //    if (Controller.GetAxis().y < 0)
        //        transform.parent.Translate(Vector3.back);
        //}

    }


}
