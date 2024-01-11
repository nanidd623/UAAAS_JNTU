using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public static class Common
    {

    }

    [Serializable]
    public class LoginUser
    {
        public LoginUser()
        {
        }

        private int _userId = 0;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }
        private string _userName = string.Empty;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        private string _roles = string.Empty;

        public string Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }
        private string _email = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public LoginUser(int userId, string userName, string roles)
        {
            UserId = userId;
            UserName = userName;
            Roles = roles;
        }
    }
}