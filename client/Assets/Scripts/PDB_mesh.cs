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
    OverlayRenderer or;
    public int atom;

    // add atom indices to here to display them selected
    public int[] selected_atoms = new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
    SFX sfx;
    int last_atom_scanned = -1;

    public int number_of_current_atoms;

    //public bool hasCollided=false;

    // Use this for initialization
    void Start () {
        //mol = PDB_parser.get_molecule(this.name);
        aminoSliderController = FindObjectOfType<AminoSliderController> ();
        meshes = GetComponentsInChildren<MeshRenderer>();
        sfx = FindObjectOfType<SFX>();
        or = FindObjectOfType<OverlayRenderer>();
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
    public Camera cam;
    Ray ray;
    BioBlox bb;
    ButtonStructure buttonStructure;
    UIController uIController;
    MeshRenderer[] meshes;
    //Vector3 light_pos;
    ExploreController exploreController;
    GameObject camera_first_person;
    Transform mo_m;
    Vector3 camera_position;
    Quaternion camera_rotation;

    void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        bb = (BioBlox)GameObject.FindObjectOfType(typeof(BioBlox));
        buttonStructure = FindObjectOfType<ButtonStructure>();
        uIController = FindObjectOfType<UIController>();
        camera_first_person = GameObject.Find("CameraFirstPerson");
        exploreController = FindObjectOfType<ExploreController>();
        mo_m = GameObject.FindGameObjectWithTag("camera_holder").transform;
    }

    //Camera main_camera;
    //bool testi = false;

    // Update is called once per frame
    void Update () {

        //get if the camera is rotating - for desabling the amino
        if (Input.GetMouseButtonDown(1))
        {
            camera_rotation = mo_m.localRotation;
            camera_position = mo_m.localPosition;
        }

        //create the ray
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
                }
            }
            has_rotated = false;
            rotating = false;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (camera_rotation == mo_m.localRotation && camera_position == mo_m.localPosition)
            {
                Ray r = cam.ScreenPointToRay(Input.mousePosition);
                atom = PDB_molecule.collide_ray(gameObject, mol, transform, r);
                if (atom != -1)
                {
                    GetAminoId(atom);
                }
            }
            //Ray r = cam.ScreenPointToRay(Input.mousePosition);
            //atom = PDB_molecule.collide_ray(gameObject, mol, transform, r);
            //if (atom != -1 && last_atom_scanned != atom)
            //{
            //    SelectAtom(atom);
            //    uIController.SetAtomNameExplorerView(mol.aminoAcidsNames[return_atom_id(atom)] + " - " + mol.aminoAcidsTags[return_atom_id(atom)]);
            //    last_atom_scanned = atom;
            //}
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

    int P1_atom_index;
    int P2_atom_index;

    // call this to select an amino acid
    public void SelectAminoAcid(int acid_number)
    {
        selected_atoms = mol.aminoAcidsAtomIds[acid_number];
        P1_atom_index = P2_atom_index = 0;
        //CLEAN THE BUTTONS
        if (protein_id == 0)
        {
            aminoSliderController.atom_selected_p1 = -1;
            uIController.P1CleanAtomButtons();
            //go through the atoms of the amino acids
            for (int i = 0; i != selected_atoms.Length; ++i)
            {
                int name = mol.names[selected_atoms[i]];
                int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                uIController.P1CreateAtomButtons(selected_atoms[i], protein_id, mol.atomNames[selected_atoms[i]], atom, acid_number, P1_atom_index);
                P1_atom_index++;
            }
            or.P1_selected_atom_id = selected_atoms[selected_atoms.Length - 1];
            aminoSliderController.ScaleAtomAtGenerationP1(selected_atoms.Length - 1);
        }
        else
        {
            aminoSliderController.atom_selected_p2 = -1;
            uIController.P2CleanAtomButtons();
            //go through the atoms of the amino acids
            for (int i = 0; i != selected_atoms.Length; ++i)
            {
                int name = mol.names[selected_atoms[i]];
                int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                uIController.P2CreateAtomButtons(selected_atoms[i], protein_id, mol.atomNames[selected_atoms[i]], atom, acid_number, P2_atom_index);
                P2_atom_index++;
            }
            or.P2_selected_atom_id = selected_atoms[selected_atoms.Length - 1];
            aminoSliderController.ScaleAtomAtGenerationP2(selected_atoms.Length - 1);
        }
    }

    // call this to select an amino acid
    public void SelectAminoAcid_when_connection_clicked(int acid_number, int atom_index)
    {
        selected_atoms = mol.aminoAcidsAtomIds[acid_number];
        P1_atom_index = P2_atom_index = 0;
        //CLEAN THE BUTTONS
        if (protein_id == 0)
        {
            aminoSliderController.atom_selected_p1 = -1;
            uIController.P1CleanAtomButtons();
            //go through the atoms of the amino acids
            for (int i = 0; i != selected_atoms.Length; ++i)
            {
                int name = mol.names[selected_atoms[i]];
                int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                uIController.P1CreateAtomButtons(selected_atoms[i], protein_id, mol.atomNames[selected_atoms[i]], atom, acid_number, P1_atom_index);
                P1_atom_index++;
            }
            or.P1_selected_atom_id = atom_index;
        }
        else
        {
            aminoSliderController.atom_selected_p2 = -1;
            uIController.P2CleanAtomButtons();
            //go through the atoms of the amino acids
            for (int i = 0; i != selected_atoms.Length; ++i)
            {
                int name = mol.names[selected_atoms[i]];
                int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                uIController.P2CreateAtomButtons(selected_atoms[i], protein_id, mol.atomNames[selected_atoms[i]], atom, acid_number, P2_atom_index);
                P2_atom_index++;
            }
            or.P2_selected_atom_id = atom_index;
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

    public void GetAminoId(int atom)
    {
        for (int i = 0; i != mol.aminoAcidsAtomIds.Count; ++i)
        {
            int[] ids = mol.aminoAcidsAtomIds[i];
            for (int j = 0; j != ids.Length; ++j)
            {
                if (ids[j] == atom)
                {
                    aminoSliderController.SliderMol[protein_id].transform.GetChild(i).GetComponent<AminoButtonController>().DisableAmino();
                    aminoSliderController.HighLight3DMesh(i, protein_id);
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
