/****** Object:  View [dbo].[vw_Conversations] ******/

/* Make sure you select the right database before running this script */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_Conversations]
AS
SELECT        TOP (100) PERCENT dbo.Conversations.conversationId, dbo.Conversations.conversationStart, dbo.Sessions.mediaType, dbo.Sessions.direction, dbo.Languages.name AS language, dbo.Sessions.dnis, 
                         dbo.Sessions.ani, dbo.Segments.segmentStart, dbo.Segments.segmentEnd, dbo.Segments.segmentType, dbo.Queues.name AS queueName, dbo.Participants.participantName, dbo.Users.name AS username, 
                         dbo.Participants.purpose, dbo.WrapUpCodes.name AS wrapUpCode, dbo.Segments.wrapUpNote, dbo.Sessions.edgeId, dbo.Sessions.outboundCampaignId, dbo.Sessions.outboundContactId, 
                         dbo.Sessions.outboundContactListId, dbo.Sessions.callbackUserName, dbo.Sessions.callbackScheduledTime, dbo.Sessions.skipEnabled, dbo.Sessions.timeoutSeconds, dbo.Segments.conference, 
                         dbo.Segments.disconnectType, dbo.Segments.errorCode, dbo.Skills.name AS RequestedRoutingSkill
FROM            dbo.Skills INNER JOIN
                         dbo.RoutingSkills ON dbo.Skills.id = dbo.RoutingSkills.RequestedRoutingSkillId RIGHT OUTER JOIN
                         dbo.Conversations INNER JOIN
                         dbo.Participants ON dbo.Participants.Conversation_conversationId = dbo.Conversations.conversationId LEFT OUTER JOIN
                         dbo.Users ON dbo.Participants.userId = dbo.Users.id INNER JOIN
                         dbo.Sessions ON dbo.Sessions.Participant_RowId = dbo.Participants.RowId INNER JOIN
                         dbo.Segments ON dbo.Segments.Session_RowId = dbo.Sessions.RowId ON dbo.RoutingSkills.Segment_RowId = dbo.Segments.RowId LEFT OUTER JOIN
                         dbo.Queues ON dbo.Queues.id = dbo.Segments.queueId LEFT OUTER JOIN
                         dbo.WrapUpCodes ON dbo.WrapUpCodes.id = dbo.Segments.wrapUpCode LEFT OUTER JOIN
                         dbo.Languages ON dbo.Languages.id = dbo.Segments.requestedLanguageId
GO