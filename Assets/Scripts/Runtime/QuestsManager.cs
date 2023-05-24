#region Namespaces

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Utilities;
using GamesConverse.UI;

#endregion

namespace GamesConverse
{
	[Serializable]
	public class Quest
	{
		#region Enumerators

		public enum Period { General, Daily, Weekly, Monthly, Yearly }
		public enum Target { PlayGame, ReachLevel, CollectCoins, CollectXP, CollectTickets, ShareApp, RateApp }
		public enum TargetSocialMedia { None, Facebook, Twitter }
		public enum Reward { Coins, XP, Tickets, Item }

		#endregion

		#region Variables

		public int ID
		{
			get
			{
				return id;
			}
		}
		public string name;
		public string description;
		public Period period;
		public string startAt;
		public string endAt;
		public Target target;
		public Game TargetGame
		{
			get
			{
				if (target == Target.ShareApp || target == Target.RateApp)
				{
					if (targetGameID > 0 || targetGame != null)
					{
						targetGameID = 0;
						targetGame = null;
					}
				}
				else if (targetGame == null && targetGameID > 0 || targetGame != null && targetGameID != targetGame.ID)
					targetGame = ItemsAndGamesManager.Instance.Games?.Find(game => game.ID == targetGameID);

				if (targetGame != null && !targetGame.IsValid())
					targetGame = null;

				return targetGame;
			}
		}
		public int targetGameID;
		public int targetValue = 1;
		public TargetSocialMedia targetSocialMedia;
		public int ReachedTargetValue
		{
			get
			{
				if (targetValue < 1)
					return default;

				return Mathf.Clamp(PlayerPrefs.GetInt(QuestPrefsKey, default), 0, targetValue);
			}
		}
		public float CurrentPlayerTargetProgress
		{
			get
			{
				if (targetValue < 1)
					return default;

				return Mathf.Clamp01((float)ReachedTargetValue / targetValue);
			}
		}
		public Reward reward;
		public Item RewardItem
		{
			get
			{
				if (reward != Reward.Item)
				{
					if (rewardItemID > 0 || rewardItem != null)
					{
						rewardItemID = 0;
						rewardItem = null;
					}
				}
				else if (rewardItem == null && rewardItemID > 0 || rewardItem != null && rewardItemID != rewardItem.ID)
					rewardItem = ItemsAndGamesManager.Instance.Items?.Find(item => item.ID == rewardItemID);

				if (rewardItem != null && !rewardItem.IsValid())
					rewardItem = null;

				return rewardItem;
			}
		}
		public int rewardItemID;
		public int rewardAmount;

		private string QuestPrefsKey => GameController.Instance.IsLoggedIn() ? $"QUEST{(ItemsAndGamesManager.Instance.UseLocalServer ? "" : "_ONLINE")}_{GameController.Instance.UserSessionDetails.username.ToUpper()}_{ID}_REACHED_TARGET_VALUE" : string.Empty;
		private string QuestClaimPrefsKey => GameController.Instance.IsLoggedIn() ? $"QUEST{(ItemsAndGamesManager.Instance.UseLocalServer ? "" : "_ONLINE")}_{GameController.Instance.UserSessionDetails.username.ToUpper()}_{ID}_CLAIMED" : string.Empty;
		[SerializeField]
		private int id;
		private Game targetGame;
		private Item rewardItem;

		#endregion

		#region Methods

		#region Static Methods

		public static Quest FromJsonObject(JSONObject json)
		{
			Quest quest = new(Convert.ToInt32(json["id"].str))
			{
				name = json["name"].str,
				description = json["description"].str,
				period = (Period)Array.IndexOf(Enum.GetNames(typeof(Period)), json["period"].str),
				startAt = json["start_at"].str,
				endAt = json["end_at"].str,
				target = (Target)Array.IndexOf(Enum.GetNames(typeof(Target)), json["target"].str),
				targetGameID = !string.IsNullOrEmpty(json["target_game_id"].str) ? Convert.ToInt32(json["target_game_id"].str) : default,
				targetValue = Convert.ToInt32(json["target_value"].str),
				targetSocialMedia = !string.IsNullOrEmpty(json["target_social_media"].str) ? (TargetSocialMedia)Array.IndexOf(Enum.GetNames(typeof(TargetSocialMedia)), json["target_social_media"].str) : default,
				reward = (Reward)Array.IndexOf(Enum.GetNames(typeof(Reward)), json["reward"].str),
				rewardItemID = !string.IsNullOrEmpty(json["reward_item_id"].str) ? Convert.ToInt32(json["reward_item_id"].str) : default,
				rewardAmount = !string.IsNullOrEmpty(json["reward_amount"].str) ? Convert.ToInt32(json["reward_amount"].str) : default
			};

			quest.FixPeriod();

			return quest;
		}

