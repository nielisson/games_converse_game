using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
 
public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] QuizManager quizManager;
    [SerializeField] Button _showAdButton;
    [SerializeField] Image imgButton;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms
 
    void Awake()
    {   
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        //Disable the button until the ad is ready to show:
        //_showAdButton.interactable = false;



        
    }

    void Start ()
    {
        imgButton = GetComponent<Image>();

    }

    void Update()
    {
        
       if (quizManager.retryPowerUp <= 0)
        {
            // Show/make interactive the button:
            _showAdButton.interactable = true;
            imgButton.GetComponent<Image>().color = new Color(1f,1f,1f,1f);
           
        }

        if (quizManager.retryPowerUp > 0)
        {
            imgButton.GetComponent<Image>().color = new Color(1f,1f,1f,0.5f);
           
        }
        
    }
 
    // Load content to the Ad Unit:   -> CALL THIS FUNCTION FROM THE BUTTON THAT OPENS THE AD
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load("Rewarded_Android", this);
    }
 
    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);
 
        if (adUnitId.Equals(_adUnitId))
        {
            // Configure the button to call the ShowAd() method when clicked:
            _showAdButton.onClick.AddListener(ShowAd);
            // Enable the button for users to click:
            //_showAdButton.interactable = true;
        }
    }
 
    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        if (quizManager.retryPowerUp <= 0)
        {
            // Disable the button:
            //_showAdButton.interactable = false;
            // Then show the ad:
            Advertisement.Show(_adUnitId, this);
        }
        
    }
 
    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            // Grant a reward: 3 new retries
            quizManager.retryPowerUp = PlayerPrefs.GetInt("MyRetryPowerUp");

            quizManager.retryPowerUp = 3;

            PlayerPrefs.SetInt("MyRetryPowerUp", quizManager.retryPowerUp);


            quizManager.retryPowerUPText.text = quizManager.retryPowerUp.ToString();

            // Load another ad:
            Advertisement.Load(_adUnitId, this);
        }
    }
 
    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }
 
    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }
 
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
 
    void OnDestroy()
    {
        // Clean up the button listeners:
        _showAdButton.onClick.RemoveAllListeners();
    }
}