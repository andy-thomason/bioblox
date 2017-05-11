using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelectionButton : MonoBehaviour, IPointerClickHandler {

    public int level_number;
    public int slot_number;

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<SFX>().StopTrack(SFX.sound_index.cutaway_cutting);
        FindObjectOfType<SFX>().Mute_Track(SFX.sound_index.warning, true);
        FindObjectOfType<GameManager>().ChangeLevel(level_number, slot_number);
    }
}
