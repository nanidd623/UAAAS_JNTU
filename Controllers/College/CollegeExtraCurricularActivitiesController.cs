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
    public class CollegeExtraCurricularActivitiesController : BaseController
    {
        //
        // GET: /CollegeExtraCurricularActivities/

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
            var collgeextraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var extraactivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactityIds = extraactivities.Select(i => i.sno).ToArray();
            ViewBag.extraactivities = extraactivities;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExtraCurricularActivities");
            }
            else
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeExtraCurricularActivities");
                }
            }
            var clgextraactivities = collgeextraactivities.Where(i => extraactityIds.Contains(i.activityid)).ToList();
            if (clgextraactivities.Count > 0)
            {
                return RedirectToAction("View");
            }
            var lstactivities = new List<CollegeExtraCirActivities>();
            foreach (var item in extraactivities)
            {
                var objactivities = new CollegeExtraCirActivities()
                {
                    activityid = item.sno,
                    //activitystatus = false,
                    activitydoc = null,
                    activitydocpath = string.Empty
                };
                lstactivities.Add(objactivities);
            }
            return View(lstactivities);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Create(List<CollegeExtraCirActivities> extracurricularInformation)
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
                foreach (var item in extracurricularInformation)
                {
                    var objextraactivities = new jntuh_college_extracurricularactivities()
                    {
                        activityid = item.activityid,
                        academicyear = ay0,
                        activitystatus = item.activitystatus,
                        collegeid = userCollegeID,
                        createdby = userID,
                        createdon = DateTime.Now,
                        isactive = true,
                    };
                    if ((objextraactivities.activityid >= 5 && objextraactivities.activityid <= 7) && objextraactivities.activitystatus == true)
                    {
                        objextraactivities.remarks = item.remarks;
                    }

                    if (item.activitydoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/College/ExtraCirActivities";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(item.activitydoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            if (item.activitydocpath == null)
                            {
                                string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                item.activitydoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                item.activitydocpath = string.Format("{0}{1}", fileName, ext);
                                objextraactivities.supportingdocuments = item.activitydocpath;
                            }
                            else
                            {
                                item.activitydoc.SaveAs(string.Format("{0}/{1}", Server.MapPath(affiliationfile), item.activitydocpath));
                                objextraactivities.supportingdocuments = item.activitydocpath;
                            }
                        }
                    }
                    db.jntuh_college_extracurricularactivities.Add(objextraactivities);
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
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var lstactivities = (from item in extraactivities
                                 let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                 where jntuhExtracurricularactivities != null
                                 select new CollegeExtraCirActivities()
                                 {
                                     activityid = item.activityid,
                                     activityDesc = jntuhExtracurricularactivities.activitydescription,
                                     activitystatus = item.activitystatus,
                                     supportingdocuments = item.supportingdocuments,
                                     remarks = item.remarks
                                 }).ToList();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return View(lstactivities);
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return View(lstactivities);
                }
            }
            if (lstactivities.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(lstactivities);
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
            var extraactivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactivitiesIds = extraactivities.Select(i => i.sno).ToArray();
            var collgeextraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == userCollegeID && extraactivitiesIds.Contains(i.activityid)).ToList();
            ViewBag.extraactivities = extraactivities;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExtraCurricularActivities");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeExtraCurricularActivities");
                }
            }
            var lstactivities = new List<CollegeExtraCirActivities>();
            foreach (var item in collgeextraactivities)
            {
                var jntuhExtracurricularactivities = extraactivities.Where(i => i.sno == item.activityid).FirstOrDefault();
                if (jntuhExtracurricularactivities != null)
                {
                    var objactivities = new CollegeExtraCirActivities()
                    {
                        activityid = item.activityid,
                        activityDesc = jntuhExtracurricularactivities.activitydescription,
                        activitystatus = item.activitystatus,
                        supportingdocuments = item.supportingdocuments,
                        activitydocpath = item.supportingdocuments,
                        activitydoc = null,
                        remarks = item.remarks
                    };
                    lstactivities.Add(objactivities);
                }
            }
            if (lstactivities.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(lstactivities);
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Edit(List<CollegeExtraCirActivities> extracurricularInformation)
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
            var collegeExtraActivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == userCollegeID).ToList();
            if (userCollegeID > 0)
            {

                //var objextraactivities = new jntuh_college_extracurricularactivities();
                //{
                //    activityid = item.activityid,
                //    academicyear = ay0,
                //    activitystatus = item.activitystatus,
                //    collegeid = userCollegeID,
                //    createdby = userID,
                //    createdon = DateTime.Now,
                //    isactive = true
                //};

                foreach (var cExAct in collegeExtraActivities)
                {
                    var collegeAct = extracurricularInformation.FirstOrDefault(i => i.activityid == cExAct.activityid);
                    if (collegeAct != null)
                    {
                        cExAct.activitystatus = collegeAct.activitystatus;
                        cExAct.updatedby = userID;
                        cExAct.updatedon = DateTime.Now;
                        if ((collegeAct.activityid >= 5 && collegeAct.activityid <= 7) && collegeAct.activitystatus)
                        {
                            cExAct.remarks = collegeAct.remarks;
                        }
                        else if ((collegeAct.activityid >= 5 && collegeAct.activityid <= 7) && collegeAct.activitystatus == false)
                        {
                            cExAct.remarks = collegeAct.remarks;
                        }
                        if (collegeAct.activitydoc != null)
                        {
                            if (collegeAct.activitydoc.FileName != cExAct.supportingdocuments)
                            {
                                var affiliationfile = "~/Content/Upload/College/ExtraCirActivities";
                                if (!Directory.Exists(Server.MapPath(affiliationfile)))
                                {
                                    Directory.CreateDirectory(Server.MapPath(affiliationfile));
                                }
                                var ext = Path.GetExtension(collegeAct.activitydoc.FileName);
                                if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                                {
                                    string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                                    collegeAct.activitydoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                                    collegeAct.activitydocpath = string.Format("{0}{1}", fileName, ext);
                                    cExAct.supportingdocuments = collegeAct.activitydocpath;
                                }
                            }
                        }
                    }
                    if (cExAct.activitystatus == false)
                    {
                        cExAct.supportingdocuments = null;
                    }
                    db.Entry(cExAct).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("View");
        }
    }
}
