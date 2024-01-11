using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeGoverningmeetingController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /Governingmeeting/
        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            ViewBag.GoverningList = getGoverningmeetings(userCollegeID);
            return View();
        }

        private List<Governingmeeting> getGoverningmeetings(int collegeId)
        {
            List<Governingmeeting> Minitesdata =new List<Governingmeeting>();
            var dbdata = db.jntuh_college_governingmeeting.Where(r => r.collegeid == collegeId).Select(s => s).ToList();
            foreach (var item in dbdata)
            {
                Governingmeeting minites=new Governingmeeting();
                minites.Id = item.id;
                minites.Academicyearid = item.academicyearid;
                minites.Collegeid = item.collegeid;
                minites.Dateofmetting = item.dateofmeeting.ToString().Split(' ')[0];
                minites.Minutescopyfile = item.minutescopy;
                minites.Remarks = item.remarks;
                Minitesdata.Add(minites);
            }
            return Minitesdata.OrderByDescending(s=>s.Id).ThenBy(r=>r.Id).ToList();
        }
            
        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index(Governingmeeting meetings)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
            var minitescopypath = "~/Content/Upload/College/MinitesCopy";
            if (meetings.Minutescopy != null)
            {
                if (!Directory.Exists(Server.MapPath(minitescopypath)))
                {
                    Directory.CreateDirectory(Server.MapPath(minitescopypath));
                }
                var ext = Path.GetExtension(meetings.Minutescopy.FileName);
                if (ext.ToUpper().Equals(".PDF"))
                {
                    string filename = jntuhCollege.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_MC";
                    meetings.Minutescopy.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(minitescopypath), filename, ext));
                    meetings.Minutescopyfile = string.Format("{0}{1}", filename, ext);
                }
                jntuh_college_governingmeeting collegegoverningmeeting= new jntuh_college_governingmeeting();
                collegegoverningmeeting.collegeid = userCollegeID;
                collegegoverningmeeting.dateofmeeting = UAAAS.Models.Utilities.DDMMYY2MMDDYY(meetings.Dateofmetting);
                collegegoverningmeeting.academicyearid = ay0;
                collegegoverningmeeting.minutescopy = meetings.Minutescopyfile;
                collegegoverningmeeting.remarks = meetings.Remarks;
                collegegoverningmeeting.createdon = DateTime.Now;
                collegegoverningmeeting.createdby = userID;
                db.jntuh_college_governingmeeting.Add(collegegoverningmeeting);
                db.SaveChanges();
                TempData["SUCCESS"] = "Saved Successfully.";
            }
            else
            {
                TempData["ERROR"] = "File cann't be Empty.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult DeleteMeeting(int Id)
        {
            return RedirectToAction("Index", "UnderConstruction");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var deletemeeting =
                db.jntuh_college_governingmeeting.Where(r => r.collegeid == userCollegeID && r.id == Id)
                    .Select(s => s)
                    .FirstOrDefault();

            if (deletemeeting!=null)
            {
                db.jntuh_college_governingmeeting.Remove(deletemeeting);
                db.SaveChanges();
                TempData["SUCCESS"] = "Governing Meeting Deleted Successfully.";
            }
            else
            {
                TempData["ERROR"] = "Governing Meeting Delete failed.";
            }

            return RedirectToAction("Index");
        }

    }
}
