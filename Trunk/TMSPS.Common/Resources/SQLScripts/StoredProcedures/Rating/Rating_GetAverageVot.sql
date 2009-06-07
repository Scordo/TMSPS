CREATE PROCEDURE [dbo].[Rating_GetAverageVote]
	@ChallengeID int
AS
BEGIN
	Select
		Top 1 AVG(Value)
	FROM
		dbo.Rating with(nolock)
	WHERE
		ChallengeID = @ChallengeID
END