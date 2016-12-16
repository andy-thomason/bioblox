using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public int current_level = -1;
    public GameObject loading_panel;
    public GameObject MenuButtons;
    public int id_user;
    DataManager dm;
    public string[] level_scores;
    public int number_of_level;
    public GameObject selection_panel;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        Application.ExternalCall("SendUserData");
    }
    
    public void ChangeLevel(int level)
    {
        loading_panel.SetActive(true);
        FindObjectOfType<EndLevelScript>().selection_panel.SetActive(false);
        current_level = level;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void SetID()
    {
        id_user = int.Parse(FindObjectOfType<DataManager>().id_user);
    }
}
