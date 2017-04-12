using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour {

    public bool tutorial_is_on = false;
    public bool tutorial_score_fixed = false;
    public bool tutorial_camera_fixed = false;

    public Transform cutaway;
    Transform cutaway_pos;
    CanvasGroup cutaway_cg;

    public Transform protein_panel_1;
    Transform protein_panel_1_pos;
    CanvasGroup protein_panel_1_cg;
    public Transform protein_panel_2;
    Transform protein_panel_2_pos;
    CanvasGroup protein_panel_2_cg;

    public Transform protein_views_1;
    Transform protein_views_1_pos;
    CanvasGroup protein_views_1_cg;
    public Transform protein_views_2;
    Transform protein_views_2_pos;
    CanvasGroup protein_views_2_cg;


    public Transform slider_string;
    Transform slider_string_pos;
    CanvasGroup slider_string_cg;
    public Transform add_connection;
    Transform add_connection_pos;
    CanvasGroup add_connection_cg;
    public Transform viz_atoms_contact;
    Transform viz_atoms_contact_pos;
    CanvasGroup viz_atoms_contact_cg;

    public Transform game_score_bar;
    Transform game_score_bar_pos;
    CanvasGroup game_score_bar_cg;

    public Transform connections_holder;
    
    public Transform save_bottom;
    Transform save_bottom_pos;
    CanvasGroup save_bottom_cg;

    public Transform tool_panel;
    Transform tool_panel_pos;
    CanvasGroup tool_panel_cg;

    public Transform mouse_instructions;
    public Transform protein_panels;
    public Transform protein_panel_name;
    public Transform protein_panel_hold;
    public Transform protein_hover_2;
    public Transform protein_hover_1;
    public Transform panel_atom;
    public Transform connection_panel;
    public Transform connection_panel_amino;
    public Transform connection_panel_atom;
    public Transform connection_panel_erase;

    public CanvasGroup hold_p1;
    public CanvasGroup hold_p2;

    List<CanvasGroup> tutorial_cg = new List<CanvasGroup>();
    List<Transform> tutorial_pos = new List<Transform>();

    int tutorial_step = -1;

    Transform background_tutorial;
    enum background_size { medium_size, short_size };
    enum background_position { down, up, left, right };

    UIController ui;

    // Use this for initialization
    void Start ()
    {
        background_tutorial = GameObject.FindGameObjectWithTag("background_tutorial").transform;

        tool_panel_pos = tool_panel.GetChild(tool_panel.childCount - 1).transform;
        tutorial_pos.Add(tool_panel_pos);
        tool_panel_cg = tool_panel.GetComponent<CanvasGroup>();
        tutorial_cg.Add(tool_panel_cg);

        tutorial_cg.Add(hold_p1);
        tutorial_cg.Add(hold_p2);

        cutaway_pos = cutaway.GetChild(cutaway.childCount-1).transform;
        tutorial_pos.Add(cutaway_pos);
        cutaway_cg = cutaway.GetComponent<CanvasGroup>();
        tutorial_cg.Add(cutaway_cg);

        protein_panel_1_pos = protein_panel_1.GetChild(protein_panel_1.childCount - 1).transform;
        tutorial_pos.Add(protein_panel_1_pos);
        protein_panel_1_cg = protein_panel_1.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_panel_1_cg);
        protein_panel_2_pos = protein_panel_2.GetChild(protein_panel_2.childCount - 1).transform;
        tutorial_pos.Add(protein_panel_2_pos);
        protein_panel_2_cg = protein_panel_2.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_panel_2_cg);

        protein_views_1_pos = protein_views_1.GetChild(protein_views_1.childCount - 1).transform;
        tutorial_pos.Add(protein_views_1_pos);
        protein_views_1_cg = protein_views_1.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_views_1_cg);
        protein_views_2_pos = protein_views_2.GetChild(protein_views_2.childCount - 1).transform;
        tutorial_pos.Add(protein_views_2_pos);
        protein_views_2_cg = protein_views_2.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_views_2_cg);

        slider_string_pos = slider_string.GetChild(slider_string.childCount - 1).transform;
        tutorial_pos.Add(slider_string_pos);
        slider_string_cg = slider_string.GetComponent<CanvasGroup>();
        tutorial_cg.Add(slider_string_cg);

        add_connection_pos = add_connection.GetChild(add_connection.childCount - 1).transform;
        tutorial_pos.Add(add_connection_pos);
        add_connection_cg = add_connection.GetComponent<CanvasGroup>();
        tutorial_cg.Add(add_connection_cg);
        viz_atoms_contact_pos = viz_atoms_contact.GetChild(viz_atoms_contact.childCount - 1).transform;
        tutorial_pos.Add(viz_atoms_contact_pos);
        viz_atoms_contact_cg = viz_atoms_contact.GetComponent<CanvasGroup>();
        tutorial_cg.Add(viz_atoms_contact_cg);

        game_score_bar_pos = game_score_bar.GetChild(game_score_bar.childCount - 1).transform;
        tutorial_pos.Add(game_score_bar_pos);
        game_score_bar_cg = game_score_bar.GetComponent<CanvasGroup>();
        tutorial_cg.Add(game_score_bar_cg);

        save_bottom_pos = save_bottom.parent.GetChild(save_bottom.parent.childCount - 1).transform;
        tutorial_pos.Add(save_bottom_pos);
        save_bottom_cg = save_bottom.parent.GetComponent<CanvasGroup>();
        tutorial_cg.Add(save_bottom_cg);
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //ShowCanvasGroupElement(tutorial_cg[tutorial_step]);
            //transform.position = tutorial_pos[tutorial_step].position;
            tutorial_step++;
            advance_tutorial();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            //ShowCanvasGroupElement(tutorial_cg[tutorial_step]);
            //transform.position = tutorial_pos[tutorial_step].position;
            tutorial_step--;
            advance_tutorial();
        }

       
    }

    void advance_tutorial()
    {
        //TUTORIAL STEPS
        switch (tutorial_step)
        {
            case 0: //WELCOME + PRESENTATION BIOBLOX
                {
                    deactivate_background();
                    StartTutorial();
                    Debug.Log("WELCOME + PRESENTATION BIOBLOX");
                }
                break;

            case 1: //MENU FROM THE TOP
                {
                    ShowCanvasGroupElement(tool_panel_cg);
                    transform.position = tool_panel_pos.position;
                    set_background(background_position.down, background_size.short_size);
                    Debug.Log("MENU FROM THE TOP");
                }
                break;


            case 2: // MOUSE INSTRUCTIONSS
                {
                    transform.position = mouse_instructions.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("MOUSE INSTRUCTIONS");
                }
                break;

            // ** PANEL PROTEINS ** //

            case 3: //SHOW PANELS
                {
                    ShowCanvasGroupElement(protein_panel_1_cg);
                    ShowCanvasGroupElement(protein_panel_2_cg);
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("SHOW PANELS");
                }
                break;

            case 4: //NAME PANEL PROTEIN
                {
                    transform.position = protein_panel_name.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("NAME PANEL PROTEIN");
                }
                break;

            case 5: //AMINO PANEL PROTEIN
                {
                    tutorial_camera_fixed = false;
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("AMINO PANEL PROTEIN");
                }
                break;

            case 6: //PROTEIN HOVER
                {
                    tutorial_camera_fixed = true;
                    ui.RestartCamera();
                    transform.position = protein_hover_2.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("PROTEIN HOVER");
                }
                break;

            case 7: //PROTEIN 1 3D CLICK FOR AMINOS
                {
                    transform.position = protein_hover_1.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("PROTEIN 1 3D CLICK FOR AMINOS");
                }
                break;

            case 8: //PROTEIN 2 PANEL CLICK FOR AMINOS
                {
                    tutorial_camera_fixed = false;
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("PROTEIN 2 PANEL CLICK FOR AMINOS");
                }
                break;

            case 9: //PROTEIN 2 PANEL CLICK FOR ATOMS
                {
                    transform.position = panel_atom.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("PROTEIN 2 PANEL CLICK FOR ATOMS");
                }
                break;

            case 10: //PROTEIN 1 PANEL VISUALIZATIONS
                {
                    transform.position = protein_views_1_pos.position;
                    ShowCanvasGroupElement(protein_views_1_cg);
                    ShowCanvasGroupElement(protein_views_2_cg);
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("PROTEIN 1 PANEL VISUALIZATIONS");
                }
                break;

            // ** CONNECTIONS ** //

            case 11: //CHANGE VIZ OF PROTEINS BEFORE CONNECTION
                {
                    transform.position = protein_views_2_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("CHANGE VIZ OF PROTEINS BEFORE CONNECTION");
                }
                break;

            case 12: //SELECT AMINO P1
                {
                    transform.position = protein_panel_1_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("SELECT AMINO P1");
                }
                break;

            case 13: //SELECT AMINO P2
                {
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("SELECT AMINO P2");
                }
                break;

            case 14: //CREATE CONNECTION
                {
                    transform.position = add_connection_pos.position;
                    ShowCanvasGroupElement(add_connection_cg);
                    set_background(background_position.up, background_size.short_size);
                    Debug.Log("CREATE CONNECTION");
                }
                break;

            case 15: //CONNECTION PANEL MACRO DESCRIPTION
                {
                    transform.position = connection_panel.position;
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("CONNECTION PANEL MACRO DESCRIPTION");
                }
                break;

            case 16: //CONNECTION PANEL AMINO MODIFICATION 
                {
                    transform.position = connection_panel_amino.position;
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("CONNECTION PANEL AMINO MODIFICATION");
                }
                break;

            case 17: //CONNECTION PANEL ATOM MODIFICATION 
                {
                    transform.position = connection_panel_atom.position;
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("CONNECTION PANEL ATOM MODIFICATION");
                }
                break;

            case 18: //PULL SLIDER TOGETHER
                {
                    transform.position = slider_string_pos.position;
                    ShowCanvasGroupElement(slider_string_cg);
                    set_background(background_position.up, background_size.short_size);
                    Debug.Log("PULL SLIDER TOGETHER");
                }
                break;

            case 19: //SHOW SCORE 0
                {
                    tutorial_score_fixed = true;
                    transform.position = game_score_bar_pos.position;
                    ShowCanvasGroupElement(game_score_bar_cg);
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("SHOW SCORE 0");
                }
                break;

            case 20: //ERASE CONNECTION
                {
                    transform.position = connection_panel_erase.position;
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("ERASE CONNECTION");
                }
                break;

            case 21: //SELECT CORRECT AMINOS IN BOTH PROTEINS
                {
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("SELECT CORRECT AMINOS IN BOTH PROTEINS");
                }
                break;

            case 22: //CONNECT BOTH CORRECT AMINOS
                {
                    transform.position = add_connection_pos.position;
                    set_background(background_position.up, background_size.short_size);
                    Debug.Log("CONNECT BOTH CORRECT AMINOS");
                }
                break;

            case 23: //CREATE CONNECTION THROUGH CONTEXTUAL PANEL
                {
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size);
                    Debug.Log("CREATE CONNECTION THROUGH CONTEXTUAL PANEL");
                }
                break;

            case 24: //PULL SLIDER TOGETHER
                {
                    tutorial_score_fixed = false;
                    transform.position = add_connection_pos.position;
                    set_background(background_position.up, background_size.short_size);
                    Debug.Log("PULL SLIDER TOGETHER");
                }
                break;

            case 25: //SHOW SCORE
                {
                    transform.position = game_score_bar_pos.position;
                    set_background(background_position.left, background_size.short_size);
                    Debug.Log("SHOW SCORE");
                }
                break;

            // ** TOOLS ** //

            case 26: //SHOW CUTAWAY
                {
                    transform.position = cutaway_pos.position;
                    set_background(background_position.down, background_size.short_size);
                    ShowCanvasGroupElement(cutaway_cg);
                    Debug.Log("SHOW CUTAWAY");
                }
                break;

            case 27: //ATOMS IN CONTACT
                {
                    ui.CutAway.value = 0;
                    transform.position = viz_atoms_contact_pos.position;
                    set_background(background_position.up, background_size.short_size);
                    ShowCanvasGroupElement(viz_atoms_contact_cg);
                    Debug.Log("ATOMS IN CONTACT");
                }
                break;

            case 28: //HOLD
                {
                    transform.position = protein_panel_hold.position;
                    set_background(background_position.right, background_size.short_size);
                    ShowCanvasGroupElement(hold_p1);
                    ShowCanvasGroupElement(hold_p2);
                    Debug.Log("HOLD");
                }
                break;

            case 29: //SAVE-LOAD PANEL
                {
                    transform.position = save_bottom_pos.position;
                    set_background(background_position.down, background_size.short_size);
                    ShowCanvasGroupElement(save_bottom_cg);
                    Debug.Log("SAVE-LOAD PANEL");
                }
                break;

        }
    }

    public void StartTutorial()
    {
        transform.position = new Vector3(6000.0f, 0, 0);
        //tutorial_step = 0;
        foreach (CanvasGroup cg in tutorial_cg)
            HideCanvasGroupElement(cg);

        tutorial_is_on = true;

        ui = FindObjectOfType<UIController>();
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

    void deactivate_background()
    {
        foreach(Transform background in background_tutorial)
        {
            background.GetChild(0).gameObject.SetActive(false);
            background.GetChild(1).gameObject.SetActive(false);
            background.gameObject.SetActive(false);
        }
    }

    void set_background(background_position bp, background_size bs)
    {
        deactivate_background();
        background_tutorial.GetChild(bp.GetHashCode()).gameObject.SetActive(true);
        background_tutorial.GetChild(bp.GetHashCode()).GetChild(bs.GetHashCode()).gameObject.SetActive(true);
    }

    public void next_step()
    {
        tutorial_step++;
        advance_tutorial();
    }

    public void back_step()
    {
        tutorial_step--;
        advance_tutorial();
    }
}
