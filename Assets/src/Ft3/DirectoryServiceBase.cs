using System.Collections.Generic;
using System.Linq;

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
            return this._chainInfos.Find(info => Util.ByteArrayToString(info.ChainId).Equals(Util.ByteArrayToString(id)));
        }
    }
}