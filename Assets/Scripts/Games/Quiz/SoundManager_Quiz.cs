using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager_Quiz : MonoBehaviour
{
    [SerializeField] private AudioClip clickUI, rightAns, wrongAns;
    [SerializeField] private AudioSource audioS;

    public void UI_click()
    {
        audioS.PlayOneShot(clickUI, 0.75f);
    }

    public void rightAnsSound()
    {
        audioS.PlayOneShot(rightAns, 0.75f);
    }

    public void wrongAnsSound()
    {
        audioS.PlayOneShot(wrongAns, 0.75f);
    }

}
