using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CounsellingReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int academicYearId = 0;
        private int AcademicYear = 0;
        private int nextAcademicYearId = 0;
        private int[] DegreeIds;
        private int[] CollegeIds;
        private int ClosureCourseId = 0;
        private int[] SubmitteCollegesId;
        private string ReportHeader = null;
        private string[] degrees;
        int InspectionPhaseId = 0;
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CounsellingReport(FormCollection frmc)
        {
            GetIds();
            List<CounsellingReport> CounsellingList = CounsellingReportList(null, null);
            return View("~/Views/Reports/CounsellingReport.cshtml");
        }
        private void GetIds()
        {
            //Get Colsure Course Id
            ClosureCourseId = db.jntuh_course_affiliation_status
                                .Where(c => c.courseAffiliationStatusCode.Trim() == "C")
                                .Select(c => c.id)
                                .FirstOrDefault();
            //Get present academic Year Id
            academicYearId = db.jntuh_academic_year
                                   .Where(year => year.isActive == true &&
                                                  year.isPresentAcademicYear == true)
                                   .Select(year => year.id)
                                   .FirstOrDefault();
            //Get Present academic Year
            AcademicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                year.id == academicYearId)
                                                 .Select(year => year.actualYear)
                                                 .FirstOrDefault();

            //Get next academic year
            nextAcademicYearId = db.jntuh_academic_year
                                   .Where(year => year.actualYear == (AcademicYear + 1))
                                   .Select(year => year.id)
                                   .FirstOrDefault();

            InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().OrderByDescending(p => p.id).ToList();

            //DEO Submitted colleges Ids
            SubmitteCollegesId = db.jntuh_dataentry_allotment
                //.Where(d => d.isCompleted == true)
                                   .Where(d => d.isCompleted == true && d.InspectionPhaseId == InspectionPhaseId) // && d.updatedOn >= now
                                   .Select(d => d.collegeID)
                                   .ToArray();

            //colleges Ids
            CollegeIds = db.jntuh_college
                         .Where(c => c.isActive == true  &&
                                     SubmitteCollegesId.Contains(c.id))
                         .Select(c => c.id)
                         .ToArray();
            //To fill college dropdown list
            ViewBag.Colleges = db.jntuh_college
                                 .Where(college => college.isActive == true &&
                                                   SubmitteCollegesId.Contains(college.id))
                                 .Select(college => new
                                 {
                                     ID = college.id,
                                     CollegeName = college.collegeCode + " - " + college.collegeName
                                 }).ToList();

            //To get all degrees
            degrees = db.jntuh_degree.Where(d => d.isActive == true).Select(d => d.degree).ToArray();

        }

        private List<CounsellingReport> CounsellingReportList(int? CollegeId, string cmd)
        {

            if (cmd == "ALL" || cmd == "Export ALL")
            {
                DegreeIds = db.jntuh_degree.Where(d => d.isActive == true).Select(d => d.id).ToArray();
                ReportHeader = "ALL.xls";

                ViewBag.Name = cmd;
            }
            else
            {
                if (cmd == "B.Tech/B.Pharm" || cmd == "Export B.Tech/B.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech" || d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Tech/B.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Tech/M.Pharm" || cmd == "Export M.Tech/M.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Tech" || d.degree == "M.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Tech/M.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MBA/MCA" || cmd == "Export MBA/MCA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MBA" || d.degree == "MCA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MBA/MCA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "Pharm.D/Pharm.D PB" || cmd == "Export Pharm.D/Pharm.D PB")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D" || d.degree == "Pharm.D PB")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D/Pharm.D PB.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "MAM/MTM" || cmd == "Export MAM/MTM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM" || d.degree == "MTM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MAM/MTM.xls";
                    ViewBag.Name = cmd;
                }


                //------------------
                else if (cmd == "B.Tech" || cmd == "Export B.Tech")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Tech.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "B.Pharm" || cmd == "Export B.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Tech" || cmd == "Export M.Tech")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Tech")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Tech.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Pharm" || cmd == "Export M.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MBA" || cmd == "Export MBA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MBA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MBA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MCA" || cmd == "Export MCA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MCA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MCA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "Pharm.D" || cmd == "Export Pharm.D")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "Pharm.D PB" || cmd == "Export Pharm.D PB")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D PB")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D PB.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "MAM" || cmd == "Export MAM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MAM.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MTM" || cmd == "Export MTM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MTM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MTM.xls";
                    ViewBag.Name = cmd;
                }
                //------------------
                else
                {
                    ViewBag.Name = "MAM/MTM";
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM" || d.degree == "MTM")).Select(d => d.degree).ToArray();
                    //degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech" || d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();           
                }
                DegreeIds = db.jntuh_degree.Where(d => d.isActive == true && degrees.Contains(d.degree)).Select(d => d.id).ToArray();
            }

            List<CounsellingReport> CounsellingReportList = new List<CounsellingReport>();
            if (CollegeId == null)
            {
                CounsellingReportList = (from ci in db.jntuh_college_intake_proposed
                                         join s in db.jntuh_specialization on ci.specializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on ci.collegeId equals c.id
                                         where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && CollegeIds.Contains(c.id) && ci.academicYearId == nextAcademicYearId && DegreeIds.Contains(d.id))//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                         
                                         orderby d.degreeDisplayOrder
                                         select new CounsellingReport
                                         {
                                             CollegeId = ci.collegeId,
                                             CollegeCode = c.collegeCode,
                                             CollegeName = c.collegeName
                                         }).Distinct().ToList();


            }
            else
            {
                CounsellingReportList = (from ci in db.jntuh_college_intake_proposed
                                         join s in db.jntuh_specialization on ci.specializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on ci.collegeId equals c.id
                                         where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && c.id == CollegeId && ci.academicYearId == nextAcademicYearId && DegreeIds.Contains(d.id))//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                        
                                         orderby d.degreeDisplayOrder
                                         select new CounsellingReport
                                         {
                                             CollegeId = ci.collegeId,
                                             CollegeCode = c.collegeCode,
                                             CollegeName = c.collegeName
                                         }).Distinct().ToList();


            }
            foreach (var item in CounsellingReportList)
            {
                //Binding collegeCode CollegeName and specializationDetails Data to table
                item.CollegeSpecializations = GetDegreewiseCollegeSpecializations(item.CollegeId);
                item.CollegeAddress = CollegeAddress(item.CollegeId);
                item.Establishyear = CollegeEstablishYear(item.CollegeId);
                item.Grade = CollegeGrade(item.CollegeId);
            }
            ViewBag.CollegeSpecializations = CounsellingReportList.OrderBy(c => c.CollegeName).ToList(); ;
            //ViewBag.CollegeSpecializations = CounsellingReportList;
            ViewBag.Count = CounsellingReportList.Count();
            return CounsellingReportList;
        }
        private List<DegreewiseCollegeSpecializations> GetDegreewiseCollegeSpecializations(int CollegeId)
        {
            List<DegreewiseCollegeSpecializations> DegreewiseCollegeSpecializationsList = new List<DegreewiseCollegeSpecializations>();
            List<Specializations> SpecializationList = (from ci in db.jntuh_college_intake_proposed
                                                        join s in db.jntuh_specialization on ci.specializationId equals s.id
                                                        join de in db.jntuh_department on s.departmentId equals de.id
                                                        join d in db.jntuh_degree on de.degreeId equals d.id
                                                        where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && ci.collegeId == CollegeId && ci.academicYearId == nextAcademicYearId)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                                        
                                                        orderby d.degreeDisplayOrder
                                                        select new Specializations
                                                        {
                                                            CollegeId = ci.collegeId,
                                                            ShiftId = ci.shiftId,
                                                            ProposedIntake = ci.proposedIntake,
                                                            CourseAffiliationStatusCodeId = ci.courseAffiliationStatusCodeId,
                                                            SpecializationId = ci.specializationId,
                                                            DepartmentId = de.id,
                                                            DegreeId = d.id
                                                        }).ToList();

            DegreeIds = db.jntuh_degree.Where(d => d.isActive == true && degrees.Contains(d.degree)).Select(d => d.id).ToArray();
            SpecializationList = SpecializationList.Where(d => DegreeIds.Contains(d.DegreeId)).ToList();
            foreach (var item in SpecializationList)
            {
                string NewSpecialization = string.Empty;
                string NewIntake = string.Empty;
                DegreewiseCollegeSpecializations DegreewiseCollegeSpecializations = new DegreewiseCollegeSpecializations();
                item.DegreeTypeId = db.jntuh_degree.Where(degree => degree.id == item.DegreeId).Select(degree => degree.degreeTypeId).FirstOrDefault();
                string degreeType = db.jntuh_degree_type.Where(degree => degree.id == item.DegreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();
                string degreeName = db.jntuh_degree.Where(degree => degree.id == item.DegreeId && degree.isActive == true).Select(degree => degree.degree).FirstOrDefault();
                string specializationName = db.jntuh_specialization.Where(specialization => specialization.id == item.SpecializationId)
                                                      .Select(specialization => specialization.specializationName).FirstOrDefault();
                if (degreeType != null)
                {
                    if (degreeType.ToUpper().Trim() == "PG" || degreeType.ToUpper().Trim() == "UG")
                    {
                        if (degreeName == "MBA" || degreeName == "MCA")
                        {
                            NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
                        }
                        else
                        {
                            NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? degreeName + "-" + specializationName : degreeName + "-" + specializationName + "-" + "(II Shift)";
                        }
                    }
                    else
                    {
                        NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                        NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";

                    }
                    DegreewiseCollegeSpecializations.Specialization = NewSpecialization;
                    DegreewiseCollegeSpecializations.Intake = NewIntake;
                    DegreewiseCollegeSpecializationsList.Add(DegreewiseCollegeSpecializations);
                }
            }
            return DegreewiseCollegeSpecializationsList;
        }
        private string GetIntake(int DegreeId, int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake)
        {
            string intake = string.Empty;
            string courseAffiliationStatusCode = string.Empty;
            courseAffiliationStatusCode = (db.jntuh_course_affiliation_status.Where(status => status.id == courseAffiliationStatusCodeId).Select(status => status.courseAffiliationStatusCode).FirstOrDefault()).ToUpper().Trim();
            if (proposedIntake != 0)
            {
                if (courseAffiliationStatusCode == "S")
                {
                    intake = proposedIntake.ToString() + "$";
                }
                else if (courseAffiliationStatusCode == "N")
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else
                {
                    intake = proposedIntake.ToString() + "*";
                }
            }
            else
            {
                intake = proposedIntake.ToString() + "*";
            }
            return intake;
        }
        private string CollegeAddress(int CollegeId)
        {
            string Address = string.Empty;
            string District = string.Empty;
            jntuh_address CollegeAddress = db.jntuh_address
                                           .Where(a => a.addressTye == "COLLEGE" &&
                                                       a.collegeId == CollegeId)
                                           .FirstOrDefault();
            if (CollegeAddress.townOrCity != null || CollegeAddress.townOrCity != string.Empty)
            {
                Address += CollegeAddress.townOrCity + ",";
            }
            if (CollegeAddress.mandal != null || CollegeAddress.mandal != string.Empty)
            {
                Address += CollegeAddress.mandal + ",";
            }
            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district
                             .Where(d => d.id == CollegeAddress.districtId)
                             .Select(d => d.districtName)
                             .FirstOrDefault();
                Address += District + ",";
            }
            if (CollegeAddress.pincode != 0)
            {
                Address += CollegeAddress.pincode.ToString();
            }
            return Address;
        }
        private string CollegeEstablishYear(int CollegeId)
        {
            string Year = string.Empty;
            int EstablishYear = db.jntuh_college_establishment
                                  .Where(e => e.collegeId == CollegeId)
                                  .Select(e => e.instituteEstablishedYear)
                                  .FirstOrDefault();
            if (EstablishYear != 0)
            {
                Year = EstablishYear.ToString();
            }
            return Year;
        }
        private string CollegeGrade(int CollegeId)
        {
            string Grade = string.Empty;
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            string CollegeGrade = db.college_grade
                                  .Where(e => e.collegeId == CollegeId && e.inspectionPhaseId == InspectionPhaseId)
                                  .Select(e => e.grade)
                                  .FirstOrDefault();
            if (!string.IsNullOrEmpty(CollegeGrade))
            {
                Grade = CollegeGrade;
            }
            return Grade;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult CounsellingReport(CounsellingReport collegeSpecializationsDetails, string cmd)
        {
            List<CounsellingReport> CounsellingList = new List<CounsellingReport>();
            int CollegeId = collegeSpecializationsDetails.CollegeId;
            GetIds();
            if (collegeSpecializationsDetails.InspectionPhaseId != 0)
            {
                InspectionPhaseId = collegeSpecializationsDetails.InspectionPhaseId;
                //DEO Submitted colleges Ids
                SubmitteCollegesId = db.jntuh_dataentry_allotment
                    //.Where(d => d.isCompleted == true)
                                       .Where(d => d.isCompleted == true && d.InspectionPhaseId == InspectionPhaseId) // && d.updatedOn >= now
                                       .Select(d => d.collegeID)
                                       .ToArray();

                //colleges Ids
                CollegeIds = db.jntuh_college
                             .Where(c => c.isActive == true &&
                                         SubmitteCollegesId.Contains(c.id))
                             .Select(c => c.id)
                             .ToArray();
                //To fill college dropdown list
                ViewBag.Colleges = db.jntuh_college
                                     .Where(college => college.isActive == true &&
                                                       SubmitteCollegesId.Contains(college.id))
                                     .Select(college => new
                                     {
                                         ID = college.id,
                                         CollegeName = college.collegeCode + " - " + college.collegeName
                                     }).ToList();
            }
            if (CollegeId == 0)
            {
                //GetIds();
                CounsellingList = CounsellingReportList(null, cmd);
            }
            else
            {
                //GetIds();
                CounsellingList = CounsellingReportList(CollegeId, cmd);
            }
           
            ViewBag.CollegeSpecializations = CounsellingList.OrderBy(c => c.CollegeName).ToList();
            //ViewBag.CollegeSpecializations = CounsellingList;
            ViewBag.Count = CounsellingList.Count();
            int Count = CounsellingList.Count();

            if ((cmd == "Export B.Tech" || cmd == "Export B.Pharm" || cmd == "Export B.Tech/B.Pharm" || cmd == "Export M.Tech" || cmd == "Export M.Pharm" || cmd == "Export M.Tech/M.Pharm" || cmd == "Export MBA" || cmd == "Export MCA" || cmd == "Export MBA/MCA" || cmd == "Export Pharm.D" || cmd == "Export Pharm.D PB" || cmd == "Export Pharm.D/Pharm.D PB" || cmd == "Export MAM" || cmd == "Export MTM" || cmd == "Export MAM/MTM") && Count != 0 || cmd == "Export ALL" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_CounsellingReport.cshtml");
            }
            else
            {
                return View("~/Views/Reports/CounsellingReport.cshtml");
            }
        }

    }
}
