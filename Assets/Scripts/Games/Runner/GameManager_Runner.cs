using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager_Runner : MonoBehaviour
{
 
    public GameObject obstacle;
    public GameObject powerUp;
    public Transform spawnPointObstacle;
    public Transform spawnPointPowerUp;
    public static int score = 0;
    public static int level = 1;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public GameObject playButton;
    public GameObject player;

    void Awake ()
    {
       
    }


   

    void Update()
    {
        
    }

    IEnumerator SpawnObjects()
    {
        while (true)
        {
            float waitTime = Random.Range(0.75f, 2f);
            yield return new WaitForSeconds(waitTime);
            Instantiate(obstacle, spawnPointObstacle.position, Quaternion.identity);
   
        }

    }


    IEnumerator SpawnPowerUp()
    {
        while (true)
        {
            // float waitTime2 = Random.Range(4f, 15f);
            float waitTime2 = Random.Range(1f, 5f);
            yield return new WaitForSeconds(waitTime2);
            Instantiate(powerUp, spawnPointPowerUp.position, Quaternion.identity);
        }
        
    }

    public void ScoreUp()
    {
        score++;
        scoreText.text = score.ToString();


        if (score >= 10 && level == 1)
        {
            level++;
            levelText.text = level.ToString(); 
       
        }

        if (score >= 20 && level == 2)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 30 && level == 3)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 40 && level == 4)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 40 && level == 5)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 50 && level == 6)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 60 && level == 7)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 70 && level == 8)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 80 && level == 9)
        {
            level++;
            levelText.text = level.ToString();
        }

        if (score >= 90 && level == 10)
        {
            //game over
            Debug.Log("game over");
        }

    }


    public void GameStart()
    {
        player.SetActive(true);
        playButton.SetActive(false);

        StartCoroutine("SpawnObjects");
        StartCoroutine("SpawnPowerUp");
        InvokeRepeating("ScoreUp", 3f, 1f);


       

    }
}
