﻿CREATE PROCEDURE [dbo].[Player_Deserialize]
	@login nvarchar(100)
AS
BEGIN
	Select
		*
	FROM
		dbo.Player
	WHERE
		[Login] = @login
END