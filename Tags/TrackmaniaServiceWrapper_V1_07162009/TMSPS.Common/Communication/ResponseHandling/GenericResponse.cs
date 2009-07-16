namespace TMSPS.Core.Communication.ResponseHandling
{
    public class GenericResponse<T> : ResponseBase<GenericResponse<T>>
    {
        #region Properties

        [RPCParam(0)]
        public T Value
        {
            get; set;
        }

        #endregion
    }
}