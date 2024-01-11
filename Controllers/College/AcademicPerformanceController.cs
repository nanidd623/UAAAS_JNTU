using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Org.BouncyCastle.Cms;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AcademicPerformanceController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();

            //New Code Written by Narayana on 08-02-2020

            var Specializations =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3 || i.academicYearId == prAy -4) && (i.approvedIntake != 0 && i.approvedIntake != null))
                    .GroupBy(r => new { r.specializationId, r.shiftId })
                    .Select(s => s)
                    .ToList();
            

            //List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == userCollegeID).ToList();

            //List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            //foreach (var item in performance)
            //{
            //    CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
            //    newPerformance.id = item.id;
            //    newPerformance.collegeId = item.collegeId;
            //    newPerformance.academicYearId = item.academicYearId;
            //    newPerformance.shiftId = item.shiftId;
            //    newPerformance.isActive = item.isActive;
            //    newPerformance.specializationId = item.specializationId;
            //    newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
            //    newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
            //    newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
            //    newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
            //    newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
            //    newPerformance.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
            //    newPerformance.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
            //    collegeAcademicPerformance.Add(newPerformance);
            //}

            List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            foreach (var item in Specializations)
            {
                CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
                //newPerformance.id = item.id;
                newPerformance.collegeId = userCollegeID;
                newPerformance.academicYearId = prAy;
                newPerformance.shiftId = (int)(item.Key.shiftId);
                //newPerformance.isActive = item.isActive;
                newPerformance.specializationId = (int)(item.Key.specializationId);
                newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == newPerformance.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == newPerformance.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                newPerformance.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newPerformance.Shift = db.jntuh_shift.Where(s => s.id == newPerformance.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeAcademicPerformance.Add(newPerformance);
            }

            collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            collegeAcademicPerformance = collegeAcademicPerformance.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            foreach (var item in collegeAcademicPerformance)
            {
                item.id =
                    db.jntuh_college_academic_performance.Where(
                        r =>
                            r.specializationId == item.specializationId && r.shiftId == item.shiftId &&
                            r.collegeId == userCollegeID && r.academicYearId == (prAy-1)).Select(s => s.id).FirstOrDefault();
                item.appearedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 1));
                item.passedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 2));
                item.passPercentage1 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 3));

                item.appearedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 1));
                item.passedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 2));
                item.passPercentage2 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 3));

                item.appearedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 1));
                item.passedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 2));
                item.passPercentage3 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 3));

                item.appearedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 1));
                item.passedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 2));
                item.passPercentage4 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 3));
            }

            ViewBag.AcademicPerformance = collegeAcademicPerformance;
            ViewBag.Count = collegeAcademicPerformance.Count();
            DateTime todayDate = DateTime.Now.Date;
            //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

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
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (collegeAcademicPerformance.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeAcademicPerformance.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "AcademicPerformance");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "AcademicPerformance");
            }
            return View();

        }

        private string GetDetails(int collegeId, int academicYearId, int specializationId, int shiftId, int yearInDegree, int flag)
        {
            string value = string.Empty;
            if (flag == 1)
                value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.appearedStudents).FirstOrDefault().ToString();
            else if (flag == 2)
                value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.passedStudents).FirstOrDefault().ToString();
            else if (flag == 3)
                value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.passPercentage).FirstOrDefault().ToString();

            return value;
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
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            // int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();


            //New Code Written by Narayana on 08-02-2020

            var Specializations =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3 || i.academicYearId == prAy - 4) && (i.approvedIntake != 0 && i.approvedIntake != null))
                    .GroupBy(r => new { r.specializationId, r.shiftId })
                    .Select(s => s)
                    .ToList();
            var splist = Specializations.Distinct().ToList();

            //List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == userCollegeID).ToList();

            //List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            //foreach (var item in performance)
            //{
            //    CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
            //    newPerformance.id = item.id;
            //    newPerformance.collegeId = item.collegeId;
            //    newPerformance.academicYearId = item.academicYearId;
            //    newPerformance.shiftId = item.shiftId;
            //    newPerformance.isActive = item.isActive;
            //    newPerformance.specializationId = item.specializationId;
            //    newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
            //    newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
            //    newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
            //    newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
            //    newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
            //    newPerformance.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
            //    newPerformance.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
            //    collegeAcademicPerformance.Add(newPerformance);
            //}

            //collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            //collegeAcademicPerformance = collegeAcademicPerformance.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            foreach (var item in Specializations)
            {
                CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
                //newPerformance.id = item.id;
                newPerformance.collegeId = userCollegeID;
                newPerformance.academicYearId = prAy;
                newPerformance.shiftId = (int)(item.Key.shiftId);
                //newPerformance.isActive = item.isActive;
                newPerformance.specializationId = (int)(item.Key.specializationId);
                newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == newPerformance.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == newPerformance.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                newPerformance.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newPerformance.Shift = db.jntuh_shift.Where(s => s.id == newPerformance.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeAcademicPerformance.Add(newPerformance);
            }

            collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            collegeAcademicPerformance = collegeAcademicPerformance.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            foreach (var item in collegeAcademicPerformance)
            {
                item.appearedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 1));
                item.passedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 2));
                item.passPercentage1 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 3));

                item.appearedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 1));
                item.passedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 2));
                item.passPercentage2 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 3));

                item.appearedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 1));
                item.passedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 2));
                item.passPercentage3 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 3));

                item.appearedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 1));
                item.passedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 2));
                item.passPercentage4 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 3));
            }

            ViewBag.AcademicPerformance = collegeAcademicPerformance;
            ViewBag.Count = collegeAcademicPerformance.Count();
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? sid, int? shiftid, string collegeId)
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
                    //else if (id != null)
                    //{
                    //    userCollegeID = db.jntuh_college_academic_performance.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    //}
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            CollegeAcademicPerformance collegeAcademicPerformance = new CollegeAcademicPerformance();
            collegeAcademicPerformance.collegeId = userCollegeID;
            if (sid != null)
            {
                ViewBag.IsUpdate = true;
                var jntuhAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true).Select(a => a).ToList();
                ViewBag.AcademicYear = jntuhAcademicYear.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();

                int actualYear = jntuhAcademicYear.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                int ay1 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                int ay2 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
                int ay3 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
                int ay4 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
                int ay5 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();

               

                List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == userCollegeID && i.specializationId==sid&&i.shiftId==shiftid).ToList();

                //List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();
                if (performance.Count != 0)
                {
                    foreach (var item in performance)
                    {
                        collegeAcademicPerformance.id = item.id;
                        collegeAcademicPerformance.collegeId = item.collegeId;
                        collegeAcademicPerformance.academicYearId = item.academicYearId;
                        collegeAcademicPerformance.shiftId = item.shiftId;
                        collegeAcademicPerformance.isActive = item.isActive;
                        collegeAcademicPerformance.specializationId = item.specializationId;
                        collegeAcademicPerformance.Specialization =
                            db.jntuh_specialization.Where(s => s.id == item.specializationId)
                                .Select(s => s.specializationName)
                                .FirstOrDefault();
                        collegeAcademicPerformance.DepartmentID =
                            db.jntuh_specialization.Where(s => s.id == item.specializationId)
                                .Select(s => s.departmentId)
                                .FirstOrDefault();
                        collegeAcademicPerformance.Department =
                            db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        collegeAcademicPerformance.degreeID =
                            db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID)
                                .Select(d => d.degreeId)
                                .FirstOrDefault();
                        collegeAcademicPerformance.Degree =
                            db.jntuh_degree.Where(d => d.id == collegeAcademicPerformance.degreeID)
                                .Select(d => d.degree)
                                .FirstOrDefault();
                        collegeAcademicPerformance.Shift =
                            db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    }
                }
                else
                {
                    
                    collegeAcademicPerformance.collegeId = userCollegeID;
                    collegeAcademicPerformance.academicYearId = ay1;
                    collegeAcademicPerformance.shiftId = (int)shiftid;
                    //collegeAcademicPerformance.isActive = item.isActive;
                    collegeAcademicPerformance.specializationId = (int)sid;
                    collegeAcademicPerformance.Specialization =
                        db.jntuh_specialization.Where(s => s.id == collegeAcademicPerformance.specializationId)
                            .Select(s => s.specializationName)
                            .FirstOrDefault();
                    collegeAcademicPerformance.DepartmentID =
                        db.jntuh_specialization.Where(s => s.id == collegeAcademicPerformance.specializationId)
                            .Select(s => s.departmentId)
                            .FirstOrDefault();
                    collegeAcademicPerformance.Department =
                        db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID)
                            .Select(d => d.departmentName)
                            .FirstOrDefault();
                    collegeAcademicPerformance.degreeID =
                        db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID)
                            .Select(d => d.degreeId)
                            .FirstOrDefault();
                    collegeAcademicPerformance.Degree =
                        db.jntuh_degree.Where(d => d.id == collegeAcademicPerformance.degreeID)
                            .Select(d => d.degree)
                            .FirstOrDefault();
                    collegeAcademicPerformance.Shift =
                        db.jntuh_shift.Where(s => s.id == collegeAcademicPerformance.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }
                

                collegeAcademicPerformance.appearedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 1));
                collegeAcademicPerformance.passedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 2));
                collegeAcademicPerformance.passPercentage1 = Convert.ToDecimal(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 3));

                collegeAcademicPerformance.appearedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 1));
                collegeAcademicPerformance.passedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 2));
                collegeAcademicPerformance.passPercentage2 = Convert.ToDecimal(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 3));

                collegeAcademicPerformance.appearedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 1));
                collegeAcademicPerformance.passedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 2));
                collegeAcademicPerformance.passPercentage3 = Convert.ToDecimal(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 3));

                collegeAcademicPerformance.appearedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 1));
                collegeAcademicPerformance.passedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 2));
                collegeAcademicPerformance.passPercentage4 = Convert.ToDecimal(GetDetails(userCollegeID, ay2, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 3));
            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            //var specialization = db.jntuh_approvedadmitted_intake.Join(db.jntuh_specialization, collegeDegree => collegeDegree.SpecializationId, degree => degree.id,
            //                                                     (collegeDegree, degree) => new
            //                                                     {
            //                                                         collegeDegree.SpecializationId,
            //                                                         collegeDegree.collegeId,
            //                                                         degree.specializationName
            //                                                     })
            //                                                 .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
            //                                                 .Select(collegeDegree => new
            //                                                 {
            //                                                     collegeDegree.sp,
            //                                                     collegeDegree.degree
            //                                                 }).ToList();
            //int[] degreeIds = (from a in specializations
            //                   join b in departments on a.departmentId equals b.id
            //                   join c in degrees on b.degreeId equals c.id
            //                   where collegespecs.Contains(a.id)
            //                   select c.id).Distinct().ToArray();
            var degrees = (from cs in db.jntuh_approvedadmitted_intake
                join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                join d in db.jntuh_department on s.departmentId equals d.id
                join de in db.jntuh_degree on d.degreeId equals de.id
                where cs.collegeId == userCollegeID
                select new
                {
                    degreeId = de.id,
                    degree = de.degree,
                    DepartmentId=d.id,
                    Department=d.departmentName,
                    specializationId=s.id,
                    specialization = s.specializationName
                }).ToList();
            degrees = degrees.GroupBy(g => g.degreeId).Select(e=>e.FirstOrDefault()).ToList();
          
            ViewBag.Degree = degrees;
            ViewBag.Count = degrees.Count();
            //ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            //ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Create", collegeAcademicPerformance);
            }
            else
            {
                return View("_Create", collegeAcademicPerformance);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegeAcademicPerformance collegeAcademicPerformance, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeAcademicPerformance.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var degrees = (from cs in db.jntuh_approvedadmitted_intake
                           join s in db.jntuh_specialization on cs.SpecializationId equals s.id
                           join d in db.jntuh_department on s.departmentId equals d.id
                           join de in db.jntuh_degree on d.degreeId equals de.id
                           where cs.collegeId == userCollegeID
                           select new
                           {
                               degreeId = de.id,
                               degree = de.degree,
                               DepartmentId = d.id,
                               Department = d.departmentName,
                               specializationId = s.id,
                               specialization = s.specializationName
                           }).ToList();
            degrees = degrees.GroupBy(g => g.degreeId).Select(e => e.FirstOrDefault()).ToList();

            ViewBag.Degree = degrees;
            //ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            //ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            if (ModelState.IsValid)
            {
                collegeAcademicPerformance.collegeId = userCollegeID;
                int AYID = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();

                for (int i = 1; i <= 4; i++)
                {
                    int appeared = 0;
                    int passed = 0;
                    decimal percentage = 0;
                    int yearInDegree = i;
                    if (i == 1)
                    {
                        appeared = collegeAcademicPerformance.appearedStudents1;
                        passed = collegeAcademicPerformance.passedStudents1;
                        percentage = collegeAcademicPerformance.passPercentage1;
                    }
                    if (i == 2)
                    {
                        appeared = collegeAcademicPerformance.appearedStudents2;
                        passed = collegeAcademicPerformance.passedStudents2;
                        percentage = collegeAcademicPerformance.passPercentage2;
                    }
                    if (i == 3)
                    {
                        appeared = collegeAcademicPerformance.appearedStudents3;
                        passed = collegeAcademicPerformance.passedStudents3;
                        percentage = collegeAcademicPerformance.passPercentage3;
                    }
                    if (i == 4)
                    {
                        appeared = collegeAcademicPerformance.appearedStudents4;
                        passed = collegeAcademicPerformance.passedStudents4;
                        percentage = collegeAcademicPerformance.passPercentage4;
                    }

                    jntuh_college_academic_performance jntuh_college_academic_performance = new jntuh_college_academic_performance();
                    jntuh_college_academic_performance.academicYearId = db.jntuh_academic_year.Where(a => a.id == AYID).Select(a => a.id).FirstOrDefault();

                    var existingId = db.jntuh_college_academic_performance.Where(p => p.specializationId == collegeAcademicPerformance.specializationId
                                                                                && p.shiftId == collegeAcademicPerformance.shiftId && p.yearInDegreeId == yearInDegree
                                                                                && p.collegeId == collegeAcademicPerformance.collegeId
                                                                                && p.academicYearId == jntuh_college_academic_performance.academicYearId).Select(p => p.id).FirstOrDefault();
                    int createdByu = Convert.ToInt32(db.jntuh_college_academic_performance.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdBy).FirstOrDefault());
                    DateTime createdonu = Convert.ToDateTime(db.jntuh_college_academic_performance.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdOn).FirstOrDefault());

                    if ((appeared > 0 && passed > 0 && existingId == 0) || (existingId > 0))
                    {
                        jntuh_college_academic_performance.id = collegeAcademicPerformance.id;
                        jntuh_college_academic_performance.collegeId = collegeAcademicPerformance.collegeId;
                        jntuh_college_academic_performance.academicYearId = db.jntuh_academic_year.Where(a => a.id == AYID).Select(a => a.id).FirstOrDefault();
                        jntuh_college_academic_performance.specializationId = collegeAcademicPerformance.specializationId;
                        jntuh_college_academic_performance.shiftId = collegeAcademicPerformance.shiftId;
                        jntuh_college_academic_performance.appearedStudents = appeared;
                        jntuh_college_academic_performance.passedStudents = passed;
                        jntuh_college_academic_performance.passPercentage = percentage;
                        jntuh_college_academic_performance.yearInDegreeId = yearInDegree;
                        if (createdByu > 0)
                        {
                            jntuh_college_academic_performance.createdBy = createdByu;
                            jntuh_college_academic_performance.createdOn = createdonu;
                        }

                        if (existingId == 0)
                        {
                            jntuh_college_academic_performance.createdBy = userID;
                            jntuh_college_academic_performance.createdOn = DateTime.Now;
                            db.jntuh_college_academic_performance.Add(jntuh_college_academic_performance);
                        }
                        else
                        {
                            jntuh_college_academic_performance.id = existingId;
                            jntuh_college_academic_performance.updatedBy = userID;
                            jntuh_college_academic_performance.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_academic_performance).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }

                if (cmd == "Add")
                {
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Success"] = "Updated successfully.";
                }

                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            else
            {
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int? sid, int? shiftid,int? id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_academic_performance.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_academic_performance.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            CollegeAcademicPerformance collegeAcademicPerformance = new CollegeAcademicPerformance();

            if (sid != null)
            {
                ViewBag.IsUpdate = true;

                ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();


                List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == userCollegeID && i.specializationId == sid&&i.shiftId==shiftid).ToList();

                //List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();
                if (performance.Count!=0)
                {
                    foreach (var item in performance)
                {
                    collegeAcademicPerformance.id = item.id;
                    collegeAcademicPerformance.collegeId = item.collegeId;
                    collegeAcademicPerformance.academicYearId = item.academicYearId;
                    collegeAcademicPerformance.shiftId = item.shiftId;
                    collegeAcademicPerformance.isActive = item.isActive;
                    collegeAcademicPerformance.specializationId = item.specializationId;
                    collegeAcademicPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeAcademicPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeAcademicPerformance.Department = db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeAcademicPerformance.degreeID = db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeAcademicPerformance.Degree = db.jntuh_degree.Where(d => d.id == collegeAcademicPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeAcademicPerformance.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }
                }
                else
                {
                  
                    
                    collegeAcademicPerformance.collegeId = userCollegeID;
                    
                    collegeAcademicPerformance.shiftId = (int)shiftid;
                    
                    collegeAcademicPerformance.specializationId =(int)sid;
                    collegeAcademicPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == collegeAcademicPerformance.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeAcademicPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == collegeAcademicPerformance.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeAcademicPerformance.Department = db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeAcademicPerformance.degreeID = db.jntuh_department.Where(d => d.id == collegeAcademicPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeAcademicPerformance.Degree = db.jntuh_degree.Where(d => d.id == collegeAcademicPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeAcademicPerformance.Shift = db.jntuh_shift.Where(s => s.id == collegeAcademicPerformance.shiftId).Select(s => s.shiftName).FirstOrDefault();
               
                }
                

                collegeAcademicPerformance.appearedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 1));
                collegeAcademicPerformance.passedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 2));
                collegeAcademicPerformance.passPercentage1 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 1, 3));

                collegeAcademicPerformance.appearedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 1));
                collegeAcademicPerformance.passedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 2));
                collegeAcademicPerformance.passPercentage2 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 2, 3));

                collegeAcademicPerformance.appearedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 1));
                collegeAcademicPerformance.passedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 2));
                collegeAcademicPerformance.passPercentage3 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 3, 3));

                collegeAcademicPerformance.appearedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 1));
                collegeAcademicPerformance.passedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 2));
                collegeAcademicPerformance.passPercentage4 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, collegeAcademicPerformance.specializationId, collegeAcademicPerformance.shiftId, 4, 3));
            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            //ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
            //                                                     (collegeDegree, degree) => new
            //                                                     {
            //                                                         collegeDegree.degreeId,
            //                                                         collegeDegree.collegeId,
            //                                                         collegeDegree.isActive,
            //                                                         degree.degree
            //                                                     })
            //                                                 .Where(collegeDegree => collegeDegree.collegeId == userCollegeID)
            //                                                 .Select(collegeDegree => new
            //                                                 {
            //                                                     collegeDegree.degreeId,
            //                                                     collegeDegree.degree
            //                                                 }).ToList();
            //ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            //ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            if (Request.IsAjaxRequest())
            {
                return PartialView("Details", collegeAcademicPerformance);
            }
            else
            {
                return View("Details", collegeAcademicPerformance);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int? id)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_academic_performance.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (id!=null&&id!=0)
            {
                var spid = db.jntuh_college_academic_performance.Where(p => p.id == id).Select(p => p.specializationId).FirstOrDefault();
                List<int> row_ids = db.jntuh_college_academic_performance.Where(s => s.specializationId == spid && s.collegeId == userCollegeID).Select(s => s.id).ToList();
                foreach (var row in row_ids)
                {

                    jntuh_college_academic_performance rowToDelete = new jntuh_college_academic_performance();
                    rowToDelete = db.jntuh_college_academic_performance.Where(i => i.id == row).Select(i => i).FirstOrDefault();
                    db.jntuh_college_academic_performance.Remove(rowToDelete);
                    db.SaveChanges();
                    TempData["success"] = "deleted successfully";
                }
            }
            else
            {
                TempData["error"] = "no data found";
            }
           

            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });


        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = "FIRST YEAR";
            ViewBag.SecondYear = "SECOND YEAR";
            ViewBag.ThirdYear = "THIRD YEAR";
            ViewBag.FourthYear = "FOURTH YEAR";

            int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            foreach (var item in performance)
            {
                CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
                newPerformance.id = item.id;
                newPerformance.collegeId = item.collegeId;
                newPerformance.academicYearId = item.academicYearId;
                newPerformance.shiftId = item.shiftId;
                newPerformance.isActive = item.isActive;
                newPerformance.specializationId = item.specializationId;
                newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                newPerformance.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeAcademicPerformance.Add(newPerformance);
            }

            collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            foreach (var item in collegeAcademicPerformance)
            {
                item.appearedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 1));
                item.passedStudents1 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 2));
                item.passPercentage1 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 1, 3));

                item.appearedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 1));
                item.passedStudents2 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 2));
                item.passPercentage2 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 2, 3));

                item.appearedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 1));
                item.passedStudents3 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 2));
                item.passPercentage3 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 3, 3));

                item.appearedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 1));
                item.passedStudents4 = Convert.ToInt32(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 2));
                item.passPercentage4 = Convert.ToDecimal(GetDetails(userCollegeID, AYID, item.specializationId, item.shiftId, 4, 3));
            }

            ViewBag.AcademicPerformance = collegeAcademicPerformance;
            ViewBag.Count = collegeAcademicPerformance.Count();
            return View();
        }

        //Get College Departments
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
    public class CollegeDegreeDepartments
    {
        public int degreeId { get; set; }
        public string degree { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int specializationId { get; set; }
        public string specialization { get; set; }


    }

}
