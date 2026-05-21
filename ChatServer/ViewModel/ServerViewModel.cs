using ChatServer.Helper;
using ChatServer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Windows.Controls;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatServer.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LogInfo> _chatLog;
        public ObservableCollection<string> _listUsers;

        public ObservableCollection<LogInfo> ChatLog
        {
            get { return _chatLog; }
            set { _chatLog = value; NotifyPropertyChanged(); }
        }
        public ObservableCollection<string> ListUsers
        {
            get { return _listUsers; }
            set { _listUsers = value; NotifyPropertyChanged(); }
        }
        public RelayCommand SendMessage { get; set; }

        private ServerModel _serverModel;
        
        public ServerViewModel()
        {
            ChatLog = new ObservableCollection<LogInfo>();
            ListUsers = new ObservableCollection<string>();
            SendMessage = new RelayCommand(OnSendMessage);
            _serverModel = new ServerModel();
            _serverModel.EventUpdateMessages += ServerModel_EventUpdateMessages;
            _serverModel.EventUpdateUsers += ServerModel_EventUpdateUsers;
        }

        void ServerModel_EventUpdateUsers(object obj)
        {
            List<UserInfo> users = (List<UserInfo>)obj;
            ListUsers = new ObservableCollection<string>(users.Where(x => x.Online).Select(x => x.Login));
        }

        void ServerModel_EventUpdateMessages(object obj)
        {
            List<LogInfo> log = (List<LogInfo>)obj;
            ChatLog = new ObservableCollection<LogInfo>(log.OrderBy(x => x.Id));
        }

        private void OnSendMessage(object obj)
        {
            TextBox tb = (TextBox)obj;
            _serverModel.Send(tb.Text);
            tb.Text = string.Empty;
        }

        public void OnCloseWindow()
        {
            _serverModel.Stop();
        }        

        private void NotifyPropertyChanged(String propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
