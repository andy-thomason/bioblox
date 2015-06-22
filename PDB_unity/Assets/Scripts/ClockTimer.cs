using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net;
public class ClockTimer : MonoBehaviour {


	public Text timeText;

	float playTime=0.0f;
	bool clockStopped=true;

	public GameObject levelTimePrefab;

	public List<float> playerLevelTimes = new List<float>();

	GameObject playerTimeZone;
	bool shouldAddLevelTimes=true;


	GameObject g;
	GameObject target;

	public void StopPlayerTimer()
	{
		clockStopped = true;
	}

	public void StartPlayerTimer()
	{
		clockStopped = false;
	}

	public void LogPlayerTime()
	{
		playerLevelTimes.Add (playTime);
		if (shouldAddLevelTimes) {
			SpawnLevelTimeIcon();
		}
	}

	void SpawnLevelTimeIcon()
	{
		g= GameObject.Instantiate(levelTimePrefab);

		g.GetComponentInChildren<Text>().text = playTime.ToString ("F2");
		Vector3 prevScale = g.transform.localScale;
		g.transform.position = new Vector3 (0, 0, 0);

		target = GameObject.Instantiate (g);
		target.transform.position = new Vector3 (0, 0, 0);
		target.name = "TimePipLevel" + playerLevelTimes.Count;
		target.transform.SetParent(playerTimeZone.transform);
		target.transform.localScale = prevScale;
		target.transform.position = new Vector3 (
			target.transform.position.x,
			target.transform.position.y,
			0);


		g.transform.SetParent (GameObject.Find ("Canvas").transform);
		g.transform.position = timeText.transform.position;

		StartCoroutine ("LerpToPos");
	}

	IEnumerator LerpToPos()
	{
		GameObject from = g;
		GameObject to = target;

		to.GetComponent<CanvasGroup> ().alpha = 0.0f;

		Vector3 start = from.transform.position;


		Vector3 startScale = new Vector3 (0, 0, 0);
		Vector3 endScale = to.transform.localScale;

		for(float t = 0.0f; t < 1.0f; t += Time.deltaTime)
		{
			from.transform.position = Vector3.Lerp(start,
			                                       to.transform.position,
			                                       t);

			from.transform.localScale = Vector3.Lerp(startScale,
			                                         endScale,
			                                         t);
			yield return null;
		}
		GameObject.Destroy (from);
		to.GetComponent<CanvasGroup> ().alpha = 1.0f;
		yield break;
	}

	public void ResetTimer()
	{
		playTime = 0.0f;
		clockStopped = true;
	}

	public void DisableText()
	{
		timeText.enabled = false;
	}

	public void EnableText()
	{
		timeText.enabled = true;
	}

	// Use this for initialization
	void Start () {
		playerTimeZone = GameObject.Find ("LevelTimesZone");
		if (!playerTimeZone) {
			Debug.LogError("Cannot find layout zone for level times, disabling level time UI");
			shouldAddLevelTimes=false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		timeText.text = string.Format ("Time\n {0}", playTime.ToString("F2"));
		if (!clockStopped) {
			playTime+=Time.deltaTime;
		}
	}
}
