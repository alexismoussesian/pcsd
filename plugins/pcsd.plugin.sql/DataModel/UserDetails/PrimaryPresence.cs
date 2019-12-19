using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.UserDetails
{
    public class PrimaryPresence
    {
        [Key]
        [Column(Order = 1)]
        [StringLength(128)]
        public string userId { get; set; }
        [Key]
        [Column(Order = 2, TypeName = "datetime2")]
        public DateTime startTime { get; set; }
        [Column(Order = 3, TypeName = "datetime2")]
        public DateTime endTime { get; set; }
        [Key]
        [Column(Order = 4)]
        [StringLength(128)]
        public string systemPresence { get; set; }
        [Column(Order = 5)]
        [StringLength(128)]
        public string organizationPresenceId { get; set; }
    }
}
