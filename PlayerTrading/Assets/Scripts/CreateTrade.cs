﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class CreateTrade : MonoBehaviour
{
    public TradeItem[] offeringItems;
    public TradeItem[] requestingItems;

    //instance
    public static CreateTrade instance;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCreateTradeButton()
    {
        List<ItemInstance> tempInventory = Trade.instance.inventory;
        List<string> itemsToOffer = new List<string>();

        foreach (TradeItem item in offeringItems)
        {
            for (int x = 0; x < item.value; ++x)
            {
                ItemInstance i = tempInventory.Find(y => y.DisplayName == item.itemName);
                if (i == null)
                {
                    Debug.Log("You don't have the offered items in your inventory.");
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
            Debug.Log("You can't trade nothing.");
            return;
        }
        List<string> itemsToRequest = new List<string>();
        foreach (TradeItem item in requestingItems)
        {
            string itemId = ""; // Trade.instance.catalog.Find(y = y.DisplayName == item.itemName.ToString);

            for (int x = 0; x < item.value; ++x)
                itemsToRequest.Add(itemId);
        }

        OpenTradeRequest tradeRequest = new OpenTradeRequest
        {
            OfferedInventoryInstanceIds = itemsToOffer,
            RequestedCatalogItemIds = itemsToRequest
        };

        PlayFabClientAPI.OpenTrade(tradeRequest,
            result => AddTradeToGroup(result.Trade.TradeId),
            error => Debug.Log(error.ErrorMessage)
        );
    }

    void AddTradeToGroup(string tradeId)
    {
        ExecuteCloudScriptRequest executeRequest = new ExecuteCloudScriptRequest
        {
            FunctionName = "AddNewTradeOffer",
            FunctionParameter = new { tradeID = tradeId }
        };

        PlayFabClientAPI.ExecuteCloudScript(executeRequest,
            result =>
            {
                Debug.Log("Trade offer created.");

                if (Trade.instance.onRefreshUI != null)
                    Trade.instance.onRefreshUI.Invoke();
            },
            error => Debug.Log(error.ErrorMessage)
        );
    }

    public void ResetItemValues()
    {
        foreach (TradeItem item in offeringItems)
            item.ResetValue();
        foreach (TradeItem item in requestingItems)
            item.ResetValue();
    }
}
