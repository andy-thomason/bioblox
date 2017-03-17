using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGo : MonoBehaviour {
    
    public GameObject images_go_default;
    public GameObject images_go_activo;
    
    void OnCollisionEnter(Collision collision)
    {
        images_go_default.SetActive(false);
        images_go_activo.SetActive(true);
    }

    void OnCollisionExit(Collision collision)
    {
        images_go_default.SetActive(true);
        images_go_activo.SetActive(false);
    }
}
