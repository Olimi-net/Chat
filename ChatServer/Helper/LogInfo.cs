using System;
using System.Collections.Generic;
using System.Text;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatServer.Helper
{
    enum ChatCode
    {
        None = 0,
        CheckIn = 100,
        Invite = 101,
        Ok = 200,
        Created = 201,
        Accepted = 202,
        NoContent = 204,
        Unauthorized = 401,
        Forbidden = 403,
        Conflict = 409,
        InternalServerError = 500, 
        UnknownError = 520
    }
    class ChatInfo
    {
        public int Code { get; set; }
        public string Token { get; set; }
        public int IdMsg { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<string> Users { get; set; }
        public List<LogInfo> Log { get; set; }
    }

    public class LogInfo
    {
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }

        public LogInfo(LogInfo x)
        {
            User = x.User;
            Message = x.Message;
            Date = x.Date;
            Id = x.Id;
            UserId = x.Id;
        }
        public LogInfo()
        {
        }

        public LogInfo(object obj)
        {
            object[] objects = (object[])obj;

            if (objects.Length == 5)
            {
                Id = (int)objects[0];
                Message = objects[1].ToString();
                UserId = (int)objects[2];
                Date = ObjectToDateTime(objects[3]);
                User = objects[4].ToString();
            }
        }

        private DateTime ObjectToDateTime(object o)
        {
            if (o.GetType() == typeof(DateTime))
            {
                try
                {
                    return Convert.ToDateTime(o);
                }
                catch (Exception)
                {
                    return default(DateTime);
                }
            }
            return default(DateTime);
        }
        
    }
}
