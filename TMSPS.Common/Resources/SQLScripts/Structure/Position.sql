﻿CREATE TABLE [dbo].[Position](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PlayerID] [int] NOT NULL,
	[ChallengeID] [int] NOT NULL,
	[OwnPosition] [smallint] NOT NULL,
	[MaxPosition] [smallint] NOT NULL,
	[Created] [datetime] NOT NULL,
 CONSTRAINT [PK_Position] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Position]  WITH CHECK ADD  CONSTRAINT [FK_Position_Challenge] FOREIGN KEY([ChallengeID])
REFERENCES [dbo].[Challenge] ([ID])
GO

ALTER TABLE [dbo].[Position] CHECK CONSTRAINT [FK_Position_Challenge]
GO

ALTER TABLE [dbo].[Position]  WITH CHECK ADD  CONSTRAINT [FK_Position_Player] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Player] ([ID])
GO

ALTER TABLE [dbo].[Position] CHECK CONSTRAINT [FK_Position_Player]
GO

ALTER TABLE [dbo].[Position] ADD  CONSTRAINT [DF_Position_Created]  DEFAULT (getdate()) FOR [Created]
GO


CREATE NONCLUSTERED INDEX [IX_PlayerID] ON [dbo].[Position] 
(
	[PlayerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_ChallengeID] ON [dbo].[Position] 
(
	[ChallengeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

