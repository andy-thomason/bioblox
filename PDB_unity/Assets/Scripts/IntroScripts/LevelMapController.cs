using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelMapController : MonoBehaviour {

    public GameObject LevelMap;
    public GameObject LevelPrefab;
    GameObject LevelPrefabReference;
    //posicion inicial
    Vector3 initial_position = new Vector3(2000.0f, -1200.0f, 0.0f);
    //variables to tansform 2d texture to sprite
    Sprite thumb_sprite;
    Rect thumb_rect_SD = new Rect(0, 0, 32, 32);
    Vector2 thumb_pivot = new Vector2(0.5f, 0.5f);
    Rect thumb_rect_HD = new Rect(0, 0, 1024, 1024);
    //canvas of levelselection and leveldescriptin
    public GameObject LevelSelectionCanvas;
    public GameObject LevelDescriptionCanvas;

    DataController dataController;
    string url_image;

    // Use this for initialization
    void Start () {
        dataController = FindObjectOfType<DataController>();
    }

    public void GenerateLevels()
    {
        foreach (string level_name in dataController.level_index)
        {
            if (level_name != "")
            {
                LevelPrefabReference = Instantiate<GameObject>(LevelPrefab);
                LevelPrefabReference.transform.SetParent(LevelMap.transform, false);
                LevelPrefabReference.GetComponent<RectTransform>().anchoredPosition = initial_position;
                //put the name
                LevelPrefabReference.GetComponentInChildren<Text>().text = level_name;
                LevelPrefabReference.GetComponent<LevelStructure>().level_name = level_name;
                //set the coordinates of it
                LevelPrefabReference.GetComponent<LevelStructure>().coordinates = initial_position;
                //image 
                StartCoroutine(DownloadThumbLD(level_name, LevelPrefabReference));
                //move next position
                initial_position += new Vector3(128.0f, 0.0f, 0.0f);

                if (initial_position.x > 8000.0f)
                {
                    initial_position += new Vector3(-6016.0f, -128.0f, 0.0f);
                }
            }
        }

    }

    IEnumerator DownloadThumbLD(string level_name, GameObject current_level)
    {
        url_image = "http://quiley.com/BB/BB32/" + level_name + ".png";
       // string url_image = "http://158.223.59.221:8080/" + level_name + ".png";
        //Debug.Log(url_image);
        WWW DataConnection = new WWW(url_image);
        yield return DataConnection;
        thumb_sprite = Sprite.Create(DataConnection.texture, thumb_rect_SD, thumb_pivot);
        current_level.GetComponent<LevelStructure>().thumb_32 = thumb_sprite;
        current_level.GetComponentInChildren<Image>().sprite = thumb_sprite;
    }

    public void CreateLevelDescription(string level_name)
    {
        LevelSelectionCanvas.SetActive(false);
        LevelDescriptionCanvas.transform.GetChild(1).GetComponent<Text>().text = level_name;
        //image 
        StartCoroutine(DownloadThumbHD(level_name, LevelDescriptionCanvas.transform.GetChild(0).gameObject));
        LevelDescriptionCanvas.SetActive(true);
    }

    public void DescriptionToMap()
    {
        LevelDescriptionCanvas.SetActive(false);
        LevelSelectionCanvas.SetActive(true);
    }
    
    IEnumerator DownloadThumbHD(string level_name, GameObject level_image)
    {
        url_image = "http://quiley.com/BB/BBHD/" + level_name + ".png";
        // string url_image = "http://158.223.59.221:8080/" + level_name + ".png";
        //Debug.Log(url_image);
        WWW DataConnection = new WWW(url_image);
        yield return DataConnection;
        thumb_sprite = Sprite.Create(DataConnection.texture, thumb_rect_HD, thumb_pivot);
        level_image.GetComponentInChildren<Image>().sprite = thumb_sprite;
    }
}
