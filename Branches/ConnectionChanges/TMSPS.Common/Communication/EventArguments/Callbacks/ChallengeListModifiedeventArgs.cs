using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class ChallengeListModifiedeventArgs : EventArgsBase<ChallengeListModifiedeventArgs>
    {
        [RPCParam(0)]
        public int CurrentChallengeIndex { get; set; }
        [RPCParam(1)]
        public int NextChallengeIndex { get; set; }
        [RPCParam(2)]
        public bool IsListModified { get; set; }
    }
}