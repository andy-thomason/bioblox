using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGo : MonoBehaviour {
    
    public GameObject images_go_default;
    public GameObject images_go_activo;
    SFX sfx;

    void Start()
    {
        sfx = FindObjectOfType<SFX>();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        sfx.PlayTrack(SFX.sound_index.level_select);
        images_go_default.SetActive(false);
        images_go_activo.SetActive(true);
    }

    void OnCollisionExit(Collision collision)
    {
        images_go_default.SetActive(true);
        images_go_activo.SetActive(false);
    }
}
