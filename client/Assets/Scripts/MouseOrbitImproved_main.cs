using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved_main : MonoBehaviour
{

    public Vector3 target;
    public float distance = 200.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = 15f;
    public float distanceMax = 35f;

    public float scroll_speed = 10;

    //private Rigidbody rigidbody;
    public Texture2D cursor_move;

    Vector3 negDistance;
    Vector3 position;
    Quaternion rotation;
    UIController ui;

    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    void Start()
    {
        ui = FindObjectOfType<UIController>();
        Init();
    }

    public void Init()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        target = new Vector3(0, 0, 0);
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        rotation = Quaternion.Euler(y, x, 0);

        negDistance = new Vector3(0.0f, 0.0f, -distance);
        position = rotation * negDistance + target;

        transform.rotation = rotation;
        transform.position = position;
    }

    void Update()
    {
        if(!ui.isOverUI)
        {
            if (Input.GetMouseButton(1))
            {
                //Cursor.SetCursor(cursor_move, Vector2.zero, CursorMode.Auto);
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scroll_speed, distanceMin, distanceMax);

            y = ClampAngle(y, yMinLimit, yMaxLimit);
            rotation = Quaternion.Euler(y, x, 0);
            negDistance = new Vector3(0.0f, 0.0f, -distance);
            position = rotation * negDistance + target;

            transform.rotation = rotation;
            transform.position = position;
        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}