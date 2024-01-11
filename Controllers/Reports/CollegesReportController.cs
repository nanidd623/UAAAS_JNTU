using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
   [ErrorHandling]
    
    public class CollegesReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CollegesReport()
        {
            CollegesReport collegesReport = new CollegesReport();
            CollegesDetails(collegesReport);
            return View("~/Views/Reports/CollegesReport.cshtml", collegesReport);
        }
        public class CollegeEditStatus
        {
            public int collegestatusId { get; set; }
            public string name { get; set; }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CollegesReport(CollegesReport colleges, string cmd)
        {
            CollegesReport collegesReport = new CollegesReport();
            List<CollegesReport> collegesList = CollegesDetails(colleges);
            ViewBag.collegesList = collegesList;
            int count = collegesList.Count();
            if (cmd == "Export" && count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Colleges.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_CollegesReport.cshtml");
            }
            if (count == 0)
            {
                TempData["Error"] = "No records found.";
            }
            return View("~/Views/Reports/CollegesReport.cshtml", collegesReport);
        }

        private List<Models.CollegesReport> CollegesDetails(CollegesReport colleges)
        {
            int districtId = colleges.districtId;
            int collegeTypeId = colleges.collegeTypeId;
            int collegeEditStatus = colleges.collegestatusId;
            bool collegeStatus = false;
            if (collegeEditStatus != 0)
            {
                if (collegeEditStatus == 1)
                    collegeStatus = true;
                else
                    collegeStatus = false;
            }

            var actualYear1 =
                   db.jntuh_academic_year.Where(q => q.isActive == true && q.isPresentAcademicYear == true)
               .Select(a => a.actualYear)
               .FirstOrDefault();
            var AcademicYearId1 =
                db.jntuh_academic_year.Where(d => d.actualYear == (actualYear1 + 1)).Select(z => z.id).FirstOrDefault();

            List<CollegesReport> collegesList = new List<CollegesReport>();
            collegesList = (from c in db.jntuh_college
                            join a in db.jntuh_address on c.id equals a.collegeId //&& a.addressTye == "College"
                            join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id
                            join cs in db.jntuh_college_edit_status on c.id equals cs.collegeId
                            where (cs.academicyearId == AcademicYearId1 && c.isActive == true && ct.isActive == true && a.addressTye == "College")
                            select new CollegesReport
                            {
                                collegeid = c.id,
                                collegeCode = c.collegeCode,
                                collegeName = c.collegeName,
                                address = a.address,
                                townorCity = a.townOrCity,
                                pincode = a.pincode,
                                landline = a.landline,
                                mobile = a.mobile,
                                email = a.email,
                                collegeTypeId = c.collegeTypeID,
                                collegeType = ct.collegeType,
                                districtId = a.districtId,
                                isCollegeEditable = cs.IsCollegeEditable
                            }).ToList();


            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId != 0)
            {
                collegesList = collegesList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId == 0)
            {
                collegesList = collegesList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId != 0)
            {
                collegesList = collegesList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId == 0)
            {
                collegesList = collegesList.Where(cl => cl.isCollegeEditable == collegeStatus).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId != 0)
            {
                collegesList = collegesList.Where(cl => cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId == 0)
            {
                collegesList = collegesList.Where(cl => cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId != 0)
            {
                collegesList = collegesList.Where(cl => cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId == 0)
            {
                collegesList = collegesList.ToList();
            }
            ViewBag.collegesReport = collegesList;
            List<CollegeEditStatus> collegeEditStatuslist = new List<CollegeEditStatus>(){                
                new CollegeEditStatus{ collegestatusId = 1, name = "Pending"},
                new CollegeEditStatus{ collegestatusId = 2, name = "Submitted"}};
            ViewBag.CollegeEditStatus = collegeEditStatuslist.ToList();
            ViewBag.Districts = db.jntuh_district.Where(d => d.isActive == true).Select(d => d).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.Where(cs => cs.isActive == true).Select(cs => cs).ToList();
            return collegesList;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult SendMailToCollege(string id)
        {
            int collegeId = 0;
            if (id != null)
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            CollegesReport collegesReport = new CollegesReport();
            collegesReport = (from a in db.jntuh_address
                              where (a.collegeId == collegeId && a.addressTye == "College")
                              select new CollegesReport
                              {
                                  collegeid = a.collegeId,
                                  email = a.email,
                                  mobile = a.mobile
                              }).FirstOrDefault();

            return PartialView("~/Views/Reports/SendMailToCollege.cshtml", collegesReport);
        }

        [HttpPost, ValidateInput(false)]
        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
       // [ValidateInput(false)]
        public ActionResult SendMailToCollege(CollegesReport colleges, HttpPostedFileBase fileUploader)
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
            string email = colleges.email;
            if (email == "")
            {
                string code = db.jntuh_college.AsNoTracking().Where(c => c.id == colleges.collegeid).Select(c => c.collegeCode).FirstOrDefault();
                //Commented on 18-06-2018 by Narayana Reddy
                //email = db.all_college_emails.AsNoTracking().Where(e => e.Code == code).Select(e => e.Email).FirstOrDefault();
            }
            int userId = db.jntuh_college_users.Where(cu => cu.collegeID == colleges.collegeid).Select(cu => cu.userID).FirstOrDefault();
            string username = db.my_aspnet_users.Find(userId).name;
            string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
            string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();
            List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
            if (fileUploader != null)
            {
                attachments.Add(new System.Net.Mail.Attachment(filepath));
            }
            IUserMailer mailer = new UserMailer();
            if (email != string.Empty && colleges.subject != null && colleges.message != null)
            {
                mailer.SendMailToCollege(email, strCc, strBcc, colleges.subject, username, colleges.message, attachments).SendAsync();
                //mailer.SendMailToCollege(email, "chandrashekarreddy.k@csstechnergy.com", "chandrashekarreddy.k@csstechnergy.com", colleges.subject, username, colleges.message, attachments).SendAsync();               
               
            }
            
            //Sending sms to mobile
            string strmobileno = colleges.mobile;
            string pMessage = colleges.smstext;
            if (colleges.mobile!=null && colleges.smstext != null)
            {
                bool pStatus = UAAAS.Models.Utilities.SendSms(strmobileno, pMessage);
            }
            TempData["Success"] = "Message sent successfully";
            return RedirectToAction("CollegesReport");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult EmailsSendingByExcel()
        {
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
       
        public ActionResult EmailsSendingByExcel(FormCollection model)
        {
          
            return RedirectToAction("EmailsSendingByExcel");
        }


    }
}
