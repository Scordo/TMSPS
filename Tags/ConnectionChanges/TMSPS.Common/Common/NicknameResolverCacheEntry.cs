using System;

namespace TMSPS.Core.Common
{
    public class NicknameResolverCacheEntry
    {
        private string _nickname;
        public string Nickname 
        {
            get
            {
                LastAccessed = DateTime.Now;
                return _nickname;
            }
            set
            {
                _nickname = value;
                LastAccessed = DateTime.Now;
            }
        }
        public DateTime LastAccessed { get; set; }

        public NicknameResolverCacheEntry()
        {
            LastAccessed = DateTime.Now;
        }

        public NicknameResolverCacheEntry(string nickname)
        {
            Nickname = nickname;
        }
    }
}