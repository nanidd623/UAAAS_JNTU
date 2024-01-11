using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DocumentFormat.OpenXml;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeAICTEComplianceController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        // GET: /CollegeAICTECompliance/

        [Authorize(Roles = "College")]
        public ActionResult Index(string collegeId, string msg)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.PCIStat = false;
            var todayDate = DateTime.Now;
            var jntuhEnclosures = db.jntuh_enclosures.AsNoTracking().ToList();
            var jntuhcollegeEnclosures = db.jntuh_college_enclosures.AsNoTracking().Where(i => i.collegeID == userCollegeID).ToList();
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var actualYear = jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var prAy = jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var ayy1 = jntuhAcademicYears.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            var pcienclosureId = jntuhEnclosures.Where(e => e.documentName == "PCI Approval Letter").Select(e => e.id).FirstOrDefault();
            var enclosureId = jntuhEnclosures.Where(e => e.documentName == "AICTE Compliance EOA Letter").Select(e => e.id).FirstOrDefault();
            var collegeEnc = jntuhcollegeEnclosures.Where(e => e.enclosureId == enclosureId).ToList();
            var pcicollegeEnc = jntuhcollegeEnclosures.Where(e => e.enclosureId == pcienclosureId).ToList();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Upload AICTE/PCI EOA Details for A.Y. 2020-21" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var eoaphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == prAy && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (eoaphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var collegeIds = (from e in db.jntuh_college_intake_existing
                              join es in db.jntuh_college_edit_status on e.collegeId equals es.collegeId
                              where (es.academicyearId == prAy && e.academicYearId == prAy && es.IsCollegeEditable == false && e.specializationId == 12) //12 -> b.pharmacy spec id  
                              select es.collegeId).ToArray();
            var collegeaicte = string.Empty;
            var jntuhCollege = db.jntuh_college.AsNoTracking().FirstOrDefault(i => i.id == userCollegeID);
            if (jntuhCollege != null)
            {
                collegeaicte = jntuhCollege.aicteorpci;
                if (jntuhCollege.aicteorpci == "PCI")
                {
                    ViewBag.PCIStat = true;
                }
            }
            if (collegeIds.Contains(userCollegeID) && string.IsNullOrEmpty(collegeaicte))
            {
                var acitepci = new Aictepci
                {
                    CollegeId = userCollegeID,
                    AictepciStatus = string.IsNullOrEmpty(collegeaicte) ? 1 : 2
                };
                return View("AICTEPCI", acitepci);
            }
            ViewBag.lastcollegeEnclosures = collegeEnc.Where(e => e.academicyearId == ayy1).Select(e => e).FirstOrDefault();
            ViewBag.presentcollegeEnclosures = collegeEnc.Where(e => e.academicyearId == prAy).Select(e => e).FirstOrDefault();
            ViewBag.pcicollegeEnclosures = pcicollegeEnc.Where(e => e.academicyearId == prAy).Select(e => e).FirstOrDefault();
            ViewBag.AcademicYear = jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.academicYear).FirstOrDefault();
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            var academicYears = new List<SelectListItem>
            {
                new SelectListItem {Value = "11", Text = "2019-20"},
                new SelectListItem {Value = "12", Text = "2020-21"}
            };
            var degrees = db.jntuh_degree.Where(d => d.isActive);
            var depts = db.jntuh_department.Where(d => d.isActive);
            var specs = db.jntuh_specialization.Where(s => s.isActive);
            ViewBag.academicYears = academicYears;
            ViewBag.Degree = degrees.OrderBy(d => d.degree);
            ViewBag.Department = depts;
            ViewBag.Specialization = specs;
            var collegeaictelst = db.jntuh_college_aicteapprovedintake.AsNoTracking().Where(i => i.collegeId == userCollegeID).OrderBy(i => i.academicyearId).ToList();
            if (collegeaictelst.Count > 0)
            {
                var status = collegeaictelst.Count(i => i.declarationstatus);
                if (status > 0)
                {
                    //return RedirectToAction("Index", "UnderConstruction");
                    return RedirectToAction("IntakeAdjustments", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                }
            }
            //if (!string.IsNullOrEmpty(msg))
            //{
            //    ViewBag.AICTE2019Records = msg;
            //}
            var aicteclglst = new List<CollegeAicteComp>();
            foreach (var clgaicte in collegeaictelst)
            {
                var spec = specs.FirstOrDefault(i => i.id == clgaicte.specializationId);
                var jntuhDepartment = depts.FirstOrDefault(i => i.id == spec.departmentId);
                if (jntuhDepartment == null) continue;
                var firstOrDefault = degrees.FirstOrDefault(i => i.id == jntuhDepartment.degreeId);
                if (firstOrDefault == null) continue;
                if (spec == null) continue;
                var aicteclg = new CollegeAicteComp()
                {
                    id = clgaicte.Id,
                    collegeId = clgaicte.collegeId,
                    academicYear = clgaicte.academicyearId == 11 ? "2019-20" : "2020-21",
                    specialization = spec.specializationName,
                    Department = jntuhDepartment.departmentName,
                    degree = firstOrDefault.degree,
                    aicteIntake = clgaicte.aicteintake,
                };
                aicteclglst.Add(aicteclg);
            }
            ViewBag.collegeAictelst = aicteclglst;
            var jntuhCollegeAicteapprovedintake = collegeaictelst.FirstOrDefault(i => i.collegeId == userCollegeID);
            if (jntuhCollegeAicteapprovedintake != null)
            {
                var aictecompliancecollege = new CollegeAICTECompliance
                {
                    collegeId = userCollegeID,
                    aictecollegeId = jntuhCollegeAicteapprovedintake.aictecollegeid
                };
                return View(aictecompliancecollege);
            }
            else
            {
                var aictecompliancecollege = new CollegeAICTECompliance
                {
                    collegeId = userCollegeID,
                };
                return View(aictecompliancecollege);
            }
        }

        [HttpPost]
        [Authorize(Roles = "College")]
        public ActionResult AictePci(string collegeId, string aicteorpciStatus)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID > 0 && !string.IsNullOrEmpty(aicteorpciStatus))
            {
                var collegeinfo = db.jntuh_college.Find(userCollegeID);
                if (collegeinfo != null)
                {
                    try
                    {
                        collegeinfo.aicteorpci = aicteorpciStatus == "Yes" ? "PCI" : "AICTE";
                        db.Entry(collegeinfo).State = EntityState.Modified;
                        db.SaveChanges();
                        if (true)
                        {
                            TempData["Success"] = "AICTE/PCI status updated successfully.";
                        }
                        else
                        {
                            TempData["Error"] = "Error while updating please try again.";
                        }
                    }
                    catch (Exception)
                    {
                        TempData["Error"] = "Error while updating please try again.";
                        throw;
                    }
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        [HttpPost]
        //[AllowAnonymous]
        [Authorize(Roles = "College")]
        public ActionResult AddEditRecord(CollegeAICTECompliance collegeAicteCompliance, string cmd)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (collegeAicteCompliance != null)
            {
                try
                {
                    var aictecompliance = new jntuh_college_aicteapprovedintake()
                    {
                        collegeId = collegeAicteCompliance.collegeId,
                        academicyearId = collegeAicteCompliance.academicYearId,
                        aictecollegeid = collegeAicteCompliance.aictecollegeId,
                        aicteintake =
                            !string.IsNullOrEmpty(collegeAicteCompliance.aicteIntake)
                                ? Convert.ToInt32(collegeAicteCompliance.aicteIntake)
                                : 0,
                        specializationId = collegeAicteCompliance.specializationId,
                        createdon = DateTime.Now,
                        isactive = 1,
                        createdby = userID
                    };
                    db.jntuh_college_aicteapprovedintake.Add(aictecompliance);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "Added successfully.";
                        return RedirectToAction("Index",
                            new
                            {
                                collegeId =
                                    Utilities.EncryptString(userCollegeID.ToString(),
                                        WebConfigurationManager.AppSettings["CryptoKey"])
                            });
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "Error while adding..";
                }
            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
        }

        [Authorize(Roles = "College")]
        public ActionResult DeletecollegeAicte(int id)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (id > 0)
            {
                try
                {
                    var clgaicte = db.jntuh_college_aicteapprovedintake.Find(id);
                    db.jntuh_college_aicteapprovedintake.Remove(clgaicte);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "Deleted successfully.";
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "Error while deleting..";
                }

            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
        }

        [Authorize(Roles = "College")]
        public ActionResult FileUpload(HttpPostedFileBase fileUploader, HttpPostedFileBase fileUploader1, HttpPostedFileBase fileUploader2, string collegeId)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var previousAcyear =
                jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            var actualYear = jntuhAcademicYears.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                .Select(a => a.actualYear)
                .FirstOrDefault();
            var ayy1 = jntuhAcademicYears.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            var ay0 = jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuhEnclosures = db.jntuh_enclosures.AsNoTracking().ToList();
            var jntuhcollegeEnclosures = db.jntuh_college_enclosures.AsNoTracking().Where(i => i.collegeID == userCollegeID).ToList();
            var pcienclosureId = jntuhEnclosures.Where(e => e.documentName == "PCI Approval Letter").Select(e => e.id).FirstOrDefault();
            var enclosureId = jntuhEnclosures.Where(e => e.documentName == "AICTE Compliance EOA Letter").Select(e => e.id).FirstOrDefault();
            var collegeEnc = jntuhcollegeEnclosures.Where(e => e.enclosureId == enclosureId).ToList();
            var pcicollegeEnc = jntuhcollegeEnclosures.Where(e => e.enclosureId == pcienclosureId).ToList();
            var presentpcicollegeEnclosures = pcicollegeEnc.Where(e => e.academicyearId == ay0).Select(e => e).FirstOrDefault();
            var lastcollegeEnclosures = collegeEnc.Where(e => e.academicyearId == ayy1).Select(e => e).FirstOrDefault();
            var presentcollegeEnclosures = collegeEnc.Where(e => e.academicyearId == ay0).Select(e => e).FirstOrDefault();
            if (!Directory.Exists(Server.MapPath("~/Content/Upload/AICTEComplainaceEOA")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/AICTEComplainaceEOA"));
            }
            if (!Directory.Exists(Server.MapPath("~/Content/Upload/PCIComplainaceEOA")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/Upload/PCIComplainaceEOA"));
            }
            if (fileUploader != null) // for last academic year
            {
                jntuh_college_enclosures jntuhCollegeEnclosures = new jntuh_college_enclosures();
                jntuhCollegeEnclosures.collegeID = userCollegeID;
                jntuhCollegeEnclosures.academicyearId = ayy1;
                jntuhCollegeEnclosures.enclosureId = enclosureId;
                jntuhCollegeEnclosures.isActive = true;
                var ext = Path.GetExtension(fileUploader.FileName);
                var fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                                  "_EOA_" + ayy1 + "_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/AICTEComplainaceEOA/"),
                    fileName));
                jntuhCollegeEnclosures.path = fileName;
                //if (lastcollegeEnclosures != null && !string.IsNullOrEmpty(lastcollegeEnclosures.path))
                //{
                //    fileName = lastcollegeEnclosures.path;
                //    jntuhCollegeEnclosures.path = fileName;
                //}
                if (lastcollegeEnclosures == null)
                {
                    jntuhCollegeEnclosures.createdBy = userID;
                    jntuhCollegeEnclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuhCollegeEnclosures);
                    db.SaveChanges();
                }
                else
                {
                    lastcollegeEnclosures.path = fileName;
                    lastcollegeEnclosures.updatedBy = userID;
                    lastcollegeEnclosures.updatedOn = DateTime.Now;
                    db.Entry(lastcollegeEnclosures).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (fileUploader1 != null) // for present academic year
            {
                jntuh_college_enclosures jntuhCollegeEnclosures = new jntuh_college_enclosures();
                jntuhCollegeEnclosures.collegeID = userCollegeID;
                jntuhCollegeEnclosures.academicyearId = ay0;
                jntuhCollegeEnclosures.enclosureId = enclosureId;
                jntuhCollegeEnclosures.isActive = true;
                var ext = Path.GetExtension(fileUploader1.FileName);
                var fileName1 = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                                   "_EOA_" + ay0 + "_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader1.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/AICTEComplainaceEOA/"),
                    fileName1));
                jntuhCollegeEnclosures.path = fileName1;
                //if (presentcollegeEnclosures != null && !string.IsNullOrEmpty(presentcollegeEnclosures.path))
                //{
                //    fileName1 = presentcollegeEnclosures.path;
                //    jntuhCollegeEnclosures.path = fileName1;
                //}
                if (presentcollegeEnclosures == null)
                {
                    jntuhCollegeEnclosures.createdBy = userID;
                    jntuhCollegeEnclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuhCollegeEnclosures);
                    db.SaveChanges();
                }
                else
                {
                    presentcollegeEnclosures.path = fileName1;
                    presentcollegeEnclosures.updatedBy = userID;
                    presentcollegeEnclosures.updatedOn = DateTime.Now;
                    db.Entry(presentcollegeEnclosures).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (fileUploader2 != null) // for pci approval letter
            {
                var jntuhCollegeEnclosures = new jntuh_college_enclosures();
                jntuhCollegeEnclosures.collegeID = userCollegeID;
                jntuhCollegeEnclosures.academicyearId = ay0;
                jntuhCollegeEnclosures.enclosureId = pcienclosureId;
                jntuhCollegeEnclosures.isActive = true;
                var ext = Path.GetExtension(fileUploader2.FileName);
                var fileName2 = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                                   "_PCI_EOA_" + ay0 + "_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader2.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/PCIComplainaceEOA/"),
                    fileName2));
                jntuhCollegeEnclosures.path = fileName2;
                //if (presentcollegeEnclosures != null && !string.IsNullOrEmpty(presentcollegeEnclosures.path))
                //{
                //    fileName1 = presentcollegeEnclosures.path;
                //    jntuhCollegeEnclosures.path = fileName1;
                //}
                if (presentpcicollegeEnclosures == null)
                {
                    jntuhCollegeEnclosures.createdBy = userID;
                    jntuhCollegeEnclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuhCollegeEnclosures);
                    db.SaveChanges();
                }
                else
                {
                    presentpcicollegeEnclosures.path = fileName2;
                    presentpcicollegeEnclosures.updatedBy = userID;
                    presentpcicollegeEnclosures.updatedOn = DateTime.Now;
                    db.Entry(presentpcicollegeEnclosures).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
        }

        [Authorize(Roles = "College")]
        public ActionResult SubmitAICTE(int collegeId)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var actualYear = jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            //var prAy = jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var ayy1 = jntuhAcademicYears.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0 && collegeId > 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            var clgaicte = db.jntuh_college_aicteapprovedintake.Where(i => i.collegeId == collegeId).ToList();
            if (clgaicte.Count > 0)
            {
                var previousyrsrecords = clgaicte.Count(i => i.academicyearId == ayy1);
                if (previousyrsrecords > 0)
                {
                    foreach (var aic in clgaicte)
                    {
                        var aicte = db.jntuh_college_aicteapprovedintake.Find(aic.Id);
                        aicte.declarationstatus = true;
                        aicte.updatedby = userID;
                        aicte.updatedon = DateTime.Now;
                        db.Entry(aicte).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    TempData["Error"] = "It is mandatory to upload AICTE Approved courses for A.Y. 2019-20.";
                    return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]), msg = TempData["Error"] });
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        [Authorize(Roles = "College")]
        public ActionResult IntakeAdjustments(string collegeId)
        {
            var todayDate = DateTime.Now;
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var actualYear = jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var prAy = jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Upload AICTE/PCI EOA Details for A.Y. 2020-21" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var eoaphase = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == prAy && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (eoaphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var ayy1 = jntuhAcademicYears.Where(a => a.actualYear == (actualYear)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            ViewBag.finaldeclcond = false;
            var adjustments = db.jntuh_college_intake_adjustments.AsNoTracking().Where(i => i.collegeId == userCollegeID).ToList();
            var adjsts = adjustments.Select(i => i.Id).ToArray();
            var adjustmentsdata = db.jntuh_college_intake_adjustmentsdata.AsNoTracking().Where(i => adjsts.Contains(i.adjustmentid)).ToList();
            var collegeaictelst = db.jntuh_college_aicteapprovedintake.AsNoTracking().Where(i => i.collegeId == userCollegeID).OrderBy(i => i.academicyearId).ToList();
            var collegeaicteayy1Lst = collegeaictelst.Where(i => i.academicyearId == ayy1).OrderBy(i => i.academicyearId).ToList();
            if (collegeaicteayy1Lst.Count == 0)
            {
                return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
            }
            if (collegeaictelst.Count > 0)
            {
                var status = collegeaictelst.Count(i => i.declarationstatus == false);
                if (status > 0)
                {
                    //return RedirectToAction("Index", "UnderConstruction");
                    return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                }
            }
            var finaldeclcond = collegeaictelst.Where(i => i.remarks == "Submitted").ToList();
            if (finaldeclcond.Count > 0)
            {
                ViewBag.finaldeclcond = true;
            }
            var aicteclglst = new List<CollegeAicteAdjustments>();
            var degrees = db.jntuh_degree.Where(d => d.isActive);
            var depts = db.jntuh_department.Where(d => d.isActive);
            var specs = db.jntuh_specialization.Where(s => s.isActive);
            foreach (var clgaicte in collegeaicteayy1Lst)
            {
                var spec = specs.FirstOrDefault(i => i.id == clgaicte.specializationId);
                var jntuhDepartment = depts.FirstOrDefault(i => i.id == spec.departmentId);
                if (jntuhDepartment == null) continue;
                var firstOrDefault = degrees.FirstOrDefault(i => i.id == jntuhDepartment.degreeId);
                if (firstOrDefault == null) continue;
                if (spec == null) continue;
                var clgaicte1 = clgaicte;
                var adjstatus = adjustments.Where(i => i.aicteapprovedId == clgaicte1.Id).Select(i => i.adjustmentstatus);
                var aicteclg = new CollegeAicteAdjustments()
                {
                    id = clgaicte.Id,
                    collegeId = clgaicte.collegeId,
                    academicYear = clgaicte.academicyearId == 11 ? "2019-20" : "2020-21",
                    specialization = spec.specializationName,
                    Department = jntuhDepartment.departmentName,
                    degree = firstOrDefault.degree,
                    aicteIntake = clgaicte.aicteintake,
                    adjustmentstatus = adjstatus.Any() ? (short)adjstatus.FirstOrDefault() : (short)0
                };
                aicteclglst.Add(aicteclg);
            }
            ViewBag.collegeAicteAdjlst = aicteclglst;
            var adjs = new List<CollegeAdjustmentsDt>();

            foreach (var adjdtcol in adjustmentsdata)
            {
                var addt = adjustments.FirstOrDefault(i => i.Id == adjdtcol.adjustmentid);
                var spec = specs.FirstOrDefault(i => i.id == addt.specalizationid);
                var jntuhDepartment = depts.FirstOrDefault(i => i.id == spec.departmentId);
                if (jntuhDepartment == null) continue;
                var firstOrDefault = degrees.FirstOrDefault(i => i.id == jntuhDepartment.degreeId);
                if (firstOrDefault == null) continue;
                var adjdata = adjustmentsdata.FirstOrDefault(i => i.Id == adjdtcol.Id);
                if (adjdata != null)
                {
                    var adjspec = specs.FirstOrDefault(i => i.id == adjdata.adjustmentspecializationid);
                    var adjjntuhDepartment = depts.FirstOrDefault(i => i.id == adjspec.departmentId);
                    if (adjjntuhDepartment == null) continue;
                    var adjfirstOrDefault = degrees.FirstOrDefault(i => i.id == adjjntuhDepartment.degreeId);
                    if (adjfirstOrDefault == null) continue;
                    if (spec != null)
                    {
                        if (adjspec != null)
                        {
                            var adjdt = new CollegeAdjustmentsDt()
                            {
                                AdjustmentId = adjdata.Id,
                                CollegeId = addt.collegeId,
                                AdjustedCourseStatus = addt.coursestaus == 1 ? "Decrease" : (addt.coursestaus == 2 ? "Closure" : "N/A"),
                                AdjustedIntake = addt.adjustedintake ?? 0,
                                AdjustedRemainingIntake = addt.remaningintake ?? 0,
                                AdjustedSpecId = addt.specalizationid,
                                AdjustedCourse = firstOrDefault.degree + "-" + spec.specializationName,
                                AicteApprovedId = addt.aicteapprovedId,
                                AicteIntake = addt.aicteintake,
                                AdjustmentStatus = addt.adjustmentstatus == 1 ? "Yes" : "No",
                                AdjustmentSpecId = adjdata.adjustmentspecializationid,
                                AdjustmentCourse = adjfirstOrDefault.degree + "-" + adjspec.specializationName,
                                AdjustmentCourseStatus = adjdata.adjustedcoursestatus == 1 ? "New" : "Increase",
                                AdjustmentIntake = adjdata.adjustmentintake
                            };
                            adjs.Add(adjdt);
                        }
                    }
                }
                else
                {
                    if (spec != null)
                    {
                        var adjdt = new CollegeAdjustmentsDt()
                        {
                            AdjustmentId = adjdata.Id,
                            CollegeId = addt.collegeId,
                            AdjustedCourseStatus = addt.coursestaus == 1 ? "Decrease" : (addt.coursestaus == 2 ? "Closure" : "N/A"),
                            AdjustedIntake = addt.adjustedintake ?? 0,
                            AdjustedRemainingIntake = addt.remaningintake ?? 0,
                            AdjustedSpecId = addt.specalizationid,
                            AdjustedCourse = firstOrDefault.degree + "-" + spec.specializationName,
                            AicteApprovedId = addt.aicteapprovedId,
                            AicteIntake = addt.aicteintake,
                            AdjustmentStatus = "No",
                            AdjustmentSpecId = 0,
                            AdjustmentCourse = "-",
                            AdjustmentCourseStatus = "",
                            AdjustmentIntake = 0
                        };
                        adjs.Add(adjdt);
                    }
                }
            }

            //if (adjustments.Count > 0)
            //{
            //    foreach (var addt in adjustments)
            //    {
            //        var spec = specs.FirstOrDefault(i => i.id == addt.specalizationid);
            //        var jntuhDepartment = depts.FirstOrDefault(i => i.id == spec.departmentId);
            //        if (jntuhDepartment == null) continue;
            //        var firstOrDefault = degrees.FirstOrDefault(i => i.id == jntuhDepartment.degreeId);
            //        if (firstOrDefault == null) continue;
            //        var adjdata = adjustmentsdata.FirstOrDefault(i => i.adjustmentid == addt.Id);
            //        if (adjdata != null)
            //        {
            //            var adjspec = specs.FirstOrDefault(i => i.id == adjdata.adjustmentspecializationid);
            //            var adjjntuhDepartment = depts.FirstOrDefault(i => i.id == adjspec.departmentId);
            //            if (adjjntuhDepartment == null) continue;
            //            var adjfirstOrDefault = degrees.FirstOrDefault(i => i.id == adjjntuhDepartment.degreeId);
            //            if (adjfirstOrDefault == null) continue;
            //            if (spec != null)
            //            {
            //                if (adjspec != null)
            //                {
            //                    var adjdt = new CollegeAdjustmentsDt()
            //                    {
            //                        AdjustmentId = addt.Id,
            //                        CollegeId = addt.collegeId,
            //                        AdjustedCourseStatus = addt.coursestaus == 1 ? "Decrease" : (addt.coursestaus == 2 ? "Closure" : "N/A"),
            //                        AdjustedIntake = addt.adjustedintake ?? 0,
            //                        AdjustedRemainingIntake = addt.remaningintake ?? 0,
            //                        AdjustedSpecId = addt.specalizationid,
            //                        AdjustedCourse = firstOrDefault.degree + "-" + spec.specializationName,
            //                        AicteApprovedId = addt.aicteapprovedId,
            //                        AicteIntake = addt.aicteintake,
            //                        AdjustmentStatus = addt.adjustmentstatus == 1 ? "Yes" : "No",
            //                        AdjustmentSpecId = adjdata.adjustmentspecializationid,
            //                        AdjustmentCourse = adjfirstOrDefault.degree + "-" + adjspec.specializationName,
            //                        AdjustmentCourseStatus = adjdata.adjustedcoursestatus == 1 ? "New" : "Increase",
            //                        AdjustmentIntake = adjdata.adjustmentintake
            //                    };
            //                    adjs.Add(adjdt);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (spec != null)
            //            {
            //                var adjdt = new CollegeAdjustmentsDt()
            //                {
            //                    AdjustmentId = addt.Id,
            //                    CollegeId = addt.collegeId,
            //                    AdjustedCourseStatus = addt.coursestaus == 1 ? "Decrease" : (addt.coursestaus == 2 ? "Closure" : "N/A"),
            //                    AdjustedIntake = addt.adjustedintake ?? 0,
            //                    AdjustedRemainingIntake = addt.remaningintake ?? 0,
            //                    AdjustedSpecId = addt.specalizationid,
            //                    AdjustedCourse = firstOrDefault.degree + "-" + spec.specializationName,
            //                    AicteApprovedId = addt.aicteapprovedId,
            //                    AicteIntake = addt.aicteintake,
            //                    AdjustmentStatus = "No",
            //                    AdjustmentSpecId = 0,
            //                    AdjustmentCourse = "-",
            //                    AdjustmentCourseStatus = "",
            //                    AdjustmentIntake = 0
            //                };
            //                adjs.Add(adjdt);
            //            }
            //        }
            //    }
            //}
            ViewBag.collegeAdjustmentslst = adjs;
            return View(aicteclglst);
        }

        [HttpGet]
        [Authorize(Roles = "College")]
        public JsonResult AdjustmentAicte(int? id)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var degrees = db.jntuh_degree.Where(d => d.isActive);
            var depts = db.jntuh_department.Where(d => d.isActive);
            var specs = db.jntuh_specialization.Where(s => s.isActive);
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var adjustmentintake = 0;
            var adjustmentId = 0;
            short adjustedcoursestatus = 0;
            var actualYear =
                jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            var prAy =
                jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var jntuhCollegeAicteapprovedintake =
                db.jntuh_college_aicteapprovedintake.AsNoTracking().Where(i => i.collegeId == userCollegeID).ToList();
            var jntuhCollegeAdjus = db.jntuh_college_intake_adjustments.Where(i => i.collegeId == userCollegeID).ToList();
            var jntuhCollegeAdjustments = jntuhCollegeAdjus.Where(i => i.aicteapprovedId == id).Select(i => i.Id).ToArray();
            if (jntuhCollegeAdjustments.Count() > 0)
            {
                adjustmentintake = db.jntuh_college_intake_adjustmentsdata.Where(i => jntuhCollegeAdjustments.Contains(i.adjustmentid)).Sum(i => i.adjustmentintake);
                var jntuhCollegeIntakeAdjustments = jntuhCollegeAdjus.FirstOrDefault(i => i.aicteapprovedId == id);
                if (jntuhCollegeIntakeAdjustments != null)
                {
                    adjustedcoursestatus = jntuhCollegeIntakeAdjustments.coursestaus;
                    adjustmentId = jntuhCollegeIntakeAdjustments.Id;
                }
            }
            else
            {
                adjustmentintake = 0;
            }

            var collegeAicteAdjustments = new CollegeAicteAdjustments();
            var specsarray = new List<SelectListItem>();
            var newspecs = new int[] { };
            if (id > 0)
            {
                var collegeaicte = jntuhCollegeAicteapprovedintake.FirstOrDefault(i => i.Id == id);
                var spec = specs.FirstOrDefault(i => i.id == collegeaicte.specializationId);
                var jntuhDepartment = depts.FirstOrDefault(i => i.id == spec.departmentId);
                var firstOrDefault = degrees.FirstOrDefault(i => i.id == jntuhDepartment.degreeId);
                //var newspecs = jntuhCollegeAicteapprovedintake.Where(i => i.academicyearId == prAy).Select(i => i.specializationId).ToArray();
                if (firstOrDefault != null)
                {
                    //var degreeIds = degrees.Where(i=>).Contains(firstOrDefault.id);
                    var deptIds = depts.Where(i => i.degreeId == firstOrDefault.id).Select(i => i.id).ToArray();
                    newspecs = specs.Where(i => deptIds.Contains(i.departmentId)).Select(i => i.id).ToArray();
                }

                var jntuhCollegeIntakeAdjustments = jntuhCollegeAdjus.LastOrDefault(i => i.aicteapprovedId == id);
                var remainingIntake = 0;
                if (jntuhCollegeIntakeAdjustments != null)
                {
                    remainingIntake = jntuhCollegeIntakeAdjustments.adjustedintake ?? 0;
                }
                foreach (var sp in newspecs)
                {
                    var spe = specs.FirstOrDefault(i => i.id == sp);
                    var jntuhDept = depts.FirstOrDefault(i => i.id == spe.departmentId);
                    var firstDefault = degrees.FirstOrDefault(i => i.id == jntuhDept.degreeId);
                    if (spe != null)
                        if (firstDefault != null)
                            specsarray.Add(new SelectListItem
                            {
                                Value = sp.ToString(),
                                Text = firstDefault.degree + " - " + spe.specializationName
                            });
                }
                var remainintake = remainingIntake - adjustmentintake;
                if (jntuhCollegeIntakeAdjustments != null && remainintake == 0)
                {
                    remainintake = -1;
                }
                if (collegeaicte != null)
                    if (firstOrDefault != null)
                        if (jntuhDepartment != null)
                            if (spec != null)
                                collegeAicteAdjustments = new CollegeAicteAdjustments
                                {
                                    collegeId = collegeaicte.collegeId,
                                    degree = firstOrDefault.degree,
                                    Department = jntuhDepartment.departmentName,
                                    specializationId = spec.id,
                                    specialization = spec.specializationName,
                                    academicYear = collegeaicte.academicyearId == 11 ? "2019-20" : "2020-21",
                                    aictecollegeId = collegeaicte.aictecollegeid,
                                    aicteIntake = collegeaicte.aicteintake,
                                    id = collegeaicte.Id,
                                    adjustmentId = adjustmentId,
                                    remaningIntake = remainintake < 0 ? 99999 : remainintake,
                                    adjustedcoursestatus = adjustedcoursestatus,
                                    oldcourseStatus = new[] { "Decrease", "Closure" },
                                    newcourseStatus = new[] { "New", "Increase" },
                                    newspecs = specsarray
                                };
            }

            return Json(collegeAicteAdjustments, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "College")]
        public JsonResult AdjustmentAicte(AdjustmentData data)
        {
            var userdata = Membership.GetUser(User.Identity.Name);
            object jsondata = null;
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var actualYear =
                jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            var prAy =
                jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (userdata == null)
            {
                jsondata = "please try after login";
            }
            else
            {
                var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

                if (userCollegeID == 0)
                {
                    userCollegeID = data.CollegeId;
                }
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                if (data != null && data.CollegeaicteId > 0)
                {
                    try
                    {
                        if (data.RemainingIntake > 0 && data.AdjustmentId > 0)
                        {
                            var adjustments = db.jntuh_college_intake_adjustments.Find(data.AdjustmentId);
                            if (adjustments != null)
                            {
                                //adjustments.remaningintake = data.RemainingIntake - data.NewAdjustmentAicteIntake;
                                adjustments.updatedby = userID;
                                adjustments.updatedon = DateTime.Now;
                                db.Entry(adjustments).State = EntityState.Modified;
                                db.SaveChanges();
                                if (true)
                                {
                                    var adjustmentsId = data.AdjustmentId;
                                    var adustmentsdata = new jntuh_college_intake_adjustmentsdata()
                                    {
                                        adjustedcoursestatus = Convert.ToInt16(data.NewCourseStatus),
                                        adjustmentid = adjustmentsId,
                                        adjustmentintake = data.NewAdjustmentAicteIntake,
                                        adjustmentspecializationid = !string.IsNullOrEmpty(data.Specialization) ? Convert.ToInt32(data.Specialization) : 0,
                                        isactive = 1,
                                        createdby = userID,
                                        createdon = DateTime.Now,
                                    };
                                    db.jntuh_college_intake_adjustmentsdata.Add(adustmentsdata);
                                    db.SaveChanges();
                                    if (true)
                                    {
                                        jsondata = "Adjustment Added Successfully.";
                                    }
                                }
                            }
                            //var inkaeadjustments = new jntuh_college_intake_adjustments()
                            //{
                            //    academicyearId = prAy,
                            //    aicteapprovedId = data.CollegeaicteId,
                            //    adjustmentstatus = 1,
                            //    collegeId = data.CollegeId,
                            //    specalizationid = data.OldSpecialization,
                            //    aicteintake = data.ActualaicteIntake,
                            //    adjustedintake = data.OldAicteIntake,
                            //    coursestaus = Convert.ToInt16(data.OldCourseStatus),
                            //    remaningintake = data.RemainingIntake - data.NewAdjustmentAicteIntake,
                            //    isactive = 1,
                            //    createdby = userID,
                            //    createdon = DateTime.Now,
                            //};
                            //db.jntuh_college_intake_adjustments.Add(inkaeadjustments);
                            //db.SaveChanges();

                        }
                        else
                        {
                            var inkaeadjustments = new jntuh_college_intake_adjustments()
                            {
                                academicyearId = prAy,
                                aicteapprovedId = data.CollegeaicteId,
                                adjustmentstatus = 1,
                                collegeId = data.CollegeId,
                                specalizationid = data.OldSpecialization,
                                aicteintake = data.ActualaicteIntake,
                                adjustedintake = data.OldAicteIntake,
                                coursestaus = Convert.ToInt16(data.OldCourseStatus),
                                remaningintake = data.ActualaicteIntake == data.OldAicteIntake ? data.ActualaicteIntake : data.ActualaicteIntake - data.OldAicteIntake,
                                isactive = 1,
                                createdby = userID,
                                createdon = DateTime.Now,
                            };
                            db.jntuh_college_intake_adjustments.Add(inkaeadjustments);
                            db.SaveChanges();
                            if (true)
                            {
                                var adjustmentsId = inkaeadjustments.Id;
                                var adustmentsdata = new jntuh_college_intake_adjustmentsdata()
                                {
                                    adjustedcoursestatus = Convert.ToInt16(data.NewCourseStatus),
                                    adjustmentid = adjustmentsId,
                                    adjustmentintake = data.NewAdjustmentAicteIntake,
                                    adjustmentspecializationid = !string.IsNullOrEmpty(data.Specialization) ? Convert.ToInt32(data.Specialization) : 0,
                                    isactive = 1,
                                    createdby = userID,
                                    createdon = DateTime.Now,
                                };
                                db.jntuh_college_intake_adjustmentsdata.Add(adustmentsdata);
                                db.SaveChanges();
                                if (true)
                                {
                                    jsondata = "Adjustment Added Successfully.";
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        jsondata = "Error while updating adjustments.";
                        throw;
                    }
                }
            }
            return Json(new { jsondata }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "College")]
        public ActionResult DeleteAdjustment(int id)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var adjdatacollection = db.jntuh_college_intake_adjustmentsdata.AsNoTracking().ToList();
            if (id > 0)
            {
                try
                {
                    var jntuhCollegeIntakeAdjustmentsdata = adjdatacollection.FirstOrDefault(i => i.Id == id);
                    if (jntuhCollegeIntakeAdjustmentsdata != null)
                    {
                        var adjustmentsid = jntuhCollegeIntakeAdjustmentsdata.adjustmentid;
                        var adjustmentsdata = adjdatacollection.Where(i => i.adjustmentid == adjustmentsid).ToList();
                        if (adjustmentsdata.Count() > 1)
                        {
                            var data = db.jntuh_college_intake_adjustmentsdata.Find(id);
                            db.jntuh_college_intake_adjustmentsdata.Remove(data);
                        }
                        else
                        {
                            var collegeIntakeAdjustmentsdata = adjustmentsdata.FirstOrDefault();
                            if (collegeIntakeAdjustmentsdata != null)
                            {
                                var clgadj = db.jntuh_college_intake_adjustments.Find(collegeIntakeAdjustmentsdata.adjustmentid);
                                db.jntuh_college_intake_adjustments.Remove(clgadj);
                            }
                            var data = db.jntuh_college_intake_adjustmentsdata.Find(id);
                            db.jntuh_college_intake_adjustmentsdata.Remove(data);
                        }
                    }
                    //var data = db.jntuh_college_intake_adjustmentsdata.Find(id);
                    //db.jntuh_college_intake_adjustmentsdata.Remove(data);
                    //var clgadj = db.jntuh_college_intake_adjustments.Find(id);
                    //db.jntuh_college_intake_adjustments.Remove(clgadj);
                    //var jntuhCollegeIntakeAdjustmentsdata = db.jntuh_college_intake_adjustmentsdata.FirstOrDefault(i => i.adjustmentid == id);
                    //if (jntuhCollegeIntakeAdjustmentsdata != null)
                    //{
                    //    var cldadjdata =
                    //        jntuhCollegeIntakeAdjustmentsdata.Id;
                    //    var data = db.jntuh_college_intake_adjustmentsdata.Find(cldadjdata);
                    //    db.jntuh_college_intake_adjustmentsdata.Remove(data);
                    //}
                    db.SaveChanges();
                    if (true)
                    {
                        //var adjustmentsdata = db.jntuh_college_intake_adjustmentsdata.Where(id)
                        TempData["Success"] = "Adjustment Deleted successfully.";
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "Error while deleting..";
                }

            }
            return RedirectToAction("IntakeAdjustments",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult AdjustmentNo(int Id)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuhAcademicYears = db.jntuh_academic_year.AsNoTracking().ToList();
            var actualYear = jntuhAcademicYears.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var prAy = jntuhAcademicYears.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            try
            {
                if (Id > 0)
                {
                    var aicteapprovedt = db.jntuh_college_aicteapprovedintake.Find(Id);
                    var inkaeadjustments = new jntuh_college_intake_adjustments()
                    {
                        academicyearId = prAy,
                        aicteapprovedId = aicteapprovedt.Id,
                        adjustmentstatus = 2, // adjustment status --> NO
                        collegeId = aicteapprovedt.collegeId,
                        specalizationid = aicteapprovedt.specializationId,
                        aicteintake = aicteapprovedt.aicteintake,
                        adjustedintake = 0,
                        coursestaus = 0,
                        remaningintake = 0,
                        isactive = 1,
                        createdby = userID,
                        createdon = DateTime.Now,
                    };
                    //var clgadj = db.jntuh_college_intake_adjustments.Find(Id);
                    //clgadj.adjustmentstatus = 2;
                    //clgadj.createdby = userID;
                    //clgadj.createdon = DateTime.Now;
                    //db.Entry(clgadj).State = EntityState.Modified;
                    db.jntuh_college_intake_adjustments.Add(inkaeadjustments);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "Adjustment status updated successfully.";
                    }
                }

            }
            catch (Exception)
            {
                TempData["ERROR"] = "Error while updating";
                throw;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [Authorize(Roles = "College")]
        public ActionResult SubmitFinalDecl(string collegeId)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)//
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            var clgaicte =
                db.jntuh_college_aicteapprovedintake.Where(i => i.collegeId == userCollegeID).ToList();
            if (clgaicte.Count > 0)
            {
                foreach (var aic in clgaicte)
                {
                    var aicte = db.jntuh_college_aicteapprovedintake.Find(aic.Id);
                    aicte.remarks = "Submitted";
                    aicte.updatedby = userID;
                    aicte.updatedon = DateTime.Now;
                    db.Entry(aicte).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("IntakeAdjustments",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"])
                });
        }
    }

    public class CollegeAicteComp
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public string academicYear { get; set; }
        public string degree { get; set; }
        public string Department { get; set; }
        public string specialization { get; set; }
        public string aictecollegeId { get; set; }
        public int aicteIntake { get; set; }

    }

    public class CollegeAdjustmentsDt
    {
        public Int32 AdjustmentId { get; set; }
        public Int32 AicteApprovedId { get; set; }
        public int CollegeId { get; set; }
        public int AicteIntake { get; set; }
        public int AdjustedSpecId { get; set; }
        public string AdjustedCourse { get; set; }
        public string AdjustedCourseStatus { get; set; }
        public int AdjustedIntake { get; set; }
        public int AdjustedRemainingIntake { get; set; }
        public string AdjustmentStatus { get; set; }
        public int AdjustmentSpecId { get; set; }
        public string AdjustmentCourse { get; set; }
        public string AdjustmentCourseStatus { get; set; }
        public int AdjustmentIntake { get; set; }
    }
}
