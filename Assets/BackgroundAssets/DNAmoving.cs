using UnityEngine;
using System.Collections;

public class DNAmoving : MonoBehaviour {
    public GameObject[] DNA;
    public float moveSpeed = 0.0f;
    public float rotationDamping = 0.0f;
	// Use this for initialization
	void Start () {
       gameObject.GetComponent<GameObject>();
	}

	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < DNA.Length; i++)
        {
            DNA[i].transform.Translate(new Vector3(moveSpeed*Time.deltaTime, 0, 0));
            DNA[i].transform.Rotate(new Vector3(1, 0, 1), rotationDamping * Time.deltaTime);
        }
	}
}
