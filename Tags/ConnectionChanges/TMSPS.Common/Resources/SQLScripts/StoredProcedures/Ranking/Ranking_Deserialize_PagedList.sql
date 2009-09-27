CREATE PROCEDURE [dbo].[Ranking_Deserialize_PagedList]
	@startIndex int,
	@endIndex int
AS
BEGIN
	Select 
		P.Nickname,
		P.Login,
		R.*
	FROM
		Rank R with(nolock)
	INNER JOIN
		Player P with(nolock) on P.ID = R.PlayerID
	WHERE
		R.Rank between @startIndex + 1  and @endIndex + 1
	Order by
		Rank Asc
END