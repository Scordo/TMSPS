CREATE PROCEDURE dbo.Player_CreateOrUpdate
	@Login nvarchar(100),
	@Nickname nvarchar(100)
AS
BEGIN
	declare @ID int
	set @ID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	declare @currentDate datetime
	SET @currentDate = getdate()
	
	declare @InsertedValues Table ( ID int, Created datetime, LastChanged datetime, LastTimePlayedChanged datetime, Wins int, TimePlayed bigint)
	
	if @ID is null
	BEGIN
		INSERT INTO dbo.Player
		(
			[Login],
			Nickname,
			LastTimePlayedChanged,
			Wins,
			TimePlayed
		)
		OUTPUT
			Inserted.ID,
			Inserted.Created,
			Inserted.LastChanged,
			Inserted.LastTimePlayedChanged,
			Inserted.Wins,
			Inserted.TimePlayed
		INTO
			@InsertedValues (ID, Created, LastChanged, LastTimePlayedChanged, Wins, TimePlayed)
		VALUES
		(
			@Login,
			@Nickname,
			@currentDate,
			0,
			0
		)
	END
	ELSE
	BEGIN
		UPDATE
			dbo.Player
		SET
			LastChanged = @currentDate,
			LastTimePlayedChanged = @currentDate,
			Nickname = @Nickname
		OUTPUT
			Inserted.ID,
			Inserted.Created,
			Inserted.LastChanged,
			Inserted.LastTimePlayedChanged,
			Inserted.Wins,
			Inserted.TimePlayed
		INTO
			@InsertedValues (ID, Created, LastChanged, LastTimePlayedChanged, Wins, TimePlayed)
		WHERE
			ID = @ID
	END
	
	Select * FROM @InsertedValues
END