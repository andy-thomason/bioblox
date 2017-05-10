using UnityEngine;
using UnityEngine.SceneManagement;

public class main_menu : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        FindObjectOfType<SFX>().Mute_Track(SFX.sound_index.warning, true);
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        SceneManager.LoadScene(0, LoadSceneMode.Single);

    }
}
