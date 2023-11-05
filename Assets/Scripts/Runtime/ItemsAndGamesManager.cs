#region Namespaces

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;
using GamesConverse.UI;

#endregion

namespace GamesConverse
{
	[Serializable]
	public class Game
	{
		#region Variables

		#region Static Variables

		private static string FavouritesPrefsKey => $"{GameController.Instance.UserSessionDetails.username.ToUpper()}_GAME_FAVOURITES";
		private static string HiddensPrefsKey => $"{GameController.Instance.UserSessionDetails.username.ToUpper()}_GAME_HIDDENS";

		#endregion

		#region Global Variables

		public int ID
		{
			get
			{
				return id;
			}
		}
		public string name = "New Game";
		public string description = string.Empty;
		public string TypeName
		{
			get
			{
				if (!ItemsAndGamesManager.Instance || ItemsAndGamesManager.Instance.GameTypes == null || ItemsAndGamesManager.Instance.GameTypes.Length < 1)
				{
					Debug.LogError("Cannot get the game's Type Name");

					return default;
				}

				return ItemsAndGamesManager.Instance.GameTypes[TypeIndex];
			}
		}
		public int TypeIndex
		{
			get
			{
				return typeIndex;
			}
			set
			{
				if (ItemsAndGamesManager.Instance.GameTypes == null)
				{
					Debug.LogWarning("Cannot set the game's Type Index");

					return;
				}

				typeIndex = Mathf.Clamp(value, 0, ItemsAndGamesManager.Instance.GameTypes.Length - 1);
			}
		}
		public string GenreName
		{
			get
			{
				if (!ItemsAndGamesManager.Instance || ItemsAndGamesManager.Instance.GameGenres == null || ItemsAndGamesManager.Instance.GameGenres.Length < 1)
				{
					Debug.LogError("Cannot get the game's Genre Name");

					return default;
				}

				return ItemsAndGamesManager.Instance.GameGenres[GenreIndex];
			}
		}
		public int GenreIndex
		{
			get
			{
				return genreIndex;
			}
			set
			{
				if (ItemsAndGamesManager.Instance.GameGenres == null)
				{
					Debug.LogWarning("Cannot set the game's Genre Index");

					return;
				}

				genreIndex = Mathf.Clamp(value, 0, ItemsAndGamesManager.Instance.GameGenres.Length - 1);
			}
		}
		public Sprite Icon
		{
			get
			{
				if (!icon && !string.IsNullOrWhiteSpace(iconPath))
				{
					iconPath = iconPath.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
					icon = Resources.Load<Sprite>(iconPath);

					if (!icon)
					{
						Debug.LogError($"Item at path \"{iconPath}\" doesn't exist anymore or has been moved!");

						iconPath = string.Empty;
					}
				}

				return icon;
			}
#if UNITY_EDITOR
			set
			{
				if (value)
				{
					string newPath = UnityEditor.AssetDatabase.GetAssetPath(value);

					if (!newPath.Contains("/Resources/"))
					{
						Debug.LogError("The assigned item must be a child of one of the Resources folders!");

						return;
					}

					iconPath = Path.ChangeExtension(newPath.Replace("Assets/Resources/", ""), null);
				}
				else
					iconPath = string.Empty;

				icon = value;
			}
#endif
		}
		public string IconPath => iconPath ?? string.Empty;
		public int Price
		{
			get
			{
				return price;
			}
			set
			{
				price = Mathf.Max(value, 0);
			}
		}
		public string scenePath;

		[SerializeField]
		private int id;
		[SerializeField]
		private int typeIndex;
		[SerializeField]
		private int genreIndex;
		[SerializeField]
		private string iconPath;
		[NonSerialized]
		private Sprite icon;
		[SerializeField]
		private int price;

		#endregion

		#endregion

		#region Methods

		#region Static Methods

