using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DataManager : MonoBehaviour {


    BioBlox bb;
    GameManager gm;
    WWWForm www_form;
    public string id_user;
    int number_of_level;
    Transform level_holder;

    // Use this for initialization
    void Start ()
    {
        bb = FindObjectOfType<BioBlox>();
        gm = FindObjectOfType<GameManager>();
        ////temp
        //StartCoroutine(insertUser());
        ////temp
    }

    // Update is called once per frame
    void Update () {
	
	}

    void SendUserData(string user_data)
    {
        string[] splitArray = user_data.Split('+');
        id_user = splitArray[0];
        string username = splitArray[1];

        //get nubmer of levels
        number_of_level = GameObject.FindGameObjectWithTag("level_holder").gameObject.transform.childCount - 1;

        www_form = new WWWForm();
        www_form.AddField("id_user", id_user);
        www_form.AddField("username", username);
        www_form.AddField("number_of_levels", number_of_level);

        StartCoroutine(insertUser());
    }

    IEnumerator insertUser()
    {
        ////temp
        //www_form = new WWWForm();
        //www_form.AddField("id_user", 2);
        //www_form.AddField("username", "pedro");
        //gm.id_user = 2;
        ////temp
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/userlog.php", www_form);
        yield return SQLQuery;
        gm.SetID();
        
        ////get the number of levels
        //level_holder = GameObject.FindGameObjectWithTag("level_holder").gameObject.transform;
        ////slots holder
        //int slot_holder_index = level_holder.GetChild(0).transform.childCount - 1;

        ////SPLIT
        //string[] splitScores = (SQLQuery.text).Split('+');

        //TUTORIAL
        if (SQLQuery.text == "0")
            gm.welcome_tutorial.SetActive(true);

        ////ASSIGN THE SCORES TO THE BUTTONS
        //for (int i = 1; i <= number_of_level; i++)
        //{
        //    string[] splitScores_slot = splitScores[i].Split('*');
        //    //Debug.Log(splitScores[i]);
        //    for (int j = 0; j <= 2; j++)
        //    {
        //        string[] splitScoresLevel = splitScores_slot[j].Split(',');
        //        level_holder.GetChild(i).transform.GetChild(slot_holder_index).transform.GetChild(j).GetComponent<SlotController>().SetValues(splitScoresLevel);

        //        if(j==0)
        //        {
        //            //assign score ui
        //            for (int l = 0; l <= 3; l++)
        //            {
        //                level_holder.GetChild(i).transform.GetChild(0).transform.GetChild(l + 1).GetComponent<Text>().text = splitScoresLevel[l];
        //            }
        //        }
        //    }

        //}
    }

    public void SendSaveData(int slot, string n_atoms, string lpj, string ei, string game_score, string P1_connections, string P2_connections, float slider_value, string connections, float bar_game_score, int n_connections)
    {
        //create position/rotation
        string p1_position = bb.molecules[0].transform.localPosition.x.ToString("F2") + "," + bb.molecules[0].transform.localPosition.y.ToString("F2") + "," + bb.molecules[0].transform.localPosition.z.ToString("F2");
        string p2_position = bb.molecules[1].transform.localPosition.x.ToString("F2") + "," + bb.molecules[1].transform.localPosition.y.ToString("F2") + "," + bb.molecules[1].transform.localPosition.z.ToString("F2");
        string p1_rotation = bb.molecules[0].transform.eulerAngles.x.ToString("F2") + "," + bb.molecules[0].transform.eulerAngles.y.ToString("F2") + "," + bb.molecules[0].transform.eulerAngles.z.ToString("F2");
        string p2_rotation = bb.molecules[1].transform.eulerAngles.x.ToString("F2") + "," + bb.molecules[1].transform.eulerAngles.y.ToString("F2") + "," + bb.molecules[1].transform.eulerAngles.z.ToString("F2");

        www_form = new WWWForm();
        www_form.AddField("id_user", gm.id_user);
        www_form.AddField("level", gm.current_level);
        www_form.AddField("slot", slot);
        www_form.AddField("n_atoms", n_atoms);
        www_form.AddField("lpj", lpj);
        www_form.AddField("ei", ei);
        www_form.AddField("game_score", game_score);
        www_form.AddField("p1_position", p1_position);
        www_form.AddField("p2_position", p2_position);
        www_form.AddField("p1_rotation", p1_rotation);
        www_form.AddField("p2_rotation", p2_rotation);
        www_form.AddField("p1_connections", P1_connections);
        www_form.AddField("p2_connections", P2_connections);
        www_form.AddField("slider_value", slider_value.ToString());
        www_form.AddField("connections", connections);
        www_form.AddField("bar_game_score", bar_game_score.ToString());
        www_form.AddField("n_connections", n_connections.ToString());
        www_form.AddField("dificulty", bb.level.dificulty.ToString());
        Debug.Log(bb.level.dificulty.ToString());
        StartCoroutine(insertSave());
    }

    IEnumerator insertSave()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/insert_score.php", www_form);
        yield return SQLQuery;
    }

    public void GetLevelScore()
    {
        www_form = new WWWForm();
        
        www_form.AddField("id_user", gm.id_user);
        www_form.AddField("level", gm.current_level);
        www_form.AddField("slot", gm.current_slot);
        StartCoroutine(GetLevelScoreCoru());
    }

    IEnumerator GetLevelScoreCoru()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/load_level.php", www_form);
        yield return SQLQuery;
        bb.SetLevelScoresBeforeStartGame(SQLQuery.text);
    }
}
