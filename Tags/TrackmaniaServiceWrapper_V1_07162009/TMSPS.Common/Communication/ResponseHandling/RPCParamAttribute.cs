using System;

namespace TMSPS.Core.Communication.ResponseHandling
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RPCParamAttribute : Attribute
    {
        public int Index { get; private set; }
        public string MemberName { get; private set; }

        public RPCParamAttribute(int index)
        {
            Index = index;
        }

        public RPCParamAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}