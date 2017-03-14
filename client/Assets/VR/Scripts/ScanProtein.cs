using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanProtein : MonoBehaviour {

    private GameObject laser;
    BioBlox bb;
    // control
    private SteamVR_TrackedObject trackedObj;
    // control access
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        laser = transform.GetChild(1).gameObject;
        bb = FindObjectOfType<BioBlox>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (Controller.GetAxis() != Vector2.zero)
        {
            bb.is_scanning_amino = true;
            laser.SetActive(true);
        }
        else
        {
            bb.is_scanning_amino = false;
            laser.SetActive(false);
        }
    }
}
