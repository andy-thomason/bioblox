using UnityEngine;
using System.Collections;

public class FadePanel : MonoBehaviour {

    bool FadeStatus = false;
    BioBlox bb;

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
        FindObjectOfType<IntroController>().MainToGame();
        bb.StartGame();
    }
}
