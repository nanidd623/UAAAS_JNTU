﻿using System;
using System.Collections.Generic;
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
    public class WomenProtectionCellComplaintsController : BaseController
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

            List<WomenProtectionCell> committee = db.jntuh_college_women_protection_cell.Where(a => a.collegeId == userCollegeID).Select(a =>
                                           new WomenProtectionCell
                                           {
                                               id = a.id,
                                               collegeId = a.collegeId,
                                               memberDesignation = a.memberDesignation,
                                               memberName = a.memberName,
                                               my_aspnet_users = a.my_aspnet_users,
                                               my_aspnet_users1 = a.my_aspnet_users1,

                                           }).OrderBy(r => r.memberName).ToList();

            List<WomenProtectionCellComplaints> complaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new WomenProtectionCellComplaints
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

            return View();

        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    WomenProtectionCellComplaints complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(oc => oc.id == id).Select(a =>
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
                                                   actionTakenSupportingDocPath = a.actiontakensupportingdoc,
                                                   createdBy = a.createdby,
                                                   createdOn = a.createdon,
                                                   updatedBy = a.updatedby,
                                                   updatedOn = a.updatedon,
                                                   jntuh_college = a.jntuh_college,
                                                   //my_aspnet_users = a.my_aspnet_users,
                                                   //my_aspnet_users1 = a.my_aspnet_users1
                                               }).FirstOrDefault();
                    return PartialView("_WomenProtectionCellComplaintsData", complaints);
                }
                else
                {
                    WomenProtectionCellComplaints complaints = new WomenProtectionCellComplaints();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        complaints.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_WomenProtectionCellComplaintsData", complaints);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    WomenProtectionCellComplaints uComplaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(oc => oc.id == id).Select(a =>
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
                                                   actionTakenSupportingDocPath = a.actiontakensupportingdoc,
                                                   createdBy = a.createdby,
                                                   createdOn = a.createdon,
                                                   updatedBy = a.updatedby,
                                                   updatedOn = a.updatedon,
                                                   jntuh_college = a.jntuh_college,
                                                   //my_aspnet_users = a.my_aspnet_users,
                                                   //my_aspnet_users1 = a.my_aspnet_users1
                                               }).FirstOrDefault();
                    return View("WomenProtectionCellComplaintsData", uComplaints);
                }
                else
                {
                    WomenProtectionCellComplaints uComplaints = new WomenProtectionCellComplaints();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        uComplaints.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("WomenProtectionCellComplaintsData", uComplaints);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(WomenProtectionCellComplaints Complaints, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (userCollegeID == 0)
                if (userCollegeID == 0)
                {
                    userCollegeID = Complaints.collegeId;
                }
            if (cmd == "Save")
            {
                try
                {
                    jntuh_college_womenprotection_antiragging_complaints jntuh_college_women_protection_cell_complaints = new jntuh_college_womenprotection_antiragging_complaints();
                    jntuh_college_women_protection_cell_complaints.id = Complaints.id;
                    jntuh_college_women_protection_cell_complaints.collegeId = userCollegeID;
                    jntuh_college_women_protection_cell_complaints.academicyear = prAy;
                    jntuh_college_women_protection_cell_complaints.complainttype = 1;
                    jntuh_college_women_protection_cell_complaints.complaintgivenbyregnum = Complaints.complaintGivenByRegNum;
                    jntuh_college_women_protection_cell_complaints.complaintgivenbyname = Complaints.complaintGivenByName;
                    jntuh_college_women_protection_cell_complaints.complaintonregnum = Complaints.complaintOnRegNum;
                    jntuh_college_women_protection_cell_complaints.complaintonname = Complaints.complaintOnName;
                    jntuh_college_women_protection_cell_complaints.complaintdescription = Complaints.complaintDescription;

                    if (Complaints.complaintSupportingDoc != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/CollegeEnclosures/WomenProtection";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(Complaints.complaintSupportingDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (Complaints.complaintSupportingDocPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                Complaints.complaintSupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                Complaints.complaintSupportingDocPath = string.Format("{0}{1}", fileName, ext);
                                jntuh_college_women_protection_cell_complaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                            }
                            else
                            {
                                Complaints.complaintSupportingDoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), Complaints.complaintSupportingDocPath));
                                jntuh_college_women_protection_cell_complaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                            }
                        }
                    }
                    else
                    {
                        jntuh_college_women_protection_cell_complaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                    }
                    jntuh_college_women_protection_cell_complaints.actiontaken = Complaints.actionsTaken;

                    if (Complaints.actionTakenSupportingDoc != null)
                    {
                        string SupportingDocumentfile = "~/Content/Upload/CollegeEnclosures/WomenProtection";
                        if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                        }
                        var ext = Path.GetExtension(Complaints.actionTakenSupportingDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (Complaints.actionTakenSupportingDocPath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                Complaints.actionTakenSupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                Complaints.actionTakenSupportingDocPath = string.Format("{0}{1}", fileName, ext);
                                jntuh_college_women_protection_cell_complaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                            }
                            else
                            {
                                Complaints.actionTakenSupportingDoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), Complaints.actionTakenSupportingDocPath));
                                jntuh_college_women_protection_cell_complaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                            }
                        }
                    }
                    else
                    {
                        jntuh_college_women_protection_cell_complaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                    }

                    jntuh_college_women_protection_cell_complaints.createdby = userID;
                    jntuh_college_women_protection_cell_complaints.createdon = DateTime.Now;

                    db.jntuh_college_womenprotection_antiragging_complaints.Add(jntuh_college_women_protection_cell_complaints);
                    db.SaveChanges();
                    TempData["ComplaintsSuccess"] = "Women Protection Cell Complaints is Added successfully.";
                    return RedirectToAction("Index", "WomenProtectionCell", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                catch { }
            }
            else
            {
                try
                {
                    jntuh_college_womenprotection_antiragging_complaints uComplaints = new jntuh_college_womenprotection_antiragging_complaints();

                    if (uComplaints != null)
                    {
                        uComplaints.id = Complaints.id;
                        uComplaints.collegeId = userCollegeID;
                        uComplaints.academicyear = prAy;
                        uComplaints.complainttype = 1;
                        uComplaints.complaintgivenbyregnum = Complaints.complaintGivenByRegNum;
                        uComplaints.complaintgivenbyname = Complaints.complaintGivenByName;
                        uComplaints.complaintonregnum = Complaints.complaintOnRegNum;
                        uComplaints.complaintonname = Complaints.complaintOnName;
                        uComplaints.complaintdescription = Complaints.complaintDescription;

                        if (Complaints.complaintSupportingDoc != null)
                        {
                            string SupportingDocumentfile = "~/Content/Upload/CollegeEnclosures/WomenProtection";
                            if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                            }
                            var ext = Path.GetExtension(Complaints.complaintSupportingDoc.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                if (Complaints.complaintSupportingDocPath == null)
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                    Complaints.complaintSupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                    Complaints.complaintSupportingDocPath = string.Format("{0}{1}", fileName, ext);
                                    uComplaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                                }
                                else
                                {
                                    Complaints.complaintSupportingDoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), Complaints.complaintSupportingDocPath));
                                    uComplaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                                }
                            }
                        }
                        else
                        {
                            uComplaints.complaintsupportingdoc = Complaints.complaintSupportingDocPath;
                        }
                        uComplaints.actiontaken = Complaints.actionsTaken;

                        if (Complaints.actionTakenSupportingDoc != null)
                        {
                            string SupportingDocumentfile = "~/Content/Upload/CollegeEnclosures/WomenProtection";
                            if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                            }
                            var ext = Path.GetExtension(Complaints.actionTakenSupportingDoc.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                if (Complaints.actionTakenSupportingDocPath == null)
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                    Complaints.actionTakenSupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                                    Complaints.actionTakenSupportingDocPath = string.Format("{0}{1}", fileName, ext);
                                    uComplaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                                }
                                else
                                {
                                    Complaints.actionTakenSupportingDoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), Complaints.actionTakenSupportingDocPath));
                                    uComplaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                                }
                            }
                        }
                        else
                        {
                            uComplaints.actiontakensupportingdoc = Complaints.actionTakenSupportingDocPath;
                        }
                        uComplaints.createdby = (int)Complaints.createdBy;
                        uComplaints.createdon = Convert.ToDateTime(Complaints.createdOn);
                        uComplaints.updatedby = userID;
                        uComplaints.updatedon = DateTime.Now;
                        db.Entry(uComplaints).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["ComplaintsSuccess"] = "Women Protection Cell Complaints is Updated successfully.";
                    }
                    return RedirectToAction("Index", "WomenProtectionCell", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                }
                catch { }
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("_WomenProtectionCellComplaintsData", Complaints);
            }
            else
            {
                return View("WomenProtectionCellComplaintsData", Complaints);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_womenprotection_antiragging_complaints complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(oc => oc.id == id).FirstOrDefault();
            if (complaints != null)
            {
                try
                {
                    db.jntuh_college_womenprotection_antiragging_complaints.Remove(complaints);
                    db.SaveChanges();
                    TempData["ComplaintsSuccess"] = "Women Protection Cell Complaints is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", "WomenProtectionCell", new { collegeId = Utilities.EncryptString(complaints.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            WomenProtectionCellComplaints complaints = db.jntuh_college_womenprotection_antiragging_complaints.Where(oc => oc.id == id).Select(a =>
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
                                                  //my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
            if (complaints != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_WomenProtectionCellComplaintDetails", complaints);
                }
                else
                {
                    return View("WomenProtectionCellComplaintDetails", complaints);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(complaints.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }
    }
}
