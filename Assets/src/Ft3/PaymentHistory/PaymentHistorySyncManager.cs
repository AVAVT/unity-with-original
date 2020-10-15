using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public class ParamPaymentPair
    {
        public readonly PaymentParam Param;
        public readonly PaymentOperation Payment;

        public ParamPaymentPair(PaymentParam param, PaymentOperation payment)
        {
            this.Param = param;
            this.Payment = payment;
        }
    }

    public class PaymentHistorySyncManager
    {
        public readonly PaymentHistoryStore PaymentHistoryStore = new PaymentHistoryStoreLocalStorage();

        public async Task SyncAccount(byte[] id, Blockchain blockchain)
        {
            Dictionary<string, dynamic> syncInfo = this.PaymentHistoryStore.GetSyncInfo(id);
            int lastBlock = -1;

            if(syncInfo.Count == 0) 
            {
                syncInfo.Add("lastBlock", lastBlock);
            }
            else
            {
                lastBlock = (int) syncInfo["lastBlock"];
            }

            PaymentHistoryEntryShort[] paymentHistory = await PaymentHistory.GetAccountById(id, blockchain.Connection, lastBlock);
            if(paymentHistory.Length == 0) return;
            
            //Add missing sender/receiver info to payment history entries
            PaymentHistoryEntry[] paymentHistoryEntries = this.MapShortEntriesToLongEntries(paymentHistory, blockchain.Id, id);
            
            this.PaymentHistoryStore.Save(id, paymentHistoryEntries);
            var highestBlock = this.GetHighestBlock(paymentHistoryEntries, lastBlock);

            syncInfo["lastBlock"] = highestBlock;
            this.PaymentHistoryStore.SaveSyncInfo(id, syncInfo);
        }

        private PaymentHistoryEntry[] MapShortEntriesToLongEntries(PaymentHistoryEntryShort[] entries, byte[] chainId, byte[] accountId)
        {
            Dictionary<string, PaymentHistoryEntryShort[]> entriesMap = this.GroupShortEntriesByTransactionRID(entries);
            List<PaymentHistoryEntry[]> paymentHistoryEntries = new List<PaymentHistoryEntry[]>();

            foreach (var value in entriesMap.Values)
            {
                paymentHistoryEntries.Add(this.PaymentHistoryEntriesFrom(value, chainId, accountId));
            }

            return paymentHistoryEntries.SelectMany(x => x).Reverse().ToArray();
        }

        private Dictionary<string, PaymentHistoryEntryShort[]> GroupShortEntriesByTransactionRID(PaymentHistoryEntryShort[] entries)
        {
            Dictionary<string, PaymentHistoryEntryShort[]> entriesGroups = new Dictionary<string, PaymentHistoryEntryShort[]>();
            
            foreach (var entry in entries)
            {  
                if(entriesGroups.ContainsKey(entry.TransactionId))
                {
                    var concatList = entriesGroups[entry.TransactionId].ToList();
                    concatList.Add(entry);
                    entriesGroups[entry.TransactionId] = concatList.ToArray();
                }
                else
                {
                    entriesGroups.Add(entry.TransactionId, new List<PaymentHistoryEntryShort>() {entry}.ToArray());
                }
            }
            
            return entriesGroups;
        }

        private PaymentHistoryEntry[] PaymentHistoryEntriesFrom(PaymentHistoryEntryShort[] entries, byte[] chainId, byte[] accountId)
        {
            if(entries.Length == 0) return new PaymentHistoryEntry[] {};

            PaymentHistoryEntryShort firstEntry = entries[0];

            // Get all the payments from the transaction which are related to the current account,
            // and then get all inputs and outputs for which current account is source or destination.
            PaymentOperation[] payments = this.GetPaymentsForChainAndAccountFromRawTransaction(
                Util.ByteArrayToString(chainId),
                accountId,
                firstEntry.TransactionData
            );

            ParamPaymentPair[] inputs = payments.ToList().Select(payment => payment.InputsWithChainAndAccount(
                                                        Util.ByteArrayToString(chainId),
                                                        Util.ByteArrayToString(accountId)
                                                    ).ToList().Select(input => new ParamPaymentPair(input, payment))
            ).SelectMany(x => x).ToArray();

            ParamPaymentPair[] outputs = payments.ToList().Select(payment => payment.OutputsWithChainAndAccount(
                                                        Util.ByteArrayToString(chainId),
                                                        Util.ByteArrayToString(accountId)
                                                    ).ToList().Select(output => new ParamPaymentPair(output, payment))
            ).SelectMany(x => x).ToArray();

            //TODO: investigate if this check has to be removed, because lib might me used with new FT3 contract which
            //maybe has new transfer type, which adds payment history entries, but transfer is not supported by this version
            //of the client lib and it will not be loaded, and in the and number of inputs and outputs will not match
            //number of entries, and therefore exception will be thrown.
            if((inputs.Length + outputs.Length) != entries.Length)
            {
                throw new Exception("Number of payment entries" + entries.Length + 
                " and number of transfer inputs and outputs" +inputs.Length + outputs.Length +
                " with address" + Util.ByteArrayToString(accountId) + 
                "in the transaction are not the same!");
            }

            List<PaymentHistoryEntry> paymentHistoryEntries = new List<PaymentHistoryEntry>();
            // In payment history entries returned from blockchain, there is no info about receiver for sent transfers
            // and sender for received transfers. In order to get sender/receiver info, we have to match payment history
            // entries to corresponding inputs and outputs extracted in previous step,
            // and then get sender/receiver from corresponding transfers.
            foreach (var entry in entries)
            {
                if(entry.IsInput)
                {
                    ParamPaymentPair input = this.MatchPaymentHistoryEntryAndPaymentParam(entry, inputs, accountId);
                    if(input == null) throw new Exception("Cannot match payment history entry to any transfer input");

                    inputs = inputs.ToList().Where(i => !i.Equals(input)).ToArray();
                    paymentHistoryEntries.Add(this.GetPaymentHistoryEntry(entry, input.Payment));
                }
                else
                {
                    ParamPaymentPair output = this.MatchPaymentHistoryEntryAndPaymentParam(entry, outputs, accountId);
                    if(output == null) throw new Exception("Cannot match payment history entry to any transfer output");

                    outputs = outputs.ToList().Where(o => !o.Equals(output)).ToArray();
                    paymentHistoryEntries.Add(this.GetPaymentHistoryEntry(entry, output.Payment));
                }
            }

            return paymentHistoryEntries.ToArray();
        }

        private ParamPaymentPair MatchPaymentHistoryEntryAndPaymentParam(PaymentHistoryEntryShort entry, ParamPaymentPair[] paramPairs, byte[] accountId)
        {
            return paramPairs.ToList().Find(para => (
                para.Param.IsAccountId(Util.ByteArrayToString(accountId)) &&
                para.Param.IsAssetId(entry.AssetId) &&
                para.Param.Amount == entry.Delta
                 //TODO: compare entry index
            ));
        }

        private PaymentHistoryEntry GetPaymentHistoryEntry(PaymentHistoryEntryShort entry, PaymentOperation payment)
        {
            dynamic other;
            if(entry.IsInput)
            {
                other = payment.OutputsWithAsset(entry.AssetId).Select(id => new dynamic[] {id.ChainId, id.AccountId}).ToArray();
            }
            else
            {
                other = payment.InputsWithAsset(entry.AssetId).Select(id => new dynamic[]{id.ChainId, id.AccountId}).ToArray();
            }

            return new PaymentHistoryEntry(
                entry.IsInput,
                entry.Delta,
                entry.Asset,
                Util.HexStringToBuffer(entry.AssetId),
                other,
                entry.Timestamp,
                Util.HexStringToBuffer(entry.TransactionId),
                entry.BlockHeight
            );
        }

        private int GetHighestBlock(PaymentHistoryEntry[] entries, int lastBlock)
        {
            int[] heights = entries.ToList().Select(blockHeight => blockHeight.BlockHeight).ToArray();
            int highest = heights[0];

            foreach (var height in heights)
            {
                if(height > highest)
                {
                    highest = height;
                }
            }
            return highest;
        }

        private PaymentOperation[] GetPaymentsForChainAndAccountFromRawTransaction(string chainId, byte[] accountId, byte[] transactionData)
        {
            PaymentOperationExtractor paymentOperationExtractor = new PaymentOperationExtractor(transactionData, chainId);
            PaymentOperation[] paymentOps = paymentOperationExtractor.Extract();
            return paymentOps.Where(transfer => transfer.HasInputOrOutputWithChainAndAccount(chainId, Util.ByteArrayToString(accountId))).ToArray();
        }
    }
}