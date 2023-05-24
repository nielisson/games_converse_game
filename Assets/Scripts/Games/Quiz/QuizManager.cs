using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class QuizManager : MonoBehaviour
{
    
    [SerializeField] RewardedAdsButton rewardAdScript;
    [SerializeField] GameObject AnsReveal;
    public GameObject heartIcon;
    public GameObject successParticles;
    public TMP_Text levelTxt;
    public TMP_Text lifePointAmount;
    public TMP_Text nickName;
    public TMP_Text retryPowerUPText;
    public TMP_Text scoreLeaderboard;
    public TMP_Text rightAnsReveal;
    public Image fullBar;
    public LeaderboardQuiz leaderboard;
    public GameObject leaderboardPanel;
    public GameObject GameOverScreen;
    public LevelUpManager levelUPscript;
    [SerializeField] GameObject LevelUpScreen;
    [SerializeField] private QuizUI quizUI;
    [SerializeField] private QuizDataScriptable quizDataMixed, quizDataGeography, quizDataCulture, quizDataHistory, quizDataPolitics;
    private List<Question> questions;
    private Question selectedQuestion;
    
    public int lifePoint;

    public int score;

    public int retryPowerUp;

    public int firstRun;

    private int level;

    private Animator heartAnimator;

    [Space]
    [Header ("Polishing + Non Essential Improvements")]
    [SerializeField] Animator plusAnimController;
    [SerializeField] TMP_Text retryTextGameOver;

    [Space]
    [Header ("Juiceness")]

    [SerializeField] GameObject initialPos1;
    [SerializeField] GameObject initialPos2;
    [SerializeField] GameObject initialPos3;
    [SerializeField] float spread;
    [SerializeField] GameObject vfxScore1, vfxScore2, vfxScore3;

    public bool btn1Selected, btn2Selected, btn3Selected;


    [Space]
    [Header ("Animation settings")]
    [SerializeField] [Range(0.5f, 0.9f)] float minAnimDuration;
    [SerializeField] [Range(0.9f, 2f)] float maxAnimDuration;
    [SerializeField] Ease easeType;
    [SerializeField] GameObject targetPosition;



    // Start is called before the first frame update
    void Start()
    {
        AnsReveal.SetActive(false);

        Vector3 vfxScoreInitial1 = vfxScore1.transform.localScale;
        Vector3 vfxScoreInitial2 = vfxScore1.transform.localScale;
        Vector3 vfxScoreInitial3 = vfxScore1.transform.localScale;




        successParticles.SetActive(false);

        heartAnimator = heartIcon.GetComponent<Animator>();

        firstRun = PlayerPrefs.GetInt("savedFirstRun") ;
        
        if (firstRun == 0) 
        {

        firstRun = 1;
        PlayerPrefs.SetInt("savedFirstRun", 1) ;

        Debug.Log("<color=red>this is the first time you are playing</color>");

        retryPowerUp = 3;

        PlayerPrefs.SetInt("MyRetryPowerUp", 3);

        retryPowerUPText.text = retryPowerUp.ToString();

        
        lifePoint = 5;

        PlayerPrefs.SetInt("MyLife", 5);
        
        lifePointAmount.text = lifePoint.ToString();
       
        
        level = 0;
        levelTxt.text = "0";


        fullBar.fillAmount = 0f;

        PlayerPrefs.SetFloat("MyFillingBar", fullBar.fillAmount);



        
        nickName.text = PlayerPrefs.GetString("PlayerID").ToString();

        }
        
        else
        {

        Debug.Log("<color=red>NOT THE FIRST TIME YOU PLAYING</color>");

        nickName.text = PlayerPrefs.GetString("PlayerID");

        
        retryPowerUp = PlayerPrefs.GetInt("MyRetryPowerUp");

        PlayerPrefs.SetInt("MyRetryPowerUp", retryPowerUp);

        retryPowerUPText.text = retryPowerUp.ToString();



        PlayerPrefs.GetInt("MyLife");
        lifePoint = PlayerPrefs.GetInt("MyLife");
        lifePointAmount.text = lifePoint.ToString();
        
        score = PlayerPrefs.GetInt("MyScore");

        
        level = PlayerPrefs.GetInt("MyLevel");
        LevelUp();


        PlayerPrefs.GetFloat("MyFillingBar");

        fullBar.fillAmount = PlayerPrefs.GetFloat("MyFillingBar");

        

        // PlayerPrefs.SetFloat("MyFillingBar", fullBar.fillAmount);

       
        Debug.Log ("<color=red> your current score is </color>"+ score);


/////// Score Animations
        vfxScore1.SetActive(false);
        vfxScore2.SetActive(false);
        vfxScore3.SetActive(false);
        
        }   

/// Windows ///

        GameOverScreen.SetActive(false);
        leaderboardPanel.SetActive(false);
        LevelUpScreen.SetActive(false);
    }

    void Update()
    {
        ///Change the color of the Retry amount
         if (retryPowerUp <= 0)
        {
            
            retryPowerUPText.GetComponent<TMP_Text>().color = Color.red; // changes the color to red when no more super powers
            plusAnimController.SetBool("isRewardBtnReady", true);

            retryTextGameOver.text = "PUB"; ///TO translate

        }
        else
        {
            retryPowerUPText.GetComponent<TMP_Text>().color = Color.white; // changes the color back
            plusAnimController.SetBool("isRewardBtnReady", false);
            
            retryTextGameOver.text = retryPowerUPText.text + "/3";
        }
    }

    void SelectQuestion() 
    {

        int val = Random.Range(0, questions.Count);

        selectedQuestion = questions[val];

        quizUI.SetQuestion(selectedQuestion);

    }

    public bool Answer(string answered)
    {
        bool correctAns = false;
        if (answered == selectedQuestion.correctAns && answered != null)
        //if (answered == selectedQuestion.correctAns) 
        {
            //CORRECT ANSWER
            
            correctAns = true;

            score += 100;

            ScoreAnim();
            
            PlayerPrefs.SetInt("MyScore", score);

            fullBar.fillAmount += 0.10f;
            
            PlayerPrefs.SetFloat("MyFillingBar", fullBar.fillAmount);

            successParticles.SetActive(true);

            StartCoroutine(SetInactive());
            

            if (fullBar.fillAmount >= 1f)
            {
                fullBar.fillAmount = 0;
                PlayerPrefs.SetFloat("MyFillingBar", 0);
                
                //change level
                LevelUp();
               

            }
           
           
        }
        else
        {
            //WRONG ANSWER
            lifePoint--;
        
            lifePointAmount.text = lifePoint.ToString();

            PlayerPrefs.SetInt("MyLife", lifePoint);

            //Heart Anim
            heartAnimator.SetBool("LostLife", true);

            successParticles.SetActive(false);

            /////REVEAL RIGHT ANSWER
            AnsReveal.SetActive(true);
            rightAnsReveal.text = selectedQuestion.correctAns;


/// Game Over ///
            if(lifePoint < 1)
            {   
                scoreLeaderboard.text = score.ToString();
                leaderboardPanel.SetActive(false);
                GameOverScreen.SetActive(true); 
                heartAnimator.SetBool("LostLife", false); 
                quizUI.stopQuestionSound = true;
            }
        
        }

        Invoke("SelectQuestion", 0.4f);

        return correctAns;
    }


    IEnumerator UpdateLeaderboard()
    {
        yield return leaderboard.SubmitScoreRoutine(score);
        yield return new WaitForSeconds(0.85f);
        leaderboardPanel.SetActive(true);
    }

  
    IEnumerator SetInactive()
    {
        yield return new WaitForSeconds(4f);
        successParticles.SetActive(false);
    }

/// Leveling Up ///
    void LevelUp()
    {
        Debug.Log("your score is " + score);

        levelUPscript.LvlUPTextUpdate();
        


        if (score < 1000)
        {
            level = 0;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 1000 && score < 2000)
        {
            level = 1;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 2000 && score < 3000)
        {
            level = 2;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 3000 && score < 4000)
        {
            level = 3;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 4000 && score < 5000)
        {
            level = 4;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 5000 && score < 6000)
        {
            level = 5;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 6000 && score < 7000)
        {
            level = 6;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 7000 && score < 8000)
        {
            level = 7;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 8000 && score < 9000)
        {
            level = 8;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 9000 && score < 10000)
        {
            level = 9;
            levelTxt.text = level.ToString();
            PlayerPrefs.SetInt("MyLevel", level);
        }

        if (score >= 10000)
        {
            level = 10;
            levelTxt.text = "MAX";
            PlayerPrefs.SetInt("MyLevel", level);
        }

        levelUPscript.levelText.text = level.ToString();
        LevelUpScreen.SetActive(true);


    }



/// GAME OVER FUNCTIONS ///

///REPRENDRE
    public void ContinueGame() 
    {
        fullBar.fillAmount = 0f;

        lifePoint = 5;
        PlayerPrefs.SetInt("MyLife", 5);
        lifePointAmount.text = lifePoint.ToString();
        StartCoroutine(ResumeGame());
    }

    IEnumerator ResumeGame()
    {
        yield return new WaitForSeconds(0.85f);

        if (retryPowerUp >= 1)
        {
            retryPowerUPText.GetComponent<TMP_Text>().color = Color.white; // Keep the color white;

            retryPowerUp--;

            PlayerPrefs.SetInt("MyRetryPowerUp", retryPowerUp);

            retryPowerUPText.text = retryPowerUp.ToString();

            leaderboardPanel.SetActive(false);
            GameOverScreen.SetActive(false);
            LevelUpScreen.SetActive(false);

        }

        else
        {
            if (retryPowerUp <= 0)
            {
                rewardAdScript.ShowAd();
                leaderboardPanel.SetActive(false);
                GameOverScreen.SetActive(false);
                LevelUpScreen.SetActive(false);
            }
        {
            // Disable the button:
            //_showAdButton.interactable = false;
            // Then show the ad:
            
        }
        }

    }
////////////////////////////////////////////////////////////////////////////
///Restart Game

    public void RestartGame() 
    { 
        StartCoroutine(ReinitializeGame());
    }

    IEnumerator ReinitializeGame()
    {

        yield return new WaitForSeconds(0.85f);
        score = 0;

        lifePoint = 5;
        PlayerPrefs.SetInt("MyLife", 5);
        lifePointAmount.text = lifePoint.ToString();

        PlayerPrefs.SetInt("MyScore", 0);

        PlayerPrefs.SetString("MyLevel", "Level 0");

        levelTxt.text = PlayerPrefs.GetString("MyLevel");

        fullBar.fillAmount = 0f;

        LevelUp();


        leaderboardPanel.SetActive(false);
        GameOverScreen.SetActive(false);
        LevelUpScreen.SetActive(false); 
    }

////////////////////////////////////////////////////////////////////////////

    public void OpenLeaderboard() 
    {
        StartCoroutine(UpdateLeaderboard());
        
    }

    public void CloseAllWindows()
    {
        leaderboardPanel.SetActive(false);
        GameOverScreen.SetActive(false);
        LevelUpScreen.SetActive(false); 
    }

     public void DeletePlayerPrefs() 
    {
        PlayerPrefs.DeleteAll();
    }

    public void ScoreAnim() 
    {
        if (btn1Selected == true)
        {
                vfxScore1.SetActive(true);

                initialPos1.transform.position += new Vector3 (Random.Range(-spread, spread), 0f, 0f);

                //animate Score to target position
                float duration = Random.Range(minAnimDuration, maxAnimDuration);
                vfxScore1.transform.DOMove (targetPosition.transform.position, duration)
                    .SetEase (easeType);
                vfxScore1.transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 1f)
                    .SetEase (Ease.Linear);
        }

          if (btn2Selected == true)
        {
                vfxScore2.SetActive(true);

                initialPos2.transform.position += new Vector3 (Random.Range(-spread, spread), 0f, 0f);

                //animate Score to target position
                float duration = Random.Range(minAnimDuration, maxAnimDuration);
                vfxScore2.transform.DOMove (targetPosition.transform.position, duration)
                    .SetEase (easeType);
                vfxScore2.transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 1f)
                    .SetEase (Ease.Linear);
        }

         if (btn3Selected == true)
        {
                vfxScore3.SetActive(true);

                initialPos3.transform.position += new Vector3 (Random.Range(-spread, spread), 0f, 0f);

                //animate Score to target position
                float duration = Random.Range(minAnimDuration, maxAnimDuration);
                vfxScore3.transform.DOMove (targetPosition.transform.position, duration)
                    .SetEase (easeType);
                vfxScore3.transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 1f)
                    .SetEase (Ease.Linear);  
        }

        

        
    }

    public void Btn1Selected()
    {
        btn1Selected = true;
        StartCoroutine(ResetBools());
    }
    public void Btn2Selected()
    {
        btn2Selected = true;
        StartCoroutine(ResetBools());
    }
    public void Btn3Selected()
    {
        btn3Selected = true;
        StartCoroutine(ResetBools());
    }

    IEnumerator ResetBools()
    {
        yield return new WaitForSeconds(0.01f);
        btn1Selected = false;
        btn2Selected = false;
        btn3Selected = false;
        yield return new WaitForSeconds(1.1f);
        vfxScore1.SetActive(false);
        vfxScore1.transform.position = initialPos1.transform.position;
        vfxScore1.transform.localScale = new Vector3 (1f, 1f, 1f);

        vfxScore2.SetActive(false);
        vfxScore2.transform.position = initialPos2.transform.position;
        vfxScore2.transform.localScale = new Vector3 (1f, 1f, 1f);

        vfxScore3.SetActive(false);
        vfxScore3.transform.position = initialPos3.transform.position;
        vfxScore3.transform.localScale = new Vector3 (1f, 1f, 1f);

    }


//////////////// Quiz Data

    public void QuizContentMixed ()
    {
        questions = quizDataMixed.questions;

        SelectQuestion();
    }

    public void QuizContentCulture ()
    {
        questions = quizDataCulture.questions;

        SelectQuestion();
    }

    public void QuizContentGeography ()
    {
        questions = quizDataGeography.questions;

        SelectQuestion();
    }

     public void QuizContentHistory ()
    {
        questions = quizDataHistory.questions;

        SelectQuestion();
    }

     public void QuizContentPolitics()
    {
        questions = quizDataPolitics.questions;

        SelectQuestion();
    }

    public void AnsRevealClose ()
    {
        AnsReveal.SetActive(false);
        
    }

}

    

[System.Serializable]
public class Question
{
    public string quesitonInfo;
    public QuestionType quesitonType;
    public Sprite questionImg;
    public AudioClip questionClip;
    public UnityEngine.Video.VideoClip quesitonVideo;
    public List<string> options;
    public string correctAns;
    public bool alreadyLearned;
}

[System.Serializable]
public enum QuestionType 
{
    TEXT,
    IMAGE,
    VIDEO,
    AUDIO
}