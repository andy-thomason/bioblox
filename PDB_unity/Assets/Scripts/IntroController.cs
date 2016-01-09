using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;
	public GameObject FadeCanvas;

	public void CameraStart()
	{
		MainCamera.SetBool ("Start", true);
	}

	public void ChangeScene()
	{
		// obsolete Application.LoadLevel ("main");
        UnityEngine.SceneManagement.SceneManager.LoadScene("main");
	}

	public void StartFadeCanvas()
	{
		FadeCanvas.SetActive (true);
	}


}
