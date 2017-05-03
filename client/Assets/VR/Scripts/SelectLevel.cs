using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SelectLevel : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;// 1
    // 2
    public GameObject laser;
    // 3
    // 4
    private Vector3 hitPoint;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    int current_level_id = -1;
    int current_level_id_temp = -1;
    GameObject current_level_selected;
    public Text name_level;
    public Text description;
    public GameObject panel;

    public Material laser_off;
    public Material laser_on;
    Renderer laser_mat;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start()
    {
        laser_mat = laser.GetComponent<Renderer>();
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
                laser_mat.material = laser_off;
            }
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            laser_mat.material = laser_off;
        }
    }

    GameObject current_panel;

    private void ShowLaser(RaycastHit hit)
    {
        // 1
        laser_mat.material = laser_on;
        // 2
        //laser.transform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        // 3
        //laserTransform.LookAt(hitPoint);
        // 4
        //laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, hit.distance);
        if (hit.transform.tag != "level_cubes")
        {
            if (current_level_id != hit.transform.GetComponent<LevelSelectionController>().level_id)
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
                description.text = current_level_selected.GetComponent<LevelSelectionController>().description;

            }
        }
        else
        {
            if (current_level_id_temp != hit.transform.GetComponent<LevelSelectionController>().temp_level_id)
            {
                try
                {
                    current_panel.SetActive(false);
                }
                catch (NullReferenceException ex)
                {
                    Debug.Log(ex);
                }

                if (hit.transform.GetComponent<LevelSelectionController>().level_id == -2)
                    FindObjectOfType<GameManager>().ChangeLevel(current_level_id);

                current_panel = hit.transform.GetChild(0).gameObject;
                current_panel.SetActive(true);
                current_level_selected = hit.transform.gameObject;
                current_level_id_temp = current_level_selected.GetComponent<LevelSelectionController>().temp_level_id;
                current_level_id = current_level_selected.GetComponent<LevelSelectionController>().level_id;
                current_panel.transform.GetChild(1).GetComponent<Text>().text = current_level_selected.GetComponent<LevelSelectionController>().level_name;
                current_panel.transform.GetChild(2).GetComponent<Text>().text = current_level_selected.GetComponent<LevelSelectionController>().description;
            }
        }
    }
}
