#region Namespaces

using UnityEngine;
using UnityEditor;
using GamesConverse;

#endregion

namespace GamesConverseEditor
{
	[CustomEditor(typeof(DontDestroyOnSceneLoad))]
	public class DontDestroyOnSceneLoadEditor : Editor
	{
		#region Variables

		public DontDestroyOnSceneLoad Instance
		{
			get
			{
				if (!instance)
					instance = target as DontDestroyOnSceneLoad;

				return instance;
			}
		}

		private DontDestroyOnSceneLoad instance;

		#endregion

		#region Methods

		#region Virtual Methods

		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("ID", Instance.ID, EditorStyles.miniBoldLabel);
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("linkedGameObjects"), new GUIContent("Linked GameObjects"));
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();

			if (GUILayout.Button("Randomize ID"))
				Instance.RandomizeID();

			EditorGUILayout.Space();
		}

		#endregion

		#region Global Methods

		private void OnEnable()
		{
			if (string.IsNullOrEmpty(Instance.ID))
				Instance.RandomizeID();
		}

		#endregion

		#endregion
	}
}
