CREATE PROCEDURE [dbo].[Player_DeserializeList_Paged_ByWins]
	@StartIndex int,
    @EndIndex int,
    @VirtualCount int OUTPUT
AS
BEGIN
	DECLARE @countSQL nvarchar(max)
    DECLARE @querySQL nvarchar(max)
    DECLARE @baseQuery nvarchar(max)

	SET @baseQuery = N'
		With MainQuery as
		(
			SELECT 
				Row_Number() Over(ORDER BY Wins Desc) as RowNumber,
				*
			FROM
				dbo.Player
			WHERE 
				1 = 1'

	SET @baseQuery = @baseQuery + ') '
	SET @querySQL = @baseQuery + 'Select * FROM MainQuery WHERE 1 = 1'
	SET @countSQL = @baseQuery + 'SELECT @VirtualCountParam = Count(*) FROM MainQuery'

	IF ((@StartIndex IS NOT NULL) AND (@StartIndex >= 0))
	    SET @querySQL = @querySQL + ' AND RowNumber >= ' + cast(@StartIndex AS nvarchar(max))

    IF ((@EndIndex IS NOT NULL) AND (@EndIndex >= 0))
		SET @querySQL = @querySQL + ' AND RowNumber <= ' + cast(@EndIndex AS nvarchar(max))

	DECLARE @CountParamDefinition nvarchar(max);
	SET @CountParamDefinition = N'@VirtualCountParam int output'

	EXEC sp_executesql @countSQL, @CountParamDefinition, @VirtualCountParam  = @VirtualCount OUTPUT
	EXEC sp_executesql @querySQL
END