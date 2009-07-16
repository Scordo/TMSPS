using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments
{
    public class FaultResponseEventArgs
    {
        #region Properties

        [RPCParam("faultString")]
        public string FaultMessage
        {
            get; set;
        }

        [RPCParam("faultCode")]
        public int FaultCode
        {
            get; set;
        }

        #endregion

        public static FaultResponseEventArgs GetFromXml(XElement element)
        {
            XElement changedParamsMessageElement = new XElement(element);
            changedParamsMessageElement.Descendants("int").ToList().ForEach(localElement => localElement.Name = "i4");
            FaultResponseEventArgsContainer result = new FaultResponseEventArgsContainer();
            List<XElement> paramElements = new List<XElement> { changedParamsMessageElement };
            List<RPCParamInfo> parameters = RPCParamInfo.GetFromType(typeof(FaultResponseEventArgsContainer), RPCParamInfo.RevtrievalMode.OnlyWithIndexes);

            RPCCommand.GetParameterElementInstance(parameters, 0, paramElements[0], result);
            return result.Fault;
        }

        class FaultResponseEventArgsContainer
        {
            [RPCParam(0)]
            public FaultResponseEventArgs Fault { get; set; }
        }
    }
}
