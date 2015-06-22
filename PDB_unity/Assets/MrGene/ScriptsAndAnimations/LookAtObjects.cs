using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class LookAtObjects : MonoBehaviour
{
    public Animator[] animations = null;
    public Animator questionAnim = null;
    public Animator goodbye = null;
    public Renderer questionMark = null;
    public GameObject Gene = null;
  
    public Collider gene;
   // public Collider question;
    private bool loop = false;
    
    //Look at functions for each object
    #region
    // Use this for initialization
    void Start()
    {
	questionMark.transform.localScale=new Vector3(0,0,0);
		this.GetComponent<Animator>().SetBool("Explaining",true);
       // Gene = GameObject.FindGameObjectWithTag("Gene");
    }

	void OnDisable()
	{
		this.GetComponent<AudioSource> ().enabled = false;

	}

    void AnimationGoing()
    {
        
        //introduction.speed = 0;
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i].speed = 1;
            animations[i] = GetComponent<Animator>();
            
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
				this.GetComponent<AudioSource>().Stop();
				this.GetComponent<Animator>().SetBool("Explaining",false);
				questionMark.gameObject.SetActive(true);
				questionMark.gameObject.transform.localScale=new Vector3(0,0,0);

				this.GetComponent<Animator>().Play("Goodbye");

				//for (int i = 0; i nimations.Length; i++)
//                {
//                    
//                    if (gene.Raycast(ray, out hit, 1000.0f)) //|| question.Raycast(ray, out hit, 1000.0f))
//                    {
//                      
//                       
//                        animations[i].Play(0);
//                        animations[i].speed = 1.0f;
//                        QuestionRenderOff();
//                        
//                    }
//                    else
//                    {
//                        animations[i].speed = -1;
//
//                    }
//                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Works");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!loop && Physics.Raycast(ray, out hit))
            {
               
                    if (gene.Raycast(ray, out hit, 1000.0f)) //|| question.Raycast(ray, out hit, 1000.0f))
                    {
                        int runState = 0;
                        QuestionRenderOn();
                        runState = Animator.StringToHash("Goodbye");
                        goodbye.SetBool(runState, true);
                        
                    }
                    else
                    {
                       //

                    }
            }
        }
    }
}

#endregion
