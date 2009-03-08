CREATE PROCEDURE dbo.Ranking_Deserialize_List_ByMost
	@top int,
	@rankLimit int
AS
BEGIN
	with MainQuery As
	(
		Select Top(@top)
			PlayerID,
			Count([Rank]) as RanksCount
		FROM
			Ranking with (nolock)
		WHERE
			[Rank] <= @rankLimit
		Group by 
			PlayerID
		Order by 
			RanksCount desc
	)

	Select
		P.Nickname,
		M.*
	FROm
		MainQuery M with(nolock)
	INNER JOIN
		Player P  with(nolock) on P.Id = M.PlayerID
END