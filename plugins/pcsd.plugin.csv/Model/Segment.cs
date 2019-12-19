using System;
using System.Collections.Generic;
using pcsd.plugin.csv.Mapping;

namespace pcsd.plugin.csv.Model
{
    public class Segment : Reflectionable
    {
        public DateTime? segmentStart { get; set; }
        public DateTime? segmentEnd { get; set; }
        public string queueId { get; set; }
        public string wrapUpCode { get; set; }
        public string wrapUpNote { get; set; }
        public IList<string> wrapUpTags { get; set; }
        public string errorCode { get; set; }
        public string disconnectType { get; set; }
        public string segmentType { get; set; }
        public IList<string> requestedRoutingUserIds { get; set; }       
        public IList<string> requestedRoutingSkillIds { get; set; }
        public string requestedLanguageId { get; set; }
        public IList<Property> properties { get; set; }
        public string sourceConversationId { get; set; }
        public string destinationConversationId { get; set; }
        public string sourceSessionId { get; set; }
        public string destinationSessionId { get; set; }
        public IList<long> sipResponseCodes { get; set; } 
        public IList<long> q850ResponseCodes { get; set; } 
        public bool? conference { get; set; }
        public string groupId { get; set; }
        public string subject { get; set; }
        public bool? audioMuted { get; set; }
        public bool? videoMuted { get; set; }
    }
}
