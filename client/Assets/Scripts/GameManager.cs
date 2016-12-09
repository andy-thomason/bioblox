using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public int current_level = -1;
    public GameObject loading_panel;
    public int id_user;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        Application.ExternalCall("SendUserData");
    }
    
    public void ChangeLevel(int level)
    {
        loading_panel.SetActive(true);
        current_level = level;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
