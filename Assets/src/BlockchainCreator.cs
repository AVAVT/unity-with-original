using Chromia.Postchain.Ft3;
using System;

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
  const string blockchainRID = "<YOUR CHAIN BRID>";
  const string blockchainUrl = "http://localhost:7743/";

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
    blockchain = await Blockchain.Initialize(blockChainRIDBuffer, new DirectoryServiceBase(new ChainConnectionInfo[] { }));

    IsInitialized = true;
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