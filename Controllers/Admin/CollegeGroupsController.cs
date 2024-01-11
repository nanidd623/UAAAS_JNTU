using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class CollegeGroupsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CollegeGroupsIndex(string cluster)
        {
            List<colleges_groups> collegesgroups = new List<colleges_groups>();           
            if (cluster != null && cluster != string.Empty)
            {
                ViewBag.Collegesgroups = true;                
                collegesgroups = db.colleges_groups.Where(c => c.clusterName == cluster).OrderBy(c => c.clusterName).Select(c => c).ToList();
            }
            else
            {
                ViewBag.Collegesgroups = false;
            }

            return View("~/Views/Admin/CollegeGroupsIndex.cshtml", collegesgroups);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CollegeGroupsIndex(List<colleges_groups> colleges_groups)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            string strstatus = string.Empty;
            if (colleges_groups != null && colleges_groups.Count() > 0)
            {

                foreach (var item in colleges_groups)
                {
                    //if college code is duplicate there may be chance to wrong by default it takes first collegecode
                    int collegeId = db.jntuh_college.Where(c => c.collegeCode == item.collegeCode).Select(c => c.id).FirstOrDefault();
                    int count = db.jntuh_ffc_schedule.Where(s => s.collegeID == collegeId && s.InspectionPhaseId == InspectionPhaseId).Count();
                    if (count == 0)
                    {
                        #region colleges_groups
                        colleges_groups collegesgroups = db.colleges_groups.Find(item.id);
                        collegesgroups.collegeGroup = item.collegeGroup;
                        collegesgroups.firstMemberGroup = item.firstMemberGroup;
                        collegesgroups.SecondMemberGroup = item.SecondMemberGroup;
                        collegesgroups.updatedBy = userID;
                        collegesgroups.updatedOn = DateTime.Now;
                        db.Entry(collegesgroups).State = EntityState.Modified;
                        db.SaveChanges();
                        #endregion
                    }
                    else
                    {
                        strstatus = "College group already scheduled you can't update group";
                    }
                }
                if (strstatus == string.Empty)
                {
                    TempData["Success"] = "College group updated successfully.";
                }
                if (strstatus != string.Empty)
                {
                    TempData["Error"] = strstatus;
                }
            }
            else
            {
                TempData["Error"] = "College group already scheduled you can't update group.";
            }
            return RedirectToAction("CollegeGroupsIndex");
        }

    }
}
