CREATE PROCEDURE dbo.Record_GetTopRecords
	@ChallengeID int,
	@MaxRecords int
AS
BEGIN
	with Ranking as
	(
		Select
			ROW_NUMBER() OVER (order by R.TimeOrScore asc, R.LastChanged asc) Rank, 
			P.Login as Login,
			P.Nickname as Nickname,
			R.TimeOrScore as TimeOrScore
		FROM
			dbo.Record R with(nolock) 
		INNER JOIN
			dbo.Player P with(nolock) ON R.PlayerID = P.ID
		WHERE
			R.ChallengeID = @ChallengeID
	)
	
	Select 
		Top(@MaxRecords) *
	FROM
		Ranking
	ORDER by 
		Rank ASC
END