		public static Game JsonObjectToGame(JSONObject json)
		{
			Game game = new(Convert.ToInt32(json["id"].str))
			{
				name = json["name"].str,
				description = string.IsNullOrEmpty(json["description"].str) ? string.Empty : json["description"].str,
				typeIndex = Convert.ToInt32(json["type_id"].str) - 1,
				genreIndex = Convert.ToInt32(json["genre_id"].str) - 1,
				iconPath = string.IsNullOrEmpty(json["icon_path"].str) ? string.Empty : json["icon_path"].str,
				price = string.IsNullOrEmpty(json["price"].str) ? 0 : Convert.ToInt32(json["price"].str),
				scenePath = json["scene_path"].str
			};

			return game;
		}

		#endregion

		#region Global Methods

		public bool IsFree()
		{
			return TypeIndex < 1 || price < 1;
		}
		public bool IsBought()
		{
			if (IsFree())
				return true;
			else if (!GameController.Instance.UserStatsDetails || !GameController.Instance.IsLoggedIn() || GameController.Instance.UserStatsDetails.boughtGames == null)
				return false;

			return Array.Exists(GameController.Instance.UserStatsDetails.boughtGames, id => ID == id);
		}
		public bool IsFavourite()
		{
			if (!GameController.Instance.IsLoggedIn())
				return false;

			if (!PlayerPrefs.HasKey(FavouritesPrefsKey))
				return false;

			return Array.Exists(JsonUtility.FromJson<Utilities.Utility.JsonArray<int>>(PlayerPrefs.GetString(FavouritesPrefsKey, string.Empty)).ToArray(), id => id == ID);
		}
		public bool IsHidden()
		{
			if (!GameController.Instance.IsLoggedIn())
				return false;

			if (!PlayerPrefs.HasKey(HiddensPrefsKey))
				return false;

			return Array.Exists(JsonUtility.FromJson<Utilities.Utility.JsonArray<int>>(PlayerPrefs.GetString(HiddensPrefsKey, string.Empty)).ToArray(), id => id == ID);
		}
		public bool IsPlayed()
		{
			return Utilities.Utility.NumberToBool(PlayerPrefs.GetInt($"GAME_{ID}_{GameController.Instance.UserStatsDetails.username.ToUpper()}_PLAYED", default));
		}
		public bool IsValid()
		{
			return ID > 0 && !string.IsNullOrWhiteSpace(scenePath);
		}
		public void MakeOrRemoveFavourite(bool confirm = false)
		{
			List<int> favourites = PlayerPrefs.HasKey(FavouritesPrefsKey) ? JsonUtility.FromJson<Utilities.Utility.JsonArray<int>>(PlayerPrefs.GetString(FavouritesPrefsKey, string.Empty)).ToList() : new();
			int favouriteIndex = favourites.IndexOf(ID);
			bool isFavourite = favouriteIndex > -1;

			if (confirm)
			{
				if (isFavourite)
					favourites.RemoveAt(favouriteIndex);
				else
					favourites.Add(ID);

				Utilities.Utility.JsonArray<int> jsonArray = new(favourites);

				PlayerPrefs.SetString(FavouritesPrefsKey, JsonUtility.ToJson(jsonArray));
				UIController.Instance.RefreshMain();
			}
			else
				UIController.Instance.ShowDialog("Are you sure?", $"Do you really want to {(isFavourite ? "remove this game from" : "add this game to")} the favourites list?", "Yes", "No", () => { MakeOrRemoveFavourite(true); });
		}
		public void MakeOrRemoveHidden(bool confirm = false)
		{
			List<int> hiddens = PlayerPrefs.HasKey(HiddensPrefsKey) ? JsonUtility.FromJson<Utilities.Utility.JsonArray<int>>(PlayerPrefs.GetString(HiddensPrefsKey, "{items:[]}")).ToList() : new();
			int hiddenIndex = hiddens.IndexOf(ID);
			bool isHidden = hiddenIndex > -1;

			if (confirm)
			{
				if (isHidden)
					hiddens.RemoveAt(hiddenIndex);
				else
					hiddens.Add(ID);

				Utilities.Utility.JsonArray<int> jsonArray = new(hiddens);

				PlayerPrefs.SetString(HiddensPrefsKey, JsonUtility.ToJson(jsonArray));
				UIController.Instance.RefreshMain();
			}
			else
				UIController.Instance.ShowDialog("Are you sure?", $"Do you really want to {(isHidden ? "show this game" : "hide this game")}?", "Yes", "No", () => { MakeOrRemoveHidden(true); });
		}
		public void MarkAsPlayed()
		{
			QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && !quest.IsDone() && quest.target == Quest.Target.PlayGame && quest.targetGameID == ID).ForEach(quest => quest.SetPlayerTargetValue(1));
			PlayerPrefs.SetInt($"GAME_{ID}_{GameController.Instance.UserStatsDetails.username.ToUpper()}_PLAYED", 1);
		}
		public void MarkAsNotPlayed()
		{
			QuestsManager.Instance.Quests.FindAll(quest => quest.IsActive() && quest.IsDone() && quest.target == Quest.Target.PlayGame && quest.targetGameID == ID).ForEach(quest => quest.SetPlayerTargetValue(0));
			PlayerPrefs.SetInt($"GAME_{ID}_{GameController.Instance.UserStatsDetails.username.ToUpper()}_PLAYED", 0);
		}
		public void BuyOrPlay()
		{
			if (IsBought())
			{
				if (!IsPlayed())
					MarkAsPlayed();

				GameController.LoadScene(Path.GetFileNameWithoutExtension(scenePath));
			}
			else
				UIController.Instance.ShowDialog("Unlock Game", $"Are you sure you want to unlock \"{name}\" for {price} coins?", "Yes!", "No", () => GameController.Instance.StartCoroutine(ItemsAndGamesManager.BuyGame(this, true, true)));
		}

