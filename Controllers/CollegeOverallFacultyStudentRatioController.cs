using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeOverallFacultyStudentRatioController : BaseController
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

            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegeOverallFacultyStudentRatio> collegeOverallFacultyStudentRatio = new List<CollegeOverallFacultyStudentRatio>();

            foreach (var item in intake)
            {
                CollegeOverallFacultyStudentRatio newIntake = new CollegeOverallFacultyStudentRatio();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.totalFaculty = item.totalFaculty;
                newIntake.ratifiedFaculty = item.ratifiedFaculty;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeOverallFacultyStudentRatio.Add(newIntake);
            }

            collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            foreach (var item in collegeOverallFacultyStudentRatio)
            {

                item.totalFaculty = item.totalFaculty;
                item.ratifiedFaculty = item.ratifiedFaculty;

                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId);

                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId);

                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId);

                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId);

                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId);

                item.approvedIntake6 = GetIntake(userCollegeID, AY6, item.specializationId, item.shiftId);

            }
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
            if (collegeOverallFacultyStudentRatio.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeOverallFacultyStudentRatio.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "collegeOverallFacultyStudentRatio");
            }
            collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            ViewBag.collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio;
            ViewBag.Count = collegeOverallFacultyStudentRatio.Count();

            var did = db.jntuh_degree.Where(d => d.isActive == true && d.degree == "B.Tech").Select(d => d.id).FirstOrDefault();
            var degree = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.jntuh_degree.id == did).Select(cd => cd).FirstOrDefault();

            if (degree != null)
            {
                int shid = db.jntuh_specialization.Where(s => s.specializationName == "Humanities").Select(s => s.id).FirstOrDefault();
                var humanities = db.jntuh_college_overall_faculty_studentratio.Where(f => f.collegeId == userCollegeID && f.specializationId == shid).Select(f => f).ToList();
                if (humanities.Count() == 0)
                {
                    ViewBag.BTech = true;
                }
            }


            return View(collegeOverallFacultyStudentRatio);

        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId)
        {
            int intake = 0;
            intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.sanctionedIntake).FirstOrDefault();
            return intake;
        }



        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio = new CollegeOverallFacultyStudentRatio();

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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            ViewBag.IsUpdate = true;
            collegeOverallFacultyStudentRatio.collegeId = userCollegeID;
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.SixthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));


            if (id != null)
            {
                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

                foreach (var item in intake)
                {
                    collegeOverallFacultyStudentRatio.id = item.id;
                    collegeOverallFacultyStudentRatio.collegeId = item.collegeId;
                    collegeOverallFacultyStudentRatio.academicYearId = item.academicYearId;
                    collegeOverallFacultyStudentRatio.shiftId = item.shiftId;
                    collegeOverallFacultyStudentRatio.isActive = item.isActive;
                    collegeOverallFacultyStudentRatio.totalFaculty = item.totalFaculty;
                    collegeOverallFacultyStudentRatio.ratifiedFaculty = item.ratifiedFaculty;
                    collegeOverallFacultyStudentRatio.specializationId = item.specializationId;
                    collegeOverallFacultyStudentRatio.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.Department = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.degreeID = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.Degree = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.shiftId = item.shiftId;
                    collegeOverallFacultyStudentRatio.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }
                collegeOverallFacultyStudentRatio.totalFaculty = collegeOverallFacultyStudentRatio.totalFaculty;
                collegeOverallFacultyStudentRatio.ratifiedFaculty = collegeOverallFacultyStudentRatio.ratifiedFaculty;

                collegeOverallFacultyStudentRatio.approvedIntake1 = GetIntake(userCollegeID, AY1, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake2 = GetIntake(userCollegeID, AY2, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake3 = GetIntake(userCollegeID, AY3, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake4 = GetIntake(userCollegeID, AY4, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake5 = GetIntake(userCollegeID, AY5, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake6 = GetIntake(userCollegeID, AY6, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                int duration = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => (int)d.degreeDuration).FirstOrDefault();
                ViewBag.duration = duration;
            }
            else
            {
                ViewBag.IsUpdate = false;
            }

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
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Count = degrees.Count();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Create", collegeOverallFacultyStudentRatio);
            }
            else
            {
                return View("_Create", collegeOverallFacultyStudentRatio);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeOverallFacultyStudentRatio.collegeId;
            }
            if (ModelState.IsValid)
            {
                collegeOverallFacultyStudentRatio.collegeId = userCollegeID;
                int presentAY = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                int duration = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => (int)d.degreeDuration).FirstOrDefault();
                ViewBag.duration = duration;
                for (int i = 0; i < duration; i++)
                {
                    int approved = 0;
                    //int admitted = 0;
                    int academicYear = 0;

                    if (i == 0)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake1;
                        academicYear = presentAY + 1;
                        /*  int[] facultyIDs = db.jntuh_college_faculty.Where(f => f.collegeId == userCollegeID && f.facultyTypeId == 1).Select(f => f.id).ToArray();
                          var totalFacultyandratifiedFaculty = db.jntuh_faculty_subjects.Where(s => facultyIDs.Contains(s.facultyId)
                                                                                          && s.specializationId == collegeOverallFacultyStudentRatio.specializationId
                                                                                          && s.shiftId == collegeOverallFacultyStudentRatio.shiftId)
                                                                                          .Select(s => new
                                                                                          {
                                                                                              facultyID = s.facultyId,
                                                                                              FacultyRatifiedByJNTU = s.jntuh_college_faculty.isFacultyRatifiedByJNTU
                                                                                          }).Distinct().ToList();
                          int totalFaculty = totalFacultyandratifiedFaculty.Count();
                          int ratifiedFavulty = totalFacultyandratifiedFaculty.Where(s => s.FacultyRatifiedByJNTU == true).Select(s => s).Count();
                          collegeOverallFacultyStudentRatio.totalFaculty = totalFaculty;
                          collegeOverallFacultyStudentRatio.ratifiedFaculty = ratifiedFavulty;*/
                    }
                    if (i == 1)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake2;
                        academicYear = presentAY;
                    }
                    if (i == 2)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake3;
                        academicYear = presentAY - 1;
                    }
                    if (i == 3)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake4;
                        academicYear = presentAY - 2;
                    }
                    if (i == 4)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake5;
                        academicYear = presentAY - 3;
                    }
                    if (i == 5)
                    {
                        approved = collegeOverallFacultyStudentRatio.approvedIntake6;
                        academicYear = presentAY - 4;
                    }

                    jntuh_college_overall_faculty_studentratio jntuh_college_overall_faculty_studentratio = new jntuh_college_overall_faculty_studentratio();
                    jntuh_college_overall_faculty_studentratio.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();

                    var existingId = db.jntuh_college_overall_faculty_studentratio.Where(p => p.specializationId == collegeOverallFacultyStudentRatio.specializationId
                                                                                && p.shiftId == collegeOverallFacultyStudentRatio.shiftId
                                                                                && p.collegeId == collegeOverallFacultyStudentRatio.collegeId
                                                                                && p.academicYearId == jntuh_college_overall_faculty_studentratio.academicYearId).Select(p => p.id).FirstOrDefault();
                    int createdByu = Convert.ToInt32(db.jntuh_college_overall_faculty_studentratio.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdBy).FirstOrDefault());
                    DateTime createdonu = Convert.ToDateTime(db.jntuh_college_overall_faculty_studentratio.Where(a => a.collegeId == userCollegeID && a.id == existingId).Select(a => a.createdOn).FirstOrDefault());



                    //if ((approved > 0 || admitted > 0 && existingId == 0) || (existingId > 0))
                    if (existingId == 0 || existingId > 0)
                    {
                        jntuh_college_overall_faculty_studentratio.id = collegeOverallFacultyStudentRatio.id;
                        jntuh_college_overall_faculty_studentratio.collegeId = collegeOverallFacultyStudentRatio.collegeId;
                        jntuh_college_overall_faculty_studentratio.academicYearId = db.jntuh_academic_year.Where(a => a.actualYear == academicYear).Select(a => a.id).FirstOrDefault();
                        jntuh_college_overall_faculty_studentratio.specializationId = collegeOverallFacultyStudentRatio.specializationId;
                        jntuh_college_overall_faculty_studentratio.shiftId = collegeOverallFacultyStudentRatio.shiftId;
                        jntuh_college_overall_faculty_studentratio.sanctionedIntake = approved;
                        jntuh_college_overall_faculty_studentratio.totalFaculty = collegeOverallFacultyStudentRatio.totalFaculty;
                        jntuh_college_overall_faculty_studentratio.ratifiedFaculty = collegeOverallFacultyStudentRatio.ratifiedFaculty;
                        jntuh_college_overall_faculty_studentratio.isActive = true;

                        if (existingId == 0)
                        {
                            jntuh_college_overall_faculty_studentratio.createdBy = userID;
                            jntuh_college_overall_faculty_studentratio.createdOn = DateTime.Now;
                            db.jntuh_college_overall_faculty_studentratio.Add(jntuh_college_overall_faculty_studentratio);
                        }
                        else
                        {
                            jntuh_college_overall_faculty_studentratio.createdBy = createdByu;
                            jntuh_college_overall_faculty_studentratio.createdOn = createdonu;
                            jntuh_college_overall_faculty_studentratio.id = existingId;
                            jntuh_college_overall_faculty_studentratio.updatedBy = userID;
                            jntuh_college_overall_faculty_studentratio.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_overall_faculty_studentratio).State = EntityState.Modified;
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
        public ActionResult Details(int? id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio = new CollegeOverallFacultyStudentRatio();
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            if (id != null)
            {

                ViewBag.IsUpdate = true;

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

                foreach (var item in intake)
                {
                    collegeOverallFacultyStudentRatio.id = item.id;
                    collegeOverallFacultyStudentRatio.collegeId = item.collegeId;
                    collegeOverallFacultyStudentRatio.academicYearId = item.academicYearId;
                    collegeOverallFacultyStudentRatio.shiftId = item.shiftId;
                    collegeOverallFacultyStudentRatio.isActive = item.isActive;
                    collegeOverallFacultyStudentRatio.totalFaculty = item.totalFaculty;
                    collegeOverallFacultyStudentRatio.ratifiedFaculty = item.ratifiedFaculty;
                    collegeOverallFacultyStudentRatio.specializationId = item.specializationId;
                    collegeOverallFacultyStudentRatio.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.Department = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.degreeID = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.Degree = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => d.degree).FirstOrDefault();
                    collegeOverallFacultyStudentRatio.shiftId = item.shiftId;
                    collegeOverallFacultyStudentRatio.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }
                collegeOverallFacultyStudentRatio.totalFaculty = collegeOverallFacultyStudentRatio.totalFaculty;

                collegeOverallFacultyStudentRatio.ratifiedFaculty = collegeOverallFacultyStudentRatio.ratifiedFaculty;
                collegeOverallFacultyStudentRatio.approvedIntake1 = GetIntake(userCollegeID, AY1, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake2 = GetIntake(userCollegeID, AY2, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake3 = GetIntake(userCollegeID, AY3, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake4 = GetIntake(userCollegeID, AY4, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake5 = GetIntake(userCollegeID, AY5, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                collegeOverallFacultyStudentRatio.approvedIntake6 = GetIntake(userCollegeID, AY6, collegeOverallFacultyStudentRatio.specializationId, collegeOverallFacultyStudentRatio.shiftId);
                int duration = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => (int)d.degreeDuration).FirstOrDefault();
                ViewBag.duration = duration;
            }
            else
            {
                ViewBag.IsUpdate = false;
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
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Details", collegeOverallFacultyStudentRatio);
            }
            else
            {
                return View("Details", collegeOverallFacultyStudentRatio);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            int specid = db.jntuh_college_overall_faculty_studentratio.Where(p => p.id == id).Select(p => p.specializationId).FirstOrDefault();
            int shiftid = db.jntuh_college_overall_faculty_studentratio.Where(p => p.id == id).Select(p => p.shiftId).FirstOrDefault();
            List<jntuh_college_overall_faculty_studentratio> jntuh_college_overall_faculty_studentratio = db.jntuh_college_overall_faculty_studentratio.Where(p => p.specializationId == specid && p.shiftId == shiftid && p.collegeId == userCollegeID).ToList();
            foreach (var item in jntuh_college_overall_faculty_studentratio)
            {
                db.jntuh_college_overall_faculty_studentratio.Remove(item);
                db.SaveChanges();
                TempData["Success"] = "College Exissting Intake Deleted successfully";
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
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegeOverallFacultyStudentRatio> collegeOverallFacultyStudentRatio = new List<CollegeOverallFacultyStudentRatio>();

            foreach (var item in intake)
            {
                CollegeOverallFacultyStudentRatio newIntake = new CollegeOverallFacultyStudentRatio();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.totalFaculty = item.totalFaculty;
                newIntake.ratifiedFaculty = item.ratifiedFaculty;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeOverallFacultyStudentRatio.Add(newIntake);
            }

            collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio.AsEnumerable()
                                                         .GroupBy(r => new { r.specializationId, r.shiftId })
                                                         .Select(r => r.First())
                                                         .ToList();
            foreach (var item in collegeOverallFacultyStudentRatio)
            {
                item.totalFaculty = item.totalFaculty;
                item.ratifiedFaculty = item.ratifiedFaculty;
                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId);
                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId);
                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId);
                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId);
                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId);
                item.approvedIntake6 = GetIntake(userCollegeID, AY6, item.specializationId, item.shiftId);
            }
            collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            ViewBag.collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio;

            ViewBag.Count = collegeOverallFacultyStudentRatio.Count();
            return View(collegeOverallFacultyStudentRatio);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegeOverallFacultyStudentRatio> collegeOverallFacultyStudentRatio = new List<CollegeOverallFacultyStudentRatio>();

            foreach (var item in intake)
            {
                CollegeOverallFacultyStudentRatio newIntake = new CollegeOverallFacultyStudentRatio();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.totalFaculty = item.totalFaculty;
                newIntake.ratifiedFaculty = item.ratifiedFaculty;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeOverallFacultyStudentRatio.Add(newIntake);
            }

            collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            foreach (var item in collegeOverallFacultyStudentRatio)
            {

                item.totalFaculty = item.totalFaculty;
                item.ratifiedFaculty = item.ratifiedFaculty;
                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId);
                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId);
                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId);
                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId);
                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId);
                item.approvedIntake6 = GetIntake(userCollegeID, AY6, item.specializationId, item.shiftId);
            }
            ViewBag.collegeOverallFacultyStudentRatio = collegeOverallFacultyStudentRatio;
            ViewBag.Count = collegeOverallFacultyStudentRatio.Count();
            return View(collegeOverallFacultyStudentRatio);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSanctionedStrength(int DegreeId, int DepartmentId, int SpecializationId, int ShiftId, int collegeid)
        {
            SactionedStrength sactionedStrength = new SactionedStrength();
            sactionedStrength.propossedIntake = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeid
                                                                && p.specializationId == SpecializationId
                                                                && p.jntuh_specialization.jntuh_department.id == DepartmentId
                                                                && p.jntuh_specialization.jntuh_department.jntuh_degree.id == DegreeId
                                                                && p.shiftId == ShiftId
                                                                ).Select(p => p.proposedIntake).FirstOrDefault();

            int duration = db.jntuh_degree.Where(d => d.id == DegreeId).Select(d => (int)d.degreeDuration).FirstOrDefault();
            sactionedStrength.degreeduration = duration;
            List<ApprovedStrength> existingData = (from e in db.jntuh_college_intake_existing
                                                   join ay in db.jntuh_academic_year on e.academicYearId equals ay.id
                                                   where (e.collegeId == collegeid && e.specializationId == SpecializationId
                                                          && e.jntuh_specialization.jntuh_department.id == DepartmentId
                                                          && e.jntuh_specialization.jntuh_department.jntuh_degree.id == DegreeId
                                                          && e.shiftId == ShiftId)
                                                   select new ApprovedStrength
                                                   {
                                                       approvedIntake = e.approvedIntake,
                                                       actualYear = ay.actualYear,
                                                       academicYear = ay.academicYear,
                                                       duration = e.jntuh_specialization.jntuh_department.jntuh_degree.degreeDuration
                                                   }).OrderByDescending(ay => ay.actualYear).ToList();

            int sscount = existingData.Count();
            var sStrength = existingData;


            if (sscount >= duration)
            {
                sStrength = existingData.Take(duration - 1).ToList();
            }
            else
            {
                int pActualYear = db.jntuh_academic_year.Where(ay => ay.isPresentAcademicYear == true).Select(ay => ay.actualYear).FirstOrDefault();

                List<ApprovedStrength> newData = new List<ApprovedStrength>();
                int rowsToAdd = duration - 1;
                for (int i = 0; i < rowsToAdd; i++)
                {
                    int year = pActualYear - i;
                    ApprovedStrength ap = existingData.Where(e => e.actualYear == year).Select(e => e).FirstOrDefault();
                    if (ap == null)
                    {
                        ApprovedStrength nap = new ApprovedStrength();
                        newData.Add(nap);
                    }
                    else
                    {
                        newData.Add(ap);
                    }
                }

                sStrength = newData;
            }

            sactionedStrength.approveIntakeYearWise = existingData;
            sactionedStrength.approveIntakeYearWise = sStrength;
            var Specializationdata = sactionedStrength;
            return Json(Specializationdata, JsonRequestBehavior.AllowGet);
        }

        public class SactionedStrength
        {
            public int propossedIntake { get; set; }
            public int degreeduration { get; set; }
            public IEnumerable<ApprovedStrength> approveIntakeYearWise { get; set; }
        }
        public class ApprovedStrength
        {
            public int approvedIntake { get; set; }
            public int actualYear { get; set; }
            public string academicYear { get; set; }
            public decimal duration { get; set; }
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditFirstYearUGFaculty(int? id, string collegeId)
        {
            CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio = new CollegeOverallFacultyStudentRatio();
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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            ViewBag.IsUpdate = true;
            collegeOverallFacultyStudentRatio.collegeId = userCollegeID;
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            collegeOverallFacultyStudentRatio.academicYearId = AY1;

            if (id != null)
            {

                jntuh_college_overall_faculty_studentratio jntuh_college_overall_faculty_studentratio = db.jntuh_college_overall_faculty_studentratio.Find(id);
                collegeOverallFacultyStudentRatio.academicYearId = AY1;
                collegeOverallFacultyStudentRatio.approvedIntake1 = jntuh_college_overall_faculty_studentratio.collegeId;
                collegeOverallFacultyStudentRatio.specializationId = jntuh_college_overall_faculty_studentratio.specializationId;
                collegeOverallFacultyStudentRatio.DepartmentID = db.jntuh_specialization.Where(s => s.id == jntuh_college_overall_faculty_studentratio.specializationId).Select(s => s.departmentId).FirstOrDefault();
                collegeOverallFacultyStudentRatio.degreeID = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                collegeOverallFacultyStudentRatio.shiftId = jntuh_college_overall_faculty_studentratio.shiftId;
                collegeOverallFacultyStudentRatio.approvedIntake1 = jntuh_college_overall_faculty_studentratio.sanctionedIntake;
                collegeOverallFacultyStudentRatio.totalFaculty = jntuh_college_overall_faculty_studentratio.totalFaculty;
                collegeOverallFacultyStudentRatio.ratifiedFaculty = jntuh_college_overall_faculty_studentratio.ratifiedFaculty;
                collegeOverallFacultyStudentRatio.isActive = jntuh_college_overall_faculty_studentratio.isActive;
                collegeOverallFacultyStudentRatio.createdBy = jntuh_college_overall_faculty_studentratio.createdBy;
                collegeOverallFacultyStudentRatio.createdOn = jntuh_college_overall_faculty_studentratio.createdOn;

            }
            else
            {
                ViewBag.IsUpdate = false;
            }

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
            ViewBag.Degree = degrees.Where(d => d.degree == "B.Tech").OrderBy(d => d.degree).ToList();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true).ToList();
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Count = degrees.Count();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_AddEditFirstYearUGFaculty", collegeOverallFacultyStudentRatio);
            }
            else
            {
                return View("_AddEditFirstYearUGFaculty", collegeOverallFacultyStudentRatio);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditFirstYearUGFaculty(CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeOverallFacultyStudentRatio.collegeId;
            }
            if (ModelState.IsValid)
            {

                jntuh_college_overall_faculty_studentratio jntuh_college_overall_faculty_studentratio = new jntuh_college_overall_faculty_studentratio();
                jntuh_college_overall_faculty_studentratio.academicYearId = collegeOverallFacultyStudentRatio.academicYearId;
                jntuh_college_overall_faculty_studentratio.collegeId = collegeOverallFacultyStudentRatio.collegeId;
                jntuh_college_overall_faculty_studentratio.specializationId = collegeOverallFacultyStudentRatio.specializationId;
                jntuh_college_overall_faculty_studentratio.shiftId = collegeOverallFacultyStudentRatio.shiftId;
                jntuh_college_overall_faculty_studentratio.sanctionedIntake = collegeOverallFacultyStudentRatio.approvedIntake1;
                jntuh_college_overall_faculty_studentratio.totalFaculty = collegeOverallFacultyStudentRatio.totalFaculty;
                jntuh_college_overall_faculty_studentratio.ratifiedFaculty = collegeOverallFacultyStudentRatio.ratifiedFaculty;

                if (collegeOverallFacultyStudentRatio.id > 0)
                {
                    jntuh_college_overall_faculty_studentratio.id = collegeOverallFacultyStudentRatio.id;
                    jntuh_college_overall_faculty_studentratio.isActive = collegeOverallFacultyStudentRatio.isActive;
                    jntuh_college_overall_faculty_studentratio.createdBy = collegeOverallFacultyStudentRatio.createdBy;
                    jntuh_college_overall_faculty_studentratio.createdOn = collegeOverallFacultyStudentRatio.createdOn;
                    jntuh_college_overall_faculty_studentratio.updatedBy = userID;
                    jntuh_college_overall_faculty_studentratio.updatedOn = DateTime.Now;
                    db.Entry(jntuh_college_overall_faculty_studentratio).State = EntityState.Modified;
                }
                else
                {
                    jntuh_college_overall_faculty_studentratio.isActive = true;
                    jntuh_college_overall_faculty_studentratio.createdBy = userID;
                    jntuh_college_overall_faculty_studentratio.createdOn = DateTime.Now;
                    db.jntuh_college_overall_faculty_studentratio.Add(jntuh_college_overall_faculty_studentratio);
                }
                db.SaveChanges();
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
        [HttpGet]
        public ActionResult FirstYearUGFacultyDetails(int id, string collegeId)
        {
            CollegeOverallFacultyStudentRatio collegeOverallFacultyStudentRatio = new CollegeOverallFacultyStudentRatio();
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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_overall_faculty_studentratio.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            ViewBag.IsUpdate = true;
            collegeOverallFacultyStudentRatio.collegeId = userCollegeID;
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            collegeOverallFacultyStudentRatio.academicYearId = AY1;


            jntuh_college_overall_faculty_studentratio jntuh_college_overall_faculty_studentratio = db.jntuh_college_overall_faculty_studentratio.Find(id);
            collegeOverallFacultyStudentRatio.academicYearId = AY1;
            collegeOverallFacultyStudentRatio.approvedIntake1 = jntuh_college_overall_faculty_studentratio.collegeId;
            collegeOverallFacultyStudentRatio.specializationId = jntuh_college_overall_faculty_studentratio.specializationId;
            collegeOverallFacultyStudentRatio.Specialization = db.jntuh_specialization.Where(s => s.id == jntuh_college_overall_faculty_studentratio.specializationId).Select(s => s.specializationName).FirstOrDefault();
            collegeOverallFacultyStudentRatio.DepartmentID = db.jntuh_specialization.Where(s => s.id == jntuh_college_overall_faculty_studentratio.specializationId).Select(s => s.departmentId).FirstOrDefault();
            collegeOverallFacultyStudentRatio.Department = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
            collegeOverallFacultyStudentRatio.degreeID = db.jntuh_department.Where(d => d.id == collegeOverallFacultyStudentRatio.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
            collegeOverallFacultyStudentRatio.Degree = db.jntuh_degree.Where(d => d.id == collegeOverallFacultyStudentRatio.degreeID).Select(d => d.degree).FirstOrDefault();
            collegeOverallFacultyStudentRatio.shiftId = jntuh_college_overall_faculty_studentratio.shiftId;
            collegeOverallFacultyStudentRatio.Shift = db.jntuh_shift.Where(s => s.id == collegeOverallFacultyStudentRatio.shiftId).Select(s => s.shiftName).FirstOrDefault();            
            collegeOverallFacultyStudentRatio.approvedIntake1 = jntuh_college_overall_faculty_studentratio.sanctionedIntake;
            collegeOverallFacultyStudentRatio.totalFaculty = jntuh_college_overall_faculty_studentratio.totalFaculty;
            collegeOverallFacultyStudentRatio.ratifiedFaculty = jntuh_college_overall_faculty_studentratio.ratifiedFaculty;
            collegeOverallFacultyStudentRatio.isActive = jntuh_college_overall_faculty_studentratio.isActive;
            collegeOverallFacultyStudentRatio.createdBy = jntuh_college_overall_faculty_studentratio.createdBy;
            collegeOverallFacultyStudentRatio.createdOn = jntuh_college_overall_faculty_studentratio.createdOn;

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
            ViewBag.Degree = degrees.Where(d => d.degree == "B.Tech").OrderBy(d => d.degree).ToList();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true).ToList();
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Count = degrees.Count();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_FirstYearUGFacultyDetails", collegeOverallFacultyStudentRatio);
            }
            else
            {
                return View("_FirstYearUGFacultyDetails", collegeOverallFacultyStudentRatio);
            }
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

        private List<jntuh_department> Departments(int id)
        {
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id && d.departmentName == "Humanities").OrderBy(d => d.departmentName).ToList();
        }
    }
}
