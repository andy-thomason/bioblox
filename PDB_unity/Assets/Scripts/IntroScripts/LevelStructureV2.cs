using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelStructureV2 : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{

    public string level_name;
    public Vector3 coordinates;
    public int level_id;
    //Transform map_canvas;
    LevelMapController levelMapController;
    public MapController mapController;
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
    Text text_level;
    Vector2 thumb_pivot = new Vector2(0.5f, 0.5f);
    WWW DataConnection;

    bool download_128 = false;
    bool download_512 = false;
    //on focus
    bool is_on_screen = false;
    //the sprite to replace
    Image thumb_image;

    //camera micro
    public Camera micro_camera;

    float lastClickTime;

    float catchTime = 0.25f;
    int number_clicks = 0;
    float time_double_clicks = 0;

    void Awake()
    {
        //micro_camera = GameObject.Find("MiscroscopeCamera").GetComponent<Camera>();
        //mapController = FindObjectOfType<MapController>();
        text_level = GetComponentInChildren<Text>();
        thumb_image = GetComponent<Image>();
    }

    void Start()
    {
        // map_canvas = gameObject.transform.parent.GetComponent<Transform>();
        //map_canvas = GameObject.Find("MapPanel").GetComponent<Transform>();
        //set the frame cmaera focus
        x_frame_min = Screen.width / 8.0f;
        y_frame_min = Screen.height / 8.0f;
        x_frame_max = Screen.width - x_frame_min;
        y_frame_max = Screen.height - y_frame_min;
        //Debug.Log(Screen.width / 2.0f + "/" + Screen.height / 2.0f);

    }

    public void init(string name_level, Vector3 position)
    {
        level_name = name_level;
        text_level.text = level_name;
        coordinates = position;
        //GetComponent<my_mesh>().init(level_name);
        StartCoroutine(DownloadThumbInit());
    }

    public void OnPointerClick(PointerEventData data)
    {
        //levelMapController = FindObjectOfType<LevelMapController>();
        //map_canvas.position = Vector3.zero;
        //map_canvas.position = new Vector3(coordinates.x, -coordinates.y, coordinates.z);
        //Vector2 testaa = new Vector2((-coordinates.x + micro_camera.WorldToScreenPoint(transform.position).x), (-coordinates.y - micro_camera.WorldToScreenPoint(transform.position).y));
        //transform.parent.GetComponent<RectTransform>().anchoredPosition = testaa;
        //Debug.Log(testaa.x + "/"+testaa.y);

        // map_canvas.Translate(new Vector3(coordinates.x, -coordinates.y, coordinates.z));
        // levelMapController.CreateLevelDescription(level_name);
        //mapController.OnClickProtein();
        if (number_clicks == 0)
        {
            time_double_clicks = Time.time;
        }
        number_clicks++;
    }


    public void OnPointerEnter(PointerEventData data)
    {
        mapController.zoom_position = coordinates;

    }

    void Update()
    {

        if (number_clicks == 2)
        {
            if (Time.time - time_double_clicks < 0.5f)
            {
                mapController.DoubleClickLevel();
            }
            number_clicks = 0;
        }
        //Debug.Log(micro_camera.WorldToScreenPoint(transform.position));
        //if the thumb is inside the frame focus
        if (micro_camera.WorldToScreenPoint(transform.position).x > x_frame_min && micro_camera.WorldToScreenPoint(transform.position).x < x_frame_max && micro_camera.WorldToScreenPoint(transform.position).y > y_frame_min && micro_camera.WorldToScreenPoint(transform.position).y < y_frame_max && micro_camera.orthographicSize <= 500)
        {
            //if (map_canvas.localScale.x > 0.5f && map_canvas.localScale.x < 1.0f && current_thumb.texture.width != 128)
            if (micro_camera.orthographicSize > 100 && micro_camera.orthographicSize <= 500 && current_thumb.texture.width != 128)
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
                else if (thumb_128)
                {
                    thumb_image.overrideSprite = thumb_128;
                    current_thumb = thumb_128;
                }
            }
            //else if(map_canvas.localScale.x >= 1.0f && current_thumb.texture.width != 512)
            else if (micro_camera.orthographicSize <= 100 && current_thumb.texture.width != 512)
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
    }
    
    //couritine download thimb
    IEnumerator DownloadThumb(string thumb_size)
    {
        url_image = "http://quiley.com/BB/BB" + thumb_size + "/" + level_name + ".png";
        //url_image = "http://158.223.59.221:8080/thumbnails/" + level_name + "." + thumb_size + ".png";
        do//wait for the server to get the image
        {
            DataConnection = new WWW(url_image);
            yield return DataConnection;
        } while (!string.IsNullOrEmpty(DataConnection.error));

    }

    IEnumerator DownloadThumbInit()
    {
        url_image = "http://quiley.com/BB/BB32/" + level_name + ".png";
        //url_image = "http://158.223.59.221:8080/thumbnails/" + level_name + ".32.png";
        do//wait for the server to get the image
        {
            DataConnection = new WWW(url_image);
            yield return DataConnection;
        } while (!string.IsNullOrEmpty(DataConnection.error));
        thumb_32 = Sprite.Create(DataConnection.texture, new Rect(0, 0, 32, 32), thumb_pivot);
        thumb_image.sprite = current_thumb = thumb_32;

    }

}
