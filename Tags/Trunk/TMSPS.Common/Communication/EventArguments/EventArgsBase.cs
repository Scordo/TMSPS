using System;
using System.Xml.Linq;
using TMSPS.Core.Common;

namespace TMSPS.Core.Communication.EventArguments
{
    public abstract class EventArgsBase<T> : EventArgs, IDump where T : EventArgsBase<T>
    {
        #region Properties

        public bool Erroneous { get { return Fault != null; } }
        public FaultResponseEventArgs Fault { get; private set; }

        #endregion

        #region Public Methods

        public static T Parse(XElement messageElement)
        {
            return Parse(messageElement, Guid.Empty);
        }

        public static T Parse(XElement messageElement, Guid callGuid)
        {
            T result;

            XElement faultElement = messageElement.Element("fault");

            if (faultElement != null)
            {
                result = (T) Activator.CreateInstance(typeof(T));
                result.Fault = FaultResponseEventArgs.GetFromXml(faultElement);
            }
            else
            {
                result = RPCCommand.GetInstance<T>(messageElement.Element("params"));
            }

            return result;
        }

        #endregion
    }
}