CREATE PROCEDURE [dbo].[Player_UpdateTimePlayed]
	@Login nvarchar(50)
AS
BEGIN
	declare @currentdate datetime
	set @currentdate = getdate()
	declare @InsertedValues Table ( TimePlayed bigint)
	
	UPDATE 
		dbo.Player
	SET
		TimePlayed = TimePlayed + CASE WHEN DateDiff(ms, LastTimePlayedChanged, @currentdate) > 43200000 THEN 0 ELSE DateDiff(ms, LastTimePlayedChanged, @currentdate) END,
		LastTimePlayedChanged = @currentdate,
		LastChanged = @currentdate
	Output
		Inserted.TimePlayed
	INTO
		@InsertedValues
	WHERE
		[Login] = @Login
		
	Select * From @InsertedValues
END
