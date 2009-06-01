ALTER PROCEDURE [dbo].[Ranking_Deserialize_List]
	@amountOfRankings int
AS
BEGIN
	Select TOP(@amountOfRankings)
		P.Nickname,
		P.Login,
		R.*
	FROM
		Rank R with(nolock)
	INNER JOIN
		Player P with(nolock) on P.ID = R.PlayerID
	Order by
		Rank Asc
END