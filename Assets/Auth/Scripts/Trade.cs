using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


	public class Trade : MonoBehaviour
	{
		public static Trade instance;
		public GameObject tradeCanvas;

		public TextMeshProUGUI inventoryText;
		public TextMeshProUGUI displayText;

		[HideInInspector] public List<ItemInstance> inventory;

		[HideInInspector] public List<CatalogItem> catalog;

		public UnityEvent onRefreshUI;

		private void Awake()
		{
			instance = this;
		}


		public void OnLoggedIn()
		{
			tradeCanvas.SetActive(true);

			onRefreshUI?.Invoke();
		}

		#region Trade UI

		public void GetInventory()
		{
			inventoryText.text = "";

			// Req to get player's inv
			var getInvRequest = new GetPlayerCombinedInfoRequest
			{
				PlayFabId = LoginRegister.instance.playFabId,
				InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
				{
					GetUserInventory = true
				}
			};

			PlayFabClientAPI.GetPlayerCombinedInfo(getInvRequest,
				result =>
				{
					inventory = result.InfoResultPayload.UserInventory;

					foreach (var item in inventory) inventoryText.text += item.DisplayName + ", ";

					Trade.instance.SetDisplayText("Set item");
				},
				error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
			);
		}

		public void GetCatalog()
		{
			// Trade.instance.SetDisplayText("Got Catalog");
			GetCatalogItemsRequest itemsRequest = new GetCatalogItemsRequest
			{
				CatalogVersion = "PlayerItems"
			};

			PlayFabClientAPI.GetCatalogItems(itemsRequest,
				result =>
				{
					//Trade.instance.SetDisplayText(result.ToJson());
					catalog = result.Catalog;
				},
				error => Trade.instance.SetDisplayText(error.ErrorMessage)
			);
			// Trade.instance.SetDisplayText("Got " + catalog.Count); //This serves no purpose as execution wraps back to GetCatalogItems
		}


		public void SetDisplayText(string text, bool isError = false)
		{
			displayText.text = text;

			displayText.color = isError ? Color.red : Color.green;
		}

		void HIdeDisplayText()
		{
			displayText.text = "";
		}

		#endregion
	}
