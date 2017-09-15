using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SetHeight : MonoBehaviour {

    public Transform main_camera;

    public void set_height()
    {
        transform.position = new Vector3(0, main_camera.position.y, main_camera.position.z + 50.0f);
    }
}
