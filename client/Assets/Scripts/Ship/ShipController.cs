using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShipController : MonoBehaviour {

    Rigidbody rb;
    BioBlox bb;
    //speed stuff
    float speed;
    int cruiseSpeed = 0;
    float deltaSpeed;//(speed - cruisespeed)
    public int minSpeed = 0;
    public int maxSpeed = 1;
    float accel, decel;

    //turning stuff
    Vector3 angVel;
    Vector3 shipRot;
    int sensitivity = 800;
    int sensitivity_xy = 200;
    
    public GameObject atom_name;
    public GameObject scanning;
    public GameObject aim;
    public Canvas canvas;
    public GameObject cabine;
    public MeshRenderer ship;
    public Texture2D cursor_aim;
    public GameObject ColorPanel;
    bool view_status = false;
    Camera RayCastingCamera;

    //color panel
    bool InsideUI = false;
    //Color[] beacon_colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.white, Color.black };
    //public Material[] material_colors;

    public GameObject beacon;

    public Vector3 cameraOffset = new Vector3(0, 1, -3); 

    float time_rotation = 1;
    float time_rotation_acu = 0;
    
    UIController uI;
    SFX sfx;
    AminoSliderController aminoSlider;

    // Use this for initialization
    void Start()
    {
        bb = FindObjectOfType<BioBlox>();
        uI = FindObjectOfType<UIController>();
        RayCastingCamera = transform.GetChild(0).GetComponent<Camera>();
        sfx = FindObjectOfType<SFX>();
        //SwitchCameraInside();
        //Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
        bb.molecules[0].GetComponent<PDB_mesh>().ship_camera = transform.GetChild(0).GetComponent<Camera>();
        bb.molecules[1].GetComponent<PDB_mesh>().ship_camera = transform.GetChild(0).GetComponent<Camera>();
        uI.atom_name = transform.GetChild(2).transform.GetChild(1).GetComponent<Text>();
    }

    void Update()
    {
        //LASER
        if (Input.GetMouseButton(1))
        {
            if(!sfx.isPlaying(SFX.sound_index.ship_scanning))
                sfx.PlayTrack(SFX.sound_index.ship_scanning);
            Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
            scanning.SetActive(true);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            atom_name.SetActive(false);
            scanning.SetActive(false);
            sfx.StopTrack(SFX.sound_index.ship_scanning);
        }

        if (Input.GetKey(KeyCode.UpArrow))
            transform.Rotate(new Vector3(-1, 0, 0));

        if (Input.GetKey(KeyCode.DownArrow))
            transform.Rotate(new Vector3(1, 0, 0));


        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(new Vector3(0, -1, 0));

        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(new Vector3(0, 1, 0));

        if (Input.GetKey(KeyCode.Space))
            transform.Translate(new Vector3(0, 0, 0.5f));

        if (Input.GetKey(KeyCode.LeftControl))
            transform.Translate(new Vector3(0, 0, -0.5f));
    }
}
