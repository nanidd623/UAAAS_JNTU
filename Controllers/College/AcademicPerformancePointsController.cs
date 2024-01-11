using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Web.Security;
using System.Web.Configuration;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AcademicPerformancePointsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /AcademicPerformancePoints/AcademicPerformancePointsCreate
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AcademicPerformancePointsCreate(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
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
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            int exisiId = db.jntuh_college_academic_performance_points.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();


            if (exisiId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("AcademicPerformancePointsView", "AcademicPerformancePoints");
            }
            if (userCollegeID > 0 && exisiId>0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("AcademicPerformancePointsEdit", "AcademicPerformancePoints", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (exisiId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            int totalAllotedPoints = 0;
            AcademicPerformancePoints academicPerformancePoints = new AcademicPerformancePoints();
            int[] allotedPoints = db.jntuh_academic_performance.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            academicPerformancePoints.allotedPoints1 = allotedPoints[0];
            academicPerformancePoints.allotedPoints2 = allotedPoints[1];
            academicPerformancePoints.allotedPoints3 = allotedPoints[2];
            academicPerformancePoints.allotedPoints4 = allotedPoints[3];
            academicPerformancePoints.allotedPoints5 = allotedPoints[4];
            academicPerformancePoints.allotedPoints6 = allotedPoints[5];
            academicPerformancePoints.allotedPoints7 = allotedPoints[6];
            academicPerformancePoints.allotedPoints8 = allotedPoints[7];
            academicPerformancePoints.allotedPoints9 = allotedPoints[8];
            academicPerformancePoints.allotedPoints10 = allotedPoints[9];
            academicPerformancePoints.totalAllotedPoints = totalAllotedPoints;
            academicPerformancePoints.collegeId = userCollegeID;
            return View("~/Views/College/AcademicPerformancePointsCreate.cshtml", academicPerformancePoints);
        }

        //
        // POST: /AcademicPerformancePoints/AcademicPerformancePointsCreate
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AcademicPerformancePointsCreate(AcademicPerformancePoints academicPerformancePoints)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = academicPerformancePoints.collegeId;
            }
            int[] typeId = db.jntuh_academic_performance.Where(o => o.isActive == true).Select(o => o.id).ToArray();
            SaveAcademicPerformance(userCollegeID, typeId[0], academicPerformancePoints.obtainedPoints1, userID);
            SaveAcademicPerformance(userCollegeID, typeId[1], academicPerformancePoints.obtainedPoints2, userID);
            SaveAcademicPerformance(userCollegeID, typeId[2], academicPerformancePoints.obtainedPoints3, userID);
            SaveAcademicPerformance(userCollegeID, typeId[3], academicPerformancePoints.obtainedPoints4, userID);
            SaveAcademicPerformance(userCollegeID, typeId[4], academicPerformancePoints.obtainedPoints5, userID);
            SaveAcademicPerformance(userCollegeID, typeId[5], academicPerformancePoints.obtainedPoints6, userID);
            SaveAcademicPerformance(userCollegeID, typeId[6], academicPerformancePoints.obtainedPoints7, userID);
            SaveAcademicPerformance(userCollegeID, typeId[7], academicPerformancePoints.obtainedPoints8, userID);
            SaveAcademicPerformance(userCollegeID, typeId[8], academicPerformancePoints.obtainedPoints9, userID);
            SaveAcademicPerformance(userCollegeID, typeId[9], academicPerformancePoints.obtainedPoints10, userID);
            int totalAllotedPoints = 0;
            int[] allotedPoints = db.jntuh_academic_performance.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            academicPerformancePoints.allotedPoints1 = allotedPoints[0];
            academicPerformancePoints.allotedPoints2 = allotedPoints[1];
            academicPerformancePoints.allotedPoints3 = allotedPoints[2];
            academicPerformancePoints.allotedPoints4 = allotedPoints[3];
            academicPerformancePoints.allotedPoints5 = allotedPoints[4];
            academicPerformancePoints.allotedPoints6 = allotedPoints[5];
            academicPerformancePoints.allotedPoints7 = allotedPoints[6];
            academicPerformancePoints.allotedPoints8 = allotedPoints[7];
            academicPerformancePoints.allotedPoints9 = allotedPoints[8];
            academicPerformancePoints.allotedPoints10 = allotedPoints[9];
            academicPerformancePoints.totalAllotedPoints = totalAllotedPoints;
            academicPerformancePoints.collegeId = userCollegeID;
            TempData["Success"] = "Academic Performance details are Saved successfully.";
            return View("~/Views/College/AcademicPerformancePointsCreate.cshtml", academicPerformancePoints);
        }

        private void SaveAcademicPerformance(int collegeId, int typeId, int obtainedPoints, int userId)
        {
            jntuh_college_academic_performance_points academicPerformancePoints = new jntuh_college_academic_performance_points();
            academicPerformancePoints.collegeId = collegeId;
            academicPerformancePoints.typeId = typeId;
            academicPerformancePoints.pointsObtained = obtainedPoints;

            int existId = db.jntuh_college_academic_performance_points.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.id)
                                                                    .FirstOrDefault();
            if (existId == 0)
            {
                academicPerformancePoints.createdBy = userId;
                academicPerformancePoints.createdOn = DateTime.Now;
                db.jntuh_college_academic_performance_points.Add(academicPerformancePoints);
            }
            else
            {
                academicPerformancePoints.id = existId;
                academicPerformancePoints.createdBy = db.jntuh_college_academic_performance_points.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.createdBy)
                                                                    .FirstOrDefault();
                academicPerformancePoints.createdOn = db.jntuh_college_academic_performance_points.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.createdOn)
                                                                    .FirstOrDefault();
                academicPerformancePoints.updatedBy = userId;
                academicPerformancePoints.updatedOn = DateTime.Now;
                db.Entry(academicPerformancePoints).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        //
        // GET: /AcademicPerformancePoints/AcademicPerformancePointsEdit
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AcademicPerformancePointsEdit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            int exisitId = db.jntuh_college_academic_performance_points.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (exisitId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("AcademicPerformancePointsCreate", "AcademicPerformancePoints");
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("AcademicPerformancePointsView", "AcademicPerformancePoints");
            }
            else
            {
                ViewBag.IsEditable = true;
            }
            AcademicPerformancePoints academicPerformancePoints = new AcademicPerformancePoints();
            List<jntuh_college_academic_performance_points> collegeAcademicPerformancePoints = db.jntuh_college_academic_performance_points.Where(o => o.collegeId == userCollegeID).ToList();
            if (collegeAcademicPerformancePoints.Count() > 0)
            {
                academicPerformancePoints.typeId1 = collegeAcademicPerformancePoints[0].typeId;
                academicPerformancePoints.typeId2 = collegeAcademicPerformancePoints[1].typeId;
                academicPerformancePoints.typeId3 = collegeAcademicPerformancePoints[2].typeId;
                academicPerformancePoints.typeId4 = collegeAcademicPerformancePoints[3].typeId;
                academicPerformancePoints.typeId5 = collegeAcademicPerformancePoints[4].typeId;
                academicPerformancePoints.typeId6 = collegeAcademicPerformancePoints[5].typeId;
                academicPerformancePoints.typeId7 = collegeAcademicPerformancePoints[6].typeId;
                academicPerformancePoints.typeId8 = collegeAcademicPerformancePoints[7].typeId;
                academicPerformancePoints.typeId9 = collegeAcademicPerformancePoints[8].typeId;
                academicPerformancePoints.typeId10 = collegeAcademicPerformancePoints[9].typeId;

                academicPerformancePoints.obtainedPoints1 = collegeAcademicPerformancePoints[0].pointsObtained;
                academicPerformancePoints.obtainedPoints2 = collegeAcademicPerformancePoints[1].pointsObtained;
                academicPerformancePoints.obtainedPoints3 = collegeAcademicPerformancePoints[2].pointsObtained;
                academicPerformancePoints.obtainedPoints4 = collegeAcademicPerformancePoints[3].pointsObtained;
                academicPerformancePoints.obtainedPoints5 = collegeAcademicPerformancePoints[4].pointsObtained;
                academicPerformancePoints.obtainedPoints6 = collegeAcademicPerformancePoints[5].pointsObtained;
                academicPerformancePoints.obtainedPoints7 = collegeAcademicPerformancePoints[6].pointsObtained;
                academicPerformancePoints.obtainedPoints8 = collegeAcademicPerformancePoints[7].pointsObtained;
                academicPerformancePoints.obtainedPoints9 = collegeAcademicPerformancePoints[8].pointsObtained;
                academicPerformancePoints.obtainedPoints10 = collegeAcademicPerformancePoints[9].pointsObtained;

            }
            int totalAllotedPoints = 0;
            int[] allotedPoints = db.jntuh_academic_performance.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            academicPerformancePoints.allotedPoints1 = allotedPoints[0];
            academicPerformancePoints.allotedPoints2 = allotedPoints[1];
            academicPerformancePoints.allotedPoints3 = allotedPoints[2];
            academicPerformancePoints.allotedPoints4 = allotedPoints[3];
            academicPerformancePoints.allotedPoints5 = allotedPoints[4];
            academicPerformancePoints.allotedPoints6 = allotedPoints[5];
            academicPerformancePoints.allotedPoints7 = allotedPoints[6];
            academicPerformancePoints.allotedPoints8 = allotedPoints[7];
            academicPerformancePoints.allotedPoints9 = allotedPoints[8];
            academicPerformancePoints.allotedPoints10 = allotedPoints[9];
            academicPerformancePoints.totalAllotedPoints = totalAllotedPoints;
            academicPerformancePoints.collegeId = userCollegeID;
            return View("~/Views/College/AcademicPerformancePointsCreate.cshtml", academicPerformancePoints);
        }

        //
        // POST: /AcademicPerformancePoints/AcademicPerformancePointsEdit
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AcademicPerformancePointsEdit(AcademicPerformancePoints academicPerformancePoints)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = academicPerformancePoints.collegeId;
            }
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId1, academicPerformancePoints.obtainedPoints1, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId2, academicPerformancePoints.obtainedPoints2, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId3, academicPerformancePoints.obtainedPoints3, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId4, academicPerformancePoints.obtainedPoints4, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId5, academicPerformancePoints.obtainedPoints5, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId6, academicPerformancePoints.obtainedPoints6, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId7, academicPerformancePoints.obtainedPoints7, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId8, academicPerformancePoints.obtainedPoints8, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId9, academicPerformancePoints.obtainedPoints9, userID);
            SaveAcademicPerformance(userCollegeID, academicPerformancePoints.typeId10, academicPerformancePoints.obtainedPoints10, userID);
            academicPerformancePoints.collegeId = userCollegeID;
            TempData["Success"] = "Academic Performance details are Updated successfully.";
            return View("~/Views/College/AcademicPerformancePointsCreate.cshtml", academicPerformancePoints);
        }

        //
        // GET: /InfrastructureParameters/InfrastructureParametersView
        [HttpGet]
        [Authorize(Roles = "Committee,DataEntry,College,Admin")]
        public ActionResult AcademicPerformancePointsView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int totalObtainedPoints = 0;
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int exisiId = db.jntuh_college_academic_performance_points.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            AcademicPerformancePoints academicPerformancePoints = new AcademicPerformancePoints();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            if (exisiId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Norecords = false;
                List<jntuh_college_academic_performance_points> collegeAcademicPerformancePoints = db.jntuh_college_academic_performance_points.Where(o => o.collegeId == userCollegeID).ToList();
                if (collegeAcademicPerformancePoints.Count() > 0)
                {
                    academicPerformancePoints.typeId1 = collegeAcademicPerformancePoints[0].typeId;
                    academicPerformancePoints.typeId2 = collegeAcademicPerformancePoints[1].typeId;
                    academicPerformancePoints.typeId3 = collegeAcademicPerformancePoints[2].typeId;
                    academicPerformancePoints.typeId4 = collegeAcademicPerformancePoints[3].typeId;
                    academicPerformancePoints.typeId5 = collegeAcademicPerformancePoints[4].typeId;
                    academicPerformancePoints.typeId6 = collegeAcademicPerformancePoints[5].typeId;
                    academicPerformancePoints.typeId7 = collegeAcademicPerformancePoints[6].typeId;
                    academicPerformancePoints.typeId8 = collegeAcademicPerformancePoints[7].typeId;
                    academicPerformancePoints.typeId9 = collegeAcademicPerformancePoints[8].typeId;
                    academicPerformancePoints.typeId10 = collegeAcademicPerformancePoints[9].typeId;

                    academicPerformancePoints.obtainedPoints1 = collegeAcademicPerformancePoints[0].pointsObtained;
                    academicPerformancePoints.obtainedPoints2 = collegeAcademicPerformancePoints[1].pointsObtained;
                    academicPerformancePoints.obtainedPoints3 = collegeAcademicPerformancePoints[2].pointsObtained;
                    academicPerformancePoints.obtainedPoints4 = collegeAcademicPerformancePoints[3].pointsObtained;
                    academicPerformancePoints.obtainedPoints5 = collegeAcademicPerformancePoints[4].pointsObtained;
                    academicPerformancePoints.obtainedPoints6 = collegeAcademicPerformancePoints[5].pointsObtained;
                    academicPerformancePoints.obtainedPoints7 = collegeAcademicPerformancePoints[6].pointsObtained;
                    academicPerformancePoints.obtainedPoints8 = collegeAcademicPerformancePoints[7].pointsObtained;
                    academicPerformancePoints.obtainedPoints9 = collegeAcademicPerformancePoints[8].pointsObtained;
                    academicPerformancePoints.obtainedPoints10 = collegeAcademicPerformancePoints[9].pointsObtained;

                }
                foreach (var item in collegeAcademicPerformancePoints)
                {
                    totalObtainedPoints += item.pointsObtained;
                }
                int totalAllotedPoints = 0;
                int[] allotedPoints = db.jntuh_academic_performance.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
                foreach (int item in allotedPoints)
                {
                    totalAllotedPoints += item;
                }
                academicPerformancePoints.allotedPoints1 = allotedPoints[0];
                academicPerformancePoints.allotedPoints2 = allotedPoints[1];
                academicPerformancePoints.allotedPoints3 = allotedPoints[2];
                academicPerformancePoints.allotedPoints4 = allotedPoints[3];
                academicPerformancePoints.allotedPoints5 = allotedPoints[4];
                academicPerformancePoints.allotedPoints6 = allotedPoints[5];
                academicPerformancePoints.allotedPoints7 = allotedPoints[6];
                academicPerformancePoints.allotedPoints8 = allotedPoints[7];
                academicPerformancePoints.allotedPoints9 = allotedPoints[8];
                academicPerformancePoints.allotedPoints10 = allotedPoints[9];
                academicPerformancePoints.totalAllotedPoints = totalAllotedPoints;
                academicPerformancePoints.totalObtainedPoints = totalObtainedPoints;
            }
            return View("~/Views/College/AcademicPerformancePointsView.cshtml", academicPerformancePoints);
        }

    }
}
