CREATE PROCEDURE [dbo].[Challenge_GetDrivenUniqueTrackIDs_ByLogin]
	@Login nvarchar(50)
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	if @PlayerID is null
		return
	
	Select 
		c.UniqueID
	FROM 
		dbo.Record r with(nolock)
	INNER JOIN
		dbo.Challenge c with(nolock) on c.ID = r.ChallengeID
	WHERE
		r.PlayerID = @PlayerID
END