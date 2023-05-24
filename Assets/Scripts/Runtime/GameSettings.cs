#region Namespaces

using System;
using System.IO;
using System.Collections;
using UnityEngine;
using Utilities;

#endregion

namespace GamesConverse
{
	[Serializable]
	public class GameSettings
	{
		#region Enumerators

		public enum Language { English, French, Portuguese }

		#endregion

		#region Variables

		public float volume;
		public bool hubSoundsOn;
		public bool pushNotificationsOn;
		public Language language;

		#endregion

		#region Methods

		#region Static Methods

		public static GameSettings Load()
		{
			return new DataSerializationUtility<GameSettings>(Path.Combine(Application.persistentDataPath, "GameSettings.bytes"), false, true).Load();
		}

		#endregion

		#region Global Methods

		public void Save()
		{
			GameController.Instance.StartCoroutine(StartSave());
		}

		#endregion

		#endregion

		#region Coroutines

		internal IEnumerator StartSave()
		{
			if (!new DataSerializationUtility<GameSettings>(Path.Combine(Application.persistentDataPath, "GameSettings.bytes"), false, false).SaveOrCreate(this))
				Debug.LogWarning("Could not save game settings!");

			yield return null;
		}

		#endregion

		#region Constructors & Operators

		#region Constructors

		public GameSettings()
		{
			volume = 1f;
			hubSoundsOn = true;
			pushNotificationsOn = true;
			language = default;
		}

		#endregion

		#region Operators

		public static implicit operator bool(GameSettings settings) => settings != null;

		#endregion

		#endregion
	}
}