		#endregion

		#endregion

		#region Constructors

		public Game(int id)
		{
			this.id = id;
		}

		#endregion
	}
	[Serializable]
	public class Item
	{
		#region Variables

		public int ID
		{
			get
			{
				return id;
			}
		}
		public string name = "New Item";
		public Sprite Sprite
		{
			get
			{
				if (!sprite && !string.IsNullOrWhiteSpace(spritePath))
				{
					spritePath = spritePath.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
					sprite = Resources.Load<Sprite>(spritePath);

					if (!sprite)
					{
						Debug.LogError($"Item at path \"{spritePath}\" doesn't exist anymore or has been moved!");

						spritePath = string.Empty;
					}
				}

				return sprite;
			}
#if UNITY_EDITOR
			set
			{
				if (value)
				{
					string newPath = UnityEditor.AssetDatabase.GetAssetPath(value);

					if (!newPath.Contains("/Resources/"))
					{
						Debug.LogError("The assigned item must be a child of one of the Resources folders!");

						return;
					}

					spritePath = Path.ChangeExtension(newPath.Replace("Assets/Resources/", ""), null);
				}
				else
					spritePath = string.Empty;

				sprite = value;
			}
#endif
		}
		public Sprite Icon
		{
			get
			{
				if (!icon && !string.IsNullOrWhiteSpace(iconPath))
				{
					iconPath = iconPath.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
					icon = Resources.Load<Sprite>(iconPath);

					if (!icon)
					{
						Debug.LogError($"Item at path \"{iconPath}\" doesn't exist anymore or has been moved!");

						iconPath = string.Empty;
					}
				}

				return icon;
			}
#if UNITY_EDITOR
			set
			{
				if (value)
				{
					string newPath = UnityEditor.AssetDatabase.GetAssetPath(value);

					if (!newPath.Contains("/Resources/"))
					{
						Debug.LogError("The assigned item must be a child of one of the Resources folders!");

						return;
					}

					iconPath = Path.ChangeExtension(newPath.Replace("Assets/Resources/", ""), null);
				}
				else
					iconPath = string.Empty;

				icon = value;
			}
#endif
		}
		public int TypeIndex
		{
			get
			{
				return typeIndex;
			}
			set
			{
				if (ItemsAndGamesManager.Instance.ItemTypes == null)
				{
					Debug.LogWarning("Cannot set the item's Type Index");

					return;
				}

				typeIndex = Mathf.Clamp(value, 0, ItemsAndGamesManager.Instance.ItemTypes.Length - 1);
			}
		}
		public string SpritePath => spritePath ?? string.Empty;
		public string IconPath => iconPath ?? string.Empty;
		public int[] dependencies;
		public int dependencyAlternative;
		public int Price
		{
			get
			{
				return price;
			}
			set
			{
				price = Mathf.Max(value, 0);
			}
		}

#pragma warning disable IDE0044 // Add readonly modifier
		[SerializeField]
		private int id;
#pragma warning restore IDE0044 // Add readonly modifier
		[SerializeField]
		private int typeIndex;
		[SerializeField]
		private string iconPath;
		[SerializeField]
		private string spritePath;
		[NonSerialized]
		private Sprite sprite;
		[NonSerialized]
		private Sprite icon;
		[SerializeField]
		private int price;

		#endregion

		#region Methods

		#region Static Methods

		public static Item JsonObjectToItem(JSONObject json)
		{
			string name = json["name"].str;
			int typeIndex = Convert.ToInt32(json["type_id"].str) - 1;
			string spritePath = string.IsNullOrWhiteSpace(json["sprite_path"].str) ? default : json["sprite_path"].str;
			string iconPath = string.IsNullOrWhiteSpace(json["icon_path"].str) ? default : json["icon_path"].str;
			int[] dependencies = string.IsNullOrWhiteSpace(json["dependencies"].str) ? null : json["dependencies"].str.Split(';').Select(link => Convert.ToInt32(link)).ToArray();
			int dependencyAlternative = string.IsNullOrWhiteSpace(json["dependency_alternative"].str) ? 0 : Convert.ToInt32(json["dependency_alternative"].str);
			int price = Convert.ToInt32(json["price"].str);

			Item item = new(Convert.ToInt32(json["id"].str))
			{
				name = name,
				typeIndex = typeIndex,
				spritePath = spritePath,
				iconPath = iconPath,
				dependencies = dependencies,
				dependencyAlternative = dependencyAlternative,
				price = price
			};

			return item;
		}

		#endregion

		#region Global Methods

		public bool IsValid()
		{
			return ID > 0 && Sprite && Icon;
		}
		public bool IsBought()
		{
			if (price < 1)
				return true;
			else if (!GameController.Instance.IsLoggedIn() || !GameController.Instance.UserStatsDetails || GameController.Instance.UserStatsDetails.boughtItems == null)
				return false;

			return Array.Exists(GameController.Instance.UserStatsDetails.boughtItems, id => ID == id);
		}

		#endregion

		#endregion

		#region Constructors

		public Item(int id)
		{
			this.id = id;
		}

		#endregion
	}
	[CreateAssetMenu(fileName = "New Items And Games Manager", menuName = "Utility/Items And Games Manager")]
	public class ItemsAndGamesManager : ScriptableObject
	{
		#region Variables

		#region Static Variables

		public static ItemsAndGamesManager Instance
		{
			get
			{
				if (!instance)
				{
					instance = Resources.Load("ScriptableObjects/ItemsAndGamesManager") as ItemsAndGamesManager;

					if (!instance)
						Debug.LogError("The default ItemsAndGamesManager instance cannot be found!");
				}

				return instance;
			}
		}
		public static string HostURL => Instance.UseLocalServer ? Instance.localServerURL : Instance.onlineServerURL;
		public static bool OfflineMode => !Instance.UseLocalServer && Application.internetReachability == NetworkReachability.NotReachable;

		private static ItemsAndGamesManager instance;

		#endregion

		#region Global Variables

		public List<Game> Games
		{
			get
			{
				if (games == null && !listsInitialized)
					ReloadGames();

				return games;
			}
		}
		public List<Item> Items
		{
			get
			{
				if (items == null && !listsInitialized)
					ReloadItems();

				return items;
			}
		}
		public string[] GameGenres
		{
			get
			{
				if (gameGenres == null || gameGenres.Length < 1)
					ReloadGameGenres();

				return gameGenres;
			}
		}
		public string[] GameTypes
		{
			get
			{
				if (gameTypes == null || gameTypes.Length < 1)
					ReloadGameTypes();

				return gameTypes;
			}
		}
		public string[] ItemTypes
		{
			get
			{
				if (itemTypes == null || itemTypes.Length < 1)
					ReloadItemTypes();

				return itemTypes;
			}
		}
		public string onlineServerURL = "https://gameconverse-test.000webhostapp.com/";
		public string localServerURL = "http://localhost/game_converse/";
		public bool UseLocalServer
		{
			get
			{
				return useLocalServer;
			}
			set
			{
				useLocalServer = value;

				ReloadGames();
			}
		}

		private List<Game> games;
		[SerializeField]
		private List<Item> items;
		private string[] gameGenres;
		private string[] gameTypes;
		private string[] itemTypes;
		[SerializeField]
		private bool useLocalServer;
		private bool listsInitialized;

		#endregion

		#endregion

		#region Methods

		#region Static Methods

		public static bool AddGame(Game game)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete games while in offline mode");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "INSERT");
			fields.AddField("name", game.name);
			fields.AddField("description", game.description ?? string.Empty);
			fields.AddField("type_id", game.TypeIndex + 1);
			fields.AddField("genre_id", game.GenreIndex + 1);
			fields.AddField("icon_path", game.IconPath);
			fields.AddField("price", game.Price);
			fields.AddField("scene_path", game.scenePath);

			UnityWebRequest request = MakeRequest("games", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot add game \"{game.name}\" to the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Adding the game \"{game.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			game = Game.JsonObjectToGame(json["game"]);

			Instance.Games.Add(game);

			Instance.games = Instance.games.Distinct().ToList();

			return Instance.SaveOfflineGamesData("GamesList", Instance.games);
		}
		public static bool ModifyGame(Game game)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete games while in offline mode");

				return false;
			}

			Instance.ReloadGames();

			int gameIndex = Instance.Games.FindIndex(g => g.ID == game.ID);

			if (gameIndex < 0)
			{
				Debug.LogError($"Cannot update game \"{game.name}\"'s data on server as it doesn't exist on the list or has been removed");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "UPDATE");
			fields.AddField("id", game.ID);
			fields.AddField("name", game.name);
			fields.AddField("description", game.description ?? string.Empty);
			fields.AddField("type_id", game.TypeIndex + 1);
			fields.AddField("genre_id", game.GenreIndex + 1);
			fields.AddField("icon_path", game.IconPath);
			fields.AddField("price", game.Price);
			fields.AddField("scene_path", game.scenePath);

			UnityWebRequest request = MakeRequest("games", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot update game \"{game.name}\"'s data on server\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Updating the game \"{game.name}\" on server has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			Instance.Games[gameIndex] = Game.JsonObjectToGame(json["game"]);
			Instance.games = Instance.games.Distinct().ToList();

			return Instance.SaveOfflineGamesData("GamesList", Instance.games);
		}
		public static bool RemoveGame(Game game)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete games while in offline mode");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "DELETE");
			fields.AddField("id", game.ID);

			UnityWebRequest request = MakeRequest("games", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot remove game \"{game.name}\" from the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Removing the game \"{game.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			if (!Instance.Games.Remove(game))
			{
				Debug.LogWarning($"The game \"{game.name}\" might have been removed from the server but not from the list. Refreshing games list...");
				Instance.ReloadGames();

				return true;
			}

			Instance.games = Instance.games.Distinct().ToList();

			return Instance.SaveOfflineGamesData("GamesList", Instance.games);
		}
		public static bool AddItem(Item item)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete items while in offline mode");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "INSERT");
			fields.AddField("name", item.name);
			fields.AddField("type_id", item.TypeIndex + 1);
			fields.AddField("sprite_path", item.SpritePath);
			fields.AddField("icon_path", item.IconPath);
			fields.AddField("dependencies", item.dependencies != null ? string.Join(";", item.dependencies) : "");
			fields.AddField("dependency_alternative", item.dependencyAlternative != 0 ? item.dependencyAlternative.ToString() : "NULL");
			fields.AddField("price", item.Price);

			UnityWebRequest request = MakeRequest("items", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot add item \"{item.name}\" to the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Adding the item \"{item.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			item = Item.JsonObjectToItem(json["item"]);

			Instance.Items.Add(item);

			Instance.items = Instance.items.Distinct().ToList();

			return Instance.SaveOfflineItemsData("ItemsList", Instance.items);
		}
		public static bool ModifyItem(Item item)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete items while in offline mode");

				return false;
			}

			Instance.ReloadItems();

			int itemIndex = Instance.items.FindIndex(g => g.ID == item.ID);

			if (itemIndex < 0)
			{
				Debug.LogError($"Cannot update item \"{item.name}\"'s data on server as it doesn't exist on the list or has been removed");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "UPDATE");
			fields.AddField("id", item.ID);
			fields.AddField("name", item.name);
			fields.AddField("type_id", item.TypeIndex + 1);
			fields.AddField("sprite_path", item.SpritePath);
			fields.AddField("icon_path", item.IconPath);
			fields.AddField("dependencies", item.dependencies != null ? string.Join(";", item.dependencies) : "");
			fields.AddField("dependency_alternative", item.dependencyAlternative != 0 ? item.dependencyAlternative.ToString() : "NULL");
			fields.AddField("price", item.Price);

			UnityWebRequest request = MakeRequest("items", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot update item \"{item.name}\"'s data on server\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Updating the item \"{item.name}\" on server has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			Instance.Items[itemIndex] = Item.JsonObjectToItem(json["item"]);
			Instance.items = Instance.items.Distinct().ToList();

			return Instance.SaveOfflineItemsData("ItemsList", Instance.items);
		}
		public static bool RemoveItem(Item item)
		{
			if (OfflineMode)
			{
				Debug.LogWarning("Cannot add/modify/delete items while in offline mode");

				return false;
			}

			WWWForm fields = new();

			fields.AddField("request", "DELETE");
			fields.AddField("id", item.ID);

			UnityWebRequest request = MakeRequest("items", fields, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot remove item \"{item.name}\" from the list\r\nResponse: {request.responseCode}\r\nError: {request.error}");
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
				Debug.LogError($"Removing the item \"{item.name}\" has failed\r\nResponse: {response}\r\nError: {message}\r\nSQL Request: {query}");

				return false;
			}

			if (!Instance.Items.Remove(item))
			{
				Debug.LogWarning($"The item \"{item.name}\" might have been removed from the server but not from the list. Refreshing items list...");
				Instance.ReloadItems();

				return true;
			}

			Instance.items = Instance.items.Distinct().ToList();

			return Instance.SaveOfflineItemsData("ItemsList", Instance.items);
		}

		internal static IEnumerator BuyOrWearItem(Item item, bool showLoading, bool chargePlayer)
		{
            if (chargePlayer && item.Price > GameController.Instance.UserStatsDetails.coins && !item.IsBought())
			{
				UIController.Instance.ShowCanvas(UIController.UICanvasType.BuyCoins);

				yield break;
			}
			else if (item.Price < 1 || item.IsBought())
			{
				GameController.Instance.UserAvatarDetails.sprites[item.TypeIndex] = item.ID;

				yield return GameController.Instance.UserAvatarDetails.StartSaveOnline();

				UIController.Instance.avatarCustomization.RefreshAvatar();

				yield break;
			}

			if (showLoading)
				UIController.Instance.ShowDialog("Please Wait...", "Loading...");

			WWWForm fields = new();

			fields.AddField("request", "BUY");
			fields.AddField("username", GameController.Instance.UserSessionDetails.username);
			fields.AddField("item_id", item.ID.ToString());

			UnityWebRequest request = MakeRequest("items", fields, false);

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { GameController.Instance.StartCoroutine(BuyOrWearItem(item, showLoading, chargePlayer)); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					yield return GameController.Instance.UserStatsDetails.StartNewAction(GameController.UserStats.ActionType.SpendingOrGain, null, item, chargePlayer ? - item.Price : 0, 0, 0);
					yield return GameController.Instance.StartAvatarRetrieving();

					UIController.Instance.avatarCustomization.RefreshItemsList();
					UIController.Instance.itemsShop.RefreshItemsList();
					UIController.Instance.HideDialog();

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Buying Item Failed ({json["response"].str}):\r\n{json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}");

					break;
			}
		}
		internal static IEnumerator BuyGame(Game game, bool showLoading, bool chargePlayer)
		{
			if (chargePlayer && game.Price > GameController.Instance.UserStatsDetails.coins && !game.IsBought())
			{
				UIController.Instance.ShowCanvas(UIController.UICanvasType.BuyCoins);

				yield break;
			}
			else if (game.IsBought())
				yield break;

			if (showLoading)
				UIController.Instance.ShowDialog("Please Wait...", "Loading...");

			WWWForm fields = new();

			fields.AddField("request", "BUY");
			fields.AddField("username", GameController.Instance.UserSessionDetails.username);
			fields.AddField("game_id", game.ID.ToString());

			UnityWebRequest request = MakeRequest("games", fields, false);

			yield return request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				UIController.Instance.ShowDialog("Error", "We've had some errors while connecting to our servers.", "Retry", "Cancel", () => { GameController.Instance.StartCoroutine(BuyGame(game, showLoading, chargePlayer)); });
				Debug.LogError($"Login Failed\r\nCode: {request.responseCode}\r\nError: {request.error}");
				request.Dispose();

				yield break;
			}

			JSONObject json = new(request.downloadHandler.text);

			request.Dispose();

			switch (json["response"].str)
			{
				case "200":
					yield return GameController.Instance.UserStatsDetails.StartNewAction(GameController.UserStats.ActionType.SpendingOrGain, game, null, chargePlayer ? - game.Price : 0, 0, 0);
					yield return GameController.Instance.StartAvatarRetrieving();

					UIController.Instance.RefreshMain();
					UIController.Instance.HideDialog();

					break;

				default:
					UIController.Instance.ShowDialog("Error", "We've had some internal errors. Please try again later.", "Okay");
					Debug.LogError($"Buying Game Failed ({json["response"].str}):\r\n{json["message"].str}\r\n{(json["query"] ? $"Query: {json["query"]}\r\n" : "")}");

					break;
			}
		}

		private static UnityWebRequest MakeRequest(string pageName, WWWForm form, bool sendRequest)
		{
			Uri uri = new($@"{HostURL}{pageName}");
			UnityWebRequest request = form != null ? UnityWebRequest.Post(uri, form) : UnityWebRequest.Get(uri);

			request.disposeCertificateHandlerOnDispose = true;
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

		public void ReloadGames()
		{
			listsInitialized = true;

			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading games from offline data...");

				goto load_offline_data;
			}
			else if (GameTypes == null || GameTypes.Length < 1 || GameGenres == null || GameGenres.Length < 1)
			{
				Debug.LogError("Cannot load games list");

				return;
			}

			UnityWebRequest request = MakeRequest("games", null, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get games list from server. Loading offline data...\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				goto load_offline_data;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (games == null)
				games = new(json.Count);
			else
				games.Clear();

			if (!json || json.Count < 1)
				return;

			foreach (JSONObject obj in json.list)
				games.Add(Game.JsonObjectToGame(obj));

			if (!SaveOfflineGamesData("GamesList", games))
				Debug.LogWarning("Could not save offline games data locally");

			return;

		load_offline_data:
			if (LoadOfflineGamesData("GamesList", out games))
				Debug.Log("Games list has been loaded from existing offline data");
			else
				Debug.LogWarning("Could not load games from offline data");
		}
		public void ReloadItems()
		{
			listsInitialized = true;

			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading items from offline data...");

				goto load_offline_data;
			}

			UnityWebRequest request = MakeRequest("items", null, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get items list from server. Loading offline data...\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				goto load_offline_data;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (items == null)
				items = new(json.Count);
			else
				items.Clear();

			if (!json || json.Count < 1)
				return;

			foreach (JSONObject obj in json.list)
				items.Add(Item.JsonObjectToItem(obj));

			if (!SaveOfflineItemsData("ItemsList", Items))
				Debug.LogWarning("Could not save offline items data locally");

			return;

		load_offline_data:
			if (LoadOfflineItemsData("ItemsList", out items))
				Debug.Log("Items list has been loaded from existing offline data");
			else
				Debug.LogWarning("Could not load items from offline data");
		}
		public void ReloadGameGenres()
		{
			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading game genres from offline data...");

				goto load_offline_data;
			}

			UnityWebRequest request = MakeRequest("game_genres", null, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get game types list from server\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				return;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (!json || json.Count < 1)
				return;

			gameGenres = new string[json.Count];

			for (int i = 0; i < json.Count; i++)
				gameGenres[i] = json.list[i].str;

			if (!SaveOfflineGamesData("GameGenresList", gameGenres.ToList()))
				Debug.LogWarning("Could not save offline game genres data locally");

			return;

		load_offline_data:
			if (LoadOfflineGamesData("GameGenresList", out List<string> gameGenresList))
			{
				gameGenres = gameGenresList.ToArray();

				Debug.Log("Game Genres list has been loaded from existing offline data");
			}
			else
				Debug.LogWarning("Could not load game genres from offline data");
		}
		public void ReloadGameTypes()
		{
			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading game types from offline data...");

				goto load_offline_data;
			}

			UnityWebRequest request = MakeRequest("game_types", null, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get game types list from server\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				return;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (!json || json.Count < 1)
				return;

			gameTypes = new string[json.Count];

			for (int i = 0; i < json.Count; i++)
				gameTypes[i] = json.list[i].str;

			if (!SaveOfflineGamesData("GameTypesList", gameTypes.ToList()))
				Debug.LogWarning("Could not save offline game types data locally");

			return;

		load_offline_data:
			if (LoadOfflineGamesData("GameTypesList", out List<string> gameTypesList))
			{
				gameTypes = gameTypesList.ToArray();

				Debug.Log("Game Types list has been loaded from existing offline data");
			}
			else
				Debug.LogWarning("Could not load game types from offline data");
		}
		public void ReloadItemTypes()
		{
			if (OfflineMode && !UseLocalServer)
			{
				Debug.Log("No internet connection detected. Loading item types from offline data...");

				goto load_offline_data;
			}

			UnityWebRequest request = MakeRequest("item_types", null, true);

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Cannot get item types list from server\r\nResponse: {request.responseCode}\r\nError: {request.error}");

				return;
			}

			JSONObject json = new(request.downloadHandler.text);

			if (!json || json.Count < 1)
				return;

			itemTypes = new string[json.Count];

			for (int i = 0; i < json.Count; i++)
				itemTypes[i] = json.list[i].str;

			if (!SaveOfflineGamesData("ItemTypesList", itemTypes.ToList()))
				Debug.LogWarning("Could not save offline item types data locally");

			return;

		load_offline_data:
			if (LoadOfflineGamesData("ItemTypesList", out List<string> itemTypesList))
			{
				itemTypes = itemTypesList.ToArray();

				Debug.Log("Item Types list has been loaded from existing offline data");
			}
			else
				Debug.LogWarning("Could not load item types from offline data");
		}

		private bool LoadOfflineGamesData<T>(string fileName, out List<T> data)
		{
			string playerLoadPath = Path.Combine(Application.persistentDataPath, fileName);
			DataSerializationUtility<List<T>> utility = new(playerLoadPath, false, true);

			data = utility.Load();

			if (data != null)
				return true;

			string loadPath = Path.Combine("Assets", fileName);

			utility = new(loadPath, true, true);

			data = utility.Load();

			if (data != null)
				return true;

			return false;
		}
		private bool SaveOfflineGamesData<T>(string fileName, List<T> data)
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
		private bool LoadOfflineItemsData<T>(string fileName, out List<T> data)
		{
			string playerLoadPath = Path.Combine(Application.persistentDataPath, fileName);
			DataSerializationUtility<List<T>> utility = new(playerLoadPath, false, true);

			data = utility.Load();

			if (data != null)
				return true;

			string loadPath = Path.Combine("Assets", fileName);

			utility = new(loadPath, true, true);

			data = utility.Load();

			if (data != null)
				return true;

			return false;
		}
		private bool SaveOfflineItemsData<T>(string fileName, List<T> data)
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
