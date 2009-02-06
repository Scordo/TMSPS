CREATE PROCEDURE dbo.Challenge_Deserialize_ByID
	@ID int
AS
BEGIN
	Select
		*
	FROM
		dbo.Challenge with(nolock)
	WHERE
		ID = @ID	
END