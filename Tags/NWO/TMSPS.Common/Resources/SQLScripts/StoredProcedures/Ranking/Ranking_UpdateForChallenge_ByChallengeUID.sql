CREATE PROCEDURE dbo.Ranking_UpdateForChallenge_ByChallengeUID
	@UniqueID char(27)
AS
BEGIN
	declare @challengeID int
	SET @challengeID = (Select TOP 1 ID FROM dbo.Challenge with (nolock) where UniqueID = @UniqueID)
	
	if (@challengeID is not null)
		exec dbo.Ranking_UpdateForChallenge @challengeID
END