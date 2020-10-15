using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    public class PaymentHistoryEntry
    {
        public readonly bool IsInput;
        public readonly int Delta;
        public readonly string Asset;
        public readonly byte[] AssetId;
        public readonly byte[] ChainId;
        public readonly dynamic[] Other;
        public readonly string Timestamp;
        public readonly byte[] TransactionId;
        public readonly int BlockHeight;

        public PaymentHistoryEntry(bool isInput, int delta, string asset,
                                    byte[] assetId, dynamic[] other,
                                    string timestamp, byte[] transactionId, int blockHeight)
        {
            this.IsInput = isInput;
            this.Delta = delta;
            this.Asset = asset;
            this.AssetId = assetId;
            this.Other = other;
            this.Timestamp = timestamp;
            this.TransactionId = transactionId;
            this.BlockHeight = blockHeight;
        }

        public dynamic[] AdaptForSerialization()
        {
            var output = new List<dynamic>() {
                this.IsInput,
                this.Delta,
                this.Asset,
                Util.ByteArrayToString(this.AssetId),
                this.Other,
                this.Timestamp,
                Util.ByteArrayToString(this.TransactionId),
                this.BlockHeight
            };
            return output.ToArray();
        }
    }

}