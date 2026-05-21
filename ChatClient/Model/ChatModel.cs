using ChatClient.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatClient.Model
{
    public class ChatModel
    {
        public delegate void EventHandler(object obj);

        public event EventHandler UserListUpdate;
        public event EventHandler MsgListUpdate;
        public event EventHandler InfoMessage;

        public bool IsLogin { get; set; }
        private ClientChat client;
        public List<string> UserList;
        public List<LogInfo> StackMessage { get; set; }

        public ChatModel()
        {
            StackMessage = new List<LogInfo>();
            UserList = new List<string>();
            client = new ClientChat("localhost", 8080);
            client.UserList += client_UserList;
            client.NewMessage += client_NewMessage;
            client.Error += client_Error;
            client.ConnectAccess += client_ConnectAccess;
        }

        void client_ConnectAccess(int code, object msg)
        {
            InfoMessage(new Tuple<int, string>(code, (string)msg));
        }

        void client_Error(int code, object msg)
        {
            InfoMessage(new Tuple<int, string>(code, (string)msg));
        }

        void client_NewMessage(int code, object obj)
        {
            List<LogInfo> msg = (List<LogInfo>)obj;
            StackMessage.AddRange(msg);
            var buffer = StackMessage.OrderByDescending(x => x.Id).Take(100);
            StackMessage = buffer.OrderBy(x => x.Id).ToList();
            MsgListUpdate(StackMessage);
        }

        void client_UserList(int code, object obj)
        {
            List<string> users = (List<string>)obj;
            UserList = users;
            UserListUpdate(UserList);
        }

        internal void Invite(string login, string password)
        {
            client.Invite(login, password);
            client.Start();
        }

        internal void CheckIn(string login, string password)
        {
            client.CheckIn(login, password);
            client.Start();
        }

        internal void SendMessage(string newMessage)
        {
            client.Send(newMessage);
        }

        internal void Close()
        {
            client.Close();
        }
    }
}
