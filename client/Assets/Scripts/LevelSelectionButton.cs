using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelectionButton : MonoBehaviour, IPointerClickHandler {

    public int level_number;
    BioBlox bb;

	// Use this for initialization
	void Start ()
    {
        bb = FindObjectOfType<BioBlox>();	
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        bb.ChangeLevel(level_number);
    }
}
