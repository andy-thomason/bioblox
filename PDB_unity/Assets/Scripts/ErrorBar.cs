using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ErrorBar : MonoBehaviour {
	

	public float score;
	float minScore;
	float maxScore;
	public Text debugText;

	public RectTransform scoreBarObj;
	Vector3 origin;

	BioBlox bloxScript;

	// Use this for initialization
	void Start () {
		minScore = 0.0f;
		maxScore = 100.0f;
		score = 0.5f;
		origin = scoreBarObj.localPosition;
		bloxScript = GameObject.Find ("BioBlox").GetComponent<BioBlox> ();
	}

	public void UpdateScore(float s)
	{
		score = s;
	}

	// Update is called once per frame
	void Update () {
		if (bloxScript) {
			score=bloxScript.ScoreRMSD();
		}
		if(debugText)
		{
			debugText.text = string.Format("Score :{0}", score.ToString("F3"));
		}
		if (scoreBarObj) {
			float lockScore = Mathf.Min(score,maxScore);
			lockScore = Mathf.Max(lockScore,minScore);

			float scaleFactor = (lockScore - minScore)/(maxScore - minScore);

			float inverseScale = 1.0f - scaleFactor;


			float height = scoreBarObj.rect.height;
			float positionOffset = (height/2.0f) *inverseScale;


			Vector3 newPos = origin + new Vector3(0,-positionOffset,0);
			Vector3 newScale = new Vector3(1,scaleFactor,1);

			scoreBarObj.localScale = newScale;
			scoreBarObj.localPosition = newPos;
		}
	}
}
