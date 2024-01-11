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
    public class PA_FinancialStandardsController : BaseController
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
                return RedirectToAction("View", "PA_FinancialStandards", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_FinancialStandards", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }

            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId, string finStandId)
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var finStandsList = db.jntuh_college_financialstandards.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();
            List<FinancialStandards> finStandsListObj = new List<FinancialStandards>();
            foreach (var item in finStandsList)
            {
                FinancialStandards finStandsObj = new FinancialStandards();
                finStandsObj.FinStandId = item.id;
                finStandsObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                finStandsObj.CollegeId = item.collegeid;
                finStandsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                finStandsObj.NumberOfStudents = item.noofstudents;
                finStandsObj.TotalAmount = item.totalamount;
                finStandsObj.ScholarshipDocumentPath = item.scholarshipdoc;

                finStandsListObj.Add(finStandsObj);
            }
            ViewBag.FinancialStandardsList = finStandsListObj;
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string finStandId)
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
                return RedirectToAction("Create", "PA_FinancialStandards");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_FinancialStandards");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_FinancialStandards");
                }
            }
            var finStandsList = db.jntuh_college_financialstandards.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();
            List<FinancialStandards> finStandsListObj = new List<FinancialStandards>();
            foreach (var item in finStandsList)
            {
                FinancialStandards finStandsObj = new FinancialStandards();
                finStandsObj.FinStandId = item.id;
                finStandsObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                finStandsObj.CollegeId = item.collegeid;
                finStandsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                finStandsObj.NumberOfStudents = item.noofstudents;
                finStandsObj.TotalAmount = item.totalamount;
                finStandsObj.ScholarshipDocumentPath = item.scholarshipdoc;

                finStandsListObj.Add(finStandsObj);
            }
            ViewBag.FinancialStandardsList = finStandsListObj;
            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            FinancialStandardsModel financialStandardsModel = new FinancialStandardsModel();
            if (finStandId != null)
            {
                int dec_finStandId = Convert.ToInt32(Utilities.DecryptString(finStandId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var finStand = db.jntuh_college_financialstandards.Find(dec_finStandId);
                financialStandardsModel.CollegeId = finStand.collegeid;
                financialStandardsModel.FinStandId = finStand.id;
                financialStandardsModel.AcademicYear = finStand.academicyear;
                financialStandardsModel.Degree = finStand.degree;
                financialStandardsModel.NumberOfStudents = Convert.ToString(finStand.noofstudents);
                financialStandardsModel.TotalAmount = Convert.ToString(finStand.totalamount);
                financialStandardsModel.ScholarshipDocumentPath = finStand.scholarshipdoc;
            }
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

            //Add
            jntuh_college_financialstandards financialstandards = new jntuh_college_financialstandards();
            financialstandards.academicyear = financialStandardsModel.AcademicYear;
            financialstandards.collegeid = financialStandardsModel.CollegeId;
            financialstandards.degree = financialStandardsModel.Degree;
            financialstandards.noofstudents = Convert.ToInt32(financialStandardsModel.NumberOfStudents);
            financialstandards.totalamount = Convert.ToDecimal(financialStandardsModel.TotalAmount);

            if (financialStandardsModel.ScholarshipDocument != null)
            {
                string scholarshipFile = "~/Content/Upload/College/FinancialStandards/Scholarship";
                if (!Directory.Exists(Server.MapPath(scholarshipFile)))
                {
                    Directory.CreateDirectory(Server.MapPath(scholarshipFile));
                }
                var ext = Path.GetExtension(financialStandardsModel.ScholarshipDocument.FileName);
                if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                {
                    if (financialStandardsModel.ScholarshipDocumentPath == null)
                    {
                        string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                        financialStandardsModel.ScholarshipDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(scholarshipFile), fileName, ext));
                        financialStandardsModel.ScholarshipDocumentPath = string.Format("{0}{1}", fileName, ext);
                        financialstandards.scholarshipdoc = financialStandardsModel.ScholarshipDocumentPath;
                    }
                    else
                    {
                        financialStandardsModel.ScholarshipDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(scholarshipFile), financialStandardsModel.ScholarshipDocumentPath));
                        financialstandards.scholarshipdoc = financialStandardsModel.ScholarshipDocumentPath;
                    }
                }
            }
            else
            {
                financialstandards.scholarshipdoc = financialStandardsModel.ScholarshipDocumentPath;
            }

            financialstandards.isactive = true;
            financialstandards.createdby = userID;
            financialstandards.createdon = DateTime.Now;

            db.jntuh_college_financialstandards.Add(financialstandards);
            db.SaveChanges();

            TempData["Success"] = "Added successfully";
            return RedirectToAction("Edit", "PA_FinancialStandards", new
            {
                collegeId = Utilities.EncryptString(financialStandardsModel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            });
            //return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string finStandId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_finStandId = Convert.ToInt32(Utilities.DecryptString(finStandId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var jntuh_college_financialstandard = db.jntuh_college_financialstandards.Where(a => a.id == dec_finStandId).FirstOrDefault();
            db.Entry(jntuh_college_financialstandard).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_FinancialStandards", new
            {
                collegeId = collegeId
            });
        }
    }

    public class FinancialStandards
    {
        public int FinStandId { get; set; }

        public int CollegeId { get; set; }

        public string AcademicYear { get; set; }

        public string Degree { get; set; }

        public int NumberOfStudents { get; set; }

        public decimal TotalAmount { get; set; }

        public string ScholarshipDocumentPath { get; set; }
    }
}
