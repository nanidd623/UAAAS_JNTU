using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class MissedProposedCoursesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /MissedProposedCourses/
        [Authorize(Roles = "College,Admin")]
        public ActionResult MissedProposedCoursesIndex(string id)
        {
            string presentAcademicYear = string.Empty;
            string nextAcademicYear = string.Empty;
            int actualYear = 0;
            int presentAcademicYearId = 0;
            int nextAcademicYearId = 0;
            int existProposedIntakeId = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (id != null)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            presentAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            presentAcademicYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            nextAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1))
                                                        .Select(a => a.id).FirstOrDefault();
            nextAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1))
                                                        .Select(a => a.academicYear).FirstOrDefault();
            

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && 
                                                                                                i.academicYearId == presentAcademicYearId).ToList();

            List<MissedProposedCourses> collegeIntakeExisting = new List<MissedProposedCourses>();

            foreach (var item in intake)
            {
                MissedProposedCourses newIntake = new MissedProposedCourses();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.collegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;
                newIntake.shiftId = item.shiftId;
                newIntake.specializationId = item.specializationId;
                newIntake.specilization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.department = db.jntuh_department.Where(d => d.id == newIntake.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeId = db.jntuh_department.Where(d => d.id == newIntake.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newIntake.degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeId).Select(d => d.degree).FirstOrDefault();
                newIntake.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            foreach (var item in collegeIntakeExisting)
            {
                item.existingIntake = db.jntuh_college_intake_existing.Where(e => e.collegeId == item.collegeId &&
                                                                                  e.academicYearId == presentAcademicYearId &&
                                                                                  e.shiftId == item.shiftId &&
                                                                                  e.specializationId == item.specializationId)
                                                                      .Select(e => e.approvedIntake)
                                                                      .FirstOrDefault();
                item.proposedIntake = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID &&
                                                                                  p.academicYearId == nextAcademicYearId &&
                                                                                  p.shiftId == item.shiftId &&
                                                                                  p.specializationId == item.specializationId)
                                                                      .Select(p => p.proposedIntake)
                                                                      .FirstOrDefault();
                existProposedIntakeId = db.jntuh_college_intake_proposed.Where(p => p.collegeId == userCollegeID &&
                                                                                  p.academicYearId == nextAcademicYearId &&
                                                                                  p.shiftId == item.shiftId &&
                                                                                  p.specializationId == item.specializationId)
                                                                      .Select(p => p.id)               
                                                                      .FirstOrDefault();
                if (existProposedIntakeId != 0)
                {
                    item.status = true;
                }
                else
                {
                    item.status = false;
                }
            }
            collegeIntakeExisting = collegeIntakeExisting.OrderBy(e => e.degree).ToList();
            ViewBag.Count = collegeIntakeExisting.Count();
            ViewBag.PresentAcademicYear = presentAcademicYear;
            ViewBag.NextAcademicYear = nextAcademicYear;
            return View("~/Views/College/MissedProposedCoursesIndex.cshtml", collegeIntakeExisting);
        }

    }
}
