CREATE PROCEDURE dbo.Position_Add
	@Login nvarchar(100),
	@UniqueChallengeID char(27),
	@Position  smallint,
	@MaxPosition  smallint
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	if @PlayerID is null
		return
		
	declare @ChallengeID int
	set @ChallengeID = (Select ID FROM dbo.Challenge with(nolock) WHERE UniqueID = @UniqueChallengeID)
	
	if @PlayerID is null
		return
	
	INSERT INTO dbo.Position
	(
		PlayerID,
		ChallengeID,
		OwnPosition,
		MaxPosition
	)
	VALUES
	(
		@PlayerID,
		@ChallengeID,
		@Position,
		@MaxPosition
	)
END