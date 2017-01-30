using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class RedirectToLogin : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        Application.OpenURL("https://bioblox3d.org/wp-login.php");
    }
}
