using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class GrievanceRedressalComplaintsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }          

             List<GrievanceRedressalCommittee> committee = db.jntuh_college_grievance_committee.Where(a => a.collegeId == userCollegeID).Select(a =>
                                            new GrievanceRedressalCommittee
                                            {
                                                id = a.id,
                                                //rowNumber = index + 1,
                                                collegeId = a.collegeId,
                                                memberDesignation = a.memberDesignation,
                                                memberName = a.memberName,
                                                my_aspnet_users = a.my_aspnet_users,
                                                my_aspnet_users1 = a.my_aspnet_users1,

                                            }).OrderBy(r => r.memberName).ToList();

            List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new GrievanceRedressalComplaints
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;

            return View();
            
        }

         [HttpGet]
         [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
         public ActionResult AddEditRecord(int? id,string collegeId)
         {
             if (Request.IsAjaxRequest())
             {
                 if (id != null)
                 {
                     ViewBag.IsUpdate = true;
                     GrievanceRedressalComplaints complaints = db.jntuh_college_grievance_complaints.Where(oc => oc.id == id).Select(a =>
                                                new GrievanceRedressalComplaints
                                                {
                                                    id = a.id,
                                                    collegeId = a.collegeId,
                                                    complaintReceived = a.complaintReceived,
                                                    actionsTaken = a.actionsTaken,
                                                    createdBy = a.createdBy,
                                                    createdOn = a.createdOn,
                                                    updatedBy = a.updatedBy,
                                                    updatedOn = a.updatedOn,
                                                    jntuh_college = a.jntuh_college,
                                                    my_aspnet_users = a.my_aspnet_users,
                                                    my_aspnet_users1 = a.my_aspnet_users1
                                                }).FirstOrDefault();
                     return PartialView("_GrievanceRedressalComplaintsData", complaints);
                 }
                 else
                 {
                     GrievanceRedressalComplaints complaints = new GrievanceRedressalComplaints();
                     if (collegeId != null)
                     {
                         int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                         complaints.collegeId = userCollegeID;
                     }
                     ViewBag.IsUpdate = false;
                     return PartialView("_GrievanceRedressalComplaintsData", complaints);
                 }
             }
             else
             {
                 if (id != null)
                 {
                     ViewBag.IsUpdate = true;
                     GrievanceRedressalComplaints uComplaints = db.jntuh_college_grievance_complaints.Where(oc => oc.id == id).Select(a =>
                                                new GrievanceRedressalComplaints
                                                {
                                                    id = a.id,
                                                    collegeId = a.collegeId,
                                                    complaintReceived = a.complaintReceived,
                                                    actionsTaken = a.actionsTaken,
                                                    createdBy = a.createdBy,
                                                    createdOn = a.createdOn,
                                                    updatedBy = a.updatedBy,
                                                    updatedOn = a.updatedOn,
                                                    jntuh_college = a.jntuh_college,
                                                    my_aspnet_users = a.my_aspnet_users,
                                                    my_aspnet_users1 = a.my_aspnet_users1
                                                }).FirstOrDefault();
                     return View("GrievanceRedressalComplaintsData", uComplaints);
                 }
                 else
                 {
                     GrievanceRedressalComplaints uComplaints = new GrievanceRedressalComplaints();
                     if (collegeId != null)
                     {
                         int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                         uComplaints.collegeId = userCollegeID;
                     }
                     ViewBag.IsUpdate = false;
                     return View("GrievanceRedressalComplaintsData", uComplaints);
                 }
             }
         }

         [HttpPost]
         [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
         public ActionResult AddEditRecord(GrievanceRedressalComplaints Complaints, string cmd)
         {
             if (ModelState.IsValid)
             {
                 int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                 int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                 if (userCollegeID == 0)
                 {
                     userCollegeID = Complaints.collegeId;
                 }
                 if (cmd == "Save")
                 {
                     try
                     {
                         jntuh_college_grievance_complaints jntuh_college_grievance_complaints = new jntuh_college_grievance_complaints();
                         jntuh_college_grievance_complaints.id = Complaints.id;
                         jntuh_college_grievance_complaints.collegeId = userCollegeID;
                         jntuh_college_grievance_complaints.complaintReceived = Complaints.complaintReceived;
                         jntuh_college_grievance_complaints.actionsTaken = Complaints.actionsTaken;
                         jntuh_college_grievance_complaints.createdBy = userID;
                         jntuh_college_grievance_complaints.createdOn = DateTime.Now;

                         db.jntuh_college_grievance_complaints.Add(jntuh_college_grievance_complaints);
                         db.SaveChanges();
                         TempData["ComplaintsSuccess"] = "Grievance Redressal Complaints is Added successfully.";
                         return RedirectToAction("Index", "GrievanceRedressalCommittee", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                     }
                     catch { }
                 }
                 else
                 {
                     try
                     {
                         jntuh_college_grievance_complaints uComplaints = new jntuh_college_grievance_complaints();

                         if (uComplaints != null)
                         {
                             uComplaints.id = Complaints.id;
                             uComplaints.collegeId = userCollegeID;
                             uComplaints.complaintReceived = Complaints.complaintReceived;
                             uComplaints.actionsTaken = Complaints.actionsTaken;
                             uComplaints.createdBy = Complaints.createdBy;
                             uComplaints.createdOn = Complaints.createdOn;
                             uComplaints.updatedBy = userID;
                             uComplaints.updatedOn = DateTime.Now;
                             db.Entry(uComplaints).State = EntityState.Modified;
                             db.SaveChanges();
                             TempData["ComplaintsSuccess"] = "Grievance Redressal Complaints is Updated successfully.";
                         }
                         return RedirectToAction("Index", "GrievanceRedressalCommittee", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                     }
                     catch { }
                 }
             }

             if (Request.IsAjaxRequest())
             {
                 return PartialView("_GrievanceRedressalComplaintsData", Complaints);
             }
             else
             {
                 return View("GrievanceRedressalComplaintsData", Complaints);
             }
         }

         [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
         public ActionResult DeleteRecord(int id)
         {
             jntuh_college_grievance_complaints complaints = db.jntuh_college_grievance_complaints.Where(oc => oc.id == id).FirstOrDefault();
             if (complaints != null)
             {
                 try
                 {
                     db.jntuh_college_grievance_complaints.Remove(complaints);
                     db.SaveChanges();
                     TempData["ComplaintsSuccess"] = "Grievance Redressal Complaints is Deleted successfully.";
                 }
                 catch { }
             }
             return RedirectToAction("Index", "GrievanceRedressalCommittee", new { collegeId = Utilities.EncryptString(complaints.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
         }

         [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
         public ActionResult Details(int id)
         {
             GrievanceRedressalComplaints grievanceRedressalComplaints = db.jntuh_college_grievance_complaints.Where(oc => oc.id == id).Select(a =>
                                               new GrievanceRedressalComplaints
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   complaintReceived = a.complaintReceived,
                                                   actionsTaken = a.actionsTaken,
                                                   my_aspnet_users = a.my_aspnet_users,
                                                   my_aspnet_users1 = a.my_aspnet_users1
                                               }).FirstOrDefault();
             if (grievanceRedressalComplaints != null)
             {
                 if (Request.IsAjaxRequest())
                 {
                     return PartialView("_GrievanceRedressalComplaintsDetails", grievanceRedressalComplaints);
                 }
                 else
                 {
                     return View("GrievanceRedressalComplaintsDetails", grievanceRedressalComplaints);
                 }
             }
             return View("Index", new { collegeId = Utilities.EncryptString(grievanceRedressalComplaints.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
         }


    }
}
