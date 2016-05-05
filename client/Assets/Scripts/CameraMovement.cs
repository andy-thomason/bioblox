using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public int speed = 2;
	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;
	
	public float minimumX = -360F;
	public float maximumX = 360F;
	
	public float minimumY = -60F;
	public float maximumY = 60F;
	
	float rotationY = 0F;
	
	// Update is called once per frame
	void Update () {

		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}


		if(Input.GetKey (KeyCode.W)) {
			transform.localPosition += transform.forward * speed * Time.deltaTime;
		}
		if(Input.GetKey (KeyCode.S)) {
			transform.localPosition -= transform.forward * speed * Time.deltaTime;
		}
		if(Input.GetKey (KeyCode.D)) {
			transform.localPosition += transform.right * speed * Time.deltaTime;
		}
		if(Input.GetKey (KeyCode.A)) {
			transform.localPosition -= transform.right * speed * Time.deltaTime;
		}

		if(Input.GetKey (KeyCode.Q)) {
			transform.localPosition += transform.up * speed * Time.deltaTime;
		}

		if(Input.GetKey (KeyCode.E)) {
			transform.localPosition -= transform.up * speed * Time.deltaTime;
		}
			
	}
}
