using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ChangeControlModeMain : MonoBehaviour {

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

    BioBlox bb;
    int previous_atoms = -1;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        bb = FindObjectOfType<BioBlox>();
    }

    // Use this for initialization
    void Start()
    {
        default_hand = transform.GetChild(0).transform.GetChild(0).gameObject;
        point_hand = transform.GetChild(0).transform.GetChild(1).gameObject;
        close_hand = transform.GetChild(0).transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Controller.GetHairTriggerDown())
        {
            default_hand.SetActive(false);
            point_hand.SetActive(false);
            close_hand.SetActive(true);
            //deselect
            bb.molecules_PDB_mesh[0].DeselectAminoAcid();
            bb.molecules_PDB_mesh[1].DeselectAminoAcid();
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
            //deselect
            bb.molecules_PDB_mesh[0].DeselectAminoAcid();
            bb.molecules_PDB_mesh[1].DeselectAminoAcid();
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

        //vibration
        if (bb.number_total_atoms > 0 && bb.number_total_atoms != previous_atoms)
        {
            Controller.TriggerHapticPulse(400);
            previous_atoms = bb.number_total_atoms;
        }
    }
}
