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
    public class PA_CollegePlacementController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //New Code Written by Narayana on 10-02-2020

            var Specializations =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                    .GroupBy(r => new { r.specializationId })
                    .Select(s => s)
                    .ToList();


            //List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == userCollegeID).ToList();

            //List<CollegePlacement> collegePlacement = new List<CollegePlacement>();

            //foreach (var item in placements)
            //{
            //    CollegePlacement newPlacement = new CollegePlacement();
            //    newPlacement.id = item.id;
            //    newPlacement.collegeId = item.collegeId;
            //    newPlacement.academicYearId = item.academicYearId;
            //    newPlacement.specializationId = item.specializationId;
            //    newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
            //    newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
            //    newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
            //    newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
            //    newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
            //    newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
            //    collegePlacement.Add(newPlacement);
            //}
            //collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            //collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();

            List<CollegePlacement> collegePlacement = new List<CollegePlacement>();

            foreach (var item in Specializations)
            {
                CollegePlacement newPlacement = new CollegePlacement();
                //newPlacement.id = item.id;
                newPlacement.collegeId = userCollegeID;
                newPlacement.academicYearId = prAy;
                newPlacement.specializationId = (int)(item.Key.specializationId);
                newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == newPlacement.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == newPlacement.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                collegePlacement.Add(newPlacement);
            }

            collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();

            foreach (var item in collegePlacement)
            {
                item.id =
                    db.jntuh_college_placement.Where(
                        r =>
                            r.specializationId == item.specializationId &&
                            r.collegeId == userCollegeID).Select(s => s.id).FirstOrDefault();

                item.totalStudentsAppeared1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 1);
                item.totalStudentsPassed1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 2);
                item.totalStudentsPlaced1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 6);

                item.totalStudentsAppeared2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 1);
                item.totalStudentsPassed2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 2);
                item.totalStudentsPlaced2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 6);

                item.totalStudentsAppeared3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 1);
                item.totalStudentsPassed3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 2);
                item.totalStudentsPlaced3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 6);
            }
            ViewBag.StudentsPlacement = collegePlacement;
            ViewBag.Count = collegePlacement.Count();
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (collegePlacement.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegePlacement.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_CollegePlacement");
            }
            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PCP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "PA_CollegePlacement");
            }
            return View();
        }

        private int? GetStudents(int collegeId, int academicYearId, int specializationId, int flag)
        {
            int? student = 0;

            if (flag == 1)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
            else
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
            return student == null ? (int?)null : Convert.ToInt32(student);
        }

        private int? GetStudents_new(int collegeId, int academicYearId, int specializationId, int flag)
        {
            int? student = 0;

            if (flag == 1)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsAppeared == null ? 0 : i.totalStudentsAppeared.Value).FirstOrDefault();
            else if (flag == 2)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
            else if (flag == 3)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsDestincion == null ? 0 : i.totalStudentsDestincion.Value).FirstOrDefault();
            else if (flag == 4)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsfirstclass == null ? 0 : i.totalStudentsfirstclass.Value).FirstOrDefault();
            else if (flag == 5)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.detainedforattendance == null ? 0 : i.detainedforattendance.Value).FirstOrDefault();
            else if (flag == 6)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
            else if (flag == 7)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.above10lpa == null ? 0 : i.above10lpa.Value).FirstOrDefault();
            else if (flag == 8)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.above5to10lpa == null ? 0 : i.above5to10lpa.Value).FirstOrDefault();
            else if (flag == 9)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.above3to5lpa == null ? 0 : i.above3to5lpa.Value).FirstOrDefault();
            else if (flag == 10)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.below3lpa == null ? 0 : i.below3lpa.Value).FirstOrDefault();
            else if (flag == 11)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.highereducation == null ? 0 : i.highereducation.Value).FirstOrDefault();
            else if (flag == 12)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.publicsector == null ? 0 : i.publicsector.Value).FirstOrDefault();
            else if (flag == 13)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.entrepreneurs == null ? 0 : i.entrepreneurs.Value).FirstOrDefault();
            else if (flag == 14)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => (int)i.placementpercentage == null ? 0 : (int)i.placementpercentage).FirstOrDefault();
            else
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.detainedforcredits == null ? 0 : i.detainedforcredits).FirstOrDefault();
            return student == null ? (int?)null : Convert.ToInt32(student);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? sid, string collegeId)
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
                    else if (sid != null)
                    {
                        userCollegeID = db.jntuh_college_placement.Where(p => p.id == sid).Select(p => p.collegeId).FirstOrDefault();
                    }
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            CollegePlacement collegePlacement = new CollegePlacement();
            collegePlacement.collegeId = userCollegeID;

            if (sid != null)
            {

                ViewBag.IsUpdate = true;

                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
                ViewBag.SixthYear = String.Format("{0}-{1}", (actualYear - 5).ToString(), (actualYear - 4).ToString().Substring(2, 2));

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
                int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 5)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == userCollegeID && i.specializationId == sid).ToList();
                if (placements.Count != 0)
                {
                    foreach (var item in placements)
                    {
                        collegePlacement.id = item.id;
                        collegePlacement.collegeId = item.collegeId;
                        collegePlacement.academicYearId = item.academicYearId;
                        collegePlacement.specializationId = item.specializationId;
                        collegePlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegePlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        collegePlacement.department = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                        collegePlacement.degreeID = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                        collegePlacement.degree = db.jntuh_degree.Where(d => d.id == collegePlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                    }
                }
                else
                {

                    collegePlacement.collegeId = userCollegeID;
                    // collegePlacement.academicYearId = item.academicYearId;
                    collegePlacement.specializationId = (int)sid;
                    collegePlacement.specialization = db.jntuh_specialization.Where(s => s.id == collegePlacement.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegePlacement.departmentID = db.jntuh_specialization.Where(s => s.id == collegePlacement.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegePlacement.department = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegePlacement.degreeID = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegePlacement.degree = db.jntuh_degree.Where(d => d.id == collegePlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                }

                collegePlacement.totalStudentsAppeared1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);


                collegePlacement.totalStudentsAppeared2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred2 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

                collegePlacement.totalStudentsAppeared3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred3 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

                collegePlacement.totalStudentsAppeared4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred4 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

                collegePlacement.totalStudentsAppeared5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred5 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

                collegePlacement.totalStudentsAppeared6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 1);
                collegePlacement.totalStudentsPassed6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 2);
                collegePlacement.totalStudentsDistinction6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 3);
                collegePlacement.totalStudentsFirstClass6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 4);
                collegePlacement.totalStudentsDetained6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 5);
                collegePlacement.totalStudentsPlaced6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 6);
                collegePlacement.totalStudentsAbove10L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 7);
                collegePlacement.totalStudents5to10L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 8);
                collegePlacement.totalStudents3to5L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 9);
                collegePlacement.totalStudentsBelow3L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 10);
                collegePlacement.totalStudentsHighEdu6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 11);
                collegePlacement.totalStudentsPubSec6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 12);
                collegePlacement.totalStudentsEnt6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 13);
                collegePlacement.totalStudentsPlacePer6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 14);
                collegePlacement.totalStudentsDetCred6 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);
            }
            else
            {
                ViewBag.IsUpdate = false;
            }
            int year = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (year).ToString(), (year + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (year - 1).ToString(), (year).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (year - 2).ToString(), (year - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (year - 3).ToString(), (year - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (year - 4).ToString(), (year - 3).ToString().Substring(2, 2));
            ViewBag.SixthYear = String.Format("{0}-{1}", (year - 5).ToString(), (year - 4).ToString().Substring(2, 2));

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
            ViewBag.Degree = degrees;
            ViewBag.Count = degrees.Count();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            //if (Request.IsAjaxRequest())
            //{
            //    return View("Edit", collegePlacement);
            //    //return PartialView("_Create", collegePlacement);
            //}
            //else
            //{
            return View("Edit", collegePlacement);
            //return View("Create", collegePlacement);
            //}
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegePlacement collegePlacement, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegePlacement.collegeId;
            }
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
                                                             }).ToList();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);

            if (ModelState.IsValid)
            {
                if (cmd == "Add")
                {
                    if (userCollegeID == 0)
                    {
                        return RedirectToAction("Create", "CollegeInformation");
                    }
                    collegePlacement.collegeId = userCollegeID;
                    int presentAY = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    var message = string.Empty;
                    for (int i = 0; i <= 2; i++)
                    {
                        int? studentsPassed = 0;
                        int? studentsPlaced = 0;
                        int? academicYear = 0;

                        if (i == 0)
                        {
                            studentsPassed = collegePlacement.totalStudentsPassed1;
                            studentsPlaced = collegePlacement.totalStudentsPlaced1;
                            academicYear = presentAY - i;
                        }
                        if (i == 1)
                        {
                            studentsPassed = collegePlacement.totalStudentsPassed2;
                            studentsPlaced = collegePlacement.totalStudentsPlaced2;
                            academicYear = presentAY - i;
                        }
                        if (i == 2)
                        {
                            studentsPassed = collegePlacement.totalStudentsPassed3;
                            studentsPlaced = collegePlacement.totalStudentsPlaced3;
                            academicYear = presentAY - i;
                        }

                        if (studentsPassed > 0)
                        {
                            jntuh_college_placement jntuh_college_placement = new jntuh_college_placement();

                            jntuh_college_placement.id = collegePlacement.id;
                            jntuh_college_placement.collegeId = collegePlacement.collegeId;
                            jntuh_college_placement.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();
                            jntuh_college_placement.specializationId = collegePlacement.specializationId;
                            jntuh_college_placement.totalStudentsPassed = (studentsPassed == null ? 0 : studentsPassed);
                            jntuh_college_placement.totalStudentsPlaced = (studentsPlaced == null ? 0 : studentsPlaced);
                            int existId = db.jntuh_college_placement.Where(p => p.specializationId == collegePlacement.specializationId
                                                                                              && p.collegeId == collegePlacement.collegeId
                                                                                              && p.academicYearId == jntuh_college_placement.academicYearId).Select(p => p.id).FirstOrDefault();
                            if (existId > 0)
                            {
                                TempData["PlacementError"] = "Specialization is already exists . Please enter a different Specialization";
                                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                            }
                            jntuh_college_placement.createdBy = userID;
                            jntuh_college_placement.createdOn = DateTime.Now;
                            db.jntuh_college_placement.Add(jntuh_college_placement);
                            db.SaveChanges();
                            TempData["PlacementSuccess"] = "Added successfully.";

                        }
                    }
                    return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                else
                {
                    collegePlacement.collegeId = userCollegeID;
                    int presentAY = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    var message = string.Empty;
                    for (int i = 0; i <= 5; i++)
                    {
                        int? studentsAppeared = 0;
                        int? studentsPassed = 0;
                        int? studentsDistinction = 0;
                        int? studentsFirstClass = 0;
                        int? studentsDetained = 0;
                        int? studentsPlaced = 0;
                        int? studentsAbove10L = 0;
                        int? students5to10L = 0;
                        int? students3to5L = 0;
                        int? studentsBelow3L = 0;
                        int? studentsHighEdu = 0;
                        int? studentsPubSec = 0;
                        int? studentsEnt = 0;
                        decimal? studentsPlacePer = 0;
                        int? studentsDetCred = 0;
                        int? academicYear = 0;

                        if (i == 0)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared1;
                            studentsPassed = collegePlacement.totalStudentsPassed1;
                            studentsDistinction = collegePlacement.totalStudentsDistinction1;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass1;
                            studentsDetained = collegePlacement.totalStudentsDetained1;
                            studentsPlaced = collegePlacement.totalStudentsPlaced1;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L1;
                            students5to10L = collegePlacement.totalStudents5to10L1;
                            students3to5L = collegePlacement.totalStudents3to5L1;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L1;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu1;
                            studentsPubSec = collegePlacement.totalStudentsPubSec1;
                            studentsEnt = collegePlacement.totalStudentsEnt1;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer1;
                            studentsDetCred = collegePlacement.totalStudentsDetCred1;
                            academicYear = presentAY - i;
                        }
                        if (i == 1)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared2;
                            studentsPassed = collegePlacement.totalStudentsPassed2;
                            studentsDistinction = collegePlacement.totalStudentsDistinction2;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass2;
                            studentsDetained = collegePlacement.totalStudentsDetained2;
                            studentsPlaced = collegePlacement.totalStudentsPlaced2;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L2;
                            students5to10L = collegePlacement.totalStudents5to10L2;
                            students3to5L = collegePlacement.totalStudents3to5L2;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L2;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu2;
                            studentsPubSec = collegePlacement.totalStudentsPubSec2;
                            studentsEnt = collegePlacement.totalStudentsEnt2;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer2;
                            studentsDetCred = collegePlacement.totalStudentsDetCred2;
                            academicYear = presentAY - i;
                        }
                        if (i == 2)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared3;
                            studentsPassed = collegePlacement.totalStudentsPassed3;
                            studentsDistinction = collegePlacement.totalStudentsDistinction3;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass3;
                            studentsDetained = collegePlacement.totalStudentsDetained3;
                            studentsPlaced = collegePlacement.totalStudentsPlaced3;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L3;
                            students5to10L = collegePlacement.totalStudents5to10L3;
                            students3to5L = collegePlacement.totalStudents3to5L3;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L3;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu3;
                            studentsPubSec = collegePlacement.totalStudentsPubSec3;
                            studentsEnt = collegePlacement.totalStudentsEnt3;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer3;
                            studentsDetCred = collegePlacement.totalStudentsDetCred3;
                            academicYear = presentAY - i;
                        }
                        if (i == 3)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared4;
                            studentsPassed = collegePlacement.totalStudentsPassed4;
                            studentsDistinction = collegePlacement.totalStudentsDistinction4;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass4;
                            studentsDetained = collegePlacement.totalStudentsDetained4;
                            studentsPlaced = collegePlacement.totalStudentsPlaced4;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L4;
                            students5to10L = collegePlacement.totalStudents5to10L4;
                            students3to5L = collegePlacement.totalStudents3to5L4;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L4;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu4;
                            studentsPubSec = collegePlacement.totalStudentsPubSec4;
                            studentsEnt = collegePlacement.totalStudentsEnt4;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer4;
                            studentsDetCred = collegePlacement.totalStudentsDetCred4;
                            academicYear = presentAY - i;
                        }
                        if (i == 4)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared5;
                            studentsPassed = collegePlacement.totalStudentsPassed5;
                            studentsDistinction = collegePlacement.totalStudentsDistinction5;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass5;
                            studentsDetained = collegePlacement.totalStudentsDetained5;
                            studentsPlaced = collegePlacement.totalStudentsPlaced5;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L5;
                            students5to10L = collegePlacement.totalStudents5to10L5;
                            students3to5L = collegePlacement.totalStudents3to5L5;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L5;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu5;
                            studentsPubSec = collegePlacement.totalStudentsPubSec5;
                            studentsEnt = collegePlacement.totalStudentsEnt5;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer5;
                            studentsDetCred = collegePlacement.totalStudentsDetCred5;
                            academicYear = presentAY - i;
                        }
                        if (i == 5)
                        {
                            studentsAppeared = collegePlacement.totalStudentsAppeared6;
                            studentsPassed = collegePlacement.totalStudentsPassed6;
                            studentsDistinction = collegePlacement.totalStudentsDistinction6;
                            studentsFirstClass = collegePlacement.totalStudentsFirstClass6;
                            studentsDetained = collegePlacement.totalStudentsDetained6;
                            studentsPlaced = collegePlacement.totalStudentsPlaced6;
                            studentsAbove10L = collegePlacement.totalStudentsAbove10L6;
                            students5to10L = collegePlacement.totalStudents5to10L6;
                            students3to5L = collegePlacement.totalStudents3to5L6;
                            studentsBelow3L = collegePlacement.totalStudentsBelow3L6;
                            studentsHighEdu = collegePlacement.totalStudentsHighEdu6;
                            studentsPubSec = collegePlacement.totalStudentsPubSec6;
                            studentsEnt = collegePlacement.totalStudentsEnt6;
                            studentsPlacePer = collegePlacement.totalStudentsPlacePer6;
                            studentsDetCred = collegePlacement.totalStudentsDetCred6;
                            academicYear = presentAY - i;
                        }

                        if (studentsPassed > 0)
                        {
                            jntuh_college_placement jntuh_college_placement = new jntuh_college_placement();

                            jntuh_college_placement.id = collegePlacement.id;
                            jntuh_college_placement.collegeId = collegePlacement.collegeId;
                            jntuh_college_placement.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();
                            jntuh_college_placement.specializationId = collegePlacement.specializationId;
                            jntuh_college_placement.totalStudentsAppeared = (studentsAppeared == null ? 0 : studentsAppeared);
                            jntuh_college_placement.totalStudentsPassed = (studentsPassed == null ? 0 : studentsPassed);
                            jntuh_college_placement.totalStudentsDestincion = (studentsDistinction == null ? 0 : studentsDistinction);
                            jntuh_college_placement.totalStudentsfirstclass = (studentsFirstClass == null ? 0 : studentsFirstClass);
                            jntuh_college_placement.detainedforattendance = (studentsDetained == null ? 0 : studentsDetained);
                            jntuh_college_placement.totalStudentsPlaced = (studentsPlaced == null ? 0 : studentsPlaced);
                            jntuh_college_placement.above10lpa = (studentsAbove10L == null ? 0 : studentsAbove10L);
                            jntuh_college_placement.above5to10lpa = (students5to10L == null ? 0 : students5to10L);
                            jntuh_college_placement.above3to5lpa = (students3to5L == null ? 0 : students3to5L);
                            jntuh_college_placement.below3lpa = (studentsBelow3L == null ? 0 : studentsBelow3L);
                            jntuh_college_placement.highereducation = (studentsHighEdu == null ? 0 : studentsHighEdu);
                            jntuh_college_placement.publicsector = (studentsPubSec == null ? 0 : studentsPubSec);
                            jntuh_college_placement.entrepreneurs = (studentsEnt == null ? 0 : studentsEnt);
                            jntuh_college_placement.placementpercentage = (studentsPlacePer == null ? 0 : studentsPlacePer);
                            jntuh_college_placement.detainedforcredits = (studentsDetCred == null ? 0 : studentsDetCred);
                            int id = db.jntuh_college_placement.Where(p => p.collegeId == collegePlacement.collegeId
                                                                         && p.specializationId == collegePlacement.specializationId
                                                                         && p.academicYearId == jntuh_college_placement.academicYearId).Select(p => p.id).FirstOrDefault();
                            if (id == 0)
                            {
                                jntuh_college_placement.createdBy = userID;
                                jntuh_college_placement.createdOn = DateTime.Now;
                                db.jntuh_college_placement.Add(jntuh_college_placement);
                                db.SaveChanges();
                            }
                            else
                            {
                                jntuh_college_placement.id = id;
                                jntuh_college_placement.createdOn = db.jntuh_college_placement.Where(p => p.id == id).Select(p => p.createdOn).FirstOrDefault();
                                jntuh_college_placement.createdBy = db.jntuh_college_placement.Where(p => p.id == id).Select(p => p.createdBy).FirstOrDefault();
                                jntuh_college_placement.updatedBy = userID;
                                jntuh_college_placement.updatedOn = DateTime.Now;
                                db.Entry(jntuh_college_placement).State = EntityState.Modified;

                            }
                            db.SaveChanges();
                            TempData["PlacementSuccess"] = "Updated successfully.";

                        }

                    }
                }
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int? sid)
        {

            CollegePlacement collegePlacement = new CollegePlacement();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_placement.Where(e => e.id == sid).Select(e => e.collegeId).FirstOrDefault();
            }
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_placement.Where(e => e.id == sid).Select(e => e.collegeId).FirstOrDefault();
            }
            ViewBag.IsUpdate = true;

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
            ViewBag.SixthYear = String.Format("{0}-{1}", (actualYear - 5).ToString(), (actualYear - 4).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
            int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 5)).Select(a => a.id).FirstOrDefault();


            List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == userCollegeID && i.specializationId == sid).ToList();

            if (placements.Count != 0)
            {
                foreach (var item in placements)
                {
                    collegePlacement.id = item.id;
                    collegePlacement.collegeId = item.collegeId;
                    collegePlacement.academicYearId = item.academicYearId;
                    collegePlacement.specializationId = item.specializationId;
                    collegePlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegePlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegePlacement.department = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegePlacement.degreeID = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegePlacement.degree = db.jntuh_degree.Where(d => d.id == collegePlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                }
            }
            else
            {
                //collegePlacement.id = item.id;
                collegePlacement.collegeId = userCollegeID;
                //collegePlacement.academicYearId = item.academicYearId;
                collegePlacement.specializationId = (int)sid;
                collegePlacement.specialization = db.jntuh_specialization.Where(s => s.id == collegePlacement.specializationId).Select(s => s.specializationName).FirstOrDefault();
                collegePlacement.departmentID = db.jntuh_specialization.Where(s => s.id == collegePlacement.specializationId).Select(s => s.departmentId).FirstOrDefault();
                collegePlacement.department = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                collegePlacement.degreeID = db.jntuh_department.Where(d => d.id == collegePlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                collegePlacement.degree = db.jntuh_degree.Where(d => d.id == collegePlacement.degreeID).Select(d => d.degree).FirstOrDefault();
            }


            //userCollegeID = db.jntuh_college_placement.Where(p => p.id == sid).Select(p => p.collegeId).FirstOrDefault();
            collegePlacement.totalStudentsAppeared1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred1 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);


            collegePlacement.totalStudentsAppeared2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer2 = GetStudents_new(userCollegeID, AY2, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred2 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

            collegePlacement.totalStudentsAppeared3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer3 = GetStudents_new(userCollegeID, AY3, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred3 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

            collegePlacement.totalStudentsAppeared4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer4 = GetStudents_new(userCollegeID, AY4, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred4 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

            collegePlacement.totalStudentsAppeared5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer5 = GetStudents_new(userCollegeID, AY5, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred5 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

            collegePlacement.totalStudentsAppeared6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 1);
            collegePlacement.totalStudentsPassed6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 2);
            collegePlacement.totalStudentsDistinction6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 3);
            collegePlacement.totalStudentsFirstClass6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 4);
            collegePlacement.totalStudentsDetained6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 5);
            collegePlacement.totalStudentsPlaced6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 6);
            collegePlacement.totalStudentsAbove10L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 7);
            collegePlacement.totalStudents5to10L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 8);
            collegePlacement.totalStudents3to5L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 9);
            collegePlacement.totalStudentsBelow3L6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 10);
            collegePlacement.totalStudentsHighEdu6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 11);
            collegePlacement.totalStudentsPubSec6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 12);
            collegePlacement.totalStudentsEnt6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 13);
            collegePlacement.totalStudentsPlacePer6 = GetStudents_new(userCollegeID, AY6, collegePlacement.specializationId, 14);
            collegePlacement.totalStudentsDetCred6 = GetStudents_new(userCollegeID, AY1, collegePlacement.specializationId, 15);

            int year = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (year).ToString(), (year + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (year - 1).ToString(), (year).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (year - 2).ToString(), (year - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));
            ViewBag.SixthYear = String.Format("{0}-{1}", (actualYear - 5).ToString(), (actualYear - 4).ToString().Substring(2, 2));
            //if (Request.IsAjaxRequest())
            //{
            //    ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
            //    return PartialView("_Details", collegePlacement);
            //}
            //else
            //{
            //ViewBag.SportsType = db.jntuh_sports_type.Where(r => r.isActive == true);
            return View("Details", collegePlacement);
            //}
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int? sid, int placementId)
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_placement.Where(e => e.id == placementId).Select(e => e.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            var ids = db.jntuh_college_placement.Where(p => p.specializationId == sid && p.collegeId == userCollegeID).Select(p => p.id).ToList();
            foreach (var item in ids)
            {
                jntuh_college_placement jntuh_college_placement = db.jntuh_college_placement.Where(p => p.id == item && p.collegeId == userCollegeID).FirstOrDefault();
                if (jntuh_college_placement != null)
                {
                    db.jntuh_college_placement.Remove(jntuh_college_placement);
                    db.SaveChanges();
                    TempData["PlacementSuccess"] = "Deleted successfully.";
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"])); ;
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PCP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

            //New Code Written by Narayana on 10-02-2020
            var Specializations =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                    .GroupBy(r => new { r.specializationId })
                    .Select(s => s)
                    .ToList();

            //List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == userCollegeID).ToList();

            //List<CollegePlacement> collegePlacement = new List<CollegePlacement>();

            //foreach (var item in placements)
            //{
            //    CollegePlacement newPlacement = new CollegePlacement();
            //    newPlacement.id = item.id;
            //    newPlacement.collegeId = item.collegeId;
            //    newPlacement.academicYearId = item.academicYearId;
            //    newPlacement.specializationId = item.specializationId;
            //    newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
            //    newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
            //    newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
            //    newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
            //    newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
            //    newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
            //    collegePlacement.Add(newPlacement);
            //}

            //collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            //collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();

            List<CollegePlacement> collegePlacement = new List<CollegePlacement>();

            foreach (var item in Specializations)
            {
                CollegePlacement newPlacement = new CollegePlacement();
                //newPlacement.id = item.id;
                newPlacement.collegeId = userCollegeID;
                newPlacement.academicYearId = prAy;
                newPlacement.specializationId = (int)(item.Key.specializationId);
                newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == newPlacement.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == newPlacement.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                collegePlacement.Add(newPlacement);
            }

            collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();

            foreach (var item in collegePlacement)
            {
                item.id =
                    db.jntuh_college_placement.Where(
                        r =>
                            r.specializationId == item.specializationId &&
                            r.collegeId == userCollegeID).Select(s => s.id).FirstOrDefault();

                item.totalStudentsAppeared1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 1);
                item.totalStudentsPassed1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 2);
                item.totalStudentsPlaced1 = GetStudents_new(userCollegeID, AY1, item.specializationId, 6);

                item.totalStudentsAppeared2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 1);
                item.totalStudentsPassed2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 2);
                item.totalStudentsPlaced2 = GetStudents_new(userCollegeID, AY2, item.specializationId, 6);

                item.totalStudentsAppeared3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 1);
                item.totalStudentsPassed3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 2);
                item.totalStudentsPlaced3 = GetStudents_new(userCollegeID, AY3, item.specializationId, 6);
            }
            ViewBag.StudentsPlacement = collegePlacement;
            ViewBag.Count = collegePlacement.Count();
            return View("View", collegePlacement);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"])); ;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegePlacement> collegePlacement = new List<CollegePlacement>();

            foreach (var item in placements)
            {
                CollegePlacement newPlacement = new CollegePlacement();
                newPlacement.id = item.id;
                newPlacement.collegeId = item.collegeId;
                newPlacement.academicYearId = item.academicYearId;
                newPlacement.specializationId = item.specializationId;
                newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                collegePlacement.Add(newPlacement);
            }
            collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();
            foreach (var item in collegePlacement)
            {
                item.totalStudentsPassed1 = GetStudents(userCollegeID, AY1, item.specializationId, 1);
                item.totalStudentsPlaced1 = GetStudents(userCollegeID, AY1, item.specializationId, 0);

                item.totalStudentsPassed2 = GetStudents(userCollegeID, AY2, item.specializationId, 1);
                item.totalStudentsPlaced2 = GetStudents(userCollegeID, AY2, item.specializationId, 0);

                item.totalStudentsPassed3 = GetStudents(userCollegeID, AY3, item.specializationId, 1);
                item.totalStudentsPlaced3 = GetStudents(userCollegeID, AY3, item.specializationId, 0);
            }
            ViewBag.StudentsPlacement = collegePlacement;
            ViewBag.Count = collegePlacement.Count();
            return View("UserView", collegePlacement);
        }

        public ActionResult Edit(CollegePlacement collegePlacement)
        {
            return View(collegePlacement);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College")]
        public ActionResult RemedialEdit(string collegeId)
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
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            //if (true)
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("RemedialView", "PA_CollegePlacement");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PRT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            var remedialTeaching = new RemedialTeaching();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 9).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                remedialTeaching.ActivityId = 22;
                remedialTeaching.CollegeId = userCollegeID;
                ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 9).Select(e => e.activitydescription).FirstOrDefault();
                if (collegeExtracurricularactivities != null)
                {
                    remedialTeaching.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    remedialTeaching.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    remedialTeaching.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View("RemedialEdit", remedialTeaching);
        }

        public ActionResult FileUpload(HttpPostedFileBase fileUploader, RemedialTeaching remedialmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 9).Select(i => i.sno).ToArray();
            var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.Where(e => masteractivities.Contains(e.activityid) && e.collegeid == 375).FirstOrDefault();

            if (collegeExtracurricularactivities != null && collegeExtracurricularactivities.sno > 0)
            {
                //update
                collegeExtracurricularactivities.academicyear = ay0;
                collegeExtracurricularactivities.activitystatus = remedialmodel.ActivitySelected;

                if (remedialmodel.ActivitySelected)
                {
                    if (remedialmodel.ActivityDocument != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/College/RemedialTeaching";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(remedialmodel.ActivityDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (remedialmodel.ActivityDocumentPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "feedback" + "-" + "JNTUH";
                                remedialmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                remedialmodel.ActivityDocumentPath = string.Format("{0}{1}", fileName, ext);
                                collegeExtracurricularactivities.supportingdocuments = remedialmodel.ActivityDocumentPath;
                            }
                            else
                            {
                                remedialmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), remedialmodel.ActivityDocumentPath));
                                collegeExtracurricularactivities.supportingdocuments = remedialmodel.ActivityDocumentPath;
                            }
                        }
                    }
                    collegeExtracurricularactivities.remarks = remedialmodel.Remarks;
                }
                else
                {
                    collegeExtracurricularactivities.supportingdocuments = null;
                    collegeExtracurricularactivities.remarks = null;
                }
                collegeExtracurricularactivities.updatedby = userID;
                collegeExtracurricularactivities.updatedon = DateTime.Now;

                db.Entry(collegeExtracurricularactivities).State = EntityState.Modified;

                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //add
                jntuh_college_extracurricularactivities clgExtraCurr = new jntuh_college_extracurricularactivities();
                clgExtraCurr.activityid = remedialmodel.ActivityId;
                clgExtraCurr.collegeid = userCollegeID;
                clgExtraCurr.academicyear = ay0;
                clgExtraCurr.activitystatus = remedialmodel.ActivitySelected;
                clgExtraCurr.isactive = true;
                if (remedialmodel.ActivitySelected)
                {
                    if (remedialmodel.ActivityDocument != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/College/RemedialTeaching";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(remedialmodel.ActivityDocument.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (remedialmodel.ActivityDocumentPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "Remedial" + "-" + "JNTUH";
                                remedialmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                remedialmodel.ActivityDocumentPath = string.Format("{0}{1}", fileName, ext);
                                clgExtraCurr.supportingdocuments = remedialmodel.ActivityDocumentPath;
                            }
                            else
                            {
                                remedialmodel.ActivityDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), remedialmodel.ActivityDocumentPath));
                                clgExtraCurr.supportingdocuments = remedialmodel.ActivityDocumentPath;
                            }
                        }
                    }
                    else
                    {
                        clgExtraCurr.supportingdocuments = remedialmodel.ActivityDocumentPath;
                    }

                    clgExtraCurr.remarks = remedialmodel.Remarks;
                }
                clgExtraCurr.createdon = DateTime.Now;
                clgExtraCurr.createdby = userID;
                db.jntuh_college_extracurricularactivities.Add(clgExtraCurr);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            return RedirectToAction("RemedialView", "PA_CollegePlacement");
            //return RedirectToAction("RemedialEdit", "PA_CollegePlacement", new
            //{
            //    collegeId = Utilities.EncryptString(remedialmodel.CollegeId.ToString(),
            //        WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            //});
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult RemedialView(string collegeId)
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

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PRT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            var remedialTeaching = new RemedialTeaching();
            remedialTeaching.CollegeId = userCollegeID;
            ViewBag.ActivityDescription = db.jntuh_extracurricularactivities.Where(e => e.activitytype == 9).Select(e => e.activitydescription).FirstOrDefault();
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 9).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == 375);
                if (collegeExtracurricularactivities != null)
                {
                    remedialTeaching.ActivitySelected = collegeExtracurricularactivities.activitystatus;
                    remedialTeaching.ActivityDocumentPath = collegeExtracurricularactivities.supportingdocuments;
                    remedialTeaching.Remarks = collegeExtracurricularactivities.remarks;
                }
            }
            return View(remedialTeaching);
        }
    }
}
