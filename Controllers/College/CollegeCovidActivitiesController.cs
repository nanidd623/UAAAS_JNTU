using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeCovidActivitiesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,College,SuperAdmin")]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "UnderConstruction");
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
            var collgeextraactivities = db.jntuh_college_covidactivities.Where(i => i.isactive == true && i.collegeid == userCollegeID && i.academicyear == ay0).ToList();
            var extraactivities = db.jntuh_covid_activites.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            ViewBag.extraactivities = extraactivities;
            var objcollgeextraactivities =
                collgeextraactivities.GroupBy(i => new { Date = i.submitteddate.ToString("dd-MM-yyyy") })
                    .ToList();
            var submitenabled = collgeextraactivities.Where(i => i.submitteddate.Date == DateTime.Now.Date).ToList();
            ViewBag.submit = submitenabled.Count > 0;
            var lstcovidactivities = objcollgeextraactivities.Select(ss => new CovidData {Date = ss.Key.Date}).ToList();
            ViewBag.collgeextraactivities = lstcovidactivities;
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
            //                                                                         editStatus.IsCollegeEditable == true &&
            //                                                                         editStatus.editFromDate <= todayDate &&
            //                                                                         editStatus.editToDate >= todayDate)
            //                                                    .Select(editStatus => editStatus.id)
            //                                                    .FirstOrDefault();
            //if (status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //    bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("VAP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            //    if (isPageEditable)
            //    {
            //        ViewBag.IsEditable = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = false;
            //        return RedirectToAction("View", "CollegeExtraCurricularActivities");
            //    }
            //}
            //if (collgeextraactivities.Count > 0)
            //{
            //    return RedirectToAction("View");
            //}
            var lstactivities = new List<CollegeCovidActivites>();
            foreach (var item in extraactivities)
            {
                var objactivities = new CollegeCovidActivites()
                {
                    activityid = item.id,
                    activitystatus = item.id == 8,
                    activitydoc = null,
                    activitydocpath = string.Empty
                };
                lstactivities.Add(objactivities);
            }
            return View(lstactivities);
        }


        [Authorize(Roles = "Admin,College,SuperAdmin")]
        [HttpPost]
        public ActionResult Create(List<CollegeCovidActivites> extracurricularInformation)
        {
            return RedirectToAction("Index", "UnderConstruction");
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
            var dates = new string[] { DateTime.Now.Date.ToString(CultureInfo.InvariantCulture), DateTime.Now.AddDays(-1).Date.ToString(CultureInfo.InvariantCulture) };
            if (userCollegeID > 0)
            {
                if (DateTime.Now.Date.ToString("dd-MM-yyyy") == "18-03-2020")
                {
                    foreach (var item in extracurricularInformation)
                    {
                        foreach (var dt in dates)
                        {
                            var objextraactivities = new jntuh_college_covidactivities
                            {
                                activityid = item.activityid,
                                academicyear = ay0,
                                activitystatus = item.activitystatus,
                                collegeid = userCollegeID,
                                createdby = userID,
                                createdon = DateTime.Now,
                                submitteddate = Convert.ToDateTime(dt),
                                remarks = item.remarks,
                                isactive = true,
                            };
                            if ((objextraactivities.activityid >= 5 && objextraactivities.activityid <= 7) && objextraactivities.activitystatus)
                            {
                                objextraactivities.remarks = item.remarks;
                            }

                            if (item.activitydoc != null)
                            {
                                const string affiliationfile = "~/Content/Upload/College/CovidActivities";
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
                            db.jntuh_college_covidactivities.Add(objextraactivities);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    foreach (var item in extracurricularInformation)
                    {
                        var objextraactivities = new jntuh_college_covidactivities
                        {
                            activityid = item.activityid,
                            academicyear = ay0,
                            activitystatus = item.activitystatus,
                            collegeid = userCollegeID,
                            createdby = userID,
                            createdon = DateTime.Now,
                            submitteddate = DateTime.Now.Date,
                            remarks = item.remarks,
                            isactive = true,
                        };
                        if ((objextraactivities.activityid >= 5 && objextraactivities.activityid <= 7) && objextraactivities.activitystatus == true)
                        {
                            objextraactivities.remarks = item.remarks;
                        }

                        if (item.activitydoc != null)
                        {
                            const string affiliationfile = "~/Content/Upload/College/CovidActivities";
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
                        db.jntuh_college_covidactivities.Add(objextraactivities);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public class CovidData
        {
            public string Date { get; set; }
        }

    }
}
