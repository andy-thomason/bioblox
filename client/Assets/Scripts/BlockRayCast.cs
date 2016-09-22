using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BlockRayCast : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            ui.isOverUI = true;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        OverIt = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OverIt = false;
        ui.isOverUI = false;
    }

    void OnDisable()
    {
        OverIt = false;
        ui.isOverUI = false;
    }

}
