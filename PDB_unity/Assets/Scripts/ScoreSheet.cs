using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AssemblyCSharp;

public class ScoreSheet : MonoBehaviour {

	struct UserData
	{
		public UserData(string lvlName, string name, string post, float scoreVal)
		{
			levelName = lvlName;
			initials = name;
			postcode = post;
			score = scoreVal;
		}
		//the level name is limited to 40 chars
		public string levelName;
		public string initials;
		public string postcode;
		public float score;
	}

	//the address for the php script that adds a score ot the database
	const string addScoreScriptAddress = "doc.gold.ac.uk/~acurt001/AddScore.php?";
	const string bestScoresOnLevelAddress = "doc.gold.ac.uk/~acurt001/BestOfLevel.php?";
	const string bestScoresOnLevelTodayAddress = "doc.gold.ac.uk/~acurt001/BestTodayOfLevel.php?";

	string currentUserInitial;
	string currentUserAreacode;

	enum SCORE_STATE{
		USER_REGISTERED=0,
		USER_ANON=1,
	};

	SCORE_STATE state = SCORE_STATE.USER_ANON;

	BioBlox bioblox;
	ClockTimer scoreTimer;

	public InputField initialField;
	public InputField areacodeField;
	public Text scoreFlavourtext;
	public Text scoreText;
	public Text initialsText;

	public GameObject submitUI;
	public GameObject continueUI;
	
	List<float> bestLevelTimes;
	List<float> bestLevelTimesToday;

	float timeout = 8.0f;

	int numComplete=0;
	
	//This function registers a new score with the score data provided
	IEnumerator AddScore(UserData userScoreData)
	{
		string hash = HashUtility.Md5Sum (userScoreData.levelName + userScoreData.initials + 
		                                  userScoreData.postcode + userScoreData.score.ToString() + "plznohackkthx");
		//Debug.Log (hash);
		
		WWW php_print = new WWW (addScoreScriptAddress +
		                         "level=" + WWW.EscapeURL(userScoreData.levelName) + "&name=" + userScoreData.initials +
		                         "&post=" + userScoreData.postcode + "&score=" + userScoreData.score + "&hash="+hash);
		float timeoutTimer = timeout;
		while (php_print.isDone == false) {
			timeoutTimer -=Time.deltaTime;
			if(timeoutTimer<0)
			{
				yield break;
			}
			yield return null;
		}
		yield return php_print;
		//Debug.Log ("Request Done");
		if (php_print.error != null) {
			Debug.Log ("Error!");
			Debug.Log (php_print.error);
		}
		yield break;
	}



	IEnumerator GetBestLevelTimes(bool onlyTodaysScores)
	{
		string levelName = bioblox.GetCurrentLevelName ();
		string hash = HashUtility.Md5Sum (levelName + "plznohackkthx");

		string address = bestScoresOnLevelAddress;
		if (onlyTodaysScores) {
			address = bestScoresOnLevelTodayAddress;
		}

		WWW php_print = new WWW (address +
			"level=" + WWW.EscapeURL (levelName) + "&hash=" + hash);
		float timeoutTimer = timeout;
		while (php_print.isDone == false) {
			timeoutTimer -=Time.deltaTime;
			if(timeoutTimer<0)
			{
				yield break;
			}
			yield return null;
		}

		if (php_print.error == null) {
			Debug.Log (php_print.text);
			if(!onlyTodaysScores)
			{
				bestLevelTimes = ParseCSV_f(php_print.text);
			}
			else
			{
				bestLevelTimesToday = ParseCSV_f(php_print.text);
			}
			numComplete++;
		} else {
			Debug.Log ("Error!");
			Debug.Log (php_print.error);
		}
		yield break;
	}

	List<float> ParseCSV_f(string s)
	{
		List<float> items = new List<float> ();
		string[] subStrings = s.Split (',');
		foreach (string str in subStrings) {
			if(str.Length!=0)
			{
			items.Add(float.Parse(str));
			}
		}
		return items;
	}


	// Use this for initialization
	void Start () {
	}

	public void ScorePlayer()
	{
		gameObject.SetActive (true);

		bioblox = GameObject.Find ("BioBlox").GetComponent<BioBlox> ();
		scoreTimer = bioblox.gameObject.GetComponent<ClockTimer> ();

		scoreText.text = scoreTimer.GetLastPlayerTime ().ToString();
		StartCoroutine("GetScoreFlavourText");
		if (state == SCORE_STATE.USER_REGISTERED) {
			submitUI.SetActive (false);
			continueUI.SetActive (true);
		} else if (state == SCORE_STATE.USER_ANON) {
			submitUI.SetActive (true);
			continueUI.SetActive (false);
		}
	}

