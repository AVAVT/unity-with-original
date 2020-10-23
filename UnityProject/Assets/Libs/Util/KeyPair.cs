using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
  public class KeyPair
  {
    public readonly byte[] PubKey;
    public readonly byte[] PrivKey;

    public KeyPair(string privateKey = null)
    {
      if (privateKey != null)
      {
        this.PrivKey = PostchainUtil.HexStringToBuffer(privateKey);
        this.PubKey = PostchainUtil.VerifyKeyPair(privateKey);
      }
      else
      {
        var keyPair = PostchainUtil.MakeKeyPair();
        this.PubKey = keyPair["pubKey"];
        this.PrivKey = keyPair["privKey"];
      }
    }

    public Dictionary<string, byte[]> MakeKeyPair()
    {
      return PostchainUtil.MakeKeyPair();
    }
  }
}