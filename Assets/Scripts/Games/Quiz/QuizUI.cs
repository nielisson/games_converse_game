using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizUI : MonoBehaviour
{
    [SerializeField] private SoundManager_Quiz soundManager;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Image quesitonImage;
    [SerializeField] private UnityEngine.Video.VideoPlayer questionVideos;
    [SerializeField] private AudioSource questionAudio;
    [SerializeField] private List<Button> options;
    [SerializeField] private Color correctCol, wrongCol, normalCol;

    private Question question;
    private bool answered;
    private float audioLenght;

    public GameObject imagePlaceholderTemp;

    public bool stopQuestionSound;

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < options.Count; i++)
        {
            Button localBtn = options[i]; 
            localBtn.onClick.AddListener(() => Onclick(localBtn));
        }
    }

    void Update()
    {
        if (stopQuestionSound == true)
        {
        StopCoroutine(PlayAudio());
        stopQuestionSound = false;
        }
    }

    public void SetQuestion(Question question)
    {
        this.question = question;

        switch (question.quesitonType)
        {
            case QuestionType.TEXT:
                ImageHolder(); 
                imagePlaceholderTemp.SetActive(true);
                quesitonImage.transform.parent.gameObject.SetActive(true);


                break;
            case QuestionType.IMAGE:
                ImageHolder(); 

                imagePlaceholderTemp.SetActive(true); 
                quesitonImage.transform.gameObject.SetActive(true); 

                quesitonImage.sprite = question.questionImg;
                
                
                break;
            case QuestionType.VIDEO:
                ImageHolder();

                imagePlaceholderTemp.SetActive(false);
                questionVideos.transform.gameObject.SetActive(true);

                questionVideos.clip = question.quesitonVideo;
                questionVideos.Play();
                break;
            case QuestionType.AUDIO:
                ImageHolder(); 

                imagePlaceholderTemp.SetActive(true);
                questionAudio.transform.gameObject.SetActive(true);

                audioLenght = question.questionClip.length;

                StartCoroutine(PlayAudio());
                
                break;
        }

        questionText.text = question.quesitonInfo;

        List<string> answerList = ShuffleList.ShuffleListItems<string>(question.options);

        for (int i=0; i < options.Count; i++)
        {
            options[i].GetComponentInChildren<TMP_Text>().text = answerList[i];
            options[i].name = answerList[i];
            options[i].image.color = normalCol;


        }

        answered = false;


    }

    IEnumerator PlayAudio() 
    {
            if (question.quesitonType == QuestionType.AUDIO)
            {
                questionAudio.PlayOneShot(question.questionClip, 0.6f);

                yield return new WaitForSeconds(audioLenght + 0.5f);

                StartCoroutine(PlayAudio());
            }
            else
            {
                StopCoroutine(PlayAudio());
                yield return null;
            }
    }



    void ImageHolder ()
    {
        quesitonImage.transform.parent.gameObject.SetActive(true);
        quesitonImage.transform.gameObject.SetActive(false);
        questionAudio.transform.gameObject.SetActive(false);
        questionVideos.transform.gameObject.SetActive(false);

    }

    private void Onclick(Button btn)
    {
        if(!answered)
        {
            answered = true;
            bool val = quizManager.Answer(btn.name);
            

            if(val)
            {
                btn.image.color = correctCol;
                soundManager.rightAnsSound();
            }
            else
            {
                btn.image.color = wrongCol;
                soundManager.wrongAnsSound();
            }
        }


    }

  
}
