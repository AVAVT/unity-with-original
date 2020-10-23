using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TMPro;
using System.Linq;

public class SampleSceneManager : MonoBehaviour
{
  const string URL_SCHEME = "unitysso";
  public string deeplinkURL;
  public TMP_Text infoText;
  public GameObject loginButton;
  public GameObject rawTxPanel;
  public TMP_InputField rawTxInput;
  public GameObject registerPanel;
  public TMP_InputField usernameInput;
  public GameObject loadingIcon;

  KeyPair tmpKeyPairForLogin;
  string accountId;

  const string testRawTx = "A582030A30820306A58202B6308202B2A1220420FBA9AD1F64838CCFEB29C92575A6FB389AFF5C392C79F82817ACE5C72D9F8149A582023C30820238A581D63081D3A21A0C186674332E6465765F72656769737465725F6163636F756E74A581B43081B1A581AE3081ABA2030C0153A5483046A2440C42303265353030663130643738656132346334326432653466626562663865353739346162663065666462333736366239353737363238663835616262636562316630A5563054A50C300AA2030C0141A2030C0154A2440C42303265353030663130643738656132346334326432653466626562663865353739346162663065666462333736366239353737363238663835616262636562316630A0020500A582015B30820157A2190C176674332E6164645F617574685F64657363726970746F72A582013830820134A2420C4031303832643838313166383130636436353566323132343031383435393563303261313164363861646631656666656532666530363633613632353363393138A2420C4031303832643838313166383130636436353566323132343031383435393563303261313164363861646631656666656532666530363633613632353363393138A581A93081A6A2030C0153A5483046A2440C42303230376464383666393930363038393638363638323235363537656161636330306666663065363435333436623838323036646233393233343539663539323931A551304FA5073005A2030C0154A2440C42303230376464383666393930363038393638363638323235363537656161636330306666663065363435333436623838323036646233393233343539663539323931A0020500A54C304AA123042102E500F10D78EA24C42D2E4FBEBF8E5794ABF0EFDB3766B9577628F85ABBCEB1F0A12304210207DD86F990608968668225657EAACC00FFF0E645346B88206DB3923459F59291A54A3048A1420440E1549F7E56330096AAFF0784F67C3F6FD034B0089D49BF65984DB9ECCA5BF5BC79A425287D784AF1CAED2C832E540AFDA9BF7DCF99A9B1C93D414FB9434CD89FA1020400";

  private void Awake()
  {
    Application.deepLinkActivated += OnDeepLinkActivated;
    if (!String.IsNullOrEmpty(Application.absoluteURL))
    {
      // Cold start and Application.absoluteURL not null so process Deep Link.
      OnDeepLinkActivated(Application.absoluteURL);
    }
    // Initialize DeepLink Manager global variable.
    else deeplinkURL = "[none]";
    DontDestroyOnLoad(gameObject);
  }

  void Start()
  {
    if (BlockchainCreator.Instance.IsInitialized) InitializeChainAndLogDemoData();
    else
    {
      loadingIcon.SetActive(true);
      BlockchainCreator.Instance.OnBlockchainInitialized += InitializeChainAndLogDemoData;
    }
  }

  async void InitializeChainAndLogDemoData()
  {
    try
    {
      var initializeResult = await BlockchainCreator.Instance.InitializeChain();
      if (initializeResult) Debug.Log("Chain Initialized");
    }
    // catch (Exception e)
    catch
    {
      Debug.LogWarning("Chain initialization failed. Maybe it's already initialized before?");
      // Debug.LogError(e);
    }

    try
    {
      BlockchainCreator.Instance.CreateSession(BlockchainCreator.adminPrivKey, new FlagsType[] { FlagsType.Account, FlagsType.Transfer });

      var adminProperties = await BlockchainCreator.Instance.GetPlotsOwnedByUser();
      Debug.Log("Admin property fetched. JSON string looks like this:");
      Debug.Log(adminProperties.ToString());

      var firstItem = adminProperties["instances"][0];
      Debug.Log("First item is:");
      Debug.Log(firstItem["name"]);
      Debug.Log($"Position: {firstItem["x_left"]}-{firstItem["y_bottom"]}");
      Debug.Log($"Width: {firstItem["width"]}");
      Debug.Log($"Height: {firstItem["height"]}");

      infoText.text = "Blockchain Initialized";
      loginButton.SetActive(true);
      loadingIcon.SetActive(false);
    }
    catch (Exception e)
    {
      Debug.LogError(e);
    }
  }

  private void OnDeepLinkActivated(string url)
  {
    // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
    deeplinkURL = url;

    if (url.Contains("success"))
    {
      string rawTx = url.Split(new string[] { "?rawTx=" }, StringSplitOptions.None)[1];
      Debug.Log(rawTx);
      FinalizeLogin(rawTx);
    }
    else
    {
      infoText.text = "Authorization Canceled on Vault";
    }
  }


