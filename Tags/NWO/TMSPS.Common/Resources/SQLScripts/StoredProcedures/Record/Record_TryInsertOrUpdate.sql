﻿USE [Aseco_NWO]
GO
/****** Object:  StoredProcedure [dbo].[Record_TryInsertOrUpdate]    Script Date: 04/26/2009 03:23:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[Record_TryInsertOrUpdate]
	@Login nvarchar(100),
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
			WHERE
				ChallengeID = @ChallengeID
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
				TimeOrScore,
				Created,
				LastChanged
			)
			VALUES
			(
				@PlayerID,
				@ChallengeID,
				@TimeOrScore,
				getdate(),
				getdate()
			)
		END
		
		;with RanksNew as
		(
			Select
				ROW_NUMBER() OVER (order by TimeOrScore asc, LastChanged asc) RowNr, *
			FROM
				dbo.Record with (nolock)
			WHERE
				ChallengeID = @ChallengeID
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