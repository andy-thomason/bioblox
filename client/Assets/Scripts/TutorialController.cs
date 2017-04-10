using UnityEngine;
using System.Collections;

public class TutorialController : MonoBehaviour {

    public Transform cutaway;
    CanvasGroup cutaway_cg;

    public Transform protein_panel_1;
    CanvasGroup protein_panel_1_cg;
    public Transform protein_panel_2;
    CanvasGroup protein_panel_2_cg;

    public Transform protein_views_1;
    CanvasGroup protein_views_1_cg;
    public Transform protein_views_2;
    CanvasGroup protein_views_2_cg;


    public Transform slider_string;
    CanvasGroup slider_string_cg;
    public Transform add_connection;
    CanvasGroup add_connection_cg;
    public Transform viz_atoms_contact;
    CanvasGroup viz_atoms_contact_cg;

    public Transform game_score_bar;
    CanvasGroup game_score_bar_cg;

    public Transform connections_holder;
    CanvasGroup connections_holder_cg;

    public Transform save_bottom;
    CanvasGroup save_bottom_cg;

    CanvasGroup[] tutorial_cg = new CanvasGroup[11];

    int tutorial_step;

    // Use this for initialization
    void Start ()
    {
        cutaway_cg = cutaway.GetComponent<CanvasGroup>();
        tutorial_cg[0] = cutaway_cg;

        protein_panel_1_cg = protein_panel_1.GetComponent<CanvasGroup>();
        tutorial_cg[1] = protein_panel_1_cg;
        protein_panel_2_cg = protein_panel_2.GetComponent<CanvasGroup>();
        tutorial_cg[2] = protein_panel_2_cg;

        protein_views_1_cg = protein_views_1.GetComponent<CanvasGroup>();
        tutorial_cg[3] = protein_views_1_cg;
        protein_views_2_cg = protein_views_2.GetComponent<CanvasGroup>();
        tutorial_cg[4] = protein_views_2_cg;

        slider_string_cg = slider_string.GetComponent<CanvasGroup>();
        tutorial_cg[5] = slider_string_cg;

        add_connection_cg = add_connection.GetComponent<CanvasGroup>();
        tutorial_cg[6] = add_connection_cg;
        viz_atoms_contact_cg = viz_atoms_contact.GetComponent<CanvasGroup>();
        tutorial_cg[7] = viz_atoms_contact_cg;

        game_score_bar_cg = game_score_bar.GetComponent<CanvasGroup>();
        tutorial_cg[8] = game_score_bar_cg;

        connections_holder_cg = connections_holder.GetComponent<CanvasGroup>();
        tutorial_cg[9] = connections_holder_cg;

        save_bottom_cg = save_bottom.parent.GetComponent<CanvasGroup>();
        tutorial_cg[10] = save_bottom_cg;
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("step");
            ShowCanvasGroupElement(tutorial_cg[tutorial_step]);
            transform.position = tutorial_cg[tutorial_step].transform.position;
            tutorial_step++;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("empezo");
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        tutorial_step = 0;
        foreach (CanvasGroup cg in tutorial_cg)
            HideCanvasGroupElement(cg);

    }
    
    void HideCanvasGroupElement(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
    }

    void ShowCanvasGroupElement(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.blocksRaycasts = true;
    }
}
