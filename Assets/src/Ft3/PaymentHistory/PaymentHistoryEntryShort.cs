namespace Chromia.Postchain.Ft3
{
    public class PaymentHistoryEntryShort
    {
        public readonly bool IsInput;
        public readonly int Delta;
        public readonly string Asset;
        public readonly string AssetId;
        public readonly int EntryIndex;
        public readonly string Timestamp;
        public readonly string TransactionId;
        public readonly byte[] TransactionData;
        public readonly int BlockHeight;

        public PaymentHistoryEntryShort(bool isInput, int delta, string asset, 
                                        string assetId, int entryIndex, string timestamp, 
                                        string transactionId, byte[] transactionData, int blockheight)
        {
            this.IsInput = isInput;
            this.Delta = delta;
            this.Asset = asset;
            this.AssetId = assetId;
            this.EntryIndex = entryIndex;
            this.Timestamp = timestamp;
            this.TransactionId = transactionId;
            this.TransactionData = transactionData;
            this.BlockHeight = blockheight;
        }

        public static PaymentHistoryEntryShort From(dynamic responseEntry)
        {
            return new PaymentHistoryEntryShort(
                (int) responseEntry["is_input"] == 1,
                (int) responseEntry["delta"],
                (string) responseEntry["asset"],
                (string) responseEntry["asset_id"],
                (int) responseEntry["entry_index"],
                (string) responseEntry["timestamp"],
                (string) responseEntry["tx_rid"],
                Util.HexStringToBuffer((string) responseEntry["tx_data"]),
                (int) responseEntry["block_height"]
            );
        }
    }
}