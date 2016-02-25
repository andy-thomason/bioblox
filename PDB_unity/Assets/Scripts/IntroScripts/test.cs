using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class test : MonoBehaviour
{
    Camera map_camera;

    void Awake()
    {
        map_camera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(map_camera.ScreenToWorldPoint(Input.mousePosition));
            Ray ray = map_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100000))
            {
                Debug.Log(hit.transform.gameObject.name);
                Debug.Log(hit.point);
            }

        }
    }
}
