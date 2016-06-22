using UnityEngine;
using System.Collections;

public class CamerFirstPerson : MonoBehaviour {

	public GameObject CameraFrame;

	public void DeleteCamera()
    {
        //only 1 active
        GameObject check_new = GameObject.FindGameObjectWithTag("FirstPerson");
        if (check_new) Destroy(check_new);
    }
	
	public void CameraFrameOff()
	{
		CameraFrame.SetActive(false);
	}
	
	public void CameraFrameOn()
	{
		CameraFrame.SetActive(true);
	}
}
