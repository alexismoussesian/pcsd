/****** Object:  View [dbo].[[vw_UserRoles]] ******/

/* Make sure you select the right database before running this script */

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_UserRoles]
AS
SELECT b.name Agent
	  ,c.name Role
	  ,d.name Division
  FROM [dbo].[UserRoles] a
  left outer join Users b on b.id = a.UserId
  left outer join Roles c on c.id = a.Roles
  left outer join Divisions d on d.id = a.Division

GO