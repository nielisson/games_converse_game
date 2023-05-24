#region Namespaces

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#endregion

namespace GamesConverse.Notifications
{
	public class NotificationsManager : MonoBehaviour
	{
		#region Modules

		[Serializable]
		public struct Notification
		{
			public string title;
			public string message;
			public string smallIcon;
			public string largeIcon;
		}

		#endregion

		#region Variables

		#region Static Variables

#if UNITY_ANDROID
		private static AndroidNotificationChannel channel;
#endif

		#endregion

		#region Global Variables

		[SerializeField, Header("Daily")]
		private bool dailyEnabled;
		[SerializeField]
		private Notification dailyNotification;
		[SerializeField, Header("After 3 Days")]
		private bool threeDaysEnabled;
		[SerializeField]
		private Notification threeDaysNotification;
		[SerializeField, Header("Weekly")]
		private bool weeklyEnabled;
		[SerializeField]
		private Notification weeklyNotification;

		#endregion

		#endregion

		#region Methods

		#region Static Methods

		public static int ScheduleNotification(Notification notification, DateTime fireTime)
		{
			return ScheduleNotification(notification.title, notification.message, notification.smallIcon, notification.largeIcon, fireTime);
		}
		public static int ScheduleNotification(string title, string message, string smallIcon, string largeIcon, DateTime fireTime)
		{
			if (GameController.Instance.Settings && !GameController.Instance.Settings.pushNotificationsOn)
				return -1;

#if UNITY_ANDROID
			AndroidNotification notification = new AndroidNotification()
			{
				Title = title,
				Text = message,
				SmallIcon = smallIcon,
				LargeIcon = largeIcon,
				FireTime = fireTime
			};

#if UNITY_EDITOR
			AndroidNotificationCenter.OnNotificationReceived += delegate (AndroidNotificationIntentData data)
			{
				Debug.Log($"Notification received: {data.Id}\r\n" +
							$"\tTitle: {data.Notification.Title}\r\n" +
							$"\tBody: {data.Notification.Text}\r\n" +
							$"\tChannel: {data.Channel}\r\n");
			};
#endif

			int notificationID = AndroidNotificationCenter.SendNotification(notification, channel.Id);

			if (fireTime != DateTime.Now)
			{
				string scheduledNotificationsKey = "GC_SCHEDULED_NOTIFICATIONS";
				List<int> scheduledNotifications = PlayerPrefs.HasKey(scheduledNotificationsKey) ? JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString(scheduledNotificationsKey, "[]")) : new List<int>();

				scheduledNotifications.Add(notificationID);
				PlayerPrefs.SetString(scheduledNotificationsKey, JsonUtility.ToJson(scheduledNotifications));
			}

			return notificationID;
#else
			return -1;
#endif
		}

		#endregion

		#region Global Methods

		private void Start()
		{
			Initialize();

			if (dailyEnabled)
				ScheduleNotification(dailyNotification, DateTime.Now.AddDays(1));

			if (threeDaysEnabled)
				ScheduleNotification(threeDaysNotification, DateTime.Now.AddDays(3));

			if (weeklyEnabled)
				ScheduleNotification(weeklyNotification, DateTime.Now.AddDays(7));
		}
		private void Initialize()
		{
#if UNITY_ANDROID
			channel = new AndroidNotificationChannel()
			{
				Id = "gc_default_channel",
				Name = "Games Converse Default Channel",
				Description = "For Generic notifications",
				Importance = Importance.Default
			};

			AndroidNotificationCenter.RegisterNotificationChannel(channel);

			string scheduledNotificationsKey = "GC_SCHEDULED_NOTIFICATIONS";

			if (PlayerPrefs.HasKey(scheduledNotificationsKey))
			{
				List<int> scheduledNotifications = JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString(scheduledNotificationsKey, "[]"));

				foreach (int id in scheduledNotifications)
					AndroidNotificationCenter.CancelNotification(id);
			}
#endif
		}

		#endregion

		#endregion
	}
}
