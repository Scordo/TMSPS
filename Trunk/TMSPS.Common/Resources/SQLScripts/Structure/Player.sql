CREATE TABLE [dbo].[Player](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Login] [nvarchar](50) NOT NULL,
	[Nickname] [nvarchar](50) NOT NULL,
	[Wins] [int] NOT NULL,
	[TimePlayed] [bigint] NOT NULL,
	[Created] [datetime] NOT NULL,
	[LastChanged] [datetime] NULL,
	[LastTimePlayedChanged] [datetime] NOT NULL,
 CONSTRAINT [PK_Player] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Player] ADD  CONSTRAINT [DF_Player_Wins]  DEFAULT ((0)) FOR [Wins]
GO

ALTER TABLE [dbo].[Player] ADD  CONSTRAINT [DF_Player_TimePlayed]  DEFAULT ((0)) FOR [TimePlayed]
GO

ALTER TABLE [dbo].[Player] ADD  CONSTRAINT [DF_Player_Created]  DEFAULT (getdate()) FOR [Created]
GO

ALTER TABLE [dbo].[Player] ADD  CONSTRAINT [DF_Player_LastTimePlayedChanged]  DEFAULT (getdate()) FOR [LastTimePlayedChanged]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_UniqueLogin] ON [dbo].[Player] 
(
	[Login] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Login] ON [dbo].[Player] 
(
	[Login] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO