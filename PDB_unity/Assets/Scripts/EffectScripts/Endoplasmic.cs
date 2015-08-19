using UnityEngine;
using System.Collections;

public class Endoplasmic : MonoBehaviour {

	public GameObject[] endoplasmic = null;
	private bool loop = false; 
	private float time = 10.0f;
	public float rotVal = 2.0f;
	public Renderer[] rend = null;
	//public Renderer colorA, colorB;
	public Color colora, colorb;
	public void moveObject(float time)
	{
		time = 2.0f;
		Quaternion rotating = transform.rotation;
		float rotX = rotating.x;
		float rotZ = rotating.z;
		float rotY = rotating.y * rotVal*Time.deltaTime;
		rotating.x = 0.0f;
		rotating.z = 0.0f;
		rotating = Quaternion.Lerp(transform.rotation,rotating,time);
		for (int i =0; i<endoplasmic.Length; i++) {
			endoplasmic[i].transform.rotation = rotating;
			if (!loop) {
				endoplasmic[i].transform.Rotate (rotX, rotY, rotZ);
			}	
		}
				
		//endoplasmic.transform.eulerAngles.x = null;
		//endoplasmic.transform.eulerAngles.z = null;
		//endoplasmic.transform.Rotate (rotating * move * Time.deltaTime, 0.0f)
	}
	public void changeColors()
	{
		colora = Color.green + Color.grey;
		colorb = Color.cyan;
		float lerp = Mathf.PingPong (Time.time, time)/time;

		for (int i =0; i<rend.Length; i++) {
			rend[i].material.color = Color.Lerp (colora, colorb, lerp);
		}
	}
	// Use this for initialization
	void Start () {
			Renderer rend = GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
			if (!loop) {
			moveObject(2.0f);
			changeColors();
			} 
		}
}
