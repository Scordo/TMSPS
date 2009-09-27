CREATE PROCEDURE [dbo].[Challenge_GetAllUniqueTrackIDs]
AS
	SELECT UniqueID From Challenge with (nolock)