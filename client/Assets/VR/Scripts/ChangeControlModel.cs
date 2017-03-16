using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeControlModel : MonoBehaviour {

    GameObject default_hand;
    GameObject point_hand;
    GameObject close_hand;

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
    }

    // Use this for initialization
    void Start ()
    {
        default_hand = transform.GetChild(0).transform.GetChild(0).gameObject;
        point_hand = transform.GetChild(0).transform.GetChild(1).gameObject;
        close_hand = transform.GetChild(0).transform.GetChild(2).gameObject;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Controller.GetHairTriggerDown())
        {
            default_hand.SetActive(false);
            point_hand.SetActive(false);
            close_hand.SetActive(true);
        }

        // release trigger
        if (Controller.GetHairTriggerUp())
        {
            default_hand.SetActive(true);
            point_hand.SetActive(false);
            close_hand.SetActive(false);
        }

        // press grip
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            default_hand.SetActive(false);
            point_hand.SetActive(true);
            close_hand.SetActive(false);
        }

        // press grip
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            default_hand.SetActive(true);
            point_hand.SetActive(false);
            close_hand.SetActive(false);
        }
        
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            default_hand.SetActive(false);
            point_hand.SetActive(true);
            close_hand.SetActive(false);
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            default_hand.SetActive(true);
            point_hand.SetActive(false);
            close_hand.SetActive(false);
        }

        //if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        //{
        //    default_hand.SetActive(true);
        //    point_hand.SetActive(false);
        //    close_hand.SetActive(false);
        //}

        //else
        //{
        //    default_hand.SetActive(true);
        //    point_hand.SetActive(false);
        //    close_hand.SetActive(false);
        //}
    }
}
