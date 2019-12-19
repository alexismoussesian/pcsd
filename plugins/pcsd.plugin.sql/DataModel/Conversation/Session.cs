using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.Conversation
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RowId { get; set; }
        public string mediaType { get; set; }
        public string sessionId { get; set; }
        public string addressOther { get; set; }
        public string addressSelf { get; set; }
        public string ani { get; set; }
        public string direction { get; set; }
        public string dnis { get; set; }
        public string outboundCampaignId { get; set; }
        public string outboundContactId { get; set; }
        public string outboundContactListId { get; set; }
        public string dispositionAnalyzer { get; set; }
        public string dispositionName { get; set; }
        public string edgeId { get; set; }
        public string remoteNameDisplayable { get; set; }
        public string roomId { get; set; }
        public string monitoredSessionId { get; set; }
        public string monitoredParticipantId { get; set; }
        public string callbackUserName { get; set; }
        public IList<string> callbackNumbers { get; set; }
        public DateTime? callbackScheduledTime { get; set; }
        public string scriptId { get; set; }
        public bool? skipEnabled { get; set; }
        public int? timeoutSeconds { get; set; }
        public string cobrowseRole { get; set; }
        public string cobrowseRoomId { get; set; }
        public string mediaBridgeId { get; set; }
        public string screenShareAddressSelf { get; set; }
        public bool? sharingScreen { get; set; }
        public string screenShareRoomId { get; set; }
        public string videoRoomId { get; set; }
        public string videoAddressSelf { get; set; }
        public IList<Segment> segments { get; set; }                
    }
}
