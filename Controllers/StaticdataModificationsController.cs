
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
using Microsoft.Ajax.Utilities;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class StaticdataModificationsController : BaseController
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
            List<college_staticdata_modifications> collegeStaticdataModifications = new List<college_staticdata_modifications>();
            List<jntuh_college_staticdata_modifications> staticDataModifications = db.jntuh_college_staticdata_modifications.Where(s => s.collegeId == userCollegeID).OrderBy(s => s.formNo).ToList();
            foreach (var item in staticDataModifications)
            {
                college_staticdata_modifications newstatic = new college_staticdata_modifications();
                newstatic.id = item.id;
                newstatic.AcademicYearid = item.academicyearId;
                newstatic.formno = item.formNo;
                newstatic.FormName =
                    db.jntuh_college_screens.Where(s => s.Id == newstatic.formno && s.IsActive == true)
                        .Select(s => s.ScreenName)
                        .FirstOrDefault();
                newstatic.justification = item.justification;
                newstatic.staticdatafilename = item.path;
                collegeStaticdataModifications.Add(newstatic);
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

            if (staticDataModifications.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.CommitteeNotUpload = true;
            }
            else
            {
                ViewBag.CommitteeNotUpload = false;
            }
            if (status == 0 && (staticDataModifications.Count() > 0) && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("DM") && a.CollegeId == userCollegeID && a.IsEditable == true).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View");
            }
            return View(collegeStaticdataModifications);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            college_staticdata_modifications modifications = new college_staticdata_modifications();
            //jntuh_college_staticdata_modifications staticdatamodifications = new jntuh_college_staticdata_modifications();
            List<SelectListItem> FarmNumbers = new List<SelectListItem>();
            //New Code is Adding on 01-29-2019
            List<jntuh_college_screens> screens =
                db.jntuh_college_screens.Where(s => s.IsActive == true).Select(s => s).ToList();
            foreach (var item in screens)
            {
                SelectListItem form = new SelectListItem();
                form.Text = item.ScreenName.ToString();
                form.Value = item.Id.ToString();
                FarmNumbers.Add(form);
            }
            ViewBag.FarmNumbers = FarmNumbers;
            if (id != null)
            {
                ViewBag.IsUpdate = true;
                jntuh_college_staticdata_modifications staticdatamodifications = db.jntuh_college_staticdata_modifications.Where(s => s.id == id).FirstOrDefault();
                modifications.id = staticdatamodifications.id;
                modifications.formno = staticdatamodifications.formNo;
                modifications.AcademicYearid = staticdatamodifications.academicyearId;
                modifications.justification = staticdatamodifications.justification;
                modifications.staticdatafilename = staticdatamodifications.path;
                modifications.collegeId = staticdatamodifications.collegeId;
                return PartialView("_AddEditRecord", modifications);
            }
            else
            {
                if (collegeId != null)
                {
                    int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    modifications.collegeId = userCollegeID;
                }
                ViewBag.IsUpdate = false;
            }
            return PartialView("_AddEditRecord", modifications);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(college_staticdata_modifications staticdatamodification, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            jntuh_college_staticdata_modifications staticdatamodifications = new jntuh_college_staticdata_modifications();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            List<SelectListItem> FarmNumbers = new List<SelectListItem>();
            List<jntuh_college_screens> screens =
                 db.jntuh_college_screens.Where(s => s.IsActive == true).Select(s => s).ToList();
            foreach (var item in screens)
            {
                SelectListItem form = new SelectListItem();
                form.Text = item.ScreenName.ToString();
                form.Value = item.Id.ToString();
                FarmNumbers.Add(form);
            }
            ViewBag.formNo = FarmNumbers;
            if (userCollegeID == 0)
            {
                userCollegeID = staticdatamodifications.collegeId;
            }
            if (ModelState.IsValid)
            {

                if (cmd == "Save")
                {
                    staticdatamodifications.collegeId = staticdatamodification.collegeId;
                    staticdatamodifications.formNo = staticdatamodification.formno;
                    staticdatamodifications.academicyearId = ay0;
                    staticdatamodifications.justification = staticdatamodification.justification;
                    string fileName = string.Empty;
                    if (staticdatamodification.staticdatafile != null)
                    {

                        string ext = Path.GetExtension(staticdatamodification.staticdatafile.FileName);
                        fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_DataJustification_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                        staticdatamodification.staticdatafile.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/DataModifications"), fileName));
                        staticdatamodifications.path = fileName;
                    }
                    staticdatamodifications.createdBy = userID;
                    staticdatamodifications.createdOn = DateTime.Now;
                    db.jntuh_college_staticdata_modifications.Add(staticdatamodifications);
                    db.SaveChanges();
                    TempData["Success"] = "Static data modifications details Added successfully.";
                }
                else
                {
                    staticdatamodifications.id = staticdatamodification.id;
                    staticdatamodifications.collegeId = staticdatamodification.collegeId;
                    staticdatamodifications.formNo = staticdatamodification.formno;
                    staticdatamodifications.academicyearId = ay0;
                    staticdatamodifications.justification = staticdatamodification.justification;
                    staticdatamodifications.createdBy = userID;
                    staticdatamodifications.createdOn =
                        db.jntuh_college_staticdata_modifications.Where(
                            e => e.id == staticdatamodification.id && e.collegeId == staticdatamodification.collegeId)
                            .Select(s => s.createdOn)
                            .FirstOrDefault();
                    string fileName = string.Empty;
                    if (staticdatamodification.staticdatafile != null)
                    {
                        if (!String.IsNullOrEmpty(staticdatamodification.staticdatafilename))
                        {
                            staticdatamodification.staticdatafile.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/DataModifications"), staticdatamodification.staticdatafilename));
                            staticdatamodifications.path = staticdatamodification.staticdatafilename;
                        }
                        else
                        {
                            string ext = Path.GetExtension(staticdatamodification.staticdatafile.FileName);
                            fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_DataJustification_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                            staticdatamodification.staticdatafile.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/DataModifications"), fileName));
                            staticdatamodifications.path = fileName;
                        }
                    }
                    else
                    {
                        staticdatamodifications.path = staticdatamodification.staticdatafilename;
                    }
                    staticdatamodifications.updatedBy = userID;
                    staticdatamodifications.updatedOn = DateTime.Now;
                    db.Entry(staticdatamodifications).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Static data modifications details Updated successfully.";

                }

            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_staticdata_modifications staticdatamodifications = db.jntuh_college_staticdata_modifications.Find(id);
            int collegeid = staticdatamodifications.collegeId;
            if (staticdatamodifications != null)
            {

                try
                {
                    db.jntuh_college_staticdata_modifications.Remove(staticdatamodifications);
                    db.SaveChanges();
                    TempData["Success"] = "Static data modifications details Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(collegeid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            jntuh_college_staticdata_modifications staticdatamodifications = db.jntuh_college_staticdata_modifications.Find(id);
            college_staticdata_modifications college_staticdata = new college_staticdata_modifications();
            college_staticdata.id = staticdatamodifications.id;
            college_staticdata.collegeId = staticdatamodifications.collegeId;
            college_staticdata.formno = staticdatamodifications.formNo;
            college_staticdata.FormName =
                db.jntuh_college_screens.Where(s => s.Id == college_staticdata.formno && s.IsActive == true)
                    .Select(s => s.ScreenName)
                    .FirstOrDefault();
            college_staticdata.justification = staticdatamodifications.justification;
            college_staticdata.staticdatafilename = staticdatamodifications.path;
            return PartialView("_Details", college_staticdata);

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            List<college_staticdata_modifications> collegeStaticdataModifications = new List<college_staticdata_modifications>();
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("DM") && a.CollegeId == userCollegeID && a.IsEditable == true).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }
            List<jntuh_college_staticdata_modifications> staticdatamodifications = db.jntuh_college_staticdata_modifications.Where(m => m.collegeId == userCollegeID).OrderBy(m => m.formNo).ToList();

            foreach (var item in staticdatamodifications)
            {
                college_staticdata_modifications newstatic = new college_staticdata_modifications();
                newstatic.id = item.id;
                newstatic.AcademicYearid = item.academicyearId;
                newstatic.formno = item.formNo;
                newstatic.FormName =
                    db.jntuh_college_screens.Where(s => s.Id == newstatic.formno && s.IsActive == true)
                        .Select(s => s.ScreenName)
                        .FirstOrDefault();
                newstatic.justification = item.justification;
                newstatic.staticdatafilename = item.path;
                collegeStaticdataModifications.Add(newstatic);
            }
            return View(collegeStaticdataModifications);
        }

        public ActionResult UserView(string id)
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Index");
            }
            List<jntuh_college_staticdata_modifications> staticdatamodifications = db.jntuh_college_staticdata_modifications.Where(m => m.collegeId == userCollegeID).OrderBy(m => m.formNo).ToList();
            List<college_staticdata_modifications> collegeStaticdataModifications = new List<college_staticdata_modifications>();
            foreach (var item in staticdatamodifications)
            {
                college_staticdata_modifications newstatic = new college_staticdata_modifications();
                newstatic.id = item.id;
                newstatic.AcademicYearid = item.academicyearId;
                newstatic.formno = item.formNo;
                newstatic.FormName =
                    db.jntuh_college_screens.Where(s => s.Id == newstatic.formno && s.IsActive == true)
                        .Select(s => s.ScreenName)
                        .FirstOrDefault();
                newstatic.justification = item.justification;
                newstatic.staticdatafilename = item.path;
                collegeStaticdataModifications.Add(newstatic);
            }
            return View(collegeStaticdataModifications);
        }

    }
}
