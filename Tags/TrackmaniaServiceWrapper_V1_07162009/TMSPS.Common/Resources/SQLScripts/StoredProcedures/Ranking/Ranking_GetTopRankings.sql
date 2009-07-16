CREATE PROCEDURE [dbo].[Ranking_GetTopRankings]
	@startIndex int,
	@endIndex int
AS
BEGIN
	with PlayerIDs AS
	(
		Select 
			Row_Number() OVER(ORDER BY dbo.Ranking_GetAmountOfRank(PlayerID, 1) desc, dbo.Ranking_GetAmountOfRank(PlayerID, 2) desc, dbo.Ranking_GetAmountOfRank(PlayerID, 3) desc) as RowNr,
			dbo.Ranking_GetAmountOfRank(PlayerID, 1) as FirstRecords, 
			dbo.Ranking_GetAmountOfRank(PlayerID, 2) as SecondRecords,
			dbo.Ranking_GetAmountOfRank(PlayerID, 3) as ThirdRecords,
			PlayerID
		FROM
			dbo.Ranking with (nolock)
		WHERE
			[Rank] <= 3
		Group by
			PlayerID
	)
		
	Select 
		R.RowNr as Position,
		P.[Login], 
		P.Nickname,
		R.FirstRecords, 
		R.SecondRecords,
		R.ThirdRecords
	From 
		Player P with (nolock)
	INNER JOIN
		PlayerIDs R with (nolock) on R.PlayerID = p.Id
	WHERE
		RowNr between @startIndex + 1  and @endIndex + 1
END
