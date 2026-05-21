using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.15
/// </created>

namespace ChatClient.Helper
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
        public int UserId { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public int Id { get; set; }
    }
}
