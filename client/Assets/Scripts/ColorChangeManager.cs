using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ColorChangeManager : MonoBehaviour {

    public GameObject ColorHolder;
    public GameObject ColorButtons;
    public Image CurrentColor;
    public Color[] AvailableColors;
    bool is_panel_open = false;
    public Camera MainCamera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeBackgroundColor(int color)
    {
        MainCamera.backgroundColor = CurrentColor.color  = AvailableColors[color];
        OpenColorPanel();
    }

    public void OpenColorPanel()
    {
        ColorHolder.SetActive(!is_panel_open);
        ColorButtons.SetActive(!is_panel_open);
        is_panel_open = !is_panel_open;
    }
}
