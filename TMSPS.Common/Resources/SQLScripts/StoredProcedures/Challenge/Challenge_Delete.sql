CREATE PROCEDURE [dbo].[Challenge_Delete]
	@challengeUniqueID varchar(27)
AS
	declare @challengeID int
	SET @challengeID = (Select Top 1 ID FROM dbo.Challenge with(nolock) WHERE UniqueID = @challengeUniqueID)

	DELETE [Session] WHERE ChallengeId = @challengeID
	DELETE [Record] WHERE ChallengeId = @challengeID
	DELETE [Rating] WHERE ChallengeId = @challengeID
	DELETE [Ranking] WHERE ChallengeId = @challengeID
	DELETE [Position] WHERE ChallengeId = @challengeID
	DELETE [Challenge] WHERE ID = @challengeID