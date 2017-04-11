using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfoPanel : MonoBehaviour, IPointerExitHandler, IPointerClickHandler
{

    public GameObject info_panel;

    public void OnPointerExit(PointerEventData eventData)
    {
        info_panel.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        info_panel.SetActive(true);
    }
}
