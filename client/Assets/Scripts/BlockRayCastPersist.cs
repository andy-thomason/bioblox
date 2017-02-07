using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BlockRayCastPersist : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    UIController ui;
    bool OverIt = false;

    void Update()
    {
        if (OverIt && FindObjectOfType<UIController>() != null)
            FindObjectOfType<UIController>().isOverUI = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OverIt = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OverIt = false;
        if(FindObjectOfType<UIController>() != null)
            FindObjectOfType<UIController>().isOverUI = false;
    }

}
