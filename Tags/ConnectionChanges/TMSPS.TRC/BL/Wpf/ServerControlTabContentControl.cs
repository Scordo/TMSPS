using System;
using System.Windows;
using System.Windows.Controls;
using TMSPS.Core.Communication;
using TMSPS.TRC.BL.Configuration;

namespace TMSPS.TRC.BL.Wpf
{
    public class ServerControlTabContentControl : UserControl
    {
        #region Properties

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

        #region Dependency Properties

        public static readonly DependencyProperty IsDisconnectedProperty = DependencyProperty.Register("IsDisconnected", typeof(bool), typeof(ServerControlTabContentControl), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register("IsConnected", typeof(bool), typeof(ServerControlTabContentControl), new FrameworkPropertyMetadata(false));

        public bool IsDisconnected
        {
            get { return (bool)GetValue(IsDisconnectedProperty); }
            set { SetValue(IsDisconnectedProperty, value); }
        }

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        #endregion

        #region Public Methods

        public virtual void Reload()
        {
            
        }

        public virtual void DoWork()
        {
            IsDisconnected = false;
            IsConnected = true;
            EnableControls();
        }

        protected virtual void EnableControls()
        {
            IsEnabled = true;
        }

        public virtual void StopWork()
        {
            IsDisconnected = true;
            IsConnected = false;
            DisableControls();
        }

        protected virtual void DisableControls()
        {
            IsEnabled = false;
        }

        #endregion
    }
}