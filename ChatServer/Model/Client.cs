using ChatServer.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    class Client
    {
        public delegate void EventHandler(object obj, Client client);

        public event EventHandler NewMessage;
        public event EventHandler ErrorMessage;
        public event EventHandler EventClose;
        public event EventHandler EventLogin;
        public event EventHandler EventGetInfo;

        private string _token;

        private bool _stop;
        private TcpClient tcpClient;
        private List<LogInfo> _messages;
        private List<UserInfo> _users;
        private int _lastIdMsg;
        private UserInfo _currentUser;
        private ChatCode _status;
        private Thread _thread;


        public Client(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            _messages = new List<LogInfo>();
            _users = new List<UserInfo>();
            _thread = new Thread(Connect);
        }

        private void Connect(object obj)
        {
            try
            {
                var stream = tcpClient.GetStream();
                while (!_stop)
                {
                    if (stream.CanRead)
                    {
                        var buffer = new byte[1024];
                        int size = stream.Read(buffer, 0, 1024);

                        using (var ms = new MemoryStream(buffer, 0, size))
                        {
                            if (size == 1024)
                            {
                                while (size == 1024)
                                {
                                    size = stream.Read(buffer, 0, 1024);
                                    ms.Write(buffer, 0, size);
                                }
                            }
                            SetRequest(ms);
                        }
                    }
                    if (stream.CanWrite)
                    {
                        var buffer = GetResponse();
                        if (buffer != null)
                            stream.Write(buffer, 0, buffer.Length);
                        else
                            Stop();
                    }
                }
            }
            catch (SocketException se)
            {
                ErrorMessage(new Tuple<int, string>(se.ErrorCode, se.Message), this);
            }
            catch (ObjectDisposedException ode)
            {
                ErrorMessage(new Tuple<int, string>(1, ode.Message), this);
            }
            catch (Exception ex)
            {
                ErrorMessage(new Tuple<int, string>(2, ex.Message), this);
            }
            EventClose(_currentUser, this);
        }

        public void Start()
        {
            _thread.Start();
        }

        private byte[] GetResponse()
        {
            var info = new ChatInfo();
            switch(_status)
            {
                case ChatCode.Accepted:
                    _token = Guid.NewGuid().ToString("N");
                    info.Token = _token;
                    info.Code = (int)_status;
                    break;
                case ChatCode.Created:
                    _token = Guid.NewGuid().ToString("N");
                    info.Token = _token;
                    info.Code = (int)_status;
                    break;
                case ChatCode.Conflict:
                case ChatCode.Forbidden:
                case ChatCode.Unauthorized:
                    info.Code = (int)_status;
                    break;
                case ChatCode.Ok:
                    info.Code = (int)_status;
                    info.IdMsg = _lastIdMsg;
                    EventGetInfo(info, this);
                    break;
                default:
                    info.Code = (int)ChatCode.InternalServerError;
                    break;
            }
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info));
        }

        private void SetRequest(MemoryStream ms)
        {
            ChatInfo info = JsonConvert.DeserializeObject<ChatInfo>(Encoding.UTF8.GetString(ms.ToArray()));

            if (info.Code == 0)
            {
                if (_token == info.Token)
                {
                    if(_currentUser != null)
                        foreach (var log in info.Log)
                        {
                            log.User = _currentUser.Login;
                            log.UserId = _currentUser.Id;
                            log.Date = DateTime.Now;
                        }
                    NewMessage(info.Log, this);
                    _status = ChatCode.Ok;
                    _lastIdMsg = info.IdMsg;
                }
                else
                {
                    _token = Guid.NewGuid().ToString("N");
                    _status = ChatCode.Forbidden;
                }
            }
            else if(info.Code == (int)ChatCode.Invite)
            {
                var user = new UserInfo();
                user.Login = info.Login;
                user.Password = info.Password;
                EventLogin(new Tuple<int, UserInfo>(info.Code, user), this);                
            }
            else if(info.Code == (int)ChatCode.CheckIn)
            {
                var user = new UserInfo();
                user.Login = info.Login;
                user.Password = info.Password;
                EventLogin(new Tuple<int, UserInfo>(info.Code, user), this);
            }
        }

        internal void Stop()
        {
            _stop = true;
            tcpClient.Close();
            Thread.Sleep(100);
            if (_thread.ThreadState == ThreadState.Running)
            {
                _thread.Abort();
            }
        }

        public void SetUser(UserInfo currentUser, ChatCode status)
        {
            _currentUser = currentUser;
            _status = status;
        }

        public void UpdateInfo(List<LogInfo> log, List<UserInfo> user)
        {
            _messages = log;
            _users = user;
        }
    }
}
