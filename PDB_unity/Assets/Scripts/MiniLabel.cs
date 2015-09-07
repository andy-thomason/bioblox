using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MiniLabel : MonoBehaviour, IPointerClickHandler {

	public LabelScript owner;
	
	// Use this for initialization
	void Start () {
	
	}

	public void OnPointerClick (PointerEventData eventData)
	{
		owner.OnPointerClick (eventData);
	}


	// Update is called once per frame
	void Update () {
	
	}
}
