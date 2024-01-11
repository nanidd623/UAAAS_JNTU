using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CommitteeSubmissionController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CommitteeSubmissionColleges(int? PhaseId)
        {
            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                           //where p.isActive == false
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().ToList();

            int InspectionPhaseId = 0;

            if (PhaseId == null)
                InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            else
                InspectionPhaseId = (int)PhaseId;

            List<CommitteeSubmission> committeeSubmissionCollegesList = (from cs in db.jntuh_committee_submission
                                                                         join c in db.jntuh_college on cs.collegeID equals c.id
                                                                         where c.isActive == true && cs.isActive == true && c.isClosed == false && cs.InspectionPhaseId == InspectionPhaseId
                                                                         orderby (cs.submittedDate) descending
                                                                         select new CommitteeSubmission
                                                                         {
                                                                             id = cs.id,
                                                                             collegeId = cs.collegeID,
                                                                             submittedDate = cs.submittedDate,
                                                                             remarks = cs.remarks,
                                                                             isActive = cs.isActive,
                                                                             collegeName = c.collegeName,
                                                                             collegeCode = c.collegeCode,
                                                                             createdBy = cs.createdBy,
                                                                             createdon = cs.createdOn
                                                                         }).ToList();

            ViewBag.CommitteeSubmissionList = committeeSubmissionCollegesList;
            return View("~/Views/DataEntry/CommitteeSubmissionColleges.cshtml", committeeSubmissionCollegesList);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetColleges()
        {
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            //Not in sub Query
            var collegesList = (from c in db.jntuh_college
                                let csc = from cs in db.jntuh_committee_submission
                                          where cs.isActive == true && cs.InspectionPhaseId == InspectionPhaseId
                                          select cs.collegeID
                                where c.isActive == true && !csc.Contains(c.id)
                                orderby c.collegeName ascending
                                select new
                                {
                                    c.id,
                                    c.collegeCode,
                                    c.collegeName
                                }).OrderBy(c => c.collegeCode).ToList();
            var collegesData = collegesList.Select(a => new SelectListItem()
            {
                Text = a.collegeCode + "-" + a.collegeName,
                Value = a.id.ToString(),
            });
            return Json(collegesData, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CommitteeSubmissionCreateCollege(int? id)
        {
            if (id != null)
            {
                ViewBag.IsUpdate = true;
                jntuh_committee_submission committeeSubmission = db.jntuh_committee_submission.Where(cs => cs.id == id).Select(cs => cs).FirstOrDefault();
                CommitteeSubmission committeeSubmissionCollegesList = new CommitteeSubmission();
                if (committeeSubmission != null)
                {
                    committeeSubmissionCollegesList.collegeId = committeeSubmission.collegeID;
                    committeeSubmissionCollegesList.strsubmittedDate = Utilities.MMDDYY2DDMMYY(committeeSubmission.submittedDate.ToString());
                    committeeSubmissionCollegesList.remarks = committeeSubmission.remarks;
                    committeeSubmissionCollegesList.isActive = committeeSubmission.isActive;
                    committeeSubmissionCollegesList.createdBy = committeeSubmission.createdBy;
                    committeeSubmissionCollegesList.createdon = committeeSubmission.createdOn;
                    committeeSubmissionCollegesList.InspectionPhaseId = committeeSubmission.InspectionPhaseId;

                    List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
                    foreach (var item in colleges)
                    {
                        item.collegeName = item.collegeCode + "-" + item.collegeName;
                    }
                    ViewBag.colleges = colleges.OrderBy(c => c.collegeCode).ToList();
                }
                return PartialView("~/Views/DataEntry/CommitteeSubmissionCreateCollege.cshtml", committeeSubmissionCollegesList);
            }
            else
            {
                CommitteeSubmission committeeSubmissionCollegesList = new CommitteeSubmission();
                return PartialView("~/Views/DataEntry/CommitteeSubmissionCreateCollege.cshtml", committeeSubmissionCollegesList);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CommitteeSubmissionCreateCollege(CommitteeSubmission committeeSubmissionCollegesList, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (cmd == "Update")
            {
                if (ModelState.IsValid == true)
                {
                    jntuh_committee_submission committeeSubmission = new jntuh_committee_submission();
                    committeeSubmission.id = committeeSubmissionCollegesList.id;
                    committeeSubmission.collegeID = committeeSubmissionCollegesList.collegeId;
                    if (committeeSubmissionCollegesList.strsubmittedDate != null)
                    {
                        committeeSubmission.submittedDate = Utilities.DDMMYY2MMDDYY(committeeSubmissionCollegesList.strsubmittedDate);
                    }
                    committeeSubmission.remarks = committeeSubmissionCollegesList.remarks;
                    committeeSubmission.isActive = committeeSubmissionCollegesList.isActive;
                    committeeSubmission.createdBy = committeeSubmissionCollegesList.createdBy;
                    committeeSubmission.createdOn = committeeSubmissionCollegesList.createdon;
                    committeeSubmission.InspectionPhaseId = committeeSubmissionCollegesList.InspectionPhaseId;

                    db.Entry(committeeSubmission).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Data Updated successfully.";
                }
                return RedirectToAction("CommitteeSubmissionColleges");
            }
            else
            {

                if (ModelState.IsValid == true)
                {
                    int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

                    int committeeId = db.jntuh_committee_submission.Where(c => c.collegeID == committeeSubmissionCollegesList.collegeId && c.isActive == true && c.InspectionPhaseId == InspectionPhaseId).Select(c => c.id).FirstOrDefault();
                    if (committeeId == 0)
                    {
                        jntuh_committee_submission committeeSubmission = new jntuh_committee_submission();
                        committeeSubmission.collegeID = committeeSubmissionCollegesList.collegeId;
                        committeeSubmission.submittedDate = Utilities.DDMMYY2MMDDYY(committeeSubmissionCollegesList.strsubmittedDate);
                        committeeSubmission.remarks = committeeSubmissionCollegesList.remarks;
                        committeeSubmission.isActive = true;
                        committeeSubmission.createdBy = userID;
                        committeeSubmission.createdOn = DateTime.Now;
                        committeeSubmission.InspectionPhaseId = InspectionPhaseId;

                        db.jntuh_committee_submission.Add(committeeSubmission);
                        db.SaveChanges();
                        TempData["Success"] = "Data Added successfully";
                    }
                    else
                    {
                        TempData["Error"] = "College alredy  exist";
                    }
                    return RedirectToAction("CommitteeSubmissionColleges");
                }

            }
            return RedirectToAction("CommitteeSubmissionColleges");
        }
        public ActionResult CommitteeSubmissionDeleteCollege(int? id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            CommitteeSubmission committeeSubmissionCollegesList = new CommitteeSubmission();
            if (ModelState.IsValid == true)
            {

                jntuh_committee_submission committeeSubmission = db.jntuh_committee_submission.Where(cs => cs.id == id).Select(cs => cs).FirstOrDefault();
                committeeSubmission.id = committeeSubmission.id;
                committeeSubmission.collegeID = committeeSubmission.collegeID;
                committeeSubmission.submittedDate = Utilities.DDMMYY2MMDDYY(committeeSubmissionCollegesList.strsubmittedDate);
                committeeSubmission.remarks = committeeSubmission.remarks;
                committeeSubmission.isActive = false;
                committeeSubmission.createdBy = committeeSubmission.createdBy;
                committeeSubmission.createdOn = committeeSubmission.createdOn;
                committeeSubmission.InspectionPhaseId = committeeSubmission.InspectionPhaseId;

                db.Entry(committeeSubmission).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Data Deleted successfully.";
            }
            return RedirectToAction("CommitteeSubmissionColleges");
        }

    }
}
