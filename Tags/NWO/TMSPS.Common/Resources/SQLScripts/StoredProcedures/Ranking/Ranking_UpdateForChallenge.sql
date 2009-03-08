CREATE PROCEDURE Ranking_UpdateForChallenge
	@ChallengeID int
AS
BEGIN
	Delete dbo.Ranking WHERE ChallengeID = @ChallengeID

	INSERT INTO dbo.Ranking
	Select
		@ChallengeID as ChallengeID,
		PlayerID, 
		[Rank]
	From 
	(
		Select
			ROW_NUMBER() OVER (order by TimeOrScore asc, LastChanged asc) [Rank], *
		FROM
			dbo.Record with (nolock)
		WHERE
			ChallengeID = @ChallengeID
	) Rankings
END