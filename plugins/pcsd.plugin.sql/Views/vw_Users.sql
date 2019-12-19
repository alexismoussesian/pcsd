SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_Users]
AS
SELECT	ua.userId,
		u.name AS [userName],
		ua.intervalUtc,
		ua.metric,
		ua.qualifier, 
		ua.intervalStartLocal, 
		ua.intervalEndLocal, 
		ua.sum
FROM    dbo.Users u RIGHT JOIN dbo.UserAggregates ua ON u.id = ua.userId

GO