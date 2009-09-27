using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class ManualFlowControlTransitionEventArgs : EventArgsBase<ManualFlowControlTransitionEventArgs>
    {
        [RPCParam(0)]
        public string Transition { get; set; }
    }
}