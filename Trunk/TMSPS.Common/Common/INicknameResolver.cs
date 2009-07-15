using System.Xml.Linq;
using TMSPS.Core.PluginSystem;

namespace TMSPS.Core.Common
{
    public interface INicknameResolver
    {
        void Init(PluginHostContext context, XElement configElement);
        string Get(string login);
        string Get(string login, bool returnLoginOnFailure);
        void Set(string login, string nickname);
    }
}