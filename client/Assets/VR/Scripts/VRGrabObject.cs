using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabObject : MonoBehaviour
{
    GameObject laser;
    BioBlox bb;
    Rigidbody protein_0;
    Rigidbody protein_1;
    // control
    private SteamVR_TrackedObject trackedObj;
    // get the obejct to grab
    private GameObject collidingObject;
    // currebntly grabing
    private GameObject objectInHand;
    // control access
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    int index_right_hand;
    int index_left_hand;
    SFX sfx;
    float timer_for_game_mode;
    bool cambio = false;

    GameMode g_mo;
    GameManager gm;

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        bb = FindObjectOfType<BioBlox>();
        sfx = FindObjectOfType<SFX>();
        gm = FindObjectOfType<GameManager>();
        g_mo = FindObjectOfType<GameMode>();
        laser = transform.GetChild(2).gameObject;

        index_left_hand = (int)SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost)).index;
        index_right_hand = (int)SteamVR_Controller.Input(SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost)).index;
    }

    void Start()
    {
        //protein_0 = bb.molecules[0].GetComponent<Rigidbody>();
        //protein_1 = bb.molecules[1].GetComponent<Rigidbody>();
        bb.set_up_vr();
    }

    private void SetCollidingObject(Collider col)
    {
        // not going to grab if I have something in hand
        if (collidingObject || !col.GetComponent<Rigidbody>())
        {
            return;
        }
        // 2
        collidingObject = col.gameObject;
        bb.is_grabbed = true;
    }


    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    // 2
    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    // 3
    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject)
        {
            return;
        }

        collidingObject = null;
        bb.is_grabbed = false;
    }

    private void GrabObject_0()
    {
        bb.molecules[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
        bb.molecules[0].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        var joint = AddFixedJoint();
        joint.connectedBody = bb.molecules[0].GetComponent<Rigidbody>();
    }

    private void GrabObject_1()
    {
        bb.molecules[1].GetComponent<Rigidbody>().velocity = Vector3.zero;
        bb.molecules[1].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        var joint = AddFixedJoint();
        joint.connectedBody = bb.molecules[1].GetComponent<Rigidbody>();
    }

    private void GrabObject()
    {
        //collidingObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //collidingObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        var joint = AddFixedJoint();
        joint.connectedBody = collidingObject.GetComponent<Rigidbody>();
    }

    // 3
    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 200000;
        fx.breakTorque = 200000;
        return fx;
    }

    public void ReleaseObject_0()
    {
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            //bb.molecules[0].GetComponent<Rigidbody>().velocity = Controller.velocity * 100;
        }
    }

    public void ReleaseObject_1()
    {
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            //bb.molecules[1].GetComponent<Rigidbody>().velocity = Controller.velocity * 100;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!g_mo.game_over)
        {
            // press trigger
            if (Controller.GetHairTriggerDown() && collidingObject)
            {
                sfx.PlayTrack(SFX.sound_index.protein_pick);
                //izq 3 der 2
                //if (Controller.index == index_left_hand)
                //{
                //    StartCoroutine(scale_protein_0());
                //    GrabObject_0();
                //}
                //else
                //{
                //    StartCoroutine(scale_protein_1());
                //    GrabObject_1();
                //}
               // StartCoroutine(collidingObject.GetComponent<PDB_mesh>().protein_id == 0 ? scale_protein_0() : scale_protein_1());
                GrabObject();
            }

            // release trigger
            if (Controller.GetHairTriggerUp())
            {
                //izq 3 der 2
                if (Controller.index == index_left_hand)
                    ReleaseObject_0();
                else
                    ReleaseObject_1();
            }

            //// press grip
            //if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            //{
            //    //izq 3 der 2
            //    if (Controller.index == control_hand.left.GetHashCode())
            //        bb.ChangeProteinRenderer(0);
            //    else
            //        bb.ChangeProteinRenderer(1);
            //}

            ////ONLY RIGHT
            //if(Controller.index == control_hand.right.GetHashCode())
            //{
            //    if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            //    {
            //        bb.is_scanning_amino = true;
            //        laser.SetActive(true);
            //    }
            //    if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
            //    {
            //        bb.is_scanning_amino = false;
            //        laser.SetActive(false);
            //    }
            //}

            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                if (Controller.GetAxis().x > 0.6f && bb.renders_enable)
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    if (Controller.index == index_left_hand)
                        bb.ChangeProteinRenderer_forward(0);
                    else
                        bb.ChangeProteinRenderer_forward(1);
                }
                else if(Controller.GetAxis().x < -0.6f && bb.renders_enable)
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    if (Controller.index == index_left_hand)
                        bb.ChangeProteinRenderer_backwards(0);
                    else
                        bb.ChangeProteinRenderer_backwards(1);
                }
                else
                {
                    if (Controller.index == index_right_hand)
                    {
                        sfx.PlayTrack(SFX.sound_index.ship_scanning);
                        bb.is_scanning_amino = true;
                        laser.SetActive(true);
                    }
                }
            }

            if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && Controller.index == index_right_hand)
            {
                bb.is_scanning_amino = false;
                sfx.StopTrack(SFX.sound_index.ship_scanning);
                laser.SetActive(false);
            }

                //// press grip
                //if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip))
                //{
                //    bb.Molecules.transform.Rotate(Controller.angularVelocity);
                //}
        }
        else
        {

        }
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            FindObjectOfType<BioBlox>().restart_position();
            FindObjectOfType<GameMode>().restart();
            FindObjectOfType<SetHeight>().set_height();
        }
        //game_mode
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Grip) && !cambio && Controller.index == index_right_hand && !bb.exhibition)
        {
            timer_for_game_mode += Time.deltaTime;

            if(timer_for_game_mode > 3.0f)
            {
                cambio = true;
                gm.is_game_mode = !gm.is_game_mode;
                g_mo.switch_mode(gm.is_game_mode);
            }
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            timer_for_game_mode = 0;
            cambio = false;
        }


        ////position on the pad
        //if (Controller.GetAxis() != Vector2.zero)
        //{
        //    if (Controller.GetAxis().x > 0)
        //        transform.parent.Translate(Vector3.right);
        //    if (Controller.GetAxis().x < 0)
        //        transform.parent.Translate(Vector3.left);
        //    if (Controller.GetAxis().y > 0)
        //        transform.parent.Translate(Vector3.forward);
        //    if (Controller.GetAxis().y < 0)
        //        transform.parent.Translate(Vector3.back);
        //}

    }

    IEnumerator scale_protein_0()
    {
        bb.molecules[0].transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        yield return new WaitForSeconds(0.1f);
        bb.molecules[0].transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator scale_protein_1()
    {
        bb.molecules[1].transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        yield return new WaitForSeconds(0.1f);
        bb.molecules[1].transform.localScale = new Vector3(1, 1, 1);
    }


}
