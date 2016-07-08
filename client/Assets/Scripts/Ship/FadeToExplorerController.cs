using UnityEngine;
using System.Collections;

public class FadeToExplorerController : MonoBehaviour {

    ExploreController ex;
    public GameObject Flare;

	// Use this for initialization
	void Start () {
        ex = FindObjectOfType<ExploreController>();
	}
	
	void ChangeToExplorer()
    {
        ex.StartExplore();
    }

    void TurnOnSun()
    {
        Flare.SetActive(true);
    }

    public void StartFading()
    {
        GetComponent<Animator>().SetBool("Start", true);
    }

    public void BackToStart()
    {
        GetComponent<Animator>().SetBool("Start", false);
    }
}
