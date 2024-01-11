

using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services.Description;
using PANAPIMVC.Controllers;
using UAAAS.Models;
using UAAAS.Mailers;
using System.Threading;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class OnlineRegistrationController : BaseController
    {       
        private int PANCount = 0;
        int adminCollegeId = 0;
        private string CheckPANStatus = string.Empty;
        private string filePath = System.Configuration.ConfigurationManager.AppSettings["PANCertificate"];
        private uaaasDBContext db = new uaaasDBContext();
        private uaaasDBContext db1 = new uaaasDBContext();

        /// <summary>
        /// This View Shows Faculty Registration INSTRUCTIONS
        /// </summary>
        /// <returns>Faculty Registration INSTRUCTIONS</returns>
        public ActionResult Welcome()
        {
            return View();
        }

        /// <summary>
        /// When we Given Appeal Option to Colleges This View Showes College INSTRUCTIONS
        /// </summary>
        /// <returns></returns>
        public ActionResult PreambleAppeal()
        {
            return View();
        }
        
        /// <summary>
        /// When we Given Colleges Edit option This View Showes College INSTRUCTIONS
        /// </summary>
        /// <returns></returns>
        public ActionResult Preamble()
        {
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true&&a.isActive==true)
                    .Select(s => s.actualYear)
                    .FirstOrDefault();
            ViewBag.NextacademicYear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.academicYear).FirstOrDefault();
            return View();
        }

        /// <summary>
        /// Email Checking with Registration number (Duplicates) 
        /// </summary>
        /// <param name="EditMobile"></param>
        /// <param name="RegistrationNumber"></param>
        /// <returns>json true/false  means Duplicates</returns>
        [HttpPost]
        public JsonResult CheckEmail(string Email)
        {           
            string CheckingEmail =
                db.jntuh_registered_faculty.Where(F => F.Email == Email.Trim()).Select(F => F.Email).FirstOrDefault();
            if (CheckingEmail != null && CheckingEmail != "")
            {
                if (CheckingEmail.Trim() == Email.Trim())
                    return Json(false);
                else
                    return Json(true);
            }
            else
                return Json(true);

        }

        /// <summary>
        /// PAN Number Checking From PAN Service (PAN API)
        /// </summary>
        /// <param name="PANNumber"></param>
        /// <returns>PAN Nuber Details ,PAN number,first name,middle name,last name</returns>
        [HttpPost]
        public JsonResult CheckPanNumber(string PANNumber)
        {
            string SendPansToAPI = string.Empty;
            string CheckingPANNumber =
                db.jntuh_registered_faculty.Where(F => F.PANNumber == PANNumber.Trim())
                    .Select(F => F.PANNumber)
                    .FirstOrDefault();
            if (CheckingPANNumber != null && CheckingPANNumber != "")
            {
                if (CheckingPANNumber.Trim() == PANNumber.Trim())
                    return Json("PAN Number already registred.", JsonRequestBehavior.AllowGet);
                else
                    return Json(true);
            }
            else if (!string.IsNullOrEmpty(PANNumber))
            {
                var panstatusdb =
                   db.jntuh_pan_status.Where(p => p.PANNumber.Trim() == PANNumber.Trim() && p.PANStatus == "E")
                       .Select(s => s.PANStatus)
                       .FirstOrDefault();
                if (panstatusdb == "E")
                {
                    return Json(true);
                }
                else
                {
                    var UserID = "V0131101";
                    SendPansToAPI = UserID + "^" + PANNumber;
                    var pandetails = SendPanRequest1(SendPansToAPI);
                    string[] pandetails1 = pandetails.Split(',');
                    jntuh_pan_status PANStatus = new jntuh_pan_status();
                    PANStatus.PANNumber = pandetails1[0].ToString();
                    PANStatus.PANStatus = pandetails1[1].ToString();
                    PANStatus.FirstName = pandetails1[3].ToString();
                    PANStatus.LastName = pandetails1[2].ToString();
                    PANStatus.MiddleName = pandetails1[4].ToString();
                    // PANStatus.MiddleName = "";
                    PANStatus.Title = pandetails1[5].ToString();
                    if (!string.IsNullOrEmpty(pandetails1[6]))
                    {
                        PANStatus.LastUpdated = UAAAS.Models.Utilities.DDMMYY2MMDDYY(pandetails1[6]);
                    }
                    PANStatus.IsActive = true;
                    PANStatus.createdby = 1;
                    PANStatus.CreateOn = DateTime.Now;
                    db.jntuh_pan_status.Add(PANStatus);
                    db.SaveChanges();
                    var panStatus = pandetails1[1].ToString();
                    if (panStatus != "E")
                        return Json("PAN Number Not Valid", JsonRequestBehavior.AllowGet);
                    else
                        return Json(true);
                }

            }
            else
                return Json(true);

        }



        [Authorize(Roles = "College,Admin")]
        public ActionResult College(string collegecode)
        {
            return RedirectToAction("College", "Dashboard");
            //get current user Id
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //get current user CollegeId
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            ViewBag.CollegeId = userCollegeID;
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(collegecode))
            {
                adminCollegeId = db.jntuh_college.Where(c => c.collegeCode == collegecode).Select(c => c.id).FirstOrDefault();
                userCollegeID = adminCollegeId;
            }

            #region CollegeNews

            //Get current Date and time
            DateTime dateTime = DateTime.Now.Date;

            //Get bulletin board events based on start date and end date
            List<CollegeDashboardNews> news = db.jntuh_newsevents.Where(n => n.isActive == true &&
                                                            n.isNews == false &&
                                                            (n.startDate == null || n.startDate <= dateTime) &&
                                                            (n.endDate == null || n.endDate >= dateTime))
                                                            .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(n => n.createdDate).ToList();

            //Get College News and events based on start date and end date
            List<CollegeDashboardNews> cNews = db.jntuh_college_news.Where(collegeNews => collegeNews.isActive == true &&
                                                              collegeNews.collegeId == userCollegeID &&
                                                              (collegeNews.startDate == null ||
                                                              collegeNews.startDate <= dateTime) &&
                                                              (collegeNews.endDate == null ||
                                                              collegeNews.endDate >= dateTime)
                                                              && collegeNews.title != "DEFICIENCY REPORT AS PER FORM 415"
                                                              )
                                                              .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(collegeNews => collegeNews.createdDate).ToList();

            CollegeDashboardNews cn = new CollegeDashboardNews();

                var notSubmittedIds = db.jntuh_college_pgcourses.Where(pg => pg.isActive == true).Select(pg => pg.collegeId).ToList();

                if (notSubmittedIds.Contains(userCollegeID))
                {
                    cn.url = "/Content/Upload/News/OnlySoftCopySubmitted.pdf";
                    cn.newstitle = "Acknowledgement for PG Courses submitted - Soft Copy only";
                    cn.createdDate = Convert.ToDateTime("09/04/2014");

                    cNews.Add(cn);
                }
            ViewBag.Events = news.Union(cNews).OrderByDescending(collegeNews => collegeNews.createdDate).Take(5);

            #endregion



            UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();
            UGWithDeficiency.CollegeId = userCollegeID;

            return View(UGWithDeficiency);
        }

        /// <summary>
        /// PAN Check Is valid or not and Duplicate when we are given Faculty edit option. 
        /// </summary>
        /// <param name="EditPANNumber"></param>
        /// <param name="RegistrationNumber"></param>
        /// <returns>It Returns PAN Number Not Valid,true,PAN Number Already Exists</returns>
        [HttpPost]
        public JsonResult EditCheckPanNumber(string EditPANNumber,string RegistrationNumber)
        {
            string SendPansToAPI = string.Empty;
            string ifpanexist =
                db.jntuh_pan_status.Where(p => p.PANNumber == EditPANNumber&&p.PANStatus=="E").Select(s => s.PANNumber).FirstOrDefault();

            if (string.IsNullOrEmpty(ifpanexist))
            {
                if (EditPANNumber.Trim().Length == 10)
                {
                    var UserID = "V0131101";
                    SendPansToAPI = UserID + "^" + EditPANNumber;
                    var pandetails = SendPanRequest1(SendPansToAPI);
                    string[] pandetails1 = pandetails.Split(',');
                    jntuh_pan_status PANStatus = new jntuh_pan_status();
                    PANStatus.PANNumber = pandetails1[0].ToString();
                    PANStatus.PANStatus = pandetails1[1].ToString();
                    PANStatus.FirstName = pandetails1[3].ToString();
                    PANStatus.LastName = pandetails1[2].ToString();
                    PANStatus.MiddleName = pandetails1[4].ToString();
                    PANStatus.Title = pandetails1[5].ToString();
                    if (!string.IsNullOrEmpty(pandetails1[6]))
                    {
                        PANStatus.LastUpdated = UAAAS.Models.Utilities.DDMMYY2MMDDYY(pandetails1[6]);
                    }
                    PANStatus.IsActive = true;
                    PANStatus.createdby = 1;
                    PANStatus.CreateOn = DateTime.Now;
                    db.jntuh_pan_status.Add(PANStatus);
                    db.SaveChanges();
                    var panStatus = pandetails1[1].ToString();
                    PANCount = 1;
                    CheckPANStatus = panStatus;
                    if (panStatus != "E")
                    {
                        return Json("PAN Number Not Valid", JsonRequestBehavior.AllowGet);
                    }                        
                    else
                    {
                        string CheckingRegistratonNumber1 = db.jntuh_registered_faculty.Where(F => F.PANNumber.Trim() == EditPANNumber.Trim()&&F.isActive!=false).Select(F => F.RegistrationNumber).FirstOrDefault();
                        int PANcount1 = db.jntuh_registered_faculty.Where(F => F.PANNumber.Trim() == EditPANNumber.Trim()&&F.isActive!=false).Select(e => e.id).Count();
                        if (CheckingRegistratonNumber1 != null && CheckingRegistratonNumber1 != "")
                        {
                            if (CheckingRegistratonNumber1.Trim() == RegistrationNumber.Trim())
                            {
                                if (PANcount1 > 1)
                                    return Json("PAN Number Already Exists.", JsonRequestBehavior.AllowGet);
                                else
                                    return Json(true);
                            }
                            else
                                return Json("PAN Number Already Exists.", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(true);
                        }
                    }
                       
                }
                else
                {
                    return Json("PAN Number Not Valid", JsonRequestBehavior.AllowGet);
                }
            }            
            string CheckingRegistratonNumber = db.jntuh_registered_faculty.Where(F => F.PANNumber.Trim() == EditPANNumber.Trim()&&F.isActive!=false).Select(F => F.RegistrationNumber).FirstOrDefault();
            int PANcount =db.jntuh_registered_faculty.Where(F => F.PANNumber.Trim() == EditPANNumber.Trim()&&F.isActive!=false).Select(e => e.id).Count();
            if (CheckingRegistratonNumber != null && CheckingRegistratonNumber != "")
            {
                if (CheckingRegistratonNumber.Trim() == RegistrationNumber.Trim())
                {
                    if (PANcount > 1)
                        return Json("PAN Number Already Exists.", JsonRequestBehavior.AllowGet);
                    else
                        return Json(true);
                }
                else
                    return Json("PAN Number Already Exists.", JsonRequestBehavior.AllowGet);
            }
            else
                return Json(true);

        }
        /// <summary>
        /// Get the PAN Details by the PAN number
        /// </summary>
        /// <param name="PANNo"></param>
        /// <returns>if its valid return firstname,middlename,lastname</returns>
        [HttpPost]
        public JsonResult GetDetailsBasedonPANNumber(string PANNo)
        {
            var jntuh_pan_status = db.jntuh_pan_status.AsNoTracking().ToList();
            string PANData = jntuh_pan_status.Where(F => F.PANNumber.Trim() == PANNo.Trim() && F.PANStatus=="E").Select(F => F.PANNumber).FirstOrDefault();
            string Details = "";
            if (!string.IsNullOrEmpty(PANData))
            {
                var PANDetails = jntuh_pan_status.Where(F => F.PANNumber.Trim() == PANNo.Trim() && F.PANStatus == "E").Select(F => F).FirstOrDefault();
                Details = PANDetails.PANNumber + "," + PANDetails.PANStatus + "," + PANDetails.FirstName + "," +
                          PANDetails.LastName + "," + PANDetails.MiddleName + "," + PANDetails.Title;
            }
            else
            {
                if(PANNo.Trim().Length==10)
                {
                    string SendPansToAPI1 = string.Empty;
                    var UserID = "V0131101";
                    SendPansToAPI1 = UserID + "^" + PANNo;
                    Details = SendPanRequest1(SendPansToAPI1);
                    var pandetails = Details;
                    string[] pandetails1 = pandetails.Split(',');
                    if (pandetails1[0] != "")
                    {
                        jntuh_pan_status PANStatus = new jntuh_pan_status();
                        PANStatus.PANNumber = pandetails1[0].ToString();
                        PANStatus.PANStatus = pandetails1[1].ToString();
                        PANStatus.FirstName = pandetails1[3].ToString();
                        PANStatus.LastName = pandetails1[2].ToString();
                        PANStatus.MiddleName = pandetails1[4].ToString();
                        PANStatus.Title = pandetails1[5].ToString();
                        if (!string.IsNullOrEmpty(pandetails1[6]))
                        {
                            PANStatus.LastUpdated = UAAAS.Models.Utilities.DDMMYY2MMDDYY(pandetails1[6]);
                        }
                        PANStatus.IsActive = true;
                        PANStatus.createdby = 1;
                        PANStatus.CreateOn = DateTime.Now;
                        db.jntuh_pan_status.Add(PANStatus);
                        db.SaveChanges();
                    }
                }
            }
            return Json(new {Details}, "application/json", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Mobile Number Checking with Registration number (Duplicates) 
        /// </summary>
        /// <param name="EditMobile"></param>
        /// <param name="RegistrationNumber"></param>
        /// <returns>json true/false  means Duplicates</returns>
        [HttpPost]
        public JsonResult CheckMobileNumber(string EditMobile, string RegistrationNumber)
        {
            var CheckingRegistrationNumber =db.jntuh_registered_faculty.Where(e => e.Mobile.Trim() == EditMobile.Trim()).Select(e => e.RegistrationNumber).FirstOrDefault();
            int Mobilecount = db.jntuh_registered_faculty.Where(F => F.Mobile.Trim() == EditMobile.Trim()).Select(e => e.id).Count();
            if (!string.IsNullOrEmpty(CheckingRegistrationNumber))
            {
                if (CheckingRegistrationNumber.Trim() == RegistrationNumber.Trim())
                {
                      if (Mobilecount > 1)
                          return Json(false);
                    else
                        return Json(true);
                }
                else
                    return Json(false);
            }

            return Json(true);
        }

        /// <summary>
        /// Aadhaar Number Checking with Registration number (Duplicates) 
        /// </summary>
        /// <param name="EditMobile"></param>
        /// <param name="RegistrationNumber"></param>
        /// <returns>json true/false means Duplicates</returns>
        [HttpPost]
        public JsonResult CheckAadhaarNumber(string EditAadhaarNumber, string RegistrationNumber)
        {
            var status = aadharcard.validateVerhoeff(EditAadhaarNumber.Trim());
            if (status)
            {
                var CheckingRegistrationNumber = db.jntuh_registered_faculty.Where(e => e.AadhaarNumber.Trim() == EditAadhaarNumber.Trim() && e.isActive != false).Select(e => e.RegistrationNumber).FirstOrDefault();
                int Aadhaarcount = db.jntuh_registered_faculty.Where(F => F.AadhaarNumber.Trim() == EditAadhaarNumber.Trim() && F.isActive != false).Select(e => e.id).Count();
                if (!string.IsNullOrEmpty(CheckingRegistrationNumber))
                {
                    if (CheckingRegistrationNumber.Trim() == RegistrationNumber.Trim())
                    {
                        if (Aadhaarcount > 1)
                            return Json(false);
                        else
                            return Json(true);
                    }
                    else
                        return Json(false);
                }

            }
            else
            {
                return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }
            return Json(true);
        }

        /// <summary>
        ///PAN Number Check Existing or NO PAN
        /// </summary>
        /// <param name="EditMobile"></param>
        /// <param name="RegistrationNumber"></param>
        /// <returns>PAN Details</returns>
        protected string SendPanRequest1(string Data)
        {
            string Details = string.Empty;
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(Data);
            var path = filePath;
            //var path = Server.MapPath("jntuh_Sign.p12");
            //X509Certificate2 uidCert = new X509Certificate2(filePath, "123456", X509KeyStorageFlags.MachineKeySet);
            X509Certificate2 uidCert = new X509Certificate2(filePath, "123", X509KeyStorageFlags.MachineKeySet);
            byte[] sig = Sign(bytes, uidCert);
            String Signature = Convert.ToBase64String(sig);
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate,
                X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };
            string post_data = "data=" + Data + "&signature=" +
                               System.Web.HttpContext.Current.Server.UrlEncode(Signature);
            string url = "https://59.163.46.2/TIN/PanInquiryBackEnd";
            // create a request
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";
            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(post_data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();
            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();
            HttpWebResponse res = (HttpWebResponse) request.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.Default);
            string backstr = sr.ReadToEnd();
            Logger Log = new Logger();
            Log.Log(backstr);
            string panRes = backstr;
            string Pan1 = string.Empty;
            string[] resp = panRes.Split('^');
            if (resp[0] == "1")
            {
                int strLength = resp.Length;
                int LastRowCount = 0;
                if (strLength == 11)
                {
                    LastRowCount = 2;

                }
                else if (strLength == 21)
                {
                    LastRowCount = 3;

                }
                else if (strLength == 31)
                {
                    LastRowCount = 4;
                }
                else if (strLength == 41)
                {
                    LastRowCount = 5;
                }
                else if (strLength == 51)
                {
                    LastRowCount = 6;
                }
                int j = 0;
                for (int k = 1; k < LastRowCount; k++)
                {
                    Details = resp[j + 1] + "," + resp[j + 2] + "," + resp[j + 3] + "," + resp[j + 4] + "," +
                              resp[j + 5] + "," + resp[j + 6] + "," + resp[j + 7] + "," + resp[j + 8] + "," +
                              resp[j + 9] + "," + resp[j + 10];



                    j = j + k;
                    j = k*10;
                }

            }
            string[] Strcount = Details.Split(',');
            return Details;
        }

        public static byte[] Sign(byte[] data, X509Certificate2 certificate)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            // setup the data to sign
            ContentInfo content = new ContentInfo(data);
            SignedCms signedCms = new SignedCms(content, false);
            CmsSigner signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
            // create the signature
            signedCms.ComputeSignature(signer);
            return signedCms.Encode();
        }

        private string VerifyFRN(string registrationNumber)
        {
            int fId =
                db.jntuh_registered_faculty.Where(f => f.UniqueID == registrationNumber)
                    .Select(f => f.id)
                    .FirstOrDefault();

            if (fId != 0)
            {
                int lastDigit = int.Parse(registrationNumber.GetLast(3));
                registrationNumber = registrationNumber.Substring(0, registrationNumber.Length - 3) +
                                     (lastDigit + 1).ToString().PadLeft(3, '0');
                VerifyFRN(registrationNumber);
            }

            return registrationNumber;
        }

        /// <summary>
        /// Faculty Registration Getting view and after login faculty show the faculty Deatals
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult FacultyNew(string fid)
        {
            return RedirectToAction("Logon", "Account");
            //TempData["SUCCESS"] = "";
            TempData["EDITERROR"] = null;
            FacultyRegistration vFaculty = new FacultyRegistration();
            //return RedirectToAction("ComingSoon", "Labs");
            //#region For Stop the online registrations   Written by Srinivas
            //if (fid==null)
            //    return RedirectToAction("LogOn", "Account");
            //#endregion
            if (TempData["FACULTY"] != null)
            {
                vFaculty = (FacultyRegistration)TempData["FACULTY"];
            }
            ViewBag.ExperienceStatus = fid == null ? 0 : 1;
            int fID = 0;
            int userID = 0;
            int collegeId = 0;
            //Faculty Registation Open on 12-04-2018
            //Faculty Registation Open on 24-04-2018
            //Faculty Registation Stop Date on 02-05-2018
            //Faculty Registation Open on Date 02-07-2018
            //if (String.IsNullOrEmpty(fid))
            //{
            //    //return RedirectToAction("Logon", "Account");
            //    return RedirectToAction("ComingSoon", "Labs");
            //}

            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                }

                //if (facultyId != fID && !Roles.IsUserInRole("Admin"))
                //{
                //    fID = facultyId;
                //}

            }
            else if (fid != null)
            {
                string fUser = "";
                string fPwd = "";

                if (TempData["FUserName"] != null)
                {
                    fUser = TempData["FUserName"].ToString();
                }

                if (TempData["FPassword"] != null)
                {
                    fPwd = TempData["FPassword"].ToString();
                }

                if (Membership.ValidateUser(fUser.TrimEnd(' '), fPwd.TrimEnd(' ')))
                {
                    FormsAuthentication.SetAuthCookie(fUser, false);
                    //int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();
                    //string fid = Utilities.EncryptString(facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);

                    return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                }

                return RedirectToAction("Logon", "Account");
            }

            //  var jntuh_registered_faculty_log = db.jntuh_registered_faculty_log.AsNoTracking().ToList();
            //  var DeactivationReason =jntuh_registered_faculty_log.Where(i => i.UserId == userID).Select(i => i.DeactivationReason).FirstOrDefault();
            // var regno =jntuh_registered_faculty.Where(i => i.id == fID).Select(i => i.RegistrationNumber).FirstOrDefault();
            //  var remarks =jntuh_registered_faculty_log.Where(i => i.UserId == userID && i.RegistrationNumber == regno).Select(i => i.FacultyApprovedStatus).FirstOrDefault();
            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;
            //fID = 98564;
            //ViewBag.remarks = remarks;
            //if (remarks == 2)
            //{
            //    TempData["remarks"] = "Not Approved because of " + DeactivationReason;
            //}
            //else
            //{
            //    TempData["remarks"] = "";
            //}
            //ViewBag.Id = fid;
            //ViewBag.FacultyID = fID;
            var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(f => f.id == fID).ToList();
            DateTime todayDate = DateTime.Now.Date;

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();

            var jntuh_departments = (from a in jntuh_department
                                     join b in jntuh_degree on a.degreeId equals b.id
                                     select new
                                     {
                                         id = a.id,
                                         departmentName = b.degree + "-" + a.departmentName
                                     }).ToList();



            string existingDepts = string.Empty;
            int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56, 71, 72, 73, 74, 75, 76, 77, 78, 60 };
            foreach (var item in jntuh_departments.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions = jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            FacultyRegistration regFaculty = new FacultyRegistration();
            //Faculty Edit Conditions
            ViewBag.IsEditable = false;
            ViewBag.IsPhdfaculty = false;
            var currentDate = DateTime.Now;
            DateTime facultyeditclosedate = new DateTime();
            if (currentDate < new DateTime(2018, 02, 15, 23, 59, 59))
            {
                //string fregno = db.jntuh_registered_faculty.Where(i => i.id == fID && (i.type == "Adjunct" || i.Blacklistfaculy == true)).Select(s => s.RegistrationNumber).FirstOrDefault();
                //if (!string.IsNullOrEmpty(fregno))
                //{
                //    TempData["EDITERROR"] = "You don't have permissions to Edit";
                //}
                //else
                //{
                //    string fregnoa = db.jntuh_registered_faculty.Where(i => i.id == fID && i.isActive != false).Select(s => s.RegistrationNumber).FirstOrDefault();
                //    if (!string.IsNullOrEmpty(fregnoa))
                //    {
                //        string editregno =
                //        db.jntuh_college_faculty_registered_copy.Where(e => e.RegistrationNumber == fregnoa && e.collegeId == 375)
                //            .Select(s => s.RegistrationNumber)
                //            .FirstOrDefault();
                //        if (string.IsNullOrEmpty(editregno))
                //        {
                //            ViewBag.IsEditable = true;
                //        }
                //        else
                //        {
                //            //ViewBag.IsPhdfaculty = true;
                //            TempData["EDITERROR"] = "The details of your profile with the submitted documents are in order and hence the edit option is not available to you.";
                //        }
                //    }
                //    else
                //    {

                //        TempData["EDITERROR"] = "Edit option not provided as this registration number was surrendered under multiple registrations.";
                //    }
                //}
            }

            // string fregno = db.jntuh_registered_faculty.Where(i => i.id == fID && i.type != "Adjunct" && i.isActive != false&&i.Blacklistfaculy==true).Select(s => s.RegistrationNumber).FirstOrDefault();


            if (fID == 0)
            {
                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                        .Select(e => new RegisteredFacultyEducation
                        {
                            educationId = e.id,
                            educationName = e.educationCategoryName,
                            studiedEducation = string.Empty,
                            specialization = string.Empty,
                            passedYear = 0,
                            percentage = 0,
                            division = 0,
                            university = string.Empty,
                            place = string.Empty,
                            facultyCertificate = string.Empty,
                        }).ToList();
            }
            else
            {
                //   var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking().ToList();



                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6)).Select(e => new RegisteredFacultyEducation
                {
                    educationId = e.id,
                    educationName = e.educationCategoryName,
                    studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                    specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                    passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                    percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                    division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                    university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                    place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                    facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }

            }
            var jntuh_registered_faculty_experience = db.jntuh_registered_faculty_experience.AsNoTracking().ToList();



            int FExID = jntuh_registered_faculty_experience.Where(E => E.facultyId == fID).Select(E => E.Id).FirstOrDefault();
            regFaculty.RFExperience = jntuh_registered_faculty_experience.Where(E => E.Id == FExID).Select(E => new RegisteredfacultyExperience
            {
                CollegeName = jntuh_college.Where(C => C.id == E.collegeId).Select(C => C.collegeName).FirstOrDefault(),
                facultyDesignation = jntuh_designation.Where(D => D.id == E.facultyDesignationId).Select(D => D.designation).FirstOrDefault(),
                CollegeId = E.collegeId,
                DesignationId = E.facultyDesignationId,
                facultyDateOfAppointment = E.facultyDateOfAppointment,
                facultyDateOfResignation = E.facultyDateOfResignation,
                RelievingLetter = E.facultyRelievingLetter,
                JoiningOrder = E.facultyJoiningOrder,
                Salary = E.facultySalary

            }).ToList();
            regFaculty.GenderId = null;
            regFaculty.isFacultyRatifiedByJNTU = null;
            ViewBag.ExperienceStatus = regFaculty.RFExperience.Count;
            //  var jntuh_edit_registred_faculty = db.jntuh_edit_registred_faculty.AsNoTracking().ToList();
            if (fID > 0)
            {

                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.NewPassword = "TEMP@PWD";
                regFaculty.ConfirmPassword = "TEMP@PWD";
                int facultyUserId = jntuh_registered_faculty.Where(e => e.id == regFaculty.id).Select(e => e.UserId).FirstOrDefault();
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.CollegeId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == regFaculty.RegistrationNumber).Select(e => e.collegeId).FirstOrDefault();
                if (regFaculty.CollegeId==0)
                {
                    regFaculty.CollegeId = db.jntuh_college_principal_registered.Where(c => c.RegistrationNumber == regFaculty.RegistrationNumber).Select(e => e.collegeId).FirstOrDefault();
                }
                //int ? FStatus= db.jntuh_registered_faculty_log.Where(F => F.RegistrationNumber == faculty.RegistrationNumber).Select(F => F.id).FirstOrDefault();
                //ViewBag.RegistrationNum = FStatus;
                // int? FStatus =jntuh_edit_registred_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).Select(i => i.Id).FirstOrDefault();
                // int? FStatus =
                //  jntuh_registered_faculty_log.Where(
                //      e => e.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim())
                //      .Select(e => e.id)
                //     .FirstOrDefault();
                //   ViewBag.RegistrationNum = FStatus;
                // ViewBag.EditStatus =jntuh_edit_registred_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).Select(i => i.FacultyStatus).FirstOrDefault();
                var PANStatus = faculty.PanStatus;
                ViewBag.PanStatus = PANStatus;
                //  ViewBag.VerificationStatus = jntuh_registered_faculty_log.Where(F => F.RegistrationNumber == faculty.RegistrationNumber && F.FacultyApprovedStatus == 2).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                //ViewBag.RegistrationNum = "5586-150420-133949";   
                ViewBag.EXFacultyId = db.jntuh_registered_faculty_experience_log.Where(F => F.facultyId == faculty.id).Select(F => F.facultyId).FirstOrDefault();
                if (faculty.DateOfBirth != null)
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = jntuh_designation.Where(e => e.id == faculty.DesignationId).Select(e => e.designation).FirstOrDefault();
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = jntuh_department.Where(e => e.id == faculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                }
                if (regFaculty.CollegeId != null)
                {
                    regFaculty.CollegeName = jntuh_college.Where(e => e.id == regFaculty.CollegeId).Select(e => e.collegeName).FirstOrDefault();
                }

                //regFaculty.CollegeId = faculty.collegeId;

                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;
                if (faculty.DateOfAppointment != null)
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;

                regFaculty.isView = true;
                regFaculty.BlacklistFaculty = faculty.Blacklistfaculy;
                regFaculty.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus ?? false;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;
                var collegeData = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (collegeData != null)
                {
                    ViewBag.CollegeName = jntuh_college.Where(e => e.id == collegeData.collegeId).Select(e => e.collegeName).FirstOrDefault();
                    ViewBag.CollegeCode = jntuh_college.Where(e => e.id == collegeData.collegeId).Select(e => e.collegeCode).FirstOrDefault();
                    if (collegeData.DepartmentId != null)
                    {
                        ViewBag.DepartmentName = jntuh_department.Where(e => e.id == collegeData.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.DepartmentName = null;
                    }
                }
                else
                {
                    ViewBag.CollegeName = null;
                }
                //int[] phddepIds = {31,29,30,32};
                //ViewBag.IsPhdfaculty = 0;
                ////regFaculty.RegistrationNumber = "89150330-163653";
                //Faculty PHD Edit Conditions
                int phdcount = regFaculty.FacultyEducation.Where(e => e.educationId == 6 && !string.IsNullOrEmpty(e.facultyCertificate)).Select(e => e).Count();
                if (phdcount == 0)
                {
                    //ViewBag.IsPhdfaculty = true;
                }
                //if (ViewBag.IsPhdfaculty1!=0 &&regFaculty.BlacklistFaculty!=true)
                //{
                //    string[] phddocumentreginumbers = { "5076-170912-153025", "86150330-124349", "45150330-135118", "24150330-154256", "89150330-163653", "32150330-184849", "01150330-195657", "09150330-233548", "65150330-234021", "32150331-101020", "54150331-143516", "85150331-143942", "27150331-153714", "03150331-231935", "23150401-101417", "92150401-140923", "20150401-191342", "95150402-105257", "29150402-115939", "14150402-121235", "02150402-134808", "67150402-155800", "04150402-162346", "68150403-015716", "19150403-074232", "16150403-102930", "33150403-121738", "29150403-194155", "94150403-212613", "15150403-231653", "14150404-072853", "19150404-105700", "00150404-122843", "31150404-140528", "72150404-150052", "98150404-151104", "61150404-163123", "55150404-164801", "32150404-172740", "47150404-190607", "66150404-195728", "63150405-105142", "50150405-134030", "58150405-142724", "82150406-112355", "28150406-114527", "55150406-124635", "46150406-125500", "17150406-145030", "98150406-145704", "90150406-150306", "57150406-150411", "32150406-151851", "68150406-152433", "61150406-154123", "51150406-154756", "84150406-155716", "69150406-174533", "95150406-190716", "90150407-101404", "12150407-102002", "00150407-105515", "42150407-110147", "74150407-110315", "38150407-114922", "96150407-121047", "16150407-122941", "30150407-124348", "94150407-130011", "06150407-131623", "68150407-140826", "06150407-142347", "01150407-151610", "73150407-153702", "42150407-154522", "15150407-155302", "91150407-164824", "8601-150407-215242", "2809-150408-115117", "9629-150408-125905", "3539-150408-131514", "0913-150408-140109", "7137-150408-140831", "6082-150408-155301", "0714-150408-163208", "1941-150408-195342", "7211-150409-131146", "0678-150409-134601", "5540-150409-151342", "9293-150409-152758", "4336-150409-162429", "8835-150409-215651", "4681-150410-101558", "8096-150410-101848", "0209-150410-101942", "9079-150410-103817", "3133-150410-105936", "4822-150410-114424", "7534-150410-120811", "9946-150410-132838", "5294-150410-135203", "9708-150410-144632", "5966-150410-145859", "5329-150410-150448", "3146-150410-162710", "4101-150411-105236", "9477-150411-120806", "1643-150411-123804", "1426-150411-125711", "2455-150411-144816", "2641-150412-134821", "1277-150413-124141", "2876-150413-192246", "6441-150414-142344", "7190-150416-102348", "2954-150416-103048", "9805-150416-115518", "8818-150416-131333", "5498-150417-133552", "1193-150417-233334", "1814-150418-121540", "7073-150418-124548", "2493-150418-142538", "3936-150418-164408", "0899-150419-090852", "2759-150419-112924", "9585-150420-105758", "1348-150420-110623", "0936-150420-114612", "5218-150420-155112", "3292-150420-162826", "4107-150425-151419", "2776-150426-174034", "2238-150427-144228", "7094-150427-205136", "9127-150429-155346", "5088-150502-135231", "6022-150505-152515", "7794-150506-111930", "3011-150506-143432", "1004-150507-114654", "9914-150522-154121", "6856-150605-145326", "9053-151223-174358", "0816-151231-123436", "8795-160203-175048", "8199-160218-202931", "3439-160220-111619", "0948-160223-154330", "5474-160225-110911", "6575-160227-133435", "1334-160302-111355", "5132-160302-122835", "9501-160303-113550", "7391-160304-125258", "0893-160304-132824", "3068-160305-201328", "9590-160306-003250", "0772-160309-102912", "7542-160310-161113", "7356-160316-133849", "6683-160517-122817", "6625-161024-141332", "2886-161117-110449", "1738-161128-150415", "6680-161207-132833", "4215-161223-144848", "4374-170104-125349", "3826-170107-145702", "7111-170111-113028", "6076-170112-142605", "9232-170119-104104", "2919-170120-152602", "6670-170122-151816", "0311-170122-203935", "1623-170128-131044", "9400-170129-112153", "9213-170131-224323", "0518-170201-100649", "4235-170201-133626", "7401-170201-135706", "4388-170205-120331", "4265-170522-122220", "3893-170522-131419", "7555-170601-152305", "0527-170623-161203", "1747-170623-161422", "8004-170911-225045", "6247-170912-140539", "3024-170914-132719", "3993-170914-140249", "1493-170915-144826", "2023-170915-153502", "6523-170918-163733", "7454-171009-094302", "1717-171026-163553" };
                //    //string[] newphdregistrations =
                //    //db.jntuh_registered_faculty_log.Select(s => s.RegistrationNumber).ToArray();
                //    if (phddocumentreginumbers.Contains(regFaculty.RegistrationNumber))
                //    {
                //        ViewBag.IsPhdfaculty = 1;
                //    }
                //}                                                         
                ////&&f.RegistrationNumber == regFaculty.RegistrationNumber                      
                //ViewBag.IsPhdfaculty = regFaculty.FacultyEducation.Where(e => e.educationId == 6 && !string.IsNullOrEmpty(e.facultyCertificate)).Select(e => e).Count();
                //var IsPhdfaculty=from f in db.jntuh_registered_faculty
                //                 join e in db.jntuh_college_faculty_registered on f.RegistrationNumber equals e.RegistrationNumber
                //                 where (f.id=)
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.id = regFaculty.id;

            }
            else
            {
                if (vFaculty.isView != null)
                {
                    regFaculty = vFaculty;

                    if (vFaculty.CollegeId != null)
                    {
                        regFaculty.CollegeName = jntuh_college.Where(e => e.id == regFaculty.CollegeId).Select(e => e.collegeName).FirstOrDefault();
                    }

                    if (vFaculty.DesignationId != null)
                    {

                        regFaculty.designation = jntuh_designation.Where(e => e.id == vFaculty.DesignationId).Select(e => e.designation).FirstOrDefault();
                    }

                    if (vFaculty.DepartmentId != null)
                    {
                        //
                        regFaculty.department = jntuh_department.Where(e => e.id == vFaculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                        //db.jntuh_department.Find(vFaculty.DepartmentId).departmentName;
                    }

                    regFaculty.OtherDepartment = vFaculty.OtherDepartment;
                    regFaculty.OtherDesignation = vFaculty.OtherDesignation;
                    regFaculty.isFacultyRatifiedByJNTU = vFaculty.isFacultyRatifiedByJNTU;


                }
            }

            return View(regFaculty);
        }
        
        /// <summary>
        /// Faculty Edit Option View Getting (Editing Faculty Details)
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditFacultyDetails(string fid)
        {
            return RedirectToAction("Logon", "Account");
            ViewBag.fid = string.IsNullOrEmpty(fid) ? "0" : fid;
            var currentDate = DateTime.Now;            
            int fID = 0;
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int facultyId =db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID =Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            else
            {
                return RedirectToAction("Logon", "Account");
                if (fid != null)
                {
                    fID =Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (fid != null)
            {
                TempData["fid"] = fid;
            }

            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;
            DateTime facultyeditclosedate = new DateTime();
            if (currentDate > new DateTime(2018, 02, 15, 23, 59, 59))
            {
                return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
            }
           //fID = 87016;
            DateTime todayDate = DateTime.Now.Date;

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            int[] notRequiredIds = {25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56};
            foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment {id = item.id, departmentName = item.departmentName});
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            var PGDeptIds =db.jntuh_degree.Join(db.jntuh_department, d => d.id, de => de.degreeId, (d, de) => new {d = d, de = de}).Where(e=>e.d.id!=4 && e.d.id!=5).Select(e => e.de.id).ToArray();

            int[] OthersSpecialization = { 155, 156, 157, 158, 159, 160, 161, 162,163, 164, 165, 166 };
            var pgSpecializations = db.jntuh_specialization.Where(e => e.isActive == true && !OthersSpecialization.Contains(e.id) && PGDeptIds.Contains(e.departmentId) || e.id == 154).Select(e => new { id = e.id, spec = e.specializationName }).OrderBy(e => e.spec).ToList();
          ViewBag.PGSpecializations = pgSpecializations;

           string inactivereg = db.jntuh_registered_faculty.Where(i => i.id == fID && i.isActive != false).Select(s => s.RegistrationNumber).FirstOrDefault();
            if (String.IsNullOrEmpty(inactivereg))
            {
                TempData["EDITERROR"] = "Edit option not provided as this registration number was surrendered under multiple registrations";
                return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
            }

            FacultyRegistration regFaculty = new FacultyRegistration();
            string fregnoa = db.jntuh_registered_faculty.Where(i => i.id == fID && (i.type == "Adjunct" || i.Blacklistfaculy == true)).Select(s => s.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(fregnoa))
            {
                TempData["EDITERROR"] = "You don't have permissions to Edit";
            }
            else
            {
                string fregno = db.jntuh_registered_faculty.Where(i => i.id == fID && i.isActive != false).Select(s => s.RegistrationNumber).FirstOrDefault();
                if (!string.IsNullOrEmpty(fregno))
                {
                    string editregno =
                db.jntuh_college_faculty_registered_copy.Where(e => e.RegistrationNumber == fregno && e.collegeId == 375)
                    .Select(s => s.RegistrationNumber)
                    .FirstOrDefault();
                    if (!string.IsNullOrEmpty(editregno))
                    {
                        TempData["EDITERROR"] = "The details of your profile with the submitted documents are in order and hence the edit option is not available to you.";
                        return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                    }
                }
            }
           
            var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(f => f.id == fID).ToList();

            var jntuh_registered_faculty_log = db.jntuh_registered_faculty_log.AsNoTracking().Where(e=>e.isActive!=false).Select(e=>e).ToList();
            var regno =jntuh_registered_faculty.Where(e => e.id == fID).Select(e => e.RegistrationNumber).FirstOrDefault();
            //int FacultyLogId = jntuh_registered_faculty_log.Where(e => e.RegFacultyId == fID && e.RegistrationNumber == regno).Select(e => e.id).FirstOrDefault();
            int FacultyLogId = 0;

            ViewBag.FirsttimeEditoption = FacultyLogId;
            if (FacultyLogId != 0)
            {
                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6)).Select(e => new RegisteredFacultyEducation
                {
                    educationId = e.id,
                    educationName = e.educationCategoryName,
                    studiedEducation = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.courseStudied).FirstOrDefault(),
                    specialization = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.specialization).FirstOrDefault(),
                    passedYear = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.passedYear).FirstOrDefault(),
                    percentage = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.marksPercentage).FirstOrDefault(),
                    division = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.division).FirstOrDefault(),
                    university = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                    place = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                    facultyCertificate = db.jntuh_registered_faculty_education_log.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.certificate).FirstOrDefault(),
                }).ToList();
                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }
            }
           else
           {
               regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6)).Select(e => new RegisteredFacultyEducation
               {
                   educationId = e.id,
                   educationName = e.educationCategoryName,
                   studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                   specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                   passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                   percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                   division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                   university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                   place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                   facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
               }).ToList();
               foreach (var item in regFaculty.FacultyEducation)
               {
                   if (item.division == null)
                       item.division = 0;
               }
           }
            if (FacultyLogId != 0)
            {
                // db.jntuh_edit_registred_faculty
                var faculty = db.jntuh_registered_faculty_log.Find(FacultyLogId);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.NewPassword = "TEMP@PWD";
                regFaculty.ConfirmPassword = "TEMP@PWD";
                int facultyUserId = db.jntuh_registered_faculty.Find(regFaculty.id).UserId;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.CollegeId =
                    db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == regFaculty.RegistrationNumber)
                        .Select(s => s.collegeId)
                        .FirstOrDefault();
                if (faculty.DateOfBirth != null)
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                if (regFaculty.CollegeId != 0)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                }
                if (faculty.PGSpecialization != null)
                {
                    regFaculty.PGSpecializationName = db.jntuh_specialization.Find(faculty.PGSpecialization).specializationName;
                    regFaculty.PGSpecialization = faculty.PGSpecialization;
                }
                regFaculty.OthersPGSpecilizationName = faculty.Remarks;
                regFaculty.CollegeId = faculty.collegeId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;
                if (faculty.DateOfAppointment != null)
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                //regFaculty.EditPANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.EditAadhaarNumber = faculty.AadhaarNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.EditMobile = faculty.Mobile;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.WorkingType = jntuh_registered_faculty.Where(e => e.id == fID).Select(e => e.WorkingType).FirstOrDefault(); ;
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                regFaculty.isView = true;
                regFaculty.PanStatus = jntuh_registered_faculty.Where(e => e.id == fID).Select(e => e.PanStatus).FirstOrDefault(); 
            }
            else
            {
                
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.NewPassword = "TEMP@PWD";
                regFaculty.ConfirmPassword = "TEMP@PWD";
                int facultyUserId = db.jntuh_registered_faculty.Find(regFaculty.id).UserId;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.CollegeId =
                   db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == regFaculty.RegistrationNumber)
                       .Select(s => s.collegeId)
                       .FirstOrDefault();
                if (faculty.DateOfBirth != null)
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                if (regFaculty.CollegeId != 0)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                }

               // regFaculty.CollegeId = faculty.collegeId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;
                if (faculty.DateOfAppointment != null)
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                //regFaculty.EditPANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.EditAadhaarNumber = faculty.AadhaarNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.EditMobile = faculty.Mobile;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.WorkingType = faculty.WorkingType;
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                regFaculty.isView = true;
                regFaculty.PanStatus = faculty.PanStatus;
            }
            return View(regFaculty);
        }

        /// <summary>
        /// Faculty Edit Option Posing (Editing Faculty Details)
        /// </summary>
        /// <param name="fid"></param>
        /// <returns>Save in database</returns>
        [HttpPost]
        public ActionResult EditFacultyDetails(FacultyRegistration faculty, string fid, string Command, HttpPostedFileBase files, FormCollection form)
        {
            return RedirectToAction("Logon", "Account");
            TempData["SUCCESS"] = null;
            TempData["ERROR"] = null;
            int fID = 0;
            if (fid == null)
            {
                if (TempData["fid"] != null)
                    fid = TempData["fid"] as string;
            }

            if (Command == "Go Back" || (faculty.Type == null && faculty.FirstName == null))
            {
                return RedirectToAction("FacultyNew");
            }

            if (fid != null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var currentDate = DateTime.Now;
            DateTime facultyeditclosedate = new DateTime();
            if (currentDate > new DateTime(2018, 02, 15,23,59,59))
            {
                return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
            }            
            string fregno =
                    db.jntuh_registered_faculty.Where(i => i.id == fID && i.type != "Adjunct" &&i.isActive!=false)
                        .Select(s => s.RegistrationNumber)
                        .FirstOrDefault();
            string editregno =
              db.jntuh_college_faculty_registered_copy.Where(e => e.RegistrationNumber == fregno && e.collegeId == 375)
                  .Select(s => s.RegistrationNumber)
                  .FirstOrDefault();
            if (!string.IsNullOrEmpty(editregno))
            {
                TempData["EDITERROR"] = "You don't have permissions to Edit";
                return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
            }
            var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(a => a.id == fID).ToList();
            var jntuh_registered_facultydata = jntuh_registered_faculty.Where(a => a.id == fID).FirstOrDefault();


            #region Dropdown Lists
            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;

            int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
            var jntuh_departments = db.jntuh_department.Select(D => D).ToList();

            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            foreach (var item in jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.division = division;


            var PGDeptIds = db.jntuh_degree.Join(db.jntuh_department, d => d.id, de => de.degreeId, (d, de) => new { d = d, de = de }).Where(e => e.d.id != 4 && e.d.id != 5).Select(e => e.de.id).ToArray();

            int[] OthersSpecialization = { 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 165, 166 };
            var pgSpecializations = db.jntuh_specialization.Where(e => e.isActive == true && !OthersSpecialization.Contains(e.id) && PGDeptIds.Contains(e.departmentId) || e.id == 154).Select(e => new { id = e.id, spec = e.specializationName }).OrderBy(e => e.spec).ToList();
            ViewBag.PGSpecializations = pgSpecializations;
            #endregion


            if (fid != null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.FacultyID = fID;
            faculty.id = fID;
            string photoPath = "~/Content/Upload/Faculty/PHOTOS";
            string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";
            string proceedingsPath = "~/Content/Upload/Faculty/PROCEEDINGS";
            string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";
            string IncomeTaxcertificatesPath = "~/Content/Upload/Faculty/INCOMETAX";
            string PhdUndertakingcertificatesPath = "~/Content/Upload/Faculty/PHDUndertaking";
            if (Command == "Update")//&& !string.IsNullOrEmpty(faculty.FirstName) &&!string.IsNullOrEmpty(faculty.LastName)
            {
                bool isFacultyValid = true;
                string errorMessage = "";

                #region VALIDATE THE FORM FOR ALL MANDATORY FIELDS Commented

                //VERIFY FOR THE FACULTY PHOTO DIMENSIONS
                //if (faculty.facultyPhoto == null)
                //{
                //    if (faculty.Photo != null && faculty.Photo.ContentLength > 0)
                //    {
                //        //System.IO.Stream fileStream = faculty.Photo.InputStream;
                //        //fileStream.Position = 0;
                //        //byte[] fileContents = new byte[faculty.Photo.ContentLength];
                //        //fileStream.Read(fileContents, 0, faculty.Photo.ContentLength);
                //        //System.Drawing.Image image =
                //        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                //        //if (image.Width > 200 || image.Height > 230)
                //        //{
                //        //    isFacultyValid = false;
                //        //    errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                //        //}

                //        //if (image.Width < 150 || image.Height < 150)
                //        //{
                //        //    isFacultyValid = false;
                //        //    errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                //        //}
                //    }
                //    else
                //    {
                //        isFacultyValid = false;
                //        errorMessage += ". " + "Photo is mandatory" + "<br>";
                //    }
                //}

                //if (faculty.facultyPANCardDocument == null)
                //{
                //    if (faculty.PANCardDocument != null)
                //    {
                //        //System.IO.Stream fileStream = faculty.PANCardDocument.InputStream;
                //        //fileStream.Position = 0;
                //        //byte[] fileContents = new byte[faculty.PANCardDocument.ContentLength];
                //        //fileStream.Read(fileContents, 0, faculty.PANCardDocument.ContentLength);
                //        //System.Drawing.Image image =
                //        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                //        //if (image.Width < 300 || image.Height < 250)
                //        //{
                //        //    isFacultyValid = false;
                //        //    errorMessage += ". " + "PANCARD document should be minimum 300x250 pixels" + "<br>";
                //        //}
                //    }
                //    else
                //    {
                //        if (faculty.Type != null && faculty.Type.ToUpper() == "EXISTFACULTY")
                //        {
                //            isFacultyValid = false;
                //            errorMessage += ". " + "PANCARD is mandatory" + "<br>";
                //        }
                //    }
                //}

                //if (faculty.facultyAadhaarCardDocument == null)
                //{
                //    if (faculty.AadhaarCardDocument != null)
                //    {
                //        //System.IO.Stream fileStream = faculty.AadhaarCardDocument.InputStream;
                //        //fileStream.Position = 0;
                //        //byte[] fileContents = new byte[faculty.AadhaarCardDocument.ContentLength];
                //        //fileStream.Read(fileContents, 0, faculty.AadhaarCardDocument.ContentLength);
                //        //System.Drawing.Image image =
                //        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                //        //if (image.Width < 200 || image.Height < 200)
                //        //{
                //        //    isFacultyValid = false;
                //        //    errorMessage += ". " + "Aadhaar Card document should be minimum 200x200 pixels" + "<br>";
                //        //}
                //    }
                //    else
                //    {
                //        isFacultyValid = false;
                //        errorMessage += ". " + "Aadhaar Card document is mandatory" + "<br>";
                //    }
                //}

                //if (faculty.SelectionCommitteeProcedings == null)
                //{

                //    //if (faculty.WorkingStatus == true)
                //    //{

                //    if (faculty.SelectionCommitteeProceedingsDocument != null)
                //    {
                //        //System.IO.Stream fileStream = faculty.SelectionCommitteeProceedingsDocument.InputStream;
                //        //fileStream.Position = 0;
                //        //byte[] fileContents = new byte[faculty.SelectionCommitteeProceedingsDocument.ContentLength];
                //        //fileStream.Read(fileContents, 0, faculty.SelectionCommitteeProceedingsDocument.ContentLength);
                //        //System.Drawing.Image image =
                //        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                //        //if (image.Width < 600 || image.Height < 800)
                //        //{
                //        //    isFacultyValid = false;
                //        //    errorMessage += ". " +
                //        //                    "Selection Committee Proceedings document should be minimum 600x800 pixels" +
                //        //                    "<br>";
                //        //}
                //    }
                //    else
                //    {
                //        isFacultyValid = false;
                //        errorMessage += ". " + "Selection Committee Proceedings document is mandatory" + "<br>";
                //    }
                //}
                //// }


                // IncomTax Document Validation checking 
                //if (faculty.IncomeTaxFileview == null)
                //{
                //    if (faculty.IncomeTaxFileUpload != null)
                //    {


                //        if (faculty.IncomeTaxFileUpload.ContentLength > 1024*1024*1)
                //        {
                //            isFacultyValid = false;
                //            errorMessage += ". " +
                //                            "Income Tax 26 As document should be maximam 1 MB Size" +
                //                            "<br>";
                //        }
                //    }
                //    else
                //    {
                //        isFacultyValid = false;
                //        errorMessage += ". " + "Income Tax 26 As document is mandatory" + "<br>";
                //    }
                //}


                //if (string.IsNullOrEmpty(faculty.EditPANNumber))
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "PAN Number is mandatory" + "<br>";
                //}

                //if (string.IsNullOrEmpty(faculty.EditAadhaarNumber))
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "Aadhaar Number is mandatory" + "<br>";
                //}
                //if (string.IsNullOrEmpty(faculty.EditMobile))
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "Mobile Number is mandatory" + "<br>";
                //}


                //if (faculty.CollegeId == 0 || faculty.CollegeId == null)
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + " Name of the Institution presently working in is mandatory" + "<br>";
                //}

                //if (faculty.DepartmentId == 0 || faculty.DepartmentId == null)
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + " Department  is mandatory" + "<br>";
                //}

                //if (faculty.TotalExperience == null)
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + " Total Experience (years)   is mandatory" + "<br>";
                //}
                //if (faculty.AadhaarCardDocument == null && faculty.facultyAadhaarCardDocument == null)
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "Aadhaar Card document is mandatory" + "<br>";
                //}
                //if (faculty.PGSpecialization == 0 || faculty.PGSpecialization == null)
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "PG Specialization is mandatory" + "<br>";
                //}
                #endregion

                if (string.IsNullOrEmpty(faculty.EditAadhaarNumber))
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Aadhaar Number is mandatory" + "<br>";
                }
                if (string.IsNullOrEmpty(faculty.EditMobile))
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Mobile Number is mandatory" + "<br>";
                }
                if (string.IsNullOrEmpty(faculty.FirstName))
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "First Name is mandatory" + "<br>";
                }
                if (string.IsNullOrEmpty(faculty.LastName))
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Last Name is mandatory" + "<br>";
                }

                if (string.IsNullOrEmpty(faculty.PANNumber))
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "PAN Number is mandatory" + "<br>";
                }
                var CheckingRegistrationNumber = db.jntuh_registered_faculty.Where(e => e.AadhaarNumber.Trim() == faculty.EditAadhaarNumber && e.isActive != false).Select(e => e.RegistrationNumber).FirstOrDefault();
                int Aadhaarcount = db.jntuh_registered_faculty.Where(F => F.AadhaarNumber.Trim() == faculty.EditAadhaarNumber && F.isActive != false).Select(e => e.id).Count();
                if (!string.IsNullOrEmpty(CheckingRegistrationNumber))
                {
                    if (CheckingRegistrationNumber.Trim() == faculty.RegistrationNumber)
                    {
                        if (Aadhaarcount > 1)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + "Aadhaar Number Already Exists" + "<br>";
                        }
                    }
                }
                var CheckingRegistrationNumberforMobile = db.jntuh_registered_faculty.Where(e => e.Mobile.Trim() == faculty.EditMobile).Select(e => e.RegistrationNumber).FirstOrDefault();
                int Mobilecount = db.jntuh_registered_faculty.Where(F => F.Mobile.Trim() == faculty.EditMobile).Select(e => e.id).Count();
                if (!string.IsNullOrEmpty(CheckingRegistrationNumberforMobile))
                {
                    if (CheckingRegistrationNumberforMobile.Trim() == faculty.RegistrationNumber)
                    {
                        if (Mobilecount > 1)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + "Mobile Number  Already Exists" + "<br>";
                        }
                    }
                }
                if (faculty.Type == null)
                {
                    if (faculty.WorkingStatus == null || faculty.WorkingStatus == false)
                    {
                        faculty.Type = "NewFaculty";
                        faculty.WorkingStatus = false;
                    }
                    else if (faculty.WorkingStatus == true)
                    {
                        faculty.Type = "ExistFaculty";
                    }
                }
                int vIndex = 0;
                foreach (var item in faculty.FacultyEducation)
                {
                    try
                    {
                        if (item.certificate != null)
                        {
                          
                        }
                        else
                        {
                            if (vIndex < 3 && item.facultyCertificate == null)
                            {
                                isFacultyValid = false;
                                errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                            }
                            else if (item.studiedEducation != null && item.facultyCertificate == null)
                            {
                                isFacultyValid = false;
                                errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        
                        throw;
                    }

                    vIndex++;
                }

                if (!isFacultyValid)
                {
                    TempData["ERROR"] = errorMessage;

                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    TempData["FACULTY"] = faculty;
                    return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                    
                }

                //If Faculty Data have in log we use log file names
                if (jntuh_registered_facultydata!=null)
                {
                    //Photo Save
                    if (faculty.Photo != null)
                    {
                        if (!Directory.Exists(Server.MapPath(photoPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(photoPath));
                        }

                        var ext = Path.GetExtension(faculty.Photo.FileName);

                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.Photo.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                            faculty.facultyPhoto = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else if (faculty.facultyPhoto != null)
                    {
                        faculty.facultyPhoto = faculty.facultyPhoto;
                    }
                    //PAN Document Save
                    if (faculty.PANCardDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(panCardsPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(panCardsPath));
                        }

                        var ext = Path.GetExtension(faculty.PANCardDocument.FileName);

                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.PANCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath),
                                fileName, ext));
                            faculty.facultyPANCardDocument = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else if (faculty.facultyPANCardDocument != null)
                    {
                        faculty.facultyPANCardDocument = faculty.facultyPANCardDocument;
                    }
                    //Aadhaar Card Save
                    if (faculty.AadhaarCardDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                        }

                        var ext = Path.GetExtension(faculty.AadhaarCardDocument.FileName);

                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.AadhaarCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                fileName, ext));
                            faculty.facultyAadhaarCardDocument = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else if (faculty.facultyAadhaarCardDocument != null)
                    {
                        faculty.facultyAadhaarCardDocument = faculty.facultyAadhaarCardDocument;
                    }
                    //SelectionCommittee Proceeding Document Save
                    if (faculty.SelectionCommitteeProceedingsDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(proceedingsPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(proceedingsPath));
                        }

                        var ext = Path.GetExtension(faculty.SelectionCommitteeProceedingsDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.SelectionCommitteeProceedingsDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(proceedingsPath), fileName, ext));
                            faculty.SelectionCommitteeProcedings = string.Format("{0}{1}", fileName, ext);

                        }
                    }
                    else if (faculty.SelectionCommitteeProcedings != null)
                    {
                        faculty.SelectionCommitteeProcedings = faculty.SelectionCommitteeProcedings;
                    }

                    //Income Tax Certificates Saving code/////////////////////
                    if (faculty.IncomeTaxFileUpload != null)
                    {
                        if (!Directory.Exists(Server.MapPath(IncomeTaxcertificatesPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(IncomeTaxcertificatesPath));
                        }

                        var ext = Path.GetExtension(faculty.IncomeTaxFileUpload.FileName);

                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.IncomeTaxFileUpload.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(IncomeTaxcertificatesPath), fileName, ext));
                            faculty.IncomeTaxFileview = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else if (faculty.IncomeTaxFileview != null)
                    {
                        faculty.IncomeTaxFileview = faculty.IncomeTaxFileview;
                    }

                    // Phd Undertaking Document Save
                    if (faculty.PHDUndertakingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(PhdUndertakingcertificatesPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(PhdUndertakingcertificatesPath));
                        }

                        var ext = Path.GetExtension(faculty.PHDUndertakingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.PHDUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PhdUndertakingcertificatesPath), fileName, ext));
                            faculty.PHDUndertakingDocumentView = string.Format("{0}{1}", fileName, ext);

                        }
                    }
                    else if (faculty.PHDUndertakingDocumentView != null)
                    {
                        faculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocumentView;
                    }

                    //Certificates Saveing Code
                    int eIndex = 0;
                    foreach (var item in faculty.FacultyEducation)
                    {
                        //if ((eIndex == 0 && item.studiedEducation == null) || (eIndex != 0 && item.studiedEducation != null))
                        //{
                        if (item.certificate != null)
                        {
                            if (!Directory.Exists(Server.MapPath(certificatesPath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(certificatesPath));
                            }

                            var ext = Path.GetExtension(item.certificate.FileName);

                            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                            {
                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1) +
                                            "_" + item.studiedEducation;
                                item.certificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                fileName, ext));
                                item.facultyCertificate = string.Format("{0}{1}", fileName, ext);
                            }
                        }
                        else if (item.facultyCertificate != null)
                        {
                            item.facultyCertificate = item.facultyCertificate;
                        }
                        //}

                        eIndex++;
                    }
                }             

            }
            //else
            //{
            //    return RedirectToAction("FacultyNew", new { @fid = fid });
            //}
            if (faculty.id > 0)
            {
                fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
            }

            bool IsValidPhoto = true;

            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfBirth).ToShortDateString();
            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfAppointment).ToShortDateString();
            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfRatification).ToShortDateString();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(v => v.Value.Errors.Any());
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                //return new HttpStatusCodeResult(1, message);
            }

            int UserId = 0;

            string userName = string.Empty;
            string password = string.Empty;
            string email = string.Empty;
            UserId = jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UserId).FirstOrDefault();
         
            if (UserId > 0 && IsValidPhoto)
            {
                jntuh_registered_faculty regFaculty = db.jntuh_registered_faculty.Where(a => a.id == fID).FirstOrDefault();
                regFaculty.UserId = UserId;

                if (faculty.isApproved == true && string.IsNullOrEmpty(faculty.UniqueID))
                {
                    regFaculty.UniqueID = string.Empty;
                    regFaculty.RegistrationNumber = jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.RegistrationNumber).FirstOrDefault();

                }
                else if (faculty.isApproved == true && !string.IsNullOrEmpty(faculty.UniqueID))
                {
                    regFaculty.UniqueID = jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UniqueID).FirstOrDefault();
                    regFaculty.RegistrationNumber = jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.RegistrationNumber).FirstOrDefault();
                }
                else
                {
                    regFaculty.UniqueID = string.Empty;
                    string regNumber = jntuh_registered_faculty.Where(f => f.UserId == regFaculty.UserId).Select(f => f.RegistrationNumber).FirstOrDefault();

                    if (string.IsNullOrEmpty(regNumber))
                    {
                        Random rnd = new Random();
                        regFaculty.RegistrationNumber = rnd.Next(0, 9999).ToString("D4") + "-" +
                                                        DateTime.Now.ToString("yyyyMMdd-HHmmss").Substring(2);
                    }
                    else
                    {
                        regFaculty.RegistrationNumber = regNumber;
                    }
                }

                regFaculty.type = faculty.Type;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId == null ? 0 : (int)faculty.GenderId;
                regFaculty.FatherOrHusbandName = faculty.FatherOrhusbandName;
                regFaculty.DateOfBirth = Convert.ToDateTime(faculty.facultyDateOfBirth);
                regFaculty.OrganizationName = faculty.OrganizationName == null ? string.Empty : faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                //regFaculty.collegeId = faculty.CollegeId;

                if (regFaculty.type == null)
                {
                    if (faculty.WorkingStatus == null || faculty.WorkingStatus == false)
                    {
                        regFaculty.type = "NewFaculty";
                        regFaculty.WorkingStatus = false;
                    }
                    else if (faculty.WorkingStatus == true)
                    {
                        regFaculty.type = "ExistFaculty";
                    }
                }
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.facultyDateOfAppointment != null)
                    regFaculty.DateOfAppointment = Convert.ToDateTime(faculty.facultyDateOfAppointment);
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU == null
                    ? false
                    : (bool)faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    regFaculty.DateOfRatification = Convert.ToDateTime(faculty.facultyDateOfRatification);
                regFaculty.ProceedingsNumber = faculty.ProceedingsNo;

                if (faculty.TotalExperience != null)
                {
                    regFaculty.TotalExperience = (int)faculty.TotalExperience;
                }
                else
                {
                    regFaculty.TotalExperience = 0;
                }



                // Add Pg Specialization
                if (faculty.PGSpecialization != null)
                {
                    if (faculty.PGSpecialization == 154)
                    {
                        regFaculty.PGSpecialization = faculty.PGSpecialization;
                        //regFaculty.Remarks = faculty.OthersPGSpecilizationName;
                    }
                    else
                    {
                        regFaculty.PGSpecialization = faculty.PGSpecialization;
                    }
                }

                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.EditMobile;
                regFaculty.PANNumber = faculty.PANNumber;
                if (regFaculty.PanStatus!="E")
                {
                    regFaculty.PanStatus = db.jntuh_pan_status.Where(p => p.PANNumber == faculty.PANNumber &&p.PANStatus=="E").Select(s => s.PANStatus).FirstOrDefault();
                }
               
                regFaculty.AadhaarNumber = faculty.EditAadhaarNumber;

                regFaculty.MotherName = faculty.MotherName;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.grosssalary = faculty.GrossSalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.ProceedingsNumber = faculty.ProceedingsNo;
                regFaculty.OrganizationName = faculty.OrganizationName;

                regFaculty.isActive = true;
                regFaculty.isApproved = faculty.isApproved;

                regFaculty.ProceedingDocument = null;
                if (faculty.SelectionCommitteeProcedings != null)
                {
                    regFaculty.ProceedingDocument = faculty.SelectionCommitteeProcedings;
                }

                regFaculty.Photo = null;
                if (faculty.facultyPhoto != null)
                {
                    regFaculty.Photo = faculty.facultyPhoto;
                }

                regFaculty.PANDocument = null;
                if (faculty.facultyPANCardDocument != null)
                {                  
                    regFaculty.PANDocument = faculty.facultyPANCardDocument;
                }

                regFaculty.IncometaxDocument = null;
                if (faculty.IncomeTaxFileview != null)
                {
                    regFaculty.IncometaxDocument = faculty.IncomeTaxFileview;
                }

                regFaculty.PHDUndertakingDocument = null;
                if (faculty.PHDUndertakingDocumentView != null)
                {
                    regFaculty.PHDUndertakingDocument = faculty.PHDUndertakingDocumentView;
                }

                regFaculty.AadhaarDocument = null;
                if (faculty.facultyAadhaarCardDocument != null)
                {
                    regFaculty.AadhaarDocument = faculty.facultyAadhaarCardDocument;
                }
                int facultyId = db.jntuh_registered_faculty.AsNoTracking().Where(f => f.UserId == UserId && f.isActive != false).Select(f => f.id).FirstOrDefault();
                if (facultyId == 0)
                {
                    return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                }
                else
                {
                    regFaculty.id = facultyId;
                    regFaculty.createdBy = jntuh_registered_faculty.Where(f => f.id == facultyId).Select(f => f.createdBy).FirstOrDefault();
                    regFaculty.createdOn = jntuh_registered_faculty.Where(f => f.id == facultyId).Select(f => f.createdOn).FirstOrDefault();
                    regFaculty.updatedBy = UserId;
                    regFaculty.updatedOn = DateTime.Now;
                    //regFaculty.FacultyApprovedStatus = 3;
                    db.Entry(regFaculty).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (regFaculty.id > 0)
                {                  
                    foreach (var item in faculty.FacultyEducation)
                    {
                        if (item.studiedEducation != null)
                        {
                            jntuh_registered_faculty_education educationnew = new jntuh_registered_faculty_education();
                            jntuh_registered_faculty_education education = db.jntuh_registered_faculty_education.Where(e=>e.facultyId==regFaculty.id&&e.educationId==item.educationId).FirstOrDefault();
                            if (education == null)
                            {

                                educationnew.facultyId = regFaculty.id;
                                educationnew.educationId = item.educationId;
                                educationnew.courseStudied = item.studiedEducation;
                                educationnew.specialization = item.specialization;
                                educationnew.passedYear = item.passedYear == null ? 0 : (int)item.passedYear;
                                educationnew.marksPercentage = item.percentage == null ? 0 : (decimal)item.percentage;
                                educationnew.division = item.division == null ? 0 : (int)item.division;
                                educationnew.boardOrUniversity = item.university;
                                educationnew.placeOfEducation = item.place;
                                educationnew.certificate = null;
                                if (item.facultyCertificate != null)
                                {
                                    educationnew.certificate = item.facultyCertificate;
                                }
                            }
                            else
                            {
                                education.certificate = null;
                                if (item.facultyCertificate != null)
                                {
                                    education.certificate = item.facultyCertificate;
                                }
                            }
                            int eid = db.jntuh_registered_faculty_education.AsNoTracking().Where(e => e.educationId == item.educationId && e.facultyId == regFaculty.id).Select(e => e.id).FirstOrDefault();
                            if (eid == 0)
                            {
                                educationnew.createdBy = UserId;
                                educationnew.createdOn = DateTime.Now;
                                db.jntuh_registered_faculty_education.Add(educationnew);
                                db.SaveChanges();
                            }
                            else
                            {
                                education.id = eid;

                                education.createdBy = db.jntuh_registered_faculty_education.Where(e => e.id == eid).Select(e => e.createdBy).FirstOrDefault();
                                education.createdOn = db.jntuh_registered_faculty_education.Where(e => e.id == eid).Select(e => e.createdOn).FirstOrDefault();
                                education.updatedBy = UserId;
                                education.updatedOn = DateTime.Now;
                                db.Entry(education).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    if (fid == null)
                    {
                        ViewBag.Javascript = "Registration completed successfully. Confirmation email sent";
                        TempData["SUCCESS"] = "Registration completed successfully. Confirmation email sent";
                        TempData["FUserName"] = faculty.Email;
                        TempData["FPassword"] = faculty.NewPassword;

                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.FacultyOnlineRegistration(faculty.Email, "FacultyOnlineRegistration",
                            "JNTUH Faculty Registration Details", faculty.Email, faculty.NewPassword,
                            regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type,
                            regFaculty.PANNumber)
                            .SendAsync();                     
                    }
                    else
                    {
                        ViewBag.Javascript = "Faculty information updated successfully";
                        TempData["SUCCESS"] = "Faculty information updated successfully ";
                    }
                }

                if (fID > 0)
                {
                    fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);
                }
            }

            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);

            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);

            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

            return RedirectToAction("FacultyNew", new { fid = fid });
        }

        /// <summary>
        /// Faculty Regitration Posing (Faculty Details)
        /// </summary>
        /// <param name="fid"></param>
        /// <returns>Save in database and Get Registration Number.and send Mail to Faculty</returns>
        [HttpPost]
        public ActionResult FacultyNew(FacultyRegistration faculty, string fid, string Command)
        {
            return RedirectToAction("Logon", "Account");
            TempData["SUCCESS"] = null;
            TempData["ERROR"] = null;

            if (Command == "Go Back" || (faculty.Type == null && faculty.FirstName == null))
            {
                return RedirectToAction("FacultyNew");
            }

            List<DistinctDepartment> depts = new List<DistinctDepartment>();

            var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_departments = (from a in jntuh_department
                                     join b in jntuh_degree on a.degreeId equals b.id
                                     select new
                                     {
                                         id = a.id,
                                         departmentName = b.degree + "-" + a.departmentName
                                     }).ToList();



            string existingDepts = string.Empty;
            int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56, 71, 72, 73, 74, 75, 76, 77, 78, 60 };
            foreach (var item in jntuh_departments.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true && c.id != 375)
                    .Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName })
                    .OrderBy(c => c.CollegeName)
                    .ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }

            ViewBag.division = division;

            int fID = 0;
            if (fid == null && faculty.id > 0)
            {
                fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
            }



            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.FacultyID = fID;

            string photoPath = "~/Content/Upload/Faculty/PHOTOS";
            string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";
            string proceedingsPath = "~/Content/Upload/Faculty/PROCEEDINGS";
            string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";
            string IncomeTaxcertificatesPath = "~/Content/Upload/Faculty/INCOMETAX";
            if (Command == "Register")
            {
                bool isFacultyValid = true;
                string errorMessage = "";

                //VALIDATE THE FORM FOR ALL MANDATORY FIELDS

                //VERIFY FOR THE FACULTY PHOTO DIMENSIONS

                if (faculty.Photo != null && faculty.Photo.ContentLength > 0)
                {
                    //System.IO.Stream fileStream = faculty.Photo.InputStream;
                    //fileStream.Position = 0;
                    //byte[] fileContents = new byte[faculty.Photo.ContentLength];
                    //fileStream.Read(fileContents, 0, faculty.Photo.ContentLength);
                    //System.Drawing.Image image =
                    //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                    //if (image.Width > 200 || image.Height > 230)
                    //{
                    //    isFacultyValid = false;
                    //    errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                    //}

                    //if (image.Width < 150 || image.Height < 150)
                    //{
                    //    isFacultyValid = false;
                    //    errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                    //}
                }
                else
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Photo is mandatory" + "<br>";
                }


                if (faculty.PANCardDocument != null)
                {
                    //System.IO.Stream fileStream = faculty.PANCardDocument.InputStream;
                    //fileStream.Position = 0;
                    //byte[] fileContents = new byte[faculty.PANCardDocument.ContentLength];
                    //fileStream.Read(fileContents, 0, faculty.PANCardDocument.ContentLength);
                    //System.Drawing.Image image =
                    //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                    //if (image.Width < 300 || image.Height < 250)
                    //{
                    //    isFacultyValid = false;
                    //    errorMessage += ". " + "PANCARD document should be minimum 300x250 pixels" + "<br>";
                    //}
                }
                else
                {
                    if (faculty.Type.ToUpper() == "EXISTFACULTY")
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "PANCARD is mandatory" + "<br>";
                    }
                }

                if (faculty.AadhaarNumber != null)
                {
                    if (faculty.AadhaarCardDocument != null)
                    {
                        //System.IO.Stream fileStream = faculty.AadhaarCardDocument.InputStream;
                        //fileStream.Position = 0;
                        //byte[] fileContents = new byte[faculty.AadhaarCardDocument.ContentLength];
                        //fileStream.Read(fileContents, 0, faculty.AadhaarCardDocument.ContentLength);
                        //System.Drawing.Image image =
                        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        //if (image.Width < 200 || image.Height < 200)
                        //{
                        //    isFacultyValid = false;
                        //    errorMessage += ". " + "Aadhaar Card document should be minimum 200x200 pixels" + "<br>";
                        //}
                    }
                    else
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Aadhaar Card document is mandatory" + "<br>";
                    }
                }

                if (faculty.ProceedingsNo != null)
                {
                    if (faculty.SelectionCommitteeProceedingsDocument != null)
                    {
                        //System.IO.Stream fileStream = faculty.SelectionCommitteeProceedingsDocument.InputStream;
                        //fileStream.Position = 0;
                        //byte[] fileContents = new byte[faculty.SelectionCommitteeProceedingsDocument.ContentLength];
                        //fileStream.Read(fileContents, 0, faculty.SelectionCommitteeProceedingsDocument.ContentLength);
                        //System.Drawing.Image image =
                        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        //if (image.Width < 600 || image.Height < 800)
                        //{
                        //    isFacultyValid = false;
                        //    errorMessage += ". " +
                        //                    "Selection Committee Proceedings document should be minimum 600x800 pixels" +
                        //                    "<br>";
                        //}
                    }
                    else
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Selection Committee Proceedings document is mandatory" + "<br>";
                    }
                }


                if (faculty.IncomeTaxFileUpload != null)
                {


                    if (faculty.IncomeTaxFileUpload.ContentLength > 1024 * 1024 * 1)
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Form16 document should be maximam 1 MB Size" + "<br>";
                    }
                }










                int vIndex = 0;
                foreach (var item in faculty.FacultyEducation)
                {
                    if (item.certificate != null)
                    {
                        //System.IO.Stream fileStream = item.certificate.InputStream;
                        //fileStream.Position = 0;
                        //byte[] fileContents = new byte[item.certificate.ContentLength];
                        //fileStream.Read(fileContents, 0, item.certificate.ContentLength);
                        //System.Drawing.Image image =
                        //    System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        //if (image.Width < 600 || image.Height < 800)
                        //{
                        //    isFacultyValid = false;
                        //    errorMessage += ". " + item.educationName + " document should be minimum 600x800 pixels" +
                        //                    "<br>";
                        //}
                    }
                    else
                    {
                        if (vIndex < 2)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                        }
                        else if (item.studiedEducation != null)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                        }
                    }

                    vIndex++;
                }


                if (!isFacultyValid)
                {
                    TempData["ERROR"] = errorMessage;

                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    TempData["FACULTY"] = faculty;
                    return RedirectToAction("FacultyNew");
                }

                //IF VALIDATION SUCCESS THEN SAVE THE IMAGES & DOCUMENTS

                if (faculty.Photo != null)
                {
                    if (!Directory.Exists(Server.MapPath(photoPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(photoPath));
                    }

                    var ext = Path.GetExtension(faculty.Photo.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        try
                        {

                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                              faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                            faculty.Photo.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                            faculty.facultyPhoto = string.Format("{0}{1}", fileName, ext);
                        }
                        catch (Exception ex)
                        {
                            //LogController Errorlog = new LogController();
                            //  string ErrorMessage = "Controller: OnlineRegistration , Action : FacultyNew , ErrorMessage: Photo name checking ," + "Pan Number Is :" + faculty.PANNumber + ", First Name :" + faculty.FirstName + ", Last Name Is: " + faculty.LastName + "," + "Error:" + ex.Message;
                            //  Errorlog.Log(ErrorMessage);
                            throw;
                        }

                    }
                }
                else if (!String.IsNullOrEmpty(faculty.facultyPhoto))
                {
                    faculty.facultyPhoto = faculty.facultyPhoto;
                }

                if (faculty.PANCardDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(panCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(panCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.PANCardDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.PANCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath),
                            fileName, ext));
                        faculty.facultyPANCardDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (!String.IsNullOrEmpty(faculty.facultyPANCardDocument))
                {
                    faculty.facultyPANCardDocument = faculty.facultyPANCardDocument;
                }

                if (faculty.AadhaarCardDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.AadhaarCardDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.AadhaarCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        faculty.facultyAadhaarCardDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (!String.IsNullOrEmpty(faculty.facultyAadhaarCardDocument))
                {
                    faculty.facultyAadhaarCardDocument = faculty.facultyAadhaarCardDocument;
                }
                else
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Aadhaar Card document is mandatory" + "<br>";
                }
                if (faculty.SelectionCommitteeProceedingsDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(proceedingsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(proceedingsPath));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeProceedingsDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeProceedingsDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(proceedingsPath), fileName, ext));
                        faculty.SelectionCommitteeProcedings = string.Format("{0}{1}", fileName, ext);

                    }
                }
                else if (!String.IsNullOrEmpty(faculty.SelectionCommitteeProcedings))
                {
                    faculty.SelectionCommitteeProcedings = faculty.SelectionCommitteeProcedings;
                }
                //SCM Document is Not Mandatory
                //else
                //{
                //    isFacultyValid = false;
                //    errorMessage += ". " + "Selection Committee Proceedings document is mandatory" + "<br>";
                //}
                int eIndex = 0;
                foreach (var item in faculty.FacultyEducation)
                {
                    if ((eIndex == 0 && item.studiedEducation == null) || (eIndex != 0 && item.studiedEducation != null))
                    {
                        if (item.certificate != null)
                        {
                            if (!Directory.Exists(Server.MapPath(certificatesPath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(certificatesPath));
                            }

                            var ext = Path.GetExtension(item.certificate.FileName);
                            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                            {
                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                  faculty.FirstName.Substring(0, 1) + "-" +
                                                  faculty.LastName.Substring(0, 1) + "_" + item.studiedEducation;
                                item.certificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                    fileName, ext));
                                item.facultyCertificate = string.Format("{0}{1}", fileName, ext);
                            }
                        }
                        else if (!String.IsNullOrEmpty(item.facultyCertificate))
                        {
                            item.facultyCertificate = item.facultyCertificate;
                        }
                        else
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                        }
                    }

                    eIndex++;
                }


                //Income Tax Certificates Saving code/////////////////////
                if (faculty.IncomeTaxFileUpload != null)
                {
                    if (!Directory.Exists(Server.MapPath(IncomeTaxcertificatesPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(IncomeTaxcertificatesPath));
                    }

                    var ext = Path.GetExtension(faculty.IncomeTaxFileUpload.FileName);

                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.IncomeTaxFileUpload.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(IncomeTaxcertificatesPath), fileName, ext));
                        faculty.IncomeTaxFileview = string.Format("{0}{1}", fileName, ext);

                    }
                }
                else if (faculty.IncomeTaxFileview != null)
                {
                    faculty.IncomeTaxFileview = faculty.IncomeTaxFileview;
                }






                if (faculty.Type.ToUpper() == "EXISTFACULTY")
                {
                    faculty.WorkingStatus = true;
                }
                if (!isFacultyValid)
                {
                    TempData["ERROR"] = errorMessage;

                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    TempData["FACULTY"] = faculty;
                    return RedirectToAction("FacultyNew");
                }
                faculty.isView = true;

                TempData["FACULTY"] = faculty;
                return RedirectToAction("FacultyNew");
            }

            if (fid == null && faculty.id > 0)
            {
                fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
            }

            bool IsValidPhoto = true;

            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfBirth).ToShortDateString();
            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfAppointment).ToShortDateString();
            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfRatification).ToShortDateString();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(v => v.Value.Errors.Any());
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                //return new HttpStatusCodeResult(1, message);
            }

            int UserId = 0;

            string userName = string.Empty;
            string password = string.Empty;
            string email = string.Empty;

            //if (ModelState.IsValid && IsValidPhoto)
            //{
            //String.IsNullOrEmpty(faculty.SelectionCommitteeProcedings) ||
            if (fid == null)
            {
                if (String.IsNullOrEmpty(faculty.facultyPhoto) || String.IsNullOrEmpty(faculty.facultyPANCardDocument) || String.IsNullOrEmpty(faculty.facultyAadhaarCardDocument))
                {
                    TempData["ERROR"] = "Please Registration Again";

                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    //TempData["FACULTY"] = faculty;
                    return RedirectToAction("FacultyNew");
                }
                foreach (var certiitem in faculty.FacultyEducation)
                {
                    if (String.IsNullOrEmpty(certiitem.facultyCertificate))
                    {
                        if (certiitem.educationId == 5 || certiitem.educationId == 6)
                        {

                        }
                        else
                        {
                            TempData["ERROR"] = "Please Registration Again";

                            if (faculty.facultyDateOfBirth != null)
                                faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                            if (faculty.facultyDateOfAppointment != null)
                                faculty.facultyDateOfAppointment =
                                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                            if (faculty.facultyDateOfRatification != null)
                                faculty.facultyDateOfRatification =
                                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);
                            return RedirectToAction("FacultyNew");
                        }
                    }
                }
                string StrEmail = db.my_aspnet_users.Where(u => u.name == faculty.Email).Select(u => u.name).FirstOrDefault();
                MembershipCreateStatus createStatus;
                if (string.IsNullOrEmpty(StrEmail))
                {

                    Membership.CreateUser(faculty.Email, faculty.NewPassword, faculty.Email, null, null, true, null,
                        out createStatus);
                }
                else
                {
                    //TempData["ERROR"] = "Your registration is successfully comleted. please login and check your details.";
                    TempData["ERROR"] = "Your Email already exists.";
                    return RedirectToAction("FacultyNew");
                }
                // Attempt to register the user


                if (createStatus == MembershipCreateStatus.Success)
                {
                    //add user role to my_aspnet_usersinroles table
                    my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
                    roleModel.roleId = 7; // 7 = Faculty Role

                    roleModel.userId =
                        db.my_aspnet_users.Where(u => u.name == faculty.Email).Select(u => u.id).FirstOrDefault();
                    db.my_aspnet_usersinroles.Add(roleModel);
                    db.SaveChanges();

                    UserId = roleModel.userId;
                    userName = faculty.Email;
                    password = faculty.NewPassword;
                    email = faculty.Email;
                }
                else
                {
                    UserId = 0;
                    TempData["ERROR"] = ErrorCodeToString1(createStatus);
                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    return View(faculty);
                }
            }
            else
            {
                UserId = db.jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UserId).FirstOrDefault();
            }
            //}

            //Ph.D Certificate Updating Code
            var FacultyPhdData = faculty.FacultyEducation.Where(e => e.educationId == 6).Select(e => e).FirstOrDefault();
            if (FacultyPhdData != null)
            {
                if (FacultyPhdData.certificate != null && FacultyPhdData.educationName != null)
                {
                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
                    }

                    var ext = Path.GetExtension(FacultyPhdData.certificate.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" +
                                          faculty.LastName.Substring(0, 1) + "_" + FacultyPhdData.studiedEducation;
                        FacultyPhdData.certificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                            fileName, ext));
                        FacultyPhdData.facultyCertificate = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else
                {
                    FacultyPhdData.facultyCertificate = FacultyPhdData.facultyCertificate;
                }

                //Set The Path To Phd. Column
                foreach (var educationDetails in faculty.FacultyEducation)
                {
                    if (educationDetails.educationId == 6)
                    {
                        educationDetails.facultyCertificate = FacultyPhdData.facultyCertificate;
                    }
                }
            }







            //if (ModelState.IsValid && UserId > 0 && IsValidPhoto)
            if (UserId > 0 && IsValidPhoto)
            {

                jntuh_registered_faculty regFaculty = new jntuh_registered_faculty();
                regFaculty.UserId = UserId;

                if (faculty.isApproved == true && string.IsNullOrEmpty(faculty.UniqueID))
                {
                    //string strNumber = DateTime.Now.Year.ToString().Substring(2, 2);
                    //strNumber += faculty.FirstName.Substring(0, 1) + faculty.LastName.Substring(0, 1);
                    //strNumber += faculty.facultyDateOfBirth.GetLast(2);
                    //if (faculty.Type != "NewFaculty")
                    //{
                    //    if (faculty.DepartmentId != null)
                    //    {
                    //        string fDepartment = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    //        strNumber += fDepartment.Substring(0, 3);
                    //    }
                    //    else
                    //    {
                    //        strNumber += "ENW";
                    //    }
                    //}
                    //else
                    //{
                    //    strNumber += "NEW";
                    //}
                    //strNumber += "001";
                    //strNumber = VerifyFRN(strNumber);
                    //regFaculty.UniqueID = strNumber;

                    regFaculty.UniqueID = string.Empty;
                    regFaculty.RegistrationNumber =
                        db.jntuh_registered_faculty.Where(f => f.id == fID)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();

                    ////send email
                    //IUserMailer mailer = new UserMailer();
                    //mailer.FacultyOnlineRegistration(faculty.Email, "FacultyOnlineRegistration", "JNTUH Faculty Unique ID", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();
                    //mailer.FacultyOnlineRegistration("aac.do.not.reply@gmail.com", "FacultyOnlineRegistration", "JNTUH Faculty Unique ID", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();

                }
                else if (faculty.isApproved == true && !string.IsNullOrEmpty(faculty.UniqueID))
                {
                    regFaculty.UniqueID =
                        db.jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UniqueID).FirstOrDefault();
                    regFaculty.RegistrationNumber =
                        db.jntuh_registered_faculty.Where(f => f.id == fID)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();
                }
                else
                {
                    regFaculty.UniqueID = string.Empty;
                    string regNumber = db.jntuh_registered_faculty.Where(f => f.UserId == regFaculty.UserId)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();

                    if (string.IsNullOrEmpty(regNumber))
                    {
                        Thread.Sleep(2000);
                        Random rnd = new Random();
                        regFaculty.RegistrationNumber = rnd.Next(0, 9999).ToString("D4") + "-" +
                                                        DateTime.Now.ToString("yyyyMMdd-HHmmss").Substring(2);
                    }
                    else
                    {
                        regFaculty.RegistrationNumber = regNumber;
                    }
                }

                regFaculty.type = faculty.Type;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId == null ? 0 : (int)faculty.GenderId;
                regFaculty.FatherOrHusbandName = faculty.FatherOrhusbandName;
                regFaculty.DateOfBirth = Convert.ToDateTime(faculty.facultyDateOfBirth);
                regFaculty.OrganizationName = faculty.OrganizationName == null ? string.Empty : faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.collegeId = faculty.CollegeId;

                if (regFaculty.type == null)
                {
                    if (faculty.WorkingStatus == null || faculty.WorkingStatus == false)
                    {
                        regFaculty.type = "NewFaculty";
                        regFaculty.WorkingStatus = false;
                    }
                    else if (faculty.WorkingStatus == true)
                    {
                        regFaculty.type = "ExistFaculty";
                    }
                }
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.facultyDateOfAppointment != null)
                    regFaculty.DateOfAppointment = Convert.ToDateTime(faculty.facultyDateOfAppointment);
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU == null
                    ? false
                    : (bool)faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    regFaculty.DateOfRatification = Convert.ToDateTime(faculty.facultyDateOfRatification);
                regFaculty.ProceedingsNumber = faculty.ProceedingsNo;

                if (faculty.TotalExperience != null)
                {
                    regFaculty.TotalExperience = (int)faculty.TotalExperience;
                }
                else
                {
                    regFaculty.TotalExperience = 0;
                }

                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.PanStatus =
                    db.jntuh_pan_status.Where(p => p.PANNumber == regFaculty.PANNumber && p.PANStatus == "E")
                        .Select(e => e.PANStatus)
                        .FirstOrDefault();
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.IncometaxDocument = faculty.IncomeTaxFileview;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.grosssalary = faculty.GrossSalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;

                regFaculty.isActive = true;
                regFaculty.Blacklistfaculy = false;
                regFaculty.isApproved = faculty.isApproved;

                regFaculty.ProceedingDocument = null;
                if (faculty.SelectionCommitteeProcedings != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(proceedingsPath), faculty.SelectionCommitteeProcedings), string.Format("{0}\\{1}", Server.MapPath(proceedingsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.SelectionCommitteeProcedings)));
                    //regFaculty.ProceedingDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.SelectionCommitteeProcedings);
                    regFaculty.ProceedingDocument = faculty.SelectionCommitteeProcedings;
                }

                regFaculty.Photo = null;
                if (faculty.facultyPhoto != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(photoPath), faculty.facultyPhoto), string.Format("{0}\\{1}", Server.MapPath(photoPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPhoto)));
                    //regFaculty.Photo = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPhoto);
                    regFaculty.Photo = faculty.facultyPhoto;
                }

                regFaculty.PANDocument = null;
                if (faculty.facultyPANCardDocument != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(panCardsPath), faculty.facultyPANCardDocument), string.Format("{0}\\{1}", Server.MapPath(panCardsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPANCardDocument)));
                    //regFaculty.PANDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPANCardDocument);
                    regFaculty.PANDocument = faculty.facultyPANCardDocument;
                }

                regFaculty.AadhaarDocument = null;
                if (faculty.facultyAadhaarCardDocument != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(aadhaarCardsPath), faculty.facultyAadhaarCardDocument), string.Format("{0}\\{1}", Server.MapPath(aadhaarCardsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyAadhaarCardDocument)));
                    //regFaculty.AadhaarDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyAadhaarCardDocument);
                    regFaculty.AadhaarDocument = faculty.facultyAadhaarCardDocument;
                }

                int facultyId =
                    db.jntuh_registered_faculty.AsNoTracking()
                        .Where(f => f.id == faculty.id)
                        .Select(f => f.id)
                        .FirstOrDefault();

                if (facultyId == 0)
                {
                    regFaculty.createdBy = UserId;
                    regFaculty.createdOn = DateTime.Now;
                    db.jntuh_registered_faculty.Add(regFaculty);
                    db.SaveChanges();
                }
                else
                {
                    jntuh_registered_faculty updateregFaculty = db.jntuh_registered_faculty.Find(facultyId);
                    regFaculty.id = facultyId;
                    regFaculty.createdBy =
                        db.jntuh_registered_faculty.Where(f => f.id == facultyId)
                            .Select(f => f.createdBy)
                            .FirstOrDefault();
                    regFaculty.createdOn =
                        db.jntuh_registered_faculty.Where(f => f.id == facultyId)
                            .Select(f => f.createdOn)
                            .FirstOrDefault();
                    updateregFaculty.updatedBy = UserId;
                    updateregFaculty.updatedOn = DateTime.Now;
                    regFaculty.updatedBy = UserId;
                    regFaculty.updatedOn = DateTime.Now;
                    db.Entry(updateregFaculty).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (regFaculty.id > 0)
                {
                    //List<jntuh_registered_faculty_education> jntuh_registered_faculty_education_education =
                    //    db.jntuh_registered_faculty_education.Where(f => f.facultyId == regFaculty.id).ToList();

                    //foreach (var item in jntuh_registered_faculty_education_education)
                    //{
                    //    db.jntuh_registered_faculty_education.Remove(item);
                    //    db.SaveChanges();
                    //}

                    foreach (var item in faculty.FacultyEducation)
                    {
                        if (item.studiedEducation != null)
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            education.facultyId = regFaculty.id;
                            education.educationId = item.educationId;
                            education.courseStudied = item.studiedEducation;
                            education.specialization = item.specialization;
                            education.passedYear = item.passedYear == null ? 0 : (int)item.passedYear;
                            education.marksPercentage = item.percentage == null ? 0 : (decimal)item.percentage;
                            education.division = item.division == null ? 0 : (int)item.division;
                            education.boardOrUniversity = item.university;
                            education.placeOfEducation = item.place;

                            education.certificate = null;
                            if (item.facultyCertificate != null)
                            {
                                //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(certificatesPath), item.facultyCertificate), string.Format("{0}\\{1}", Server.MapPath(certificatesPath), regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate)));
                                //education.certificate = regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate);
                                education.certificate = item.facultyCertificate;
                            }

                            int eid =
                                db.jntuh_registered_faculty_education.AsNoTracking()
                                    .Where(e => e.educationId == item.educationId && e.facultyId == regFaculty.id)
                                    .Select(e => e.id)
                                    .FirstOrDefault();

                            if (eid == 0)
                            {
                                education.createdBy = UserId;
                                education.createdOn = DateTime.Now;
                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                            else
                            {
                                jntuh_registered_faculty_education educationupdate =
                                    db.jntuh_registered_faculty_education.Find(eid);
                                //jntuh_registered_faculty_education educationupdate = db.jntuh_registered_faculty_education.AsNoTracking()
                                //    .Where(e => e.id == eid)
                                //    .Select(e => e)
                                //    .FirstOrDefault();
                                education.id = eid;
                                education.createdBy =
                                    db.jntuh_registered_faculty_education.Where(e => e.id == eid)
                                        .Select(e => e.createdBy)
                                        .FirstOrDefault();
                                education.createdOn =
                                    db.jntuh_registered_faculty_education.Where(e => e.id == eid)
                                        .Select(e => e.createdOn)
                                        .FirstOrDefault();
                                if (String.IsNullOrEmpty(educationupdate.certificate))
                                {
                                    educationupdate.certificate = item.facultyCertificate;
                                }
                                educationupdate.updatedBy = UserId;
                                educationupdate.updatedOn = DateTime.Now;
                                education.updatedBy = UserId;
                                education.updatedOn = DateTime.Now;
                                db.Entry(educationupdate).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (fid == null)
                    {
                        ViewBag.Javascript = "Registration completed successfully. Confirmation email sent";
                        TempData["SUCCESS"] = "Registration completed successfully. Confirmation email sent";
                        TempData["FUserName"] = faculty.Email;
                        TempData["FPassword"] = faculty.NewPassword;

                        //send email

                        IUserMailer mailer = new UserMailer();
                        mailer.FacultyOnlineRegistration(faculty.Email, "FacultyOnlineRegistration",
                            "JNTUH Faculty Registration Details", faculty.Email, faculty.NewPassword,
                            regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber)
                            .SendAsync();
                        //mailer.FacultyOnlineRegistration("aac.do.not.reply@gmail.com", "FacultyOnlineRegistration", "JNTUH Faculty Registration Details", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();

                        // RAMESH : DO NOT UNCOMMENT SMS CODE UNTIL FURTHUR AUTHENTICATED MAIL FROM AAC

                        //send sms
                        if (!string.IsNullOrEmpty(regFaculty.Mobile))
                        {
                            bool pStatus = UAAAS.Models.Utilities.SendSms(regFaculty.Mobile,
                                "JNTUH: Your registration with JNTUH affiliated faculty portal is completed. Your registration number is " +
                                regFaculty.RegistrationNumber);
                        }
                    }
                    else
                    {
                        ViewBag.Javascript = "Faculty information updated successfully";
                        TempData["SUCCESS"] = "Faculty information updated successfully";
                    }
                }

                if (fid == null)
                {
                    fid = UAAAS.Models.Utilities.EncryptString(regFaculty.id.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);
                }
            }

            //   string FacultyErrorMessage = "Controller: OnlineRegistration , Action : FacultyNew , Error Type: Date Checking ," + "Registration Number Is :" + faculty.RegistrationNumber + ", Email Is :" + faculty.Email + ", " + "Dtae of Birth :" + faculty.facultyDateOfBirth;
            //  Errorlog.Log(FacultyErrorMessage);
            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);

            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);

            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

            return RedirectToAction("FacultyNew", new { fid = fid });
        }

        /// <summary>
        /// Adjuct Faculty View
        /// </summary>
        /// <param name="faculty"></param>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AdjunctFacty(FacultyRegistration faculty, string fid, string Command)
        {
            faculty.Type = "Adjunct";
            TempData["SUCCESS"] = null;
            TempData["ERROR"] = null;
            return RedirectToAction("AdjunctFacty", "OnlineRegistration", new { fid = fid });
            if (Command == "Go Back" || (faculty.Type == null && faculty.FirstName == null))
            {
                return RedirectToAction("AdjunctFacty");
            }

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;

            int[] notRequiredIds = {25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56};

            foreach (
                var item in
                    db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment {id = item.id, departmentName = item.departmentName});
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(c => new {CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName})
                    .OrderBy(c => c.CollegeName)
                    .ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }

            ViewBag.division = division;

            int fID = 0;
            if (fid != null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.FacultyID = fID;

            string photoPath = "~/Content/Upload/Faculty/PHOTOS";
            string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";
            string proceedingsPath = "~/Content/Upload/Faculty/PROCEEDINGS";
            string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";

            if (Command == "Register")
            {
                bool isFacultyValid = true;
                string errorMessage = "";

                //VALIDATE THE FORM FOR ALL MANDATORY FIELDS

                //VERIFY FOR THE FACULTY PHOTO DIMENSIONS

                if (faculty.Photo != null && faculty.Photo.ContentLength > 0)
                {
                    System.IO.Stream fileStream = faculty.Photo.InputStream;
                    fileStream.Position = 0;
                    byte[] fileContents = new byte[faculty.Photo.ContentLength];
                    fileStream.Read(fileContents, 0, faculty.Photo.ContentLength);
                    System.Drawing.Image image =
                        System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                    if (image.Width > 200 || image.Height > 230)
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                    }

                    if (image.Width < 150 || image.Height < 150)
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Photo should be minimum 150x150 and maximum 200x230 pixels" + "<br>";
                    }
                }
                else
                {
                    isFacultyValid = false;
                    errorMessage += ". " + "Photo is mandatory" + "<br>";
                }


                if (faculty.PANCardDocument != null)
                {
                    System.IO.Stream fileStream = faculty.PANCardDocument.InputStream;
                    fileStream.Position = 0;
                    byte[] fileContents = new byte[faculty.PANCardDocument.ContentLength];
                    fileStream.Read(fileContents, 0, faculty.PANCardDocument.ContentLength);
                    System.Drawing.Image image =
                        System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                    if (image.Width < 300 || image.Height < 250)
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "PANCARD document should be minimum 300x250 pixels" + "<br>";
                    }
                }
                else
                {
                    if (faculty.Type.ToUpper() == "EXISTFACULTY")
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "PANCARD is mandatory" + "<br>";
                    }
                }

                if (faculty.AadhaarNumber != null)
                {
                    if (faculty.AadhaarCardDocument != null)
                    {
                        System.IO.Stream fileStream = faculty.AadhaarCardDocument.InputStream;
                        fileStream.Position = 0;
                        byte[] fileContents = new byte[faculty.AadhaarCardDocument.ContentLength];
                        fileStream.Read(fileContents, 0, faculty.AadhaarCardDocument.ContentLength);
                        System.Drawing.Image image =
                            System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        if (image.Width < 200 || image.Height < 200)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + "Aadhaar Card document should be minimum 200x200 pixels" + "<br>";
                        }
                    }
                    else
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Aadhaar Card document is mandatory" + "<br>";
                    }
                }

                if (faculty.ProceedingsNo != null)
                {
                    if (faculty.SelectionCommitteeProceedingsDocument != null)
                    {
                        System.IO.Stream fileStream = faculty.SelectionCommitteeProceedingsDocument.InputStream;
                        fileStream.Position = 0;
                        byte[] fileContents = new byte[faculty.SelectionCommitteeProceedingsDocument.ContentLength];
                        fileStream.Read(fileContents, 0, faculty.SelectionCommitteeProceedingsDocument.ContentLength);
                        System.Drawing.Image image =
                            System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        if (image.Width < 600 || image.Height < 800)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " +
                                            "Selection Committee Proceedings document should be minimum 600x800 pixels" +
                                            "<br>";
                        }
                    }
                    else
                    {
                        isFacultyValid = false;
                        errorMessage += ". " + "Selection Committee Proceedings document is mandatory" + "<br>";
                    }
                }

                int vIndex = 0;
                foreach (var item in faculty.FacultyEducation)
                {
                    if (item.certificate != null)
                    {
                        System.IO.Stream fileStream = item.certificate.InputStream;
                        fileStream.Position = 0;
                        byte[] fileContents = new byte[item.certificate.ContentLength];
                        fileStream.Read(fileContents, 0, item.certificate.ContentLength);
                        System.Drawing.Image image =
                            System.Drawing.Image.FromStream(new System.IO.MemoryStream(fileContents));

                        if (image.Width < 600 || image.Height < 800)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document should be minimum 600x800 pixels" +
                                            "<br>";
                        }
                    }
                    else
                    {
                        if (vIndex < 2)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                        }
                        else if (item.studiedEducation != null)
                        {
                            isFacultyValid = false;
                            errorMessage += ". " + item.educationName + " document is mandatory" + "<br>";
                        }
                    }

                    vIndex++;
                }



                if (!isFacultyValid)
                {
                    TempData["ERROR"] = errorMessage;

                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    TempData["FACULTY"] = faculty;
                    return RedirectToAction("AdjunctFacty");
                }

                //IF VALIDATION SUCCESS THEN SAVE THE IMAGES & DOCUMENTS

                if (faculty.Photo != null)
                {
                    if (!Directory.Exists(Server.MapPath(photoPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(photoPath));
                    }

                    var ext = Path.GetExtension(faculty.Photo.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.Photo.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                        faculty.facultyPhoto = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (faculty.facultyPhoto != null)
                {
                    faculty.facultyPhoto = faculty.facultyPhoto;
                }

                if (faculty.PANCardDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(panCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(panCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.PANCardDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.PANCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath),
                            fileName, ext));
                        faculty.facultyPANCardDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (faculty.facultyPANCardDocument != null)
                {
                    faculty.facultyPANCardDocument = faculty.facultyPANCardDocument;
                }

                if (faculty.AadhaarCardDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }

                    var ext = Path.GetExtension(faculty.AadhaarCardDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.AadhaarCardDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        faculty.facultyAadhaarCardDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (faculty.facultyAadhaarCardDocument != null)
                {
                    faculty.facultyAadhaarCardDocument = faculty.facultyAadhaarCardDocument;
                }

                if (faculty.SelectionCommitteeProceedingsDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(proceedingsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(proceedingsPath));
                    }

                    var ext = Path.GetExtension(faculty.SelectionCommitteeProceedingsDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                          faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1);
                        faculty.SelectionCommitteeProceedingsDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(proceedingsPath), fileName, ext));
                        faculty.SelectionCommitteeProcedings = string.Format("{0}{1}", fileName, ext);

                    }
                }
                else if (faculty.SelectionCommitteeProcedings != null)
                {
                    faculty.SelectionCommitteeProcedings = faculty.SelectionCommitteeProcedings;
                }

                int eIndex = 0;
                foreach (var item in faculty.FacultyEducation)
                {
                    //if ((eIndex == 0 && item.studiedEducation == null) || (eIndex != 0 && item.studiedEducation != null))
                    if (item.studiedEducation != null)
                    {
                        if (item.certificate != null)
                        {
                            if (!Directory.Exists(Server.MapPath(certificatesPath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(certificatesPath));
                            }

                            var ext = Path.GetExtension(item.certificate.FileName);

                            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                            {
                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                  faculty.FirstName.Substring(0, 1) + "-" +
                                                  faculty.LastName.Substring(0, 1) + "_" + item.studiedEducation;
                                item.certificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                    fileName, ext));
                                item.facultyCertificate = string.Format("{0}{1}", fileName, ext);
                            }
                        }
                        else if (item.facultyCertificate != null)
                        {
                            item.facultyCertificate = item.facultyCertificate;
                        }
                    }

                    eIndex++;
                }
                //Being: Save Membership files to content directory
                SaveMemberShipDocuments(faculty);
                //End : Save Membership files to content directory

                if (faculty.Type.ToUpper() == "EXISTFACULTY")
                {
                    faculty.WorkingStatus = true;
                }

                faculty.isView = true;

                TempData["FACULTY"] = faculty;
                return RedirectToAction("AdjunctFacty");
            }

            if (fid == null && faculty.id > 0)
            {
                fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
            }

            bool IsValidPhoto = true;

            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfBirth).ToShortDateString();
            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfAppointment).ToShortDateString();
            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.DDMMYY2MMDDYY(faculty.facultyDateOfRatification).ToShortDateString();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(v => v.Value.Errors.Any());
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                //return new HttpStatusCodeResult(1, message);
            }

            int UserId = 0;

            string userName = string.Empty;
            string password = string.Empty;
            string email = string.Empty;

            //if (ModelState.IsValid && IsValidPhoto)
            //{
            if (fid == null)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                string StrEmail = db.my_aspnet_users.Where(u => u.name == faculty.Email).Select(u => u.name).FirstOrDefault();
                if (string.IsNullOrEmpty(StrEmail))
                {

                    Membership.CreateUser(faculty.Email, faculty.NewPassword, faculty.Email, null, null, true, null,
                        out createStatus);
                }
                else
                {
                    TempData["ERROR"] = "Your registration is successfully comleted. please login and check your details.";
                    return RedirectToAction("FacultyNew");
                }

                if (createStatus == MembershipCreateStatus.Success)
                {
                    //add user role to my_aspnet_usersinroles table
                    my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
                    roleModel.roleId = 7; // 7 = Faculty Role
                    roleModel.userId =
                        db.my_aspnet_users.Where(u => u.name == faculty.Email).Select(u => u.id).FirstOrDefault();
                    db.my_aspnet_usersinroles.Add(roleModel);
                    db.SaveChanges();

                    UserId = roleModel.userId;
                    userName = faculty.Email;
                    password = faculty.NewPassword;
                    email = faculty.Email;
                }
                else
                {
                    UserId = 0;
                    TempData["ERROR"] = ErrorCodeToString1(createStatus);
                    if (faculty.facultyDateOfBirth != null)
                        faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);
                    if (faculty.facultyDateOfAppointment != null)
                        faculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);
                    if (faculty.facultyDateOfRatification != null)
                        faculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

                    return View(faculty);
                }
            }
            else
            {
                UserId = db.jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UserId).FirstOrDefault();
            }
            //}

            //if (ModelState.IsValid && UserId > 0 && IsValidPhoto)
            if (UserId > 0 && IsValidPhoto)
            {
                jntuh_registered_faculty regFaculty = new jntuh_registered_faculty();
                regFaculty.UserId = UserId;

                if (faculty.isApproved == true && string.IsNullOrEmpty(faculty.UniqueID))
                {
                    //string strNumber = DateTime.Now.Year.ToString().Substring(2, 2);
                    //strNumber += faculty.FirstName.Substring(0, 1) + faculty.LastName.Substring(0, 1);
                    //strNumber += faculty.facultyDateOfBirth.GetLast(2);
                    //if (faculty.Type != "NewFaculty")
                    //{
                    //    if (faculty.DepartmentId != null)
                    //    {
                    //        string fDepartment = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    //        strNumber += fDepartment.Substring(0, 3);
                    //    }
                    //    else
                    //    {
                    //        strNumber += "ENW";
                    //    }
                    //}
                    //else
                    //{
                    //    strNumber += "NEW";
                    //}
                    //strNumber += "001";
                    //strNumber = VerifyFRN(strNumber);
                    //regFaculty.UniqueID = strNumber;

                    regFaculty.UniqueID = string.Empty;
                    regFaculty.RegistrationNumber =
                        db.jntuh_registered_faculty.Where(f => f.id == fID)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();

                    ////send email
                    //IUserMailer mailer = new UserMailer();
                    //mailer.FacultyOnlineRegistration(faculty.Email, "FacultyOnlineRegistration", "JNTUH Faculty Unique ID", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();
                    //mailer.FacultyOnlineRegistration("aac.do.not.reply@gmail.com", "FacultyOnlineRegistration", "JNTUH Faculty Unique ID", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();

                }
                else if (faculty.isApproved == true && !string.IsNullOrEmpty(faculty.UniqueID))
                {
                    regFaculty.UniqueID =
                        db.jntuh_registered_faculty.Where(f => f.id == fID).Select(f => f.UniqueID).FirstOrDefault();
                    regFaculty.RegistrationNumber =
                        db.jntuh_registered_faculty.Where(f => f.id == fID)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();
                }
                else
                {
                    regFaculty.UniqueID = string.Empty;
                    string regNumber =
                        db.jntuh_registered_faculty.Where(f => f.UserId == regFaculty.UserId)
                            .Select(f => f.RegistrationNumber)
                            .FirstOrDefault();

                    if (string.IsNullOrEmpty(regNumber))
                    {
                        Random rnd = new Random();
                        regFaculty.RegistrationNumber = rnd.Next(0, 9999).ToString("D4") + "-" +
                                                        DateTime.Now.ToString("yyyyMMdd-HHmmss").Substring(2);
                    }
                    else
                    {
                        regFaculty.RegistrationNumber = regNumber;
                    }
                }

                regFaculty.type = faculty.Type;
                regFaculty.WorkingType = faculty.WorkingType;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId == null ? 0 : (int) faculty.GenderId;
                regFaculty.FatherOrHusbandName = faculty.FatherOrhusbandName;
                regFaculty.DateOfBirth = Convert.ToDateTime(faculty.facultyDateOfBirth);
                regFaculty.OrganizationName = faculty.OrganizationName == null ? string.Empty : faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.collegeId = faculty.CollegeId;

                if (regFaculty.type == null)
                {
                    if (faculty.WorkingStatus == null || faculty.WorkingStatus == false)
                    {
                        regFaculty.type = "NewFaculty";
                        regFaculty.WorkingStatus = false;
                    }
                    else if (faculty.WorkingStatus == true)
                    {
                        regFaculty.type = "ExistFaculty";
                    }
                }
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.facultyDateOfAppointment != null)
                    regFaculty.DateOfAppointment = Convert.ToDateTime(faculty.facultyDateOfAppointment);
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU == null
                    ? false
                    : (bool) faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    regFaculty.DateOfRatification = Convert.ToDateTime(faculty.facultyDateOfRatification);
                regFaculty.ProceedingsNumber = faculty.ProceedingsNo;

                if (faculty.TotalExperience != null)
                {
                    regFaculty.TotalExperience = (int) faculty.TotalExperience;
                }
                else
                {
                    regFaculty.TotalExperience = 0;
                }

                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;

                regFaculty.MotherName = faculty.MotherName;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.grosssalary = faculty.GrossSalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;

                regFaculty.isActive = true;
                regFaculty.isApproved = faculty.isApproved;

                regFaculty.ProceedingDocument = null;
                if (faculty.SelectionCommitteeProcedings != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(proceedingsPath), faculty.SelectionCommitteeProcedings), string.Format("{0}\\{1}", Server.MapPath(proceedingsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.SelectionCommitteeProcedings)));
                    //regFaculty.ProceedingDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.SelectionCommitteeProcedings);
                    regFaculty.ProceedingDocument = faculty.SelectionCommitteeProcedings;
                }

                regFaculty.Photo = null;
                if (faculty.facultyPhoto != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(photoPath), faculty.facultyPhoto), string.Format("{0}\\{1}", Server.MapPath(photoPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPhoto)));
                    //regFaculty.Photo = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPhoto);
                    regFaculty.Photo = faculty.facultyPhoto;
                }

                regFaculty.PANDocument = null;
                if (faculty.facultyPANCardDocument != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(panCardsPath), faculty.facultyPANCardDocument), string.Format("{0}\\{1}", Server.MapPath(panCardsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPANCardDocument)));
                    //regFaculty.PANDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyPANCardDocument);
                    regFaculty.PANDocument = faculty.facultyPANCardDocument;
                }

                regFaculty.AadhaarDocument = null;
                if (faculty.facultyAadhaarCardDocument != null)
                {
                    //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(aadhaarCardsPath), faculty.facultyAadhaarCardDocument), string.Format("{0}\\{1}", Server.MapPath(aadhaarCardsPath), regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyAadhaarCardDocument)));
                    //regFaculty.AadhaarDocument = regFaculty.RegistrationNumber + Path.GetExtension(faculty.facultyAadhaarCardDocument);
                    regFaculty.AadhaarDocument = faculty.facultyAadhaarCardDocument;
                }
                regFaculty.NOCFile = faculty.NOCFile;
                regFaculty.PresentInstituteAssignedRole = faculty.PresentInstituteAssignedRole;
                regFaculty.PresentInstituteAssignedResponsebility = faculty.PresentInstituteAssignedResponsebility;
                regFaculty.Accomplish1 = faculty.Accomplish1;
                regFaculty.Accomplish2 = faculty.Accomplish2;
                regFaculty.Accomplish3 = faculty.Accomplish3;
                regFaculty.Accomplish4 = faculty.Accomplish4;
                regFaculty.Accomplish5 = faculty.Accomplish5;
                regFaculty.Professional = faculty.Professional;
                regFaculty.Professional2 = faculty.Professional2;
                regFaculty.Professiona3 = faculty.Professiona3;
                regFaculty.MembershipNo1 = faculty.MembershipNo1;
                regFaculty.MembershipNo2 = faculty.MembershipNo2;
                regFaculty.MembershipNo3 = faculty.MembershipNo3;
                regFaculty.MembershipCertificate1 = faculty.MembershipCertificate1;
                regFaculty.MembershipCertificate2 = faculty.MembershipCertificate2;
                regFaculty.MembershipCertificate3 = faculty.MembershipCertificate3;
                if (faculty.AdjunctDepartment != null)
                {
                    regFaculty.AdjunctDepartment = faculty.AdjunctDepartment;
                }

                if (faculty.AdjunctDesignation != null)
                {

                    regFaculty.AdjunctDesignation = faculty.AdjunctDesignation;
                }

                int facultyId =
                    db.jntuh_registered_faculty.AsNoTracking()
                        .Where(f => f.id == faculty.id)
                        .Select(f => f.id)
                        .FirstOrDefault();

                if (facultyId == 0)
                {
                    regFaculty.createdBy = 1;
                    regFaculty.createdOn = DateTime.Now;
                    db.jntuh_registered_faculty.Add(regFaculty);
                    db.SaveChanges();
                }
                else
                {
                    regFaculty.id = facultyId;
                    regFaculty.createdBy =
                        db.jntuh_registered_faculty.Where(f => f.id == facultyId)
                            .Select(f => f.createdBy)
                            .FirstOrDefault();
                    regFaculty.createdOn =
                        db.jntuh_registered_faculty.Where(f => f.id == facultyId)
                            .Select(f => f.createdOn)
                            .FirstOrDefault();
                    regFaculty.updatedBy = 1;
                    regFaculty.updatedOn = DateTime.Now;
                    db.Entry(regFaculty).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (regFaculty.id > 0)
                {
                    List<jntuh_registered_faculty_education> jntuh_registered_faculty_education_education =
                        db.jntuh_registered_faculty_education.Where(f => f.facultyId == regFaculty.id).ToList();

                    //foreach (var item in jntuh_registered_faculty_education_education)
                    //{
                    //    db.jntuh_registered_faculty_education.Remove(item);
                    //    db.SaveChanges();
                    //}

                    foreach (var item in faculty.FacultyEducation)
                    {
                        if (item.studiedEducation != null)
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            education.facultyId = regFaculty.id;
                            education.educationId = item.educationId;
                            education.courseStudied = item.studiedEducation;
                            education.specialization = item.specialization;
                            education.passedYear = item.passedYear == null ? 0 : (int) item.passedYear;
                            education.marksPercentage = item.percentage == null ? 0 : (decimal) item.percentage;
                            education.division = item.division == null ? 0 : (int) item.division;
                            education.boardOrUniversity = item.university;
                            education.placeOfEducation = item.place;

                            education.certificate = null;
                            if (item.facultyCertificate != null)
                            {
                                //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(certificatesPath), item.facultyCertificate), string.Format("{0}\\{1}", Server.MapPath(certificatesPath), regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate)));
                                //education.certificate = regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate);
                                education.certificate = item.facultyCertificate;
                            }

                            int eid =
                                db.jntuh_registered_faculty_education.AsNoTracking()
                                    .Where(e => e.educationId == item.educationId && e.facultyId == regFaculty.id)
                                    .Select(e => e.id)
                                    .FirstOrDefault();

                            if (eid == 0)
                            {
                                education.createdBy = 1;
                                education.createdOn = DateTime.Now;
                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                            else
                            {
                                education.id = eid;
                                education.createdBy =
                                    db.jntuh_registered_faculty_education.Where(e => e.id == eid)
                                        .Select(e => e.createdBy)
                                        .FirstOrDefault();
                                education.createdOn =
                                    db.jntuh_registered_faculty_education.Where(e => e.id == eid)
                                        .Select(e => e.createdOn)
                                        .FirstOrDefault();
                                education.updatedBy = 1;
                                education.updatedOn = DateTime.Now;
                                db.Entry(education).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }

                    if (fid == null)
                    {
                        ViewBag.Javascript = "Registration completed successfully. Confirmation email sent";
                        TempData["SUCCESS"] = "Registration completed successfully. Confirmation email sent";
                        TempData["FUserName"] = faculty.Email;
                        TempData["FPassword"] = faculty.NewPassword;

                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.FacultyOnlineRegistration(faculty.Email, "FacultyOnlineRegistration",
                            "JNTUH Faculty Registration Details", faculty.Email, faculty.NewPassword,
                            regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber)
                            .SendAsync();
                        //mailer.FacultyOnlineRegistration("aac.do.not.reply@gmail.com", "FacultyOnlineRegistration", "JNTUH Faculty Registration Details", faculty.Email, faculty.NewPassword, regFaculty.RegistrationNumber, regFaculty.UniqueID, regFaculty.type, regFaculty.PANNumber).SendAsync();

                        // RAMESH : DO NOT UNCOMMENT SMS CODE UNTIL FURTHUR AUTHENTICATED MAIL FROM AAC

                        //send sms
                        if (!string.IsNullOrEmpty(regFaculty.Mobile))
                        {
                            bool pStatus = UAAAS.Models.Utilities.SendSms(regFaculty.Mobile,
                                "JNTUH: Your registration with JNTUH affiliated faculty portal is completed. Your registration number is " +
                                regFaculty.RegistrationNumber);
                        }
                    }
                    else
                    {
                        ViewBag.Javascript = "Faculty information updated successfully";
                        TempData["SUCCESS"] = "Faculty information updated successfully";
                    }
                }

                if (fid == null)
                {
                    fid = UAAAS.Models.Utilities.EncryptString(regFaculty.id.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);
                }
            }

            if (faculty.facultyDateOfBirth != null)
                faculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth);

            if (faculty.facultyDateOfAppointment != null)
                faculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment);

            if (faculty.facultyDateOfRatification != null)
                faculty.facultyDateOfRatification =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification);

            return RedirectToAction("AdjunctFacty", new {fid = fid});
        }


      
        


        /// <summary>
        /// Save membership Documents Method
        /// </summary>
        /// <param name="faculty"></param>
        private void SaveMemberShipDocuments(FacultyRegistration faculty)
        {
            string membershpeDocumentsPath = "~/Content/Upload/Faculty/MEMBERSHIPDOCUMENTS";
            string NOCDocumentsPath = "~/Content/Upload/Faculty/NOCPDOCUMENTS";

            if (faculty.MembershipFile1 != null)
            {
                if (!Directory.Exists(Server.MapPath(membershpeDocumentsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(membershpeDocumentsPath));
                }

                var ext = Path.GetExtension(faculty.MembershipFile1.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                      faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1) + "_" +
                                      faculty.MembershipNo1;
                    faculty.MembershipFile1.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(membershpeDocumentsPath),
                        fileName, ext));
                    faculty.MembershipCertificate1 = string.Format("{0}{1}", fileName, ext);
                }
            }
            if (faculty.MembershipFile2 != null)
            {
                if (!Directory.Exists(Server.MapPath(membershpeDocumentsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(membershpeDocumentsPath));
                }

                var ext = Path.GetExtension(faculty.MembershipFile2.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                      faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1) + "_" +
                                      faculty.MembershipNo2;
                    faculty.MembershipFile2.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(membershpeDocumentsPath),
                        fileName, ext));
                    faculty.MembershipCertificate2 = string.Format("{0}{1}", fileName, ext);
                }
            }
            if (faculty.MembershipFile3 != null)
            {
                if (!Directory.Exists(Server.MapPath(membershpeDocumentsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(membershpeDocumentsPath));
                }

                var ext = Path.GetExtension(faculty.MembershipFile3.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                      faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1) + "_" +
                                      faculty.MembershipNo3;
                    faculty.MembershipFile3.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(membershpeDocumentsPath),
                        fileName, ext));
                    faculty.MembershipCertificate3 = string.Format("{0}{1}", fileName, ext);
                }
            }
            if (faculty.NOCUploadFile != null)
            {
                if (!Directory.Exists(Server.MapPath(NOCDocumentsPath)))
                {
                    Directory.CreateDirectory(Server.MapPath(NOCDocumentsPath));
                }

                var ext = Path.GetExtension(faculty.NOCUploadFile.FileName);

                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                      faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1) + "_" +
                                      faculty.MembershipNo3;
                    faculty.NOCUploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(NOCDocumentsPath), fileName,
                        ext));
                    faculty.NOCFile = string.Format("{0}{1}", fileName, ext);
                }
            }
        }

        /// <summary>
        /// Faculty Experiance Getting View
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult facultyExperience(string fid)
        {

            ViewBag.fid = string.IsNullOrEmpty(fid) ? "0" : fid;
            int fID = 0;
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int facultyId =
                    db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID =
                        Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }



            }
            else
            {
                if (fid != null)
                {
                    fID =
                        Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (fid != null)
            {
                TempData["fid"] = fid;
            }

            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(c => new {PresentcollegeId = c.id, PresentCollegeName = c.collegeCode + "-" + c.collegeName})
                    .OrderBy(c => c.PresentCollegeName)
                    .ToList();

            facultyExperience ExperiencedEtails = new facultyExperience();

            jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
            jntuh_college_faculty_registered FRegister =
                db.jntuh_college_faculty_registered.Where(F => F.RegistrationNumber == faculty.RegistrationNumber)
                    .Select(F => F)
                    .FirstOrDefault();
            ;
            string CloolegeName =
                db.jntuh_college.Where(C => C.id == faculty.collegeId).Select(C => C.collegeName).FirstOrDefault();
            string Designation =
                db.jntuh_designation.Where(D => D.id == faculty.DesignationId)
                    .Select(D => D.designation)
                    .FirstOrDefault();
            ExperiencedEtails.id = fID;
            ExperiencedEtails.RegistrationNumber = faculty.RegistrationNumber;
            int? CollegeId = 0;
            if (CloolegeName != null && CloolegeName != "")
                ExperiencedEtails.CollegeName = CloolegeName;
            if (faculty.DesignationId != null)
                ExperiencedEtails.facultyDesignation = Designation;
            ExperiencedEtails.DepartmentId = faculty.DepartmentId;
            if (faculty.DepartmentId != null)
            {
                ExperiencedEtails.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
            }
            if (faculty.DateOfAppointment != null)
                ExperiencedEtails.DateofJoining =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
            ExperiencedEtails.FirstName = faculty.FirstName;
            ExperiencedEtails.LastName = faculty.LastName;
            ExperiencedEtails.RegistrationNumber = faculty.RegistrationNumber;
            return View(ExperiencedEtails);

        }

        /// <summary>
        /// Faculty Experience View Posting
        /// </summary>
        /// <param name="ExperienceDetails"></param>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <returns>Save in Table</returns>
        [HttpPost]
        public ActionResult facultyExperience(facultyExperience ExperienceDetails, string fid, string Command)
        {
            int fID = 0; //!string.IsNullOrEmpty(fid) ? Convert.ToInt32(fid) : 0;
            if (fid == null)
            {
                if (TempData["fid"] != null)
                    fid = TempData["fid"] as string;
            }



            if (fid != null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(c => new {PresentcollegeId = c.id, PresentCollegeName = c.collegeCode + "-" + c.collegeName})
                    .OrderBy(c => c.PresentCollegeName)
                    .ToList();
            string StrRelievingLetter = "~/Content/Upload/Faculty/RelievingLetter";
            string StrJoiningLetter = "~/Content/Upload/Faculty/JoiningLetter";
            string StrSCMDocument = "~/Content/Upload/Faculty/SCMDocument";

            var fileName = "";
            if (ExperienceDetails.facultyRelievingLetter != null)
            {
                if (!Directory.Exists(Server.MapPath(StrRelievingLetter)))
                {
                    Directory.CreateDirectory(Server.MapPath(StrRelievingLetter));
                }
                var ext = Path.GetExtension(ExperienceDetails.facultyRelievingLetter.FileName);
                fileName = ExperienceDetails.facultyRelievingLetter.FileName;
                if (ext.ToUpper().Equals(".PDF"))
                {
                    ExperienceDetails.facultyRelievingLetter.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(StrRelievingLetter), fID, fileName));
                    ExperienceDetails.RelievingLetter = string.Format("{0}{1}", fID, fileName);
                }

                else if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    ExperienceDetails.facultyRelievingLetter.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(StrRelievingLetter), fID, fileName));
                    ExperienceDetails.RelievingLetter = string.Format("{0}{1}", fID, fileName);
                }

            }

            if (ExperienceDetails.facultyJoiningOrder != null)
            {
                if (!Directory.Exists(Server.MapPath(StrJoiningLetter)))
                {
                    Directory.CreateDirectory(Server.MapPath(StrJoiningLetter));
                }
                var ext = Path.GetExtension(ExperienceDetails.facultyJoiningOrder.FileName);
                fileName = ExperienceDetails.facultyJoiningOrder.FileName;
                if (ext.ToUpper().Equals(".PDF"))
                {
                    ExperienceDetails.facultyJoiningOrder.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(StrJoiningLetter), fID, fileName));
                    ExperienceDetails.JoiningOrder = string.Format("{0}{1}", fID, fileName);
                }

                else if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    ExperienceDetails.facultyJoiningOrder.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(StrJoiningLetter), fID, fileName));
                    ExperienceDetails.JoiningOrder = string.Format("{0}{1}", fID, fileName);
                }
            }

            // Add SCM Document
            if (ExperienceDetails.facultySCMDocument != null)
            {
                if (!Directory.Exists(Server.MapPath(StrSCMDocument)))
                {
                    Directory.CreateDirectory(Server.MapPath(StrSCMDocument));
                }
                var ext = Path.GetExtension(ExperienceDetails.facultySCMDocument.FileName);
                fileName = ExperienceDetails.facultySCMDocument.FileName;
                if (ext.ToUpper().Equals(".PDF"))
                {
                    ExperienceDetails.facultySCMDocument.SaveAs(string.Format("{0}/{1}{2}",Server.MapPath(StrSCMDocument), fID, fileName));
                    ExperienceDetails.SCMDocument = string.Format("{0}{1}", fID, fileName);
                }

                else if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                {
                    ExperienceDetails.facultySCMDocument.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(StrSCMDocument), fID, fileName));
                    ExperienceDetails.SCMDocument = string.Format("{0}{1}", fID, fileName);
                }
            }
            //End SCM


            jntuh_registered_faculty_experience_log Experience = new jntuh_registered_faculty_experience_log();
            Experience.facultyId = fID;
            Experience.collegeId = ExperienceDetails.PresentcollegeId;
            Experience.facultyDesignationId = ExperienceDetails.facultyNewDesignationId;

            if (ExperienceDetails.facultyDateOfAppointment1 != null)
            {
                ExperienceDetails.facultyDateOfAppointment1 =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(ExperienceDetails.facultyDateOfAppointment1);
                Experience.facultyDateOfAppointment = Convert.ToDateTime(ExperienceDetails.facultyDateOfAppointment1);
            }

            if (ExperienceDetails.facultyDateOfResignation1 != null)
            {
                ExperienceDetails.facultyDateOfResignation1 =
                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(ExperienceDetails.facultyDateOfResignation1);
                Experience.facultyDateOfResignation = Convert.ToDateTime(ExperienceDetails.facultyDateOfResignation1);
            }


            Experience.facultyRelievingLetter = ExperienceDetails.RelievingLetter;
            Experience.facultyJoiningOrder = ExperienceDetails.JoiningOrder;
            Experience.facultySalary = ExperienceDetails.facultySalary;
            Experience.isActive = true;
            Experience.FacultySCMDocument = ExperienceDetails.SCMDocument;
            int facultyLogId =
                db.jntuh_registered_faculty_experience_log.AsNoTracking()
                    .Where(
                        f =>
                            f.facultyId == fID && f.facultyDesignationId == ExperienceDetails.facultyNewDesignationId &&
                            f.collegeId == ExperienceDetails.PresentcollegeId)
                    .Select(f => f.Id)
                    .FirstOrDefault();
            int FUID =
                db.jntuh_registered_faculty.AsNoTracking()
                    .Where(f => f.id == fID)
                    .Select(f => f.UserId)
                    .FirstOrDefault();
            int FLogId =
                db.jntuh_registered_faculty_log.AsNoTracking()
                    .Where(f => f.UserId == FUID)
                    .Select(f => f.id)
                    .FirstOrDefault();
            jntuh_registered_faculty_log reg = new jntuh_registered_faculty_log();
            FacultyRegistration reg1 = new FacultyRegistration();
            // reg.FacultyApprovedStatus
            if (facultyLogId == 0)
            {
                Experience.createdBy = 1;
                Experience.createdOn = DateTime.Now;
                db.jntuh_registered_faculty_experience_log.Add(Experience);
                db.SaveChanges();

                var FLog = db.jntuh_registered_faculty_log.Find(FLogId);
                if (FLog != null)
                {
                    FLog.FacultyApprovedStatus = 0;
                    db.Entry(FLog).State = EntityState.Modified;
                    db.SaveChanges();
                }


                ViewBag.Javascript = "Already Exist";
                TempData["Success"] = "Faculty Experience Details added Successfully";
                TempData["SUCCESS1"] = "Modified data will be reflected after verification";

            }
            else
            {
                ViewBag.Javascript = "Already Exist";
                TempData["Error"] = "Already Exist";
            }
            return RedirectToAction("FacultyNew", new {fid = fid});
        }


        public class Colleges
        {
            public int collegeId { get; set; }
            public string collegeName { get; set; }
        }

        /// <summary>
        /// Faculty Details List
        /// </summary>
        /// <param name="collegeid"></param>
        /// <returns>List of Faculty</returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyList(int? collegeid)
        {
            List<Colleges> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new Colleges
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            colleges.Add(new Colleges() {collegeId = 0, collegeName = "New Faculty"});
            colleges.Add(new Colleges() {collegeId = -1, collegeName = "Adjunct"});

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            if (collegeid != null)
            {

                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                if (collegeid != 0 && collegeid != -1)
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(c => c.collegeId == collegeid).ToList();
                }
                else if (collegeid == 0)
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(c => c.type == "NewFaculty").ToList();
                }
                else
                {
                    jntuh_registered_faculty = db.jntuh_registered_faculty.Where(c => c.type == "Adjunct").ToList();

                }

                var data = jntuh_registered_faculty.Select(a => new FacultyRegistration
                {
                    id = a.id,
                    Type = a.type,
                    RegistrationNumber = a.RegistrationNumber,
                    UniqueID = a.UniqueID,
                    FirstName = a.FirstName,
                    MiddleName = a.MiddleName,
                    LastName = a.LastName,
                    GenderId = a.GenderId,
                    Email = a.Email,
                    facultyPhoto = a.Photo,
                    Mobile = a.Mobile,
                    PANNumber = a.PANNumber,
                    AadhaarNumber = a.AadhaarNumber,
                    isActive = a.isActive,
                    isApproved = a.isApproved,
                    SamePANNumberCount =
                        jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
                    SameAadhaarNumberCount =
                        jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count()
                }).ToList();

                teachingFaculty.AddRange(data);
                // return View(teachingFaculty);

            }
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult DateWiseCounts()
        {
            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<DateWiseCounts> dateWiseCount = new List<DateWiseCounts>();

            var data = jntuh_registered_faculty.GroupBy(a => a.createdOn.Value.Date)
                .Select(g => new DateWiseCounts {Date = g.Key, TotalFacultyRegistrations = g.Count()})
                .OrderByDescending(b => b.Date).ToList();
            dateWiseCount.AddRange(data);
            return View(dateWiseCount);
        }
        
        /// <summary>
        /// PAN Duplicates List in View
        /// </summary>
        /// <returns>list view</returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PANNumberDuplicates()
        {
            string[] registrationNumbers =
                db.jntuh_college_faculty_registered.Select(f => f.RegistrationNumber).ToArray();

            var prinicipalregnos = db.jntuh_college_principal_registered.Select(f => f.RegistrationNumber).ToArray();

            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var dataDuplicatelist = jntuh_registered_faculty.Where(c => c.PANNumber != null)
                .GroupBy(a => new {a.PANNumber})
                .Where(g => g.Count() > 1)
                .Select(g => new PANNumberDuplicates {PanNumber = g.Key.PANNumber, PanNumberDuplicatecount = g.Count()})
                .ToList();

            var data = jntuh_registered_faculty.Join(dataDuplicatelist, r => r.PANNumber, ro => ro.PanNumber,
                (r, ro) => new {r, ro})
                .Select(g =>
                    new FacultyRegistration
                    {
                        CollegeName =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        CollegeCode =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved,
                        SamePANNumberCount = g.ro.PanNumberDuplicatecount
                    });

            teachingFaculty.AddRange(data);
            teachingFaculty =
                teachingFaculty.Where(
                    f =>
                        f.isActive == true &&
                        (registrationNumbers.Contains(f.RegistrationNumber) ||
                         (prinicipalregnos.Contains(f.RegistrationNumber)))).OrderBy(f => f.CollegeCode).ToList();
            return View(teachingFaculty);
        }

        /// <summary>
        /// PAN Verification Screen
        /// </summary>
        /// <param name="collegeid"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PANVerification(int? collegeid)
        {
            string[] registrationNumbers =
                db.jntuh_college_faculty_registered.Select(f => f.RegistrationNumber).ToArray();
            List<Colleges> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new Colleges
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var dataDuplicatelist =
                jntuh_registered_faculty.Where(
                    c => c.PANNumber != null && registrationNumbers.Contains(c.RegistrationNumber))
                    .GroupBy(a => new {a.PANNumber})
                    .Where(g => g.Count() > 1)
                    .Select(
                        g => new PANNumberDuplicates {PanNumber = g.Key.PANNumber, PanNumberDuplicatecount = g.Count()})
                    .ToList();

            var data = jntuh_registered_faculty.Join(dataDuplicatelist, r => r.PANNumber, ro => ro.PanNumber,
                (r, ro) => new {r, ro})
                .Select(g =>
                    new FacultyRegistration
                    {
                        CollegeName =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        CollegeCode =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved,
                        SamePANNumberCount = g.ro.PanNumberDuplicatecount,
                        PanVerificationStatus = g.r.PanVerificationStatus
                    });

            teachingFaculty.AddRange(data);

            teachingFaculty =
                teachingFaculty.Where(
                    f =>
                        (f.PanVerificationStatus == null || f.PanVerificationStatus == "") && f.isActive == true &&
                        registrationNumbers.Contains(f.RegistrationNumber)).OrderBy(f => f.PANNumber).ToList();
          
            return View(teachingFaculty);
        }

        /// <summary>
        /// PAN Verification Screen
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult PANVerificationScreen(string fid)
        {
            //TempData["SUCCESS"] = "";

            FacultyRegistration vFaculty = new FacultyRegistration();
            if (TempData["FACULTY"] != null)
            {
                vFaculty = (FacultyRegistration) TempData["FACULTY"];
            }
            ViewBag.ExperienceStatus = fid == null ? 0 : 1;
            int fID = 0;
            int userID = 0;
            if (!string.IsNullOrEmpty(User.Identity.Name))
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int facultyId =
                    db.jntuh_registered_faculty.Where(f => f.UserId == userID).Select(f => f.id).FirstOrDefault();

                if (fid != null)
                {
                    fID =
                        Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }

            }
            else if (fid != null)
            {
                string fUser = "";
                string fPwd = "";

                if (TempData["FUserName"] != null)
                {
                    fUser = TempData["FUserName"].ToString();
                }

                if (TempData["FPassword"] != null)
                {
                    fPwd = TempData["FPassword"].ToString();
                }

                if (Membership.ValidateUser(fUser.TrimEnd(' '), fPwd.TrimEnd(' ')))
                {
                    FormsAuthentication.SetAuthCookie(fUser, false);
                    return RedirectToAction("FacultyNew", "OnlineRegistration", new {fid = fid});
                }
            }
            var DeactivationReason =
                db.jntuh_registered_faculty_log.Where(i => i.UserId == userID)
                    .Select(i => i.DeactivationReason)
                    .FirstOrDefault();
            var regno =
                db.jntuh_registered_faculty.Where(i => i.id == fID).Select(i => i.RegistrationNumber).FirstOrDefault();
            var remarks =
                db.jntuh_registered_faculty_log.Where(i => i.UserId == userID && i.RegistrationNumber == regno)
                    .Select(i => i.FacultyApprovedStatus)
                    .FirstOrDefault();
            ViewBag.Id = fid;
            ViewBag.FacultyID = fID;
            ViewBag.remarks = remarks;
            if (remarks == 2)
            {
                TempData["remarks"] = "Not Approved because of " + DeactivationReason;
            }
            else
            {
                TempData["remarks"] = "";
            }
            DateTime todayDate = DateTime.Now.Date;

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            int[] notRequiredIds = {25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56};
            foreach (
                var item in
                    db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment {id = item.id, departmentName = item.departmentName});
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true).ToList();
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(c => new {CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName})
                    .OrderBy(c => c.CollegeName)
                    .ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 40; i++)
            {
                prevExperience.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem {Text = i.ToString(), Value = i.ToString()});
            }
            ViewBag.division = division;

            FacultyRegistration regFaculty = new FacultyRegistration();

            if (fID == 0)
            {
                regFaculty.FacultyEducation =
                    db.jntuh_education_category.Where(
                        e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                        .Select(e => new RegisteredFacultyEducation
                        {
                            educationId = e.id,
                            educationName = e.educationCategoryName,
                            studiedEducation = string.Empty,
                            specialization = string.Empty,
                            passedYear = 0,
                            percentage = 0,
                            division = 0,
                            university = string.Empty,
                            place = string.Empty,
                            facultyCertificate = string.Empty,
                        }).ToList();
            }
            else
            {
                regFaculty.FacultyEducation =
                    db.jntuh_education_category.Where(
                        e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                        .Select(e => new RegisteredFacultyEducation
                        {
                            educationId = e.id,
                            educationName = e.educationCategoryName,
                            studiedEducation =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.courseStudied)
                                    .FirstOrDefault(),
                            specialization =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.specialization)
                                    .FirstOrDefault(),
                            passedYear =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.passedYear)
                                    .FirstOrDefault(),
                            percentage =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.marksPercentage)
                                    .FirstOrDefault(),
                            division =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.division)
                                    .FirstOrDefault(),
                            university =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.boardOrUniversity)
                                    .FirstOrDefault(),
                            place =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.placeOfEducation)
                                    .FirstOrDefault(),
                            facultyCertificate =
                                db.jntuh_registered_faculty_education.Where(
                                    fe => fe.educationId == e.id && fe.facultyId == fID)
                                    .Select(fe => fe.certificate)
                                    .FirstOrDefault(),
                        }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }
            }
            int FExID =
                db.jntuh_registered_faculty_experience.Where(E => E.facultyId == fID).Select(E => E.Id).FirstOrDefault();
            regFaculty.RFExperience = db.jntuh_registered_faculty_experience.Where(E => E.Id == FExID)
                .Select(E => new RegisteredfacultyExperience

                {
                    CollegeName =
                        db.jntuh_college.Where(C => C.id == E.collegeId).Select(C => C.collegeName).FirstOrDefault(),
                    facultyDesignation =
                        db.jntuh_designation.Where(D => D.id == E.facultyDesignationId)
                            .Select(D => D.designation)
                            .FirstOrDefault(),
                    CollegeId = E.collegeId,
                    DesignationId = E.facultyDesignationId,
                    facultyDateOfAppointment = E.facultyDateOfAppointment,
                    facultyDateOfResignation = E.facultyDateOfResignation,
                    RelievingLetter = E.facultyRelievingLetter,
                    JoiningOrder = E.facultyJoiningOrder,
                    Salary = E.facultySalary

                }).ToList();
            regFaculty.GenderId = null;
            regFaculty.isFacultyRatifiedByJNTU = null;
            ViewBag.ExperienceStatus = regFaculty.RFExperience.Count;
            if (fID > 0)
            {
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.NewPassword = "TEMP@PWD";
                regFaculty.ConfirmPassword = "TEMP@PWD";
                int facultyUserId = db.jntuh_registered_faculty.Find(regFaculty.id).UserId;
                regFaculty.UserName =
                    db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                int? FStatus =
                    db.jntuh_registered_faculty_log.Where(F => F.RegistrationNumber == faculty.RegistrationNumber)
                        .Select(F => F.id)
                        .FirstOrDefault();
                ViewBag.RegistrationNum = FStatus;
                ViewBag.VerificationStatus =
                    db.jntuh_registered_faculty_log.Where(
                        F => F.RegistrationNumber == faculty.RegistrationNumber && F.FacultyApprovedStatus == 2)
                        .Select(F => F.FacultyApprovedStatus)
                        .FirstOrDefault();
                //ViewBag.RegistrationNum = "5586-150420-133949";   
                ViewBag.EXFacultyId =
                    db.jntuh_registered_faculty_experience_log.Where(F => F.facultyId == faculty.id)
                        .Select(F => F.facultyId)
                        .FirstOrDefault();
                if (faculty.DateOfBirth != null)
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.DesignationId = faculty.DesignationId;
                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }

                regFaculty.CollegeId = faculty.collegeId;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.OtherDepartment = faculty.OtherDepartment;
                regFaculty.OtherDesignation = faculty.OtherDesignation;
                if (faculty.DateOfAppointment != null)
                    regFaculty.facultyDateOfAppointment =
                        UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                    regFaculty.facultyDateOfRatification =
                        UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.Email = faculty.Email;
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.id = regFaculty.id;
            }
            else
            {
                if (vFaculty.isView != null)
                {
                    regFaculty = vFaculty;

                    if (vFaculty.CollegeId != null)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(vFaculty.CollegeId).collegeName;
                    }

                    if (vFaculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(vFaculty.DesignationId).designation;
                    }

                    if (vFaculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(vFaculty.DepartmentId).departmentName;
                    }

                    regFaculty.OtherDepartment = vFaculty.OtherDepartment;
                    regFaculty.OtherDesignation = vFaculty.OtherDesignation;
                    regFaculty.isFacultyRatifiedByJNTU = vFaculty.isFacultyRatifiedByJNTU;


                }
            }

            return View(regFaculty);
        }
        /// <summary>
        /// Faculty Not Approved Information Pratial View with Resons Getting
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <param name="facultyid"></param>
        /// <returns>Partion View Getting</returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult NotApprovedInformation(string fid, string Command, string facultyid)
        {

            var notapproved = db.jntuh_faculty_deactivation_reason.ToList();
            ViewBag.notapproved = notapproved;
            TempData["facultyid"] = facultyid;
            TempData["fid"] = fid;
            return PartialView("_NotApprovedInformation", notapproved);
        }

        /// <summary>
        /// Faculty Not Approved Information Pratial View with Resons Posting
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <param name="facultyid"></param>
        /// <returns>Partion View Posting</returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult NotApprovedFacultyInformation(string fid, int facultyid, string others)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var uid = db.jntuh_registered_faculty.Where(i => i.id == facultyid).FirstOrDefault();
            var faculty_log =
                db.jntuh_registered_faculty_log.Where(
                    i => i.UserId == uid.UserId && i.RegistrationNumber == uid.RegistrationNumber).FirstOrDefault();

            if (uid.UserId != null)
            {
                uid.PanDeactivationReason = others;
                uid.PanVerificationStatus = "Not Approved";
                db.SaveChanges();
            }

            TempData["facultyerror"] = "";

            return RedirectToAction("PANVerification");
        }

        /// <summary>
        /// Showing Aadhaar Number Duplicates 
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <param name="facultyid"></param>
        /// <returns>Duplicate Aadhaar no count show on count in view</returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AadhaarNumberDuplicates()
        {
            string[] registrationNumbers =
                db.jntuh_college_faculty_registered.Select(f => f.RegistrationNumber).ToArray();

            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var dataAadhaarDuplicate = jntuh_registered_faculty.Where(c => c.AadhaarNumber != null)
                .GroupBy(a => new {a.AadhaarNumber})
                .Where(g => g.Count() > 1)
                .Select(
                    g =>
                        new AadhaarNumberDuplicates
                        {
                            AadhaarNumber = g.Key.AadhaarNumber,
                            AadhaarNumberDuplicatecount = g.Count()
                        })
                .OrderBy(b => b.AadhaarNumber).ToList();

            var data = jntuh_registered_faculty.Join(dataAadhaarDuplicate, r => r.AadhaarNumber, ro => ro.AadhaarNumber,
                (r, ro) => new {r, ro})
                .Select(g =>
                    new FacultyRegistration
                    {
                        CollegeName =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        CollegeCode =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved,
                        SameAadhaarNumberCount = g.ro.AadhaarNumberDuplicatecount
                    });

            teachingFaculty.AddRange(data);
            teachingFaculty =
                teachingFaculty.Where(f => f.isActive == true && registrationNumbers.Contains(f.RegistrationNumber))
                    .OrderBy(f => f.CollegeCode)
                    .ToList();
            return View(teachingFaculty);
        }

        /// <summary>
        /// Faculty Information View
        /// </summary>
        /// <param name="faculty"></param>
        /// <param name="fid"></param>
        /// <param name="Command"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult FacultyInformation(FacultyRegistration faculty, string fid, string Command)
        {
            var uid = db.jntuh_registered_faculty.Where(i => i.id == faculty.id).FirstOrDefault();
            fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(),
                System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
            TempData["SUCCESS"] = null;
            TempData["ERROR"] = null;
            if (uid.UserId != null)
            {
                uid.PanDeactivationReason = "";
                uid.PanVerificationStatus = "Approved";
                db.SaveChanges();
            }
            return RedirectToAction("PANVerification");
        }

        /// <summary>
        /// PAN Verification Completed View
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PANVerificationCompleted()
        {
            string[] registrationNumbers =
                db.jntuh_college_faculty_registered.Select(f => f.RegistrationNumber).ToArray();
            List<Colleges> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new Colleges
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var dataDuplicatelist =
                jntuh_registered_faculty.Where(
                    c => c.PANNumber != null && registrationNumbers.Contains(c.RegistrationNumber))
                    .GroupBy(a => new {a.PANNumber})
                    .Where(g => g.Count() > 1)
                    .Select(
                        g => new PANNumberDuplicates {PanNumber = g.Key.PANNumber, PanNumberDuplicatecount = g.Count()})
                    .ToList();

            var data = jntuh_registered_faculty.Join(dataDuplicatelist, r => r.PANNumber, ro => ro.PanNumber,
                (r, ro) => new {r, ro})
                .Select(g =>
                    new FacultyRegistration
                    {
                        CollegeName =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        CollegeCode =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved,
                        SamePANNumberCount = g.ro.PanNumberDuplicatecount,
                        PanVerificationStatus = g.r.PanVerificationStatus,
                        PanDeactivationReasion = g.r.PanDeactivationReason
                    });

            teachingFaculty.AddRange(data);
            string[] PanNumbers = teachingFaculty.Select(F => F.PANNumber).Distinct().ToArray();
            string StrPannumber = "";
            foreach (string str in PanNumbers)
            {
                if (StrPannumber != "")
                    StrPannumber += Environment.NewLine + str;
                else
                    StrPannumber = str;
            }
            //teachingFaculty = teachingFaculty.Where(f => f.PanVerificationStatus != null && f.PanVerificationStatus != "" && f.isActive == true && registrationNumbers.Contains(f.RegistrationNumber)).OrderBy(f => f.PANNumber).ToList();
            teachingFaculty =
                teachingFaculty.Where(f => f.isActive == true && registrationNumbers.Contains(f.RegistrationNumber))
                    .OrderBy(f => f.PANNumber)
                    .ToList();
         
            return View(teachingFaculty);
        }

        #region College wise PAN Verification with VJAYAKUMARI MADAM Coardination
        /// <summary>
        /// College wise PAN Verification with VJAYAKUMARI MADAM Coardination,PAN Verifications Collegewise
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        public ActionResult PANVerificationCollegewise()
        {
            // string[] CollegeregistrationNumbers = db.jntuh_college_faculty_registered.Select(F => F.RegistrationNumber).ToArray();
            string[] registrationNumbers =
                db.jntuh_college_faculty_registered.Select(f => f.RegistrationNumber).ToArray();
            List<Colleges> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new Colleges
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            List<jntuh_registered_faculty> jntuh_registered_faculty =
                db.jntuh_registered_faculty.AsNoTracking().ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var dataDuplicatelist =
                jntuh_registered_faculty.Where(
                    c => c.PANNumber != null && registrationNumbers.Contains(c.RegistrationNumber))
                    .GroupBy(a => new {a.PANNumber})
                    .Where(g => g.Count() > 1)
                    .Select(
                        g => new PANNumberDuplicates {PanNumber = g.Key.PANNumber, PanNumberDuplicatecount = g.Count()})
                    .ToList();

            var data = jntuh_registered_faculty.Join(dataDuplicatelist, r => r.PANNumber, ro => ro.PanNumber,
                (r, ro) => new {r, ro})
                .Select(g =>
                    new FacultyRegistration
                    {
                        CollegeName =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        CollegeCode =
                            jntuh_college.Where(c => c.id == g.r.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved,
                        SamePANNumberCount = g.ro.PanNumberDuplicatecount,
                        PanVerificationStatus = g.r.PanVerificationStatus,
                        PanDeactivationReasion = g.r.PanDeactivationReason
                    });

            teachingFaculty.AddRange(data);
            string[] PanNumbers = teachingFaculty.Select(F => F.PANNumber).Distinct().ToArray();
            string StrPannumber = "";
            //foreach (string str in PanNumbers)
            //{
            //    if (StrPannumber != "")
            //        StrPannumber += Environment.NewLine + str;
            //    else
            //        StrPannumber = str;
            //}

            //teachingFaculty = teachingFaculty.Where(f => f.PanVerificationStatus != null && f.PanVerificationStatus != "" && f.isActive == true && registrationNumbers.Contains(f.RegistrationNumber)).OrderBy(f => f.PANNumber).ToList();

            teachingFaculty =
                teachingFaculty.Where(f => f.isActive == true && registrationNumbers.Contains(f.RegistrationNumber))
                    .OrderBy(f => f.PANNumber)
                    .ToList();

            // teachingFaculty = teachingFaculty.Where(f => f.isActive == true).OrderBy(f => f.PANNumber).ToList();

            //string[] RegNumbers = teachingFaculty.Select(F => F.RegistrationNumber).ToArray();

            //int[] Collegeids = db.jntuh_college_faculty_registered.Where(F => RegNumbers.Contains(F.RegistrationNumber)).Select(f => f.collegeId).Distinct().ToArray();
            //ViewBag.Colleges = colleges.Where(c => Collegeids.Contains(c.collegeId)).OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            //if(collegeid!=null && collegeid!=0)
            //{
            //    teachingFaculty = teachingFaculty.Where(F => F.CollegeId == collegeid).Select(F => F).ToList();
            //}
            //else
            //{
            //    teachingFaculty = teachingFaculty.Take(10).ToList();
            //}



            //int totalPages = 1;
            //int totalLabs = teachingFaculty.Count();
            //int First = 0;
            //int second = 0;
            //if (totalLabs > 100)
            //{
            //    totalPages = totalLabs / 100;
            //    if (totalLabs > 100 * totalPages)
            //    {
            //        totalPages = totalPages + 1;
            //    }
            //}
            //if (pageNumber == null)
            //{
            //    First = 0;
            //    second = 100;
            //}
            //else if (pageNumber > 1)
            //{
            //    First = ((pageNumber ?? default(int)) * 100) - 100;

            //    //second = (pageNumber ?? default(int)) * 100;
            //    second = 100;
            //    int Total = teachingFaculty.Count;
            //    if (Total <= ((pageNumber ?? default(int)) * 100))
            //    {

            //        second = Total - First;
            //    }
            //}

            //ViewBag.Pages = totalPages;
            //teachingFaculty = teachingFaculty.GetRange(First, totalLabs > 100 ? second : totalLabs);
            return View(teachingFaculty);
        }

        #endregion

        #region Status Codes
        /// <summary>
        /// Status Codes
        /// </summary>
        /// <param name="createStatus"></param>
        /// <returns></returns>
        private static string ErrorCodeToString1(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion


        /// <summary>
        /// Print Faculty Information in View
        /// </summary>
        /// <param name="preview"></param>
        /// <param name="strfacultyId"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public ActionResult FacultyData(int preview, string strfacultyId, string reg)
        {
            if (!string.IsNullOrEmpty(strfacultyId))
            {
                int fid =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strfacultyId,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                string pdfPath = string.Empty;
                if (preview == 0)
                {
                    pdfPath = SaveFacultyDataPdf(preview, fid, reg);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                return File(pdfPath, "application/pdf", "Faculty Information-" + reg + ".pdf");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }
        /// <summary>
        /// Saving Faculty Data in PDF file
        /// </summary>
        /// <param name="preview"></param>
        /// <param name="fid"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public string SaveFacultyDataPdf(int preview, int fid, string reg)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/temp/FacultyPrint");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/temp/FacultyPrint")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/temp/FacultyPrint"));
            }

            if (preview == 0)
            {
                fullPath = path + "/" + "Faculty_Data_Print_" + reg + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf"; //
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeCode = reg;
                iTextEvents.CollegeName = reg;
                iTextEvents.formType = "Faculty Information";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/FacultyData.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = GetFacultyData(fid, contents);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        /// <summary>
        /// Get Faculty Data Method
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public string GetFacultyData(int fid, string contents)
        {

            var facultydata = (from r in db.jntuh_registered_faculty
                join ca in db.jntuh_college on r.collegeId equals ca.id into rcdata
                from rc in rcdata.DefaultIfEmpty()
                join dep in db.jntuh_department on r.DepartmentId equals dep.id into rcdepdata
                from rcd in rcdepdata.DefaultIfEmpty()
                join des in db.jntuh_designation on r.DesignationId equals des.id into rcdepdesData
                from rcdd in rcdepdesData.DefaultIfEmpty()
                where r.id == fid
                select new FacultyRegistration()
                {
                    id = r.id,
                    CollegeId = r.collegeId,
                    CollegeName = rc.collegeName,
                    Eid = r.UserId,
                    Type = r.type,
                    RegistrationNumber = r.RegistrationNumber,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    MiddleName = r.MiddleName,
                    GenderId = r.GenderId,
                    FatherOrhusbandName = r.FatherOrHusbandName,
                    MotherName = r.MotherName,
                    DateOfBirth = r.DateOfBirth,
                    WorkingStatus = r.WorkingStatus,
                    OrganizationName = r.OrganizationName,
                    DesignationId = r.DesignationId,
                    designation = rcdd.designation,
                    OtherDesignation = r.OtherDesignation,
                    DepartmentId = r.DepartmentId,
                    department = rcd.departmentName,
                    OtherDepartment = r.OtherDesignation,
                    GrossSalary = r.grosssalary,
                    DateOfAppointment = r.DateOfAppointment,
                    isFacultyRatifiedByJNTU = r.isFacultyRatifiedByJNTU,
                    ProceedingsNo = r.ProceedingsNumber,
                    SelectionCommitteeProcedings = r.ProceedingDocument,
                    AICTEFacultyId = r.AICTEFacultyId,
                    TotalExperience = r.TotalExperience,
                    TotalExperiencePresentCollege = r.TotalExperiencePresentCollege,
                    PANNumber = r.PANNumber,
                    AadhaarNumber = r.AadhaarNumber,
                    Mobile = r.Mobile,
                    Email = r.Email,
                    National = r.National,
                    InterNational = r.InterNational,
                    Awards = r.Awards,
                    Citation = r.Citation,
                    facultyPhoto = r.Photo,
                    facultyPANCardDocument = r.PANDocument,
                    facultyAadhaarCardDocument = r.AadhaarDocument,
                    isActive = r.isActive,
                    isApproved = r.isApproved
                }).ToList();
            var facultyeducationdata = (from educatgy in db.jntuh_education_category
                join edu in db.jntuh_registered_faculty_education on educatgy.id equals edu.educationId
                where
                    edu.facultyId == fid &&
                    (educatgy.id == 1 || educatgy.id == 3 || educatgy.id == 4 || educatgy.id == 5 || educatgy.id == 6)
                select new
                {
                    id = edu.id,
                    educationId = educatgy.id,
                    facultyId = edu.facultyId,
                    studiedEducation = edu.courseStudied,
                    educationName = educatgy.educationCategoryName,
                    specialization = edu.specialization,
                    passedYear = edu.passedYear,
                    marksPercentage = edu.marksPercentage,
                    division = edu.division,
                    boardOrUniversity = edu.boardOrUniversity,
                    placeOfEducation = edu.placeOfEducation,
                    certificte = edu.certificate,

                }).ToList();
            string contentdata = string.Empty;

            var Gender = "";
            var presentworking = "";
            if (facultydata[0].GenderId == 1)
            {
                Gender = "Male";
            }
            else
            {
                Gender = "FeMale";
            }

            if (facultydata[0].WorkingStatus == true)
            {
                presentworking = "Yes";
            }
            else
            {
                presentworking = "No";
            }

            string facultyphoto = "";
            if (!string.IsNullOrEmpty(facultydata[0].facultyPhoto))
            {
                facultyphoto = "/Content/Upload/Faculty/PHOTOS/" + facultydata[0].facultyPhoto;
            }
            string imgpath = @"~" + facultyphoto.Replace("%20", " ");
            imgpath = System.Web.HttpContext.Current.Server.MapPath(imgpath);


            string facultyPAndoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].facultyPANCardDocument))
            {
                facultyPAndoc = "/Content/Upload/Faculty/PANCARDS/" + facultydata[0].facultyPANCardDocument;
            }
            string pandocpath = @"~" + facultyPAndoc.Replace("%20", " ");
            pandocpath = System.Web.HttpContext.Current.Server.MapPath(pandocpath);

            string facultyAadhaaardoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].facultyAadhaarCardDocument))
            {
                facultyAadhaaardoc = "/Content/Upload/Faculty/AADHAARCARDS/" + facultydata[0].facultyAadhaarCardDocument;
            }
            string aadhaardocpath = @"~" + facultyAadhaaardoc.Replace("%20", " ");
            aadhaardocpath = System.Web.HttpContext.Current.Server.MapPath(aadhaardocpath);

            string facultySCMdoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].SelectionCommitteeProcedings))
            {
                facultySCMdoc = "/Content/Upload/Faculty/PROCEEDINGS/" + facultydata[0].SelectionCommitteeProcedings;
            }
            string scmdocpath = @"~" + facultySCMdoc.Replace("%20", " ");
            scmdocpath = System.Web.HttpContext.Current.Server.MapPath(scmdocpath);


            string Middlename = "--";
            if (!string.IsNullOrEmpty(facultydata[0].MiddleName))
            {
                Middlename = facultydata[0].MiddleName;
            }

            contentdata += "<p style='text-align:left'><b><u> Faculty Information</u></b></p>";

            contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'>"; //cellspacing='0' cellpadding='5'
            contentdata +=
                "<tr><td style='text-align:right'>Registration ID  <b>:</b> </td><td colspan='2' style='text-align:left'>" +
                facultydata[0].RegistrationNumber + "</td>";
            contentdata += "<td rowspan='2'  style='text-align:center'><img src='" + imgpath +
                           "' height='40px' width='50px'/></td></tr>";
            contentdata +=
                "<tr><td style='text-align:right'>First Name  : </td><td colspan='2' style='text-align:left'>" +
                facultydata[0].FirstName + "</td></tr>";
            contentdata += "<tr><td style='text-align:right'>Last Name  : </td><td style='text-align:left'>" +
                           facultydata[0].LastName + "</td>";
            contentdata += "<td style='text-align:right'>Middle Name : </td><td style='text-align:left'>" + Middlename +
                           "</td></tr>";
            contentdata += "<tr><td style='text-align:right'>Email Id  : </td><td  style='text-align:left'>" +
                           facultydata[0].Email + "</td><td style='text-align:right'>Mobile No  : </td><td>" +
                           facultydata[0].Mobile + "</td></tr>";
            contentdata += "<tr><td style='text-align:right'>Father's/Husband's : </td><td style='text-align:left'>" +
                           facultydata[0].FatherOrhusbandName + "</td>";
            contentdata += "<td style='text-align:right'>Mother Name  : </td><td>" + facultydata[0].MotherName +
                           "</td></tr>";
            contentdata += "<tr><td style='text-align:right'>Gender  : </td><td>" + Gender + "</td></tr>";
            contentdata += "<td style='text-align:right'>Date of Birth  : </td><td>" +
                           UAAAS.Models.Utilities.MMDDYY2DDMMYY(facultydata[0].DateOfBirth.ToString()) + "</td></tr>";

            contentdata += "<tr><td style='text-align:right'>Working Type  : </td><td>" + facultydata[0].Type + "</td>";
            if (presentworking == "Yes")
            {
                contentdata += "<td style='text-align:right'>Present Working  : </td><td>" + presentworking +
                               "</td></tr>";
            }
            else
            {
                contentdata += "<td style='text-align:right'>Present Working  : </td><td>" + presentworking +
                               "</td></tr>";
            }
            contentdata += "<tr><td style='text-align:right'>PAN Number  : </td><td>" + facultydata[0].PANNumber +
                           "</td>";
            contentdata += "<td style='text-align:right'>Aadhaar Number  : </td><td>" + facultydata[0].AadhaarNumber +
                           "</td></tr>";

            contentdata += "<tr><td style='text-align:right'>Parent Organization  : </td><td>" +
                           facultydata[0].CollegeName +
                           "</td><td style='text-align:right'>Date of Appointment  : </td><td>" +
                           UAAAS.Models.Utilities.MMDDYY2DDMMYY(facultydata[0].DateOfAppointment.ToString()) +
                           "</td></tr>";
            contentdata += "<tr><td style='text-align:right'>Department  : </td><td>" + facultydata[0].department +
                           "</td>";
            contentdata += "<td style='text-align:right'>Designation  : </td><td>" + facultydata[0].department +
                           "</td></tr>";
            if (facultydata[0].department == "Others")
            {
                contentdata += "<tr><td style='text-align:right'>Other Department : </td><td>" +
                               facultydata[0].OtherDepartment + "</td>";
            }
            if (facultydata[0].designation == "Others")
            {
                contentdata += "<td style='text-align:right'>Other Designation  : </td><td>" +
                               facultydata[0].OtherDesignation + "</td></tr>";
            }
            contentdata +=
                "<tr><td style='text-align:right'>College Selection Committee Proceedings Number  : </td><td>" +
                facultydata[0].ProceedingsNo +
                "</td><td style='text-align:right'>Experience in the present Institution (years)</td><td>" +
                facultydata[0].TotalExperiencePresentCollege + "</td></tr>";

            contentdata += "<tr><td style='text-align:right'>Total Experience  : </td><td>" +
                           facultydata[0].TotalExperience +
                           "</td><td style='text-align:right'>AICTE Faculty Id</td><td>" + facultydata[0].AICTEFacultyId +
                           "</td></tr>";


            contentdata += "<tr><td style='text-align:right'>Gross Salary Last Drawn</td><td>" +
                           facultydata[0].GrossSalary + "</td>";
            contentdata += "<td style='text-align:right'>Select Commite Proceeding Document  : </td><td><img src='" +
                           scmdocpath + "' height='30px' width='40px'/></td></tr>";


            contentdata += "<tr><td style='text-align:right'>PAN Document  : </td><td><img src='" + pandocpath +
                           "' height='30px' width='40px'/></td>";
            contentdata += "<td style='text-align:right'>Aadhaaar Document  : </td><td><img src='" + aadhaardocpath +
                           "' height='30px' width='40px'/></td></tr>";

            contentdata += "</table><br/>";
            contentdata += "<p style='text-align:left'><b><u>Educational Qualifications</u></b></p>";
            contentdata += "<br/><table border='1' cellspacing='0' cellpadding='5'><thead><tr>";
            contentdata += "<th></th>";
            contentdata += "<th>Course Studied</th>";
            contentdata += "<th>Branch / Specialization</th>";
            contentdata += "<th>Year of Passing (YYYY)</th>";
            contentdata += "<th>% of Marks / CGPA</th>";
            contentdata += "<th>Division</th>";
            contentdata += "<th>Board/University</th>";
            contentdata += "<th>Place</th>";
            contentdata += "<th>Scanned Certificate</th>";
            contentdata += "</tr></thead><tbody>";
            foreach (var item in facultyeducationdata)
            {
                string facultycertificatedoc = "";
                if (!string.IsNullOrEmpty(item.certificte))
                {
                    facultycertificatedoc = "/Content/Upload/Faculty/CERTIFICATES/" + item.certificte;
                }
                string certificatedocpath = @"~" + facultycertificatedoc.Replace("%20", " ");
                certificatedocpath = System.Web.HttpContext.Current.Server.MapPath(certificatedocpath);
                contentdata += "<tr>";
                contentdata += "<td>" + item.educationName + "</td>";
                contentdata += "<td>" + item.studiedEducation + "</td>";
                contentdata += "<td>" + item.specialization + "</td>";
                contentdata += "<td>" + item.passedYear + "</td>";
                contentdata += "<td>" + item.marksPercentage + "</td>";
                contentdata += "<td>" + item.division + "</td>";
                contentdata += "<td>" + item.boardOrUniversity + "</td>";
                contentdata += "<td>" + item.placeOfEducation + "</td>";
                contentdata += "<td><img src='" + certificatedocpath + "' height='60px' width='40px'/></td></tr>";
            }
            contentdata += "</tbody></table><br/>";
            contentdata += "<p style='text-align:left'><b><u>Publications</u></b></p>";
            contentdata +=
                "<br/><table border='0' cellspacing='0' width='100%' cellpadding='5'><tr><td width='30%' style='text-align:right'>National : </td><td style='text-align:left'>" +
                facultydata[0].National + "</td></tr>";
            contentdata +=
                "<tr><td width='30%' style='text-align:right'>InterNational : </td><td style='text-align:left'>" +
                facultydata[0].InterNational + "</td></tr>";
            contentdata += "<tr><td width='30%' style='text-align:right'>Citation : </td><td style='text-align:left'>" +
                           facultydata[0].Citation + "</td></tr>";
            contentdata += "<tr><td width='30%' style='text-align:right'>Awards : </td><td style='text-align:left'>" +
                           facultydata[0].Awards + "</td></tr></table>";

            contents = contents.Replace("##COLLEGE_RANDOMCODE##", contentdata);
            return contents;
        }

        /// <summary>
        /// Duplicate PAN Numbers 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PANDuplicateAssocitedColleges()
        {

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_college = db.jntuh_college.ToList();



            var PANNumbers =
                db.jntuh_registered_faculty.Where(e => e.PanStatus == "EE" || e.PanStatus == "HD" || e.PanStatus == "ID")
                    .Select(e => e.PANNumber)
                    .ToArray();
            var jntuh_registered_faculty =db.jntuh_registered_faculty.AsNoTracking().Where(e => e.PanStatus == "D").Select(e => e).ToList();

            teachingFaculty =db.jntuh_registered_faculty.Join(db.jntuh_college_faculty_registered, r => r.RegistrationNumber,ro => ro.RegistrationNumber, (r, ro) => new {r, ro})
                .Where(e =>e.r.PanStatus == "D" && !PANNumbers.Contains(e.r.PANNumber))
                    .Select(g =>new FacultyRegistration
                        {
                            //  CollegeName = jntuh_college.Where(c => c.id == g.ro.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                            //   CollegeCode = jntuh_college.Where(c => c.id == g.ro.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                            CollegeId = g.ro.collegeId,
                            PANNumber = g.r.PANNumber,
                            id = g.r.id,
                            Type = g.r.type,
                            RegistrationNumber = g.r.RegistrationNumber,
                            UniqueID = g.r.UniqueID,
                            FirstName = g.r.FirstName,
                            MiddleName = g.r.MiddleName,
                            LastName = g.r.LastName,
                            GenderId = g.r.GenderId,
                            Email = g.r.Email,
                            facultyPhoto = g.r.Photo,
                            Mobile = g.r.Mobile,
                            Absent = g.r.Absent??false,
                            BlacklistFaculty = g.r.Blacklistfaculy,
                            AadhaarNumber = g.r.AadhaarNumber,
                            isActive = g.r.isActive,
                            isApproved = g.r.isApproved,

                        }).ToList();







            //  teachingFaculty.AddRange(data);
            // teachingFaculty = teachingFaculty.OrderBy(f => f.PANNumber).ToList();
            return View(teachingFaculty); //.Where(e => !PANNumbers.Contains(e.PANNumber)).Select(e => e).ToList()
        }

        /// <summary>
        /// PAN Approved View by Admin
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PANApproved(string fid)
        {
            if (!string.IsNullOrEmpty(fid))
            {
                int fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                var Regfacultydata = db.jntuh_registered_faculty.FirstOrDefault(e => e.id == fID);
                if (Regfacultydata != null)
                {
                    Regfacultydata.PanStatus = "EE";
                    Regfacultydata.updatedBy = 1;
                    Regfacultydata.updatedOn = DateTime.Now;
                    db.Entry(Regfacultydata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "PAN Status Updated Successfully";
                    return RedirectToAction("PANDuplicateAssocitedColleges", "OnlineRegistration");
                }
            }
            TempData["Error"] = "PAN Status Updated Failed";
            return RedirectToAction("PANDuplicateAssocitedColleges");
        }

        /// <summary>
        /// PAN Status Given HD view by Admin
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PANHide(string fid)
        {
            if (!string.IsNullOrEmpty(fid))
            {

                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var Regfacultydata = db.jntuh_registered_faculty.FirstOrDefault(e => e.id == fID);
                if (Regfacultydata != null)
                {
                    Regfacultydata.PanStatus = "HD";
                    Regfacultydata.updatedBy = 1;
                    Regfacultydata.updatedOn = DateTime.Now;
                    db.Entry(Regfacultydata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "PAN Status Updated Successfully";
                    return RedirectToAction("PANDuplicateAssocitedColleges", "OnlineRegistration");
                }
            }
            TempData["Error"] = "PAN Status Updated Failed";
            return RedirectToAction("PANDuplicateAssocitedColleges");
        }

        /// <summary>
        /// PAN Status InValid (Not Approved) View by Admin
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult NotApproved(string fid)
        {
            if (!string.IsNullOrEmpty(fid))
            {

                int fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                var Regfacultydata = db.jntuh_registered_faculty.FirstOrDefault(e => e.id == fID);
                if (Regfacultydata != null)
                {
                    Regfacultydata.PanStatus = "ID";
                    Regfacultydata.updatedBy = 1;
                    Regfacultydata.updatedOn = DateTime.Now;
                    db.Entry(Regfacultydata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "PAN Status Updated Successfully";
                    return RedirectToAction("PANDuplicateAssocitedColleges", "OnlineRegistration");
                }
            }
            TempData["Error"] = "PAN Status Updated Failed";
            return RedirectToAction("PANDuplicateAssocitedColleges");
        }

        /// <summary>
        /// PAN Duplicates With Hide Given HD Status code
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PANDuplicatesWithHide()
        {
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
           // var jntuh_college = db.jntuh_college.ToList();
           var PANNumbers =db.jntuh_registered_faculty.Where(e =>e.PanStatus == "HD").Select(e => e.PANNumber).Distinct().ToArray();
           // var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().Where(e => e.PanStatus == "D").Select(e => e).ToList();

            teachingFaculty = db.jntuh_registered_faculty.Join(db.jntuh_college_faculty_registered, r => r.RegistrationNumber, ro => ro.RegistrationNumber, (r, ro) => new { r, ro })
                .Where(e => PANNumbers.Contains(e.r.PANNumber))
                    .Select(g => new FacultyRegistration
                    {
                        //  CollegeName = jntuh_college.Where(c => c.id == g.ro.collegeId).Select(c => c.collegeName).FirstOrDefault(),
                        //   CollegeCode = jntuh_college.Where(c => c.id == g.ro.collegeId).Select(c => c.collegeCode).FirstOrDefault(),
                        CollegeId = g.ro.collegeId,
                        PANNumber = g.r.PANNumber,
                        id = g.r.id,
                        Type = g.r.type,
                        RegistrationNumber = g.r.RegistrationNumber,
                        UniqueID = g.r.UniqueID,
                        FirstName = g.r.FirstName,
                        MiddleName = g.r.MiddleName,
                        LastName = g.r.LastName,
                        GenderId = g.r.GenderId,
                        Email = g.r.Email,
                        facultyPhoto = g.r.Photo,
                        Mobile = g.r.Mobile,
                        Absent = g.r.Absent??false,
                        BlacklistFaculty = g.r.Blacklistfaculy,
                        AadhaarNumber = g.r.AadhaarNumber,
                        isActive = g.r.isActive,
                        isApproved = g.r.isApproved
                    }).ToList();
            return View(teachingFaculty);
        }


        /// <summary>
        /// Know Faculty Registration Number
        /// </summary>
        /// <param name="PANNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult KnowRegistrationNumber(string PANNumber)
        {
            List<KnowyourRegistrationNumbers> FacultyData=new List<KnowyourRegistrationNumbers>();

            if (!string.IsNullOrEmpty(PANNumber))
            {
                FacultyData = db.jntuh_registered_faculty.Where(e => e.PANNumber.Trim() == PANNumber.Trim()).Select(e => new KnowyourRegistrationNumbers
                 {
                     RegistrationNumber = e.RegistrationNumber,
                     FirstName = e.FirstName,
                     Lastname = e.LastName,
                     MiddleName = e.MiddleName,
                 }).ToList();
            }
            return PartialView("_KnowRegisrationNumber", FacultyData);
        }


       /// <summary>
        /// Phd Faculty Uploading Phd Document Getting 
       /// </summary>
       /// <param name="UserName"></param>
       /// <param name="Password"></param>
       /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult PhdFacultyUploadingPhdDocument(string UserName, string Password)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            var EmailUserName = UAAAS.Models.Utilities.DecryptString(UserName, WebConfigurationManager.AppSettings["CryptoKey"]);
            var MobilePassword = UAAAS.Models.Utilities.DecryptString(Password, WebConfigurationManager.AppSettings["CryptoKey"]);
            if (!string.IsNullOrEmpty(EmailUserName) && !string.IsNullOrEmpty(Password))
            {
                var faculty = db.jntuh_registered_faculty.Where(e => e.Email == EmailUserName && e.Mobile.Substring(3, e.Mobile.Length - 1) == MobilePassword.Substring(3, MobilePassword.Length-1)).Select(e => e).FirstOrDefault();
                if (faculty != null)
                {
                        regFaculty.id = faculty.id;
                        regFaculty.Type = faculty.type;
                        //int facultyUserId = jntuh_registered_faculty.Where(e => e.id == regFaculty.id).Select(e => e.UserId).FirstOrDefault();
                        //regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == facultyUserId).Select(u => u.name).FirstOrDefault();
                        regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                        regFaculty.UniqueID = faculty.UniqueID;
                        regFaculty.FirstName = faculty.FirstName;
                        regFaculty.MiddleName = faculty.MiddleName;
                        regFaculty.LastName = faculty.LastName;
                        regFaculty.GenderId = faculty.GenderId;
                        regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                        regFaculty.MotherName = faculty.MotherName;
                        if (faculty.DateOfBirth != null)
                            regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                        regFaculty.OrganizationName = faculty.OrganizationName;
                        regFaculty.DesignationId = faculty.DesignationId;
                        regFaculty.DepartmentId = faculty.DepartmentId;
                        regFaculty.CollegeId = faculty.collegeId;
                        regFaculty.WorkingStatus = faculty.WorkingStatus;
                        regFaculty.OtherDepartment = faculty.OtherDepartment;
                        regFaculty.OtherDesignation = faculty.OtherDesignation;
                        if (faculty.DateOfAppointment != null)
                            regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                        regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                        if (faculty.DateOfRatification != null)
                            regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                        regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                        regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                        regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                        regFaculty.GrossSalary = faculty.grosssalary;
                        regFaculty.TotalExperience = faculty.TotalExperience;
                        regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                        regFaculty.PANNumber = faculty.PANNumber;
                        regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                        regFaculty.Email = faculty.Email;
                        regFaculty.Mobile = faculty.Mobile;
                        regFaculty.National = faculty.National;
                        regFaculty.InterNational = faculty.InterNational;
                        regFaculty.Citation = faculty.Citation;
                        regFaculty.Awards = faculty.Awards;
                        regFaculty.facultyPhoto = faculty.Photo;
                        regFaculty.facultyPANCardDocument = faculty.PANDocument;
                        regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                        regFaculty.isActive = faculty.isActive;
                        regFaculty.isApproved = faculty.isApproved;
                        regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                        regFaculty.isView = true;
                        regFaculty.BlacklistFaculty = faculty.Blacklistfaculy;
                        regFaculty.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus ?? false;
                        regFaculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocument;
                        regFaculty.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;
                        if (faculty.DesignationId != null)
                        {
                            regFaculty.designation = db.jntuh_designation.Where(e => e.id == faculty.DesignationId).Select(e => e.designation).FirstOrDefault();
                        }

                        var collegeData = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                        if (collegeData != null)
                        {
                            ViewBag.CollegeName = db.jntuh_college.Where(e => e.id == collegeData.collegeId).Select(e => e.collegeName).FirstOrDefault();
                            ViewBag.CollegeCode = db.jntuh_college.Where(e => e.id == collegeData.collegeId).Select(e => e.collegeCode).FirstOrDefault();
                            if (collegeData.DepartmentId != null)
                            {
                                ViewBag.DepartmentName = db.jntuh_department.Where(e => e.id == collegeData.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                            }
                            else
                            {
                                ViewBag.DepartmentName = null;
                            }
                        }
                        else
                        {
                            ViewBag.CollegeName = null;
                            ViewBag.DepartmentName = null;
                            ViewBag.CollegeCode = null;
                        }
                }
            }
            return View(regFaculty);
        }

        /// <summary>
        ///Phd Undertaking Document Uploading 
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public ActionResult UpdatePHD(FacultyRegistration faculty, string command)
        {
            string PhdUndertakingcertificatesPath = "~/Content/Upload/Faculty/PHDUndertaking";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (command == "Update" && faculty.PhdUndertakingDocumentstatus == false && !string.IsNullOrEmpty(faculty.RegistrationNumber) && !string.IsNullOrEmpty(faculty.PhdUndertakingDocumentText))
            {

                var list = db.jntuh_registered_faculty.SingleOrDefault(i => i.id == faculty.id && i.RegistrationNumber == faculty.RegistrationNumber);

                if (list != null)
                {
                    if (faculty.PHDUndertakingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(PhdUndertakingcertificatesPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(PhdUndertakingcertificatesPath));
                        }

                        var ext = Path.GetExtension(faculty.PHDUndertakingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {

                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + list.FirstName.Substring(0, 1) + "-" + list.LastName.Substring(0, 1);

                            faculty.PHDUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PhdUndertakingcertificatesPath), fileName, ext));
                            faculty.PHDUndertakingDocumentView = string.Format("{0}{1}", fileName, ext);

                        }
                    }
                    else if (faculty.PHDUndertakingDocumentView != null)
                    {
                        faculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocumentView;
                    }

                    if (faculty.PhdUndertakingDocumentstatus == false)
                    {
                        list.PhdUndertakingDocumentText = faculty.PhdUndertakingDocumentText;
                        list.PHDUndertakingDocument = faculty.PHDUndertakingDocumentView;
                        list.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus;
                    }

                    list.updatedBy = userID;
                    list.updatedOn = DateTime.Now;
                    db.Entry(list).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Phd Undertaking Remarks updated successfully.";
                }
                else
                {
                    TempData["ERROR"] = "Phd Undertaking Remarks updation failed.";
                }
                // return RedirectToAction("FacultyNew", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
            }
            else if (command == "Update" && faculty.PhdUndertakingDocumentstatus == true && !string.IsNullOrEmpty(faculty.RegistrationNumber) && (faculty.PHDUndertakingDocument != null || !string.IsNullOrEmpty(faculty.PHDUndertakingDocumentView)))
            {
                var list = db.jntuh_registered_faculty.SingleOrDefault(i => i.id == faculty.id && i.RegistrationNumber == faculty.RegistrationNumber);

                if (list != null)
                {

                    if (faculty.PHDUndertakingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(PhdUndertakingcertificatesPath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(PhdUndertakingcertificatesPath));
                        }

                        var ext = Path.GetExtension(faculty.PHDUndertakingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {

                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + list.FirstName.Substring(0, 1) + "-" + list.LastName.Substring(0, 1);

                            faculty.PHDUndertakingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(PhdUndertakingcertificatesPath), fileName, ext));
                            faculty.PHDUndertakingDocumentView = string.Format("{0}{1}", fileName, ext);

                        }
                    }
                    else if (faculty.PHDUndertakingDocumentView != null)
                    {
                        faculty.PHDUndertakingDocumentView = faculty.PHDUndertakingDocumentView;
                    }

                    if (faculty.PhdUndertakingDocumentstatus == true)
                    {
                        list.PHDUndertakingDocument = faculty.PHDUndertakingDocumentView;
                        list.PhdUndertakingDocumentstatus = faculty.PhdUndertakingDocumentstatus;
                    }


                    list.updatedBy = userID;
                    list.updatedOn = DateTime.Now;
                    db.Entry(list).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Phd Undertaking Document updated successfully.";
                }
                else
                {
                    TempData["ERROR"] = "Phd Undertaking Document updation failed.";
                }
            }
            else
            {
                TempData["ERROR"] = "Please Enter All Mandatory Fields.";
            }

            return RedirectToAction("PhdFacultyUploadingPhdDocument", new { UserName = UAAAS.Models.Utilities.EncryptString(faculty.Email, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]), Password = UAAAS.Models.Utilities.EncryptString(faculty.Mobile, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        /// <summary>
        /// Adding Faculty PHD Document Getting Action
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Faculty")]
        public ActionResult AddPhdDocument(string fid)
        {
            int faid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            int[] notRequiredIds ={25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56, 60, 73, 71, 75, 76, 78, 72, 74,77};
            foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;


            jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Where(a => a.id == faid).Select(s => s).FirstOrDefault();
            AddphdDocuments AddphdDocuments = new AddphdDocuments();
            AddphdDocuments.Id = faid;
            if (jntuh_registered_faculty.DepartmentId != null)
            {
                AddphdDocuments.DepartmentId = (int)jntuh_registered_faculty.DepartmentId;
            }
            
           
            AddphdDocuments.RegistrationNumber = jntuh_registered_faculty.RegistrationNumber;
            AddphdDocuments.FirstName = jntuh_registered_faculty.FirstName;
            AddphdDocuments.LastName = jntuh_registered_faculty.LastName;
            AddphdDocuments.Name = jntuh_registered_faculty.FirstName + " " + jntuh_registered_faculty.MiddleName + " " + jntuh_registered_faculty.LastName;
            AddphdDocuments.PhdDocumentName = jntuh_registered_faculty.Others1;
            AddphdDocuments.PhdReason = jntuh_registered_faculty.Others2;
            AddphdDocuments.PhdUndertakingDocumentstatus = true;
            return View(AddphdDocuments);
        }

        /// <summary>
        /// Adding Faculty PHD Document Posting Action
        /// </summary>
        /// <param name="fid"></param>
        /// <returns>Update in registered faculty Table</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Faculty")]
        public ActionResult AddPhdDocument(AddphdDocuments phddocument)
        {
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                string phdDocumentPath = "~/Content/Upload/Faculty/PhdDocument";
                if (phddocument != null)
                {





                    if (!Directory.Exists(Server.MapPath(phdDocumentPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(phdDocumentPath));
                    }
                    if (phddocument.PhdDocumentfile != null)
                    {
                        var ext = Path.GetExtension(phddocument.PhdDocumentfile.FileName);
                        //phddocument.PhdDocumentName == null || phddocument.PhdDocumentName == string.Empty
                        if (String.IsNullOrEmpty(phddocument.PhdDocumentName))
                        {
                            phddocument.PhdDocumentName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                          phddocument.FirstName.Substring(0, 1) + "-" +
                                                          phddocument.LastName.Substring(0, 1) + ext;
                        }
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (
                                System.IO.File.Exists(
                                    Server.MapPath("~/Content/Upload/Faculty/PhdDocument/" + phddocument.PhdDocumentName)))
                            {
                                System.IO.File.Delete(
                                    Server.MapPath("~/Content/Upload/Faculty/PhdDocument/" + phddocument.PhdDocumentName));
                            }
                            phddocument.PhdDocumentfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(phdDocumentPath),
                                phddocument.PhdDocumentName));
                        }
                    }
                   
                    jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Where(r => r.id == phddocument.Id).FirstOrDefault();

                    if (phddocument.PhdUndertakingDocumentstatus == true && (phddocument.PhdDocumentfile != null || phddocument.PhdDocumentName!=null) && phddocument.DepartmentId != null)
                    {
                        if (jntuh_registered_faculty != null)
                        {
                            jntuh_registered_faculty.Others1 = phddocument.PhdDocumentName;
                            jntuh_registered_faculty.DepartmentId = phddocument.DepartmentId != null ? phddocument.DepartmentId : jntuh_registered_faculty.DepartmentId;
                            jntuh_registered_faculty.updatedBy = userID;

                            jntuh_registered_faculty.updatedOn = DateTime.Now;
                            db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                            db.SaveChanges();
                            if (System.IO.File.Exists(Server.MapPath("~/Content/Upload/Faculty/PhdDocument/" + phddocument.PhdDocumentName)))
                            {
                                TempData["SUCCESS"] = "PHD Document is Uploaded Successfully";
                            }
                        }
                    }
                    else if (phddocument.PhdUndertakingDocumentstatus == false && phddocument.PhdReason!=null)
                    {
                        if (jntuh_registered_faculty != null)
                        {
                            jntuh_registered_faculty.Others2 = phddocument.PhdReason;
                            jntuh_registered_faculty.updatedBy = userID;
                            jntuh_registered_faculty.updatedOn = DateTime.Now;
                            db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["SUCCESS"] = "PHD Reason is Uploaded Successfully";
                        }
                    }

                    
                   
                   

                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["SUCCESS"] = "PHD Document is Uploaded Failed";
            }

            string fid = UAAAS.Models.Utilities.EncryptString(phddocument.Id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
        }

        /// <summary>
        /// Mobility Request Getting
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Faculty")]
        public ActionResult MobilityRequest(string fid)
        {
            MobilityRequest MobilityRequest = new MobilityRequest();
            int faid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));


            var CollegeDetails=db.jntuh_college.Where(e => e.isActive == true).Select(e => new {Id = e.id, CollegeName = e.collegeCode + "-" + e.collegeName}).ToList();

            ViewBag.Colleges = CollegeDetails;

            var Departments = db.jntuh_department.Where(e => e.isActive == true).Select(e => new {Id=e.id,DeptName=e.departmentName}).ToList();


            if (faid!=0)
            {
                var CollegeFacultyData = (from a in db.jntuh_registered_faculty
                join b in db.jntuh_college_faculty_registered on a.RegistrationNumber equals b.RegistrationNumber
                where a.id==faid
                select new
                {
                    FacultyName=a.FirstName+" "+a.LastName+" "+a.MiddleName,
                    FacultyId=a.id,
                    FacultyRegistrationNo=a.RegistrationNumber,
                    FacultyCollegeId=b.collegeId,
                    FacultyDateofAppointment=a.DateOfAppointment,
                    FacultyDepartmentId=b.DepartmentId
                }).FirstOrDefault();

                if (CollegeFacultyData != null)
                {
                    MobilityRequest.FacultyName = CollegeFacultyData.FacultyName;
                    MobilityRequest.FacultyId = CollegeFacultyData.FacultyId;
                    MobilityRequest.FacultyRegistration = CollegeFacultyData.FacultyRegistrationNo;
                    MobilityRequest.PresentWorkingCollegeId = CollegeFacultyData.FacultyCollegeId;
                    MobilityRequest.IsPresentlyWorking = false;
                    if (CollegeFacultyData.FacultyCollegeId != 0)
                    {
                        MobilityRequest.PresentWorkingCollege =
                            CollegeDetails.Where(e => e.Id == CollegeFacultyData.FacultyCollegeId)
                                .Select(e => e.CollegeName)
                                .FirstOrDefault();
                    }

                    if (CollegeFacultyData.FacultyDepartmentId != 0)
                    {
                        MobilityRequest.DepartmentName =
                            Departments.Where(e => e.Id == CollegeFacultyData.FacultyDepartmentId)
                                .Select(e => e.DeptName).FirstOrDefault();
                    }

                    MobilityRequest.DeptId = CollegeFacultyData.FacultyDepartmentId??0;
                    MobilityRequest.PreviousDateofappointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(CollegeFacultyData.FacultyDateofAppointment.ToString());
                    //MobilityRequest.Dateofappointment = CollegeFacultyData.FacultyDateofAppointment;

                }


            }
            


            return View(MobilityRequest);
        }

        public ActionResult PA_Preamble()
        {
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true && a.isActive == true)
                    .Select(s => s.actualYear)
                    .FirstOrDefault();
            ViewBag.NextacademicYear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.academicYear).FirstOrDefault();
            return View();
        }
    }
}
