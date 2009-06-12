CREATE PROCEDURE [dbo].[Ranking_ReCreateAll]
AS
BEGIN
	declare @challengeID int
	declare ChallengeIDs cursor for
		SELECT ID from dbo.Challenge with (nolock)

	OPEN ChallengeIDs

	FETCH NEXT FROM ChallengeIDs 
	INTO @challengeID

	WHILE @@FETCH_STATUS = 0
	BEGIN
	  exec Ranking_UpdateForChallenge @challengeID
		
	  FETCH NEXT FROM ChallengeIDs INTO @challengeID
	END

	CLOSE ChallengeIDs
	DEALLOCATE ChallengeIDs
END
