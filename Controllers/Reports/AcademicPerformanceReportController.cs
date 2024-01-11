using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class AcademicPerformanceReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult AcademicPerformanceReport()
        {
            AcademicPerformanceReport academicPerformanceReport = new AcademicPerformanceReport();
            AcademicPerformanceDetails(academicPerformanceReport);

            return View("~/Views/Reports/AcademicPerformanceReport.cshtml", academicPerformanceReport);
        }
        public class CollegeEditStatus
        {
            public int collegestatusId { get; set; }
            public string name { get; set; }
        }
        [HttpPost]
        public ActionResult AcademicPerformanceReport(AcademicPerformanceReport academicPerformance, string cmd)
        {
            AcademicPerformanceReport academicPerformanceReport = new AcademicPerformanceReport();

            List<AcademicPerformanceReport> academicPerformanceList = AcademicPerformanceDetails(academicPerformance);
            int count = academicPerformanceList.Count();
            if (cmd == "Export" && count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=AcademicPerformance Report.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AcademicPerformanceReport.cshtml", academicPerformanceReport);

            }
            if (count == 0)
            {
                TempData["Error"] = "No records found.";
            }
            return View("~/Views/Reports/AcademicPerformanceReport.cshtml", academicPerformanceReport);

        }

        private List<AcademicPerformanceReport> AcademicPerformanceDetails(AcademicPerformanceReport academicPerformance)
        {
            int districtId = academicPerformance.districtId;
            int collegeTypeId = academicPerformance.collegeTypeId;
            int collegeEditStatus = academicPerformance.collegestatusId;
            bool collegeStatus = false;
            if (collegeEditStatus != 0)
            {
                if (collegeEditStatus == 1)
                    collegeStatus = true;
                else
                    collegeStatus = false;
            }
            List<AcademicPerformanceReport> academicPerformanceList = new List<AcademicPerformanceReport>();
            academicPerformanceList = (from acp in db.jntuh_college_academic_performance
                                       join acy in db.jntuh_academic_year on acp.academicYearId equals acy.id
                                       join c in db.jntuh_college on acp.collegeId equals c.id
                                       join a in db.jntuh_address on c.id equals a.collegeId
                                       join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id
                                       join cs in db.jntuh_college_edit_status on c.id equals cs.collegeId
                                       join s in db.jntuh_specialization on acp.specializationId equals s.id
                                       where (acy.isActive == true && c.isActive == true && s.isActive == true && ct.isActive == true && a.addressTye == "College")
                                       group acp by new
                                       {
                                           collegeid = acp.collegeId,
                                           districtId = a.districtId,
                                           collegeTypeId = c.collegeTypeID,
                                           collegeCode = c.collegeCode,
                                           collegeName = c.collegeName,
                                           specializationId = acp.specializationId,
                                           specializationName = s.specializationName,
                                           academicyear = acy.academicYear
                                       } into g
                                       select new AcademicPerformanceReport
                                       {
                                           collegeid = g.Key.collegeid,
                                           districtId = g.Key.districtId,
                                           collegeTypeId = g.Key.collegeTypeId,
                                           collegeCode = g.Key.collegeCode,
                                           collegeName = g.Key.collegeName,
                                           specializationId = g.Key.specializationId,
                                           specializationName = g.Key.specializationName,
                                           academicyear = g.Key.academicyear,
                                           appearedstudents = g.Sum(p => p.appearedStudents),
                                           passedstudents = g.Sum(p => p.passedStudents),
                                           passPercentage = (g.Sum(p => p.passedStudents) / g.Sum(p => p.appearedStudents)) * 100
                                       }).ToList();
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId != 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId == 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId != 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId == 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.isCollegeEditable == collegeStatus).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId != 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId == 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId != 0)
            {
                academicPerformanceList = academicPerformanceList.Where(cl => cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId == 0)
            {
                academicPerformanceList = academicPerformanceList.ToList();
            }           
            ViewBag.academicPerformanceList = academicPerformanceList;

            List<CollegeEditStatus> collegeEditStatuslist = new List<CollegeEditStatus>(){                
                new CollegeEditStatus{ collegestatusId = 1, name = "Pending"},
                new CollegeEditStatus{ collegestatusId = 2, name = "Submitted"}};
            ViewBag.CollegeEditStatus = collegeEditStatuslist.ToList();
            ViewBag.Districts = db.jntuh_district.Where(d => d.isActive == true).Select(d => d).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.Where(cs => cs.isActive == true).Select(cs => cs).ToList();
            return academicPerformanceList;
        }
    }
}
