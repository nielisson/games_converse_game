#region Namespaces

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Utilities;
using GamesConverse.UI;
using GamesConverse.Notifications;

#endregion

namespace GamesConverse
{
	[DefaultExecutionOrder(100)]
	public class GameController : MonoBehaviour
	{
		#region Modules

		[Serializable]
		public class UserSession
		{
			#region Variables

			public string username;
			public string password;

			#endregion

			#region Methods

			#region Static Methods

			public static UserSession Load()
			{
				return new DataSerializationUtility<UserSession>(Path.Combine(Application.persistentDataPath, "UserSession.bytes"), false, true).Load();
			}
			public static bool Delete()
			{
				return new DataSerializationUtility<UserSession>(Path.Combine(Application.persistentDataPath, "UserSession.bytes"), false, true).Delete();
			}

			#endregion

			#region Global Methods

			public bool Save()
			{
				return new DataSerializationUtility<UserSession>(Path.Combine(Application.persistentDataPath, "UserSession.bytes"), false, true).SaveOrCreate(this);
			}
			public UnityWebRequest GetRequest()
			{
				WWWForm postData = new WWWForm();

				postData.AddField("username", username);
				postData.AddField("password", password);

				UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}login", postData);

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				return request;
			}

			#endregion

			#endregion

			#region Operators

			public static implicit operator bool(UserSession session) => session != null;

			#endregion
		}
		[Serializable]
		public class UserStats
		{
			#region Enumerators

			public enum ActionType { InAppPurchase = 1, SpendingOrGain }

			#endregion

			#region Variables
			
			public string username;
			public string nickname;
			public int country;
			public int favouriteGameGenre;
			public int coins;
			public int xp;
			public int level;
			public int impact;
			public int globalImpact;
			public int tickets;
			public int[] boughtItems;
			public int[] boughtGames;

			[SerializeField, HideInInspector]
			private readonly string identifier;

			#endregion

			#region Methods

			#region Static Methods

			public static UserStats Load()
			{
				return new DataSerializationUtility<UserStats>(Path.Combine(Application.persistentDataPath, "UserStats.bytes"), false, true).Load();
			}
			public static bool Delete()
			{
				return new DataSerializationUtility<UserStats>(Path.Combine(Application.persistentDataPath, "UserStats.bytes"), false, true).Delete();
			}
			public static int LevelFromXP(int xp)
			{
				int level;

				if (xp < 100)
					level = 0;
				else if (xp < 300)
					level = 1;
				else if (xp < 750)
					level = 2;
				else if (xp < 1400)
					level = 3;
				else
				{
					level = 4;

					while (xp >= XPFromLevel(level + 1))
						level++;
				}

				return level;
			}
			public static int XPFromLevel(int level)
			{
				if (level <= 0)
					return 0;

#pragma warning disable IDE0066 // Convert switch statement to expression
				switch (level)
				{
					case 1:
						return 100;

					case 2:
						return 300;

					case 3:
						return 750;

					case 4:
						return 1400;

					default:
						return 100 * level * level - 50 * level;
				}
#pragma warning restore IDE0066 // Convert switch statement to expression
			}

			#endregion

			#region Global Methods

			public bool Save()
			{
				return new DataSerializationUtility<UserStats>(Path.Combine(Application.persistentDataPath, "UserStats.bytes"), false, true).SaveOrCreate(this);
			}
			public UnityWebRequest GetRequest()
			{
				UnityWebRequest request = UnityWebRequest.Get($"{ItemsAndGamesManager.HostURL}statistics?identifier={identifier}");

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				return request;
			}
			public UnityWebRequest PostRequest(WWWForm fields)
			{
				if (fields == null)
					return GetRequest();

				UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}statistics?identifier={identifier}", fields);

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				return request;
			}
			public void NewAction(ActionType actionType, Game game = null, Item item = null, int coins = 0, int xp = 0, int tickets = 0)
			{
				if (coins == 0 && xp == 0 && tickets == 0 && game == null && item == null)
					return;

				this.coins += coins;
				this.xp += xp;
				impact += coins > 0 ? coins * 10 : 0;
				globalImpact += coins > 0 ? coins * 10 : 0;
				this.tickets += tickets;
				level = LevelFromXP(this.xp);

				Instance.StartCoroutine(StartNewAction(actionType, game, item, coins, xp, tickets));
			}

			internal IEnumerator StartNewAction(ActionType actionType, Game game, Item item, int coins, int xp, int tickets)
			{
				UIController.Instance.ShowDialog("Please Wait...", "Loading...");

				WWWForm fields = new WWWForm();

				fields.AddField("new_action", (int)actionType);

				if (game != null)
					fields.AddField("game_id", game.ID);

				if (item != null)
					fields.AddField("item_id", item.ID);

				fields.AddField("coins", coins);
				fields.AddField("xp", xp);
				fields.AddField("tickets", tickets);

				UnityWebRequest request = PostRequest(fields);

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { Instance.StartCoroutine(StartNewAction(actionType, game, item, coins, xp, tickets)); });
					Debug.LogError($"Action Register Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
					request.Dispose();

					yield break;
				}

