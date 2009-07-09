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
using System.Windows.Shapes;
using TMSPS.TRC.BL.Configuration;

namespace TMSPS.TRC.Forms
{
    /// <summary>
    /// Interaction logic for ManageServersForm.xaml
    /// </summary>
    public partial class ManageServersForm 
    {
        private ActionMode Mode { get; set; }
        public ServerInfoList Servers { get { return DataContext as ServerInfoList; } }

        
        public ManageServersForm()
        {
            InitializeComponent();
            Mode = ActionMode.ServerView;
            DataContextChanged += ManageServersForm_DataContextChanged;
        }

        private void ManageServersForm_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Servers != null)
                Servers.CollectionChanged += Servers_CollectionChanged;

            HandleDataContextChanged();
        }

        private void Servers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HandleDataContextChanged();
        }

        private void HandleDataContextChanged()
        {
            ManageServerControlUI.IsEnabled = (Servers != null && Servers.Count > 0);
            DeleteServerButton.IsEnabled = (ServerListBox.SelectedValue != null);
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServerListBox.SelectedItem == null)
                return;

            ServerInfo modifiedServerInfo = ManageServerControlUI.GetValidServerInfo();

            if (modifiedServerInfo == null)
                return;

            ((ServerInfo)ServerListBox.SelectedItem).CopyFrom(modifiedServerInfo);
            Mode = ActionMode.ServerView;
            ManageServerControlUI.WasModified = false;
            ManageServerControlUI.HasChangedAndIsValid = false;
            EnsureCorrectButtonStates();
        }

        private void ServerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && Mode == ActionMode.ServerAddition)
            {
                Servers.Remove((ServerInfo) e.RemovedItems[0]);
                Mode = ActionMode.ServerView;
            }

            ServerInfo currentServerInfo = ServerListBox.SelectedValue as ServerInfo;
            ManageServerControlUI.DataContext = currentServerInfo == null ? null : currentServerInfo.Clone();

            EnsureCorrectButtonStates();
        }

        private void AddServerButton_Click(object sender, RoutedEventArgs e)
        {
            ServerInfo newServer = new ServerInfo();
            Servers.Add(newServer);

            ServerListBox.SelectedIndex = ServerListBox.Items.Count - 1;
            Mode = ActionMode.ServerAddition;
            EnsureCorrectButtonStates();
        }

        private void LeaveDialogButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DeleteServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == ActionMode.ServerAddition)
                Mode = ActionMode.ServerView;

            Servers.Remove((ServerInfo) ServerListBox.SelectedItem);
        }

        private void DiscardChangesButton_Click(object sender, RoutedEventArgs e)
        {
            ManageServerControlUI.Reset();
            EnsureCorrectButtonStates();
        }

        private void EnsureCorrectButtonStates()
        {
            AddServerButton.IsEnabled = (Mode != ActionMode.ServerAddition);
            DeleteServerButton.IsEnabled = (ServerListBox.SelectedValue != null);
        }
    }

    public enum ActionMode
    {
        ServerView,
        ServerAddition,
        ServerModification,
    }
}
