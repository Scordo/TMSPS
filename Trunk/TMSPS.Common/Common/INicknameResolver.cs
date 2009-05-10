using System.Xml.Linq;
using TMSPS.Core.PluginSystem;

namespace TMSPS.Core.Common
{
    public interface INicknameResolver
    {
        void Init(PluginHostContext context, XElement configElement);
        string Get(string login);
        void Set(string login, string nickname);
    }
}