using System.Collections.Generic;

namespace pcsd.plugin.sql.DataModel.ConversationAggregates
{
    class Datum
    {
        public string interval { get; set; }
        public IList<Metric> metrics { get; set; }
    }
}
