CREATE PROCEDURE [dbo].[Player_UpdateTimePlayed_list]
	@Logins nvarchar(max)
AS
BEGIN
	declare @loginsXML xml; set @loginsXML = @Logins
	declare @currentdate datetime
	set @currentdate = getdate()

	UPDATE 
		P
	SET
		TimePlayed = TimePlayed + CASE WHEN DateDiff(ms, LastTimePlayedChanged, @currentdate) > 60000 THEN 0 ELSE DateDiff(ms, LastTimePlayedChanged, @currentdate) END,
		LastTimePlayedChanged = @currentdate,
		LastChanged = @currentdate
	FROM
		dbo.Player P with(nolock)
	INNER JOIN
	(
		SELECT 
			ParamValues.ID.value('.','nvarchar(100)') AS [Login]
		FROM 
			@loginsXML.nodes('/l/i') as ParamValues(ID) 
	) LoginTable ON LoginTable.[Login] = P.[Login]
END
