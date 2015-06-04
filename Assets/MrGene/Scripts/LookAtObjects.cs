using UnityEngine;
using System.Collections;

public class LookAtObjects : MonoBehaviour
{
    public GameObject GeneMan;

    public Transform target;
    public Transform targetOriginal;
    public Transform cameraWatch;
    public Animator[] animations;
    public Animator[] BothHands;
    public Animator normalMotion;

    public Camera mainCamera;

    //Mouse components
    public Collider coll;
    private bool zoomIn = false;

    private bool loop;
    public int zoom = 10;
    public int defaultOf = 57;
    public float smoothness = 20f;

    //Rotation Variables
    public float rotationDamping = 0.0f;

    //Look at functions for each object
#region
    // Use this for initialization
    void Start()
    {
        LookAtPlayer();
        AnimationGoing();
        normalMotion.speed = 0.5f;
        for (int i = 0; i < BothHands.Length; i++)
        {
            BothHands[i].Update(Time.deltaTime);
        }
    }

    void AnimationGoing()
    {
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].speed = 0;
        }
    }

    void LookAtPlayer()
    {
        Quaternion rotation = Quaternion.LookRotation(cameraWatch.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    }

    #endregion
    //Mouse select functions

    #region

    // Update is called once per frame
    void Update() {

        //Highlighting Messages
        //Select object and zoom in to GeneMan
        //zoomIn = !zoomIn;
      //  mainCamera.transform.LookAt(targetOriginal);
      
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!loop && Physics.Raycast(ray, out hit))
            {
             
                for (int i = 0; i < animations.Length; i++)
                {
                    
                    if (coll.Raycast(ray, out hit, 1000.0f))
                    {
                        animations[i].Play(0);
                        animations[i].speed = 1.0f;
   
                    }
                    else
                    {
                        animations[i].speed = -15.0f;
                    }
                }
                
            }
                //Camera components
                    //mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoom, Time.deltaTime * smoothness);
                    //mainCamera.transform.LookAt(target);
               
                //else
                //{
                //    mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultOf, Time.deltaTime * smoothness);
                //    mainCamera.transform.LookAt(targetOriginal);
                //    zoomIn = false;
                //}
            }
        
        }
   
       

}



#endregion

