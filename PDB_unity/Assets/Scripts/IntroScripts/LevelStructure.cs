using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelStructure : MonoBehaviour, IPointerClickHandler
{

    public string level_name;
    Transform map_canvas;
    LevelMapController levelMapController;
    //thumbs
    public Sprite thumb_32;
    public Sprite nrnalo;
    public Sprite thumb_128;
    public Sprite thumb_512;
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

    void Start()
    {
        map_canvas = gameObject.transform.parent.GetComponent<Transform>();
        //map_canvas = GameObject.Find("MapPanel").GetComponent<Transform>();
        //set the frame cmaera focus
        x_frame_min = Screen.width / 4.0f;
        y_frame_min = Screen.height / 4.0f;
        x_frame_max = Screen.width - x_frame_min;
        y_frame_max = Screen.height - y_frame_min;

    }

    public void OnPointerClick(PointerEventData data)
    {
        levelMapController = FindObjectOfType<LevelMapController>();
       // levelMapController.CreateLevelDescription(level_name);
    }

    void Update()
    {
        //if the thumb is inside the frame focus
        if (transform.position.x > x_frame_min && transform.position.x < x_frame_max && transform.position.y > y_frame_min && transform.position.y < y_frame_max)
        {
            if (map_canvas.localScale.x > 0.5f && map_canvas.localScale.x < 1.0f)
            {
                //if the thumb is null, download ONCE
                if(!download_128)
                {
                    download_128 = true;
                    StartCoroutine(DownloadThumb("128"));
                }
                //ask if the download is ready
                //if the thumb has been already dwnoaded, then loaded directly and dont enter here
                if (DataConnection.isDone && !thumb_128)
                {
                    thumb_temp = Sprite.Create(DataConnection.texture, new Rect(0, 0, 128, 128), thumb_pivot);
                    GetComponentInChildren<Image>().overrideSprite = thumb_temp;
                    thumb_128 = thumb_temp;
                }
                else
                {
                    GetComponentInChildren<Image>().overrideSprite = thumb_128;
                }
            }
            else if(map_canvas.localScale.x > 1.0f)
            {
                //if the thumb is null, download ONCE
                if (!download_512)
                {
                    download_512 = true;
                    StartCoroutine(DownloadThumb("512"));
                }
                //ask if the download is ready
                //if the thumb has been already dwnoaded, then loaded directly and dont enter here
                if (DataConnection.isDone && !thumb_512)
                {
                    thumb_temp = Sprite.Create(DataConnection.texture, new Rect(0, 0, 512, 512), thumb_pivot);
                    GetComponentInChildren<Image>().overrideSprite = thumb_temp;
                    thumb_512 = thumb_temp;
                }
                else
                {
                    GetComponentInChildren<Image>().overrideSprite = thumb_512;
                }
            }
        }
        //if not, set the low resolution thimb
        else
        {
           GetComponentInChildren<Image>().overrideSprite = thumb_32;
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
