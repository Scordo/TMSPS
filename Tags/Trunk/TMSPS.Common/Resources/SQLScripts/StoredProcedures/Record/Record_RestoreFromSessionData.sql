CREATE PROCEDURE [dbo].[Record_RestoreFromSessionData]
	@challengeID int
AS
BEGIN

	declare @dataTable table(PlayerID int, BestRecordTime int, BestSessionTime int)

	INSERT INTO 
		@dataTable
	Select 
		S.PlayerID, 
		(Select Min(TimeOrScore) From Record with(nolock) Where ChallengeID = @challengeID And PlayerID = s.PlayerID) as BestRecordTime,
		(Select Min(TimeOrScore) From Session with(nolock) Where ChallengeID = @challengeID And PlayerID = s.PlayerID) as BestSessionTime
	FROM 
		(Select Distinct PlayerID from Session with(nolock) Where ChallengeID = @challengeID) S 
	    

	INSERT INTO Record (PlayerID, ChallengeID, TimeOrScore, Created, LastChanged)
	Select 
		PlayerID,
		@challengeID,
		BestSessionTime as TimeOrScore,
		getdate() as Created,
		getdate() as LastChanged
	From 
		@dataTable 
	WHERE 
		BestRecordTime is null
		
	UPDATE 
		Record
	SET 
		TimeOrScore = NR.BestSessionTime 
	FROM 
		Record R
	INNER JOIN 
	(
		Select 
			PlayerID,
			@challengeID as ChallengeID,
			BestSessionTime
		FROM
			@dataTable
		WHERE
			BestRecordTime is not null AND
			BestSessionTime < BestRecordTime
	) NR ON R.ChallengeID = NR.ChallengeID AND R.PlayerID = NR.PlayerID 
	
	exec dbo.Ranking_UpdateForChallenge @challengeID
END
