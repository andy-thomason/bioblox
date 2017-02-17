using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class AminoInfoPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    UIController ui;
    bool OverIt = false;

    void Start()
    {
        ui = FindObjectOfType<UIController>();
    }

    void Update()
    {
        if (OverIt)
            ui.is_over_amino_info_panel = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OverIt = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OverIt = false;
        ui.is_over_amino_info_panel = false;
    }
}
