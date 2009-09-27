CREATE PROCEDURE [dbo].[Rating_GetAverageVote]
	@ChallengeID int
AS
BEGIN
	Select
		Top 1 AVG(Cast(Value as float)) as AverageVote,
		Count(Value) as VotesCount
	FROM
		dbo.Rating with(nolock)
	WHERE
		ChallengeID = @ChallengeID
END