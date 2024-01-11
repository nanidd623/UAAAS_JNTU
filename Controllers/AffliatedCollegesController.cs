using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AffliatedCollegesController : BaseController
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
            ViewBag.AcademicYear = db.jntuh_academic_year.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DownloadReport(TotalAffiliatedColleges objModel)
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
            StringBuilder sb = new StringBuilder();
            sb = GetDegreeWiseData(objModel, ay0);
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Degree Wise Colleges Data";

            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " Data " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");
        }

        public StringBuilder GetDegreeWiseData(TotalAffiliatedColleges objModel, int presentAcademicYear)
        {
            List<int> engineeringlst = new List<int>();
            List<int> pharmacylst = new List<int>();
            List<int> standalonelst = new List<int>();

            int count = 0;
            int EngineeringAppliedColleges = 0;
            int EngineeringApprovedColleges = 0;

            int PharmacyAppliedColleges = 0;
            int PharmacyApprovedColleges = 0;

            int StandaloneAppliedColleges = 0;
            int StandaloneApprovedColleges = 0;

            StringBuilder html = new StringBuilder();

            #region Total Affiliated Colleges Table

            html.Append("<table border='1' rules='all'>");
            html.Append("<thead>");
            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {
                count = count + 1;
            }
            int firstTableColspan = (count * 2) + 2;
            int secondTableColspan = (count * 3) + 2;
            html.Append("<tr><th colspan=" + firstTableColspan + " style=background-color:blue;color:white>JNTUH-Total Affiliated Colleges</th></tr>");
            html.Append("<tr>");
            html.Append("<th>S.No</th>");
            html.Append("<th>Degree</th>");

            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {
                var academicYear = db.jntuh_academic_year.Where(a => a.id == i).Select(a => a.academicYear).FirstOrDefault();
                html.Append("<th>Applied Colleges (" + academicYear + ")</th>");
                html.Append("<th>Approved Colleges (" + academicYear + ")</th>");
            }
            html.Append("</tr>");
            html.Append("</thead>");

            #region Engineering

            html.Append("<tr>");
            html.Append("<td>1</td>");
            html.Append("<td>Engineering</td>");

            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {

                EngineeringAppliedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                              join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                              join d in db.jntuh_department on s.departmentId equals d.id
                                              join deg in db.jntuh_degree on d.degreeId equals deg.id
                                              join c in db.jntuh_college on ie.collegeId equals c.id
                                              join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                              where new[] { 1, 4 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i

                                              select ie.collegeId).Distinct().Count();

                html.Append("<td>" + EngineeringAppliedColleges + "</td>");
                engineeringlst.Add(EngineeringAppliedColleges);

                EngineeringApprovedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                               join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                               join d in db.jntuh_department on s.departmentId equals d.id
                                               join deg in db.jntuh_degree on d.degreeId equals deg.id
                                               join c in db.jntuh_college on ie.collegeId equals c.id
                                               join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                               where new[] { 1, 4 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i && ie.ApprovedIntake > 0

                                               select ie.collegeId).Distinct().Count();

                html.Append("<td>" + EngineeringApprovedColleges + "</td>");
                engineeringlst.Add(EngineeringApprovedColleges);
            }
            html.Append("</tr>");
            #endregion Engineering

            #region Pharmacy

            html.Append("<tr>");
            html.Append("<td>2</td>");
            html.Append("<td>Pharmacy</td>");

            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {
                PharmacyAppliedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                           join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                           join d in db.jntuh_department on s.departmentId equals d.id
                                           join deg in db.jntuh_degree on d.degreeId equals deg.id
                                           join c in db.jntuh_college on ie.collegeId equals c.id
                                           join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                           where new[] { 5, 2, 9, 10 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i

                                           select ie.collegeId).Distinct().Count();

                html.Append("<td>" + PharmacyAppliedColleges + "</td>");
                pharmacylst.Add(PharmacyAppliedColleges);

                PharmacyApprovedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                            join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                            join d in db.jntuh_department on s.departmentId equals d.id
                                            join deg in db.jntuh_degree on d.degreeId equals deg.id
                                            join c in db.jntuh_college on ie.collegeId equals c.id
                                            join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                            where new[] { 5, 2, 9, 10 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i && ie.ApprovedIntake > 0

                                            select ie.collegeId).Distinct().Count();

                html.Append("<td>" + PharmacyApprovedColleges + "</td>");
                pharmacylst.Add(PharmacyApprovedColleges);
            }
            html.Append("</tr>");
            #endregion Pharmacy

            #region Standalone MBA / MCA

            html.Append("<tr>");
            html.Append("<td>3</td>");
            html.Append("<td>Standalone MBA / MCA</td>");

            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {
                StandaloneAppliedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                             join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                             join d in db.jntuh_department on s.departmentId equals d.id
                                             join deg in db.jntuh_degree on d.degreeId equals deg.id
                                             join c in db.jntuh_college on ie.collegeId equals c.id
                                             join es in db.jntuh_college_edit_status on c.id equals es.collegeId
                                             join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id

                                             where new[] { 3, 6, 7 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i && ct.id == 3

                                             select ie.collegeId).Distinct().Count();

                html.Append("<td>" + StandaloneAppliedColleges + "</td>");
                standalonelst.Add(StandaloneAppliedColleges);

                StandaloneApprovedColleges = (from ie in db.jntuh_approvedadmitted_intake
                                              join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                              join d in db.jntuh_department on s.departmentId equals d.id
                                              join deg in db.jntuh_degree on d.degreeId equals deg.id
                                              join c in db.jntuh_college on ie.collegeId equals c.id
                                              join es in db.jntuh_college_edit_status on c.id equals es.collegeId
                                              join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id

                                              where new[] { 3, 6, 7 }.Contains(deg.id) && ie.collegeId != 375 && es.academicyearId == i && ie.AcademicYearId == i && ie.ApprovedIntake > 0 && ct.id == 3

                                              select ie.collegeId).Distinct().Count();

                html.Append("<td>" + StandaloneApprovedColleges + "</td>");
                standalonelst.Add(StandaloneApprovedColleges);
            }
            html.Append("</tr>");

            #endregion Standalone MBA / MCA

            #region Total

            html.Append("<tr><td colspan=2 style=text-align:center><b>Total</b></td>");
            for (int i = 0; i < engineeringlst.Count; i++)
            {
                var cnengt = engineeringlst[i] + (pharmacylst.Count > 0 ? pharmacylst[i] : 0) + (standalonelst.Count > 0 ? standalonelst[i] : 0);
                html.Append("<td><b>" + cnengt + "</b></td>");
            }
            html.Append("</tr>");

            #endregion Total

            html.Append("</table>");

            #endregion Total Affiliated Colleges Table

            html.Append("<br></br></br></br><br></br><br></br><br></br>");

            #region Total Colleges and Seats Table

            html.Append("<table border='1' rules='all'>");
            html.Append("<thead>");
            html.Append("<tr><th colspan=" + secondTableColspan + " style=background-color:blue;color:white>Total Colleges and Seats</th></tr>");
            html.Append("<tr>");
            html.Append("<th>S.No</th>");
            html.Append("<th>Program</th>");


            for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
            {
                var academicYear = db.jntuh_academic_year.Where(a => a.id == i).Select(a => a.academicYear).FirstOrDefault();
                html.Append("<th>Total Colleges (" + academicYear + ")</th>");
                html.Append("<th>Total Courses (" + academicYear + ")</th>");
                html.Append("<th>Total Seats (" + academicYear + ")</th>");
            }
            html.Append("</tr>");
            html.Append("</thead>");

            var degreesList = db.jntuh_degree.OrderBy(d => d.degree).ToList();

            int sno = 1;
            int TotalColleges = 0;
            int TotalCourses = 0;
            int? TotalSeats = 0;

            List<int> integrated = new List<int>();
            List<int> bpharm = new List<int>();
            List<int> btech = new List<int>();
            List<int> mpharm = new List<int>();
            List<int> mtech = new List<int>();
            List<int> mba = new List<int>();
            List<int> mca = new List<int>();
            List<int> mtm = new List<int>();
            List<int> pharmd = new List<int>();
            List<int> pharmdpb = new List<int>();

            foreach (var item in degreesList)
            {
                html.Append("<tr>");
                html.Append("<td>" + sno++ + "</td>");
                html.Append("<td>" + item.degree + "</td>");

                for (int i = objModel.AcademicYearFrom; i <= objModel.AcademicYearTo; i++)
                {
                    TotalColleges = (from ie in db.jntuh_approvedadmitted_intake
                                     join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                     join d in db.jntuh_department on s.departmentId equals d.id
                                     join deg in db.jntuh_degree on d.degreeId equals deg.id
                                     join c in db.jntuh_college on ie.collegeId equals c.id
                                     join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                     where deg.id == item.id && ie.collegeId != 375 && es.academicyearId == i &&
                                     ie.AcademicYearId == i && ie.ApprovedIntake != 0

                                     select ie.collegeId).Distinct().Count();

                    TotalCourses = (from ie in db.jntuh_approvedadmitted_intake
                                    join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                    join d in db.jntuh_department on s.departmentId equals d.id
                                    join deg in db.jntuh_degree on d.degreeId equals deg.id
                                    join c in db.jntuh_college on ie.collegeId equals c.id
                                    join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                    where deg.id == item.id && ie.collegeId != 375 && es.academicyearId == i &&
                                    ie.AcademicYearId == i && ie.ApprovedIntake != 0

                                    select ie.SpecializationId).Count();
                    if (presentAcademicYear == i)
                    {
                        var proposedTotalSeats = (from ie in db.jntuh_college_intake_existing
                                                  join s in db.jntuh_specialization on ie.specializationId equals s.id
                                                  join d in db.jntuh_department on s.departmentId equals d.id
                                                  join deg in db.jntuh_degree on d.degreeId equals deg.id
                                                  join c in db.jntuh_college on ie.collegeId equals c.id
                                                  join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                                  where deg.id == item.id && ie.collegeId != 375 && es.academicyearId == i &&
                                                  ie.academicYearId == i

                                                  select ie.proposedIntake).ToList();

                        TotalSeats = proposedTotalSeats.Sum();
                    }
                    else
                    {
                        var approvedTotalSeats = (from ie in db.jntuh_approvedadmitted_intake
                                                  join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                                  join d in db.jntuh_department on s.departmentId equals d.id
                                                  join deg in db.jntuh_degree on d.degreeId equals deg.id
                                                  join c in db.jntuh_college on ie.collegeId equals c.id
                                                  join es in db.jntuh_college_edit_status on c.id equals es.collegeId

                                                  where deg.id == item.id && ie.collegeId != 375 && es.academicyearId == i &&
                                                  ie.AcademicYearId == i && ie.ApprovedIntake != 0

                                                  select ie.ApprovedIntake).ToList();

                        TotalSeats = approvedTotalSeats.Sum();
                    }

                    html.Append("<td>" + TotalColleges + "</td>");
                    html.Append("<td>" + TotalCourses + "</td>");
                    html.Append("<td>" + TotalSeats + "</td>");

                    switch (item.id)
                    {
                        case 1:
                            mtech.Add(TotalColleges);
                            mtech.Add(TotalCourses);
                            mtech.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 2:
                            mpharm.Add(TotalColleges);
                            mpharm.Add(TotalCourses);
                            mpharm.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 3:
                            mca.Add(TotalColleges);
                            mca.Add(TotalCourses);
                            mca.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 4:
                            btech.Add(TotalColleges);
                            btech.Add(TotalCourses);
                            btech.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 5:
                            bpharm.Add(TotalColleges);
                            bpharm.Add(TotalCourses);
                            bpharm.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 6:
                            mba.Add(TotalColleges);
                            mba.Add(TotalCourses);
                            mba.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 7:
                            integrated.Add(TotalColleges);
                            integrated.Add(TotalCourses);
                            integrated.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 8:
                            mtm.Add(TotalColleges);
                            mtm.Add(TotalCourses);
                            mtm.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 9:
                            pharmd.Add(TotalColleges);
                            pharmd.Add(TotalCourses);
                            pharmd.Add(Convert.ToInt32(TotalSeats));
                            break;
                        case 10:
                            pharmdpb.Add(TotalColleges);
                            pharmdpb.Add(TotalCourses);
                            pharmdpb.Add(Convert.ToInt32(TotalSeats));
                            break;
                    }
                }
                html.Append("</tr>");
            }

            html.Append("<tr><td colspan=2 style=text-align:center><b>Total</b></td>");
            for (int i = 0; i < mtech.Count; i++)
            {
                var cnengt = mtech[i] + (mpharm.Count > 0 ? mpharm[i] : 0) + (mca.Count > 0 ? mca[i] : 0) + (btech.Count > 0 ? btech[i] : 0) + (bpharm.Count > 0 ? bpharm[i] : 0) + (mba.Count > 0 ? mba[i] : 0) + (integrated.Count > 0 ? integrated[i] : 0) + (mtm.Count > 0 ? mtm[i] : 0) + (pharmd.Count > 0 ? pharmd[i] : 0) + (pharmdpb.Count > 0 ? pharmdpb[i] : 0);
                html.Append("<td><b>" + cnengt + "</b></td>");
            }
            html.Append("</tr>");
            html.Append("</table>");

            #endregion Total Colleges and Seats Table

            return html;
        }
    }
}