				JSONObject json = new JSONObject(request.downloadHandler.text);

				request.Dispose();

				switch (json["response"].str)
				{
					case "200":
						JSONObject result = json["result"];

						username = result["username"].str;
						nickname = result["nickname"].str;
						country = Convert.ToInt32(result["country"].str);
						favouriteGameGenre = Convert.ToInt32(result["fav_game_genre"].str);
						this.coins = Convert.ToInt32(result["coins"].str);
						this.xp = Convert.ToInt32(result["xp"].str);
						level = LevelFromXP(this.xp);
						impact = Convert.ToInt32(result["impact"].str);
						globalImpact = Convert.ToInt32(result["global_impact"].str);
						this.tickets = Convert.ToInt32(result["tickets"].str);
						boughtItems = result["bought_items"].list.Select(item => Convert.ToInt32(item.str)).ToArray();
						boughtGames = result["bought_games"].list.Select(item => Convert.ToInt32(item.str)).ToArray();

						if (!Save())
							Debug.LogWarning("Retrieving user statistics succeeded but we could not save data locally!");

						UIController.Instance.RefreshUserStats();
						UIController.Instance.RefreshProgression();
						UIController.Instance.RefreshImpact();
						UIController.Instance.HideDialog();

						break;

					default:
						UIController.Instance.ShowDialog("Error", "We've had some internal errors!", "Okay");
						Debug.LogError($"Action Register Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

						break;
				}

				List<Quest> quests = QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && (game == null && quest.targetGameID < 1 || game != null && game.ID == quest.targetGameID) && (quest.target == Quest.Target.ReachLevel || quest.target == Quest.Target.CollectCoins || quest.target == Quest.Target.CollectXP || quest.target == Quest.Target.CollectTickets));

				foreach (Quest quest in quests)
					switch (quest.target)
					{
						case Quest.Target.ReachLevel:
							quest.SetPlayerTargetValue(level);

							break;

						case Quest.Target.CollectCoins:
							quest.SetPlayerTargetValue(quest.ReachedTargetValue + coins);

							break;

						case Quest.Target.CollectXP:
							quest.SetPlayerTargetValue(quest.ReachedTargetValue + xp);

							break;

						case Quest.Target.CollectTickets:
							quest.SetPlayerTargetValue(quest.ReachedTargetValue + tickets);

							break;
					}
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public UserStats(UserSession session)
			{
				identifier = session.username;
			}

			#endregion

			#region Operators

			public static implicit operator bool(UserStats stats) => stats != null;

			#endregion

