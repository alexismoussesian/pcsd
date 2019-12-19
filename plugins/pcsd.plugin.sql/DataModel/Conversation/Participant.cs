using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.Conversation
{
    public class Participant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        public string participantId { get; set; }
        public string participantName { get; set; }
        public string userId { get; set; }
        public string purpose { get; set; }
        public string externalContactId { get; set; }
        public string externalOrganizationId { get; set; }
        public IList<Session> sessions { get; set; }        
    }
}
