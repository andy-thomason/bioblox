using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour
{

    public bool is_hint_moving = false;
    bool first_time = false;
    float journeyLength_0;
    float speed = 2.0F;
    float startTime;

    Vector3 default_position_molecule_0;
    Vector3 from;
    Vector3 to;

    public int level_id;
    public string level_name;
    Transform mc;

    // Use this for initialization
    void Start()
    {
        default_position_molecule_0 = transform.localPosition;
        mc = GameObject.FindGameObjectWithTag("level_selection_view").transform;
    }

    // Update is called once per frame
    void Update()
    {

        if (first_time)
        {

            float distCovered = (Time.time - startTime) * speed;
            float fracJourney = distCovered / journeyLength_0;

            //Debug.Log(molecules_PDB_mesh[0].mol.pos - molecules_PDB_mesh[1].mol.pos);

            transform.localPosition = Vector3.Lerp(from, to, fracJourney);

            //molecules[0].transform.GetChild(0).transform.localPosition = Vector3.Lerp(molecules[0].transform.GetChild(0).transform.localPosition, (molecules_PDB_mesh[0].mol.pos - molecules_PDB_mesh[1].mol.pos), fracJourney);

        }

    }

    public void StartHintMovement()
    {
        first_time = true;
        from = transform.localPosition;
        to = mc.localPosition;
        journeyLength_0 = Vector3.Distance(from, to);
        startTime = Time.time;
    }

    public void StopHintMovement()
    {
        to = default_position_molecule_0;
        from = transform.localPosition;
        journeyLength_0 = Vector3.Distance(from, to);
        startTime = Time.time;
    }


    public void change_scene()
    {
        FindObjectOfType<GameManager>().ChangeLevel(0);
        //SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
