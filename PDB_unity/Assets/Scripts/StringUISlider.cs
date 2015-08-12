using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StringUISlider : MonoBehaviour, IBeginDragHandler,
IDragHandler,IEndDragHandler {
	
	public RectTransform sliderBar;

	public ConnectionManager man;
	
	// Use this for initialization
	void Start () {
	
	}

	float MoveToProjectedPosition()
	{
		Vector3 axis = Vector3.up;
		Vector3 top = sliderBar.transform.position + axis * sliderBar.rect.height / 2;
		Vector3 bot = top + axis * -sliderBar.rect.height;

		Vector3 bar = bot - top;

		float dot = Vector3.Dot (this.transform.position, bar);
		if (dot < 0) {
			this.transform.position = top;
		} else if (dot > 1) {
			this.transform.position = bot;
		} else {
			this.transform.position = top + bar * dot;
		}
		return dot;
	}

	float CalculateDistanceSlid(float dist)
	{
		dist = Mathf.Max (0, dist);
		dist = Mathf.Min (1, dist);

		float invDist = dist - 1;

		return man.maxDistance * invDist;
	}

	void ChangeMinDistance(float percent)
	{
		man.minDistance = percent;
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
	}
	
	public void OnDrag (PointerEventData eventData)
	{

		Vector3 pos = this.transform.position;
		pos += new Vector3 (eventData.delta.x, eventData.delta.y, 0);
		this.transform.position = pos;
		float dist = MoveToProjectedPosition ();

		ChangeMinDistance (CalculateDistanceSlid (dist));
	}
	
	public void OnEndDrag (PointerEventData eventData)
	{

	}

	// Update is called once per frame
	void Update () {


	}
}
