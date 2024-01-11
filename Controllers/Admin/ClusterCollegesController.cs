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
    public class ClusterCollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ClusterCollegesIndex(string cluster)
        {
            List<college_clusters> collegeclusters = new List<college_clusters>();
            if (cluster != null && cluster != string.Empty)
            {
                ViewBag.ClusterColleges = true;
                collegeclusters = db.college_clusters.Where(c => c.clusterName == cluster).OrderBy(c => c.clusterName).Select(c => c).ToList();
            }
            else
            {
                ViewBag.ClusterColleges = false;
            }
            return View("~/Views/Admin/ClusterCollegesIndex.cshtml", collegeclusters);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ClusterCollegesIndex(List<college_clusters> college_clusters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            string strstatus = string.Empty;
            if (college_clusters != null && college_clusters.Count() > 0)
            {
                foreach (var item in college_clusters)
                {
                    int count = db.jntuh_ffc_schedule.Where(s => s.collegeID == item.collegeId && s.InspectionPhaseId == InspectionPhaseId).Count();
                    if (count == 0)
                    {
                        #region college_clusters
                        college_clusters collegeclusters = db.college_clusters.Find(item.id);
                        collegeclusters.isActive = item.isActive;
                        collegeclusters.updatedBy = userID;
                        collegeclusters.updatedOn = DateTime.Now;
                        db.Entry(collegeclusters).State = EntityState.Modified;
                        db.SaveChanges();
                        #endregion

                        #region colleges_groups

                        string strcollegecode = db.jntuh_college.Find(item.collegeId).collegeCode;
                        int collegeGroupId = db.colleges_groups.Where(cg => cg.clusterName == item.clusterName && cg.collegeCode == strcollegecode).Select(cg => cg.id).FirstOrDefault();
                        colleges_groups colleges_groups = db.colleges_groups.Find(collegeGroupId);
                        if (colleges_groups != null)
                        {
                            colleges_groups.isActive = item.isActive;
                            colleges_groups.updatedBy = userID;
                            colleges_groups.updatedOn = DateTime.Now;
                            db.Entry(colleges_groups).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //This code is required are not i have doubt 
                            //when we update the cluster colleges if not present in collegegroups adding those clustercolleges records in collegegroups screen.
                            colleges_groups addcollegesgroups = new colleges_groups();
                            addcollegesgroups.clusterName = item.clusterName;
                            addcollegesgroups.collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                            addcollegesgroups.firstMemberGroup = string.Empty;
                            addcollegesgroups.SecondMemberGroup = string.Empty;
                            addcollegesgroups.isActive = item.isActive;
                            addcollegesgroups.createdBy = userID;
                            addcollegesgroups.createdOn = DateTime.Now;
                            db.colleges_groups.Add(addcollegesgroups);
                            db.SaveChanges();
                        }
                        #endregion

                    }
                    else
                    {
                        strstatus = "Cluster college already scheduled you can't change status";
                    }
                }
                if (strstatus == string.Empty)
                {
                    TempData["Success"] = "Cluster college status changed successfully.";
                }
                if (strstatus != string.Empty)
                {
                    TempData["Error"] = strstatus;
                }
            }
            else
            {
                TempData["Error"] = "Cluster college status is not changed.";
            }
            return RedirectToAction("ClusterCollegesIndex");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddClusterColleges()
        {
            ClusterScheduleEmails clusterScheduleEmails = new ClusterScheduleEmails();
            return View("~/Views/Admin/AddClusterColleges.cshtml", clusterScheduleEmails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddClusterColleges(ClusterScheduleEmails clusterScheduleEmails, int[] ChooseRight)
        {
            int count = 0;
            if (ChooseRight.Count() > 0)
            {
                count = db.college_clusters.Where(c => c.clusterName == clusterScheduleEmails.cluster).Select(c => c).Count();
                if (count == 0)
                {
                    foreach (var collegeId in ChooseRight)
                    {
                        #region college_clusters
                        college_clusters college_clusters = new college_clusters();
                        int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                        college_clusters.collegeId = collegeId;
                        college_clusters.clusterName = clusterScheduleEmails.cluster;
                        college_clusters.isActive = false;
                        college_clusters.createdBy = userID;
                        college_clusters.createdOn = DateTime.Now;
                        college_clusters.isEditable = true;
                        db.college_clusters.Add(college_clusters);
                        db.SaveChanges();
                        #endregion

                        #region colleges_groups
                        colleges_groups colleges_groups = new colleges_groups();
                        colleges_groups.clusterName = clusterScheduleEmails.cluster;
                        colleges_groups.collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        colleges_groups.firstMemberGroup = string.Empty;
                        colleges_groups.SecondMemberGroup = string.Empty;
                        colleges_groups.isActive = true;
                        colleges_groups.createdBy = userID;
                        colleges_groups.createdOn = DateTime.Now;
                        db.colleges_groups.Add(colleges_groups);
                        db.SaveChanges();
                        #endregion
                    }
                    TempData["Success"] = "Cluster saved successfully";
                }
                else
                {
                    TempData["Success"] = "Cluster already exist";
                }
            }
            else
            {
                TempData["Error"] = "Cluster not saved";
            }
            return RedirectToAction("ClusterCollegesIndex");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ChangeClusterName(string cluster)
        {
            ClusterScheduleEmails clusterScheduleEmails = new ClusterScheduleEmails();
            clusterScheduleEmails.cluster = cluster;
            return PartialView("~/Views/Admin/ChangeClusterName.cshtml", clusterScheduleEmails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ChangeClusterName(ClusterScheduleEmails clusterScheduleEmails)
        {
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            var clusterDetails = db.college_clusters.Where(c => c.clusterName == clusterScheduleEmails.cluster).Select(c => c).ToList();
            foreach (var item in clusterDetails)
            {
                int count = db.jntuh_ffc_schedule.Where(s => s.collegeID == item.collegeId && s.InspectionPhaseId == InspectionPhaseId).Count();
                if (count == 0)
                {
                    #region college_clusters
                    college_clusters college_clusters = db.college_clusters.Find(item.id);
                    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    college_clusters.clusterName = clusterScheduleEmails.newcluster;
                    college_clusters.updatedBy = userID;
                    college_clusters.updatedOn = DateTime.Now;                  
                    db.Entry(college_clusters).State = EntityState.Modified;
                    db.SaveChanges();
                    #endregion

                    #region colleges_groups
                    string strcollegecode = db.jntuh_college.Find(item.collegeId).collegeCode;
                    int collegeGroupId = db.colleges_groups.Where(cg => cg.clusterName == item.clusterName && cg.collegeCode == strcollegecode).Select(cg => cg.id).FirstOrDefault();
                    colleges_groups colleges_groups = db.colleges_groups.Find(collegeGroupId);
                    if (colleges_groups != null)
                    {
                        colleges_groups.clusterName = clusterScheduleEmails.newcluster;
                        colleges_groups.updatedBy = userID;
                        colleges_groups.updatedOn = DateTime.Now;
                        db.Entry(colleges_groups).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    #endregion
                    TempData["Success"] = "Cluster name changed successfully";
                }
                else
                {
                    TempData["Error"] = "You can't change cluster name its already schedule";
                }
            }

            return RedirectToAction("ClusterCollegesIndex");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCollegeToExistingCluster(string cluster)
        {
            ClusterScheduleEmails clusterScheduleEmails = new ClusterScheduleEmails();
            clusterScheduleEmails.cluster = cluster;
            return PartialView("~/Views/Admin/AddCollegeToExistingCluster.cshtml", clusterScheduleEmails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddCollegeToExistingCluster(ClusterScheduleEmails clusterScheduleEmails)
        {
            int count = 0;
            count = db.college_clusters.Where(c => c.clusterName == clusterScheduleEmails.cluster && c.collegeId == clusterScheduleEmails.collegeId).Select(c => c.collegeId).Count();
            if (count == 0)
            {

                #region college_clusters
                college_clusters college_clusters = new college_clusters();
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                college_clusters.collegeId = clusterScheduleEmails.collegeId;
                college_clusters.clusterName = clusterScheduleEmails.cluster;
                college_clusters.isActive = true;
                college_clusters.createdBy = userID;
                college_clusters.createdOn = DateTime.Now;
                college_clusters.isEditable = true;
                db.college_clusters.Add(college_clusters);
                db.SaveChanges();
                #endregion

                #region colleges_groups
                colleges_groups colleges_groups = new colleges_groups();
                colleges_groups.clusterName = clusterScheduleEmails.cluster;
                colleges_groups.collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == clusterScheduleEmails.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                colleges_groups.firstMemberGroup = string.Empty;
                colleges_groups.SecondMemberGroup = string.Empty;
                colleges_groups.isActive = true;
                colleges_groups.createdBy = userID;
                colleges_groups.createdOn = DateTime.Now;
                db.colleges_groups.Add(colleges_groups);
                db.SaveChanges();
                #endregion
                TempData["Success"] = "College saved successfully";
            }
            else
            {
                TempData["Error"] = "College already exist in this cluster";
            }
            return RedirectToAction("ClusterCollegesIndex");
        }

    }
}
