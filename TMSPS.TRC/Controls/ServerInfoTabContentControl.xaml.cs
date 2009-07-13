using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMSPS.TRC.BL.Wpf;

namespace TMSPS.TRC.Controls
{
    /// <summary>
    /// Interaction logic for ServerInfoTabContentControl.xaml
    /// </summary>
    public partial class ServerInfoTabContentControl
    {
        public ServerInfoTabContentControl()
        {
            InitializeComponent();
        }

        public override void DoWork()
        {
            IsEnabled = true;
        }

        public override void StopWork()
        {
            IsEnabled = false;
        }
    }
}
