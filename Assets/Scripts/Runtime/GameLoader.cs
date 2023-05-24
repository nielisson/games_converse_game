#region Namespaces

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#endregion

namespace GamesConverse
{
	public class GameLoader : MonoBehaviour
	{
		#region Variables

		public RectTransform progressBarContainer;
		public Image progressBar;
		public string mainSceneName;
		public float waitBeforeLoading;

		#endregion

		#region Methods

		private void Start()
		{
			StartCoroutine(StartLoadingGame());
		}
		private IEnumerator StartLoadingGame()
		{
			progressBarContainer.gameObject.SetActive(false);

			yield return new WaitForSeconds(waitBeforeLoading);

			progressBar.fillAmount = 0f;

			progressBarContainer.gameObject.SetActive(true);

			yield return new WaitForEndOfFrame();

			AsyncOperation operation = SceneManager.LoadSceneAsync(mainSceneName);

			operation.allowSceneActivation = true;

			while (!operation.isDone)
			{
				progressBar.fillAmount = Mathf.Clamp01(operation.progress / .9f);

				yield return null;
			}
		}

		#endregion
	}
}
