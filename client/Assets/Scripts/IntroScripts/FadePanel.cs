using UnityEngine;
using System.Collections;

public class FadePanel : MonoBehaviour {

    bool FadeStatus = false;
    BioBlox bb;
    public Light intro_light;

    void Start()
    {
        bb = FindObjectOfType<BioBlox>();
    }

	public void FadeCycle()
    {
        gameObject.GetComponent<Animator>().SetBool("Fade", FadeStatus);
        FadeStatus = !FadeStatus;
    }

    public void LoadGame()
    {
        intro_light.enabled = false;
        FindObjectOfType<IntroController>().MainToGame();
        bb.StartGame();
    }
}
