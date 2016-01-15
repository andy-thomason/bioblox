using UnityEngine;
using System.Collections;

public class MapController : MonoBehaviour {

    public GameObject MapCanvas;
    float current_scroll;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (MapCanvas.transform.localScale.x >= 0.05f && MapCanvas.transform.localScale.x <= 2.0f)
            {
                /*Vector3 temp = MainCamera.transform.position;
                temp.z += Input.GetAxis("Mouse ScrollWheel") * 5;
                MainCamera.transform.position = temp;*/

                current_scroll = Input.GetAxis("Mouse ScrollWheel") / 10;
                MapCanvas.transform.localScale += new Vector3(current_scroll, current_scroll, current_scroll);
            }

            if(MapCanvas.transform.localScale.x <= 0.05f)
            {
                MapCanvas.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            if (MapCanvas.transform.localScale.x >= 2.0f)
            {
                MapCanvas.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            }
        }
    }
}
