using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatServer.Helper
{
    class UserInfo
    {
        public int Id { get; set; }
        public bool Online { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        public UserInfo() 
        { 
        }

        public UserInfo(object obj)
        {
            object[] objects = (object[])obj;

            if (objects.Length == 3)
            {
                Id = (int)objects[0];
                Login = objects[1].ToString();
                Password = objects[2].ToString();
            }
        }

        public bool IsValid()
        {
            return !(string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password));
        }
    }
}
