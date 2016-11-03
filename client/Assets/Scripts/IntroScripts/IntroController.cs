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
    public CanvasGroup IntroCanvas;
    public GameObject IntroCamera;
    public Canvas AboutButton;
    public Canvas PlayButton;
    public Light intro_light;
    UIController ui;
    BioBlox bb;
    public Animator SFXAnimator;
    SFX sfx;
    XML xml;

    void Awake()
    {
        IntroCamera.SetActive(true);
        //PlayButton.worldCamera = IntroCamera.GetComponent<Camera>();
        // AboutButton.worldCamera = IntroCamera.GetComponent<Camera>();
        ui = FindObjectOfType<UIController>();
        bb = FindObjectOfType<BioBlox>();
        xml = FindObjectOfType<XML>();
        sfx = FindObjectOfType<SFX>();
        //sfx.PlayTrack(SFX.sound_index.main_audio);
        SFXAnimator.SetBool("Start", true);
    }

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
        sfx.PlayTrack(SFX.sound_index.button_click);
        FadeCanvas.SetBool("Fade", true);
        StartCoroutine(WaitForFade());
    }

    public void MainToGame()
    {
        intro_light.enabled = false;
        GameCanvas.alpha = 1;
        GameCanvas.blocksRaycasts = true;
        IntroCamera.SetActive(false);
        FindObjectOfType<ConnectionManager>().DisableSlider();
        IntroCanvas.interactable = false;
        IntroCanvas.blocksRaycasts = false;
        ui.LevelClickled.GetComponent<LevelInfo>().SendData();
    }
    
    public void GameToMain()
    {
        if(ui.number_atoms_end_level.text != "0" && bb.is_score_valid)
            xml.SaveXML(bb.current_level, ui.number_atoms_end_level.text, ui.time_to_save);

        bb.game_status = BioBlox.GameStatus.MainScreen;
        bb.Reset();
        ui.Reset_UI();
        ui.EndLevelPanel.SetActive(false);
        intro_light.enabled = true;
        GameCanvas.alpha = 0;
        GameCanvas.blocksRaycasts = false;
        IntroCamera.SetActive(true);
        FadeCanvas.SetBool("Fade", false);
        IntroCanvas.interactable = true;
        IntroCanvas.blocksRaycasts = true;
        sfx.StopTrack(SFX.sound_index.warning);
    }

    IEnumerator WaitForFade()
    {
        yield return new WaitForSeconds(1);
        MainToGame();
        bb.StartGame();
    }


}
