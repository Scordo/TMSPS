CREATE PROCEDURE [dbo].[Ranking_GetTopRankingsCount]
AS
BEGIN
	Select Count(PlayerID) From (Select Distinct PlayerID from dbo.ranking with(nolock) where [Rank] <= 3) P
END
