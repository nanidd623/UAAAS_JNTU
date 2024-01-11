using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeEssentialRequirementsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult Index()
        {
            DateTime todayDate = DateTime.Now.Date;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collgeessentialreq = db.jntuh_college_essential_requirements.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var essentialreq = db.jntuh_essential_requirements.Where(i => i.isactive == true && i.essentialtype == 1).ToList();
            ViewBag.essentialreqs = essentialreq;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeEssentialRequirements");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ER") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeEssentialRequirements");
                }
            }
            if (collgeessentialreq.Count > 0)
            {
                return RedirectToAction("View");
            }
            var lstrequirements = new List<CollegeEssentialReq>();
            foreach (var item in essentialreq)
            {
                var objrequirements = new CollegeEssentialReq()
                {
                    essentialid = item.id,
                    essentialdoc = null,
                    essentialdocpath = string.Empty
                };
                lstrequirements.Add(objrequirements);
            }
            return View(lstrequirements);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Create(List<CollegeEssentialReq> essentialReqInformation)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID > 0)
            {
                foreach (var item in essentialReqInformation)
                {
                    var objessentialReq = new jntuh_college_essential_requirements()
                    {
                        essentialid = item.essentialid,
                        acadamicyearid = ay0,
                        essentialstatus = item.essentialstatus,
                        collegeid = userCollegeID,
                        createdby = userID,
                        createdon = DateTime.Now,
                        isactive = true
                    };

                    if (item.essentialdoc != null)
                    {
                        string affiliationfile = "~/Content/Upload/College/EssentialRequirements";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(item.essentialdoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (item.essentialdocpath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                item.essentialdoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                item.essentialdocpath = string.Format("{0}{1}", fileName, ext);
                                objessentialReq.supportintdocument = item.essentialdocpath;
                            }
                            else
                            {
                                item.essentialdoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.essentialdocpath));
                                objessentialReq.supportintdocument = item.essentialdocpath;
                            }
                        }
                    }
                    db.jntuh_college_essential_requirements.Add(objessentialReq);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult View()
        {
            DateTime todayDate = DateTime.Now.Date;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var masteractivities = db.jntuh_essential_requirements.Where(i => i.isactive == true && i.essentialtype == 1).ToList();
            var extraactivities = db.jntuh_college_essential_requirements.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lstrequirements = new List<CollegeEssentialReq>();
            foreach (var item in extraactivities)
            {
                var objrequirements = new CollegeEssentialReq()
                {
                    essentialid = item.essentialid,
                    essentialDesc = masteractivities.Where(i => i.id == item.essentialid).FirstOrDefault().essentialdescription,
                    essentialstatus = item.essentialstatus,
                    supportingdocuments = item.supportintdocument
                };
                lstrequirements.Add(objrequirements);
            }
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return View(lstrequirements);
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ER") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return View(lstrequirements);
                }
            }
            return View(lstrequirements);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult Edit(string collegeId)
        {
            DateTime todayDate = DateTime.Now.Date;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collgeessentialreqs = db.jntuh_college_essential_requirements.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var essentialreqs = db.jntuh_essential_requirements.Where(i => i.isactive == true && i.essentialtype == 1).ToList();
            ViewBag.essentialreqs = essentialreqs;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeEssentialRequirements");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("ER") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeEssentialRequirements");
                }
            }
            var lstessentialreq = new List<CollegeEssentialReq>();
            foreach (var item in collgeessentialreqs)
            {
                var objessentialreq = new CollegeEssentialReq()
                {
                    essentialid = item.essentialid,
                    essentialDesc = essentialreqs.Where(i => i.id == item.essentialid).FirstOrDefault().essentialdescription,
                    essentialstatus = item.essentialstatus,
                    supportingdocuments = item.supportintdocument,
                    essentialdocpath = item.supportintdocument,
                    essentialdoc = null,
                };
                lstessentialreq.Add(objessentialreq);
            }
            return View(lstessentialreq);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Edit(List<CollegeEssentialReq> extracurricularInformation)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collegeEssenReqs = db.jntuh_college_essential_requirements.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            if (userCollegeID > 0)
            {
                foreach (var cExAct in collegeEssenReqs)
                {
                    var collegeAct = extracurricularInformation.Where(i => i.essentialid == cExAct.essentialid).FirstOrDefault();
                    cExAct.essentialstatus = collegeAct.essentialstatus;
                    cExAct.updatedby = userID;
                    cExAct.updatedon = DateTime.Now;
                    if (collegeAct.essentialdoc != null)
                    {
                        if (collegeAct.essentialdoc.FileName != cExAct.supportintdocument)
                        {
                            string affiliationfile = "~/Content/Upload/College/EssentialRequirements";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(collegeAct.essentialdoc.FileName);
                            if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                collegeAct.essentialdoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                collegeAct.essentialdocpath = string.Format("{0}{1}", fileName, ext);
                                cExAct.supportintdocument = collegeAct.essentialdocpath;
                            }
                        }
                    }
                    if (cExAct.essentialstatus == false)
                    {
                        cExAct.supportintdocument = null;
                    }
                    db.Entry(cExAct).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("View");
        }
    }
}
