using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

	public List<AudioClip> clips;
	public List<string> names;

	public List<GameObject> sources;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		for (int i=0; i<sources.Count; ++i) {
			if(sources[i].GetComponent<AudioSource>().isPlaying==false)
			{
				GameObject.Destroy(sources[i]);
				sources.RemoveAt(i);
				i--;

			}
		}
	}

	void SpawnAudioObject(AudioClip clip,string name)
	{
		GameObject g = new GameObject ();
		g.name = "AudioClip" + name + sources.Count;
		g.AddComponent<AudioSource> ();
		AudioSource source = g.GetComponent<AudioSource> ();
		source.volume = 0.5f;
		source.clip = clip;
		source.Play ();
		sources.Add (g);
	}
	
	public AudioSource Play(string clipname)
	{
		for (int i=0; i<names.Count; ++i) {
			if (clipname == names [i]) {
				SpawnAudioObject(clips[i],names [i]);
				return sources[sources.Count-1].GetComponent<AudioSource>();
			}
		}
		return null;
	}

}
