CREATE PROCEDURE dbo.Record_GetBest
	@Login nvarchar(50),
	@ChallengeID int
AS
BEGIN
	Select 
		TimeOrScore
	FROM
		dbo.Record R with(nolock)
	INNER JOIN
		dbo.Player P with(nolock) ON R.PlayerID = P.ID
	WHERE
		R.ChallengeID = @ChallengeID AND
		P.Login = @Login
END