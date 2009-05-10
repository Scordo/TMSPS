using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.PluginSystem;

namespace TMSPS.Core.Common
{
    public abstract class NicknameResolverBase : INicknameResolver
    {
        #region Properties

        protected PluginHostContext Context { get; private set;}
        protected Dictionary<string, NicknameResolverCacheEntry> NicknameCache { get; private set; }

        #endregion


        #region INicknameResolver Members

        public void Init(PluginHostContext context, XElement configElement)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            Context = context;
            NicknameCache = new Dictionary<string, NicknameResolverCacheEntry>();
            ReadConfigSettings(configElement);
        }

        public abstract void ReadConfigSettings(XElement configElement);
        public abstract string Get(string login);
        public abstract void Set(string login, string nickname);

        protected void UpdateCacheForLogin(string login, string nickname)
        {
            if (NicknameCache.ContainsKey(login))
                NicknameCache[login].Nickname = nickname;
            else
                NicknameCache[login] = new NicknameResolverCacheEntry(nickname);
        }

        #endregion
    }
}