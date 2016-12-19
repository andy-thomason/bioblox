using UnityEngine;
using System.Collections;

public class EndLevelScript : MonoBehaviour {

    public GameObject selection_panel;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClosePanel()
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        selection_panel.SetActive(false);
    }

    public void OpenPanel()
    {
        selection_panel.SetActive(true);
    }

    public void Retry()
    {
        FindObjectOfType<UIController>().RestartLevel();
        selection_panel.SetActive(false);
    }
}
