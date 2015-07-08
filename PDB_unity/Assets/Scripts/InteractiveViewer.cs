using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class InteractiveViewer : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
IDragHandler,IEndDragHandler{


	public Camera cam;
	GameObject mol;

	public int molNum;
	public float maxRadius=22.0f;
	public float scrollSpeed=0.1f;

	Vector2 oldMousePos;
	// Use this for initialization
	void Start () {

	}

	public void OnPointerClick (PointerEventData eventData)
	{
	
	}
	
	public void OnBeginDrag (PointerEventData eventData)
	{
		mol = GameObject.Find ("BioBlox").GetComponent<BioBlox> ().molecules [molNum];
		oldMousePos = eventData.position;
	}
	
	public void OnDrag (PointerEventData eventData)
	{
		Vector2 mousePos = eventData.position;
		Vector2 delta = mousePos - oldMousePos;
		delta.Normalize ();
		delta *= scrollSpeed;
		oldMousePos = mousePos;
		cam.transform.Translate (new Vector3 (delta.x * -gameObject.transform.localScale.x, -delta.y,0));
		Vector3 pos = cam.transform.position;
		pos.x = 0;
		if (pos.magnitude > maxRadius) {
			pos=pos.normalized*maxRadius;
		}
		pos.x = cam.transform.position.x;
		cam.transform.position = pos;
	}
	
	public void OnEndDrag (PointerEventData eventData)
	{

	}

	// Update is called once per frame
	void Update () {
	
	}
}
