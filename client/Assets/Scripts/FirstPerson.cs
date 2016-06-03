using UnityEngine;
using System.Collections;

public class FirstPerson : MonoBehaviour {

    GameObject temp_camera;
    Vector3 molecule_position;

    void Start()
    {
        molecule_position = transform.localPosition;
    }

    // Update is called once per frame
    void Update ()
    {
        if (transform.GetComponentInChildren<Camera>())
        {

            temp_camera = GameObject.FindGameObjectWithTag("FirstPerson");

            if (Input.GetKey(KeyCode.W))
            {
                transform.localPosition += new Vector3(temp_camera.transform.forward.x, 0, temp_camera.transform.forward.z) * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.S))
            {
                transform.localPosition -= new Vector3(temp_camera.transform.forward.x, 0, temp_camera.transform.forward.z) * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.localPosition += temp_camera.transform.right * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                transform.localPosition -= temp_camera.transform.right * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.localPosition += temp_camera.transform.up * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.localPosition -= temp_camera.transform.up * 5 * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.X))
            {
                transform.Rotate(Vector3.right * Time.deltaTime * 10, Space.World);
            }

            if (Input.GetKey(KeyCode.Z))
            {
                transform.Rotate(Vector3.left * Time.deltaTime * 10, Space.World);
            }
        } else
        {
            Vector3 molToOrigin = molecule_position - transform.position;
            if (molToOrigin.sqrMagnitude > 1.0f)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.AddForce(molToOrigin.normalized * 50000.0f);
            }
        }
    }
}
