CREATE PROCEDURE dbo.Player_IncreaseWins
	@Login nvarchar(100)
AS
BEGIN
	declare @InsertedValues Table ( Wins int)

	UPDATE 
		dbo.Player
	SET
		Wins = Wins + 1,
		LastChanged = getdate()
	Output
		Inserted.Wins
	INTO
		@InsertedValues
	WHERE
		[Login] = @Login
		
	Select * From @InsertedValues
END