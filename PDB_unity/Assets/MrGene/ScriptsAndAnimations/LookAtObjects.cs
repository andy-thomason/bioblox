using UnityEngine;
using System.Collections;

public class LookAtObjects : MonoBehaviour
{
    public Animator[] animations = null;
    public Animator questionAnim = null;

    public Renderer questionMark = null;
  
    public Collider gene;
    public Collider question;
    private bool loop = false;

    int clickHash = Animator.StringToHash("Explaining");


    //Look at functions for each object
    #region
    // Use this for initialization
    void Start()
    {
        AnimationGoing();
    
    }

    void AnimationGoing()
    {
        //introduction.speed = 0;
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].speed = 0;
        }
    }
    
    //Question mark render mask
    #region
    void QuestionRenderOff()
    {
        //questionAnim.Play(0);
        //questionMark.enabled = true;
        //yield return new WaitForSeconds(1);
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
                        animations[i].Play(0);
                        animations[i].speed = 1.0f;
                        QuestionRenderOff();

                    }
                    //else {
                    //    animations[i].speed = -15.0f;

                    //}
                }
            }
        }
    }
}

#endregion
