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

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AntiRaggingCommitteeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(s => s.academicYear).FirstOrDefault();
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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
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

            //Getting Support Document
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Anti-Ragging Support Document")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == ay0 && r.enclosureId == enclosureId)
                    .Select(s => s.path)
                    .FirstOrDefault();

            IEnumerable<AntiRaggingCommittee> committee = (from ac in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                           join d in db.jntuh_grc_designation on ac.memberDesignation equals d.id
                                                           join ad in db.jntuh_designation on ac.actualDesignation equals ad.id
                                                           where ac.collegeId == userCollegeID && ac.complainttype == 3
                                                           select new AntiRaggingCommittee
                                                           {
                                                               id = ac.id,
                                                               //rowNumber = index + 1,
                                                               collegeId = ac.collegeId,
                                                               memberDesignation = (int)ac.memberDesignation,
                                                               memberName = ac.memberName,
                                                               designationName = d.Designation,
                                                               //my_aspnet_users = ac.my_aspnet_users,
                                                               //my_aspnet_users1 = ac.my_aspnet_users1,
                                                               actualDesignationId = ac.actualDesignation,
                                                               actualDesignation = ad.designation,
                                                               registrationNumber = ac.registrationnumber,
                                                               Email = ac.email,
                                                               Mobile = ac.mobile

                                                           }).OrderBy(r => r.memberName).ToList();

            List<AntiRaggingComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 3).Select(a =>
                                              new AntiRaggingComplaints
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
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

            //var pathdata = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID).Select(e => e).ToList();

            //ViewBag.Path = pathdata.Where(e => e.enclosureId == 25).Select(e => e.path).FirstOrDefault();
            var pathdata = db.jntuh_college_enclosures.Where(e => e.collegeID == userCollegeID&&e.academicyearId==ay0&&e.enclosureId==42).Select(e => e.path).FirstOrDefault();

            ViewBag.Path = pathdata;

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
                return RedirectToAction("View", "AntiRaggingCommittee");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "AntiRaggingCommittee");
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string cmd, HttpPostedFileBase file)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                //if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                //{
                //    if (collegeId != null)
                //    {
                //        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                //    }
                //}
            } 
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (ModelState.IsValid)
            {
                if (cmd == "Upload")
                    try
                    {

                        jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                        //  jntuh_college_enclosures.id = collegeEnclosures.id;
                        jntuh_college_enclosures.collegeID = userCollegeID;
                        jntuh_college_enclosures.enclosureId = 42;
                        jntuh_college_enclosures.isActive = true;

                        if (file != null)
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegeEnclosures/Ragging")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegeEnclosures/Ragging"));
                            }

                            var ext = Path.GetExtension(file.FileName);
                            if (ext.ToUpper().Equals(".PDF"))
                            {
                                string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_" + 42 + "_" + "Ragging" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                file.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeEnclosures/Ragging"), fileName, ext));
                                jntuh_college_enclosures.path = string.Format("{0}/{1}{2}", "~/Content/Upload/CollegeEnclosures/Ragging", fileName, ext);
                            }
                        }

                        int existingId = db.jntuh_college_enclosures.Where(d => d.enclosureId == 42 && d.collegeID == userCollegeID&&d.academicyearId==ay0).Select(d => d.id).FirstOrDefault();
                        if (existingId > 0)
                        {
                            jntuh_college_enclosures.id = existingId;
                            jntuh_college_enclosures.academicyearId = ay0;
                            jntuh_college_enclosures.isActive = true;
                            jntuh_college_enclosures.createdBy = db.jntuh_college_enclosures.Where(d => d.id == existingId).Select(d => d.createdBy).FirstOrDefault();
                            jntuh_college_enclosures.createdOn = db.jntuh_college_enclosures.Where(d => d.id == existingId).Select(d => d.createdOn).FirstOrDefault();
                            if (file == null)
                                jntuh_college_enclosures.path = db.jntuh_college_enclosures.Where(d => d.id == existingId).Select(d => d.path).FirstOrDefault();

                            jntuh_college_enclosures.updatedBy = userID;
                            jntuh_college_enclosures.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_enclosures).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "Updated Successfully.";
                        }
                        else
                        {
                            jntuh_college_enclosures.academicyearId = ay0;
                            jntuh_college_enclosures.createdBy = userID;
                            jntuh_college_enclosures.createdOn = DateTime.Now;

                            db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                            db.SaveChanges();
                            TempData["Success"] = "Added Successfully.";
                        }




                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch (Exception ex)
                    {

                    }
            }
            return View();
        }

        
        
        
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id,string collegeId)
        {
            ViewBag.actualDesignation = db.jntuh_designation.Where(ad => ad.isActive == true).Select(ad => ad).ToList();
            ViewBag.Designation = db.jntuh_grc_designation.Where(u => u.isActive == true).Select(u => u).ToList();
            if (Request.IsAjaxRequest())
            {
                if (id != null&&id!=0)
                {
                    ViewBag.IsUpdate = true;
                    AntiRaggingCommittee committee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                              new AntiRaggingCommittee
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
                                                  Email = a.email,
                                                  Mobile = a.mobile,

                                              }).FirstOrDefault();
                    if (committee==null)
                    {
                        TempData["Error"] = "No Data Found";
                        return RedirectToAction("Index", "AntiRaggingCommittee");
                    }
                    return PartialView("_AntiRaggingCommitteeData", committee);
                }
                else
                {
                    AntiRaggingCommittee committee = new AntiRaggingCommittee();
                    if (collegeId != null)
                    {

                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        committee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_AntiRaggingCommitteeData",committee);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    AntiRaggingCommittee uCommittee = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).Select(a =>
                                               new AntiRaggingCommittee
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
                                                   Email = a.email,
                                                   Mobile = a.mobile,

                                               }).FirstOrDefault();
                    return View("AntiRaggingCommitteeData", uCommittee);
                }
                else
                {
                    AntiRaggingCommittee uCommittee = new AntiRaggingCommittee();
                    if (collegeId != null)
                    {

                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        uCommittee.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("AntiRaggingCommitteeData", uCommittee);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(AntiRaggingCommittee Committee, string cmd)
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
                        jntuh_college_women_protection_cell_rti_antiragging_details jntuh_college_antiragging_committee = new jntuh_college_women_protection_cell_rti_antiragging_details();
                        jntuh_college_antiragging_committee.id = Committee.id;
                        jntuh_college_antiragging_committee.collegeId = userCollegeID;
                        jntuh_college_antiragging_committee.academicyear = prAy;
                        jntuh_college_antiragging_committee.complainttype = 3;
                        jntuh_college_antiragging_committee.memberName = Committee.memberName;
                        jntuh_college_antiragging_committee.registrationnumber = Committee.registrationNumber;
                        jntuh_college_antiragging_committee.email = Committee.Email;
                        jntuh_college_antiragging_committee.mobile = Committee.Mobile;
                        jntuh_college_antiragging_committee.memberDesignation = Committee.memberDesignation;
                        jntuh_college_antiragging_committee.actualDesignation = Committee.actualDesignationId;
                        jntuh_college_antiragging_committee.createdBy = userID;
                        jntuh_college_antiragging_committee.createdOn = DateTime.Now;

                        db.jntuh_college_women_protection_cell_rti_antiragging_details.Add(jntuh_college_antiragging_committee);
                        db.SaveChanges();
                        TempData["Success"] = "Anti Ragging Committee is Added successfully.";
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
                            uCommittee.complainttype = 3;
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
                            TempData["Success"] = "Anti Ragging Committee is Updated successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_AntiRaggingCommitteeData", Committee);
            }
            else
            {
                return View("AntiRaggingCommitteeData", Committee);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_women_protection_cell_rti_antiragging_details complaints = db.jntuh_college_women_protection_cell_rti_antiragging_details.Where(oc => oc.id == id).FirstOrDefault();
                if (complaints != null)
                {
                    try
                    {
                        db.jntuh_college_women_protection_cell_rti_antiragging_details.Remove(complaints);
                        db.SaveChanges();
                        TempData["Success"] = "Anti Ragging Committee is Deleted successfully.";
                        return RedirectToAction("Index", "AntiRaggingCommittee", new { collegeId = Utilities.EncryptString(complaints.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                return RedirectToAction("Index", "AntiRaggingCommittee");
            
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            AntiRaggingCommittee AntiRaggingCommittee = (from gc in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                         join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                         join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                         where (gc.id == id)
                                                         select new AntiRaggingCommittee
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
                                                                            Email = gc.email,
                                                                           Mobile = gc.mobile
                                                                       }).FirstOrDefault();
            if (AntiRaggingCommittee != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_AntiRaggingCommitteeDetails", AntiRaggingCommittee);
                }
                else
                {
                    return View("AntiRaggingCommitteeDetails", AntiRaggingCommittee);
                }
            }
            return View("Index");
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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            ViewBag.PreviousYear = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(s => s.academicYear).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AR") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                //if(false)
                {
                    return RedirectToAction("Index");
                }
            }

            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "Anti-Ragging Support Document")
                    .Select(e => e.id)
                    .FirstOrDefault();
            ViewBag.SupportDocument =
                db.jntuh_college_enclosures.Where(
                    r => r.collegeID == userCollegeID && r.academicyearId == prAy && r.enclosureId == enclosureId)
                    .Select(s => s.path)
                    .FirstOrDefault();

            IEnumerable<AntiRaggingCommittee> committee = (from ac in db.jntuh_college_women_protection_cell_rti_antiragging_details
                                                           join d in db.jntuh_grc_designation on ac.memberDesignation equals d.id
                                                           join ad in db.jntuh_designation on ac.actualDesignation equals ad.id
                                                           where ac.collegeId == userCollegeID && ac.complainttype == 3
                                                           select new AntiRaggingCommittee
                                                           {
                                                               id = ac.id,
                                                               collegeId = ac.collegeId,
                                                               memberDesignation = (int)ac.memberDesignation,
                                                               memberName = ac.memberName,
                                                               designationName = d.Designation,
                                                               //my_aspnet_users = gc.my_aspnet_users,
                                                               //my_aspnet_users1 = gc.my_aspnet_users1,
                                                               actualDesignationId = ac.actualDesignation,
                                                               actualDesignation = ad.designation,
                                                               registrationNumber = ac.registrationnumber,
                                                               Email = ac.email,
                                                               Mobile = ac.mobile

                                                           }).OrderBy(r => r.memberName).ToList();

            List<AntiRaggingComplaints> complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(a => a.collegeId == userCollegeID && a.complainttype == 3).Select(a =>
                                              new AntiRaggingComplaints
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
            IEnumerable<AntiRaggingCommittee> committee = (from ac in db.jntuh_college_antiragging_committee
                                                           join d in db.jntuh_grc_designation on ac.memberDesignation equals d.id
                                                           join ad in db.jntuh_designation on ac.actualDesignation equals ad.id
                                                           where ac.collegeId == userCollegeID
                                                           select new AntiRaggingCommittee
                                                           {
                                                               id = ac.id,
                                                               collegeId = ac.collegeId,
                                                               memberDesignation = ac.memberDesignation,
                                                               memberName = ac.memberName,
                                                               designationName = d.Designation,
                                                               my_aspnet_users = ac.my_aspnet_users,
                                                               my_aspnet_users1 = ac.my_aspnet_users1,
                                                               actualDesignationId = ac.actualDesignation,
                                                               actualDesignation = ad.designation
                                                           }).ToList();

            List<AntiRaggingComplaints> complaints = db.jntuh_college_antiragging_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new AntiRaggingComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
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
        public ActionResult AntiRaggingSupportfile(HttpPostedFileBase antiraggingfileUploader, string collegeId)
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
                db.jntuh_enclosures.Where(e => e.documentName == "Anti-Ragging Support Document")
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
            string filepath = "~/Content/Upload/CollegeEnclosures/AntiRagging";
            if (antiraggingfileUploader != null)
            {
                if (!Directory.Exists(Server.MapPath(filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(filepath));
                }
                if (college_enclosures != null && college_enclosures.path != null)
                {

                    antiraggingfileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
                        college_enclosures.path));
                    jntuh_college_enclosures.path = college_enclosures.path;
                }
                else
                {
                    string ext = Path.GetExtension(antiraggingfileUploader.FileName);
                    fileName =
                        db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                        "_ARS_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                    antiraggingfileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath(filepath),
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
