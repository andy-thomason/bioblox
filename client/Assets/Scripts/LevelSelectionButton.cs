using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class LevelSelectionButton : MonoBehaviour, IPointerClickHandler {

    public int level_number { get; set; }
    BioBlox bb;

	// Use this for initialization
	void Start ()
    {
        bb.GetComponent<BioBlox>();	
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        bb.current_level = level_number;
    }
}
