using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;
	public Animator FadeCanvas;
    public GameObject MicroscopeView;
    public Transform Poster;
    public Scrollbar PosterScrollBar;
    public GameObject PosterScrollBarCanvas;


    public void CameraStart()
	{
		MainCamera.SetBool ("Start", true);
	}

    public void CameraToAbout()
    {
        MainCamera.SetBool("About", true);
    }

    public void AboutToMain()
    {
        MainCamera.SetBool("About", false);
        PosterScrollBarCanvas.SetActive(false);
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

    public void ScrollPoster()
    {
        Poster.position = new Vector3(5.7f,2.0f + (PosterScrollBar.value * 1.88f),-0.1f);
    }

    public void EnableScrollBarCanvas()
    {
        PosterScrollBarCanvas.SetActive(true);
    }


}
