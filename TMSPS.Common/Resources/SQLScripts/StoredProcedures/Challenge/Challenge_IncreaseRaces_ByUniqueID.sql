CREATE PROCEDURE [dbo].[Challenge_IncreaseRaces_ByUniqueID]
	@UniqueID varchar(27)
AS
BEGIN
	declare @races int
	set @races = (Select Races FROM dbo.Challenge with(nolock) WHERE UniqueID = @UniqueID)
	
	if @races is not null
	BEGIN
		set @races = @races + 1
		update Races set Races = @races WHERE UniqueID = @UniqueID
	END
	
	select @races
END
