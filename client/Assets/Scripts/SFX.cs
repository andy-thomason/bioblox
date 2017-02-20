using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SFX : MonoBehaviour {

    public enum sound_index { main_audio, amb, amino_click, button_click, connection_click, protein_colliding, warning, end_level, string_reel_in, string_reel_out, harpoon_shoot, harpoon_hit_protein, harpoon_hit_ground, connection_exist, slider_mouse_in, slider_mouse_out, cutaway_start, cutaway_cutting, cutaway_protein, camera_shrink, camera_expand, ship, ship_scanning };
    public AudioClip ReelIn;
    public AudioClip ReelOut;
    AudioClip main_music;
    public AudioSource[] audioSource;
    public AudioSource[] audioSource_collision;
    // Use this for initialization

    void Awake ()
    {
        audioSource = GetComponents<AudioSource>();
        audioSource_collision = transform.GetChild(0).GetComponents<AudioSource>();
    }

    void Update()
    {
        if (audioSource[0].volume < 0.1f)
        {
            audioSource[0].volume += 0.005f;
        }
    }

    #region MUTE BUTTON
    bool is_audio_playing = true;
    bool is_sfx_playing = true;
    public Sprite audio_off;
    public Sprite audio_on;
    public Image audio_mute_image;

    public void MuteMusic()
    {
        PlayTrack(SFX.sound_index.button_click);
        Mute_Music(is_audio_playing);

        is_audio_playing = !is_audio_playing;
    }

    public void MuteSFX()
    {
        PlayTrack(SFX.sound_index.button_click);
        Mute_SFX(is_sfx_playing);

        is_sfx_playing = !is_sfx_playing;
    }
    #endregion

    public void PlayTrack(sound_index index)
    {
        //Debug.Log(index.GetHashCode());
        audioSource[index.GetHashCode()].Play();
    }

    public void PlayTrackChild(int index)
    {
        //Debug.Log(index.GetHashCode());
        audioSource_collision[index].Play();
    }

    public void StopTrack(sound_index index)
    {
        //Debug.Log(index.GetHashCode());
        audioSource[index.GetHashCode()].Stop();
    }

    public bool isPlaying(sound_index index)
    {
        return audioSource[index.GetHashCode()].isPlaying;
    }

    public bool isPlaying_collision(int index)
    {
        return audioSource_collision[index].isPlaying;
    }

    public void StopReel()
    {
        audioSource[sound_index.string_reel_out.GetHashCode()].Stop();
        audioSource[sound_index.slider_mouse_out.GetHashCode()].Play();
        FindObjectOfType<ConnectionManager>().scrolling_down = false;
        FindObjectOfType<ConnectionManager>().scrolling_up = false;
    }

    public void ReelSound(sound_index index)
    {
        if(index == sound_index.string_reel_in)
        {
            audioSource[sound_index.string_reel_out.GetHashCode()].clip = ReelIn;
            audioSource[sound_index.string_reel_out.GetHashCode()].Play();
        }
        else
        {
            audioSource[sound_index.string_reel_out.GetHashCode()].clip = ReelOut;
            audioSource[sound_index.string_reel_out.GetHashCode()].Play();
        }

    }

    public void SliderMouseIn()
    {
        audioSource[sound_index.slider_mouse_in.GetHashCode()].Play();
    }

    public void StopCutawaySFX()
    {
        audioSource[sound_index.cutaway_protein.GetHashCode()].Stop();
        audioSource[sound_index.slider_mouse_out.GetHashCode()].Play();
    }

    public void PlayTrackDelay(sound_index index, float delay)
    {
        audioSource[index.GetHashCode()].PlayDelayed(delay);
    }

    public void Mute_SFX(bool status)
    {
        foreach(AudioSource aus in audioSource)
        {
            aus.mute = status;
        }

        foreach (AudioSource aus in audioSource_collision)
        {
            aus.mute = status;
        }
        audioSource[0].mute = !is_audio_playing;
    }

    public void Mute_Music(bool status)
    {
        audioSource[0].mute = status;
    }
}
