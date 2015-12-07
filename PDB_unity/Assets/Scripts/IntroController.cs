using UnityEngine;
using System.Collections;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;
	public GameObject FadeCanvas;

	public void CameraStart()
	{
		MainCamera.SetBool ("Start", true);
	}

	public void ChangeScene()
	{
		Application.LoadLevel ("main");
	}

	public void StartFadeCanvas()
	{
		FadeCanvas.SetActive (true);
	}


}
