using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class PrincipalSCMController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Accounts")]
        public ActionResult Index()
        {
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            List<ScmPrinicipal> scmPrincipalList = new List<ScmPrinicipal>();
            scmPrincipalList = Getscmprinicpaldetails(prAy);

            return View(scmPrincipalList);
        }

        private List<ScmPrinicipal> Getscmprinicpaldetails(int prAy)
        {
            var lstprinipals = new List<ScmPrinicipal>();
            var plinkdetails =
                db.jntuh_college_links_assigned.Where(
                    a => a.academicyearId == prAy && a.isActive == true && a.linkId == 2)
                    .Select(s => s)
                    .FirstOrDefault();
            if (plinkdetails != null)
            {
                var fromdate = Convert.ToDateTime(plinkdetails.fromdate);
                var scmproceedingsrequests =
                    db.jntuh_scmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId == 0 && s.SpecializationId == 0)
                        .Select(s => s)
                        .ToList();
                var scmproceedingsfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.CreatedOn >= fromdate)
                        .Select(s => s)
                        .ToList();
                var scmids = scmproceedingsrequests.Where(s => s.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                var scmprinicipalregnos = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).Select(i => i.RegistrationNumber).ToArray();
                var scmprinicipals = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).OrderByDescending(s => s.CreatedOn).ToList();
                var colleges = db.jntuh_college.Where(i => i.isActive).ToList();
                var deparments = db.jntuh_department.ToList();
                var regfaculty = db.jntuh_registered_faculty.Where(i => scmprinicipalregnos.Contains(i.RegistrationNumber)).ToList();
                foreach (var prinip in scmprinicipals)
                {
                    var clg = colleges.FirstOrDefault(i => i.id == prinip.CollegeId);
                    var faculty = regfaculty.FirstOrDefault(i => i.RegistrationNumber == prinip.RegistrationNumber);
                    var jntuhDepartment = deparments.FirstOrDefault(i => faculty != null && i.id == faculty.DepartmentId);

                    lstprinipals.Add(new ScmPrinicipal
                    {
                        RegistrationNo = prinip.RegistrationNumber,
                        CollegeCode = clg != null ? clg.collegeCode : "",
                        CollegeName = clg != null ? clg.collegeName : "",
                        FacultyName = faculty != null ? (faculty.FirstName + faculty.MiddleName + faculty.LastName) : "",
                        Department = jntuhDepartment != null ? jntuhDepartment.departmentName : "",
                        CreatedDate = prinip.CreatedOn.ToString("dd/MM/yyyy hh:mm")
                    });
                }
            }
            return lstprinipals;
        }

        public ActionResult PrincipalSCMListExcel()
        {
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var gv = new GridView();
            gv.DataSource = Getscmprinicpaldetails(prAy);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Principal SCM List";
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

        public class ScmPrinicipal
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Department { get; set; }
            public string RegistrationNo { get; set; }
            public string FacultyName { get; set; }
            public string CreatedDate { get; set; }
        }

    }
}
