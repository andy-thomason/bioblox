using UnityEngine;
using System.Collections;
using System.IO;

public class DataController : MonoBehaviour {

    public string[] level_index;

	// Use this for initialization
	void Awake ()
    {
        //GET the xml from the server	
        StartCoroutine(DownloadData());
    }

    IEnumerator DownloadData()
    {
        WWW DataConnection = new WWW("http://quiley.com/BB/names4.txt");
        //WWW DataConnection = new WWW("http://158.223.59.221:8080/data/names.txt"); 
        yield return DataConnection;
        Debug.Log(DataConnection.text);
        level_index = DataConnection.text.Split('\n');
        /*foreach(string caca in level_index)
        {
            Debug.Log(caca);
        }*/
    }
}
