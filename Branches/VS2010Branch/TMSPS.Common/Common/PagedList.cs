using System.Collections.Generic;

namespace TMSPS.Core.Common
{
    public class PagedList<T> : List<T>
    {
        #region Properties

        public int VirtualCount { get; set; }
        public int StartIndex { get; set; }

        #endregion

        #region Constructors

        public PagedList()
        {
            
        }

        public PagedList(IEnumerable<T> collection) : base(collection)
        {

        }

        public PagedList(int capacity) : base(capacity)
        {

        }

        #endregion
    }
}