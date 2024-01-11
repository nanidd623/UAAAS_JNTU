using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FacultyReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private List<FacultyReport> FacultyReportDetails(FacultyReport FacultyReport)
        {
            List<FacultyReport> FacultyReportList = new List<FacultyReport>();
            FacultyReportList = (from Faculty in db.jntuh_college_faculty
                                 join College in db.jntuh_college on Faculty.collegeId equals College.id
                                 join Category in db.jntuh_faculty_category on Faculty.facultyCategoryId equals Category.id
                                 join Designation in db.jntuh_designation on Faculty.facultyDesignationId equals Designation.id
                                 join Department in db.jntuh_department on Faculty.facultyDepartmentId equals Department.id
                                 join Degree in db.jntuh_degree on Department.degreeId equals Degree.id
                                 join EditStatus in db.jntuh_college_edit_status on Faculty.collegeId equals EditStatus.collegeId
                                 select new FacultyReport
                                 {
                                     CollegeId=Faculty.collegeId,
                                     CollegeCode=College.collegeCode,
                                     CollegeName=College.collegeName,
                                     Name=Faculty.facultyFirstName+" "+Faculty.facultyLastName+" "+Faculty.facultySurname,
                                     Category=Category.facultyCategory,
                                     Designation=Designation.designation,
                                     Department=Degree.degree+"-"+Department.departmentName,
                                     Gender=Faculty.facultyGenderId == 1 ? "Male":"Female",
                                     Experience = Faculty.facultyPreviousExperience,
                                     PayScale=Faculty.facultyPayScale,
                                     UniversityRatified = Faculty.isFacultyRatifiedByJNTU == true ? "YES" : "NO",
                                     IsCollegeEditable=EditStatus.IsCollegeEditable,
                                     CollegeTypeId=College.collegeTypeID,
                                     DateOfAppointment=Faculty.facultyDateOfAppointment,
                                     GengerId=Faculty.facultyGenderId,
                                 }).Take(20000)
                                   .ToList();               
            
            FacultyReportList = FacultyReportList.OrderBy(f => f.CollegeId).ToList();
            if (FacultyReport.SelectedCollegeType != null)
            {
                FacultyReport.IsCollegeEditable = FacultyReport.SelectedCollegeType.Trim() == "PendingColleges" ? true : false;
                FacultyReport.GengerId = FacultyReport.SelectedGender.Trim() == "Male" ? 1 : 0;
                if (FacultyReport.SelectedCollegeType.Trim() == "--All Colleges--" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "--All Colleges--" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.CollegeTypeId == FacultyReport.CollegeTypeId &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.DistrictId == FacultyReport.DistrictId)
                                                 .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.CollegeTypeId == FacultyReport.CollegeTypeId)
                                                 .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.DistrictId == FacultyReport.DistrictId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.CollegeTypeId == FacultyReport.CollegeTypeId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.CollegeTypeId == FacultyReport.CollegeTypeId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.CollegeTypeId == FacultyReport.CollegeTypeId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId == 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.CollegeTypeId == FacultyReport.CollegeTypeId &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender == "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.CollegeTypeId==FacultyReport.CollegeTypeId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() != "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId == 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.IsCollegeEditable == FacultyReport.IsCollegeEditable &&
                                                                     c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
                else if (FacultyReport.SelectedCollegeType.Trim() == "All Colleges" && FacultyReport.DistrictId != 0 && FacultyReport.CollegeTypeId != 0 && FacultyReport.SelectedGender != "--Gender--")
                {
                    FacultyReportList = FacultyReportList.Where(c => c.DistrictId == FacultyReport.DistrictId &&
                                                                     c.CollegeTypeId==FacultyReport.CollegeTypeId &&
                                                                     c.GengerId == FacultyReport.GengerId)
                                                         .ToList();
                }
            }
            string[] SelectedCollegeTypes = { "All Colleges",
                                              "PendingColleges",
                                              "Submitted Colleges",
                                            };
            string[] SelectedGender = { "--Gender--",
                                        "Male",
                                        "Female",
                                      };
            ViewBag.SelectedGender = SelectedGender;
            ViewBag.Districts = db.jntuh_district
                                  .Where(district => district.isActive == true)
                                  .ToList();
            ViewBag.CollegeTypes = db.jntuh_college_type
                                  .Where(c => c.isActive == true)
                                  .ToList();
            ViewBag.SelectedCollegeTypes = SelectedCollegeTypes;
            ViewBag.FacultyReportDetails = FacultyReportList;
            ViewBag.Count = FacultyReportList.Count();
            return FacultyReportList;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult FacultyReport()
        {
            FacultyReport FacultyReport = new FacultyReport();
            FacultyReportDetails(FacultyReport);
            return View("~/Views/Reports/FacultyReport.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FacultyReport(FacultyReport FacultyReport, string cmd)
        {
            List<FacultyReport> FacultyReportList = FacultyReportDetails(FacultyReport);
            int Count = FacultyReportList.Count();
            if (cmd == "Export To Excel" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=CollegeSocietyReport.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_FacultyReport.cshtml");
            }
            else
            {
                return View("~/Views/Reports/FacultyReport.cshtml");
            }
        }
    }
}
