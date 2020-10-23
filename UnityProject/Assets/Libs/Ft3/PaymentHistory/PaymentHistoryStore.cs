using System.Collections.Generic;

namespace Chromia.Postchain.Ft3
{
    public interface PaymentHistoryStore
    {
        int GetCount(byte[] accountId);
        PaymentHistoryIterator GetIterator(byte[] accountId, int pageSize);
        void Save(byte[] accountId, PaymentHistoryEntry[] paymentHistoryEntries);
        PaymentHistoryEntry[] GetEntries(byte[] accountId, int start, int pageSize);
        Dictionary<string, dynamic> GetSyncInfo(byte[] accountId);
        void SaveSyncInfo(byte[] accountId, Dictionary<string, dynamic> syncInfo);
    }
}