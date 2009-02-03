using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;

namespace TMSPS.Core.Communication.ResponseHandling
{
    [DebuggerDisplay("Name: {MemberName}, Index: {Index}")]
    public class RPCParamInfo
    {
        public enum RevtrievalMode { OnlyWithIndexes, OnlyWithMemberName}

        #region Properties

        public int Index { get; private set; }
        public string MemberName { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        #endregion

        #region Public Methods

        public static List<RPCParamInfo> GetFromType(Type type, RevtrievalMode revtrievalMode)
        {
            List<RPCParamInfo> result = new List<RPCParamInfo>();

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                RPCParamAttribute[] attributes = (RPCParamAttribute[])propertyInfo.GetCustomAttributes(typeof(RPCParamAttribute), false);

                if (attributes.Length == 0)
                    continue;

                RPCParamAttribute attribute = attributes[0];

                if (revtrievalMode == RevtrievalMode.OnlyWithIndexes && attribute.MemberName != null)
                    continue;

                if (revtrievalMode == RevtrievalMode.OnlyWithMemberName && attribute.MemberName == null)
                    continue;

                RPCParamInfo paramInfo = new RPCParamInfo();
                paramInfo.Index = attribute.Index;
                paramInfo.MemberName = attribute.MemberName;
                paramInfo.PropertyInfo = propertyInfo;

                result.Add(paramInfo);
            }

            result.Sort((x, y) => x.Index - y.Index);

            return result;
        }

        

        #endregion
    }
}