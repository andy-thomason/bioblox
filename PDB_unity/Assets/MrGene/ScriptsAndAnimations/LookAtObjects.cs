using UnityEngine;
using System.Collections;

public class LookAtObjects : MonoBehaviour
{
    public Animator[] animations;
    public Animator[] BothHands;
    public Animator normalMotion;

    private bool firstA = false;
    //Mouse components
    public Collider coll;
    //   private bool zoomIn = false;

    private bool loop = false;
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
        AnimationGoing();
        normalMotion.speed = 0.5f;
    }

    void AnimationGoing()
    {
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].speed = 0;
        }
    }

 

    #endregion
    //Mouse select functions
    #region

    // Update is called once per frame
    void Update() {
        //AnimationGoing();

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
                        firstA = true;
                    }
                    else
                    {
                        animations[i].StopPlayback();
                        animations[i].Play(0);
                        animations[i].speed = -15.0f;
                    }
                }

            }
            else
            {
                for (int i = 0; i < animations.Length; i++)
                {
                    if(firstA)
                    {
                            //Debug.Log(animations[i].GetCurrentAnimatorStateInfo(0).nameHash);
                            animations[i].StopPlayback();
                            animations[i].Play(0);
                            animations[i].speed = -15.0f;
                    }
                }
            }  
            }
        
        }
   
}



#endregion

