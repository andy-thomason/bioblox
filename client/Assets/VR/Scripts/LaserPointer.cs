using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    GameObject current_level;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();

    }

    // Update is called once per frame
    void Update ()
    {
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            RaycastHit hit;
            Debug.Log(current_level);
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100))
            {
                //normal scale prevuious lvel
                if(current_level != null)
                    current_level.transform.localScale = new Vector3(50f, 50f, 0.005f);

                hitPoint = hit.point;
                current_level = hit.transform.gameObject;
                ShowLaser(hit);
                current_level.transform.localScale = new Vector3(60f, 60f, 0.005f);

            }
            else  
            {
                laser.SetActive(false);
            }
        }
        else
        {
            laser.SetActive(false);
        }
    }

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }
}