		#endregion

		#region Global Methods

		public bool IsActive()
		{
			if (string.IsNullOrEmpty(startAt))
				return true;

			string now = DateTime.Now.ToString("Y-m-d H:i:s");

			return string.Compare(now, startAt) >= 0 && (string.IsNullOrEmpty(endAt) || string.Compare(now, endAt) < 0);
		}
		public bool IsDone()
		{
			return ReachedTargetValue >= targetValue;
		}
		public bool IsClaimed()
		{
			return Utilities.Utility.NumberToBool(PlayerPrefs.GetInt(QuestClaimPrefsKey, default)) || reward == Reward.Item && RewardItem != null && RewardItem.IsBought();
		}
		public void SetPlayerTargetValue(int newValue)
		{
			bool done = IsDone();

			PlayerPrefs.SetInt(QuestPrefsKey, Mathf.Clamp(newValue, 0, targetValue));

			if (!done && done != IsDone())
			{
				UIController.Instance.ShowDialog("Congrats!", "You've just finished a quest!", "Show Quests", "Ignore", () =>
				{
					UIController.Instance.ShowCanvas(UIController.UICanvasType.Quests);

					switch (period)
					{
						case Period.Daily:
							UIController.Instance.ShowQuestsTab(Period.Daily);

							break;

						case Period.Weekly:
							UIController.Instance.ShowQuestsTab(Period.Weekly);

							break;

						case Period.Monthly:
							UIController.Instance.ShowQuestsTab(Period.Monthly);

							break;

						case Period.Yearly:
							UIController.Instance.ShowQuestsTab(Period.Yearly);

							break;

						default:
							UIController.Instance.ShowQuestsTab(Period.General);

							break;
					}
				});
				GameController.Instance.PlayHubClip(GameController.Instance.questFinishedSound, true);
			}
		}
		public void Go(UnityAction action = null)
		{
			switch (target)
			{
				case Target.RateApp:
					switch (Application.platform)
					{
						case RuntimePlatform.Android:
							UIController.Instance.VisitGooglePlayStore(action);

							break;

						case RuntimePlatform.IPhonePlayer:
							UIController.Instance.VisitAppleAppStore(action);

							break;

						default:
							UIController.Instance.OpenRateDialog(action);

							break;
					}

					break;

				case Target.ShareApp:
					switch (targetSocialMedia)
					{
						case TargetSocialMedia.Facebook:
							UIController.Instance.ShareOnFacebook(action);

							break;

						case TargetSocialMedia.Twitter:
							UIController.Instance.ShareOnTwitter(action);

							break;
					}

					break;

				default:
					GameController.Instance.PlayUIClickSound();

					if (TargetGame != null)
						TargetGame.BuyOrPlay();
					else
						UIController.Instance.ShowCanvas(UIController.UICanvasType.Main);

					break;
			}
		}
		public void Claim(UnityAction action = null)
		{
			if (action != null)
				action.Invoke();

			PlayerPrefs.SetInt(QuestClaimPrefsKey, 1);
		}

		internal void FixPeriod()
		{
			switch (period)
			{
				case Period.Daily:
					if (!UIController.Instance.quests.dailyBackground)
					{
						if (UIController.Instance.quests.generalBackground)
							period = Period.General;
						else if (UIController.Instance.quests.weeklyBackground)
							period = Period.Weekly;
						else if (UIController.Instance.quests.monthlyBackground)
							period = Period.Monthly;
						else if (UIController.Instance.quests.yearlyBackground)
							period = Period.Yearly;
					}

					break;

				case Period.Weekly:
					if (!UIController.Instance.quests.weeklyBackground)
					{
						if (UIController.Instance.quests.generalBackground)
							period = Period.General;
						else if (UIController.Instance.quests.dailyBackground)
							period = Period.Daily;
						else if (UIController.Instance.quests.monthlyBackground)
							period = Period.Monthly;
						else if (UIController.Instance.quests.yearlyBackground)
							period = Period.Yearly;
					}

					break;

				case Period.Monthly:
					if (!UIController.Instance.quests.monthlyBackground)
					{
						if (UIController.Instance.quests.generalBackground)
							period = Period.General;
						else if (UIController.Instance.quests.dailyBackground)
							period = Period.Daily;
						else if (UIController.Instance.quests.weeklyBackground)
							period = Period.Weekly;
						else if (UIController.Instance.quests.yearlyBackground)
							period = Period.Yearly;
					}

					break;

				case Period.Yearly:
					if (!UIController.Instance.quests.yearlyBackground)
					{
						if (UIController.Instance.quests.generalBackground)
							period = Period.General;
						else if (UIController.Instance.quests.dailyBackground)
							period = Period.Daily;
						else if (UIController.Instance.quests.weeklyBackground)
							period = Period.Weekly;
						else if (UIController.Instance.quests.monthlyBackground)
							period = Period.Monthly;
					}

					break;

				case Period.General:
					if (!UIController.Instance.quests.generalBackground)
					{
						if (UIController.Instance.quests.dailyBackground)
							period = Period.Daily;
						else if (UIController.Instance.quests.weeklyBackground)
							period = Period.Weekly;
						else if (UIController.Instance.quests.monthlyBackground)
							period = Period.Monthly;
						else if(UIController.Instance.quests.yearlyBackground)
							period = Period.Yearly;
					}

					break;
			}
		}

