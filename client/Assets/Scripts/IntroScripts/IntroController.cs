using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour {

	public Animator MainCamera;
	public Animator FadeCanvas;
    public GameObject MicroscopeView;
    public Transform Poster;
    public Scrollbar PosterScrollBar;
    public GameObject PosterScrollBarCanvas;
    public GameObject IntroLabModel;
    public GameObject GameCamera;
    public CanvasGroup GameCanvas;


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

	public void StartFadeCanvas()
	{
        FadeCanvas.SetBool("Fade", true);
	}

    public void MainToMicroscopeCamera()
    {
        GetComponent<Camera>().enabled = false;
        MicroscopeView.SetActive(true);
        IntroLabModel.SetActive(false);
    }

    public void MainToGame()
    {
        GetComponent<Camera>().enabled = false;
        GameCamera.SetActive(true);
        GameCanvas.alpha = 1;
    }

    public void ScrollPoster()
    {
        Poster.position = new Vector3(5.7f,(2.0f + (PosterScrollBar.value * 1.88f)) - 600.0f,-0.1f);
    }

    public void EnableScrollBarCanvas()
    {
        PosterScrollBarCanvas.SetActive(true);
    }
    //change to game scene
    public void ChangeToGameScene()
    {
        FadeCanvas.SetBool("Fade", true);
        StartCoroutine(WaitForSeconds());

    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("main");
    }


}
