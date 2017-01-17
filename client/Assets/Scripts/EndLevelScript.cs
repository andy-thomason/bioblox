using UnityEngine;
using System.Collections;

public class EndLevelScript : MonoBehaviour {

    public CanvasGroup selection_panel;

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
        selection_panel.alpha = 0;
        selection_panel.blocksRaycasts = false;
    }

    public void OpenPanel()
    {
        selection_panel.alpha = 1;
        selection_panel.blocksRaycasts = true;
    }

    public void Retry()
    {
        FindObjectOfType<UIController>().RestartLevel();
        selection_panel.alpha = 0;
        selection_panel.blocksRaycasts = false;
    }
}
