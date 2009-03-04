ALTER PROCEDURE [dbo].[Challenge_DeleteTracksNotInProvidedList]
	@uniqueTrackIDs nvarchar(max)
AS
BEGIN
	declare @ids xml; set @ids = @uniqueTrackIDs
	declare @challenegeIDsToDelete table(ID int)

	INSERT INTO 
		@challenegeIDsToDelete
	Select
		ID
	FROM
		dbo.challenge C with(nolock)
	LEFT JOIN
	(
		SELECT 
			ParamValues.ID.value('.','char(27)') AS ParamID
		FROM 
			@ids.nodes('/l/i') as ParamValues(ID) 
	) IDs ON IDs.ParamID = C.UniqueID
	WHERE 
		IDs.ParamID is null


	DELETE [Session] FROM [Session] S INNER JOIN @challenegeIDsToDelete CID on S.ChallengeId = CID.ID
	DELETE [Record] FROM [Record] R INNER JOIN @challenegeIDsToDelete CID on R.ChallengeId = CID.ID
	DELETE [Rating] FROM [Rating] R INNER JOIN @challenegeIDsToDelete CID on R.ChallengeId = CID.ID
	DELETE [Ranking] FROM [Ranking] R INNER JOIN @challenegeIDsToDelete CID on R.ChallengeId = CID.ID
	DELETE [Position] FROM [Position] P INNER JOIN @challenegeIDsToDelete CID on P.ChallengeId = CID.ID
	DELETE [Challenge] FROM [Challenge] C INNER JOIN @challenegeIDsToDelete CID on C.ID = CID.ID

END