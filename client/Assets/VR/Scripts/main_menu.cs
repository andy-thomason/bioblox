using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        SceneManager.LoadScene(0, LoadSceneMode.Single);

    }
}
