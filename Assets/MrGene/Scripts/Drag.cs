using UnityEngine;
using System.Collections;

public class Drag : MonoBehaviour {

    private Vector3 dist;
    private float posX;
    private float posY;

    void OnMouseDown()
    {
        dist = Camera.main.WorldToScreenPoint(transform.position);
        posX = Input.mousePosition.x - dist.x;
        posY = Input.mousePosition.y - dist.y;
    }

    void OnMouseDrag()
    {
        Vector3 cursorPos = new Vector3(Input.mousePosition.x - posX, Input.mousePosition.y - posY, dist.z);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(cursorPos);
        transform.position = worldPos;
        Debug.Log("Works");

    }
}
