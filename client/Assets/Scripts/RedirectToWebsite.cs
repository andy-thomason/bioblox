using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RedirectToWebsite : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL("http://bioblox.org/");
    }
}
