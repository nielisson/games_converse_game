#region Namespaces

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using GamesConverse;

#endregion

namespace GamesConverseEditor
{
	[CustomEditor(typeof(ItemsAndGamesManager))]
	public class ItemsAndGamesManagerEditor : Editor
	{
		#region Variables

		public ItemsAndGamesManager Instance
		{
			get
			{
				return instance;
			}
		}

		private ItemsAndGamesManager instance;
		private Object tempScene;
		private Game tempGame;
		private Item tempItem;
		private bool editServerURLs;
		private bool modifyGameOrItem;
		private bool isReady;
		private int tempItemIndex;

		#endregion

		#region Methods

		#region Virtual Methods

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Items & Games Manager", EditorStyles.boldLabel);

			if (!editServerURLs)
			{
				bool newUseTestLink = EditorGUILayout.ToggleLeft("Use Local Server", Instance.UseLocalServer, EditorStyles.miniBoldLabel);

				if (Instance.UseLocalServer != newUseTestLink)
				{
					Instance.UseLocalServer = newUseTestLink;

					EditorUtility.SetDirty(Instance);
				}
			}

			EditorGUILayout.EndHorizontal();

			if (editServerURLs)
			{
				EditorGUI.indentLevel++;

				Instance.onlineServerURL = EditorGUILayout.TextField("Online Server", Instance.onlineServerURL);
				Instance.localServerURL = EditorGUILayout.TextField("Local Server", Instance.localServerURL);

				EditorGUI.indentLevel--;
			}
			else
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.TextField(ItemsAndGamesManager.HostURL);
				EditorGUI.EndDisabledGroup();

				if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
				{
					ItemsAndGamesManager.Instance.ReloadGameGenres();
					ItemsAndGamesManager.Instance.ReloadGameTypes();
					ItemsAndGamesManager.Instance.ReloadItemTypes();
					ItemsAndGamesManager.Instance.ReloadGames();
					ItemsAndGamesManager.Instance.ReloadItems();
					ResetInspector();
				}

				EditorGUILayout.EndHorizontal();
			}

			editServerURLs = GUILayout.Toggle(editServerURLs, "Edit Server Links", new GUIStyle(EditorStyles.miniButton));

			EditorGUILayout.Space();

			if (!isReady)
			{
				EditorGUILayout.HelpBox("There seem to be some problems while getting data from the connected server. Please check your console for more information...", MessageType.Error);

				if (GUILayout.Button("Retry"))
					OnEnable();

				return;
			}

