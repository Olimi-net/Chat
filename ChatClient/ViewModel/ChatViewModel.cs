using ChatClient.Helper;
using ChatClient.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatClient.ViewModel
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public const string IncorrectLogginOrPassword = "Недопустимая пара логин-пароль";
        public const string DuplicateLogin = "Такой логин уже занят";

        public event PropertyChangedEventHandler PropertyChanged;

        private ChatModel model;
        private bool _canUseChat;
        private bool _auth;
        private string _actualUser;
        private string _lastError;
        private ObservableCollection<LogInfo> _chatLog;
        private ObservableCollection<string> _listUsers;

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

        public bool CanUseChat
        {
            get { return _canUseChat; }
            set { _canUseChat = value; NotifyPropertyChanged(); }
        }

        public bool IsAuth
        {
            get { return _auth; }
            set { _auth = value; NotifyPropertyChanged(); }
        }

        public string ActualUser
        {
            get { return _actualUser; }
            set { _actualUser = value; NotifyPropertyChanged(); }
        }

        public string LastError
        {
            get { return _lastError; }
            set { _lastError = value; NotifyPropertyChanged(); }
        }


        public string UserLogin { get; set; }

        public string NewLogin { get; set; }

        public string NewMessage { get; set; }

        public RelayCommand SendMessage { get; set; }
        public RelayCommand Invite { get; set; }
        public RelayCommand CheckIn { get; set; }

        public ChatViewModel()
        {
            IsAuth = true;
            model = new ChatModel();
            SendMessage = new RelayCommand(OnSendMessage);
            Invite = new RelayCommand(OnInvite);
            CheckIn = new RelayCommand(OnCheckIn);
            ChatLog = new ObservableCollection<LogInfo>();
            ListUsers = new ObservableCollection<string>();
            model.MsgListUpdate += model_MsgListUpdate;
            model.UserListUpdate += model_UserListUpdate;
            model.InfoMessage += model_InfoMessage;
        }

        void model_InfoMessage(object obj)
        {
            Tuple<int, string> info = (Tuple<int, string>)obj;
            if (info.Item1 == 0)
            {
                IsAuth = false;
                CanUseChat = true;
                ActualUser = info.Item2;
            }
            else if (info.Item1 == (int)ChatCode.Forbidden)
            {
                return;
            }
            else if (info.Item1 == (int)ChatCode.Conflict)
            {
                LastError = info.Item2;
                IsAuth = true;
                CanUseChat = false;
            }
            else if (info.Item1 == (int)ChatCode.Unauthorized)
            {
                LastError = info.Item2;
                IsAuth = true;
                CanUseChat = false;
            }
        }

        void model_UserListUpdate(object obj)
        {
            List<string> list = (List<string>)obj;
            ListUsers = new ObservableCollection<string>(list);
        }

        void model_MsgListUpdate(object obj)
        {
            List<LogInfo> list = (List<LogInfo>)obj;
            ChatLog = new ObservableCollection<LogInfo>(list);
        }

        public void OnCheckIn(object obj)
        {
            PasswordBox pass = (PasswordBox)obj;
            model.CheckIn(UserLogin, pass.Password);
        }

        private void OnInvite(object obj)
        {
            PasswordBox pass = (PasswordBox)obj;
            model.Invite(NewLogin, pass.Password);
        }

        private void OnSendMessage(object obj)
        {
            TextBox tb = (TextBox)obj;
            model.SendMessage(tb.Text);
            tb.Text = "";
        }

        private void NotifyPropertyChanged(String propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void Close()
        {
            model.Close();
        }
    }
}
