using ChatServer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatServer.Model
{
    class Server
    {
        private const string GlobalMsg = "Global message";
        public delegate void EventHandler(object obj);

        public EventHandler OnlineStatusUpdate;
        public EventHandler NewMessage;
        public EventHandler ErrorMsg;
        public EventHandler AddNewUser;

        private TcpListener _tcpListener;
        private List<LogInfo> _listMessage;
        private List<Client> _clients;
        private List<UserInfo> _users;
        private int _lastIdUser;
        private Object lockMsg = new Object();
        private Object lockUsr = new Object();  

        public Server(List<UserInfo> users)
        {
            _users = users;
            var user = _users.OrderByDescending(x => x.Id).ToList().FirstOrDefault();
            if (user != null)
            {
                _lastIdUser = user.Id;
            }
            _listMessage = new List<LogInfo>();
            _tcpListener = new TcpListener(IPAddress.Any, 8080);
            _clients = new List<Client>();
        }

        void client_ErrorMessage(object obj, Client client)
        {
            var error = (Tuple<int, string>)obj;
            ErrorMsg(error);
        }

        void client_NewMessage(object obj, Client client)
        {
            if (obj != null)
            {
                var msg = (List<LogInfo>)obj;
                NewMessages(msg);
            }
        }

        internal void Start()
        {
            _tcpListener.Start();
            while(true)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    var client = new Client(tcpClient);
                    client.NewMessage += client_NewMessage;
                    client.ErrorMessage += client_ErrorMessage;
                    client.EventClose += client_EventClose;
                    client.EventLogin += client_EventLogin;
                    client.EventGetInfo += client_EventGetInfo;
                    _clients.Add(client);
                    client.Start();
                }
                catch (Exception e)
                {
                    ErrorMsg(new Tuple<int, string>(-1,e.Message));
                }
            }
        }

        void client_EventGetInfo(object obj, Client client)
        {
            try
            {
                ChatInfo info = (ChatInfo)obj;
                lock (lockMsg)
                {
                    info.Log = _listMessage.Where(x => x.Id > info.IdMsg).Select(x=>new LogInfo(x)).ToList();                    
                }
                info.Users = _users.Where(x => x.Online).Select(x => x.Login).ToList();
            }
            catch (Exception ex)
            {
                ErrorMsg(new Tuple<int, string>(-2, ex.Message));
            }
        }

        void client_EventLogin(object obj, Client client)
        {
            var info = (Tuple<int, UserInfo>)obj;

            if (!info.Item2.IsValid())
            {
                client.SetUser(null, ChatCode.Unauthorized);
                return;
            }

            if (info.Item1 == (int)ChatCode.CheckIn)
            {
                var user = _users.FirstOrDefault(x => x.Login.ToUpper() == info.Item2.Login.ToUpper() 
                    && x.Password == info.Item2.Password);
                if (user == null)
                {
                    client.SetUser(null, ChatCode.Unauthorized);
                }
                else
                {
                    user.Online = true;
                    client.SetUser(user, ChatCode.Accepted);
                    OnlineStatusUpdate(_users);
                }
            }
            else if (info.Item1 == (int)ChatCode.Invite)
            {
                var user = _users.FirstOrDefault(x => x.Login.ToUpper() == info.Item2.Login.ToUpper());
                if (user == null)
                {
                    lock (lockUsr)
                    {
                        _lastIdUser++;
                        info.Item2.Id = _lastIdUser;
                        _users.Add(info.Item2);
                        info.Item2.Online = true;
                        client.SetUser(info.Item2, ChatCode.Created);
                        OnlineStatusUpdate(_users);
                        AddNewUser(info.Item2);
                    }
                }
                else
                {
                    client.SetUser(null, ChatCode.Conflict);
                }
            }
        }        

        void client_EventClose(object obj, Client client)
        {
            UserInfo user = (UserInfo)obj;
            if (user != null)
            {
                var currentUser = _users.FirstOrDefault(x => x.Id == user.Id);
                if (currentUser != null)
                {
                    currentUser.Online = false;
                }
            }
            client.NewMessage -= client_NewMessage;
            client.ErrorMessage -= client_ErrorMessage;
            client.EventClose -= client_EventClose;
            _clients.Remove(client);
        }

        internal void Stop()
        {
            try
            {
                foreach (var client in _clients)
                {
                    client.Stop();
                }
                _tcpListener.Stop();
            }
            catch (Exception ex)
            {
                ErrorMsg(new Tuple<int, string>(-3, ex.Message));
            }
        }

        internal void Send(string msg)
        {
            NewMessages(new List<LogInfo>() { 
                new LogInfo() { Message = msg, Date = DateTime.Now, User = GlobalMsg, UserId = 1 } });
        }

        private void NewMessages(List<LogInfo> msgs)        
        {
            if(msgs != null && msgs.Count > 0)
                NewMessage(msgs);
        }

        public void UpdateMessage(List<LogInfo> msgs)
        {
            lock (lockMsg)
            {
                _listMessage.AddRange(msgs);
            }
        }
    }
}
