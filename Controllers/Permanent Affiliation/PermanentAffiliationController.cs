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
    public class PermanentAffiliationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

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

            var PA_list = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<PermanentAffiliation> paListObj = new List<PermanentAffiliation>();
            foreach (var item in PA_list)
            {
                PermanentAffiliation paObj = new PermanentAffiliation();
                paObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                paObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                paObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                paObj.Duration = Convert.ToInt32(item.affiliationDuration);
                paObj.FilePath = item.filePath;
                paObj.affiliationTypeId = item.affiliationTypeId;
                paObj.affiliationId = item.id;
                paListObj.Add(paObj);
            }
            ViewBag.PAList = paListObj;
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
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PPA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            var PA_list = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == dec_affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<PermanentAffiliation> paListObj = new List<PermanentAffiliation>();
            foreach (var item in PA_list)
            {
                PermanentAffiliation paObj = new PermanentAffiliation();
                paObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                paObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                paObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                paObj.Duration = Convert.ToInt32(item.affiliationDuration);
                paObj.FilePath = item.filePath;
                paObj.affiliationTypeId = item.affiliationTypeId;
                paObj.affiliationId = item.id; 

                paListObj.Add(paObj);
            }
            ViewBag.PAList = paListObj;
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();
            PermanentAffiliationModel pamodel = new PermanentAffiliationModel();
            if (dec_affiliationId > 0)
            {
                var clgAffiliation = db.jntuh_college_affiliation.Where(a => a.id == dec_affiliationId).Select(a => a).FirstOrDefault();
                pamodel.affiliationId = clgAffiliation.id;
                pamodel.affiliationTypeId = clgAffiliation.affiliationTypeId; 
                pamodel.collegeId = clgAffiliation.collegeId;
                pamodel.AcademicYear = Convert.ToInt32(clgAffiliation.academicYearid);
                pamodel.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", clgAffiliation.affiliationFromDate);
                pamodel.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", clgAffiliation.affiliationToDate);
                pamodel.affiliationfilepath = clgAffiliation.filePath;
                pamodel.Duration = Convert.ToString(clgAffiliation.affiliationDuration);
            }
            pamodel.affiliationTypeId = dec_affiliationTypeId;
            return View(pamodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(PermanentAffiliationModel pamodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //var clgAffiliation = db.jntuh_college_affiliation.Where(a => a.id == ugcmodel.affiliationId).Select(a => a).FirstOrDefault();

            var PAlist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == pamodel.affiliationTypeId && a.id != pamodel.affiliationId && a.affiliationStatus == "Yes").ToList();

            bool newAffiliation = false;
            bool returnStatus = false;

            if (PAlist.Count == 0)
            {
                newAffiliation = true;
            }
            else
            {
                foreach (var item in PAlist)
                {
                    newAffiliation = false;

                    returnStatus = IsBetween(Convert.ToDateTime(pamodel.AffiliationFromDate), item.affiliationFromDate, item.affiliationToDate);

                    if (!returnStatus)
                    {
                        returnStatus = IsBetween(Convert.ToDateTime(pamodel.AffiliationToDate), item.affiliationFromDate, item.affiliationToDate);

                        if (!returnStatus)
                        {
                            returnStatus = IsBetween(item.affiliationFromDate, Convert.ToDateTime(pamodel.AffiliationFromDate), Convert.ToDateTime(pamodel.AffiliationToDate));

                            if (!returnStatus)
                            {
                                returnStatus = IsBetween(item.affiliationToDate, Convert.ToDateTime(pamodel.AffiliationFromDate), Convert.ToDateTime(pamodel.AffiliationToDate));

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
                var jntuh_college_affiliation = db.jntuh_college_affiliation.Where(a => a.id == pamodel.affiliationId).Select(a => a).FirstOrDefault();
                //jntuh_college_affiliation.id = ugcmodel.affiliationId;
                jntuh_college_affiliation.collegeId = pamodel.collegeId;
                jntuh_college_affiliation.academicYearid = Convert.ToInt32(pamodel.AcademicYear);
                jntuh_college_affiliation.affiliationTypeId = pamodel.affiliationTypeId;
                jntuh_college_affiliation.affiliationFromDate = Convert.ToDateTime(pamodel.AffiliationFromDate);
                jntuh_college_affiliation.affiliationToDate = Convert.ToDateTime(pamodel.AffiliationToDate);

                if (pamodel.affiliationfile != null)
                {
                    string affiliationfile = "~/Content/Upload/College/PermanentAffiliation";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(pamodel.affiliationfile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (pamodel.affiliationfilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            pamodel.affiliationfile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            pamodel.affiliationfilepath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_affiliation.filePath = pamodel.affiliationfilepath;
                        }
                        else
                        {
                            pamodel.affiliationfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), pamodel.affiliationfilepath));
                            jntuh_college_affiliation.filePath = pamodel.affiliationfilepath;
                        }
                    }
                }
                //else
                //{
                //    jntuh_college_affiliation.filePath = ugcmodel.affiliationfilepath;
                //}

                jntuh_college_affiliation.createdOn = db.jntuh_college_affiliation.Where(a => a.id == pamodel.affiliationId).Select(a => a.createdOn).FirstOrDefault();
                jntuh_college_affiliation.createdBy = db.jntuh_college_affiliation.Where(a => a.id == pamodel.affiliationId).Select(a => a.createdBy).FirstOrDefault();
                jntuh_college_affiliation.updatedOn = DateTime.Now;
                jntuh_college_affiliation.updatedBy = userID;
                jntuh_college_affiliation.affiliationDuration = Convert.ToInt32(pamodel.Duration);
                jntuh_college_affiliation.affiliationStatus = "Yes";

                db.Entry(jntuh_college_affiliation).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Updated successfully";

                return RedirectToAction("Create", "AffiliationTypes");
            }
            else
            {
                TempData["Error"] = "There is an Affiliation Status already in the given Dates.";
                return RedirectToAction("Edit", "PermanentAffiliation", new
                {
                    affiliationTypeId = Utilities.EncryptString(pamodel.affiliationTypeId.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]).ToString(),
                    affiliationId = Utilities.EncryptString(pamodel.affiliationId.ToString(),
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
            var PA_list = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == dec_affiliationTypeId && a.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).ToList();
            List<PermanentAffiliation> paListObj = new List<PermanentAffiliation>();
            foreach (var item in PA_list)
            {
                PermanentAffiliation paObj = new PermanentAffiliation();
                paObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                paObj.AffiliationFromDate = string.Format("{0:dd/MM/yyyy}", item.affiliationFromDate);
                paObj.AffiliationToDate = string.Format("{0:dd/MM/yyyy}", item.affiliationToDate);
                paObj.Duration = Convert.ToInt32(item.affiliationDuration);
                paObj.FilePath = item.filePath;
                paObj.affiliationTypeId = item.affiliationTypeId;
                paObj.affiliationId = item.id; 

                paListObj.Add(paObj);
            }
            ViewBag.PAList = paListObj;
            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive && a.actualYear > 2013).OrderByDescending(a => a.actualYear).ToList();
            PermanentAffiliationModel pamodel = new PermanentAffiliationModel();
            pamodel.affiliationTypeId = dec_affiliationTypeId;
            return View(pamodel);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Add(PermanentAffiliationModel pamodel)
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
            var PAlist = db.jntuh_college_affiliation.Where(a => a.collegeId == userCollegeID && a.affiliationTypeId == pamodel.affiliationTypeId && a.affiliationStatus == "Yes").OrderBy(a => a.id).ToList();

            bool newAffiliation = false;
            bool returnStatus = false;
            //DateTime? dt = null; //string.IsNullOrEmpty(date) ? (DateTime?)null : DateTime.Parse(date);
            if (PAlist.Count == 0)
            {
                newAffiliation = true;
            }
            else
            {
                var affFromDate = Convert.ToDateTime(DateTime.ParseExact(pamodel.AffiliationFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                var affToDate = Convert.ToDateTime(DateTime.ParseExact(pamodel.AffiliationToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                foreach (var item in PAlist)
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
                jntuh_college_affiliation.academicYearid = ay0;
                jntuh_college_affiliation.affiliationTypeId = pamodel.affiliationTypeId;
                jntuh_college_affiliation.affiliationFromDate = Convert.ToDateTime(DateTime.ParseExact(pamodel.AffiliationFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(pamodel.AffiliationFromDate);
                jntuh_college_affiliation.affiliationToDate = Convert.ToDateTime(DateTime.ParseExact(pamodel.AffiliationToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)); //Convert.ToDateTime(pamodel.AffiliationToDate);

                if (pamodel.affiliationfile != null)
                {
                    string affiliationfile = "~/Content/Upload/College/PermanentAffiliation";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(pamodel.affiliationfile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (pamodel.affiliationfilepath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            pamodel.affiliationfile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            pamodel.affiliationfilepath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_affiliation.filePath = pamodel.affiliationfilepath;
                        }
                        else
                        {
                            pamodel.affiliationfile.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), pamodel.affiliationfilepath));
                            jntuh_college_affiliation.filePath = pamodel.affiliationfilepath;
                        }
                    }
                }
                else
                {
                    jntuh_college_affiliation.filePath = pamodel.affiliationfilepath;
                }

                jntuh_college_affiliation.createdOn = DateTime.Now;
                jntuh_college_affiliation.createdBy = userID;
                jntuh_college_affiliation.affiliationDuration = Convert.ToInt32(pamodel.Duration);
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
                return RedirectToAction("Add", "PermanentAffiliation", new { affiliationTypeId = Utilities.EncryptString(pamodel.affiliationTypeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            return RedirectToAction("Edit", "PermanentAffiliation", new
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
    public class PermanentAffiliation
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
