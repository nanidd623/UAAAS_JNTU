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
    public class PA_SanctionedDetailsController : BaseController
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
                return RedirectToAction("View", "PA_SanctionedDetails", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_SanctionedDetails", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    intakesId = ""
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var sanctionedList = db.jntuh_college_jntu_pci_aicte_intakes.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            List<SanctionedDetails> sanDetListObj = new List<SanctionedDetails>();
            foreach (var item in sanctionedList)
            {
                SanctionedDetails sanDetailsObj = new SanctionedDetails();
                sanDetailsObj.CollegeId = item.collegeid;
                sanDetailsObj.IntakesId = item.id;
                sanDetailsObj.AcademicYear = item.acedamicyears;
                sanDetailsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                sanDetailsObj.Department = db.jntuh_department.Where(a => a.id == item.department).Select(a => a.departmentName).FirstOrDefault();
                sanDetailsObj.Specialization = db.jntuh_specialization.Where(a => a.id == item.specialization).Select(a => a.specializationName).FirstOrDefault();
                sanDetailsObj.JNTUHSanctioned = item.jntuh;
                sanDetailsObj.PCISanctioned = item.pci == null ? 0 : item.pci;
                sanDetailsObj.AICTESanctioned = item.aicte;

                sanDetListObj.Add(sanDetailsObj);
            }
            ViewBag.SanctionedDetailsList = sanDetListObj;
            SanctionedDetailsModel sanDetailsmodel = new SanctionedDetailsModel();
            return View(sanDetailsmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string intakesId)
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
                return RedirectToAction("Create", "PA_SanctionedDetails");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_SanctionedDetails");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PSD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_SanctionedDetails");
                }
            }
            var sanctionedList = db.jntuh_college_jntu_pci_aicte_intakes.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            List<SanctionedDetails> sanDetListObj = new List<SanctionedDetails>();
            foreach (var item in sanctionedList)
            {
                SanctionedDetails sanDetailsObj = new SanctionedDetails();
                sanDetailsObj.CollegeId = item.collegeid;
                sanDetailsObj.IntakesId = item.id;
                sanDetailsObj.AcademicYear = item.acedamicyears;
                //sanDetailsObj.AcademicYearId = item.acedamicyears;
                sanDetailsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault();
                sanDetailsObj.Department = db.jntuh_department.Where(a => a.id == item.department).Select(a => a.departmentName).FirstOrDefault();
                sanDetailsObj.Specialization = db.jntuh_specialization.Where(a => a.id == item.specialization).Select(a => a.specializationName).FirstOrDefault();
                sanDetailsObj.JNTUHSanctioned = item.jntuh;
                sanDetailsObj.PCISanctioned = item.pci == null ? 0 : item.pci;
                sanDetailsObj.AICTESanctioned = item.aicte;

                sanDetListObj.Add(sanDetailsObj);
            }
            ViewBag.SanctionedDetailsList = sanDetListObj;

            List<SelectListItem> academicYears = new List<SelectListItem>();
            int instituteEstYear = db.jntuh_college_establishment.Where(e => e.collegeId == userCollegeID).Select(e => e.instituteEstablishedYear).FirstOrDefault();
            for (int i = instituteEstYear; i <= DateTime.Now.Year; i++)
            {
                string next_year = ((i + 1) % 100).ToString();
                if (next_year.ToString().Length == 1)
                {
                    next_year = "0" + next_year;
                }
                academicYears.Add(new SelectListItem { Text = (i + "-" + next_year).ToString(), Value = (i + "-" + next_year).ToString() });
            }
            ViewBag.AcademicYears = academicYears;

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            var requiredDegrees = new[] { 5, 2, 9, 10 };

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id) && requiredDegrees.Contains(d.id)).ToList();

            SanctionedDetailsModel sanDetailsmodel = new SanctionedDetailsModel();
            if (intakesId != null)
            {
                int dec_intakeId = Convert.ToInt32(Utilities.DecryptString(intakesId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var intake = db.jntuh_college_jntu_pci_aicte_intakes.Find(dec_intakeId);
                sanDetailsmodel.CollegeId = intake.collegeid;
                sanDetailsmodel.IntakesId = intake.id;
                sanDetailsmodel.AcademicYearId = intake.acedamicyearid;
                sanDetailsmodel.AcademicYear = intake.acedamicyears;
                sanDetailsmodel.DegreeId = intake.degree;
                sanDetailsmodel.DepartmentId = intake.department;
                sanDetailsmodel.SpecializationId = intake.specialization;
                sanDetailsmodel.JNTUHSanctioned = intake.jntuh;
                sanDetailsmodel.PCISanctioned = intake.pci;
                sanDetailsmodel.AICTESanctioned = Convert.ToInt32(intake.aicte);
            }
            sanDetailsmodel.CollegeId = userCollegeID;
            int userEstablishmentID = db.jntuh_college_establishment.Where(establishment => establishment.collegeId == userCollegeID)
                                                                    .Select(establishment => establishment.id).FirstOrDefault();
            jntuh_college_establishment jntuh_college_establishment = db.jntuh_college_establishment.Find(userEstablishmentID);
            sanDetailsmodel.YearOfEstablishment = jntuh_college_establishment.instituteEstablishedYear;
            return View("Create", sanDetailsmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(SanctionedDetailsModel sanDetailsmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (sanDetailsmodel.IntakesId > 0)
            {
                //Update
                var intake = db.jntuh_college_jntu_pci_aicte_intakes.Where(a => a.id == sanDetailsmodel.IntakesId).Select(a => a).FirstOrDefault();
                intake.acedamicyearid = sanDetailsmodel.AcademicYearId;
                intake.acedamicyears = sanDetailsmodel.AcademicYear;
                intake.collegeid = sanDetailsmodel.CollegeId;
                intake.degree = sanDetailsmodel.DegreeId;
                intake.department = sanDetailsmodel.DepartmentId;
                intake.specialization = sanDetailsmodel.SpecializationId;
                intake.jntuh = Convert.ToInt32(sanDetailsmodel.JNTUHSanctioned);
                intake.pci = sanDetailsmodel.PCISanctioned;
                intake.aicte = sanDetailsmodel.AICTESanctioned;

                intake.updatedon = DateTime.Now;
                intake.updatedby = userID;

                db.Entry(intake).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //Add
                jntuh_college_jntu_pci_aicte_intakes sanctionedIntake = new jntuh_college_jntu_pci_aicte_intakes();
                sanctionedIntake.acedamicyearid = db.jntuh_academic_year.Where(a => a.academicYear == sanDetailsmodel.AcademicYear).Select(a => a.id).FirstOrDefault();
                sanctionedIntake.acedamicyears = sanDetailsmodel.AcademicYear;
                sanctionedIntake.collegeid = sanDetailsmodel.CollegeId;
                sanctionedIntake.degree = sanDetailsmodel.DegreeId;
                sanctionedIntake.department = sanDetailsmodel.DepartmentId;
                sanctionedIntake.specialization = sanDetailsmodel.SpecializationId;
                sanctionedIntake.jntuh = Convert.ToInt32(sanDetailsmodel.JNTUHSanctioned);
                sanctionedIntake.pci = sanDetailsmodel.PCISanctioned;
                sanctionedIntake.aicte = sanDetailsmodel.AICTESanctioned;

                sanctionedIntake.isactive = true;
                sanctionedIntake.createdby = userID;
                sanctionedIntake.createdon = DateTime.Now;

                db.jntuh_college_jntu_pci_aicte_intakes.Add(sanctionedIntake);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            return RedirectToAction("Edit", "PA_SanctionedDetails", new
            {
                collegeId = Utilities.EncryptString(sanDetailsmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string intakesId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_intakeId = Convert.ToInt32(Utilities.DecryptString(intakesId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var intake = db.jntuh_college_jntu_pci_aicte_intakes.Where(a => a.id == dec_intakeId).FirstOrDefault();
            db.Entry(intake).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_SanctionedDetails", new
            {
                collegeId = collegeId
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string id)
        {
            if (Membership.GetUser() == null)
            {
                return Json("login again", JsonRequestBehavior.AllowGet);
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (id == string.Empty)
            {
                id = "0";
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //New Code Return by Narayana Reddy on 05-03-2019.
            int did = Convert.ToInt32(id);
            var departments = (from cs in db.jntuh_approvedadmitted_intake
                               join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where de.id == did && cs.collegeId == userCollegeID
                               select new
                               {
                                   degreeId = de.id,
                                   degree = de.degree,
                                   DepartmentId = d.id,
                                   Department = d.departmentName,
                                   specializationId = s.id,
                                   specialization = s.specializationName
                               }).ToList();

            var departmentList = departments.GroupBy(g => g.DepartmentId).Select(s => s.FirstOrDefault()).ToList();
            var departmentsData = departmentList.Select(a => new SelectListItem()
            {
                Text = a.Department,
                Value = a.DepartmentId.ToString(),
            });
            return Json(departmentsData, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSpecialization(string id)
        {
            if (Membership.GetUser() == null)
            {
                return Json("login again", JsonRequestBehavior.AllowGet);
            }
            if (id == string.Empty)
            {
                id = "0";
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int deid = Convert.ToInt32(id);
            var secializationList = (from cs in db.jntuh_approvedadmitted_intake
                                     join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                                     join d in db.jntuh_department on s.departmentId equals d.id
                                     join de in db.jntuh_degree on d.degreeId equals de.id
                                     where d.id == deid && cs.collegeId == userCollegeID
                                     select new
                                     {
                                         degreeId = de.id,
                                         degree = de.degree,
                                         DepartmentId = d.id,
                                         Department = d.departmentName,
                                         specializationId = s.id,
                                         specialization = s.specializationName
                                     }).ToList();
            secializationList =
                secializationList.GroupBy(g => g.specializationId).Select(e => e.FirstOrDefault()).ToList();
            var specializationdata = secializationList.Select(s => new SelectListItem()
            {
                Text = s.specialization,
                Value = s.specializationId.ToString(),
            });
            return Json(specializationdata, JsonRequestBehavior.AllowGet);
        }
    }

    public class SanctionedDetails
    {
        public int CollegeId { get; set; }

        public int IntakesId { get; set; }

        public int AcademicYearId { get; set; }

        public string AcademicYear { get; set; }

        public string Degree { get; set; }

        public string Department { get; set; }

        public string Specialization { get; set; }

        public int JNTUHSanctioned { get; set; }

        public int? PCISanctioned { get; set; }

        public int? AICTESanctioned { get; set; }
    }
}
