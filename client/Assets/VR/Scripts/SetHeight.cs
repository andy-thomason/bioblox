using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SetHeight : MonoBehaviour {

    public Transform Headset;
    public Transform molecules;

	// Use this for initialization
	void Start ()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
            transform.position = new Vector3(0, (-1.42f + Headset.position.y), 0);
        else
        {
            transform.localPosition = new Vector3(0, (-119.0f + Headset.position.y), (26.0f + Headset.position.z));
            //PROTEINS
            //molecules.GetChild(0).transform.position = Vector3.zero;
            //molecules.GetChild(1).transform.position = Vector3.zero;
            molecules.position = new Vector3(0, (45.0f + Headset.position.y), (18.0f + Headset.position.z));
           // molecules.GetChild(0).localPosition = Vector3.zero;
            //molecules.GetChild(1).localPosition = Vector3.zero;
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