		#endregion

		#endregion

		#region Constructors

		public Quest(int id)
		{
			this.id = id;
		}

		#endregion
	}
	[CreateAssetMenu(fileName = "New Quests Manager", menuName = "Utility/Quests Manager")]
	public class QuestsManager : ScriptableObject
	{
		#region Variables

		#region Static Variables

		public static QuestsManager Instance
		{
			get
			{
				if (!instance)
				{
					instance = Resources.Load("ScriptableObjects/QuestsManager") as QuestsManager;

					if (!instance)
						Debug.LogError("The default QuestsManager instance cannot be found!");
				}

				return instance;
			}
		}
		public static string HostURL => Instance.UseLocalServer ? ItemsAndGamesManager.Instance.localServerURL : ItemsAndGamesManager.Instance.onlineServerURL;
		public static bool OfflineMode => !Instance.UseLocalServer && Application.internetReachability == NetworkReachability.NotReachable;

		private static QuestsManager instance;

		#endregion

		#region Global Variables

		public bool UseLocalServer => ItemsAndGamesManager.Instance.UseLocalServer;
		public List<Quest> Quests
		{
			get
			{
				if (quests == null && !listsInitialized)
					ReloadQuests();

				return quests;
			}
		}

		private List<Quest> quests;
		private bool listsInitialized;

		#endregion

		#endregion

		#region Methods

		#region Static Methods

		public static bool AddQuest(Quest quest)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete quests while in offline mode");

				return false;
			}

			quest.FixPeriod();

			WWWForm fields = new();

			fields.AddField("request", "INSERT");
			fields.AddField("name", quest.name);
			fields.AddField("description", quest.description);
			fields.AddField("period", quest.period.ToString());
			fields.AddField("start_at", quest.startAt ?? string.Empty);
			fields.AddField("end_at", quest.endAt ?? string.Empty);
			fields.AddField("target", quest.target.ToString());
			fields.AddField("target_game_id", quest.targetGameID);
			fields.AddField("target_value", Mathf.Max(quest.targetValue, 1));
			fields.AddField("target_social_media", quest.targetSocialMedia.ToString());
			fields.AddField("reward", quest.reward.ToString());
			fields.AddField("reward_item_id", quest.rewardItemID);
			fields.AddField("reward_amount", quest.rewardAmount);

