SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_Queues]
AS
SELECT	ca.mediaType, 
		ca.queueId,
		q.name AS [queueName], 
		ca.intervalUtc, 
		ca.metric, 
		ca.intervalStartLocal, 
		ca.intervalEndLocal, 
		ca.count, 
		ca.max, 
		ca.min, 
		ca.sum        
FROM	dbo.Queues q RIGHT JOIN dbo.ConversationAggregates ca ON q.id = ca.queueId
GO
