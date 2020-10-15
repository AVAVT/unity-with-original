using System.Collections.Generic;
using Hanssens.Net;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public class PaymentHistoryStoreLocalStorage : PaymentHistoryStore
    {
        private Dictionary<string, PaymentHistoryEntry[]> _entriesCache = new Dictionary<string, PaymentHistoryEntry[]>();
        private LocalStorage _localStorage = new LocalStorage();

        public int GetCount(byte[] accountId)
        {
            return this.GetEntriesFor(accountId).Length;
        }
        public PaymentHistoryIterator GetIterator(byte[] accountId, int pageSize)
        {
            return new PaymentHistoryIterator(this, accountId, pageSize);
        }

        public void Save(byte[] accountId, PaymentHistoryEntry[] paymentHistoryEntries)
        {
            PaymentHistoryEntry[] entries = this.LoadFromStore(accountId);
            PaymentHistoryEntry[] newEntries = paymentHistoryEntries.ToList().Concat(entries).ToArray();
            this.SaveToStore(accountId, newEntries);
            this._entriesCache[Util.ByteArrayToString(accountId).ToUpper()] = newEntries;
        }

        public PaymentHistoryEntry[] GetEntries(byte[] accountId, int start, int pageSize)
        {
            PaymentHistoryEntry[] entries = this.GetEntriesFor(accountId);
            if(entries.Length < start)
            {
                return new PaymentHistoryEntry[]{};
            }

            return entries.ToList().GetRange(start, Math.Min(entries.Length, start + pageSize)).ToArray();
        }

        public Dictionary<string, dynamic> GetSyncInfo(byte[] accountId)
        {
            string key = "FT3_LIB_P_H_S_I_" + Util.ByteArrayToString(accountId).ToUpper();
            dynamic value;
            try
            {
                value = _localStorage.Get(key);
            }
            catch(ArgumentNullException)
            {
                return new Dictionary<string, dynamic>();
            }
            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(value);
        }

        public void SaveSyncInfo(byte[] accountId, Dictionary<string, dynamic> syncInfo)
        {
            string key = "FT3_LIB_P_H_S_I_" + Util.ByteArrayToString(accountId).ToUpper();
            var value = JsonConvert.SerializeObject(syncInfo);

            _localStorage.Store(key, value);
        }

        private PaymentHistoryEntry[] GetEntriesFor(byte[] accountId)
        {
            string accountIdString = Util.ByteArrayToString(accountId).ToUpper();
            PaymentHistoryEntry[] entries = this._entriesCache[accountIdString];

            if(entries == null)
            {
                entries = this.LoadFromStore(accountId);
                this._entriesCache[accountIdString] = entries;
            }

            return entries;
        }

        private PaymentHistoryEntry[] LoadFromStore(byte[] id)
        {
            string key = "FT3_LIB_P_H_" + Util.ByteArrayToString(id).ToUpper();
            dynamic value;
            try
            {
                value = _localStorage.Get(key);
            }
            catch(ArgumentNullException)
            {
                return new PaymentHistoryEntry[]{};
            }

            PaymentHistoryEntry[] entries = JsonConvert.DeserializeObject<PaymentHistoryEntry[]>(value.ToString());
            
            return entries.ToList().Select(entry => this.MapToPaymentHistoryEntry(entry)).ToArray();
        }

        private void SaveToStore(byte[] id, PaymentHistoryEntry[] entries)
        {
            string key = "FT3_LIB_P_H" + Util.ByteArrayToString(id).ToUpper();
            var value = entries.ToList().Select(entry => entry.AdaptForSerialization());

            _localStorage.Store(key, JsonConvert.SerializeObject(value));
        }

        private PaymentHistoryEntry MapToPaymentHistoryEntry(dynamic entry)
        {
            return new PaymentHistoryEntry(
                (bool) entry["isInput"],
                (int) entry["delta"],
                (string) entry["asset"],
                Util.HexStringToBuffer((string) entry["assetId"]),
                (dynamic[]) entry["other"],
                (string) entry["timestamp"],
                Util.HexStringToBuffer((string) entry["transactionId"]),
                (int) entry["blockHeight"]
            );
        }
    }
}
