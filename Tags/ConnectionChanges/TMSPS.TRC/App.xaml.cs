using System;
using System.IO;
using System.Windows;
using TMSPS.TRC.BL.Configuration;

namespace TMSPS.TRC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        #region Constants

        public const string PROFILE_FILE_EXTENSION = "profile";
        public const string PROFILE_SUBFOLDER_NAME = "Profiles";

        #endregion

        #region Properties

        public string AppDirectory { get; private set; }
        public string ProfilesDirectory { get; private set; }
        public MainForm MainForm { get { return (MainForm) Windows[0]; } }
        public Profile CurrentProfile { get { return MainForm.CurrentProfile; } }

        #endregion

        #region Constructor

        public App()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ProfilesDirectory = Path.Combine(AppDirectory, PROFILE_SUBFOLDER_NAME);
        }

        #endregion

        #region Public Methods

        public void Log(string message)
        {
            MainForm.Log(message);
        }

        #endregion
    }
}