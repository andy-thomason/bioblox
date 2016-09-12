using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class LevelInfo : MonoBehaviour, IPointerClickHandler {
    
    public string level_name;
    public string level_description;
    public Sprite level_image;
    public int level;
    //meshes distance
    public int mesh_offset_1 = -1;
    public int mesh_offset_2 = 2;

    UIController UI;

    void Awake()
    {
        UI = FindObjectOfType<UIController>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        UI.LoadLevelDescription(level_name, level_description, level_image, level, mesh_offset_1, mesh_offset_2);
    }


}
