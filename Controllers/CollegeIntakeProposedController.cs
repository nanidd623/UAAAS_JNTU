using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeIntakeProposedController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeIntakeProposed/

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            //We are using another duplicate Action Result for Admin Please verify the Below action
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.NextAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                     a.actualYear == (presentYear + 1))
                                                         .Select(a => a.academicYear).FirstOrDefault();

            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(l => l.isActive == true && l.isPresentAcademicYear == true).Select(l => l.academicYear).FirstOrDefault();

           

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.collegeId = item.collegeId;
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                newProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == userCollegeID &&
                                                                             ei.academicYearId == academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
                collegeIntakeProposedList.Add(newProposed);
            }

            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            ViewBag.Count = collegeIntakeProposedList.Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (collegeIntakeProposedList.Count() == 0 && status == 0)
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeIntakeProposedList.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeIntakeProposed");
            }
            return View(collegeIntakeProposedList);
        }

        private List<jntuh_department> Departments(int id)
        {
            int[] otherdepartments = {60,61,65,66,67,68,71,72,73,74,75,76,77,78,29,30,31,32,33,34};
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id && !otherdepartments.Contains(d.id)).OrderBy(d => d.departmentName).ToList();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Index");
            }
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.NextAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                     a.actualYear == (presentYear + 1))
                                                         .Select(a => a.academicYear).FirstOrDefault();

            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(l => l.isActive == true && l.isPresentAcademicYear == true).Select(l => l.academicYear).FirstOrDefault();

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.collegeId = item.collegeId;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                newProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == userCollegeID &&
                                                                             ei.academicYearId == academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
                collegeIntakeProposedList.Add(newProposed);
            }
            
            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            ViewBag.IntakeExisting = collegeIntakeProposedList;
            ViewBag.Count = collegeIntakeProposedList.Count();
            return View("View", collegeIntakeProposedList);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var DepartmentList = this.Departments(Convert.ToInt32(id));

            var DepartmentsData = DepartmentList.Select(a => new SelectListItem()
            {
                Text = a.departmentName,
                Value = a.id.ToString(),
            });
            return Json(DepartmentsData, JsonRequestBehavior.AllowGet);
        }

        private List<jntuh_department> ColgDepartments(int id)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();

            var DepartmentsDatas = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
            var deps = db.jntuh_department.Where(e => e.isActive == true && DepartmentsDatas.Contains(e.id)).ToList();
            int[] otherdepartments = { 60, 61, 65, 66, 67, 68, 71, 72, 73, 74, 75, 76, 77, 78, 29, 30, 31, 32, 33, 34 };
            return deps.Where(d => d.isActive == true && d.degreeId == id && !otherdepartments.Contains(d.id)).OrderBy(d => d.departmentName).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetCollegeDepartments(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var DepartmentList = this.ColgDepartments(Convert.ToInt32(id));

            var DepartmentsData = DepartmentList.Select(a => new SelectListItem()
            {
                Text = a.departmentName,
                Value = a.id.ToString(),
            });
            return Json(DepartmentsData, JsonRequestBehavior.AllowGet);
        }

        private List<jntuh_specialization> Specializations(int id)
        {
            return db.jntuh_specialization.Where(s => s.isActive == true && s.departmentId == id).OrderBy(s => s.specializationName).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSpecialization(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var SecializationList = this.Specializations(Convert.ToInt32(id));
            var Specializationdata = SecializationList.Select(s => new SelectListItem()
            {
                Text = s.specializationName,
                Value = s.id.ToString(),
            });
            return Json(Specializationdata, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            //We are using another duplicate Action Result for Admin Please verify the Below action
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_intake_proposed.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();
                    collegeIntakeProposed.collegeId = userCollegeID;
                    int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                               year.isPresentAcademicYear == true)
                                                              .Select(year => year.actualYear).FirstOrDefault();
                    ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).ToList();
                    ViewBag.Degree = degrees.OrderBy(d => d.degree);
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                    ViewBag.CourseAffiliationstatusText = db.jntuh_course_affiliation_status.Where(t => t.isActive == true).Select(t => t.courseAffiliationStatusDescription);

                  

                    List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

                    //List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

                    foreach (var item in proposed)
                    {
                        collegeIntakeProposed.id = item.id;
                        collegeIntakeProposed.collegeId = item.collegeId;
                        collegeIntakeProposed.academicYearId = item.academicYearId;
                        collegeIntakeProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                        collegeIntakeProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                        collegeIntakeProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == collegeIntakeProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                        collegeIntakeProposed.proposedIntake = item.proposedIntake;
                        collegeIntakeProposed.specializationId = item.specializationId;
                        collegeIntakeProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeIntakeProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        collegeIntakeProposed.Department = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                        collegeIntakeProposed.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                        collegeIntakeProposed.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                        collegeIntakeProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                        collegeIntakeProposed.shiftId = item.shiftId;
                        collegeIntakeProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                                     ei.shiftId == item.shiftId &&
                                                                                     ei.collegeId == userCollegeID &&
                                                                                     ei.academicYearId == item.academicYearId)
                                                                                .Select(ei => ei.approvedIntake).FirstOrDefault();
                    }
                    return PartialView("_Create", collegeIntakeProposed);
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();
                    collegeIntakeProposed.collegeId = userCollegeID;
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                     {
                                                                         collegeDegree.degreeId,
                                                                         collegeDegree.collegeId,
                                                                         collegeDegree.isActive,
                                                                         degree.degree
                                                                     })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.degree
                                                                 }).ToList();
                    ViewBag.Degree = degrees.OrderBy(d => d.degree);
                    ViewBag.Count = degrees.Count();
                    int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                               year.isPresentAcademicYear == true)
                                                              .Select(year => year.actualYear).FirstOrDefault();
                    ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                    ViewBag.CourseAffiliationstatusText = db.jntuh_course_affiliation_status.Where(t => t.isActive == true).Select(t => t.courseAffiliationStatusDescription);
                    return PartialView("_Create", collegeIntakeProposed);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();
                    collegeIntakeProposed.collegeId = userCollegeID;
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).ToList();
                    ViewBag.Degree = degrees.OrderBy(d => d.degree);
                    ViewBag.Count = degrees.Count();
                    int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                               year.isPresentAcademicYear == true)
                                                              .Select(year => year.actualYear).FirstOrDefault();
                    ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                    ViewBag.CourseAffiliationstatusText = db.jntuh_course_affiliation_status.Where(t => t.isActive == true).Select(t => t.courseAffiliationStatusDescription);

                  
                    List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

                    //List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

                    foreach (var item in proposed)
                    {
                        collegeIntakeProposed.id = item.id;
                        collegeIntakeProposed.collegeId = item.collegeId;
                        collegeIntakeProposed.academicYearId = item.academicYearId;
                        collegeIntakeProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                        collegeIntakeProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                        collegeIntakeProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == collegeIntakeProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                        collegeIntakeProposed.proposedIntake = item.proposedIntake;
                        collegeIntakeProposed.specializationId = item.specializationId;
                        collegeIntakeProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeIntakeProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        collegeIntakeProposed.Department = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                        collegeIntakeProposed.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                        collegeIntakeProposed.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                        collegeIntakeProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                        collegeIntakeProposed.shiftId = item.shiftId;
                        collegeIntakeProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                                     ei.shiftId == item.shiftId &&
                                                                                     ei.collegeId == userCollegeID &&
                                                                                     ei.academicYearId == item.academicYearId)
                                                                                .Select(ei => ei.approvedIntake).FirstOrDefault();
                    }
                    return View("Create", collegeIntakeProposed);
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();
                    collegeIntakeProposed.collegeId = userCollegeID;
                    int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                               year.isPresentAcademicYear == true)
                                                              .Select(year => year.actualYear).FirstOrDefault();
                    ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                  (collegeDegree, degree) => new
                                                                  {
                                                                      collegeDegree.degreeId,
                                                                      collegeDegree.collegeId,
                                                                      collegeDegree.isActive,
                                                                      degree.degree
                                                                  })
                                                              .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                              .Select(collegeDegree => new
                                                              {
                                                                  collegeDegree.degreeId,
                                                                  collegeDegree.degree
                                                              }).ToList();
                    ViewBag.Degree = degrees.OrderBy(d => d.degree);
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                    ViewBag.CourseAffiliationstatusText = db.jntuh_course_affiliation_status.Where(t => t.isActive == true).Select(t => t.courseAffiliationStatusDescription);
                    return View("Create", collegeIntakeProposed);
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegeIntakeProposed collegeIntakeProposed, string cmd)
        {
            //We are using another duplicate Action Result for Admin Please verify the Below action
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeIntakeProposed.collegeId;
            }
            if (ModelState.IsValid)
            {
                collegeIntakeProposed.collegeId = userCollegeID;
                if (cmd == "Add")
                {
                    var Id = db.jntuh_college_intake_proposed.Where(p => p.specializationId == collegeIntakeProposed.specializationId &&
                                                                 p.shiftId == collegeIntakeProposed.shiftId && p.collegeId == collegeIntakeProposed.collegeId).Select(p => p.id).FirstOrDefault();
                    if (Id > 0)
                    {
                        TempData["IntakeError"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                        ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).OrderBy(d => d.degree).ToList();
                        ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                        ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                        ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                        ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_intake_proposed jntuh_college_intake_proposed = new jntuh_college_intake_proposed();
                        jntuh_college_intake_proposed.id = collegeIntakeProposed.id;
                        jntuh_college_intake_proposed.collegeId = collegeIntakeProposed.collegeId;
                        jntuh_college_intake_proposed.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == DateTime.Now.Year).Select(a => a.id).FirstOrDefault();
                        jntuh_college_intake_proposed.specializationId = collegeIntakeProposed.specializationId;
                        jntuh_college_intake_proposed.shiftId = collegeIntakeProposed.shiftId;
                        jntuh_college_intake_proposed.courseAffiliationStatusCodeId = collegeIntakeProposed.courseAffiliationStatusCodeId;
                        jntuh_college_intake_proposed.proposedIntake = collegeIntakeProposed.proposedIntake;
                        jntuh_college_intake_proposed.isActive = true;
                        jntuh_college_intake_proposed.createdBy = userID;
                        jntuh_college_intake_proposed.createdOn = DateTime.Now;
                        db.jntuh_college_intake_proposed.Add(jntuh_college_intake_proposed);
                        db.SaveChanges();
                        TempData["IntakeSuccess"] = "College Intake Proposed Details are Added successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                }
                else
                {
                    var IdUpdate = db.jntuh_college_intake_proposed.Where(p => p.specializationId == collegeIntakeProposed.specializationId &&
                                                                p.shiftId == collegeIntakeProposed.shiftId && p.collegeId == collegeIntakeProposed.collegeId &&
                                                                p.id != collegeIntakeProposed.id).Select(p => p.id).FirstOrDefault();
                    if (IdUpdate > 0)
                    {
                        TempData["IntakeError"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                        ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).OrderBy(d => d.degree).ToList();
                        ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                        ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                        ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                        ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_intake_proposed jntuh_college_intake_proposed = new jntuh_college_intake_proposed();
                        jntuh_college_intake_proposed.id = collegeIntakeProposed.id;
                        jntuh_college_intake_proposed.collegeId = collegeIntakeProposed.collegeId;
                        jntuh_college_intake_proposed.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == DateTime.Now.Year).Select(a => a.id).FirstOrDefault();
                        jntuh_college_intake_proposed.specializationId = collegeIntakeProposed.specializationId;
                        jntuh_college_intake_proposed.shiftId = collegeIntakeProposed.shiftId;
                        jntuh_college_intake_proposed.courseAffiliationStatusCodeId = collegeIntakeProposed.courseAffiliationStatusCodeId;
                        jntuh_college_intake_proposed.proposedIntake = collegeIntakeProposed.proposedIntake;
                        jntuh_college_intake_proposed.isActive = true;
                        jntuh_college_intake_proposed.createdBy = collegeIntakeProposed.createdBy;
                        jntuh_college_intake_proposed.createdOn = collegeIntakeProposed.createdOn;
                        jntuh_college_intake_proposed.updatedBy = userID;
                        jntuh_college_intake_proposed.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_intake_proposed).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["IntakeSuccess"] = "College Intake Proposed Details are Updated successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }

                }

            }
            else
            {
                ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).OrderBy(d => d.degree).ToList();
                ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            //We are using another duplicate Action Result for Admin Please verify the Below action
            jntuh_college_intake_proposed jntuh_college_intake_proposed = db.jntuh_college_intake_proposed.Where(p => p.id == id).FirstOrDefault();
            int userCollegeID = db.jntuh_college_intake_proposed.Where(p => p.id == id).Select(p=>p.collegeId).FirstOrDefault();
            if (jntuh_college_intake_proposed != null)
            {
                db.jntuh_college_intake_proposed.Remove(jntuh_college_intake_proposed);
                db.SaveChanges();
                TempData["IntakeSuccess"] = "College Proposed Intake Deleted successfully";
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                               year.isPresentAcademicYear == true)
                                                              .Select(year => year.actualYear).FirstOrDefault();
            ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_intake_proposed.Where(p => p.id == id).Select(p => p.collegeId).FirstOrDefault();
            }
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_intake_proposed.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
           

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

            CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();

            foreach (var item in proposed)
            {
                collegeIntakeProposed.id = item.id;
                collegeIntakeProposed.collegeId = item.collegeId;
                collegeIntakeProposed.academicYearId = item.academicYearId;
                collegeIntakeProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                collegeIntakeProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                collegeIntakeProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == collegeIntakeProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                collegeIntakeProposed.proposedIntake = item.proposedIntake;
                collegeIntakeProposed.specializationId = item.specializationId;
                collegeIntakeProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                collegeIntakeProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                collegeIntakeProposed.Department = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                collegeIntakeProposed.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                collegeIntakeProposed.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                collegeIntakeProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeProposed.shiftId = item.shiftId;
                collegeIntakeProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == userCollegeID &&
                                                                             ei.academicYearId == item.academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
            }

            if (collegeIntakeProposed != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_Details", collegeIntakeProposed);
                }
                else
                {
                    return View("Details", collegeIntakeProposed);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminEdit(string id)
        {
            //We are using another duplicate Action Result for College User Please verify the Above action
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();


            ViewBag.NextAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                     a.actualYear == (presentYear + 1))
                                                         .Select(a => a.academicYear).FirstOrDefault();

            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(l => l.isActive == true && l.isPresentAcademicYear == true).Select(l => l.academicYear).FirstOrDefault();



            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.collegeId = item.collegeId;
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                newProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == userCollegeID &&
                                                                             ei.academicYearId == academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
                newProposed.isActive = item.isActive;
                collegeIntakeProposedList.Add(newProposed);
            }
            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(intakeProposed => intakeProposed.Degree).ToList();
            ViewBag.Count = collegeIntakeProposedList.Count();
            return View(collegeIntakeProposedList);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminEditProposed(int? id, string adminId)
        {
            //We are using another duplicate Action Result for College User Please verify the Above action
            int collegeId = Convert.ToInt32(Utilities.DecryptString(adminId, WebConfigurationManager.AppSettings["CryptoKey"]));
            ViewBag.IsUpdate = true;
            CollegeIntakeProposed collegeIntakeProposed = new CollegeIntakeProposed();
            int academicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                       year.isPresentAcademicYear == true)
                                                      .Select(year => year.actualYear).FirstOrDefault();
            ViewBag.Year = String.Format("{0}-{1}", (academicYear + 1).ToString(), (academicYear + 2).ToString().Substring(2, 2));
            var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                         (collegeDegree, degree) => new
                                                         {
                                                             collegeDegree.degreeId,
                                                             collegeDegree.collegeId,
                                                             collegeDegree.isActive,
                                                             degree.degree
                                                         })
                                                     .Where(collegeDegree => collegeDegree.collegeId == collegeId && collegeDegree.isActive == true)
                                                     .Select(collegeDegree => new
                                                     {
                                                         collegeDegree.degreeId,
                                                         collegeDegree.degree
                                                     }).ToList();
            ViewBag.Degree = degrees.OrderBy(d => d.degree);
            ViewBag.Count = degrees.Count();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
            ViewBag.CourseAffiliationstatusText = db.jntuh_course_affiliation_status.Where(t => t.isActive == true).Select(t => t.courseAffiliationStatusDescription);

            if (id != null)
            {
                List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId && p.id == id).ToList();

                foreach (var item in proposed)
                {
                    collegeIntakeProposed.id = item.id;
                    collegeIntakeProposed.collegeId = item.collegeId;
                    collegeIntakeProposed.academicYearId = item.academicYearId;
                    collegeIntakeProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                    collegeIntakeProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                    collegeIntakeProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == collegeIntakeProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                    collegeIntakeProposed.proposedIntake = item.proposedIntake;
                    collegeIntakeProposed.specializationId = item.specializationId;
                    collegeIntakeProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeIntakeProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeIntakeProposed.Department = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeIntakeProposed.degreeID = db.jntuh_department.Where(d => d.id == collegeIntakeProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeIntakeProposed.Degree = db.jntuh_degree.Where(d => d.id == collegeIntakeProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeIntakeProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    collegeIntakeProposed.shiftId = item.shiftId;
                    collegeIntakeProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                                 ei.shiftId == item.shiftId &&
                                                                                 ei.collegeId == collegeId &&
                                                                                 ei.academicYearId == item.academicYearId)
                                                                            .Select(ei => ei.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                ViewBag.IsUpdate = false;

            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_AdminEditProposed", collegeIntakeProposed);
            }
            else
            {
                return View("_AdminEditProposed", collegeIntakeProposed);
            }


        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AdminEditProposed(CollegeIntakeProposed collegeIntakeProposed, string cmd, string adminId)
        {
            //We are using another duplicate Action Result for College User Please verify the Above action
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = Convert.ToInt32(Utilities.DecryptString(adminId, WebConfigurationManager.AppSettings["CryptoKey"]));
            ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == collegeId && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).OrderBy(d => d.degree).ToList();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
            if (ModelState.IsValid)
            {
                if (cmd == "Add")
                {
                    var Id = db.jntuh_college_intake_proposed.Where(p => p.specializationId == collegeIntakeProposed.specializationId &&
                                                                 p.shiftId == collegeIntakeProposed.shiftId && p.collegeId == collegeId).Select(p => p.id).FirstOrDefault();
                    if (Id > 0)
                    {
                        TempData["AdminIntakeError"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                        return RedirectToAction("AdminEdit", new { id = adminId });
                    }
                    else
                    {
                        jntuh_college_intake_proposed jntuh_college_intake_proposed = new jntuh_college_intake_proposed();
                        jntuh_college_intake_proposed.id = collegeIntakeProposed.id;
                        jntuh_college_intake_proposed.collegeId = collegeId;
                        jntuh_college_intake_proposed.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == DateTime.Now.Year).Select(a => a.id).FirstOrDefault();
                        jntuh_college_intake_proposed.specializationId = collegeIntakeProposed.specializationId;
                        jntuh_college_intake_proposed.shiftId = collegeIntakeProposed.shiftId;
                        jntuh_college_intake_proposed.courseAffiliationStatusCodeId = collegeIntakeProposed.courseAffiliationStatusCodeId;
                        jntuh_college_intake_proposed.proposedIntake = collegeIntakeProposed.proposedIntake;
                        jntuh_college_intake_proposed.isActive = true;
                        jntuh_college_intake_proposed.createdBy = userID;
                        jntuh_college_intake_proposed.createdOn = DateTime.Now;
                        db.jntuh_college_intake_proposed.Add(jntuh_college_intake_proposed);
                        db.SaveChanges();
                        TempData["AdminIntakeSuccess"] = "College Intake Proposed Details are Added successfully.";
                        return RedirectToAction("AdminEdit", new { id = adminId });
                    }
                }
                else
                {
                    var IdUpdate = db.jntuh_college_intake_proposed.Where(p => p.specializationId == collegeIntakeProposed.specializationId &&
                                                                    p.shiftId == collegeIntakeProposed.shiftId && p.collegeId == collegeId &&
                                                                    p.id != collegeIntakeProposed.id).Select(p => p.id).FirstOrDefault();
                    if (IdUpdate > 0)
                    {
                        TempData["AdminIntakeError"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                        return RedirectToAction("AdminEdit", new { id = adminId });
                    }
                    else
                    {
                        jntuh_college_intake_proposed jntuh_college_intake_proposed = new jntuh_college_intake_proposed();
                        jntuh_college_intake_proposed.id = collegeIntakeProposed.id;
                        jntuh_college_intake_proposed.collegeId = collegeId;
                        jntuh_college_intake_proposed.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == DateTime.Now.Year).Select(a => a.id).FirstOrDefault();
                        jntuh_college_intake_proposed.specializationId = collegeIntakeProposed.specializationId;
                        jntuh_college_intake_proposed.shiftId = collegeIntakeProposed.shiftId;
                        jntuh_college_intake_proposed.courseAffiliationStatusCodeId = collegeIntakeProposed.courseAffiliationStatusCodeId;
                        jntuh_college_intake_proposed.proposedIntake = collegeIntakeProposed.proposedIntake;
                        jntuh_college_intake_proposed.isActive = true;
                        jntuh_college_intake_proposed.createdBy = collegeIntakeProposed.createdBy;
                        jntuh_college_intake_proposed.createdOn = collegeIntakeProposed.createdOn;
                        jntuh_college_intake_proposed.updatedBy = userID;
                        jntuh_college_intake_proposed.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_intake_proposed).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["AdminIntakeSuccess"] = "College Intake Proposed Details are Updated successfully.";
                        return RedirectToAction("AdminEdit", new { id = adminId });
                    }
                }


            }
            else
            {
                return RedirectToAction("AdminEdit", new { id = adminId });
            }

        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminDelete(int id, string adminId)
        {
            //We are using another duplicate Action Result for College User Please verify the Above action
            jntuh_college_intake_proposed jntuh_college_intake_proposed = db.jntuh_college_intake_proposed.Where(p => p.id == id).FirstOrDefault();
            if (jntuh_college_intake_proposed != null)
            {
                if (jntuh_college_intake_proposed.isActive == true)
                    jntuh_college_intake_proposed.isActive = false;
                else
                    jntuh_college_intake_proposed.isActive = true;

                db.Entry(jntuh_college_intake_proposed).State = EntityState.Modified;
                db.SaveChanges();

                TempData["AdminIntakeSuccess"] = "Status changed successfully";

                return RedirectToAction("AdminEdit", new { id = adminId });
            }
            return RedirectToAction("AdminEdit", new { id = adminId });
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.NextAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                     a.actualYear == (presentYear + 1))
                                                         .Select(a => a.academicYear).FirstOrDefault();

            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(l => l.isActive == true && l.isPresentAcademicYear == true).Select(l => l.academicYear).FirstOrDefault();
            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.collegeId = item.collegeId;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                newProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == userCollegeID &&
                                                                             ei.academicYearId == academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
                collegeIntakeProposedList.Add(newProposed);
            }
            ViewBag.IntakeExisting = collegeIntakeProposedList;
            ViewBag.Count = collegeIntakeProposedList.Count();
            return View("UserView", collegeIntakeProposedList);
        }
    }
}
