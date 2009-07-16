CREATE PROCEDURE [dbo].[Challenge_IncreaseRaces_ByID]
	@ID int
AS
BEGIN
	declare @races int
	set @races = (Select Races FROM dbo.Challenge with(nolock) WHERE ID = @ID)
	
	if @races is not null
	BEGIN
		set @races = @races + 1
		update Races set Races = @races WHERE ID = @ID
	END
	
	select @races
END