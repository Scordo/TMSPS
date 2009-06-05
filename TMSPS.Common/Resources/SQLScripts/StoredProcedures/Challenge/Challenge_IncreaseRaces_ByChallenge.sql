ALTER PROCEDURE [dbo].[Challenge_IncreaseRaces_ByChallenge]
	@ID int,
	@UniqueID varchar(27),
	@Name nvarchar(100),
	@Author nvarchar(50),
	@Environment nvarchar(20)
AS
BEGIN
	declare @InsertedValues Table ( ID int, Created datetime, LastChanged datetime, Races int)
	
	if (@ID is null)
		set @ID = (Select ID FROM dbo.Challenge with (nolock) WHERE UniqueID = @UniqueID)
		
	if (not Exists(Select * From dbo.Challenge with (nolock) WHERE ID = @ID))
	BEGIN
		INSERT INTO dbo.Challenge
		(
			UniqueID,
			Name,
			Author,
			Environment,
			Races
		)
		OUTPUT
			Inserted.ID,
			Inserted.Created,
			Inserted.LastChanged,
			Inserted.Races
		INTO
			@InsertedValues (ID, Created, LastChanged, Races)
		VALUES
		(
			@UniqueID,
			@Name,
			@Author,
			@Environment,
			1
		)
	END
	ELSE
	BEGIN
		UPDATE
			dbo.Challenge
		SET
			LastChanged = getdate(),
			Races = Races + 1
		OUTPUT
			Inserted.ID,
			Inserted.Created,
			Inserted.LastChanged,
			Inserted.Races
		INTO
			@InsertedValues (ID, Created, LastChanged, Races)
		WHERE
			ID = @ID
	END
	
	Select * FROM @InsertedValues
END