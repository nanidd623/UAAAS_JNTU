using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class jntuhuserlogin
    {

        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public Nullable<DateTime> Login { get; set; }
        public Nullable<DateTime> Logout { get; set; }
        public Nullable<int> Userid { get; set; }
        public string Time { get; set; }
        public int id { get; set; }
        public virtual jntuh_college jntuhcollege { get; set; }
        public virtual user_login_logout userlogin { get; set; }
    }
}