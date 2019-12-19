using System.Collections.Generic;

namespace pcsd.plugin.sql.DataModel.UserAggregates
{
    class Datum
    {
        public string interval { get; set; }
        public IList<Metric> metrics { get; set; }
    }
}
