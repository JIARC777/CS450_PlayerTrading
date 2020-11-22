using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class TradeOffers : MonoBehaviour
{
    [System.Serializable]
    public class TradeOfferInfo
    {
        public List<string> playerIds;
        public List<string> playerDisplayNames;
        public List<string> tradeIds;
    }

    public Button[] tradeOfferButtons;
    public List<TradeInfo> tradeOffers;
    public TradeOfferInfo tradeOfferInfo;
    private int numTradeOffers;

    public static TradeOffers instance;
    private void Awake()
    {
        instance = this;
    }

    public void UpdateTradeOffers()
    {
        DisableAllTradeOfferButtons();

        ExecuteCloudScriptRequest getTradeOffersRequest = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTradeIDs"
        };

        Debug.Log(getTradeOffersRequest);
        PlayFabClientAPI.ExecuteCloudScript(getTradeOffersRequest,
            result =>
            {
                Debug.Log(result.FunctionResult);
                string rawData = result.FunctionResult.ToString();
                tradeOfferInfo = JsonUtility.FromJson<TradeOfferInfo>(rawData);
                GetTradeInfo();
            },
            error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        ); 
    }

    void DisableAllTradeOfferButtons()
    {
        foreach (Button button in  tradeOfferButtons)
            button.gameObject.SetActive(false);
    }

    void GetTradeInfo()
    {
        numTradeOffers = tradeOfferInfo.playerIds.Count;
        Debug.Log(numTradeOffers);
        tradeOffers = new List<TradeInfo>();
        if (numTradeOffers == 0)
            UpdateTradeOffersUI();
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
                error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
            ); 
        }
    }

    void UpdateTradeOffersUI()
    {
        Debug.Log(tradeOfferButtons.Length);
        for (int x = 0; x < tradeOfferButtons.Length; ++x)
        {
            tradeOfferButtons[x].gameObject.SetActive(x < tradeOffers.Count);
            if (!tradeOfferButtons[x].gameObject.activeInHierarchy) continue;

            tradeOfferButtons[x].onClick.RemoveAllListeners();
            Debug.Log(tradeOfferInfo.playerDisplayNames[x]);
            int tradeIndex = x;
            tradeOfferButtons[x].onClick.AddListener(() => OnTradeOfferButton(tradeIndex));
            tradeOfferButtons[x].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tradeOfferInfo.playerDisplayNames[x];
            Debug.Log(x);
        }
    }

    public void OnTradeOfferButton (int tradeIndex)
    {
        ViewTradeWindow.instance.SetTradeWindow(tradeIndex);

        
    }
}
