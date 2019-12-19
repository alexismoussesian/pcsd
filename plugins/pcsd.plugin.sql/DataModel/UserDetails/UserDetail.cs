using System.Collections.Generic;

namespace pcsd.plugin.sql.DataModel.UserDetails
{
    public class UserDetail
    {
        public string userId { get; set; }
        public IList<PrimaryPresence> primaryPresence { get; set; }
        public IList<RoutingStatus> routingStatus { get; set; }
    }
}
