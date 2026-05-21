using ChatClient.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatClient.Model
{

    class ClientChat
    {
        public delegate void EventHandler(int code, object msg);

        public event EventHandler NewMessage;
        public event EventHandler UserList;
        public event EventHandler Error;
        public event EventHandler ConnectAccess;
        
        private Thread _thread;
        
        private List<string> _sendMessage;
        private string _token;
        private ChatCode _status;
        private string _login;
        private string _password;
        private int _lastMsg;
        private bool _stop;

        public string Url { get; set; }
        public int Port { get; set; }

        public ClientChat(string url, int port)
        {
            Url = url;
            Port = port;
            _sendMessage = new List<string>();
            _thread = new Thread(Connect);
            _thread.Start();
        }

        public void Start()
        {
            if (!_stop)
            {
                if (_thread.ThreadState == ThreadState.Stopped)
                {
                    _thread = new Thread(Connect);
                }
                if (_stop) return;
                if (_thread.ThreadState == ThreadState.Unstarted)
                {
                    _thread.Start();
                }
            }
        }

        private void Connect()
        {
            try
            {
                Connect(Url, Port);
            }
            catch (Exception ex)
            {
                Error((int)ChatCode.UnknownError, ex.Message);
            }
        }

        private void Connect(string url, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(url, port);
                    var stream = client.GetStream();
                    while (client.Connected)
                    {
                        if (_stop) return;
                        if (_status == ChatCode.None || (int)_status > 300) continue;

                        if (stream.CanWrite)
                        {
                            var buffer = GetRequest();
                            stream.Write(buffer, 0, buffer.Length);
                        }
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
                                if(!SetResponse(ms)) return;
                            }
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                Error(se.ErrorCode, se.Message);
            }
        }

        private bool SetResponse(MemoryStream ms)
        {
            ChatInfo info = JsonConvert.DeserializeObject<ChatInfo>(Encoding.UTF8.GetString(ms.ToArray()));

            switch ((ChatCode)info.Code)
            {
                case ChatCode.Ok:
                    if (info.Log != null)
                    {
                        var log = info.Log.OrderByDescending(x => x.Id).ToList().FirstOrDefault();
                        if (log != null)
                        {
                            _lastMsg = log.Id;
                            if (info.Log.Count > 0)
                                NewMessage(0, info.Log);
                        }
                    }
                    if (info.Users != null)
                        UserList(0, info.Users);
                    return true;
                case ChatCode.Created:
                    _status = ChatCode.CheckIn;
                    _token = info.Token;
                    ConnectAccess(0, _login);
                    return true;
                case ChatCode.Accepted:
                    _token = info.Token;
                    ConnectAccess(0, _login);
                    return true;
                case ChatCode.NoContent:
                    if (info.Users != null)
                        UserList(0, info.Users);
                    return true;
                case ChatCode.Unauthorized:
                    Error(info.Code, "Unauthorized");
                    _token = null;
                    return false;
                case ChatCode.Forbidden:
                    Error(info.Code, "Forbidden");
                    _token = null;
                    _status = ChatCode.CheckIn;
                    return false;
                case ChatCode.Conflict:
                    Error(info.Code, "Conflict");
                    _token = null;
                    return false;
            }
            return false;            
        }

        private byte[] GetRequest()
        {
            var info = new ChatInfo();


            if (string.IsNullOrEmpty(_token))
            {
                info.Login = _login;
                info.Password = _password;
                info.Code = (int)_status;
            }
            else
            {
                info.IdMsg = _lastMsg;
                info.Token = _token;
                info.Log = _sendMessage.Select(x => new LogInfo() { Message = x }).ToList();
                _sendMessage.Clear();
            }
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info));
        }

        public void Send(string s)
        {
            _sendMessage.Add(s);
        }

        public void Invite(string login, string password)
        {
            _login = login;
            _password = password;
            _status = ChatCode.Invite;            
        }

        public void CheckIn(string login, string password)
        {
            _login = login;
            _password = password;
            _status = ChatCode.CheckIn;            
        }

        internal void Close()
        {
            _stop = true;
            
            if (_thread.ThreadState == ThreadState.Running)
            {
                _thread.Abort();
            }
        }
    }
}