	public void DEBUGGetBestToday()
	{
		StartCoroutine ("GetBestLevelTimes", false);
	}

	IEnumerator GetScoreFlavourText()
	{
		StartCoroutine ("GetBestLevelTimes", false);
		StartCoroutine ("GetBestLevelTimes", true);

		scoreFlavourtext.text = "Comparing scores on database...";

		while (numComplete != 2)
		{
			yield return null;
		}
		float playerScore = scoreTimer.GetLastPlayerTime ();

		//best level score ever
		if ( (bestLevelTimes.Count != 0 && playerScore < bestLevelTimes [0]) || bestLevelTimes.Count == 0) {
			scoreFlavourtext.text = "That is the new level reccord! Well done.";
			yield break;
		}
		else if(playerScore < bestLevelTimes[bestLevelTimes.Count - 1])
		{
			//1st is score 0 and 2cnd is score 1, add 1 to position
			int position = bestLevelTimes.Count + 1;
			string positionAppend = "th";
			for(int i = bestLevelTimes.Count - 1; i > 0; --i)
			{
				if(playerScore < bestLevelTimes[i])
				{
					continue;
				}
				position = i + 1;
			}
			if(position == 1)
			{
				positionAppend = "cnd";
			}
			else if(position == 2)
			{
				positionAppend = "rd";
			}
			scoreFlavourtext.text = "Bravo. Your score is in " + position.ToString() + positionAppend + " place of all Time!";
			yield break;
		}
		//best level score today
		if ((bestLevelTimesToday.Count != 0 && playerScore < bestLevelTimesToday [0]) || bestLevelTimesToday.Count == 0) {
			scoreFlavourtext.text = "Thats the best score today! Awesome!";
			yield break;
		} else if (playerScore < bestLevelTimesToday [bestLevelTimesToday.Count - 1]) {
			int position = bestLevelTimesToday.Count + 1;
			string positionAppend = "th";
			for(int i = bestLevelTimesToday.Count - 1; i > 0; --i)
			{
				if(playerScore < bestLevelTimesToday[i])
				{
					continue;
				}
				position = i + 1;
			}
			if(position == 1)
			{
				positionAppend = "cnd";
			}
			else if(position == 2)
			{
				positionAppend = "rd";
			}
			scoreFlavourtext.text = "Bravo. Your score is in " + position.ToString() + positionAppend + " place today. Keep it up!";
			yield break;
		}
		//get some scores for the server and find something interesting to say
		//best in area
		//beat your last score
		//etc
		scoreFlavourtext.text = "Keep trying!";
	}

	bool CheckAreaCodeValidity(string areacode)
	{
		//not sure how to do this just yet
		return true;
	}

	bool ReadInput()
	{
		string initial = "";
		string area = "";

		if (initialField.text.Length ==3) {
			initial = initialField.text;
		} else {
			initialField.gameObject.GetComponent<Image>().color = Color.red;
			return false;
		}
		if (areacodeField.text.Length > 0) {
			area = areacodeField.text;
		}
		initialField.gameObject.GetComponent<Image> ().color = Color.white;
		areacodeField.gameObject.GetComponent<Image> ().color = Color.white;
		currentUserInitial = initial;
		if (area != null) {
			currentUserAreacode = area;
		}
		initialsText.text = currentUserInitial;
		return true;
	}

	public void SubmitScoreAndContinue()
	{
		if (state == SCORE_STATE.USER_REGISTERED) {
			UserData data = new UserData (bioblox.GetCurrentLevelName (),
		                             currentUserInitial,
		                             currentUserAreacode,
		                             scoreTimer.GetLastPlayerTime ());
			StartCoroutine ("AddScore", data);
			Continue ();
		} else {
			ResetUser();
			Continue();
		}

	}

	public void SubmitInfoAndContinue()
	{
		if (ReadInput()) {
			state=SCORE_STATE.USER_REGISTERED;
			SubmitScoreAndContinue();
		}
	}

	public void Continue()
	{
		gameObject.SetActive (false);
		//bioblox.Reload ();
	}

	public void ResetUser()
	{
		currentUserInitial = "";
		currentUserAreacode = "";
		state = SCORE_STATE.USER_ANON;
		submitUI.SetActive (true);
		continueUI.SetActive (false);
		initialsText.text = "ANO";
	}
	// Update is called once per frame
	void Update () {
	
	}
}
