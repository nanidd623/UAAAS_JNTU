using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class ApprovedAdmittedIntakeController : BaseController
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
            var years = db.jntuh_academic_year.OrderBy(a => a.academicYear).ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.id.ToString(),
                    Text = x.academicYear.ToString()
                });
            ViewBag.AcademicYear = new MultiSelectList(years, "Value", "Text");

            //if (years != null)
            //{
            //    ViewBag.AcademicYear = years.Select(N => new SelectListItem { Text = N.academicYear, Value = N.id.ToString() });
            //}

            var colleges = db.jntuh_college.OrderBy(a => a.collegeName).ToList()
                .Select(x => new SelectListItem
                {
                    Value = x.collegeCode.ToString(),
                    Text = x.collegeName.ToString()
                });
            ViewBag.Colleges = new MultiSelectList(colleges, "Value", "Text");

            //var colleges = db.jntuh_college.OrderBy(a => a.id).ToList();
            //if (colleges != null)
            //{
            //    ViewBag.Colleges = colleges.Select(N => new SelectListItem { Text = N.collegeName, Value = N.id.ToString() });
            //}
            return View();
        }

        #region Report using linq query

        //public ActionResult DownloadReport1(ApprovedAdmittedIntakeModel intakeModel)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
        //    int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
        //    if (userCollegeID == 375)
        //    {
        //        userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
        //    }
        //    var gv = new GridView();

        //    int[] academicYearsArray = intakeModel.AcademicYears.Select(int.Parse).ToArray();
        //    //string[] collegesArray = intakeModel.Colleges.Select(string.Parse).ToArray();

        //    List<ApprovedAdmittedIntakeList> intakeList = new List<ApprovedAdmittedIntakeList>();

        //    intakeList = (from ai in db.jntuh_approvedadmitted_intake
        //                  join s in db.jntuh_specialization on ai.SpecializationId equals s.id
        //                  join d in db.jntuh_department on s.departmentId equals d.id
        //                  join dg in db.jntuh_degree on d.degreeId equals dg.id
        //                  join c in db.jntuh_college on ai.collegeId equals c.id
        //                  join a in db.jntuh_academic_year on ai.AcademicYearId equals a.id

        //                  where academicYearsArray.Contains(ai.AcademicYearId) && intakeModel.Colleges.Contains(c.collegeCode)

        //                  select new ApprovedAdmittedIntakeList
        //                  {
        //                      AcademicYearId = ai.AcademicYearId,
        //                      CollegeId = c.id,
        //                      DegreeId = d.degreeId,
        //                      SpecializationId = s.id,
        //                      AcademicYear = a.academicYear,//db.jntuh_academic_year.Where(ay => ay.id == ai.AcademicYearId).Select(ay => ay.academicYear).FirstOrDefault(),//a.academicYear,
        //                      CollegeCode = c.collegeCode,
        //                      CollegeName = c.collegeName,
        //                      Specialization = dg.degree + "-" + s.specializationName
        //                  }).OrderBy(a => a.AcademicYear).ThenBy(c => c.CollegeName).ThenBy(s => s.Specialization).ToList();

        //    intakeList = intakeList.AsEnumerable().GroupBy(i => new { i.CollegeId, i.SpecializationId }).Select(c => c.FirstOrDefault()).ToList();

        //    var list = intakeList.Select(x => new { x.AcademicYear, x.CollegeCode, x.CollegeName, x.Specialization }).ToList();

        //    gv.DataSource = list;
        //    gv.DataBind();
        //    Response.ClearContent();
        //    Response.Buffer = true;
        //    string reportName = "Approved and Admitted Intake List";
        //    Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
        //    Response.ContentType = "application/ms-excel";
        //    Response.Charset = "";
        //    StringWriter objStringWriter = new StringWriter();
        //    HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
        //    gv.RenderControl(objHtmlTextWriter);
        //    Response.Output.Write(objStringWriter.ToString());
        //    Response.Flush();
        //    Response.End();
        //    return View();
        //}

        #endregion Report using linq query

        public ActionResult DownloadReport(ApprovedAdmittedIntakeModel intakeModel)
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

            int[] academicYearsArray = intakeModel.AcademicYears.Select(int.Parse).ToArray();

            string query = string.Empty;

            query = "SELECT a.academicYear AS 'Academic Year',c.collegeCode AS 'College Code',c.collegeName AS 'College Name',concat(dg.degree,'-',s.specializationName) AS Specialization,";

            foreach (var item in academicYearsArray)
            {
                string academicYear = db.jntuh_academic_year.Where(a => a.id == item).Select(a => a.academicYear).FirstOrDefault();
                query += "MAX(CASE WHEN ai.AcademicYearId=" + item + " THEN ai.approvedIntake END) AS 'Approved Intake " + academicYear + "'";
                query += ",MAX(CASE WHEN ai.AcademicYearId=" + item + " THEN ai.AdmittedIntake END) AS 'Admitted Intake " + academicYear + "',";
            }


            query = query.Remove(query.Length - 1, 1);

            query += "FROM jntuh_approvedadmitted_intake ai JOIN jntuh_specialization s ON ai.SpecializationId=s.id JOIN jntuh_department d ON d.id=s.departmentId JOIN jntuh_degree dg ON dg.id=d.degreeId JOIN jntuh_college c ON ai.collegeId=c.id JOIN jntuh_academic_year a ON ai.AcademicYearId=a.id WHERE ai.`AcademicYearId` IN (";

            foreach (var item in academicYearsArray)
            {
                query += item + ",";
            }

            query = query.Remove(query.Length - 1, 1);

            query += ") AND c.collegecode IN (";

            foreach (var item in intakeModel.Colleges)
            {
                query += "'" + item + "'" + ",";
            }

            query = query.Remove(query.Length - 1, 1);

            query += ") GROUP BY ai.collegeId,ai.SpecializationId,a.academicYear ORDER BY a.academicYear,c.collegename,concat(dg.degree,'-',s.specializationName);";

            string connectionString = ConfigurationManager.ConnectionStrings["MySqlMembershipConnection"].ConnectionString;

            MySqlConnection connection = null;
            MySqlDataReader reader = null;
            DataTable table = new DataTable();

            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                dataAdapter.SelectCommand = new MySqlCommand(query, connection);
                dataAdapter.Fill(table);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection != null)
                    connection.Close();
            }

            gv.DataSource = table;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Approved and Admitted Intake List";
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

        //public List<T> GetApprovedAdmittedIntakeListData(ApprovedAdmittedIntakeModel intakeModel)
        //{
        //    //List<string> intakeList = new List<string>();

        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
        //    int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
        //    if (userCollegeID == 375)
        //    {
        //        userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
        //    }

        //    int[] academicYearsArray = intakeModel.AcademicYears.Select(int.Parse).ToArray();
        //    int[] collegesArray = intakeModel.Colleges.Select(int.Parse).ToArray();

        //    var intakeList = (from ai in db.jntuh_approvedadmitted_intake
        //                      join s in db.jntuh_specialization on ai.SpecializationId equals s.id
        //                      join d in db.jntuh_department on s.departmentId equals d.id
        //                      join dg in db.jntuh_degree on d.degreeId equals dg.id
        //                      join c in db.jntuh_college on ai.collegeId equals c.id

        //                      where academicYearsArray.Contains(ai.AcademicYearId) && collegesArray.Contains(c.id)

        //                      select new
        //                      {
        //                          c.collegeCode,
        //                          c.collegeName,
        //                          dg.degree,
        //                          s.specializationName
        //                      }).ToList();

        //    return "";
        //}
    }

    public class ApprovedAdmittedIntakeList
    {
        public int AcademicYearId { get; set; }
        public int CollegeId { get; set; }
        public int DegreeId { get; set; }
        public int SpecializationId { get; set; }
        public string AcademicYear { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Specialization { get; set; }
    }
}
