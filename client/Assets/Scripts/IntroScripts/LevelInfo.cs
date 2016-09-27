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
    public int camera_zoom = 240;

    UIController UI;
    SFX sfx;

    void Awake()
    {
        UI = FindObjectOfType<UIController>();
        sfx = FindObjectOfType<SFX>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        UI.SaveLastLevelPlayed(gameObject);
        UI.LoadLevelDescriptionIntro(level_name, level_description, level_image, level);
    }

    public void SendData()
    {
        UI.LoadLevelDescription(level, mesh_offset_1, mesh_offset_2, camera_zoom);
    }


}
