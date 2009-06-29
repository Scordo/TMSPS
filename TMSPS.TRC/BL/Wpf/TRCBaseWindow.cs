using System.Windows;

namespace TMSPS.TRC.BL.Wpf
{
    public class TRCBaseWindow : Window
    {
        public App Application { get { return (App) System.Windows.Application.Current; } }
    }
}