﻿CREATE TABLE [dbo].[Ranking](
	[ChallengeID] [int] NOT NULL,
	[PlayerID] [int] NOT NULL,
	[Rank] [int] NOT NULL,
	
	CONSTRAINT [UK_ChallengeID_PlayerID] UNIQUE NONCLUSTERED 
	(
		[ChallengeID] ASC,
		[PlayerID] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
	
	CONSTRAINT [UK_ChallengeID_Rank] UNIQUE NONCLUSTERED 
	(
		[ChallengeID] ASC,
		[Rank] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_ChallengeID] ON [dbo].[Ranking] 
(
	[ChallengeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_PlayerID] ON [dbo].[Ranking] 
(
	[PlayerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO