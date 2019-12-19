using System;
using System.Collections.Generic;
using System.Linq;
using pcsd.plugin.csv.Mapping;

namespace pcsd.plugin.csv.Model
{
    public class Conversation : Reflectionable
    {
        public string conversationId { get; set; }
        public DateTime? conversationStart { get; set; }
        public DateTime? conversationEnd { get; set; }
        public IList<Participant> participants { get; set; }
        public bool IsFinished() { return participants.All(part => !part.sessions.Any(sess => sess.segments.Any(seg => seg.segmentEnd == null))); }
    }
}
