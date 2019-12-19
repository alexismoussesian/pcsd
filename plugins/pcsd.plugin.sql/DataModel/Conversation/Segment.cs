using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.Conversation
{
    public class Segment
    {
        private IList<string> _requestedRoutingSkillIds;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        public DateTime? segmentStart { get; set; }
        public DateTime? segmentEnd { get; set; }
        public string queueId { get; set; }
        public string wrapUpCode { get; set; }
        public string wrapUpNote { get; set; }
        // public IList<string> wrapUpTags { get; set; } // commented as EF doesn't support list of string
        public string errorCode { get; set; }
        public string disconnectType { get; set; }
        public string segmentType { get; set; }
        // public IList<string> requestedRoutingUserIds { get; set; } // commented as EF doesn't support list of string
        /// <summary>
        /// The code below is required because Entity Framework does not support collections of primitive data types (i.e. IList<string>)
        /// </summary>
        [NotMapped]
        public IList<string> requestedRoutingSkillIds
        {
            get { return _requestedRoutingSkillIds; }
            set
            {
                _requestedRoutingSkillIds = value;
                routingSkills = new List<RoutingSkill>();
                foreach (var skillId in _requestedRoutingSkillIds)
                {
                    routingSkills.Add(new RoutingSkill() {RequestedRoutingSkillId = skillId});
                }
            }
        }
        public IList<RoutingSkill> routingSkills { get; set; }
        public string requestedLanguageId { get; set; }
        public IList<Property> properties { get; set; }
        public string sourceConversationId { get; set; }
        public string destinationConversationId { get; set; }
        public string sourceSessionId { get; set; }
        public string destinationSessionId { get; set; }
        // public IList<long> sipResponseCodes { get; set; } // commented as EF doesn't support list of long
        // public IList<long> q850ResponseCodes { get; set; } // commented as EF doesn't support list of long
        public bool? conference { get; set; }
        public string groupId { get; set; }
        public string subject { get; set; }
        public bool? audioMuted { get; set; }
        public bool? videoMuted { get; set; }
    }
}
