using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectLevel : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;// 1
    public GameObject laserPrefab;
    // 2
    private GameObject laser;
    // 3
    private Transform laserTransform;
    // 4
    private Vector3 hitPoint;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    int current_level_id = -1;
    GameObject current_level_selected;
    public Text name_level;
    public GameObject panel;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        // 1
        laser = Instantiate(laserPrefab);
        // 2
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            RaycastHit hit;
            // 2
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100))
            {
                hitPoint = hit.point;
                ShowLaser(hit);
            }
            else
            {
                laser.SetActive(false);
            }
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            laser.SetActive(false);
        }
    }

    private void ShowLaser(RaycastHit hit)
    {
        // 1
        laser.SetActive(true);
        // 2
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        // 3
        laserTransform.LookAt(hitPoint);
        // 4
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);

        if(current_level_id != hit.transform.GetComponent<LevelSelectionController>().level_id)
        {
            if (current_level_id != -1)
                current_level_selected.GetComponent<LevelSelectionController>().StopHintMovement();

            if (hit.transform.GetComponent<LevelSelectionController>().level_id == -2)
                FindObjectOfType<GameManager>().ChangeLevel(current_level_id);

            panel.SetActive(true);
            current_level_selected = hit.transform.gameObject;
            current_level_selected.GetComponent<LevelSelectionController>().StartHintMovement();
            current_level_id = current_level_selected.GetComponent<LevelSelectionController>().level_id;
            name_level.text = current_level_selected.GetComponent<LevelSelectionController>().level_name;
        }
    }
}
