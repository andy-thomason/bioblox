using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour {
	public GameObject[] proteins=null;
	private bool loop = false;
	//public float damping;
	public float rotate = 1.0f;
	//public float translate = 0.01f;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		//Vector3 move = new Vector3 (0, 2.0f, 3.0f);
		if (!loop) {
			for (int i =0; i<proteins.Length; i++) {
				proteins[i].transform.Rotate(0.0f, 2.0f * Time.deltaTime * rotate, 2.0f * Time.deltaTime * rotate);
				//proteins[i].transform.Translate(move * Time.deltaTime * translate);
			}
		}
	}
}
