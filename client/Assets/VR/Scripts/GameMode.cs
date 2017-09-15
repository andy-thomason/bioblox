using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMode : MonoBehaviour {

    //2 minutes gameplay
    public float set_game_play_time;
    float game_play_time;
    GameManager gm;
    BioBlox bb;
    SFX sfx;
    public bool game_over = false;
    public bool win = false;
    public Text timer_text;
    public GameObject menu;
    public GameObject lost_object;
    public Sprite almost_sprite;
    public Sprite fail_sprite;
    public GameObject win_object;
    float timer_win = 0;
    bool timer_win_condition = false;

    // Use this for initialization
    void Start ()
    {
        gm = FindObjectOfType<GameManager>();
        game_play_time = set_game_play_time;
        bb = FindObjectOfType<BioBlox>();
        sfx = FindObjectOfType<SFX>();

        if(gm.is_game_mode)
        {
            game_play_time = set_game_play_time;
            timer_text.gameObject.SetActive(true);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(gm.is_game_mode && !game_over && !win)
        {
            game_play_time -= Time.deltaTime;
            string minutes = Mathf.Floor(game_play_time / 60).ToString("00");
            string seconds = Mathf.Floor(game_play_time % 60).ToString("00");
            timer_text.text = minutes + ":" + seconds;

            if (game_play_time < 1.0f)
                game_over_call();

            if (bb.bar_value == 1.0f && bb.is_score_valid)
            {
                timer_win += Time.deltaTime;

                if(timer_win > 3 && !timer_win_condition)
                {
                    win_call();
                    timer_win_condition = true;
                }
            }
            else
            {
                timer_win = 0;
            }
        }
		
	}

    public void switch_mode(bool status)
    {
        if(status)
        {
            game_play_time = set_game_play_time;
            timer_text.gameObject.SetActive(true);
        }
        else
        {
            timer_text.gameObject.SetActive(false);
        }
    }

    void game_over_call()
    {
        sfx.Mute_Track(SFX.sound_index.warning, true);
        game_over = true;
        VRGrabObject[] hands = FindObjectsOfType<VRGrabObject>();
        hands[0].ReleaseObject_0();
        hands[0].ReleaseObject_1();
        hands[1].ReleaseObject_0();
        hands[1].ReleaseObject_1();
        bb.Molecules.gameObject.SetActive(false);
        menu.SetActive(false);
        timer_text.gameObject.SetActive(false);
        sfx.StopTrack(SFX.sound_index.warning);

        if (bb.bar_value > 0.7f)
        {
            lost_object.GetComponent<Image>().sprite = almost_sprite;
        }
        else
        {
            lost_object.GetComponent<Image>().sprite = fail_sprite;
        }

        sfx.PlayTrack(SFX.sound_index.almost);

        lost_object.SetActive(true);
        bb.overlaping_r_h.active = bb.overlaping_l_h.active = false;
    }

    void win_call()
    {
        sfx.Mute_Track(SFX.sound_index.warning, true);
        win = true;
        //timer_text.gameObject.SetActive(false);
        menu.SetActive(false);
        win_object.SetActive(true);
        sfx.PlayTrack(SFX.sound_index.well_done);
        bb.overlaping_r_h.active = bb.overlaping_l_h.active = false;
    }


    public void restart()
    {
        timer_text.gameObject.SetActive(true);
        win_object.SetActive(false);
        bb.Molecules.gameObject.SetActive(true);
        lost_object.SetActive(false);
        menu.SetActive(true);
        bb.restart_position();
        game_play_time = set_game_play_time;
        game_over = false;
        win = false;
    }
}