			#endregion
		}
		[Serializable]
		public class UserAvatar
		{
			#region Variables

			public int[] sprites;

			[SerializeField, HideInInspector]
			private readonly string username;

			#endregion

			#region Methods

			#region Static Methods

			public static UserAvatar Load()
			{
				return new DataSerializationUtility<UserAvatar>(Path.Combine(Application.persistentDataPath, "UserAvatar.bytes"), false, true).Load();
			}
			public static bool Delete()
			{
				return new DataSerializationUtility<UserAvatar>(Path.Combine(Application.persistentDataPath, "UserAvatar.bytes"), false, true).Delete();
			}

			#endregion

			#region Global Methods

			public bool Save()
			{
				return new DataSerializationUtility<UserAvatar>(Path.Combine(Application.persistentDataPath, "UserAvatar.bytes"), false, true).SaveOrCreate(this);
			}
			public void SaveOnline()
			{
				Instance.StartCoroutine(StartSaveOnline());
			}
			public UnityWebRequest GetRequest()
			{
				UnityWebRequest request = UnityWebRequest.Get($"{ItemsAndGamesManager.HostURL}avatar?username={username}");

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				return request;
			}
			public UnityWebRequest PostRequest(WWWForm fields)
			{
				UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}avatar?username={username}", fields);

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				return request;
			}

			internal IEnumerator StartSaveOnline()
			{
				WWWForm fields = new WWWForm();

				fields.AddField("sprites", string.Join(";", sprites));

				UnityWebRequest request = PostRequest(fields);

				yield return request.SendWebRequest();
				
				if (request.result != UnityWebRequest.Result.Success)
				{
					UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { Instance.StartCoroutine(StartSaveOnline()); });
					Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
					request.Dispose();

					yield break;
				}

				JSONObject json = new JSONObject(request.downloadHandler.text);

				request.Dispose();

				switch (json["response"].str)
				{
					case "200":
						JSONObject result = json["result"];

						Instance.userAvatar = new UserAvatar(Instance.userSession)
						{
							sprites = result["sprites"].str.Split(';').Select(id => Convert.ToInt32(id)).ToArray()
						};

						if (!Instance.userAvatar.Save())
							Debug.LogWarning("Retrieving user avatar succeeded but we could not save data locally!");

						break;

					default:
						UIController.Instance.ShowDialog("Error", "We've had some internal errors!", "Okay");
						Debug.LogError($"Stats Retrieving Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

						break;
				}
			}

			#endregion

			#endregion

			#region Constructors & Operators

			#region Constructors

			public UserAvatar(UserSession session)
			{
				sprites = UIController.Instance.avatarCustomization.defaultSprites.ToArray();
				username = session.username;
			}

			#endregion

			#region Operators

			public static implicit operator bool(UserAvatar stats) => stats != null;

			#endregion

			#endregion
		}

		#endregion

		#region Variables

		#region Static Variables

		public static GameController Instance
		{
			get
			{
				if (!instance)
					instance = FindObjectOfType<GameController>();

				return instance;
			}
		}

		private static GameController instance;

		#endregion

		#region Global Variables

		[Header("General")]
		public string mainSceneName;
		public string weekTaskTitle;
		public string weekTaskDate;
		public int weekGlobalImpactTarget;
		public double activeUsersPeriodInDays;
		[Header("Fortune Wheel")]
		public float minFortuneWheelSpinTime;
		public float maxFortuneWheelSpinTime;
		[Header("Sounds")]
		public AudioClip ambianceSound;
		public AudioClip[] buyShopSounds;
		public AudioClip clickSound;
		public AudioClip dialogPopupSound;
		public AudioClip giftShakingSound;
		public AudioClip giftOpenedSound;
		public AudioClip giftClaimSound;
		public AudioClip questFinishedSound;
		public AudioClip wheelSpinStartSound;
		public AudioClip wheelSpinFailSound;
		public AudioClip[] wheelSpinStepSounds;
		[Header("Links")]
		public string websiteLink = "https://gamesconverse.space/";
		public string websiteReportLink = "https://gamesconverse.space/";
		public string googlePlayStoreLink = "https://play.google.com/";
		public string appleAppStoreLink = "https://apps.apple.com/app/";
		public string facebookPageLink = "https://www.facebook.com/gamesconverse";
		public string instagramPageLink = "https://www.instagram.com/gamesconverse/";
		public string twitterPageLink = "https://twitter.com/games_converse";
		[TextArea]
		public string facebookShareLink = "https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fgamesconverse.space%2F";
		[TextArea]
		public string twitterShareLink = "https://twitter.com/intent/tweet?url=https%3A%2F%2Fgamesconverse.space%2F&text=Games+Converse+is+a+mobile+games+platform+that+raises+awareness+about+environmental+and+animal+rights+issues%21";
		public GameSettings Settings => settings;
		public UserSession UserSessionDetails => userSession;
		public UserStats UserStatsDetails => userStats;
		public UserAvatar UserAvatarDetails => userAvatar;
		public string[] Countries => countries;

		private GameSettings settings;
		private UserSession userSession;
		private UserStats userStats;
		private UserAvatar userAvatar;
		private AudioSource uiAudioSource;
		private AudioSource ambientAudioSource;
		private string[] countries;

		#endregion

		#endregion

		#region Methods

		#region Awake

		public void RestartGame()
		{
			Awake();
		}

		private void Awake()
		{
			UIController.Instance.HideDialog();
			UIController.Instance.ShowCanvas(UIController.UICanvasType.Splash);
			StartCoroutine(StartGame());
		}

		private IEnumerator StartGame()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { RestartGame(); }, "Quit Game", () => { Application.Quit(); }, "Restart"))
				yield break;

			settings = GameSettings.Load();
			userSession = UserSession.Load();
			userStats = UserStats.Load();
			userAvatar = UserAvatar.Load();

			bool loginSucceeded = false;

			UnityWebRequest request = UnityWebRequest.Get($"{ItemsAndGamesManager.HostURL}/countries");

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Retreiving Countries Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				goto server_error;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);
			countries = new string[json.Count];

			for (int i = 0; i < countries.Length; i++)
				countries[i] = json.list[i]["name"].str;

			UIController.Instance.register.country.ClearOptions();
			UIController.Instance.account.country.ClearOptions();
			UIController.Instance.register.country.options.Add(new Dropdown.OptionData("Select Country"));
			UIController.Instance.account.country.options.Add(new Dropdown.OptionData("Select Country"));

			foreach (string country in countries)
			{
				UIController.Instance.register.country.options.Add(new Dropdown.OptionData(country));
				UIController.Instance.account.country.options.Add(new Dropdown.OptionData(country));
			}

			UIController.Instance.register.favouriteGameGenre.ClearOptions();
			UIController.Instance.account.favouriteGameGenre.ClearOptions();
			UIController.Instance.register.favouriteGameGenre.options.Add(new Dropdown.OptionData("Select Genre"));
			UIController.Instance.account.favouriteGameGenre.options.Add(new Dropdown.OptionData("Select Genre"));

			foreach (string genre in ItemsAndGamesManager.Instance.GameGenres)
			{
				UIController.Instance.register.favouriteGameGenre.options.Add(new Dropdown.OptionData(genre));
				UIController.Instance.account.favouriteGameGenre.options.Add(new Dropdown.OptionData(genre));
			}

			if (userSession)
			{
				request = userSession.GetRequest();

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
					request.Dispose();

					goto server_error;
				}

				json = new JSONObject(request.downloadHandler.text);

				request.Dispose();

				loginSucceeded = json["response"].str == "200";

				if (loginSucceeded)
				{
					yield return StartStatsRetrieving();
					yield return StartAvatarRetrieving();

					//NotificationsManager.ScheduleNotification("Welcome Back!", $"Welcome back {userStats.nickname}!", "app_icon_small", "app_icon_large", DateTime.Now);
				}
				else if (userSession && !UserSession.Delete() || userStats && !UserStats.Delete())
					Debug.LogWarning("Cannot delete current User Session");
			}

			if (!settings)
			{
				settings = new GameSettings();

				yield return settings.StartSave();

				UIController.Instance.ShowCanvas(UIController.UICanvasType.Language);
			}
			else
			{
				UIController.Instance.RefreshMain();
				UIController.Instance.RefreshSettings();

				if (loginSucceeded)
					UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);
				else
					UIController.Instance.ShowCanvas(UIController.UICanvasType.Login);
			}

			DontDestroyOnSceneLoad dontDestroyOnSceneLoad = GetComponent<DontDestroyOnSceneLoad>();

			if (!uiAudioSource)
			{
				uiAudioSource = Utilities.Utility.NewAudioSource("game_ui_source", 0f, 0f, Settings.volume, null, false, false, false);

				if (dontDestroyOnSceneLoad)
					dontDestroyOnSceneLoad.linkedGameObjects.Add(uiAudioSource.gameObject);
			}

			if (!ambientAudioSource)
			{
				ambientAudioSource = Utilities.Utility.NewAudioSource("game_ambient_source", 0f, 0f, Settings.volume, ambianceSound, true, Settings.hubSoundsOn && Settings.volume > 0f, false);

				if (dontDestroyOnSceneLoad)
					dontDestroyOnSceneLoad.linkedGameObjects.Add(ambientAudioSource.gameObject);
			}

			if (dontDestroyOnSceneLoad)
				dontDestroyOnSceneLoad.RefreshLinkedObjects();

			if (loginSucceeded && settings)
				CheckForDailyReward();

			yield break;

		server_error:
			UIController.Instance.ShowDialog("Error", "We've had some errors connecting to our servers...", "Restart", "Quit Game", () => { Awake(); }, () => { Application.Quit(); });
		}

		#endregion

		#region Utilities

		#region Static Methods

		public static void LoadScene(string sceneName)
		{
			Instance.StartCoroutine(StartLoadScene(sceneName));
		}

		internal static IEnumerator StartLoadScene(string sceneName)
		{
			UIController.Instance.ShowCanvas(UIController.UICanvasType.GameLoading);

			AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

			while (!operation.isDone)
			{
				UIController.Instance.gameLoading.progressBar.fillAmount = Mathf.Clamp01(operation.progress / .9f);

				yield return null;
			}

			UIController.Instance.HideAllCanvases();

			if (Instance.mainSceneName == sceneName)
			{
				UIController.Instance.RefreshQuests();
				UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);
				Instance.ambientAudioSource.UnPause();

				if (!Instance.ambientAudioSource.isPlaying)
					Instance.ambientAudioSource.Play();

				IEnumerable<Quest> quests = QuestsManager.Instance.Quests.Where(quest => !quest.IsClaimed());

				if (quests.Any())
					NotificationsManager.ScheduleNotification("Quest Completed!", "You've successfully completed a quest. Feel free to claim it!", "app_icon_small", "app_icon_large", DateTime.Now);
			}
			else
				Instance.ambientAudioSource.Pause();
		}

		#endregion

		#region Global Methods

		public void PlayUIClickSound()
		{
			PlayHubClip(clickSound);
		}
		public bool IsLoggedIn()
		{
			return userSession && !string.IsNullOrEmpty(userSession.username);
		}
		public void PlayHubClip(AudioClip clip, bool force = false)
		{
			if (!Settings || !uiAudioSource || !Settings.hubSoundsOn || Settings.volume <= 0f || uiAudioSource.isPlaying && !force)
				return;

			uiAudioSource.volume = Settings.volume;

			if (force)
			{
				if (uiAudioSource.isPlaying)
					uiAudioSource.Stop();

				uiAudioSource.clip = clip;

				uiAudioSource.Play();
			}
			else
				uiAudioSource.PlayOneShot(clip);
		}
		public bool IsHubClipPlaying()
		{
			return uiAudioSource.isPlaying;
		}

		private void CheckForDailyReward()
		{
			string dailyRewardKey = $"FORTUNE_WHEEL_REWARD_{userSession.username.ToUpper()}_{DateTime.UtcNow:yyyy-MM-dd}";

			if (PlayerPrefs.GetInt(dailyRewardKey, 0) == 0)
			{
				UserStatsDetails.NewAction(UserStats.ActionType.SpendingOrGain, null, null, 0, 0, 1);
				NotificationsManager.ScheduleNotification("Congrats!", "You've got 1 ticket as a daily login reward!", "app_icon_small", "app_icon_large", DateTime.Now);
				UIController.Instance.ShowCanvas(UIController.UICanvasType.FortuneWheel);
				PlayerPrefs.SetInt(dailyRewardKey, 1);
			}
		}

		#endregion

		#endregion

		#region Coroutines

		internal IEnumerator StartLogin()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartLogin()); }))
				yield break;

			UserSession userSession = new UserSession()
			{
				username = UIController.Instance.login.username.text,
				password = UIController.Instance.login.password.text
			};
			UnityWebRequest request = userSession.GetRequest();

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartLogin()); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					userSession.username = json["email"].str;

					if (!userSession.Save())
						Debug.LogWarning("Login succeeded but we could not save session data locally!");

					this.userSession = userSession;

					yield return StartStatsRetrieving();
					yield return StartAvatarRetrieving();

					UIController.Instance.HideDialog();
					UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);
					NotificationsManager.ScheduleNotification("Welcome To Games Converse!", $"Hi {userStats.nickname}!\r\nHope you enjoy the journey :D", "app_icon_small", "app_icon_large", DateTime.Now);

					if (userSession && settings)
						CheckForDailyReward();

					break;

				case "402":
					UIController.Instance.ShowDialog("Account Activation", "You are required to activate your account. Please check your email inbox!", "Okay");
					UIController.Instance.ShowCanvas(UIController.UICanvasType.AccountActivation);

					UIController.Instance.accountActivation.passwordResetEmail = default;
					this.userSession = userSession;

					break;

				case "403":
				case "404":
					UIController.Instance.ShowDialog("Login Failed", "Please verify your username and password, then try again!", "Okay");

					break;

				case "501":
					UIController.Instance.ShowDialog("Error", "We're having some trouble sending you the reactivation mail...", "Retry", "Cancel", () => { StartCoroutine(StartLogin()); });

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Register Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartRegister()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartRegister()); }))
				yield break;

			string password = UIController.Instance.register.password.text;
			string confirmPassword = UIController.Instance.register.confirmPassword.text;

			if (password != confirmPassword)
			{
				UIController.Instance.ShowDialog("Register Failed", "Make sure that the Password and Confirm Password fields match.", "Sure");

				yield break;
			}

			string username = UIController.Instance.register.username.text.Trim();
			string email = UIController.Instance.register.email.text.Trim();
			string nickname = UIController.Instance.register.nickname.text.Trim();
			int country = UIController.Instance.register.country.value;
			int genre = UIController.Instance.register.favouriteGameGenre.value;

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nickname) || string.IsNullOrEmpty(password))
			{
				UIController.Instance.ShowDialog("Register Failed", "Please fill-in all the required fields!", "Okay");

				yield break;
			}

			WWWForm postData = new WWWForm();

			postData.AddField("username", username);
			postData.AddField("password", password);
			postData.AddField("email", email);
			postData.AddField("nickname", nickname);
			postData.AddField("country", country);
			postData.AddField("genre", genre);

			UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}register", postData);

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartRegister()); });
				Debug.LogError($"Register Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					UIController.Instance.ShowDialog("Success", "Your account has been created successfully! Please activate it using the code sent to your email address. Just in case, do not forget to check your spam folder!", "Got it!");
					UIController.Instance.ShowCanvas(UIController.UICanvasType.AccountActivation);

					UIController.Instance.accountActivation.passwordResetEmail = default;
					userSession = new UserSession()
					{
						username = username,
						password = password
					};

					break;

				case "401":
				case "403":
					UIController.Instance.ShowDialog("Register Failed", json["message"].str, "Okay");

					break;

				case "501":
					UIController.Instance.ShowDialog("Error", "We're having some trouble sending you the confirmation mail...", "Retry", "Cancel", () => { StartCoroutine(StartRegister()); });

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Register Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartPasswordReset1()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartPasswordReset1()); }))
				yield break;

			string email = UIController.Instance.passwordReset1.email.text;

			WWWForm postData = new WWWForm();

			postData.AddField("email", email);

			UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}reset_password", postData);

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartPasswordReset1()); });
				Debug.LogError($"Password Reset Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					UIController.Instance.ShowDialog("Success", "We have received your request! You'll receive a mail in case your address exists in our databases. Just in case, do not forget to check your spam folder!", "Got it!");
					UIController.Instance.ShowCanvas(UIController.UICanvasType.AccountActivation);

					UIController.Instance.accountActivation.passwordResetEmail = email;
					userSession = null;

					break;

				case "401":
				case "403":
					UIController.Instance.ShowDialog("Reset Failed", json["message"].str, "Okay");

					break;

				case "501":
					UIController.Instance.ShowDialog("Error", "We're having some trouble sending you the reactivation mail...", "Retry", "Cancel", () => { StartCoroutine(StartPasswordReset1()); });

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Password Reset Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartPasswordReset2()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartPasswordReset2()); }))
				yield break;

			string code = UIController.Instance.accountActivation.code;
			string email = UIController.Instance.accountActivation.passwordResetEmail;
			string newPassword = UIController.Instance.passwordReset2.newPassword.text;

			WWWForm postData = new WWWForm();

			postData.AddField("code", code);
			postData.AddField("email", email);
			postData.AddField("new_password", newPassword);

			UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}reset_password", postData);

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartPasswordReset2()); });
				Debug.LogError($"Password Reset Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					UIController.Instance.ShowDialog("Success", "Your password has been changed successfully!\r\nPlease login using the new submitted password.", "Got it!");
					UIController.Instance.ShowCanvas(UIController.UICanvasType.Login);

					UIController.Instance.accountActivation.code = default;
					UIController.Instance.accountActivation.passwordResetEmail = default;
					userSession = null;

					break;

				case "401":
				case "403":
					UIController.Instance.ShowDialog("Reset Failed", json["message"].str, "Okay");

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Password Reset Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartAccountActivation()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartPasswordReset1()); }))
				yield break;

			bool passwordResetActivation = !userSession && !string.IsNullOrWhiteSpace(UIController.Instance.accountActivation.passwordResetEmail);

			WWWForm postData = new WWWForm();

			if (passwordResetActivation)
				postData.AddField("email", UIController.Instance.accountActivation.passwordResetEmail);
			else
			{
				postData.AddField("username", userSession.username);
				postData.AddField("password", userSession.password);
			}

			string code = $"{UIController.Instance.accountActivation.codeField1.text}{UIController.Instance.accountActivation.codeField2.text}{UIController.Instance.accountActivation.codeField3.text}{UIController.Instance.accountActivation.codeField4.text}";

			postData.AddField("code", code);

			UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}{(passwordResetActivation ? "reset_password" : "login")}", postData);

			request.disposeCertificateHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartAccountActivation()); });
				Debug.LogError($"Password Reset Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					if (passwordResetActivation)
					{
						UIController.Instance.accountActivation.code = code;

						UIController.Instance.HideDialog();
						UIController.Instance.ShowCanvas(UIController.UICanvasType.PasswordReset2);
					}
					else
					{
						yield return StartStatsRetrieving();
						yield return StartAvatarRetrieving();

						UIController.Instance.HideDialog();
						userSession.Save();

						userStats = new UserStats(userSession);

						userStats.Save();
						UIController.Instance.ShowDialog("Account Activation", "You've successfully activated your account!", "Okay", () =>
						{
							UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);
							NotificationsManager.ScheduleNotification("Welcome To Games Converse!", $"Hi {userStats.nickname}!\r\nHope you enjoy the journey :D", "app_icon_small", "app_icon_large", DateTime.Now);
						});
					}

					break;

				case "402":
				case "403":
				case "404":
					UIController.Instance.ShowDialog("Confirmation Failed", "Please enter a valid confirmation code", "Okay");

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Account Activation Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartLogout()
		{
			UIController.Instance.ShowCanvas(UIController.UICanvasType.Splash);
			UIController.Instance.ShowDialog("Please Wait...", "Logging out...");

			yield return null;

			userSession = null;
			userStats = null;
			userAvatar = null;

			if (!UserSession.Delete() || !UserStats.Delete() || !UserAvatar.Delete())
				UIController.Instance.ShowDialog("Error", "We've had some internal issues while logging out of your session.", "Retry", "Cancel", () => { StartCoroutine(StartLogout()); }, () => { UIController.Instance.ShowCanvas(UIController.UICanvasType.Main); });
			else
			{
				UIController.Instance.HideDialog();
				UIController.Instance.ShowCanvas(UIController.UICanvasType.Login);
			}
		}
		internal IEnumerator StartResendActivationCode()
		{
			if (!UIController.Instance.TryShowInternetConnectionCheckDialog(() => { StartCoroutine(StartResendActivationCode()); }))
				yield break;

			if (userSession)
			{
				UnityWebRequest request = userSession.GetRequest();

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartResendActivationCode()); });
					Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
					request.Dispose();

					yield break;
				}

				JSONObject json = new JSONObject(request.downloadHandler.text);

				request.Dispose();

				switch (json["response"].str)
				{
					case "200":
						if (!userSession.Save())
							Debug.LogWarning("Login succeeded but we could not save session data locally!");

						yield return StartStatsRetrieving();
						yield return StartAvatarRetrieving();

						UIController.Instance.ShowDialog("Info", "Your account has already been activated!", "Yay!");
						UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);

						break;

					case "402":
						UIController.Instance.ShowDialog("Success", "We've resent your activation code, please check your email inbox. Just in case, do not forget to check your spam folder!", "Okay");

						break;

					case "501":
						UIController.Instance.ShowDialog("Error", "We're having some trouble sending you the reactivation mail...", "Retry", "Cancel", () => { StartCoroutine(StartResendActivationCode()); });

						break;

					default:
						UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
						Debug.LogError($"Register Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

						break;
				}
			}
			else if (!string.IsNullOrWhiteSpace(UIController.Instance.accountActivation.passwordResetEmail))
			{
				WWWForm postData = new WWWForm();

				postData.AddField("email", UIController.Instance.accountActivation.passwordResetEmail);

				UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}reset_password", postData);

				request.disposeCertificateHandlerOnDispose = true;
				request.disposeDownloadHandlerOnDispose = true;
				request.disposeUploadHandlerOnDispose = true;

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartResendActivationCode()); });
					Debug.LogError($"Password Reset Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
					request.Dispose();

					yield break;
				}

				JSONObject json = new JSONObject(request.downloadHandler.text);

				request.Dispose();

				switch (json["response"].str)
				{
					case "200":
						UIController.Instance.ShowDialog("Success", "We've sent you an activation code, please check your email inbox. Just in case, do not forget to check your spam folder!", "Got it!");
						UIController.Instance.ShowCanvas(UIController.UICanvasType.AccountActivation);

						break;

					case "401":
					case "403":
						UIController.Instance.ShowDialog("Reset Failed", json["message"].str, "Okay");

						break;

					case "501":
						UIController.Instance.ShowDialog("Error", "We're having some trouble sending you the reactivation mail...", "Retry", "Cancel", () => { StartCoroutine(StartResendActivationCode()); });

						break;

					default:
						UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
						Debug.LogError($"Password Reset Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

						break;
				}
			}
			else
				UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
		}
		internal IEnumerator StartUpdateAccount()
		{
			if (ItemsAndGamesManager.OfflineMode)
			{
				Debug.LogWarning("Could not retrieve player stats from our servers...");

				yield break;
			}
			else if (!userSession || !userStats)
			{
				Debug.LogError("No user session is present at the moment, please login first!");

				yield break;
			}

			WWWForm fields = new WWWForm();
			string username = UIController.Instance.account.username.text.Trim();
			string nickname = UIController.Instance.account.nickname.text.Trim();
			int country = UIController.Instance.account.country.value;
			int favouriteGameGenre = UIController.Instance.account.favouriteGameGenre.value;
			string oldPassword = UIController.Instance.account.oldPassword.text;
			string newPassword = UIController.Instance.account.newPassword.text;
			string confirmPassword = UIController.Instance.account.confirmPassword.text;
			bool isPasswordRequest = !string.IsNullOrEmpty(oldPassword);

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(nickname) || country < 1 || favouriteGameGenre < 1 || isPasswordRequest && (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword)))
			{
				UIController.Instance.ShowDialog("Update Failed", "Please fill-in all the required fields!", "Okay");

				yield break;
			}
			else if (isPasswordRequest && oldPassword == newPassword)
			{
				UIController.Instance.ShowDialog("Update Failed", "Old & New passwords cannot match.", "Got it!");

				yield break;
			}
			else if (isPasswordRequest && newPassword != confirmPassword)
			{
				UIController.Instance.ShowDialog("Update Failed", "New & Confirm passwords have to match.", "Got it!");

				yield break;
			}

			fields.AddField("identifier", userSession.username);
			fields.AddField("request", isPasswordRequest ? "PASSWORD" : "PROFILE");
			fields.AddField("username", username);
			fields.AddField("nickname", nickname);
			fields.AddField("country", country);
			fields.AddField("fav_game_genre", favouriteGameGenre);
			fields.AddField("old_password", oldPassword);
			fields.AddField("new_password", newPassword);

			UnityWebRequest request = UnityWebRequest.Post($"{ItemsAndGamesManager.HostURL}account", fields);

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartLogin()); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					userSession.password = UIController.Instance.account.newPassword.text;

					if (!userSession.Save())
						Debug.LogWarning("Password has been changed but could not be saved locally!");

					UIController.Instance.account.oldPassword.text = string.Empty;
					UIController.Instance.account.newPassword.text = string.Empty;
					UIController.Instance.account.confirmPassword.text = string.Empty;

					UIController.Instance.RefreshInputUpdateAccount();
					UIController.Instance.ShowDialog("Success", "You've successfully updated your profile settings!", "Okay");

					break;

				case "403":
					UIController.Instance.ShowDialog("Update Failed", json["message"].str, "Okay");

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors!", "Okay");
					Debug.LogError($"Stats Retrieving Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartStatsRetrieving()
		{
			if (ItemsAndGamesManager.OfflineMode)
			{
				Debug.LogWarning("Could not retrieve player stats from our servers...");

				yield break;
			}
			else if (!userSession)
			{
				Debug.LogError("No user session is present at the moment, please login first!");

				yield break;
			}

			if (!userStats)
				userStats = new UserStats(userSession);

			UnityWebRequest request = userStats.GetRequest();

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartLogin()); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					JSONObject result = json["result"];

					userStats = new UserStats(userSession)
					{
						username = result["username"].str,
						nickname = result["nickname"].str,
						country = Convert.ToInt32(result["country"].str),
						favouriteGameGenre = Convert.ToInt32(result["fav_game_genre"].str),
						coins = Convert.ToInt32(result["coins"].str),
						xp = Convert.ToInt32(result["xp"].str),
						level = UserStats.LevelFromXP(Convert.ToInt32(result["xp"].str)),
						impact = Convert.ToInt32(result["impact"].str),
						globalImpact = Convert.ToInt32(result["global_impact"].str),
						tickets = Convert.ToInt32(result["tickets"].str),
						boughtItems = result["bought_items"].list.Select(item => Convert.ToInt32(item.str)).ToArray(),
						boughtGames = result["bought_games"].list.Select(item => Convert.ToInt32(item.str)).ToArray()
					};

					if (!userStats.Save())
						Debug.LogWarning("Retrieving user statistics succeeded but we could not save data locally!");

					UIController.Instance.RefreshUserStats();
					UIController.Instance.RefreshProgression();
					UIController.Instance.RefreshMain();
					UIController.Instance.RefreshImpact();
					UIController.Instance.RefreshQuests();
					UIController.Instance.RefreshUpdateAccount();
					UIController.Instance.RefreshInputUpdateAccount();
					UIController.Instance.itemsShop.RefreshItemsList();

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors!", "Okay");
					Debug.LogError($"Stats Retrieving Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}
		internal IEnumerator StartAvatarRetrieving()
		{
			if (ItemsAndGamesManager.OfflineMode)
			{
				Debug.LogWarning("Could not retrieve player stats from our servers...");

				yield break;
			}
			else if (!userSession)
			{
				Debug.LogError("No user session is present at the moment, please login first!");

				yield break;
			}

			if (!userAvatar)
				userAvatar = new UserAvatar(userSession);

			UnityWebRequest request = userAvatar.GetRequest();

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { StartCoroutine(StartLogin()); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new JSONObject(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					JSONObject result = json["result"];

					if (result["sprites"].str != null)
						userAvatar.sprites = result["sprites"].str?.Split(';').Select(id => Convert.ToInt32(id)).ToArray();

					UIController.Instance.avatarCustomization.RefreshItemsList();
					UIController.Instance.avatarCustomization.RefreshAvatar();

					if (!userAvatar.Save())
						Debug.LogWarning("Retrieving user avatar succeeded but we could not save data locally!");

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors!", "Okay");
					Debug.LogError($"Avatar Retrieving Failed: {json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}{(json["error"] ? $"Error: {json["error"]}" : "")}");

					break;
			}
		}

		#endregion

		#region Update

#pragma warning disable IDE0051 // Remove unused private members
		private void Update()
#pragma warning restore IDE0051 // Remove unused private members
		{
			if (SceneManager.GetActiveScene().name != mainSceneName || !Settings || !ambientAudioSource)
				return;

			if (ambientAudioSource.volume != Settings.volume)
				ambientAudioSource.volume = Settings.volume;

			if (Instance.ambientAudioSource.isPlaying == Settings.hubSoundsOn && Instance.ambientAudioSource.isPlaying == (Settings.volume > 0f))
				return;

			if (Settings.hubSoundsOn && Settings.volume > 0f)
			{
				Instance.ambientAudioSource.UnPause();

				if (!Instance.ambientAudioSource.isPlaying)
					Instance.ambientAudioSource.Play();
			}
			else
				Instance.ambientAudioSource.Pause();
		}

		#endregion

		#endregion
	}
}
