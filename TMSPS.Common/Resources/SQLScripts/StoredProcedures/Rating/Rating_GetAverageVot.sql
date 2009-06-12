CREATE PROCEDURE Rating_GetAverageVote
	@ChallengeID int
AS
BEGIN
	Select
		Top 1 AVG(Cast(Value as float))
	FROM
		dbo.Rating with(nolock)
	WHERE
		ChallengeID = @ChallengeID
END