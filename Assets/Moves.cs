using UnityEngine;
using System.Collections;

public class Moves : MonoBehaviour {

    public GameObject[] blocks;
    private Vector3[] paths;
    public float moveSpeed = 0.0f;

    IEnumerator MoveOn(bool loop)
    {
        do
        {
            foreach (GameObject block in blocks)
            {
               // Instantiate(block);
                block.transform.Translate(new Vector3(Random.Range(0, 4), Random.Range(0, 4), Random.Range(0, 4)));
                yield return new WaitForSeconds(2.0f);
            }
        }
        while (loop);
    }

    IEnumerator moveToPosition(Vector3 target)
    {
        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return 0;
        }
       
    }
	// Use this for initialization
	void Start () {
        StartCoroutine(MoveOn(true));
	}

    // Update is called once per frame
    void Update()
    {
    }

    }
