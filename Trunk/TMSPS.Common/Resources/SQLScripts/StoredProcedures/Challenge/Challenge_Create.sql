CREATE PROCEDURE dbo.Challenge_Create
	@UniqueID char(27),
	@Name nvarchar(100),
	@Author nvarchar(50),
	@Environment nvarchar(20),
	@Races int
AS
BEGIN
	declare @ID int
	set @ID = (Select ID FROM dbo.Challenge WHERE UniqueID = @UniqueID)
	
	if @ID is null
	BEGIN
		declare @InsertedValues Table ( ID int, Created datetime, LastChanged datetime  )
	
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
			Inserted.LastChanged
		INTO
			@InsertedValues (ID, Created, LastChanged)
		VALUES
		(
			@UniqueID,
			@Name,
			@Author,
			@Environment,
			@Races
		)
			
		Select * FROM @InsertedValues
	END
	ELSE
	BEGIN
		Select
			ID,
			Created,
			LastChanged
		FROM
			dbo.Challenge with(nolock)
		WHERE
			ID = @ID
	END
END

