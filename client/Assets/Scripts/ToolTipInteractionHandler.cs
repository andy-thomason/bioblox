using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ToolTipInteractionHandler : MonoBehaviour, IPointerExitHandler, IPointerClickHandler
{
    private float timer;
    private bool HoverElement;
    private Vector3 CurrentMousePosition;
    private ToolTipController toolTipController;
    private bool ToolTipExist;
    
    // init
    void Start()
    {
        toolTipController = FindObjectOfType<ToolTipController>();
        ToolTipExist = false;
    }
    
    void Update()
    {
        //if the mouse if over, start the timer
        //if (HoverElement)
        //{
        //    timer += Time.fixedDeltaTime;
        //    Debug.Log(timer);
        //}
        //if the mouse is moving restart the timer for the tooltip
        //if (CurrentMousePosition != Input.mousePosition)
        //{
        //    timer = 0;
        //}
        //last frame mouse position = current mouse position
        //CurrentMousePosition = Input.mousePosition;

        //over 4 seconds spawn
        //if (timer > 0.5f)
        //{
        //    toolTipController.SpawnToolTip(gameObject);
        //    ToolTipExist = true;
        //}
        //if the timer is 0 and the tool was created then call delete
        //if (timer == 0 && ToolTipExist)
        //{
        //    toolTipController.DestroyToolTip();
        //    ToolTipExist = false;
        //}
    }
    
    //// Called when the mouse hovers over the transform
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    HoverElement = true;
    //    CurrentMousePosition = Input.mousePosition;
    //}
    
    // called when the mouse leaves the transform
    public void OnPointerExit(PointerEventData eventData)
    {
        toolTipController.DestroyToolTip();
        ToolTipExist = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        toolTipController.SpawnToolTip(gameObject);
        ToolTipExist = true;
    }

    //void OnDisable()
    //{
    //    toolTipController.DestroyToolTip();
    //    ToolTipExist = false;
    //}
}
