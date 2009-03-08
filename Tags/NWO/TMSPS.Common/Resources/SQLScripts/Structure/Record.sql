CREATE TABLE [dbo].[Record](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PlayerID] [int] NOT NULL,
	[ChallengeID] [int] NOT NULL,
	[TimeOrScore] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastChanged] [datetime] NULL,
 CONSTRAINT [PK_Record] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Record]  WITH CHECK ADD  CONSTRAINT [FK_Record_Challenge] FOREIGN KEY([ChallengeID])
REFERENCES [dbo].[Challenge] ([ID])
GO

ALTER TABLE [dbo].[Record] CHECK CONSTRAINT [FK_Record_Challenge]
GO

ALTER TABLE [dbo].[Record]  WITH CHECK ADD  CONSTRAINT [FK_Record_Player] FOREIGN KEY([PlayerID])
REFERENCES [dbo].[Player] ([ID])
GO

ALTER TABLE [dbo].[Record] CHECK CONSTRAINT [FK_Record_Player]
GO

ALTER TABLE [dbo].[Record] ADD  CONSTRAINT [DF_Record_Created]  DEFAULT (getdate()) FOR [Created]
GO


CREATE UNIQUE NONCLUSTERED INDEX [IX_UniqueRecord] ON [dbo].[Record] 
(
	[ChallengeID] ASC,
	[PlayerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO