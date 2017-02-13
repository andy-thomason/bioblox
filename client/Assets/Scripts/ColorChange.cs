using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorChange : MonoBehaviour, IPointerClickHandler
{

    public int BackgroundColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<ColorChangeManager>().ChangeBackgroundColor(BackgroundColor);
    }
}
