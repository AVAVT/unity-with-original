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

      BlockchainCreator.Instance.CreateSession(BlockchainCreator.adminPrivKey);

      var adminProperties = await BlockchainCreator.Instance.GetLandOwnedByUser();
      Debug.Log("Admin property fetched. JSON string looks like this:");
      Debug.Log(adminProperties.ToString());
      Debug.Log("First item is:");
      Debug.Log(adminProperties["instances"][0]["name"]);

      var queryResult = await BlockchainCreator.Instance.blockchain.Query<JObject>(
      "marketplace.find_instance_by_id",
      ("structure", adminProperties["instances"][0]["_structure"]["name"].ToString()),
      ("id", Util.HexStringToBuffer(adminProperties["instances"][0]["id"].ToString()))
    );

      if (queryResult.control.Error) throw new Exception(queryResult.control.ErrorMessage);

      Debug.Log(queryResult.content.ToString());
    }
    catch (Exception e)
    {
      Debug.LogError(e);
    }
  }
}
