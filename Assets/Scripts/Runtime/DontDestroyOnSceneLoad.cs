#region Namespaces

using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

#endregion

namespace GamesConverse
{
	[DefaultExecutionOrder(-1)]
	[DisallowMultipleComponent]
	public class DontDestroyOnSceneLoad : MonoBehaviour
	{
		#region Variables

		public string ID;
		public List<GameObject> linkedGameObjects;

		#endregion

		#region Methods

		#region Awake & Destroy

		private void Awake()
		{
			DontDestroyOnSceneLoad[] behaviours = FindObjectsOfType<DontDestroyOnSceneLoad>();

			if (behaviours.Length > 0 && Array.Exists(behaviours, behaviour => behaviour.ID == ID && behaviour != this))
			{
				Utility.Destroy(true, gameObject);

				return;
			}

			RefreshLinkedObjects();
		}
		private void OnDestroy()
		{
			linkedGameObjects.ForEach(gameObject => Utility.Destroy(true, gameObject));
		}

		#endregion

		#region Utilities

		public void RefreshLinkedObjects()
		{
			DontDestroyOnLoad(gameObject);

			foreach (GameObject gameObject in linkedGameObjects)
				DontDestroyOnLoad(gameObject);
		}
		public void RandomizeID()
		{
			DontDestroyOnSceneLoad[] behaviours = FindObjectsOfType<DontDestroyOnSceneLoad>();

			ID = string.Empty;

			while (string.IsNullOrEmpty(ID) || Array.Exists(behaviours, behaviour => behaviour.ID == ID && behaviour != this))
				ID = Utility.RandomString(16, true, true, true, false);

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif
		}

		#endregion

		#endregion
	}
}
