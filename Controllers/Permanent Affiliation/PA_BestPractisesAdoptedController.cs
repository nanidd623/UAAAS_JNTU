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

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_BestPractisesAdoptedController : BaseController
    {
        private readonly uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult Index()
        {
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return View(new List<CollegeExtraCirActivities>());
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //var jntuhAcademicYear = db.jntuh_academic_year.Select(s => s).ToList();
            //var actualYear = jntuhAcademicYear.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            //var ay0 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collgeextraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var extraactivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 12).ToList();
            var extraactityIds = extraactivities.Select(i => i.sno).ToArray();
            ViewBag.extraactivities = extraactivities;
            var status = GetPageEditableStatus(userCollegeId);
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_BestPractisesAdopted");
            }
            else
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBPA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_BestPractisesAdopted");
                }
            }

            var clgdesCondtions = collgeextraactivities.Where(i => extraactityIds.Contains(i.activityid)).ToList();
            if (clgdesCondtions.Count > 0)
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
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("View");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault();
            var jntuhAcademicYear = db.jntuh_academic_year.Select(s => s).ToList();
            var actualYear = jntuhAcademicYear.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var ay0 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeId <= 0) return RedirectToAction("View");
            foreach (var item in extracurricularInformation)
            {
                var objextraactivities = new jntuh_college_extracurricularactivities()
                {
                    activityid = item.activityid,
                    academicyear = ay0,
                    activitystatus = item.activitystatus,
                    collegeid = userCollegeId,
                    createdby = userId,
                    createdon = DateTime.Now,
                    isactive = true,
                    remarks = item.remarks
                };
                //if ((objextraactivities.activityid >= 5 && objextraactivities.activityid <= 7) && objextraactivities.activitystatus)
                //{
                //    objextraactivities.remarks = item.remarks;
                //}

                if (item.activitydoc != null)
                {
                    const string affiliationfile = "~/Content/Upload/College/BestPractisesAdopted";
                    if (!Directory.Exists(Server.MapPath(affiliationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(affiliationfile));
                    }
                    var ext = Path.GetExtension(item.activitydoc.FileName);
                    if (ext != null && (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF")))
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
            return RedirectToAction("View");
        }

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult View()
        {
            //var todayDate = DateTime.Now.Date;
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return View(new List<CollegeExtraCirActivities>());
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 12).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            //var jntuhAcademicYear = db.jntuh_academic_year.Select(s => s).ToList();
            //var actualYear = jntuhAcademicYear.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //var ay0 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
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
            var status = GetPageEditableStatus(userCollegeId);
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return View(lstactivities);
            }
            else
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBPA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
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
            //var todayDate = DateTime.Now.Date;
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return View(new List<CollegeExtraCirActivities>());
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //var jntuhAcademicYear = db.jntuh_academic_year.Select(s => s).ToList();
            //var actualYear = jntuhAcademicYear.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //var ay0 = jntuhAcademicYear.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collgeextraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            var extraactivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 12).ToList();
            ViewBag.extraactivities = extraactivities;
            var status = GetPageEditableStatus(userCollegeId);
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_BestPractisesAdopted");
            }
            else
            {
                ViewBag.IsEditable = true;
                var isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PBPA") && a.CollegeId == userCollegeId).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PA_BestPractisesAdopted");
                }
            }
            var lstactivities = new List<CollegeExtraCirActivities>();
            foreach (var item in collgeextraactivities)
            {
                var jntuhExtracurricularactivities = extraactivities.FirstOrDefault(i => i.sno == item.activityid);
                if (jntuhExtracurricularactivities == null) continue;
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
            var membershipUser = Membership.GetUser(User.Identity.Name);
            if (membershipUser == null) return RedirectToAction("View");
            var userId = Convert.ToInt32(membershipUser.ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault();
            //var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            //var actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //var ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collegeExtraActivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive && i.collegeid == userCollegeId).ToList();
            if (userCollegeId <= 0) return RedirectToAction("View");
            foreach (var cExAct in collegeExtraActivities)
            {
                var collegeAct = extracurricularInformation.FirstOrDefault(i => i.activityid == cExAct.activityid);
                if (collegeAct != null)
                {
                    cExAct.activitystatus = collegeAct.activitystatus;
                    cExAct.updatedby = userId;
                    cExAct.updatedon = DateTime.Now;
                    cExAct.remarks = collegeAct.remarks;
                    //if ((collegeAct.activityid >= 5 && collegeAct.activityid <= 7) && collegeAct.activitystatus)
                    //{
                    //    cExAct.remarks = collegeAct.remarks;
                    //}
                    //else if ((collegeAct.activityid >= 5 && collegeAct.activityid <= 7) && collegeAct.activitystatus == false)
                    //{
                    //    cExAct.remarks = collegeAct.remarks;
                    //}
                    if (collegeAct.activitydoc != null)
                    {
                        if (collegeAct.activitydoc.FileName != cExAct.supportingdocuments)
                        {
                            const string affiliationfile = "~/Content/Upload/College/BestPractisesAdopted";
                            if (!Directory.Exists(Server.MapPath(affiliationfile)))
                            {
                                Directory.CreateDirectory(Server.MapPath(affiliationfile));
                            }
                            var ext = Path.GetExtension(collegeAct.activitydoc.FileName);
                            if (ext != null && (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF")))
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
            return RedirectToAction("View");
        }

    }
}
