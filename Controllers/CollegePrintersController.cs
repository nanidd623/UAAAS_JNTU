using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.Security;
using UAAAS.Models;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegePrintersController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

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
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int existId = db.jntuh_college_computer_lab_printers.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
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
            if (userCollegeID > 0 && existId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegePrinters");
            }
            if (userCollegeID > 0 && existId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegePrinters", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (existId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegePrinters");
            }

            int academicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            List<CollegePrinterDetails> collegePrinterDetails = (from CollegeDegree in DegreeIds
                                                                 join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                 orderby Degree.degree
                                                                 where (Degree.isActive == true)
                                                                 select new CollegePrinterDetails
                                                                     {
                                                                         degreeId = CollegeDegree,
                                                                         degree = Degree.degree,
                                                                         availableComputers = 0,
                                                                         availablePrinters = 0,
                                                                         collegeId = userCollegeID
                                                                     }).ToList();
            foreach (var item in collegePrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
            }
            ViewBag.Count = collegePrinterDetails.Count();
            return View(collegePrinterDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegePrinterDetails> collegePrinterDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePrinterDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            SavePrintersDetails(collegePrinterDetails);

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in DegreeIds
                                                          join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                          orderby Degree.degree
                                                          where (Degree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0,
                                                              collegeId = userCollegeID
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }

            ViewBag.Count = PrinterDetails.Count();
            return View(PrinterDetails);
        }

        private void SavePrintersDetails(ICollection<CollegePrinterDetails> collegePrinterDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePrinterDetails)
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
                foreach (CollegePrinterDetails item in collegePrinterDetails)
                {
                    jntuh_college_computer_lab_printers details = new jntuh_college_computer_lab_printers();
                    details.degreeId = item.degreeId;
                    details.collegeId = userCollegeID;
                    details.availablePrinters = item.availablePrinters;
                    int existId = db.jntuh_college_computer_lab_printers.Where(l => l.collegeId == userCollegeID && l.degreeId == item.degreeId).Select(l => l.id).FirstOrDefault();
                    if (existId == 0)
                    {
                        details.createdBy = userID;
                        details.createdOn = DateTime.Now;
                        db.jntuh_college_computer_lab_printers.Add(details);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        details.id = existId;
                        details.createdOn = db.jntuh_college_computer_lab_printers.Where(d => d.id == existId).Select(d => d.createdOn).FirstOrDefault();
                        details.createdBy = db.jntuh_college_computer_lab_printers.Where(d => d.id == existId).Select(d => d.createdBy).FirstOrDefault();
                        details.updatedBy = userID;
                        details.updatedOn = DateTime.Now;
                        db.Entry(details).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Update";
                    }
                }
                if (message == "Update")
                {
                    TempData["Success"] = "College Printers Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "College Printers Details are Added successfully";
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
            int ExistId = db.jntuh_college_computer_lab_printers.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (ExistId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegePrinters");
            }
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
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegePrinters");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegePrinters");
                }
            }

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in DegreeIds
                                                          join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                          orderby Degree.degree
                                                          where (Degree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0,
                                                              collegeId = userCollegeID
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }
            ViewBag.Update = true;
            ViewBag.Count = PrinterDetails.Count();
            return View("Create", PrinterDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegePrinterDetails> collegePrinterDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePrinterDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SavePrintersDetails(collegePrinterDetails);

            var cSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                    .Select(s => s.specializationId)
                    .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in DegreeIds
                                                          join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                          orderby Degree.degree
                                                          where (Degree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0,
                                                              collegeId = userCollegeID
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }
            ViewBag.Update = true;
            ViewBag.Count = PrinterDetails.Count();
            return View("View");
            // Changed by Naushad Khan
            // return View("Create",PrinterDetails);
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
            int ExistId = db.jntuh_college_computer_lab_printers.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
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
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PS") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in DegreeIds
                                                          join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                          orderby Degree.degree
                                                          where (Degree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }
            if (ExistId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Update = true;
                ViewBag.Count = PrinterDetails.Count();
            }
            return View("View", PrinterDetails);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(Degree => Degree.degreeId).ToArray();
            int ExistId = db.jntuh_college_computer_lab_printers.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();
            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in DegreeIds
                                                          join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                          orderby Degree.degree
                                                          where (Degree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }
            if (ExistId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Update = true;
                ViewBag.Count = PrinterDetails.Count();
            }
            return View("UserView", PrinterDetails);
        }
    }
}
