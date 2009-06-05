CREATE PROCEDURE dbo.Challenge_Deserialize_ByUniqueID
	@UniqueID varchar(27)
AS
BEGIN
	Select
		*
	FROM
		dbo.Challenge with(nolock)
	WHERE
		UniqueID = @UniqueID
END