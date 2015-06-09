using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TransformLerper : MonoBehaviour {
	
		List<Vector3> positions = new List<Vector3> ();
		List<Quaternion> quaternions = new List<Quaternion> ();
		
		int index;
		public float speed=1;

			
		public bool finished=true;
		
		public Transform target;
	int numRunning=0;
		
		IEnumerator LerpTransforms ()
	{
		numRunning++;
		float t = 0;
		Vector3 start = target.position;
		Quaternion startRot = target.rotation;
		if (index == positions.Count) {
			finished = true;
			yield break;
		}
		while (t<=1) {
			t += Time.deltaTime*speed;
			if(positions[index]!=null)
			{
			target.position = Vector3.Lerp (start, positions [index], t);
			}
			if(quaternions[index]!=null)
			{
			target.rotation = 
						Quaternion.Slerp (startRot, quaternions [index], t);
			}
			yield return null;
		}
		index++;
		StartCoroutine ("LerpTransforms");
		yield break;
	} 
		
		public void AddTransformPoint(Vector3 vec,
		                              Quaternion rot)
		{
			positions.Add (vec);
			quaternions.Add (rot);
		}
		public void AddTransformPoint(Quaternion rot)
		{
		if (positions.Count > 0) {
			positions.Add (positions [positions.Count - 1]);
		} else {
			positions.Add(this.transform.position);
		}
		quaternions.Add (rot);
		}
		public void StartTransform()
		{
			if (finished) {
			finished=false;
			StartCoroutine ("LerpTransforms");
		}
			
		}
	// Use this for initialization
	void Start () {
		target = transform;
		index = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
