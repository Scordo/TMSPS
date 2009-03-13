Create FUNCTION [dbo].[Ranking_GetAmountOfRank](@playerID int, @rank int) returns int
AS
BEGIN
	return (Select Count(*) FROM dbo.Ranking with (nolock) where PlayerID = @playerID and [Rank] = @rank)
END