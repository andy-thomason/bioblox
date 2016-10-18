using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using AssemblyCSharp;

public class PDB_mesh : MonoBehaviour {
    public PDB_molecule mol;
    public GameObject other;
    public int protein_id;

    //bool allowInteraction=true;

    //Quaternion start;
    //Quaternion end;
    //bool startRotation=false;
    public bool shouldCollide = false;
    float t=0;
    AminoSliderController aminoSliderController;
    public int atom;
    public Camera ship_camera;

    // add atom indices to here to display them selected
    public int[] selected_atoms = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
    SFX sfx;
    int last_atom_scanned = -1;

    //public bool hasCollided=false;

    // Use this for initialization
    void Start () {
        mol = PDB_parser.get_molecule(this.name);
        aminoSliderController = FindObjectOfType<AminoSliderController> ();
        meshes = GetComponentsInChildren<MeshRenderer>();
        sfx = FindObjectOfType<SFX>();
    }

    public void AlignPointToVector(Vector3 point, Vector3 targetDir)
    {
        //Vector3 localPos = point;
        //Vector3 startDir = localPos;
        
        //Quaternion targetQ=Quaternion.LookRotation(targetDir);
        //Quaternion startQ=Quaternion.LookRotation(startDir);
        //start=transform.rotation;
        
        //Quaternion toFront = targetQ * Quaternion.Inverse (startQ);

        //end=toFront;
        //startRotation=true;
        t=0;
    }

    public Vector3 GetAtomWorldPositon(int atomIndex)
    {
        return transform.TransformPoint (mol.atom_centres [atomIndex]);
    }

    public void BringAtomToFocus(int atomIndex)
    {
        Camera c = GameObject.FindGameObjectWithTag ("MainCamera").
        GetComponent<Camera> ();

        Vector3 localPos = mol.atom_centres [atomIndex];
        BringPointToFocus (localPos, c);
    }

    //rotates this object to bring that point closest to the camera
    void BringPointToFocus(Vector3 localPoint,Camera c)
    {
        //Vector3 startDir = localPoint;
        //Vector3 targetDir=c.transform.position-this.transform.position;
        
        //Quaternion targetQ=Quaternion.LookRotation(targetDir);
        //Quaternion startQ=Quaternion.LookRotation(startDir);
        //start=transform.rotation;
        
        //Quaternion toFront = targetQ * Quaternion.Inverse (startQ);
        
        
        //end=toFront;
        //startRotation=true;
        t=0;
    }

    public void OneAxisDock()
    {
        shouldCollide = true;
        //allowInteraction = false;
        //startRotation = false;

        Rigidbody rigidBody1 = gameObject.GetComponent<Rigidbody> ();
        Rigidbody rigidBody2 = other.GetComponent<Rigidbody> ();

        rigidBody1.constraints = RigidbodyConstraints.FreezeRotation;
        rigidBody2.constraints = RigidbodyConstraints.None;

        rigidBody1.drag = 2.0f;
        rigidBody2.drag = 2.0f;

        Vector3 usToOth = other.transform.position - gameObject.transform.position;
        Vector3 othToUs = gameObject.transform.position - other.transform.position;


        Vector3 anchor = transform.InverseTransformPoint (transform.position + usToOth.normalized);
        Vector3 otherAnchor = other.transform.InverseTransformPoint (other.transform.position + othToUs.normalized);

        SpringJoint j = gameObject.AddComponent<SpringJoint> ();

        j.anchor = anchor;
        j.connectedBody = rigidBody2;
        j.autoConfigureConnectedAnchor = false;
        j.connectedAnchor = otherAnchor;
        j.damper = 6.0f;
        j.spring = 0.5f;
    }

