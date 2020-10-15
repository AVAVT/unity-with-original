using Chromia.Postchain.Ft3;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BlockchainCreator
{

  private static BlockchainCreator _instance;

  public static BlockchainCreator Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = new BlockchainCreator();
      }

      return _instance;
    }
  }
  const string blockchainRID = "FE76BCCC0B059F2249450597BF81CF5A8E03BA229E33A1B418C2BAC82A422EE9";
  const string blockchainUrl = "http://localhost:7740/";
  public const string adminPrivKey = "3B8A4A224DC5A1C56B8B8C39A6FD5461BA4C3579506B3E85A6163350FD5E00CA";

  public Blockchain blockchain { get; private set; }
  public bool IsInitialized { get; private set; }
  public KeyPair keyPair { get; private set; }
  public BlockchainSession session { get; private set; }

  BlockchainCreator()
  {
    IsInitialized = false;
    InitializeBlockchain();
  }

  async void InitializeBlockchain()
  {
    byte[] blockChainRIDBuffer = Util.HexStringToBuffer(blockchainRID);
    blockchain = await Blockchain.Initialize(
      blockChainRIDBuffer,
      new DirectoryServiceBase(
        new ChainConnectionInfo[] { new ChainConnectionInfo(blockChainRIDBuffer, blockchainUrl) }
      )
    );

    IsInitialized = true;
  }

  public async Task<bool> InitializeChain()
  {
    if (!IsInitialized) return false;

    var adminKeyPair = new KeyPair(adminPrivKey);
    var authDescriptor = new SingleSignatureAuthDescriptor(adminKeyPair.PubKey, new FlagsType[] { FlagsType.Account, FlagsType.Transfer });
    var admin = new User(adminKeyPair, authDescriptor);
    try
    {
      // Setup tokens and structures
      var result = await blockchain.Call(new Operation("initialize_chain", new dynamic[] { }), admin);
      if (result.Error) throw new Exception(result.ErrorMessage);

      // Create Honeydew Valley plot structure
      result = await blockchain.Call(new Operation("marketplace.def_original", new dynamic[] {
        "Honeydew Valley",
        new dynamic[]{"IPlot"},
        new dynamic[]{},
        new dynamic[]{
          new dynamic[]{"name", "Honeydew Valley"},
          new dynamic[]{
            "image",
            new Dictionary<string, object>(){
              {"type", "base64"},
              {"data", "PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0NTAiIGhlaWdodD0iNDAwIiB2aWV3Qm94PSIwIDAgNDUwIDQwMCIgZmlsbD0ibm9uZSI"}
            }
          },
          new dynamic[]{ "x_left", 0 },
          new dynamic[]{ "y_bottom", 0 },
          new dynamic[]{ "width", 15 },
          new dynamic[]{ "height", 10 },
          new dynamic[]{ "description", "Best place to stay" },
          new dynamic[]{ "designer_name", "Admin" }
        },
        authDescriptor.ID,
        authDescriptor.ID
      }), admin);
      if (result.Error) throw new Exception(result.ErrorMessage);

      var tokenQueryResult = await blockchain.Query<Asset[]>("ft3.get_asset_by_name", ("name", "WONDER TOKEN"));

      // Create one instance of the plot structure
      result = await blockchain.Call(new Operation("marketplace.new_instance", new dynamic[] {
        "Honeydew Valley",
        new dynamic[]{
          new dynamic[]{"id", Util.HexStringToBuffer(System.Guid.NewGuid().ToString().Replace("-", ""))},
          new dynamic[]{"asset_id", tokenQueryResult.content[0].GetId()},
          new dynamic[]{"price", 40},
          new dynamic[]{"is_listed", 1}
        },
        authDescriptor.ID
      }), admin);

      if (result.Error) throw new Exception(result.ErrorMessage);

      return true;
    }
    catch (Exception e)
    {
      throw e;
    }
  }

  public async Task<JObject> GetLandOwnedByUser()
  {
    if (!IsInitialized || session == null) throw new Exception("Blockchain is not initialized!");

    var queryResult = await blockchain.Query<JObject>(
      "marketplace.find_original_instances_by_owner_id",
      ("owner_id", session.User.AuthDescriptor.ID),
      ("after_rowid", 0),
      ("page_size", 40)
    );

    if (queryResult.control.Error) throw new Exception(queryResult.control.ErrorMessage);

    return queryResult.content;
  }

  public bool CreateSession(string privKey)
  {
    if (!IsInitialized) return false;

    keyPair = new KeyPair(privKey);

    var authDescriptor = new SingleSignatureAuthDescriptor(keyPair.PubKey, new FlagsType[] { FlagsType.Account, FlagsType.Transfer });
    var user = new User(keyPair, authDescriptor);

    session = new BlockchainSession(user, blockchain);

    return true;
  }
}