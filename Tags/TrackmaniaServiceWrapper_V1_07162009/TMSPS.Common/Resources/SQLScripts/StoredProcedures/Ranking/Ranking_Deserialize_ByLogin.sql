CREATE PROCEDURE [dbo].[Ranking_Deserialize_ByLogin]
	@login nvarchar(50)
AS
BEGIN
	Select TOP 1
		P.Nickname,
		P.Login,
		R.*
	FROM
		Rank R with(nolock)
	INNER JOIN
		Player P with(nolock) on P.ID = R.PlayerID
	WHERE
		P.Login = @login
END
