using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public int current_level = -1;
    public GameObject loading_panel;
    public GameObject MenuButtons;
    public int id_user;
    DataManager dm;
    public string[] level_scores;
    public int number_of_level;
    public GameObject selection_panel;
    public Text DidYouKnow;
    public string[] loading_facts;
    public GameObject tutorial_panel;
    SFX sfx;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        sfx = FindObjectOfType<SFX>();

        DidYouKnow.text = loading_facts[Random.Range(0, 11)];

        //if NOT DEMO, GET THE USER ID
        if (!FindObjectOfType<BioBlox>().isDemo)
        {
            Application.ExternalCall("SendUserData");
        }
        else // DISABLE THE REST OF THE LEVELS / IS DONE HERE BECAUSE IS ONLY ONCE
        {
            Transform level_holder = GameObject.FindGameObjectWithTag("level_holder").transform;
            int button_child = level_holder.GetChild(0).transform.childCount - 1;
            foreach (Transform levels in level_holder)
            {
                levels.GetChild(button_child).gameObject.SetActive(false);
            }
            level_holder.GetChild(0).transform.GetChild(button_child).gameObject.SetActive(true);
        }
    }
    
    public void ChangeLevel(int level)
    {
        loading_panel.SetActive(true);
        selection_panel.SetActive(false);
        current_level = level;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void EndLoading()
    {
        loading_panel.SetActive(false);
        DidYouKnow.text = loading_facts[Random.Range(0, 11)];
    }

    public void SetID()
    {
        id_user = int.Parse(FindObjectOfType<DataManager>().id_user);
    }

    public void CloseTutorialPanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        tutorial_panel.SetActive(false);
    }
}
