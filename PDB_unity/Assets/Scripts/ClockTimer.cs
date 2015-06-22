using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net;
public class ClockTimer : MonoBehaviour {


	public Text timeText;

	float playTime=0.0f;
	bool clockStopped=true;


	public List<float> playerLevelTimes = new List<float>();

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
	
	}
	
	// Update is called once per frame
	void Update () {
		timeText.text = string.Format ("Time : {0}", playTime.ToString("F2"));
		if (!clockStopped) {
			playTime+=Time.deltaTime;
		}
	}
}
