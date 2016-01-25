using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
                LevelMap.transform.GetChild(level_index).GetComponent<RectTransform>().anchoredPosition = level_position[level_index];
                LevelMap.transform.GetChild(level_index).GetComponent<LevelStructureV2>().enabled = true;
                LevelMap.transform.GetChild(level_index).GetComponent<LevelStructureV2>().init(level_name, level_position[level_index]);
                level_index++;
            }
        }
        /*
        foreach (Transform child in LevelMap.transform)
    {
        child.GetComponent<RectTransform>().anchoredPosition = level_position[level_index];
        child.GetComponent<LevelStructureV2>().init(dataController.level_index[level_index], level_position[level_index]);

    }*/
    yield return null;
    /*foreach (string level_name in dataController.level_index)
    {
        if (level_name != "")
        {
            LevelPrefabReference = Instantiate<GameObject>(LevelPrefab);
            LevelPrefabReference.transform.SetParent(transform, false);
            //LevelPrefabReference.GetComponent<RectTransform>().anchoredPosition = level_position[level_index];
            //put the name
            LevelPrefabReference.GetComponentInChildren<Text>().text = level_name;
            LevelPrefabReference.GetComponent<LevelStructure>().level_name = level_name;
            LevelPrefabReference.GetComponent<LevelStructure>().level_id = level_index;
            //set the coordinates of it
           // LevelPrefabReference.GetComponent<LevelStructure>().coordinates = level_position[level_index];
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
    }*/
    }

    public void DescriptionToMap()
    {
        LevelDescriptionCanvas.SetActive(false);
        LevelSelectionCanvas.SetActive(true);
    }

    Vector3[] level_position = new[] { new Vector3(-4000, 2000, 0), new Vector3(-3850, 2000, 0), new Vector3(-3700, 2000, 0), new Vector3(-3550, 2000, 0), new Vector3(-3400, 2000, 0), new Vector3(-3250, 2000, 0), new Vector3(-3100, 2000, 0), new Vector3(-2950, 2000, 0), new Vector3(-2800, 2000, 0), new Vector3(-2650, 2000, 0), new Vector3(-2500, 2000, 0), new Vector3(-2350, 2000, 0), new Vector3(-2200, 2000, 0), new Vector3(-2050, 2000, 0), new Vector3(-1900, 2000, 0), new Vector3(-1750, 2000, 0), new Vector3(-1600, 2000, 0), new Vector3(-1450, 2000, 0), new Vector3(-1300, 2000, 0), new Vector3(-1150, 2000, 0), new Vector3(-1000, 2000, 0), new Vector3(-850, 2000, 0), new Vector3(-700, 2000, 0), new Vector3(-550, 2000, 0), new Vector3(-400, 2000, 0), new Vector3(-250, 2000, 0), new Vector3(-100, 2000, 0), new Vector3(50, 2000, 0), new Vector3(200, 2000, 0), new Vector3(350, 2000, 0), new Vector3(500, 2000, 0), new Vector3(650, 2000, 0), new Vector3(800, 2000, 0), new Vector3(950, 2000, 0), new Vector3(1100, 2000, 0), new Vector3(1250, 2000, 0), new Vector3(1400, 2000, 0), new Vector3(1550, 2000, 0), new Vector3(1700, 2000, 0), new Vector3(1850, 2000, 0), new Vector3(2000, 2000, 0), new Vector3(2150, 2000, 0), new Vector3(2300, 2000, 0), new Vector3(2450, 2000, 0), new Vector3(2600, 2000, 0), new Vector3(2750, 2000, 0), new Vector3(2900, 2000, 0), new Vector3(3050, 2000, 0), new Vector3(3200, 2000, 0), new Vector3(3350, 2000, 0), new Vector3(3500, 2000, 0), new Vector3(3650, 2000, 0), new Vector3(3800, 2000, 0), new Vector3(3950, 2000, 0), new Vector3(4100, 2000, 0), new Vector3(4250, 2000, 0), new Vector3(4400, 2000, 0), new Vector3(4550, 2000, 0), new Vector3(4700, 2000, 0), new Vector3(4850, 2000, 0), new Vector3(5000, 2000, 0), new Vector3(5150, 2000, 0), new Vector3(5300, 2000, 0), new Vector3(5450, 2000, 0), new Vector3(5600, 2000, 0), new Vector3(5750, 2000, 0), new Vector3(5900, 2000, 0), new Vector3(6050, 2000, 0), new Vector3(6200, 2000, 0), new Vector3(6350, 2000, 0), new Vector3(6500, 2000, 0), new Vector3(6650, 2000, 0), new Vector3(6800, 2000, 0), new Vector3(6950, 2000, 0), new Vector3(-4000, 1850, 0), new Vector3(-3850, 1850, 0), new Vector3(-3700, 1850, 0), new Vector3(-3550, 1850, 0), new Vector3(-3400, 1850, 0), new Vector3(-3250, 1850, 0), new Vector3(-3100, 1850, 0), new Vector3(-2950, 1850, 0), new Vector3(-2800, 1850, 0), new Vector3(-2650, 1850, 0), new Vector3(-2500, 1850, 0), new Vector3(-2350, 1850, 0), new Vector3(-2200, 1850, 0), new Vector3(-2050, 1850, 0), new Vector3(-1900, 1850, 0), new Vector3(-1750, 1850, 0), new Vector3(-1600, 1850, 0), new Vector3(-1450, 1850, 0), new Vector3(-1300, 1850, 0), new Vector3(-1150, 1850, 0), new Vector3(-1000, 1850, 0), new Vector3(-850, 1850, 0), new Vector3(-700, 1850, 0), new Vector3(-550, 1850, 0), new Vector3(-400, 1850, 0), new Vector3(-250, 1850, 0), new Vector3(-100, 1850, 0), new Vector3(50, 1850, 0), new Vector3(200, 1850, 0), new Vector3(350, 1850, 0), new Vector3(500, 1850, 0), new Vector3(650, 1850, 0), new Vector3(800, 1850, 0), new Vector3(950, 1850, 0), new Vector3(1100, 1850, 0), new Vector3(1250, 1850, 0), new Vector3(1400, 1850, 0), new Vector3(1550, 1850, 0), new Vector3(1700, 1850, 0), new Vector3(1850, 1850, 0), new Vector3(2000, 1850, 0), new Vector3(2150, 1850, 0), new Vector3(2300, 1850, 0), new Vector3(2450, 1850, 0), new Vector3(2600, 1850, 0), new Vector3(2750, 1850, 0), new Vector3(2900, 1850, 0), new Vector3(3050, 1850, 0), new Vector3(3200, 1850, 0), new Vector3(3350, 1850, 0), new Vector3(3500, 1850, 0), new Vector3(3650, 1850, 0), new Vector3(3800, 1850, 0), new Vector3(3950, 1850, 0), new Vector3(4100, 1850, 0), new Vector3(4250, 1850, 0), new Vector3(4400, 1850, 0), new Vector3(4550, 1850, 0), new Vector3(4700, 1850, 0), new Vector3(4850, 1850, 0), new Vector3(5000, 1850, 0), new Vector3(5150, 1850, 0), new Vector3(5300, 1850, 0), new Vector3(5450, 1850, 0), new Vector3(5600, 1850, 0), new Vector3(5750, 1850, 0), new Vector3(5900, 1850, 0), new Vector3(6050, 1850, 0), new Vector3(6200, 1850, 0), new Vector3(6350, 1850, 0), new Vector3(6500, 1850, 0), new Vector3(6650, 1850, 0), new Vector3(6800, 1850, 0), new Vector3(6950, 1850, 0), new Vector3(-4000, 1700, 0), new Vector3(-3850, 1700, 0), new Vector3(-3700, 1700, 0), new Vector3(-3550, 1700, 0), new Vector3(-3400, 1700, 0), new Vector3(-3250, 1700, 0), new Vector3(-3100, 1700, 0), new Vector3(-2950, 1700, 0), new Vector3(-2800, 1700, 0), new Vector3(-2650, 1700, 0), new Vector3(-2500, 1700, 0), new Vector3(-2350, 1700, 0), new Vector3(-2200, 1700, 0), new Vector3(-2050, 1700, 0), new Vector3(-1900, 1700, 0), new Vector3(-1750, 1700, 0), new Vector3(-1600, 1700, 0), new Vector3(-1450, 1700, 0), new Vector3(-1300, 1700, 0), new Vector3(-1150, 1700, 0), new Vector3(-1000, 1700, 0), new Vector3(-850, 1700, 0), new Vector3(-700, 1700, 0), new Vector3(-550, 1700, 0), new Vector3(-400, 1700, 0), new Vector3(-250, 1700, 0), new Vector3(-100, 1700, 0), new Vector3(50, 1700, 0), new Vector3(200, 1700, 0), new Vector3(350, 1700, 0), new Vector3(500, 1700, 0), new Vector3(650, 1700, 0), new Vector3(800, 1700, 0), new Vector3(950, 1700, 0), new Vector3(1100, 1700, 0), new Vector3(1250, 1700, 0), new Vector3(1400, 1700, 0), new Vector3(1550, 1700, 0), new Vector3(1700, 1700, 0), new Vector3(1850, 1700, 0), new Vector3(2000, 1700, 0), new Vector3(2150, 1700, 0), new Vector3(2300, 1700, 0), new Vector3(2450, 1700, 0), new Vector3(2600, 1700, 0), new Vector3(2750, 1700, 0), new Vector3(2900, 1700, 0), new Vector3(3050, 1700, 0), new Vector3(3200, 1700, 0), new Vector3(3350, 1700, 0), new Vector3(3500, 1700, 0), new Vector3(3650, 1700, 0), new Vector3(3800, 1700, 0), new Vector3(3950, 1700, 0), new Vector3(4100, 1700, 0), new Vector3(4250, 1700, 0), new Vector3(4400, 1700, 0), new Vector3(4550, 1700, 0), new Vector3(4700, 1700, 0), new Vector3(4850, 1700, 0), new Vector3(5000, 1700, 0), new Vector3(5150, 1700, 0), new Vector3(5300, 1700, 0), new Vector3(5450, 1700, 0), new Vector3(5600, 1700, 0), new Vector3(5750, 1700, 0), new Vector3(5900, 1700, 0), new Vector3(6050, 1700, 0), new Vector3(6200, 1700, 0), new Vector3(6350, 1700, 0), new Vector3(6500, 1700, 0), new Vector3(6650, 1700, 0), new Vector3(6800, 1700, 0), new Vector3(6950, 1700, 0), new Vector3(-4000, 1550, 0), new Vector3(-3850, 1550, 0), new Vector3(-3700, 1550, 0), new Vector3(-3550, 1550, 0), new Vector3(-3400, 1550, 0), new Vector3(-3250, 1550, 0), new Vector3(-3100, 1550, 0), new Vector3(-2950, 1550, 0), new Vector3(-2800, 1550, 0), new Vector3(-2650, 1550, 0), new Vector3(-2500, 1550, 0), new Vector3(-2350, 1550, 0), new Vector3(-2200, 1550, 0), new Vector3(-2050, 1550, 0), new Vector3(-1900, 1550, 0), new Vector3(-1750, 1550, 0), new Vector3(-1600, 1550, 0), new Vector3(-1450, 1550, 0), new Vector3(-1300, 1550, 0), new Vector3(-1150, 1550, 0), new Vector3(-1000, 1550, 0), new Vector3(-850, 1550, 0), new Vector3(-700, 1550, 0), new Vector3(-550, 1550, 0), new Vector3(-400, 1550, 0), new Vector3(-250, 1550, 0), new Vector3(-100, 1550, 0), new Vector3(50, 1550, 0), new Vector3(200, 1550, 0), new Vector3(350, 1550, 0), new Vector3(500, 1550, 0), new Vector3(650, 1550, 0), new Vector3(800, 1550, 0), new Vector3(950, 1550, 0), new Vector3(1100, 1550, 0), new Vector3(1250, 1550, 0), new Vector3(1400, 1550, 0), new Vector3(1550, 1550, 0), new Vector3(1700, 1550, 0), new Vector3(1850, 1550, 0), new Vector3(2000, 1550, 0), new Vector3(2150, 1550, 0), new Vector3(2300, 1550, 0), new Vector3(2450, 1550, 0), new Vector3(2600, 1550, 0), new Vector3(2750, 1550, 0), new Vector3(2900, 1550, 0), new Vector3(3050, 1550, 0), new Vector3(3200, 1550, 0), new Vector3(3350, 1550, 0), new Vector3(3500, 1550, 0), new Vector3(3650, 1550, 0), new Vector3(3800, 1550, 0), new Vector3(3950, 1550, 0), new Vector3(4100, 1550, 0), new Vector3(4250, 1550, 0), new Vector3(4400, 1550, 0), new Vector3(4550, 1550, 0), new Vector3(4700, 1550, 0), new Vector3(4850, 1550, 0), new Vector3(5000, 1550, 0), new Vector3(5150, 1550, 0), new Vector3(5300, 1550, 0), new Vector3(5450, 1550, 0), new Vector3(5600, 1550, 0), new Vector3(5750, 1550, 0), new Vector3(5900, 1550, 0), new Vector3(6050, 1550, 0), new Vector3(6200, 1550, 0), new Vector3(6350, 1550, 0), new Vector3(6500, 1550, 0), new Vector3(6650, 1550, 0), new Vector3(6800, 1550, 0), new Vector3(6950, 1550, 0), new Vector3(-4000, 1400, 0), new Vector3(-3850, 1400, 0), new Vector3(-3700, 1400, 0), new Vector3(-3550, 1400, 0), new Vector3(-3400, 1400, 0), new Vector3(-3250, 1400, 0), new Vector3(-3100, 1400, 0), new Vector3(-2950, 1400, 0), new Vector3(-2800, 1400, 0), new Vector3(-2650, 1400, 0), new Vector3(-2500, 1400, 0), new Vector3(-2350, 1400, 0), new Vector3(-2200, 1400, 0), new Vector3(-2050, 1400, 0), new Vector3(-1900, 1400, 0), new Vector3(-1750, 1400, 0), new Vector3(-1600, 1400, 0), new Vector3(-1450, 1400, 0), new Vector3(-1300, 1400, 0), new Vector3(-1150, 1400, 0), new Vector3(-1000, 1400, 0), new Vector3(-850, 1400, 0), new Vector3(-700, 1400, 0), new Vector3(-550, 1400, 0), new Vector3(-400, 1400, 0), new Vector3(-250, 1400, 0), new Vector3(-100, 1400, 0), new Vector3(50, 1400, 0), new Vector3(200, 1400, 0), new Vector3(350, 1400, 0), new Vector3(500, 1400, 0), new Vector3(650, 1400, 0), new Vector3(800, 1400, 0), new Vector3(950, 1400, 0), new Vector3(1100, 1400, 0), new Vector3(1250, 1400, 0), new Vector3(1400, 1400, 0), new Vector3(1550, 1400, 0), new Vector3(1700, 1400, 0), new Vector3(1850, 1400, 0), new Vector3(2000, 1400, 0), new Vector3(2150, 1400, 0), new Vector3(2300, 1400, 0), new Vector3(2450, 1400, 0), new Vector3(2600, 1400, 0), new Vector3(2750, 1400, 0), new Vector3(2900, 1400, 0), new Vector3(3050, 1400, 0), new Vector3(3200, 1400, 0), new Vector3(3350, 1400, 0), new Vector3(3500, 1400, 0), new Vector3(3650, 1400, 0), new Vector3(3800, 1400, 0), new Vector3(3950, 1400, 0), new Vector3(4100, 1400, 0), new Vector3(4250, 1400, 0), new Vector3(4400, 1400, 0), new Vector3(4550, 1400, 0), new Vector3(4700, 1400, 0), new Vector3(4850, 1400, 0), new Vector3(5000, 1400, 0), new Vector3(5150, 1400, 0), new Vector3(5300, 1400, 0), new Vector3(5450, 1400, 0), new Vector3(5600, 1400, 0), new Vector3(5750, 1400, 0), new Vector3(5900, 1400, 0), new Vector3(6050, 1400, 0), new Vector3(6200, 1400, 0), new Vector3(6350, 1400, 0), new Vector3(6500, 1400, 0), new Vector3(6650, 1400, 0), new Vector3(6800, 1400, 0), new Vector3(6950, 1400, 0), new Vector3(-4000, 1250, 0), new Vector3(-3850, 1250, 0), new Vector3(-3700, 1250, 0), new Vector3(-3550, 1250, 0), new Vector3(-3400, 1250, 0), new Vector3(-3250, 1250, 0), new Vector3(-3100, 1250, 0), new Vector3(-2950, 1250, 0), new Vector3(-2800, 1250, 0), new Vector3(-2650, 1250, 0), new Vector3(-2500, 1250, 0), new Vector3(-2350, 1250, 0), new Vector3(-2200, 1250, 0), new Vector3(-2050, 1250, 0), new Vector3(-1900, 1250, 0), new Vector3(-1750, 1250, 0), new Vector3(-1600, 1250, 0), new Vector3(-1450, 1250, 0), new Vector3(-1300, 1250, 0), new Vector3(-1150, 1250, 0), new Vector3(-1000, 1250, 0), new Vector3(-850, 1250, 0), new Vector3(-700, 1250, 0), new Vector3(-550, 1250, 0), new Vector3(-400, 1250, 0), new Vector3(-250, 1250, 0), new Vector3(-100, 1250, 0), new Vector3(50, 1250, 0), new Vector3(200, 1250, 0), new Vector3(350, 1250, 0), new Vector3(500, 1250, 0), new Vector3(650, 1250, 0), new Vector3(800, 1250, 0), new Vector3(950, 1250, 0), new Vector3(1100, 1250, 0), new Vector3(1250, 1250, 0), new Vector3(1400, 1250, 0), new Vector3(1550, 1250, 0), new Vector3(1700, 1250, 0), new Vector3(1850, 1250, 0), new Vector3(2000, 1250, 0), new Vector3(2150, 1250, 0), new Vector3(2300, 1250, 0), new Vector3(2450, 1250, 0), new Vector3(2600, 1250, 0), new Vector3(2750, 1250, 0), new Vector3(2900, 1250, 0), new Vector3(3050, 1250, 0), new Vector3(3200, 1250, 0), new Vector3(3350, 1250, 0), new Vector3(3500, 1250, 0), new Vector3(3650, 1250, 0), new Vector3(3800, 1250, 0), new Vector3(3950, 1250, 0), new Vector3(4100, 1250, 0), new Vector3(4250, 1250, 0), new Vector3(4400, 1250, 0), new Vector3(4550, 1250, 0), new Vector3(4700, 1250, 0), new Vector3(4850, 1250, 0), new Vector3(5000, 1250, 0), new Vector3(5150, 1250, 0), new Vector3(5300, 1250, 0), new Vector3(5450, 1250, 0), new Vector3(5600, 1250, 0), new Vector3(5750, 1250, 0), new Vector3(5900, 1250, 0), new Vector3(6050, 1250, 0), new Vector3(6200, 1250, 0), new Vector3(6350, 1250, 0), new Vector3(6500, 1250, 0), new Vector3(6650, 1250, 0), new Vector3(6800, 1250, 0), new Vector3(6950, 1250, 0), new Vector3(-4000, 1100, 0), new Vector3(-3850, 1100, 0), new Vector3(-3700, 1100, 0), new Vector3(-3550, 1100, 0), new Vector3(-3400, 1100, 0), new Vector3(-3250, 1100, 0), new Vector3(-3100, 1100, 0), new Vector3(-2950, 1100, 0), new Vector3(-2800, 1100, 0), new Vector3(-2650, 1100, 0), new Vector3(-2500, 1100, 0), new Vector3(-2350, 1100, 0), new Vector3(-2200, 1100, 0), new Vector3(-2050, 1100, 0), new Vector3(-1900, 1100, 0), new Vector3(-1750, 1100, 0), new Vector3(-1600, 1100, 0), new Vector3(-1450, 1100, 0), new Vector3(-1300, 1100, 0), new Vector3(-1150, 1100, 0), new Vector3(-1000, 1100, 0), new Vector3(-850, 1100, 0), new Vector3(-700, 1100, 0), new Vector3(-550, 1100, 0), new Vector3(-400, 1100, 0), new Vector3(-250, 1100, 0), new Vector3(-100, 1100, 0), new Vector3(50, 1100, 0), new Vector3(200, 1100, 0), new Vector3(350, 1100, 0), new Vector3(500, 1100, 0), new Vector3(650, 1100, 0), new Vector3(800, 1100, 0), new Vector3(950, 1100, 0), new Vector3(1100, 1100, 0), new Vector3(1250, 1100, 0), new Vector3(1400, 1100, 0), new Vector3(1550, 1100, 0), new Vector3(1700, 1100, 0), new Vector3(1850, 1100, 0), new Vector3(2000, 1100, 0), new Vector3(2150, 1100, 0), new Vector3(2300, 1100, 0), new Vector3(2450, 1100, 0), new Vector3(2600, 1100, 0), new Vector3(2750, 1100, 0), new Vector3(2900, 1100, 0), new Vector3(3050, 1100, 0), new Vector3(3200, 1100, 0), new Vector3(3350, 1100, 0), new Vector3(3500, 1100, 0), new Vector3(3650, 1100, 0), new Vector3(3800, 1100, 0), new Vector3(3950, 1100, 0), new Vector3(4100, 1100, 0), new Vector3(4250, 1100, 0), new Vector3(4400, 1100, 0), new Vector3(4550, 1100, 0), new Vector3(4700, 1100, 0), new Vector3(4850, 1100, 0), new Vector3(5000, 1100, 0), new Vector3(5150, 1100, 0), new Vector3(5300, 1100, 0), new Vector3(5450, 1100, 0), new Vector3(5600, 1100, 0), new Vector3(5750, 1100, 0), new Vector3(5900, 1100, 0), new Vector3(6050, 1100, 0), new Vector3(6200, 1100, 0), new Vector3(6350, 1100, 0), new Vector3(6500, 1100, 0), new Vector3(6650, 1100, 0), new Vector3(6800, 1100, 0), new Vector3(6950, 1100, 0), new Vector3(-4000, 950, 0), new Vector3(-3850, 950, 0), new Vector3(-3700, 950, 0), new Vector3(-3550, 950, 0), new Vector3(-3400, 950, 0), new Vector3(-3250, 950, 0), new Vector3(-3100, 950, 0), new Vector3(-2950, 950, 0), new Vector3(-2800, 950, 0), new Vector3(-2650, 950, 0), new Vector3(-2500, 950, 0), new Vector3(-2350, 950, 0), new Vector3(-2200, 950, 0), new Vector3(-2050, 950, 0), new Vector3(-1900, 950, 0), new Vector3(-1750, 950, 0), new Vector3(-1600, 950, 0), new Vector3(-1450, 950, 0), new Vector3(-1300, 950, 0), new Vector3(-1150, 950, 0), new Vector3(-1000, 950, 0), new Vector3(-850, 950, 0), new Vector3(-700, 950, 0), new Vector3(-550, 950, 0), new Vector3(-400, 950, 0), new Vector3(-250, 950, 0), new Vector3(-100, 950, 0), new Vector3(50, 950, 0), new Vector3(200, 950, 0), new Vector3(350, 950, 0), new Vector3(500, 950, 0), new Vector3(650, 950, 0), new Vector3(800, 950, 0), new Vector3(950, 950, 0), new Vector3(1100, 950, 0), new Vector3(1250, 950, 0), new Vector3(1400, 950, 0), new Vector3(1550, 950, 0), new Vector3(1700, 950, 0), new Vector3(1850, 950, 0), new Vector3(2000, 950, 0), new Vector3(2150, 950, 0), new Vector3(2300, 950, 0), new Vector3(2450, 950, 0), new Vector3(2600, 950, 0), new Vector3(2750, 950, 0), new Vector3(2900, 950, 0), new Vector3(3050, 950, 0), new Vector3(3200, 950, 0), new Vector3(3350, 950, 0), new Vector3(3500, 950, 0), new Vector3(3650, 950, 0), new Vector3(3800, 950, 0), new Vector3(3950, 950, 0), new Vector3(4100, 950, 0), new Vector3(4250, 950, 0), new Vector3(4400, 950, 0), new Vector3(4550, 950, 0), new Vector3(4700, 950, 0), new Vector3(4850, 950, 0), new Vector3(5000, 950, 0), new Vector3(5150, 950, 0), new Vector3(5300, 950, 0), new Vector3(5450, 950, 0), new Vector3(5600, 950, 0), new Vector3(5750, 950, 0), new Vector3(5900, 950, 0), new Vector3(6050, 950, 0), new Vector3(6200, 950, 0), new Vector3(6350, 950, 0), new Vector3(6500, 950, 0), new Vector3(6650, 950, 0), new Vector3(6800, 950, 0), new Vector3(6950, 950, 0), new Vector3(-4000, 800, 0), new Vector3(-3850, 800, 0), new Vector3(-3700, 800, 0), new Vector3(-3550, 800, 0), new Vector3(-3400, 800, 0), new Vector3(-3250, 800, 0), new Vector3(-3100, 800, 0), new Vector3(-2950, 800, 0), new Vector3(-2800, 800, 0), };
}
