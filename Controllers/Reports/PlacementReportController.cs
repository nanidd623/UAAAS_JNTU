using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class PlacementReportController : BaseController
    {
     
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PlacementReport()
        {
            PlacementReport placementReport = new PlacementReport();
            PalcementDetails(placementReport);
            return View("~/Views/Reports/PlacementReport.cshtml", placementReport);
        }
        public class CollegeEditStatus
        {
            public int collegestatusId { get; set; }
            public string name { get; set; }
        }
        [HttpPost]
        public ActionResult PlacementReport(PlacementReport placement, string cmd)
        {
            PlacementReport placementReport = new PlacementReport();
            List<PlacementReport> placementList = PalcementDetails(placement);
            int count = placementList.Count();
            if (cmd == "Export" && count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Placement Report.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_PlacementReport.cshtml", placementReport);
            }
            if (count == 0)
            {
                TempData["Error"] = "No records found.";
            }
            return View("~/Views/Reports/PlacementReport.cshtml", placementReport);

        }

        private List<PlacementReport> PalcementDetails(PlacementReport placement)
        {
            int districtId = placement.districtId;
            int collegeTypeId = placement.collegeTypeId;
            int collegeEditStatus = placement.collegestatusId;
            bool collegeStatus = false;
            if (collegeEditStatus != 0)
            {
                if (collegeEditStatus == 1)
                    collegeStatus = true;
                else
                    collegeStatus = false;
            }            
            List<PlacementReport> placementList = new List<PlacementReport>();
            placementList = (from acp in db.jntuh_college_placement
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
                                 academicyear = acy.academicYear,
                             } into g
                             select new PlacementReport
                             {
                                 collegeid = g.Key.collegeid,
                                 districtId = g.Key.districtId,
                                 collegeTypeId = g.Key.collegeTypeId,
                                 collegeCode = g.Key.collegeCode,
                                 collegeName = g.Key.collegeName,
                                 specializationId = g.Key.specializationId,
                                 specializationName = g.Key.specializationName,
                                 academicyear = g.Key.academicyear,
                                 totalStudentsPassed = g.Sum(p => p.totalStudentsPassed),
                                 totalStudentsPlaced = g.Sum(p => p.totalStudentsPlaced),
                                 PlacedPercentage = (g.Sum(p => p.totalStudentsPlaced) / g.Sum(p => p.totalStudentsPassed)) * 100
                             }).ToList();

            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId != 0)
            {
                placementList = placementList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId == 0)
            {
                placementList = placementList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId != 0)
            {
                placementList = placementList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId == 0)
            {
                placementList = placementList.Where(cl => cl.isCollegeEditable == collegeStatus).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId != 0)
            {
                placementList = placementList.Where(cl => cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId == 0)
            {
                placementList = placementList.Where(cl => cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId != 0)
            {
                placementList = placementList.Where(cl => cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId == 0)
            {
                placementList = placementList.ToList();
            }
           
            ViewBag.placementList = placementList;

            List<CollegeEditStatus> collegeEditStatuslist = new List<CollegeEditStatus>(){                
                new CollegeEditStatus{ collegestatusId = 1, name = "Pending"},
                new CollegeEditStatus{ collegestatusId = 2, name = "Submitted"}};
            ViewBag.CollegeEditStatus = collegeEditStatuslist.ToList();
            ViewBag.Districts = db.jntuh_district.Where(d => d.isActive == true).Select(d => d).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.Where(cs => cs.isActive == true).Select(cs => cs).ToList();
            return placementList;
        }   

    }
}
