using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chromia.Postchain.Client;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SampleSceneManager : MonoBehaviour
{
  async void Start()
  {
    do
    {
      await Task.Delay(1);
    } while (!BlockchainCreator.Instance.IsInitialized);

    try
    {
      var initializeResult = await BlockchainCreator.Instance.InitializeChain();
      if (initializeResult) Debug.Log("Chain Initialized");
    }
    catch (Exception e)
    {
      Debug.Log("Chain initialization failed. Maybe it's already initialized before?");
      Debug.LogError(e);
    }

    try
    {
      BlockchainCreator.Instance.CreateSession(BlockchainCreator.adminPrivKey);

      var adminProperties = await BlockchainCreator.Instance.GetPlotsOwnedByUser();
      Debug.Log("Admin property fetched. JSON string looks like this:");
      Debug.Log(adminProperties.ToString());

      var firstItem = adminProperties["instances"][0];
      Debug.Log("First item is:");
      Debug.Log(firstItem["name"]);
      Debug.Log($"Position: {firstItem["x_left"]}-{firstItem["y_bottom"]}");
      Debug.Log($"Width: {firstItem["width"]}");
      Debug.Log($"Height: {firstItem["height"]}");

    }
    catch (Exception e)
    {
      Debug.LogError(e);
    }
  }
}