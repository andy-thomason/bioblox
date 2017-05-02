using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class RestartPosition : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<BioBlox>().restart_position();

    }
}
