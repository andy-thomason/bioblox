using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TransformLerper : MonoBehaviour {
	
		List<Vector3> positions = new List<Vector3> ();
		List<Quaternion> quaternions = new List<Quaternion> ();
		
		int index;
			
		public bool finished=true;
		
		Transform target;
		
		IEnumerator LerpTransforms()
		{
			float t = 0;
			Vector3 start = target.position;
			Quaternion startRot = target.rotation;
			if (index == positions.Count) {
				finished=true;
				yield break;
			}
			while (t<=1){
				t += Time.deltaTime;
				target.position = Vector3.Lerp (start,positions[index], t);
				target.rotation = 
					Quaternion.Slerp (startRot,quaternions[index], t);
				yield return null;
			}
			index++;
			StartCoroutine ("LerpTransforms");
		} 
		
		public void AddTransformPoint(Vector3 vec,
		                              Quaternion rot)
		{
			positions.Add (vec);
			quaternions.Add (rot);
		}
		public void StartTransform()
		{
			finished = false;
			StartCoroutine ("LerpTransforms");
			
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
