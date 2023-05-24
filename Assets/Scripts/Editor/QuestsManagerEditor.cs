#region Namespaces

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GamesConverse;

#endregion

namespace GamesConverseEditor
{
	[CustomEditor(typeof(QuestsManager))]
	public class QuestsManagerEditor : Editor
	{
		#region Variables

		public QuestsManager Instance
		{
			get
			{
				return instance;
			}
		}

		private QuestsManager instance;
		private Quest tempQuest;
		private bool modifyQuest;
		private bool isReady;

		#endregion

		#region Methods

		#region Virtual Methods

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Quests Manager", EditorStyles.boldLabel);

			if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
			{
				QuestsManager.Instance.ReloadQuests();
				ResetInspector();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (!isReady)
			{
				EditorGUILayout.HelpBox("There seem to be some problems while getting data from the connected server. Please check your console for more information...", MessageType.Error);

				if (GUILayout.Button("Retry"))
					OnEnable();

				return;
			}

			if (tempQuest == null)
			{
				List<Quest> quests = instance.Quests.ToList();

				quests.Reverse();

				foreach (Quest quest in quests)
				{
					EditorGUILayout.BeginHorizontal(GUI.skin.box);
					EditorGUILayout.LabelField($"{quest.name} - {quest.period}", EditorStyles.miniBoldLabel);

					if (GUILayout.Button(EditorGUIUtility.IconContent("d_editicon.sml"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						tempQuest = quest;
						modifyQuest = true;
					}

					if (GUILayout.Button(EditorGUIUtility.IconContent("P4_DeletedLocal"), new GUIStyle(EditorStyles.miniButton) { stretchWidth = false }))
					{
						QuestsManager.RemoveQuest(quest);

						return;
					}

					EditorGUILayout.EndHorizontal();
				}
			}
			else
			{
				EditorGUILayout.BeginVertical(GUI.skin.box);
				EditorGUILayout.BeginHorizontal();

				tempQuest.name = EditorGUILayout.TextField("Name", tempQuest.name);

				if (modifyQuest)
					EditorGUILayout.LabelField($"ID: {tempQuest.ID}", EditorStyles.miniBoldLabel, GUILayout.Width(EditorStyles.miniBoldLabel.CalcSize(new GUIContent($"ID: {tempQuest.ID}")).x));

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.LabelField("Description");

				EditorGUI.indentLevel++;

				tempQuest.description = EditorGUILayout.TextArea(tempQuest.description, new GUIStyle(EditorStyles.textArea), GUILayout.Height(100f));

				EditorGUI.indentLevel--;

				tempQuest.period = (Quest.Period)EditorGUILayout.EnumPopup("Period", tempQuest.period);

				EditorGUI.indentLevel++;

				tempQuest.startAt = EditorGUILayout.TextField("Start At Time", tempQuest.startAt);
				tempQuest.endAt = EditorGUILayout.TextField("End At Time", tempQuest.endAt);

				EditorGUI.indentLevel--;

				tempQuest.target = (Quest.Target)EditorGUILayout.EnumPopup("Target", tempQuest.target);

				EditorGUI.indentLevel++;

				if (tempQuest.target != Quest.Target.RateApp && tempQuest.target != Quest.Target.ShareApp && tempQuest.target != Quest.Target.PlayGame)
					tempQuest.targetValue = Mathf.Max(EditorGUILayout.IntField("Value", tempQuest.targetValue), 1);

				if (tempQuest.target != Quest.Target.RateApp && tempQuest.target != Quest.Target.ShareApp)
				{
					List<Game> games = ItemsAndGamesManager.Instance.Games;
					string[] gamesNames = games.Select(game => game.name).ToArray();
					int targetGameIndex = games.FindIndex(game => game.ID == tempQuest.targetGameID) + 1;

					ArrayUtility.Insert(ref gamesNames, 0, "None");

					int newTargetGameIndex = EditorGUILayout.Popup("Game", targetGameIndex, gamesNames);

					if (targetGameIndex != newTargetGameIndex)
						tempQuest.targetGameID = newTargetGameIndex > 0 ? games[newTargetGameIndex - 1].ID : default;
				}

				if (tempQuest.target == Quest.Target.ShareApp)
					tempQuest.targetSocialMedia = (Quest.TargetSocialMedia)EditorGUILayout.EnumPopup("Social Media", tempQuest.targetSocialMedia);

				EditorGUI.indentLevel--;

				tempQuest.reward = (Quest.Reward)EditorGUILayout.EnumPopup("Reward", tempQuest.reward);

				EditorGUI.indentLevel++;

				if (tempQuest.reward == Quest.Reward.Item)
				{
					List<Item> items = ItemsAndGamesManager.Instance.Items;
					string[] itemsNames = items.Select(item => item.name).ToArray();
					int rewardItemIndex = items.FindIndex(item => item.ID == tempQuest.rewardItemID) + 1;

					ArrayUtility.Insert(ref itemsNames, 0, "None");

					int newRewardItemIndex = EditorGUILayout.Popup("Item", rewardItemIndex, itemsNames);

					if (rewardItemIndex != newRewardItemIndex)
						tempQuest.rewardItemID = newRewardItemIndex > 0 ? items[newRewardItemIndex - 1].ID : default;
				}
				else
					tempQuest.rewardAmount = Mathf.Max(EditorGUILayout.IntField("Amount", tempQuest.rewardAmount), 0);

				EditorGUI.indentLevel--;
			}

			if (GUILayout.Button(modifyQuest ? "Save Changes" : "Add Quest"))
			{
				if (tempQuest == null)
					tempQuest = new Quest(0);
				else
				{
					if (modifyQuest)
					{
						if (QuestsManager.ModifyQuest(tempQuest))
							ResetInspector();
					}
					else if (QuestsManager.AddQuest(tempQuest))
						ResetInspector();
				}
			}

			if (tempQuest != null)
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

		[MenuItem("Tools/Quests Manager")]
		public static void SelectQuestsManager()
		{
			if (!QuestsManager.Instance)
			{
				EditorUtility.DisplayDialog("Quests  Manager", "The quests manager asset has been deleted or moved!", "Okay");

				return;
			}

			Selection.activeObject = QuestsManager.Instance;
		}

		#endregion

		#region Global Methods

		private void ResetInspector()
		{
			tempQuest = null;
			modifyQuest = false;

			if (Instance)
				EditorUtility.SetDirty(Instance);
		}
		private void OnEnable()
		{
			instance = target as QuestsManager;
			isReady = Instance.Quests != null;

			AssetDatabase.Refresh();
			QuestsManager.Instance.ReloadQuests();
			ResetInspector();
			AssetDatabase.Refresh();
		}

		#endregion

		#endregion
	}
}
