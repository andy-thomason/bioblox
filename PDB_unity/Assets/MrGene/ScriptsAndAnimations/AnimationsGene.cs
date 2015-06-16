using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class AnimationsGene : MonoBehaviour
{
    public Animator[] animations = null;
   // public Animator hello = null;
    //public Animator goodbye = null;
    public Renderer questionMark = null;
   // public GameObject Gene = null;

    public Collider gene;
    // public Collider question;
    private bool loop = false;

    //Look at functions for each object
    #region
    // Use this for initialization
    void Start()
    {
              
        AnimationGoing();

    }

    private void AnimationGoing()
    {

        //introduction.speed = 0;
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].speed = 1.0f;
            //   animations[i] = GetComponent<Animator>();

        }
    }
    public enum AnimationControl
    {
        Main,
        Hello,
        Explaining,
        Goodbye
    }
    public AnimationControl anims;

    public void UpdateAnims()
    {
        for (int i = 0; i < animations.Length; i++)
        {
            switch (anims)
            {
                case AnimationControl.Hello:
                 //   animations[i].
                    break;


            }
        }
    }

    //Question mark render mask
    #region
    void QuestionRenderOff()
    {
        questionMark.enabled = false;

    }

    IEnumerator QuestionRenderOn()
    {
        questionMark.enabled = false;
        yield return new WaitForSeconds(1);
        questionMark.enabled = true;

    }
    #endregion

    #endregion
    //Mouse select
    #region

    // Update is called once per frame
    void Update()
    {
        //AnimationGoing();

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Works");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!loop && Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < animations.Length; i++)
                {

                    if (gene.Raycast(ray, out hit, 1000.0f)) //|| question.Raycast(ray, out hit, 1000.0f))
                    {

                        //animations[i].Play(0);
                        //animations[i].speed = 1.0f;
                        //QuestionRenderOff();
                        QuestionRenderOff();

                    }
                    //else
                    //{
                    //    animations[i].speed = -2.0f;

                    //}
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Works");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!loop && Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < animations.Length; i++)
                {

                    if (gene.Raycast(ray, out hit, 1000.0f)) //|| question.Raycast(ray, out hit, 1000.0f))
                    {
                        AnimationClip clip = new AnimationClip();

                        animations[i].Play(0);
                        animations[i].speed = 1.0f;
                        QuestionRenderOff();

                    }
                    else
                    {
                        animations[i].Play("Goodbye", 1);

                    }
                }

                //if (gene.Raycast(ray, out hit, 1000.0f)) //|| question.Raycast(ray, out hit, 1000.0f))
                //{
                //    int runState = 0;
                //    QuestionRenderOn();
                //    runState = Animator.StringToHash("Goodbye");
                //    goodbye.SetBool(runState, true);

                //}
                //else
                //{
                //   //

                //}
            }
        }
    }
}

#endregion
