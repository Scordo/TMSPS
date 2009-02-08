CREATE PROCEDURE dbo.Session_Add
	@Login nvarchar(50),
	@ChallengeID int,
	@TimeOrScore int
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	if @PlayerID is null
		return
		
	INSERT INTO dbo.Session
	(
		PlayerID,
		ChallengeID,
		TimeOrScore
	)
	VALUES
	(
		@PlayerID,
		@ChallengeID,
		@TimeOrScore
	)
END