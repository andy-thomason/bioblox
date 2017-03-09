using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelection : MonoBehaviour
{
    public int level_number;

    public void SelectLevel()
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<GameManager>().ChangeLevel(level_number);
    }
}
