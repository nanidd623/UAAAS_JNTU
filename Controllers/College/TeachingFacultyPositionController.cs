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
    public class TeachingFacultyPositionController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private void collegeTeachingFacultyPosition(int collegeId)
        {
            int[] fId;
            string teachingFacultyPosition = string.Empty;
            int ProfessorDesignationId = 0;
            int assistentProfessorDesignationId = 0;
            int associateProfessorDesignationId = 0;
            int designationId = 0;
            int[] facultyId;
            bool professorRatified;
            int total = 0;
            bool assistantProfessorRatified;
            bool associateProfessorRatified;
            int ProfessorCount = 0;
            int assistentProfessorCount = 0;
            int associateProfessorCount = 0;
            int ProfessorRatifiedCount = 0;
            int assistentProfessorRatifiedCount = 0;
            int associateProfessorRatifiedCount = 0;
            int facultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            ProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Professor").Select(d => d.id).FirstOrDefault();
            assistentProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Assistant Professor").Select(d => d.id).FirstOrDefault();
            associateProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Associate Professor").Select(d => d.id).FirstOrDefault();
            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId).ToList();

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
                collegeIntakeProposedList.Add(newProposed);
            }
            //collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(o => o.Degree).ToList();
            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            facultyId = db.jntuh_college_faculty.Where(f => f.collegeId == collegeId && f.facultyTypeId == facultyTypeId).Select(f => f.id).ToArray();
            foreach (var item in collegeIntakeProposedList)
            {
                ProfessorCount = 0;
                assistentProfessorCount = 0;
                associateProfessorCount = 0;
                ProfessorRatifiedCount = 0;
                assistentProfessorRatifiedCount = 0;
                associateProfessorRatifiedCount = 0;
                fId = db.jntuh_faculty_subjects.Where(s => facultyId.Contains(s.facultyId) && s.specializationId == item.specializationId &&
                                                               s.shiftId == item.shiftId)
                                                    .Select(s => s.facultyId)
                                                    .Distinct()
                                                    .ToArray();
                foreach (var id in fId)
                {
                    designationId = db.jntuh_college_faculty.Where(f => f.id == id).Select(d => d.facultyDesignationId).FirstOrDefault();

                    if (designationId == ProfessorDesignationId)
                    {
                        ProfessorCount++;
                        professorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                        if (professorRatified == true)
                        {
                            ProfessorRatifiedCount++;
                        }
                    }
                    if (designationId == assistentProfessorDesignationId)
                    {
                        assistentProfessorCount++;
                        assistantProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                        if (assistantProfessorRatified == true)
                        {
                            assistentProfessorRatifiedCount++;
                        }
                    }
                    if (designationId == associateProfessorDesignationId)
                    {
                        associateProfessorCount++;
                        associateProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                        if (associateProfessorRatified == true)
                        {
                            associateProfessorRatifiedCount++;
                        }
                    }
                }
                string FacultyStudentRation = "1 : ";
                int Ratio = 0;
                int Intake = FacultyTotalIntake(item.degreeID, item.specializationId, item.shiftId, collegeId);
                total = ProfessorRatifiedCount + assistentProfessorRatifiedCount + associateProfessorRatifiedCount;
                jntuh_college_teaching_faculty_position TeachingFacultyPositions = new jntuh_college_teaching_faculty_position();
                TeachingFacultyPositions.collegeId = collegeId;
                TeachingFacultyPositions.specializationId = item.specializationId;
                TeachingFacultyPositions.shiftId = item.shiftId;
                TeachingFacultyPositions.intake = Intake;
                TeachingFacultyPositions.professors = ProfessorCount;
                TeachingFacultyPositions.assocProfessors = associateProfessorCount;
                TeachingFacultyPositions.asstProfessors = assistentProfessorCount;
                TeachingFacultyPositions.ratified = total;
                if (Intake != 0 && (ProfessorCount + associateProfessorCount + assistentProfessorCount) != 0)
                {
                    Ratio = (Intake) / (ProfessorCount + associateProfessorCount + assistentProfessorCount);
                    TeachingFacultyPositions.facultyStudentRatio = FacultyStudentRation + Convert.ToString(Ratio);
                }
                else
                {
                    TeachingFacultyPositions.facultyStudentRatio = "0:0";
                }
                TeachingFacultyPositions.facultyStudentRatio = FacultyStudentRation + Convert.ToString(Ratio);
                db.jntuh_college_teaching_faculty_position.Add(TeachingFacultyPositions);
                db.SaveChanges();
            }
        }

        private int FacultyTotalIntake(int degreeId, int specializationId, int shiftId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();

            int totalIntake1 = 0;
            int totalIntake2 = 0;
            int totalIntake3 = 0;
            int totalIntake4 = 0;
            int totalIntake5 = 0;

            totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.proposedIntake).FirstOrDefault();
            totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
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
            return totalIntake;
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult TeachingFacultyPositionIndex(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            List<jntuh_college_teaching_faculty_position> teachingFacultypositions = db.jntuh_college_teaching_faculty_position.Where(p => p.collegeId == userCollegeID).ToList();
            if (teachingFacultypositions.Count() == 0)
            {
                collegeTeachingFacultyPosition(userCollegeID);
            }
            teachingFacultypositions = db.jntuh_college_teaching_faculty_position.Where(p => p.collegeId == userCollegeID).ToList();

            List<TeachingFacultyPosition> teachingFacultypositionsList = new List<TeachingFacultyPosition>();
            foreach (var item in teachingFacultypositions)
            {
                TeachingFacultyPosition teachingFacultyPosition = new TeachingFacultyPosition();
                teachingFacultyPosition.id = item.id;
                teachingFacultyPosition.collegeId = item.collegeId;
                teachingFacultyPosition.specializationId = item.specializationId;
                teachingFacultyPosition.specializationName = db.jntuh_specialization.Where(s => s.isActive == true
                                                                                                && s.id == teachingFacultyPosition.specializationId)
                                                                                    .Select(s => s.specializationName)
                                                                                    .FirstOrDefault();
                teachingFacultyPosition.departmentId = db.jntuh_specialization.Where(s => s.isActive == true &&
                                                                                          s.id == teachingFacultyPosition.specializationId)
                                                                               .Select(s => s.departmentId)
                                                                               .FirstOrDefault();
                teachingFacultyPosition.departmentName = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                        d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.departmentName)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeId = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                  d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.degreeId)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeName = db.jntuh_degree.Where(d => d.isActive == true &&
                                                                                d.id == teachingFacultyPosition.degreeId)
                                                                    .Select(d => d.degree)
                                                                    .FirstOrDefault();
                teachingFacultyPosition.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == teachingFacultyPosition.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                teachingFacultyPosition.shiftId = item.shiftId;
                teachingFacultyPosition.shiftName = db.jntuh_shift.Where(s => s.isActive == true && s.id == item.shiftId)
                                                                  .Select(s => s.shiftName)
                                                                  .FirstOrDefault();
                teachingFacultyPosition.intake = item.intake;
                teachingFacultyPosition.professors = item.professors;
                teachingFacultyPosition.assocProfessors = item.assocProfessors;
                teachingFacultyPosition.asstProfessors = item.asstProfessors;
                teachingFacultyPosition.ratified = item.ratified;
                teachingFacultyPosition.facultyStudentRatio = item.facultyStudentRatio;
                teachingFacultypositionsList.Add(teachingFacultyPosition);
            }
            teachingFacultypositionsList = teachingFacultypositionsList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.departmentName).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ToList();
            ViewBag.Count = teachingFacultypositionsList.Count();
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
            if (teachingFacultypositionsList.Count() == 0 && status == 0 && (Roles.IsUserInRole("College")))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            if (status == 0 && teachingFacultypositionsList.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("TeachingFacultyPositionView", "TeachingFacultyPosition");
            }
            ViewBag.TeachingFacultyPosition = teachingFacultypositionsList;

            return View("~/Views/College/TeachingFacultyPositionIndex.cshtml", teachingFacultypositionsList);
        }

        //
        // GET: /TeachingFacultyPosition/TeachingFacultyPositionAddOrEdit
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult TeachingFacultyPositionAddOrEdit(int? id, string collegeId)
        {
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
                        userCollegeID = db.jntuh_college_teaching_faculty_position.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            TeachingFacultyPosition teachingFacultyPosition = new TeachingFacultyPosition();
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
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            if (id != null)
            {
                ViewBag.IsUpdate = true;
                jntuh_college_teaching_faculty_position collegeTeachingFacultyPosition = db.jntuh_college_teaching_faculty_position.Where(t => t.collegeId == userCollegeID &&
                                                                                                                                        t.id == id)
                                                                                                                            .FirstOrDefault();
                teachingFacultyPosition.id = collegeTeachingFacultyPosition.id;
                teachingFacultyPosition.collegeId = userCollegeID;
                teachingFacultyPosition.specializationId = collegeTeachingFacultyPosition.specializationId;
                teachingFacultyPosition.departmentId = db.jntuh_specialization.Where(s => s.isActive == true && s.id == teachingFacultyPosition.specializationId)
                                                                              .Select(s => s.departmentId)
                                                                              .FirstOrDefault();
                teachingFacultyPosition.degreeId = db.jntuh_department.Where(d => d.isActive == true && d.id == teachingFacultyPosition.departmentId)
                                                                      .Select(d => d.degreeId)
                                                                      .FirstOrDefault();
                teachingFacultyPosition.shiftId = collegeTeachingFacultyPosition.shiftId;
                teachingFacultyPosition.intake = collegeTeachingFacultyPosition.intake;
                teachingFacultyPosition.professors = collegeTeachingFacultyPosition.professors;
                teachingFacultyPosition.assocProfessors = collegeTeachingFacultyPosition.assocProfessors;
                teachingFacultyPosition.asstProfessors = collegeTeachingFacultyPosition.asstProfessors;
                teachingFacultyPosition.ratified = collegeTeachingFacultyPosition.ratified;
                teachingFacultyPosition.facultyStudentRatio = collegeTeachingFacultyPosition.facultyStudentRatio;
                teachingFacultyPosition.createdBy = collegeTeachingFacultyPosition.createdBy;
                teachingFacultyPosition.createdOn = collegeTeachingFacultyPosition.createdOn;
            }
            else
            {
                teachingFacultyPosition.collegeId = userCollegeID;
                ViewBag.IsUpdate = false;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("~/Views/College/_TeachingFacultyPositionAddOrEdit.cshtml", teachingFacultyPosition);
            }
            else
            {
                return View("~/Views/College/TeachingFacultyPositionAddOrEdit.cshtml", teachingFacultyPosition);
            }
        }

        //
        // POST: /TeachingFacultyPosition/TeachingFacultyPositionAddOrEdit
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult TeachingFacultyPositionAddOrEdit(TeachingFacultyPosition teachingFacultyPosition, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = teachingFacultyPosition.collegeId;
            }
            if (ModelState.IsValid)
            {
                if (cmd == "Add")
                {
                    int existSpecializationAndShiftId = db.jntuh_college_teaching_faculty_position.Where(p => p.specializationId == teachingFacultyPosition.specializationId &&
                                                                                   p.shiftId == teachingFacultyPosition.shiftId &&
                                                                                   p.collegeId == userCollegeID)
                                                                       .Select(p => p.id)
                                                                       .FirstOrDefault();
                    if (existSpecializationAndShiftId > 0)
                    {
                        TempData["Error"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                    }
                    else
                    {
                        jntuh_college_teaching_faculty_position collegeTeachingFacultyPosition = new jntuh_college_teaching_faculty_position();
                        collegeTeachingFacultyPosition.collegeId = userCollegeID;
                        collegeTeachingFacultyPosition.specializationId = teachingFacultyPosition.specializationId;
                        collegeTeachingFacultyPosition.shiftId = teachingFacultyPosition.shiftId;
                        collegeTeachingFacultyPosition.intake = teachingFacultyPosition.intake;
                        collegeTeachingFacultyPosition.professors = teachingFacultyPosition.professors;
                        collegeTeachingFacultyPosition.assocProfessors = teachingFacultyPosition.assocProfessors;
                        collegeTeachingFacultyPosition.asstProfessors = teachingFacultyPosition.asstProfessors;
                        collegeTeachingFacultyPosition.ratified = teachingFacultyPosition.ratified;
                        collegeTeachingFacultyPosition.facultyStudentRatio = teachingFacultyPosition.facultyStudentRatio;
                        collegeTeachingFacultyPosition.createdBy = userID;
                        collegeTeachingFacultyPosition.createdOn = DateTime.Now;
                        db.jntuh_college_teaching_faculty_position.Add(collegeTeachingFacultyPosition);
                        db.SaveChanges();
                        TempData["Success"] = "College Teaching Faculty Position Details are Added successfully.";
                    }
                    return RedirectToAction("TeachingFacultyPositionIndex", "TeachingFacultyPosition", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                else
                {
                    int existSpecializationAndShiftId = db.jntuh_college_teaching_faculty_position.Where(p => p.specializationId == teachingFacultyPosition.specializationId &&
                                                                                   p.shiftId == teachingFacultyPosition.shiftId &&
                                                                                   p.collegeId == userCollegeID &&
                                                                                   p.id != teachingFacultyPosition.id)
                                                                       .Select(p => p.id)
                                                                       .FirstOrDefault();
                    if (existSpecializationAndShiftId > 0)
                    {
                        TempData["Error"] = "Specialization and shift is already exists . Please enter a different Specialization and shift";
                    }
                    else
                    {
                        jntuh_college_teaching_faculty_position collegeTeachingFacultyPosition = new jntuh_college_teaching_faculty_position();
                        collegeTeachingFacultyPosition.id = teachingFacultyPosition.id;
                        collegeTeachingFacultyPosition.collegeId = userCollegeID;
                        collegeTeachingFacultyPosition.specializationId = teachingFacultyPosition.specializationId;
                        collegeTeachingFacultyPosition.shiftId = teachingFacultyPosition.shiftId;
                        collegeTeachingFacultyPosition.intake = teachingFacultyPosition.intake;
                        collegeTeachingFacultyPosition.professors = teachingFacultyPosition.professors;
                        collegeTeachingFacultyPosition.assocProfessors = teachingFacultyPosition.assocProfessors;
                        collegeTeachingFacultyPosition.asstProfessors = teachingFacultyPosition.asstProfessors;
                        collegeTeachingFacultyPosition.ratified = teachingFacultyPosition.ratified;
                        collegeTeachingFacultyPosition.facultyStudentRatio = teachingFacultyPosition.facultyStudentRatio;
                        collegeTeachingFacultyPosition.createdBy = teachingFacultyPosition.createdBy;
                        collegeTeachingFacultyPosition.createdOn = teachingFacultyPosition.createdOn;
                        collegeTeachingFacultyPosition.updatedBy = userID;
                        collegeTeachingFacultyPosition.updatedOn = DateTime.Now;
                        db.Entry(collegeTeachingFacultyPosition).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "College Teaching Faculty Position Details are Updated successfully.";
                    }
                    return RedirectToAction("TeachingFacultyPositionIndex", "TeachingFacultyPosition", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
            }
            else
            {
                return RedirectToAction("TeachingFacultyPositionIndex", "TeachingFacultyPosition", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
        }

        //
        // GET: /TeachingFacultyPosition/TeachingFacultyPositionDelete
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult TeachingFacultyPositionDelete(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            jntuh_college_teaching_faculty_position teachinfFacultyPosition = db.jntuh_college_teaching_faculty_position.Where(t => t.id == id)
                                                                                                                     .FirstOrDefault();
            int userCollegeID = db.jntuh_college_teaching_faculty_position.Where(p => p.id == id).Select(p => p.collegeId).FirstOrDefault();
            if (teachinfFacultyPosition != null)
            {
                db.jntuh_college_teaching_faculty_position.Remove(teachinfFacultyPosition);
                db.SaveChanges();
                TempData["Success"] = "College Teaching Faculty Position Details are Deleted successfully.";
            }
            return RedirectToAction("TeachingFacultyPositionIndex", "TeachingFacultyPosition", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        //
        // GET: /TeachingFacultyPosition/TeachingFacultyPositionView
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult TeachingFacultyPositionView(string id)
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
                return RedirectToAction("TeachingFacultyPositionIndex", "TeachingFacultyPosition");
            }
            List<jntuh_college_teaching_faculty_position> teachingFacultypositions = db.jntuh_college_teaching_faculty_position.Where(p => p.collegeId == userCollegeID).ToList();

            List<TeachingFacultyPosition> teachingFacultypositionsList = new List<TeachingFacultyPosition>();

            foreach (var item in teachingFacultypositions)
            {
                TeachingFacultyPosition teachingFacultyPosition = new TeachingFacultyPosition();
                teachingFacultyPosition.id = item.id;
                teachingFacultyPosition.collegeId = item.collegeId;
                teachingFacultyPosition.specializationId = item.specializationId;
                teachingFacultyPosition.specializationName = db.jntuh_specialization.Where(s => s.isActive == true
                                                                                                && s.id == teachingFacultyPosition.specializationId)
                                                                                    .Select(s => s.specializationName)
                                                                                    .FirstOrDefault();
                teachingFacultyPosition.departmentId = db.jntuh_specialization.Where(s => s.isActive == true &&
                                                                                          s.id == teachingFacultyPosition.specializationId)
                                                                               .Select(s => s.departmentId)
                                                                               .FirstOrDefault();
                teachingFacultyPosition.departmentName = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                        d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.departmentName)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeId = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                  d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.degreeId)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeName = db.jntuh_degree.Where(d => d.isActive == true &&
                                                                                d.id == teachingFacultyPosition.degreeId)
                                                                    .Select(d => d.degree)
                                                                    .FirstOrDefault();
                teachingFacultyPosition.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == teachingFacultyPosition.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                teachingFacultyPosition.shiftId = item.shiftId;
                teachingFacultyPosition.shiftName = db.jntuh_shift.Where(s => s.isActive == true && s.id == item.shiftId)
                                                                  .Select(s => s.shiftName)
                                                                  .FirstOrDefault();
                teachingFacultyPosition.intake = item.intake;
                teachingFacultyPosition.professors = item.professors;
                teachingFacultyPosition.assocProfessors = item.assocProfessors;
                teachingFacultyPosition.asstProfessors = item.asstProfessors;
                teachingFacultyPosition.ratified = item.ratified;
                teachingFacultyPosition.facultyStudentRatio = item.facultyStudentRatio;
                teachingFacultypositionsList.Add(teachingFacultyPosition);
            }
            teachingFacultypositionsList = teachingFacultypositionsList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.departmentName).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ToList();
            ViewBag.Count = teachingFacultypositionsList.Count();
            ViewBag.TeachingFacultyPosition = teachingFacultypositionsList;
            return View("~/Views/College/TeachingFacultyPositionView.cshtml", teachingFacultypositionsList);
        }

        public ActionResult UserTeachingFacultyPositionView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            List<jntuh_college_teaching_faculty_position> teachingFacultypositions = db.jntuh_college_teaching_faculty_position.Where(p => p.collegeId == userCollegeID).ToList();

            List<TeachingFacultyPosition> teachingFacultypositionsList = new List<TeachingFacultyPosition>();

            foreach (var item in teachingFacultypositions)
            {
                TeachingFacultyPosition teachingFacultyPosition = new TeachingFacultyPosition();
                teachingFacultyPosition.id = item.id;
                teachingFacultyPosition.collegeId = item.collegeId;
                teachingFacultyPosition.specializationId = item.specializationId;
                teachingFacultyPosition.specializationName = db.jntuh_specialization.Where(s => s.isActive == true
                                                                                                && s.id == teachingFacultyPosition.specializationId)
                                                                                    .Select(s => s.specializationName)
                                                                                    .FirstOrDefault();
                teachingFacultyPosition.departmentId = db.jntuh_specialization.Where(s => s.isActive == true &&
                                                                                          s.id == teachingFacultyPosition.specializationId)
                                                                               .Select(s => s.departmentId)
                                                                               .FirstOrDefault();
                teachingFacultyPosition.departmentName = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                        d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.departmentName)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeId = db.jntuh_department.Where(d => d.isActive == true &&
                                                                                  d.id == teachingFacultyPosition.departmentId)
                                                                            .Select(d => d.degreeId)
                                                                            .FirstOrDefault();
                teachingFacultyPosition.degreeName = db.jntuh_degree.Where(d => d.isActive == true &&
                                                                                d.id == teachingFacultyPosition.degreeId)
                                                                    .Select(d => d.degree)
                                                                    .FirstOrDefault();
                teachingFacultyPosition.shiftId = item.shiftId;
                teachingFacultyPosition.shiftName = db.jntuh_shift.Where(s => s.isActive == true && s.id == item.shiftId)
                                                                  .Select(s => s.shiftName)
                                                                  .FirstOrDefault();
                teachingFacultyPosition.intake = item.intake;
                teachingFacultyPosition.professors = item.professors;
                teachingFacultyPosition.assocProfessors = item.assocProfessors;
                teachingFacultyPosition.asstProfessors = item.asstProfessors;
                teachingFacultyPosition.ratified = item.ratified;
                teachingFacultyPosition.facultyStudentRatio = item.facultyStudentRatio;
                teachingFacultypositionsList.Add(teachingFacultyPosition);
            }

            ViewBag.Count = teachingFacultypositionsList.Count();
            ViewBag.TeachingFacultyPosition = teachingFacultypositionsList;
            return View("~/Views/College/UserTeachingFacultyPositionView.cshtml", teachingFacultypositionsList);
        }
    }
}
