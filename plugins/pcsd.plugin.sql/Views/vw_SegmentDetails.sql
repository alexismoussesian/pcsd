/****** Object:  View [dbo].[vw_SegmentDetails] ******/

/* Make sure you select the right database before running this script */


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_SegmentDetails]
AS
SELECT        dbo.Participants.Conversation_conversationId, dbo.Participants.participantName, dbo.Users.name, dbo.Participants.purpose, DATEDIFF(ss, dbo.Segments.segmentStart, dbo.Segments.segmentEnd) AS Time, 
                         dbo.Segments.segmentType, dbo.Queues.name AS [Workgroup Queue], dbo.WrapUpCodes.name AS Expr1, dbo.Segments.disconnectType, dbo.Segments.subject
FROM            dbo.Participants INNER JOIN
                         dbo.Segments ON dbo.Participants.RowId = dbo.Segments.Session_RowId LEFT OUTER JOIN
                         dbo.Users ON dbo.Participants.userId = dbo.Users.id LEFT OUTER JOIN
                         dbo.Queues ON dbo.Segments.queueId = dbo.Queues.id LEFT OUTER JOIN
                         dbo.WrapUpCodes ON dbo.Segments.wrapUpCode = dbo.WrapUpCodes.id

GO


