using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class PlayerManagerQuiz : MonoBehaviour
{
    public TMP_InputField playerNameInputField;
    public LeaderboardQuiz leaderboard;


    // Start is called before the first frame update
    void Start()
    {
       StartCoroutine(SetupRoutine()); 
    }

    public void SetPlayerName()
    {
        LootLockerSDKManager.SetPlayerName(playerNameInputField.text, (response) => 
        {
            if (response.success)
            {
                Debug.Log("succesfully set player name");
            }
            else
            {
                Debug.Log("could not set player name" + response.Error);
            }
        });
    }

    IEnumerator SetupRoutine()
    {
        yield return LoginRoutine();
        yield return leaderboard.FetchTopHighscoresRoutine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) => 
        {
            if (response.success)
            {
                Debug.Log("Login success");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = true;
            }
            else
            {
                Debug.Log("Couldn't start session");
                done = true;
            }
        });

        yield return new WaitWhile(() => done == false);


    }
}