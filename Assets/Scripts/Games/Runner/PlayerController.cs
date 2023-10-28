using UnityEngine;
using GamesConverse;
using TMPro;

public class PlayerController : MonoBehaviour
{
	Rigidbody rb;
	Animator animator;
	public float jumpForce;
	public bool canJump;
	public AudioSource audioS;
	public AudioSource mainMusicAudioS;
	public AudioClip[] jumSsound;
	public AudioClip gameOver;
    public AudioClip obstacleImpact;

    public GameObject mainScreen;
	public GameObject gameOverScreen;

    public TextMeshProUGUI gameOverScore;

	public GameObject activateShield;

	private bool hasShield;


    private void Awake ()
	{
		rb = GetComponent<Rigidbody>();
		animator = gameObject.GetComponent<Animator>();
	}
	// Start is called before the first frame update
	void Start()
	{
		audioS = GetComponent<AudioSource>();
        gameOverScreen.SetActive(false);
        mainScreen.SetActive(true);
        activateShield.SetActive(false);



    }

    // Update is called once per frame
    void Update()
	{
		if (Input.GetMouseButtonDown(0) && canJump)
		{
			//jump
			Debug.Log("it should jump");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			animator.SetBool("CanJump", true);
            audioS.pitch = Random.Range(0.8f, 1.2f);
            audioS.PlayOneShot(jumSsound[Random.Range(0, jumSsound.Length)], 0.3f);

        }
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Ground")
		{
			
			canJump = true;
		   

		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.tag == "Ground")
		{
			canJump = false;
			animator.SetBool("CanJump", false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasShield)
		{
            if (other.gameObject.tag == "Obstacle")
            {
                audioS.PlayOneShot(obstacleImpact, 0.9f);

                hasShield = false;
				activateShield.SetActive(false);

                

                canJump = true;
               
                
				

            }
        } 
		
		else if (hasShield == false)
		{
            if (other.gameObject.tag == "Obstacle")
            {
                canJump = false;
                gameOverScreen.SetActive(true);
                
                //SceneManager.LoadScene("Game_Runner");


                mainMusicAudioS.Stop();
                audioS.PlayOneShot(gameOver, 0.9f);

                //pause
                Time.timeScale = 0;

                int c02saved = (GameManager_Runner.score * 6) - (GameManager_Runner.score * 1);

                Debug.Log("<color=green>This is how much Co2 you saved:</color> " + c02saved);

                gameOverScore.text = c02saved.ToString();

                // Add score
                Game game = ItemsAndGamesManager.Instance.Games.Find(g => g.name == "Runner");

                GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, game, null, 0, c02saved, 0);
            }
        }


		

	}

	public void CanJump()
	{
		canJump = true;
	}

	public void ActivateShield()
	{
		activateShield.SetActive(true);
	}

	public void HasShield()
	{
		hasShield = true;

    }

}
