using System.Collections.Generic;

namespace pcsd.plugin.sql.DataModel.ConversationAggregates
{
    class Queuedata
    {
        public Group group { get; set; }
        public IList<Datum> data { get; set; }
    }
}