			UnityWebRequest request = MakeRequest("quests", fields);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot add quest \"{quest.name}\" to the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				return false;
			}

			JSONObject json = new(request.downloadHandler.text);
			string response = json["response"].str;
			string message = json["message"].str;
			string query = json["query"]?.str;

			request.Dispose();

			if (response != "200")
			{
				Debug.LogError($"Adding the quest \"{quest.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			quest = Quest.FromJsonObject(json["quest"]);

			Instance.Quests.Add(quest);

			Instance.quests = Instance.quests.Distinct().ToList();

			return Instance.SaveOfflineQuestsData("QuestsList", Instance.quests);
		}
		public static bool ModifyQuest(Quest quest)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete quests while in offline mode");

				return false;
			}

			Instance.ReloadQuests();
			quest.FixPeriod();

			int questIndex = Instance.Quests.FindIndex(g => g.ID == quest.ID);

			if (questIndex < 0)
			{
				Debug.LogError($"Cannot update quest \"{quest.name}\"'s data on server as it doesn't exist on the list or has been removed");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "UPDATE");
			fields.AddField("id", quest.ID);
			fields.AddField("name", quest.name);
			fields.AddField("description", quest.description);
			fields.AddField("period", quest.period.ToString());
			fields.AddField("start_at", quest.startAt ?? string.Empty);
			fields.AddField("end_at", quest.endAt ?? string.Empty);
			fields.AddField("target", quest.target.ToString());
			fields.AddField("target_game_id", quest.targetGameID);
			fields.AddField("target_value", Mathf.Max(quest.targetValue, 1));
			fields.AddField("target_social_media", quest.targetSocialMedia.ToString());
			fields.AddField("reward", quest.reward.ToString());
			fields.AddField("reward_item_id", quest.rewardItemID);
			fields.AddField("reward_amount", quest.rewardAmount);

			UnityWebRequest request = MakeRequest("quests", fields);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot update quest \"{quest.name}\"'s data on server\r\nResponse: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				return false;
			}

			JSONObject json = new(request.downloadHandler.text);
			string response = json["response"].str;
			string message = json["message"].str;
			string query = json["query"]?.str;

			request.Dispose();

			if (response != "200")
			{
				Debug.LogError($"Updating the quest \"{quest.name}\" on server has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			Instance.Quests[questIndex] = Quest.FromJsonObject(json["quest"]);
			Instance.quests = Instance.quests.Distinct().ToList();

			return Instance.SaveOfflineQuestsData("QuestsList", Instance.quests);
		}
		public static bool RemoveQuest(Quest quest)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete quests while in offline mode");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "DELETE");
			fields.AddField("id", quest.ID);

			UnityWebRequest request = MakeRequest("quests", fields);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot remove quest \"{quest.name}\" from the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				return false;
			}

			JSONObject json = new(request.downloadHandler.text);
			string response = json["response"].str;
			string message = json["message"].str;
			string query = json["query"]?.str;

			request.Dispose();

			if (response != "200")
			{
				Debug.LogError($"Removing the quest \"{quest.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			if (!Instance.Quests.Remove(quest))
			{
				Debug.LogWarning($"The quest \"{quest.name}\" might have been removed from the server but not from the list. Refreshing quests list...");
				Instance.ReloadQuests();

				return true;
			}

			Instance.quests = Instance.quests.Distinct().ToList();

			return Instance.SaveOfflineQuestsData("QuestsList", Instance.quests);
		}

		private static UnityWebRequest MakeRequest(string pageName, WWWForm form = null, bool sendRequest = true)
		{
			Uri uri = new($@"{HostURL}{pageName}");
			UnityWebRequest request = form != null ? UnityWebRequest.Post(uri, form) : UnityWebRequest.Get(uri);

			if (HostURL.StartsWith("https"))
			{
				request.certificateHandler = new Utilities.Utility.CerificateHandlerOverrider();
				request.disposeCertificateHandlerOnDispose = true;
			}

			request.disposeDownloadHandlerOnDispose = true;
			request.disposeUploadHandlerOnDispose = true;

			if (sendRequest)
			{
				request.SendWebRequest();

				while (request.result == UnityWebRequest.Result.InProgress)
					continue;
			}

			return request;
		}

		#endregion

		#region Global Methods

		public void ReloadQuests()
		{
			listsInitialized = true;

			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading quests from offline data...");

				goto load_offline_data;
			}

			UnityWebRequest request = MakeRequest("quests");

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get quests list from server. Loading offline data...\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				goto load_offline_data;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (quests == null)
				quests = new List<Quest>(json.Count);
			else
				quests.Clear();

			if (!json || json.Count < 1)
				return;

			foreach (JSONObject obj in json.list)
				quests.Add(Quest.FromJsonObject(obj));

			if (!SaveOfflineQuestsData("QuestsList", quests))
				Debug.LogWarning("Could not save offline quests data locally");

			return;

		load_offline_data:
			if (LoadOfflineQuestsData("QuestsList", out quests))
				Debug.Log("Quests list has been loaded from existing offline data");
			else
				Debug.LogWarning("Could not load quests from offline data");
		}

		private bool LoadOfflineQuestsData<T>(string fileName, out List<T> data)
		{
			string playerLoadPath = Path.Combine(Application.persistentDataPath, fileName);
			DataSerializationUtility<List<T>> utility = new(playerLoadPath, false, true);

			data = utility.Load();

			if (data != null)
				return true;

			string loadPath = Path.Combine("Assets", fileName);

			utility = new DataSerializationUtility<List<T>>(loadPath, true, true);

			data = utility.Load();

			if (data != null)
				return true;

			return false;
		}
		private bool SaveOfflineQuestsData<T>(string fileName, List<T> data)
		{
			if (data == null || data.Count < 1)
				return false;

#if UNITY_EDITOR
			string savePath = Path.Combine("Assets", fileName);
			string playerSavePath =
#else
			string savePath =
#endif
			Path.Combine(Application.persistentDataPath, fileName);

			DataSerializationUtility<List<T>> utility = new(savePath, true);

			if (!utility.SaveOrCreate(data))
				return false;

#if UNITY_EDITOR
			utility = new DataSerializationUtility<List<T>>(playerSavePath, false);

			if (!utility.SaveOrCreate(data))
				return false;
#endif

			return true;
		}

		#endregion

		#endregion
	}
}
