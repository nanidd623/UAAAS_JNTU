using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class PaymentLoginController : Controller
    {
        //
        // GET: /PaymentLogin/
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Applicationmirrors()
        {
            return View();

        }
        public ActionResult LogOn()
        {
            return View();
        }


        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                MembershipUserCollection allUsers = Membership.GetAllUsers();

                bool isOnline = true;
                //foreach (MembershipUser user in allUsers)
                //{
                //    // if user is currently online, add to gridview list
                //    if (user.IsOnline == isOnline && user.UserName == model.UserName && Roles.GetRolesForUser(user.UserName)[0] == "College")
                //    {
                //        ViewBag.Multiple = true;
                //        return View(model);
                //    }
                //}

                //  Restrict Multiple Logins on same user name

                foreach (MembershipUser user in allUsers)
                {
                    // if user is currently online, add to gridview list
                    if (user.IsOnline == isOnline && user.UserName == model.UserName && Roles.GetRolesForUser(user.UserName)[0] == "College")
                    {
                        if (user.UserName != "cstl")
                        {
                            ViewBag.Multiple = true;
                            return View(model);
                        }

                    }
                }

                if (Membership.ValidateUser(model.UserName.TrimEnd(' '), model.Password.TrimEnd(' ')))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    int userID = Convert.ToInt32(Membership.GetUser(model.UserName).ProviderUserKey);
                    MembershipUser user = Membership.GetUser(false);

                    //login log
                    string _sessionId = Guid.NewGuid().ToString();
                    user_login_logout userLog = new user_login_logout();
                    userLog.UserId = userID;
                    userLog.IPAddress = Request.UserHostAddress;
                    userLog.SessionId = _sessionId;
                    userLog.Login = DateTime.Now;
                    userLog.Logout = null;
                    userLog.isOnline = true;
                    userLog.isActive = true;
                    userLog.createdBy = userID;
                    userLog.createdOn = userLog.Login;
                    db.user_login_logout.Add(userLog);
                    db.SaveChanges();

                    Session["LOGIN_GUID"] = _sessionId;

                    //Get browser details
                    var browser = Request.Browser;
                    user_browsers uBrowser = new user_browsers();
                    uBrowser.UserId = userID;
                    uBrowser.IPAddress = Request.UserHostAddress;
                    uBrowser.Type = browser.Type;
                    uBrowser.Name = browser.Browser;
                    uBrowser.Version = browser.Version;
                    uBrowser.MajorVersion = browser.MajorVersion.ToString();
                    uBrowser.MinorVersion = browser.MinorVersion.ToString();
                    uBrowser.Platform = browser.Platform;
                    uBrowser.IsBeta = browser.Beta.Equals(true) ? "True" : "False";
                    uBrowser.IsCrawler = browser.Crawler.Equals(true) ? "True" : "False";
                    uBrowser.IsAOL = browser.AOL.Equals(true) ? "True" : "False";
                    uBrowser.IsWin16 = browser.Win16.Equals(true) ? "True" : "False";
                    uBrowser.IsWin32 = browser.Win32.Equals(true) ? "True" : "False";
                    uBrowser.SupportsFrames = browser.Frames.Equals(true) ? "True" : "False";
                    uBrowser.SupportsTables = browser.Tables.Equals(true) ? "True" : "False";
                    uBrowser.SupportsCookies = browser.Cookies.Equals(true) ? "True" : "False";
                    uBrowser.SupportsVBScript = browser.VBScript.Equals(true) ? "True" : "False";
                    uBrowser.SupportsJavaScript = browser.EcmaScriptVersion.ToString();
                    uBrowser.SupportsJavaApplets = browser.JavaApplets.Equals(true) ? "True" : "False";
                    uBrowser.SupportsActiveXControls = browser.ActiveXControls.Equals(true) ? "True" : "False";
                    uBrowser.SupportsJavaScriptVersion = browser["JavaScriptVersion"].ToString();
                    uBrowser.createdBy = userID;
                    uBrowser.createdOn = DateTime.Now;
                    db.user_browsers.Add(uBrowser);
                    db.SaveChanges();

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();

                        string StrCollegeFee = ""; string StrLatefeeCollegeFee= "";
                        if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("College")).Select(r => r.id).FirstOrDefault()))
                        {
                            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                            //string CollegeCode = db.jntuh_college.Where(C => C.id == userID).Select(C => C.collegeCode).FirstOrDefault();
                            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
                            StrCollegeFee = db.jntuh_paymentresponse.Where(E => E.CollegeId == clgCode && E.AuthStatus == "0300" && E.PaymentTypeID == 7).Select(E => E.ErrorDescription).FirstOrDefault();
                            StrLatefeeCollegeFee = db.jntuh_paymentresponse.Where(E => E.CollegeId == clgCode && E.AuthStatus == "0300" && E.PaymentTypeID == 8).Select(E => E.ErrorDescription).FirstOrDefault();
                           var currentDate = DateTime.Now;

                           #region Payment
                           //if (currentDate > new DateTime(2016, 3, 20, 23, 59, 59))
                           //{
                           //    if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("College")).Select(r => r.id).FirstOrDefault()))
                           //    {
                           //        return RedirectToAction("College", "Dashboard");

                           //    }
                           //}
                           //else
                           //{
                           //}
                           #endregion
                           
                               if (StrCollegeFee == null || StrCollegeFee == "")
                               {

                                   TempData["PaymentStatus"] = 1;
                                   return RedirectToAction("FeeDetailsandPayment", "PaymentLogin");
                               }
                               else if (StrLatefeeCollegeFee == null || StrLatefeeCollegeFee == "")
                               {

                                   return RedirectToAction("LateFeeDetailsandPayment", "LateFee");
                               }
                           
                            
                            

                        }
                       
                        //else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("Committee")).Select(r => r.id).FirstOrDefault()))
                        //{
                        //    return RedirectToAction("Index", "Committee");
                        //}
                        //else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry")).Select(r => r.id).FirstOrDefault()))
                        //{
                        //    return RedirectToAction("DataEntryAssignedColleges", "DataEntryAssignedColleges");
                        //}
                        //else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("Faculty")).Select(r => r.id).FirstOrDefault()))
                        //{
                        //    int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();
                        //    string FacultyType = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.type).FirstOrDefault();
                        //    string fid = Utilities.EncryptString(facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);

                        //    if (FacultyType != "Adjunct")
                        //        return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                        //    else
                        //        return RedirectToAction("AdjunctFacty", "OnlineRegistration", new { fid = fid });
                        //    //TempData["Error"] = "This Service is unavilable for short time";
                        //}
                        //else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("Admin")).Select(r => r.id).FirstOrDefault()))
                        //{
                        //    return RedirectToAction("Admin", "Dashboard");
                        //}
                        //else
                        //{
                        //    return RedirectToAction("Index", "Home");
                        //}
                    }
                }
                else
                {

                    TempData["Error"] = "The username or password provided is incorrect.";
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult FeeDetailsandPayment()
        {
            #region PAyment
            //var currentDate1 = DateTime.Now;
            //if (currentDate1 >= new DateTime(2016, 3, 20, 23, 59, 59))
            //{
            //    return RedirectToAction("College", "Dashboard");

            //}
            //else
            //{
            //}
            #endregion
           
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                //int userCollegeID = 179;
                string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();

                DateTime todayDate = DateTime.Now.Date;
                int Pagestatus = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                         editStatus.IsCollegeEditable == true &&
                                                                                         editStatus.editFromDate <= todayDate &&
                                                                                         editStatus.editToDate >= todayDate)
                                                                    .Select(editStatus => editStatus.id)
                                                                    .FirstOrDefault();
                if (Pagestatus == 0 && Roles.IsUserInRole("College"))
                {
                    ViewBag.NotUpload = true;
                }

                else
                {
                    ViewBag.NotUpload = false;
                }

                if (userCollegeID > 0 && userCollegeID != null)
                {

                    //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
                    var jntuh_specialization = db.jntuh_specialization;
                    var jntuh_department = db.jntuh_department;
                    var jntuh_degree = db.jntuh_degree;
                    var jntuh_shift = db.jntuh_shift;
                    List<int> pgdegrees = new List<int>();
                    List<int> ugdegrees = new List<int>();
                    List<int> totaldegrees = new List<int>();
                    long ugSpecializationAmmount = 0;
                    long pgSpecializationAmmount = 0;
                    long applicationFee = 0;
                    int ugCount = 0;
                    int pgCount = 0;
                    if (TempData["PaymentStatus"] != null)
                        TempData["PaymentStatus1"] = TempData["PaymentStatus"];
                    else
                    {
                        int status = 0;
                        TempData["PaymentStatus1"] = "";
                    }

                    var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

                    int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                    foreach (var item in intakeExisting)
                    {
                        jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                              .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                                                              .Select(e => e)
                                                              .FirstOrDefault();


                        if (details != null)
                        {
                            if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
                            {


                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                    {
                                        ugSpecializationAmmount = 25000;
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                    else
                                    {
                                        ugCount++;
                                        ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                }

                            }
                            else
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    pgCount++;
                                    pgSpecializationAmmount = pgCount * 12000;
                                    totaldegrees.AddRange(pgdegrees);
                                }

                            }
                        }



                        // totaldegrees.AddRange(pgdegrees);
                        //totaldegrees.AddRange(ugdegrees);
                    }
                    if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                        applicationFee = 1000;
                    else
                        applicationFee = 750;


                    // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                    //ugdegrees.AsEnumerable().Where(r=> new{r.co})
                    ViewBag.userCollegeID = userCollegeID;
                    ViewBag.countofUgcourse = ugdegrees.Count;
                    ViewBag.countofPgcourse = pgdegrees.Count;
                    ViewBag.ugSpecializationAmmount = ugSpecializationAmmount;
                    ViewBag.pgSpecializationAmmount = pgSpecializationAmmount;
                    ViewBag.applicationFee = applicationFee;

                    ViewBag.totalFee = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                    ViewBag.collegeCode = clgCode;
                    ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                    var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode).ToList();
                    ViewBag.Payments = payments;
                    var currentYear = DateTime.Now.Year;
                    ViewBag.IsPaymentDone = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300") > 0;
                    var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl"];
                    var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                    var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                    var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                    var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
                    var msg = "";
                    if (userCollegeID == 375)
                    {
                        msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                        var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                        msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                    }
                    else
                    {
                        msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                        var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                        msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                    }

                    ViewBag.msg = msg;

                }
                return View();
            
           
        }
        public ActionResult Payment(int? id, string collegeId)
        {
            return View();
        }
        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {

            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            req.TxnAmount = txnAmount;
            req.CollegeCode = collegeCode;
            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = 7;
            db.jntuh_paymentrequests.Add(req);

            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public static string GetHMACSHA256(string text, string key)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(key);
            byte[] message = encoder.GetBytes(text);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
        //[HttpPost]
        //public ActionResult FeeDetailsandPayment(string msg)
        //{
        //    var appSettings = WebConfigurationManager.AppSettings;
        //    var req = new jntuh_paymentrequests();
        //    //req.MerchantID = appSettings["MerchantID"];
        //    //req.CustomerID = appSettings["CustomerID"];
        //    //req.SecurityID = appSettings["SecurityID"];
        //    //req.CurrencyType = appSettings["CurrencyType"];
        //    //req.TxnDate = DateTime.Now;
        //    //db.jntuh_paymentrequests.Add(req);
        //    //db.SaveChanges();
        //    SaveResponse(msg, "ChallanNumber");
        //    return RedirectToAction("FeeDetailsandPayment");
        //}

       

    }
}
