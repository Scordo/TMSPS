CREATE Procedure [dbo].[Record_DeleteFalseRecords]
	@challengeID int,
	@possibleTimeOrScore int
AS
BEGIN
	print 'Removing  Records and Sessions'
	delete from record where challengeID = @challengeID and TimeOrScore < @possibleTimeOrScore
	delete from session where challengeID = @challengeID and TimeOrScore < @possibleTimeOrScore

	print 'Restore Session Records and updating Ranks'
	exec dbo.Record_RestoreFromSessionData @challengeID
END
