using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelectionButton : MonoBehaviour, IPointerClickHandler {

    public int level_number;

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<GameManager>().ChangeLevel(level_number);
    }
}
