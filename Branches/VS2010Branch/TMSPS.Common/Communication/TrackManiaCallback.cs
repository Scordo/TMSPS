namespace TMSPS.Core.Communication
{
    public enum TrackManiaCallback
    {
        PlayerConnect,
        PlayerDisconnect,
        PlayerChat,
        PlayerManialinkPageAnswer,
        Echo,
        ServerStart,
        ServerStop,
        BeginRace,
        EndRace,
        BeginRound,
        EndRound,
        StatusChanged,
        PlayerCheckpoint,
        PlayerFinish,
        PlayerIncoherence,
        BillUpdated,
        TunnelDataReceived,
        ChallengeListModified,
        PlayerInfoChanged,
        ManualFlowControlTransition,
        BeginChallenge,
        EndChallenge
    }
}