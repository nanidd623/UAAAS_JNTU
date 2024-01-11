using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class CounsellingReportProvisionalController : BaseController
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
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CounsellingReportNew(FormCollection frmc)
        {
            GetIds();
            List<CounsellingReport> CounsellingList = CounsellingReportList(null, null);

            return View("~/Views/Reports/CounsellingReportNew.cshtml");
        }
        private void GetIds()
        {

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

            //InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            //get old inspection phases
            //ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
            //                               join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
            //                               join a in db.jntuh_academic_year on p.academicYearId equals a.id
            //                               select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().OrderByDescending(p => p.id).ToList();

            //DEO Submitted colleges Ids
            SubmitteCollegesId = db.jntuh_dataentry_allotment
                //.Where(d => d.isCompleted == true)
                                   .Where(d => d.isCompleted == true)// && d.InspectionPhaseId == InspectionPhaseId) 
                                   .Select(d => d.collegeID)
                                   .ToArray();

            //colleges Ids
            CollegeIds = db.jntuh_college
                         .Where(c => c.isActive == true && c.isNew == false &&
                                     SubmitteCollegesId.Contains(c.id))
                         .Select(c => c.id)
                         .ToArray();

            CollegeIds = db.jntuh_college_news
                         .Where(n => n.title.Contains("Grant of Affiliation is available at your portal for download") && n.collegeId == 399)
                         .Select(c => c.collegeId)
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
                CounsellingReportList = (from ci in db.jntuh_college_intake_existing
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
                CounsellingReportList = (from ci in db.jntuh_college_intake_existing
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
                item.Grade = CollegeDistrict(item.CollegeId);
            }

            CounsellingReportList = CounsellingReportList.Where(c => c.CollegeSpecializations.Count() > 0).OrderBy(c => c.CollegeName).ToList();
            ViewBag.CollegeSpecializations = CounsellingReportList;
            //ViewBag.CollegeSpecializations = CounsellingReportList;
            ViewBag.Count = CounsellingReportList.Count();
            return CounsellingReportList;
        }
        private List<DegreewiseCollegeSpecializations> GetDegreewiseCollegeSpecializations(int CollegeId)
        {
            List<CollegeFacultyLabs> affiliatedCourses = CollegeCoursesAllClear(CollegeId);
            int[] specializationids = affiliatedCourses.Select(a => a.SpecializationId).ToArray();

            List<DegreewiseCollegeSpecializations> DegreewiseCollegeSpecializationsList = new List<DegreewiseCollegeSpecializations>();
            List<Specializations> SpecializationList = (from ci in db.jntuh_college_intake_existing
                                                        join s in db.jntuh_specialization on ci.specializationId equals s.id
                                                        join de in db.jntuh_department on s.departmentId equals de.id
                                                        join d in db.jntuh_degree on de.degreeId equals d.id
                                                        where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && ci.collegeId == CollegeId && ci.academicYearId == nextAcademicYearId)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                                        
                                                        orderby d.degreeDisplayOrder //specializationids.Contains(ci.specializationId) && 
                                                        select new Specializations
                                                        {
                                                            CollegeId = ci.collegeId,
                                                            ShiftId = ci.shiftId,
                                                            ProposedIntake = ci.approvedIntake == null ? 0 : (int)ci.approvedIntake,
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
                            //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewIntake = item.ProposedIntake.ToString();
                            NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
                        }
                        else
                        {
                            //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewIntake = item.ProposedIntake.ToString();
                            NewSpecialization = item.ShiftId == 1 ? degreeName + "-" + specializationName : degreeName + "-" + specializationName + "-" + "(II Shift)";
                        }
                    }
                    else
                    {
                        //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                        NewIntake = item.ProposedIntake.ToString();
                        NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";

                    }
                    DegreewiseCollegeSpecializations.Specialization = NewSpecialization;
                    DegreewiseCollegeSpecializations.Intake = NewIntake;

                    if (!specializationids.Contains(item.SpecializationId))
                    {
                        DegreewiseCollegeSpecializations.Provisional = "Provisional";
                    }

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
        private string CollegeDistrict(int CollegeId)
        {
            string District = string.Empty;
            jntuh_address CollegeAddress = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == CollegeId).FirstOrDefault();

            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district.Where(d => d.id == CollegeAddress.districtId).Select(d => d.districtName).FirstOrDefault();
            }

            return District;
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
        //[Authorize(Roles = "Admin")]
        public ActionResult CounsellingReportNew(CounsellingReport collegeSpecializationsDetails, string cmd)
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
                             .Where(c => c.isActive == true && c.isNew == false &&
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
                return PartialView("~/Views/Reports/_CounsellingReportProvisional.cshtml", CounsellingList);
            }
            else
            {
                return View("~/Views/Reports/CounsellingReportNew.cshtml");
            }
        }

        //affiliation validation
        public List<CollegeFacultyLabs> CollegeCoursesAllClear(int collegeId)
        {
            List<CollegeFacultyLabs> affiliatedCourses = new List<CollegeFacultyLabs>();

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int rowCount = 0; int assessmentCount = 0;

            List<CollegeFacultyLabs> faculty = DeficienciesInFaculty(collegeId);
            List<CollegeFacultyLabs> labs = DeficienciesInLabs(collegeId);

            List<CollegeFacultyLabs> clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            DegreeType = a.f.DegreeType,
                                            DegreeDisplayOrder = a.f.DegreeDisplayOrder,
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

            List<CollegeFacultyLabs> deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            DegreeType = a.f.DegreeType,
                                            DegreeDisplayOrder = a.f.DegreeDisplayOrder,
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

            int affiliatedCount = 0; int defiencyRows = 0;

            List<string> deficiencyDepartments = new List<string>();

            foreach (var course in deficiencyCourses)
            {
                if (!deficiencyDepartments.Contains(course.Department))
                {
                    deficiencyDepartments.Add(course.Department);
                }

                //FIVE percent relaxation for faculty only
                decimal percentage = 0;

                if (course.LabsDeficiency == "NIL")
                {
                    percentage = (course.Required * 10) / 100;
                }

                bool isCourseAffiliated = false;

                var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

                if (affiliation != null)
                {
                    if (affiliation.isAffiliated == true)
                    {
                        isCourseAffiliated = true;
                    }
                }

                if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL") || isCourseAffiliated != false)
                {
                    if (course.TotalIntake != 0)
                    {
                        affiliatedCount++; affiliatedCourses.Add(course);
                        rowCount++;
                    }
                }
                else if (isCourseAffiliated == false)
                {
                    defiencyRows++;
                    assessmentCount++;
                }
            }

            foreach (var course in clearedCourses)
            {
                string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                   .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                List<string> clearedDepartments = clearedCourses.Where(a => a.DegreeType == "UG").Select(a => a.Department).ToList();

                if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                {
                    defiencyRows++;
                    assessmentCount++;
                }
                else
                {
                    if (course.TotalIntake != 0)
                    {
                        affiliatedCount++; affiliatedCourses.Add(course);
                        rowCount++;
                    }
                }
            }

            return affiliatedCourses;
        }

        public List<CollegeFacultyLabs> DeficienciesInFaculty(int? collegeID)
        {
            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            var degrees = db.jntuh_degree.Select(t => t).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in facultyCounts)
            {
                //item.requiredFaculty = (item.requiredFaculty - item.requiredFaculty * 10 / 100);
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
                newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
                newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newFaculty.Degree = item.Degree;
                newFaculty.Department = item.Department;
                newFaculty.Specialization = item.Specialization;
                newFaculty.SpecializationId = item.specializationId;
                newFaculty.TotalIntake = item.approvedIntake1;

                if (departments.Contains(item.Department))
                {
                    //newFaculty.TotalIntake = totalBtechFirstYearIntake;
                    newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
                    newFaculty.Available = item.totalFaculty;
                }
                else
                {
                    //newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                    newFaculty.Available = adjustedFaculty;
                }

                newFaculty.Deficiency = minimumRequirementMet;

                if (adjustedPHDFaculty > 0)
                {
                    newFaculty.PhdDeficiency = "NO";
                }
                else if (degreeType.Equals("PG"))
                {
                    newFaculty.PhdDeficiency = "YES";
                }
                else
                {
                    newFaculty.PhdDeficiency = "-";
                }

                lstFaculty.Add(newFaculty);
                deptloop++;
            }

            return lstFaculty;
        }

        public List<CollegeFacultyLabs> DeficienciesInLabs(int? collegeID)
        {
            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            var labMaster = db.jntuh_lab_master.ToList();
            var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in deficiencies)
            {
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();

                List<string> defs = new List<string>();
                labsWithDeficiency.ForEach(l =>
                {
                    if (l.Equals("No Equipement Uploaded"))
                    {
                        defs.Add(l);
                    }
                    else
                    {
                        string[] strLab = l.Split('-');

                        int specializationid = Convert.ToInt32(strLab[3]);
                        int year = Convert.ToInt32(strLab[0]);
                        int semester = Convert.ToInt32(strLab[1]);
                        string labCode = strLab[2].Replace("$", "-");

                        var requiredLabs = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).ToList();
                        int requiredCount = requiredLabs.Count();
                        int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

                        if (requiredCount > availableCount)
                        {
                            string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
                            defs.Add(year + "-" + semester + "-" + labName);
                        }
                    }
                });

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = item.degree;
                newFaculty.Department = item.department;
                newFaculty.Specialization = item.specializationName;
                newFaculty.SpecializationId = item.specializationid;
                newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));

                lstFaculty.Add(newFaculty);
            }

            return lstFaculty;
        }

        public class CollegeFacultyLabs
        {
            public int SNo { get; set; }
            public int DegreeDisplayOrder { get; set; }
            public string DegreeType { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public int SpecializationId { get; set; }
            public string Shift { get; set; }
            public int TotalIntake { get; set; }
            public int Required { get; set; }
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
        }

        public class CollegeFacultyWithIntakeReport
        {
            public int id { get; set; }
            public int collegeId { get; set; }
            public int academicYearId { get; set; }
            public string collegeCode { get; set; }
            public string collegeName { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public int shiftId { get; set; }
            public string Shift { get; set; }
            public int specializationId { get; set; }
            public int DepartmentID { get; set; }
            public int? degreeDisplayOrder { get; set; }

            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }
        }

        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();

            if (collegeId != null)
            {
                var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
                }
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
                foreach (var item in intake)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = item.jntuh_specialization.specializationName;
                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }

                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.isApproved == null || rf.isApproved == true) && (rf.PANNumber != null || rf.AadhaarNumber != null))
                                                 .Select(rf => new
                                                 {
                                                     RegistrationNumber = rf.RegistrationNumber,
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber
                                                 }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID != 0).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber
                }).Where(e => e.Department != null)
                                                 .ToList();

                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }

                    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    //====================================
                    // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                        }
                    }
                    intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    if (intakedetails.id > 0)
                    {
                        int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        if (swf != null)
                        {
                            intakedetails.specializationWiseFaculty = (int)swf;
                        }
                        intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                        intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                    }

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharmacy";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
                        int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
                        int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

                        int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        if (facultydeficiencyId == 0)
                        {
                            intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
                        }
                        else
                        {
                            int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                            bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
                            int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
                            intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
                        }
                    }
                }
            }

            return intakedetailsList;
        }

        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();

            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => specializationIds.Contains(l.SpecializationID))
                                                        .Select(l => new Lab
                                                        {
                                                            ////// EquipmentID=l.id,                                                               
                                                            degreeId = l.DegreeID,
                                                            degree = l.jntuh_degree.degree,
                                                            degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                            departmentId = l.DepartmentID,
                                                            department = l.jntuh_department.departmentName,
                                                            specializationId = l.SpecializationID,
                                                            specializationName = l.jntuh_specialization.specializationName,
                                                            year = l.Year,
                                                            Semester = l.Semester,
                                                            Labcode = l.Labcode,
                                                            LabName = l.LabName
                                                        })
                                                        .OrderBy(l => l.degreeDisplayOrder)
                                                        .ThenBy(l => l.department)
                                                        .ThenBy(l => l.specializationName)
                                                        .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                        .ToList();

            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeID).ToList();

            foreach (var item in collegeLabMaster)
            {
                Lab lstlabs = new Lab();
                lstlabs.collegeId = (int)collegeID;
                lstlabs.EquipmentID = item.EquipmentID;
                lstlabs.degree = item.degree;
                lstlabs.department = item.department;
                lstlabs.specializationName = item.specializationName;
                lstlabs.specializationId = item.specializationId;
                lstlabs.Semester = item.Semester;
                lstlabs.year = item.year;
                lstlabs.Labcode = item.Labcode;
                //lstlabs.RandomId = (int)rid;
                lstlabs.LabName = item.LabName;
                lstlabs.EquipmentNo = 1;
                lstlabs.RandomCode = strcollegecode;
                lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                if (jntuh_college_laboratories_deficiency.Count() != 0)
                {
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID).Select(ld => ld.Deficiency).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID).Select(ld => ld.Id).FirstOrDefault();
                }
                else
                {
                    lstlabs.deficiency = null;
                    lstlabs.id = 0;
                }
                lstlaboratories.Add(lstlabs);
            }

            lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName)
                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();

            return lstlaboratories;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            if (flag == 1)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else //admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }

            return intake;
        }
    }
}