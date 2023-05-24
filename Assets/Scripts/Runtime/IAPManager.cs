#region Namespaces

using System;
using UnityEngine;
using UnityEngine.Purchasing;
using GamesConverse.UI;

#endregion

namespace GamesConverse.IAP
{
	public class IAPManager : MonoBehaviour
	{
		#region Variables

		public IAPManager Instance
		{
			get
			{
				return instance;
			}
		}

		private IAPManager instance;

		#endregion

		#region Methods

		public void BuyCoins(Product product)
		{
			GameController.Instance.PlayHubClip(GameController.Instance.buyShopSounds[UnityEngine.Random.Range(0, GameController.Instance.buyShopSounds.Length)]);
			GameController.Instance.UserStatsDetails.NewAction(GameController.UserStats.ActionType.InAppPurchase, null, null, Convert.ToInt32(new JSONObject(product.metadata.localizedDescription)["amount"].str));
		}
		public void ShowPurchaseFailureDialog(Product _, PurchaseFailureReason reason)
		{
			UIController.Instance.ShowDialog("Payment Failed", $"The payment process was unsuccessful!\r\nReason: {reason}", "Okay");
		}
		private void OnEnable()
		{
			instance = this;
		}

		#endregion
	}
}
