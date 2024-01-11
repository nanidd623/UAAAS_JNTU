using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;
using System.Web.Security;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ExcelDownloadController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DownloadExcel(string collegesType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var gv = new GridView();

            if (collegesType == "1" || collegesType == "2")
            {
                gv.DataSource = GetNBACollegeData(collegesType);
            }
            else
            {
                gv.DataSource = GetNAACCollegeData(collegesType);
            }
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;

            string reportName = collegesType == "1" ? "NBA Autonomous Colleges" :
                collegesType == "2" ? "NBA Non-Autonomous Colleges" :
                collegesType == "3" ? "NAAC Autonomous Colleges" : "NAAC Non-Autonomous Colleges";

            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " Data " + DateTime.Now + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View();
        }

        public List<NBACollegeData> GetNBACollegeData(string collegesType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            List<NBACollegeData> collegedata = null;

            if (collegesType == "1")// NBA Autonomous Colleges
            {
                collegedata = (from c in db.jntuh_college
                               join ie in db.jntuh_college_intake_existing
                                   on c.id equals ie.collegeId
                               join sp in db.jntuh_specialization
                                   on ie.specializationId equals sp.id
                               join d in db.jntuh_department
                                   on sp.departmentId equals d.id
                               join deg in db.jntuh_degree
                                   on d.degreeId equals deg.id
                               join ca in db.jntuh_college_affiliation
                                   on c.id equals ca.collegeId

                               where ca.affiliationTypeId == 7 && ca.affiliationStatus == "yes" && c.isActive == true &&
                               sp.isActive == true && d.isActive == true && deg.isActive == true && c.collegeCode != "zz" && ie.nbaFrom != null

                               select new NBACollegeData
                               {
                                   collegeCode = c.collegeCode,
                                   collegeName = c.collegeName,
                                   Degree = deg.degree,
                                   Department = d.departmentName,
                                   Specialization = sp.specializationName,
                                   NBAFrom = ie.nbaFrom,
                                   NBATo = ie.nbaTo
                               }).OrderBy(c => c.collegeName).ThenBy(deg => deg.Degree).ThenBy(d => d.Department).ThenBy(sp => sp.Specialization).ToList();
            }
            else // NBA Non-Autonomous Colleges
            {
                var nbaAutonomousColleges = (from c in db.jntuh_college
                                             //join ie in db.jntuh_college_intake_existing
                                             //    on c.id equals ie.collegeId
                                             //join sp in db.jntuh_specialization
                                             //    on ie.specializationId equals sp.id
                                             //join d in db.jntuh_department
                                             //    on sp.departmentId equals d.id
                                             //join deg in db.jntuh_degree
                                             //    on d.degreeId equals deg.id
                                             join ca in db.jntuh_college_affiliation
                                                 on c.id equals ca.collegeId

                                             where ca.affiliationTypeId == 7 && ca.affiliationStatus == "yes" && c.isActive == true && c.collegeCode != "zz"
                                             //sp.isActive == true && d.isActive == true && deg.isActive == true &&  && ie.nbaFrom != null

                                             select c.id).Distinct().ToList();

                collegedata = (from c in db.jntuh_college
                               join ie in db.jntuh_college_intake_existing
                                   on c.id equals ie.collegeId
                               join sp in db.jntuh_specialization
                                   on ie.specializationId equals sp.id
                               join d in db.jntuh_department
                                   on sp.departmentId equals d.id
                               join deg in db.jntuh_degree
                                   on d.degreeId equals deg.id
                               join ca in db.jntuh_college_affiliation
                                   on c.id equals ca.collegeId

                               where c.isActive == true && sp.isActive == true && d.isActive == true && deg.isActive == true && c.collegeCode != "zz" && ie.nbaFrom != null &&
                               !nbaAutonomousColleges.Contains(c.id)

                               select new NBACollegeData
                               {
                                   collegeCode = c.collegeCode,
                                   collegeName = c.collegeName,
                                   Degree = deg.degree,
                                   Department = d.departmentName,
                                   Specialization = sp.specializationName,
                                   NBAFrom = ie.nbaFrom,
                                   NBATo = ie.nbaTo
                               }).OrderBy(c => c.collegeName).ThenBy(deg => deg.Degree).ThenBy(d => d.Department).ThenBy(sp => sp.Specialization).ToList();
            }
            return collegedata;
        }

        public List<NAACCollegeData> GetNAACCollegeData(string collegesType)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            List<NAACCollegeData> collegedata = null;

            if (collegesType == "3") // NAAC Autonomous Colleges
            {
                var autonomousColleges = db.jntuh_college_affiliation.Where(ca => ca.affiliationTypeId == 7 && ca.affiliationStatus == "yes").
                    Select(ca => ca.collegeId).ToList();

                collegedata = (from c in db.jntuh_college
                               join jca in db.jntuh_college_affiliation
                                   on c.id equals jca.collegeId

                               where autonomousColleges.Contains(jca.collegeId) && jca.affiliationTypeId == 2 && jca.affiliationStatus == "yes" && c.collegeCode != "zz"

                               select new NAACCollegeData
                               {
                                   collegeCode = c.collegeCode,
                                   collegeName = c.collegeName,
                                   //Degree = deg.degree,
                                   //Department = d.departmentName,
                                   //Specialization = sp.specializationName,
                                   AffiliationFromDate = jca.affiliationFromDate,
                                   AffiliationToDate = jca.affiliationToDate
                               }).OrderBy(c => c.collegeName).ToList();
            }
            else // NAAC Non-Autonomous Colleges
            {
                var autonomousColleges = db.jntuh_college_affiliation.Where(ca => ca.affiliationTypeId == 7 && ca.affiliationStatus == "yes").
                    Select(ca => ca.collegeId).ToList();

                var naacAutonomousColleges = (from c in db.jntuh_college
                                              join jca in db.jntuh_college_affiliation
                                                  on c.id equals jca.collegeId

                                              where autonomousColleges.Contains(jca.collegeId) && jca.affiliationTypeId == 2 && jca.affiliationStatus == "yes" && c.collegeCode != "zz"

                                              select c.id).ToList();

                collegedata = (from c in db.jntuh_college
                               join jca in db.jntuh_college_affiliation
                                   on c.id equals jca.collegeId

                               where jca.affiliationTypeId == 2 && jca.affiliationStatus == "yes" && c.collegeCode != "zz" &&
                               !naacAutonomousColleges.Contains(jca.collegeId)

                               select new NAACCollegeData
                               {
                                   collegeCode = c.collegeCode,
                                   collegeName = c.collegeName,
                                   //Degree = deg.degree,
                                   //Department = d.departmentName,
                                   //Specialization = sp.specializationName,
                                   AffiliationFromDate = jca.affiliationFromDate,
                                   AffiliationToDate = jca.affiliationToDate
                               }).OrderBy(c => c.collegeName).ToList();
            }
            return collegedata;
        }

        public class NBACollegeData
        {
            //public int collegeId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public Nullable<System.DateTime> NBAFrom { get; set; }
            public Nullable<System.DateTime> NBATo { get; set; }
        }

        public class NAACCollegeData
        {
            //public int collegeId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }
            //public string Degree { get; set; }
            //public string Department { get; set; }
            //public string Specialization { get; set; }
            public Nullable<System.DateTime> AffiliationFromDate { get; set; }
            public Nullable<System.DateTime> AffiliationToDate { get; set; }
        }
    }
}
