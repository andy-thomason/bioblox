using UnityEngine;
using System.Collections;

public class DataManager : MonoBehaviour {


    BioBlox bb;
    GameManager gm;
    WWWForm www_form;

    // Use this for initialization
    void Start ()
    {
        bb = FindObjectOfType<BioBlox>();
        gm = FindObjectOfType<GameManager>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void SendUserData(string user_data)
    {
        string[] splitArray = user_data.Split('+');
        string id_user = splitArray[0];
        string username = splitArray[1];

        gm.id_user = int.Parse(id_user);

        www_form = new WWWForm();
        www_form.AddField("id_user", id_user);
        www_form.AddField("username", username);
        StartCoroutine(insertUser());
    }

    IEnumerator insertUser()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/userlog.php", www_form);
        yield return SQLQuery;
    }

    public void SendSaveData(string n_atoms, string lpj, string ei)
    {
        //create position/rotation
        string p1_position = bb.molecules[0].transform.localPosition.x + "," + bb.molecules[0].transform.localPosition.y + "," + bb.molecules[0].transform.localPosition.z;
        string p2_position = bb.molecules[1].transform.localPosition.x + "," + bb.molecules[1].transform.localPosition.y + "," + bb.molecules[1].transform.localPosition.z;
        string p1_rotation = bb.molecules[0].transform.eulerAngles.x + "," + bb.molecules[0].transform.eulerAngles.y + "," + bb.molecules[0].transform.eulerAngles.z;
        string p2_rotation = bb.molecules[1].transform.eulerAngles.x + "," + bb.molecules[1].transform.eulerAngles.y + "," + bb.molecules[1].transform.eulerAngles.z;

        www_form = new WWWForm();
        www_form.AddField("id_user", gm.id_user);
        www_form.AddField("level", gm.current_level);
        www_form.AddField("n_atoms", n_atoms);
        www_form.AddField("lpj", lpj);
        www_form.AddField("ei", ei);
        www_form.AddField("p1_position", p1_position);
        www_form.AddField("p2_position", p2_position);
        www_form.AddField("p1_rotation", p1_rotation);
        www_form.AddField("p2_rotation", p2_rotation);
        StartCoroutine(insertSave());
    }

    IEnumerator insertSave()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/insert_score.php", www_form);
        yield return SQLQuery;
    }
}
