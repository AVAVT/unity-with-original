namespace Chromia.Postchain.Ft3
{
    public class ChainConnectionInfo
    {
        public readonly string Url;
        public readonly byte[] ChainId;

        public ChainConnectionInfo(byte[] chainId, string url)
        {
            this.ChainId = chainId;
            this.Url = url;
        }
    }
    
}