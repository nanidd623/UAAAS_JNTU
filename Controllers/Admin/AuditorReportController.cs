using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AuditorReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        
        //
        // GET: /AuditorReport/AuditorReportIndex
        [Authorize(Roles = "Admin")]
        public ActionResult AuditorReportIndex()
        {
            //get ffc committee details
            List<jntuh_ffc_committee> ffcCommitteeDetails = db.jntuh_ffc_committee.OrderBy(ffcCommittee => ffcCommittee.isConvenor)
                                                                                  .ToList();

            List<AuditorReport> auditorReportDetails = new List<AuditorReport>();

            int? collegeId = 0;
            DateTime? inspectiondate;
            DateTime? alternateInspectiondate;
                                                    
            if (ffcCommitteeDetails.Count() > 0)
            {
                //get all the details of audit repoters based on ffc committee details
                foreach (var committeeDetails in ffcCommitteeDetails)
                {
                    AuditorReport auditor = new AuditorReport();
                    auditor.name = db.jntuh_ffc_auditor.Where(ffcAuditor => ffcAuditor.id == committeeDetails.auditorID)
                                                       .Select(ffcAuditor => ffcAuditor.auditorName)
                                                       .FirstOrDefault();
                    auditor.prefferedDesignation = db.jntuh_ffc_auditor.Where(ffcAuditor => ffcAuditor.id == committeeDetails.auditorID)
                                                       .Select(ffcAuditor => ffcAuditor.auditorPreferredDesignation)
                                                       .FirstOrDefault();
                    auditor.auditorPlace = db.jntuh_ffc_auditor.Where(ffcAuditor => ffcAuditor.id == committeeDetails.auditorID)
                                                       .Select(ffcAuditor => ffcAuditor.auditorPlace)
                                                       .FirstOrDefault();
                    collegeId = db.jntuh_ffc_schedule.Where(schedule => schedule.id == committeeDetails.scheduleID)
                                                     .Select(schedule => schedule.collegeID)
                                                     .FirstOrDefault();
                    if (collegeId != 0)
                    {
                        auditor.collegeName = db.jntuh_college.Where(college => college.id == collegeId)
                                                               .Select(college => college.collegeName)
                                                               .FirstOrDefault();
                        auditor.collegeCode = db.jntuh_college.Where(college => college.id == collegeId)
                                                               .Select(college => college.collegeCode)
                                                               .FirstOrDefault();
                    }
                    inspectiondate = db.jntuh_ffc_schedule.Where(schedule => schedule.id == committeeDetails.scheduleID)
                                                                                           .Select(schedule => schedule.inspectionDate)
                                                                                           .FirstOrDefault();

                    alternateInspectiondate = db.jntuh_ffc_schedule.Where(schedule => schedule.id == committeeDetails.scheduleID)
                                                                                           .Select(schedule => schedule.alternateInspectionDate)
                                                                                           .FirstOrDefault();
                    if (inspectiondate != null)
                    {
                        auditor.inspectionDate = Utilities.MMDDYY2DDMMYY(inspectiondate.ToString());
                    }
                    if (alternateInspectiondate != null)
                    {
                        auditor.alternateInspectionDate = Utilities.MMDDYY2DDMMYY(alternateInspectiondate.ToString());
                    }
                    auditorReportDetails.Add(auditor);                    
                }
            }
            ViewBag.Count = auditorReportDetails.Count();
            return View("~/Views/Admin/AuditorReportIndex.cshtml", auditorReportDetails);
        }
    }
}
