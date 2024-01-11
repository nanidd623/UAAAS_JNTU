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
    public class FacultyStudentRatioController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /FacultyStudentRatio/FacultyStudentRatioCreate
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult FacultyStudentRatioCreate(string collegeId)
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
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();
            int existId = db.jntuh_college_faculty_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (userCollegeID > 0 && existId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("FacultyStudentRatioView", "FacultyStudentRatio");
            }
            if (userCollegeID > 0 && existId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("FacultyStudentRatioEdit", "FacultyStudentRatio", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (existId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            
            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> facultyStudentRatioDetails = new List<FacultyStudentRatio>();
            int totalIntake = 0;
            int totalFaculty = 0;
            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                totalIntake = GetIntake(item.degreeId, userCollegeID);
                totalFaculty = getTotalFaculty(item.degreeId, userCollegeID);
                facultyStudentRatio.totalIntake = totalIntake;
                facultyStudentRatio.totalFaculty = totalFaculty;

                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatioDetails.Add(facultyStudentRatio);

                //Data saved in  jntuh_college_faculty_student_ratio
                jntuh_college_faculty_student_ratio facultyStudentRatio1 = new jntuh_college_faculty_student_ratio();
                facultyStudentRatio1.collegeId = userCollegeID;
                facultyStudentRatio1.degreeId = item.degreeId;
                facultyStudentRatio1.totalIntake = totalIntake;
                facultyStudentRatio1.totalFaculty = totalFaculty;

                 existId = db.jntuh_college_faculty_student_ratio.Where(ratio => ratio.collegeId == userCollegeID &&
                                                                                    ratio.degreeId == item.degreeId)
                                                                   .Select(ratio => ratio.id)
                                                                   .FirstOrDefault();
                if (existId == 0)
                {
                    facultyStudentRatio1.createdBy = userID;
                    facultyStudentRatio1.createdOn = DateTime.Now;
                    db.jntuh_college_faculty_student_ratio.Add(facultyStudentRatio1);
                    db.SaveChanges();                   
                }

            }
            facultyStudentRatioDetails = facultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            ViewBag.Count = facultyStudentRatioDetails.Count();
            return View("~/Views/College/FacultyStudentRatioCreate.cshtml", facultyStudentRatioDetails);
        }
        //29/05/2014 Modification Start
        private int GetIntake(int degreeId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId).Select(e => e.shiftId).ToArray();
                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                }
                if (duration >= 5)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                }
                if (duration == 4)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                }
                if (duration == 3)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                }
                if (duration == 2)
                {
                    totalIntake += totalIntake1 + totalIntake2;
                }
                if (duration == 1)
                {
                    totalIntake += totalIntake1;
                }
            }

            return totalIntake;
        }

        private int getTotalFaculty(int degreeId, int collegeId)
        {
            int totalFaculty = 0;
            int[] fId;
            int otherDesignationId = db.jntuh_designation.Where(d => d.designation == "Other").Select(d => d.id).FirstOrDefault();
            int facultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            int[] facultyId = db.jntuh_college_faculty
                                .Where(f => f.collegeId == collegeId &&
                                            f.facultyTypeId == facultyTypeId &&
                                            f.facultyDesignationId != otherDesignationId)
                                .Select(f => f.id)
                                .ToArray();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            foreach (var item in specializationsId)
            {
                int[] shiftId = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == item).Select(e => e.shiftId).ToArray();
                foreach (var sId in shiftId)
                {
                    fId = db.jntuh_faculty_subjects.Where(s => facultyId.Contains(s.facultyId) && s.specializationId == item &&
                                                                   s.shiftId == sId)
                                                        .Select(s => s.facultyId)
                                                        .Distinct()
                                                        .ToArray();
                    totalFaculty += fId.Count();
                }
            }
            return totalFaculty;
        }
        //29/05/2014 Modification End
        //
        // POST: /FacultyStudentRatio/FacultyStudentRatioCreate
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult FacultyStudentRatioCreate(ICollection<FacultyStudentRatio> facultyStudentRatioDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in facultyStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveFacultyStudentRatioDetails(facultyStudentRatioDetails);
            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> collegeFacultyStudentRatioDetails = new List<FacultyStudentRatio>();

            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                facultyStudentRatio.totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalIntake)
                                                                                        .FirstOrDefault();
                facultyStudentRatio.totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalFaculty)
                                                                                        .FirstOrDefault();
                collegeFacultyStudentRatioDetails.Add(facultyStudentRatio);
            }
            collegeFacultyStudentRatioDetails = collegeFacultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            ViewBag.Count = collegeFacultyStudentRatioDetails.Count();
            return View("~/Views/College/FacultyStudentRatioCreate.cshtml", collegeFacultyStudentRatioDetails);
        }

        private void SaveFacultyStudentRatioDetails(ICollection<FacultyStudentRatio> facultyStudentRatioDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in facultyStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            var message = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (FacultyStudentRatio item in facultyStudentRatioDetails)
                {
                    jntuh_college_faculty_student_ratio facultyStudentRatio = new jntuh_college_faculty_student_ratio();
                    facultyStudentRatio.collegeId = userCollegeID;
                    facultyStudentRatio.degreeId = item.degreeId;
                    facultyStudentRatio.totalIntake = item.totalIntake;
                    facultyStudentRatio.totalFaculty = item.totalFaculty;

                    int existId = db.jntuh_college_faculty_student_ratio.Where(ratio => ratio.collegeId == userCollegeID &&
                                                                                        ratio.degreeId == item.degreeId)
                                                                       .Select(ratio => ratio.id)
                                                                       .FirstOrDefault();

                    if (existId == 0)
                    {
                        facultyStudentRatio.createdBy = userID;
                        facultyStudentRatio.createdOn = DateTime.Now;
                        db.jntuh_college_faculty_student_ratio.Add(facultyStudentRatio);
                        db.SaveChanges();
                        message = "Save";
                    }
                    else
                    {
                        facultyStudentRatio.id = existId; ;
                        facultyStudentRatio.createdOn = db.jntuh_college_faculty_student_ratio.Where(d => d.id == existId).Select(d => d.createdOn).FirstOrDefault();
                        facultyStudentRatio.createdBy = db.jntuh_college_faculty_student_ratio.Where(d => d.id == existId).Select(d => d.createdBy).FirstOrDefault();
                        facultyStudentRatio.updatedBy = userID;
                        facultyStudentRatio.updatedOn = DateTime.Now;
                        db.Entry(facultyStudentRatio).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Update";
                    }
                }
                if (message == "Update")
                {
                    TempData["Success"] = "Faculty Student Ratio Details are Updated successfully";
                }
                else
                {
                    TempData["Success"] = "Faculty Student Ratio Details are Added successfully";
                }
            }
        }

        //
        // GET: /FacultyStudentRatio/FacultyStudentRatioEdit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult FacultyStudentRatioEdit(string collegeId)
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
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();

            int existId = db.jntuh_college_faculty_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (existId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("FacultyStudentRatioCreate", "FacultyStudentRatio");
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("FacultyStudentRatioView", "FacultyStudentRatio");
            }
            else
            {
                ViewBag.IsEditable = true;
            }

            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> collegeFacultyStudentRatioDetails = new List<FacultyStudentRatio>();

            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                facultyStudentRatio.totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalIntake)
                                                                                        .FirstOrDefault();
                facultyStudentRatio.totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalFaculty)
                                                                                        .FirstOrDefault();
                collegeFacultyStudentRatioDetails.Add(facultyStudentRatio);
            }
            collegeFacultyStudentRatioDetails = collegeFacultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            ViewBag.Count = collegeFacultyStudentRatioDetails.Count();
            ViewBag.Update = true;
            return View("~/Views/College/FacultyStudentRatioCreate.cshtml", collegeFacultyStudentRatioDetails);      
        }

        //
        // POST: /FacultyStudentRatio/FacultyStudentRatioEdit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult FacultyStudentRatioEdit(ICollection<FacultyStudentRatio> facultyStudentRatioDetails)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in facultyStudentRatioDetails)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveFacultyStudentRatioDetails(facultyStudentRatioDetails);
            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> collegeFacultyStudentRatioDetails = new List<FacultyStudentRatio>();

            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                facultyStudentRatio.totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalIntake)
                                                                                        .FirstOrDefault();
                facultyStudentRatio.totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalFaculty)
                                                                                        .FirstOrDefault();
                collegeFacultyStudentRatioDetails.Add(facultyStudentRatio);
            }
            collegeFacultyStudentRatioDetails = collegeFacultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            ViewBag.Count = collegeFacultyStudentRatioDetails.Count();
            ViewBag.Update = true;
            return View("~/Views/College/FacultyStudentRatioCreate.cshtml", collegeFacultyStudentRatioDetails);            
        }

        //
        // GET: /FacultyStudentRatio/FacultyStudentRatioView
        [Authorize(Roles = "College,Committee,DataEntry,Admin")]
        [HttpGet]
        public ActionResult FacultyStudentRatioView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();

            int existId = db.jntuh_college_faculty_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> collegeFacultyStudentRatioDetails = new List<FacultyStudentRatio>();

            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                facultyStudentRatio.totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalIntake)
                                                                                        .FirstOrDefault();
                facultyStudentRatio.totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalFaculty)
                                                                                        .FirstOrDefault();
                collegeFacultyStudentRatioDetails.Add(facultyStudentRatio);
            }
            if (existId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = collegeFacultyStudentRatioDetails.Count();
                ViewBag.Update = true;
            }
            collegeFacultyStudentRatioDetails = collegeFacultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            return View("~/Views/College/FacultyStudentRatioView.cshtml", collegeFacultyStudentRatioDetails);
        }

        public ActionResult UserFacultyStudentRatioView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int[] degree = db.jntuh_college_degree.Where(d => d.isActive == true && d.collegeId == userCollegeID).Select(d => d.degreeId).ToArray();

            int existId = db.jntuh_college_faculty_student_ratio.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == userCollegeID && d.isActive == true).ToList();

            List<FacultyStudentRatio> collegeFacultyStudentRatioDetails = new List<FacultyStudentRatio>();

            foreach (var item in collegeDegree)
            {
                FacultyStudentRatio facultyStudentRatio = new FacultyStudentRatio();
                facultyStudentRatio.id = 0;
                facultyStudentRatio.degreeId = item.degreeId;
                facultyStudentRatio.collegeId = userCollegeID;
                facultyStudentRatio.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                facultyStudentRatio.totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalIntake)
                                                                                        .FirstOrDefault();
                facultyStudentRatio.totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == userCollegeID && f.degreeId == item.degreeId)
                                                                                        .Select(f => f.totalFaculty)
                                                                                        .FirstOrDefault();
                collegeFacultyStudentRatioDetails.Add(facultyStudentRatio);
            }
            if (existId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Count = collegeFacultyStudentRatioDetails.Count();
            }
            collegeFacultyStudentRatioDetails = collegeFacultyStudentRatioDetails.OrderBy(f => f.degree).ToList();
            return View("~/Views/College/UserFacultyStudentRatioView.cshtml", collegeFacultyStudentRatioDetails);
        }
    }
}
