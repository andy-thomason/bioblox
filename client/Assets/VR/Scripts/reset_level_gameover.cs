using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reset_level_gameover : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<SFX>().Mute_Track(SFX.sound_index.warning, true);
        FindObjectOfType<GameMode>().restart();
    }
}
