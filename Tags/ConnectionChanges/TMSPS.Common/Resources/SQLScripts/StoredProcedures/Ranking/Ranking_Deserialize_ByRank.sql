CREATE PROCEDURE [dbo].[Ranking_Deserialize_ByRank]
	@rank int
AS
BEGIN
	Select TOP 1
		P.Nickname,
		P.Login,
		R.*
	FROM
		Rank R with(nolock)
	INNER JOIN
		Player P with(nolock) on P.ID = R.PlayerID
	WHERE
		R.[Rank] = @rank
END
