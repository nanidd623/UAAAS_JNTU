using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    public class PA_FacultyInformationController : BaseController
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
                return RedirectToAction("View", "PA_FacultyInformation", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_FacultyInformation", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    uploadsId = ""
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var facultyList = db.jntuh_college_faculty_information.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();
            List<FacultyInfo> facultyListObj = new List<FacultyInfo>();
            foreach (var item in facultyList)
            {
                FacultyInfo facultyInfoObj = new FacultyInfo();
                facultyInfoObj.FacultyInfoId = item.id;
                facultyInfoObj.CollegeId = item.collegeid;
                facultyInfoObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                facultyInfoObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                facultyInfoObj.Staff = item.staff;
                facultyInfoObj.Students = item.student;
                facultyInfoObj.FacultyOnRole = item.numberoffacultyonrole;
                facultyInfoObj.FacultyTerminatedResigned = item.numberoffacultyterminatedorregined;
                facultyInfoObj.PayscaleSelected = item.payscaleimplemented ? "Yes" : "No";
                facultyInfoObj.AssiProfPayScale = item.asstprofpayscale;
                facultyInfoObj.AssiProfPay = item.asstprofpay;
                facultyInfoObj.AssoProfPayScale = item.assocprofpayscale;
                facultyInfoObj.AssoProfPay = item.assocprofpay;
                facultyInfoObj.ProfPayScale = item.profpayscale;
                facultyInfoObj.ProfPay = item.profpays;
                facultyInfoObj.RetentionPercentage = Convert.ToString(item.retentionpercentage);
                facultyInfoObj.Oppurtunities = Convert.ToBoolean(item.oppurtunities) ? "Yes" : "No";

                facultyListObj.Add(facultyInfoObj);
            }
            ViewBag.FacultyInformationList = facultyListObj;
            FacultyInformationModel facInfomodel = new FacultyInformationModel();
            return View(facInfomodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string facultyInfoId)
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
                return RedirectToAction("Create", "PA_FacultyInformation");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_FacultyInformation");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PFI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_FacultyInformation");
                }
            }
            var facultyList = db.jntuh_college_faculty_information.Where(c => c.collegeid == userCollegeID && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();
            List<FacultyInfo> facultyListObj = new List<FacultyInfo>();
            foreach (var item in facultyList)
            {
                FacultyInfo facultyInfoObj = new FacultyInfo();
                facultyInfoObj.FacultyInfoId = item.id;
                facultyInfoObj.CollegeId = item.collegeid;
                facultyInfoObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault();
                facultyInfoObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                facultyInfoObj.Staff = item.staff;
                facultyInfoObj.Students = item.student;
                facultyInfoObj.FacultyOnRole = item.numberoffacultyonrole;
                facultyInfoObj.FacultyTerminatedResigned = item.numberoffacultyterminatedorregined;
                facultyInfoObj.PayscaleSelected = item.payscaleimplemented ? "Yes" : "No";
                facultyInfoObj.AssiProfPayScale = item.asstprofpayscale;
                facultyInfoObj.AssiProfPay = item.asstprofpay;
                facultyInfoObj.AssoProfPayScale = item.assocprofpayscale;
                facultyInfoObj.AssoProfPay = item.assocprofpay;
                facultyInfoObj.ProfPayScale = item.profpayscale;
                facultyInfoObj.ProfPay = item.profpays;
                facultyInfoObj.RetentionPercentage = Convert.ToString(item.retentionpercentage);
                facultyInfoObj.Oppurtunities = Convert.ToBoolean(item.oppurtunities) ? "Yes" : "No";

                facultyListObj.Add(facultyInfoObj);
            }
            ViewBag.FacultyInformationList = facultyListObj;

            List<SelectListItem> payScales = new List<SelectListItem>();

            payScales.Add(new SelectListItem { Text = "AICTE Pay scale V", Value = "AICTE Pay scale V" });
            payScales.Add(new SelectListItem { Text = "AICTE Pay scale VI", Value = "AICTE Pay scale VI" });
            payScales.Add(new SelectListItem { Text = "AICTE Pay scale VII", Value = "AICTE Pay scale VII" });
            payScales.Add(new SelectListItem { Text = "Others", Value = "Others" });

            ViewBag.PayScales = payScales;

            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            FacultyInformationModel facInfomodel = new FacultyInformationModel();
            if (facultyInfoId != null)
            {
                int dec_facultyInfoId = Convert.ToInt32(Utilities.DecryptString(facultyInfoId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var facultyInfo = db.jntuh_college_faculty_information.Find(dec_facultyInfoId);
                facInfomodel.FacultyInfoId = facultyInfo.id;
                facInfomodel.AcademicYearId = facultyInfo.academicyear;
                facInfomodel.CollegeId = facultyInfo.collegeid;
                facInfomodel.DegreeID = facultyInfo.degree;
                facInfomodel.Staff = Convert.ToString(facultyInfo.staff);
                facInfomodel.Students = Convert.ToString(facultyInfo.student);
                facInfomodel.FacultyOnRole = Convert.ToString(facultyInfo.numberoffacultyonrole);
                facInfomodel.FacultyTerminatedResigned = Convert.ToString(facultyInfo.numberoffacultyterminatedorregined);
                facInfomodel.PayscaleSelected = facultyInfo.payscaleimplemented ? 1 : 2;
                facInfomodel.AssiProfPayScale = facultyInfo.asstprofpayscale;
                facInfomodel.AssoProfPayScale = facultyInfo.assocprofpayscale;
                facInfomodel.ProfPayScale = facultyInfo.profpayscale;
                facInfomodel.Oppurtunities = Convert.ToBoolean(facultyInfo.oppurtunities) ? 1 : 2;
            }
            facInfomodel.CollegeId = userCollegeID;
            return View("Create", facInfomodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(FacultyInformationModel facInfomodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            //Add
            jntuh_college_faculty_information facultyInfo = new jntuh_college_faculty_information();
            facultyInfo.academicyear = facInfomodel.AcademicYearId;
            facultyInfo.collegeid = facInfomodel.CollegeId;
            facultyInfo.degree = facInfomodel.DegreeID;
            facultyInfo.staff = Convert.ToInt32(facInfomodel.Staff);
            facultyInfo.student = Convert.ToInt32(facInfomodel.Students);
            facultyInfo.numberoffacultyonrole = Convert.ToInt32(facInfomodel.FacultyOnRole);
            facultyInfo.numberoffacultyterminatedorregined = Convert.ToInt32(facInfomodel.FacultyTerminatedResigned);
            facultyInfo.payscaleimplemented = facInfomodel.PayscaleSelected == 1 ? true : false;

            if (facInfomodel.PayscaleSelected == 1)
            {
                facultyInfo.asstprofpayscale = facInfomodel.AssiProfPayScale;
                facultyInfo.asstprofpay = facInfomodel.AssiProfPay;

                facultyInfo.assocprofpayscale = facInfomodel.AssoProfPayScale;
                facultyInfo.assocprofpay = facInfomodel.AssoProfPay;

                facultyInfo.profpayscale = facInfomodel.ProfPayScale;
                facultyInfo.profpays = facInfomodel.ProfPay;
            }
            facultyInfo.retentionpercentage = Convert.ToDecimal(facInfomodel.RetentionPercentage);
            facultyInfo.oppurtunities = facInfomodel.Oppurtunities == 1 ? true : false;
            facultyInfo.isactive = true;
            facultyInfo.createdby = userID;
            facultyInfo.createdon = DateTime.Now;

            db.jntuh_college_faculty_information.Add(facultyInfo);
            db.SaveChanges();

            TempData["Success"] = "Added successfully";
            return RedirectToAction("Edit", "PA_FacultyInformation", new
            {
                collegeId = Utilities.EncryptString(facInfomodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),

                facultyInfoId = ""
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string facultyInfoId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_facultyInfoId = Convert.ToInt32(Utilities.DecryptString(facultyInfoId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var facultyInfo = db.jntuh_college_faculty_information.Where(a => a.id == dec_facultyInfoId).FirstOrDefault();
            db.Entry(facultyInfo).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_FacultyInformation", new
            {
                collegeId = collegeId,
                facultyInfoId = ""
            });
        }
    }

    public class FacultyInfo
    {
        public int FacultyInfoId { get; set; }

        public int CollegeId { get; set; }

        public string AcademicYear { get; set; }

        public string Degree { get; set; }

        public int Staff { get; set; }

        public int Students { get; set; }

        public int FacultyOnRole { get; set; }

        public int FacultyTerminatedResigned { get; set; }

        public string PayscaleSelected { get; set; }

        public string AssiProfPayScale { get; set; }

        public string AssiProfPay { get; set; }

        public string AssoProfPayScale { get; set; }

        public string AssoProfPay { get; set; }

        public string ProfPayScale { get; set; }

        public string ProfPay { get; set; }

        public string RetentionPercentage { get; set; }

        public string Oppurtunities { get; set; }
    }
}
