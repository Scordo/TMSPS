CREATE PROCEDURE dbo.Player_UpdateTimePlayed
	@Login nvarchar(50)
AS
BEGIN
	declare @currentdate datetime
	set @currentdate = getdate()
	declare @InsertedValues Table ( TimePlayed bigint)
	
	UPDATE 
		dbo.Player
	SET
		TimePlayed = TimePlayed + DateDiff(ms, LastTimePlayedChanged, @currentdate),
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