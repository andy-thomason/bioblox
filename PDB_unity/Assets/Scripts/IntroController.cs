using UnityEngine;
using System.Collections;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;

	public void CameraStart()
	{
		MainCamera.SetBool ("Start", true);
		Debug.Log ("caca");
	}
}
