ALTER PROCEDURE dbo.Ranking_GetRanksCount
AS
BEGIN
	Select Count(*) From dbo.Rank with (nolock)
END