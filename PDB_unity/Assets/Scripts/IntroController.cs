using UnityEngine;
using System.Collections;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;
	public Animator FadeCanvas;
    public GameObject MicroscopeView;

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
        FadeCanvas.SetBool("Fade", true);
	}

    public void MainToMicroscopeCamera()
    {
        gameObject.GetComponent<Camera>().enabled = false;
        MicroscopeView.SetActive(true);
    }


}
