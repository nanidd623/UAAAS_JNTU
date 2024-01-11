using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class AddMissedCoursesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult MissedCourses()
        {
            MissedCourses missedCourses = new Models.MissedCourses();
            ViewBag.collegeCodes = db.jntuh_college.Where(c => c.isActive == true).OrderBy(c => c.collegeCode)
                                                .Select(c => c).ToList();
            return View("~/Views/Admin/MissedCourses.cshtml", missedCourses);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult MissedCourses(MissedCourses missedCourses)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int academicYearId = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.actualYear).FirstOrDefault();
            int proposedYear = actualYear + 1;
            int proposedYearId = db.jntuh_academic_year.Where(ay => ay.actualYear == proposedYear).Select(ay => ay.id).FirstOrDefault();
            //e.isActive == true && 
            var existingCourses = db.jntuh_college_intake_existing.Where(e => e.collegeId == missedCourses.collegeID && e.academicYearId == academicYearId && e.approvedIntake != 0).ToList();

            int total = existingCourses.Count();
            int inserted = 0;
            if (existingCourses != null)
            {
                foreach (var item in existingCourses)
                {
                    var proposedCoursesId = db.jntuh_college_intake_proposed.Where(p => p.collegeId == missedCourses.collegeID && p.specializationId == item.specializationId && p.shiftId == item.shiftId && p.academicYearId == proposedYearId)
                                                                            .Select(p => p.id).FirstOrDefault();
                    if (proposedCoursesId == 0)
                    {
                        jntuh_college_intake_proposed jntuh_college_intake_proposed = new jntuh_college_intake_proposed();
                        jntuh_college_intake_proposed.academicYearId = proposedYearId;
                        jntuh_college_intake_proposed.collegeId = item.collegeId;
                        jntuh_college_intake_proposed.specializationId = item.specializationId;
                        jntuh_college_intake_proposed.shiftId = item.shiftId;
                        jntuh_college_intake_proposed.courseAffiliationStatusCodeId = 1;
                        jntuh_college_intake_proposed.proposedIntake = item.approvedIntake;
                        jntuh_college_intake_proposed.isActive = true;
                        jntuh_college_intake_proposed.createdBy = userID;
                        jntuh_college_intake_proposed.createdOn = DateTime.Now;
                        db.jntuh_college_intake_proposed.Add(jntuh_college_intake_proposed);
                        db.SaveChanges();
                        inserted++;
                    }
                }

                if (inserted > 0)
                {
                    TempData["Status"] = inserted.ToString() + " of " + total + " specializations inserted.";
                }
            }
            return RedirectToAction("MissedCourses");
        }

    }
}
