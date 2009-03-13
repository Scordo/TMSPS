CREATE PROCEDURE [dbo].[Player_UpdateTimePlayed_List]
	@Logins nvarchar(max)
AS
BEGIN
	declare @loginsXML xml; set @loginsXML = @Logins
	declare @currentdate datetime
	set @currentdate = getdate()

	UPDATE 
		P
	SET
		TimePlayed = TimePlayed + DateDiff(ms, LastTimePlayedChanged, @currentdate),
		LastTimePlayedChanged = @currentdate,
		LastChanged = @currentdate
	FROM
		dbo.Player P
	INNER JOIN
	(
		SELECT 
			ParamValues.ID.value('.','nvarchar(100)') AS [Login]
		FROM 
			@loginsXML.nodes('/l/i') as ParamValues(ID) 
	) LoginTable ON LoginTable.[Login] = P.[Login]
END