using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeOffers : MonoBehaviour
{
	public Button[] tradeOfferButtons;
	public List<TradeInfo> tradeOffers;
	public TradeOfferInfo tradeOfferInfo;
	private int numTradeOffers;

	//instance
	public static TradeOffers instance;

	private void Awake()
	{
		instance = this;
	}


	#region Trade System

	public void UpdateTradeOffers()
	{
		
		DisableAllTradeOfferButtons();

		ExecuteCloudScriptRequest getTradeOffersRequest = new ExecuteCloudScriptRequest
		{
			FunctionName = "GetTradeIDs"
		};
		PlayFabClientAPI.ExecuteCloudScript(getTradeOffersRequest,
			result =>
			{
				// Debug.Log("Resulted" + result);
				string rawData = result.FunctionResult.ToString();//Not receiving correct data
				tradeOfferInfo = JsonUtility.FromJson<TradeOfferInfo>(rawData);
				GetTradeInfo();
			},
			error => Debug.Log(error.ErrorMessage)
		);
	}

	void DisableAllTradeOfferButtons()
	{
		foreach (Button button in tradeOfferButtons)
		{
			button.gameObject.SetActive(false);
		}
	}

	void GetTradeInfo()
	{
		numTradeOffers = tradeOfferInfo.playerIds.Count;
		tradeOffers = new List<TradeInfo>();

		if (numTradeOffers == 0)
		{
			UpdateTradeOffersUI();
		}

		for (int x = 0; x < tradeOfferInfo.playerIds.Count; ++x)
		{
			GetTradeStatusRequest tradeStatusRequest = new GetTradeStatusRequest
			{
				OfferingPlayerId = tradeOfferInfo.playerIds[x],
				TradeId = tradeOfferInfo.tradeIds[x]
			};

			PlayFabClientAPI.GetTradeStatus(tradeStatusRequest,
				result =>
				{
					tradeOffers.Add(result.Trade);
					if (tradeOffers.Count == numTradeOffers)
						UpdateTradeOffersUI();
				},
				error => Trade.instance.SetDisplayText(error.ErrorMessage)
			);
		}
	}

	void UpdateTradeOffersUI()
	{
		Debug.Log("Updating offers");
		for (int x = 0; x < tradeOfferButtons.Length; ++x)
		{
			tradeOfferButtons[x].gameObject.SetActive(x < tradeOffers.Count);
			if (!tradeOfferButtons[x].gameObject.activeInHierarchy) continue;

			tradeOfferButtons[x].onClick.RemoveAllListeners();

			int tradeIndex = x;

			tradeOfferButtons[x].onClick.AddListener(() => OnTradeOfferButton(tradeIndex));
			tradeOfferButtons[x].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tradeOfferInfo.playerDisplayNames[x];
		}
	}

	public void OnTradeOfferButton(int tradeIndex)
	{
		ViewTradeWindow.instance.SetTradeWindow(tradeIndex);
	}

	#endregion Trade System
}


[System.Serializable]
public class TradeOfferInfo
{
	public List<string> playerIds;
	public List<string> playerDisplayNames;
	public List<string> tradeIds;
}