CREATE PROCEDURE dbo.Player_DeserializeList
	@top int,
	@sorting int,
	@asc bit
AS
BEGIN
	declare @sql nvarchar(max)
	set @sql = 'Select top ' + Cast(@top as nvarchar(max)) + ' * from dbo.Player'
	
	if @sorting = 0
		set @sql = @sql + ' order by Wins'
	
	if @sorting = 1
		set @sql = @sql + ' order by TimePlayed'	
		
	if @asc = 0
		set @sql = @sql + ' desc'
		
	exec sp_executesql @sql
END