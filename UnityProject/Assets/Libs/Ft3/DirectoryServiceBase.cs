using System.Collections.Generic;
using System.Linq;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
  public class DirectoryServiceBase : DirectoryService
  {
    private List<ChainConnectionInfo> _chainInfos;

    public DirectoryServiceBase(ChainConnectionInfo[] chainInfos)
    {
      this._chainInfos = chainInfos.ToList();
    }

    public ChainConnectionInfo GetChainConnectionInfo(byte[] id)
    {
      return this._chainInfos.Find(info => PostchainUtil.ByteArrayToString(info.ChainId).Equals(PostchainUtil.ByteArrayToString(id)));
    }
  }
}