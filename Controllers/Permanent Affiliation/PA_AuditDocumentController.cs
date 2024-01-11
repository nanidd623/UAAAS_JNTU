using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using UAAAS.Models.Permanent_Affiliation;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_AuditDocumentController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_AuditDocument", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_AuditDocument", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }

            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            var finStandsList = db.jntuh_college_financialstandards.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            finStandsList = finStandsList.AsEnumerable().GroupBy(f => new { f.academicyear }).Select(f => f.First()).ToList();
            List<AuditDocument> auditDocumentListObj = new List<AuditDocument>();
            foreach (var item in finStandsList)
            {
                AuditDocument auditDocumentObj = new AuditDocument();
                auditDocumentObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                auditDocumentObj.AuditDocumentPath = item.auditdoc;

                auditDocumentListObj.Add(auditDocumentObj);
            }
            ViewBag.AuditDocumentsList = auditDocumentListObj;
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "PA_AuditDocument");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_AuditDocument");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PAD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_AuditDocument");
                }
            }
            var finStandsList = db.jntuh_college_financialstandards.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            finStandsList = finStandsList.AsEnumerable().GroupBy(f => new { f.academicyear }).Select(f => f.First()).ToList();
            List<AuditDocument> auditDocumentListObj = new List<AuditDocument>();
            foreach (var item in finStandsList)
            {
                AuditDocument auditDocumentObj = new AuditDocument();
                auditDocumentObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                auditDocumentObj.AuditDocumentPath = item.auditdoc;

                auditDocumentListObj.Add(auditDocumentObj);
            }
            ViewBag.AuditDocumentsList = auditDocumentListObj;
            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            FinancialStandardsModel financialStandardsModel = new FinancialStandardsModel();
            financialStandardsModel.CollegeId = userCollegeID;
            return View("Create", financialStandardsModel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(FinancialStandardsModel financialStandardsModel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            //Update
            var financialstandards = db.jntuh_college_financialstandards.Where(f => f.academicyear == financialStandardsModel.AcademicYear && f.collegeid == userCollegeID && f.isactive == true).ToList();

            if (financialStandardsModel.AuditDocument != null)
            {
                string auditedFile = "~/Content/Upload/College/FinancialStandards/AuditedDocuments";
                if (!Directory.Exists(Server.MapPath(auditedFile)))
                {
                    Directory.CreateDirectory(Server.MapPath(auditedFile));
                }
                var ext = Path.GetExtension(financialStandardsModel.AuditDocument.FileName);
                if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                {
                    if (financialStandardsModel.AuditDocumentPath == null)
                    {
                        string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                        financialStandardsModel.AuditDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(auditedFile), fileName, ext));
                        financialStandardsModel.AuditDocumentPath = string.Format("{0}{1}", fileName, ext);
                    }
                    else
                    {
                        financialStandardsModel.AuditDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(auditedFile), financialStandardsModel.AuditDocumentPath));
                    }
                }
            }

            foreach (var item in financialstandards) 
            {
                item.auditdoc = financialStandardsModel.AuditDocumentPath;
                item.updatedby = userID;
                item.updatedon = DateTime.Now;

                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }

            TempData["Success"] = "Added successfully";
            return RedirectToAction("Edit", "PA_AuditDocument", new
            {
                collegeId = Utilities.EncryptString(financialStandardsModel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            });
        }
    }

    public class AuditDocument
    {
        public string AcademicYear { get; set; }

        public string AuditDocumentPath { get; set; }
    }
}
