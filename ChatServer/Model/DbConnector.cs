using ChatServer.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.15
/// </created>

namespace ChatServer.Model
{
    class DbConnector
    {
        public delegate void EventHandler(object obj);
        public event EventHandler EventUpdateMessages;
        public event EventHandler LastException;

        private int _lastMsgId;

        private const string QueryUser = "Select [Id],[Login],[Password] from [Users]";
        private const string QueryInsertMessage =
                "Insert into [ChatLog] ([Text], [UserId], [Date]) Values (@text, @user, @date);";
        private const string QueryInsertUser = 
                "Insert into [Users] ([Id], [Login], [Password]) Values (@id, @login, @password);";
        private const string QueryLastIndexMessage =
                "select top 1 [Id] from [ChatLog]  order by [Id] desc";
        private const string QueryMessage =
                "Select m.[Id],m.[Text], m.[UserId], m.[Date],u.[Login] from [ChatLog] m,[Users] u where m.UserId = u.Id";
        private const string QueryInsertError =
                "Insert into [ErrorLog] ([Text], [Code], [Date]) Values (@text, @code, @date);";
        private string connectionString;

        public DbConnector()
        {
            connectionString = "DataSource=\"chat.sdf\"; Password='chat'";
            if (!File.Exists("chat.sdf"))
            {
                CreateDB(connectionString);
                CreateTable();
                InsertUser(new UserInfo() { Id = 1, Login = "Root", Password = "" });
            }

            _lastMsgId = GetLastMsgIndex();
        }

        private void CreateDB(string connectionString)
        {
            using (var en = new SqlCeEngine(connectionString))
            {
                en.CreateDatabase();
            }
        }

        private bool CreateTable()
        {
            string[] query = {
                "Create table [Users](" 
                    + "[Id] int PRIMARY KEY, " 
                    + "[Login] nvarchar(50), " 
                    + "[Password] nvarchar(50)); ",
                "Create table [ChatLog](" 
                    + "[Id] int IDENTITY(1,1) PRIMARY KEY, " 
                    + "[Text] nvarchar(250), " 
                    + "[UserId] int, " 
                    + "[Date] datetime, "
                    + "CONSTRAINT fk_messages_user_id FOREIGN KEY (UserId) REFERENCES Users (Id));",
                "Create table [ErrorLog](" 
                    + "[Id] int IDENTITY(1,1) PRIMARY KEY, " 
                    + "[Text] nvarchar(250), " 
                    + "[Code] int,"
                    + "[Date] datetime);"
            };

            for (int j = 0; j < query.Length; j++)
                if(!Put(query[j])) 
                    return false;

            return true;
        }

        private bool Put(string query)
        {
            try
            {
                using (var connect = new SqlCeConnection(connectionString))
                {
                    connect.Open();
                    using (var cmd = new SqlCeCommand(query, connect))
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }                    
                }
            }
            catch (Exception ex)
            {
                LastException(ex.Message);
            }
            return false;
        }

        private int GetLastMsgIndex()
        {
            object result;
            if (Get(QueryLastIndexMessage, out result))
            {
                List<object> list = (List<object>)result;
                if (list.Count > 0)
                {
                    object[] array = (object[])list[0];
                    if (array.Length > 0)
                    {
                        return (int)array[0];
                    }
                }
            }
            return 0;
        }

        private bool Get(string query, out object result)
        {
            try
            {
                using (var connect = new SqlCeConnection(connectionString))
                {
                    connect.Open();
                    using (var cmd = new SqlCeCommand(query, connect))
                    {
                        using (SqlCeDataReader r = cmd.ExecuteReader())
                        {
                            int amount = r.RecordsAffected;

                            var collection = new List<object>();

                            while (r.Read())
                            {
                                var obj = new object[r.FieldCount];
                                for (int i = 0; i < r.FieldCount; i++)
                                    obj[i] = r[i];

                                collection.Add(obj);
                            }
                            result = collection;
                            return true;
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                LastException(ex.Message);
            }
            result = null;
            return false;
        }

        internal void NewMessage(List<LogInfo> newMessages)
        {
            if (InsertMessage(newMessages))
            {
                var msg = GetMessages(_lastMsgId);
                EventUpdateMessages(msg);
            }            
        }

        internal List<UserInfo> GetUsers()
        {
            object result;
            if(Get(QueryUser, out result)){
                List<object> list = (List<object>)result;
                return list.Select(x => new UserInfo(x)).ToList();
            }
            return new List<UserInfo>();
        }

        private List<LogInfo> GetMessages(int id)
        {
            object result;
            if (Get(QueryMessage + " and m.[Id] > " + id, out result))
            {
                List<object> list = (List<object>)result;
                return list.Select(x => new LogInfo(x)).ToList();
            }
            return new List<LogInfo>();            
        }

        private bool InsertMessage(List<LogInfo> info)
        {
            try
            {
                using (var connect = new SqlCeConnection(connectionString))
                {
                    connect.Open();
                    using (var transaction = connect.BeginTransaction())
                    {
                        using (var cmd = new SqlCeCommand(QueryInsertMessage, connect))
                        {
                            cmd.Parameters.Add("@text", SqlDbType.NVarChar, 250);
                            cmd.Parameters.Add("@user", SqlDbType.Int);
                            cmd.Parameters.Add("@date", SqlDbType.DateTime);
                            try
                            {
                                foreach (var message in info)
                                {
                                    cmd.Parameters["@text"].Value = message.Message;
                                    cmd.Parameters["@user"].Value = message.UserId;
                                    cmd.Parameters["@date"].Value = message.Date;
                                    if (cmd.ExecuteNonQuery() != 1)
                                    {
                                        throw new Exception();
                                    }
                                }
                                transaction.Commit();
                                return true;
                            }
                            catch (Exception)
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastException(ex.Message);
            }
            return false;
        }

        public bool InsertUser(UserInfo user)
        {
            try
            {
                using (var connect = new SqlCeConnection(connectionString))
                {
                    connect.Open();
                    using (var transaction = connect.BeginTransaction())
                    {
                        using (var cmd = new SqlCeCommand(QueryInsertUser, connect))
                        {
                            cmd.Parameters.Add("@id", SqlDbType.Int);
                            cmd.Parameters.Add("@login", SqlDbType.NVarChar, 50);
                            cmd.Parameters.Add("@password", SqlDbType.NVarChar, 50);
                            cmd.Parameters["@id"].Value = user.Id;
                            cmd.Parameters["@login"].Value = user.Login;
                            cmd.Parameters["@password"].Value = user.Password;
                                    
                            cmd.ExecuteNonQuery();

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastException(ex.Message);
            }
            return false;
        }

        internal bool InsertError(Tuple<int, string> error)
        {
            try
            {
                using (var connect = new SqlCeConnection(connectionString))
                {
                    connect.Open();
                    using (var transaction = connect.BeginTransaction())
                    {
                        using (var cmd = new SqlCeCommand(QueryInsertError, connect))
                        {
                            cmd.Parameters.Add("@text", SqlDbType.NVarChar, 250);
                            cmd.Parameters.Add("@code", SqlDbType.Int);
                            cmd.Parameters.Add("@date", SqlDbType.DateTime);
                            cmd.Parameters["@text"].Value = error.Item2;
                            cmd.Parameters["@code"].Value = error.Item1;
                            cmd.Parameters["@date"].Value = DateTime.Now;

                            cmd.ExecuteNonQuery();

                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastException(ex.Message);
            }
            return false;
        }
    }
}
