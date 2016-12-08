using UnityEngine;
using System.Collections;

public class DataManager : MonoBehaviour {

    WWWForm user_insert;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SendUserData(string user_data)
    {
        string[] splitArray = user_data.Split('+');
        string id_user = splitArray[0];
        string username = splitArray[1];

        user_insert = new WWWForm();
        user_insert.AddField("id_user", id_user);
        user_insert.AddField("username", username);
        StartCoroutine(insertUser());
    }

    IEnumerator insertUser()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/userlog.php", user_insert);
        yield return SQLQuery;
    }
}
