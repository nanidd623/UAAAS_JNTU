using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.Data;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeLegalSoftwarController : BaseController
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
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(d => d.degreeId).ToArray();
            int existId = db.jntuh_college_legal_software.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID > 0 && existId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeLegalSoftwar");
            }

            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID > 0 && existId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeLegalSoftwar", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (existId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLegalSoftwar");
            }

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegeLegalSoftwarDetails> collegeLegalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                           join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                           orderby Degree.degreeDisplayOrder
                                                                           where (Degree.isActive == true)
                                                                           select new CollegeLegalSoftwarDetails
                                                                           {
                                                                               degreeId = CollegeDegree,
                                                                               degree = Degree.degree,
                                                                               availableApplicationSoftware = 0,
                                                                               availableSystemSoftware = 0,
                                                                               collegeId = userCollegeID
                                                                           }).ToList();

            ViewBag.Count = collegeLegalSoftwarDetails.Count();
            return View(collegeLegalSoftwarDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegeLegalSoftwarDetails> collegeLegalSoftwarDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeLegalSoftwarDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveCollegeLegalSoftwarDetails(collegeLegalSoftwarDetails);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                    join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                    orderby Degree.degreeDisplayOrder
                                                                    where (Degree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree,
                                                                        degree = Degree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0,
                                                                        collegeId = userCollegeID
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }

            ViewBag.Count = legalSoftwarDetails.Count();
            return View(legalSoftwarDetails);
        }

        private void SaveCollegeLegalSoftwarDetails(ICollection<CollegeLegalSoftwarDetails> collegeLegalSoftwarDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            
            if (userCollegeID == 0)
            {
                foreach (var item in collegeLegalSoftwarDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (CollegeLegalSoftwarDetails item in collegeLegalSoftwarDetails)
                {
                    jntuh_college_legal_software details = new jntuh_college_legal_software();
                    details.degreeId = item.degreeId;
                    details.collegeId = userCollegeID;
                    details.availableSystemSoftware = item.availableSystemSoftware;
                    details.availableApplicationSoftware = item.availableApplicationSoftware;
                    int existId = db.jntuh_college_legal_software.Where(l => l.collegeId == userCollegeID && l.degreeId == item.degreeId).Select(l => l.id).FirstOrDefault();
                    if (existId == 0)
                    {
                        details.createdBy = userID;
                        details.createdOn = DateTime.Now;
                        db.jntuh_college_legal_software.Add(details);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        details.id = existId;
                        details.createdOn = db.jntuh_college_legal_software.Where(d => d.id == existId).Select(d => d.createdOn).FirstOrDefault();
                        details.createdBy = db.jntuh_college_legal_software.Where(d => d.id == existId).Select(d => d.createdBy).FirstOrDefault();
                        details.updatedBy = userID;
                        details.updatedOn = DateTime.Now;
                        db.Entry(details).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Update";
                    }
                }
                if (message == "Update")
                {
                    TempData["Success"] = "College Legal Software Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "College Legal Software Details are Added successfully";
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
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
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(Degree => Degree.degreeId).ToArray();
            int ExistId = db.jntuh_college_legal_software.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (ExistId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeLegalSoftwar");
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeLegalSoftwar");
            }
            else
            {

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeLegalSoftwar");
                }
            }

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                    join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                    orderby Degree.degreeDisplayOrder
                                                                    where (Degree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree,
                                                                        degree = Degree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0,
                                                                        collegeId = userCollegeID
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }
            ViewBag.Update = true;
            ViewBag.Count = legalSoftwarDetails.Count();
            return View("Create", legalSoftwarDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegeLegalSoftwarDetails> collegeLegalSoftwarDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            
            if (userCollegeID == 0)
            {
                foreach (var item in collegeLegalSoftwarDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SaveCollegeLegalSoftwarDetails(collegeLegalSoftwarDetails);

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                    join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                    orderby Degree.degreeDisplayOrder
                                                                    where (Degree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree,
                                                                        degree = Degree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0,
                                                                        collegeId = userCollegeID
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }
            ViewBag.Update = true;
            ViewBag.Count = legalSoftwarDetails.Count();
            return View("View");
            // return View("Create", legalSoftwarDetails);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(Degree => Degree.degreeId).ToArray();
            int ExistId = db.jntuh_college_legal_software.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                    join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                    orderby Degree.degree
                                                                    where (Degree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree,
                                                                        degree = Degree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }
            if (ExistId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {

                ViewBag.Update = true;
                ViewBag.Count = legalSoftwarDetails.Count();
            }
            return View("View", legalSoftwarDetails);
        }

        public ActionResult UserView(string id)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(Degree => Degree.degreeId).ToArray();
            int ExistId = db.jntuh_college_legal_software.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in DegreeIds
                                                                    join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                    orderby Degree.degree
                                                                    where (Degree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree,
                                                                        degree = Degree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == userCollegeID &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }
            if (ExistId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Update = true;
                ViewBag.Count = legalSoftwarDetails.Count();
            }
            return View("UserView", legalSoftwarDetails);
        }
    }
}
