using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelection : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().Mute_Track(SFX.sound_index.warning, true);
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<GameManager>().ChangeLevel();

    }
}
