using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reset_level_gameover : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<GameMode>().restart();
    }
}
