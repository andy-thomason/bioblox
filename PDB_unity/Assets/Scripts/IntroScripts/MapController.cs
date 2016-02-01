using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour, IPointerClickHandler
{
    
    float current_scroll;
    public Camera micro_camera;
    public Vector3 zoom_position;
    RectTransform map_panel;
    public GameObject level_description;

    // Use this for initialization
    void Awake () {
        map_panel = GetComponent<RectTransform>();
    }

    bool clicko = false;
    public void OnPointerClick(PointerEventData data)
    {
        //Debug.Log("position mapa : "+map_panel.position);
        //Debug.Log(micro_camera.ScreenToWorldPoint(Input.mousePosition));
        StopAllCoroutines();
    }

    // Update is called once per frame
    void Update ()
    {
        // float key = Mathf.Lerp(0.0f, 40.0f, Time.time);
        //Debug.Log(key);
        //Debug.Log(Input.mousePosition);
        //Debug.Log(micro_camera.cameraToWorldMatrix);

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            StopAllCoroutines();
            if (micro_camera.orthographicSize >= 100 && micro_camera.orthographicSize <= 4700)
            {
                /*Vector3 temp = MainCamera.transform.position;
                temp.z += Input.GetAxis("Mouse ScrollWheel") * 5;
                MainCamera.transform.position = temp;*/

               // current_scroll = Input.GetAxis("Mouse ScrollWheel") / 10;
                if(Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveMapCanvas());
                    micro_camera.orthographicSize = micro_camera.orthographicSize + 200.0f;
                    //map_panel.anchoredPosition = new Vector2((-zoom_position.x + 400.0f), (-zoom_position.y - 237.0f));
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveMapCanvas());
                    micro_camera.orthographicSize = micro_camera.orthographicSize - 200.0f;
                    //map_panel.anchoredPosition = new Vector2((-zoom_position.x + 400.0f), (-zoom_position.y - 237.0f));
                }

                if(micro_camera.orthographicSize > 4700)
                {
                    micro_camera.orthographicSize = 4700;
                }
                
                if(micro_camera.orthographicSize < 100)
                {
                    micro_camera.orthographicSize = 100;
                }
            }
        }
       
        //if maximum zoom, show canvas descrptin
        if(micro_camera.orthographicSize == 100 && !level_description.activeSelf)
        {
            level_description.SetActive(true);
        }
        if (micro_camera.orthographicSize > 100 && level_description.activeSelf)
        {
            level_description.SetActive(false);
        }
    }

    IEnumerator MoveMapCanvas()
    {
        float timeSinceStarted = 0f;
        Vector3 newPosition = map_panel.position - micro_camera.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0;
        while (true)
        {
            timeSinceStarted += Time.deltaTime;
            map_panel.anchoredPosition = Vector3.Lerp(map_panel.anchoredPosition, newPosition, timeSinceStarted);

            // If the object has arrived, stop the coroutine
            if (map_panel.anchoredPosition.x == newPosition.x && map_panel.anchoredPosition.y == newPosition.y)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    public void DoubleClickLevel()
    {
        StopAllCoroutines();
        StartCoroutine(MoveMapCanvasDoubleClick());
    }

    IEnumerator MoveMapCanvasDoubleClick()
    {
        float timeSinceStarted = 0f;
        Vector3 newPosition = new Vector2((-zoom_position.x), (-zoom_position.y));
        while (true)
        {
            timeSinceStarted += Time.deltaTime;

            if(timeSinceStarted <= 1.0f)
            {
                map_panel.anchoredPosition = Vector3.Lerp(map_panel.anchoredPosition, newPosition, timeSinceStarted);
            }

            if (micro_camera.orthographicSize > 100.0f)
            {
                micro_camera.orthographicSize = micro_camera.orthographicSize - 100.0f;
            }
            

            // If the object has arrived, stop the coroutine
            if (timeSinceStarted > 1.0f && micro_camera.orthographicSize == 100.0f)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    public void OnClickProtein()
    {
        StopAllCoroutines();
        StartCoroutine(MoveMapCanvas());
        micro_camera.orthographicSize = micro_camera.orthographicSize - 1000.0f;
    }

}
