CREATE PROCEDURE dbo.Rating_Vote
	@Login nvarchar(100),
	@ChallengeID int,
	@Rating tinyint
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	if @PlayerID is null
		return null
	
	if exists(Select * From dbo.Rating with(nolock) where PlayerID=@PlayerID AND ChallengeID = @ChallengeID)
	BEGIN
		UPDATE 
			dbo.Rating
		SET
			LastChanged = getdate(),
			Value = @Rating
		WHERE
			PlayerID=@PlayerID AND 
			ChallengeID = @ChallengeID
	END
	ELSE
	BEGIN
		INSERT INTO dbo.Rating
		(
			PlayerID,
			ChallengeID,
			Value
		)
		VALUES
		(
			@PlayerID,
			@ChallengeID,
			@Rating
		)
	END
	
	Select
		Top 1 AVG(Value)
	FROM
		dbo.Rating with(nolock)
	WHERE
		ChallengeID = @ChallengeID
END