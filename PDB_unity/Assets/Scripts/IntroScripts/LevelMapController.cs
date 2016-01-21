using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelMapController : MonoBehaviour
{

    public GameObject LevelMap;
    public GameObject LevelPrefab;
    GameObject LevelPrefabReference;
    //posicion inicial
    //Vector3 initial_position = new Vector3(2000.0f, -1200.0f, 0.0f);

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
    int level_index = 0;
    WWW DataConnection;

    // Use this for initialization
    void Start()
    {
        dataController = FindObjectOfType<DataController>();
    }

    public void GenerateLevels()
    {
        /*foreach (string level_name in dataController.level_index)
        {
            if (level_name != "")
            {
                LevelPrefabReference = Instantiate<GameObject>(LevelPrefab);
                LevelPrefabReference.transform.SetParent(LevelMap.transform, false);
                LevelPrefabReference.GetComponent<RectTransform>().anchoredPosition = level_position[level_index];
                //put the name
                LevelPrefabReference.GetComponentInChildren<Text>().text = level_name;
                LevelPrefabReference.GetComponent<LevelStructure>().level_name = level_name;
                LevelPrefabReference.GetComponent<LevelStructure>().level_id = level_index;
                //set the coordinates of it
                LevelPrefabReference.GetComponent<LevelStructure>().coordinates = level_position[level_index];
                //image 
                StartCoroutine(DownloadThumb8(level_name, LevelPrefabReference));
                //move next position
                // initial_position += new Vector3(128.0f, 0.0f, 0.0f);

                // if (initial_position.x > 8000.0f)
                // {
                // initial_position += new Vector3(-6016.0f, -128.0f, 0.0f);
                //}
                level_index++;
            }
        }*/
        StartCoroutine(CreateLevelsCoru());

    }

    IEnumerator DownloadThumb8(string level_name, GameObject current_level)
    {
        url_image = "http://quiley.com/BB/BB32/" + level_name + ".png";
        //url_image = "http://158.223.59.221:8080/thumbnails/" + level_name + ".8.png";
        WWW DataConnection;
        do
        {
            //wait for the server to get the image
            DataConnection = new WWW(url_image);
            yield return DataConnection;
        } while (!string.IsNullOrEmpty(DataConnection.error));
        thumb_sprite = Sprite.Create(DataConnection.texture, thumb_rect_SD, thumb_pivot);
        current_level.GetComponent<LevelStructure>().thumb_32 = thumb_sprite;
        current_level.GetComponentInChildren<Image>().sprite = thumb_sprite;
    }

    IEnumerator CreateLevelsCoru()
    {
        foreach (string level_name in dataController.level_index)
        {
            if (level_name != "")
            {
                LevelPrefabReference = Instantiate<GameObject>(LevelPrefab);
                LevelPrefabReference.transform.SetParent(transform, false);
                LevelPrefabReference.GetComponent<RectTransform>().anchoredPosition = level_position[level_index];
                //put the name
                LevelPrefabReference.GetComponentInChildren<Text>().text = level_name;
                LevelPrefabReference.GetComponent<LevelStructure>().level_name = level_name;
                LevelPrefabReference.GetComponent<LevelStructure>().level_id = level_index;
                //set the coordinates of it
                LevelPrefabReference.GetComponent<LevelStructure>().coordinates = level_position[level_index];
                //image 
                url_image = "http://quiley.com/BB/BB32/" + level_name + ".png";
                //url_image = "http://158.223.59.221:8080/thumbnails/" + level_name + ".8.png";
                do
                {//wait for the server to get the image
                    DataConnection = new WWW(url_image);
                    yield return DataConnection;
                } while (!string.IsNullOrEmpty(DataConnection.error));
                thumb_sprite = Sprite.Create(DataConnection.texture, thumb_rect_SD, thumb_pivot);
                LevelPrefabReference.GetComponent<LevelStructure>().thumb_32 = thumb_sprite;
                LevelPrefabReference.GetComponentInChildren<Image>().sprite = thumb_sprite;

                level_index++;
            }
        }
    }

    public void DescriptionToMap()
    {
        LevelDescriptionCanvas.SetActive(false);
        LevelSelectionCanvas.SetActive(true);
    }

    Vector3[] level_position = new []{new Vector3(2000,-1200,0), new Vector3(2128,-1200,0), new Vector3(2256,-1200,0), new Vector3(2384,-1200,0), new 
Vector3(2512,-1200,0), new Vector3(2640,-1200,0), new Vector3(2768,-1200,0), new Vector3(2896,-1200,0), new Vector3(3024,-1200,0), new Vector3(3152,-1200,0), new 
Vector3(3280,-1200,0), new Vector3(3408,-1200,0), new Vector3(3536,-1200,0), new Vector3(3664,-1200,0), new Vector3(3792,-1200,0), new Vector3(3920,-1200,0), new 
Vector3(4048,-1200,0), new Vector3(4176,-1200,0), new Vector3(4304,-1200,0), new Vector3(4432,-1200,0), new Vector3(4560,-1200,0), new Vector3(4688,-1200,0), new 
Vector3(4816,-1200,0), new Vector3(4944,-1200,0), new Vector3(5072,-1200,0), new Vector3(5200,-1200,0), new Vector3(5328,-1200,0), new Vector3(5456,-1200,0), new 
Vector3(5584,-1200,0), new Vector3(5712,-1200,0), new Vector3(5840,-1200,0), new Vector3(5968,-1200,0), new Vector3(6096,-1200,0), new Vector3(6224,-1200,0), new 
Vector3(6352,-1200,0), new Vector3(6480,-1200,0), new Vector3(6608,-1200,0), new Vector3(6736,-1200,0), new Vector3(6864,-1200,0), new Vector3(6992,-1200,0), new 
Vector3(7120,-1200,0), new Vector3(7248,-1200,0), new Vector3(7376,-1200,0), new Vector3(7504,-1200,0), new Vector3(7632,-1200,0), new Vector3(7760,-1200,0), new 
Vector3(7888,-1200,0), new Vector3(8016,-1200,0), new Vector3(8144,-1200,0), new Vector3(8272,-1200,0), new Vector3(8400,-1200,0), new Vector3(8528,-1200,0), new 
Vector3(8656,-1200,0), new Vector3(8784,-1200,0), new Vector3(8912,-1200,0), new Vector3(9040,-1200,0), new Vector3(9168,-1200,0), new Vector3(9296,-1200,0), new 
Vector3(9424,-1200,0), new Vector3(9552,-1200,0), new Vector3(9680,-1200,0), new Vector3(9808,-1200,0), new Vector3(9936,-1200,0), new Vector3(10064,-1200,0), 
new Vector3(10192,-1200,0), new Vector3(10320,-1200,0), new Vector3(10448,-1200,0), new Vector3(10576,-1200,0), new Vector3(10704,-1200,0), new 
Vector3(10832,-1200,0), new Vector3(10960,-1200,0), new Vector3(11088,-1200,0), new Vector3(11216,-1200,0), new Vector3(11344,-1200,0), new 
Vector3(11472,-1200,0), new Vector3(11600,-1200,0), new Vector3(11728,-1200,0), new Vector3(11856,-1200,0), new Vector3(11984,-1200,0), new 
Vector3(2000,-1328,0), new Vector3(2128,-1328,0), new Vector3(2256,-1328,0), new Vector3(2384,-1328,0), new Vector3(2512,-1328,0), new Vector3(2640,-1328,0), new 
Vector3(2768,-1328,0), new Vector3(2896,-1328,0), new Vector3(3024,-1328,0), new Vector3(3152,-1328,0), new Vector3(3280,-1328,0), new Vector3(3408,-1328,0), new 
Vector3(3536,-1328,0), new Vector3(3664,-1328,0), new Vector3(3792,-1328,0), new Vector3(3920,-1328,0), new Vector3(4048,-1328,0), new Vector3(4176,-1328,0), new 
Vector3(4304,-1328,0), new Vector3(4432,-1328,0), new Vector3(4560,-1328,0), new Vector3(4688,-1328,0), new Vector3(4816,-1328,0), new Vector3(4944,-1328,0), new 
Vector3(5072,-1328,0), new Vector3(5200,-1328,0), new Vector3(5328,-1328,0), new Vector3(5456,-1328,0), new Vector3(5584,-1328,0), new Vector3(5712,-1328,0), new 
Vector3(5840,-1328,0), new Vector3(5968,-1328,0), new Vector3(6096,-1328,0), new Vector3(6224,-1328,0), new Vector3(6352,-1328,0), new Vector3(6480,-1328,0), new 
Vector3(6608,-1328,0), new Vector3(6736,-1328,0), new Vector3(6864,-1328,0), new Vector3(6992,-1328,0), new Vector3(7120,-1328,0), new Vector3(7248,-1328,0), new 
Vector3(7376,-1328,0), new Vector3(7504,-1328,0), new Vector3(7632,-1328,0), new Vector3(7760,-1328,0), new Vector3(7888,-1328,0), new Vector3(8016,-1328,0), new 
Vector3(8144,-1328,0), new Vector3(8272,-1328,0), new Vector3(8400,-1328,0), new Vector3(8528,-1328,0), new Vector3(8656,-1328,0), new Vector3(8784,-1328,0), new 
Vector3(8912,-1328,0), new Vector3(9040,-1328,0), new Vector3(9168,-1328,0), new Vector3(9296,-1328,0), new Vector3(9424,-1328,0), new Vector3(9552,-1328,0), new 
Vector3(9680,-1328,0), new Vector3(9808,-1328,0), new Vector3(9936,-1328,0), new Vector3(10064,-1328,0), new Vector3(10192,-1328,0), new Vector3(10320,-1328,0), 
new Vector3(10448,-1328,0), new Vector3(10576,-1328,0), new Vector3(10704,-1328,0), new Vector3(10832,-1328,0), new Vector3(10960,-1328,0), new 
Vector3(11088,-1328,0), new Vector3(11216,-1328,0), new Vector3(11344,-1328,0), new Vector3(11472,-1328,0), new Vector3(11600,-1328,0), new 
Vector3(11728,-1328,0), new Vector3(11856,-1328,0), new Vector3(11984,-1328,0), new Vector3(2000,-1456,0), new Vector3(2128,-1456,0), new Vector3(2256,-1456,0), 
new Vector3(2384,-1456,0), new Vector3(2512,-1456,0), new Vector3(2640,-1456,0), new Vector3(2768,-1456,0), new Vector3(2896,-1456,0), new Vector3(3024,-1456,0), 
new Vector3(3152,-1456,0), new Vector3(3280,-1456,0), new Vector3(3408,-1456,0), new Vector3(3536,-1456,0), new Vector3(3664,-1456,0), new Vector3(3792,-1456,0), 
new Vector3(3920,-1456,0), new Vector3(4048,-1456,0), new Vector3(4176,-1456,0), new Vector3(4304,-1456,0), new Vector3(4432,-1456,0), new Vector3(4560,-1456,0), 
new Vector3(4688,-1456,0), new Vector3(4816,-1456,0), new Vector3(4944,-1456,0), new Vector3(5072,-1456,0), new Vector3(5200,-1456,0), new Vector3(5328,-1456,0), 
new Vector3(5456,-1456,0), new Vector3(5584,-1456,0), new Vector3(5712,-1456,0), new Vector3(5840,-1456,0), new Vector3(5968,-1456,0), new Vector3(6096,-1456,0), 
new Vector3(6224,-1456,0), new Vector3(6352,-1456,0), new Vector3(6480,-1456,0), new Vector3(6608,-1456,0), new Vector3(6736,-1456,0), new Vector3(6864,-1456,0), 
new Vector3(6992,-1456,0), new Vector3(7120,-1456,0), new Vector3(7248,-1456,0), new Vector3(7376,-1456,0), new Vector3(7504,-1456,0), new Vector3(7632,-1456,0), 
new Vector3(7760,-1456,0), new Vector3(7888,-1456,0), new Vector3(8016,-1456,0), new Vector3(8144,-1456,0), new Vector3(8272,-1456,0), new Vector3(8400,-1456,0), 
new Vector3(8528,-1456,0), new Vector3(8656,-1456,0), new Vector3(8784,-1456,0), new Vector3(8912,-1456,0), new Vector3(9040,-1456,0), new Vector3(9168,-1456,0), 
new Vector3(9296,-1456,0), new Vector3(9424,-1456,0), new Vector3(9552,-1456,0), new Vector3(9680,-1456,0), new Vector3(9808,-1456,0), new Vector3(9936,-1456,0), 
new Vector3(10064,-1456,0), new Vector3(10192,-1456,0), new Vector3(10320,-1456,0), new Vector3(10448,-1456,0), new Vector3(10576,-1456,0), new 
Vector3(10704,-1456,0), new Vector3(10832,-1456,0), new Vector3(10960,-1456,0), new Vector3(11088,-1456,0), new Vector3(11216,-1456,0), new 
Vector3(11344,-1456,0), new Vector3(11472,-1456,0), new Vector3(11600,-1456,0), new Vector3(11728,-1456,0), new Vector3(11856,-1456,0), new 
Vector3(11984,-1456,0), new Vector3(2000,-1584,0), new Vector3(2128,-1584,0), new Vector3(2256,-1584,0), new Vector3(2384,-1584,0), new Vector3(2512,-1584,0), 
new Vector3(2640,-1584,0), new Vector3(2768,-1584,0), new Vector3(2896,-1584,0), new Vector3(3024,-1584,0), new Vector3(3152,-1584,0), new Vector3(3280,-1584,0), 
new Vector3(3408,-1584,0), new Vector3(3536,-1584,0), new Vector3(3664,-1584,0), new Vector3(3792,-1584,0), new Vector3(3920,-1584,0), new Vector3(4048,-1584,0), 
new Vector3(4176,-1584,0), new Vector3(4304,-1584,0), new Vector3(4432,-1584,0), new Vector3(4560,-1584,0), new Vector3(4688,-1584,0), new Vector3(4816,-1584,0), 
new Vector3(4944,-1584,0), new Vector3(5072,-1584,0), new Vector3(5200,-1584,0), new Vector3(5328,-1584,0), new Vector3(5456,-1584,0), new Vector3(5584,-1584,0), 
new Vector3(5712,-1584,0), new Vector3(5840,-1584,0), new Vector3(5968,-1584,0), new Vector3(6096,-1584,0), new Vector3(6224,-1584,0), new Vector3(6352,-1584,0), 
new Vector3(6480,-1584,0), new Vector3(6608,-1584,0), new Vector3(6736,-1584,0), new Vector3(6864,-1584,0), new Vector3(6992,-1584,0), new Vector3(7120,-1584,0), 
new Vector3(7248,-1584,0), new Vector3(7376,-1584,0), new Vector3(7504,-1584,0), new Vector3(7632,-1584,0), new Vector3(7760,-1584,0), new Vector3(7888,-1584,0), 
new Vector3(8016,-1584,0), new Vector3(8144,-1584,0), new Vector3(8272,-1584,0), new Vector3(8400,-1584,0), new Vector3(8528,-1584,0), new Vector3(8656,-1584,0), 
new Vector3(8784,-1584,0), new Vector3(8912,-1584,0), new Vector3(9040,-1584,0), new Vector3(9168,-1584,0), new Vector3(9296,-1584,0), new Vector3(9424,-1584,0), 
new Vector3(9552,-1584,0), new Vector3(9680,-1584,0), new Vector3(9808,-1584,0), new Vector3(9936,-1584,0), new Vector3(10064,-1584,0), new 
Vector3(10192,-1584,0), new Vector3(10320,-1584,0), new Vector3(10448,-1584,0), new Vector3(10576,-1584,0), new Vector3(10704,-1584,0), new 
Vector3(10832,-1584,0), new Vector3(10960,-1584,0), new Vector3(11088,-1584,0), new Vector3(11216,-1584,0), new Vector3(11344,-1584,0), new 
Vector3(11472,-1584,0), new Vector3(11600,-1584,0), new Vector3(11728,-1584,0), new Vector3(11856,-1584,0), new Vector3(11984,-1584,0), new 
Vector3(2000,-1712,0), new Vector3(2128,-1712,0), new Vector3(2256,-1712,0), new Vector3(2384,-1712,0), new Vector3(2512,-1712,0), new Vector3(2640,-1712,0), new 
Vector3(2768,-1712,0), new Vector3(2896,-1712,0), new Vector3(3024,-1712,0), new Vector3(3152,-1712,0), new Vector3(3280,-1712,0), new Vector3(3408,-1712,0), new 
Vector3(3536,-1712,0), new Vector3(3664,-1712,0), new Vector3(3792,-1712,0), new Vector3(3920,-1712,0), new Vector3(4048,-1712,0), new Vector3(4176,-1712,0), new 
Vector3(4304,-1712,0), new Vector3(4432,-1712,0), new Vector3(4560,-1712,0), new Vector3(4688,-1712,0), new Vector3(4816,-1712,0), new Vector3(4944,-1712,0), new 
Vector3(5072,-1712,0), new Vector3(5200,-1712,0), new Vector3(5328,-1712,0), new Vector3(5456,-1712,0), new Vector3(5584,-1712,0), new Vector3(5712,-1712,0), new 
Vector3(5840,-1712,0), new Vector3(5968,-1712,0), new Vector3(6096,-1712,0), new Vector3(6224,-1712,0), new Vector3(6352,-1712,0), new Vector3(6480,-1712,0), new 
Vector3(6608,-1712,0), new Vector3(6736,-1712,0), new Vector3(6864,-1712,0), new Vector3(6992,-1712,0), new Vector3(7120,-1712,0), new Vector3(7248,-1712,0), new 
Vector3(7376,-1712,0), new Vector3(7504,-1712,0), new Vector3(7632,-1712,0), new Vector3(7760,-1712,0), new Vector3(7888,-1712,0), new Vector3(8016,-1712,0), new 
Vector3(8144,-1712,0), new Vector3(8272,-1712,0), new Vector3(8400,-1712,0), new Vector3(8528,-1712,0), new Vector3(8656,-1712,0), new Vector3(8784,-1712,0), new 
Vector3(8912,-1712,0), new Vector3(9040,-1712,0), new Vector3(9168,-1712,0), new Vector3(9296,-1712,0), new Vector3(9424,-1712,0), new Vector3(9552,-1712,0), new 
Vector3(9680,-1712,0), new Vector3(9808,-1712,0), new Vector3(9936,-1712,0), new Vector3(10064,-1712,0), new Vector3(10192,-1712,0), new Vector3(10320,-1712,0), 
new Vector3(10448,-1712,0), new Vector3(10576,-1712,0), new Vector3(10704,-1712,0), new Vector3(10832,-1712,0), new Vector3(10960,-1712,0), new 
Vector3(11088,-1712,0), new Vector3(11216,-1712,0), new Vector3(11344,-1712,0), new Vector3(11472,-1712,0), new Vector3(11600,-1712,0), new 
Vector3(11728,-1712,0), new Vector3(11856,-1712,0), new Vector3(11984,-1712,0), new Vector3(2000,-1840,0), new Vector3(2128,-1840,0), new Vector3(2256,-1840,0), 
new Vector3(2384,-1840,0), new Vector3(2512,-1840,0), new Vector3(2640,-1840,0), new Vector3(2768,-1840,0), new Vector3(2896,-1840,0), new Vector3(3024,-1840,0), 
new Vector3(3152,-1840,0), new Vector3(3280,-1840,0), new Vector3(3408,-1840,0), new Vector3(3536,-1840,0), new Vector3(3664,-1840,0), new Vector3(3792,-1840,0), 
new Vector3(3920,-1840,0), new Vector3(4048,-1840,0), new Vector3(4176,-1840,0), new Vector3(4304,-1840,0), new Vector3(4432,-1840,0), new Vector3(4560,-1840,0), 
new Vector3(4688,-1840,0), new Vector3(4816,-1840,0), new Vector3(4944,-1840,0), new Vector3(5072,-1840,0), new Vector3(5200,-1840,0), new Vector3(5328,-1840,0), 
new Vector3(5456,-1840,0), new Vector3(5584,-1840,0), new Vector3(5712,-1840,0), new Vector3(5840,-1840,0), new Vector3(5968,-1840,0), new Vector3(6096,-1840,0), 
new Vector3(6224,-1840,0), new Vector3(6352,-1840,0), new Vector3(6480,-1840,0), new Vector3(6608,-1840,0), new Vector3(6736,-1840,0), new Vector3(6864,-1840,0), 
new Vector3(6992,-1840,0), new Vector3(7120,-1840,0), new Vector3(7248,-1840,0), new Vector3(7376,-1840,0), new Vector3(7504,-1840,0), new Vector3(7632,-1840,0), 
new Vector3(7760,-1840,0), new Vector3(7888,-1840,0), new Vector3(8016,-1840,0), new Vector3(8144,-1840,0), new Vector3(8272,-1840,0), new Vector3(8400,-1840,0), 
new Vector3(8528,-1840,0), new Vector3(8656,-1840,0), new Vector3(8784,-1840,0), new Vector3(8912,-1840,0), new Vector3(9040,-1840,0), new Vector3(9168,-1840,0), 
new Vector3(9296,-1840,0), new Vector3(9424,-1840,0), new Vector3(9552,-1840,0), new Vector3(9680,-1840,0), new Vector3(9808,-1840,0), new Vector3(9936,-1840,0), 
new Vector3(10064,-1840,0), new Vector3(10192,-1840,0), new Vector3(10320,-1840,0), new Vector3(10448,-1840,0), new Vector3(10576,-1840,0), new 
Vector3(10704,-1840,0), new Vector3(10832,-1840,0), new Vector3(10960,-1840,0), new Vector3(11088,-1840,0), new Vector3(11216,-1840,0), new 
Vector3(11344,-1840,0), new Vector3(11472,-1840,0), new Vector3(11600,-1840,0), new Vector3(11728,-1840,0), new Vector3(11856,-1840,0), new 
Vector3(11984,-1840,0), new Vector3(2000,-1968,0), new Vector3(2128,-1968,0), new Vector3(2256,-1968,0), new Vector3(2384,-1968,0), new Vector3(2512,-1968,0), 
new Vector3(2640,-1968,0), new Vector3(2768,-1968,0), new Vector3(2896,-1968,0), new Vector3(3024,-1968,0), new Vector3(3152,-1968,0), new Vector3(3280,-1968,0), 
new Vector3(3408,-1968,0), new Vector3(3536,-1968,0), new Vector3(3664,-1968,0), new Vector3(3792,-1968,0), new Vector3(3920,-1968,0), new Vector3(4048,-1968,0), 
new Vector3(4176,-1968,0), new Vector3(4304,-1968,0), new Vector3(4432,-1968,0), new Vector3(4560,-1968,0), new Vector3(4688,-1968,0), new Vector3(4816,-1968,0), 
new Vector3(4944,-1968,0), new Vector3(5072,-1968,0), new Vector3(5200,-1968,0), new Vector3(5328,-1968,0), new Vector3(5456,-1968,0), new Vector3(5584,-1968,0), 
new Vector3(5712,-1968,0), new Vector3(5840,-1968,0), new Vector3(5968,-1968,0), new Vector3(6096,-1968,0), new Vector3(6224,-1968,0), new Vector3(6352,-1968,0), 
new Vector3(6480,-1968,0), new Vector3(6608,-1968,0), new Vector3(6736,-1968,0), new Vector3(6864,-1968,0), new Vector3(6992,-1968,0), new Vector3(7120,-1968,0), 
new Vector3(7248,-1968,0), new Vector3(7376,-1968,0), new Vector3(7504,-1968,0), new Vector3(7632,-1968,0), new Vector3(7760,-1968,0), new Vector3(7888,-1968,0), 
new Vector3(8016,-1968,0), new Vector3(8144,-1968,0), new Vector3(8272,-1968,0), new Vector3(8400,-1968,0), new Vector3(8528,-1968,0), new Vector3(8656,-1968,0), 
new Vector3(8784,-1968,0), new Vector3(8912,-1968,0), new Vector3(9040,-1968,0), new Vector3(9168,-1968,0), new Vector3(9296,-1968,0), new Vector3(9424,-1968,0), 
new Vector3(9552,-1968,0), new Vector3(9680,-1968,0), new Vector3(9808,-1968,0), new Vector3(9936,-1968,0), new Vector3(10064,-1968,0), new 
Vector3(10192,-1968,0), new Vector3(10320,-1968,0), new Vector3(10448,-1968,0), new Vector3(10576,-1968,0), new Vector3(10704,-1968,0), new 
Vector3(10832,-1968,0), new Vector3(10960,-1968,0), new Vector3(11088,-1968,0), new Vector3(11216,-1968,0), new Vector3(11344,-1968,0), new 
Vector3(11472,-1968,0), new Vector3(11600,-1968,0), new Vector3(11728,-1968,0), new Vector3(11856,-1968,0), new Vector3(11984,-1968,0), new 
Vector3(2000,-2096,0), new Vector3(2128,-2096,0), new Vector3(2256,-2096,0), new Vector3(2384,-2096,0), new Vector3(2512,-2096,0), new Vector3(2640,-2096,0), new 
Vector3(2768,-2096,0), new Vector3(2896,-2096,0), new Vector3(3024,-2096,0), new Vector3(3152,-2096,0), new Vector3(3280,-2096,0), new Vector3(3408,-2096,0), new 
Vector3(3536,-2096,0), new Vector3(3664,-2096,0), new Vector3(3792,-2096,0), new Vector3(3920,-2096,0), new Vector3(4048,-2096,0), new Vector3(4176,-2096,0), new 
Vector3(4304,-2096,0), new Vector3(4432,-2096,0), new Vector3(4560,-2096,0), new Vector3(4688,-2096,0), new Vector3(4816,-2096,0), new Vector3(4944,-2096,0), new 
Vector3(5072,-2096,0), new Vector3(5200,-2096,0), new Vector3(5328,-2096,0), new Vector3(5456,-2096,0), new Vector3(5584,-2096,0), new Vector3(5712,-2096,0), new 
Vector3(5840,-2096,0), new Vector3(5968,-2096,0), new Vector3(6096,-2096,0), new Vector3(6224,-2096,0), new Vector3(6352,-2096,0), new Vector3(6480,-2096,0), new 
Vector3(6608,-2096,0), new Vector3(6736,-2096,0), new Vector3(6864,-2096,0), new Vector3(6992,-2096,0), new Vector3(7120,-2096,0), new Vector3(7248,-2096,0), new 
Vector3(7376,-2096,0), new Vector3(7504,-2096,0), new Vector3(7632,-2096,0), new Vector3(7760,-2096,0), new Vector3(7888,-2096,0), new Vector3(8016,-2096,0), new 
Vector3(8144,-2096,0), new Vector3(8272,-2096,0), new Vector3(8400,-2096,0), new Vector3(8528,-2096,0), new Vector3(8656,-2096,0), new Vector3(8784,-2096,0), new 
Vector3(8912,-2096,0), new Vector3(9040,-2096,0), new Vector3(9168,-2096,0), new Vector3(9296,-2096,0), new Vector3(9424,-2096,0), new Vector3(9552,-2096,0), new 
Vector3(9680,-2096,0), new Vector3(9808,-2096,0), new Vector3(9936,-2096,0), new Vector3(10064,-2096,0), new Vector3(10192,-2096,0), new Vector3(10320,-2096,0), 
new Vector3(10448,-2096,0), new Vector3(10576,-2096,0), new Vector3(10704,-2096,0), new Vector3(10832,-2096,0), new Vector3(10960,-2096,0), new 
Vector3(11088,-2096,0), new Vector3(11216,-2096,0), new Vector3(11344,-2096,0), new Vector3(11472,-2096,0), new Vector3(11600,-2096,0), new 
Vector3(11728,-2096,0), new Vector3(11856,-2096,0), new Vector3(11984,-2096,0), new Vector3(2000,-2224,0), new Vector3(2128,-2224,0), new Vector3(2256,-2224,0), 
new Vector3(2384,-2224,0), new Vector3(2512,-2224,0), new Vector3(2640,-2224,0), new Vector3(2768,-2224,0), new Vector3(2896,-2224,0), new Vector3(3024,-2224,0), 
new Vector3(3152,-2224,0), new Vector3(3280,-2224,0), new Vector3(3408,-2224,0), new Vector3(3536,-2224,0), new Vector3(3664,-2224,0), new Vector3(3792,-2224,0), 
new Vector3(3920,-2224,0), new Vector3(4048,-2224,0), new Vector3(4176,-2224,0), new Vector3(4304,-2224,0), new Vector3(4432,-2224,0), new Vector3(4560,-2224,0), 
new Vector3(4688,-2224,0), new Vector3(4816,-2224,0), new Vector3(4944,-2224,0), new Vector3(5072,-2224,0), new Vector3(5200,-2224,0), new Vector3(5328,-2224,0), 
new Vector3(5456,-2224,0), new Vector3(5584,-2224,0), new Vector3(5712,-2224,0), new Vector3(5840,-2224,0), new Vector3(5968,-2224,0), new Vector3(6096,-2224,0), 
new Vector3(6224,-2224,0), new Vector3(6352,-2224,0), new Vector3(6480,-2224,0), new Vector3(6608,-2224,0), new Vector3(6736,-2224,0), new Vector3(6864,-2224,0), 
new Vector3(6992,-2224,0), new Vector3(7120,-2224,0), new Vector3(7248,-2224,0), new Vector3(7376,-2224,0), new Vector3(7504,-2224,0), new Vector3(7632,-2224,0), 
new Vector3(7760,-2224,0), new Vector3(7888,-2224,0), new Vector3(8016,-2224,0), new Vector3(8144,-2224,0), new Vector3(8272,-2224,0), new Vector3(8400,-2224,0), 
new Vector3(8528,-2224,0), new Vector3(8656,-2224,0), new Vector3(8784,-2224,0), new Vector3(8912,-2224,0), new Vector3(9040,-2224,0), new Vector3(9168,-2224,0), 
new Vector3(9296,-2224,0), new Vector3(9424,-2224,0), new Vector3(9552,-2224,0), new Vector3(9680,-2224,0), new Vector3(9808,-2224,0), new Vector3(9936,-2224,0), 
new Vector3(10064,-2224,0), new Vector3(10192,-2224,0), new Vector3(10320,-2224,0), new Vector3(10448,-2224,0), new Vector3(10576,-2224,0), new 
Vector3(10704,-2224,0), new Vector3(10832,-2224,0), new Vector3(10960,-2224,0), new Vector3(11088,-2224,0), new Vector3(11216,-2224,0), new 
Vector3(11344,-2224,0), new Vector3(11472,-2224,0), new Vector3(11600,-2224,0), new Vector3(11728,-2224,0), new Vector3(11856,-2224,0), new 
Vector3(11984,-2224,0), new Vector3(2000,-2352,0), new Vector3(2128,-2352,0), new Vector3(2256,-2352,0), new Vector3(2384,-2352,0), new Vector3(2512,-2352,0), 
new Vector3(2640,-2352,0), new Vector3(2768,-2352,0), new Vector3(2896,-2352,0), new Vector3(3024,-2352,0), new Vector3(3152,-2352,0), new Vector3(3280,-2352,0), 
new Vector3(3408,-2352,0), new Vector3(3536,-2352,0), new Vector3(3664,-2352,0), new Vector3(3792,-2352,0), new Vector3(3920,-2352,0), new Vector3(4048,-2352,0), 
new Vector3(4176,-2352,0), new Vector3(4304,-2352,0), new Vector3(4432,-2352,0), new Vector3(4560,-2352,0), new Vector3(4688,-2352,0), new Vector3(4816,-2352,0), 
new Vector3(4944,-2352,0), new Vector3(5072,-2352,0), new Vector3(5200,-2352,0), new Vector3(5328,-2352,0), new Vector3(5456,-2352,0), new Vector3(5584,-2352,0), 
new Vector3(5712,-2352,0), new Vector3(5840,-2352,0), new Vector3(5968,-2352,0), new Vector3(6096,-2352,0), new Vector3(6224,-2352,0), new Vector3(6352,-2352,0), 
new Vector3(6480,-2352,0), new Vector3(6608,-2352,0), new Vector3(6736,-2352,0), new Vector3(6864,-2352,0), new Vector3(6992,-2352,0), new Vector3(7120,-2352,0), 
new Vector3(7248,-2352,0), new Vector3(7376,-2352,0), new Vector3(7504,-2352,0), new Vector3(7632,-2352,0), new Vector3(7760,-2352,0), new Vector3(7888,-2352,0), 
new Vector3(8016,-2352,0), new Vector3(8144,-2352,0), new Vector3(8272,-2352,0), new Vector3(8400,-2352,0), new Vector3(8528,-2352,0), new Vector3(8656,-2352,0), 
new Vector3(8784,-2352,0), new Vector3(8912,-2352,0), new Vector3(9040,-2352,0), new Vector3(9168,-2352,0), new Vector3(9296,-2352,0), new Vector3(9424,-2352,0), 
new Vector3(9552,-2352,0), new Vector3(9680,-2352,0), new Vector3(9808,-2352,0), new Vector3(9936,-2352,0), new Vector3(10064,-2352,0), new 
Vector3(10192,-2352,0), new Vector3(10320,-2352,0), new Vector3(10448,-2352,0), new Vector3(10576,-2352,0), new Vector3(10704,-2352,0), new 
Vector3(10832,-2352,0), new Vector3(10960,-2352,0), new Vector3(11088,-2352,0), new Vector3(11216,-2352,0), new Vector3(11344,-2352,0), new 
Vector3(11472,-2352,0), new Vector3(11600,-2352,0), new Vector3(11728,-2352,0), new Vector3(11856,-2352,0), new Vector3(11984,-2352,0), new 
Vector3(2000,-2480,0), new Vector3(2128,-2480,0), new Vector3(2256,-2480,0), new Vector3(2384,-2480,0), new Vector3(2512,-2480,0), new Vector3(2640,-2480,0), new 
Vector3(2768,-2480,0), new Vector3(2896,-2480,0), new Vector3(3024,-2480,0), new Vector3(3152,-2480,0), new Vector3(3280,-2480,0), new Vector3(3408,-2480,0), new 
Vector3(3536,-2480,0), new Vector3(3664,-2480,0), new Vector3(3792,-2480,0), new Vector3(3920,-2480,0), new Vector3(4048,-2480,0), new Vector3(4176,-2480,0), new 
Vector3(4304,-2480,0), new Vector3(4432,-2480,0), new Vector3(4560,-2480,0), new Vector3(4688,-2480,0), new Vector3(4816,-2480,0), new Vector3(4944,-2480,0), new 
Vector3(5072,-2480,0), new Vector3(5200,-2480,0), new Vector3(5328,-2480,0), new Vector3(5456,-2480,0), new Vector3(5584,-2480,0), new Vector3(5712,-2480,0), new 
Vector3(5840,-2480,0), new Vector3(5968,-2480,0), new Vector3(6096,-2480,0), new Vector3(6224,-2480,0), new Vector3(6352,-2480,0), new Vector3(6480,-2480,0), new 
Vector3(6608,-2480,0), new Vector3(6736,-2480,0), new Vector3(6864,-2480,0), new Vector3(6992,-2480,0), new Vector3(7120,-2480,0), new Vector3(7248,-2480,0), new 
Vector3(7376,-2480,0), new Vector3(7504,-2480,0), new Vector3(7632,-2480,0), new Vector3(7760,-2480,0), new Vector3(7888,-2480,0), new Vector3(8016,-2480,0), new 
Vector3(8144,-2480,0), new Vector3(8272,-2480,0), new Vector3(8400,-2480,0), new Vector3(8528,-2480,0), new Vector3(8656,-2480,0), new Vector3(8784,-2480,0), new 
Vector3(8912,-2480,0), new Vector3(9040,-2480,0), new Vector3(9168,-2480,0), new Vector3(9296,-2480,0), new Vector3(9424,-2480,0), new Vector3(9552,-2480,0), new 
Vector3(9680,-2480,0), new Vector3(9808,-2480,0), new Vector3(9936,-2480,0), new Vector3(10064,-2480,0), new Vector3(10192,-2480,0), new Vector3(10320,-2480,0), 
new Vector3(10448,-2480,0), new Vector3(10576,-2480,0), new Vector3(10704,-2480,0), new Vector3(10832,-2480,0), new Vector3(10960,-2480,0), new 
Vector3(11088,-2480,0), new Vector3(11216,-2480,0), new Vector3(11344,-2480,0), new Vector3(11472,-2480,0), new Vector3(11600,-2480,0), new 
Vector3(11728,-2480,0), new Vector3(11856,-2480,0), new Vector3(11984,-2480,0), new Vector3(2000,-2608,0), new Vector3(2128,-2608,0), new Vector3(2256,-2608,0), 
new Vector3(2384,-2608,0), new Vector3(2512,-2608,0), new Vector3(2640,-2608,0), new Vector3(2768,-2608,0), new Vector3(2896,-2608,0), new Vector3(3024,-2608,0), 
new Vector3(3152,-2608,0), new Vector3(3280,-2608,0), new Vector3(3408,-2608,0), new Vector3(3536,-2608,0), new Vector3(3664,-2608,0), new Vector3(3792,-2608,0), 
new Vector3(3920,-2608,0), new Vector3(4048,-2608,0), new Vector3(4176,-2608,0), new Vector3(4304,-2608,0), new Vector3(4432,-2608,0), new Vector3(4560,-2608,0), 
new Vector3(4688,-2608,0), new Vector3(4816,-2608,0), new Vector3(4944,-2608,0), new Vector3(5072,-2608,0), new Vector3(5200,-2608,0), new Vector3(5328,-2608,0), 
new Vector3(5456,-2608,0), new Vector3(5584,-2608,0), new Vector3(5712,-2608,0), new Vector3(5840,-2608,0), new Vector3(5968,-2608,0), new Vector3(6096,-2608,0), 
new Vector3(6224,-2608,0), new Vector3(6352,-2608,0), new Vector3(6480,-2608,0), new Vector3(6608,-2608,0), new Vector3(6736,-2608,0), new Vector3(6864,-2608,0), 
new Vector3(6992,-2608,0), new Vector3(7120,-2608,0), new Vector3(7248,-2608,0), new Vector3(7376,-2608,0), new Vector3(7504,-2608,0), new Vector3(7632,-2608,0), 
new Vector3(7760,-2608,0), new Vector3(7888,-2608,0), new Vector3(8016,-2608,0), new Vector3(8144,-2608,0), new Vector3(8272,-2608,0), new Vector3(8400,-2608,0), 
new Vector3(8528,-2608,0), new Vector3(8656,-2608,0), new Vector3(8784,-2608,0), new Vector3(8912,-2608,0), new Vector3(9040,-2608,0), new Vector3(9168,-2608,0), 
new Vector3(9296,-2608,0), new Vector3(9424,-2608,0), new Vector3(9552,-2608,0), new Vector3(9680,-2608,0), new Vector3(9808,-2608,0), new Vector3(9936,-2608,0), 
new Vector3(10064,-2608,0), new Vector3(10192,-2608,0), new Vector3(10320,-2608,0), new Vector3(10448,-2608,0), new Vector3(10576,-2608,0), new 
Vector3(10704,-2608,0), new Vector3(10832,-2608,0), new Vector3(10960,-2608,0), new Vector3(11088,-2608,0), new Vector3(11216,-2608,0), new 
Vector3(11344,-2608,0), new Vector3(11472,-2608,0), new Vector3(11600,-2608,0), new Vector3(11728,-2608,0), new Vector3(11856,-2608,0), new 
Vector3(11984,-2608,0), new Vector3(2000,-2736,0), new Vector3(2128,-2736,0), new Vector3(2256,-2736,0), new Vector3(2384,-2736,0), new Vector3(2512,-2736,0), 
new Vector3(2640,-2736,0), new Vector3(2768,-2736,0), new Vector3(2896,-2736,0), new Vector3(3024,-2736,0), new Vector3(3152,-2736,0), new Vector3(3280,-2736,0), 
new Vector3(3408,-2736,0), new Vector3(3536,-2736,0), new Vector3(3664,-2736,0), new Vector3(3792,-2736,0), new Vector3(3920,-2736,0), new Vector3(4048,-2736,0), 
new Vector3(4176,-2736,0), new Vector3(4304,-2736,0), new Vector3(4432,-2736,0), new Vector3(4560,-2736,0), new Vector3(4688,-2736,0), new Vector3(4816,-2736,0), 
new Vector3(4944,-2736,0), new Vector3(5072,-2736,0), new Vector3(5200,-2736,0), new Vector3(5328,-2736,0), new Vector3(5456,-2736,0), new Vector3(5584,-2736,0), 
new Vector3(5712,-2736,0), new Vector3(5840,-2736,0), new Vector3(5968,-2736,0), new Vector3(6096,-2736,0), new Vector3(6224,-2736,0), new Vector3(6352,-2736,0), 
new Vector3(6480,-2736,0), new Vector3(6608,-2736,0), new Vector3(6736,-2736,0), new Vector3(6864,-2736,0), new Vector3(6992,-2736,0), new Vector3(7120,-2736,0), 
new Vector3(7248,-2736,0), new Vector3(7376,-2736,0), new Vector3(7504,-2736,0), new Vector3(7632,-2736,0), new Vector3(7760,-2736,0), new Vector3(7888,-2736,0), 
new Vector3(8016,-2736,0), new Vector3(8144,-2736,0), new Vector3(8272,-2736,0), new Vector3(8400,-2736,0), new Vector3(8528,-2736,0), new Vector3(8656,-2736,0), 
new Vector3(8784,-2736,0), new Vector3(8912,-2736,0), new Vector3(9040,-2736,0), new Vector3(9168,-2736,0), new Vector3(9296,-2736,0), new Vector3(9424,-2736,0), 
new Vector3(9552,-2736,0), new Vector3(9680,-2736,0), new Vector3(9808,-2736,0), new Vector3(9936,-2736,0), new Vector3(10064,-2736,0), new 
Vector3(10192,-2736,0), new Vector3(10320,-2736,0), new Vector3(10448,-2736,0), new Vector3(10576,-2736,0), new Vector3(10704,-2736,0), new 
Vector3(10832,-2736,0), new Vector3(10960,-2736,0), new Vector3(11088,-2736,0), new Vector3(11216,-2736,0), new Vector3(11344,-2736,0), new 
Vector3(11472,-2736,0), new Vector3(11600,-2736,0), new Vector3(11728,-2736,0), new Vector3(11856,-2736,0), new Vector3(11984,-2736,0), new 
Vector3(2000,-2864,0), new Vector3(2128,-2864,0), new Vector3(2256,-2864,0), new Vector3(2384,-2864,0), new Vector3(2512,-2864,0), new Vector3(2640,-2864,0), new 
Vector3(2768,-2864,0), new Vector3(2896,-2864,0), new Vector3(3024,-2864,0), new Vector3(3152,-2864,0), new Vector3(3280,-2864,0), new Vector3(3408,-2864,0), new 
Vector3(3536,-2864,0), new Vector3(3664,-2864,0), new Vector3(3792,-2864,0), new Vector3(3920,-2864,0), new Vector3(4048,-2864,0), new Vector3(4176,-2864,0), new 
Vector3(4304,-2864,0), new Vector3(4432,-2864,0), new Vector3(4560,-2864,0), new Vector3(4688,-2864,0), new Vector3(4816,-2864,0), new Vector3(4944,-2864,0), new 
Vector3(5072,-2864,0), new Vector3(5200,-2864,0), new Vector3(5328,-2864,0), new Vector3(5456,-2864,0), new Vector3(5584,-2864,0), new Vector3(5712,-2864,0), new 
Vector3(5840,-2864,0), new Vector3(5968,-2864,0), new Vector3(6096,-2864,0), new Vector3(6224,-2864,0), new Vector3(6352,-2864,0), new Vector3(6480,-2864,0), new 
Vector3(6608,-2864,0), new Vector3(6736,-2864,0), new Vector3(6864,-2864,0), new Vector3(6992,-2864,0), new Vector3(7120,-2864,0), new Vector3(7248,-2864,0), new 
Vector3(7376,-2864,0), new Vector3(7504,-2864,0), new Vector3(7632,-2864,0), new Vector3(7760,-2864,0), new Vector3(7888,-2864,0), new Vector3(8016,-2864,0), new 
Vector3(8144,-2864,0), new Vector3(8272,-2864,0), new Vector3(8400,-2864,0), new Vector3(8528,-2864,0), new Vector3(8656,-2864,0), new Vector3(8784,-2864,0), new 
Vector3(8912,-2864,0), new Vector3(9040,-2864,0), new Vector3(9168,-2864,0), new Vector3(9296,-2864,0), new Vector3(9424,-2864,0), new Vector3(9552,-2864,0), new 
Vector3(9680,-2864,0), new Vector3(9808,-2864,0), new Vector3(9936,-2864,0), new Vector3(10064,-2864,0), new Vector3(10192,-2864,0), new Vector3(10320,-2864,0), 
new Vector3(10448,-2864,0), new Vector3(10576,-2864,0), new Vector3(10704,-2864,0), new Vector3(10832,-2864,0), new Vector3(10960,-2864,0), new 
Vector3(11088,-2864,0), new Vector3(11216,-2864,0), new Vector3(11344,-2864,0), new Vector3(11472,-2864,0), new Vector3(11600,-2864,0), new 
Vector3(11728,-2864,0), new Vector3(11856,-2864,0), new Vector3(11984,-2864,0), new Vector3(2000,-2992,0), new Vector3(2128,-2992,0), new Vector3(2256,-2992,0), 
new Vector3(2384,-2992,0), new Vector3(2512,-2992,0), new Vector3(2640,-2992,0), new Vector3(2768,-2992,0), new Vector3(2896,-2992,0), new Vector3(3024,-2992,0), 
new Vector3(3152,-2992,0), new Vector3(3280,-2992,0), new Vector3(3408,-2992,0), new Vector3(3536,-2992,0), new Vector3(3664,-2992,0), new Vector3(3792,-2992,0), 
new Vector3(3920,-2992,0), new Vector3(4048,-2992,0), new Vector3(4176,-2992,0), new Vector3(4304,-2992,0), new Vector3(4432,-2992,0), new Vector3(4560,-2992,0), 
new Vector3(4688,-2992,0), new Vector3(4816,-2992,0), new Vector3(4944,-2992,0), new Vector3(5072,-2992,0), new Vector3(5200,-2992,0), new Vector3(5328,-2992,0), 
new Vector3(5456,-2992,0), new Vector3(5584,-2992,0), new Vector3(5712,-2992,0), new Vector3(5840,-2992,0), new Vector3(5968,-2992,0), new Vector3(6096,-2992,0), 
new Vector3(6224,-2992,0), new Vector3(6352,-2992,0), new Vector3(6480,-2992,0), new Vector3(6608,-2992,0), new Vector3(6736,-2992,0), new Vector3(6864,-2992,0), 
new Vector3(6992,-2992,0), new Vector3(7120,-2992,0), new Vector3(7248,-2992,0), new Vector3(7376,-2992,0), new Vector3(7504,-2992,0), new Vector3(7632,-2992,0), 
new Vector3(7760,-2992,0), new Vector3(7888,-2992,0), new Vector3(8016,-2992,0), new Vector3(8144,-2992,0), new Vector3(8272,-2992,0), new Vector3(8400,-2992,0), 
new Vector3(8528,-2992,0), new Vector3(8656,-2992,0), new Vector3(8784,-2992,0), new Vector3(8912,-2992,0), new Vector3(9040,-2992,0), new Vector3(9168,-2992,0), 
new Vector3(9296,-2992,0), new Vector3(9424,-2992,0), new Vector3(9552,-2992,0), new Vector3(9680,-2992,0), new Vector3(9808,-2992,0), new Vector3(9936,-2992,0), 
new Vector3(10064,-2992,0), new Vector3(10192,-2992,0), new Vector3(10320,-2992,0), new Vector3(10448,-2992,0), new Vector3(10576,-2992,0), new 
Vector3(10704,-2992,0), new Vector3(10832,-2992,0), new Vector3(10960,-2992,0), new Vector3(11088,-2992,0), new Vector3(11216,-2992,0), new 
Vector3(11344,-2992,0), new Vector3(11472,-2992,0), new Vector3(11600,-2992,0), new Vector3(11728,-2992,0), new Vector3(11856,-2992,0), new 
Vector3(11984,-2992,0), new Vector3(2000,-3120,0), new Vector3(2128,-3120,0), new Vector3(2256,-3120,0), new Vector3(2384,-3120,0), new Vector3(2512,-3120,0), 
new Vector3(2640,-3120,0), new Vector3(2768,-3120,0), new Vector3(2896,-3120,0), new Vector3(3024,-3120,0), new Vector3(3152,-3120,0), new Vector3(3280,-3120,0), 
new Vector3(3408,-3120,0), new Vector3(3536,-3120,0), new Vector3(3664,-3120,0), new Vector3(3792,-3120,0), new Vector3(3920,-3120,0), new Vector3(4048,-3120,0), 
new Vector3(4176,-3120,0), new Vector3(4304,-3120,0), new Vector3(4432,-3120,0), new Vector3(4560,-3120,0), new Vector3(4688,-3120,0), new Vector3(4816,-3120,0), 
new Vector3(4944,-3120,0), new Vector3(5072,-3120,0), new Vector3(5200,-3120,0), new Vector3(5328,-3120,0), new Vector3(5456,-3120,0), new Vector3(5584,-3120,0), 
new Vector3(5712,-3120,0), new Vector3(5840,-3120,0), new Vector3(5968,-3120,0), new Vector3(6096,-3120,0), new Vector3(6224,-3120,0), new Vector3(6352,-3120,0), 
new Vector3(6480,-3120,0), new Vector3(6608,-3120,0), new Vector3(6736,-3120,0), new Vector3(6864,-3120,0), new Vector3(6992,-3120,0), new Vector3(7120,-3120,0), 
new Vector3(7248,-3120,0), new Vector3(7376,-3120,0), new Vector3(7504,-3120,0), new Vector3(7632,-3120,0), new Vector3(7760,-3120,0), new Vector3(7888,-3120,0), 
new Vector3(8016,-3120,0), new Vector3(8144,-3120,0), new Vector3(8272,-3120,0), new Vector3(8400,-3120,0), new Vector3(8528,-3120,0), new Vector3(8656,-3120,0), 
new Vector3(8784,-3120,0), new Vector3(8912,-3120,0), new Vector3(9040,-3120,0), new Vector3(9168,-3120,0), new Vector3(9296,-3120,0), new Vector3(9424,-3120,0), 
new Vector3(9552,-3120,0), new Vector3(9680,-3120,0), new Vector3(9808,-3120,0), new Vector3(9936,-3120,0), new Vector3(10064,-3120,0), new 
Vector3(10192,-3120,0), new Vector3(10320,-3120,0), new Vector3(10448,-3120,0), new Vector3(10576,-3120,0), new Vector3(10704,-3120,0), new 
Vector3(10832,-3120,0), new Vector3(10960,-3120,0), new Vector3(11088,-3120,0), new Vector3(11216,-3120,0), new Vector3(11344,-3120,0), new 
Vector3(11472,-3120,0), new Vector3(11600,-3120,0), new Vector3(11728,-3120,0), new Vector3(11856,-3120,0), new Vector3(11984,-3120,0), new 
Vector3(2000,-3248,0), new Vector3(2128,-3248,0), new Vector3(2256,-3248,0), new Vector3(2384,-3248,0), new Vector3(2512,-3248,0), new Vector3(2640,-3248,0), new 
Vector3(2768,-3248,0), new Vector3(2896,-3248,0), new Vector3(3024,-3248,0), new Vector3(3152,-3248,0), new Vector3(3280,-3248,0), new Vector3(3408,-3248,0), new 
Vector3(3536,-3248,0), new Vector3(3664,-3248,0), new Vector3(3792,-3248,0), new Vector3(3920,-3248,0), new Vector3(4048,-3248,0), new Vector3(4176,-3248,0), new 
Vector3(4304,-3248,0), new Vector3(4432,-3248,0), new Vector3(4560,-3248,0), new Vector3(4688,-3248,0), new Vector3(4816,-3248,0), new Vector3(4944,-3248,0), new 
Vector3(5072,-3248,0), new Vector3(5200,-3248,0), new Vector3(5328,-3248,0), new Vector3(5456,-3248,0), new Vector3(5584,-3248,0), new Vector3(5712,-3248,0), new 
Vector3(5840,-3248,0), new Vector3(5968,-3248,0), new Vector3(6096,-3248,0), new Vector3(6224,-3248,0), new Vector3(6352,-3248,0), new Vector3(6480,-3248,0), new 
Vector3(6608,-3248,0), new Vector3(6736,-3248,0), new Vector3(6864,-3248,0), new Vector3(6992,-3248,0), new Vector3(7120,-3248,0), new Vector3(7248,-3248,0), new 
Vector3(7376,-3248,0), new Vector3(7504,-3248,0), new Vector3(7632,-3248,0), new Vector3(7760,-3248,0), new Vector3(7888,-3248,0), new Vector3(8016,-3248,0), new 
Vector3(8144,-3248,0), new Vector3(8272,-3248,0), new Vector3(8400,-3248,0), new Vector3(8528,-3248,0), new Vector3(8656,-3248,0), new Vector3(8784,-3248,0), new 
Vector3(8912,-3248,0), new Vector3(9040,-3248,0), new Vector3(9168,-3248,0), new Vector3(9296,-3248,0), new Vector3(9424,-3248,0), new Vector3(9552,-3248,0), new 
Vector3(9680,-3248,0), new Vector3(9808,-3248,0), new Vector3(9936,-3248,0), new Vector3(10064,-3248,0), new Vector3(10192,-3248,0), new Vector3(10320,-3248,0), 
new Vector3(10448,-3248,0), new Vector3(10576,-3248,0), new Vector3(10704,-3248,0), new Vector3(10832,-3248,0), new Vector3(10960,-3248,0), new 
Vector3(11088,-3248,0), new Vector3(11216,-3248,0), new Vector3(11344,-3248,0), new Vector3(11472,-3248,0), new Vector3(11600,-3248,0), new 
Vector3(11728,-3248,0), new Vector3(11856,-3248,0), new Vector3(11984,-3248,0), new Vector3(2000,-3376,0), new Vector3(2128,-3376,0), new Vector3(2256,-3376,0), 
new Vector3(2384,-3376,0), new Vector3(2512,-3376,0), new Vector3(2640,-3376,0), new Vector3(2768,-3376,0), new Vector3(2896,-3376,0), new Vector3(3024,-3376,0), 
new Vector3(3152,-3376,0), new Vector3(3280,-3376,0), new Vector3(3408,-3376,0), new Vector3(3536,-3376,0), new Vector3(3664,-3376,0), new Vector3(3792,-3376,0), 
new Vector3(3920,-3376,0), new Vector3(4048,-3376,0), new Vector3(4176,-3376,0), new Vector3(4304,-3376,0), new Vector3(4432,-3376,0), new Vector3(4560,-3376,0), 
new Vector3(4688,-3376,0), new Vector3(4816,-3376,0), new Vector3(4944,-3376,0), new Vector3(5072,-3376,0), new Vector3(5200,-3376,0), new Vector3(5328,-3376,0), 
new Vector3(5456,-3376,0), new Vector3(5584,-3376,0), new Vector3(5712,-3376,0), new Vector3(5840,-3376,0), new Vector3(5968,-3376,0), new Vector3(6096,-3376,0), 
new Vector3(6224,-3376,0), new Vector3(6352,-3376,0), new Vector3(6480,-3376,0), new Vector3(6608,-3376,0), new Vector3(6736,-3376,0), new Vector3(6864,-3376,0), 
new Vector3(6992,-3376,0), new Vector3(7120,-3376,0), new Vector3(7248,-3376,0), new Vector3(7376,-3376,0), new Vector3(7504,-3376,0), new Vector3(7632,-3376,0), 
new Vector3(7760,-3376,0), new Vector3(7888,-3376,0), new Vector3(8016,-3376,0), new Vector3(8144,-3376,0), new Vector3(8272,-3376,0), new Vector3(8400,-3376,0), 
new Vector3(8528,-3376,0), new Vector3(8656,-3376,0), new Vector3(8784,-3376,0), new Vector3(8912,-3376,0), new Vector3(9040,-3376,0), new Vector3(9168,-3376,0), 
new Vector3(9296,-3376,0), new Vector3(9424,-3376,0), new Vector3(9552,-3376,0), new Vector3(9680,-3376,0), new Vector3(9808,-3376,0), new Vector3(9936,-3376,0), 
new Vector3(10064,-3376,0), new Vector3(10192,-3376,0), new Vector3(10320,-3376,0), new Vector3(10448,-3376,0), new Vector3(10576,-3376,0), new 
Vector3(10704,-3376,0), new Vector3(10832,-3376,0), new Vector3(10960,-3376,0), new Vector3(11088,-3376,0), new Vector3(11216,-3376,0), new 
Vector3(11344,-3376,0), new Vector3(11472,-3376,0), new Vector3(11600,-3376,0), new Vector3(11728,-3376,0), new Vector3(11856,-3376,0), new 
Vector3(11984,-3376,0), new Vector3(2000,-3504,0), new Vector3(2128,-3504,0), new Vector3(2256,-3504,0), new Vector3(2384,-3504,0), new Vector3(2512,-3504,0), 
new Vector3(2640,-3504,0), new Vector3(2768,-3504,0), new Vector3(2896,-3504,0), new Vector3(3024,-3504,0), new Vector3(3152,-3504,0), new Vector3(3280,-3504,0), 
new Vector3(3408,-3504,0), new Vector3(3536,-3504,0), new Vector3(3664,-3504,0), new Vector3(3792,-3504,0), new Vector3(3920,-3504,0), new Vector3(4048,-3504,0), 
new Vector3(4176,-3504,0), new Vector3(4304,-3504,0), new Vector3(4432,-3504,0), new Vector3(4560,-3504,0), new Vector3(4688,-3504,0), new Vector3(4816,-3504,0), 
new Vector3(4944,-3504,0), new Vector3(5072,-3504,0), new Vector3(5200,-3504,0), new Vector3(5328,-3504,0), new Vector3(5456,-3504,0), new Vector3(5584,-3504,0), 
new Vector3(5712,-3504,0), new Vector3(5840,-3504,0), new Vector3(5968,-3504,0), new Vector3(6096,-3504,0), new Vector3(6224,-3504,0), new Vector3(6352,-3504,0), 
new Vector3(6480,-3504,0), new Vector3(6608,-3504,0), new Vector3(6736,-3504,0), new Vector3(6864,-3504,0), new Vector3(6992,-3504,0), new Vector3(7120,-3504,0), 
new Vector3(7248,-3504,0), new Vector3(7376,-3504,0), new Vector3(7504,-3504,0), new Vector3(7632,-3504,0), new Vector3(7760,-3504,0), new Vector3(7888,-3504,0), 
new Vector3(8016,-3504,0), new Vector3(8144,-3504,0), new Vector3(8272,-3504,0), new Vector3(8400,-3504,0), new Vector3(8528,-3504,0), new Vector3(8656,-3504,0), 
new Vector3(8784,-3504,0), new Vector3(8912,-3504,0), new Vector3(9040,-3504,0), new Vector3(9168,-3504,0), new Vector3(9296,-3504,0), new Vector3(9424,-3504,0), 
new Vector3(9552,-3504,0), new Vector3(9680,-3504,0), new Vector3(9808,-3504,0), new Vector3(9936,-3504,0), new Vector3(10064,-3504,0), new 
Vector3(10192,-3504,0), new Vector3(10320,-3504,0), new Vector3(10448,-3504,0), new Vector3(10576,-3504,0), new Vector3(10704,-3504,0), new 
Vector3(10832,-3504,0), new Vector3(10960,-3504,0), new Vector3(11088,-3504,0), new Vector3(11216,-3504,0), new Vector3(11344,-3504,0), new 
Vector3(11472,-3504,0), new Vector3(11600,-3504,0), new Vector3(11728,-3504,0), new Vector3(11856,-3504,0), new Vector3(11984,-3504,0), new 
Vector3(2000,-3632,0), new Vector3(2128,-3632,0), new Vector3(2256,-3632,0), new Vector3(2384,-3632,0), new Vector3(2512,-3632,0), new Vector3(2640,-3632,0), new 
Vector3(2768,-3632,0), new Vector3(2896,-3632,0), new Vector3(3024,-3632,0), new Vector3(3152,-3632,0), new Vector3(3280,-3632,0), new Vector3(3408,-3632,0), new 
Vector3(3536,-3632,0), new Vector3(3664,-3632,0), new Vector3(3792,-3632,0), new Vector3(3920,-3632,0), new Vector3(4048,-3632,0), new Vector3(4176,-3632,0), new 
Vector3(4304,-3632,0), new Vector3(4432,-3632,0), new Vector3(4560,-3632,0), new Vector3(4688,-3632,0), new Vector3(4816,-3632,0), new Vector3(4944,-3632,0), new 
Vector3(5072,-3632,0), new Vector3(5200,-3632,0), new Vector3(5328,-3632,0), new Vector3(5456,-3632,0), new Vector3(5584,-3632,0), new Vector3(5712,-3632,0), new 
Vector3(5840,-3632,0), new Vector3(5968,-3632,0), new Vector3(6096,-3632,0), new Vector3(6224,-3632,0), new Vector3(6352,-3632,0), new Vector3(6480,-3632,0), new 
Vector3(6608,-3632,0), new Vector3(6736,-3632,0), new Vector3(6864,-3632,0), new Vector3(6992,-3632,0), new Vector3(7120,-3632,0), new Vector3(7248,-3632,0), new 
Vector3(7376,-3632,0), new Vector3(7504,-3632,0), new Vector3(7632,-3632,0), new Vector3(7760,-3632,0), new Vector3(7888,-3632,0), new Vector3(8016,-3632,0), new 
Vector3(8144,-3632,0), new Vector3(8272,-3632,0), new Vector3(8400,-3632,0), new Vector3(8528,-3632,0), new Vector3(8656,-3632,0), new Vector3(8784,-3632,0), new 
Vector3(8912,-3632,0), new Vector3(9040,-3632,0), new Vector3(9168,-3632,0), new Vector3(9296,-3632,0), new Vector3(9424,-3632,0), new Vector3(9552,-3632,0), new 
Vector3(9680,-3632,0), new Vector3(9808,-3632,0), new Vector3(9936,-3632,0), new Vector3(10064,-3632,0), new Vector3(10192,-3632,0), new Vector3(10320,-3632,0), 
new Vector3(10448,-3632,0), new Vector3(10576,-3632,0), new Vector3(10704,-3632,0), new Vector3(10832,-3632,0), new Vector3(10960,-3632,0), new 
Vector3(11088,-3632,0), new Vector3(11216,-3632,0), new Vector3(11344,-3632,0), new Vector3(11472,-3632,0), new Vector3(11600,-3632,0), new 
Vector3(11728,-3632,0), new Vector3(11856,-3632,0), new Vector3(11984,-3632,0), new Vector3(2000,-3760,0), new Vector3(2128,-3760,0), new Vector3(2256,-3760,0), 
new Vector3(2384,-3760,0), new Vector3(2512,-3760,0), new Vector3(2640,-3760,0), new Vector3(2768,-3760,0), new Vector3(2896,-3760,0), new Vector3(3024,-3760,0), 
new Vector3(3152,-3760,0), new Vector3(3280,-3760,0), new Vector3(3408,-3760,0), new Vector3(3536,-3760,0), new Vector3(3664,-3760,0), new Vector3(3792,-3760,0), 
new Vector3(3920,-3760,0), new Vector3(4048,-3760,0), new Vector3(4176,-3760,0), new Vector3(4304,-3760,0), new Vector3(4432,-3760,0), new Vector3(4560,-3760,0), 
new Vector3(4688,-3760,0), new Vector3(4816,-3760,0), new Vector3(4944,-3760,0), new Vector3(5072,-3760,0), new Vector3(5200,-3760,0), new Vector3(5328,-3760,0), 
new Vector3(5456,-3760,0), new Vector3(5584,-3760,0), new Vector3(5712,-3760,0), new Vector3(5840,-3760,0), new Vector3(5968,-3760,0), new Vector3(6096,-3760,0), 
new Vector3(6224,-3760,0), new Vector3(6352,-3760,0), new Vector3(6480,-3760,0), new Vector3(6608,-3760,0), new Vector3(6736,-3760,0), new Vector3(6864,-3760,0), 
new Vector3(6992,-3760,0), new Vector3(7120,-3760,0), new Vector3(7248,-3760,0), new Vector3(7376,-3760,0), new Vector3(7504,-3760,0), new Vector3(7632,-3760,0), 
new Vector3(7760,-3760,0), new Vector3(7888,-3760,0), new Vector3(8016,-3760,0), new Vector3(8144,-3760,0), new Vector3(8272,-3760,0), new Vector3(8400,-3760,0), 
new Vector3(8528,-3760,0), new Vector3(8656,-3760,0), new Vector3(8784,-3760,0), new Vector3(8912,-3760,0), new Vector3(9040,-3760,0), new Vector3(9168,-3760,0), 
new Vector3(9296,-3760,0), new Vector3(9424,-3760,0), new Vector3(9552,-3760,0), new Vector3(9680,-3760,0), new Vector3(9808,-3760,0), new Vector3(9936,-3760,0), 
new Vector3(10064,-3760,0), new Vector3(10192,-3760,0), new Vector3(10320,-3760,0), new Vector3(10448,-3760,0), new Vector3(10576,-3760,0), new 
Vector3(10704,-3760,0), new Vector3(10832,-3760,0), new Vector3(10960,-3760,0), new Vector3(11088,-3760,0), new Vector3(11216,-3760,0), new 
Vector3(11344,-3760,0), new Vector3(11472,-3760,0), new Vector3(11600,-3760,0), new Vector3(11728,-3760,0), new Vector3(11856,-3760,0), new 
Vector3(11984,-3760,0), new Vector3(2000,-3888,0), new Vector3(2128,-3888,0), new Vector3(2256,-3888,0), new Vector3(2384,-3888,0), new Vector3(2512,-3888,0), 
new Vector3(2640,-3888,0), new Vector3(2768,-3888,0), new Vector3(2896,-3888,0), new Vector3(3024,-3888,0), new Vector3(3152,-3888,0), new Vector3(3280,-3888,0), 
new Vector3(3408,-3888,0), new Vector3(3536,-3888,0), new Vector3(3664,-3888,0), new Vector3(3792,-3888,0), new Vector3(3920,-3888,0), new Vector3(4048,-3888,0), 
new Vector3(4176,-3888,0), new Vector3(4304,-3888,0), new Vector3(4432,-3888,0), new Vector3(4560,-3888,0), new Vector3(4688,-3888,0), new Vector3(4816,-3888,0), 
new Vector3(4944,-3888,0), new Vector3(5072,-3888,0), new Vector3(5200,-3888,0), new Vector3(5328,-3888,0), new Vector3(5456,-3888,0), new Vector3(5584,-3888,0), 
new Vector3(5712,-3888,0), new Vector3(5840,-3888,0), new Vector3(5968,-3888,0), new Vector3(6096,-3888,0), new Vector3(6224,-3888,0), new Vector3(6352,-3888,0), 
new Vector3(6480,-3888,0), new Vector3(6608,-3888,0), new Vector3(6736,-3888,0), new Vector3(6864,-3888,0), new Vector3(6992,-3888,0), new Vector3(7120,-3888,0), 
new Vector3(7248,-3888,0), new Vector3(7376,-3888,0), new Vector3(7504,-3888,0), new Vector3(7632,-3888,0), new Vector3(7760,-3888,0), new Vector3(7888,-3888,0), 
new Vector3(8016,-3888,0), new Vector3(8144,-3888,0), new Vector3(8272,-3888,0), new Vector3(8400,-3888,0), new Vector3(8528,-3888,0), new Vector3(8656,-3888,0), 
new Vector3(8784,-3888,0), new Vector3(8912,-3888,0), new Vector3(9040,-3888,0), new Vector3(9168,-3888,0), new Vector3(9296,-3888,0), new Vector3(9424,-3888,0), 
new Vector3(9552,-3888,0), new Vector3(9680,-3888,0), new Vector3(9808,-3888,0), new Vector3(9936,-3888,0), new Vector3(10064,-3888,0), new 
Vector3(10192,-3888,0), new Vector3(10320,-3888,0), new Vector3(10448,-3888,0), new Vector3(10576,-3888,0), new Vector3(10704,-3888,0), new 
Vector3(10832,-3888,0), new Vector3(10960,-3888,0), new Vector3(11088,-3888,0), new Vector3(11216,-3888,0), new Vector3(11344,-3888,0), new 
Vector3(11472,-3888,0), new Vector3(11600,-3888,0), new Vector3(11728,-3888,0), new Vector3(11856,-3888,0), new Vector3(11984,-3888,0), new 
Vector3(2000,-4016,0), new Vector3(2128,-4016,0), new Vector3(2256,-4016,0), new Vector3(2384,-4016,0), new Vector3(2512,-4016,0), new Vector3(2640,-4016,0), new 
Vector3(2768,-4016,0), new Vector3(2896,-4016,0), new Vector3(3024,-4016,0), new Vector3(3152,-4016,0), new Vector3(3280,-4016,0), new Vector3(3408,-4016,0), new 
Vector3(3536,-4016,0), new Vector3(3664,-4016,0), new Vector3(3792,-4016,0), new Vector3(3920,-4016,0), new Vector3(4048,-4016,0), new Vector3(4176,-4016,0), new 
Vector3(4304,-4016,0), new Vector3(4432,-4016,0), new Vector3(4560,-4016,0), new Vector3(4688,-4016,0), new Vector3(4816,-4016,0), new Vector3(4944,-4016,0), new 
Vector3(5072,-4016,0), new Vector3(5200,-4016,0), new Vector3(5328,-4016,0), new Vector3(5456,-4016,0), new Vector3(5584,-4016,0), new Vector3(5712,-4016,0), new 
Vector3(5840,-4016,0), new Vector3(5968,-4016,0), new Vector3(6096,-4016,0), new Vector3(6224,-4016,0), new Vector3(6352,-4016,0), new Vector3(6480,-4016,0), new 
Vector3(6608,-4016,0), new Vector3(6736,-4016,0), new Vector3(6864,-4016,0), new Vector3(6992,-4016,0), new Vector3(7120,-4016,0), new Vector3(7248,-4016,0), new 
Vector3(7376,-4016,0), new Vector3(7504,-4016,0), new Vector3(7632,-4016,0), new Vector3(7760,-4016,0), new Vector3(7888,-4016,0), new Vector3(8016,-4016,0), new 
Vector3(8144,-4016,0), new Vector3(8272,-4016,0), new Vector3(8400,-4016,0), new Vector3(8528,-4016,0), new Vector3(8656,-4016,0), new Vector3(8784,-4016,0), new 
Vector3(8912,-4016,0), new Vector3(9040,-4016,0), new Vector3(9168,-4016,0), new Vector3(9296,-4016,0), new Vector3(9424,-4016,0), new Vector3(9552,-4016,0), new 
Vector3(9680,-4016,0), new Vector3(9808,-4016,0), new Vector3(9936,-4016,0), new Vector3(10064,-4016,0), new Vector3(10192,-4016,0), new Vector3(10320,-4016,0), 
new Vector3(10448,-4016,0), new Vector3(10576,-4016,0), new Vector3(10704,-4016,0), new Vector3(10832,-4016,0), new Vector3(10960,-4016,0), new 
Vector3(11088,-4016,0), new Vector3(11216,-4016,0), new Vector3(11344,-4016,0), new Vector3(11472,-4016,0), new Vector3(11600,-4016,0), new 
Vector3(11728,-4016,0), new Vector3(11856,-4016,0), new Vector3(11984,-4016,0), new Vector3(2000,-4144,0), new Vector3(2128,-4144,0), new Vector3(2256,-4144,0), 
new Vector3(2384,-4144,0), new Vector3(2512,-4144,0), new Vector3(2640,-4144,0), new Vector3(2768,-4144,0), new Vector3(2896,-4144,0), new Vector3(3024,-4144,0), 
new Vector3(3152,-4144,0), new Vector3(3280,-4144,0), new Vector3(3408,-4144,0), new Vector3(3536,-4144,0), new Vector3(3664,-4144,0), new Vector3(3792,-4144,0), 
new Vector3(3920,-4144,0), new Vector3(4048,-4144,0), new Vector3(4176,-4144,0), new Vector3(4304,-4144,0), new Vector3(4432,-4144,0), new Vector3(4560,-4144,0), 
new Vector3(4688,-4144,0), new Vector3(4816,-4144,0), new Vector3(4944,-4144,0), new Vector3(5072,-4144,0), new Vector3(5200,-4144,0), new Vector3(5328,-4144,0), 
new Vector3(5456,-4144,0), new Vector3(5584,-4144,0), new Vector3(5712,-4144,0), new Vector3(5840,-4144,0), new Vector3(5968,-4144,0), new Vector3(6096,-4144,0), 
new Vector3(6224,-4144,0), new Vector3(6352,-4144,0), new Vector3(6480,-4144,0), new Vector3(6608,-4144,0), new Vector3(6736,-4144,0), new Vector3(6864,-4144,0), 
new Vector3(6992,-4144,0), new Vector3(7120,-4144,0), new Vector3(7248,-4144,0), new Vector3(7376,-4144,0), new Vector3(7504,-4144,0), new Vector3(7632,-4144,0), 
new Vector3(7760,-4144,0), new Vector3(7888,-4144,0), new Vector3(8016,-4144,0), new Vector3(8144,-4144,0), new Vector3(8272,-4144,0), new Vector3(8400,-4144,0), 
new Vector3(8528,-4144,0), new Vector3(8656,-4144,0), new Vector3(8784,-4144,0), new Vector3(8912,-4144,0), new Vector3(9040,-4144,0), new Vector3(9168,-4144,0), 
new Vector3(9296,-4144,0), new Vector3(9424,-4144,0), new Vector3(9552,-4144,0), new Vector3(9680,-4144,0), new Vector3(9808,-4144,0), new Vector3(9936,-4144,0), 
new Vector3(10064,-4144,0), new Vector3(10192,-4144,0), new Vector3(10320,-4144,0), new Vector3(10448,-4144,0), new Vector3(10576,-4144,0), new 
Vector3(10704,-4144,0), new Vector3(10832,-4144,0), new Vector3(10960,-4144,0), new Vector3(11088,-4144,0), new Vector3(11216,-4144,0), new 
Vector3(11344,-4144,0), new Vector3(11472,-4144,0), new Vector3(11600,-4144,0), new Vector3(11728,-4144,0), new Vector3(11856,-4144,0), new 
Vector3(11984,-4144,0), new Vector3(2000,-4272,0), new Vector3(2128,-4272,0), new Vector3(2256,-4272,0), new Vector3(2384,-4272,0), new Vector3(2512,-4272,0), 
new Vector3(2640,-4272,0), new Vector3(2768,-4272,0), new Vector3(2896,-4272,0), new Vector3(3024,-4272,0), new Vector3(3152,-4272,0), new Vector3(3280,-4272,0), 
new Vector3(3408,-4272,0), new Vector3(3536,-4272,0), new Vector3(3664,-4272,0), new Vector3(3792,-4272,0), new Vector3(3920,-4272,0), new Vector3(4048,-4272,0), 
new Vector3(4176,-4272,0), new Vector3(4304,-4272,0), new Vector3(4432,-4272,0), new Vector3(4560,-4272,0), new Vector3(4688,-4272,0), new Vector3(4816,-4272,0), 
new Vector3(4944,-4272,0), new Vector3(5072,-4272,0), new Vector3(5200,-4272,0), new Vector3(5328,-4272,0), new Vector3(5456,-4272,0), new Vector3(5584,-4272,0), 
new Vector3(5712,-4272,0), new Vector3(5840,-4272,0), new Vector3(5968,-4272,0), new Vector3(6096,-4272,0), new Vector3(6224,-4272,0), new Vector3(6352,-4272,0), 
new Vector3(6480,-4272,0), new Vector3(6608,-4272,0), new Vector3(6736,-4272,0), new Vector3(6864,-4272,0), new Vector3(6992,-4272,0), new Vector3(7120,-4272,0), 
new Vector3(7248,-4272,0), new Vector3(7376,-4272,0), new Vector3(7504,-4272,0), new Vector3(7632,-4272,0), new Vector3(7760,-4272,0), new Vector3(7888,-4272,0), 
new Vector3(8016,-4272,0), new Vector3(8144,-4272,0), new Vector3(8272,-4272,0), new Vector3(8400,-4272,0), new Vector3(8528,-4272,0), new Vector3(8656,-4272,0), 
new Vector3(8784,-4272,0), new Vector3(8912,-4272,0), new Vector3(9040,-4272,0), new Vector3(9168,-4272,0), new Vector3(9296,-4272,0), new Vector3(9424,-4272,0), 
new Vector3(9552,-4272,0), new Vector3(9680,-4272,0), new Vector3(9808,-4272,0), new Vector3(9936,-4272,0), new Vector3(10064,-4272,0), new 
Vector3(10192,-4272,0), new Vector3(10320,-4272,0), new Vector3(10448,-4272,0), new Vector3(10576,-4272,0), new Vector3(10704,-4272,0), new 
Vector3(10832,-4272,0), new Vector3(10960,-4272,0), new Vector3(11088,-4272,0), new Vector3(11216,-4272,0), new Vector3(11344,-4272,0), new 
Vector3(11472,-4272,0), new Vector3(11600,-4272,0), new Vector3(11728,-4272,0), new Vector3(11856,-4272,0), new Vector3(11984,-4272,0), new 
Vector3(2000,-4400,0), new Vector3(2128,-4400,0), new Vector3(2256,-4400,0), new Vector3(2384,-4400,0), new Vector3(2512,-4400,0), new Vector3(2640,-4400,0), new 
Vector3(2768,-4400,0), new Vector3(2896,-4400,0), new Vector3(3024,-4400,0), new Vector3(3152,-4400,0), new Vector3(3280,-4400,0), new Vector3(3408,-4400,0), new 
Vector3(3536,-4400,0), new Vector3(3664,-4400,0), new Vector3(3792,-4400,0), new Vector3(3920,-4400,0), new Vector3(4048,-4400,0), new Vector3(4176,-4400,0), new 
Vector3(4304,-4400,0), new Vector3(4432,-4400,0), new Vector3(4560,-4400,0), new Vector3(4688,-4400,0), new Vector3(4816,-4400,0), new Vector3(4944,-4400,0), new 
Vector3(5072,-4400,0), new Vector3(5200,-4400,0), new Vector3(5328,-4400,0), new Vector3(5456,-4400,0), new Vector3(5584,-4400,0), new Vector3(5712,-4400,0), new 
Vector3(5840,-4400,0), new Vector3(5968,-4400,0), new Vector3(6096,-4400,0), new Vector3(6224,-4400,0), new Vector3(6352,-4400,0), new Vector3(6480,-4400,0), new 
Vector3(6608,-4400,0), new Vector3(6736,-4400,0), new Vector3(6864,-4400,0), new Vector3(6992,-4400,0), new Vector3(7120,-4400,0), new Vector3(7248,-4400,0), new 
Vector3(7376,-4400,0), new Vector3(7504,-4400,0), new Vector3(7632,-4400,0), new Vector3(7760,-4400,0), new Vector3(7888,-4400,0), new Vector3(8016,-4400,0), new 
Vector3(8144,-4400,0), new Vector3(8272,-4400,0), new Vector3(8400,-4400,0), new Vector3(8528,-4400,0), new Vector3(8656,-4400,0), new Vector3(8784,-4400,0), new 
Vector3(8912,-4400,0), new Vector3(9040,-4400,0), new Vector3(9168,-4400,0), new Vector3(9296,-4400,0), new Vector3(9424,-4400,0), new Vector3(9552,-4400,0), new 
Vector3(9680,-4400,0), new Vector3(9808,-4400,0), new Vector3(9936,-4400,0), new Vector3(10064,-4400,0), new Vector3(10192,-4400,0), new Vector3(10320,-4400,0), 
new Vector3(10448,-4400,0), new Vector3(10576,-4400,0), new Vector3(10704,-4400,0), new Vector3(10832,-4400,0), new Vector3(10960,-4400,0), new 
Vector3(11088,-4400,0), new Vector3(11216,-4400,0), new Vector3(11344,-4400,0), new Vector3(11472,-4400,0), new Vector3(11600,-4400,0), new 
Vector3(11728,-4400,0), new Vector3(11856,-4400,0), new Vector3(11984,-4400,0), new Vector3(2000,-4528,0), new Vector3(2128,-4528,0), new Vector3(2256,-4528,0), 
new Vector3(2384,-4528,0), new Vector3(2512,-4528,0), new Vector3(2640,-4528,0), new Vector3(2768,-4528,0), new Vector3(2896,-4528,0), new Vector3(3024,-4528,0), 
new Vector3(3152,-4528,0), new Vector3(3280,-4528,0), new Vector3(3408,-4528,0), new Vector3(3536,-4528,0), new Vector3(3664,-4528,0), new Vector3(3792,-4528,0), 
new Vector3(3920,-4528,0), new Vector3(4048,-4528,0), new Vector3(4176,-4528,0), new Vector3(4304,-4528,0), new Vector3(4432,-4528,0), new Vector3(4560,-4528,0), 
new Vector3(4688,-4528,0), new Vector3(4816,-4528,0), new Vector3(4944,-4528,0), new Vector3(5072,-4528,0), new Vector3(5200,-4528,0), new Vector3(5328,-4528,0), 
new Vector3(5456,-4528,0), new Vector3(5584,-4528,0), new Vector3(5712,-4528,0), new Vector3(5840,-4528,0), new Vector3(5968,-4528,0), new Vector3(6096,-4528,0), 
new Vector3(6224,-4528,0), new Vector3(6352,-4528,0), new Vector3(6480,-4528,0), new Vector3(6608,-4528,0), new Vector3(6736,-4528,0), new Vector3(6864,-4528,0), 
new Vector3(6992,-4528,0), new Vector3(7120,-4528,0), new Vector3(7248,-4528,0), new Vector3(7376,-4528,0), new Vector3(7504,-4528,0), new Vector3(7632,-4528,0), 
new Vector3(7760,-4528,0), new Vector3(7888,-4528,0), new Vector3(8016,-4528,0), new Vector3(8144,-4528,0), new Vector3(8272,-4528,0), new Vector3(8400,-4528,0), 
new Vector3(8528,-4528,0), new Vector3(8656,-4528,0), new Vector3(8784,-4528,0), new Vector3(8912,-4528,0), new Vector3(9040,-4528,0), new Vector3(9168,-4528,0), 
new Vector3(9296,-4528,0), new Vector3(9424,-4528,0), new Vector3(9552,-4528,0), new Vector3(9680,-4528,0), new Vector3(9808,-4528,0), new Vector3(9936,-4528,0), 
new Vector3(10064,-4528,0), new Vector3(10192,-4528,0), new Vector3(10320,-4528,0), new Vector3(10448,-4528,0), new Vector3(10576,-4528,0), new 
Vector3(10704,-4528,0), new Vector3(10832,-4528,0), new Vector3(10960,-4528,0), new Vector3(11088,-4528,0), new Vector3(11216,-4528,0), new 
Vector3(11344,-4528,0), new Vector3(11472,-4528,0), new Vector3(11600,-4528,0), new Vector3(11728,-4528,0), new Vector3(11856,-4528,0), new 
Vector3(11984,-4528,0), new Vector3(2000,-4656,0), new Vector3(2128,-4656,0), new Vector3(2256,-4656,0), new Vector3(2384,-4656,0), new Vector3(2512,-4656,0), 
new Vector3(2640,-4656,0), new Vector3(2768,-4656,0), new Vector3(2896,-4656,0), new Vector3(3024,-4656,0), new Vector3(3152,-4656,0), new Vector3(3280,-4656,0), 
new Vector3(3408,-4656,0), new Vector3(3536,-4656,0), new Vector3(3664,-4656,0), new Vector3(3792,-4656,0), new Vector3(3920,-4656,0), new Vector3(4048,-4656,0), 
new Vector3(4176,-4656,0), new Vector3(4304,-4656,0), new Vector3(4432,-4656,0), new Vector3(4560,-4656,0), new Vector3(4688,-4656,0), new Vector3(4816,-4656,0), 
new Vector3(4944,-4656,0), new Vector3(5072,-4656,0), new Vector3(5200,-4656,0), new Vector3(5328,-4656,0), new Vector3(5456,-4656,0), new Vector3(5584,-4656,0), 
new Vector3(5712,-4656,0), new Vector3(5840,-4656,0), new Vector3(5968,-4656,0), new Vector3(6096,-4656,0), new Vector3(6224,-4656,0), new Vector3(6352,-4656,0), 
new Vector3(6480,-4656,0), new Vector3(6608,-4656,0), new Vector3(6736,-4656,0), new Vector3(6864,-4656,0), new Vector3(6992,-4656,0), new Vector3(7120,-4656,0), 
new Vector3(7248,-4656,0), new Vector3(7376,-4656,0), new Vector3(7504,-4656,0), new Vector3(7632,-4656,0), new Vector3(7760,-4656,0), new Vector3(7888,-4656,0), 
new Vector3(8016,-4656,0), new Vector3(8144,-4656,0), new Vector3(8272,-4656,0), new Vector3(8400,-4656,0), new Vector3(8528,-4656,0), new Vector3(8656,-4656,0), 
new Vector3(8784,-4656,0), new Vector3(8912,-4656,0), new Vector3(9040,-4656,0), new Vector3(9168,-4656,0), new Vector3(9296,-4656,0), new Vector3(9424,-4656,0), 
new Vector3(9552,-4656,0), new Vector3(9680,-4656,0), new Vector3(9808,-4656,0), new Vector3(9936,-4656,0), new Vector3(10064,-4656,0), new 
Vector3(10192,-4656,0), new Vector3(10320,-4656,0), new Vector3(10448,-4656,0), new Vector3(10576,-4656,0), new Vector3(10704,-4656,0), new 
Vector3(10832,-4656,0), new Vector3(10960,-4656,0), new Vector3(11088,-4656,0), new Vector3(11216,-4656,0), new Vector3(11344,-4656,0), new 
Vector3(11472,-4656,0), new Vector3(11600,-4656,0), new Vector3(11728,-4656,0), new Vector3(11856,-4656,0), new Vector3(11984,-4656,0), new 
Vector3(2000,-4784,0), new Vector3(2128,-4784,0), new Vector3(2256,-4784,0), new Vector3(2384,-4784,0), new Vector3(2512,-4784,0), new Vector3(2640,-4784,0), new 
Vector3(2768,-4784,0), new Vector3(2896,-4784,0), new Vector3(3024,-4784,0), new Vector3(3152,-4784,0), new Vector3(3280,-4784,0), new Vector3(3408,-4784,0), new 
Vector3(3536,-4784,0), new Vector3(3664,-4784,0), new Vector3(3792,-4784,0), new Vector3(3920,-4784,0), new Vector3(4048,-4784,0), new Vector3(4176,-4784,0), new 
Vector3(4304,-4784,0), new Vector3(4432,-4784,0), new Vector3(4560,-4784,0), new Vector3(4688,-4784,0), new Vector3(4816,-4784,0), new Vector3(4944,-4784,0), new 
Vector3(5072,-4784,0), new Vector3(5200,-4784,0), new Vector3(5328,-4784,0), new Vector3(5456,-4784,0), new Vector3(5584,-4784,0), new Vector3(5712,-4784,0), new 
Vector3(5840,-4784,0), new Vector3(5968,-4784,0), new Vector3(6096,-4784,0), new Vector3(6224,-4784,0), new Vector3(6352,-4784,0), new Vector3(6480,-4784,0), new 
Vector3(6608,-4784,0), new Vector3(6736,-4784,0), new Vector3(6864,-4784,0), new Vector3(6992,-4784,0), new Vector3(7120,-4784,0), new Vector3(7248,-4784,0), new 
Vector3(7376,-4784,0), new Vector3(7504,-4784,0), new Vector3(7632,-4784,0), new Vector3(7760,-4784,0), new Vector3(7888,-4784,0), new Vector3(8016,-4784,0), new 
Vector3(8144,-4784,0), new Vector3(8272,-4784,0), new Vector3(8400,-4784,0), new Vector3(8528,-4784,0), new Vector3(8656,-4784,0), new Vector3(8784,-4784,0), new 
Vector3(8912,-4784,0), new Vector3(9040,-4784,0), new Vector3(9168,-4784,0), new Vector3(9296,-4784,0), new Vector3(9424,-4784,0), new Vector3(9552,-4784,0), new 
Vector3(9680,-4784,0), new Vector3(9808,-4784,0), new Vector3(9936,-4784,0), new Vector3(10064,-4784,0), new Vector3(10192,-4784,0), new Vector3(10320,-4784,0), 
new Vector3(10448,-4784,0), new Vector3(10576,-4784,0), new Vector3(10704,-4784,0), new Vector3(10832,-4784,0), new Vector3(10960,-4784,0), new 
Vector3(11088,-4784,0), new Vector3(11216,-4784,0), new Vector3(11344,-4784,0), new Vector3(11472,-4784,0), new Vector3(11600,-4784,0), new 
Vector3(11728,-4784,0), new Vector3(11856,-4784,0), new Vector3(11984,-4784,0), new Vector3(2000,-4912,0), new Vector3(2128,-4912,0), new Vector3(2256,-4912,0), 
new Vector3(2384,-4912,0), new Vector3(2512,-4912,0), new Vector3(2640,-4912,0), new Vector3(2768,-4912,0), new Vector3(2896,-4912,0), new Vector3(3024,-4912,0), 
new Vector3(3152,-4912,0), new Vector3(3280,-4912,0), new Vector3(3408,-4912,0), new Vector3(3536,-4912,0), new Vector3(3664,-4912,0), new Vector3(3792,-4912,0), 
new Vector3(3920,-4912,0), new Vector3(4048,-4912,0), new Vector3(4176,-4912,0), new Vector3(4304,-4912,0), new Vector3(4432,-4912,0), new Vector3(4560,-4912,0), 
new Vector3(4688,-4912,0), new Vector3(4816,-4912,0), new Vector3(4944,-4912,0), new Vector3(5072,-4912,0), new Vector3(5200,-4912,0), new Vector3(5328,-4912,0), 
new Vector3(5456,-4912,0), new Vector3(5584,-4912,0), new Vector3(5712,-4912,0), new Vector3(5840,-4912,0), new Vector3(5968,-4912,0), new Vector3(6096,-4912,0), 
new Vector3(6224,-4912,0), new Vector3(6352,-4912,0), new Vector3(6480,-4912,0), new Vector3(6608,-4912,0), new Vector3(6736,-4912,0), new Vector3(6864,-4912,0), 
new Vector3(6992,-4912,0), new Vector3(7120,-4912,0), new Vector3(7248,-4912,0), new Vector3(7376,-4912,0), new Vector3(7504,-4912,0), new Vector3(7632,-4912,0), 
new Vector3(7760,-4912,0), new Vector3(7888,-4912,0), new Vector3(8016,-4912,0), new Vector3(8144,-4912,0), new Vector3(8272,-4912,0), new Vector3(8400,-4912,0), 
new Vector3(8528,-4912,0), new Vector3(8656,-4912,0), new Vector3(8784,-4912,0), new Vector3(8912,-4912,0), new Vector3(9040,-4912,0), new Vector3(9168,-4912,0), 
new Vector3(9296,-4912,0), new Vector3(9424,-4912,0), new Vector3(9552,-4912,0), new Vector3(9680,-4912,0), new Vector3(9808,-4912,0), new Vector3(9936,-4912,0), 
new Vector3(10064,-4912,0), new Vector3(10192,-4912,0), new Vector3(10320,-4912,0), new Vector3(10448,-4912,0), new Vector3(10576,-4912,0), new 
Vector3(10704,-4912,0), new Vector3(10832,-4912,0), new Vector3(10960,-4912,0), new Vector3(11088,-4912,0), new Vector3(11216,-4912,0), new 
Vector3(11344,-4912,0), new Vector3(11472,-4912,0), new Vector3(11600,-4912,0), new Vector3(11728,-4912,0), new Vector3(11856,-4912,0), new 
Vector3(11984,-4912,0), new Vector3(2000,-5040,0), new Vector3(2128,-5040,0), new Vector3(2256,-5040,0), new Vector3(2384,-5040,0), new Vector3(2512,-5040,0), 
new Vector3(2640,-5040,0), new Vector3(2768,-5040,0), new Vector3(2896,-5040,0), new Vector3(3024,-5040,0), new Vector3(3152,-5040,0), new Vector3(3280,-5040,0), 
new Vector3(3408,-5040,0), new Vector3(3536,-5040,0), new Vector3(3664,-5040,0), new Vector3(3792,-5040,0), new Vector3(3920,-5040,0), new Vector3(4048,-5040,0), 
new Vector3(4176,-5040,0), new Vector3(4304,-5040,0), new Vector3(4432,-5040,0), new Vector3(4560,-5040,0), new Vector3(4688,-5040,0), new Vector3(4816,-5040,0), 
new Vector3(4944,-5040,0), new Vector3(5072,-5040,0), new Vector3(5200,-5040,0), new Vector3(5328,-5040,0), new Vector3(5456,-5040,0), new Vector3(5584,-5040,0), 
new Vector3(5712,-5040,0), new Vector3(5840,-5040,0), new Vector3(5968,-5040,0), new Vector3(6096,-5040,0), new Vector3(6224,-5040,0), new Vector3(6352,-5040,0), 
new Vector3(6480,-5040,0), new Vector3(6608,-5040,0), new Vector3(6736,-5040,0), new Vector3(6864,-5040,0), new Vector3(6992,-5040,0), new Vector3(7120,-5040,0), 
new Vector3(7248,-5040,0), new Vector3(7376,-5040,0), new Vector3(7504,-5040,0), new Vector3(7632,-5040,0), new Vector3(7760,-5040,0), new Vector3(7888,-5040,0), 
new Vector3(8016,-5040,0), new Vector3(8144,-5040,0), new Vector3(8272,-5040,0), new Vector3(8400,-5040,0), new Vector3(8528,-5040,0), new Vector3(8656,-5040,0), 
new Vector3(8784,-5040,0), new Vector3(8912,-5040,0), new Vector3(9040,-5040,0), new Vector3(9168,-5040,0), new Vector3(9296,-5040,0), new Vector3(9424,-5040,0), 
new Vector3(9552,-5040,0), new Vector3(9680,-5040,0), new Vector3(9808,-5040,0), new Vector3(9936,-5040,0), new Vector3(10064,-5040,0), new 
Vector3(10192,-5040,0), new Vector3(10320,-5040,0), new Vector3(10448,-5040,0), new Vector3(10576,-5040,0), new Vector3(10704,-5040,0), new 
Vector3(10832,-5040,0), new Vector3(10960,-5040,0), new Vector3(11088,-5040,0), new Vector3(11216,-5040,0), new Vector3(11344,-5040,0), new 
Vector3(11472,-5040,0), new Vector3(11600,-5040,0), new Vector3(11728,-5040,0), new Vector3(11856,-5040,0), new Vector3(11984,-5040,0), new 
Vector3(2000,-5168,0), new Vector3(2128,-5168,0), new Vector3(2256,-5168,0), new Vector3(2384,-5168,0), new Vector3(2512,-5168,0), new Vector3(2640,-5168,0), new 
Vector3(2768,-5168,0), new Vector3(2896,-5168,0), new Vector3(3024,-5168,0), new Vector3(3152,-5168,0), new Vector3(3280,-5168,0), new Vector3(3408,-5168,0), new 
Vector3(3536,-5168,0), new Vector3(3664,-5168,0), new Vector3(3792,-5168,0), new Vector3(3920,-5168,0), new Vector3(4048,-5168,0), new Vector3(4176,-5168,0), new 
Vector3(4304,-5168,0), new Vector3(4432,-5168,0), new Vector3(4560,-5168,0), new Vector3(4688,-5168,0), new Vector3(4816,-5168,0), new Vector3(4944,-5168,0), new 
Vector3(5072,-5168,0), new Vector3(5200,-5168,0), new Vector3(5328,-5168,0), new Vector3(5456,-5168,0), new Vector3(5584,-5168,0), new Vector3(5712,-5168,0), new 
Vector3(5840,-5168,0), new Vector3(5968,-5168,0), new Vector3(6096,-5168,0), new Vector3(6224,-5168,0), new Vector3(6352,-5168,0), new Vector3(6480,-5168,0), new 
Vector3(6608,-5168,0), new Vector3(6736,-5168,0), new Vector3(6864,-5168,0), new Vector3(6992,-5168,0), new Vector3(7120,-5168,0), new Vector3(7248,-5168,0), new 
Vector3(7376,-5168,0), new Vector3(7504,-5168,0), new Vector3(7632,-5168,0), new Vector3(7760,-5168,0), new Vector3(7888,-5168,0), new Vector3(8016,-5168,0), new 
Vector3(8144,-5168,0), new Vector3(8272,-5168,0), new Vector3(8400,-5168,0), new Vector3(8528,-5168,0), new Vector3(8656,-5168,0), new Vector3(8784,-5168,0), new 
Vector3(8912,-5168,0), new Vector3(9040,-5168,0), new Vector3(9168,-5168,0), new Vector3(9296,-5168,0), new Vector3(9424,-5168,0), new Vector3(9552,-5168,0), new 
Vector3(9680,-5168,0), new Vector3(9808,-5168,0), new Vector3(9936,-5168,0), new Vector3(10064,-5168,0), new Vector3(10192,-5168,0), new Vector3(10320,-5168,0), 
new Vector3(10448,-5168,0), new Vector3(10576,-5168,0), new Vector3(10704,-5168,0), new Vector3(10832,-5168,0), new Vector3(10960,-5168,0), new 
Vector3(11088,-5168,0), new Vector3(11216,-5168,0), new Vector3(11344,-5168,0), new Vector3(11472,-5168,0), new Vector3(11600,-5168,0), new 
Vector3(11728,-5168,0), new Vector3(11856,-5168,0), new Vector3(11984,-5168,0), new Vector3(2000,-5296,0), new Vector3(2128,-5296,0), new Vector3(2256,-5296,0), 
new Vector3(2384,-5296,0), new Vector3(2512,-5296,0), new Vector3(2640,-5296,0), new Vector3(2768,-5296,0), new Vector3(2896,-5296,0), new Vector3(3024,-5296,0), 
new Vector3(3152,-5296,0), new Vector3(3280,-5296,0), new Vector3(3408,-5296,0), new Vector3(3536,-5296,0), new Vector3(3664,-5296,0), new Vector3(3792,-5296,0), 
new Vector3(3920,-5296,0), new Vector3(4048,-5296,0), new Vector3(4176,-5296,0), new Vector3(4304,-5296,0), new Vector3(4432,-5296,0), new Vector3(4560,-5296,0), 
new Vector3(4688,-5296,0), new Vector3(4816,-5296,0), new Vector3(4944,-5296,0), new Vector3(5072,-5296,0), new Vector3(5200,-5296,0), new Vector3(5328,-5296,0), 
new Vector3(5456,-5296,0), new Vector3(5584,-5296,0), new Vector3(5712,-5296,0), new Vector3(5840,-5296,0), new Vector3(5968,-5296,0), new Vector3(6096,-5296,0), 
new Vector3(6224,-5296,0), new Vector3(6352,-5296,0), new Vector3(6480,-5296,0), new Vector3(6608,-5296,0), new Vector3(6736,-5296,0), new Vector3(6864,-5296,0), 
new Vector3(6992,-5296,0), new Vector3(7120,-5296,0), new Vector3(7248,-5296,0), new Vector3(7376,-5296,0), new Vector3(7504,-5296,0), new Vector3(7632,-5296,0), 
new Vector3(7760,-5296,0), new Vector3(7888,-5296,0), new Vector3(8016,-5296,0), new Vector3(8144,-5296,0), new Vector3(8272,-5296,0), new Vector3(8400,-5296,0), 
new Vector3(8528,-5296,0), new Vector3(8656,-5296,0), new Vector3(8784,-5296,0), new Vector3(8912,-5296,0), new Vector3(9040,-5296,0), new Vector3(9168,-5296,0), 
new Vector3(9296,-5296,0), new Vector3(9424,-5296,0), new Vector3(9552,-5296,0), new Vector3(9680,-5296,0), new Vector3(9808,-5296,0), new Vector3(9936,-5296,0), 
new Vector3(10064,-5296,0), new Vector3(10192,-5296,0), new Vector3(10320,-5296,0), new Vector3(10448,-5296,0), new Vector3(10576,-5296,0), new 
Vector3(10704,-5296,0), new Vector3(10832,-5296,0), new Vector3(10960,-5296,0), new Vector3(11088,-5296,0), new Vector3(11216,-5296,0), new 
Vector3(11344,-5296,0), new Vector3(11472,-5296,0), new Vector3(11600,-5296,0), new Vector3(11728,-5296,0), new Vector3(11856,-5296,0), new 
Vector3(11984,-5296,0), new Vector3(2000,-5424,0), new Vector3(2128,-5424,0), new Vector3(2256,-5424,0), new Vector3(2384,-5424,0), new Vector3(2512,-5424,0), 
new Vector3(2640,-5424,0), new Vector3(2768,-5424,0), new Vector3(2896,-5424,0), new Vector3(3024,-5424,0), new Vector3(3152,-5424,0), new Vector3(3280,-5424,0), 
new Vector3(3408,-5424,0), new Vector3(3536,-5424,0), new Vector3(3664,-5424,0), new Vector3(3792,-5424,0), new Vector3(3920,-5424,0), new Vector3(4048,-5424,0), 
new Vector3(4176,-5424,0), new Vector3(4304,-5424,0), new Vector3(4432,-5424,0), new Vector3(4560,-5424,0), new Vector3(4688,-5424,0), new Vector3(4816,-5424,0), 
new Vector3(4944,-5424,0), new Vector3(5072,-5424,0), new Vector3(5200,-5424,0), new Vector3(5328,-5424,0), new Vector3(5456,-5424,0), new Vector3(5584,-5424,0), 
new Vector3(5712,-5424,0), new Vector3(5840,-5424,0), new Vector3(5968,-5424,0), new Vector3(6096,-5424,0), new Vector3(6224,-5424,0), new Vector3(6352,-5424,0), 
new Vector3(6480,-5424,0), new Vector3(6608,-5424,0), new Vector3(6736,-5424,0), new Vector3(6864,-5424,0), new Vector3(6992,-5424,0), new Vector3(7120,-5424,0), 
new Vector3(7248,-5424,0), new Vector3(7376,-5424,0), new Vector3(7504,-5424,0), new Vector3(7632,-5424,0), new Vector3(7760,-5424,0), new Vector3(7888,-5424,0), 
new Vector3(8016,-5424,0), new Vector3(8144,-5424,0), new Vector3(8272,-5424,0), new Vector3(8400,-5424,0), new Vector3(8528,-5424,0), new Vector3(8656,-5424,0), 
new Vector3(8784,-5424,0), new Vector3(8912,-5424,0), new Vector3(9040,-5424,0), new Vector3(9168,-5424,0), new Vector3(9296,-5424,0), new Vector3(9424,-5424,0), 
new Vector3(9552,-5424,0), new Vector3(9680,-5424,0), new Vector3(9808,-5424,0), new Vector3(9936,-5424,0), new Vector3(10064,-5424,0), new 
Vector3(10192,-5424,0), new Vector3(10320,-5424,0), new Vector3(10448,-5424,0), new Vector3(10576,-5424,0), new Vector3(10704,-5424,0), new 
Vector3(10832,-5424,0), new Vector3(10960,-5424,0), new Vector3(11088,-5424,0), new Vector3(11216,-5424,0), new Vector3(11344,-5424,0), new 
Vector3(11472,-5424,0), new Vector3(11600,-5424,0), new Vector3(11728,-5424,0), new Vector3(11856,-5424,0), new Vector3(11984,-5424,0), new 
Vector3(2000,-5552,0), new Vector3(2128,-5552,0), new Vector3(2256,-5552,0), new Vector3(2384,-5552,0), new Vector3(2512,-5552,0), new Vector3(2640,-5552,0), new 
Vector3(2768,-5552,0), new Vector3(2896,-5552,0), new Vector3(3024,-5552,0), new Vector3(3152,-5552,0), new Vector3(3280,-5552,0), new Vector3(3408,-5552,0), new 
Vector3(3536,-5552,0), new Vector3(3664,-5552,0), new Vector3(3792,-5552,0), new Vector3(3920,-5552,0), new Vector3(4048,-5552,0), new Vector3(4176,-5552,0), new 
Vector3(4304,-5552,0), new Vector3(4432,-5552,0), new Vector3(4560,-5552,0), new Vector3(4688,-5552,0), new Vector3(4816,-5552,0), new Vector3(4944,-5552,0), new 
Vector3(5072,-5552,0), new Vector3(5200,-5552,0), new Vector3(5328,-5552,0), new Vector3(5456,-5552,0), new Vector3(5584,-5552,0), new Vector3(5712,-5552,0), new 
Vector3(5840,-5552,0), new Vector3(5968,-5552,0), new Vector3(6096,-5552,0), new Vector3(6224,-5552,0), new Vector3(6352,-5552,0), new Vector3(6480,-5552,0), new 
Vector3(6608,-5552,0), new Vector3(6736,-5552,0), new Vector3(6864,-5552,0), new Vector3(6992,-5552,0), new Vector3(7120,-5552,0), new Vector3(7248,-5552,0), new 
Vector3(7376,-5552,0), new Vector3(7504,-5552,0), new Vector3(7632,-5552,0), new Vector3(7760,-5552,0), new Vector3(7888,-5552,0), new Vector3(8016,-5552,0), new 
Vector3(8144,-5552,0), new Vector3(8272,-5552,0), new Vector3(8400,-5552,0), new Vector3(8528,-5552,0), new Vector3(8656,-5552,0), new Vector3(8784,-5552,0), new 
Vector3(8912,-5552,0), new Vector3(9040,-5552,0), new Vector3(9168,-5552,0), new Vector3(9296,-5552,0), new Vector3(9424,-5552,0), new Vector3(9552,-5552,0), new 
Vector3(9680,-5552,0), new Vector3(9808,-5552,0), new Vector3(9936,-5552,0), new Vector3(10064,-5552,0), new Vector3(10192,-5552,0), new Vector3(10320,-5552,0), 
new Vector3(10448,-5552,0), new Vector3(10576,-5552,0), new Vector3(10704,-5552,0), new Vector3(10832,-5552,0), new Vector3(10960,-5552,0), new 
Vector3(11088,-5552,0), new Vector3(11216,-5552,0), new Vector3(11344,-5552,0), new Vector3(11472,-5552,0), new Vector3(11600,-5552,0), new 
Vector3(11728,-5552,0), new Vector3(11856,-5552,0), new Vector3(11984,-5552,0), new Vector3(2000,-5680,0), new Vector3(2128,-5680,0), new Vector3(2256,-5680,0), 
new Vector3(2384,-5680,0), new Vector3(2512,-5680,0), new Vector3(2640,-5680,0), new Vector3(2768,-5680,0), new Vector3(2896,-5680,0), new Vector3(3024,-5680,0), 
new Vector3(3152,-5680,0), new Vector3(3280,-5680,0), new Vector3(3408,-5680,0), new Vector3(3536,-5680,0), new Vector3(3664,-5680,0), new Vector3(3792,-5680,0), 
new Vector3(3920,-5680,0), new Vector3(4048,-5680,0), new Vector3(4176,-5680,0), new Vector3(4304,-5680,0), new Vector3(4432,-5680,0), new Vector3(4560,-5680,0), 
new Vector3(4688,-5680,0), new Vector3(4816,-5680,0), new Vector3(4944,-5680,0), new Vector3(5072,-5680,0), new Vector3(5200,-5680,0), new Vector3(5328,-5680,0), 
new Vector3(5456,-5680,0), new Vector3(5584,-5680,0), new Vector3(5712,-5680,0), new Vector3(5840,-5680,0), new Vector3(5968,-5680,0), new Vector3(6096,-5680,0), 
new Vector3(6224,-5680,0), new Vector3(6352,-5680,0), new Vector3(6480,-5680,0), new Vector3(6608,-5680,0), new Vector3(6736,-5680,0), new Vector3(6864,-5680,0), 
new Vector3(6992,-5680,0), new Vector3(7120,-5680,0), new Vector3(7248,-5680,0), new Vector3(7376,-5680,0), new Vector3(7504,-5680,0), new Vector3(7632,-5680,0), 
new Vector3(7760,-5680,0), new Vector3(7888,-5680,0), new Vector3(8016,-5680,0), new Vector3(8144,-5680,0), new Vector3(8272,-5680,0), new Vector3(8400,-5680,0), 
new Vector3(8528,-5680,0), new Vector3(8656,-5680,0), new Vector3(8784,-5680,0), new Vector3(8912,-5680,0), new Vector3(9040,-5680,0), new Vector3(9168,-5680,0), 
new Vector3(9296,-5680,0), new Vector3(9424,-5680,0), new Vector3(9552,-5680,0), new Vector3(9680,-5680,0), new Vector3(9808,-5680,0), new Vector3(9936,-5680,0), 
new Vector3(10064,-5680,0), new Vector3(10192,-5680,0), new Vector3(10320,-5680,0), new Vector3(10448,-5680,0), new Vector3(10576,-5680,0), new 
Vector3(10704,-5680,0), new Vector3(10832,-5680,0), new Vector3(10960,-5680,0), new Vector3(11088,-5680,0), new Vector3(11216,-5680,0), new 
Vector3(11344,-5680,0), new Vector3(11472,-5680,0), new Vector3(11600,-5680,0), new Vector3(11728,-5680,0), new Vector3(11856,-5680,0), new 
Vector3(11984,-5680,0), new Vector3(2000,-5808,0), new Vector3(2128,-5808,0), new Vector3(2256,-5808,0), new Vector3(2384,-5808,0), new Vector3(2512,-5808,0), 
new Vector3(2640,-5808,0), new Vector3(2768,-5808,0), new Vector3(2896,-5808,0), new Vector3(3024,-5808,0), new Vector3(3152,-5808,0), new Vector3(3280,-5808,0), 
new Vector3(3408,-5808,0), new Vector3(3536,-5808,0), new Vector3(3664,-5808,0), new Vector3(3792,-5808,0), new Vector3(3920,-5808,0), new Vector3(4048,-5808,0), 
new Vector3(4176,-5808,0), new Vector3(4304,-5808,0), new Vector3(4432,-5808,0), new Vector3(4560,-5808,0), new Vector3(4688,-5808,0), new Vector3(4816,-5808,0), 
new Vector3(4944,-5808,0), new Vector3(5072,-5808,0), new Vector3(5200,-5808,0), new Vector3(5328,-5808,0), new Vector3(5456,-5808,0), new Vector3(5584,-5808,0), 
new Vector3(5712,-5808,0), new Vector3(5840,-5808,0), new Vector3(5968,-5808,0), new Vector3(6096,-5808,0), new Vector3(6224,-5808,0), new Vector3(6352,-5808,0), 
new Vector3(6480,-5808,0), new Vector3(6608,-5808,0), new Vector3(6736,-5808,0), new Vector3(6864,-5808,0), new Vector3(6992,-5808,0), new Vector3(7120,-5808,0), 
new Vector3(7248,-5808,0), new Vector3(7376,-5808,0), new Vector3(7504,-5808,0), new Vector3(7632,-5808,0), new Vector3(7760,-5808,0), new Vector3(7888,-5808,0), 
new Vector3(8016,-5808,0), new Vector3(8144,-5808,0), new Vector3(8272,-5808,0), new Vector3(8400,-5808,0), new Vector3(8528,-5808,0), new Vector3(8656,-5808,0), 
new Vector3(8784,-5808,0), new Vector3(8912,-5808,0), new Vector3(9040,-5808,0), new Vector3(9168,-5808,0), new Vector3(9296,-5808,0), new Vector3(9424,-5808,0), 
new Vector3(9552,-5808,0), new Vector3(9680,-5808,0), new Vector3(9808,-5808,0), new Vector3(9936,-5808,0), new Vector3(10064,-5808,0), new 
Vector3(10192,-5808,0), new Vector3(10320,-5808,0), new Vector3(10448,-5808,0), new Vector3(10576,-5808,0), new Vector3(10704,-5808,0), new 
Vector3(10832,-5808,0), new Vector3(10960,-5808,0), new Vector3(11088,-5808,0), new Vector3(11216,-5808,0), new Vector3(11344,-5808,0), new 
Vector3(11472,-5808,0), new Vector3(11600,-5808,0), new Vector3(11728,-5808,0), new Vector3(11856,-5808,0), new Vector3(11984,-5808,0), new 
Vector3(2000,-5936,0), new Vector3(2128,-5936,0), new Vector3(2256,-5936,0), new Vector3(2384,-5936,0), new Vector3(2512,-5936,0), new Vector3(2640,-5936,0), new 
Vector3(2768,-5936,0), new Vector3(2896,-5936,0), new Vector3(3024,-5936,0), new Vector3(3152,-5936,0), new Vector3(3280,-5936,0), new Vector3(3408,-5936,0), new 
Vector3(3536,-5936,0), new Vector3(3664,-5936,0), new Vector3(3792,-5936,0), new Vector3(3920,-5936,0), new Vector3(4048,-5936,0), new Vector3(4176,-5936,0), new 
Vector3(4304,-5936,0), new Vector3(4432,-5936,0), new Vector3(4560,-5936,0), new Vector3(4688,-5936,0), new Vector3(4816,-5936,0), new Vector3(4944,-5936,0), new 
Vector3(5072,-5936,0), new Vector3(5200,-5936,0), new Vector3(5328,-5936,0), new Vector3(5456,-5936,0), new Vector3(5584,-5936,0), new Vector3(5712,-5936,0), new 
Vector3(5840,-5936,0), new Vector3(5968,-5936,0), new Vector3(6096,-5936,0), new Vector3(6224,-5936,0), new Vector3(6352,-5936,0), new Vector3(6480,-5936,0), new 
Vector3(6608,-5936,0), new Vector3(6736,-5936,0), new Vector3(6864,-5936,0), new Vector3(6992,-5936,0), new Vector3(7120,-5936,0), new Vector3(7248,-5936,0), new 
Vector3(7376,-5936,0), new Vector3(7504,-5936,0), new Vector3(7632,-5936,0), new Vector3(7760,-5936,0), new Vector3(7888,-5936,0), new Vector3(8016,-5936,0), new 
Vector3(8144,-5936,0), new Vector3(8272,-5936,0), new Vector3(8400,-5936,0), new Vector3(8528,-5936,0), new Vector3(8656,-5936,0), new Vector3(8784,-5936,0), new 
Vector3(8912,-5936,0), new Vector3(9040,-5936,0), new Vector3(9168,-5936,0), new Vector3(9296,-5936,0), new Vector3(9424,-5936,0), new Vector3(9552,-5936,0), new 
Vector3(9680,-5936,0), new Vector3(9808,-5936,0), new Vector3(9936,-5936,0), new Vector3(10064,-5936,0), new Vector3(10192,-5936,0), new Vector3(10320,-5936,0), 
new Vector3(10448,-5936,0), new Vector3(10576,-5936,0), new Vector3(10704,-5936,0), new Vector3(10832,-5936,0), new Vector3(10960,-5936,0), new 
Vector3(11088,-5936,0), new Vector3(11216,-5936,0), new Vector3(11344,-5936,0), new Vector3(11472,-5936,0), new Vector3(11600,-5936,0), new 
Vector3(11728,-5936,0), new Vector3(11856,-5936,0), new Vector3(11984,-5936,0), new Vector3(2000,-6064,0), new Vector3(2128,-6064,0), new Vector3(2256,-6064,0), 
new Vector3(2384,-6064,0), new Vector3(2512,-6064,0), new Vector3(2640,-6064,0), new Vector3(2768,-6064,0), new Vector3(2896,-6064,0), new Vector3(3024,-6064,0), 
new Vector3(3152,-6064,0), new Vector3(3280,-6064,0), new Vector3(3408,-6064,0), new Vector3(3536,-6064,0), new Vector3(3664,-6064,0), new Vector3(3792,-6064,0), 
new Vector3(3920,-6064,0), new Vector3(4048,-6064,0), new Vector3(4176,-6064,0), new Vector3(4304,-6064,0), new Vector3(4432,-6064,0), new Vector3(4560,-6064,0), 
new Vector3(4688,-6064,0), new Vector3(4816,-6064,0), new Vector3(4944,-6064,0), new Vector3(5072,-6064,0), new Vector3(5200,-6064,0), new Vector3(5328,-6064,0), 
new Vector3(5456,-6064,0), new Vector3(5584,-6064,0), new Vector3(5712,-6064,0), new Vector3(5840,-6064,0), new Vector3(5968,-6064,0), new Vector3(6096,-6064,0), 
new Vector3(6224,-6064,0), new Vector3(6352,-6064,0), new Vector3(6480,-6064,0), new Vector3(6608,-6064,0), new Vector3(6736,-6064,0), new Vector3(6864,-6064,0), 
new Vector3(6992,-6064,0), new Vector3(7120,-6064,0), new Vector3(7248,-6064,0), new Vector3(7376,-6064,0), new Vector3(7504,-6064,0), new Vector3(7632,-6064,0), 
new Vector3(7760,-6064,0), new Vector3(7888,-6064,0), new Vector3(8016,-6064,0), new Vector3(8144,-6064,0), new Vector3(8272,-6064,0), new Vector3(8400,-6064,0), 
new Vector3(8528,-6064,0), new Vector3(8656,-6064,0), new Vector3(8784,-6064,0), new Vector3(8912,-6064,0), new Vector3(9040,-6064,0), new Vector3(9168,-6064,0), 
new Vector3(9296,-6064,0), new Vector3(9424,-6064,0), new Vector3(9552,-6064,0), new Vector3(9680,-6064,0), new Vector3(9808,-6064,0), new Vector3(9936,-6064,0), 
new Vector3(10064,-6064,0), new Vector3(10192,-6064,0), new Vector3(10320,-6064,0), new Vector3(10448,-6064,0), new Vector3(10576,-6064,0), new 
Vector3(10704,-6064,0), new Vector3(10832,-6064,0), new Vector3(10960,-6064,0), new Vector3(11088,-6064,0), new Vector3(11216,-6064,0), new 
Vector3(11344,-6064,0), new Vector3(11472,-6064,0), new Vector3(11600,-6064,0), new Vector3(11728,-6064,0), new Vector3(11856,-6064,0), new 
Vector3(11984,-6064,0), new Vector3(2000,-6192,0), new Vector3(2128,-6192,0), new Vector3(2256,-6192,0), new Vector3(2384,-6192,0), new Vector3(2512,-6192,0), 
new Vector3(2640,-6192,0), new Vector3(2768,-6192,0), new Vector3(2896,-6192,0), new Vector3(3024,-6192,0), new Vector3(3152,-6192,0), new Vector3(3280,-6192,0), 
new Vector3(3408,-6192,0), new Vector3(3536,-6192,0), new Vector3(3664,-6192,0), new Vector3(3792,-6192,0), new Vector3(3920,-6192,0), new Vector3(4048,-6192,0), 
new Vector3(4176,-6192,0), new Vector3(4304,-6192,0), new Vector3(4432,-6192,0), new Vector3(4560,-6192,0), new Vector3(4688,-6192,0), new Vector3(4816,-6192,0), 
new Vector3(4944,-6192,0), new Vector3(5072,-6192,0), new Vector3(5200,-6192,0), new Vector3(5328,-6192,0), new Vector3(5456,-6192,0), new Vector3(5584,-6192,0), 
new Vector3(5712,-6192,0), new Vector3(5840,-6192,0), new Vector3(5968,-6192,0), new Vector3(6096,-6192,0), new Vector3(6224,-6192,0), new Vector3(6352,-6192,0), 
new Vector3(6480,-6192,0), new Vector3(6608,-6192,0), new Vector3(6736,-6192,0), new Vector3(6864,-6192,0), new Vector3(6992,-6192,0), new Vector3(7120,-6192,0), 
new Vector3(7248,-6192,0), new Vector3(7376,-6192,0), new Vector3(7504,-6192,0), new Vector3(7632,-6192,0), new Vector3(7760,-6192,0), new Vector3(7888,-6192,0), 
new Vector3(8016,-6192,0), new Vector3(8144,-6192,0), new Vector3(8272,-6192,0), new Vector3(8400,-6192,0), new Vector3(8528,-6192,0), new Vector3(8656,-6192,0), 
new Vector3(8784,-6192,0), new Vector3(8912,-6192,0), new Vector3(9040,-6192,0), new Vector3(9168,-6192,0), new Vector3(9296,-6192,0), new Vector3(9424,-6192,0), 
new Vector3(9552,-6192,0), new Vector3(9680,-6192,0), new Vector3(9808,-6192,0), new Vector3(9936,-6192,0), new Vector3(10064,-6192,0), new 
Vector3(10192,-6192,0), new Vector3(10320,-6192,0), new Vector3(10448,-6192,0), new Vector3(10576,-6192,0), new Vector3(10704,-6192,0), new 
Vector3(10832,-6192,0), new Vector3(10960,-6192,0), new Vector3(11088,-6192,0), new Vector3(11216,-6192,0), new Vector3(11344,-6192,0), new 
Vector3(11472,-6192,0), new Vector3(11600,-6192,0), new Vector3(11728,-6192,0), new Vector3(11856,-6192,0), new Vector3(11984,-6192,0), new 
Vector3(2000,-6320,0), new Vector3(2128,-6320,0), new Vector3(2256,-6320,0), new Vector3(2384,-6320,0), new Vector3(2512,-6320,0), new Vector3(2640,-6320,0), new 
Vector3(2768,-6320,0), new Vector3(2896,-6320,0), new Vector3(3024,-6320,0), new Vector3(3152,-6320,0), new Vector3(3280,-6320,0), new Vector3(3408,-6320,0), new 
Vector3(3536,-6320,0), new Vector3(3664,-6320,0), new Vector3(3792,-6320,0), new Vector3(3920,-6320,0), new Vector3(4048,-6320,0), new Vector3(4176,-6320,0), new 
Vector3(4304,-6320,0), new Vector3(4432,-6320,0), new Vector3(4560,-6320,0), new Vector3(4688,-6320,0), new Vector3(4816,-6320,0), new Vector3(4944,-6320,0), new 
Vector3(5072,-6320,0), new Vector3(5200,-6320,0), new Vector3(5328,-6320,0), new Vector3(5456,-6320,0), new Vector3(5584,-6320,0), new Vector3(5712,-6320,0), new 
Vector3(5840,-6320,0), new Vector3(5968,-6320,0), new Vector3(6096,-6320,0), new Vector3(6224,-6320,0), new Vector3(6352,-6320,0), new Vector3(6480,-6320,0), new 
Vector3(6608,-6320,0), new Vector3(6736,-6320,0), new Vector3(6864,-6320,0), new Vector3(6992,-6320,0), new Vector3(7120,-6320,0), new Vector3(7248,-6320,0), new 
Vector3(7376,-6320,0), new Vector3(7504,-6320,0), new Vector3(7632,-6320,0), new Vector3(7760,-6320,0), new Vector3(7888,-6320,0), new Vector3(8016,-6320,0), new 
Vector3(8144,-6320,0), new Vector3(8272,-6320,0), new Vector3(8400,-6320,0), new Vector3(8528,-6320,0), new Vector3(8656,-6320,0), new Vector3(8784,-6320,0), new 
Vector3(8912,-6320,0), new Vector3(9040,-6320,0), new Vector3(9168,-6320,0), new Vector3(9296,-6320,0), new Vector3(9424,-6320,0), new Vector3(9552,-6320,0), new 
Vector3(9680,-6320,0), new Vector3(9808,-6320,0), new Vector3(9936,-6320,0), new Vector3(10064,-6320,0), new Vector3(10192,-6320,0), new Vector3(10320,-6320,0), 
new Vector3(10448,-6320,0), new Vector3(10576,-6320,0), new Vector3(10704,-6320,0), new Vector3(10832,-6320,0), new Vector3(10960,-6320,0), new 
Vector3(11088,-6320,0), new Vector3(11216,-6320,0), new Vector3(11344,-6320,0), new Vector3(11472,-6320,0), new Vector3(11600,-6320,0), new 
Vector3(11728,-6320,0), new Vector3(11856,-6320,0), new Vector3(11984,-6320,0), new Vector3(2000,-6448,0), new Vector3(2128,-6448,0), new Vector3(2256,-6448,0), 
new Vector3(2384,-6448,0), new Vector3(2512,-6448,0), new Vector3(2640,-6448,0), new Vector3(2768,-6448,0), new Vector3(2896,-6448,0), new Vector3(3024,-6448,0), 
new Vector3(3152,-6448,0), new Vector3(3280,-6448,0), new Vector3(3408,-6448,0), new Vector3(3536,-6448,0), new Vector3(3664,-6448,0), new Vector3(3792,-6448,0), 
new Vector3(3920,-6448,0), new Vector3(4048,-6448,0), new Vector3(4176,-6448,0), new Vector3(4304,-6448,0), new Vector3(4432,-6448,0), new Vector3(4560,-6448,0), 
new Vector3(4688,-6448,0), new Vector3(4816,-6448,0), new Vector3(4944,-6448,0), new Vector3(5072,-6448,0), new Vector3(5200,-6448,0), new Vector3(5328,-6448,0), 
new Vector3(5456,-6448,0), new Vector3(5584,-6448,0), new Vector3(5712,-6448,0), new Vector3(5840,-6448,0), new Vector3(5968,-6448,0), new Vector3(6096,-6448,0), 
new Vector3(6224,-6448,0), new Vector3(6352,-6448,0), new Vector3(6480,-6448,0), new Vector3(6608,-6448,0), new Vector3(6736,-6448,0), new Vector3(6864,-6448,0), 
new Vector3(6992,-6448,0), new Vector3(7120,-6448,0), new Vector3(7248,-6448,0), new Vector3(7376,-6448,0), new Vector3(7504,-6448,0), new Vector3(7632,-6448,0), 
new Vector3(7760,-6448,0), new Vector3(7888,-6448,0), new Vector3(8016,-6448,0), new Vector3(8144,-6448,0), new Vector3(8272,-6448,0), new Vector3(8400,-6448,0), 
new Vector3(8528,-6448,0), new Vector3(8656,-6448,0), new Vector3(8784,-6448,0), new Vector3(8912,-6448,0), new Vector3(9040,-6448,0), new Vector3(9168,-6448,0), 
new Vector3(9296,-6448,0), new Vector3(9424,-6448,0), new Vector3(9552,-6448,0), new Vector3(9680,-6448,0), new Vector3(9808,-6448,0), new Vector3(9936,-6448,0), 
new Vector3(10064,-6448,0), new Vector3(10192,-6448,0), new Vector3(10320,-6448,0), new Vector3(10448,-6448,0), new Vector3(10576,-6448,0), new 
Vector3(10704,-6448,0), new Vector3(10832,-6448,0), new Vector3(10960,-6448,0), new Vector3(11088,-6448,0), new Vector3(11216,-6448,0), new 
Vector3(11344,-6448,0), new Vector3(11472,-6448,0), new Vector3(11600,-6448,0), new Vector3(11728,-6448,0), new Vector3(11856,-6448,0), new 
Vector3(11984,-6448,0), new Vector3(2000,-6576,0), new Vector3(2128,-6576,0), new Vector3(2256,-6576,0), new Vector3(2384,-6576,0), new Vector3(2512,-6576,0), 
new Vector3(2640,-6576,0), new Vector3(2768,-6576,0), new Vector3(2896,-6576,0), new Vector3(3024,-6576,0), new Vector3(3152,-6576,0), new Vector3(3280,-6576,0), 
new Vector3(3408,-6576,0), new Vector3(3536,-6576,0), new Vector3(3664,-6576,0), new Vector3(3792,-6576,0), new Vector3(3920,-6576,0), new Vector3(4048,-6576,0), 
new Vector3(4176,-6576,0), new Vector3(4304,-6576,0), new Vector3(4432,-6576,0), new Vector3(4560,-6576,0), new Vector3(4688,-6576,0), new Vector3(4816,-6576,0), 
new Vector3(4944,-6576,0), new Vector3(5072,-6576,0), new Vector3(5200,-6576,0), new Vector3(5328,-6576,0), new Vector3(5456,-6576,0), new Vector3(5584,-6576,0), 
new Vector3(5712,-6576,0), new Vector3(5840,-6576,0), new Vector3(5968,-6576,0), new Vector3(6096,-6576,0), new Vector3(6224,-6576,0), new Vector3(6352,-6576,0), 
new Vector3(6480,-6576,0), new Vector3(6608,-6576,0), new Vector3(6736,-6576,0), new Vector3(6864,-6576,0), new Vector3(6992,-6576,0), new Vector3(7120,-6576,0), 
new Vector3(7248,-6576,0), new Vector3(7376,-6576,0), new Vector3(7504,-6576,0), new Vector3(7632,-6576,0), new Vector3(7760,-6576,0), new Vector3(7888,-6576,0), 
new Vector3(8016,-6576,0), new Vector3(8144,-6576,0), new Vector3(8272,-6576,0), new Vector3(8400,-6576,0), new Vector3(8528,-6576,0), new Vector3(8656,-6576,0), 
new Vector3(8784,-6576,0), new Vector3(8912,-6576,0), new Vector3(9040,-6576,0), new Vector3(9168,-6576,0), new Vector3(9296,-6576,0), new Vector3(9424,-6576,0), 
new Vector3(9552,-6576,0), new Vector3(9680,-6576,0), new Vector3(9808,-6576,0), new Vector3(9936,-6576,0), new Vector3(10064,-6576,0), new 
Vector3(10192,-6576,0), new Vector3(10320,-6576,0), new Vector3(10448,-6576,0), new Vector3(10576,-6576,0), new Vector3(10704,-6576,0), new 
Vector3(10832,-6576,0), new Vector3(10960,-6576,0), new Vector3(11088,-6576,0), new Vector3(11216,-6576,0), new Vector3(11344,-6576,0), new 
Vector3(11472,-6576,0), new Vector3(11600,-6576,0), new Vector3(11728,-6576,0), new Vector3(11856,-6576,0), new Vector3(11984,-6576,0), new 
Vector3(2000,-6704,0), new Vector3(2128,-6704,0), new Vector3(2256,-6704,0), new Vector3(2384,-6704,0), new Vector3(2512,-6704,0), new Vector3(2640,-6704,0), new 
Vector3(2768,-6704,0), new Vector3(2896,-6704,0), new Vector3(3024,-6704,0), new Vector3(3152,-6704,0), new Vector3(3280,-6704,0), new Vector3(3408,-6704,0), new 
Vector3(3536,-6704,0), new Vector3(3664,-6704,0), new Vector3(3792,-6704,0), new Vector3(3920,-6704,0), new Vector3(4048,-6704,0), new Vector3(4176,-6704,0), new 
Vector3(4304,-6704,0), new Vector3(4432,-6704,0), new Vector3(4560,-6704,0), new Vector3(4688,-6704,0), new Vector3(4816,-6704,0), new Vector3(4944,-6704,0), new 
Vector3(5072,-6704,0), new Vector3(5200,-6704,0), new Vector3(5328,-6704,0), new Vector3(5456,-6704,0), new Vector3(5584,-6704,0), new Vector3(5712,-6704,0), new 
Vector3(5840,-6704,0), new Vector3(5968,-6704,0), new Vector3(6096,-6704,0), new Vector3(6224,-6704,0), new Vector3(6352,-6704,0), new Vector3(6480,-6704,0), new 
Vector3(6608,-6704,0), new Vector3(6736,-6704,0), new Vector3(6864,-6704,0), new Vector3(6992,-6704,0), new Vector3(7120,-6704,0), new Vector3(7248,-6704,0), new 
Vector3(7376,-6704,0), new Vector3(7504,-6704,0), new Vector3(7632,-6704,0), new Vector3(7760,-6704,0), new Vector3(7888,-6704,0), new Vector3(8016,-6704,0), new 
Vector3(8144,-6704,0), new Vector3(8272,-6704,0), new Vector3(8400,-6704,0), new Vector3(8528,-6704,0), new Vector3(8656,-6704,0), new Vector3(8784,-6704,0), new 
Vector3(8912,-6704,0), new Vector3(9040,-6704,0), new Vector3(9168,-6704,0), new Vector3(9296,-6704,0), new Vector3(9424,-6704,0), new Vector3(9552,-6704,0), new 
Vector3(9680,-6704,0), new Vector3(9808,-6704,0), new Vector3(9936,-6704,0), new Vector3(10064,-6704,0), new Vector3(10192,-6704,0), new Vector3(10320,-6704,0), 
new Vector3(10448,-6704,0), new Vector3(10576,-6704,0), new Vector3(10704,-6704,0), new Vector3(10832,-6704,0), new Vector3(10960,-6704,0), new 
Vector3(11088,-6704,0), new Vector3(11216,-6704,0), new Vector3(11344,-6704,0), new Vector3(11472,-6704,0), new Vector3(11600,-6704,0), new 
Vector3(11728,-6704,0), new Vector3(11856,-6704,0), new Vector3(11984,-6704,0), new Vector3(2000,-6832,0), new Vector3(2128,-6832,0), new Vector3(2256,-6832,0), 
new Vector3(2384,-6832,0), new Vector3(2512,-6832,0), new Vector3(2640,-6832,0), new Vector3(2768,-6832,0), new Vector3(2896,-6832,0), new Vector3(3024,-6832,0), 
new Vector3(3152,-6832,0), new Vector3(3280,-6832,0), new Vector3(3408,-6832,0), new Vector3(3536,-6832,0), new Vector3(3664,-6832,0), new Vector3(3792,-6832,0), 
new Vector3(3920,-6832,0), new Vector3(4048,-6832,0), new Vector3(4176,-6832,0), new Vector3(4304,-6832,0), new Vector3(4432,-6832,0), new Vector3(4560,-6832,0), 
new Vector3(4688,-6832,0), new Vector3(4816,-6832,0), new Vector3(4944,-6832,0), new Vector3(5072,-6832,0), new Vector3(5200,-6832,0), new Vector3(5328,-6832,0), 
new Vector3(5456,-6832,0), new Vector3(5584,-6832,0), new Vector3(5712,-6832,0), new Vector3(5840,-6832,0), new Vector3(5968,-6832,0), new Vector3(6096,-6832,0), 
new Vector3(6224,-6832,0), new Vector3(6352,-6832,0), new Vector3(6480,-6832,0), new Vector3(6608,-6832,0), new Vector3(6736,-6832,0), new Vector3(6864,-6832,0), 
new Vector3(6992,-6832,0), new Vector3(7120,-6832,0), new Vector3(7248,-6832,0), new Vector3(7376,-6832,0), new Vector3(7504,-6832,0), new Vector3(7632,-6832,0), 
new Vector3(7760,-6832,0), new Vector3(7888,-6832,0), new Vector3(8016,-6832,0), new Vector3(8144,-6832,0), new Vector3(8272,-6832,0), new Vector3(8400,-6832,0), 
new Vector3(8528,-6832,0), new Vector3(8656,-6832,0), new Vector3(8784,-6832,0), new Vector3(8912,-6832,0), new Vector3(9040,-6832,0), new Vector3(9168,-6832,0), 
new Vector3(9296,-6832,0), new Vector3(9424,-6832,0), new Vector3(9552,-6832,0), new Vector3(9680,-6832,0), new Vector3(9808,-6832,0), new Vector3(9936,-6832,0), 
new Vector3(10064,-6832,0), new Vector3(10192,-6832,0), new Vector3(10320,-6832,0), new Vector3(10448,-6832,0), new Vector3(10576,-6832,0), new 
Vector3(10704,-6832,0), new Vector3(10832,-6832,0), new Vector3(10960,-6832,0), new Vector3(11088,-6832,0), new Vector3(11216,-6832,0), new 
Vector3(11344,-6832,0), new Vector3(11472,-6832,0), new Vector3(11600,-6832,0), new Vector3(11728,-6832,0), new Vector3(11856,-6832,0), new 
Vector3(11984,-6832,0), new Vector3(2000,-6960,0), new Vector3(2128,-6960,0), new Vector3(2256,-6960,0), new Vector3(2384,-6960,0), new Vector3(2512,-6960,0), 
new Vector3(2640,-6960,0), new Vector3(2768,-6960,0), new Vector3(2896,-6960,0), new Vector3(3024,-6960,0), new Vector3(3152,-6960,0), new Vector3(3280,-6960,0), 
new Vector3(3408,-6960,0), new Vector3(3536,-6960,0), new Vector3(3664,-6960,0), new Vector3(3792,-6960,0), new Vector3(3920,-6960,0), new Vector3(4048,-6960,0), 
new Vector3(4176,-6960,0), new Vector3(4304,-6960,0), new Vector3(4432,-6960,0), new Vector3(4560,-6960,0), new Vector3(4688,-6960,0), new Vector3(4816,-6960,0), 
new Vector3(4944,-6960,0), new Vector3(5072,-6960,0), new Vector3(5200,-6960,0), new Vector3(5328,-6960,0), new Vector3(5456,-6960,0), new Vector3(5584,-6960,0), 
new Vector3(5712,-6960,0), new Vector3(5840,-6960,0), new Vector3(5968,-6960,0), new Vector3(6096,-6960,0), new Vector3(6224,-6960,0), new Vector3(6352,-6960,0), 
new Vector3(6480,-6960,0), new Vector3(6608,-6960,0), new Vector3(6736,-6960,0), new Vector3(6864,-6960,0), new Vector3(6992,-6960,0), new Vector3(7120,-6960,0), 
new Vector3(7248,-6960,0), new Vector3(7376,-6960,0), new Vector3(7504,-6960,0), new Vector3(7632,-6960,0), new Vector3(7760,-6960,0), new Vector3(7888,-6960,0), 
new Vector3(8016,-6960,0), new Vector3(8144,-6960,0), new Vector3(8272,-6960,0), new Vector3(8400,-6960,0), new Vector3(8528,-6960,0), new Vector3(8656,-6960,0), 
new Vector3(8784,-6960,0), new Vector3(8912,-6960,0), new Vector3(9040,-6960,0), new Vector3(9168,-6960,0), new Vector3(9296,-6960,0), new Vector3(9424,-6960,0), 
new Vector3(9552,-6960,0), new Vector3(9680,-6960,0), new Vector3(9808,-6960,0), new Vector3(9936,-6960,0), new Vector3(10064,-6960,0), new 
Vector3(10192,-6960,0), new Vector3(10320,-6960,0), new Vector3(10448,-6960,0), new Vector3(10576,-6960,0), new Vector3(10704,-6960,0), new 
Vector3(10832,-6960,0), new Vector3(10960,-6960,0), new Vector3(11088,-6960,0), new Vector3(11216,-6960,0), new Vector3(11344,-6960,0), new 
Vector3(11472,-6960,0), new Vector3(11600,-6960,0), new Vector3(11728,-6960,0), new Vector3(11856,-6960,0), new Vector3(11984,-6960,0), new 
Vector3(2000,-7088,0), new Vector3(2128,-7088,0), new Vector3(2256,-7088,0), new Vector3(2384,-7088,0), new Vector3(2512,-7088,0), new Vector3(2640,-7088,0), new 
Vector3(2768,-7088,0), new Vector3(2896,-7088,0), new Vector3(3024,-7088,0), new Vector3(3152,-7088,0), new Vector3(3280,-7088,0), new Vector3(3408,-7088,0), new 
Vector3(3536,-7088,0), new Vector3(3664,-7088,0), new Vector3(3792,-7088,0), new Vector3(3920,-7088,0), new Vector3(4048,-7088,0), new Vector3(4176,-7088,0), new 
Vector3(4304,-7088,0), new Vector3(4432,-7088,0), new Vector3(4560,-7088,0), new Vector3(4688,-7088,0), new Vector3(4816,-7088,0), new Vector3(4944,-7088,0), new 
Vector3(5072,-7088,0), new Vector3(5200,-7088,0), new Vector3(5328,-7088,0), new Vector3(5456,-7088,0), new Vector3(5584,-7088,0), new Vector3(5712,-7088,0), new 
Vector3(5840,-7088,0), new Vector3(5968,-7088,0), new Vector3(6096,-7088,0), new Vector3(6224,-7088,0), new Vector3(6352,-7088,0), new Vector3(6480,-7088,0), new 
Vector3(6608,-7088,0), new Vector3(6736,-7088,0), new Vector3(6864,-7088,0), new Vector3(6992,-7088,0), new Vector3(7120,-7088,0), new Vector3(7248,-7088,0), new 
Vector3(7376,-7088,0), new Vector3(7504,-7088,0), new Vector3(7632,-7088,0), new Vector3(7760,-7088,0), new Vector3(7888,-7088,0), new Vector3(8016,-7088,0), new 
Vector3(8144,-7088,0), new Vector3(8272,-7088,0), new Vector3(8400,-7088,0), new Vector3(8528,-7088,0), new Vector3(8656,-7088,0), new Vector3(8784,-7088,0), new 
Vector3(8912,-7088,0), new Vector3(9040,-7088,0), new Vector3(9168,-7088,0), new Vector3(9296,-7088,0), new Vector3(9424,-7088,0), new Vector3(9552,-7088,0), new 
Vector3(9680,-7088,0), new Vector3(9808,-7088,0), new Vector3(9936,-7088,0), new Vector3(10064,-7088,0), new Vector3(10192,-7088,0), new Vector3(10320,-7088,0), 
new Vector3(10448,-7088,0), new Vector3(10576,-7088,0), new Vector3(10704,-7088,0), new Vector3(10832,-7088,0), new Vector3(10960,-7088,0), new 
Vector3(11088,-7088,0), new Vector3(11216,-7088,0), new Vector3(11344,-7088,0), new Vector3(11472,-7088,0), new Vector3(11600,-7088,0), new 
Vector3(11728,-7088,0), new Vector3(11856,-7088,0), new Vector3(11984,-7088,0), new Vector3(2000,-7216,0), new Vector3(2128,-7216,0), new Vector3(2256,-7216,0), 
new Vector3(2384,-7216,0), new Vector3(2512,-7216,0), new Vector3(2640,-7216,0), new Vector3(2768,-7216,0), new Vector3(2896,-7216,0), new Vector3(3024,-7216,0), 
new Vector3(3152,-7216,0), new Vector3(3280,-7216,0), new Vector3(3408,-7216,0), new Vector3(3536,-7216,0), new Vector3(3664,-7216,0), new Vector3(3792,-7216,0), 
new Vector3(3920,-7216,0), new Vector3(4048,-7216,0), new Vector3(4176,-7216,0), new Vector3(4304,-7216,0), new Vector3(4432,-7216,0), new Vector3(4560,-7216,0), 
new Vector3(4688,-7216,0), new Vector3(4816,-7216,0), new Vector3(4944,-7216,0), new Vector3(5072,-7216,0), new Vector3(5200,-7216,0), new Vector3(5328,-7216,0), 
new Vector3(5456,-7216,0), new Vector3(5584,-7216,0), new Vector3(5712,-7216,0), new Vector3(5840,-7216,0), new Vector3(5968,-7216,0), new Vector3(6096,-7216,0), 
new Vector3(6224,-7216,0), new Vector3(6352,-7216,0), new Vector3(6480,-7216,0), new Vector3(6608,-7216,0), new Vector3(6736,-7216,0), new Vector3(6864,-7216,0), 
new Vector3(6992,-7216,0), new Vector3(7120,-7216,0), new Vector3(7248,-7216,0), new Vector3(7376,-7216,0), new Vector3(7504,-7216,0), new Vector3(7632,-7216,0), 
new Vector3(7760,-7216,0), new Vector3(7888,-7216,0), new Vector3(8016,-7216,0), new Vector3(8144,-7216,0), new Vector3(8272,-7216,0), new Vector3(8400,-7216,0), 
new Vector3(8528,-7216,0), new Vector3(8656,-7216,0), new Vector3(8784,-7216,0), new Vector3(8912,-7216,0), new Vector3(9040,-7216,0), new Vector3(9168,-7216,0), 
new Vector3(9296,-7216,0), new Vector3(9424,-7216,0), new Vector3(9552,-7216,0), new Vector3(9680,-7216,0), new Vector3(9808,-7216,0), new Vector3(9936,-7216,0), 
new Vector3(10064,-7216,0), new Vector3(10192,-7216,0), new Vector3(10320,-7216,0), new Vector3(10448,-7216,0), new Vector3(10576,-7216,0), new 
Vector3(10704,-7216,0), new Vector3(10832,-7216,0), new Vector3(10960,-7216,0), new Vector3(11088,-7216,0), new Vector3(11216,-7216,0), new 
Vector3(11344,-7216,0), new Vector3(11472,-7216,0), new Vector3(11600,-7216,0), new Vector3(11728,-7216,0), new Vector3(11856,-7216,0), new 
Vector3(11984,-7216,0), new Vector3(2000,-7344,0), new Vector3(2128,-7344,0), new Vector3(2256,-7344,0), new Vector3(2384,-7344,0), new Vector3(2512,-7344,0), 
new Vector3(2640,-7344,0), new Vector3(2768,-7344,0), new Vector3(2896,-7344,0), new Vector3(3024,-7344,0), new Vector3(3152,-7344,0), new Vector3(3280,-7344,0), 
new Vector3(3408,-7344,0), new Vector3(3536,-7344,0), new Vector3(3664,-7344,0), new Vector3(3792,-7344,0), new Vector3(3920,-7344,0), new Vector3(4048,-7344,0), 
new Vector3(4176,-7344,0), new Vector3(4304,-7344,0), new Vector3(4432,-7344,0), new Vector3(4560,-7344,0), new Vector3(4688,-7344,0), new Vector3(4816,-7344,0), 
new Vector3(4944,-7344,0), new Vector3(5072,-7344,0), new Vector3(5200,-7344,0), new Vector3(5328,-7344,0), new Vector3(5456,-7344,0), new Vector3(5584,-7344,0), 
new Vector3(5712,-7344,0), new Vector3(5840,-7344,0), new Vector3(5968,-7344,0), new Vector3(6096,-7344,0), new Vector3(6224,-7344,0), new Vector3(6352,-7344,0), 
new Vector3(6480,-7344,0), new Vector3(6608,-7344,0), new Vector3(6736,-7344,0), new Vector3(6864,-7344,0), new Vector3(6992,-7344,0), new Vector3(7120,-7344,0), 
new Vector3(7248,-7344,0), new Vector3(7376,-7344,0), new Vector3(7504,-7344,0), new Vector3(7632,-7344,0), new Vector3(7760,-7344,0), new Vector3(7888,-7344,0), 
new Vector3(8016,-7344,0), new Vector3(8144,-7344,0), new Vector3(8272,-7344,0), new Vector3(8400,-7344,0), new Vector3(8528,-7344,0), new Vector3(8656,-7344,0), 
new Vector3(8784,-7344,0), new Vector3(8912,-7344,0), new Vector3(9040,-7344,0), new Vector3(9168,-7344,0), new Vector3(9296,-7344,0), new Vector3(9424,-7344,0), 
new Vector3(9552,-7344,0), new Vector3(9680,-7344,0), new Vector3(9808,-7344,0), new Vector3(9936,-7344,0), new Vector3(10064,-7344,0), new 
Vector3(10192,-7344,0), new Vector3(10320,-7344,0), new Vector3(10448,-7344,0), new Vector3(10576,-7344,0), new Vector3(10704,-7344,0), new 
Vector3(10832,-7344,0), new Vector3(10960,-7344,0), new Vector3(11088,-7344,0), new Vector3(11216,-7344,0), new Vector3(11344,-7344,0), new 
Vector3(11472,-7344,0), new Vector3(11600,-7344,0), new Vector3(11728,-7344,0), new Vector3(11856,-7344,0), new Vector3(11984,-7344,0), new 
Vector3(2000,-7472,0), new Vector3(2128,-7472,0), new Vector3(2256,-7472,0), new Vector3(2384,-7472,0), new Vector3(2512,-7472,0), new Vector3(2640,-7472,0), new 
Vector3(2768,-7472,0), new Vector3(2896,-7472,0), new Vector3(3024,-7472,0), new Vector3(3152,-7472,0), new Vector3(3280,-7472,0), new Vector3(3408,-7472,0), new 
Vector3(3536,-7472,0), new Vector3(3664,-7472,0), new Vector3(3792,-7472,0), new Vector3(3920,-7472,0), new Vector3(4048,-7472,0), new Vector3(4176,-7472,0), new 
Vector3(4304,-7472,0), new Vector3(4432,-7472,0), new Vector3(4560,-7472,0), new Vector3(4688,-7472,0), new Vector3(4816,-7472,0), new Vector3(4944,-7472,0), new 
Vector3(5072,-7472,0), new Vector3(5200,-7472,0), new Vector3(5328,-7472,0), new Vector3(5456,-7472,0), new Vector3(5584,-7472,0), new Vector3(5712,-7472,0), new 
Vector3(5840,-7472,0), new Vector3(5968,-7472,0), new Vector3(6096,-7472,0), new Vector3(6224,-7472,0), new Vector3(6352,-7472,0), new Vector3(6480,-7472,0), new 
Vector3(6608,-7472,0), new Vector3(6736,-7472,0), new Vector3(6864,-7472,0), new Vector3(6992,-7472,0), new Vector3(7120,-7472,0), new Vector3(7248,-7472,0), new 
Vector3(7376,-7472,0), new Vector3(7504,-7472,0), new Vector3(7632,-7472,0), new Vector3(7760,-7472,0), new Vector3(7888,-7472,0), new Vector3(8016,-7472,0), new 
Vector3(8144,-7472,0), new Vector3(8272,-7472,0), new Vector3(8400,-7472,0), new Vector3(8528,-7472,0), new Vector3(8656,-7472,0), new Vector3(8784,-7472,0), new 
Vector3(8912,-7472,0), new Vector3(9040,-7472,0), new Vector3(9168,-7472,0), new Vector3(9296,-7472,0), new Vector3(9424,-7472,0), new Vector3(9552,-7472,0), new 
Vector3(9680,-7472,0), new Vector3(9808,-7472,0), new Vector3(9936,-7472,0), new Vector3(10064,-7472,0), new Vector3(10192,-7472,0), new Vector3(10320,-7472,0), 
new Vector3(10448,-7472,0), new Vector3(10576,-7472,0), new Vector3(10704,-7472,0), new Vector3(10832,-7472,0), new Vector3(10960,-7472,0), new 
Vector3(11088,-7472,0), new Vector3(11216,-7472,0), new Vector3(11344,-7472,0), new Vector3(11472,-7472,0), new Vector3(11600,-7472,0), new 
Vector3(11728,-7472,0), new Vector3(11856,-7472,0), new Vector3(11984,-7472,0), new Vector3(2000,-7600,0), new Vector3(2128,-7600,0), new Vector3(2256,-7600,0), 
new Vector3(2384,-7600,0), new Vector3(2512,-7600,0), new Vector3(2640,-7600,0), new Vector3(2768,-7600,0), new Vector3(2896,-7600,0), new Vector3(3024,-7600,0), 
new Vector3(3152,-7600,0), new Vector3(3280,-7600,0), new Vector3(3408,-7600,0), new Vector3(3536,-7600,0), new Vector3(3664,-7600,0), new Vector3(3792,-7600,0), 
new Vector3(3920,-7600,0), new Vector3(4048,-7600,0), new Vector3(4176,-7600,0), new Vector3(4304,-7600,0), new Vector3(4432,-7600,0), new Vector3(4560,-7600,0), 
new Vector3(4688,-7600,0), new Vector3(4816,-7600,0), new Vector3(4944,-7600,0), new Vector3(5072,-7600,0), new Vector3(5200,-7600,0), new Vector3(5328,-7600,0), 
new Vector3(5456,-7600,0), new Vector3(5584,-7600,0), new Vector3(5712,-7600,0), new Vector3(5840,-7600,0), new Vector3(5968,-7600,0), new Vector3(6096,-7600,0), 
new Vector3(6224,-7600,0), new Vector3(6352,-7600,0), new Vector3(6480,-7600,0), new Vector3(6608,-7600,0), new Vector3(6736,-7600,0), new Vector3(6864,-7600,0), 
new Vector3(6992,-7600,0), new Vector3(7120,-7600,0), new Vector3(7248,-7600,0), new Vector3(7376,-7600,0), new Vector3(7504,-7600,0), new Vector3(7632,-7600,0), 
new Vector3(7760,-7600,0), new Vector3(7888,-7600,0), new Vector3(8016,-7600,0), new Vector3(8144,-7600,0), new Vector3(8272,-7600,0), new Vector3(8400,-7600,0), 
new Vector3(8528,-7600,0), new Vector3(8656,-7600,0), new Vector3(8784,-7600,0), new Vector3(8912,-7600,0), new Vector3(9040,-7600,0), new Vector3(9168,-7600,0), 
new Vector3(9296,-7600,0), new Vector3(9424,-7600,0), new Vector3(9552,-7600,0), new Vector3(9680,-7600,0), new Vector3(9808,-7600,0), new Vector3(9936,-7600,0), 
new Vector3(10064,-7600,0), new Vector3(10192,-7600,0), new Vector3(10320,-7600,0), new Vector3(10448,-7600,0), new Vector3(10576,-7600,0), new 
Vector3(10704,-7600,0), new Vector3(10832,-7600,0), new Vector3(10960,-7600,0), new Vector3(11088,-7600,0), new Vector3(11216,-7600,0), new 
Vector3(11344,-7600,0), new Vector3(11472,-7600,0), new Vector3(11600,-7600,0), new Vector3(11728,-7600,0), new Vector3(11856,-7600,0), new 
Vector3(11984,-7600,0), new Vector3(2000,-7728,0), new Vector3(2128,-7728,0), new Vector3(2256,-7728,0), new Vector3(2384,-7728,0), new Vector3(2512,-7728,0), 
new Vector3(2640,-7728,0), new Vector3(2768,-7728,0), new Vector3(2896,-7728,0), new Vector3(3024,-7728,0), new Vector3(3152,-7728,0), new Vector3(3280,-7728,0), 
new Vector3(3408,-7728,0), new Vector3(3536,-7728,0), new Vector3(3664,-7728,0), new Vector3(3792,-7728,0), new Vector3(3920,-7728,0), new Vector3(4048,-7728,0), 
new Vector3(4176,-7728,0), new Vector3(4304,-7728,0), new Vector3(4432,-7728,0), new Vector3(4560,-7728,0), new Vector3(4688,-7728,0), new Vector3(4816,-7728,0), 
new Vector3(4944,-7728,0), new Vector3(5072,-7728,0), new Vector3(5200,-7728,0), new Vector3(5328,-7728,0), new Vector3(5456,-7728,0), new Vector3(5584,-7728,0), 
new Vector3(5712,-7728,0), new Vector3(5840,-7728,0), new Vector3(5968,-7728,0), new Vector3(6096,-7728,0), new Vector3(6224,-7728,0), new Vector3(6352,-7728,0), 
new Vector3(6480,-7728,0), new Vector3(6608,-7728,0), new Vector3(6736,-7728,0), new Vector3(6864,-7728,0), new Vector3(6992,-7728,0), new Vector3(7120,-7728,0), 
new Vector3(7248,-7728,0), new Vector3(7376,-7728,0), new Vector3(7504,-7728,0), new Vector3(7632,-7728,0), new Vector3(7760,-7728,0), new Vector3(7888,-7728,0), 
new Vector3(8016,-7728,0), new Vector3(8144,-7728,0), new Vector3(8272,-7728,0), new Vector3(8400,-7728,0), new Vector3(8528,-7728,0), new Vector3(8656,-7728,0), 
new Vector3(8784,-7728,0), new Vector3(8912,-7728,0), new Vector3(9040,-7728,0), new Vector3(9168,-7728,0), new Vector3(9296,-7728,0), new Vector3(9424,-7728,0), 
new Vector3(9552,-7728,0), new Vector3(9680,-7728,0), new Vector3(9808,-7728,0), new Vector3(9936,-7728,0), new Vector3(10064,-7728,0), new 
Vector3(10192,-7728,0), new Vector3(10320,-7728,0), new Vector3(10448,-7728,0), new Vector3(10576,-7728,0), new Vector3(10704,-7728,0), new 
Vector3(10832,-7728,0), new Vector3(10960,-7728,0), new Vector3(11088,-7728,0), new Vector3(11216,-7728,0), new Vector3(11344,-7728,0), new 
Vector3(11472,-7728,0), new Vector3(11600,-7728,0), new Vector3(11728,-7728,0), new Vector3(11856,-7728,0), new Vector3(11984,-7728,0), new 
Vector3(2000,-7856,0), new Vector3(2128,-7856,0), new Vector3(2256,-7856,0), new Vector3(2384,-7856,0), new Vector3(2512,-7856,0), new Vector3(2640,-7856,0), new 
Vector3(2768,-7856,0), new Vector3(2896,-7856,0), new Vector3(3024,-7856,0), new Vector3(3152,-7856,0), new Vector3(3280,-7856,0), new Vector3(3408,-7856,0), new 
Vector3(3536,-7856,0), new Vector3(3664,-7856,0), new Vector3(3792,-7856,0), new Vector3(3920,-7856,0), new Vector3(4048,-7856,0), new Vector3(4176,-7856,0), new 
Vector3(4304,-7856,0), new Vector3(4432,-7856,0), new Vector3(4560,-7856,0), new Vector3(4688,-7856,0), new Vector3(4816,-7856,0), new Vector3(4944,-7856,0), new 
Vector3(5072,-7856,0), new Vector3(5200,-7856,0), new Vector3(5328,-7856,0), new Vector3(5456,-7856,0), new Vector3(5584,-7856,0), new Vector3(5712,-7856,0), new 
Vector3(5840,-7856,0), new Vector3(5968,-7856,0), new Vector3(6096,-7856,0), new Vector3(6224,-7856,0), new Vector3(6352,-7856,0), new Vector3(6480,-7856,0), new 
Vector3(6608,-7856,0), new Vector3(6736,-7856,0), new Vector3(6864,-7856,0), new Vector3(6992,-7856,0), new Vector3(7120,-7856,0), new Vector3(7248,-7856,0), new 
Vector3(7376,-7856,0), new Vector3(7504,-7856,0), new Vector3(7632,-7856,0), new Vector3(7760,-7856,0), new Vector3(7888,-7856,0), new Vector3(8016,-7856,0), new 
Vector3(8144,-7856,0), new Vector3(8272,-7856,0), new Vector3(8400,-7856,0), new Vector3(8528,-7856,0), new Vector3(8656,-7856,0), new Vector3(8784,-7856,0), new 
Vector3(8912,-7856,0), new Vector3(9040,-7856,0), new Vector3(9168,-7856,0), new Vector3(9296,-7856,0), new Vector3(9424,-7856,0), new Vector3(9552,-7856,0), new 
Vector3(9680,-7856,0), new Vector3(9808,-7856,0), new Vector3(9936,-7856,0), new Vector3(10064,-7856,0), new Vector3(10192,-7856,0), new Vector3(10320,-7856,0), 
new Vector3(10448,-7856,0), new Vector3(10576,-7856,0), new Vector3(10704,-7856,0), new Vector3(10832,-7856,0), new Vector3(10960,-7856,0), new 
Vector3(11088,-7856,0), new Vector3(11216,-7856,0), new Vector3(11344,-7856,0), new Vector3(11472,-7856,0), new Vector3(11600,-7856,0), new 
Vector3(11728,-7856,0), new Vector3(11856,-7856,0), new Vector3(11984,-7856,0), new Vector3(2000,-7984,0), new Vector3(2128,-7984,0), new Vector3(2256,-7984,0), 
new Vector3(2384,-7984,0), new Vector3(2512,-7984,0), new Vector3(2640,-7984,0), new Vector3(2768,-7984,0), new Vector3(2896,-7984,0), new Vector3(3024,-7984,0), 
new Vector3(3152,-7984,0), new Vector3(3280,-7984,0), new Vector3(3408,-7984,0), new Vector3(3536,-7984,0), new Vector3(3664,-7984,0), new Vector3(3792,-7984,0), 
new Vector3(3920,-7984,0), new Vector3(4048,-7984,0), new Vector3(4176,-7984,0), new Vector3(4304,-7984,0), new Vector3(4432,-7984,0), new Vector3(4560,-7984,0), 
new Vector3(4688,-7984,0), new Vector3(4816,-7984,0), new Vector3(4944,-7984,0), new Vector3(5072,-7984,0), new Vector3(5200,-7984,0), new Vector3(5328,-7984,0), 
new Vector3(5456,-7984,0), new Vector3(5584,-7984,0), new Vector3(5712,-7984,0), new Vector3(5840,-7984,0), new Vector3(5968,-7984,0), new Vector3(6096,-7984,0), 
new Vector3(6224,-7984,0), new Vector3(6352,-7984,0), new Vector3(6480,-7984,0), new Vector3(6608,-7984,0), new Vector3(6736,-7984,0), new Vector3(6864,-7984,0), 
new Vector3(6992,-7984,0), new Vector3(7120,-7984,0), new Vector3(7248,-7984,0), new Vector3(7376,-7984,0), new Vector3(7504,-7984,0), new Vector3(7632,-7984,0), 
new Vector3(7760,-7984,0), new Vector3(7888,-7984,0), new Vector3(8016,-7984,0), new Vector3(8144,-7984,0), new Vector3(8272,-7984,0), new Vector3(8400,-7984,0), 
new Vector3(8528,-7984,0), new Vector3(8656,-7984,0), new Vector3(8784,-7984,0), new Vector3(8912,-7984,0), new Vector3(9040,-7984,0), new Vector3(9168,-7984,0), 
new Vector3(9296,-7984,0), new Vector3(9424,-7984,0), new Vector3(9552,-7984,0), new Vector3(9680,-7984,0), new Vector3(9808,-7984,0), new Vector3(9936,-7984,0), 
new Vector3(10064,-7984,0), new Vector3(10192,-7984,0), new Vector3(10320,-7984,0), new Vector3(10448,-7984,0), new Vector3(10576,-7984,0), new 
Vector3(10704,-7984,0), new Vector3(10832,-7984,0), new Vector3(10960,-7984,0), new Vector3(11088,-7984,0), new Vector3(11216,-7984,0), new 
Vector3(11344,-7984,0), new Vector3(11472,-7984,0), new Vector3(11600,-7984,0), new Vector3(11728,-7984,0), new Vector3(11856,-7984,0), new 
Vector3(11984,-7984,0), new Vector3(2000,-8112,0), new Vector3(2128,-8112,0), new Vector3(2256,-8112,0), new Vector3(2384,-8112,0), new Vector3(2512,-8112,0), 
new Vector3(2640,-8112,0), new Vector3(2768,-8112,0), new Vector3(2896,-8112,0), new Vector3(3024,-8112,0), new Vector3(3152,-8112,0), new Vector3(3280,-8112,0), 
new Vector3(3408,-8112,0), new Vector3(3536,-8112,0), new Vector3(3664,-8112,0), new Vector3(3792,-8112,0), new Vector3(3920,-8112,0), new Vector3(4048,-8112,0), 
new Vector3(4176,-8112,0), new Vector3(4304,-8112,0), new Vector3(4432,-8112,0), new Vector3(4560,-8112,0), new Vector3(4688,-8112,0), new Vector3(4816,-8112,0), 
new Vector3(4944,-8112,0), new Vector3(5072,-8112,0), new Vector3(5200,-8112,0), new Vector3(5328,-8112,0), new Vector3(5456,-8112,0), new Vector3(5584,-8112,0), 
new Vector3(5712,-8112,0), new Vector3(5840,-8112,0), new Vector3(5968,-8112,0), new Vector3(6096,-8112,0), new Vector3(6224,-8112,0), new Vector3(6352,-8112,0), 
new Vector3(6480,-8112,0), new Vector3(6608,-8112,0), new Vector3(6736,-8112,0), new Vector3(6864,-8112,0), new Vector3(6992,-8112,0), new Vector3(7120,-8112,0), 
new Vector3(7248,-8112,0), new Vector3(7376,-8112,0), new Vector3(7504,-8112,0), new Vector3(7632,-8112,0), new Vector3(7760,-8112,0), new Vector3(7888,-8112,0), 
new Vector3(8016,-8112,0), new Vector3(8144,-8112,0), new Vector3(8272,-8112,0), new Vector3(8400,-8112,0), new Vector3(8528,-8112,0), new Vector3(8656,-8112,0), 
new Vector3(8784,-8112,0), new Vector3(8912,-8112,0), new Vector3(9040,-8112,0), new Vector3(9168,-8112,0), new Vector3(9296,-8112,0), new Vector3(9424,-8112,0), 
new Vector3(9552,-8112,0), new Vector3(9680,-8112,0), new Vector3(9808,-8112,0), new Vector3(9936,-8112,0), new Vector3(10064,-8112,0), new 
Vector3(10192,-8112,0), new Vector3(10320,-8112,0), new Vector3(10448,-8112,0), new Vector3(10576,-8112,0), new Vector3(10704,-8112,0), new 
Vector3(10832,-8112,0), new Vector3(10960,-8112,0), new Vector3(11088,-8112,0), new Vector3(11216,-8112,0), new Vector3(11344,-8112,0), new 
Vector3(11472,-8112,0), new Vector3(11600,-8112,0), new Vector3(11728,-8112,0), new Vector3(11856,-8112,0), new Vector3(11984,-8112,0), new 
Vector3(2000,-8240,0), new Vector3(2128,-8240,0), new Vector3(2256,-8240,0), new Vector3(2384,-8240,0), new Vector3(2512,-8240,0), new Vector3(2640,-8240,0), new 
Vector3(2768,-8240,0), new Vector3(2896,-8240,0), new Vector3(3024,-8240,0), new Vector3(3152,-8240,0), new Vector3(3280,-8240,0), new Vector3(3408,-8240,0), new 
Vector3(3536,-8240,0), new Vector3(3664,-8240,0), new Vector3(3792,-8240,0), new Vector3(3920,-8240,0), new Vector3(4048,-8240,0), new Vector3(4176,-8240,0), new 
Vector3(4304,-8240,0), new Vector3(4432,-8240,0), new Vector3(4560,-8240,0), new Vector3(4688,-8240,0), new Vector3(4816,-8240,0), new Vector3(4944,-8240,0), new 
Vector3(5072,-8240,0), new Vector3(5200,-8240,0), new Vector3(5328,-8240,0), new Vector3(5456,-8240,0), new Vector3(5584,-8240,0), new Vector3(5712,-8240,0), new 
Vector3(5840,-8240,0), new Vector3(5968,-8240,0), new Vector3(6096,-8240,0), new Vector3(6224,-8240,0), new Vector3(6352,-8240,0), new Vector3(6480,-8240,0), new 
Vector3(6608,-8240,0), new Vector3(6736,-8240,0), new Vector3(6864,-8240,0), new Vector3(6992,-8240,0), new Vector3(7120,-8240,0), new Vector3(7248,-8240,0), new 
Vector3(7376,-8240,0), new Vector3(7504,-8240,0), new Vector3(7632,-8240,0), new Vector3(7760,-8240,0), new Vector3(7888,-8240,0), new Vector3(8016,-8240,0), new 
Vector3(8144,-8240,0), new Vector3(8272,-8240,0), new Vector3(8400,-8240,0), new Vector3(8528,-8240,0), new Vector3(8656,-8240,0), new Vector3(8784,-8240,0), new 
Vector3(8912,-8240,0), new Vector3(9040,-8240,0), new Vector3(9168,-8240,0), new Vector3(9296,-8240,0), new Vector3(9424,-8240,0), new Vector3(9552,-8240,0), new 
Vector3(9680,-8240,0), new Vector3(9808,-8240,0), new Vector3(9936,-8240,0), new Vector3(10064,-8240,0), new Vector3(10192,-8240,0), new Vector3(10320,-8240,0), 
new Vector3(10448,-8240,0), new Vector3(10576,-8240,0), new Vector3(10704,-8240,0), new Vector3(10832,-8240,0), new Vector3(10960,-8240,0), new 
Vector3(11088,-8240,0), new Vector3(11216,-8240,0), new Vector3(11344,-8240,0), new Vector3(11472,-8240,0), new Vector3(11600,-8240,0), new 
Vector3(11728,-8240,0), new Vector3(11856,-8240,0), new Vector3(11984,-8240,0), new Vector3(2000,-8368,0), new Vector3(2128,-8368,0), new Vector3(2256,-8368,0), 
new Vector3(2384,-8368,0), new Vector3(2512,-8368,0), new Vector3(2640,-8368,0), new Vector3(2768,-8368,0), new Vector3(2896,-8368,0), new Vector3(3024,-8368,0), 
new Vector3(3152,-8368,0), new Vector3(3280,-8368,0), new Vector3(3408,-8368,0), new Vector3(3536,-8368,0), new Vector3(3664,-8368,0), new Vector3(3792,-8368,0), 
new Vector3(3920,-8368,0), new Vector3(4048,-8368,0), new Vector3(4176,-8368,0), new Vector3(4304,-8368,0), new Vector3(4432,-8368,0), new Vector3(4560,-8368,0), 
new Vector3(4688,-8368,0), new Vector3(4816,-8368,0), new Vector3(4944,-8368,0), new Vector3(5072,-8368,0), new Vector3(5200,-8368,0), new Vector3(5328,-8368,0), 
new Vector3(5456,-8368,0), new Vector3(5584,-8368,0), new Vector3(5712,-8368,0), new Vector3(5840,-8368,0), new Vector3(5968,-8368,0), new Vector3(6096,-8368,0), 
new Vector3(6224,-8368,0), new Vector3(6352,-8368,0), new Vector3(6480,-8368,0), new Vector3(6608,-8368,0), new Vector3(6736,-8368,0), new Vector3(6864,-8368,0), 
new Vector3(6992,-8368,0), new Vector3(7120,-8368,0), new Vector3(7248,-8368,0), new Vector3(7376,-8368,0), new Vector3(7504,-8368,0), new Vector3(7632,-8368,0), 
new Vector3(7760,-8368,0), new Vector3(7888,-8368,0), new Vector3(8016,-8368,0), new Vector3(8144,-8368,0), new Vector3(8272,-8368,0), new Vector3(8400,-8368,0), 
new Vector3(8528,-8368,0), new Vector3(8656,-8368,0), new Vector3(8784,-8368,0), new Vector3(8912,-8368,0), new Vector3(9040,-8368,0), new Vector3(9168,-8368,0), 
new Vector3(9296,-8368,0), new Vector3(9424,-8368,0), new Vector3(9552,-8368,0), new Vector3(9680,-8368,0), new Vector3(9808,-8368,0), new Vector3(9936,-8368,0), 
new Vector3(10064,-8368,0), new Vector3(10192,-8368,0), new Vector3(10320,-8368,0), new Vector3(10448,-8368,0), new Vector3(10576,-8368,0), new 
Vector3(10704,-8368,0), new Vector3(10832,-8368,0), new Vector3(10960,-8368,0), new Vector3(11088,-8368,0), new Vector3(11216,-8368,0), new 
Vector3(11344,-8368,0), new Vector3(11472,-8368,0), new Vector3(11600,-8368,0), new Vector3(11728,-8368,0), new Vector3(11856,-8368,0), new 
Vector3(11984,-8368,0), new Vector3(2000,-8496,0), new Vector3(2128,-8496,0), new Vector3(2256,-8496,0), new Vector3(2384,-8496,0), new Vector3(2512,-8496,0), 
new Vector3(2640,-8496,0), new Vector3(2768,-8496,0), new Vector3(2896,-8496,0), new Vector3(3024,-8496,0), new Vector3(3152,-8496,0), new Vector3(3280,-8496,0), 
new Vector3(3408,-8496,0), new Vector3(3536,-8496,0), new Vector3(3664,-8496,0), new Vector3(3792,-8496,0), new Vector3(3920,-8496,0), new Vector3(4048,-8496,0), 
new Vector3(4176,-8496,0), new Vector3(4304,-8496,0), new Vector3(4432,-8496,0), new Vector3(4560,-8496,0), new Vector3(4688,-8496,0), new Vector3(4816,-8496,0), 
new Vector3(4944,-8496,0), new Vector3(5072,-8496,0), new Vector3(5200,-8496,0), new Vector3(5328,-8496,0), new Vector3(5456,-8496,0), new Vector3(5584,-8496,0), 
new Vector3(5712,-8496,0), new Vector3(5840,-8496,0), new Vector3(5968,-8496,0), new Vector3(6096,-8496,0), new Vector3(6224,-8496,0), new Vector3(6352,-8496,0), 
new Vector3(6480,-8496,0), new Vector3(6608,-8496,0), new Vector3(6736,-8496,0), new Vector3(6864,-8496,0), new Vector3(6992,-8496,0), new Vector3(7120,-8496,0), 
new Vector3(7248,-8496,0), new Vector3(7376,-8496,0), new Vector3(7504,-8496,0), new Vector3(7632,-8496,0), new Vector3(7760,-8496,0), new Vector3(7888,-8496,0), 
new Vector3(8016,-8496,0), new Vector3(8144,-8496,0), new Vector3(8272,-8496,0), new Vector3(8400,-8496,0), new Vector3(8528,-8496,0), new Vector3(8656,-8496,0), 
new Vector3(8784,-8496,0), new Vector3(8912,-8496,0), new Vector3(9040,-8496,0), new Vector3(9168,-8496,0), new Vector3(9296,-8496,0), new Vector3(9424,-8496,0), 
new Vector3(9552,-8496,0), new Vector3(9680,-8496,0), new Vector3(9808,-8496,0), new Vector3(9936,-8496,0), new Vector3(10064,-8496,0), new 
Vector3(10192,-8496,0), new Vector3(10320,-8496,0), new Vector3(10448,-8496,0), new Vector3(10576,-8496,0), new Vector3(10704,-8496,0), new 
Vector3(10832,-8496,0), new Vector3(10960,-8496,0), new Vector3(11088,-8496,0), new Vector3(11216,-8496,0), new Vector3(11344,-8496,0), new 
Vector3(11472,-8496,0), new Vector3(11600,-8496,0), new Vector3(11728,-8496,0), new Vector3(11856,-8496,0), new Vector3(11984,-8496,0), new 
Vector3(2000,-8624,0), new Vector3(2128,-8624,0), new Vector3(2256,-8624,0), new Vector3(2384,-8624,0), new Vector3(2512,-8624,0), new Vector3(2640,-8624,0), new 
Vector3(2768,-8624,0), new Vector3(2896,-8624,0), new Vector3(3024,-8624,0), new Vector3(3152,-8624,0), new Vector3(3280,-8624,0), new Vector3(3408,-8624,0), new 
Vector3(3536,-8624,0), new Vector3(3664,-8624,0), new Vector3(3792,-8624,0), new Vector3(3920,-8624,0), new Vector3(4048,-8624,0), new Vector3(4176,-8624,0), new 
Vector3(4304,-8624,0), new Vector3(4432,-8624,0), new Vector3(4560,-8624,0), new Vector3(4688,-8624,0), new Vector3(4816,-8624,0), new Vector3(4944,-8624,0), new 
Vector3(5072,-8624,0), new Vector3(5200,-8624,0), new Vector3(5328,-8624,0), new Vector3(5456,-8624,0), new Vector3(5584,-8624,0), new Vector3(5712,-8624,0), new 
Vector3(5840,-8624,0), new Vector3(5968,-8624,0), new Vector3(6096,-8624,0), new Vector3(6224,-8624,0), new Vector3(6352,-8624,0), new Vector3(6480,-8624,0), new 
Vector3(6608,-8624,0), new Vector3(6736,-8624,0), new Vector3(6864,-8624,0), new Vector3(6992,-8624,0), new Vector3(7120,-8624,0), new Vector3(7248,-8624,0), new 
Vector3(7376,-8624,0), new Vector3(7504,-8624,0), new Vector3(7632,-8624,0), new Vector3(7760,-8624,0), new Vector3(7888,-8624,0), new Vector3(8016,-8624,0), new 
Vector3(8144,-8624,0), new Vector3(8272,-8624,0), new Vector3(8400,-8624,0), new Vector3(8528,-8624,0), new Vector3(8656,-8624,0), new Vector3(8784,-8624,0), new 
Vector3(8912,-8624,0), new Vector3(9040,-8624,0), new Vector3(9168,-8624,0), new Vector3(9296,-8624,0), new Vector3(9424,-8624,0), new Vector3(9552,-8624,0), new 
Vector3(9680,-8624,0), new Vector3(9808,-8624,0), new Vector3(9936,-8624,0), new Vector3(10064,-8624,0), new Vector3(10192,-8624,0), new Vector3(10320,-8624,0), 
new Vector3(10448,-8624,0), new Vector3(10576,-8624,0), new Vector3(10704,-8624,0), new Vector3(10832,-8624,0), new Vector3(10960,-8624,0), new 
Vector3(11088,-8624,0), new Vector3(11216,-8624,0), new Vector3(11344,-8624,0), new Vector3(11472,-8624,0), new Vector3(11600,-8624,0), new 
Vector3(11728,-8624,0), new Vector3(11856,-8624,0), new Vector3(11984,-8624,0), new Vector3(2000,-8752,0), new Vector3(2128,-8752,0), new Vector3(2256,-8752,0), 
new Vector3(2384,-8752,0), new Vector3(2512,-8752,0), new Vector3(2640,-8752,0), new Vector3(2768,-8752,0), new Vector3(2896,-8752,0), new Vector3(3024,-8752,0), 
new Vector3(3152,-8752,0), new Vector3(3280,-8752,0), new Vector3(3408,-8752,0), new Vector3(3536,-8752,0), new Vector3(3664,-8752,0), new Vector3(3792,-8752,0), 
new Vector3(3920,-8752,0), new Vector3(4048,-8752,0), new Vector3(4176,-8752,0), new Vector3(4304,-8752,0), new Vector3(4432,-8752,0), new Vector3(4560,-8752,0), 
new Vector3(4688,-8752,0), new Vector3(4816,-8752,0), new Vector3(4944,-8752,0), new Vector3(5072,-8752,0), new Vector3(5200,-8752,0), new Vector3(5328,-8752,0), 
new Vector3(5456,-8752,0), new Vector3(5584,-8752,0), new Vector3(5712,-8752,0), new Vector3(5840,-8752,0), new Vector3(5968,-8752,0), new Vector3(6096,-8752,0), 
new Vector3(6224,-8752,0), new Vector3(6352,-8752,0), new Vector3(6480,-8752,0), new Vector3(6608,-8752,0), new Vector3(6736,-8752,0), new Vector3(6864,-8752,0), 
new Vector3(6992,-8752,0), new Vector3(7120,-8752,0), new Vector3(7248,-8752,0), new Vector3(7376,-8752,0), new Vector3(7504,-8752,0), new Vector3(7632,-8752,0), 
new Vector3(7760,-8752,0), new Vector3(7888,-8752,0), new Vector3(8016,-8752,0), new Vector3(8144,-8752,0), new Vector3(8272,-8752,0), new Vector3(8400,-8752,0), 
new Vector3(8528,-8752,0), new Vector3(8656,-8752,0), new Vector3(8784,-8752,0), new Vector3(8912,-8752,0), new Vector3(9040,-8752,0), new Vector3(9168,-8752,0), 
new Vector3(9296,-8752,0), new Vector3(9424,-8752,0), new Vector3(9552,-8752,0), new Vector3(9680,-8752,0), new Vector3(9808,-8752,0), new Vector3(9936,-8752,0), 
new Vector3(10064,-8752,0), new Vector3(10192,-8752,0), new Vector3(10320,-8752,0), new Vector3(10448,-8752,0), new Vector3(10576,-8752,0), new 
Vector3(10704,-8752,0), new Vector3(10832,-8752,0), new Vector3(10960,-8752,0), new Vector3(11088,-8752,0), new Vector3(11216,-8752,0), new 
Vector3(11344,-8752,0), new Vector3(11472,-8752,0), new Vector3(11600,-8752,0), new Vector3(11728,-8752,0), new Vector3(11856,-8752,0), new 
Vector3(11984,-8752,0), new Vector3(2000,-8880,0), new Vector3(2128,-8880,0), new Vector3(2256,-8880,0), new Vector3(2384,-8880,0), new Vector3(2512,-8880,0), 
new Vector3(2640,-8880,0), new Vector3(2768,-8880,0), new Vector3(2896,-8880,0), new Vector3(3024,-8880,0), new Vector3(3152,-8880,0), new Vector3(3280,-8880,0), 
new Vector3(3408,-8880,0), new Vector3(3536,-8880,0), new Vector3(3664,-8880,0), new Vector3(3792,-8880,0), new Vector3(3920,-8880,0), new Vector3(4048,-8880,0), 
new Vector3(4176,-8880,0), new Vector3(4304,-8880,0), new Vector3(4432,-8880,0), new Vector3(4560,-8880,0), new Vector3(4688,-8880,0), new Vector3(4816,-8880,0), 
new Vector3(4944,-8880,0), new Vector3(5072,-8880,0), new Vector3(5200,-8880,0), new Vector3(5328,-8880,0), new Vector3(5456,-8880,0), new Vector3(5584,-8880,0), 
new Vector3(5712,-8880,0), new Vector3(5840,-8880,0), new Vector3(5968,-8880,0), new Vector3(6096,-8880,0), new Vector3(6224,-8880,0), new Vector3(6352,-8880,0), 
new Vector3(6480,-8880,0), new Vector3(6608,-8880,0), new Vector3(6736,-8880,0), new Vector3(6864,-8880,0), new Vector3(6992,-8880,0), new Vector3(7120,-8880,0), 
new Vector3(7248,-8880,0), new Vector3(7376,-8880,0), new Vector3(7504,-8880,0), new Vector3(7632,-8880,0), new Vector3(7760,-8880,0), new Vector3(7888,-8880,0), 
new Vector3(8016,-8880,0), new Vector3(8144,-8880,0), new Vector3(8272,-8880,0), new Vector3(8400,-8880,0), new Vector3(8528,-8880,0), new Vector3(8656,-8880,0), 
new Vector3(8784,-8880,0), new Vector3(8912,-8880,0), new Vector3(9040,-8880,0), new Vector3(9168,-8880,0), new Vector3(9296,-8880,0), new Vector3(9424,-8880,0), 
new Vector3(9552,-8880,0), new Vector3(9680,-8880,0), new Vector3(9808,-8880,0), new Vector3(9936,-8880,0), new Vector3(10064,-8880,0), new 
Vector3(10192,-8880,0), new Vector3(10320,-8880,0), new Vector3(10448,-8880,0), new Vector3(10576,-8880,0), new Vector3(10704,-8880,0), new 
Vector3(10832,-8880,0), new Vector3(10960,-8880,0), new Vector3(11088,-8880,0), new Vector3(11216,-8880,0), new Vector3(11344,-8880,0), new 
Vector3(11472,-8880,0), new Vector3(11600,-8880,0), new Vector3(11728,-8880,0), new Vector3(11856,-8880,0), new Vector3(11984,-8880,0), new 
Vector3(2000,-9008,0), new Vector3(2128,-9008,0), new Vector3(2256,-9008,0), new Vector3(2384,-9008,0), new Vector3(2512,-9008,0), new Vector3(2640,-9008,0), new 
Vector3(2768,-9008,0), new Vector3(2896,-9008,0), new Vector3(3024,-9008,0), new Vector3(3152,-9008,0), new Vector3(3280,-9008,0), new Vector3(3408,-9008,0), new 
Vector3(3536,-9008,0), new Vector3(3664,-9008,0), new Vector3(3792,-9008,0), new Vector3(3920,-9008,0), new Vector3(4048,-9008,0), new Vector3(4176,-9008,0), new 
Vector3(4304,-9008,0), new Vector3(4432,-9008,0), new Vector3(4560,-9008,0), new Vector3(4688,-9008,0), new Vector3(4816,-9008,0), new Vector3(4944,-9008,0), new 
Vector3(5072,-9008,0), new Vector3(5200,-9008,0), new Vector3(5328,-9008,0), new Vector3(5456,-9008,0), new Vector3(5584,-9008,0), new Vector3(5712,-9008,0), new 
Vector3(5840,-9008,0), new Vector3(5968,-9008,0), new Vector3(6096,-9008,0), new Vector3(6224,-9008,0), new Vector3(6352,-9008,0), new Vector3(6480,-9008,0), new 
Vector3(6608,-9008,0), new Vector3(6736,-9008,0), new Vector3(6864,-9008,0), new Vector3(6992,-9008,0), new Vector3(7120,-9008,0), new Vector3(7248,-9008,0), new 
Vector3(7376,-9008,0), new Vector3(7504,-9008,0), new Vector3(7632,-9008,0), new Vector3(7760,-9008,0), new Vector3(7888,-9008,0), new Vector3(8016,-9008,0), new 
Vector3(8144,-9008,0), new Vector3(8272,-9008,0), new Vector3(8400,-9008,0), new Vector3(8528,-9008,0), new Vector3(8656,-9008,0), new Vector3(8784,-9008,0), new 
Vector3(8912,-9008,0), new Vector3(9040,-9008,0), new Vector3(9168,-9008,0), new Vector3(9296,-9008,0), new Vector3(9424,-9008,0), new Vector3(9552,-9008,0), new 
Vector3(9680,-9008,0), new Vector3(9808,-9008,0), new Vector3(9936,-9008,0), new Vector3(10064,-9008,0), new Vector3(10192,-9008,0), new Vector3(10320,-9008,0), 
new Vector3(10448,-9008,0), new Vector3(10576,-9008,0), new Vector3(10704,-9008,0), new Vector3(10832,-9008,0), new Vector3(10960,-9008,0), new 
Vector3(11088,-9008,0), new Vector3(11216,-9008,0), new Vector3(11344,-9008,0), new Vector3(11472,-9008,0), new Vector3(11600,-9008,0), new 
Vector3(11728,-9008,0), new Vector3(11856,-9008,0), new Vector3(11984,-9008,0), new Vector3(2000,-9136,0), new Vector3(2128,-9136,0), new Vector3(2256,-9136,0), 
new Vector3(2384,-9136,0), new Vector3(2512,-9136,0), new Vector3(2640,-9136,0), new Vector3(2768,-9136,0), new Vector3(2896,-9136,0), new Vector3(3024,-9136,0), 
new Vector3(3152,-9136,0), new Vector3(3280,-9136,0), new Vector3(3408,-9136,0), new Vector3(3536,-9136,0), new Vector3(3664,-9136,0), new Vector3(3792,-9136,0), 
new Vector3(3920,-9136,0), new Vector3(4048,-9136,0), new Vector3(4176,-9136,0), new Vector3(4304,-9136,0), new Vector3(4432,-9136,0), new Vector3(4560,-9136,0), 
new Vector3(4688,-9136,0), new Vector3(4816,-9136,0), new Vector3(4944,-9136,0), new Vector3(5072,-9136,0), new Vector3(5200,-9136,0), new Vector3(5328,-9136,0), 
new Vector3(5456,-9136,0), new Vector3(5584,-9136,0), new Vector3(5712,-9136,0), new Vector3(5840,-9136,0), new Vector3(5968,-9136,0), new Vector3(6096,-9136,0), 
new Vector3(6224,-9136,0), new Vector3(6352,-9136,0), new Vector3(6480,-9136,0), new Vector3(6608,-9136,0), new Vector3(6736,-9136,0), new Vector3(6864,-9136,0), 
new Vector3(6992,-9136,0), new Vector3(7120,-9136,0), new Vector3(7248,-9136,0), new Vector3(7376,-9136,0), new Vector3(7504,-9136,0), new Vector3(7632,-9136,0), 
new Vector3(7760,-9136,0), new Vector3(7888,-9136,0), new Vector3(8016,-9136,0), new Vector3(8144,-9136,0), new Vector3(8272,-9136,0), new Vector3(8400,-9136,0), 
new Vector3(8528,-9136,0), new Vector3(8656,-9136,0), new Vector3(8784,-9136,0), new Vector3(8912,-9136,0), new Vector3(9040,-9136,0), new Vector3(9168,-9136,0), 
new Vector3(9296,-9136,0), new Vector3(9424,-9136,0), new Vector3(9552,-9136,0), new Vector3(9680,-9136,0), new Vector3(9808,-9136,0), new Vector3(9936,-9136,0), 
new Vector3(10064,-9136,0), new Vector3(10192,-9136,0), new Vector3(10320,-9136,0), new Vector3(10448,-9136,0), new Vector3(10576,-9136,0), new 
Vector3(10704,-9136,0), new Vector3(10832,-9136,0), new Vector3(10960,-9136,0), new Vector3(11088,-9136,0), new Vector3(11216,-9136,0), new 
Vector3(11344,-9136,0), new Vector3(11472,-9136,0), new Vector3(11600,-9136,0), new Vector3(11728,-9136,0), new Vector3(11856,-9136,0), new 
Vector3(11984,-9136,0), new Vector3(2000,-9264,0), new Vector3(2128,-9264,0), new Vector3(2256,-9264,0), new Vector3(2384,-9264,0), new Vector3(2512,-9264,0), 
new Vector3(2640,-9264,0), new Vector3(2768,-9264,0), new Vector3(2896,-9264,0), new Vector3(3024,-9264,0), new Vector3(3152,-9264,0), new Vector3(3280,-9264,0), 
new Vector3(3408,-9264,0), new Vector3(3536,-9264,0), new Vector3(3664,-9264,0), new Vector3(3792,-9264,0), new Vector3(3920,-9264,0), new Vector3(4048,-9264,0), 
new Vector3(4176,-9264,0), new Vector3(4304,-9264,0), new Vector3(4432,-9264,0), new Vector3(4560,-9264,0), new Vector3(4688,-9264,0), new Vector3(4816,-9264,0), 
new Vector3(4944,-9264,0)}; 
}
