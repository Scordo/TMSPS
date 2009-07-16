using System.Collections.Generic;

namespace TMSPS.Core.Communication.ResponseHandling
{
    public class GenericListResponse<T> : ResponseBase<GenericListResponse<T>>
    {
        #region Properties

        [RPCParam(0)]
        public List<T> Value
        {
            get; set;
        }

        #endregion
    }
}