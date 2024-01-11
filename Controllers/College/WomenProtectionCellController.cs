using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class WomenProtectionCellController : BaseController
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

            //IEnumerable<WomenProtectionCell> committee = (from gc in db.jntuh_college_women_protection_cell
            //                                              join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
            //                                              join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
            //                                              where gc.collegeId == userCollegeID
            //                                              select new WomenProtectionCell
            //                                              {
            //                                                  id = gc.id,
            //                                                  collegeId = gc.collegeId,
            //                                                  memberDesignation = gc.memberDesignation,
            //                                                  memberName = gc.memberName,
            //                                                  designationName = d.Designation,
            //                                                  my_aspnet_users = gc.my_aspnet_users,
            //                                                  my_aspnet_users1 = gc.my_aspnet_users1,
            //                                                  actualDesignationId = gc.actualDesignation,
            //                                                  actualDesignation = ad.designation

            //                                              }).OrderBy(r => r.memberName).ToList();

            IEnumerable<WomenProtectionCell> committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                          join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                          join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                          where gc.collegeId == userCollegeID && gc.complainttype == 1
                                                          select new WomenProtectionCell
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
                                                               Email = gc.email,
                                                               Mobile = gc.mobile

                                                          }).OrderBy(r => r.memberName).ToList();

            //List<WomenProtectionCellComplaints> complaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
            //                                  new WomenProtectionCellComplaints
            //                                  {
            //                                      id = a.id,
            //                                      collegeId = a.collegeId,
            //                                      complaintReceived = a.complaintReceived,
            //                                      actionsTaken = a.actionsTaken,
            //                                      my_aspnet_users = a.my_aspnet_users,
            //                                      my_aspnet_users1 = a.my_aspnet_users1,
            //                                  }).OrderBy(r => r.actionsTaken).ToList();

            List<WomenProtectionCellComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 1).Select(a =>
                                              new WomenProtectionCellComplaints
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
            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(s => s.academicYear).FirstOrDefault();
            //Getting Support Document
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Women Protection Cell Support Document")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == prAy && r.enclosureId == enclosureId)
                    .Select(s => s.path)
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
                return RedirectToAction("View", "WomenProtectionCell");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("WP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "WomenProtectionCell");
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
                    //WomenProtectionCell committee = db.jntuh_college_women_protection_cell.Where(oc => oc.id == id).Select(a =>
                    //                          new WomenProtectionCell
                    //                          {
                    //                              id = a.id,
                    //                              collegeId = a.collegeId,
                    //                              memberDesignation = a.memberDesignation,
                    //                              memberName = a.memberName,
                    //                              createdBy = a.createdBy,
                    //                              createdOn = a.createdOn,
                    //                              updatedBy = a.updatedBy,
                    //                              updatedOn = a.updatedOn,
                    //                              jntuh_college = a.jntuh_college,
                    //                              my_aspnet_users = a.my_aspnet_users,
                    //                              my_aspnet_users1 = a.my_aspnet_users1,
                    //                              actualDesignationId = a.actualDesignation
                    //                          }).FirstOrDefault();

                    WomenProtectionCell committee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                              new WomenProtectionCell
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
                    return PartialView("_WomenProtectionCellData", committee);
                }
                else
                {
                    WomenProtectionCell committee = new WomenProtectionCell();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        committee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_WomenProtectionCellData", committee);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    WomenProtectionCell uCommittee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                               new WomenProtectionCell
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
                    return View("WomenProtectionCellData", uCommittee);
                }
                else
                {
                    WomenProtectionCell uCommittee = new WomenProtectionCell();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        uCommittee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("WomenProtectionCellData", uCommittee);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(WomenProtectionCell Committee, string cmd)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
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
                        jntuh_college_women_protection_cell_rti_antiragging_details jntuh_college_women_protection_cell = new jntuh_college_women_protection_cell_rti_antiragging_details();
                        jntuh_college_women_protection_cell.id = Committee.id;
                        jntuh_college_women_protection_cell.collegeId = userCollegeID;
                        jntuh_college_women_protection_cell.academicyear = prAy;
                        jntuh_college_women_protection_cell.complainttype = 1;
                        jntuh_college_women_protection_cell.memberName = Committee.memberName;
                        jntuh_college_women_protection_cell.registrationnumber = Committee.registrationNumber;
                        jntuh_college_women_protection_cell.email = Committee.Email;
                        jntuh_college_women_protection_cell.mobile = Committee.Mobile;
                        jntuh_college_women_protection_cell.memberDesignation = Committee.memberDesignation;
                        jntuh_college_women_protection_cell.actualDesignation = Committee.actualDesignationId;
                        jntuh_college_women_protection_cell.createdBy = userID;
                        jntuh_college_women_protection_cell.createdOn = DateTime.Now;

                        db.jntuh_college_women_protection_cell_rti_antiragging_details.Add(jntuh_college_women_protection_cell);
                        db.SaveChanges();
                        TempData["Success"] = "Women Protection Cell is Added successfully.";
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
                            uCommittee.complainttype = 1;
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
                            TempData["Success"] = "Women Protection Cell is Updated successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_WomenProtectionCellData", Committee);
            }
            else
            {
                return View("WomenProtectionCellData", Committee);
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
                    TempData["Success"] = "Women Protection Cell is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(committee.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            WomenProtectionCell committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                             join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                             join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                             where (gc.id == id)
                                             select new WomenProtectionCell
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

                                                     }).FirstOrDefault();
            if (committee != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_WomenProtectionCellDetails", committee);
                }
                else
                {
                    return View("WomenProtectionCellDetails", committee);
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("WP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                //if (false)
                {
                    return RedirectToAction("Index");
                }
            }

            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(s => s.academicYear).FirstOrDefault();
            //Getting Support Document
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Women Protection Cell Support Document")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == prAy && r.enclosureId == enclosureId)
                    .Select(s => s.path)
                    .FirstOrDefault();

            //IEnumerable<WomenProtectionCell> committee = (from gc in db.jntuh_college_women_protection_cell
            //                                              join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
            //                                              join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
            //                                              where gc.collegeId == userCollegeID
            //                                              select new WomenProtectionCell
            //                                                      {
            //                                                          id = gc.id,
            //                                                          collegeId = gc.collegeId,
            //                                                          memberDesignation = gc.memberDesignation,
            //                                                          memberName = gc.memberName,
            //                                                          designationName = d.Designation,
            //                                                          my_aspnet_users = gc.my_aspnet_users,
            //                                                          my_aspnet_users1 = gc.my_aspnet_users1,
            //                                                          actualDesignationId = gc.actualDesignation,
            //                                                          actualDesignation = ad.designation

            //                                                      }).OrderBy(r => r.memberName).ToList();

            IEnumerable<WomenProtectionCell> committee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                          join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                          join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                          where gc.collegeId == userCollegeID && gc.complainttype == 1
                                                          select new WomenProtectionCell
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

            //List<WomenProtectionCellComplaints> complaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
            //                                  new WomenProtectionCellComplaints
            //                                  {
            //                                      id = a.id,
            //                                      collegeId = a.collegeId,
            //                                      //complaintReceived = a.complaintReceived,
            //                                      actionsTaken = a.actionsTaken,
            //                                      my_aspnet_users = a.my_aspnet_users,
            //                                      my_aspnet_users1 = a.my_aspnet_users1,
            //                                  }).OrderBy(r => r.actionsTaken).ToList();

            List<WomenProtectionCellComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 1).Select(a =>
                                              new WomenProtectionCellComplaints
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
            IEnumerable<WomenProtectionCell> committee = (from gc in db.jntuh_college_women_protection_cell
                                                          join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                          join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                          where gc.collegeId == userCollegeID
                                                          select new WomenProtectionCell
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
                                                          }).ToList();

            List<WomenProtectionCellComplaints> complaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new WomenProtectionCellComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  //complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1,
                                              }).ToList();

            ViewBag.Committee = committee;
            ViewBag.Complaints = complaints;
            ViewBag.CommitteeCount = committee.Count();
            ViewBag.ComplaintsCount = complaints.Count();
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult WomenProtectionSupportfile(HttpPostedFileBase WomenProtectfileUploader, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string fileName = string.Empty;
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Women Protection Cell Support Document")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var college_enclosures =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.academicyearId == ay0 && e.collegeID == userCollegeID)
                    .Select(e => e)
                    .FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.academicyearId = ay0;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;
            string filepath = "~/Content/Upload/CollegeEnclosures/WomenProtection";
            if (WomenProtectfileUploader != null)
            {
                if (!Directory.Exists(Server.MapPath(filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(filepath));
                }
                if (college_enclosures != null && college_enclosures.path != null)
                {

                    WomenProtectfileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
                        college_enclosures.path));
                    jntuh_college_enclosures.path = college_enclosures.path;
                }
                else
                {
                    string ext = Path.GetExtension(WomenProtectfileUploader.FileName);
                    fileName =
                        db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                        "_WPS_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                    WomenProtectfileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
                        fileName));
                    jntuh_college_enclosures.path = fileName;
                }
            }
            else if (!string.IsNullOrEmpty(college_enclosures.path))
            {
                fileName = college_enclosures.path;
                jntuh_college_enclosures.path = fileName;
            }

            if (college_enclosures == null)
            {
                jntuh_college_enclosures.createdBy = userID;
                jntuh_college_enclosures.createdOn = DateTime.Now;
                db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                db.SaveChanges();
                TempData["Success"] = "Supporting Document Save successfuly";
            }
            else
            {
                college_enclosures.path = college_enclosures.path;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Supporting Document Update successfuly";
            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
        }
    }
}
