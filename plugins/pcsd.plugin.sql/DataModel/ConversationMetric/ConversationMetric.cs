using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.ConversationMetric
{
    public class ConversationMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        //[Key]
        public string SessionId { get; set; }
        public string ConversationId { get; set; }
        public string ParticipantId { get; set; }
        public string attrName { get; set; }
        public long? attrValue { get; set; }
        public DateTime? emitDate { get; set; }
    }
}
