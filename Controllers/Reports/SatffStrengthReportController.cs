using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SatffStrengthReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //public ActionResult SatffStrengthReport()
        //{
        //    SatffStrengthReport satffStrengthReport = new SatffStrengthReport();
        //    StaffStrengthReportDetails(satffStrengthReport);
        //    return View("~/Views/Reports/SatffStrengthReport.cshtml", satffStrengthReport);
        //}
        //[HttpPost]
        //public ActionResult SatffStrengthReport(SatffStrengthReport satffStrength,string cmd)
        //{
        //   SatffStrengthReport satffStrengthReport = new SatffStrengthReport();
        //   List<SatffStrengthReport> satffStrengthReportList = StaffStrengthReportDetails(satffStrength);
        //   int count = satffStrengthReportList.Count();
        //   if (cmd == "Export" && count != 0)
        //   {
        //       Response.ClearContent();
        //       Response.Buffer = true;
        //       Response.AddHeader("content-disposition", "attachment; filename=SatffStrength Report.xls");
        //       Response.ContentType = "application/vnd.ms-excel";
        //       return PartialView("~/Views/Reports/_SatffStrengthReport.cshtml", satffStrengthReport);
        //   }
        //   if (count == 0)
        //   {
        //       TempData["Error"] = "No records found.";
        //   }
        //    return View("~/Views/Reports/SatffStrengthReport.cshtml", satffStrengthReport);
        //}

        //private List<SatffStrengthReport> StaffStrengthReportDetails(SatffStrengthReport satffStrength)
        //{
        //    int districtId = satffStrength.districtId;
        //    int collegeTypeId = satffStrength.collegeTypeId;
        //    int collegeEditStatus = satffStrength.collegestatusId;
        //    bool collegeStatus = false;
        //    if (collegeEditStatus != 0)
        //    {
        //        if (collegeEditStatus == 1)
        //            collegeStatus = true;
        //        else
        //            collegeStatus = false;
        //    }
            
        //    List<SatffStrengthReport> satffStrengthReportList = new List<SatffStrengthReport>();
        //    satffStrengthReportList = (from t in db.jntuh_college_teaching_faculty_position
        //                               join c in db.jntuh_college on t.collegeId equals c.id
        //                               join a in db.jntuh_address on c.id equals a.collegeId
        //                               join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id
        //                               join cs in db.jntuh_college_edit_status on c.id equals cs.collegeId
        //                               join s in db.jntuh_specialization on t.specializationId equals s.id
        //                               join de in db.jntuh_department on s.departmentId equals de.id
        //                               join d in db.jntuh_degree on de.degreeId equals d.id
        //                               where (c.isActive == true && a.addressTye == "College" && ct.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true)
        //                               select new SatffStrengthReport
        //                               {
        //                                   collegeid = t.collegeId,
        //                                   collegeCode = c.collegeCode,
        //                                   collegeName = c.collegeName,
        //                                   districtId = a.districtId,
        //                                   isCollegeEditable = cs.IsCollegeEditable,
        //                                   collegeTypeId = c.collegeTypeID,
        //                                   degree = d.degree,
        //                                   department = de.departmentName,
        //                                   specialization = s.specializationName,
        //                                   professors = t.professors,
        //                                   assoProfessors = t.assocProfessors,
        //                                   assisProfessors = t.asstProfessors,
        //                                   intake = t.intake,
        //                                   ratio = t.facultyStudentRatio

        //                               }).ToList();
        //    if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId != 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
        //    }
        //    if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId == 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId).ToList();
        //    }
        //    if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId != 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.collegeTypeId == collegeTypeId).ToList();
        //    }
        //    if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId == 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.isCollegeEditable == collegeStatus).ToList();
        //    }
        //    if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId != 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
        //    }
        //    if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId == 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.districtId == districtId).ToList();
        //    }
        //    if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId != 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.Where(cl => cl.collegeTypeId == collegeTypeId).ToList();
        //    }
        //    if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId == 0)
        //    {
        //        satffStrengthReportList = satffStrengthReportList.ToList();
        //    }
        //    ViewBag.satffStrengthReportList = satffStrengthReportList;

        //    List<CollegeEditStatus> collegeEditStatuslist = new List<CollegeEditStatus>(){                
        //        new CollegeEditStatus{ collegestatusId = 1, name = "Pending"},
        //        new CollegeEditStatus{ collegestatusId = 2, name = "Submitted"}};
        //    ViewBag.CollegeEditStatus = collegeEditStatuslist.ToList();
        //    ViewBag.Districts = db.jntuh_district.Where(d => d.isActive == true).Select(d => d).ToList();
        //    ViewBag.CollegeType = db.jntuh_college_type.Where(cs => cs.isActive == true).Select(cs => cs).ToList();
        //    return satffStrengthReportList;
        //}
        public class CollegeEditStatus
        {
            public int collegestatusId { get; set; }
            public string name { get; set; }
        }
        

    }
}
