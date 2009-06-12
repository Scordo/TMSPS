CREATE PROCEDURE [dbo].[Ranking_GetNextRank]
	@login nvarchar(50)
AS
BEGIN
	declare @rank int;
	set @rank =
	(
		Select TOP 1
			R.Rank - 1
		FROM
			Rank R with(nolock)
		INNER JOIN
			Player P with(nolock) on P.ID = R.PlayerID
		WHERE
			P.Login = @login
	)
	
	if @rank is null
		set @rank = (Select Max(Rank) FROM dbo.Rank with (nolock))
	
	exec dbo.[Ranking_Deserialize_ByRank] @rank
END