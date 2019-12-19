namespace pcsd.plugin.sql.DataModel
{
    class TransactionCounter
    {
        public long RowsUpdated { get; set; } = 0;
        public long RowsAdded { get; set; } = 0;
        public long RowsIgnored { get; set; } = 0;
        public long RowsPutIntoTheBag { get; set; } = 0;
        public long RowsRemovedFromTheBag { get; set; } = 0;
    }
}
