using System;
using System.Collections.Generic;
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
    public class CollegeDocumentsController : BaseController
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

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            IEnumerable<CollegeDocuments> collegeDocuments = db.jntuh_college_document.Where(a => a.collegeId == userCollegeID)
                .Select(a => new CollegeDocuments
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    documentId = a.documentId,
                    documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                    scannedCopy = a.scannedCopy
                }).OrderBy(d => d.documentId).ToList();
            ViewBag.CollegeDocuments = collegeDocuments;
            ViewBag.Count = collegeDocuments.Count();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (collegeDocuments.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeDocuments.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeDocuments");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "CollegeDocuments");
            }
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id,string collegeId)
        {
            ViewBag.Documents = db.jntuh_documents_required.Where(d => d.isActive == true).Select(d => d).ToList();

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeDocuments collegeDocuments = db.jntuh_college_document.Where(cd => cd.id == id).Select(cd =>
                                              new CollegeDocuments
                                              {
                                                  id = cd.id,
                                                  collegeId = cd.collegeId,
                                                  documentId = cd.documentId,
                                                  scannedCopy = cd.scannedCopy,
                                                  createdBy = cd.createdBy,
                                                  createdOn = cd.createdOn,
                                                  updatedBy = cd.updatedBy,
                                                  updatedOn = cd.updatedOn
                                              }).FirstOrDefault();
                    return PartialView("_Create", collegeDocuments);
                }
                else
                {
                    CollegeDocuments collegeDocuments = new CollegeDocuments();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegeDocuments.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_Create", collegeDocuments);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeDocuments collegeDocuments = db.jntuh_college_document.Where(cd => cd.id == id).Select(cd =>
                                               new CollegeDocuments
                                               {
                                                   id = cd.id,
                                                   collegeId = cd.collegeId,
                                                   documentId = cd.documentId,
                                                   scannedCopy = cd.scannedCopy,
                                                   scannedDocument = null,
                                                   createdBy = cd.createdBy,
                                                   createdOn = cd.createdOn,
                                                   updatedBy = cd.updatedBy,
                                                   updatedOn = cd.updatedOn
                                               }).FirstOrDefault();
                    return View("Create", collegeDocuments);
                }
                else
                {
                    CollegeDocuments collegeDocuments = new CollegeDocuments();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegeDocuments.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("Create", collegeDocuments);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(CollegeDocuments collegeDocument, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeDocument.collegeId;
            }
            if (ModelState.IsValid)
            {
               
                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_document jntuh_college_document = new jntuh_college_document();
                        jntuh_college_document.id = collegeDocument.id;
                        jntuh_college_document.collegeId = userCollegeID;
                        jntuh_college_document.documentId = collegeDocument.documentId;

                        if (collegeDocument.scannedDocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegePhotos")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegePhotos"));
                            }

                            var ext = Path.GetExtension(collegeDocument.scannedDocument.FileName);

                            if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                            {
                                string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "-" +
                                                  collegeDocument.documentId;
                                collegeDocument.scannedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegePhotos"), fileName, ext));
                                jntuh_college_document.scannedCopy = string.Format("{0}/{1}{2}", "~/Content/Upload/CollegePhotos", fileName, ext);
                            }
                        }
                        else if (collegeDocument.scannedCopy != null)
                        {
                            jntuh_college_document.scannedCopy = collegeDocument.scannedCopy;
                        }

                        int existingId = db.jntuh_college_document.Where(d => d.documentId == collegeDocument.documentId && d.collegeId == userCollegeID).Select(d => d.id).FirstOrDefault();
                        if (existingId > 0)
                        {
                            jntuh_college_document.id = existingId;
                            jntuh_college_document.createdBy = db.jntuh_college_document.Where(d => d.id == existingId).Select(d => d.createdBy).FirstOrDefault();
                            jntuh_college_document.createdOn = db.jntuh_college_document.Where(d => d.id == existingId).Select(d => d.createdOn).FirstOrDefault();
                            jntuh_college_document.updatedBy = userID;
                            jntuh_college_document.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_document).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Updated Successfully.";
                        }
                        else
                        {
                            jntuh_college_document.createdBy = userID;
                            jntuh_college_document.createdOn = DateTime.Now;

                            db.jntuh_college_document.Add(jntuh_college_document);
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
                        jntuh_college_document jntuh_college_document = new jntuh_college_document();
                        jntuh_college_document.id = collegeDocument.id;
                        jntuh_college_document.collegeId = userCollegeID;
                        jntuh_college_document.documentId = collegeDocument.documentId;

                        if (collegeDocument.scannedDocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegePhotos")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegePhotos"));
                            }

                            var ext = Path.GetExtension(collegeDocument.scannedDocument.FileName);

                            if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                            {
                                string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "-" +
                                                  collegeDocument.documentId;
                                collegeDocument.scannedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegePhotos"), fileName, ext));
                                jntuh_college_document.scannedCopy = string.Format("{0}/{1}{2}", "~/Content/Upload/CollegePhotos", fileName, ext);
                            }
                        }
                        else if (collegeDocument.scannedCopy != null)
                        {
                            jntuh_college_document.scannedCopy = collegeDocument.scannedCopy;
                        }

                        jntuh_college_document.createdBy = collegeDocument.createdBy;
                        jntuh_college_document.createdOn = collegeDocument.createdOn;
                        jntuh_college_document.updatedBy = userID;
                        jntuh_college_document.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_document).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["CollegesSuccess"] = "Updated Successfully.";

                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Create", collegeDocument);
            }
            else
            {
                return View("Create", collegeDocument);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_document.Where(d => d.id == id).Select(d=>d.collegeId).FirstOrDefault();
            }
            jntuh_college_document document = db.jntuh_college_document.Where(d => d.id == id && d.collegeId == userCollegeID).FirstOrDefault();
            if (document != null)
            {
                try
                {
                    db.jntuh_college_document.Remove(document);
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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("CP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }

            IEnumerable<CollegeDocuments> collegeDocuments = db.jntuh_college_document.Where(a => a.collegeId == userCollegeID)
                .Select(a => new CollegeDocuments
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    documentId = a.documentId,
                    documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                    scannedCopy = a.scannedCopy
                }).OrderBy(d => d.documentId).ToList();

            ViewBag.CollegeDocuments = collegeDocuments;
            ViewBag.Count = collegeDocuments.Count();
            return View("View", collegeDocuments);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            IEnumerable<CollegeDocuments> collegeDocuments = db.jntuh_college_document.Where(a => a.collegeId == userCollegeID)
                .Select(a => new CollegeDocuments
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    documentId = a.documentId,
                    documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                    scannedCopy = a.scannedCopy
                }).OrderBy(d => d.documentId).ToList();

            ViewBag.CollegeDocuments = collegeDocuments;
            ViewBag.Count = collegeDocuments.Count();
            return View("UserView", collegeDocuments);
        }
    }
}
