using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class InfrastructureReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]        
        public ActionResult InfrastructureReport()
        {
            InfrastructureReport infrastructureReport = new InfrastructureReport();
            InfrastructureDetails(infrastructureReport);
            return View("~/Views/Reports/InfrastructureReport.cshtml", infrastructureReport);
        }
        public class CollegeEditStatus
        {
            public int collegestatusId { get; set; }
            public string name { get; set; }
        }
        [HttpPost]
        public ActionResult InfrastructureReport(InfrastructureReport infrastructure1, string cmd)
        {
            InfrastructureReport infrastructureReport = new InfrastructureReport();            
            List<InfrastructureReport> infrastructureList = InfrastructureDetails(infrastructure1);
            int count = infrastructureList.Count();
            if (cmd == "Export" && count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Infrastructure Report.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_InfrastructureReport.cshtml", infrastructureReport);
            }
            if (count == 0)
            {
                TempData["Error"] = "No records found.";
            }           

            return View("~/Views/Reports/InfrastructureReport.cshtml", infrastructureReport);
        }

        private List<InfrastructureReport> InfrastructureDetails(InfrastructureReport infrastructure1)
        {
            int districtId = infrastructure1.districtId;
            int collegeTypeId = infrastructure1.collegeTypeId;
            int collegeEditStatus = infrastructure1.collegestatusId;
            bool collegeStatus = false;
            if (collegeEditStatus != 0)
            {
                if (collegeEditStatus == 1)
                    collegeStatus = true;
                else
                    collegeStatus = false;
            }            
            int[] ClassRomsIds = db.jntuh_area_requirement
                                   .Where(a => a.areaType == "INSTRUCTIONAL" && a.requirementType == "Class Rooms" && a.isActive == true)
                                   .Select(a => a.id)
                                   .ToArray();
            int[] LaboratorysIds = db.jntuh_area_requirement
                                  .Where(a => a.areaType == "INSTRUCTIONAL" && a.requirementType == "Laboratory" && a.isActive == true)
                                  .Select(a => a.id)
                                  .ToArray();

            var Infrastructure = (from c in db.jntuh_college
                                  join a in db.jntuh_address on c.id equals a.collegeId
                                  join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id
                                  join cs in db.jntuh_college_edit_status on c.id equals cs.collegeId
                                  where (c.isActive == true && ct.isActive == true && a.addressTye == "College")
                                  select new
                                  {
                                      c.id,
                                      c.collegeCode,
                                      c.collegeName,
                                      c.collegeTypeID,
                                      a.districtId

                                  }).ToList();
            List<InfrastructureReport> InfrastructureList = new List<Models.InfrastructureReport>();
            foreach (var item in Infrastructure)
            {
                InfrastructureReport infrastructure = new InfrastructureReport();
                infrastructure.collegeid = item.id;
                infrastructure.collegeCode = item.collegeCode;
                infrastructure.collegeName = item.collegeName;
                infrastructure.collegeTypeId = item.collegeTypeID;
                infrastructure.districtId = item.districtId;
                infrastructure.NoofClassrooms = db.jntuh_college_area
                                                    .Where(a => ClassRomsIds.Contains(a.areaRequirementId) && a.collegeId == item.id)
                                                    .Select(a => a.availableRooms).Sum();
                infrastructure.NoofLaboratorys = db.jntuh_college_area
                                                   .Where(a => LaboratorysIds.Contains(a.areaRequirementId) && a.collegeId == item.id)
                                                   .Select(a => a.availableRooms).Sum();

                InfrastructureList.Add(infrastructure);
            }
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId != 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId != 0 && collegeTypeId == 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId != 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.isCollegeEditable == collegeStatus && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus != 0 && districtId == 0 && collegeTypeId == 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.isCollegeEditable == collegeStatus).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId != 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.districtId == districtId && cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId != 0 && collegeTypeId == 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.districtId == districtId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId != 0)
            {
                InfrastructureList = InfrastructureList.Where(cl => cl.collegeTypeId == collegeTypeId).ToList();
            }
            if (collegeEditStatus == 0 && districtId == 0 && collegeTypeId == 0)
            {
                InfrastructureList = InfrastructureList.ToList();
            }
            ViewBag.InfrastructureList = InfrastructureList;

            List<CollegeEditStatus> collegeEditStatuslist = new List<CollegeEditStatus>(){                
                new CollegeEditStatus{ collegestatusId = 1, name = "Pending"},
                new CollegeEditStatus{ collegestatusId = 2, name = "Submitted"}};
            ViewBag.CollegeEditStatus = collegeEditStatuslist.ToList();
            ViewBag.Districts = db.jntuh_district.Where(d => d.isActive == true).Select(d => d).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.Where(cs => cs.isActive == true).Select(cs => cs).ToList();
            return InfrastructureList;
        }

    }
}
