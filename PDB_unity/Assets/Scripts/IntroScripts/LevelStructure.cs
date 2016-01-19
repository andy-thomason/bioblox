using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelStructure : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{

    public string level_name;
    public Vector3 coordinates;
    Transform map_canvas;
    LevelMapController levelMapController;
    //thumbs
    public Sprite thumb_32;
    public Sprite nrnalo;
    public Sprite thumb_128;
    public Sprite thumb_512;
    public Sprite current_thumb;
    public Sprite white_loading;
    //frame
    float x_frame_min;
    float y_frame_min;
    float x_frame_max;
    float y_frame_max;
    //WWWW
    string url_image;
    Sprite thumb_temp;
    Vector2 thumb_pivot = new Vector2(0.5f, 0.5f);
    WWW DataConnection;

    bool download_128 = false;
    bool download_512 = false;
    //on focus
    bool is_on_screen = false;
    //the sprite to replace
    Image thumb_image;

    void Start()
    {
        map_canvas = gameObject.transform.parent.GetComponent<Transform>();
        //map_canvas = GameObject.Find("MapPanel").GetComponent<Transform>();
        //set the frame cmaera focus
        x_frame_min = Screen.width / 8.0f;
        y_frame_min = Screen.height / 8.0f;
        x_frame_max = Screen.width - x_frame_min;
        y_frame_max = Screen.height - y_frame_min;
        thumb_image = GetComponentInChildren<Image>();

    }

    public void OnPointerClick(PointerEventData data)
    {
        levelMapController = FindObjectOfType<LevelMapController>();
        //map_canvas.position = Vector3.zero;
        //map_canvas.position = new Vector3(coordinates.x, -coordinates.y, coordinates.z);
        // Vector2 testaa = new Vector2((-coordinates.x + 400.0f) * map_canvas.localScale.x, (-coordinates.y - 240.0f) * map_canvas.localScale.x);
        // transform.parent.GetComponent<RectTransform>().anchoredPosition = testaa;
        // Debug.Log(testaa.x + "/"+testaa.y);

        // map_canvas.Translate(new Vector3(coordinates.x, -coordinates.y, coordinates.z));
        // levelMapController.CreateLevelDescription(level_name);
        Vector2 viewportPoint = GameObject.Find("MiscroscopeCamera").GetComponent<Camera>().WorldToViewportPoint(coordinates);
        map_canvas.GetComponent<RectTransform>().anchorMax = viewportPoint;
        map_canvas.GetComponent<RectTransform>().anchorMin = viewportPoint;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Debug.Log(coordinates);
    }

    void Update()
    {
        //if the thumb is inside the frame focus
        if (transform.position.x > x_frame_min && transform.position.x < x_frame_max && transform.position.y > y_frame_min && transform.position.y < y_frame_max)
        {
            if (map_canvas.localScale.x > 0.5f && map_canvas.localScale.x < 1.0f && current_thumb.texture.width != 128)
            {
                //if the thumb is null, download ONCE
                if (!download_128)
                {
                    //thumb_image.overrideSprite = white_loading;
                    download_128 = true;
                    StartCoroutine(DownloadThumb("128"));
                }
                //ask if the download is ready
                //if the thumb has been already dwnoaded, then loaded directly and dont enter here
                else if (DataConnection.isDone && !thumb_128)
                {
                    thumb_temp = Sprite.Create(DataConnection.texture, new Rect(0, 0, 128, 128), thumb_pivot);
                    thumb_128 = thumb_temp;
                }
                //replace the downloaded thumb and change the value of the current thumb
                else if(thumb_128)
                {
                    thumb_image.overrideSprite = thumb_128;
                    current_thumb = thumb_128;
                }
            }
            else if(map_canvas.localScale.x >= 1.0f && current_thumb.texture.width != 512)
            {
                //if the thumb is null, download ONCE
                if (!download_512)
                {
                    download_512 = true;
                    StartCoroutine(DownloadThumb("512"));
                }
                //ask if the download is ready
                //if the thumb has been already dwnoaded, then loaded directly and dont enter here
                else if (DataConnection.isDone && !thumb_512)
                {
                    thumb_temp = Sprite.Create(DataConnection.texture, new Rect(0, 0, 512, 512), thumb_pivot);
                    thumb_512 = thumb_temp;
                }
                //replace the downloaded thumb and change the value of the current thumb
                else if (thumb_512)
                {
                    thumb_image.overrideSprite = thumb_512;
                    current_thumb = thumb_512;
                }
            }
        }
        //if not, set the low resolution thimb only once
        else if(current_thumb.texture.width != 32)
        {
           GetComponentInChildren<Image>().overrideSprite = thumb_32;
           current_thumb = thumb_32;
        }
    }

    //couritine download thimb
    IEnumerator DownloadThumb(string thumb_size)
    {
        url_image = "http://quiley.com/BB/BB"+ thumb_size +"/"+ level_name + ".png";
        // string url_image = "http://158.223.59.221:8080/" + level_name + ".png";
        DataConnection = new WWW(url_image);
        yield return DataConnection;
    }

}
