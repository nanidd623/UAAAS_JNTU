using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.DataEntry
{
    [ErrorHandling]
    public class DataEntryCompletedCollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DataEntryCompletedColleges(int? PhaseId)
        {
            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id                                          
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().ToList();

            int InspectionPhaseId = 0;

            if (PhaseId == null)
                InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            else
                InspectionPhaseId = (int)PhaseId;

            List<DataEntryColleges> dataEntryCompletedColleges = (from dcl in db.jntuh_dataentry_allotment
                                                                 join u in db.my_aspnet_users on dcl.userID equals u.id
                                                                 join c in db.jntuh_college on dcl.collegeID equals c.id
                                                                  where (c.isActive == true && dcl.isCompleted == true && dcl.isActive == true && dcl.InspectionPhaseId == InspectionPhaseId)
                                                                 select new DataEntryColleges
                                                                 {
                                                                     id = dcl.id,
                                                                     userId = dcl.userID,
                                                                     collegeId = dcl.collegeID,
                                                                     isActive = dcl.isActive,
                                                                     isCompleted = dcl.isCompleted,
                                                                     isVerified = dcl.isVerified,
                                                                     userName = u.name,
                                                                     collegeName = c.collegeName,
                                                                     collegeCode = c.collegeCode,
                                                                     createdBy = dcl.createdBy,
                                                                     createdon = dcl.createdOn
                                                                 }).ToList();

            ViewBag.DataEntryCompletedColleges = dataEntryCompletedColleges;
            return View("~/Views/DataEntry/DataEntryCompletedColleges.cshtml", dataEntryCompletedColleges);
        }

    }
}
