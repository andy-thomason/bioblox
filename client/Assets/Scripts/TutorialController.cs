using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TutorialController : MonoBehaviour {

    public bool tutorial_is_on = false;
    public bool tutorial_score_fixed = false;
    public bool tutorial_camera_fixed = false;
    public bool tutorial_no_delete_link = false;

    public Transform cutaway;
    Transform cutaway_pos;
    CanvasGroup cutaway_cg;

    public CanvasGroup protein_panel_1_cg;
    public Transform protein_panel_1_pos;
    public CanvasGroup protein_panel_2_cg;
    public Transform protein_panel_2_pos;

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
    CanvasGroup add_connection_cg;
    public Transform viz_atoms_contact;
    Transform viz_atoms_contact_pos;
    CanvasGroup viz_atoms_contact_cg;

    public Transform game_score_bar;
    CanvasGroup game_score_bar_cg;

    public Transform connections_holder;
    
    public Transform save_bottom;
    Transform save_bottom_pos;
    CanvasGroup save_bottom_cg;

    public Transform mouse_instructions;
    public Transform protein_panels;
    public Transform protein_panel_name_0;
    public Transform protein_panel_name_1;
    public Transform protein_panel_hold;
    public Transform protein_hover_2;
    public Transform protein_hover_1;
    public Transform panel_atom;
    public Transform connection_panel;
    public Transform connection_panel_amino;
    public Transform connection_panel_atom;
    public Transform connection_panel_erase;
    public Transform corner_down;
    public Transform viz_0;
    public Transform viz_1;

    CanvasGroup sound_cg;
    CanvasGroup sound_fx_cg;
    public CanvasGroup grid_cg;
    public CanvasGroup bg_color_cg;
    public CanvasGroup bg_color_boton_cg;
    public CanvasGroup reset_camera_cg;
    public CanvasGroup control_panel_cg;
    public CanvasGroup dock_example_cg;
    public CanvasGroup background_top_menu;

    public Transform sound_pos;
    public Transform grid_pos;
    public Transform bg_color_pos;
    public Transform reset_camera_pos;
    public Transform control_panel_pos;
    public Transform dock_example_pos;
    public Transform add_connection_pos;
    public Transform game_score_bar_pos;

    public CanvasGroup hold_p1;
    public CanvasGroup hold_p2;

    List<CanvasGroup> tutorial_cg = new List<CanvasGroup>();

    public int tutorial_step = -1;

    Transform background_tutorial;
    enum background_size { large_size, medium_size, short_size };
    enum background_position { down, up, left, right };

    UIController ui;
    BioBlox bb;
    AminoSliderController asc;

    public Button hint_button;
    Image hint_button_color;

    int[] hand_rotation_degrees = { 0, 180, 270, 90};
    Transform hand_rotation;
    public Sprite hand_alt;
    public Sprite hand_normal;
    Image hand;

    public GameObject intro_tutorial;
    public GameObject outro_tutorial;

    Animator hand_animation;

    public Sprite[] tutorial_texts;

    public Color default_color;
    public Color high_color;

    public Transform extra_hand;

    // Use this for initialization
    void Start ()
    {
        background_tutorial = GameObject.FindGameObjectWithTag("background_tutorial").transform;
        hand_rotation = transform.GetChild(0);
        hand = transform.GetComponentInChildren<Image>();

        tutorial_cg.Add(grid_cg);
        tutorial_cg.Add(bg_color_cg);
        tutorial_cg.Add(bg_color_boton_cg);
        tutorial_cg.Add(reset_camera_cg);
        tutorial_cg.Add(control_panel_cg);
        tutorial_cg.Add(dock_example_cg);
        tutorial_cg.Add(background_top_menu);

        tutorial_cg.Add(hold_p1);
        tutorial_cg.Add(hold_p2);

        cutaway_pos = cutaway.GetChild(cutaway.childCount-1).transform;
        cutaway_cg = cutaway.GetComponent<CanvasGroup>();
        tutorial_cg.Add(cutaway_cg);
        
        tutorial_cg.Add(protein_panel_1_cg);
        tutorial_cg.Add(protein_panel_2_cg);

        protein_views_1_pos = protein_views_1.GetChild(protein_views_1.childCount - 1).transform;
        protein_views_1_cg = protein_views_1.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_views_1_cg);
        protein_views_2_pos = protein_views_2.GetChild(protein_views_2.childCount - 1).transform;
        protein_views_2_cg = protein_views_2.GetComponent<CanvasGroup>();
        tutorial_cg.Add(protein_views_2_cg);

        slider_string_pos = slider_string.GetChild(slider_string.childCount - 1).transform;
        slider_string_cg = slider_string.GetComponent<CanvasGroup>();
        tutorial_cg.Add(slider_string_cg);
        
        add_connection_cg = add_connection.GetComponent<CanvasGroup>();
        tutorial_cg.Add(add_connection_cg);
        viz_atoms_contact_pos = viz_atoms_contact.GetChild(viz_atoms_contact.childCount - 1).transform;
        viz_atoms_contact_cg = viz_atoms_contact.GetComponent<CanvasGroup>();
        tutorial_cg.Add(viz_atoms_contact_cg);
        
        game_score_bar_cg = game_score_bar.GetComponent<CanvasGroup>();
        tutorial_cg.Add(game_score_bar_cg);

        save_bottom_pos = save_bottom.parent.GetChild(save_bottom.parent.childCount - 1).transform;
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

    public void StartTutorial()
    {
        tutorial_step = 0;
        deactivate_background();
        hand_animation = transform.GetComponentInChildren<Animator>();


        ui = FindObjectOfType<UIController>();
        bb = FindObjectOfType<BioBlox>();
        asc = FindObjectOfType<AminoSliderController>();
        sound_cg = GameObject.FindGameObjectWithTag("music_boton").GetComponent<CanvasGroup>();
        tutorial_cg.Add(sound_cg);
        sound_fx_cg = GameObject.FindGameObjectWithTag("sfx_boton").GetComponent<CanvasGroup>();
        tutorial_cg.Add(sound_fx_cg);

        //tutorial_step = 0;
        foreach (CanvasGroup cg in tutorial_cg)
            HideCanvasGroupElement(cg);

        tutorial_is_on = true;
        advance_tutorial();
    }


    void advance_tutorial()
    {
        ui.isOverUI = false;
        //TUTORIAL STEPS
        switch (tutorial_step)
        {
            case 0: //WELCOME + PRESENTATION BIOBLOX
                {
                    transform.position = new Vector3(2000, 0, 0);
                    hand_animation.enabled = false;
                    Debug.Log("WELCOME + PRESENTATION BIOBLOX");
                    intro_tutorial.SetActive(true);


                    HideCanvasGroupElement(background_top_menu);
                    HideCanvasGroupElement(sound_fx_cg);
                    HideCanvasGroupElement(sound_cg);
                }
                break;


            // ** Main Menus functionalities ** //


            case 1: //MENU FROM THE TOP / SOUNDS
                {
                    transform.GetComponentInChildren<Animator>().enabled = true;
                    intro_tutorial.SetActive(false);
                    ShowCanvasGroupElement(background_top_menu);
                    ShowCanvasGroupElement(sound_fx_cg);
                    ShowCanvasGroupElement(sound_cg);
                    transform.position = sound_pos.position;
                    set_background(background_position.down, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / SOUNDS");
                    
                    HideCanvasGroupElement(grid_cg);
                }
                break;

            case 2: //MENU FROM THE TOP / GRID
                {
                    ShowCanvasGroupElement(grid_cg);
                    transform.position = grid_pos.position;
                    set_background(background_position.down, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / GRID");

                    HideCanvasGroupElement(bg_color_cg);
                    HideCanvasGroupElement(bg_color_boton_cg);
                }
                break;

            case 3: //MENU FROM THE TOP / BACKGROUND COLOR
                {
                    ShowCanvasGroupElement(bg_color_cg);
                    ShowCanvasGroupElement(bg_color_boton_cg);
                    transform.position = bg_color_pos.position;
                    set_background(background_position.down, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / BACKGROUND COLOR");

                    HideCanvasGroupElement(reset_camera_cg);
                }
                break;

            case 4: //MENU FROM THE TOP / RESET CAMERA
                {
                    ShowCanvasGroupElement(reset_camera_cg);
                    transform.position = reset_camera_pos.position;
                    set_background(background_position.down, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / RESET CAMERA");

                    HideCanvasGroupElement(control_panel_cg);
                }
                break;

            case 5: //MENU FROM THE TOP / CONTROL PANEL
                {
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    if(bb.is_hint_moving)
                    {
                        hint_button.onClick.Invoke();
                        hint_button_color.color = new Color(255, 255, 255);
                    }
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK

                    ShowCanvasGroupElement(control_panel_cg);
                    transform.position = control_panel_pos.position;
                    set_background(background_position.down, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / CONTROL PANEL");

                    HideCanvasGroupElement(dock_example_cg);
                }
                break;

            case 6: //MENU FROM THE TOP / DOCK ANIMATION
                {
                    ShowCanvasGroupElement(dock_example_cg);
                    transform.position = dock_example_pos.position;
                    set_background(background_position.down, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MENU FROM THE TOP / DOCK ANIMATION");
                }
                break;


            // ** The workspaceand camera ** //


            case 7: //MOUSE CONTROLS / ZOOM
                {
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    if (bb.is_hint_moving)
                    {
                        hint_button.onClick.Invoke();
                        hint_button_color.color = new Color(255, 255, 255);
                    }
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MOUSE CONTROLS / ZOOM");
                }
                break;

            case 8: //MOUSE CONTROLS / ROTATE CAMERA
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MOUSE CONTROLS / ROTATE CAMERA");
                }
                break;

            case 9: //MOUSE CONTROLS / KEYBOARD
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MOUSE CONTROLS / KEYBOARD");
                }
                break;

            case 10: //MOUSE CONTROLS / ROTATE PROTEINS
                {
                    ui.RestartCamera();
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("MOUSE CONTROLS / ROTATE PROTEINS");
                }
                break;


            // ** Exploring Proteins ** //


            case 11: //Exploring Proteins INTRO
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("Exploring Proteins INTRO");
                    
                    HideCanvasGroupElement(protein_panel_1_cg);
                    HideCanvasGroupElement(protein_panel_2_cg);
                }
                break;
            case 12: //PROTEIN PANELS
                {
                    ui.RestartCamera();
                    ShowCanvasGroupElement(protein_panel_1_cg);
                    ShowCanvasGroupElement(protein_panel_2_cg);
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PROTEIN PANELS");
                }
                break;

            case 13: //PROTEIN PANELS DESCRIPTION
                {
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PROTEIN PANELS DESCRIPTION");
                    extra_hand.position = new Vector3(2000, 0, 0);
                }
                break;

            case 14: //PROTEIN PANELS NAME
                {
                    transform.position = protein_panel_name_1.position;
                    position_extra_hand(background_position.right, protein_panel_name_0);
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PROTEIN PANELS NAME");
                    
                    HideCanvasGroupElement(protein_views_1_cg);
                    HideCanvasGroupElement(protein_views_2_cg);
                }
                break;

            case 15: //PROTEIN PANELS RENDERERS
                {
                    ShowCanvasGroupElement(protein_views_1_cg);
                    ShowCanvasGroupElement(protein_views_2_cg);
                    transform.position = viz_1.position;
                    position_extra_hand(background_position.right, viz_0);
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PROTEIN PANELS RENDERERS");
                }
                break;

            case 16: //PROTEIN PANELS AMINO
                {
                    transform.position = protein_panel_2_pos.position;
                    position_extra_hand(background_position.right, protein_panel_1_pos);
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PROTEIN PANELS AMINO");
                }
                break;

            case 17: //SELECT AMINO FROM 3D MODEL
                {
                    extra_hand.position = new Vector3(2000, 0, 0);
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT AMINO FROM 3D MODEL");
                }
                break;

            case 18: //SELECT AMINO FROM AMINO PANEL
                {
                    transform.position = protein_panel_1_pos.position;
                    position_extra_hand(background_position.right, protein_panel_2_pos);
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT AMINO FROM AMINO PANEL");
                }
                break;

            case 19: //SELECT ATOM FROM AMINO PANEL
                {
                    transform.position = panel_atom.position;
                    set_background(background_position.right, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT ATOM FROM AMINO PANEL");
                    extra_hand.position = new Vector3(2000, 0, 0);
                }
                break;

            case 20: //SELECT DIFFERENT REDNER
                {
                    transform.position = viz_1.position;
                    position_extra_hand(background_position.right, viz_0);
                    set_background(background_position.right, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT DIFFERENT REDNER");
                }
                break;


            // ** Make Connections ** //


            case 21: //SELECT AMINO FOR COONECTION
                {
                    //SET SCORE AND DELETE LINKS DISABLE
                    tutorial_score_fixed = true;
                    tutorial_no_delete_link = true;
                    //SET SCORE AND DELETE LINKS DISABLE
                    //SET TO NORMAL RENDER
                    ui.ToggleNormalMesh(0);
                    ui.ToggleNormalMesh(1);
                    //SET TO NORMAL RENDER
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT AMINO FOR COONECTION");

                    extra_hand.position = new Vector3(2000, 0, 0);
                    HideCanvasGroupElement(add_connection_cg);
                }
                break;

            case 22: //CREATE CONNECTION
                {
                    ShowCanvasGroupElement(add_connection_cg);
                    transform.position = add_connection_pos.position;
                    set_background(background_position.up, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CREATE CONNECTION");
                }
                break;

            case 23: //CONNECTION PANEL DESCRIPTION
                {
                    transform.position = connection_panel.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CONNECTION PANEL DESCRIPTION");
                }
                break;

            case 24: //CONNECTION PANEL FURTHER DESCRIPTION
                {
                    transform.position = connection_panel.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CONNECTION PANEL FURTHER DESCRIPTION");
                }
                break;

            case 25: //CONNECTION PANEL ERASE LINK
                {
                    transform.position = connection_panel_erase.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CONNECTION PANEL ERASE LINK");
                    
                    HideCanvasGroupElement(slider_string_cg);
                }
                break;

            case 26: //SLIDER
                {
                    ShowCanvasGroupElement(slider_string_cg);
                    transform.position = slider_string_pos.position;
                    set_background(background_position.up, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SLIDER");
                    
                    HideCanvasGroupElement(game_score_bar_cg);
                }
                break;

            case 27: //SCORE AND DELETE LINK
                {
                    asc.SliderMol[0].transform.GetChild(169).GetComponent<Image>().color = default_color;
                    asc.SliderMol[1].transform.GetChild(14).GetComponent<Image>().color = default_color;
                    //DEACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_no_delete_link = false;
                    //DEACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    ShowCanvasGroupElement(game_score_bar_cg);
                    transform.position = game_score_bar_pos.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SCORE AND DELETE LINK");
                    extra_hand.position = new Vector3(2000, 0, 0);
                }
                break;

            case 28: //SELECT CORRECT FIRST PAIR OF AMINO
                {
                    asc.SliderMol[0].transform.GetChild(169).GetComponent<Image>().color = high_color;
                    asc.SliderMol[1].transform.GetChild(14).GetComponent<Image>().color = high_color;
                    asc.ScrollbarAmino1.value = 0.222f;
                    asc.ScrollbarAmino2.value = 0.766f;
                    //asc.SliderMol[0]. amino lys 15 child 14

                    position_extra_hand(background_position.right, protein_panel_1_pos);
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SELECT CORRECT FIRST PAIR OF AMINO");
                }
                break;

            case 29: //CREATE CONNECTION
                {
                    asc.SliderMol[0].transform.GetChild(169).GetComponent<Image>().color = default_color;
                    asc.SliderMol[1].transform.GetChild(14).GetComponent<Image>().color = default_color;
                    transform.position = add_connection_pos.position;
                    set_background(background_position.up, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CREATE CONNECTION");
                    extra_hand.position = new Vector3(2000, 0, 0);
                }
                break;

            case 30: //EXPLANATION OF CONTEXTUAL MENU
                {
                    asc.SliderMol[0].transform.GetChild(21).GetComponent<Image>().color = default_color;
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("EXPLANATION OF CONTEXTUAL MENU");
                }
                break;

            case 31: //OPEN CONTEXTUAL MENU
                {
                    //child 21
                    asc.SliderMol[0].transform.GetChild(21).GetComponent<Image>().color = high_color;
                    asc.ScrollbarAmino1.value = 0.91f;
                    transform.position = protein_panel_1_pos.position;
                    set_background(background_position.right, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("EXPLANATION OF CONTEXTUAL MENU");
                }
                break;

            case 32: //CREATE CONNECTION CONTEXTUAL MENU
                {
                    asc.SliderMol[0].transform.GetChild(21).GetComponent<Image>().color = default_color;
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_score_fixed = true;
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CREATE CONNECTION CONTEXTUAL MENU");
                }
                break;

            case 33: //PULL TOGETHER
                {
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_score_fixed = false;
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    transform.position = slider_string_pos.position;
                    set_background(background_position.up, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("PULL TOGETHER");
                }
                break;

            case 34: //SCORE GOING UP
                {
                    ui.CutAway.value = -100;
                    transform.position = game_score_bar_pos.position;
                    set_background(background_position.left, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("SCORE GOING UP");

                    HideCanvasGroupElement(cutaway_cg);
                }
                break;


            case 35: //cutaway
                {
                    ShowCanvasGroupElement(cutaway_cg);
                    transform.position = cutaway_pos.position;
                    set_background(background_position.down, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CUTAWAY");

                    HideCanvasGroupElement(viz_atoms_contact_cg);
                }
                break;

            case 36: //CONTACT VIZ
                {
                    ui.CutAway.value = -100;
                    ShowCanvasGroupElement(viz_atoms_contact_cg);
                    transform.position = viz_atoms_contact_pos.position;
                    set_background(background_position.up, background_size.short_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CONTACT VIZ");

                    HideCanvasGroupElement(save_bottom_cg);
                }
                break;

            case 37: //CONTROL MENU
                {
                    outro_tutorial.SetActive(false);
                    ShowCanvasGroupElement(save_bottom_cg);
                    transform.position = save_bottom_pos.position;
                    set_background(background_position.down, background_size.medium_size, tutorial_texts[tutorial_step]);
                    Debug.Log("CONTROL MENU");
                }
                break;

            case 38: //CHAO
                {

                    outro_tutorial.SetActive(true);
                    hand_animation.enabled = false;
                    transform.position = new Vector3(2000, 0, 0);
                }
                break;

        }

        ui.isOverUI = false;
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
            background.GetChild(2).gameObject.SetActive(false);
            //ERASE TEXTS
            //background.GetChild(0).GetChild(0).GetComponent<Text>().text = "";
            //background.GetChild(1).GetChild(0).GetComponent<Text>().text = "";
            //background.GetChild(2).GetChild(0).GetComponent<Text>().text = "";
            background.gameObject.SetActive(false);
        }
    }

    void set_background(background_position bp, background_size bs, Sprite text)
    {
        ui.isOverUI = false;
        deactivate_background();
        background_tutorial.GetChild(bp.GetHashCode()).gameObject.SetActive(true);
        background_tutorial.GetChild(bp.GetHashCode()).GetChild(bs.GetHashCode()).gameObject.SetActive(true);
        background_tutorial.GetChild(bp.GetHashCode()).GetChild(bs.GetHashCode()).GetChild(0).GetComponent<Image>().sprite = text;
        //set hand rotation and sprite
        hand_rotation.rotation = Quaternion.AngleAxis(hand_rotation_degrees[bp.GetHashCode()], Vector3.forward);
        if (bp == background_position.right)
            hand.sprite = hand_alt;
        else
            hand.sprite = hand_normal;
        //hand_model.sprite = hand_image[bp.GetHashCode()];
    }

    void position_extra_hand(background_position bp, Transform element)
    {
        extra_hand.position = element.position;
        //set hand rotation and sprite
        extra_hand.rotation = Quaternion.AngleAxis(hand_rotation_degrees[bp.GetHashCode()], Vector3.forward);
        if (bp == background_position.right)
            extra_hand.GetComponentInChildren<Image>().sprite = hand_alt;
        else
            extra_hand.GetComponentInChildren<Image>().sprite = hand_normal;
    }

    public void next_step()
    {
        tutorial_step++;
        advance_tutorial();
    }

    public void back_step()
    {
        if(tutorial_step > 0)
        {
            tutorial_step--;
            advance_tutorial();
        }
    }

    public void exit_tutorial()
    {
        outro_tutorial.SetActive(false);
        GameManager gm = FindObjectOfType<GameManager>();
        gm.is_tutorial = false;
        tutorial_step = -1;
        gm.selection_panel.alpha = 1;
        gm.selection_panel.interactable = true;
        gm.selection_panel.blocksRaycasts = true;
    }
}
//case 2: // MOUSE INSTRUCTIONSS
//                {
//                    transform.position = mouse_instructions.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("MOUSE INSTRUCTIONS");
//                }
//                break;

//            // ** PANEL PROTEINS ** //

//            case 3: //SHOW PANELS
//                {
//                    ShowCanvasGroupElement(protein_panel_1_cg);
//                    ShowCanvasGroupElement(protein_panel_2_cg);
//transform.position = protein_panels.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("SHOW PANELS");
//                }
//                break;

//            case 4: //NAME PANEL PROTEIN
//                {
//                    transform.position = protein_panel_name.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("NAME PANEL PROTEIN");
//                }
//                break;

//            case 5: //AMINO PANEL PROTEIN
//                {
//                    tutorial_camera_fixed = false;
//                    transform.position = protein_panel_2_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("AMINO PANEL PROTEIN");
//                }
//                break;

//            case 6: //PROTEIN HOVER
//                {
//                    tutorial_camera_fixed = true;
//                    ui.RestartCamera();
//                    transform.position = protein_hover_2.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("PROTEIN HOVER");
//                }
//                break;

//            case 7: //PROTEIN 1 3D CLICK FOR AMINOS
//                {
//                    transform.position = protein_hover_1.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("PROTEIN 1 3D CLICK FOR AMINOS");
//                }
//                break;

//            case 8: //PROTEIN 2 PANEL CLICK FOR AMINOS
//                {
//                    tutorial_camera_fixed = false;
//                    transform.position = protein_panel_2_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("PROTEIN 2 PANEL CLICK FOR AMINOS");
//                }
//                break;

//            case 9: //PROTEIN 2 PANEL CLICK FOR ATOMS
//                {
//                    transform.position = panel_atom.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("PROTEIN 2 PANEL CLICK FOR ATOMS");
//                }
//                break;

//            case 10: //PROTEIN 1 PANEL VISUALIZATIONS
//                {
//                    transform.position = protein_views_1_pos.position;
//                    ShowCanvasGroupElement(protein_views_1_cg);
//                    ShowCanvasGroupElement(protein_views_2_cg);
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("PROTEIN 1 PANEL VISUALIZATIONS");
//                }
//                break;

//            // ** CONNECTIONS ** //

//            case 11: //CHANGE VIZ OF PROTEINS BEFORE CONNECTION
//                {
//                    transform.position = protein_views_2_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("CHANGE VIZ OF PROTEINS BEFORE CONNECTION");
//                }
//                break;

//            case 12: //SELECT AMINO P1
//                {
//                    transform.position = protein_panel_1_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("SELECT AMINO P1");
//                }
//                break;

//            case 13: //SELECT AMINO P2
//                {
//                    transform.position = protein_panel_2_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("SELECT AMINO P2");
//                }
//                break;

//            case 14: //CREATE CONNECTION
//                {
//                    transform.position = add_connection_pos.position;
//                    ShowCanvasGroupElement(add_connection_cg);
//                    set_background(background_position.up, background_size.short_size);
//Debug.Log("CREATE CONNECTION");
//                }
//                break;

//            case 15: //CONNECTION PANEL MACRO DESCRIPTION
//                {
//                    transform.position = connection_panel.position;
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("CONNECTION PANEL MACRO DESCRIPTION");
//                }
//                break;

//            case 16: //CONNECTION PANEL AMINO MODIFICATION 
//                {
//                    transform.position = connection_panel_amino.position;
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("CONNECTION PANEL AMINO MODIFICATION");
//                }
//                break;

//            case 17: //CONNECTION PANEL ATOM MODIFICATION 
//                {
//                    transform.position = connection_panel_atom.position;
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("CONNECTION PANEL ATOM MODIFICATION");
//                }
//                break;

//            case 18: //PULL SLIDER TOGETHER
//                {
//                    transform.position = slider_string_pos.position;
//                    ShowCanvasGroupElement(slider_string_cg);
//                    set_background(background_position.up, background_size.short_size);
//Debug.Log("PULL SLIDER TOGETHER");
//                }
//                break;

//            case 19: //SHOW SCORE 0
//                {
//                    tutorial_score_fixed = true;
//                    transform.position = game_score_bar_pos.position;
//                    ShowCanvasGroupElement(game_score_bar_cg);
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("SHOW SCORE 0");
//                }
//                break;

//            case 20: //ERASE CONNECTION
//                {
//                    transform.position = connection_panel_erase.position;
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("ERASE CONNECTION");
//                }
//                break;

//            case 21: //SELECT CORRECT AMINOS IN BOTH PROTEINS
//                {
//                    transform.position = protein_panels.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("SELECT CORRECT AMINOS IN BOTH PROTEINS");
//                }
//                break;

//            case 22: //CONNECT BOTH CORRECT AMINOS
//                {
//                    transform.position = add_connection_pos.position;
//                    set_background(background_position.up, background_size.short_size);
//Debug.Log("CONNECT BOTH CORRECT AMINOS");
//                }
//                break;

//            case 23: //CREATE CONNECTION THROUGH CONTEXTUAL PANEL
//                {
//                    transform.position = protein_panel_2_pos.position;
//                    set_background(background_position.right, background_size.short_size);
//Debug.Log("CREATE CONNECTION THROUGH CONTEXTUAL PANEL");
//                }
//                break;

//            case 24: //PULL SLIDER TOGETHER
//                {
//                    tutorial_score_fixed = false;
//                    transform.position = add_connection_pos.position;
//                    set_background(background_position.up, background_size.short_size);
//Debug.Log("PULL SLIDER TOGETHER");
//                }
//                break;

//            case 25: //SHOW SCORE
//                {
//                    transform.position = game_score_bar_pos.position;
//                    set_background(background_position.left, background_size.short_size);
//Debug.Log("SHOW SCORE");
//                }
//                break;

//            // ** TOOLS ** //

//            case 26: //SHOW CUTAWAY
//                {
//                    transform.position = cutaway_pos.position;
//                    set_background(background_position.down, background_size.short_size);
//                    ShowCanvasGroupElement(cutaway_cg);
//Debug.Log("SHOW CUTAWAY");
//                }
//                break;

//            case 27: //ATOMS IN CONTACT
//                {
//                    ui.CutAway.value = 0;
//                    transform.position = viz_atoms_contact_pos.position;
//                    set_background(background_position.up, background_size.short_size);
//                    ShowCanvasGroupElement(viz_atoms_contact_cg);
//Debug.Log("ATOMS IN CONTACT");
//                }
//                break;

//            case 28: //HOLD
//                {
//                    transform.position = protein_panel_hold.position;
//                    set_background(background_position.right, background_size.short_size);
//                    ShowCanvasGroupElement(hold_p1);
//                    ShowCanvasGroupElement(hold_p2);
//Debug.Log("HOLD");
//                }
//                break;