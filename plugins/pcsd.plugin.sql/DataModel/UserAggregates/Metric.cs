namespace pcsd.plugin.sql.DataModel.UserAggregates
{
    class Metric
    {
        public string metric { get; set; }
        public string qualifier { get; set; }
        public Stats stats { get; set; }
    }
}
