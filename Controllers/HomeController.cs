using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Data.Objects;
using System.Web.Services;
using UAAAS.Helpers;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.Security;
using MySql.Data.MySqlClient;
using UAAAS.Mailers;
using System.Diagnostics;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class HomeController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        LogController Errorlog = new LogController();
        public ActionResult Index()
        {
            string serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            TempData["ServerUrl"] = serverURL;
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
                int[] FacultyDataEntryUserIds = { 113279, 113280, 113281, 113282, 113283, 113284, 113285, 113286, 113287, 113288, 113289, 113290, 113291, 113292, 113293, 113294, 113295, 113296, 113297, 113298 };
                if (userRoles.Contains(
                              db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                                  .Select(r => r.id)
                                  .FirstOrDefault()) || userRoles.Contains(
                                      db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry"))
                                          .Select(r => r.id)
                                          .FirstOrDefault()))
                {
                    string strHostName = System.Net.Dns.GetHostName();
                    string sipaddress = string.Empty;
                    IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                    foreach (IPAddress ipAddress in ipEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                        {
                            sipaddress = ipAddress.ToString();
                        }
                    }
                    if (sipaddress == "10.10.10.5")
                    {
                        TempData["Error"] = "You are not authorized to access this service,so contact your Administrator";
                        return RedirectToAction("LogOn", "Account");
                    }
                }
                if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("College"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    // return RedirectToAction("College","OnlineRegistration");
                    return RedirectToAction("CollegeDashboard", "Dashboard");

                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Committee"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "Committee");
                }
                else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry")).Select(r => r.id).FirstOrDefault()))
                {
                    //if (FacultyDataEntryUserIds.Contains(userID))
                    //{
                    //    return RedirectToAction("SCMFacultyCertitficatesVerification", "FacultyVerificationDENew");
                    //}
                   
                 //return RedirectToAction("Index", "FacultyVerificationDENew");
                    return RedirectToAction("Welcome", "UnderConstruction");

                }
                else if (
               userRoles.Contains(
                   db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                       .Select(r => r.id)
                       .FirstOrDefault()))
                {
                    //Added Faculty Verification Instructions Page in 419
                    return RedirectToAction("FacultyVericicationPreamble", "FacultyVerification");
                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Faculty"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    int facultyId =
                        db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                            .Select(f => f.id)
                            .FirstOrDefault();
                    string FacultyType =
                        db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                            .Select(f => f.type)
                            .FirstOrDefault();
                    string fid = Utilities.EncryptString(facultyId.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);

                    if (FacultyType != "Adjunct")
                        return RedirectToAction("Index", "NewOnlineRegistration", new { id = fid });
                    else
                        return RedirectToAction("AdjunctFacty", "OnlineRegistration",
                            new { fid = fid });
                    //TempData["Error"] = "This Service is unavilable for short time";
                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    return RedirectToAction("Admin", "Dashboard");
                    return RedirectToAction("Admin", "Dashboard");
                }
                else if (
                           userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("Complaints"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "CollegeComplaints");

                }
                else if (
                           userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("Accounts"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "CollegesFee");

                }
                else if (
                       userRoles.Contains(
                           db.my_aspnet_roles.Where(r => r.name.Equals("Operations"))
                               .Select(r => r.id)
                               .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "FacultyVerificationDENew");

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            //MembershipUser userdata = Membership.GetUser(Membership.GetUser().ProviderUserKey.ToString());
            //string userId = Membership.GetUser().ProviderUserKey.ToString();

            //   string FacultyErrorMessage2 = "Controller: Home , Action : Index, Error Type: Date Checking ,Welcome to Home";
            //  Errorlog.Log(FacultyErrorMessage2);

            //ViewBag.Message = "Welcome to ASP.NET MVC!";

            //Change the dtae format to dd/MM/yyyy 12:00:00 AM
            DateTime dt = DateTime.Now.Date;
            //Binding news to view bag start date and end date
            ViewBag.News = db.jntuh_newsevents.Where(n => n.isActive == true && n.isNews == true)
                                     .Where(n => (n.startDate == null && n.endDate == null) || (n.startDate >= DateTime.Now && n.endDate <= DateTime.Now))
                                     .OrderByDescending(n => n.id).Take(10);

            //RAMESH : NOT USING EVENTS

            //Binding events to view bag start date and end date
            ViewBag.Events = db.jntuh_newsevents.Where(n => n.isActive == true &&
                                                     n.isNews == false &&
                                                     (n.startDate == null || n.startDate <= dt) &&
                                                     (n.endDate == null || n.endDate <= dt)).ToList();

            //IUserMailer mailer = new UserMailer();
            //mailer.Welcome("ramesh.bandi@csstechnergy.com", "LoginInformation", "JNTUH Account password reset", "Username1", "Password1", string.Empty, string.Empty).SendAsync();

            //Configuration webConfig = WebConfigurationManager.OpenWebConfiguration("~");
            //MailSettingsSectionGroup settings =
            //    (MailSettingsSectionGroup)webConfig.GetSectionGroup("system.net/mailSettings");
            //SmtpSection smtp = settings.Smtp;
            //SmtpNetworkElement net = smtp.Network;
            //net.Host = "smtp.gmail.com";
            //net.Port = 587;
            //net.EnableSsl = true;
            //net.Password = "uaaas@aac";

            //if (net.UserName == "aac.do.not.reply@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply.1@gmail.com";
            //    webConfig.Save();
            //}
            //else if (net.UserName == "aac.do.not.reply.1@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply.2@gmail.com";
            //    webConfig.Save();
            //}
            //else if (net.UserName == "aac.do.not.reply.2@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply.3@gmail.com";
            //    webConfig.Save();
            //}
            //else if (net.UserName == "aac.do.not.reply.3@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply.4@gmail.com";
            //    webConfig.Save();
            //}
            //else if (net.UserName == "aac.do.not.reply.4@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply.5@gmail.com";
            //    webConfig.Save();
            //}
            //else if (net.UserName == "aac.do.not.reply.5@gmail.com")
            //{
            //    net.UserName = "aac.do.not.reply@gmail.com";
            //    webConfig.Save();
            //}

            //mailer.Welcome("ramesh.bandi@csstechnergy.com", "LoginInformation", "JNTUH Account password reset", "Username2", "Password2", string.Empty, string.Empty).SendAsync();

            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult ABASView()
        {
            return View();
        }


        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {

                string newPassword = "";

                try
                {
                    var userdata = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Email == model.UserName).Select(e => e.id).FirstOrDefault();
                    if (userdata != 0)
                    {
                        newPassword = Membership.Provider.ResetPassword(model.UserName, null);
                    }
                    else
                    {
                        TempData["Error"] = "Password reset failed. Please reenter your correct values and try again.";
                        return View(model);
                    }

                }
                catch (NotSupportedException e)
                {
                    TempData["Error"] = "An error has occurred resetting your password: " + e.Message + "." +
                                        "Please check your values and try again.";
                }
                catch (MembershipPasswordException e)
                {
                    TempData["Error"] = "Invalid password answer. Please reenter the answer and try again.";
                }
                catch (System.Configuration.Provider.ProviderException e)
                {
                    TempData["Error"] = "The specified user name does not exist. Please check your value and try again.";
                }

                if (newPassword != "")
                {
                    my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.UserName).ProviderUserKey);
                    //send email
                    IUserMailer mailer = new UserMailer();
                    //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                    //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                    //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                    //              .SendAsync();

                    mailer.Welcome(my_aspnet_membership.Email, "LoginInformation", "JNTUH Account password reset",
                        model.UserName, newPassword, string.Empty, string.Empty).SendAsync();
                    //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();

                    string[] email = my_aspnet_membership.Email.Split('@');
                    string maskEmail = email[0].Substring(0, 2) +
                                       new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                       email[0].Substring(email[0].Length - 1, 1);


                    TempData["Success"] = "Password has been reset & sent successfully to " + maskEmail + "@" + email[1];
                }
                else
                {
                    TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpGet]
        public ActionResult ResetCollegePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetCollegePassword(ResteCollegePassword model)
        {
            if (ModelState.IsValid)
            {

                string newPassword = "";

                try
                {
                    var userdata = db.my_aspnet_users.Where(e => e.name == model.UserName).Select(e => e.id).FirstOrDefault();


                    if (userdata != 0)
                    {
                        newPassword = Membership.Provider.ResetPassword(model.UserName, null);
                        //ResetPassword(model.UserName, null);
                    }
                    else
                    {
                        TempData["Error"] = "Password reset failed. Please reenter your correct values and try again.";
                        return View(model);
                    }

                }
                catch (NotSupportedException e)
                {
                    TempData["Error"] = "An error has occurred resetting your password: " + e.Message + "." +
                                        "Please check your values and try again.";
                }
                catch (MembershipPasswordException e)
                {
                    TempData["Error"] = "Invalid password answer. Please reenter the answer and try again.";
                }
                catch (System.Configuration.Provider.ProviderException e)
                {
                    TempData["Error"] = "The specified user name does not exist. Please check your value and try again.";
                }

                if (newPassword != "")
                {
                    my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.UserName).ProviderUserKey);
                    //send email
                    IUserMailer mailer = new UserMailer();
                    //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                    //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                    //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                    //              .SendAsync();

                    mailer.Welcome(my_aspnet_membership.Email, "LoginInformation", "JNTUH Account password reset",
                        model.UserName, newPassword, string.Empty, string.Empty).SendAsync();
                    //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();

                    string[] email = my_aspnet_membership.Email.Split('@');
                    string maskEmail = email[0].Substring(0, 2) +
                                       new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                       email[0].Substring(email[0].Length - 1, 1);


                    TempData["Success"] = "Password has been reset & sent successfully to " + maskEmail + "@" + email[1];
                }
                else
                {
                    TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                }
            }
            return RedirectToAction("ResetCollegePassword");
        }


        public ActionResult FacultyForgotEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FacultyForgotEmail(FacultyForgotEmail model)
        {
            if (ModelState.IsValid)
            {

                TempData["Email"] =
                    db.jntuh_registered_faculty.Where(
                        e =>
                            e.RegistrationNumber == model.RegistrationNumber && e.PANNumber == model.PanNumber &&
                            e.DateOfBirth == model.DateofBirth).Select(e => e.Email).FirstOrDefault();
                if (TempData["Email"] == null)
                {
                    TempData["Error"] = "Your Details Doesn't Match. Please Provide Valid Details.";
                }

            }
            else
            {
                TempData["Error"] = "Please Enter Mandatory Fields.";
            }
            return View(model);
        }



        [HttpGet]
        public ActionResult ChangeUserName()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ChangeUserName(ChangeuserName model)
        {
            if (ModelState.IsValid)
            {
                int count = 0;
                string newPassword = "";

                var myaspnetuser = db.my_aspnet_users.Where(e => e.name == model.OldEmailId).Select(e => e.id).FirstOrDefault();


                var myaspnetuseristhere = db.my_aspnet_users.Where(e => e.name == model.NewEmailId).Select(e => e.id).FirstOrDefault();

                var mymembershipdata = db.my_aspnet_membership.Where(e => e.Email == model.OldEmailId).Select(e => e).FirstOrDefault();


                var registraredfacultydata = db.jntuh_registered_faculty.Where(e => e.Email == model.OldEmailId && e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Mobile == model.MobileNo && e.DateOfBirth == model.DateofBirth).Select(e => e).FirstOrDefault();
                if (myaspnetuseristhere == 0)
                {

                    if (myaspnetuser != 0 && mymembershipdata != null && registraredfacultydata != null)
                    {
                        try
                        {

                            string constr = ConfigurationManager.ConnectionStrings["MySqlMembershipConnection"].ConnectionString;
                            using (MySqlConnection con = new MySqlConnection(constr))
                            {
                                using (MySqlCommand cmd = new MySqlCommand("Update_Myaspnet_UserId", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("_OldEmail", model.OldEmailId);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("_NewEmail", model.NewEmailId);
                                    con.Open();
                                    count = cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }


                            //var myaspnetuserdata =
                            //    db.my_aspnet_users.Where(e => e.id == myaspnetuser).Select(e => e).FirstOrDefault();
                            //myaspnetuserdata.name = model.NewEmailId;

                            //  myaspnetuserdata.email = null;
                            //  myaspnetuserdata.SelectedRole = "0";
                            //db.Entry(myaspnetuserdata).State = EntityState.Modified;
                            //db.SaveChanges();
                            if (count > 0)
                            {
                                mymembershipdata.Email = model.NewEmailId;
                                db.Entry(mymembershipdata).State = EntityState.Modified;
                                db.SaveChanges();

                                registraredfacultydata.Email = model.NewEmailId;
                                db.Entry(registraredfacultydata).State = EntityState.Modified;
                                db.SaveChanges();
                            }




                            //  db.SaveChanges();

                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                        validationError.PropertyName,
                                        validationError.ErrorMessage);
                                }
                            }
                        }

                        var userdata = db.jntuh_registered_faculty.Where(e => e.Email == model.NewEmailId && e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Mobile == model.MobileNo && e.DateOfBirth == model.DateofBirth).Select(e => e).FirstOrDefault();
                        if (userdata != null && count > 0)
                        {
                            newPassword = Membership.Provider.ResetPassword(model.NewEmailId, null);
                            //var jntuh_changeuserid = db.jntuh_changeuserid.AsNoTracking().ToList();
                            //var changeuserId =jntuh_changeuserid.Where(e => e.UserId == userdata.UserId).Select(e => e.Id).FirstOrDefault();
                            //if (changeuserId != 0)
                            //{
                            //    var updatechangeUserid = db.jntuh_changeuserid.Find(changeuserId);
                            //    updatechangeUserid.RegistrationNumber = userdata.RegistrationNumber;
                            //    updatechangeUserid.OldEmailId = model.OldEmailId;
                            //    updatechangeUserid.NewEmailId = model.NewEmailId;
                            //    updatechangeUserid.UserId = userdata.UserId;
                            //    updatechangeUserid.IsActive = true;
                            //    updatechangeUserid.UpdatedBy = 1;
                            //    updatechangeUserid.UpdatedOn = DateTime.Now;
                            //    db.Entry(updatechangeUserid).State = EntityState.Modified;
                            //    db.SaveChanges();
                            //}
                            //else
                            //{

                            jntuh_changeuserid changeUserIdtbl = new jntuh_changeuserid();
                            changeUserIdtbl.RegistrationNumber = userdata.RegistrationNumber;
                            changeUserIdtbl.OldEmailId = model.OldEmailId;
                            changeUserIdtbl.NewEmailId = model.NewEmailId;
                            changeUserIdtbl.UserId = userdata.UserId;
                            changeUserIdtbl.IsActive = true;
                            changeUserIdtbl.CreatedBy = 1;
                            changeUserIdtbl.CreatedOn = DateTime.Now;
                            db.jntuh_changeuserid.Add(changeUserIdtbl);
                            db.SaveChanges();
                            //}

                        }

                        if (newPassword != "")
                        {
                            my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.NewEmailId).ProviderUserKey);
                            //send email
                            IUserMailer mailer = new UserMailer();
                            //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                            //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                            //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                            //              .SendAsync();

                            //mailer.Welcome(my_aspnet_membership.Email, "LoginInformation",
                            //    "JNTUH Account password reset",
                            //    model.NewEmailId, newPassword, string.Empty, string.Empty).SendAsync();


                            string MailBody = string.Empty;
                            MailBody += "<p>Dear " + my_aspnet_membership.Email + "</p><br/>";
                            MailBody += "<p>You can now login to www.jntuhaac.in with the following credentials.</p>";
                            MailBody += "<p><b>UserName: " + model.NewEmailId + "</b></p>";
                            MailBody += "<p><b>Password: " + newPassword + "</b></p><br/>";
                            MailBody += "<p>Thanks & Regards</p>";
                            MailBody += "<p>Director, AAC,</p>";
                            MailBody += "<p>JNTUH, Hyderabad</p>";


                            MailMessage message = new MailMessage();
                            message.To.Add(my_aspnet_membership.Email);
                            message.Subject = "JNTUH Account Change UserId";
                            message.Body = MailBody;
                            message.IsBodyHtml = true;
                            //  message.Attachments.Add(new Attachment(filepath));
                            //  message.Attachments.Add(new Attachment(filepathsecond));
                            var smtp = new SmtpClient();
                            smtp.Credentials = new NetworkCredential("supportaac@jntuh.ac.in", "uaaac@aac");
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 587;
                            smtp.EnableSsl = true;
                            smtp.Send(message);



                            //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();

                            string[] email = my_aspnet_membership.Email.Split('@');
                            string maskEmail = email[0].Substring(0, 2) +
                                               new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                               email[0].Substring(email[0].Length - 1, 1);


                            TempData["Success"] = "UserId and Password has been reset & sent successfully to " +
                                                  maskEmail + "@" + email[1];
                        }
                        else
                        {
                            TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                        }
                        return RedirectToAction("ChangeUserName", "Account");
                    }
                    else
                    {
                        TempData["Error"] = "Details Doesn't Match,please try again";
                        return RedirectToAction("ChangeUserName", "Account");
                    }
                }
                else
                {
                    TempData["Error"] = "Your New Email Address Already Exist";
                    return RedirectToAction("ChangeUserName", "Account");
                }
            }
            else
            {
                TempData["Error"] = "Please Provide All Mandatory Fields.";
                return RedirectToAction("ChangeUserName", "Account");
            }
        }

        [HttpGet]
        public ActionResult CollegeList()
        {
            List<CollegesReport> clgList = new List<CollegesReport>();

            clgList = db.jntuh_college.Join(db.jntuh_address, c => c.id, a => a.collegeId,
                (c, a) => new { c, a })
                .Join(db.jntuh_district, jd => jd.a.districtId, jnd => jnd.id, (jd, jnd) => new { jd,jnd})
                .Where(m => m.jd.c.isActive == true && m.jd.a.addressTye == "college")
                .Select(m => new CollegesReport
            {
                collegeid =m.jd.c.id,
                collegeCode = m.jd.c.collegeCode,
                collegeName = m.jd.c.collegeName,
                email = m.jd.a.email,
                mobile = m.jd.a.landline,
                mandal = m.jd.a.mandal,
                districtName=m.jnd.districtName

            }).OrderBy(m => m.collegeName).ToList();

            return View(clgList);
        }
        [HttpGet]
        public ActionResult CollegeDetails(string Id)
        {
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(Id, WebConfigurationManager.AppSettings["CryptoKey"]));
            jntuh_college collegedate =
                db.jntuh_college.Where(c => c.id == collegeId && c.isActive == true).Select(s => s).FirstOrDefault();
            if (collegedate != null)
            {
                ViewBag.collegecode = collegedate.collegeCode.Trim();
                ViewBag.collegename = collegedate.collegeName.Trim();
            }

            var jntuh_address = db.jntuh_address.Where(s => s.collegeId == collegeId && s.addressTye == "COLLEGE").Select(e => e).FirstOrDefault();
            ViewBag.Address = jntuh_address.address;
            ViewBag.Email = jntuh_address.email;
            ViewBag.LandLine = jntuh_address.landline;
            ViewBag.website = jntuh_address.website;
            ViewBag.mandal = jntuh_address.mandal;
            ViewBag.pincode = jntuh_address.pincode;
            ViewBag.district = db.jntuh_district.Where(s => s.id == jntuh_address.districtId).Select(a => a.districtName).FirstOrDefault(); ;

            var PrincipalReg = db.jntuh_college_principal_registered.Where(e => e.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();
            ViewBag.Principalreg = PrincipalReg;
            var principal = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == PrincipalReg).Select(e => new { e.FirstName, e.MiddleName, e.LastName, e.Photo ,e.DesignationId,e.DepartmentId,e.Email}).FirstOrDefault();
            

            if (principal != null)
            {
                var principalname = principal.FirstName + " " + principal.MiddleName + " " + principal.LastName;
                ViewBag.principalname = principalname;
                ViewBag.principalEmail = principal.Email;
                ViewBag.principalDesignation = db.jntuh_designation.Where(s => s.id == principal.DesignationId).Select(a => a.designation).FirstOrDefault() ;
                ViewBag.principalDepartment = db.jntuh_department.Where(s => s.id == principal.DepartmentId).Select(a => a.departmentName).FirstOrDefault();
                ViewBag.principalphoto = principal.Photo;
            }
      
            var jntuh_departments = (from de in db.jntuh_degree
                                     join d in db.jntuh_department on de.id equals d.degreeId
                                     where de.isActive == true && d.isActive == true
                                     select new
                                     {
                                         degeeid = de.id,
                                         degee = de.degree,
                                         did = d.id,
                                         dept = d.departmentName
                                     }).ToList();

            List<jntuh_specialization> jntuh_specializations = db.jntuh_specialization.AsNoTracking().ToList();
            List<jntuh_designation> jntuh_designations = db.jntuh_designation.AsNoTracking().ToList();
            var TeachingFacultyData = db.jntuh_college_faculty_registered.Join(db.jntuh_registered_faculty,
                   CLGREG => CLGREG.RegistrationNumber, REG => REG.RegistrationNumber,
                   (CLGREG, REG) => new { CLGREG = CLGREG, REG = REG }).Where(e => e.CLGREG.collegeId == collegeId).Select(e => new
                   {
                       e.REG.FirstName,
                       e.REG.MiddleName,
                       e.REG.LastName,
                       e.REG.RegistrationNumber,
                       e.REG.id,
                       collegeid = e.CLGREG.id,
                       depId = e.CLGREG.DepartmentId,
                       Aadhaarno = e.CLGREG.AadhaarNumber,
                       AadhaarDoc = e.CLGREG.AadhaarDocument,
                       specId = e.CLGREG.SpecializationId,
                       IdentfdFor = e.CLGREG.IdentifiedFor,
                       e.REG.DesignationId,
                       e.CLGREG.DepartmentId,
                       e.REG.Absent,
                       e.REG.NotQualifiedAsperAICTE,
                       e.REG.PANNumber,
                       e.REG.NoSCM,
                       e.REG.PHDundertakingnotsubmitted,
                       e.REG.Blacklistfaculy,
                       e.REG.Photo
                   }).ToList();
            List<FacultyRegistration> FacultyRegistrationlist = new List<FacultyRegistration>();

            foreach (var teachingdata in TeachingFacultyData)
            {
                FacultyRegistration FacultyRegistration = new FacultyRegistration();
                FacultyRegistration.FirstName = teachingdata.FirstName;
                FacultyRegistration.LastName = teachingdata.LastName;
                FacultyRegistration.MiddleName = teachingdata.MiddleName;
                FacultyRegistration.RegistrationNumber = teachingdata.RegistrationNumber;
                FacultyRegistration.facultyPhoto = teachingdata.Photo;
                if (teachingdata.specId != null)
                {
                    int SpecId = (int)teachingdata.specId;
                    FacultyRegistration.SpecializationName = jntuh_specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault();
                }
                if (teachingdata.DepartmentId != null)
                {
                    int deptId = (int)teachingdata.DepartmentId;
                    FacultyRegistration.DepartmentName = jntuh_departments.Where(e => e.did == deptId).Select(e => e.degee + "-" + e.dept).FirstOrDefault();
                }
                if (teachingdata.DesignationId != null)
                {
                    int desigId = (int)teachingdata.DesignationId;
                    FacultyRegistration.designation = jntuh_designations.Where(e => e.id == desigId).Select(e => e.designation).FirstOrDefault();
                }
                FacultyRegistrationlist.Add(FacultyRegistration);
            }

            return View(FacultyRegistrationlist.OrderBy(d => d.DepartmentName));
        }


        public ActionResult DirectorsList()
        {
            return View();
        }

        public ActionResult AuditcellView() 
        {
            return View();
        }
    }
}