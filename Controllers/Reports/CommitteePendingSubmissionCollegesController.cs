using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CommitteePendingSubmissionCollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult CommitteePendingSubmissionColleges()
        {

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            List<CommitteePendingSubmissionColleges> committeePendingSubmissionColleges = (from s in db.jntuh_ffc_schedule
                                                                                           join cs in db.jntuh_committee_submission on s.collegeID equals cs.collegeID into csl //&& cs.isActive==true                                                                                             
                                                                                           from cs in csl.DefaultIfEmpty()
                                                                                           join c in db.jntuh_ffc_committee on s.id equals c.scheduleID
                                                                                           join a in db.jntuh_ffc_auditor on c.auditorID equals a.id
                                                                                           join dep in db.jntuh_department on a.auditorDepartmentID equals dep.id
                                                                                           join des in db.jntuh_designation on a.auditorDesignationID equals des.id
                                                                                           where (cs.collegeID == null && c.isConvenor == 1 && s.InspectionPhaseId == InspectionPhaseId)
                                                                                           group s by new
                                                                                           {
                                                                                               auditorname = a.auditorName,
                                                                                               departmentname = dep.departmentName,
                                                                                               designation = des.designation,
                                                                                               auditorpreferreddesignation = a.auditorPreferredDesignation,
                                                                                               auditoremail1 = a.auditorEmail1,
                                                                                               auditoremail2 = a.auditorEmail2,
                                                                                               auditormobile1 = a.auditorMobile1,
                                                                                               auditormobile2 = a.auditorMobile2,
                                                                                               auditorplace = a.auditorPlace
                                                                                           } into g
                                                                                           orderby g.Count() descending
                                                                                           select new CommitteePendingSubmissionColleges
                                                                                           {

                                                                                               auditorname = g.Key.auditorname,
                                                                                               departmentname = g.Key.departmentname,
                                                                                               designation = g.Key.designation,
                                                                                               auditorpreferreddesignation = g.Key.auditorpreferreddesignation,
                                                                                               auditoremail1 = g.Key.auditoremail1,
                                                                                               auditoremail2 = g.Key.auditoremail2,
                                                                                               auditormobile1 = g.Key.auditormobile1,
                                                                                               auditormobile2 = g.Key.auditormobile2,
                                                                                               auditorplace = g.Key.auditorplace,
                                                                                               id = g.Count()
                                                                                           }).ToList();

            return View("~/Views/Reports/CommitteePendingSubmissionColleges.cshtml", committeePendingSubmissionColleges);
        }
        [HttpPost]
        public ActionResult CommitteePendingSubmissionColleges(CommitteePendingSubmissionColleges committeePendingSubmission, string cmd)
        {
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            List<CommitteePendingSubmissionColleges> committeePendingSubmissionColleges = (from s in db.jntuh_ffc_schedule
                                                                                           join cs in db.jntuh_committee_submission on s.collegeID equals cs.collegeID into csl //&& cs.isActive==true                                                                                             
                                                                                           from cs in csl.DefaultIfEmpty()
                                                                                           join c in db.jntuh_ffc_committee on s.id equals c.scheduleID
                                                                                           join a in db.jntuh_ffc_auditor on c.auditorID equals a.id
                                                                                           join dep in db.jntuh_department on a.auditorDepartmentID equals dep.id
                                                                                           join des in db.jntuh_designation on a.auditorDesignationID equals des.id
                                                                                           where (cs.collegeID == null && c.isConvenor == 1 && s.InspectionPhaseId == InspectionPhaseId)
                                                                                           group s by new
                                                                                           {
                                                                                               auditorname = a.auditorName,
                                                                                               departmentname = dep.departmentName,
                                                                                               designation = des.designation,
                                                                                               auditorpreferreddesignation = a.auditorPreferredDesignation,
                                                                                               auditoremail1 = a.auditorEmail1,
                                                                                               auditoremail2 = a.auditorEmail2,
                                                                                               auditormobile1 = a.auditorMobile1,
                                                                                               auditormobile2 = a.auditorMobile2,
                                                                                               auditorplace = a.auditorPlace
                                                                                           } into g
                                                                                           orderby g.Count() descending
                                                                                           select new CommitteePendingSubmissionColleges
                                                                                           {

                                                                                               auditorname = g.Key.auditorname,
                                                                                               departmentname = g.Key.departmentname,
                                                                                               designation = g.Key.designation,
                                                                                               auditorpreferreddesignation = g.Key.auditorpreferreddesignation,
                                                                                               auditoremail1 = g.Key.auditoremail1,
                                                                                               auditoremail2 = g.Key.auditoremail2,
                                                                                               auditormobile1 = g.Key.auditormobile1,
                                                                                               auditormobile2 = g.Key.auditormobile2,
                                                                                               auditorplace = g.Key.auditorplace,
                                                                                               id = g.Count()
                                                                                           }).ToList();
            ViewBag.committeePendingSubmissionColleges = committeePendingSubmissionColleges;

            if (committeePendingSubmissionColleges.Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=CommitteePendingSubmissionColleges.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_CommitteePendingSubmissionColleges.cshtml");
            }
            return View("~/Views/Reports/CommitteePendingSubmissionColleges.cshtml", committeePendingSubmissionColleges);
        }


    }
}