			if (tempGame == null && tempItem == null)
			{
				EditorGUILayout.LabelField("Games", EditorStyles.miniBoldLabel);

				foreach (Game game in instance.Games)
				{
					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					EditorGUILayout.LabelField(game.name, EditorStyles.miniBoldLabel);

					if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						tempGame = game;
						modifyGameOrItem = true;

						if (!string.IsNullOrEmpty(tempGame.scenePath))
						{
							tempScene = AssetDatabase.LoadAssetAtPath<Object>(tempGame.scenePath);

							if (tempGame == null)
								EditorUtility.DisplayDialog("Games Manager", "The current game's scene has been removed or doesn't exist anymore", "Okay");
						}
					}

					if (GUILayout.Button(EditorGUIUtility.IconContent("P4_DeletedLocal"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						ItemsAndGamesManager.RemoveGame(game);

						return;
					}

					EditorGUILayout.EndHorizontal();
				}
			}

			if (tempGame != null)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.BeginHorizontal();

				tempGame.name = EditorGUILayout.TextField("Name", tempGame.name);

				if (modifyGameOrItem)
					EditorGUILayout.LabelField($"ID: {tempGame.ID}", EditorStyles.miniBoldLabel, GUILayout.Width(EditorStyles.miniBoldLabel.CalcSize(new GUIContent($"ID: {tempGame.ID}")).x));

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.LabelField("Description");

				EditorGUI.indentLevel++;

				tempGame.description = EditorGUILayout.TextArea(tempGame.description, new GUIStyle(EditorStyles.textArea), GUILayout.Height(100f));

				EditorGUI.indentLevel--;

				int newTypeIndex = EditorGUILayout.Popup("Type", tempGame.TypeIndex, Instance.GameTypes);

				if (tempGame.TypeIndex != newTypeIndex)
				{
					tempGame.TypeIndex = newTypeIndex;

					EditorUtility.SetDirty(Instance);
				}

				int newGenreIndex = EditorGUILayout.Popup("Genre", tempGame.GenreIndex, Instance.GameGenres);

				if (tempGame.GenreIndex != newGenreIndex)
				{
					tempGame.GenreIndex = newGenreIndex;

					EditorUtility.SetDirty(Instance);
				}

				Sprite newIcon = EditorGUILayout.ObjectField("Icon", tempGame.Icon, typeof(Sprite), false) as Sprite;

				if (tempGame.Icon != newIcon)
				{
					tempGame.Icon = newIcon;

					EditorUtility.SetDirty(Instance);
				}

				if (tempGame.TypeIndex > 0)
				{
					int newPrice = EditorGUILayout.IntField("Price", tempGame.Price);

					if (tempGame.Price != newPrice)
					{
						tempGame.Price = newPrice;

						EditorUtility.SetDirty(Instance);
					}
				}

				EditorGUILayout.BeginHorizontal();

				tempScene = EditorGUILayout.ObjectField("Scene", tempScene, typeof(Object), false);

				if (GUILayout.Button(EditorGUIUtility.IconContent("_Popup"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					EditorApplication.ExecuteMenuItem("File/Build Settings...");

				EditorGUILayout.EndHorizontal();

				if (tempScene != null)
				{
					string newScenePath = AssetDatabase.GetAssetPath(tempScene);

					if (!newScenePath.ToLower().EndsWith(".unity"))
					{
						tempScene = null;

						EditorUtility.DisplayDialog("Games Manager", "Please assign a valid scene", "Okay");

						return;
					}

					Scene scene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.AdditiveWithoutLoading);
					bool isActiveScene = SceneManager.GetActiveScene().buildIndex == scene.buildIndex;

					if (scene.buildIndex < 0)
					{
						EditorUtility.DisplayDialog("Games Manager", "The selected scene asset is not valid as it has not been assigned to the Build Settings scenes list", "Okay");

						tempScene = null;
					}
					else if (tempGame.scenePath != newScenePath)
					{
						tempGame.scenePath = newScenePath;

						EditorUtility.SetDirty(Instance);
					}

					if (!isActiveScene)
						EditorSceneManager.CloseScene(scene, true);
				}
				else if (!string.IsNullOrWhiteSpace(tempGame.scenePath))
					tempGame.scenePath = default;
			}

			if (tempItem == null)
				if (GUILayout.Button(modifyGameOrItem ? "Save Changes" : "Add Game"))
				{
					if (tempGame == null)
						tempGame = new Game(0);
					else
					{
						if (string.IsNullOrWhiteSpace(tempGame.scenePath))
						{
							EditorUtility.DisplayDialog("Items & Games Manager", "Please assign a scene before adding a new game", "Okay");

							return;
						}

						if (modifyGameOrItem)
						{
							if (ItemsAndGamesManager.ModifyGame(tempGame))
								ResetInspector();
						}
						else if (ItemsAndGamesManager.AddGame(tempGame))
							ResetInspector();
					}
				}

			if (tempGame == null && tempItem == null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Items", EditorStyles.miniBoldLabel);

				foreach (Item item in instance.Items)
				{
					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					EditorGUILayout.LabelField(item.name, EditorStyles.miniBoldLabel);

					if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						tempItem = item;
						modifyGameOrItem = true;
						tempItemIndex = instance.Items.IndexOf(item);
					}

					if (GUILayout.Button(EditorGUIUtility.IconContent("P4_DeletedLocal"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						ItemsAndGamesManager.RemoveItem(item);

						return;
					}

					EditorGUILayout.EndHorizontal();
				}
			}

			if (tempItem != null)
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.BeginHorizontal();

				tempItem.name = EditorGUILayout.TextField("Name", tempItem.name);

				if (modifyGameOrItem)
					EditorGUILayout.LabelField($"ID: {tempItem.ID}", EditorStyles.miniBoldLabel, GUILayout.Width(EditorStyles.miniBoldLabel.CalcSize(new GUIContent($"ID: {tempItem.ID}")).x));

				EditorGUI.indentLevel++;

				EditorGUILayout.EndHorizontal();

				int newTypeIndex = EditorGUILayout.Popup("Type", tempItem.TypeIndex, Instance.ItemTypes);

				if (tempItem.TypeIndex != newTypeIndex)
				{
					tempItem.TypeIndex = newTypeIndex;

					EditorUtility.SetDirty(Instance);
				}

				Sprite newSprite = EditorGUILayout.ObjectField("Sprite", tempItem.Sprite, typeof(Sprite), false) as Sprite;

				if (tempItem.Sprite != newSprite)
				{
					tempItem.Sprite = newSprite;

					EditorUtility.SetDirty(Instance);
				}

				Sprite newIcon = EditorGUILayout.ObjectField("Icon", tempItem.Icon, typeof(Sprite), false) as Sprite;

				if (tempItem.Icon != newIcon)
				{
					tempItem.Icon = newIcon;

					EditorUtility.SetDirty(Instance);
				}

				serializedObject.Update();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("items").GetArrayElementAtIndex(tempItemIndex).FindPropertyRelative("dependencies"), new GUIContent("Dependencies IDs"), true);
				serializedObject.ApplyModifiedProperties();

				if (tempItem.dependencies != null && tempItem.dependencies.Length > 0)
				{
					int newDependencyAlternative = EditorGUILayout.IntField("Alternative ID", tempItem.dependencyAlternative);

					if (tempItem.dependencyAlternative != newDependencyAlternative)
					{
						tempItem.dependencyAlternative = newDependencyAlternative;

						EditorUtility.SetDirty(Instance);
					}
				}

				int newPrice = EditorGUILayout.IntField("Price", tempItem.Price);

				if (tempItem.Price != newPrice)
				{
					tempItem.Price = newPrice;

					EditorUtility.SetDirty(Instance);
				}

				EditorGUI.indentLevel--;
			}

			if (tempGame == null)
				if (GUILayout.Button(modifyGameOrItem ? "Save Changes" : "Add Item"))
				{
					if (tempItem == null)
						tempItem = new Item(0);
					else
					{
						if (modifyGameOrItem)
						{
							if (ItemsAndGamesManager.ModifyItem(tempItem))
								ResetInspector();
						}
						else if (ItemsAndGamesManager.AddItem(tempItem))
							ResetInspector();
					}
				}

			if (tempGame != null || tempItem != null)
			{
				if (GUILayout.Button("Cancel"))
					ResetInspector();

				EditorGUILayout.EndVertical();
			}

			if (EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(Instance);
		}

		#endregion

		#region Static Methods

		[MenuItem("Tools/Items And Games Manager")]
		public static void SelectItemsAndGamesManager()
		{
			if (!ItemsAndGamesManager.Instance)
			{
				EditorUtility.DisplayDialog("Items & Games  Manager", "The items & games manager asset has been deleted or moved!", "Okay");

				return;
			}

			Selection.activeObject = ItemsAndGamesManager.Instance;
		}

		#endregion

		#region Global Methods

		private void ResetInspector()
		{
			tempGame = null;
			tempScene = null;
			tempItem = null;
			modifyGameOrItem = false;

			if (Instance)
				EditorUtility.SetDirty(Instance);
		}
		private void OnEnable()
		{
			instance = target as ItemsAndGamesManager;
			isReady = Instance.GameGenres != null && Instance.GameGenres.Length > 0 && Instance.GameTypes != null && Instance.GameTypes.Length > 0 && Instance.ItemTypes != null && Instance.ItemTypes.Length > 0;

			AssetDatabase.Refresh();
			ItemsAndGamesManager.Instance.ReloadGameGenres();
			ItemsAndGamesManager.Instance.ReloadGameTypes();
			ItemsAndGamesManager.Instance.ReloadItemTypes();
			ItemsAndGamesManager.Instance.ReloadGames();
			ItemsAndGamesManager.Instance.ReloadItems();
			ResetInspector();
			AssetDatabase.Refresh();
		}

		#endregion

		#endregion
	}
}
