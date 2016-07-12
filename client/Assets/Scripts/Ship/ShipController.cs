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
    public GameObject exit_button;
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

    ExploreController explorerController;
    FadeToExplorerController fadePanel;
    UIController uI;

    // Use this for initialization
    void Start()
    {
        speed = cruiseSpeed;
        //rb = GetComponent<Rigidbody>();
        bb = FindObjectOfType<BioBlox>();
        explorerController = FindObjectOfType<ExploreController>();
        fadePanel = FindObjectOfType<FadeToExplorerController>();
        uI = FindObjectOfType<UIController>();
        RayCastingCamera = transform.GetChild(0).GetComponent<Camera>();
        exit_button.SetActive(true);
        //SwitchCameraInside();
        //Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
    }
	
	// Update is called once per frame
	void FixedUpdate () {

       /* time_rotation_acu += Time.fixedDeltaTime;

        if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 || Input.GetKey(KeyCode.I) || Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.LeftShift))
        {
            time_rotation_acu = 0;
            time_rotation = 1;
        }
        if (time_rotation_acu > 4)
            time_rotation = 10;
            */
        /*if (Physics.Raycast(transform.position, Vector3.down, 3))
            GetComponent<Rigidbody>().AddForce(Vector3.up * 2);*/

        /* Ray ray = new Ray(transform.position, -transform.up);
         RaycastHit hit;

         if (Physics.Raycast(ray, out hit, 5))
         {
             float proportionalHeight = (5 - hit.distance) / 5;
             Vector3 appliedHoverForce = Vector3.up * proportionalHeight * 30;
             GetComponent<Rigidbody>().AddForce(appliedHoverForce, ForceMode.Acceleration);
         }*/


        BvhSphereCollider molecule_1 = new BvhSphereCollider(bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, transform.position, 2.0f);
        BvhSphereCollider molecule_2 = new BvhSphereCollider(bb.molecules[1].GetComponent<PDB_mesh>().mol, bb.molecules[1].transform, transform.position, 2.0f);

        if (molecule_1.results.Count > 0)
        {
            for (int i = 0; i < molecule_1.results.Count; i++)
            {
                Vector3 dir = transform.position - bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[i].index]);
                //float distancia = Vector3.Distance(transform.position, bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[i].index]));
                dir = dir.normalized;
                GetComponent<Rigidbody>().AddForce((dir * 250));
            }
        }

        if (molecule_2.results.Count > 0)
        {
            for (int i = 0; i < molecule_2.results.Count; i++)
            {
                Vector3 dir = transform.position - bb.molecules[1].transform.TransformPoint(bb.molecules[1].GetComponent<PDB_mesh>().mol.atom_centres[molecule_2.results[i].index]);
                //float distancia = Vector3.Distance(transform.position, bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[i].index]));
                dir = dir.normalized;
                GetComponent<Rigidbody>().AddForce((dir * 250));
            }
        }
        //Debug.Log(bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[molecule_1.results[0].index]));

        /*float altitude = PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, new Ray(transform.position, -transform.up), gameObject);
        if (altitude < 3.0f)
        {
            Debug.Log("puja");
            GetComponent<Rigidbody>().velocity = Vector3.zero;

        }*/

        //Debug.Log(PDB_molecule.collide_ray_distance_object(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, new Ray(transform.position, -transform.up), gameObject));


        //ANGULAR DYNAMICS//

        shipRot = transform.GetChild(1).localEulerAngles; //make sure you're getting the right child (the ship).  I don't know how they're numbered in general.
       // Debug.Log("antes: " + shipRot.x + "-" + shipRot.y + "-" + shipRot.z);
        //if ((shipRot.x < 1.0f || shipRot.x < -358 || shipRot.x > 358) && (shipRot.y < 1.0f || shipRot.y < -358 || shipRot.y > 358) && (shipRot.z < 1.0f || shipRot.z < -358 || shipRot.z > 358))
           // shipRot = Vector3.zero;

       // Debug.Log("despues: " + shipRot.x + "-" + shipRot.y + "-" + shipRot.z);

        //since angles are only stored (0,360), convert to +- 180
        if (shipRot.x > 180) shipRot.x -= 360;
        if (shipRot.y > 180) shipRot.y -= 360;
        if (shipRot.z > 180) shipRot.z -= 360;

        //vertical stick adds to the pitch velocity
        //         (*************************** this *******************************) is a nice way to get the square without losing the sign of the value
        angVel.x += Input.GetAxis("Vertical") * Mathf.Abs(Input.GetAxis("Vertical")) * sensitivity_xy * Time.fixedDeltaTime;

        //horizontal stick adds to the roll and yaw velocity... also thanks to the .5 you can't turn as fast/far sideways as you can pull up/down
        float turn = Input.GetAxis("Horizontal") * Mathf.Abs(Input.GetAxis("Horizontal")) * sensitivity * Time.fixedDeltaTime;
        angVel.y += turn * .5f;
        angVel.z -= turn * .5f;


        //shoulder buttons add to the roll and yaw.  No deltatime here for a quick response
        //comment out the .y parts if you don't want to turn when you hit them
        if (Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.I))
        {
            angVel.y -= 20;
            angVel.z += 50;
            speed -= 5 * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.O))
        {
            angVel.y += 20;
            angVel.z -= 50;
            speed -= 5 * Time.fixedDeltaTime;
        }


        //your angular velocity is higher when going slower, and vice versa.  There probably exists a better function for this.
        angVel /= 1 + deltaSpeed * .001f;

        //this is what limits your angular velocity.  Basically hard limits it at some value due to the square magnitude, you can
        //tweak where that value is based on the coefficient
        angVel -= angVel.normalized * angVel.sqrMagnitude * .08f * Time.fixedDeltaTime;


        //and finally rotate.  
        transform.GetChild(1).Rotate(angVel * Time.fixedDeltaTime);

        //this limits your rotation, as well as gradually realigns you.  It's a little convoluted, but it's
        //got the same square magnitude functionality as the angular velocity, plus a constant since x^2
        //is very small when x is small.  Also realigns faster based on speed.  feel free to tweak
        //aca el problema
        transform.GetChild(1).Rotate(-shipRot.normalized * .025f * (shipRot.sqrMagnitude + 500) * (1 + speed / maxSpeed) * Time.fixedDeltaTime);


        //LINEAR DYNAMICS//

        deltaSpeed = speed - cruiseSpeed;

        //This, I think, is a nice way of limiting your speed.  Your acceleration goes to zero as you approach the min/max speeds, and you initially
        //brake and accelerate a lot faster.  Could potentially do the same thing with the angular stuff.
        decel = speed - minSpeed;
        accel = maxSpeed - speed;

        //simple accelerations
        if (Input.GetKey(KeyCode.Joystick1Button1) || Input.GetKey(KeyCode.LeftShift))
            speed += accel * Time.fixedDeltaTime;
        else if (Input.GetKey(KeyCode.LeftControl))
            speed -= accel * Time.fixedDeltaTime;
        //else if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space))
           // speed -= decel * Time.fixedDeltaTime;

        //if not accelerating or decelerating, tend toward cruise, using a similar principle to the accelerations above
        //(added clamping since it's more of a gradual slowdown/speedup)
        else if (Mathf.Abs(deltaSpeed) > .1f)
            speed -= Mathf.Clamp(deltaSpeed * Mathf.Abs(deltaSpeed), -30, 100) * Time.fixedDeltaTime;


        //moves camera (make sure you're GetChild()ing the camera's index)
        //I don't mind directly connecting this to the speed of the ship, because that always changes smoothly
        transform.GetChild(0).localPosition = cameraOffset + new Vector3(0, 0, -deltaSpeed * .02f);


        float sqrOffset = transform.GetChild(1).localPosition.sqrMagnitude;
        Vector3 offsetDir = transform.GetChild(1).localPosition.normalized;


        //this takes care of realigning after collisions, where the ship gets displaced due to its rigidbody.
        //I'm pretty sure this is the best way to do it (have the ship and the rig move toward their mutual center)
        //transform.GetChild(1).Translate(-offsetDir * sqrOffset * 20 * Time.fixedDeltaTime);
        //(**************** this ***************) is what actually makes the whole ship move through the world!
        transform.Translate((offsetDir * sqrOffset * 50 + transform.GetChild(1).forward * speed) * Time.fixedDeltaTime, Space.World);

        //Debug.Log(shipRot.x+"-"+shipRot.y+"-"+shipRot.z);
        //comment this out for starfox, remove the x and z components for shadows of the empire, and leave the whole thing for free roam
        transform.Rotate(shipRot.x * Time.fixedDeltaTime, (shipRot.y * Mathf.Abs(shipRot.y) * .02f) * Time.fixedDeltaTime, shipRot.z * Time.fixedDeltaTime);
    }

    GameObject temp_beacon;

    void Update()
    {
        //LASER
        if (Input.GetMouseButton(1))
        {
            Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
            atom_name.SetActive(true);
            scanning.SetActive(true);

            Ray ray = RayCastingCamera.ScreenPointToRay(Input.mousePosition);
            //MOLECULE 1 ATOM ID UI
            int atomID_molecule_1 = PDB_molecule.collide_ray(gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, ray);
            int atom_id_molecule_1 = bb.molecules[0].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule_1);
            //MOLECULE 2 ATOM ID UI
            int atomID_molecule_2 = PDB_molecule.collide_ray(gameObject, bb.molecules[1].GetComponent<PDB_mesh>().mol, bb.molecules[1].transform, ray);
            int atom_id_molecule_2 = bb.molecules[1].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule_2);
            //change the text in the UI depending which atom is being raycasted
            if (atom_id_molecule_1 >= 0)
                atom_name.GetComponent<Text>().text = bb.molecules[0].GetComponent<PDB_mesh>().mol.aminoAcidsNames[bb.molecules[0].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule_1)] + " - " + bb.molecules[0].GetComponent<PDB_mesh>().mol.aminoAcidsTags[bb.molecules[0].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule_1)];
            else if (atom_id_molecule_2 >= 0)
                atom_name.GetComponent<Text>().text = bb.molecules[1].GetComponent<PDB_mesh>().mol.aminoAcidsNames[bb.molecules[1].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule_2)] + " - " + bb.molecules[1].GetComponent<PDB_mesh>().mol.aminoAcidsTags[bb.molecules[1].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule_2)];
            else
                atom_name.GetComponent<Text>().text = "";
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            atom_name.SetActive(false);
            scanning.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.V))
            SwitchCameraInside();

        if (Input.GetKeyDown(KeyCode.Home))
        {
            transform.position = new Vector3(0, -600, -8);
            transform.LookAt(new Vector3(5, -600, -4));
        }
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            transform.position = new Vector3(0, 70, 5);
            transform.LookAt(new Vector3(0, 0, 5));
        }
        
        //place beacon
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //save the current beacon before creating one
            if (temp_beacon)
                explorerController.StoreBeacons(temp_beacon);

            Ray ray = RayCastingCamera.ScreenPointToRay(Input.mousePosition);
            //MOLECULE 1 ATOM ID UI
            int atomID_molecule_1 = PDB_molecule.collide_ray(gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, ray);
            //int atom_id_molecule_1 = bb.molecules[0].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule_1);
            //MOLECULE 2 ATOM ID UI
            int atomID_molecule_2 = PDB_molecule.collide_ray(gameObject, bb.molecules[1].GetComponent<PDB_mesh>().mol, bb.molecules[1].transform, ray);
            //int atom_id_molecule_2 = bb.molecules[1].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule_2);
            //change the text in the UI depending which atom is being raycasted
            if (atomID_molecule_1 >= 0)
            {
                GameObject temp = Instantiate(beacon);
                temp.transform.position = bb.molecules[0].transform.TransformPoint(bb.molecules[0].GetComponent<PDB_mesh>().mol.atom_centres[atomID_molecule_1]);

                Ray r_temp;
                do
                {
                    temp.transform.position -= temp.transform.forward * 5;
                    r_temp = RayCastingCamera.ScreenPointToRay(temp.transform.forward);

                } while (PDB_molecule.collide_ray(bb.molecules[0].gameObject, bb.molecules[0].GetComponent<PDB_mesh>().mol, bb.molecules[0].transform, r_temp) != -1);
                temp.transform.position -= temp.transform.forward * 20;
                temp.transform.LookAt(bb.molecules[0].transform.position);
                temp.transform.SetParent(bb.molecules[0].transform);
                explorerController.StoreBeacons(temp);

            }
            else if (atomID_molecule_2 >= 0)
            {
                GameObject temp = Instantiate(beacon);
                temp.transform.position = bb.molecules[1].transform.TransformPoint(bb.molecules[1].GetComponent<PDB_mesh>().mol.atom_centres[atomID_molecule_2]);

                Ray r_temp;
                do
                {
                    temp.transform.position -= temp.transform.forward * 5;
                    r_temp = RayCastingCamera.ScreenPointToRay(temp.transform.forward);

                } while (PDB_molecule.collide_ray(bb.molecules[1].gameObject, bb.molecules[1].GetComponent<PDB_mesh>().mol, bb.molecules[1].transform, r_temp) != -1);
                temp.transform.position -= temp.transform.forward * 20;
                //temp.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                temp.transform.LookAt(bb.molecules[1].transform.position);
                temp.transform.SetParent(bb.molecules[1].transform);
                explorerController.StoreBeacons(temp);
            }
            //show the color panel
            //ColorPanel.SetActive(true);
        }
    }

    void SwitchCameraInside()
    {
        //camera_in.enabled = !view_status;
        //camera_out.enabled = view_status;
        cabine.SetActive(!view_status);
        exit_button.SetActive(!view_status);
        ship.enabled = view_status;
        if (view_status)
        {
            cameraOffset = new Vector3(0, 1, -3);
        }
        else
        {

            cameraOffset = new Vector3(0, 0, 0);
        }
        view_status = !view_status;
    }

    public void EndExplore()
    {
        atom_name.SetActive(false);
        scanning.SetActive(false);
        exit_button.SetActive(false);
        FindObjectOfType<UIController>().EndExplore();
    }

    public void BeaconColor(int index)
    {
       // temp_beacon.transform.GetChild(0).transform.GetChild(0).GetComponent<Light>().color = beacon_colors[index];
       // temp_beacon.transform.GetChild(1).GetComponent<MeshRenderer>().material = material_colors[index];
    }

    public void CloseColorPanel()
    {
        ColorPanel.SetActive(false);
        Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
        InsideUI = false;
    }

    public void DeleteBeacon()
    {
        Destroy(temp_beacon);
        temp_beacon = null;
        CloseColorPanel();
    }

    public void MouseUIEnter()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        InsideUI = true;
    }

    public void MouseUIExit()
    {
        Cursor.SetCursor(cursor_aim, Vector2.zero, CursorMode.Auto);
        InsideUI = false;
    }
}
