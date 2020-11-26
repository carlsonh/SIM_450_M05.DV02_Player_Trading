using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


namespace Auth.Scripts
{
	public class CreateTrade : MonoBehaviour
	{
		public TradeItem[] offeringItems;
		public TradeItem[] requestingItems;

		public static CreateTrade instance;
		//instance
		void Awake() => instance = this;

		public void OnCreateTradeButton()
		{
			List<ItemInstance> tempInventory = Trade.instance.inventory;
			List<String> itemsToOffer = new List<string>();

			foreach (TradeItem item in offeringItems)
			{
				for (int x = 0; x < item.value; ++x)
				{
					ItemInstance i = tempInventory.Find(playfabItem => playfabItem.DisplayName == item.itemName);

					if (i == null)
					{
						Trade.instance.SetDisplayText("You don't have the offered items.");
						return;
					}
					else
					{
						itemsToOffer.Add(i.ItemInstanceId);
						tempInventory.Remove(i);
					}
				}
			}

			if (itemsToOffer.Count == 0)
			{
				Trade.instance.SetDisplayText("You can't trade nothing");
				return;
			}
			
			// get the requested items
			List<string> itemsToRequest = new List<string>();
			
			foreach(TradeItem item in requestingItems)
			{
				string itemId = Trade.instance.catalog.Find(y => y.DisplayName == item.itemName).ItemId;
				for(int x = 0; x < item.value; ++x)
					itemsToRequest.Add(itemId);
			}

			OpenTradeRequest tradeRequest = new OpenTradeRequest
			{
				OfferedInventoryInstanceIds = itemsToOffer,
				RequestedCatalogItemIds = itemsToRequest
			};
			
			PlayFabClientAPI.OpenTrade(tradeRequest,
				result =>
				{
					Trade.instance.SetDisplayText("Trade created");
					AddTradeToGroup(result.Trade.TradeId);
				},
				error => Trade.instance.SetDisplayText(error.ErrorMessage, true));
		}

		void AddTradeToGroup(string tradeId)
		{
			ExecuteCloudScriptRequest executeRequest = new ExecuteCloudScriptRequest
			{
				FunctionName = "AddNewTradeOffer",
				FunctionParameter = new {tradeID = tradeId}
			};
			
			PlayFabClientAPI.ExecuteCloudScript(executeRequest,
				result =>
				{
					Trade.instance.SetDisplayText("Trade Off Created.");
					Trade.instance.onRefreshUI?.Invoke();
				},
				error => Trade.instance.SetDisplayText(error.ErrorMessage)
			);
		}

		public void ResetItemValues()
		{
			foreach (TradeItem item in offeringItems)
			{
				item.ResetValue();
			}

			foreach (TradeItem item in requestingItems)
			{
				item.ResetValue();
			}
		}
	}
}