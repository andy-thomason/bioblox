using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class test : MonoBehaviour
{

    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(Vector2.zero, GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition));
        if (hit.collider != null)
        {
            Debug.Log(hit.transform.gameObject.name);
            Debug.Log(hit.point);
        }
        else
        {
            Debug.Log("Nothing");
        }
    }
}
