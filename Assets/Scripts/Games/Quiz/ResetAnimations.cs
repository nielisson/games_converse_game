using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetAnimations : MonoBehaviour
{
    Animator heartAnimator;

    // Start is called before the first frame update
    void Start()
    {
       heartAnimator = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetAnim()
    {
       heartAnimator.SetBool("LostLife", false);  
       
    }
}
