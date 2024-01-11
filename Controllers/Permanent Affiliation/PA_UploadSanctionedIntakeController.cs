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
using UAAAS.Models.Permanent_Affiliation;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_UploadSanctionedIntakeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            if (userCollegeID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_UploadSanctionedIntake", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
            if (userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PA_UploadSanctionedIntake", new
                {
                    collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    uploadsId = ""
                });
            }
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PUSI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            var supportingdocsList = db.jntuh_college_jntu_pci_aicte_supportingdocs.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            List<SanctionedDocs> sanDocsListObj = new List<SanctionedDocs>();
            foreach (var item in supportingdocsList)
            {
                SanctionedDocs sanDocsObj = new SanctionedDocs();
                sanDocsObj.CollegeId = item.collegeid;
                sanDocsObj.UploadsId = item.id;
                sanDocsObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.acedamicyearid).Select(a => a.academicYear).FirstOrDefault();
                sanDocsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                sanDocsObj.JNTUHSanctionedDocumentPath = item.jntudoc;
                sanDocsObj.PCISanctionedDocumentPath = item.pcidoc;
                sanDocsObj.AICTESanctionedDocumentPath = item.aictedoc;

                sanDocsListObj.Add(sanDocsObj);
            }
            ViewBag.SanctionedDocsList = sanDocsListObj;
            UploadSanctionedDetails sanDocsmodel = new UploadSanctionedDetails();
            return View(sanDocsmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId, string uploadsId)
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
                return RedirectToAction("Create", "PA_UploadSanctionedIntake");
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var status = GetPageEditableStatus(userCollegeID);

            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_UploadSanctionedIntake");
            }
            else
            {
                ViewBag.IsEditable = true;

                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PUSI") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_UploadSanctionedIntake");
                }
            }
            var supportingdocsList = db.jntuh_college_jntu_pci_aicte_supportingdocs.Where(c => c.collegeid == userCollegeID && c.isactive == true).ToList();
            List<SanctionedDocs> sanDocsListObj = new List<SanctionedDocs>();
            foreach (var item in supportingdocsList)
            {
                SanctionedDocs sanDocsObj = new SanctionedDocs();
                sanDocsObj.CollegeId = item.collegeid;
                sanDocsObj.UploadsId = item.id;
                sanDocsObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.acedamicyearid).Select(a => a.academicYear).FirstOrDefault();
                sanDocsObj.Degree = db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault();
                sanDocsObj.JNTUHSanctionedDocumentPath = item.jntudoc;
                sanDocsObj.PCISanctionedDocumentPath = item.pcidoc;
                sanDocsObj.AICTESanctionedDocumentPath = item.aictedoc;

                sanDocsListObj.Add(sanDocsObj);
            }
            ViewBag.SanctionedDocsList = sanDocsListObj;

            List<SelectListItem> academicYears = new List<SelectListItem>();
            int instituteEstYear = db.jntuh_college_establishment.Where(e => e.collegeId == userCollegeID).Select(e => e.instituteEstablishedYear).FirstOrDefault();
            for (int i = instituteEstYear; i <= DateTime.Now.Year; i++)
            {
                string next_year = ((i + 1) % 100).ToString();
                if (next_year.ToString().Length == 1)
                {
                    next_year = "0" + next_year;
                }
                academicYears.Add(new SelectListItem { Text = (i + "-" + next_year).ToString(), Value = (i + "-" + next_year).ToString() });
            }
            ViewBag.AcademicYears = academicYears;

            var degreeId = db.jntuh_college_degree.Where(cd => cd.collegeId == userCollegeID && cd.isActive == true).Select(cd => cd.degreeId).ToList();

            ViewBag.Degrees = db.jntuh_degree.Where(d => degreeId.Contains(d.id)).ToList();

            UploadSanctionedDetails sanDocsmodel = new UploadSanctionedDetails();
            if (uploadsId != null)
            {
                int dec_uploadsId = Convert.ToInt32(Utilities.DecryptString(uploadsId, WebConfigurationManager.AppSettings["CryptoKey"]));
                var supportingdocs = db.jntuh_college_jntu_pci_aicte_supportingdocs.Find(dec_uploadsId);
                sanDocsmodel.CollegeId = supportingdocs.collegeid;
                sanDocsmodel.SupportingDocsId = supportingdocs.id;
                sanDocsmodel.AcademicYear = db.jntuh_academic_year.Where(a => a.id == supportingdocs.acedamicyearid).Select(a => a.academicYear).FirstOrDefault();
                sanDocsmodel.DegreeId = supportingdocs.degreeid;
                sanDocsmodel.JNTUHSanctionedDocumentPath = supportingdocs.jntudoc;
                sanDocsmodel.PCISanctionedDocumentPath = supportingdocs.pcidoc;
                sanDocsmodel.AICTESanctionedDocumentPath = supportingdocs.aictedoc;
            }
            sanDocsmodel.CollegeId = userCollegeID;
            return View("Create", sanDocsmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(UploadSanctionedDetails sanDocsmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (sanDocsmodel.SupportingDocsId > 0)
            {
                //Update
                var supportingdocs = db.jntuh_college_jntu_pci_aicte_supportingdocs.Where(a => a.id == sanDocsmodel.SupportingDocsId).Select(a => a).FirstOrDefault();
                supportingdocs.acedamicyearid = db.jntuh_academic_year.Where(a => a.academicYear == sanDocsmodel.AcademicYear).Select(a => a.id).FirstOrDefault();
                supportingdocs.collegeid = sanDocsmodel.CollegeId;
                supportingdocs.degreeid = sanDocsmodel.DegreeId;

                if (sanDocsmodel.JNTUHSanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.JNTUHSanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.JNTUHSanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.JNTUHSanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.JNTUHSanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            supportingdocs.jntudoc = sanDocsmodel.JNTUHSanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.JNTUHSanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.JNTUHSanctionedDocumentPath));
                            supportingdocs.jntudoc = sanDocsmodel.JNTUHSanctionedDocumentPath;
                        }
                    }
                }

                if (sanDocsmodel.PCISanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.PCISanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.PCISanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.PCISanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.PCISanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            supportingdocs.pcidoc = sanDocsmodel.PCISanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.PCISanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.PCISanctionedDocumentPath));
                            supportingdocs.pcidoc = sanDocsmodel.PCISanctionedDocumentPath;
                        }
                    }
                }

                if (sanDocsmodel.AICTESanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.AICTESanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.AICTESanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.AICTESanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.AICTESanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            supportingdocs.aictedoc = sanDocsmodel.AICTESanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.AICTESanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.AICTESanctionedDocumentPath));
                            supportingdocs.aictedoc = sanDocsmodel.AICTESanctionedDocumentPath;
                        }
                    }
                }

                supportingdocs.updatedon = DateTime.Now;
                supportingdocs.updatedby = userID;

                db.Entry(supportingdocs).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";
            }
            else
            {
                //Add
                jntuh_college_jntu_pci_aicte_supportingdocs sanctionedDocs = new jntuh_college_jntu_pci_aicte_supportingdocs();
                sanctionedDocs.acedamicyearid = db.jntuh_academic_year.Where(a => a.academicYear == sanDocsmodel.AcademicYear).Select(a => a.id).FirstOrDefault();
                sanctionedDocs.collegeid = sanDocsmodel.CollegeId;
                sanctionedDocs.degreeid = sanDocsmodel.DegreeId;

                if (sanDocsmodel.JNTUHSanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.JNTUHSanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.JNTUHSanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.JNTUHSanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.JNTUHSanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            sanctionedDocs.jntudoc = sanDocsmodel.JNTUHSanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.JNTUHSanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.JNTUHSanctionedDocumentPath));
                            sanctionedDocs.jntudoc = sanDocsmodel.JNTUHSanctionedDocumentPath;
                        }
                    }
                }
                else
                {
                    sanctionedDocs.jntudoc = sanDocsmodel.JNTUHSanctionedDocumentPath;
                }

                if (sanDocsmodel.PCISanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.PCISanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.PCISanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.PCISanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.PCISanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            sanctionedDocs.pcidoc = sanDocsmodel.PCISanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.PCISanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.PCISanctionedDocumentPath));
                            sanctionedDocs.pcidoc = sanDocsmodel.PCISanctionedDocumentPath;
                        }
                    }
                }
                else
                {
                    sanctionedDocs.pcidoc = sanDocsmodel.PCISanctionedDocumentPath;
                }

                if (sanDocsmodel.AICTESanctionedDocument != null)
                {
                    string SupportingDocumentfile = "~/Content/Upload/College/SanctionedDocuments";
                    if (!Directory.Exists(Server.MapPath(SupportingDocumentfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(SupportingDocumentfile));
                    }
                    var ext = Path.GetExtension(sanDocsmodel.AICTESanctionedDocument.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (sanDocsmodel.AICTESanctionedDocumentPath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            sanDocsmodel.AICTESanctionedDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(SupportingDocumentfile), fileName, ext));
                            sanDocsmodel.AICTESanctionedDocumentPath = string.Format("{0}{1}", fileName, ext);
                            sanctionedDocs.aictedoc = sanDocsmodel.AICTESanctionedDocumentPath;
                        }
                        else
                        {
                            sanDocsmodel.AICTESanctionedDocument.SaveAs(string.Format("{0}/{1}", Server.MapPath(SupportingDocumentfile), sanDocsmodel.AICTESanctionedDocumentPath));
                            sanctionedDocs.aictedoc = sanDocsmodel.AICTESanctionedDocumentPath;
                        }
                    }
                }
                else
                {
                    sanctionedDocs.aictedoc = sanDocsmodel.AICTESanctionedDocumentPath;
                }

                sanctionedDocs.isactive = true;
                sanctionedDocs.createdby = userID;
                sanctionedDocs.createdon = DateTime.Now;

                db.jntuh_college_jntu_pci_aicte_supportingdocs.Add(sanctionedDocs);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";
            }
            return RedirectToAction("Edit", "PA_UploadSanctionedIntake", new
            {
                collegeId = Utilities.EncryptString(sanDocsmodel.CollegeId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),

                uploadsId = ""
            });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string collegeId, string uploadsId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int dec_uploadsId = Convert.ToInt32(Utilities.DecryptString(uploadsId, WebConfigurationManager.AppSettings["CryptoKey"]));

            var supportingdocs = db.jntuh_college_jntu_pci_aicte_supportingdocs.Where(a => a.id == dec_uploadsId).FirstOrDefault();
            db.Entry(supportingdocs).State = EntityState.Deleted;
            db.SaveChanges();

            TempData["Success"] = "Deleted successfully";

            return RedirectToAction("Edit", "PA_UploadSanctionedIntake", new
            {
                collegeId = collegeId,
                uploadsId = ""
            });
        }
    }

    public class SanctionedDocs
    {
        public int CollegeId { get; set; }

        public int UploadsId { get; set; }

        public string AcademicYear { get; set; }

        public string Degree { get; set; }

        public string JNTUHSanctionedDocumentPath { get; set; }

        public string PCISanctionedDocumentPath { get; set; }

        public string AICTESanctionedDocumentPath { get; set; }
    }
}