  /*
   * The following function is a draft that mimic js ft3-lib SSO module
   */
  public void InitializeLogin()
  {
    tmpKeyPairForLogin = new KeyPair();

#if UNITY_EDITOR
    var successUrl = "https://avavt.github.io/unity-with-original";
    var cancelUrl = "https://avavt.github.io/unity-with-original";
#else
    var successUrl = URL_SCHEME + "://success";
    var cancelUrl = URL_SCHEME + "://cancel";
#endif

    Application.OpenURL($"https://dev.vault.chromia-development.com/?route=/authorize&dappId={BlockchainCreator.blockchainRID}&pubkey={PostchainUtil.ByteArrayToString(tmpKeyPairForLogin.PubKey)}&successAction={Uri.EscapeDataString(successUrl)}&cancelAction={Uri.EscapeDataString(cancelUrl)}&version=0.1");

#if UNITY_EDITOR
    infoText.text = "(Editor Only) paste rawTx after login:";
    loginButton.SetActive(false);
    rawTxPanel.SetActive(true);
#else
    infoText.text = "Waiting for Vault Authorization...";
    loginButton.SetActive(false);
    loadingIcon.SetActive(true);
#endif
  }

  public void FinalizeLoginFromEditor()
  {
    rawTxPanel.SetActive(false);
    FinalizeLogin(rawTxInput.text);
  }

  async void FinalizeLogin(string rawTx)
  {
    infoText.text = "Creating account...";
    loadingIcon.SetActive(true);
    Debug.Log(".1");

    var authDescriptor = new SingleSignatureAuthDescriptor(tmpKeyPairForLogin.PubKey, new FlagsType[] { FlagsType.Transfer });
    var user = new User(tmpKeyPairForLogin, authDescriptor);

    Debug.Log(".2");

    GTXValue value = Gtx.Deserialize(PostchainUtil.HexStringToBuffer(rawTx));
    var jobj = JArray.FromObject(value.ToObjectArray());
    Debug.Log(jobj);

    Debug.Log(".3");

    var transaction = new GTXClient(
      new RESTClient("http://localhost:7740/", BlockchainCreator.blockchainRID)
    ).NewTransaction(new byte[][] { value.Array[0].Array[2].Array[0].ByteArray, user.KeyPair.PubKey });

    Debug.Log(".4");

    foreach (var operation in value.Array[0].Array[1].Array)
    {
      Debug.Log(operation.Array[0].String);
      if (operation.Array[0].String == "ft3.dev_register_account")
      {
        var primaryAuthDescriptor = new SingleSignatureAuthDescriptor(PostchainUtil.HexStringToBuffer(operation.Array[1].Array[0].Array[1].Array[0].String), new FlagsType[] { FlagsType.Account, FlagsType.Transfer });
        transaction.AddOperation(operation.Array[0].String, new dynamic[] { primaryAuthDescriptor.ToGTV() });
      }
      else if (operation.Array[0].String == "ft3.add_auth_descriptor")
      {
        accountId = operation.Array[1].Array[0].String;
        var disposableAuthDescriptor = new SingleSignatureAuthDescriptor(PostchainUtil.HexStringToBuffer(operation.Array[1].Array[2].Array[1].Array[0].String), new FlagsType[] { FlagsType.Transfer });
        transaction.AddOperation(operation.Array[0].String,
            operation.Array[1].Array[0].String,
            operation.Array[1].Array[1].String,
            disposableAuthDescriptor.ToGTV()
          );
      }
    }

    Debug.Log(".5");

    transaction.GtxObject.AddSignature(
      value.Array[0].Array[2].Array[0].ByteArray,
      value.Array[1].Array[0].ByteArray
    );

    transaction.Sign(user.KeyPair.PrivKey, user.KeyPair.PubKey);
    Debug.Log(".6");
    try
    {
      await transaction.PostAndWaitConfirmation();

      if (accountId != "")
      {
        Debug.Log(".7");
        BlockchainCreator.Instance.CreateSession(PostchainUtil.ByteArrayToString(user.KeyPair.PrivKey), new FlagsType[] { FlagsType.Transfer });
        // BlockchainCreator.Instance.SetAccountById(accountId);
        Debug.Log(".8");
        var player = await BlockchainCreator.Instance.session.Query<JObject>("find_by_account_id", ("account_id", accountId));

        if (player.content == null)
        {
          infoText.text = "Account created. Please choose an username:";
          loadingIcon.SetActive(false);
          registerPanel.SetActive(true);
        }
        else
        {
          infoText.text = $"Welcome, {player.content["username"]}!";
          loadingIcon.SetActive(false);
        }
      }
      else throw new System.Exception("Invalid sso transaction");
    }
    catch (Exception e)
    {
      Debug.LogError(e);
    }
  }

  public async void RegisterUsername()
  {
    if (usernameInput.text.Trim().Length == 0) return;

    loadingIcon.SetActive(true);
    registerPanel.SetActive(false);

    var result = await BlockchainCreator.Instance.session.Call(new Operation(
      "create_player",
      new dynamic[]{
        usernameInput.text.Trim(),
        accountId,
        BlockchainCreator.Instance.session.User.AuthDescriptor.ID
      }
    ));

    if (result.Error)
    {
      Debug.LogError(result.ErrorMessage);
    }
    else
    {
      var player = await BlockchainCreator.Instance.session.Query<JObject>("find_by_account_id", ("account_id", accountId));

      if (player.content != null)
      {
        infoText.text = $"Welcome, {player.content["username"]}!";
        loadingIcon.SetActive(false);
      }
    }
  }
}