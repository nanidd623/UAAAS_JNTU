using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ReportsListController : BaseController
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
        public ActionResult NBACollegesList()
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
            List<NBACollegesList> collegedata = new List<NBACollegesList>();

            collegedata = (from c in db.jntuh_college
                           join ie in db.jntuh_college_nbaaccreditationdata
                               on c.id equals ie.collegeid
                           join sp in db.jntuh_specialization
                               on ie.specealizationid equals sp.id
                           join d in db.jntuh_department
                               on sp.departmentId equals d.id
                           join deg in db.jntuh_degree
                               on d.degreeId equals deg.id
                           join ca in db.jntuh_college_affiliation
                               on c.id equals ca.collegeId

                           where c.isActive == true &&
                           sp.isActive == true && d.isActive == true && deg.isActive == true && c.id != 375 && ie.nbafrom != null && ie.accademicyear == ay0

                           select new NBACollegesList
                           {
                               sno = ie.sno,
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               Degree = deg.degree,
                               Department = d.departmentName,
                               Specialization = sp.specializationName,
                               NBAFrom = ie.nbafrom,
                               NBATo = ie.nbato,
                               NBAApprovalLetter = ie.nbaapprovalletter
                           }).OrderBy(c => c.CollegeName).ThenBy(deg => deg.Degree).ThenBy(d => d.Department).ThenBy(sp => sp.Specialization).ToList();

            collegedata = collegedata.AsEnumerable().GroupBy(i => new { i.sno }).Select(c => c.FirstOrDefault()).ToList();

            return View(collegedata);
        }

        public ActionResult NBACollegesListExcel()
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
            gv.DataSource = GetNBACollegesListData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "NBA Colleges List";
            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
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

        public List<NBACollegesListExport> GetNBACollegesListData()
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
            List<NBACollegesList> collegedata = new List<NBACollegesList>();

            string url = "http://" + Request.Url.Host + "/Content/Upload/College/NBAAccredited_Latest/";

            collegedata = (from c in db.jntuh_college
                           join ie in db.jntuh_college_nbaaccreditationdata
                               on c.id equals ie.collegeid
                           join sp in db.jntuh_specialization
                               on ie.specealizationid equals sp.id
                           join d in db.jntuh_department
                               on sp.departmentId equals d.id
                           join deg in db.jntuh_degree
                               on d.degreeId equals deg.id
                           join ca in db.jntuh_college_affiliation
                               on c.id equals ca.collegeId

                           where c.isActive == true &&
                           sp.isActive == true && d.isActive == true && deg.isActive == true && c.id != 375 && ie.nbafrom != null && ie.accademicyear == ay0

                           select new NBACollegesList
                           {
                               sno = ie.sno,
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               Degree = deg.degree,
                               Department = d.departmentName,
                               Specialization = sp.specializationName,
                               NBAFrom = ie.nbafrom,
                               NBATo = ie.nbato,
                               NBAApprovalLetter = url + ie.nbaapprovalletter
                           }).OrderBy(c => c.CollegeName).ThenBy(deg => deg.Degree).ThenBy(d => d.Department).ThenBy(sp => sp.Specialization).ToList();

            collegedata = collegedata.AsEnumerable().GroupBy(i => new { i.sno }).Select(c => c.FirstOrDefault()).ToList();

            List<NBACollegesListExport> listExport = new List<NBACollegesListExport>();

            foreach (var c in collegedata)
            {
                NBACollegesListExport objExport = new NBACollegesListExport()
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    Degree = c.Degree,
                    Department = c.Department,
                    Specialization = c.Specialization,
                    NBAFrom = Convert.ToDateTime(c.NBAFrom).ToString("dd/MM/yyyy"),
                    NBATo = Convert.ToDateTime(c.NBATo).ToString("dd/MM/yyyy"),
                    NBAApprovalLetter = c.NBAApprovalLetter
                };
                listExport.Add(objExport);
            }

            return listExport;
        }


        [Authorize(Roles = "Admin")]
        public ActionResult AutonomousCollegesList()
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
            List<AutonomousCollegesList> collegedata = new List<AutonomousCollegesList>();

            collegedata = (from a in db.jntuh_college_affiliation
                           join c in db.jntuh_college
                           on a.collegeId equals c.id

                           where a.affiliationTypeId == 7 && a.affiliationStatus == "yes" && c.isActive == true &&
                           c.id != 375

                           select new AutonomousCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               AffiliationFromDate = a.affiliationFromDate,
                               AffiliationToDate = a.affiliationToDate,
                               AffiliationDocument = a.filePath,
                           }).OrderBy(c => c.CollegeName).ToList();

            return View(collegedata);
        }

        public ActionResult AutonomousCollegesListExcel()
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
            gv.DataSource = GetAutonomousCollegesListData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Autonomous Colleges List";
            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
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

        public List<AutonomousCollegesListExport> GetAutonomousCollegesListData()
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
            List<AutonomousCollegesList> collegedata = new List<AutonomousCollegesList>();

            string url = "http://" + Request.Url.Host + "/Content/Upload/College/UGC/";

            collegedata = (from a in db.jntuh_college_affiliation
                           join c in db.jntuh_college
                           on a.collegeId equals c.id

                           where a.affiliationTypeId == 7 && a.affiliationStatus == "yes" && c.isActive == true &&
                           c.id != 375

                           select new AutonomousCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               AffiliationFromDate = a.affiliationFromDate,
                               AffiliationToDate = a.affiliationToDate,
                               AffiliationDocument = "" + url + a.filePath,
                           }).OrderBy(c => c.CollegeName).ToList();

            List<AutonomousCollegesListExport> listexport = new List<AutonomousCollegesListExport>();

            foreach (var c in collegedata)
            {
                AutonomousCollegesListExport objExport = new AutonomousCollegesListExport()
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    AffiliationFromDate = c.AffiliationFromDate != null ? Convert.ToDateTime(c.AffiliationFromDate).ToString("dd/MM/yyyy") : "",
                    AffiliationToDate = c.AffiliationToDate != null ? Convert.ToDateTime(c.AffiliationToDate).ToString("dd/MM/yyyy") : "",
                    AffiliationDocument = c.AffiliationDocument,
                };
                listexport.Add(objExport);
            }

            return listexport;
        }


        [Authorize(Roles = "Admin")]
        public ActionResult MinorityCollegesList()
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
            List<MinorityCollegesList> collegedata = new List<MinorityCollegesList>();

            collegedata = (from c in db.jntuh_college
                           join ms in db.jntuh_college_minoritystatus on c.id equals ms.collegeId

                           where c.collegeStatusID == 1 && c.isActive == true
                           && c.id != 375

                           select new MinorityCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               StatusFromDate = ms.statusFromdate,
                               StatusToDate = ms.statusTodate,
                               StatusDocument = ms.statusFile,
                           }).OrderBy(c => c.CollegeName).ToList();

            return View(collegedata);
        }

        public ActionResult MinorityCollegesListExcel()
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
            gv.DataSource = GetMinorityCollegesListData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Minority Colleges List";
            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
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

        public List<MinorityCollegesListExport> GetMinorityCollegesListData()
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
            List<MinorityCollegesList> collegedata = new List<MinorityCollegesList>();

            string url = "http://" + Request.Url.Host + "/Content/Upload/College/CollegeStatus/";

            collegedata = (from c in db.jntuh_college
                           join ms in db.jntuh_college_minoritystatus on c.id equals ms.collegeId

                           where c.collegeStatusID == 1 && c.isActive == true
                           && c.id != 375

                           select new MinorityCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               StatusFromDate = ms.statusFromdate,
                               StatusToDate = ms.statusTodate,
                               StatusDocument = url + ms.statusFile,
                           }).OrderBy(c => c.CollegeName).ToList();

            List<MinorityCollegesListExport> listexport = new List<MinorityCollegesListExport>();

            foreach (var c in collegedata)
            {
                MinorityCollegesListExport objExport = new MinorityCollegesListExport()
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    StatusFromDate = c.StatusFromDate != null ? Convert.ToDateTime(c.StatusFromDate).ToString("dd/MM/yyyy") : "",
                    StatusToDate = c.StatusToDate != null ? Convert.ToDateTime(c.StatusToDate).ToString("dd/MM/yyyy") : "",
                    StatusDocument = c.StatusDocument,
                };
                listexport.Add(objExport);
            }

            return listexport;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult NAACCollegesList()
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
            List<NAACCollegesList> collegedata = new List<NAACCollegesList>();

            collegedata = (from c in db.jntuh_college
                           join jca in db.jntuh_college_affiliation
                                   on c.id equals jca.collegeId

                           where jca.affiliationTypeId == 2 && jca.affiliationStatus == "yes"
                           && c.isActive == true && c.id != 375

                           select new NAACCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               NAACFromDate = jca.affiliationFromDate,
                               NAACToDate = jca.affiliationToDate,
                               NAACApprovalLetter = jca.filePath,
                           }).OrderBy(c => c.CollegeName).ToList();

            return View(collegedata);
        }

        public ActionResult NAACCollegesListExcel()
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
            gv.DataSource = GetNAACCollegesListData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "NAAC Colleges List";
            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
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

        public List<NAACCollegesListExport> GetNAACCollegesListData()
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
            List<NAACCollegesList> collegedata = new List<NAACCollegesList>();

            string url = "http://" + Request.Url.Host + "/Content/Upload/College/NAAC/";

            collegedata = (from c in db.jntuh_college
                           join jca in db.jntuh_college_affiliation
                                   on c.id equals jca.collegeId

                           where jca.affiliationTypeId == 2 && jca.affiliationStatus == "yes"
                           && c.isActive == true && c.id != 375

                           select new NAACCollegesList
                           {
                               CollegeCode = c.collegeCode,
                               CollegeName = c.collegeName,
                               NAACFromDate = jca.affiliationFromDate,
                               NAACToDate = jca.affiliationToDate,
                               NAACApprovalLetter = url + jca.filePath,
                           }).OrderBy(c => c.CollegeName).ToList();

            List<NAACCollegesListExport> listexport = new List<NAACCollegesListExport>();

            foreach (var c in collegedata)
            {
                NAACCollegesListExport objExport = new NAACCollegesListExport()
                {
                    CollegeCode = c.CollegeCode,
                    CollegeName = c.CollegeName,
                    NAACFromDate = c.NAACFromDate != null ? Convert.ToDateTime(c.NAACFromDate).ToString("dd/MM/yyyy") : "",
                    NAACToDate = c.NAACToDate != null ? Convert.ToDateTime(c.NAACToDate).ToString("dd/MM/yyyy") : "",
                    NAACApprovalLetter = c.NAACApprovalLetter,
                };
                listexport.Add(objExport);
            }

            return listexport;
        }
    }
}
