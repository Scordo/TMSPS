CREATE PROCEDURE [dbo].[Ranking_ReCreateRanks]
AS
BEGIN
	declare @challengesCount int
	Select @challengesCount = Count(ID) FROM Challenge with (nolock)

	Truncate Table dbo.[Rank]

	INSERT INTO 
		dbo.[Rank]
	Select
		PlayerID,
		ROW_NUMBER() OVER (order by (AVG(Cast(Rank as decimal(15,4))) + cast((@challengesCount+1) as decimal(19,2)) / cast((Count([Rank])+1)as decimal(19,2)) * cast((@challengesCount - Count([Rank])) as decimal(19,2))) ASC) [Rank],
		AVG(Cast(Rank as decimal(15,4))) as AverageRank,
		Count([Rank]) as RecordsCount,
		@challengesCount as ChallengesCount,
		(AVG(Cast(Rank as decimal(15,4))) + cast((@challengesCount+1) as decimal(19,2)) / cast((Count([Rank])+1)as decimal(19,2)) * cast((@challengesCount - Count([Rank])) as decimal(19,2))) as Score
	From
		Ranking with (nolock)
	GROUP By 
		PlayerID
END