CREATE FUNCTION dbo.Ranking_GetTopRankingsCount() returns int
AS
BEGIN
	return (Select Count(PlayerID) From (Select Distinct PlayerID from dbo.ranking with(nolock) where [Rank] <= 3) P)
END