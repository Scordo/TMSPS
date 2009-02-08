CREATE PROCEDURE dbo.Record_TryInsertOrUpdate
	@Login nvarchar(50),
	@ChallengeID int,
	@TimeOrScore int
AS
BEGIN
	declare @PlayerID int
	set @PlayerID = (Select ID FROM dbo.Player with(nolock) WHERE Login = @Login)
	
	declare @oldPosition int
	declare @newPosition int
	declare @oldTimeOrScore int
	
	if @PlayerID is not null
	BEGIN
		;with RanksOld as
		(
			Select
				ROW_NUMBER() OVER (order by TimeOrScore asc, LastChanged asc) RowNr, *
			FROM
				dbo.Record with (nolock)
		)
		
		Select  
			@oldPosition = RowNr,
			@oldTimeOrScore = TimeOrScore 
		From 
			RanksOld with (nolock) 
		WHERE 
			PlayerID = @PlayerID AND
			ChallengeID = @ChallengeID
		
		if @oldPosition is not null
		BEGIN						
			Update
				dbo.Record			
			SET
				TimeOrScore = @TimeOrScore,
				LastChanged = getdate()
			WHERE
				PlayerID = @PlayerID AND
				ChallengeID = @ChallengeID AND
				TimeOrScore > @TimeOrScore
		END
		ELSE
		BEGIN
			INSERT INTO dbo.Record
			(
				PlayerID,
				ChallengeID,
				TimeOrScore
			)
			VALUES
			(
				@PlayerID,
				@ChallengeID,
				@TimeOrScore
			)
		END
		
		;with RanksNew as
		(
			Select
				ROW_NUMBER() OVER (order by TimeOrScore asc, LastChanged asc) RowNr, *
			FROM
				dbo.Record with (nolock)
		)
		
		Select  
			@newPosition = RowNr 
		From 
			RanksNew
		WHERE 
			PlayerID = @PlayerID AND
			ChallengeID = @ChallengeID
	END
	
	
	declare @newBest bit
	set @newBest = 0
		
	if (@PlayerID is not null) AND ((@oldTimeOrScore is null) OR (@TimeOrScore < @oldTimeOrScore))
		set @newBest = 1
		
	Select
		@oldPosition as OldPosition,
		@newPosition as NewPosition,
		@newBest as NewBest	
END