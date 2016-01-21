using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MapStopCourutine : MonoBehaviour, IPointerDownHandler
{

    public void OnPointerDown(PointerEventData data)
    {
        StopAllCoroutines();
    }
}
