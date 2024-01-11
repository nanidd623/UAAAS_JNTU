using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    public class CollegeLoginInformationController : BaseController
    {
        //
        // GET: /CollegeLoginInformation/
        uaaasDBContext db = new uaaasDBContext();
       
        public ActionResult Index1()
        {


            var list = (from i in db.jntuh_college
                        join j in db.user_login_logout on i.id equals j.UserId
                        where j.Login.Value.Year == DateTime.Now.Year
                        select new { i.collegeCode, i.collegeName, j.Login, j.Logout,i.id }).ToList();

           // List<jntuh_college> jntuh_college = db.jntuh_college.ToList();
            List<jntuhuserlogin> jntuhlogineddata = new List<jntuhuserlogin>();
           // List<jntuhuserlogin> jntuhlogineddata = list;
            foreach (var item in list)
            {
                jntuhuserlogin jntuhlogindetails = new jntuhuserlogin();
                jntuhlogindetails.collegeCode = item.collegeCode;
                jntuhlogindetails.collegeName = item.collegeName;
                jntuhlogindetails.Login = item.Login; //db.user_login_logout.Where(i => i.UserId == item.id).Select(i => i.Login).FirstOrDefault();
                jntuhlogindetails.Logout = item.Logout;//db.user_login_logout.Where(i => i.UserId == item.id).Select(i => i.Logout).FirstOrDefault();
                var enddt = item.Logout;
                var sdt = item.Login;
                var total = "";
                if (enddt != null)
                {
                    total = enddt.Value.Subtract(sdt.Value).ToString().Split('.')[0].ToString();
                }
                else
                { 
                    total="";
                }
                jntuhlogindetails.Time = total;
                jntuhlogineddata.Add(jntuhlogindetails);
            }         
            ViewBag.Count = jntuhlogineddata.Count();
            return View(jntuhlogineddata);
        }

    }
   
}
