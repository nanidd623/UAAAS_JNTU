using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class RTIDetailsController : BaseController
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

            IEnumerable<RTIDetails> committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                 join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                 join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                 where gc.collegeId == userCollegeID && gc.complainttype == 2
                                                 select new RTIDetails
                                                 {
                                                     id = gc.id,
                                                     collegeId = gc.collegeId,
                                                     memberDesignation = (int)gc.memberDesignation,
                                                     memberName = gc.memberName,
                                                     designationName = d.Designation,
                                                     //my_aspnet_users = gc.my_aspnet_users,
                                                     //my_aspnet_users1 = gc.my_aspnet_users1,
                                                     actualDesignationId = gc.actualDesignation,
                                                     actualDesignation = ad.designation,
                                                     registrationNumber = gc.registrationnumber,
                                                     Email=gc.email,
                                                     Mobile=gc.mobile
                                                 }).OrderBy(r => r.memberName).ToList();

            List<RTIComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 2).Select(a =>
                                              new RTIComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  //complaintReceived = a.complaintReceived,
                                                  complaintGivenByRegNum = a.complaintgivenbyregnum,
                                                  complaintGivenByName = a.complaintgivenbyname,
                                                  complaintOnRegNum = a.complaintonregnum,
                                                  complaintOnName = a.complaintonname,
                                                  complaintDescription = a.complaintdescription,
                                                  complaintSupportingDocPath = a.complaintsupportingdoc,
                                                  actionsTaken = a.actiontaken,
                                                  actionTakenSupportingDocPath = a.actiontakensupportingdoc
                                                  //my_aspnet_users = a.my_aspnet_users,
                                                  //my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (committee.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.CommitteeNotUpload = true;
            }
            else
            {
                ViewBag.CommitteeNotUpload = false;
            }
            if (complaints.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.ComplaintsNotUpload = true;
            }
            else
            {
                ViewBag.ComplaintsNotUpload = false;
            }
            if (status == 0 && (committee.Count() > 0 || complaints.Count() > 0) && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "RTIDetails");
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("RT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "RTIDetails");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    RTIDetails committee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                              new RTIDetails
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  memberDesignation = (int)a.memberDesignation,
                                                  memberName = a.memberName,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,
                                                  //jntuh_college = a.jntuh_college,
                                                  //my_aspnet_users = a.my_aspnet_users,
                                                  //my_aspnet_users1 = a.my_aspnet_users1,
                                                  actualDesignationId = a.actualDesignation,
                                                  registrationNumber = a.registrationnumber,
                                                  Email=a.email,
                                                  Mobile=a.mobile
                                              }).FirstOrDefault();
                    return PartialView("_RTIDetailsData", committee);
                }
                else
                {
                    RTIDetails committee = new RTIDetails();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        committee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_RTIDetailsData", committee);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    RTIDetails uCommittee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                               new RTIDetails
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   memberDesignation = (int)a.memberDesignation,
                                                   memberName = a.memberName,
                                                   createdBy = a.createdBy,
                                                   createdOn = a.createdOn,
                                                   updatedBy = a.updatedBy,
                                                   updatedOn = a.updatedOn,
                                                   //jntuh_college = a.jntuh_college,
                                                   //my_aspnet_users = a.my_aspnet_users,
                                                   //my_aspnet_users1 = a.my_aspnet_users1,
                                                   actualDesignationId = a.actualDesignation,
                                                   registrationNumber = a.registrationnumber,
                                                   Email=a.email,
                                                   Mobile=a.mobile
                                               }).FirstOrDefault();
                    return View("RTIDetailsData", uCommittee);
                }
                else
                {
                    RTIDetails uCommittee = new RTIDetails();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        uCommittee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("RTIDetailsData", uCommittee);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(RTIDetails Committee, string cmd)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userCollegeID == 0)
                if (userCollegeID == 0)
                {
                    userCollegeID = Committee.collegeId;
                }
            if (ModelState.IsValid)
            {

                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_women_protection_cell_rti_antiragging_details jntuh_college_rti_details = new jntuh_college_women_protection_cell_rti_antiragging_details();
                        jntuh_college_rti_details.id = Committee.id;
                        jntuh_college_rti_details.collegeId = userCollegeID;
                        jntuh_college_rti_details.academicyear = prAy;
                        jntuh_college_rti_details.complainttype = 2;
                        jntuh_college_rti_details.memberName = Committee.memberName;
                        jntuh_college_rti_details.registrationnumber = Committee.registrationNumber;
                        jntuh_college_rti_details.email = Committee.Email;
                        jntuh_college_rti_details.mobile = Committee.Mobile;
                        jntuh_college_rti_details.memberDesignation = Committee.memberDesignation;
                        jntuh_college_rti_details.actualDesignation = Committee.actualDesignationId;
                        jntuh_college_rti_details.createdBy = userID;
                        jntuh_college_rti_details.createdOn = DateTime.Now;

                        db.jntuh_college_women_protection_cell_rti_antiragging_details.Add(jntuh_college_rti_details);
                        db.SaveChanges();
                        TempData["Success"] = "RTI Details is Added successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_women_protection_cell_rti_antiragging_details uCommittee = new jntuh_college_women_protection_cell_rti_antiragging_details();

                        if (uCommittee != null)
                        {
                            uCommittee.id = Committee.id;
                            uCommittee.collegeId = userCollegeID;
                            uCommittee.academicyear = prAy;
                            uCommittee.complainttype = 2;
                            uCommittee.memberName = Committee.memberName;
                            uCommittee.registrationnumber = Committee.registrationNumber;
                            uCommittee.email = Committee.Email;
                            uCommittee.mobile = Committee.Mobile;
                            uCommittee.memberDesignation = Committee.memberDesignation;
                            uCommittee.actualDesignation = Committee.actualDesignationId;
                            uCommittee.createdBy = (int)Committee.createdBy;
                            uCommittee.createdOn = Convert.ToDateTime(Committee.createdOn);
                            uCommittee.updatedBy = userID;
                            uCommittee.updatedOn = DateTime.Now;
                            db.Entry(uCommittee).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "RTI Details is Updated successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_RTIDetailsData", Committee);
            }
            else
            {
                return View("RTIDetailsData", Committee);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_women_protection_cell_rti_antiragging_details committee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).FirstOrDefault();
            if (committee != null)
            {
                try
                {
                    db.jntuh_college_women_protection_cell_rti_antiragging_details.Remove(committee);
                    db.SaveChanges();
                    TempData["Success"] = "RTI Details is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(committee.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            RTIDetails committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                    join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                    join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                    where (gc.id == id)
                                    select new RTIDetails
                                    {
                                        id = gc.id,
                                        collegeId = gc.collegeId,
                                        memberDesignation = (int)gc.memberDesignation,
                                        designationName = d.Designation,
                                        memberName = gc.memberName,
                                        //my_aspnet_users = gc.my_aspnet_users,
                                        //my_aspnet_users1 = gc.my_aspnet_users1,
                                        actualDesignationId = gc.actualDesignation,
                                        actualDesignation = ad.designation,
                                        registrationNumber = gc.registrationnumber,
                                        Email=gc.email,
                                        Mobile=gc.mobile
                                    }).OrderBy(r => r.memberName).FirstOrDefault();
            if (committee != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_RTIDetails", committee);
                }
                else
                {
                    return View("RTIDetails", committee);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(committee.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("RT") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                //if (false)
                {
                    return RedirectToAction("Index");
                }
            }

            IEnumerable<RTIDetails> committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                 join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                 join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                 where gc.collegeId == userCollegeID && gc.complainttype == 2
                                                 select new RTIDetails
                                                 {
                                                     id = gc.id,
                                                     collegeId = gc.collegeId,
                                                     memberDesignation = (int)gc.memberDesignation,
                                                     memberName = gc.memberName,
                                                     designationName = d.Designation,
                                                     //my_aspnet_users = gc.my_aspnet_users,
                                                     //my_aspnet_users1 = gc.my_aspnet_users1,
                                                     actualDesignationId = gc.actualDesignation,
                                                     actualDesignation = ad.designation,
                                                     registrationNumber = gc.registrationnumber,
                                                     Email=gc.email,
                                                     Mobile=gc.mobile

                                                 }).OrderBy(r => r.memberName).ToList();

            List<RTIComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 2).Select(a =>
                                              new RTIComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  //complaintReceived = a.complaintReceived,
                                                  complaintGivenByRegNum = a.complaintgivenbyregnum,
                                                  complaintGivenByName = a.complaintgivenbyname,
                                                  complaintOnRegNum = a.complaintonregnum,
                                                  complaintOnName = a.complaintonname,
                                                  complaintDescription = a.complaintdescription,
                                                  complaintSupportingDocPath = a.complaintsupportingdoc,
                                                  actionsTaken = a.actiontaken,
                                                  actionTakenSupportingDocPath = a.actiontakensupportingdoc
                                                  //my_aspnet_users = a.my_aspnet_users,
                                                  //my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            return View();
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            IEnumerable<RTIDetails> committee = (from gc in db.jntuh_college_rti_details
                                                 join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                 join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                 where gc.collegeId == userCollegeID
                                                 select new RTIDetails
                                                 {
                                                     id = gc.id,
                                                     collegeId = gc.collegeId,
                                                     memberDesignation = gc.memberDesignation,
                                                     memberName = gc.memberName,
                                                     designationName = d.Designation,
                                                     my_aspnet_users = gc.my_aspnet_users,
                                                     my_aspnet_users1 = gc.my_aspnet_users1,
                                                     actualDesignationId = gc.actualDesignation,
                                                     actualDesignation = ad.designation

                                                 }).OrderBy(r => r.memberName).ToList();

            List<RTIComplaints> complaints = db.jntuh_college_rti_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new RTIComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).OrderBy(r => r.actionsTaken).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            return View();
        }
    }
}
