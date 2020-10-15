namespace Chromia.Postchain.Ft3
{
    public class TransferParam
    {
        public readonly string AccountId;
        public readonly string AssetId;
        public readonly int Amount;

        public TransferParam(string accountId, string assetId, int amount)
        {
            this.AccountId = accountId;
            this.AssetId = assetId;
            this.Amount = amount;
        }

        public bool IsAccountId(string accountId)
        {
            return this.AccountId.ToUpper().Equals(accountId.ToUpper());
        }

        public bool IsAssetId(string assetId)
        {
            return this.AssetId.ToUpper().Equals(assetId.ToUpper());
        }
    }
}