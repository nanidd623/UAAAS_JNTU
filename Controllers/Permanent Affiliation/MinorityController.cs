using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
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
    public class MinorityController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private uaaasDBContext db1 = new uaaasDBContext();

        public ActionResult View(string collegeStatus)
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            var MinorityList = db.jntuh_college_minoritystatus.Where(a => a.collegeId == userCollegeID).OrderBy(a => a.id).ToList();
            List<Minority> minorityListObj = new List<Minority>();
            foreach (var item in MinorityList)
            {
                Minority minorityObj = new Minority();
                minorityObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                minorityObj.StatusFromDate = string.Format("{0:dd/MM/yyyy}", item.statusFromdate);
                minorityObj.StatusToDate = string.Format("{0:dd/MM/yyyy}", item.statusTodate);
                minorityObj.StatusFile = item.statusFile;
                minorityObj.CollegeStatus = db.jntuh_college_status.Where(s => s.id == item.collegeStatusid).Select(s => s.collegeStatus).FirstOrDefault();

                minorityListObj.Add(minorityObj);
            }
            ViewBag.MinorityList = minorityListObj;
            return View();
        }

        public ActionResult Edit(string collegeStatus)
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
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            var MinorityList = db.jntuh_college_minoritystatus.Where(a => a.collegeId == userCollegeID).OrderBy(a => a.id).ToList();
            List<Minority> minorityListObj = new List<Minority>();
            foreach (var item in MinorityList)
            {
                Minority minorityObj = new Minority();
                minorityObj.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearid).Select(a => a.academicYear).FirstOrDefault();
                minorityObj.StatusFromDate = string.Format("{0:dd/MM/yyyy}", item.statusFromdate);
                minorityObj.StatusToDate = string.Format("{0:dd/MM/yyyy}", item.statusTodate);
                minorityObj.StatusFile = item.statusFile;
                minorityObj.CollegeStatus = db.jntuh_college_status.Where(s => s.id == item.collegeStatusid).Select(s => s.collegeStatus).FirstOrDefault();

                minorityListObj.Add(minorityObj);
            }
            ViewBag.MinorityList = minorityListObj;
            ViewBag.AcademicYears = db.jntuh_academic_year.Where(a => a.isActive).OrderByDescending(a => a.id).Take(6);
            MinorityModel minoritymodel = new MinorityModel();

            ViewBag.CollegeStatus = db.jntuh_college_status.Where(s => s.isActive);
            return View(minoritymodel);
        }

    }
    public class Minority
    {
        public string AcademicYear { get; set; }
        public string StatusFromDate { get; set; }
        public string StatusToDate { get; set; }
        public string StatusFile { get; set; }
        public string CollegeStatus { get; set; }
    }
}
