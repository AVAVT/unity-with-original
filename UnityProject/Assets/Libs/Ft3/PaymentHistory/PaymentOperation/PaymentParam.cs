namespace Chromia.Postchain.Ft3
{
    public class PaymentParam
    {
        public readonly string ChainId;
        public readonly string AccountId;
        public readonly string AssetId;
        public readonly int Amount;

        public PaymentParam(string chainId, string accountId, string assetId, int amount)
        {
            this.ChainId = chainId;
            this.AccountId = accountId;
            this.AssetId = assetId;
            this.Amount = amount;
        }

        public bool IsChainId(string chainId)
        {
            return this.ChainId.ToUpper().Equals(chainId.ToUpper());
        }

        public bool IsAccountId(string accountId)
        {
            return this.AccountId.ToUpper().Equals(accountId.ToUpper());
        }

        public bool IsAssetId(string assetId)
        {
            return this.AssetId.ToUpper().Equals(assetId.ToUpper());
        }

        public static PaymentParam FromTransferParam(TransferParam param, string chainId)
        {
            return new PaymentParam(
                chainId,
                param.AccountId,
                param.AssetId,
                param.Amount
            );
        }
    }
}