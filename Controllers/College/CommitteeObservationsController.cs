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
    public class CommitteeObservationsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /CommitteeObservations/CommitteeObservationsCreate
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult CommitteeObservationsCreate(string collegeId)
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
            int exisiId = db.jntuh_college_committee_observations.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();

            if (exisiId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("CommitteeObservationsView", "CommitteeObservations");
            }
            if (exisiId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("CommitteeObservationsEdit", "CommitteeObservations", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (exisiId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            CommitteeObservations committeeObservations = new CommitteeObservations();
            committeeObservations.collegeId = userCollegeID;
            return View("~/Views/College/CommitteeObservationsCreate.cshtml", committeeObservations);
        }

        //
        // POST: /CommitteeObservations/CommitteeObservationsCreate
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult CommitteeObservationsCreate(CommitteeObservations committeeObservations)
        {            
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = committeeObservations.collegeId;
            }
            int[] committeeObservationsId = db.jntuh_committee_observations.Where(o => o.isActive == true).Select(o => o.id).ToArray();
            SaveObservation(committeeObservationsId[0], committeeObservations.committeeObservations1, userID, userCollegeID);
            SaveObservation(committeeObservationsId[1], committeeObservations.committeeObservations2, userID, userCollegeID);
            SaveObservation(committeeObservationsId[2], committeeObservations.committeeObservations3, userID, userCollegeID);
            SaveObservation(committeeObservationsId[3], committeeObservations.committeeObservations4, userID, userCollegeID);
            SaveObservation(committeeObservationsId[4], committeeObservations.committeeObservations5, userID, userCollegeID);
            SaveObservation(committeeObservationsId[5], committeeObservations.committeeObservations6, userID, userCollegeID);
            SaveObservation(committeeObservationsId[6], committeeObservations.committeeObservations7, userID, userCollegeID);
            SaveObservation(committeeObservationsId[7], committeeObservations.committeeObservations8, userID, userCollegeID);
            SaveObservation(committeeObservationsId[8], committeeObservations.committeeObservations9, userID, userCollegeID);
            SaveObservation(committeeObservationsId[9], committeeObservations.committeeObservations10, userID, userCollegeID);
            SaveObservation(committeeObservationsId[10], committeeObservations.committeeObservations11, userID, userCollegeID);
            SaveObservation(committeeObservationsId[11], committeeObservations.committeeObservations12, userID, userCollegeID);
            SaveObservation(committeeObservationsId[12], committeeObservations.committeeObservations13, userID, userCollegeID);
            committeeObservations.collegeId = userCollegeID;
            TempData["Success"] = "Committee Observation details are Saved successfully.";
            return View("~/Views/College/CommitteeObservationsCreate.cshtml", committeeObservations);
        }

        private void SaveObservation(int observationId, string observation, int userId, int collegeId)
        {
            jntuh_college_committee_observations collegeCommitteeObservations = new jntuh_college_committee_observations();
            collegeCommitteeObservations.collegeId=collegeId;
            collegeCommitteeObservations.observationTypeId = observationId;
            collegeCommitteeObservations.observations = observation;
            collegeCommitteeObservations.isActive = true;

            int existId = db.jntuh_college_committee_observations.Where(o => o.collegeId == collegeId && o.observationTypeId == observationId)
                                                                 .Select(o => o.id)
                                                                 .FirstOrDefault();
            if (existId == 0)
            {
                collegeCommitteeObservations.createdBy = userId;
                collegeCommitteeObservations.createdOn = DateTime.Now;
                db.jntuh_college_committee_observations.Add(collegeCommitteeObservations);
            }
            else
            {
                collegeCommitteeObservations.id = existId;
                collegeCommitteeObservations.createdBy = db.jntuh_college_committee_observations.Where(o => o.collegeId == collegeId && o.observationTypeId == observationId)
                                                               .Select(o => o.createdBy)
                                                               .FirstOrDefault();
                collegeCommitteeObservations.createdOn = db.jntuh_college_committee_observations.Where(o => o.collegeId == collegeId && o.observationTypeId == observationId)
                                                               .Select(o => o.createdOn)
                                                               .FirstOrDefault();
                collegeCommitteeObservations.updatedBy = userId;
                collegeCommitteeObservations.updatedOn = DateTime.Now;
                db.Entry(collegeCommitteeObservations).State = EntityState.Modified;
            }

            db.SaveChanges();
        }

        //
        // GET: /CommitteeObservations/CommitteeObservationsEdit
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult CommitteeObservationsEdit(string collegeId)
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
            int exisitId = db.jntuh_college_committee_observations.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (exisitId == 0)
            {
                return RedirectToAction("CommitteeObservationsCreate", "CommitteeObservations");
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("CommitteeObservationsView", "CommitteeObservations");
            }
            else
            {
                ViewBag.IsEditable = true;
            }
            CommitteeObservations committeeObservations = new CommitteeObservations();
            List<jntuh_college_committee_observations> collegeCommitteeObsercvations = db.jntuh_college_committee_observations.Where(o => o.collegeId == userCollegeID).ToList();
            if (collegeCommitteeObsercvations.Count() > 0)
            {
                committeeObservations.committeeObservationId1 = collegeCommitteeObsercvations[0].observationTypeId;
                committeeObservations.committeeObservations1 = collegeCommitteeObsercvations[0].observations;
                committeeObservations.committeeObservationId2 = collegeCommitteeObsercvations[1].observationTypeId;
                committeeObservations.committeeObservations2 = collegeCommitteeObsercvations[1].observations;
                committeeObservations.committeeObservationId3 = collegeCommitteeObsercvations[2].observationTypeId;
                committeeObservations.committeeObservations3 = collegeCommitteeObsercvations[2].observations;
                committeeObservations.committeeObservationId4 = collegeCommitteeObsercvations[3].observationTypeId;
                committeeObservations.committeeObservations4 = collegeCommitteeObsercvations[3].observations;
                committeeObservations.committeeObservationId5 = collegeCommitteeObsercvations[4].observationTypeId;
                committeeObservations.committeeObservations5 = collegeCommitteeObsercvations[4].observations;
                committeeObservations.committeeObservationId6 = collegeCommitteeObsercvations[5].observationTypeId;
                committeeObservations.committeeObservations6 = collegeCommitteeObsercvations[5].observations;
                committeeObservations.committeeObservationId7 = collegeCommitteeObsercvations[6].observationTypeId;
                committeeObservations.committeeObservations7 = collegeCommitteeObsercvations[6].observations;
                committeeObservations.committeeObservationId8 = collegeCommitteeObsercvations[7].observationTypeId;
                committeeObservations.committeeObservations8 = collegeCommitteeObsercvations[7].observations;
                committeeObservations.committeeObservationId9 = collegeCommitteeObsercvations[8].observationTypeId;
                committeeObservations.committeeObservations9 = collegeCommitteeObsercvations[8].observations;
                committeeObservations.committeeObservationId10 = collegeCommitteeObsercvations[9].observationTypeId;
                committeeObservations.committeeObservations10 = collegeCommitteeObsercvations[9].observations;
                committeeObservations.committeeObservationId11 = collegeCommitteeObsercvations[10].observationTypeId;
                committeeObservations.committeeObservations11 = collegeCommitteeObsercvations[10].observations;
                committeeObservations.committeeObservationId12 = collegeCommitteeObsercvations[11].observationTypeId;
                committeeObservations.committeeObservations12 = collegeCommitteeObsercvations[11].observations;
                committeeObservations.committeeObservationId13 = collegeCommitteeObsercvations[12].observationTypeId;
                committeeObservations.committeeObservations13 = collegeCommitteeObsercvations[12].observations;
            }
            committeeObservations.collegeId = userCollegeID;

            return View("~/Views/College/CommitteeObservationsCreate.cshtml", committeeObservations);
        }

        //
        // POST: /CommitteeObservations/CommitteeObservationsEdit
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult CommitteeObservationsEdit(CommitteeObservations committeeObservations)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = committeeObservations.collegeId;
            }
            SaveObservation(committeeObservations.committeeObservationId1, committeeObservations.committeeObservations1, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId2, committeeObservations.committeeObservations2, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId3, committeeObservations.committeeObservations3, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId4, committeeObservations.committeeObservations4, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId5, committeeObservations.committeeObservations5, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId6, committeeObservations.committeeObservations6, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId7, committeeObservations.committeeObservations7, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId8, committeeObservations.committeeObservations8, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId9, committeeObservations.committeeObservations9, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId10, committeeObservations.committeeObservations10, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId11, committeeObservations.committeeObservations11, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId12, committeeObservations.committeeObservations12, userID, userCollegeID);
            SaveObservation(committeeObservations.committeeObservationId13, committeeObservations.committeeObservations13, userID, userCollegeID);
            committeeObservations.collegeId = userCollegeID;
            TempData["Success"] = "Committee Observation details are Updated successfully.";
            return View("~/Views/College/CommitteeObservationsCreate.cshtml", committeeObservations);
        }

        //
        // GET: /CommitteeObservations/CommitteeObservationsEdit
        [HttpGet]
        [Authorize(Roles = "Committee,DataEntry,College,Admin")]
        public ActionResult CommitteeObservationsView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int exisiId = db.jntuh_college_committee_observations.Where(o => o.collegeId == userCollegeID).Select(o => o.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            CommitteeObservations committeeObservations = new CommitteeObservations();
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
                List<jntuh_college_committee_observations> collegeCommitteeObsercvations = db.jntuh_college_committee_observations.Where(o => o.collegeId == userCollegeID).ToList();
                if (collegeCommitteeObsercvations.Count() > 0)
                {
                    committeeObservations.committeeObservations1 = collegeCommitteeObsercvations[0].observations;
                    committeeObservations.committeeObservations2 = collegeCommitteeObsercvations[1].observations;
                    committeeObservations.committeeObservations3 = collegeCommitteeObsercvations[2].observations;
                    committeeObservations.committeeObservations4 = collegeCommitteeObsercvations[3].observations;
                    committeeObservations.committeeObservations5 = collegeCommitteeObsercvations[4].observations;
                    committeeObservations.committeeObservations6 = collegeCommitteeObsercvations[5].observations;
                    committeeObservations.committeeObservations7 = collegeCommitteeObsercvations[6].observations;
                    committeeObservations.committeeObservations8 = collegeCommitteeObsercvations[7].observations;
                    committeeObservations.committeeObservations9 = collegeCommitteeObsercvations[8].observations;
                    committeeObservations.committeeObservations10 = collegeCommitteeObsercvations[9].observations;
                    committeeObservations.committeeObservations11 = collegeCommitteeObsercvations[10].observations;
                    committeeObservations.committeeObservations12 = collegeCommitteeObsercvations[11].observations;
                    committeeObservations.committeeObservations13 = collegeCommitteeObsercvations[12].observations;
                }
            }
            return View("~/Views/College/CommitteeObservationsView.cshtml", committeeObservations);
        }
    }
}