    public void AutoDock(int[] thisAtomIndicies, int[] otherAtomIndicies)
    {
        shouldCollide = true;
        //allowInteraction = false;
        //startRotation = false;
        PDB_molecule otherMol = other.GetComponent<PDB_mesh> ().mol;
        Rigidbody r1 = gameObject.GetComponent<Rigidbody> ();
        Rigidbody r2 = other.GetComponent<Rigidbody> ();

        r1.constraints = RigidbodyConstraints.None;
        r2.constraints = RigidbodyConstraints.None;
        r1.drag = 2.0f;
        r2.drag = 2.0f;

        for (int i=0; i<thisAtomIndicies.Length; ++i) {
            int index1=thisAtomIndicies[i];
            int index2=otherAtomIndicies[i];

            Vector3 atomPos1 = mol.atom_centres[index1];
            Vector3 atomPos2 =otherMol.atom_centres[index2];

            float atomRad1= mol.atom_radii[index1];
            float atomRad2=otherMol.atom_radii[index2];

            SpringJoint j=gameObject.AddComponent<SpringJoint>();

            j.anchor=atomPos1;
            j.connectedBody=r2;
            j.autoConfigureConnectedAnchor=false;
            j.connectedAnchor=atomPos2;
            j.damper=3.0f;
            j.spring=3.0f;
            j.minDistance=atomRad1 + atomRad2 -0.5f;
        }
    }

    public bool OneAxisHasDocked()
    {
        Tuple<int,int>[] pairs = mol.spring_pairs;
        PDB_molecule otherMol = other.GetComponent<PDB_mesh> ().mol;
        for(int i = 0; i < pairs.Length; i++)
        {
            Vector3 pos1 = transform.TransformPoint(mol.atom_centres[mol.serial_to_atom[pairs[i].First]]);
            Vector3 pos2 = other.transform.TransformPoint(otherMol.atom_centres[otherMol.serial_to_atom[pairs[i].Second]]);

            float rad=mol.atom_radii[mol.serial_to_atom[pairs[i].First]];

            float sqrdDist= (pos1-pos2).sqrMagnitude;
            float sqrdRad = (rad+rad)*(rad*rad);
            float offset = 0.4f;

            if(sqrdDist > sqrdRad + offset)
            {
                return false;
            }
        }
        return true;
    }

    public bool HasDocked ()
    {
        for (int i=0; i<gameObject.GetComponents<SpringJoint>().Length; ++i) {
            SpringJoint s=gameObject.GetComponents<SpringJoint>()[i];

            Vector3 wpos1=transform.TransformPoint(s.anchor);
            Vector3 wpos2=other.transform.TransformPoint(s.connectedAnchor);

            float dist = (wpos1-wpos2).sqrMagnitude;

            if(dist > s.minDistance*s.minDistance)
            {
                return false;
            }
        }
        return true;
    }

    //at the moment very fake
    public void AutoDockCheap()
    {
        shouldCollide = false;
        this.transform.position = mol.pos;
        this.transform.rotation = Quaternion.identity;
    }

    Vector3 lastMousePos;
    bool rotating = false;
    bool has_rotated = false;
    Camera cam;
    Ray ray;
    BioBlox bb;
    ButtonStructure buttonStructure;
    UIController uIController;
    MeshRenderer[] meshes;
    //Vector3 light_pos;
    ExploreController exploreController;
    //test
    GameObject camera_first_person;

