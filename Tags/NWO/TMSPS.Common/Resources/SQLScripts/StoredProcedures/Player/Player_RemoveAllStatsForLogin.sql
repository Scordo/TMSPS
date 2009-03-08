CREATE PROCEDURE dbo.Player_RemoveAllStatsForLogin
	@login nvarchar(100)
AS
BEGIN
	declare @playerID int
	Set @playerID = (Select top 1 ID from dbo.Player with(nolock) where [Login] = @login)

	declare @result int; set @result = 0

	if (@playerID is not null)
	BEGIN
		DELETE dbo.Position where PlayerID = @playerID
		DELETE dbo.Ranking where PlayerID = @playerID
		DELETE dbo.Rating where PlayerID = @playerID
		DELETE dbo.Record where PlayerID = @playerID
		DELETE dbo.Session where PlayerID = @playerID
		UPDATE dbo.Player set Wins = 0, TimePlayed = 0 where ID = @playerID
		set @result = 1
	END;

	select @result
END