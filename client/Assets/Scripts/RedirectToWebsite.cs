using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RedirectToWebsite : MonoBehaviour, IPointerClickHandler
{
    public int type_website;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(type_website == 0)
            Application.OpenURL("http://bioblox.org/");
        else
            Application.OpenURL("http://www.rcsb.org");

    }
}
