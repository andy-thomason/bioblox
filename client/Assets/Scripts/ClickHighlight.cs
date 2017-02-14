using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickHighlight : MonoBehaviour, IPointerClickHandler
{
    public bool is_clicked = false;
    Image button_color;
    Color active_color;
    Color normal_color = new Color(255, 255, 255);

    void Start()
    {
        active_color = FindObjectOfType<UIController>().GridToggleColor_pressed;
        button_color = GetComponent<Image>();
        button_color.color = is_clicked ? active_color : normal_color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        button_color.color = !is_clicked ? active_color : normal_color;
        is_clicked = !is_clicked;
    }
}
