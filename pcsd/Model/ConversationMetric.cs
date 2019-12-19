using System;

namespace pcstat.Model
{
    public class ConversationMetric
    {
        public string SessionId { get; set; }
        public string ConversationId { get; set; }
        public string ParticipantId { get; set; }
        public string attrName { get; set; }
        public long? attrValue { get; set; }
        public DateTime? emitDate { get; set; }

    }
}
