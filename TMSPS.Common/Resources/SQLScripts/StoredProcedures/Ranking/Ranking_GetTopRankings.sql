CREATE PROCEDURE [dbo].[Ranking_GetTopRankings]
	@startIndex int,
	@endIndex int
AS
BEGIN
	with PlayerIDs AS
	(
		Select 
			Row_Number() OVER(ORDER BY SUM(CASE WHEN Rank = 1 THEN 1 ELSE 0 END) desc, SUM(CASE WHEN Rank = 2 THEN 1 ELSE 0 END) desc, SUM(CASE WHEN Rank = 3 THEN 1 ELSE 0 END) desc) as RowNr,
			SUM(CASE WHEN Rank = 1 THEN 1 ELSE 0 END) as FirstRecords, 
			SUM(CASE WHEN Rank = 2 THEN 1 ELSE 0 END) as SecondRecords,
			SUM(CASE WHEN Rank = 3 THEN 1 ELSE 0 END) as ThirdRecords,
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
