using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BioBlox : MonoBehaviour
{
    // dictaing which gametype, puzzle or museum
    //bool simpleGame = true;

    //public int level;

    // controls whether the win state should attempt to fade out the molecules
    public bool winShouldFadeMol = false;

    // score card for saving scores. 
    //ScoreSheet scoreCard;

    // filenames for the levels, without the .txt
    //public List<string> filenames = new List<string> ();

    // the level we are currently on, incremented at the end of a level
    public int current_level = 0;

    struct Level {
      public string pdbFile;
      public string chainsA;
      public string chainsB;
      public string lod;
      public Vector3 offset;
      public float separation;

      public Level(string pdbFile, string chainsA, string chainsB, string lod, Vector3 offset, float separation) {
        this.pdbFile = pdbFile;
        this.chainsA = chainsA;
        this.chainsB =chainsB;
        this.lod = lod;
        this.offset = offset;
        this.separation = separation;
      }
    };

    Level[] levels = {
       new Level("2PTC", "E", "I", "1", new Vector3(0, 0, 0), 35),
       new Level("4KC3", "A", "B", "1", new Vector3(0, 0, 0), 40),
       new Level("1FSS", "A", "B", "1", new Vector3(-20, 0, 0), 40),
       new Level("1EMV", "A", "B", "1", new Vector3(-20, 0, 0), 40),
       new Level("1GRN", "A", "B", "1", new Vector3(-20, 0, 0), 40),
       new Level("1OHZ", "A", "B", "1", new Vector3(-20, 0, 0), 40)
    };
    
    // a holder variable for the current event system
    EventSystem eventSystem;
    //files
    //List<string> filename_levels = new List<string>();

    //levels name
//<<<<<<< HEAD
//    List<Tuple<string, string>> level_mesh_default = new List<Tuple<string, string>>();
//    List<Tuple<string, string>> level_mesh_bs = new List<Tuple<string, string>>();
//    List<Tuple<string, string>> level_mesh_carbon = new List<Tuple<string, string>>();
//=======
//    //List<Tuple<string, string>> level_mesh_default = new List<Tuple<string, string>>();
//    //List<Tuple<string, string>> level_mesh_bs = new List<Tuple<string, string>>();
//>>>>>>> chains
    //public Dictionary<string, string> level_mesh_default = new Dictionary<string, string>{
    //    {"0",  "1"}, {"4KC3_A_se_1",  "4KC3_B_se_1"}
    //};

    //public Dictionary<string, string> level_mesh_bs = new Dictionary<string, string>{
    //    {"2PTC_E_bs_1",  "2PTC_I_bs_1"}, {"4KC3_A_bs_1",  "4KC3_B_bs_1"}
    //};



    // the win and loose spash images
    //public GameObject winSplash;
    //public GameObject looseSplash;
    public GameObject goSplash;

    // a variable controlling the size of the area faded out during the win state
    public float shaderKVal = -0.03f;
    
    // a bool that will exit the win splash if set to true
    public bool exitWinSplash = false;
    
    // a list of win conditions where two atoms must be paired
    List<Tuple<int,int>> winCondition = new List<Tuple<int,int>> ();
    // the molecules in the scene
    //public GameObject[] prefab_molecules;
    //public GameObject[] prefab_molecules_bs;
    // the molecules in the scene
    public GameObject[] molecules;
    public BitArray[] atoms_touching;
    public BitArray[] atoms_bad;

    //  NOT CURRENTLY IN USE
    //  sites are smaller regions of the molecules that can be selected and manipulated independtly from the molecules
    GameObject[] sites = new GameObject[2];
    //  wheter the player is moving the molecules, playerIsMoving[0] being molecule[0]
    //bool[] playerIsMoving = new bool[2]{false,false};
    // the original positions of the molecules, used to provide a returning force during the puzzle mode
    Vector3[] originPosition = new Vector3[2];
    // game object target of the "popping" co-routines to shrink and grow the object out of and into the scene
    GameObject popTarget;

    //  score to achive to win
    public float winScore = 10.0f;
    //  torque applied to the molecule to move them when player drags
    public float uiScrollSpeed = 10.0f;

    //  force being applied to molecules to return them to their origin positions
    public float repulsiveForce = 30000.0f;
    //  force used for physics
    public float seperationForce = 10000.0f;
    //  force applied by string
    public float stringForce = 20000.0f;
    //private float ScoreScaleValue;

    public Slider rmsScoreSlider;
    public Slider heuristicScoreSlider;
    public Image heuristicScore;
    public Text GameScoreValue;
    public Text ElectricScore;
    public Text LennardScore;
    public Text NumberOfAtoms;
    public Text SimpleScore;
    public RectTransform HintTextPanel;
    //values for external reference
    public int electric_score;
    public int lennard_score;
    //public Slider overrideSlider;
    public Slider cutawaySlider;
    public Slider thicknessSlider;
    public GameObject invalidDockText;
    public GameObject InvalidDockScore;
    //public List<Slider> dockSliders = new List<Slider> ();
    public float dockOverrideOffset = 0.0f;
    //Animator of the tools menu
    public GameObject EndLevelMenu;

    //block flares prefabs
    //public GameObject FlareBlock1;
    //public GameObject FlareBlock2;

    // colors of the labels and an offset that is randomly decided randomize colours
    List<Color> colorPool = new List<Color>();
    //int randomColorPoolOffset;

    public Button lockButton;

    //public float triangleOffset = 10.0f;
    //GameObject[] featureTriangle =new GameObject[2];

    // shape scoring
    public int num_touching_0 = 0;
    public int num_touching_1 = 0;
    public int num_invalid = 0;
    public int num_connections = 0;

    public Camera MainCamera;
    LineRenderer line_renderer;
    Camera camera;
    public GameObject IntroCamera;
    UIController uiController;
    AminoSliderController aminoSlider;
    public Transform TutorialHand;
    float button_offset = 0.0f;
    public Transform SliderString;
    public Sprite default_hand;
    public Sprite slider_hand;
    public CanvasGroup ScorePanel;
    public GameObject SimpleScoretemp;
    public GameObject Filter;
    public GameObject HintText;
    public GameObject ExitTutorialButton;
    int[] meshes_offset = new int[] {-1,-2,-3};
    //public int mesh_offset_1;
    //public int mesh_offset_2;
    public bool is_score_valid;
    public float game_time = 0;
    bool playing = false;

    bool first_time = true;
    SFX sfx;
    //obkect where the molecules will be parrented
    public Transform Molecules;
    public Toggle ToggleMode;

    public enum GameState
    {
        Setup,
        Waiting,
        Picking,
        Docking,
        Locked
    }

    public enum GameStatus
    {
        MainScreen,
        GameScreen
    }

    public GameState game_state;
    public GameStatus game_status;

    public int hint_stage = 0;

    //scoring
    PDB_score scoring;

    // Use this for initialization
    void Start ()
    {
        uiController = FindObjectOfType<UIController>();
        sfx = FindObjectOfType<SFX>();
        //filenames 
        /*filename_levels.Add("2PTC");
        filename_levels.Add("4KC3");
        filename_levels.Add("1FSS");
        filename_levels.Add("1EMV");
        filename_levels.Add("1GRN");
        filename_levels.Add("1OHZ"); 
        filename_levels.Add("2VIR");
        //fill the levels

        //name of the meshes in Resources/Mesh - normal one with cutaway material
        level_mesh_default.Add(new Tuple<string, string>("0", "1"));
        level_mesh_default.Add(new Tuple<string, string>("4KC3_A_se_1", "4KC3_B_se_1"));
        level_mesh_default.Add(new Tuple<string, string>("1FSS_A_se_1", "1FSS_B_se_1"));
        level_mesh_default.Add(new Tuple<string, string>("1EMV_A_se_1", "1EMV_B_se_1"));
        level_mesh_default.Add(new Tuple<string, string>("1GRN_A_se_1", "1GRN_B_se_1"));
        level_mesh_default.Add(new Tuple<string, string>("1OHZ_A_se_1", "1OHZ_B_se_1"));
        level_mesh_default.Add(new Tuple<string, string>("2VIR_AB_se_1", "2VIR_C_se_1"));

        //name of the meshes in Resources/Mesh - ball and stick renderer
        level_mesh_bs.Add(new Tuple<string, string>("2PTC_E_bs_1", "2PTC_I_bs_1"));
        level_mesh_bs.Add(new Tuple<string, string>("4KC3_A_bs_1", "4KC3_B_bs_1"));
        level_mesh_bs.Add(new Tuple<string, string>("1FSS_A_bs_1", "1FSS_B_bs_1"));
        level_mesh_bs.Add(new Tuple<string, string>("1EMV_A_bs_1", "1EMV_B_bs_1"));
        level_mesh_bs.Add(new Tuple<string, string>("1GRN_A_bs_1", "1GRN_B_bs_1"));
        level_mesh_bs.Add(new Tuple<string, string>("1OHZ_A_bs_1", "1OHZ_B_bs_1"));
<<<<<<< HEAD
        level_mesh_bs.Add(new Tuple<string, string>("2VIR_AB_bs_1", "2VIR_C_bs_1"));
        //name of the meshes in Resources/Mesh - ball and stick renderer
        level_mesh_carbon.Add(new Tuple<string, string>("2PTC_E_ca_1", "2PTC_I_ca_1"));
        level_mesh_carbon.Add(new Tuple<string, string>("4KC3_A_ca_1", "4KC3_B_ca_1"));
        level_mesh_carbon.Add(new Tuple<string, string>("1FSS_A_ca_1", "1FSS_B_ca_1"));
        level_mesh_carbon.Add(new Tuple<string, string>("1EMV_A_ca_1", "1EMV_B_ca_1"));
        level_mesh_carbon.Add(new Tuple<string, string>("1GRN_A_ca_1", "1GRN_B_ca_1"));
        level_mesh_carbon.Add(new Tuple<string, string>("1OHZ_A_ca_1", "1OHZ_B_ca_1"));
        //level_mesh_carbon.Add(new Tuple<string, string>("2VIR_AB_ca_1", "2VIR_C_ca_1"));
=======
        level_mesh_bs.Add(new Tuple<string, string>("2VIR_AB_bs_1", "2VIR_C_bs_1"));*/
    //>>>>>>> chains
}

public void StartGame()
    {
        game_status = GameStatus.GameScreen;
        uiController.isOverUI = false;
        uiController.ToggleHintFromIntro();

        if (current_level == 0)
            ToggleMode.isOn = true;
        else
            ToggleMode.isOn = false;

        //if (uiController.previous_level == level)
        //    uiController.RestartLevel();

        game_time = 0;
        playing = true;
        //winSplash.SetActive (false);
        //looseSplash.SetActive (false);
        //goSplash.SetActive (false);

        game_state = GameState.Setup;

        Time.fixedDeltaTime = 0.033f;

        colorPool.Add(Color.red);
        colorPool.Add(Color.blue);
        colorPool.Add(Color.cyan);
        colorPool.Add(Color.green);
        colorPool.Add(Color.magenta);
        colorPool.Add(Color.yellow);
        colorPool.Add(Color.white);
        colorPool.Add(Color.gray);
        colorPool.Add(new Color(1.0f, 0.5f, 0.1f));
        //randomColorPoolOffset = 0; //Random.Range (0, colorPool.Count - 1);
        Debug.Log("Start");
        //filenames.Add ("jigsawBlue");

        //filenames.Add ("2W9G");

        //filenames.Add ("betabarrel_b");
        //filenames.Add("2ptc_u_new_edited");
        //empty fiels name
//<<<<<<< HEAD
        //filenames.Clear();
        ////set the fielanem
        //filenames.Add(filename_levels[level]);
        //set the hint image
//=======
        //filenames.Clear();
        //filenames.Add(filename_levels[level]);
//>>>>>>> chains


        StartCoroutine(game_loop());

        if (first_time)
        {

            eventSystem = EventSystem.current;
            //update
            line_renderer = GameObject.FindObjectOfType<LineRenderer>() as LineRenderer;
            camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            //first eprson
            aminoSlider = FindObjectOfType<AminoSliderController>();
            first_time = false;
        }
        aminoSlider.init();

        molecules[0].GetComponent<PDB_mesh>().DeselectAminoAcid();
        molecules[1].GetComponent<PDB_mesh>().DeselectAminoAcid();
        aminoSlider.DeselectAmino();

        //UI INIT
        uiController.init();

    }
    public string GetCurrentLevelName ()
    {
        return levels[current_level].pdbFile;
    }

    //converts an atom serial number (unique file identifier) into a index
    //also outputs the molecule index that that atom serial exists in
    int GetAtomIndexFromID (int id, out int moleculeNum)
    {
        moleculeNum = -1;
        if (molecules.Length == 2) {
            PDB_molecule mol1 = molecules [0].GetComponent<PDB_mesh> ().mol;
            PDB_molecule mol2 = molecules [1].GetComponent<PDB_mesh> ().mol;
            if (id >= mol1.serial_to_atom.Length) {
                moleculeNum = 1;
                return mol2.serial_to_atom [id];
            } else {
                moleculeNum = 0;
                return mol1.serial_to_atom [id];
            }
        }
        return -1;
    }

    //calculates a label positions within a dome infront of the molecules
    //stops labels from dissapering behind the molecules and uses the size of the largest collision sphere in the bvh
    public void GetLabelPos (List<int> atomIds, int molNum, Transform t)
    {
        Vector3 sumAtomPos = Vector3.zero;
        for (int i = 0; i < atomIds.Count; ++i) {
            sumAtomPos += GetAtomPos (atomIds [i], molNum);
        }
        sumAtomPos /= atomIds.Count;

        //if the molecule does not exist, or the atom id is bad
        if (molNum == -1 || molecules.Length == 0) {
            return;
        }

        PDB_mesh molMesh = molecules [molNum].GetComponent<PDB_mesh> ();
        Vector3 atomPosW = molMesh.transform.TransformPoint (sumAtomPos);

        Vector3 transToAt = atomPosW - molMesh.transform.position;
        //atom is behind molecule
        if (atomPosW.z > molMesh.transform.position.z) {
            //we project onto a circle on the xy plan
            transToAt.z = 0;
        }
        //project the label onto the first bvh radius
        transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
        atomPosW = transToAt + molMesh.transform.position;
        t.position = atomPosW;
        t.position += new Vector3 (0, 0, -10);
    }

    public void GetMiniLabelPos(int atomID, int molNum, Transform t)
    {
        Vector3 atomPos = GetAtomPos (atomID, molNum);
        //if the molecule does not exist, or the atom id is bad
        if (molNum == -1 || molecules.Length == 0) {
            return;
        }
        
        PDB_mesh molMesh = molecules [molNum].GetComponent<PDB_mesh> ();
        Vector3 atomPosW = molMesh.transform.TransformPoint (atomPos);
        
        Vector3 transToAt = atomPosW - molMesh.transform.position;
        //atom is behind molecule
        if (atomPosW.z > molMesh.transform.position.z) {
            //we project onto a circle on the xy plan
            transToAt.z = 0;
        }
        //project the label onto the first bvh radius
        //transToAt = transToAt.normalized * molMesh.mol.bvh_radii [0];
        atomPosW = transToAt + molMesh.transform.position;
        t.position = atomPosW + new Vector3 (-3, 3, 0);
    }

    //returns an atoms local position from a atom index and molecule index    
    public Vector3 GetAtomPos (int atomID, int molNum)
    {
        if (atomID == -1 || molNum >= molecules.Length || molecules [molNum] == null) {
            Debug.LogError ("Bad index " + atomID + " " + molNum + " " + molecules.Length);
            return Vector3.zero;
        }
        return molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID];
    }

    //returns an atoms world position from atom index and molecule index
    public Vector3 GetAtomWorldPos (int atomID, int molNum)
    {
        if (atomID == -1 || molNum >= molecules.Length || molecules [molNum] == null) {
            Debug.LogError ("Bad index");
            return Vector3.zero;
        }
        return molecules [molNum].transform.TransformPoint (
            molecules [molNum].GetComponent<PDB_mesh> ().mol.atom_centres [atomID]
        );
    }
    

    // Update handles (badly) a few things that dont fit anywhere else.
    void Update ()
    {

        if(game_status == GameStatus.GameScreen)
        {
            if (playing)
                game_time += Time.deltaTime;

            if (line_renderer && camera)
            {
                line_renderer.clear();
            }


            if (ToggleMode.isOn && uiController.MainCanvas.GetComponent<CanvasGroup>().alpha == 1)
                UpdateHint();
        }
    }

    void PopInSound (GameObject g)
    {
       // this.GetComponent<AudioManager> ().Play ("Blip");
        PopIn (g);
    }

    public void PopIn (GameObject g)
    {
        popTarget = g;
        StartCoroutine ("PopInCo");
    }

    public void PopInWaitDisappear (GameObject g, float waitTime)
    {
        popTarget = g;
        StartCoroutine ("PopInWaitDisappearCo", waitTime);
    }

    public void PopOut (GameObject g)
    {
        popTarget = g;
        StartCoroutine ("PopOutCo");
    }

    IEnumerator PopInCo ()
    {
        GameObject target = popTarget;
        target.SetActive (true);
        popTarget = null;
        float scaleSpeed = 3.0f;
        Vector3 targetScale = target.transform.localScale;
        target.transform.localScale = new Vector3 (0, 0, 0);
        for (float t=0; t<1; t+=Time.deltaTime*scaleSpeed) {
            target.transform.localScale = targetScale * t;
            yield return null;
        }
        target.transform.localScale = targetScale;
        yield break;
    }

    IEnumerator PopInWaitDisappearCo (float waitTime)
    {
        GameObject target = popTarget;
        target.SetActive (true);
        popTarget = null;
        float scaleSpeed = 3.0f;
        Vector3 targetScale = target.transform.localScale;
        target.transform.localScale = new Vector3 (0, 0, 0);
        for (float t=0; t<1; t+=Time.deltaTime*scaleSpeed) {
            target.transform.localScale = targetScale * t;
            yield return null;
        }
        target.transform.localScale = targetScale;
        yield return new WaitForSeconds (waitTime);
        target.SetActive (false);
        yield break;
    }

    IEnumerator PopOutCo ()
    {
        GameObject target = popTarget;
        popTarget = null;
        float scaleSpeed = 3.0f;
        Vector3 targetScale = target.transform.localScale;
        for (float t=1; t>0; t-=Time.deltaTime*scaleSpeed) {
            target.transform.localScale = targetScale * t;
            yield return null;
        }
        target.SetActive (false);
        target.transform.localScale = targetScale;
        yield break;
    }

    //performs lignad RMSD scoring (sum of  the distance of each atom from its original position in the file)
    public float ScoreRMSD ()
    {
        if (molecules.Length == 0 || !molecules [0] || !molecules [0]) {
            return 0.0f;
        }
        float rmsd = 0.0f;
        int count = 0;
        PDB_mesh receptor = molecules [0].GetComponent<PDB_mesh> ();
        PDB_mesh ligand = molecules [1].GetComponent<PDB_mesh> ();
        Vector3 offset = ligand.mol.pos - receptor.mol.pos;
        Matrix4x4 transMat = ligand.transform.worldToLocalMatrix * receptor.transform.localToWorldMatrix;
        for (int i = 0; i < ligand.mol.atom_centres.Length; i++) {
            if (ligand.mol.names [i] == PDB_molecule.atom_C) {
                rmsd += (transMat.MultiplyPoint3x4 (ligand.mol.atom_centres [i] + offset) - ligand.mol.atom_centres [i]).sqrMagnitude;
                count++;
            }
        }
        return Mathf.Sqrt (rmsd / count);
    }

    //Handle the dock slider inputs
    public void HandleDockSlider(Slider slide)
    {
        //if we are at the max value then reveal labelsif (slide.value > slide.maxValue - 10.0f) {


    }

    public void HandleCameraSlider(Slider slide)
    {
        Transform t = MainCamera.gameObject.transform;
        //GameObject cellCam = MainCamera.gameObject;
        float dist = t.position.magnitude;

        Vector3 dir = new Vector3 (Mathf.Cos(Mathf.Deg2Rad * slide.value),
                                  0,
                                  Mathf.Sin(Mathf.Deg2Rad * slide.value));

        if (MainCamera.gameObject) {
            MainCamera.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0,slide.value + 90,0));
        }

        Vector3 pos = dir * dist;
        t.position = pos;
        t.rotation = Quaternion.LookRotation (-dir, Vector3.up);
    }


    /*public void ManageSliders()
    {
        ConnectionManager conMan = this.GetComponent<ConnectionManager> ();

        bool allInPickZone = true;
        if (overrideSlider.gameObject.activeSelf && conMan.connectionMinDistances.Length == dockSliders.Count) {
            float slider_pos = overrideSlider.value - dockOverrideOffset;

            // see if the sliders are in the "sticky" region near the constrainer
            float val0 = dockSliders[0].value;
            bool all_together = true;
            for (int i = 1; i < dockSliders.Count; ++i)
            {
                all_together = all_together && dockSliders[i].value == val0;
            }

            if (true || all_together && val0 >= slider_pos - 2)
            {
                // all sliders are being moved by the master
                for (int i = 0; i < dockSliders.Count; ++i)
                {
                    dockSliders[i].value = slider_pos;
                    conMan.connectionMinDistances[i] = dockSliders[i].value;
                }
            } else
            {
                // sliders are individual.
                for (int i = 0; i < dockSliders.Count; ++i)
                {
                    if (dockSliders[i].value >= slider_pos)
                    {
                        dockSliders[i].value = slider_pos;
                    }
                    conMan.connectionMinDistances[i] = dockSliders[i].value;
                }
            }

            // Return to pick state if all on the right.
            for (int i = 1; i < dockSliders.Count; ++i)
            {
                if (dockSliders[i].value < dockSliders[i].maxValue-5)
                {
                    allInPickZone = false;
                }
            }
        }
    }*/
    /*
    //creates a err label. The label object contains a atom id and name used to name the instance
    void CreateLabel (PDB_molecule.Label argLabel, int molNum, string labelName, PDB_molecule mol)
    {
        GameObject newLabel = GameObject.Instantiate<GameObject> (prefabLabel);
        if (!newLabel) {
            Debug.Log ("Could not create Label");
        }

        LabelScript laSc = newLabel.GetComponent<LabelScript> ();
        newLabel.name = labelName + laSc.labelID;

        //assigned the label a new color from a random range
        newLabel.GetComponent<Image> ().color = colorPool [(argLabel.uniqueLabelID + randomColorPoolOffset) % colorPool.Count];
        newLabel.GetComponent<Light> ().color = newLabel.GetComponent<Image> ().color;
        laSc.atomIds = argLabel.atomIds;
        laSc.moleculeNumber = molNum;

        laSc.owner = this;
        laSc.labelID = argLabel.uniqueLabelID;
        //3D and 2D arrows see LabelScript
        laSc.cloudIs3D = true;
        //GameObject foundObject = GameObject.Find ("Label" + (activeLabels.Count + 1));
        //newLabel.transform.position = foundObject.transform.position;

        //we group all the labels under a single empty transform for convienence in the unity hierarchy
        GameObject canvas = GameObject.Find ("Labels");
        newLabel.transform.SetParent (canvas.transform);

        laSc.Init (mol);
        laSc.gameObject.SetActive (false);
        while (activeLabels.Count < argLabel.uniqueLabelID + 1) {
            activeLabels.Add (null);
        }

        activeLabels [argLabel.uniqueLabelID] = laSc;
    }*/

    // The pick button resets the sliders to the right.
    /*public void Pick()
    {
        if (game_state == GameState.Picking || game_state == GameState.Docking) {
            ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
        
            overrideSlider.value = conMan.maxDistance;
            for (int i = 0; i < dockSliders.Count; ++i) {
                dockSliders [i].value = conMan.maxDistance;
            }

            //conMan.Reset ();

            //selectedLabel [0] = null;
            //selectedLabel [1] = null;
            //game_state = GameState.Picking;

            ManageSliders ();
        }
    }
    
    // The dock button resets the sliders to the middle.
    public void Dock()
    {
        if (game_state == GameState.Picking || game_state == GameState.Docking) {
            ConnectionManager conMan = gameObject.GetComponent<ConnectionManager> ();
            
            overrideSlider.value = conMan.maxDistance * 0.5f;
            for (int i = 0; i < dockSliders.Count; ++i) {
                dockSliders [i].value = conMan.maxDistance * 0.5f;
            }
            ManageSliders ();
        }
    }*/
    
    // The lock button establishes the Locked state.
    public void Lock()
    {
        if (game_state == GameState.Docking) {
            game_state = GameState.Locked;
        }
    }
    
    public void SolidClicked(int molecule_id)
    {
        make_moleculesT (false, MeshTopology.Triangles, molecules[molecule_id].name,molecule_id);
    }
    
    public void PointClicked(int molecule_id)
    {
        make_moleculesT (false, MeshTopology.Points, molecules[molecule_id].name,molecule_id);
    }
    
    public void WireClicked(int molecule_id)
    {
        make_moleculesT (false, MeshTopology.Lines, molecules[molecule_id].name,molecule_id);
    }


    
    public void Reset ()
    {
        //clears the molecules and re-randomizes the colour range
        //randomColorPoolOffset = 0; //Random.Range (0, colorPool.Count - 1);
        //GameObject.Destroy (molecules [0].gameObject);
        //GameObject.Destroy (molecules [1].gameObject);
        foreach(Transform proteins in Molecules)
        {
            Destroy(proteins.gameObject);
        }

        ////clear the old win condition
        //winCondition.Clear ();

        //game_state = GameState.Picking;

        ////Clear score
        //LennardScore.text = "0";
        //ElectricScore.text = "0";
        aminoSlider.Reset();
        gameObject.GetComponent<ConnectionManager>().Reset();
        //molecules = null;
        //heuristicScore.fillAmount = 0;
        //GameScoreValue.text = "0";
        //MainCamera.fieldOfView = 60;

        //GetComponent<AminoSliderController> ().init ();
    }

    //since a molecule may be too large for one mesh we may have to make several
    void make_molecule_mesh (PDB_mesh mesh, Material material, int layerNum, MeshTopology mesh_type)
    {
        foreach (Transform child in mesh.transform) {
            Destroy(child.gameObject);
        }

        for (int i=0; i<mesh.mol.mesh.Length; ++i) {
            Mesh cur = mesh.mol.mesh [i];
            if (mesh_type != MeshTopology.Triangles) {
                Mesh new_mesh = new Mesh();
                new_mesh.vertices = cur.vertices;
                new_mesh.colors = cur.colors;
                new_mesh.normals = cur.normals;
                new_mesh.SetIndices(cur.GetIndices(0), mesh_type, 0);
                cur = new_mesh;
            }
            GameObject obj = new GameObject ();
            obj.name = cur.name;
            obj.layer = layerNum;
            MeshFilter f = obj.AddComponent<MeshFilter> ();
            MeshRenderer r = obj.AddComponent<MeshRenderer>();
            r.material = material;
            f.mesh = cur;
            obj.transform.SetParent (mesh.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }
    }

    // Creates the molecule objects including the PDB_mesh script.
    GameObject make_molecule (string name, string proto, int layerNum, MeshTopology mesh_type, int index)
    {
        GameObject obj = GameObject.Find (name);
        if (obj == null) {
            obj = new GameObject ();
        }

        obj.name = name;
        obj.SetActive (true);
        //the layer numbers are to seperate the molecules in certain cameras
        obj.layer = layerNum;

        //just the one PDB_mesh component should be sufficient...
        PDB_mesh p = obj.GetComponent<PDB_mesh>();
        if (!p)
        {
            p = obj.AddComponent<PDB_mesh>();
            //obj.AddComponent<FirstPerson>();
        }

        PDB_molecule mol = PDB_parser.get_molecule (name);
        p.mol = mol;
        p.protein_id = index;

        //making mesh
        //GameObject pdb = GameObject.Find (proto);
        //MeshRenderer pdbr = pdb.GetComponent<MeshRenderer> ();
        // make_molecule_mesh (p, pdbr.material, layerNum, mesh_type);
        //making mesh

        /*
                //block flare
                if (index == 0)
                {
                    GameObject temp = Instantiate(FlareBlock1);
                    temp.transform.SetParent(obj.transform);

                }
                else
                {
                    GameObject temp = Instantiate(FlareBlock2);
                    temp.transform.SetParent(obj.transform);
                }
                */
        return obj;
    }

    // set the object to its initial position etc.
    void reset_molecule(GameObject obj, int molNum, Vector3 offset)
    {
        PDB_mesh p = obj.GetComponent<PDB_mesh> ();
        Rigidbody ri = obj.AddComponent<Rigidbody> ();
        PDB_molecule mol = p.mol;

        float mass = 1000;
        float r = mol.bvh_radii [0] * 0.5f;
        float val = 0.4f * mass * r * r;

        ri.drag = 2f;
        ri.angularDrag = 5f;
        ri.useGravity = false;
        ri.mass = mass;
        ri.inertiaTensor = new Vector3 (val, val, val);

        obj.transform.Translate (offset);
        obj.transform.Rotate (0, 0, 270);
    }


    //applies forces to both molecules to return them to their respective origins
    //aca se mueve
    void ApplyReturnToOriginForce ()
    {
        for (int i = 0; i < molecules.Length; ++i) {
            Vector3 molToOrigin = originPosition [i] - molecules [i].transform.position;
            if (molToOrigin.sqrMagnitude > 1.0f) {
                Rigidbody rb = molecules [i].GetComponent<Rigidbody> ();
                rb.AddForce (molToOrigin.normalized * repulsiveForce);
            }
        }
    }

    public void DebugDock()
    {
        if (molecules [0] && molecules [1]) {
            molecules[0].GetComponent<PDB_mesh>().AutoDockCheap();
            molecules[1].GetComponent<PDB_mesh>().AutoDockCheap();
            repulsiveForce = 0;
            Debug.Log("Docking");
        }

    }

    // create both molecules
    void make_molecules(bool init, MeshTopology mesh_type) {
        Level level = levels[current_level];

        uiController.SetHintImage(level.pdbFile); //HINT

        //<<<<<<< HEAD
        //        GameObject mol1 = make_molecule (file + ".1", "Proto1", 7, mesh_type,0);
        //        mol1.transform.SetParent(Molecules);
        //        GameObject mol2 = make_molecule (file + ".2", "Proto2", 7, mesh_type,1);
        //        mol2.transform.SetParent(Molecules);
        //=======
        // These filenames refer to the prefabs in Assets/Resources/Mesh
        string mol1_se_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
        string mol2_se_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;
        string mol1_bs_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod;
        string mol2_bs_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod;
        string mol1_ca_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod;
        string mol2_ca_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod;

        // Make two PDB_mesh instances from the PDB file and a chain selection.
        GameObject mol1 = make_molecule (level.pdbFile + "." + level.chainsA, "Proto1", 7, mesh_type, 0);
        mol1.transform.SetParent(Molecules);
        GameObject mol2 = make_molecule (level.pdbFile + "." + level.chainsB, "Proto2", 7, mesh_type, 1);
        mol2.transform.SetParent(Molecules);

        //DEFAULT
        GameObject mol1_mesh = Instantiate(Resources.Load(mol1_se_filename)) as GameObject;
        mol1_mesh.transform.SetParent(mol1.transform);

        GameObject mol2_mesh = Instantiate(Resources.Load(mol2_se_filename)) as GameObject;
        mol2_mesh.transform.SetParent(mol2.transform);

        // Ioannis
        scoring = new PDB_score(mol1.GetComponent<PDB_mesh>().mol, mol1.gameObject.transform, mol2.GetComponent<PDB_mesh>().mol, mol2.gameObject.transform);

        //BALLS AND STICK 1
        mol1_mesh = Instantiate(Resources.Load(mol1_bs_filename)) as GameObject;
        mol1_mesh.transform.SetParent(mol1.transform);
        //mol1_mesh.name = "bs_p1";
        mol1_mesh.SetActive(false);
        //BALLS AND STICK 1
        mol2_mesh = Instantiate(Resources.Load(mol2_bs_filename)) as GameObject;
        mol2_mesh.transform.SetParent(mol2.transform);
        //mol2_mesh.name = "bs_p2";
        mol2_mesh.SetActive(false);

        //CARBON ALPHA 1
        mol1_mesh = Instantiate(Resources.Load(mol1_ca_filename)) as GameObject;
        mol1_mesh.transform.SetParent(mol1.transform);
        //mol1_mesh.name = "ca_p1";
        mol1_mesh.SetActive(false);
        //CARBON ALPHA 2
        mol2_mesh = Instantiate(Resources.Load(mol2_ca_filename)) as GameObject;
        mol2_mesh.transform.SetParent(mol2.transform);
        //mol2_mesh.name = "ca_p2";
        mol2_mesh.SetActive(false);

        //TRANSPARENT 1
        mol1_mesh = Instantiate(Resources.Load(mol1_se_filename)) as GameObject;
        mol1_mesh.transform.SetParent(mol1.transform);
       // mol1_mesh.name = "transparent_p1";
        FixTransparentMolecule(mol1_mesh, 0);
        mol1_mesh.SetActive(false);
        //TRANSPARENT 2
        mol2_mesh = Instantiate(Resources.Load(mol2_se_filename)) as GameObject;
        mol2_mesh.transform.SetParent(mol2.transform);
        //mol2_mesh.name = "transparent_p1";
        FixTransparentMolecule(mol2_mesh, 1);
        mol2_mesh.SetActive(false);

//        //<<<<<<< HEAD
//        //ball and stick 1
//        mol1_mesh = Instantiate(Resources.Load("Mesh/" + level_mesh_bs[level].First)) as GameObject;
////=======
//        // transparent 1
//        mol1_mesh = Instantiate(Resources.Load(mol1_se_filename)) as GameObject;
//        mol1_mesh.transform.SetParent(mol1.transform);
//        mol1_mesh.name = "transparent_p1";
//        FixTransparentMolecule(mol1_mesh);
//        mol1_mesh.SetActive(false);

//        // transparent 2
//        mol2_mesh = Instantiate(Resources.Load(mol2_se_filename)) as GameObject;
//        mol2_mesh.transform.SetParent(mol2.transform);
//        mol2_mesh.name = "transparent_p2";
//        FixTransparentMolecule(mol2_mesh);
//        mol2_mesh.SetActive(false);

//        // ball and stick 1
//        mol1_mesh = Instantiate(Resources.Load(mol1_bs_filename)) as GameObject;
////>>>>>>> chains
//        mol1_mesh.transform.SetParent(mol1.transform);
//        mol1_mesh.name = "bs_p1";
//        mol1_mesh.SetActive(false);

//        // ball and stick 2
//        mol2_mesh = Instantiate(Resources.Load(mol2_bs_filename)) as GameObject;
//        mol2_mesh.transform.SetParent(mol2.transform);
//        mol2_mesh.name = "bs_p2";
//        mol2_mesh.SetActive(false);
//        //carbon 1
//        mol1_mesh = Instantiate(Resources.Load("Mesh/" + level_mesh_carbon[level].First)) as GameObject;
//        mol1_mesh.transform.SetParent(mol1.transform);
//        mol1_mesh.name = "c_p1";
//        mol1_mesh.SetActive(false);
//        //carbon 2
//        mol2_mesh = Instantiate(Resources.Load("Mesh/" + level_mesh_carbon[level].Second)) as GameObject;
//        mol2_mesh.transform.SetParent(mol2.transform);
//        mol2_mesh.name = "c_p2";
//        mol2_mesh.SetActive(false);
//        //transpant 1
//        mol1_mesh = Instantiate(Resources.Load("Mesh/" + level_mesh_default[level].First)) as GameObject;
//        mol1_mesh.transform.SetParent(mol1.transform);
//        mol1_mesh.name = "transparent_p1";
//        FixTransparentMolecule(mol1_mesh,0);
//        mol1_mesh.SetActive(false);
//        //transpant 2
//        mol2_mesh = Instantiate(Resources.Load("Mesh/" + level_mesh_default[level].Second)) as GameObject;
//        mol2_mesh.transform.SetParent(mol2.transform);
//        mol2_mesh.name = "transparent_p2";
//        FixTransparentMolecule(mol2_mesh,1);
//        mol2_mesh.SetActive(false);

        //GameObject mol1_mesh = Instantiate(prefab_molecules[0]);
        //mol1_mesh.transform.SetParent(mol1.transform);

        //GameObject mol2_mesh= Instantiate(prefab_molecules[1]);
        //mol2_mesh.transform.SetParent(mol2.transform);

        ////Ioannis
        //scoring = new PDB_score(mol1.GetComponent<PDB_mesh>().mol, mol1.gameObject.transform, mol2.GetComponent<PDB_mesh>().mol, mol2.gameObject.transform);

        ////transpant 1
        //mol1_mesh = Instantiate(prefab_molecules[0]);
        //mol1_mesh.transform.SetParent(mol1.transform);
        //mol1_mesh.name = "transparent_p1";
        //FixTransparentMolecule(mol1_mesh);
        ////transpant 2
        //mol2_mesh = Instantiate(prefab_molecules[1]);
        //mol2_mesh.transform.SetParent(mol2.transform);
        //mol2_mesh.name = "transparent_p2";
        //FixTransparentMolecule(mol2_mesh);
        ////ball and stick 1
        //mol1_mesh = Instantiate(prefab_molecules_bs[0]);
        //mol1_mesh.transform.SetParent(mol1.transform);
        //mol1_mesh.name = "bs_p1";
        ////ball and stick 2
        //mol2_mesh = Instantiate(prefab_molecules_bs[1]);
        //mol2_mesh.transform.SetParent(mol2.transform);
        //mol2_mesh.name = "bs_p2";


        if (init) {
            molecules = new GameObject[2];
            molecules [0] = mol1.gameObject;
            molecules [1] = mol2.gameObject;
            Vector3 xoff = new Vector3(level.separation, 0, 0);

            reset_molecule(molecules[0], 0, level.offset - xoff);
            reset_molecule(molecules[1], 1, level.offset + xoff);
        }
    }

    // create both molecules
    void make_moleculesT(bool init, MeshTopology mesh_type, string name, int index) {
        //Debug.Log ("make_molecules");
        //string file = filenames [current_level];
        //Debug.Log (file);
        
        make_molecule (name, "Proto1", 7, mesh_type,index);
        //GameObject mol2 = make_molecule (name, "Proto2", 7, mesh_type,1);
    }

    //main meat of the initilisation logic and level completion logic
    IEnumerator game_loop ()
    {
        if (game_status == GameStatus.GameScreen)
        {
            // for each level
            for (;;)
            {
                Debug.Log("start level " + current_level);
                //if true, we have no more levels listed in the vector
                //to be replaced with level selection. Talk to andy on PDB file selection
                if (current_level >= levels.Length)
                {
                    Debug.LogError("No next level");
                    current_level = 0;
                }

                if (lockButton)
                {
                    //lockButton.gameObject.SetActive(false);
                    //lockButton.interactable = false;
                }
                make_molecules(true, MeshTopology.Triangles);

                // This is very grubby, must generalise.
                GameObject mol1 = molecules[0];
                GameObject mol2 = molecules[1];

                originPosition[0] = mol1.transform.position;
                originPosition[1] = mol2.transform.position;

                PDB_mesh p1 = mol1.GetComponent<PDB_mesh>();
                PDB_mesh p2 = mol2.GetComponent<PDB_mesh>();

                //create the win condition from the file specified paired atoms
                for (int i = 0; i < p1.mol.pairedLabels.Length; ++i)
                {
                    winCondition.Add(new Tuple<int, int>(p1.mol.pairedLabels[i].First,
                                     p1.mol.pairedLabels[i].Second));
                }
                //debug 3D texture
                //GameObject.Find ("Test").GetComponent<Tex3DMap> ().Build (p1.mol);
                p1.other = p2.gameObject;
                p2.other = p1.gameObject;
                p1.gameObject.SetActive(false);
                p2.gameObject.SetActive(false);

                //pop the molecules in for a visually pleasing effect
                PopInSound(mol1.gameObject);
                yield return new WaitForSeconds(0.2f);
                PopInSound(mol2.gameObject);

                //this is the connection manager for the complex game, it handles grappling between the molecules
                ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();
                /*
                for (int i = 0; i < dockSliders.Count; ++i) {
                    dockSliders[i].maxValue =  conMan.maxDistance;
                    dockSliders[i].minValue = conMan.minDistance;
                    dockSliders[i].value =conMan.maxDistance;
                    //dockSliders[i].gameObject.SetActive(false);
                    //sliderConstarinedByOverride.Add(true);
                }
                overrideSlider.maxValue = conMan.maxDistance;
                overrideSlider.minValue = conMan.minDistance;
                overrideSlider.value = conMan.maxDistance;

                overrideSlider.interactable = false;*/

                mol1.transform.localScale = new Vector3(1, 1, 1);
                mol2.transform.localScale = new Vector3(1, 1, 1);
                yield return new WaitForSeconds(0.1f);
                eventSystem.enabled = true;


                // Enter waiting state
                game_state = GameState.Waiting;

                /*if (goSplash) {
                    PopInWaitDisappear (goSplash, 1.0f);
                }*/

                // Enter picking state
                game_state = GameState.Picking;

                // In this loop, the game state is either Picking or Docking.
                while (game_state == GameState.Picking || game_state == GameState.Docking || game_status == GameStatus.GameScreen)
                {
                    // start the new frame
                    yield return new WaitForEndOfFrame();
                    //Debug.Log ("gs = " + game_state);

                    // measure the score
                    float rms_distance_score = ScoreRMSD();
                    if (rmsScoreSlider)
                    {
                        rmsScoreSlider.value = rms_distance_score * 0.1f;
                        /*float scaleGameScore = 1.0f - (rms_distance_score * 0.1f);
                        if(scaleGameScore <= 1.0f && scaleGameScore > 0)
                        {
                            GameScore.fillAmount = scaleGameScore;                        
                            GameScoreValue.text = ((int)(scaleGameScore * 1250)).ToString();
                        }
                        else
                        {
                            GameScore.fillAmount = 0;
                            GameScoreValue.text = "0";
                        }*/
                    }

                    if (lockButton)
                    {
                        //lockButton.gameObject.SetActive (rms_distance_score < winScore);
                        //lockButton.interactable = (rms_distance_score < winScore);
                    }
                }

                Debug.Log("exited docking loop " + game_state);

                if (game_state == GameState.Locked)
                {
                    Debug.Log("locking");

                    eventSystem.enabled = false;

                    //StartCoroutine("DockingOneAxis");
                    if (sites[0])
                    {
                        PopOut(sites[0]);
                    }

                    if (sites[1])
                    {
                        PopOut(sites[1]);
                    }

                    //lockButton.gameObject.SetActive(false);
                    //lockButton.interactable = false;

                    Debug.Log("Docked");

                    this.GetComponent<AudioManager>().Play("Win");

                    GameObject parent = new GameObject();
                    Rigidbody r = parent.AddComponent<Rigidbody>();
                    molecules[0].transform.SetParent(parent.transform, true);
                    molecules[1].transform.SetParent(parent.transform, true);

                    r.angularDrag = 1.0f;
                    r.constraints = RigidbodyConstraints.FreezePosition;
                    r.useGravity = false;
                    parent.name = "MoveableParent";

                    //this is to stop the molecules rumbling around as they inherit the pearents velocity
                    Component.Destroy(molecules[0].GetComponent<Rigidbody>());
                    Component.Destroy(molecules[1].GetComponent<Rigidbody>());

                    //StartCoroutine ("WinSplash", new Vector3 (0, 0, 0));
                    GameObject.Destroy(sites[0]);
                    GameObject.Destroy(sites[1]);

                    Debug.Log("current_level=" + current_level);
                    current_level++;
                    if (current_level == levels.Length)
                    {
                        Debug.Log("End of levels");
                        current_level = 0;
                    }
                    conMan.Reset();
                    Reset();
                    molecules = null;
                    //GetComponent<AminoButtonController> ().EmptyAminoSliders ();
                    //EndLevelMenu.SetActive(true);
                }
            }
        }
    }

    public int work_done = 0;
    int number_total_atoms = 0;
    int sound_to_play;

    // Physics simulation
    void FixedUpdate() {

        if (game_status == GameStatus.GameScreen)
        {

            num_touching_0 = 0;
            num_touching_1 = 0;
            num_invalid = 0;
            num_connections = 0;
            //Debug.Log ("game_state=" + game_state + "molecules.Length=" + molecules.Length)

            //score system display
            if (scoring == null)
            {
                return;
            }


            if (molecules != null && molecules.Length >= 2)
            {

                // Get a list of atoms that collide.
                GameObject obj0 = molecules[0];
                GameObject obj1 = molecules[1];
                PDB_mesh mesh0 = (PDB_mesh)obj0.GetComponent<PDB_mesh>();
                PDB_mesh mesh1 = (PDB_mesh)obj1.GetComponent<PDB_mesh>();
                Rigidbody r0 = obj0.GetComponent<Rigidbody>();
                Rigidbody r1 = obj1.GetComponent<Rigidbody>();
                Transform t0 = obj0.transform;
                Transform t1 = obj1.transform;
                PDB_molecule mol0 = mesh0.mol;
                PDB_molecule mol1 = mesh1.mol;
                GridCollider b = new GridCollider(mol0, t0, mol1, t1, 0);
                work_done = b.work_done;

                BitArray ba0 = new BitArray(mol0.atom_centres.Length);
                BitArray ba1 = new BitArray(mol1.atom_centres.Length);
                BitArray bab0 = new BitArray(mol0.atom_centres.Length);
                BitArray bab1 = new BitArray(mol1.atom_centres.Length);
                atoms_touching = new BitArray[] { ba0, ba1 };
                atoms_bad = new BitArray[] { bab0, bab1 };


                // Apply forces to the rigid bodies.
                foreach (GridCollider.Result r in b.results)
                {
                    Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
                    Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
                    float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
                    float distance = (c1 - c0).magnitude;

                    num_connections++;

                    if (distance < min_d)
                    {
                        Vector3 normal = (c0 - c1).normalized * (min_d - distance);
                        normal *= seperationForce;
                        r0.AddForceAtPosition(normal, c0);
                        r1.AddForceAtPosition(-normal, c1);

                        if (!ba0[r.i0]) { num_touching_0++; ba0.Set(r.i0, true); }
                        if (!ba1[r.i1]) { num_touching_1++; ba1.Set(r.i1, true); }
                        if (distance < min_d * 0.5)
                        {
                            num_invalid++;
                            bab0.Set(r.i0, true);
                            bab1.Set(r.i1, true);
                        }
                    }

                }

                if (num_touching_0 + num_touching_1 != 0)
                {
                    if (uiController.expert_mode)
                    {
                        scoring.calcScore();
                        ////set values for refence
                        lennard_score = (int)scoring.vdwScore;
                        electric_score = (int)scoring.elecScore;
                        if (scoring.elecScore < 50000) ElectricScore.text = (scoring.elecScore).ToString("F1");
                        if (scoring.vdwScore < 50000) LennardScore.text = (scoring.vdwScore).ToString("F1");
                    }
                    if (NumberOfAtoms) NumberOfAtoms.text = (num_touching_0 + num_touching_1).ToString();
                    if (SimpleScore) SimpleScore.text = "Score: " + (num_touching_0 + num_touching_1).ToString() + " atoms touching.";
                }
                else
                {
                    ElectricScore.text = LennardScore.text = "0.0";
                    NumberOfAtoms.text = "0";
                    SimpleScore.text = "Score: 0 atoms touching.";
                }

                //heuristicScoreSlider.value = num_invalid != 0 ? 1.0f : 1.0f - (num_touching_0 + num_touching_1) * 0.013f;

                //num_invalid = when the physics fails
                //ElectricScore.text = num_invalid != 0 ? ElectricScore.text = (scoring.elecScore).ToString("F2") : "0";

                //LennardScore.text = num_invalid != 0 ? LennardScore.text = (scoring.vdwScore).ToString("F2") : "0";
                //Debug.Log ("num_touching_0: "+num_touching_0+" / num_touching_1: "+num_touching_1);
                //Debug.Log ("num_invalid: "+num_invalid);

            }

            invalidDockText.SetActive(num_invalid != 0);
            InvalidDockScore.SetActive(num_invalid != 0);
            is_score_valid = num_invalid == 0;
            if (num_invalid != 0 && !sfx.isPlaying(SFX.sound_index.warning))
                sfx.PlayTrackDelay(SFX.sound_index.warning, 1.5f);
            else if (num_invalid == 0 && sfx.isPlaying(SFX.sound_index.warning))
                sfx.StopTrack(SFX.sound_index.warning);
            /*
            //lock button
            if (num_invalid == 0 && (lennard_score != 0 || electric_score != 0))
            {
                lockButton.interactable = true;
            }
            else
            {
                lockButton.interactable = false;
            }*/
            
            if (eventSystem != null && eventSystem.IsActive())
            {
                ApplyReturnToOriginForce();
            }

            if (number_total_atoms+2 < num_touching_0 + num_touching_1)
            {
                sound_to_play = Random.Range(0, 4);
                if(!sfx.isPlaying_collision(sound_to_play))
                    sfx.PlayTrackChild(sound_to_play);
            }

            number_total_atoms = num_touching_0 + num_touching_1;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
        hint_stage = 0;
    }


    void UpdateHint()
    {
        AminoSliderController sliders = GetComponent<AminoSliderController>();
        GameObject hobj = GameObject.Find("HintText");
        Text hintText = hobj.GetComponent<Text>();
        ConnectionManager conMan = this.GetComponent<ConnectionManager> ();

        string[] aas = { "LYS I  15", "ASP E 189", "ILE E  73", "ARG I  17", "GLN E 175", "ARG I  39" };
        sliders.FilterButtons(aas);

        // state machine for hints.
        // todo: turn this into a JSON file.
        switch (hint_stage) {
            case 0:
                {
                    TutorialHand.rotation = Quaternion.AngleAxis(90, -Vector3.forward);
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[1].transform.GetChild(14).transform.position.x, aminoSlider.SliderMol[1].transform.GetChild(14).transform.position.y + button_offset, aminoSlider.SliderMol[1].transform.GetChild(14).transform.position.z);
                    //Debug.Log(MainCamera.ScreenToWorldPoint(aminoSlider.SliderMol[1].transform.GetChild(14).transform.localPosition));
                    //Debug.Log(MainCamera.WorldToScreenPoint(aminoSlider.SliderMol[1].transform.GetChild(14).transform.localPosition));
                    hintText.text = "Welcome! In the panel on the bottom left there are buttons which represents amino acids. Select the blue button marked LYS I 15.";
                HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                if (sliders.IsConnectionMade(aas[1], aas[0])) {
                    hint_stage = 5;
                } else if (sliders.IsSelected(1, aas[0])) {
                    hint_stage = 1;
                }
            } break;
            case 1: {
                    //TutorialHand.rotation = Quaternion.AngleAxis(90, -Vector3.forward);
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[0].transform.GetChild(168).transform.position.x, aminoSlider.SliderMol[0].transform.GetChild(168).transform.position.y + button_offset, aminoSlider.SliderMol[0].transform.GetChild(168).transform.position.z);
                    hintText.text = "And now select the red button marked ASP E 189.";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsSelected(1, aas[0])) {
                    hint_stage =  0;
                } else if (sliders.IsSelected(0, aas[1])) {
                    hint_stage =  2;
                }
            } break;
            case 2:
                {
                    TutorialHand.position = new Vector3(uiController.AddConnectionButton.position.x, uiController.AddConnectionButton.position.y, uiController.AddConnectionButton.position.z);
                    hintText.text = "Good. Now press the '+' button to add a connection. This will connect both amino acids. You can spin the molecules to see the atoms in the hole.";
                HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                if (!sliders.IsSelected(1, aas[0])) {
                    hint_stage =  0;
                } else if (!sliders.IsSelected(0, aas[1])) {
                    hint_stage =  1;
                } else if (sliders.IsConnectionMade(aas[1], aas[0])) {
                    hint_stage =  3;
                }
            } break;
            case 3:
                {
                    TutorialHand.GetChild(0).GetComponent<Image>().sprite = slider_hand;
                    TutorialHand.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    TutorialHand.position = new Vector3(SliderString.position.x + button_offset, SliderString.position.y, SliderString.position.z);
                    hintText.text = "Congratulations, you now have one connection. You can pull the connection gently in with the slider on the left. It won't dock correctly, but it shows how things work.";
                HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                if (!sliders.IsConnectionMade(aas[1], aas[0])) {
                    hint_stage = 0;
                } else if (conMan.SliderStrings.value < 0.5) {
                    hint_stage = 4;
                }
            } break;
            case 4:
                {
                    hintText.text = "Good. Now we can let the slider out and make some more connections.";
                HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                if (!sliders.IsConnectionMade(aas[1], aas[0])) {
                    hint_stage = 0;
                } else if (conMan.SliderStrings.value > 0.95) {
                    hint_stage = 5;
                }
            } break;
            case 5:
                {
                    TutorialHand.GetChild(0).GetComponent<Image>().sprite = default_hand;
                    TutorialHand.rotation = Quaternion.AngleAxis(90, -Vector3.forward);
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[1].transform.GetChild(16).transform.position.x, aminoSlider.SliderMol[1].transform.GetChild(16).transform.position.y + button_offset, aminoSlider.SliderMol[1].transform.GetChild(16).transform.position.z);
                hintText.text = "You can rotate the proteins by holding the left mouse click over and also select the amino acids directly by clicking them! Now lets make two more connections. First select the blue button marked ARG I 17";
                HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                if (!sliders.IsConnectionMade(aas[1], aas[0])) {
                    hint_stage = 0;
                } else if (sliders.IsSelected(1, aas[3])) {
                    hint_stage = 6;
                }
                else if (sliders.IsConnectionMade(aas[2], aas[3]))
                {
                    hint_stage = 8;
                }
                else if (!sliders.IsSelected(1, aas[3]))
                {
                    hint_stage = 5;
                }
                } break;
            case 6:
                {
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[0].transform.GetChild(54).transform.position.x, aminoSlider.SliderMol[0].transform.GetChild(54).transform.position.y + button_offset, aminoSlider.SliderMol[0].transform.GetChild(54).transform.position.z);

                    hintText.text = "Now select the grey button marked ILE E 73";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsConnectionMade(aas[1], aas[0]))
                    {
                        hint_stage = 0;
                    }
                    else if (sliders.IsSelected(0, aas[2]))
                    {
                        hint_stage = 7;
                    }
                    else if (!sliders.IsSelected(1, aas[3]))
                    {
                        hint_stage = 5;
                    }
                    else if (!sliders.IsSelected(0, aas[2]))
                    {
                        hint_stage = 6;
                    }
                }
                break;
            case 7:
                {
                    TutorialHand.position = new Vector3(uiController.AddConnectionButton.position.x, uiController.AddConnectionButton.position.y, uiController.AddConnectionButton.position.z);

                    hintText.text = "Good. Now press the '+' button to add a connection. This will connect both amino acids.";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsSelected(1, aas[3]))
                    {
                        hint_stage = 5;
                    }
                    else if (!sliders.IsSelected(0, aas[2]))
                    {
                        hint_stage = 6;
                    }
                    else if (sliders.IsConnectionMade(aas[2], aas[3]))
                    {
                        hint_stage = 8;
                    }
                }
                break;
            case 8:
                {
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[1].transform.GetChild(38).transform.position.x, aminoSlider.SliderMol[1].transform.GetChild(38).transform.position.y + button_offset, aminoSlider.SliderMol[1].transform.GetChild(38).transform.position.z);

                    hintText.text = "Try rotate the camera, by holding the right mouse click. One more connection to go; select the blue button marked ARG I 39";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsConnectionMade(aas[1], aas[0]))
                    {
                        hint_stage = 5;
                    }
                    else if (sliders.IsSelected(1, aas[5]))
                    {
                        hint_stage = 9;
                    }
                    else if (!sliders.IsSelected(1, aas[5]))
                    {
                        hint_stage = 8;
                    }
                }
                break;
            case 9:
                {
                    TutorialHand.position = new Vector3(aminoSlider.SliderMol[0].transform.GetChild(154).transform.position.x, aminoSlider.SliderMol[0].transform.GetChild(154).transform.position.y + button_offset, aminoSlider.SliderMol[0].transform.GetChild(154).transform.position.z);

                    hintText.text = "Select the pink button marked GLN E 175";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsConnectionMade(aas[1], aas[0]))
                    {
                        hint_stage = 5;
                    }
                    else if (!sliders.IsConnectionMade(aas[2], aas[3]))
                    {
                        hint_stage = 5;
                    }
                    else if (sliders.IsSelected(0, aas[4]))
                    {
                        hint_stage = 10;
                    }
                    else if (!sliders.IsSelected(1, aas[5]))
                    {
                        hint_stage = 8;
                    }
                    else if (!sliders.IsSelected(0, aas[4]))
                    {
                        hint_stage = 9;
                    }
                }
                break;
            case 10:
                {
                    TutorialHand.position = new Vector3(uiController.AddConnectionButton.position.x, uiController.AddConnectionButton.position.y, uiController.AddConnectionButton.position.z);

                    hintText.text = "Good. Now press the '+' button to add a connection. This will connect both amino acids.";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsSelected(1, aas[5]))
                    {
                        hint_stage = 8;
                    }
                    else if (!sliders.IsSelected(0, aas[4]))
                    {
                        hint_stage = 9;
                    }
                    else if (sliders.IsConnectionMade(aas[4], aas[5]))
                    {
                        hint_stage = 11;
                    }
                }
                break;
            case 11:
                {
                    TutorialHand.GetChild(0).GetComponent<Image>().sprite = slider_hand;
                    TutorialHand.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    TutorialHand.position = new Vector3(SliderString.position.x + button_offset, SliderString.position.y, SliderString.position.z);
                    hintText.text = "Gently pull in the strings all the way using the left hand slider.";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    Debug.Log("1: " + sliders.IsConnectionMade(aas[1], aas[0]) + " - 2: " + sliders.IsConnectionMade(aas[2], aas[3]) + " - 3: " + sliders.IsConnectionMade(aas[4], aas[5]));
                    if (!sliders.IsConnectionMade(aas[1], aas[0]) || !sliders.IsConnectionMade(aas[2], aas[3]) || !sliders.IsConnectionMade(aas[4], aas[5]))
                    {
                        hint_stage = 5;
                    }
                    else if (conMan.SliderStrings.value == 0)
                    {
                        hint_stage = 12;
                    }
                }
                break;
            case 12:
                {
                    TutorialHand.GetChild(0).GetComponent<Image>().sprite = default_hand;
                    TutorialHand.position = new Vector3(6000.0f,0,0);

                    hintText.text = "Now we have a dock, but perhaps not the best one. We can 'wiggle' the connections using the top arrows on the two right hand connection tabs. Keep the blue fingers the same as they are correct. Now press the 'Exit Tutorial' button and start playing!";
                    HintTextPanel.sizeDelta = new Vector2(520, LayoutUtility.GetPreferredHeight(hobj.GetComponent<RectTransform>()) + 20);
                    if (!sliders.IsConnectionMade(aas[1], aas[0]))
                    {
                        hint_stage = 5;
                    }
                }
                break;
        }
    }

    public void ToggleGameMode()
    {
        bool status = ToggleMode.isOn;
        //ScorePanel.alpha = status ? 0 : 1;
        //Filter.SetActive(!status);
        //SimpleScoretemp.SetActive(status);
        HintText.SetActive(status);
        HintTextPanel.gameObject.SetActive(status);
        ExitTutorialButton.SetActive(status);

        if (!status)
            TutorialHand.position = new Vector3(5000.0f, 0, 0);

        Transform Amino1 = GameObject.Find("ContentPanelA1").transform;
        Transform Amino2 = GameObject.Find("ContentPanelA2").transform;

        if (!status)
        {
            //actgive all amino
            foreach (Transform childTransform in Amino1)
            {
                childTransform.gameObject.SetActive(true);
            }
            foreach (Transform childTransform in Amino2)
            {
                childTransform.gameObject.SetActive(true);
            }
        }
        uiController.ToggleAminoButtonRL(!status);
    }

    public void ExitTutorial()
    {
        ToggleMode.isOn = false;
        ToggleGameMode();
        aminoSlider.DeleteAllAminoConnections();
        TutorialHand.position = new Vector3(6000.0f, 0, 0);
        uiController.isOverUI = false;
    }

//<<<<<<< HEAD
    public Material transparent_0;
    public Material transparent_1;
    bool switch_material = true;
    //bool switch_material = true;
//>>>>>>> chains

    //fix the transparent molecule
    public void FixTransparentMolecule(GameObject protein, int id_protein)
    {
        foreach (Transform molecule_renderer in protein.transform)
        {
            molecule_renderer.GetComponent<Renderer>().material = id_protein == 0 ? transparent_0 : transparent_1;
            molecule_renderer.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            molecule_renderer.GetComponent<MeshRenderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }
}

