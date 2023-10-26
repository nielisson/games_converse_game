using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_Hit_A_Mole : MonoBehaviour
{
    [SerializeField] private List<Mole> moles;
    
    [Header("UI objects")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameUI;
    
    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private TMPro.TextMeshProUGUI GameOverText;
    [SerializeField] private GameObject bombText;
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    [SerializeField] private AudioSource audioS;

    
    // Hardcoded variables you may want to tune
    private float startingTime = 30f;

    //Global Variables
    private float timeRemaining;
    private HashSet<Mole> currentMoles = new HashSet<Mole>();
    private int score;
    private bool playing = false;

    public void StartGame()
    {
        audioS.volume = 0.9f;

        // Hide/show the Ui elements we do/don't want to see
        playButton.SetActive(false);
        GameOverUI.SetActive(false);
        bombText.SetActive(false);
        gameUI.SetActive(true);


        // Hide all the visible moles
        for (int i = 0; i < moles.Count; i++)
        {
            moles[i].Hide();
            moles[i].SetIndex(i);
        }

        //Remove any old game state
        currentMoles.Clear();
        //Start with 30 sec
        timeRemaining = startingTime;
        score = 0;
        scoreText.text = "0";
        playing = true;
    }
    

    public void GameOver (int type)
    {
        audioS.volume = 0.35f;

        //Show the message
        if (type == 0)
        {
            GameOverUI.SetActive (true);
            GameOverText.text = "Score: " + score;
        }
        else
        {
            GameOverUI.SetActive(true);
            GameOverText.text = "Score: " + score;
        }
        // Hide all moles
        foreach (Mole mole in moles)
        {
            mole.StopGame();
        }
        // Stop the game and show the start UI
        playing = false;
        playButton.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            //update time
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                GameOver(0);
            }
            timeText.text = $"{(int)timeRemaining / 60}:{(int)timeRemaining % 60:D2}";
            // Check if we need to start any more moles
            if (currentMoles.Count <= (score / 10))
            {
                // Choose a random mole.
                int index = Random.Range(0, moles.Count);
                // doesnt matter if it's already doing something, we will just try again next frame
                if (!currentMoles.Contains(moles[index]))
                {
                    currentMoles.Add(moles[index]);
                    moles[index].Activate(score /10);
                }
            }

        }
        
    }
    public void AddScore(int moleIndex)
    {
        //Add and update score
        score += 1;
        scoreText.text = $"{score}";
        //Increase time by a little bit
        timeRemaining += 2;
        // remove from active moles
        currentMoles.Remove(moles[moleIndex]);
    }

    public void Missed(int moleIndex, bool isMole)
    {
        if (isMole)
        {
            // Decrease time by a little bit
            timeRemaining -= 2;
        }
        // Remove from active moles
        currentMoles.Remove(moles[moleIndex]);
    }
}
