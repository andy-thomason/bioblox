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
    public Text ranking_user;
    public Text total_number_users;
    public Transform ranking_container;
    WWWForm www_form;

    public bool is_tutorial = false;

    public GameObject welcome_tutorial;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        sfx = FindObjectOfType<SFX>();

        DidYouKnow.text = loading_facts[Random.Range(0, 11)];

        ////if NOT DEMO, GET THE USER ID
        //if (!FindObjectOfType<BioBlox>().isDemo)
        //{
        //    Application.ExternalCall("SendUserData");
        //}
        //else // DISABLE THE REST OF THE LEVELS / IS DONE HERE BECAUSE IS ONLY ONCE
        //{
        //    id_user = -1;
        //    Transform level_holder = GameObject.FindGameObjectWithTag("level_holder").transform;
        //    int button_child = level_holder.GetChild(0).transform.childCount - 1;

        //    for(int i = 1; i < level_holder.childCount; i++)
        //    {
        //        level_holder.GetChild(i).transform.GetChild(button_child).gameObject.SetActive(false);
        //        level_holder.GetChild(i).transform.GetChild(button_child - 1).GetComponent<LevelSelectionButton>().enabled = false;
        //        level_holder.GetChild(i).transform.GetChild(button_child - 1).gameObject.AddComponent<RedirectToLogin>();
        //        level_holder.GetChild(i).transform.GetChild(button_child - 1).transform.GetChild(0).GetComponent<Text>().text = "Log in to play";
        //    }
        //    //disable load slots from the first level
        //    level_holder.GetChild(0).transform.GetChild(button_child).gameObject.SetActive(false);
        //    //disable ranking
        //    ranking_user.transform.parent.gameObject.SetActive(false);
        //    //if demo show leaderboard
        //    load_leaderboard();
        //}
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

    public void Custom_ChangeLevel()
    {
        loading_panel.SetActive(true);
        selection_panel.alpha = 0;
        selection_panel.blocksRaycasts = false;
        current_level = -2;
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
        //LOAD LEAVERBOARD
        load_leaderboard();
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

    //GET THE LEADERBOARD
    public void load_leaderboard()
    {
        www_form = new WWWForm();
        www_form.AddField("id_user", id_user);
        StartCoroutine(get_leaderboard());
    }

    IEnumerator get_leaderboard()
    {
        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/leaderboard.php", www_form);
        yield return SQLQuery;

        //split the leaderboard
        string[] splitScores = (SQLQuery.text).Split('/');
        int user_rank = int.Parse(splitScores[1]) - 1;
        total_number_users.text = splitScores[0];
        ranking_user.text = splitScores[1];

        string[] splitScores_each = (splitScores[2]).Split('*');
        //Debug.Log(SQLQuery.text);
        //Debug.Log(splitScores.Length);
        //EmptyLeaderboard();

        //dont highlight
        if (FindObjectOfType<BioBlox>().isDemo)
            user_rank = -1;

        //fill the scores
        for (int i = 0; i < splitScores_each.Length - 1; i++)
        {
            string[] splitScores_slot = splitScores_each[i].Split('+');
            ranking_container.GetChild(i).GetChild(1).GetComponent<Text>().text = splitScores_slot[0];
            ranking_container.GetChild(i).GetChild(2).GetComponent<Text>().text = splitScores_slot[1];

            if(i == user_rank)
            {
                ranking_container.GetChild(i).GetComponent<Image>().color = default_background_color;
            }
        }
    }

    public void close_welcome_tutorial()
    {
        welcome_tutorial.SetActive(false);
    }

    public void start_tutorial()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);

        is_tutorial = true;
        ChangeLevel(0, -1);
        tutorial_panel.SetActive(false);
        welcome_tutorial.SetActive(false);
        open_selection_once_at_start = true;
        switch_to_game_mode();
    }

    public void caca()
    {
        StartCoroutine(cacaca());
    }
    IEnumerator cacaca()
    {
        //using (WWW www = new WWW("https://files.rcsb.org/view/2PTC.pdb"))
        //using (WWW www = new WWW("http://82.15.223.84/pdb_file_merged.pdb"))
        using (WWW www = new WWW("http://quiley.com/pdb_file_merged.pdb"))
        {
            yield return www;

            if (www.error != null)
                throw new System.Exception("WWW download had an error:" + www.error);

            //pdb_file = www.text;
            //PDB_parser.caca(www.text);
            FindObjectOfType<PDBCustom>().pdb_file = www.text;
            Debug.Log(FindObjectOfType<PDBCustom>().pdb_file);
        }
        
        GetComponent<GameManager>().Custom_ChangeLevel();
    }
}
