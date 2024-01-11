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
    public class CollegeEnclosuresController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
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
            ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
              new
              {
                  id = d.id,
                  documentName = d.documentName
              }).ToList();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            var Encls = db.jntuh_enclosures.Where(a => a.isActive == true).ToList();
            var colEnclsLst = new List<CollegeEnclosures>();
            IEnumerable<CollegeEnclosures> collegeEnclosures = db.jntuh_college_enclosures.Where(a => a.collegeID == userCollegeID && a.isActive == false && a.academicyearId == ay0)
                                                                .Select(a => new CollegeEnclosures
                                                                {
                                                                    id = a.id,
                                                                    collegeId = userCollegeID,
                                                                    AcademicYearid = a.academicyearId,
                                                                    enclosureId = a.enclosureId,
                                                                    documentName = db.jntuh_enclosures.Where(d => d.id == a.enclosureId && d.isActive == true).Select(d => d.documentName).FirstOrDefault(),
                                                                    path = a.path
                                                                }).OrderBy(d => d.enclosureId).ToList();
            foreach (var item in Encls)
            {
                var sss = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.id).FirstOrDefault();
                var objEncls = new CollegeEnclosures()
                {
                    id = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.id).FirstOrDefault(),
                    collegeId = userCollegeID,
                    AcademicYearid = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.AcademicYearid).FirstOrDefault(),
                    enclosureId = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.enclosureId).FirstOrDefault() > 0 ? collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.id).FirstOrDefault() : item.id,
                    documentName = !string.IsNullOrEmpty(collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.documentName).FirstOrDefault()) ? collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.documentName).FirstOrDefault() : item.documentName,
                    path = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.path).FirstOrDefault(),
                    msgstr = collegeEnclosures.Where(i => i.enclosureId == item.id).Select(i => i.id).FirstOrDefault() > 0 ? "Edit" : "Add$" + item.id.ToString()
                };
                colEnclsLst.Add(objEncls);
            }
            string strPartA = collegeEnclosures.Where(s => s.enclosureId == 20).Select(s => s.path).FirstOrDefault();
            string strPartB = collegeEnclosures.Where(s => s.enclosureId == 21).Select(s => s.path).FirstOrDefault();
            string strPartAffidavit = collegeEnclosures.Where(e => e.enclosureId == 18).Select(e => e.path).FirstOrDefault();
            string strAllEnclousres = collegeEnclosures.Where(e => e.enclosureId == 22).Select(e => e.path).FirstOrDefault();
            string strAffidavit = collegeEnclosures.Where(e => e.enclosureId == 24).Select(e => e.path).FirstOrDefault();
            if (String.IsNullOrEmpty(strPartA))
            {
                TempData["PartA"] = "Upload AICTE/PCI Application Report (2023-24)";
            }
            if (String.IsNullOrEmpty(strPartB))
            {
                TempData["PartB"] = "Upload AICTE/PCI Deficiency Report (2023-24)";
            }
            if (String.IsNullOrEmpty(strPartAffidavit))
            {
                TempData["Affidavit"] = "Upload  Affidavit File";
            }
            if (String.IsNullOrEmpty(strAllEnclousres))
            {
                TempData["AllEnclousres"] = " Upload AICTE/PCI Part A & Part B Enclousres";
            }
            if (String.IsNullOrEmpty(strAffidavit))
            {
                TempData["Declaration"] = "Upload DECLARATION & DATA SUBMISSION";
            }
            ViewBag.CollegeEnclosures = colEnclsLst; //collegeEnclosures;
            ViewBag.Count = collegeEnclosures.Count();
            DateTime todayDate = DateTime.Now.Date;
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (collegeEnclosures.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeEnclosures.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeEnclosures");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("SC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            if (!isPageEditable)
            {
                return RedirectToAction("View", "CollegeEnclosures");
            }
            int[] mustuploads = new int[] { 18, 20, 21, 22 };
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId, string msgstr)
        {
            //ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e=>e.id).Select(d => d).ToList();
            ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
                new
                {
                    id = d.id,
                    documentName = d.documentName
                }).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var updateStatus = msgstr.Split('$');
            if (Request.IsAjaxRequest())
            {
                if (id != null && id > 0 && updateStatus[0] != "Add")
                {
                    ViewBag.IsUpdate = true;
                    CollegeEnclosures collegeEnclosures = db.jntuh_college_enclosures.Where(cd => cd.id == id && cd.academicyearId == ay0).Select(cd =>
                                              new CollegeEnclosures
                                              {
                                                  id = cd.id,
                                                  collegeId = cd.collegeID,
                                                  AcademicYearid = cd.academicyearId,
                                                  enclosureId = cd.enclosureId,
                                                  path = cd.path,
                                                  isActive = (bool)cd.isActive,
                                                  createdBy = cd.createdBy,
                                                  createdOn = cd.createdOn,
                                                  updatedBy = cd.updatedBy,
                                                  updatedOn = cd.updatedOn,
                                                  msgstr = msgstr,
                                                  enclosureName = db.jntuh_enclosures.Where(i => i.id == cd.enclosureId).Select(i => i.documentName).FirstOrDefault()
                                              }).FirstOrDefault();
                    return PartialView("_Create", collegeEnclosures);
                }
                else
                {
                    CollegeEnclosures CollegeEnclosures = new CollegeEnclosures();
                    CollegeEnclosures.msgstr = updateStatus[0];
                    if (updateStatus[0] == "Add" && !string.IsNullOrEmpty(updateStatus[1]))
                    {
                        CollegeEnclosures.enclosureId = Convert.ToInt32(updateStatus[1]);
                        CollegeEnclosures.enclosureName = db.jntuh_enclosures.Where(i => i.id == CollegeEnclosures.enclosureId).Select(i => i.documentName).FirstOrDefault();
                    }
                    if (collegeId != null)
                    {
                        //int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        CollegeEnclosures.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_Create", CollegeEnclosures);
                }
            }
            else
            {
                //int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(CollegeEnclosures collegeEnclosures, string cmd)
        {
            ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
                new
                {
                    id = d.id,
                    documentName = d.documentName
                }).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0)
            {
                userCollegeID = collegeEnclosures.collegeId;
            }
            if (ModelState.IsValid)
            {
                if (cmd == "Save" || cmd == "Add")
                {
                    try
                    {
                        jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                        jntuh_college_enclosures.id = collegeEnclosures.id;
                        jntuh_college_enclosures.collegeID = userCollegeID;
                        jntuh_college_enclosures.academicyearId = ay0;
                        jntuh_college_enclosures.enclosureId = collegeEnclosures.enclosureId;
                        jntuh_college_enclosures.isActive = collegeEnclosures.isActive;
                        if (collegeEnclosures.scannedDocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegeEnclosures")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegeEnclosures"));
                            }
                            var ext = Path.GetExtension(collegeEnclosures.scannedDocument.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".TXT") || ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX") || ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX") || ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                            {
                                string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_" +
                                                   collegeEnclosures.enclosureId + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                collegeEnclosures.scannedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeEnclosures"), fileName, ext));
                                jntuh_college_enclosures.path = string.Format("{0}/{1}{2}", "~/Content/Upload/CollegeEnclosures", fileName, ext);
                            }
                        }
                        else if (collegeEnclosures.path != null)
                        {
                            jntuh_college_enclosures.path = collegeEnclosures.path;
                        }

                        int existingId = db.jntuh_college_enclosures.Where(d => d.enclosureId == collegeEnclosures.enclosureId && d.collegeID == userCollegeID && d.academicyearId == ay0).Select(d => d.id).FirstOrDefault();
                        if (existingId > 0)
                        {
                            jntuh_college_enclosures.id = existingId;
                            jntuh_college_enclosures.isActive = collegeEnclosures.isActive;
                            jntuh_college_enclosures.createdBy = db.jntuh_college_enclosures.Where(d => d.id == existingId).Select(d => d.createdBy).FirstOrDefault();
                            jntuh_college_enclosures.createdOn = db.jntuh_college_enclosures.Where(d => d.id == existingId).Select(d => d.createdOn).FirstOrDefault();
                            jntuh_college_enclosures.updatedBy = userID;
                            jntuh_college_enclosures.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_enclosures).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Updated Successfully.";
                        }
                        else
                        {
                            jntuh_college_enclosures.createdBy = userID;
                            jntuh_college_enclosures.createdOn = DateTime.Now;

                            db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Added Successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                        jntuh_college_enclosures.id = collegeEnclosures.id;
                        jntuh_college_enclosures.academicyearId = ay0;
                        jntuh_college_enclosures.collegeID = userCollegeID;
                        jntuh_college_enclosures.enclosureId = collegeEnclosures.enclosureId;
                        jntuh_college_enclosures.isActive = collegeEnclosures.isActive;
                        if (collegeEnclosures.scannedDocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegeEnclosures")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegeEnclosures"));
                            }

                            var ext = Path.GetExtension(collegeEnclosures.scannedDocument.FileName);

                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".TXT") || ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX") || ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX") || ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                            {
                                string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_" +
                                                  collegeEnclosures.enclosureId + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                collegeEnclosures.scannedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegeEnclosures"), fileName, ext));
                                jntuh_college_enclosures.path = string.Format("{0}/{1}{2}", "~/Content/Upload/CollegeEnclosures", fileName, ext);
                            }
                        }
                        else if (collegeEnclosures.path != null)
                        {
                            jntuh_college_enclosures.path = collegeEnclosures.path;
                        }
                        jntuh_college_enclosures.createdBy = collegeEnclosures.createdBy;
                        jntuh_college_enclosures.createdOn = collegeEnclosures.createdOn;
                        jntuh_college_enclosures.updatedBy = userID;
                        jntuh_college_enclosures.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_enclosures).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["CollegesSuccess"] = "Updated Successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }
            if (Request.IsAjaxRequest())
            {
                ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
                new
                {
                    id = d.id,
                    documentName = d.documentName
                }).ToList();
                return PartialView("_Create", collegeEnclosures);
            }
            else
            {
                ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
                new
                {
                    id = d.id,
                    documentName = d.documentName
                }).ToList();
                return View("_Create", collegeEnclosures);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            ViewBag.Documents = db.jntuh_enclosures.Where(d => d.isActive == true).OrderByDescending(e => e.id).Select(d =>
                new
                {
                    id = d.id,
                    documentName = d.documentName
                }).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_enclosures.Where(d => d.id == id).Select(d => d.collegeID).FirstOrDefault();
            }
            jntuh_college_enclosures document = db.jntuh_college_enclosures.Where(d => d.id == id && d.collegeID == userCollegeID && d.academicyearId == ay0).FirstOrDefault();
            if (document != null)
            {
                try
                {
                    db.jntuh_college_enclosures.Remove(document);
                    db.SaveChanges();
                    TempData["CollegesSuccess"] = "Deleted Successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("SC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }
            IEnumerable<CollegeEnclosures> CollegeEnclosures = db.jntuh_college_enclosures.Where(a => a.collegeID == userCollegeID && a.isActive == false && a.academicyearId == ay0)
                .Select(a => new CollegeEnclosures
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    AcademicYearid = a.academicyearId,
                    enclosureId = a.enclosureId,
                    documentName = db.jntuh_enclosures.Where(d => d.id == a.enclosureId && d.isActive == true).Select(d => d.documentName).FirstOrDefault(),
                    path = a.path
                }).OrderBy(d => d.enclosureId).ToList();
            ViewBag.CollegeEnclosures = CollegeEnclosures;
            ViewBag.Count = CollegeEnclosures.Count();
            return View("View", CollegeEnclosures);
        }

        /// <summary>
        /// Previous Academic Year College Enclosures
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,College")]
        public ActionResult PreviousView()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int ay1 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            IEnumerable<CollegeEnclosures> CollegeEnclosures = db.jntuh_college_enclosures.Where(a => a.collegeID == userCollegeID && a.isActive == false && a.academicyearId == ay1)
                .Select(a => new CollegeEnclosures
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    AcademicYearid = a.academicyearId,
                    enclosureId = a.enclosureId,
                    documentName = db.jntuh_enclosures.Where(d => d.id == a.enclosureId && d.isActive == true).Select(d => d.documentName).FirstOrDefault(),
                    path = a.path
                }).OrderBy(d => d.enclosureId).ToList();

            ViewBag.CollegeEnclosures = CollegeEnclosures;
            ViewBag.Count = CollegeEnclosures.Count();
            return View("View", CollegeEnclosures);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            IEnumerable<CollegeEnclosures> CollegeEnclosures = db.jntuh_college_enclosures.Where(a => a.collegeID == userCollegeID && a.isActive == false)
                .Select(a => new CollegeEnclosures
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    enclosureId = a.enclosureId,
                    documentName = db.jntuh_enclosures.Where(d => d.id == a.enclosureId && d.isActive == true).Select(d => d.documentName).FirstOrDefault(),
                    path = a.path
                }).OrderBy(d => d.enclosureId).ToList();

            ViewBag.CollegeEnclosures = CollegeEnclosures;
            ViewBag.Count = CollegeEnclosures.Count();
            return View("UserView", CollegeEnclosures);
        }
    }
}
