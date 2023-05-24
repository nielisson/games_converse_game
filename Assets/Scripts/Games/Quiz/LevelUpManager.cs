using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelUpManager : MonoBehaviour
{
    public TMP_Text levelText;
    [SerializeField] TMP_Text cheeringText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] string[] cheerText;
    [SerializeField] string[] contentText;
    


    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LvlUPTextUpdate()
    {
        int randNumber = Random.Range(0, 4);

        if (randNumber == 0)
        {
            cheeringText.text = cheerText[0];
            bodyText.text = contentText[0];
        }

        if (randNumber == 1)
        {
            cheeringText.text = cheerText[1];
            bodyText.text = contentText[1];
        }

         if (randNumber == 2)
        {
            cheeringText.text = cheerText[2];
            bodyText.text = contentText[2];
        }

         if (randNumber == 3)
        {
            cheeringText.text = cheerText[3];
            bodyText.text = contentText[3];
        }

         if (randNumber == 4)
        {
            cheeringText.text = cheerText[4];
            bodyText.text = contentText[4];
        }


        Debug.Log("the number for the Cheer text" + randNumber);
    }

    
}
