using ChatServer.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    class ServerModel
    {
        public delegate void EventHandler(object obj);

        public event EventHandler EventUpdateMessages;
        public event EventHandler EventUpdateUsers;

        private Thread _serverThread;
        private Server _server;
        private DbConnector _dbConnector;
        private bool _stop;
        private Object lockIoObject = new Object();

        public ServerModel()
        {
            _dbConnector = new DbConnector();
            _dbConnector.EventUpdateMessages += DbConnector_EventUpdateMessages;
            _dbConnector.LastException += DbConnector_LastException;

            InitServer();
            
        }

        void DbConnector_LastException(object obj)
        {
            lock (lockIoObject)
            {
                using (var sw = new StreamWriter("Error.log", true))
                {
                    sw.WriteLine(obj);
                }
            }
        }

        private void InitServer()
        {
            try
            {
                _server = new Server(_dbConnector.GetUsers());
                _server.OnlineStatusUpdate += UpdateUsers;
                _server.NewMessage += NewMessage;
                _server.ErrorMsg += ErrorMessage;
                _server.AddNewUser += AddUser;
                _serverThread = new Thread(_server.Start);
                _serverThread.Start();
            }
            catch (Exception ex)
            {
                ErrorMessage(new Tuple<int, string>(-4, ex.Message));
                if (_serverThread != null && _serverThread.ThreadState == ThreadState.Running)
                {
                    _serverThread.Abort();
                }                
            }
        }

        void DbConnector_EventUpdateMessages(object obj)
        {
            List<LogInfo> newMessages = (List<LogInfo>)obj;
            _server.UpdateMessage(newMessages);
            EventUpdateMessages(obj);
        }

        private void ErrorMessage(object obj)
        {
            //if (!_stop)
            {
                Tuple<int, string> error = (Tuple<int, string>)obj;
                _dbConnector.InsertError(error);
            }
        }

        private void NewMessage(object obj)
        {
            List<LogInfo> newMessages = (List<LogInfo>)obj;
            _dbConnector.NewMessage(newMessages);            
        }

        private void AddUser(object obj)
        {
            UserInfo user =(UserInfo)obj;
            _dbConnector.InsertUser(user);
        }

        private void UpdateUsers(object obj)
        {
            EventUpdateUsers(obj);
        }

        internal void Stop()
        {
            _stop = true;
            _server.Stop();

            Thread.Sleep(1000);

            if(_serverThread.ThreadState == ThreadState.Running)
                _serverThread.Abort();
        }

        internal void Send(string msg)
        {
            if(!string.IsNullOrEmpty(msg))
                _server.Send(msg);
        }
    }
}
