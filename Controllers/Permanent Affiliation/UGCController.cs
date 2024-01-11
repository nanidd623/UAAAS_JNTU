using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
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
    public class UGCController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private uaaasDBContext db1 = new uaaasDBContext();

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(int affiliationTypeId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            var UGClist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<UGC> ugcListObj = new List<UGC>();
            foreach (var item in UGClist)
            {
                UGC ugcObj = new UGC();
                ugcObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                ugcObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                ugcObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                ugcObj.Duration = Convert.ToInt32(item.affiliationDuration);
                ugcObj.FilePath = item.filePath;
                ugcObj.affiliationTypeId = item.affiliationTypeId;
                ugcObj.affiliationId = item.id;

                ugcListObj.Add(ugcObj);
            }
            ViewBag.UGCList = ugcListObj;
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string affiliationTypeId, string affiliationId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PUGC") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }

                //ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            int dec_affiliationTypeId = Convert.ToInt32(Utilities.DecryptString(affiliationTypeId, WebConfigurationManager.AppSettings["CryptoKey"]));

            int dec_affiliationId = 0;
            if (affiliationId != null)
            {
                dec_affiliationId = Convert.ToInt32(Utilities.DecryptString(affiliationId, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var UGClist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == dec_affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<UGC> ugcListObj = new List<UGC>();
            foreach (var item in UGClist)
            {
                UGC ugcObj = new UGC();
                ugcObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                ugcObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                //ugcObj.AffiliationFromDate = item.affiliationFromDate.ToString("dd/MM/yyyy");
                ugcObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                ugcObj.Duration = Convert.ToInt32(item.affiliationDuration);
                ugcObj.FilePath = item.filePath;
                ugcObj.affiliationTypeId = item.affiliationTypeId;
                ugcObj.affiliationId = item.id;

                ugcListObj.Add(ugcObj);
            }
            ViewBag.UGCList = ugcListObj;
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();
            UGCModel ugcmodel = new UGCModel();
            if (dec_affiliationId > 0)
            {
                var clgAffiliation = db.jntuh_college_affiliation.Where(a => a.id == dec_affiliationId).Select(a => a).FirstOrDefault();
                ugcmodel.affiliationId = clgAffiliation.id;
                ugcmodel.affiliationTypeId = clgAffiliation.affiliationTypeId;
                ugcmodel.collegeId = clgAffiliation.collegeId;
                ugcmodel.AcademicYear = Convert.ToInt32(clgAffiliation.academicYearid);
                ugcmodel.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", clgAffiliation.affiliationFromDate);
                ugcmodel.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", clgAffiliation.affiliationToDate);
                ugcmodel.affiliationfilepath = clgAffiliation.filePath;
                ugcmodel.Duration = Convert.ToString(clgAffiliation.affiliationDuration);
            }
            ugcmodel.affiliationTypeId = dec_affiliationTypeId;
            return View(ugcmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(UGCModel ugcmodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //var clgAffiliation = db.jntuh_college_affiliation.Where(a => a.id == ugcmodel.affiliationId).Select(a => a).FirstOrDefault();

            var UGClist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == ugcmodel.affiliationTypeId && a.id != ugcmodel.affiliationId && a.affiliationStatus == "Yes").ToList();

            bool newAffiliation = false;
            bool returnStatus = false;

            if (UGClist.Count == 0)
            {
                newAffiliation = true;
            }
            else
            {
                foreach (var item in UGClist)
                {
                    newAffiliation = false;

                    returnStatus = IsBetween(Convert.ToDateTime(ugcmodel.AffiliationFromDate), item.affiliationFromDate, item.affiliationToDate);

                    if (!returnStatus)
                    {
                        returnStatus = IsBetween(Convert.ToDateTime(ugcmodel.AffiliationToDate), item.affiliationFromDate, item.affiliationToDate);

                        if (!returnStatus)
                        {
                            returnStatus = IsBetween(item.affiliationFromDate, Convert.ToDateTime(ugcmodel.AffiliationFromDate), Convert.ToDateTime(ugcmodel.AffiliationToDate));

                            if (!returnStatus)
                            {
                                returnStatus = IsBetween(item.affiliationToDate, Convert.ToDateTime(ugcmodel.AffiliationFromDate), Convert.ToDateTime(ugcmodel.AffiliationToDate));

                                if (!returnStatus)
                                {
                                    newAffiliation = true;
                                }
                                else
                                {
                                    newAffiliation = false;
                                    break;
                                }
                            }
                            else
                            {
                                newAffiliation = false;
                                break;
                            }
                        }
                        else
                        {
                            newAffiliation = false;
                            break;
                        }
                    }
                    else
                    {
                        newAffiliation = false;
                        break;
                    }
                }
            }

            if (newAffiliation)
            {
                var jntuh_college_affiliation = db.jntuh_college_affiliation.Where(a => a.id == ugcmodel.affiliationId).Select(a => a).FirstOrDefault();
                //jntuh_college_affiliation.id = ugcmodel.affiliationId;
                jntuh_college_affiliation.collegeId = ugcmodel.collegeId;
                jntuh_college_affiliation.academicYearid = Convert.ToInt32(ugcmodel.AcademicYear);
                jntuh_college_affiliation.affiliationTypeId = ugcmodel.affiliationTypeId;
                jntuh_college_affiliation.affiliationFromDate = Convert.ToDateTime(ugcmodel.AffiliationFromDate);
                jntuh_college_affiliation.affiliationToDate = Convert.ToDateTime(ugcmodel.AffiliationToDate);

                if (ugcmodel.affiliationfile != null)
                {
                    string affiliationfile = "~/Content/Upload/College/UGC";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(ugcmodel.affiliationfile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (ugcmodel.affiliationfilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            ugcmodel.affiliationfile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            ugcmodel.affiliationfilepath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                        }
                        else
                        {
                            ugcmodel.affiliationfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), ugcmodel.affiliationfilepath));
                            jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                        }
                    }
                }
                //else
                //{
                //    jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                //}

                jntuh_college_affiliation.createdOn = db.jntuh_college_affiliation.Where(a => a.id == ugcmodel.affiliationId).Select(a => a.createdOn).FirstOrDefault();
                jntuh_college_affiliation.createdBy = db.jntuh_college_affiliation.Where(a => a.id == ugcmodel.affiliationId).Select(a => a.createdBy).FirstOrDefault();
                jntuh_college_affiliation.updatedOn = DateTime.Now;
                jntuh_college_affiliation.updatedBy = userID;
                jntuh_college_affiliation.affiliationDuration = Convert.ToInt32(ugcmodel.Duration);
                jntuh_college_affiliation.affiliationStatus = "Yes";

                db.Entry(jntuh_college_affiliation).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";

                return RedirectToAction("Create", "AffiliationTypes");
            }
            else
            {
                TempData["Error"] = "There is an Affiliation Status already in the given Dates.";
                return RedirectToAction("Edit", "UGC", new
                {
                    affiliationTypeId = Utilities.EncryptString(ugcmodel.affiliationTypeId.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    affiliationId = Utilities.EncryptString(ugcmodel.affiliationId.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Add(string affiliationTypeId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;

            var status = GetPageEditableStatus(userCollegeID);

            int dec_affiliationTypeId = Convert.ToInt32(Utilities.DecryptString(affiliationTypeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            var UGClist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == dec_affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<UGC> ugcListObj = new List<UGC>();
            foreach (var item in UGClist)
            {
                UGC ugcObj = new UGC();
                ugcObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                ugcObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                ugcObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                ugcObj.Duration = Convert.ToInt32(item.affiliationDuration);
                ugcObj.FilePath = item.filePath;
                ugcObj.affiliationTypeId = item.affiliationTypeId;
                ugcObj.affiliationId = item.id;

                ugcListObj.Add(ugcObj);
            }
            ViewBag.UGCList = ugcListObj;
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();
            UGCModel ugcmodel = new UGCModel();
            ugcmodel.affiliationTypeId = dec_affiliationTypeId;
            return View(ugcmodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Add(UGCModel ugcmodel)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var UGClist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == ugcmodel.affiliationTypeId && a.affiliationStatus == "Yes").OrderBy(a => a.id).ToList();

            bool newAffiliation = false;
            bool returnStatus = false;
            //DateTime? dt = null; //string.IsNullOrEmpty(date) ? (DateTime?)null : DateTime.Parse(date);
            if (UGClist.Count == 0)
            {
                newAffiliation = true;
            }
            else
            {
                var affFromDate = Convert.ToDateTime(DateTime.ParseExact(ugcmodel.AffiliationFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                var affToDate = Convert.ToDateTime(DateTime.ParseExact(ugcmodel.AffiliationToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                foreach (var item in UGClist)
                {
                    newAffiliation = false;

                    returnStatus = IsBetween(affFromDate, item.affiliationFromDate, item.affiliationToDate);

                    if (!returnStatus)
                    {
                        returnStatus = IsBetween(affToDate, item.affiliationFromDate, item.affiliationToDate);

                        if (!returnStatus)
                        {
                            returnStatus = IsBetween(item.affiliationFromDate, affFromDate, affToDate);

                            if (!returnStatus)
                            {
                                returnStatus = IsBetween(item.affiliationToDate, affFromDate, affToDate);

                                if (!returnStatus)
                                {
                                    newAffiliation = true;
                                }
                                else
                                {
                                    newAffiliation = false;
                                    break;
                                }
                            }
                            else
                            {
                                newAffiliation = false;
                                break;
                            }
                        }
                        else
                        {
                            newAffiliation = false;
                            break;
                        }
                    }
                    else
                    {
                        newAffiliation = false;
                        break;
                    }
                }
            }

            if (newAffiliation)
            {
                jntuh_college_affiliation jntuh_college_affiliation = new jntuh_college_affiliation();
                jntuh_college_affiliation.collegeId = userCollegeID;
                jntuh_college_affiliation.academicYearid = Convert.ToInt32(ugcmodel.AcademicYear);
                jntuh_college_affiliation.affiliationTypeId = ugcmodel.affiliationTypeId;
                jntuh_college_affiliation.affiliationFromDate = Convert.ToDateTime(DateTime.ParseExact(ugcmodel.AffiliationFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(ugcmodel.AffiliationFromDate)
                jntuh_college_affiliation.affiliationToDate = Convert.ToDateTime(DateTime.ParseExact(ugcmodel.AffiliationToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(ugcmodel.AffiliationToDate);

                if (ugcmodel.affiliationfile != null)
                {
                    string affiliationfile = "~/Content/Upload/College/UGC";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(ugcmodel.affiliationfile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (ugcmodel.affiliationfilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            ugcmodel.affiliationfile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            ugcmodel.affiliationfilepath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                        }
                        else
                        {
                            ugcmodel.affiliationfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), ugcmodel.affiliationfilepath));
                            jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                        }
                    }
                }
                else
                {
                    jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                }

                jntuh_college_affiliation.createdOn = DateTime.Now;
                jntuh_college_affiliation.createdBy = userID;
                jntuh_college_affiliation.affiliationDuration = Convert.ToInt32(ugcmodel.Duration);
                jntuh_college_affiliation.affiliationStatus = "Yes";

                db.jntuh_college_affiliation.Add(jntuh_college_affiliation);
                db.SaveChanges();

                TempData["Success"] = "Added successfully";

                return RedirectToAction("Create", "AffiliationTypes", new { collegeId = "" });
            }
            else
            {
                //Show that particular affiliation is already present.
                TempData["Error"] = "There is an Affiliation Status already in the given Dates.";
                return RedirectToAction("Add", "UGC", new { affiliationTypeId = Utilities.EncryptString(ugcmodel.affiliationTypeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
        }

        public bool IsBetween(DateTime? newDate, DateTime? fromDate, DateTime? toDate)
        {
            if (newDate == fromDate) return true;
            if (newDate == toDate) return true;

            if (fromDate <= toDate)
                return (newDate >= fromDate && newDate <= toDate);
            else
                return !(newDate >= toDate && newDate <= fromDate);
        }

        [HttpPost]
        public ActionResult DeleteStatus(string affiliationId)
        {
            DeleteFunctionality(affiliationId);

            TempData["Success"] = "Removed Successfully";
            //return RedirectToAction("Create", "AffiliationTypes");
            //return View("Create", "AffiliationTypes");
            //return Redirect(Request.UrlReferrer.ToString());
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string affiliationTypeId, string affiliationId)
        {
            DeleteFunctionality(affiliationId);

            TempData["Success"] = "Removed Successfully";
            return RedirectToAction("Edit", "UGC", new
            {
                affiliationTypeId = affiliationTypeId
            });
        }

        public void DeleteFunctionality(string affiliationId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            int dec_affiliationId = Convert.ToInt32(Utilities.DecryptString(affiliationId, WebConfigurationManager.AppSettings["CryptoKey"]));
            var jntuh_college_affiliation = db.jntuh_college_affiliation.Where(a => a.id == dec_affiliationId).FirstOrDefault();

            jntuh_college_affiliation.affiliationStatus = "No";
            jntuh_college_affiliation.updatedBy = userID;
            jntuh_college_affiliation.updatedOn = DateTime.Now;

            db.Entry(jntuh_college_affiliation).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
    public class UGC
    {
        public string AcademicYear { get; set; }
        public string AffiliationFromDate { get; set; }
        public string AffiliationToDate { get; set; }
        public string FilePath { get; set; }
        public int Duration { get; set; }
        public int affiliationTypeId { get; set; }
        public int affiliationId { get; set; }
    }
}
