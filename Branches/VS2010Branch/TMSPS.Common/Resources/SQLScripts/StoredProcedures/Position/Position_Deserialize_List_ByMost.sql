CREATE PROCEDURE [dbo].[Position_Deserialize_List_ByMost]
	@top int,
	@positionLimit int
AS
BEGIN
	with MainQuery As
	(
		Select Top(@top)
			PlayerID,
			Count(OwnPosition) as PositionsCount
		FROM
			Position with (nolock)
		WHERE
			OwnPosition <= @positionLimit
		Group by 
			PlayerID
		Order by 
			PositionsCount desc
	)

	Select
		P.Nickname,
		M.*
	FROm
		MainQuery M with(nolock)
	INNER JOIN
		Player P  with(nolock) on P.Id = M.PlayerID
END
