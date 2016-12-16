using UnityEngine;
using System.Collections;

public class EndLevelScript : MonoBehaviour {

    public GameObject selection_panel;
    UIController ui;

    // Use this for initialization
    void Start()
    {
        ui = FindObjectOfType<UIController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ClosePanel()
    {
        selection_panel.SetActive(false);
    }

    public void OpenPanel()
    {
        selection_panel.SetActive(true);
    }

    public void Retry()
    {
        ui.RestartLevel();
    }
}
