using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelection : MonoBehaviour
{
    public int level_number;

    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<GameManager>().ChangeLevel(level_number);
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-50.0f),ForceMode.Impulse);

    }
}
