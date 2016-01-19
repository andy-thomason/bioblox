using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour, IPointerClickHandler
{
    
    float current_scroll;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (transform.localScale.x >= 0.05f && transform.localScale.x <= 2.05f)
            {
                /*Vector3 temp = MainCamera.transform.position;
                temp.z += Input.GetAxis("Mouse ScrollWheel") * 5;
                MainCamera.transform.position = temp;*/

               // current_scroll = Input.GetAxis("Mouse ScrollWheel") / 10;
                if(Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    transform.localScale += new Vector3(-0.1f, -0.1f, -0.1f);
                }
               // MapCanvas.transform.position = Input.mousePosition;
            }

            if(transform.localScale.x <= 0.05f)
            {
                transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
            if (transform.localScale.x >= 2.05f)
            {
                transform.localScale = new Vector3(2.05f, 2.05f, 2.05f);
            }
        }
    }

    public void OnPointerClick(PointerEventData data)
    {
        Vector2 viewportPoint = GameObject.Find("MiscroscopeCamera").GetComponent<Camera>().WorldToViewportPoint(Input.mousePosition);
        Debug.Log(viewportPoint);
        GetComponent<RectTransform>().anchorMax = viewportPoint;
        GetComponent<RectTransform>().anchorMin = viewportPoint;

    }
}
