using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public int current_level = -1;
    public int current_slot;
    public GameObject loading_panel;
    public GameObject MenuButtons;
    public int id_user;
    public string[] level_scores;
    public int number_of_level;
    public CanvasGroup selection_panel;
    public Text DidYouKnow;
    public string[] loading_facts;
    public GameObject tutorial_panel;
    SFX sfx;
    bool open_selection_once_at_start = false;
    public enum game_type_mode { science_mode, game_mode };
    public int game_type = 1;
    public Color deactivated_game_mode;
    public Image science_mode_image;
    public Image game_mode_image;
    public Color default_background_color;
    public Dropdown leader_board_dropdown;
    BioBlox bb;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        sfx = FindObjectOfType<SFX>();
        bb = FindObjectOfType<BioBlox>();

        DidYouKnow.text = loading_facts[Random.Range(0, 11)];

        //if NOT DEMO, GET THE USER ID
        if (!FindObjectOfType<BioBlox>().isDemo)
        {
            Application.ExternalCall("SendUserData");
        }
        else // DISABLE THE REST OF THE LEVELS / IS DONE HERE BECAUSE IS ONLY ONCE
        {
            id_user = -1;
            Transform level_holder = GameObject.FindGameObjectWithTag("level_holder").transform;
            int button_child = level_holder.GetChild(0).transform.childCount - 1;

            for(int i = 1; i < level_holder.childCount; i++)
            {
                level_holder.GetChild(i).transform.GetChild(button_child).gameObject.SetActive(false);
                level_holder.GetChild(i).transform.GetChild(button_child - 1).GetComponent<LevelSelectionButton>().enabled = false;
                level_holder.GetChild(i).transform.GetChild(button_child - 1).gameObject.AddComponent<RedirectToLogin>();
                level_holder.GetChild(i).transform.GetChild(button_child - 1).transform.GetChild(0).GetComponent<Text>().text = "Log in to play";
            }
            //disable load slots from the first level
            level_holder.GetChild(0).transform.GetChild(button_child).gameObject.SetActive(false);
            //level_holder.GetChild(0).transform.GetChild(button_child - 1).GetComponent<Button>().interactable = true;
            //level_holder.GetChild(0).transform.GetChild(button_child - 1).transform.GetChild(0).GetComponent<Text>().text = "Load";
        }
    }
    
    public void ChangeLevel(int level, int slot)
    {
        loading_panel.SetActive(true);
        selection_panel.alpha = 0;
        selection_panel.blocksRaycasts = false;
        current_level = level;
        current_slot = slot;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void EndLoading()
    {
        loading_panel.SetActive(false);
        FindObjectOfType<UIController>().isOverUI = false;
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
        if (!open_selection_once_at_start)
        {
            selection_panel.alpha = 1;
            selection_panel.interactable = true;
            open_selection_once_at_start = true;
        }
        FindObjectOfType<UIController>().isOverUI = false;
    }

    public void switch_to_science_mode()
    {
        science_mode_image.color = Color.white;
        game_type = game_type_mode.science_mode.GetHashCode();
        game_mode_image.color = deactivated_game_mode;
        FindObjectOfType<BioBlox>().SwitchScienceMode();
    }

    public void switch_to_game_mode()
    {
        game_mode_image.color = Color.white;
        game_type = game_type_mode.game_mode.GetHashCode();
        science_mode_image.color = deactivated_game_mode;
        FindObjectOfType<BioBlox>().SwitchGameMode();
    }
}
