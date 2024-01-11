using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AdminController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public class OnlineUAAASUsers
        {
            public string username { get; set; }
            public string datetime { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string mobile { get; set; }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            List<OnlineUAAASUsers> loggedInUsers = new List<OnlineUAAASUsers>();
            MembershipUserCollection allUsers = Membership.GetAllUsers();
            MembershipUserCollection filteredUsers = new MembershipUserCollection();

            bool isOnline = true;
            foreach (MembershipUser user in allUsers)
            {
                // if user is currently online, add to gridview list
                if (user.IsOnline == isOnline)
                {
                    filteredUsers.Add(user);

                    int userID = Convert.ToInt32(user.ProviderUserKey);
                    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                    string code = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
                    string name = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeName).FirstOrDefault();
                    string email = db.my_aspnet_membership.Where(m => m.userId == userID).Select(m => m.Email).FirstOrDefault();
                    loggedInUsers.Add(new OnlineUAAASUsers { username = user.UserName, datetime = user.LastActivityDate.ToString(), code = code, name = name, email = email });
                }
            }
            ViewBag.OnlineUsers = loggedInUsers.OrderByDescending(l => l.datetime);
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PhdDetails(int? collegeId)
        {
            var colleges = db.jntuh_college.Where(e => e.isActive && e.id != 375).Select(e => new Colleges { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            colleges.Insert(0, new Colleges() {collegeId = 999, collegeName = "All"});
            ViewBag.Colleges = colleges;
            var admindetails = new List<AdminFacultyphdDetails>();
            if (collegeId != null)
            {
                var phdDetails = db.jntuh_faculty_phddetails.Select(e => e).ToList();
                if (phdDetails.Count <= 0) return View(admindetails);
                if (collegeId == 999)
                {
                    var objdetails = (from t in phdDetails
                                      join cf in db.jntuh_registered_faculty on t.Facultyid equals cf.id
                                      join dd in db.jntuh_college_faculty_registered on cf.RegistrationNumber equals dd.RegistrationNumber
                                      where cf.isActive 
                                      select new AdminFacultyphdDetails
                                      {
                                          RegistrationNumber = cf.RegistrationNumber,
                                          FirstName = cf.FirstName,
                                          LastName = cf.LastName,
                                          MiddleName = cf.MiddleName,
                                          University = t.University,
                                          EncryptId = Utilities.EncryptString(t.Facultyid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]),
                                          PhdAdmissionYear = t.AdmissionYear ?? 0,
                                          PhdawardYear = t.Phdawardyear ?? 0
                                      }).ToList();
                    return View(objdetails);
                }
                else
                {
                    var objdetails = (from t in phdDetails
                                      join cf in db.jntuh_registered_faculty on t.Facultyid equals cf.id
                                      join dd in db.jntuh_college_faculty_registered on cf.RegistrationNumber equals dd.RegistrationNumber
                                      where cf.isActive && dd.collegeId == collegeId
                                      select new AdminFacultyphdDetails
                                      {
                                          RegistrationNumber = cf.RegistrationNumber,
                                          FirstName = cf.FirstName,
                                          LastName = cf.LastName,
                                          MiddleName = cf.MiddleName,
                                          University = t.University,
                                          EncryptId = Utilities.EncryptString(t.Facultyid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]),
                                          PhdAdmissionYear = t.AdmissionYear ?? 0,
                                          PhdawardYear = t.Phdawardyear ?? 0
                                      }).ToList();
                    return View(objdetails);
                }
               
            }

            return View(admindetails);
        }

        public class Colleges
        {
            public int collegeId { get; set; }
            public string collegeName { get; set; }
        }
    }
}
