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
    public GameObject LeftLevelObject;
    public GameObject RightLevelObject;
    //get the current level for the next/bacj level
    public int current_level;
    float x_frame_border;
    LevelMapController levelMapController;

    // Use this for initialization
    void Awake () {
        map_panel = GetComponent<RectTransform>();
        levelMapController = FindObjectOfType<LevelMapController>();
        x_frame_border = Screen.width / 4.0f;
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

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            StopAllCoroutines();
            if (micro_camera.orthographicSize >= 80 && micro_camera.orthographicSize <= 4700)
            {
               // current_scroll = Input.GetAxis("Mouse ScrollWheel") / 10;
                if(Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveMapCanvas());
                    micro_camera.orthographicSize = micro_camera.orthographicSize + 110.0f;
                    //map_panel.anchoredPosition = new Vector2((-zoom_position.x + 400.0f), (-zoom_position.y - 237.0f));
                }
                else
                {
                    StopAllCoroutines();
                    StartCoroutine(MoveMapCanvas());
                    micro_camera.orthographicSize = micro_camera.orthographicSize - 110.0f;
                    //map_panel.anchoredPosition = new Vector2((-zoom_position.x + 400.0f), (-zoom_position.y - 237.0f));
                }

                if(micro_camera.orthographicSize > 4700)
                {
                    micro_camera.orthographicSize = 4700;
                }
                
                if(micro_camera.orthographicSize < 80)
                {
                    micro_camera.orthographicSize = 80;
                }
            }
        }
       
        //if maximum zoom, show canvas descrptin
        if(micro_camera.orthographicSize == 80 && !level_description.activeSelf)
        {
            level_description.SetActive(true);
            LeftLevelObject.SetActive(true);
            RightLevelObject.SetActive(true);
        }
        if (micro_camera.orthographicSize > 80 && level_description.activeSelf)
        {
            level_description.SetActive(false);
            LeftLevelObject.SetActive(false);
            RightLevelObject.SetActive(false);
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
        Vector3 newPosition = new Vector2((-zoom_position.x - x_frame_border + 150.0f), (-zoom_position.y));
        while (true)
        {
            timeSinceStarted += Time.deltaTime;

            if(timeSinceStarted <= 1.0f)
            {
                map_panel.anchoredPosition = Vector3.Lerp(map_panel.anchoredPosition, newPosition, timeSinceStarted);
            }

            if (micro_camera.orthographicSize > 80.0f)
            {
                micro_camera.orthographicSize = micro_camera.orthographicSize - 110.0f;
            }
            

            // If the object has arrived, stop the coroutine
            if (timeSinceStarted > 1.0f && micro_camera.orthographicSize == 80.0f)
            {
                yield break;
            }

            // Otherwise, continue next frame
            yield return null;
        }
    }

    IEnumerator MoveNextLevel(Vector3 newPosition, bool direction)
    {
        float timeSinceStarted = 0f;

        if (direction)
        {
            newPosition = new Vector3((-newPosition.x - x_frame_border + 150.0f), (-newPosition.y));
        }
        else
        {
            newPosition = new Vector3((-newPosition.x - x_frame_border - 150.0f), (-newPosition.y));
        }

        while (true)
        {
            timeSinceStarted += Time.deltaTime;

            if (timeSinceStarted <= 1.0f)
            {
                map_panel.anchoredPosition = Vector3.Lerp(map_panel.anchoredPosition, newPosition, timeSinceStarted);
            }

            // If the object has arrived, stop the coroutine
            if (timeSinceStarted > 1.0f)
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

    //navigation right
    public void NextLevelRight()
    {
        StopAllCoroutines();
        Debug.Log("current level: " + current_level);
        current_level++;
        StartCoroutine(MoveNextLevel(levelMapController.level_position[current_level], true));
        Debug.Log("next level: " + current_level);
    }

    //navigation right
    public void NextLevelLeftt()
    {
        StopAllCoroutines();
        Debug.Log("current level: " + current_level);
        current_level--;
        StartCoroutine(MoveNextLevel(levelMapController.level_position[current_level], false));
        Debug.Log("next level: " + current_level);
    }

}
