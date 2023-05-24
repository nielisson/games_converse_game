#region Namespaces

using UnityEngine;
using UnityEngine.UI;

#endregion

namespace GamesConverse
{
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(300)]
	[RequireComponent(typeof(Button))]
	public class BackToHubButton : MonoBehaviour
	{
		private void Start()
		{
			Button button = GetComponent<Button>();

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() =>
			{
				GameController.Instance.PlayHubClip(GameController.Instance.clickSound);

				if (!GameController.Instance)
				{
					Debug.LogWarning("Could not go back to Main Hub Menu!\r\nPlease launch the game from the hub in order to use this button.");

					return;
				}

				Time.timeScale = 1f;

				GameController.LoadScene(GameController.Instance.mainSceneName);
			});
		}
	}
}
