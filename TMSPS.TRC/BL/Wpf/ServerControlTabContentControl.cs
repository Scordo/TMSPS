using System;
using System.Windows.Controls;
using TMSPS.Core.Communication;
using TMSPS.TRC.BL.Configuration;

namespace TMSPS.TRC.BL.Wpf
{
    public class ServerControlTabContentControl : UserControl
    {
        #region Proeprties

        public ServerControlDataContext Context
        {
            get
            {
                Func<ServerControlDataContext> getDataContext = () => (ServerControlDataContext)DataContext;

                if (Dispatcher.CheckAccess())
                    return getDataContext();

                return (ServerControlDataContext)Dispatcher.Invoke(getDataContext, null);
            }
        }

        public TrackManiaRPCClient RPCClient { get { return Context.RPCClient; } }
        public ServerInfo ServerInfo { get { return Context.ServerInfo; } }

        #endregion

        #region Public Methods

        public virtual void DoWork()
        {
        }

        public virtual void StopWork()
        {
        }

        #endregion
    }
}