using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonAnimation : MonoBehaviour
{
    public AudioSource audioS;
    public AudioClip btnSquish;
    [Range(0, 1)]
    public float soundVolume;

    [Space]
    [Header("Distorting Anim")]
    public float speed = 0.2f;

    public int xNorm, yNorm, zNorm;

    public int MaxSizeDistort;

    // Start is called before the first frame update
    void Start()
    {
      
    }


    void Update()
    {
        
    }

    public void buttonAnim()
    {
        
        audioS.PlayOneShot(btnSquish, soundVolume);
        StartCoroutine(SquashAndStretch());
    }

    IEnumerator SquashAndStretch()
    {
        this.gameObject.transform.localScale += new Vector3(MaxSizeDistort, 1, 1) * Time.deltaTime * speed;
        yield return new WaitForSeconds(0.1f);
        this.gameObject.transform.localScale = new Vector3(xNorm, yNorm, zNorm);
        yield return new WaitForSeconds(0.1f);
        this.gameObject.transform.localScale += new Vector3(1, MaxSizeDistort, 1) * Time.deltaTime * speed;
        yield return new WaitForSeconds(0.1f);
        this.gameObject.transform.localScale = new Vector3(xNorm, yNorm, zNorm);
    }

}