    void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        bb = (BioBlox)GameObject.FindObjectOfType(typeof(BioBlox));
        buttonStructure = FindObjectOfType<ButtonStructure>();
        uIController = FindObjectOfType<UIController>();
        camera_first_person = GameObject.Find("CameraFirstPerson");
        exploreController = FindObjectOfType<ExploreController>();
    }

    //Camera main_camera;
    //bool testi = false;

    // Update is called once per frame
    void Update () {

        ray = cam.ScreenPointToRay (Input.mousePosition);
        //create a ray to the cursor and cast it, if it hits at all
        int atomID = PDB_molecule.collide_ray (gameObject, mol, transform, ray);
        Vector3 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0) && !uIController.isOverUI)
        {
            if (atomID != -1)
            {
                // "rotating" only gets set if we first click on an atom.
                rotating = true;
                has_rotated = false;
            }
        }
        else if (Input.GetMouseButton(0))
        { //left mouse button
            if (rotating)
            {
                if (t > 0.3f)
                {
                    //if there is no recent input reset previous position
                    lastMousePos = Input.mousePosition;
                }
                //t = 0.0f;
                Vector3 mouseDelta = mousePos - lastMousePos;

                if (mouseDelta.magnitude != 0)
                {
                    has_rotated = true;
                }
                Vector3 dirRight = Vector3.right;
                Vector3 dirUp = Vector3.up;

                transform.RotateAround(transform.position, dirRight, mouseDelta.y);
                transform.RotateAround(transform.position, dirUp, -mouseDelta.x);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // click without movement selects an atom/amino acid.
            if (rotating && !has_rotated)
            {
                Ray r = cam.ScreenPointToRay(Input.mousePosition);
                atom = PDB_molecule.collide_ray(gameObject, mol, transform, r);
                if (atom != -1)
                {
                    SelectAtom(atom);
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    //first person only spawn camera
                    //if (uIController.first_person)
                    //{
                    //    uIController.ChangeCCTVLoading();
                    //    //only 1 active
                    //    GameObject check_new = GameObject.FindGameObjectWithTag("FirstPerson");
                    //    if (check_new)
                    //    {
                    //        Destroy(check_new);
                    //        uIController.FirstPersonCameraReference = null;
                    //        uIController.first_person_protein = -1;
                    //    }
                    //    GameObject temp = Instantiate(camera_first_person);
                    //    //set the camera
                    //    uIController.FirstPersonCameraReference = temp;
                    //    uIController.FirstPersonCameraReferenceCamera = temp.GetComponent<Camera>();
                    //    //setting the refernce of which protein is being used for the camera
                    //    uIController.first_person_protein = protein_id;

                    //    temp.tag = "FirstPerson";
                    //    temp.transform.SetParent(transform, false);

                    //    temp.GetComponent<Animator>().enabled = true;

                    //    temp.GetComponent<MouseOrbitImproved>().protein = gameObject;
                    //    temp.GetComponent<MouseOrbitImproved>().enabled = true;

                    //    if (protein_id == 0)
                    //        uIController.DropDownP1.value = UIController.protein_render.transparent.GetHashCode();
                    //    else
                    //        uIController.DropDownP2.value = UIController.protein_render.transparent.GetHashCode();
                    //}

                    //EXPLORER MODE ONLY - PLACE SHIP
                    //if (uIController.explore_view)
                    //{
                    //    sfx.StopTrack(SFX.sound_index.ship);
                    //    uIController.ChangeCCTVLoading();
                    //    //only 1 active
                    //    GameObject check_new = GameObject.FindGameObjectWithTag("space_ship");
                    //    if (check_new) Destroy(check_new);
                    //    GameObject temp = Instantiate(space_ship);
                    //    sfx.PlayTrackDelay(SFX.sound_index.ship, 0.6f);
                    //    temp.tag = "space_ship";
                    //    //temp.transform.SetParent(transform, false);
                    //    temp.transform.position = transform.TransformPoint(mol.atom_centres[atom]);

                    //    Ray r_temp;
                    //    do
                    //    {
                    //        temp.transform.position -= temp.transform.forward * 5;
                    //        r_temp = cam.ScreenPointToRay(temp.transform.forward);

                    //    } while (PDB_molecule.collide_ray(gameObject, mol, transform, r_temp) != -1);
                    //    temp.transform.position -= temp.transform.forward * 20;
                    //    temp.transform.GetChild(0).GetComponent<Animator>().enabled = true;
                    //    Vector3 temp_pos = protein_id == 0 ? bb.molecules[0].transform.position : bb.molecules[1].transform.position;
                    //    temp.transform.LookAt(temp_pos);
                    //    exploreController.StartExplore(temp);
                    //}

                }
            }
            has_rotated = false;
            rotating = false;
        }
        else if (Input.GetMouseButton(1) && uIController.explore_view)
        {
            Ray r = ship_camera.ScreenPointToRay(Input.mousePosition);
            atom = PDB_molecule.collide_ray(gameObject, mol, transform, r);
            if (atom != -1 && last_atom_scanned != atom)
            {
                SelectAtom(atom);
                uIController.SetAtomNameExplorerView(mol.aminoAcidsNames[return_atom_id(atom)] + " - " + mol.aminoAcidsTags[return_atom_id(atom)]);
                last_atom_scanned = atom;
            }
        }
        else
        {
            has_rotated = false;
            rotating = false;
        }
        
        lastMousePos = mousePos;
    }

    public float select_fudge = 0.67f;

    public Vector4[] GetSelectedAtomUniforms(Camera camera) {
        int len = selected_atoms.Length;
        Vector4[] result = new Vector4[11]; // see PDB.shader _Atom0..10
        int imax = Mathf.Min (len, result.Length);
        float scale = select_fudge * Mathf.Tan (camera.fieldOfView * (3.14159f/180));
        for (int i = 0; i != imax; ++i) {
            int sel = selected_atoms[i];
            Vector3 atom_pos = transform.TransformPoint(mol.atom_centres[sel]);
            Vector3 screen_pos = camera.WorldToViewportPoint(atom_pos);
            float size = scale * screen_pos.z / mol.atom_radii[sel];
            result[i] = new Vector4(screen_pos.x, screen_pos.y, size * camera.aspect, size);
        }
        for (int i = len; i < result.Length; ++i) {
            result[i] = new Vector4(0, 0, 1e37f, 1e37f);
        }
        return result;
    }

    // call this to select an amino acid
    public void SelectAminoAcid(int acid_number)
    {
        selected_atoms = mol.aminoAcidsAtomIds[acid_number];
        //Debug.Log("Aminoacids selected:" + acid_number);
        //go through the atoms of the amino acids
        for (int i = 0; i != selected_atoms.Length; ++i)
        {
            //Debug.Log("amino acid atoms id:" + selected_atoms[i]);
            //DECODER
            int remainder1;
            int quotient1 = Math.DivRem(mol.names[selected_atoms[i]], Convert.ToInt32(0x1000000), out remainder1);
            int remainder2;
            int quotient2 = Math.DivRem(remainder1, Convert.ToInt32(0x10000), out remainder2);
            int remainder3;
            int quotient3 = Math.DivRem(remainder2, Convert.ToInt32(0x100), out remainder3);
            string atom_name = string.Concat(Convert.ToChar(quotient1), Convert.ToChar(quotient2), Convert.ToChar(quotient3), Convert.ToChar(remainder3)).Trim();

            //Debug.Log("atom_name: " + atom_name);
        }
    }

    // call this to deselect amino acids
    public void DeselectAminoAcid() {
        selected_atoms = new int[0];
    }

    public void SelectAtom(int atom) {
        for (int i = 0; i != mol.aminoAcidsAtomIds.Count; ++i) {
            int[] ids = mol.aminoAcidsAtomIds[i];
            for (int j = 0; j != ids.Length; ++j)
            {
                if (ids[j] == atom)
                {
                    SelectAminoAcid(i);                    
                    aminoSliderController.HighLight3DMesh(i,protein_id);
                    return;
                }
            }
        }
    }

    public int return_atom_id(int atom)
    {
        int id_atom = -1;
        for (int i = 0; i != mol.aminoAcidsAtomIds.Count; ++i)
        {
            int[] ids = mol.aminoAcidsAtomIds[i];
            for (int j = 0; j != ids.Length; ++j)
            {
                if (ids[j] == atom)
                {
                    id_atom = i;
                }
            }
        }
        return id_atom;
    }
}
