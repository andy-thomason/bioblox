using UnityEngine;
using System.Collections;

public class CameraRotation : MonoBehaviour {

    Vector3 position_camera;
    UIController ui;
    float coordinate;


    // Use this for initialization
    void Start()
    {
        ui = FindObjectOfType<UIController>();
        position_camera = transform.localPosition;
    }
    
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            coordinate = transform.localPosition.y;
            coordinate++;
            transform.localPosition = new Vector3(transform.localPosition.x, coordinate, transform.localPosition.z);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            coordinate = transform.localPosition.y;
            coordinate--;
            transform.localPosition = new Vector3(transform.localPosition.x, coordinate, transform.localPosition.z);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            coordinate = transform.localPosition.x;
            coordinate--;
            transform.localPosition = new Vector3(coordinate, transform.localPosition.y, transform.localPosition.z);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            coordinate = transform.localPosition.x;
            coordinate++;
            transform.localPosition = new Vector3(coordinate, transform.localPosition.y, transform.localPosition.z);
        }

    }
}
