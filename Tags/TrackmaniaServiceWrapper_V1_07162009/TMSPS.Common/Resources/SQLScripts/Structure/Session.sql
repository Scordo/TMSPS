﻿CREATE TABLE [dbo].[Session](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PlayerID] [int] NOT NULL,
	[ChallengeID] [int] NOT NULL,
	[TimeOrScore] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
 CONSTRAINT [PK_Session] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Session]  WITH CHECK ADD  CONSTRAINT [FK_Session_Challenge] FOREIGN KEY([ChallengeID])
REFERENCES [dbo].[Challenge] ([ID])
GO

ALTER TABLE [dbo].[Session] CHECK CONSTRAINT [FK_Session_Challenge]
GO

ALTER TABLE [dbo].[Session]  WITH CHECK ADD  CONSTRAINT [FK_Session_Player] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Player] ([ID])
GO

ALTER TABLE [dbo].[Session] CHECK CONSTRAINT [FK_Session_Player]
GO

ALTER TABLE [dbo].[Session] ADD  CONSTRAINT [DF_Session_Created]  DEFAULT (getdate()) FOR [Created]
GO

CREATE NONCLUSTERED INDEX [IX_ChallengeID] ON [dbo].[Session] 
(
	[ChallengeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_PlayerID_ChallengeID] ON [dbo].[Session] 
(
	[TimeOrScore] ASC,
	[PlayerID] ASC,
	[ChallengeID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO