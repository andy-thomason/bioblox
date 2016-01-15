using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelStructure : MonoBehaviour, IPointerClickHandler
{

    public string level_name;
    Transform map_canvas;
    LevelMapController levelMapController;
    public Sprite testa;
    public Sprite nrnalo;

    void Awake()
    {
        //map_canvas = gameObject.transform.parent.GetComponent<Transform>();
        map_canvas = GameObject.Find("MapPanel").GetComponent<Transform>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        levelMapController = FindObjectOfType<LevelMapController>();
       // levelMapController.CreateLevelDescription(level_name);
    }

    void Update()
    {
        if(transform.position.x >200 && transform.position.x < 600 && transform.position.y > 200 && transform.position.y < 400)
        {
            if (map_canvas.localScale.x > 0.5f)
            {
                GetComponentInChildren<Image>().overrideSprite = testa;
            }
        }
        else
        {
            GetComponentInChildren<Image>().overrideSprite = nrnalo;
        }
    }

}
