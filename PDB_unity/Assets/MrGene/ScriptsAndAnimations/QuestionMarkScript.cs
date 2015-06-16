using UnityEngine;
using System.Collections;

public class QuestionMarkScript : MonoBehaviour {

	public GameObject gene;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
		
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				if(hit.collider.gameObject==this.gameObject)
				{
					gene.SetActive(true);
					GameObject.Find("BioBlox").GetComponent<BioBlox>().PopOut(this.gameObject);
				}
			}
		}
	}
}
