using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TMSPS.Core.PluginSystem.Plugins
{
    internal partial class TMSPSCorePlugin
    {
        //const string COMMAND_RESTART_SERVER = "restartserver";

        private void HandleCommand(ServerCommand command)
        {
            switch (command.MainCommand.ToLower(Context.Culture))
            {
                //case COMMAND_RESTART_SERVER:
                //    HandleRestartServerCommand(command);
                //    break;
            }
        }

        //private void HandleRestartServerCommand(ServerCommand command)
        //{
        //    int timeoutInSeconds = 0;

        //    if (command.PartsWithoutMainCommand.Count > 0)
        //        int.TryParse(command.PartsWithoutMainCommand[0], NumberStyles.None, Context.Culture, out timeoutInSeconds);

        //    Context.RPCClient.Methods.StopServer();
        //    Context.RPCClient.Methods.StartServerInternet(Context.ServerInfo.ServerLogin, Context.ServerInfo.ServerLoginPassword);
        //    //Context.RPCClient.Methods.StartServerLan();
        //}
    }
}
