CREATE PROCEDURE dbo.Ranking_Deserialize_ByLogin
	@login nvarchar(50)
AS
BEGIN
	declare @challengesCount int
	Select @challengesCount = Count(ID) FROM Challenge with (nolock)

	;with MainQuery as
	(
		Select
			ROW_NUMBER() OVER (order by (AVG([Rank]) + (@challengesCount+1) / (Count([Rank])+1) * (@challengesCount - Count([Rank]))) ASC) [Rank],
			PlayerID,
			AVG([Rank]) as AverageRank,
			Count([Rank]) as RecordsCount,
			@challengesCount as ChallengesCount,
			(AVG([Rank]) + (@challengesCount+1) / (Count([Rank])+1) * (@challengesCount - Count([Rank]))) as Score
		From
			Ranking with (nolock)
		GROUP By 
			PlayerID
	)

	Select Top 1
		P.Nickname,
		P.Login,
		M.*
	FROM
		MainQuery M with(nolock)
	INNER JOIN
		Player P with(nolock) on P.ID = m.PlayerID
	WHERE
		P.Login = @login
END