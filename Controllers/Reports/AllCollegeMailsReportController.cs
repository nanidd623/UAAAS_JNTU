using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.IO.Compression;


namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AllCollegeMailsReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AllCollegeMails()
        {
            CollegeMails CollegeMails = new CollegeMails();
            List<AllCollegeMails> allCollegeMails = (from c in db.jntuh_college
                                                     join a in db.jntuh_address on c.id equals a.collegeId into t
                                                     from rt in t.Where(ad => ad.addressTye == "College").DefaultIfEmpty()
                                                     where (c.isActive == true)
                                                     orderby (c.collegeName.ToUpper().Trim())
                                                     select new AllCollegeMails
                                                                  {
                                                                      collegeId = c.id,
                                                                      collegeCode = c.collegeCode,
                                                                      collegeName = c.collegeName,
                                                                      collegeTypeId = c.collegeTypeID,
                                                                      email = rt.email,
                                                                      mobileNo = rt.mobile,
                                                                      isSelect = false
                                                                  }).ToList();
            CollegeMails.collegeMails = allCollegeMails;
            ViewBag.Count = allCollegeMails.Count;
            return View("~/Views/Reports/AllCollegeMails.cshtml", CollegeMails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult AllCollegeMails(CollegeMails allCollegeMails, HttpPostedFileBase fileUploader)
        {
            string fileName = string.Empty;
            var filepath = string.Empty;
            if (fileUploader != null)
            {
                fileName = Path.GetFileName(fileUploader.FileName);
                filepath = Path.Combine(Server.MapPath("~/Content/Attachments"), fileName);
                FileInfo f = new FileInfo(filepath);
                if (f.Exists)
                {
                    f.Delete();
                }
                fileUploader.SaveAs(filepath);
            }
            if (allCollegeMails.collegeMails.Count() != 0)
            {
                foreach (var item in allCollegeMails.collegeMails.Where(s => s.isSelect == true).ToList())
                {
                    if (item.isSelect == true && item.email != string.Empty)
                    {
                        //send email to the selected colleges 
                        //-------------------
                        string strTo = string.Empty;
                        string strSubject = string.Empty;
                        string strMessage = string.Empty;

                        strTo = item.email;
                        string strCc = string.Empty; //ConfigurationManager.AppSettings["EmailCC"].ToString();
                        string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();
                        if (allCollegeMails.Subject != null)
                        {
                            strSubject = allCollegeMails.Subject;
                        }
                        if (allCollegeMails.Message != null)
                        {
                            strMessage = allCollegeMails.Message;
                        }
                        List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                        if (fileUploader != null)
                        {
                            attachments.Add(new System.Net.Mail.Attachment(filepath));
                        }
                        IUserMailer mailer = new UserMailer();
                        if (strTo != string.Empty && strSubject != string.Empty && strMessage != string.Empty)
                        {
                            mailer.SendAttachmentToAllColleges(strTo, strCc, strBcc, strSubject, strMessage, attachments).SendAsync();
                            //mailer.SendAttachmentToAllColleges(strTo, "chandrashekarreddy.k@csstechnergy.com", "chandrashekarreddy.k@csstechnergy.com", strSubject, strMessage, attachments).SendAsync();
                        }

                        //Sending sms to mobile
                        string strmobileno = item.mobileNo;
                        string pMessage = allCollegeMails.SMS;
                        if (item.mobileNo != null && allCollegeMails.SMS != null)
                        {
                            bool pStatus = UAAAS.Models.Utilities.SendSms(strmobileno, pMessage);
                        }
                    }
                }
            }
            TempData["Success"] = "Message sent successfully";
            return RedirectToAction("AllCollegeMails");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult SelectedCollegeMails()
        {
            TempData["Show"] = null;
            CollegeMails CollegeMails = new CollegeMails();
            var actualYear =
                db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            var AcademicYearId =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
            List<AllCollegeMails> allCollegeMails = (from c in db.jntuh_college
                                                     join s in db.jntuh_college_edit_status on c.id equals s.collegeId
                                                     join a in db.jntuh_address on c.id equals a.collegeId into t
                                                     from rt in t.Where(ad => ad.addressTye == "College").DefaultIfEmpty()
                                                     where (s.academicyearId == AcademicYearId && c.isActive == true && s.IsCollegeEditable == false)
                                                     orderby (c.collegeCode.ToUpper().Trim())
                                                     select new AllCollegeMails
                                                     {
                                                         collegeId = c.id,
                                                         collegeCode = c.collegeCode,
                                                         collegeName = c.collegeName,
                                                         collegeTypeId = c.collegeTypeID,
                                                         email = rt.email,
                                                         mobileNo = rt.mobile,
                                                         isSelect = false
                                                     }).ToList();
            CollegeMails.collegeMails = allCollegeMails;
            ViewBag.Count = allCollegeMails.Count;
            return View(CollegeMails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SelectedCollegeMailsNew(CollegeMails objlist)
        {
            if (objlist.collegeMails.Count() != 0)
            {
                CollegeMails obj = new CollegeMails();
                obj.collegeMails = objlist.collegeMails.Where(s => s.isSelect == true).ToList();
                TempData["Show"] = true;
                return View("~/Views/Reports/SelectedCollegeMailsNew.cshtml", obj);
            }
            else
            {
                return View("~/Views/Reports/SelectedCollegeMailsNew.cshtml");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FinalEmailSend(CollegeMails objlist, HttpPostedFileBase fileUploader, string command)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            string fileName = string.Empty;
            var filepath = string.Empty;
            if (fileUploader != null)
            {
                fileName = Path.GetFileName(fileUploader.FileName);
                filepath = Path.Combine(Server.MapPath("~/Content/Upload/UploadFileToUsers"), fileName);
                FileInfo f = new FileInfo(filepath);
                if (f.Exists)
                {
                    f.Delete();
                }
                fileUploader.SaveAs(filepath);
            }

            if (command == "Send")
            {
                TempData["Show"] = null;
                string EmailID = ConfigurationManager.AppSettings["DuaacMail"].ToString();
                string password = ConfigurationManager.AppSettings["DuaacPwd"].ToString();

                var jntuh_address = db.jntuh_address.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

                var Collegecodes = objlist.collegeMails.Select(e => e.collegeCode).Distinct().ToList();

                //string EmailID = "patnala.shiva@syntizen.com";
                //string password = "9989810356";

                string bodytext1 = " <div style='text-align:center;padding-left:190px;width:63%;'>";
                bodytext1 += "<table width=100%>";
                bodytext1 += "<tr>";
                bodytext1 += "<td width=15%>";
                bodytext1 += @"<img src=""http://jntuhaac.in//Content/Images/new_logo.jpg"" alt=""Logo"" style='width:65px;height:65px;'/></td>";
                bodytext1 += "<td width=70%><b style='color:brown;'>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD<br>DIRECTORATE OF AFFILIATIONS & ACADEMIC AUDIT <br>Kukatpally, Hyderabad – 500 085, Telangana, India</b></td>";
                bodytext1 += "<td width=15%>";
                bodytext1 += @"<img src=""http://jntuhaac.in//Content/Images/NAAC1.png"" alt='Logo' style='width:65px;height:65px;'/></td>";
                bodytext1 += "</tr>";
                bodytext1 += "</table>";
                bodytext1 += "</div><hr/>";
                bodytext1 += "<div style='background-color:papayawhip;padding:10px;border:2px solid brown;'>";

                string ChahgeMessage = string.Empty;
                ChahgeMessage = objlist.Message.Replace("\r\n", "<br/>");
                bodytext1 += ChahgeMessage;

                bodytext1 += "</div>";


                foreach (var item in Collegecodes)
                {
                    int i = Collegecodes.IndexOf(item);
                    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                    mail.From = new MailAddress(EmailID);

                    if (filepath == "" || filepath == null)
                    {

                    }
                    else
                    {
                        mail.Attachments.Add(new System.Net.Mail.Attachment(filepath));
                    }

                    mail.Subject = objlist.Subject;
                    mail.Body = bodytext1;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    mail.Headers.Add("Disposition-Notification-To", EmailID);
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    NetworkCredential networkCredential = new NetworkCredential(EmailID, password);
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = networkCredential;
                    smtp.Port = 587;


                    int count = 1;
                    string ToEmails = string.Empty;
                    string strmobileno = string.Empty;
                    var id = jntuh_college.Where(s => s.collegeCode == item).Select(e => e.id).FirstOrDefault();
                    var Collegedata = jntuh_address.Where(s => s.collegeId == id).Select(e => e).ToList();

                    foreach (var item1 in Collegedata)
                    {

                        if (count == Collegedata.Count())
                        {
                            string PrinicipalRegNo = db.jntuh_college_principal_registered.Where(r => r.collegeId == item1.collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();

                            if (PrinicipalRegNo != null)
                            {
                                var PrinicipalData = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == PrinicipalRegNo).Select(e => e).FirstOrDefault();
                                if (PrinicipalData == null)
                                {
                                    ToEmails += item1.email;
                                    strmobileno = null;
                                    string pMessage = objlist.SMS;
                                    if (strmobileno != null && objlist.SMS != null)
                                    {
                                        bool pStatus = UAAAS.Models.Utilities.SendSMS(strmobileno, pMessage);
                                    }
                                    strmobileno = string.Empty;
                                }
                                else
                                {
                                    ToEmails += item1.email + "," + PrinicipalData.Email;
                                    strmobileno = PrinicipalData.Mobile;
                                    string pMessage = objlist.SMS;
                                    if (strmobileno != null && objlist.SMS != null)
                                    {
                                        bool pStatus = UAAAS.Models.Utilities.SendSMS(strmobileno, pMessage);
                                    }
                                    strmobileno = string.Empty;
                                }

                            }
                            else
                            {
                                ToEmails += item1.email;
                                strmobileno = item1.mobile;
                                string pMessage = objlist.SMS;
                                if (strmobileno != null && objlist.SMS != null)
                                {
                                    bool pStatus = UAAAS.Models.Utilities.SendSMS(strmobileno, pMessage);
                                }
                                strmobileno = string.Empty;
                            }

                        }
                        else
                        {
                            ToEmails += item1.email + ",";
                            strmobileno = item1.mobile;
                            string pMessage = objlist.SMS;
                            if (strmobileno != null && objlist.SMS != null)
                            {
                                bool pStatus = UAAAS.Models.Utilities.SendSMS(strmobileno, pMessage);
                            }
                            strmobileno = string.Empty;
                        }

                        count++;
                    }

                    mail.To.Add(ToEmails);

                    try
                    {
                        smtp.Send(mail);

                        try
                        {
                            if (objlist.Select == true)
                            {
                                jntuh_college_news news = new jntuh_college_news();
                                news.collegeId = id;
                                news.title = objlist.Subject;
                                news.navigateURL = null;
                                news.startDate = null;
                                news.endDate = null;
                                news.isActive = true;
                                news.isLatest = true;
                                news.createdBy = userid;
                                news.createdOn = DateTime.Now;
                                db.jntuh_college_news.Add(news);
                                db.SaveChanges();
                            }
                            else
                            {

                            }
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }

                        TempData["Success"] = "Mail sent suceessfully";
                    }
                    catch
                    {
                        TempData["Failure"] = "Mail not sent suceessfully";
                        continue;
                    }
                }

                CollegeMails obj = new CollegeMails();
                obj.collegeMails = objlist.collegeMails.Where(s => s.isSelect == true).ToList();
                TempData["Show"] = true;
                return View("~/Views/Reports/SelectedCollegeMailsNew.cshtml", objlist);
            }
            else if (command == "Print")
            {
                if (objlist.collegeMails.Count() != 0)
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename = CollegeMailsData.xls");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/Reports/_MailCollegeDataNew.cshtml", objlist);
                }
                else
                {
                    return View("~/Views/Reports/SelectedCollegeMailsNew.cshtml", objlist);
                }
            }
            else
            {
                return View("~/Views/Reports/SelectedCollegeMailsNew.cshtml", objlist);
            }

        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult BulkEmails()
        {
            return View();
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult BulkEmails(HttpPostedFileBase[] fileUploader)
        //{
        //var uploads = Server.MapPath("~/Content/Upload/Emails");
        //using (ZipArchive archive = new ZipArchive(fileUploader.InputStream))
        //{
        //    foreach (ZipArchiveEntry entry in archive.Entries)
        //    {
        //        if (!string.IsNullOrEmpty(Path.GetExtension(entry.FullName))) //make sure it's not a folder
        //        {
        //            entry.ExtractToFile(Path.Combine(uploads, entry.FullName));
        //        }
        //        else
        //        {
        //            Directory.CreateDirectory(Path.Combine(uploads, entry.FullName));
        //        }
        //    }
        //}
        //ViewBag.Files = Directory.EnumerateFiles(uploads);
        //    return null;
        //}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult BulkEmails(CollegeBulkMails emails)
        {
            CollegeBulkMails collegeBulkMails = new CollegeBulkMails();

            if (ModelState.IsValid)
            {   //iterating through multiple file collection 
                string PrincipalRrgitrationnumber = "";
                string InputFileName = "";
                //string[] InputFileNamedata ;
                string collegecode = "";
                string MobileNumbers = "";
                string mobile = "";
                var collegesdata = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var collegeAddress = db.jntuh_address.ToList();
                var PrincipalsData = db.jntuh_college_principal_registered.ToList();
                string[] Principals = PrincipalsData.Select(p => p.RegistrationNumber).ToArray();
                var FacultyDetails = db.jntuh_registered_faculty.Where(rf => Principals.Contains(rf.RegistrationNumber)).ToList();
                List<BulkCollegeMails> BulkCollegeMailsData = new List<BulkCollegeMails>();
                ViewBag.Subect = emails.Subject;
                ViewBag.Message = emails.Message;
                foreach (HttpPostedFileBase file in emails.files)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        BulkCollegeMails BulkeMails = new BulkCollegeMails();
                        InputFileName = "";
                        //InputFileNamedata = new string[];
                        PrincipalRrgitrationnumber = "";
                        InputFileName = Path.GetFileName(file.FileName);
                        string[] InputFileNamedata = InputFileName.Split('_');
                        collegecode = "";
                        MobileNumbers = "";
                        

                        BulkeMails.Collegeemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "COLLEGE").Select(c => c.email).FirstOrDefault();
                        BulkeMails.Secretaryemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SECRETARY").Select(c => c.email).FirstOrDefault();
                        BulkeMails.Societyemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SOCIETY").Select(c => c.email).FirstOrDefault();

                        //BulkeMails.collegeId = collegesdata.Where(c => c.collegeCode == file.FileName.Substring(0,2)).Select(c => c.id).FirstOrDefault();
                        BulkeMails.collegeCode = file.FileName.Substring(0,2);
                        BulkeMails.collegeName = collegesdata.Where(c => c.collegeCode == file.FileName.Substring(0,2)).Select(c => c.collegeName).FirstOrDefault();
                        BulkeMails.collegeId = collegesdata.Where(c => c.collegeCode == file.FileName.Substring(0,2)).Select(c => c.id).FirstOrDefault();
                        BulkeMails.Collegeemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "COLLEGE").Select(c => c.email).FirstOrDefault();
                        BulkeMails.Secretaryemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SECRETARY").Select(c => c.email).FirstOrDefault();
                        BulkeMails.Societyemail = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SOCIETY").Select(c => c.email).FirstOrDefault();
                        PrincipalRrgitrationnumber = PrincipalsData.Where(p => p.collegeId == BulkeMails.collegeId).Select(p => p.RegistrationNumber).FirstOrDefault();
                        if (!string.IsNullOrEmpty(PrincipalRrgitrationnumber))
                            BulkeMails.principalemail = FacultyDetails.Where(rf => rf.RegistrationNumber == PrincipalRrgitrationnumber).Select(rf => rf.Email).FirstOrDefault();
                        BulkeMails.FileName = InputFileName;
                        mobile = FacultyDetails.Where(rf => rf.RegistrationNumber == PrincipalRrgitrationnumber).Select(rf => rf.Mobile).FirstOrDefault();
                        if (!string.IsNullOrEmpty(mobile))
                        {
                            if (!string.IsNullOrEmpty(MobileNumbers))
                                MobileNumbers += "," + FacultyDetails.Where(rf => rf.RegistrationNumber == PrincipalRrgitrationnumber).Select(rf => rf.Mobile).FirstOrDefault();
                            else
                                MobileNumbers += FacultyDetails.Where(rf => rf.RegistrationNumber == PrincipalRrgitrationnumber).Select(rf => rf.Mobile).FirstOrDefault();
                        }
                        mobile = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "COLLEGE").Select(c => c.mobile).FirstOrDefault();
                        if (!string.IsNullOrEmpty(mobile))
                        {
                            MobileNumbers = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "COLLEGE").Select(c => c.mobile).FirstOrDefault();
                        }
                        mobile = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SECRETARY").Select(c => c.mobile).FirstOrDefault();
                        if (!string.IsNullOrEmpty(mobile))
                        {
                            if (!string.IsNullOrEmpty(MobileNumbers))
                                MobileNumbers += "," + collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SECRETARY").Select(c => c.mobile).FirstOrDefault();
                            else
                                MobileNumbers += collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SECRETARY").Select(c => c.mobile).FirstOrDefault();

                        }
                        mobile = collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SOCIETY").Select(c => c.mobile).FirstOrDefault();
                        if (!string.IsNullOrEmpty(mobile))
                        {
                            if (!string.IsNullOrEmpty(MobileNumbers))
                                MobileNumbers += "," + collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SOCIETY").Select(c => c.mobile).FirstOrDefault();
                            else
                                MobileNumbers += collegeAddress.Where(c => c.collegeId == BulkeMails.collegeId && c.addressTye == "SOCIETY").Select(c => c.mobile).FirstOrDefault();
                        }

                        BulkeMails.Mobile = MobileNumbers;
                        var ServerSavePath = Path.Combine(Server.MapPath("~/Content/Upload/Emails/") + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);
                        //assigning file uploaded status to ViewBag for showing message to user.  
                        ViewBag.UploadStatus = BulkCollegeMailsData.Count().ToString() + " files uploaded successfully.";
                        BulkCollegeMailsData.Add(BulkeMails);
                        
                        
                    }

                }
                
                collegeBulkMails.Subject = emails.Subject;
                collegeBulkMails.Message = emails.Message;
                collegeBulkMails.SMS = emails.SMS;
                collegeBulkMails.Select = emails.Select;
                collegeBulkMails.Bulkemails = BulkCollegeMailsData;
            }
            TempData["BulkCollegeMailsData"] = collegeBulkMails;
            //Response.ClearContent();
            //Response.Buffer = true;
            //Response.AddHeader("content-disposition", "attachment; filename=College Emails.XLS");
            //Response.ContentType = "application/vnd.ms-excel";
            //return PartialView("~/Views/AllCollegeMailsReport/Collegeemaildata.cshtml", collegeBulkMails.Bulkemails.OrderBy(s => s.collegeName).ToList());

            return View(collegeBulkMails);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult BulkEmailssending()
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //CollegeBulkMails collegeBulkMailsdata = new CollegeBulkMails();
            var temp = TempData["BulkCollegeMailsData"];
            var collegeBulkMailsdata = TempData["BulkCollegeMailsData"] as CollegeBulkMails;
            IUserMailer mailer = new UserMailer();
            string Emails = "";
            string strCc = string.Empty;
            string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();
            string filepath = string.Empty;
            List<BulkCollegeMails> BulkCollegeMailsList = new List<BulkCollegeMails>();
            foreach (var item in (collegeBulkMailsdata.Bulkemails))
            {
                BulkCollegeMails bulkCollegeMails=new  BulkCollegeMails();
                bulkCollegeMails.collegeCode = item.collegeCode;
                bulkCollegeMails.collegeName = item.collegeName;
                bulkCollegeMails.Collegeemail = item.Collegeemail;
                bulkCollegeMails.Societyemail = item.Societyemail;
                bulkCollegeMails.Secretaryemail = item.Secretaryemail;
                bulkCollegeMails.principalemail = item.principalemail;
                bulkCollegeMails.FileName = item.FileName;

                BulkCollegeMailsList.Add(bulkCollegeMails);
                if (!string.IsNullOrEmpty(item.Collegeemail) && string.IsNullOrEmpty(Emails))
                {
                    Emails = item.Collegeemail;
                }
                if ((!string.IsNullOrEmpty(item.Societyemail)))
                {
                    if (!string.IsNullOrEmpty(Emails))
                        Emails += "," + item.Societyemail;
                    else
                        Emails = "," + item.Societyemail;
                }
                if ((!string.IsNullOrEmpty(item.Secretaryemail)))
                {
                    if (!string.IsNullOrEmpty(Emails))
                        Emails += "," + item.Secretaryemail;
                    else
                        Emails = "," + item.Secretaryemail;
                }
                if ((!string.IsNullOrEmpty(item.principalemail)))
                {
                    if (!string.IsNullOrEmpty(Emails))
                        Emails += "," + item.principalemail;
                    else
                        Emails = "," + item.principalemail;
                }
                List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                filepath = Path.Combine(Server.MapPath("~/Content/Upload/Emails/"), item.FileName);
                if (item.FileName != null)
                {
                    attachments.Add(new System.Net.Mail.Attachment(filepath));
                }
                mailer.SendAttachmentToAllColleges(Emails, strCc, strBcc, collegeBulkMailsdata.Subject, collegeBulkMailsdata.Message.Replace("\r\n", "<br>"), attachments).SendAsync();
                Emails = "";
                if (item.Mobile != null && !string.IsNullOrEmpty(collegeBulkMailsdata.SMS))
                {
                    bool pStatus = UAAAS.Models.Utilities.SendSms(item.Mobile, collegeBulkMailsdata.SMS);
                }
                if (collegeBulkMailsdata.Select == true)
                {
                    jntuh_college_news news = new jntuh_college_news();
                    news.collegeId = item.collegeId;
                    news.title = collegeBulkMailsdata.Subject;
                    news.navigateURL = "/Content/Upload/Emails/" + item.FileName; 
                    news.startDate = null;
                    news.endDate = null;
                    news.isActive = true;
                    news.isLatest = true;
                    news.createdBy = userid;
                    news.createdOn = DateTime.Now;
                    db.jntuh_college_news.Add(news);
                    db.SaveChanges();
                }
            }
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=College Emails.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/AllCollegeMailsReport/Collegeemaildata.cshtml", BulkCollegeMailsList.OrderBy(s => s.collegeName).ToList());
            return RedirectToAction("BulkEmails");
        }
    }

}
