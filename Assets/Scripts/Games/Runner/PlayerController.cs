using UnityEngine;
using GamesConverse;

public class PlayerController : MonoBehaviour
{
	Rigidbody rb;
	Animator animator;
	public float jumpForce;
	bool canJump;

	private void Awake ()
	{
		rb = GetComponent<Rigidbody>();
		animator = gameObject.GetComponent<Animator>();
	}
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(0) && canJump)
		{
			//jump
			rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			animator.SetBool("CanJump", true);
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
		if (other.gameObject.tag == "Obstacle")
		{
			//SceneManager.LoadScene("Game_Runner");

			//pause
			Time.timeScale = 0;
			int c02saved = (GameManager_Runner.score * 6) - (GameManager_Runner.score * 1);
			Debug.Log("<color=red>This is how much Co2 you saved:</color> " + c02saved);

			// Add score
			Game game = ItemsAndGamesManager.Instance.Games.Find(g => g.name == "Runner");

			GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.SpendingOrGain, game, null, 0, c02saved, 0);
		}

	}

}
