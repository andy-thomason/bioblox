using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

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
    public int current_level;

    struct Level {
      public string pdbFile;
      public string chainsA;
      public string chainsB;
      public string lod;
    public string lod_bs;
    public Vector3 offset;
      public float separation;

      public Level(string pdbFile, string chainsA, string chainsB, string lod, string lod_bs, Vector3 offset, float separation) {
        this.pdbFile = pdbFile;
        this.chainsA = chainsA;
        this.chainsB =chainsB;
            this.lod = lod;
            this.lod_bs = lod_bs;
            this.offset = offset;
        this.separation = separation;
      }
    };

    Level[] levels = {
       new Level("2PTC", "E", "I", "2", "1", new Vector3(0, 0, 0), 35),
       new Level("4KC3", "A", "B", "2", "1", new Vector3(0, 0, 0), 40),
       new Level("1FSS", "A", "B", "2", "1", new Vector3(-20, 0, 0), 40),
       new Level("1EMV", "A", "B", "2", "1", new Vector3(-20, 0, 0), 40),
       new Level("1GRN", "A", "B", "2", "1", new Vector3(-20, 0, 0), 40),
       new Level("1OHZ", "A", "B", "2", "1", new Vector3(-20, 0, 0), 40)
    };

    enum protein_view {normal, transparent, bs};
    
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
    public PDB_mesh[] molecules_PDB_mesh;
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
    public float LJseperationForce = -300.0f;
    public float LJMax = 10.0f;
    public float LJMin = -10.0f;
    public float LJinflation = 1.0f;

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
    public float lennard_jones = 0;

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

    bool loaded = false;

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
    public PDB_score scoring;

    public GameObject GameManager;
    public GameObject MenuButtons;

    void Awake()
    {
        //creatt ehe sene manager to keep track of the level
        if(GameObject.FindGameObjectWithTag("GameManager") == null)
        {
            Instantiate(GameManager);
            MenuButtons.SetActive(false);
        }
    }

    //public void cacaca()
    //{
    //    SceneManager.LoadScene(0, LoadSceneMode.Single);
    //}

    // Use this for initialization
    void Start ()
    {
        #if UNITY_WEBGL
                Application.targetFrameRate = -1;
        #endif

        game_status = BioBlox.GameStatus.MainScreen;
        uiController = FindObjectOfType<UIController>();
        sfx = FindObjectOfType<SFX>();

        eventSystem = EventSystem.current;
        //update
        line_renderer = GameObject.FindObjectOfType<LineRenderer>() as LineRenderer;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //first eprson
        aminoSlider = FindObjectOfType<AminoSliderController>();

        molecules = new GameObject[2];
        molecules_PDB_mesh = new PDB_mesh[2];
        //set the trypsin fiesr level temp
        current_level = FindObjectOfType<GameManager>().current_level;
        
        if (current_level != -1)
            StartCoroutine(DownloadMolecules());
        else
            uiController.EndLevel();
    }

    public void StartGame()
    {
        //Reset();
        game_status = GameStatus.GameScreen;
        uiController.isOverUI = false;
        uiController.ToggleHintFromIntro();

        if (current_level == 0)
            ToggleMode.isOn = true;
        else
            ToggleMode.isOn = false;
        
        game_time = 0;
        playing = true;

        game_state = GameState.Setup;
        
        aminoSlider.init();

        molecules_PDB_mesh[0].DeselectAminoAcid();
        molecules_PDB_mesh[1].DeselectAminoAcid();
        aminoSlider.DeselectAmino();

        //UI INIT
        uiController.init();

        FindObjectOfType<GameManager>().loading_panel.SetActive(false);
        //loading_panel_start.SetActive(false);

        StartCoroutine(game_loop());

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

    float time_valid_score = 0;
    public Button SubmitButton;
    public GameObject validating_holder;

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

        //check if score is valid
        if(is_score_valid)
        {
            time_valid_score += Time.deltaTime;
        }
        else
        {
            time_valid_score = 0;
            SubmitButton.gameObject.SetActive(false);
            validating_holder.SetActive(true);
        }

        if (time_valid_score > 5.0f)
        {
            validating_holder.SetActive(false);
            SubmitButton.gameObject.SetActive(true);
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
    
    // The lock button establishes the Locked state.
    public void Lock()
    {
        if (game_state == GameState.Docking) {
            game_state = GameState.Locked;
        }
    }


    
    public void Reset ()
    {
        foreach(Transform proteins in Molecules)
        {
            Destroy(proteins.gameObject);
        }

        aminoSlider.Reset();
        gameObject.GetComponent<ConnectionManager>().Reset();
        
       // IntroCamera.SetActive(false);

        //FindObjectOfType<ConnectionManager>().DisableSlider();
       // IntroCanvas.interactable = false;
        //IntroCanvas.blocksRaycasts = false;

        //ui.LevelClickled.GetComponent<LevelInfo>().SendData();

        uiController.Reset_UI();
        uiController.EndLevelPanel.SetActive(false);
        
        //sfx.StopTrack(SFX.sound_index.warning);
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
        GameObject obj = Instantiate(mesh_temp);

        obj.name = name;
       // obj.SetActive (true);
        //the layer numbers are to seperate the molecules in certain cameras
        obj.layer = layerNum;

        //just the one PDB_mesh component should be sufficient...
        PDB_mesh p  = obj.AddComponent<PDB_mesh>();

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
        ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();
        if (conMan.SliderStrings.value > 0.5) {
            for (int i = 0; i < molecules.Length; ++i) {
                Vector3 molToOrigin = originPosition [i] - molecules [i].transform.position;
                if (molToOrigin.sqrMagnitude > 1.0f) {
                    Rigidbody rb = molecules [i].GetComponent<Rigidbody> ();
                    rb.AddForce (molToOrigin.normalized * repulsiveForce);
                }
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

        // These filenames refer to the prefabs in Assets/Resources/Mesh
        string mol1_se_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
        string mol2_se_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;
        string mol1_bs_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod;
        string mol2_bs_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod;
        string mol1_ca_filename = "Mesh/" + level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod;
        string mol2_ca_filename = "Mesh/" + level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod;

        Debug.Log(mol1_bs_filename);

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
        
        if (init) {
            molecules = new GameObject[2];
            molecules_PDB_mesh = new PDB_mesh[2];
            molecules [0] = mol1.gameObject;
            molecules [1] = mol2.gameObject;
            molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
            molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

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

                //make_molecules(true, MeshTopology.Triangles);

                // This is very grubby, must generalise.
                GameObject mol1 = molecules[0];
                GameObject mol2 = molecules[1];

                originPosition[0] = mol1.transform.position;
                originPosition[1] = mol2.transform.position;

                //create the win condition from the file specified paired atoms
                for (int i = 0; i < molecules_PDB_mesh[0].mol.pairedLabels.Length; ++i)
                {
                    winCondition.Add(new Tuple<int, int>(molecules_PDB_mesh[0].mol.pairedLabels[i].First,
                                      molecules_PDB_mesh[0].mol.pairedLabels[i].Second));
                }
                //debug 3D texture
                //GameObject.Find ("Test").GetComponent<Tex3DMap> ().Build (p1.mol);
                molecules_PDB_mesh[0].other = molecules_PDB_mesh[1].gameObject;
                molecules_PDB_mesh[1].other = molecules_PDB_mesh[0].gameObject;
                molecules_PDB_mesh[0].gameObject.SetActive(false);
                molecules_PDB_mesh[1].gameObject.SetActive(false);

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


            ConnectionManager conMan = gameObject.GetComponent<ConnectionManager>();
            if (molecules != null && molecules.Length >= 2 && conMan.SliderStrings.value <= 0.5)
            {

                // Get a list of atoms that collide.
                GameObject obj0 = molecules[0];
                GameObject obj1 = molecules[1];
                PDB_mesh mesh0 = obj0.GetComponent<PDB_mesh>();
                PDB_mesh mesh1 = obj1.GetComponent<PDB_mesh>();
                Rigidbody r0 = obj0.GetComponent<Rigidbody>();
                Rigidbody r1 = obj1.GetComponent<Rigidbody>();
                Transform t0 = obj0.transform;
                Transform t1 = obj1.transform;
                PDB_molecule mol0 = mesh0.mol;
                PDB_molecule mol1 = mesh1.mol;
                GridCollider b = new GridCollider(mol0, t0, mol1, t1, LJinflation);
                work_done = b.work_done;

                BitArray ba0 = new BitArray(mol0.atom_centres.Length);
                BitArray ba1 = new BitArray(mol1.atom_centres.Length);
                BitArray bab0 = new BitArray(mol0.atom_centres.Length);
                BitArray bab1 = new BitArray(mol1.atom_centres.Length);
                atoms_touching = new BitArray[] { ba0, ba1 };
                atoms_bad = new BitArray[] { bab0, bab1 };
                Matrix4x4 t0mx = t0.localToWorldMatrix;
                Matrix4x4 t1mx = t1.localToWorldMatrix;
                lennard_jones = 0.0f;
                //List<string> debug_csv = new List<string>();

                // Apply forces to the rigid bodies.
                foreach (GridCollider.Result r in b.results)
                {
                    Vector3 ac0 = mol0.atom_centres[r.i0];
                    Vector3 ac1 = mol1.atom_centres[r.i1];
                    Vector3 c0 = t0mx * new Vector4(ac0.x, ac0.y, ac0.z, 1);
                    Vector3 c1 = t1mx * new Vector4(ac1.x, ac1.y, ac1.z, 1);
                    float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
                    float distance = (c1 - c0).magnitude;

                    // see https://en.wikipedia.org/wiki/Lennard-Jones_potential
                    /*
                    // 12-6 potential
                    float ljr = distance / (min_d * 1.122f);
                    float lennard_jones_potential = Mathf.Pow(ljr, -12) - 2 * Mathf.Pow(ljr, -6);
                    float lennard_jones_force = 12 * Mathf.Pow(ljr, -7) - 12 * Mathf.Pow(ljr, -13);
                    */

                    /*float ljr = distance / min_d;
                    float lennard_jones_potential = Mathf.Pow(ljr, -12) - Mathf.Pow(ljr, -6);
                    float lennard_jones_force = 6 * Mathf.Pow(ljr, -7) - 12 * Mathf.Pow(ljr, -13);*/

                    // 8-6 potential
                    float ljr = distance / (min_d * 1.1547f);
                    float lennard_jones_potential = 3 * Mathf.Pow(ljr, -8) - 4 * Mathf.Pow(ljr, -6);
                    float lennard_jones_force = (-8 * 3) * Mathf.Pow(ljr, -9) - (-6 * 4) * Mathf.Pow(ljr, -7);

                    lennard_jones_force = Mathf.Min(lennard_jones_force, LJMax);
                    lennard_jones_force = Mathf.Max(lennard_jones_force, LJMin);
                    //debug_csv.Add(string.Format("{0},{1},{2},{3},{4}", r.i0, r.i1, ljr, lennard_jones_potential, lennard_jones_force));
                    lennard_jones += lennard_jones_potential;

                    num_connections++;

                    Vector3 normal = (c0 - c1).normalized;
                    //normal *= seperationForce * (min_d - distance);

                    normal *= lennard_jones_force * LJseperationForce;
                    r0.AddForceAtPosition(normal, c0);
                    r1.AddForceAtPosition(-normal, c1);

                    if (distance < min_d * 1.2f)
                    {
                        if (!ba0[r.i0]) { num_touching_0++; ba0.Set(r.i0, true); }
                        if (!ba1[r.i1]) { num_touching_1++; ba1.Set(r.i1, true); }
                    } else if (distance < min_d * 0.8f)
                    {
                        num_invalid++;
                        bab0.Set(r.i0, true);
                        bab1.Set(r.i1, true);
                    }
                }

                //System.IO.File.WriteAllLines(@"C:\Users\Public\LJP.txt", debug_csv.ToArray());

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
                    //if (SimpleScore) SimpleScore.text = "Score: " + (num_touching_0 + num_touching_1).ToString() + " atoms touching.";
                }
                else
                {
                    ElectricScore.text = LennardScore.text = "0.0";
                    NumberOfAtoms.text = "0";
                    //SimpleScore.text = "Score: 0 atoms touching.";
                }

                //heuristicScoreSlider.value = num_invalid != 0 ? 1.0f : 1.0f - (num_touching_0 + num_touching_1) * 0.013f;

                //num_invalid = when the physics fails
                //ElectricScore.text = num_invalid != 0 ? ElectricScore.text = (scoring.elecScore).ToString("F2") : "0";

                //LennardScore.text = num_invalid != 0 ? LennardScore.text = (scoring.vdwScore).ToString("F2") : "0";
                //Debug.Log ("num_touching_0: "+num_touching_0+" / num_touching_1: "+num_touching_1);
                //Debug.Log ("num_invalid: "+num_invalid);

            }
            
            InvalidDockScore.SetActive(num_invalid != 0);
            is_score_valid = num_invalid == 0;
            //if (num_invalid != 0 && !sfx.isPlaying(SFX.sound_index.warning))
            //    sfx.PlayTrackDelay(SFX.sound_index.warning, 1.5f);
            //else if (num_invalid == 0 && sfx.isPlaying(SFX.sound_index.warning))
            //    sfx.StopTrack(SFX.sound_index.warning);
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
                   // Debug.Log("1: " + sliders.IsConnectionMade(aas[1], aas[0]) + " - 2: " + sliders.IsConnectionMade(aas[2], aas[3]) + " - 3: " + sliders.IsConnectionMade(aas[4], aas[5]));
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
        {
            TutorialHand.position = new Vector3(5000.0f, 0, 0);
            TutorialHand.gameObject.SetActive(false);
        }
        else
        {
            TutorialHand.gameObject.SetActive(true);
        }

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
    public Material normal_0;
    public Material normal_1;
    public Material bs_0;
    public Material bs_1;
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

    //fix the transparent molecule
    public void FixNormalMolecule(GameObject protein, int id_protein)
    {
        foreach (Transform molecule_renderer in protein.transform)
        {
            molecule_renderer.GetComponent<Renderer>().material = id_protein == 0 ? normal_0 : normal_1;
        }
    }

    //fix the transparent molecule
    public void FixBSMolecule(GameObject protein, int id_protein)
    {
        if(protein.transform.childCount != 0)
        {
            foreach (Transform molecule_renderer in protein.transform)
            {
                molecule_renderer.GetComponent<Renderer>().material = id_protein == 0 ? bs_0 : bs_1;
            }
        }
        else
        {
            protein.GetComponent<Renderer>().material = id_protein == 0 ? bs_0 : bs_1;
        }
    }

    #region DOWNLOAD ASSETS FROM BUNDLE
    //IEnumerator DownloadMolecules()
    //{
    //    Level level = levels[current_level];

    //    string BundleURL = "https://ageofalgo.com/BB/AssetBundles/" + level.pdbFile.ToLower() + "_fbx";
    //    Debug.Log(BundleURL);

    //    uiController.SetHintImage(level.pdbFile); //HINT

    //    // These filenames refer to the fbx in the asset bundle in the server
    //    string mol1_se_filename = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod_bs + ".fbx";
    //    string mol2_se_filename = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod_bs + ".fbx";
    //    string mol1_bs_filename = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs + ".fbx";
    //    string mol2_bs_filename = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs + ".fbx";
    //    // string mol1_ca_filename = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod + ".fbx";
    //    //string mol2_ca_filename = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod + ".fbx";

    //    // Download the file from the URL. It will not be saved in the Cache
    //    using (WWW www = new WWW(BundleURL))
    //    {
    //        yield return www;

    //        if (www.error != null)
    //            throw new System.Exception("WWW download had an error:" + www.error);

    //        AssetBundle bundle = www.assetBundle;

    //        // Make two PDB_mesh instances from the PDB file and a chain selection.
    //        GameObject mol1 = make_molecule(level.pdbFile + "." + level.chainsA, "Proto1", 7, MeshTopology.Triangles, 0);
    //        mol1.transform.SetParent(Molecules);
    //        GameObject mol2 = make_molecule(level.pdbFile + "." + level.chainsB, "Proto2", 7, MeshTopology.Triangles, 1);
    //        mol2.transform.SetParent(Molecules);

    //        //DEFAULT
    //        GameObject mol1_mesh = Instantiate(bundle.LoadAsset(mol1_se_filename)) as GameObject;
    //        mol1_mesh.transform.SetParent(mol1.transform);
    //        FixNormalMolecule(mol1_mesh, 0);

    //        GameObject mol2_mesh = Instantiate(bundle.LoadAsset(mol2_se_filename)) as GameObject;
    //        mol2_mesh.transform.SetParent(mol2.transform);
    //        FixNormalMolecule(mol2_mesh, 1);

    //        // Ioannis
    //        scoring = new PDB_score(mol1.GetComponent<PDB_mesh>().mol, mol1.gameObject.transform, mol2.GetComponent<PDB_mesh>().mol, mol2.gameObject.transform);

    //        //BALLS AND STICK 1
    //        mol1_mesh = Instantiate(bundle.LoadAsset(mol1_bs_filename)) as GameObject;
    //        mol1_mesh.transform.SetParent(mol1.transform);
    //        FixBSMolecule(mol1_mesh, 0);
    //        //mol1_mesh.name = "bs_p1";
    //        mol1_mesh.SetActive(false);
    //        //BALLS AND STICK 1
    //        mol2_mesh = Instantiate(bundle.LoadAsset(mol2_bs_filename)) as GameObject;
    //        mol2_mesh.transform.SetParent(mol2.transform);
    //        FixBSMolecule(mol2_mesh, 1);
    //        //mol2_mesh.name = "bs_p2";
    //        mol2_mesh.SetActive(false);

    //        ////CARBON ALPHA 1
    //        //mol1_mesh = Instantiate(Resources.Load(mol1_ca_filename)) as GameObject;
    //        //mol1_mesh.transform.SetParent(mol1.transform);
    //        ////mol1_mesh.name = "ca_p1";
    //        //mol1_mesh.SetActive(false);
    //        ////CARBON ALPHA 2
    //        //mol2_mesh = Instantiate(Resources.Load(mol2_ca_filename)) as GameObject;
    //        //mol2_mesh.transform.SetParent(mol2.transform);
    //        ////mol2_mesh.name = "ca_p2";
    //        //mol2_mesh.SetActive(false);

    //        //TRANSPARENT 1
    //        mol1_mesh = Instantiate(bundle.LoadAsset(mol1_se_filename)) as GameObject;
    //        mol1_mesh.transform.SetParent(mol1.transform);
    //        // mol1_mesh.name = "transparent_p1";
    //        FixTransparentMolecule(mol1_mesh, 0);
    //        mol1_mesh.SetActive(false);
    //        //TRANSPARENT 2
    //        mol2_mesh = Instantiate(bundle.LoadAsset(mol2_se_filename)) as GameObject;
    //        mol2_mesh.transform.SetParent(mol2.transform);
    //        //mol2_mesh.name = "transparent_p1";
    //        FixTransparentMolecule(mol2_mesh, 1);
    //        mol2_mesh.SetActive(false);


    //        molecules = new GameObject[2];
    //        molecules_PDB_mesh = new PDB_mesh[2];
    //        molecules[0] = mol1.gameObject;
    //        molecules[1] = mol2.gameObject;
    //        molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
    //        molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

    //        Vector3 xoff = new Vector3(level.separation, 0, 0);

    //        reset_molecule(molecules[0], 0, level.offset - xoff);
    //        reset_molecule(molecules[1], 1, level.offset + xoff);


    //        // Unload the AssetBundles compressed contents to conserve memory
    //        bundle.Unload(false);
    //    } // memory is freed from the web stream (www.Dispose() gets called implicitly)

    //    StartGame();
    //}

    //IEnumerator DownloadMolecules()
    //{
    //    Level level = levels[current_level];

    //    string BundleURL = "https://ageofalgo.com/BB/AssetBundles/" + level.pdbFile.ToLower();
    //    //Debug.Log(BundleURL);

    //    uiController.SetHintImage(level.pdbFile); //HINT

    //    // These filenames refer to the fbx in the asset bundle in the server
    //    string mol1_se_filename = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod + ".bytes";
    //    string mol2_se_filename = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod + ".bytes";
    //    string mol1_bs_filename = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs + ".bytes";
    //    string mol2_bs_filename = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs + ".bytes";
    //    // string mol1_ca_filename = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod + ".fbx";
    //    //string mol2_ca_filename = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod + ".fbx";

    //    // Make two PDB_mesh instances from the PDB file and a chain selection.
    //    GameObject mol1 = make_molecule(level.pdbFile + "." + level.chainsA, "Proto1", 7, MeshTopology.Triangles, 0);
    //    mol1.transform.SetParent(Molecules);
    //    GameObject mol2 = make_molecule(level.pdbFile + "." + level.chainsB, "Proto2", 7, MeshTopology.Triangles, 1);
    //    mol2.transform.SetParent(Molecules);

    //    molecules = new GameObject[2];
    //    molecules_PDB_mesh = new PDB_mesh[2];
    //    molecules[0] = mol1.gameObject;
    //    molecules[1] = mol2.gameObject;
    //    molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
    //    molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

    //    Vector3 offset_position_0 = new Vector3(-molecules_PDB_mesh[0].mol.pos.x, -molecules_PDB_mesh[0].mol.pos.y, -molecules_PDB_mesh[0].mol.pos.z);
    //    Vector3 offset_position_1 = new Vector3(-molecules_PDB_mesh[1].mol.pos.x, -molecules_PDB_mesh[1].mol.pos.y, -molecules_PDB_mesh[1].mol.pos.z);


    //    //DEFAULT
    //    GameObject parent_molecule = new GameObject();
    //    parent_molecule.name = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;

    //    //DEFAULT
    //    GameObject parent_molecule_1 = new GameObject();
    //    parent_molecule.name = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;


    //    // Download the file from the URL. It will not be saved in the Cache
    //    using (WWW www = new WWW(BundleURL))
    //    {
    //        yield return www;

    //        if (www.error != null)
    //            throw new System.Exception("WWW download had an error:" + www.error);

    //        AssetBundle bundle = www.assetBundle;

    //        TextAsset txt = bundle.LoadAsset(mol1_se_filename, typeof(TextAsset)) as TextAsset;
    //        byte[] bytes = txt.bytes;
    //        Stream stream = new MemoryStream(bytes);
    //        PLYDecoder(stream, parent_molecule.transform, 0, protein_view.normal);
    //        GameObject transparency_0 = Instantiate(parent_molecule);
    //        parent_molecule.transform.SetParent(mol1.transform);
    //        parent_molecule.transform.Translate(offset_position_0);


    //        txt = bundle.LoadAsset(mol2_se_filename, typeof(TextAsset)) as TextAsset;
    //        bytes = txt.bytes;
    //        stream = new MemoryStream(bytes);
    //        PLYDecoder(stream, parent_molecule_1.transform, 1, protein_view.normal);
    //        GameObject transparency_1 = Instantiate(parent_molecule);
    //        parent_molecule_1.transform.SetParent(mol2.transform);
    //        parent_molecule_1.transform.Translate(offset_position_1);

    //        // Ioannis
    //        scoring = new PDB_score(mol1.GetComponent<PDB_mesh>().mol, mol1.gameObject.transform, mol2.GetComponent<PDB_mesh>().mol, mol2.gameObject.transform);


    //        //BALLS AND STICK 1
    //        parent_molecule = new GameObject();
    //        parent_molecule.name = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs;
    //        txt = bundle.LoadAsset(mol1_bs_filename, typeof(TextAsset)) as TextAsset;
    //        bytes = txt.bytes;
    //        stream = new MemoryStream(bytes);
    //        PLYDecoder(stream, parent_molecule.transform, 0, protein_view.bs);
    //        parent_molecule.transform.SetParent(mol1.transform);
    //        parent_molecule.SetActive(false);
    //        parent_molecule.transform.Translate(offset_position_0);

    //        //BALLS AND STICK 2
    //        parent_molecule = new GameObject();
    //        parent_molecule.name = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs;
    //        txt = bundle.LoadAsset(mol2_bs_filename, typeof(TextAsset)) as TextAsset;
    //        bytes = txt.bytes;
    //        stream = new MemoryStream(bytes);
    //        PLYDecoder(stream, parent_molecule.transform, 1, protein_view.bs);
    //        parent_molecule.transform.SetParent(mol2.transform);
    //        parent_molecule.SetActive(false);
    //        parent_molecule.transform.Translate(offset_position_1);

    //        //TRANSPARENT 1
    //        transparency_0.transform.SetParent(mol1.transform);
    //        // mol1_mesh.name = "transparent_p1";
    //        FixTransparentMolecule(transparency_0, 0);
    //        transparency_0.SetActive(false);
    //        transparency_0.transform.Translate(offset_position_0);

    //        //TRANSPARENT 2
    //        transparency_1.transform.SetParent(mol2.transform);
    //        // mol1_mesh.name = "transparent_p1";
    //        FixTransparentMolecule(transparency_1, 1);
    //        transparency_1.SetActive(false);
    //        transparency_1.transform.Translate(offset_position_1);

    //        Vector3 xoff = new Vector3(level.separation, 0, 0);

    //        reset_molecule(molecules[0], 0, level.offset - xoff);
    //        reset_molecule(molecules[1], 1, level.offset + xoff);


    //        // Unload the AssetBundles compressed contents to conserve memory
    //        bundle.Unload(false);
    //    } // memory is freed from the web stream (www.Dispose() gets called implicitly)

    //}
    string mol1_se_filename;
    string mol2_se_filename;
    string mol1_bs_filename;
    string mol2_bs_filename;
    string mol1_ca_filename;
    string mol2_ca_filename;
    //string mol1_ca_filename = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod + ".fbx";
    //string mol2_ca_filename = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod + ".fbx";

    //creation of the files
    TextAsset mol1_se_filename_txt;
    TextAsset mol2_se_filename_txt;
    TextAsset mol1_bs_filename_txt;
    TextAsset mol2_bs_filename_txt;
    TextAsset mol1_ca_filename_txt;
    TextAsset mol2_ca_filename_txt;
    byte[] txt_bytes;
    Level level;
    GameObject mol1;
    GameObject mol2;
    Vector3 offset_position_0;
    Vector3 offset_position_1;
    public GameObject parent_molecule;
    GameObject parent_molecule_reference;
    Stream stream;
    GameObject transparency_0;
    GameObject transparency_1;


    IEnumerator DownloadMolecules()
    {
        level = levels[current_level];

#if UNITY_WEBGL
        string BundleURL = "https://bioblox3d.org/wp-content/themes/write/game_data/Asset/AssetBundlesWebGL/" + level.pdbFile.ToLower();
#endif

#if UNITY_STANDALONE
        string BundleURL = "https://bioblox3d.org/wp-content/themes/write/game_data/Asset/AssetBundlesWindows/" + level.pdbFile.ToLower();
#endif

        // These filenames refer to the fbx in the asset bundle in the server
        mol1_se_filename = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod + ".bytes";
        mol2_se_filename = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod + ".bytes";
        mol1_bs_filename = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs + ".bytes";
        mol2_bs_filename = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs + ".bytes";
        mol1_ca_filename = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod_bs + ".bytes";
        mol2_ca_filename = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod_bs + ".bytes";

        using (WWW www = new WWW(BundleURL))
        {
            yield return www;

            if (www.error != null)
                throw new System.Exception("WWW download had an error:" + www.error);

            AssetBundle bundle = www.assetBundle;

            mol1_se_filename_txt = bundle.LoadAsset(mol1_se_filename, typeof(TextAsset)) as TextAsset;
            mol2_se_filename_txt = bundle.LoadAsset(mol2_se_filename, typeof(TextAsset)) as TextAsset;
            mol1_bs_filename_txt = bundle.LoadAsset(mol1_bs_filename, typeof(TextAsset)) as TextAsset;
            mol2_bs_filename_txt = bundle.LoadAsset(mol2_bs_filename, typeof(TextAsset)) as TextAsset;
            mol1_ca_filename_txt = bundle.LoadAsset(mol1_ca_filename, typeof(TextAsset)) as TextAsset;
            mol2_ca_filename_txt = bundle.LoadAsset(mol2_ca_filename, typeof(TextAsset)) as TextAsset;

            bundle.Unload(false);
        }

        // Make two PDB_mesh instances from the PDB file and a chain selection.
        mol1 = make_molecule(level.pdbFile + "." + level.chainsA, "Proto1", 7, MeshTopology.Triangles, 0);
        mol1.transform.SetParent(Molecules);
        mol2 = make_molecule(level.pdbFile + "." + level.chainsB, "Proto2", 7, MeshTopology.Triangles, 1);
        mol2.transform.SetParent(Molecules);

        molecules[0] = mol1.gameObject;
        molecules[1] = mol2.gameObject;
        molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
        molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

        // Ioannis scoring
        scoring = new PDB_score(molecules_PDB_mesh[0].mol, mol1.gameObject.transform, molecules_PDB_mesh[1].mol, mol2.gameObject.transform);

        offset_position_0 = new Vector3(-molecules_PDB_mesh[0].mol.pos.x, -molecules_PDB_mesh[0].mol.pos.y, -molecules_PDB_mesh[0].mol.pos.z);
        offset_position_1 = new Vector3(-molecules_PDB_mesh[1].mol.pos.x, -molecules_PDB_mesh[1].mol.pos.y, -molecules_PDB_mesh[1].mol.pos.z);

        System.GC.Collect();

        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
        txt_bytes = mol1_se_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
        transparency_0 = Instantiate(parent_molecule_reference);
        parent_molecule_reference.transform.SetParent(mol1.transform);
        parent_molecule_reference.SetActive(true);
        parent_molecule_reference.transform.Translate(offset_position_0);

        //DEFAULT
        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;
        txt_bytes = mol2_se_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
        transparency_1 = Instantiate(parent_molecule_reference);
        parent_molecule_reference.transform.SetParent(mol2.transform);
        parent_molecule_reference.SetActive(true);
        parent_molecule_reference.transform.Translate(offset_position_1);

        //BALLS AND STICK 1
        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs;
        txt_bytes = mol1_bs_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.bs);
        parent_molecule_reference.transform.SetParent(mol1.transform);
        parent_molecule_reference.SetActive(false);
        parent_molecule_reference.transform.Translate(offset_position_0);

        //BALLS AND STICK 2
        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs;
        txt_bytes = mol2_bs_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.bs);
        parent_molecule_reference.transform.SetParent(mol2.transform);
        parent_molecule_reference.SetActive(false);
        parent_molecule_reference.transform.Translate(offset_position_1);

        //C&A 1
        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod_bs;
        txt_bytes = mol1_ca_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
        parent_molecule_reference.transform.SetParent(mol1.transform);
        parent_molecule_reference.SetActive(false);
        parent_molecule_reference.transform.Translate(offset_position_0);

        //C&A 2
        parent_molecule_reference = Instantiate(parent_molecule);
        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod_bs;
        txt_bytes = mol2_ca_filename_txt.bytes;
        stream = new MemoryStream(txt_bytes);
        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
        parent_molecule_reference.transform.SetParent(mol2.transform);
        parent_molecule_reference.SetActive(false);
        parent_molecule_reference.transform.Translate(offset_position_1);

        //TRANSPARENT 1
        transparency_0.transform.SetParent(mol1.transform);
        // mol1_mesh.name = "transparent_p1";
        FixTransparentMolecule(transparency_0, 0);
        transparency_0.SetActive(false);
        transparency_0.transform.Translate(offset_position_0);

        //TRANSPARENT 2
        transparency_1.transform.SetParent(mol2.transform);
        // mol1_mesh.name = "transparent_p1";
        FixTransparentMolecule(transparency_1, 1);
        transparency_1.SetActive(false);
        transparency_1.transform.Translate(offset_position_1);

        Vector3 xoff = new Vector3(level.separation, 0, 0);

        reset_molecule(molecules[0], 0, level.offset - xoff);
        reset_molecule(molecules[1], 1, level.offset + xoff);

        uiController.SetHintImage(level.pdbFile); //HINT

        StartGame();

        //create_mesh_1();
        //create_mesh_11();
        //create_mesh2();
        //StartGame();
    }

    public GameObject mesh_temp;
    GameObject mesh_reference;

    void PLYDecoder(Stream stream, Transform parent_molecule, int id_protein, protein_view protein_view )
    {
        PlyLoader loader = new PlyLoader();
        Mesh[] mesh = loader.load_memory(stream);

        for (int i = 0; i != mesh.Length; ++i)
        {
            //GameObject g = new GameObject();
            mesh_reference = Instantiate(mesh_temp);
            mesh[i].name = mesh_reference.name = "mesh" + i;
            MeshFilter mf = mesh_reference.AddComponent<MeshFilter>();
            mf.mesh = mesh[i];
            MeshRenderer mr = mesh_reference.AddComponent<MeshRenderer>();
            if (id_protein == 0)
            {
                mr.GetComponent<Renderer>().material = protein_view == protein_view.normal ? normal_0 : protein_view == protein_view.bs ? bs_0 : transparent_0;
            }
            else
            {
                mr.GetComponent<Renderer>().material = protein_view == protein_view.normal ? normal_1 : protein_view == protein_view.bs ? bs_1 : transparent_1;
            }
            mesh_reference.transform.SetParent(parent_molecule);
           // System.GC.Collect();
        }
    }
#endregion

    





    public void download()
    {
        StartCoroutine(DownloadMolecules());
    }

    

    //public void create_mesh_1()
    //{

    //    // Make two PDB_mesh instances from the PDB file and a chain selection.
    //    mol1 = make_molecule(level.pdbFile + "." + level.chainsA, "Proto1", 7, MeshTopology.Triangles, 0);
    //    mol1.transform.SetParent(Molecules);
    //    mol2 = make_molecule(level.pdbFile + "." + level.chainsB, "Proto2", 7, MeshTopology.Triangles, 1);
    //    mol2.transform.SetParent(Molecules);

    //    molecules[0] = mol1.gameObject;
    //    molecules[1] = mol2.gameObject;
    //    molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
    //    molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

    //    // Ioannis scoring
    //    scoring = new PDB_score(molecules_PDB_mesh[0].mol, mol1.gameObject.transform, molecules_PDB_mesh[1].mol, mol2.gameObject.transform);

    //    offset_position_0 = new Vector3(-molecules_PDB_mesh[0].mol.pos.x, -molecules_PDB_mesh[0].mol.pos.y, -molecules_PDB_mesh[0].mol.pos.z);
    //    offset_position_1 = new Vector3(-molecules_PDB_mesh[1].mol.pos.x, -molecules_PDB_mesh[1].mol.pos.y, -molecules_PDB_mesh[1].mol.pos.z);

    //    System.GC.Collect();
    //}
    
    //public void create_mesh_11()
    //{
    //    parent_molecule_reference = Instantiate(parent_molecule);
    //    parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
    //    txt_bytes = mol1_se_filename_txt.bytes;
    //    stream = new MemoryStream(txt_bytes);
    //    PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
    //    transparency_0 = Instantiate(parent_molecule_reference);
    //    parent_molecule_reference.transform.SetParent(mol1.transform);
    //    parent_molecule_reference.transform.Translate(offset_position_0);

    //    //DEFAULT
    //    parent_molecule_reference = Instantiate(parent_molecule);
    //    parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;
    //    txt_bytes = mol2_se_filename_txt.bytes;
    //    stream = new MemoryStream(txt_bytes);
    //    PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
    //    transparency_1 = Instantiate(parent_molecule_reference);
    //    parent_molecule_reference.transform.SetParent(mol2.transform);
    //    parent_molecule_reference.transform.Translate(offset_position_1);

    //     //TRANSPARENT 1
    //    transparency_0.transform.SetParent(mol1.transform);
    //    // mol1_mesh.name = "transparent_p1";
    //    FixTransparentMolecule(transparency_0, 0);
    //    transparency_0.SetActive(false);
    //    transparency_0.transform.Translate(offset_position_0);

    //    //TRANSPARENT 2
    //    transparency_1.transform.SetParent(mol2.transform);
    //    // mol1_mesh.name = "transparent_p1";
    //    FixTransparentMolecule(transparency_1, 1);
    //    transparency_1.SetActive(false);
    //    transparency_1.transform.Translate(offset_position_1);

    //    Vector3 xoff = new Vector3(level.separation, 0, 0);

    //    reset_molecule(molecules[0], 0, level.offset - xoff);
    //    reset_molecule(molecules[1], 1, level.offset + xoff);

    //    uiController.SetHintImage(level.pdbFile); //HINT

    //    System.GC.Collect();
    //}

    //public void create_mesh2()
    //{
    //    //BALLS AND STICK 1
    //    parent_molecule = new GameObject();
    //    parent_molecule.name = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs;
    //    txt_bytes = mol1_bs_filename_txt.bytes;
    //    stream = new MemoryStream(txt_bytes);
    //    PLYDecoder(stream, parent_molecule.transform, 0, protein_view.bs);
    //    parent_molecule.transform.SetParent(mol1.transform);
    //    parent_molecule.SetActive(false);
    //    parent_molecule.transform.Translate(offset_position_0);

    //    //BALLS AND STICK 2
    //    parent_molecule = new GameObject();
    //    parent_molecule.name = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs;
    //    txt_bytes = mol2_bs_filename_txt.bytes;
    //    stream = new MemoryStream(txt_bytes);
    //    PLYDecoder(stream, parent_molecule.transform, 1, protein_view.bs);
    //    parent_molecule.transform.SetParent(mol2.transform);
    //    parent_molecule.SetActive(false);
    //    parent_molecule.transform.Translate(offset_position_1);

    //    System.GC.Collect();

    //}

    //public void start_juego()
    //{
    //    StartGame();
    //}

    //public void create_from_server()
    //{
    //    //DEFAULT
    //    parent_molecule = new GameObject();
    //    parent_molecule.name = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
    //    txt_bytes = mol1_se_filename_txt.bytes;
    //    stream = new MemoryStream(txt_bytes);
    //    PLYDecoder(stream,parent_molecule.transform, 0, protein_view.normal);
    //    transparency_0 = Instantiate(parent_molecule);
    //    parent_molecule.transform.SetParent(mol1.transform);
    //    parent_molecule.transform.Translate(offset_position_0);

    //    System.GC.Collect();

    //}
}

