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
    public Transform corner_down;

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

    public CanvasGroup hold_p1;
    public CanvasGroup hold_p2;

    List<CanvasGroup> tutorial_cg = new List<CanvasGroup>();

    int tutorial_step = -1;

    Transform background_tutorial;
    enum background_size { medium_size, short_size };
    enum background_position { down, up, left, right };

    UIController ui;
    BioBlox bb;

    // Use this for initialization
    void Start ()
    {
        background_tutorial = GameObject.FindGameObjectWithTag("background_tutorial").transform;

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

        add_connection_pos = add_connection.GetChild(add_connection.childCount - 1).transform;
        add_connection_cg = add_connection.GetComponent<CanvasGroup>();
        tutorial_cg.Add(add_connection_cg);
        viz_atoms_contact_pos = viz_atoms_contact.GetChild(viz_atoms_contact.childCount - 1).transform;
        viz_atoms_contact_cg = viz_atoms_contact.GetComponent<CanvasGroup>();
        tutorial_cg.Add(viz_atoms_contact_cg);

        game_score_bar_pos = game_score_bar.GetChild(game_score_bar.childCount - 1).transform;
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
        transform.position = new Vector3(6000.0f, 0, 0);


        ui = FindObjectOfType<UIController>();
        bb = FindObjectOfType<BioBlox>();
        sound_cg = GameObject.FindGameObjectWithTag("music_boton").GetComponent<CanvasGroup>();
        tutorial_cg.Add(sound_cg);
        sound_fx_cg = GameObject.FindGameObjectWithTag("sfx_boton").GetComponent<CanvasGroup>();
        tutorial_cg.Add(sound_fx_cg);

        //tutorial_step = 0;
        foreach (CanvasGroup cg in tutorial_cg)
            HideCanvasGroupElement(cg);

        tutorial_is_on = true;
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


            // ** Main Menus functionalities ** //


            case 1: //MENU FROM THE TOP / SOUNDS
                {
                    ShowCanvasGroupElement(background_top_menu);
                    ShowCanvasGroupElement(sound_fx_cg);
                    ShowCanvasGroupElement(sound_cg);
                    transform.position = sound_pos.position;
                    set_background(background_position.down, background_size.medium_size, "This is the Control Menu. Here you can adjust some visual parameters of the interface and receive some help during the gameplay. Press (    ) and (    )  to turn off/ on the volume of music and effects respectively.");
                    Debug.Log("MENU FROM THE TOP / SOUNDS");
                }
                break;

            case 2: //MENU FROM THE TOP / GRID
                {
                    ShowCanvasGroupElement(grid_cg);
                    transform.position = grid_pos.position;
                    set_background(background_position.down, background_size.short_size, "Press (    ) to activate or deactivate the grid.");
                    Debug.Log("MENU FROM THE TOP / GRID");
                }
                break;

            case 3: //MENU FROM THE TOP / BACKGROUND COLOR
                {
                    ShowCanvasGroupElement(bg_color_cg);
                    ShowCanvasGroupElement(bg_color_boton_cg);
                    transform.position = bg_color_pos.position;
                    set_background(background_position.down, background_size.short_size, "Press (    ) to change the background colour. Try it picking one colour of your preference. ");
                    Debug.Log("MENU FROM THE TOP / BACKGROUND COLOR");
                }
                break;

            case 4: //MENU FROM THE TOP / RESET CAMERA
                {
                    ShowCanvasGroupElement(reset_camera_cg);
                    transform.position = reset_camera_pos.position;
                    set_background(background_position.down, background_size.short_size, "Press (    ) to rest the camera to the frontal view. Try moving the camera and then press the button. ");
                    Debug.Log("MENU FROM THE TOP / RESET CAMERA");
                }
                break;

            case 5: //MENU FROM THE TOP / CONTROL PANEL
                {
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    if(bb.is_hint_moving)
                    {
                        bb.StartHintMovement();
                    }
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK

                    ShowCanvasGroupElement(control_panel_cg);
                    transform.position = control_panel_pos.position;
                    set_background(background_position.down, background_size.short_size, "Press (    ) to display the main controls. Very useful if you forget them. ");
                    Debug.Log("MENU FROM THE TOP / CONTROL PANEL");
                }
                break;

            case 6: //MENU FROM THE TOP / DOCK ANIMATION
                {
                    ShowCanvasGroupElement(dock_example_cg);
                    transform.position = dock_example_pos.position;
                    set_background(background_position.down, background_size.short_size, "Press (    ) to display an animated preview of a correct protein dock of the level.");
                    Debug.Log("MENU FROM THE TOP / DOCK ANIMATION");
                }
                break;


            // ** The workspaceand camera ** //


            case 7: //MOUSE CONTROLS / ZOOM
                {
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    if (bb.is_hint_moving)
                    {
                        bb.StartHintMovement();
                    }
                    //DEACTIVATE THE DOCKING ANIMATION IN CASE THE USER GOES BACK
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, "Very good! now we know you know the functionalities of the Control menu, you will learn how to manipulate the proteins and move the camera to explore the workspace from different angles.To zoom in and out the camera, scroll themouse wheel. Remember, you can always reset the camera position, pressing (     ) in the Control Menu.");
                    Debug.Log("MOUSE CONTROLS / ZOOM");
                }
                break;

            case 8: //MOUSE CONTROLS / ROTATE CAMERA
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, "You can turn the camera to see the proteins from different angles. Simply, Left-click in the background and drag the mouse to turn the camera. Remember, you can always reset the camera position, pressing (     ) in the Control Menu.");
                    Debug.Log("MOUSE CONTROLS / ROTATE CAMERA");
                }
                break;

            case 9: //MOUSE CONTROLS / KEYBOARD
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, "You can also pan the camera. Press any of the keyboard arrows to move the camera in that direction.");
                    Debug.Log("MOUSE CONTROLS / KEYBOARD");
                }
                break;

            case 10: //MOUSE CONTROLS / ROTATE PROTEINS
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, "You can rotate a single protein model to see it from different angles, simple hold a Left-click on it and move.");
                    Debug.Log("MOUSE CONTROLS / ROTATE PROTEINS");
                }
                break;


            // ** Exploring Proteins ** //


            case 11: //Exploring Proteins INTRO
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, "Brilliant! now you can manipulate the protein models, the camera and the main menus of the game, it is time to learn how to explore the proteins, its amino acids and atoms, and make connections among them.");
                    Debug.Log("Exploring Proteins INTRO");
                }
                break;
            case 12: //PROTEIN PANELS
                {
                    ShowCanvasGroupElement(protein_panel_1_cg);
                    ShowCanvasGroupElement(protein_panel_2_cg);
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.medium_size, "First, let’s start with the Protein Panels. This two panels shows the proteins content. There’s one for each protein in the level. Please,place the cursor over the protein models to see which panel gets highlighted with red and blue. Now you can easily identify which panel correspond to each protein model.");
                    Debug.Log("PROTEIN PANELS");
                }
                break;

            case 13: //PROTEIN PANELS DESCRIPTION
                {
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.short_size, "The Protein Panels are divided in 3 parts: Name, Protein Panel, View Panel. We will describe them briefly for now, but don’t worry, later, we’re going to work with them. ");
                    Debug.Log("PROTEIN PANELS DESCRIPTION");
                }
                break;

            case 14: //PROTEIN PANELS NAME
                {
                    transform.position = protein_panel_name.position;
                    set_background(background_position.right, background_size.short_size, "First, at the top of each panel, you can see the protein’s name, in this case 2PTC.E and 2PTC.I respectively.");
                    Debug.Log("PROTEIN PANELS NAME");
                }
                break;

            case 15: //PROTEIN PANELS RENDERERS
                {
                    ShowCanvasGroupElement(protein_views_1_cg);
                    ShowCanvasGroupElement(protein_views_2_cg);
                    transform.position = protein_views_1_pos.position;
                    set_background(background_position.right, background_size.short_size, "To explore the protein model, you can switch among visualizations using theVisualization Section in the Protein Panel. Please, Left-click over the four different visualization modes. You will notice that in some modes you can see all the atoms and other allow you to see the shape of the protein. During the game you can switch any time the visualization modes to facilitate your task. ");
                    Debug.Log("PROTEIN PANELS RENDERERS");
                }
                break;

            case 16: //PROTEIN PANELS AMINO
                {
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.right, background_size.short_size, "The main area is the Aminoacid Panel. Here you can explore the content of each protein, selecting a specific amino acid of its chain.");
                    Debug.Log("PROTEIN PANELS AMINO");
                }
                break;

            case 17: //SELECT AMINO FROM 3D MODEL
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, "Now, Left-click over a protein, picking any part of it. You will see that the amino acid will appear in the protein and the Aminoacid Panel in the Protein Panel will highlight in green the selected amino acid chain.");
                    Debug.Log("SELECT AMINO FROM 3D MODEL");
                }
                break;

            case 18: //SELECT AMINO FROM AMINO PANEL
                {
                    transform.position = protein_panel_1_pos.position;
                    set_background(background_position.right, background_size.medium_size, "Now, go to the Amonoacid Panel and select different amino acids from the list Left-click on them. You will notice that different amino acid appears in the protein model, as you navigate the list.");
                    Debug.Log("SELECT AMINO FROM AMINO PANEL");
                }
                break;

            case 19: //SELECT ATOM FROM AMINO PANEL
                {
                    transform.position = panel_atom.position;
                    set_background(background_position.right, background_size.medium_size, "Additionally, you can open each amino acid from the Aminoacid Boardby clicking (    ) to open the Atom List. Use this to explore and change the active atom of the selected amino acid. You will notice the active atom is highlighted in green in both Atom list and the protein model.");
                    Debug.Log("SELECT ATOM FROM AMINO PANEL");
                }
                break;

            case 20: //SELECT DIFFERENT REDNER
                {
                    transform.position = protein_views_2.position;
                    set_background(background_position.right, background_size.medium_size, "To explore the protein model, you can switch among visualizations using theVisualization Section in the Protein Panel. Please, Left-click over the four different visualization modes. You will notice that in some modes you can see all the atoms and other allow you to see the shape of the protein. During the game you can switch any time the visualization modes to facilitate your task.");
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
                    set_background(background_position.left, background_size.medium_size, "Excellent! now you can explore a protein, let’s make some connections! To make a connection you need to select two amino acids, one from each protein model. You can select them either from the protein model or the protein panel.You will notice the amino acid are selected when they are highlighted in green.");
                    Debug.Log("SELECT AMINO FOR COONECTION");
                }
                break;

            case 22: //CREATE CONNECTION
                {
                    ShowCanvasGroupElement(add_connection_cg);
                    transform.position = add_connection.position;
                    set_background(background_position.up, background_size.short_size, "Create a connection pressing (                         )");
                    Debug.Log("CREATE CONNECTION");
                }
                break;

            case 23: //CONNECTION PANEL DESCRIPTION
                {
                    transform.position = connections_holder.position;
                    set_background(background_position.left, background_size.medium_size, "You will also notice that this panel has appeared next to the score bar. This is a Connection Panel. It appears each time you create a connection. So, if you have 2 connections you will have 2 Connection Panels.");
                    Debug.Log("CONNECTION PANEL DESCRIPTION");
                }
                break;

            case 24: //CONNECTION PANEL FURTHER DESCRIPTION
                {
                    transform.position = connections_holder.position;
                    set_background(background_position.left, background_size.medium_size, "This panel, indicates the two amino acids connected and allows you to edit the connection, changing both amino acids and their respective atoms. This panel allows you to explore other amino acids in the Protein Panel, without losing the existing connection. ");
                    Debug.Log("CONNECTION PANEL FURTHER DESCRIPTION");
                }
                break;

            case 25: //CONNECTION PANEL ERASE LINK
                {
                    transform.position = connection_panel_erase.position;
                    set_background(background_position.left, background_size.short_size, "The panel will remain on the screen until you delete the connection, pressing the (                   ) button. In this tutorial, the button is deactivated. ");
                    Debug.Log("CONNECTION PANEL ERASE LINK");
                }
                break;

            case 26: //SLIDER
                {
                    ShowCanvasGroupElement(slider_string_cg);
                    transform.position = slider_string_pos.position;
                    set_background(background_position.up, background_size.short_size, "Now you have the connection established, please drag the Slider (                     ) to join the protein models. You will see how the proteins gets closer and collide with each other. ");
                    Debug.Log("SLIDER");
                }
                break;

            case 27: //SCORE AND DELETE LINK
                {
                    //DEACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_no_delete_link = false;
                    //DEACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    ShowCanvasGroupElement(game_score_bar_cg);
                    transform.position = game_score_bar.position;
                    set_background(background_position.left, background_size.medium_size, "The Score Bar indicates the quality of the connection. You see the score is 0%?. That’s because not all the connections are equally compatible or efficient. Let’s try another connection. Please delete the existing connections clicking the button in the Connection Panel. ");
                    Debug.Log("SCORE AND DELETE LINK");
                }
                break;

            case 28: //SELECT CORRECT FIRST PAIR OF AMINO
                {
                    transform.position = protein_panels.position;
                    set_background(background_position.right, background_size.medium_size, "Good!, now, go to theProtein Panel and select the following amino acids: SER E 190 and RYS I 15 from 2PTC.E and 2PTC.I respectively. ");
                    Debug.Log("SELECT CORRECT FIRST PAIR OF AMINO");
                }
                break;

            case 29: //CREATE CONNECTION
                {
                    transform.position = add_connection.position;
                    set_background(background_position.up, background_size.short_size, "Create a connection pressing (                         )");
                    Debug.Log("CREATE CONNECTION");
                }
                break;

            case 30: //EXPLANATION OF CONTEXTUAL MENU
                {
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.medium_size, "Now, you will learn another way to create a connection, using a Contextual Panel. This panel appears when you Right-click over an amino acid label in the Aminoacid Panel or over the protein model, suggesting possible connections. That connections are very effective.");
                    Debug.Log("EXPLANATION OF CONTEXTUAL MENU");
                }
                break;

            case 31: //OPEN CONTEXTUAL MENU
                {
                    transform.position = protein_panel_2_pos.position;
                    set_background(background_position.left, background_size.short_size, "Please search the amino acid TYR E 39 in the protein 2PTC.E and Right-click over it.");
                    Debug.Log("EXPLANATION OF CONTEXTUAL MENU");
                }
                break;

            case 32: //CREATE CONNECTION CONTEXTUAL MENU
                {
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_score_fixed = true;
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    transform.position = corner_down.position;
                    set_background(background_position.left, background_size.short_size, "In the Contextual Panel, select ARG I 17/O, and press (             ) to make a connection without needing to search for the amino acid in the second Protein Panel.");
                    Debug.Log("CREATE CONNECTION CONTEXTUAL MENU");
                }
                break;

            case 33: //PULL TOGETHER
                {
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    tutorial_score_fixed = false;
                    //ACTIVATE THE ERASE BOTON IN THE CONNECTION PANEL
                    transform.position = add_connection_pos.position;
                    set_background(background_position.up, background_size.short_size, "Now you have 2 connections, drag the slider (             ) to join both proteins.");
                    Debug.Log("PULL TOGETHER");
                }
                break;

            case 34: //SCORE GOING UP
                {
                    
                    transform.position = game_score_bar_pos.position;
                    set_background(background_position.left, background_size.short_size, "Now you see the score is 80%. Excellent job!");
                    Debug.Log("SCORE GOING UP");
                }
                break;


            case 35: //CONTROL MENU
                {
                    ShowCanvasGroupElement(save_bottom_cg);
                    transform.position = save_bottom.position;
                    set_background(background_position.down, background_size.short_size, "This is the Control Menu.Here you can adjust some visual parameters of the interface and receive some help during the gameplay.Press() and()  to turn off/ on the volume of music and effects respectively.");
                    Debug.Log("CONTROL MENU");
                }
                break;

        }
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
            //ERASE TEXTS
            background.GetChild(0).GetChild(0).GetComponent<Text>().text = "";
            background.GetChild(1).GetChild(0).GetComponent<Text>().text = "";
            background.gameObject.SetActive(false);
        }
    }

    void set_background(background_position bp, background_size bs, string text)
    {
        deactivate_background();
        background_tutorial.GetChild(bp.GetHashCode()).gameObject.SetActive(true);
        background_tutorial.GetChild(bp.GetHashCode()).GetChild(bs.GetHashCode()).gameObject.SetActive(true);
        background_tutorial.GetChild(bp.GetHashCode()).GetChild(bs.GetHashCode()).GetChild(0).GetComponent<Text>().text = text;
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