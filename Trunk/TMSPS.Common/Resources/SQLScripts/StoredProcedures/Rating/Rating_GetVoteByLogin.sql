CREATE PROCEDURE [dbo].[Rating_GetVoteByLogin]
	@Login nvarchar(50),
	@ChallengeID int
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	if @PlayerID is null
		return null
		
	Select
		Top 1 Value
	FROM
		dbo.Rating with(nolock)
	WHERE
		PlayerID = @PlayerID AND
		ChallengeID = @ChallengeID
END
