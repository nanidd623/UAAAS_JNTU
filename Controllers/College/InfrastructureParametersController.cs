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
    public class InfrastructureParametersController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /InfrastructureParameters/InfrastructureParametersCreate
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult InfrastructureParametersCreate(string collegeId)
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
            int exisiId = db.jntuh_college_infrastructure_parameters.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
           

           
            if (exisiId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("InfrastructureParametersView", "InfrastructureParameters");
            }
            if (userCollegeID > 0 && exisiId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("InfrastructureParametersEdit", "InfrastructureParameters", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            InfrastructureParameters infrastructureParameters = new InfrastructureParameters();
            int[] allotedPoints = db.jntuh_infrastructure_parameters.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            infrastructureParameters.allottedPoints1 = allotedPoints[0];
            infrastructureParameters.allottedPoints2 = allotedPoints[1];
            infrastructureParameters.allottedPoints3 = allotedPoints[2];
            infrastructureParameters.allottedPoints4 = allotedPoints[3];
            infrastructureParameters.allottedPoints5 = allotedPoints[4];
            infrastructureParameters.allottedPoints6 = allotedPoints[5];
            infrastructureParameters.allottedPoints7 = allotedPoints[6];
            infrastructureParameters.allottedPoints8 = allotedPoints[7];
            infrastructureParameters.allottedPoints9 = allotedPoints[8];
            infrastructureParameters.allottedPoints10 = allotedPoints[9];
            infrastructureParameters.totalAllotedPoints = totalAllotedPoints;
            infrastructureParameters.collegeId = userCollegeID;
            return View("~/Views/College/InfrastructureParametersCreate.cshtml", infrastructureParameters);
        }

        //
        // POST: /InfrastructureParameters/InfrastructureParametersCreate
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult InfrastructureParametersCreate(InfrastructureParameters infrastructureParameters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = infrastructureParameters.collegeId;
            }
            int[] typeId = db.jntuh_infrastructure_parameters.Where(o => o.isActive == true).Select(o => o.id).ToArray();
            SaveInfraStructure(userCollegeID, typeId[0], infrastructureParameters.Infrastructure1, userID);
            SaveInfraStructure(userCollegeID, typeId[1], infrastructureParameters.Infrastructure2, userID);
            SaveInfraStructure(userCollegeID, typeId[2], infrastructureParameters.Infrastructure3, userID);
            SaveInfraStructure(userCollegeID, typeId[3], infrastructureParameters.Infrastructure4, userID);
            SaveInfraStructure(userCollegeID, typeId[4], infrastructureParameters.Infrastructure5, userID);
            SaveInfraStructure(userCollegeID, typeId[5], infrastructureParameters.Infrastructure6, userID);
            SaveInfraStructure(userCollegeID, typeId[6], infrastructureParameters.Infrastructure7, userID);
            SaveInfraStructure(userCollegeID, typeId[7], infrastructureParameters.Infrastructure8, userID);
            SaveInfraStructure(userCollegeID, typeId[8], infrastructureParameters.Infrastructure9, userID);
            SaveInfraStructure(userCollegeID, typeId[9], infrastructureParameters.Infrastructure10, userID);
            int totalAllotedPoints = 0;
            int[] allotedPoints = db.jntuh_infrastructure_parameters.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            infrastructureParameters.allottedPoints1 = allotedPoints[0];
            infrastructureParameters.allottedPoints2 = allotedPoints[1];
            infrastructureParameters.allottedPoints3 = allotedPoints[2];
            infrastructureParameters.allottedPoints4 = allotedPoints[3];
            infrastructureParameters.allottedPoints5 = allotedPoints[4];
            infrastructureParameters.allottedPoints6 = allotedPoints[5];
            infrastructureParameters.allottedPoints7 = allotedPoints[6];
            infrastructureParameters.allottedPoints8 = allotedPoints[7];
            infrastructureParameters.allottedPoints9 = allotedPoints[8];
            infrastructureParameters.allottedPoints10 = allotedPoints[9];
            infrastructureParameters.totalAllotedPoints = totalAllotedPoints;
            infrastructureParameters.collegeId = userCollegeID;
            TempData["Success"] = "Infrastructure Parameters details are Saved successfully.";
            return View("~/Views/College/InfrastructureParametersCreate.cshtml", infrastructureParameters);
        }

        private void SaveInfraStructure(int collegeId, int typeId, int obtainedPoints, int userId)
        {
            jntuh_college_infrastructure_parameters infrastructureParameters = new jntuh_college_infrastructure_parameters();
            infrastructureParameters.collegeId = collegeId;
            infrastructureParameters.typeId = typeId;
            infrastructureParameters.pointsObtained = obtainedPoints;

            int existId = db.jntuh_college_infrastructure_parameters.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.id)
                                                                    .FirstOrDefault();
            if(existId == 0)
            {
                infrastructureParameters.createdBy = userId;
                infrastructureParameters.createdOn = DateTime.Now;
                db.jntuh_college_infrastructure_parameters.Add(infrastructureParameters);
            }
            else
            {
                infrastructureParameters.id = existId;
                infrastructureParameters.createdBy = db.jntuh_college_infrastructure_parameters.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.createdBy)
                                                                    .FirstOrDefault();
                infrastructureParameters.createdOn = db.jntuh_college_infrastructure_parameters.Where(i => i.collegeId == collegeId && i.typeId == typeId)
                                                                    .Select(i => i.createdOn)
                                                                    .FirstOrDefault();
                infrastructureParameters.updatedBy = userId;
                infrastructureParameters.updatedOn = DateTime.Now;
                db.Entry(infrastructureParameters).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        //
        // GET: /InfrastructureParameters/InfrastructureParametersEdit
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult InfrastructureParametersEdit(string collegeId)
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
            int exisitId = db.jntuh_college_infrastructure_parameters.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (exisitId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("InfrastructureParametersCreate", "InfrastructureParameters");
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("InfrastructureParametersView", "InfrastructureParameters");
            }
            else
            {
                ViewBag.IsEditable = true;
            }
            InfrastructureParameters infrastructureParameters = new InfrastructureParameters();
            List<jntuh_college_infrastructure_parameters> collegeInfrastructureParameters = db.jntuh_college_infrastructure_parameters.Where(o => o.collegeId == userCollegeID).ToList();
            if (collegeInfrastructureParameters.Count() > 0)
            {
                infrastructureParameters.InfrastructureId1 = collegeInfrastructureParameters[0].typeId;
                infrastructureParameters.InfrastructureId2 = collegeInfrastructureParameters[1].typeId;
                infrastructureParameters.InfrastructureId3 = collegeInfrastructureParameters[2].typeId;
                infrastructureParameters.InfrastructureId4 = collegeInfrastructureParameters[3].typeId;
                infrastructureParameters.InfrastructureId5 = collegeInfrastructureParameters[4].typeId;
                infrastructureParameters.InfrastructureId6 = collegeInfrastructureParameters[5].typeId;
                infrastructureParameters.InfrastructureId7 = collegeInfrastructureParameters[6].typeId;
                infrastructureParameters.InfrastructureId8 = collegeInfrastructureParameters[7].typeId;
                infrastructureParameters.InfrastructureId9 = collegeInfrastructureParameters[8].typeId;
                infrastructureParameters.InfrastructureId10 = collegeInfrastructureParameters[9].typeId;

                infrastructureParameters.Infrastructure1 = collegeInfrastructureParameters[0].pointsObtained;
                infrastructureParameters.Infrastructure2 = collegeInfrastructureParameters[1].pointsObtained;
                infrastructureParameters.Infrastructure3 = collegeInfrastructureParameters[2].pointsObtained;
                infrastructureParameters.Infrastructure4 = collegeInfrastructureParameters[3].pointsObtained;
                infrastructureParameters.Infrastructure5 = collegeInfrastructureParameters[4].pointsObtained;
                infrastructureParameters.Infrastructure6 = collegeInfrastructureParameters[5].pointsObtained;
                infrastructureParameters.Infrastructure7 = collegeInfrastructureParameters[6].pointsObtained;
                infrastructureParameters.Infrastructure8 = collegeInfrastructureParameters[7].pointsObtained;
                infrastructureParameters.Infrastructure9 = collegeInfrastructureParameters[8].pointsObtained;
                infrastructureParameters.Infrastructure10 = collegeInfrastructureParameters[9].pointsObtained;

            }
            int totalAllotedPoints = 0;
            int[] allotedPoints = db.jntuh_infrastructure_parameters.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
            foreach (int item in allotedPoints)
            {
                totalAllotedPoints += item;
            }
            infrastructureParameters.allottedPoints1 = allotedPoints[0];
            infrastructureParameters.allottedPoints2 = allotedPoints[1];
            infrastructureParameters.allottedPoints3 = allotedPoints[2];
            infrastructureParameters.allottedPoints4 = allotedPoints[3];
            infrastructureParameters.allottedPoints5 = allotedPoints[4];
            infrastructureParameters.allottedPoints6 = allotedPoints[5];
            infrastructureParameters.allottedPoints7 = allotedPoints[6];
            infrastructureParameters.allottedPoints8 = allotedPoints[7];
            infrastructureParameters.allottedPoints9 = allotedPoints[8];
            infrastructureParameters.allottedPoints10 = allotedPoints[9];
            infrastructureParameters.totalAllotedPoints = totalAllotedPoints;
            infrastructureParameters.collegeId = userCollegeID;
            return View("~/Views/College/InfrastructureParametersCreate.cshtml", infrastructureParameters);
        }

        //
        // POST: /InfrastructureParameters/InfrastructureParametersEdit
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult InfrastructureParametersEdit(InfrastructureParameters infrastructureParameters)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = infrastructureParameters.collegeId;
            }
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId1, infrastructureParameters.Infrastructure1, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId2, infrastructureParameters.Infrastructure2, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId3, infrastructureParameters.Infrastructure3, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId4, infrastructureParameters.Infrastructure4, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId5, infrastructureParameters.Infrastructure5, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId6, infrastructureParameters.Infrastructure6, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId7, infrastructureParameters.Infrastructure7, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId8, infrastructureParameters.Infrastructure8, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId9, infrastructureParameters.Infrastructure9, userID);
            SaveInfraStructure(userCollegeID, infrastructureParameters.InfrastructureId10, infrastructureParameters.Infrastructure10, userID);
            infrastructureParameters.collegeId = userCollegeID;
            TempData["Success"] = "Infrastructure Parameters details are Updated successfully.";
            return View("~/Views/College/InfrastructureParametersCreate.cshtml", infrastructureParameters);
        }

        //
        // GET: /InfrastructureParameters/InfrastructureParametersView
        [HttpGet]
        [Authorize(Roles = "Committee,DataEntry,College,Admin")]
        public ActionResult InfrastructureParametersView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int totalObtainedPoints = 0;
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int exisiId = db.jntuh_college_infrastructure_parameters.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            InfrastructureParameters infrastructureParameters = new InfrastructureParameters();
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
                List<jntuh_college_infrastructure_parameters> collegeInfrastructureParameters = db.jntuh_college_infrastructure_parameters.Where(o => o.collegeId == userCollegeID).ToList();
                if (collegeInfrastructureParameters.Count() > 0)
                {
                    infrastructureParameters.InfrastructureId1 = collegeInfrastructureParameters[0].typeId;
                    infrastructureParameters.InfrastructureId2 = collegeInfrastructureParameters[1].typeId;
                    infrastructureParameters.InfrastructureId3 = collegeInfrastructureParameters[2].typeId;
                    infrastructureParameters.InfrastructureId4 = collegeInfrastructureParameters[3].typeId;
                    infrastructureParameters.InfrastructureId5 = collegeInfrastructureParameters[4].typeId;
                    infrastructureParameters.InfrastructureId6 = collegeInfrastructureParameters[5].typeId;
                    infrastructureParameters.InfrastructureId7 = collegeInfrastructureParameters[6].typeId;
                    infrastructureParameters.InfrastructureId8 = collegeInfrastructureParameters[7].typeId;
                    infrastructureParameters.InfrastructureId9 = collegeInfrastructureParameters[8].typeId;
                    infrastructureParameters.InfrastructureId10 = collegeInfrastructureParameters[9].typeId;

                    infrastructureParameters.Infrastructure1 = collegeInfrastructureParameters[0].pointsObtained;
                    infrastructureParameters.Infrastructure2 = collegeInfrastructureParameters[1].pointsObtained;
                    infrastructureParameters.Infrastructure3 = collegeInfrastructureParameters[2].pointsObtained;
                    infrastructureParameters.Infrastructure4 = collegeInfrastructureParameters[3].pointsObtained;
                    infrastructureParameters.Infrastructure5 = collegeInfrastructureParameters[4].pointsObtained;
                    infrastructureParameters.Infrastructure6 = collegeInfrastructureParameters[5].pointsObtained;
                    infrastructureParameters.Infrastructure7 = collegeInfrastructureParameters[6].pointsObtained;
                    infrastructureParameters.Infrastructure8 = collegeInfrastructureParameters[7].pointsObtained;
                    infrastructureParameters.Infrastructure9 = collegeInfrastructureParameters[8].pointsObtained;
                    infrastructureParameters.Infrastructure10 = collegeInfrastructureParameters[9].pointsObtained;

                }
                foreach (var item in collegeInfrastructureParameters)
                {
                    totalObtainedPoints += item.pointsObtained;
                }
                int totalAllotedPoints = 0;                
                int[] allotedPoints = db.jntuh_infrastructure_parameters.Where(i => i.isActive == true).Select(i => i.pointsAllotted).ToArray();
                foreach (int item in allotedPoints)
                {
                    totalAllotedPoints += item;
                }
                infrastructureParameters.allottedPoints1 = allotedPoints[0];
                infrastructureParameters.allottedPoints2 = allotedPoints[1];
                infrastructureParameters.allottedPoints3 = allotedPoints[2];
                infrastructureParameters.allottedPoints4 = allotedPoints[3];
                infrastructureParameters.allottedPoints5 = allotedPoints[4];
                infrastructureParameters.allottedPoints6 = allotedPoints[5];
                infrastructureParameters.allottedPoints7 = allotedPoints[6];
                infrastructureParameters.allottedPoints8 = allotedPoints[7];
                infrastructureParameters.allottedPoints9 = allotedPoints[8];
                infrastructureParameters.allottedPoints10 = allotedPoints[9];
                infrastructureParameters.totalAllotedPoints = totalAllotedPoints;
                infrastructureParameters.totalObtainedPoints = totalObtainedPoints;
            }
            return View("~/Views/College/InfrastructureParametersView.cshtml", infrastructureParameters);
        }
    }
}
