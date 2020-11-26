using System.Collections;
using System.Collections.Generic;
using Auth.Scripts;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ViewTradeWindow : MonoBehaviour
{
	public TextMeshProUGUI headerText;
	public TextMeshProUGUI offeredItemsText;
	public TextMeshProUGUI requestingItemsText;

	private TradeInfo curTradeOffer;

	//instance
	public static ViewTradeWindow instance;

	private void Awake() => instance = this;


	#region Trade UI

	public void SetTradeWindow(int tradeOfferIndex)
	{
		curTradeOffer = TradeOffers.instance.tradeOffers[tradeOfferIndex];

		Dictionary<string, int> offeredItemsCount = new Dictionary<string, int>();
		Dictionary<string, int> requestingItemsCount = new Dictionary<string, int>();

		headerText.text = TradeOffers.instance.tradeOfferInfo.playerDisplayNames[tradeOfferIndex] + " wants to trade...";

		foreach (string itemId in curTradeOffer.OfferedCatalogItemIds)
		{
			if (!offeredItemsCount.ContainsKey(itemId))
				offeredItemsCount.Add(itemId, 0);

			offeredItemsCount[itemId]++;
		}

		offeredItemsText.text = "";

		foreach (KeyValuePair<string, int> item in offeredItemsCount)
		{
			string itemName = Trade.instance.catalog.Find(y => y.ItemId == item.Key).DisplayName;

			offeredItemsText.text += string.Format("x{0} {1}\n", item.Value, itemName);
		}


		foreach (string itemId in curTradeOffer.RequestedCatalogItemIds)
		{
			if (!requestingItemsCount.ContainsKey(itemId))
				requestingItemsCount.Add(itemId, 0);

			requestingItemsCount[itemId]++;
		}

		requestingItemsText.text = "";

		foreach (KeyValuePair<string, int> item in requestingItemsCount)
		{
			string itemName = Trade.instance.catalog.Find(y => y.ItemId == item.Key).DisplayName;
			requestingItemsText.text += string.Format("x{0} {1}\n", item.Value, itemName);
		}
	}

	public void OnAcceptTradeButton()
	{
		List<string> inventoryItemsToSend = new List<string>();
		List<ItemInstance> tempInventory = Trade.instance.inventory;
		for (int x = 0; x < curTradeOffer.RequestedCatalogItemIds.Count; ++x)
		{
			ItemInstance item = tempInventory.Find(y => y.ItemId == curTradeOffer.RequestedCatalogItemIds[x]);
			if (item == null)
			{
				Trade.instance.SetDisplayText("You don't have the requested items in your inventory.");
				return;
			}

			inventoryItemsToSend.Add(item.ItemInstanceId);
			tempInventory.Remove(item);
		}

		AcceptTradeRequest acceptTradeRequest = new AcceptTradeRequest
		{
			TradeId = curTradeOffer.TradeId,
			OfferingPlayerId = curTradeOffer.OfferingPlayerId,
			AcceptedInventoryInstanceIds = inventoryItemsToSend
		};
		PlayFabClientAPI.AcceptTrade(acceptTradeRequest,
			result => RemoveTradeOwnerFromGroup(result.Trade.OfferingPlayerId),
			error => Trade.instance.SetDisplayText(error.ErrorMessage)
		);
	}

	void RemoveTradeOwnerFromGroup(string offeringPlayerId)
	{
		ExecuteCloudScriptRequest executeRequest = new ExecuteCloudScriptRequest
		{
			FunctionName = "AcceptTrade",
			FunctionParameter = new {tradeOwnerId = offeringPlayerId}
		};

		PlayFabClientAPI.ExecuteCloudScript(executeRequest,
			result =>
			{
				Trade.instance.onRefreshUI?.Invoke();
			},
			error => Debug.Log(error.ErrorMessage));

	}

	public void ResetUI()
	{
		headerText.text = "";
		offeredItemsText.text = "";
		requestingItemsText.text = "";
	}

	#endregion
}