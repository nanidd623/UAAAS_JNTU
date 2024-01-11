using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SendUsernamePasswordController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult SendEmails()
        {
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SendEmails(all_college_emails emails)
        {
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            //var listEmails = db.all_college_emails.Where(e => e.id > 252 && e.id <= 333).ToList();
            var listEmails = db.all_college_emails.Where(e => e.id >= 445).ToList();
            //var listEmails = db.all_college_emails.Where(e => e.id == 444).ToList();

            foreach (var item in listEmails)
            {
                jntuh_college jntuh_college = new jntuh_college();
                jntuh_college.collegeName = item.CollegeName;
                jntuh_college.collegeCode = item.Code.Replace(" ", "");
                jntuh_college.collegeTypeID = 1;
                jntuh_college.collegeStatusID = 2;
                jntuh_college.societyName = string.Empty;
                jntuh_college.isActive = true;
                jntuh_college.createdBy = createdBy;
                jntuh_college.createdOn = DateTime.Now;
                jntuh_college.collegeAffiliationTypeID = db.jntuh_college_affiliation_type.Select(at => at.id).FirstOrDefault();
                jntuh_college.jntuh_college_status = db.jntuh_college_status.Find(2);
                jntuh_college.jntuh_college_type = db.jntuh_college_type.Find(1);

                var rowExists = (from c in db.jntuh_college
                                 where c.collegeCode == item.Code && c.collegeName == item.CollegeName && c.isActive == true
                                 select c.collegeName);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_college.Add(jntuh_college);
                    db.SaveChanges();
                }
                else
                {
                    jntuh_college.id = db.jntuh_college.Where(c => c.collegeCode == item.Code && c.collegeName == item.CollegeName).Select(c => c.id).FirstOrDefault();
                }

                if (jntuh_college.id > 0)
                {
                    string password = string.Empty;

                    var chars = "abcdefghjknpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
                    var random = new Random();
                    var randomPwd = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                    password = "jntuhAAC@123";

                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(item.Username, password, item.Email, null, null, true, null, out createStatus);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        //add user role to my_aspnet_usersinroles table
                        my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
                        roleModel.roleId = db.my_aspnet_roles.Where(r => r.name == "College").Select(r => r.id).FirstOrDefault();
                        roleModel.userId = db.my_aspnet_users.Where(u => u.name == item.Username).Select(u => u.id).FirstOrDefault();
                        db.my_aspnet_usersinroles.Add(roleModel);
                        db.SaveChanges();

                        jntuh_college_users collegeUsers = new jntuh_college_users();
                        collegeUsers.userID = db.my_aspnet_users.Where(u => u.name == item.Username).Select(u => u.id).FirstOrDefault();
                        collegeUsers.collegeID = jntuh_college.id;
                        collegeUsers.createdBy = createdBy;
                        collegeUsers.createdOn = DateTime.Now;
                        db.jntuh_college_users.Add(collegeUsers);
                        db.SaveChanges();

                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.Welcome(item.Email, "LoginInformation", "JNTUH Account Login Details", item.Username, password, string.Empty, string.Empty).SendAsync();

                        TempData["Success"] = "User credentials created successfully";
                    }
                }
            }


            //var listColleges = db.jntuh_college.Select(c => c.id).ToList();

            //foreach (var item in listColleges)
            //{
            //    jntuh_college_edit_status jntuh_college_edit_status = new jntuh_college_edit_status();
            //    jntuh_college_edit_status.collegeId = item;
            //    jntuh_college_edit_status.editFromDate = DateTime.Now.AddDays(-6);
            //    jntuh_college_edit_status.editToDate = DateTime.Now.AddDays(1);
            //    jntuh_college_edit_status.createdBy = 1;
            //    jntuh_college_edit_status.createdOn = DateTime.Now;

            //    var rowExists = db.jntuh_college_edit_status.AsNoTracking().Where(s => s.collegeId == item);

            //    if (rowExists.Count() == 0)
            //    {
            //        db.jntuh_college_edit_status.Add(jntuh_college_edit_status);
            //        db.SaveChanges();
            //    }
            //}

            return View(emails);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult InsertEditStatus()
        {
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var listColleges = db.jntuh_college.Select(c => c.id).ToList();

            foreach (var item in listColleges)
            {
                jntuh_college_edit_status jntuh_college_edit_status = new jntuh_college_edit_status();
                jntuh_college_edit_status.collegeId = item;
                jntuh_college_edit_status.editFromDate = DateTime.Now.AddDays(-6);
                jntuh_college_edit_status.editToDate = DateTime.Now.AddDays(1);
                jntuh_college_edit_status.createdBy = 1;
                jntuh_college_edit_status.createdOn = DateTime.Now;

                var rowExists = db.jntuh_college_edit_status.AsNoTracking().Where(s => s.collegeId == item);

                if (rowExists.Count() == 0)
                {
                    //db.jntuh_college_edit_status.Add(jntuh_college_edit_status);
                    //db.SaveChanges();
                }
            }

            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult InsertEditStatus(jntuh_college_edit_status status)
        {
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var listColleges = db.jntuh_college.Select(c => c.id).ToList();

            foreach (var item in listColleges)
            {
                jntuh_college_edit_status jntuh_college_edit_status = new jntuh_college_edit_status();
                jntuh_college_edit_status.collegeId = item;
                jntuh_college_edit_status.editFromDate = DateTime.Now.AddDays(-6);
                jntuh_college_edit_status.editToDate = DateTime.Now.AddDays(1);
                jntuh_college_edit_status.createdBy = 1;
                jntuh_college_edit_status.createdOn = DateTime.Now;

                var rowExists = db.jntuh_college_edit_status.AsNoTracking().Where(s => s.collegeId == item);

                if (rowExists.Count() == 0)
                {
                    db.jntuh_college_edit_status.Add(jntuh_college_edit_status);
                    db.SaveChanges();
                }
            }

            return View();
        }
    }
}
