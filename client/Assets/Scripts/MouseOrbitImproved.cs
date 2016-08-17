using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour
{

    public Vector3 target;
    public GameObject protein;
    public float distance = 15.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = 15f;
    public float distanceMax = 35f;

    private Rigidbody rigidbody;

    Vector3 negDistance;
    Vector3 position;

    float x = 0.0f;
    float y = 0.0f;

    // Use this for initialization
    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        //rigidbody = GetComponent<Rigidbody>();

        //// Make the rigid body not change rotation
        //if (rigidbody != null)
        //{
        //    rigidbody.freezeRotation = true;
        //}
    }

    void LateUpdate()
    {
        target = protein.transform.TransformPoint(protein.GetComponent<PDB_mesh>().mol.atom_centres[protein.GetComponent<PDB_mesh>().atom]);

        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            //RaycastHit hit;
            //if (Physics.Linecast(target, transform.position, out hit))
            //{
            //    distance -= hit.distance;
            //}
            negDistance = new Vector3(0.0f, 0.0f, -distance);
            position = rotation * negDistance + target;

            transform.rotation = rotation;
            transform.position = position;
        }
        else
        {

            Quaternion rotation = Quaternion.Euler(0, 0, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            //RaycastHit hit;
            //if (Physics.Linecast(target, transform.position, out hit))
            //{
            //    distance -= hit.distance;
            //}
            negDistance = new Vector3(0.0f, 0.0f, -distance);
            position = rotation * negDistance + target;

            //transform.rotation = rotation;
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