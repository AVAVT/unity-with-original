using System;

namespace Chromia.Postchain.Ft3
{
    public class PaymentHistoryIterator
    {
        public readonly byte[] AccountId;
        public readonly int PageSize;
        private int _currentPage = -1;
        private PaymentHistoryStore _paymentHistoryStore;

        public PaymentHistoryIterator(PaymentHistoryStore paymentHistoryStore, byte[] accountId, int pageSize)
        {
            this._paymentHistoryStore = paymentHistoryStore;
            this.AccountId = accountId;
            this.PageSize = pageSize;
        }

        public int GetPageCount()
        {
            return (int) Math.Ceiling((double) this._paymentHistoryStore.GetCount(this.AccountId) / (double) this.PageSize);
        }

        public int GetTotalCount()
        {
            return this._paymentHistoryStore.GetCount(this.AccountId);
        }

        public int GetCurrent()
        {
            return this.GetPageCount() != 0 ? this._currentPage : 0;
        }

        public PaymentHistoryEntry[] Rewind()
        {
            var entries = this._paymentHistoryStore.GetEntries(this.AccountId, 0, this.PageSize);
            this._currentPage = 0;
            return entries;
        }

        public PaymentHistoryEntry[] Prev()
        {
            if(this._currentPage == 0)
            {
                return null;
            }

            var page = this._currentPage - 1;
            var entries = this._paymentHistoryStore.GetEntries(this.AccountId, page * this.PageSize, this.PageSize);
            this._currentPage = page;

            return entries;
        }

        public PaymentHistoryEntry[] Next()
        {
            if (!this.HasMore())
            {
                 return null;
            }

            var page = this._currentPage + 1;
            var entries = this._paymentHistoryStore.GetEntries(this.AccountId, page * this.PageSize, this.PageSize);
            this._currentPage = page;
            return entries;
        }

        public PaymentHistoryEntry[] FastForward()
        {
            var page = this.GetPageCount() - 1;
            var entries = this._paymentHistoryStore.GetEntries(this.AccountId, page * this.PageSize, this.PageSize);
            this._currentPage = page;
            return entries;
        }

        public bool HasMore()
        {
            return this._currentPage < this.GetPageCount() - 1;
        }
    }
}

