using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using UAAAS.Models;
using System.Configuration;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class CounsellingReportNewController : BaseController
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
        private string bpharmacycondition;
        private string pharmdcondition;
        private string pharmadpbcondition;
        private decimal pharmadpbrequiredfaculty;
        private decimal BpharmacyrequiredFaculty;
        private decimal PharmDRequiredFaculty;
        private decimal PharmDPBRequiredFaculty;
        private int TotalcollegeFaculty;
        private int Group1PharmacyFaculty;
        private int Group2PharmacyFaculty;
        private int Group3PharmacyFaculty;
        private int Group4PharmacyFaculty;
        private int Group5PharmacyFaculty;
        private int Group6PharmacyFaculty;
        private int Allgroupscount;
        private int ApprovedIntake;
        private int specializationId;
        private int PharmaDApprovedIntake;
        private int PharmaDspecializationId;
        private int PharmaDPBApprovedIntake;
        private int PharmaDPBspecializationId;
        private bool bpharmacydeficiecny { get; set; }
        private string PharmacyandPharmDMeet = "";
        private int BpharmcyAvilableFaculty = 0;
        private string DeficiencyInPharmacy = "";

        public ActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
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


            CollegeIds =
              db.jntuh_college_edit_status.Where(a => a.IsCollegeEditable == false)
                  .Select(z => z.collegeId)
                  .ToArray();


            //colleges Ids
            //CollegeIds = db.jntuh_college
            //             .Where(c => c.isActive == true && c.isNew == false &&
            //                         SubmitteCollegesId.Contains(c.id))
            //             .Select(c => c.id)
            //             .ToArray();

            //CollegeIds = db.jntuh_college_news
            //             .Where(n => n.title.Contains("Grant of Affiliation is available at your portal for download"))
            //             .Select(c => c.collegeId)
            //             .ToArray();

            //CollegeIds = db.college_clusters.Where(c => c.clusterName.Equals("2015-court-cases")).Select(c => (int)c.collegeId).ToArray();

            //To fill college dropdown list
            ViewBag.Colleges = db.jntuh_college.AsNoTracking()
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

        [Authorize(Roles = "Admin,SuperAdmin")]
        private List<CounsellingReport> CounsellingReportList(int? CollegeId, string cmd)
        {
            //CollegeId = 420;
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
                    //  degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();
                    //degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Pharmacy" || d.degree == "Pharm.D" || d.degree == "Pharm.D PB")).Select(d => d.degree).ToArray();
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
                else if (cmd == "5-Year MBA(Integrated)" || cmd == "Export MAM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "5-Year MBA(Integrated)")).Select(d => d.degree).ToArray();
                    ReportHeader = "5-YearMBAIntegrated.xls";
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
                //College Id =&& ci.collegeId==4
                //Check Complete Colleges=21,416,42,&&ci.collegeId==300
                // int[] checkcollegeids = { 180,4, 7, 8, 9, 11, 12, 17, 22, 23, 26, 29, 32, 35, 38, 40, 41, 42, 43, 48, 50, 68, 70, 74, 75, 77, 79, 81, 84, 85, 86, 87, 91, 100, 103, 108, 109, 111, 113, 121, 123, 128, 130, 132, 140, 141, 144, 145, 148, 152, 153, 156, 162, 164, 166, 168, 171, 172, 173, 175, 176, 177, 178, 179, 183, 184, 185, 187, 188, 192, 193, 194, 195, 196, 198, 203, 207, 211, 214, 215, 218, 225, 227, 228, 242, 244, 245, 250, 254, 256, 260, 261, 264, 266, 271, 273, 276, 282, 287, 292, 293, 299, 304, 306, 307, 308, 309, 321, 322, 324, 327, 329, 334, 350, 352, 360, 365, 366, 367, 371, 373, 380, 382, 391, 399, 423, 429, 435, 439, 441, 455 };
                int cId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);

                if (cId == 375)
                {

                    CounsellingReportList = (from ci in db.jntuh_college_intake_existing
                                             join s in db.jntuh_specialization on ci.specializationId equals s.id
                                             join de in db.jntuh_department on s.departmentId equals de.id
                                             join d in db.jntuh_degree on de.degreeId equals d.id
                                             join c in db.jntuh_college on ci.collegeId equals c.id
                                             join es in db.jntuh_college_edit_status on c.id equals es.collegeId
                                             where
                                                 (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true &&
                                                  ci.academicYearId == nextAcademicYearId && es.academicyearId == nextAcademicYearId &&
                                                  DegreeIds.Contains(d.id) && es.IsCollegeEditable == false)
                                             //&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                         
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
                                             join es in db.jntuh_college_edit_status on c.id equals es.collegeId
                                             where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && ci.collegeId == cId && ci.academicYearId == nextAcademicYearId && es.academicyearId == nextAcademicYearId && DegreeIds.Contains(d.id) && es.IsCollegeEditable == false)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                         
                                             orderby d.degreeDisplayOrder
                                             select new CounsellingReport
                                             {
                                                 CollegeId = ci.collegeId,
                                                 CollegeCode = c.collegeCode,
                                                 CollegeName = c.collegeName
                                             }).Distinct().ToList();
                }

            }
            else
            {
                CounsellingReportList = (from ci in db.jntuh_college_intake_existing
                                         join s in db.jntuh_specialization on ci.specializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on ci.collegeId equals c.id
                                         join es in db.jntuh_college_edit_status on c.id equals es.collegeId
                                         where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && c.id == CollegeId && ci.academicYearId == nextAcademicYearId && es.academicyearId == nextAcademicYearId && DegreeIds.Contains(d.id) && es.IsCollegeEditable == false)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                        
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
                //if (item.CollegeId == 410 || item.CollegeId == 34)
                //{

                //}
                //Binding collegeCode CollegeName and specializationDetails Data to table
                DeficiencyInPharmacy = "";
                BpharmcyAvilableFaculty = 0;
                item.CollegeSpecializations = GetDegreewiseCollegeSpecializations(item.CollegeId);
                item.CollegeAddress = CollegeAddress(item.CollegeId);
                item.Establishyear = CollegeEstablishYear(item.CollegeId);
                item.Grade = CollegeDistrict(item.CollegeId);
            }

            CounsellingReportList = CounsellingReportList.Where(c => c.CollegeSpecializations.Count() > 0).OrderBy(c => c.CollegeName).ToList();
            // CounsellingReportList = CounsellingReportList.OrderBy(c => c.CollegeName).ToList();
            ViewBag.CollegeSpecializations = CounsellingReportList;
            //ViewBag.CollegeSpecializations = CounsellingReportList;
            ViewBag.Count = CounsellingReportList.Count();
            return CounsellingReportList;
        }

        private List<DegreewiseCollegeSpecializations> GetDegreewiseCollegeSpecializations(int CollegeId)
        {
            List<CollegeFacultyLabs> affiliatedCourses = CollegeCoursesAllClear(CollegeId);   //CollegeCoursesNotClear(CollegeId); Commentted by Suresh
            //  List<CollegeFacultyLabs> affiliatedCourses = CollegeCoursesNotClear(CollegeId);
            int[] specializationids = affiliatedCourses.Select(a => a.SpecializationId).ToArray();

            List<DegreewiseCollegeSpecializations> DegreewiseCollegeSpecializationsList = new List<DegreewiseCollegeSpecializations>();
            List<Specializations> SpecializationList = (from ci in db.jntuh_college_intake_existing
                                                        join s in db.jntuh_specialization on ci.specializationId equals s.id
                                                        join de in db.jntuh_department on s.departmentId equals de.id
                                                        join d in db.jntuh_degree on de.degreeId equals d.id
                                                        where (specializationids.Contains(ci.specializationId) && ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && ci.collegeId == CollegeId && ci.academicYearId == nextAcademicYearId && ci.proposedIntake != 0)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                                        
                                                        orderby d.degreeDisplayOrder
                                                        select new Specializations
                                                        {
                                                            CollegeId = ci.collegeId,
                                                            ShiftId = ci.shiftId,
                                                            ProposedIntake = ci.proposedIntake == null ? 0 : (int)ci.proposedIntake,
                                                            SpecializationId = ci.specializationId,
                                                            DepartmentId = de.id,
                                                            DegreeId = d.id
                                                        }).ToList();


            DegreeIds = db.jntuh_degree.Where(d => d.isActive == true && degrees.Contains(d.degree)).Select(d => d.id).ToArray();
            SpecializationList = SpecializationList.Where(d => DegreeIds.Contains(d.DegreeId)).ToList();
            List<Specializations> NewSpecializationList = new List<Specializations>();
            foreach (var speccheck in SpecializationList)
            {
                int id =
                    affiliatedCourses.Where(
                        s => s.ShiftId == speccheck.ShiftId && s.SpecializationId == speccheck.SpecializationId)
                        .Select(s => s.SpecializationId)
                        .FirstOrDefault();
                if (id != 0)
                {
                    NewSpecializationList.Add(speccheck);
                }
            }
            int[] OthersSpecIds = new int[] { 31, 37, 42, 48, 155, 156, 157, 158 };
            foreach (var humanities in OthersSpecIds)
            {
                var id = affiliatedCourses.Where(s => s.ShiftId == 1 && s.SpecializationId == humanities).Select(s => s).FirstOrDefault();
                if (id != null)
                {
                    NewSpecializationList.Add(new Specializations { CollegeId = CollegeId, SpecializationId = humanities, ShiftId = id.ShiftId, DegreeId = 4, ProposedIntake = id.TotalIntake, AvailableFaculty = id.Available, RequiredFaculty = id.Required, DepartmentId = id.DepartmentId });
                }
            }
            foreach (var item in NewSpecializationList)
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
                            //NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
                            NewSpecialization = specializationName;
                        }
                        else
                        {
                            //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewIntake = item.ProposedIntake.ToString();
                            //NewSpecialization = item.ShiftId == 1 ? degreeName + "-" + specializationName : degreeName + "-" + specializationName + "-" + "(II Shift)";
                            NewSpecialization = degreeName + "-" + specializationName;
                        }
                    }
                    else
                    {
                        //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);

                        NewIntake = item.ProposedIntake.ToString();
                        //NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
                        NewSpecialization = specializationName;
                    }
                    if (item.ShiftId == 1)
                    {
                        DegreewiseCollegeSpecializations.RequiredFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.Required)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.AvailableFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.Available)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.PHDAvailableFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.AvailablePhdFaculty)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.PHDRequiredFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.RequiredPhdFaculty)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.ApprovedIntake =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.TotalIntake)
                                .FirstOrDefault()
                                .ToString();
                        DegreewiseCollegeSpecializations.NTPLFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.NTPLFaculty)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.CSEPhDFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.CSEPhDFaculty)
                                .FirstOrDefault();
                    }
                    else
                    {
                        DegreewiseCollegeSpecializations.RequiredFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.Required)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.AvailableFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.Available)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.PHDAvailableFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.AvailablePhdFaculty)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.PHDRequiredFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.RequiredPhdFaculty)
                                .FirstOrDefault();
                        DegreewiseCollegeSpecializations.ApprovedIntake =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.TotalIntake)
                                .FirstOrDefault()
                                .ToString();
                        DegreewiseCollegeSpecializations.NTPLFaculty =
                           affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                               .Select(e => e.NTPLFaculty)
                               .FirstOrDefault();
                        DegreewiseCollegeSpecializations.CSEPhDFaculty =
                            affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId)
                                .Select(e => e.CSEPhDFaculty)
                                .FirstOrDefault();
                    }
                    DegreewiseCollegeSpecializations.Specialization = NewSpecialization;
                    DegreewiseCollegeSpecializations.Intake = NewIntake;

                    //if (affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId && e.ShiftId == item.ShiftId).Select(e => e.TotalIntake).FirstOrDefault() == 0)
                    //{
                    //    DegreewiseCollegeSpecializations.ApprovedIntake = "0";
                    //}
                    //else
                    //{
                    //    if (item.ProposedIntake < affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId).Select(e => e.TotalIntake).FirstOrDefault())
                    //    {
                    //        DegreewiseCollegeSpecializations.ApprovedIntake = NewIntake;
                    //    }
                    //    else
                    //    {
                    //        if (item.ProposedIntake <= 30)
                    //        {
                    //            if (item.ProposedIntake >= affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId).Select(e => e.TotalIntake).FirstOrDefault())
                    //            {
                    //                DegreewiseCollegeSpecializations.ApprovedIntake = NewIntake;
                    //            }
                    //            else
                    //            {
                    //                DegreewiseCollegeSpecializations.ApprovedIntake = affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId).Select(e => e.TotalIntake).FirstOrDefault().ToString();
                    //            }
                    //        }
                    //        else
                    //        {
                    //            DegreewiseCollegeSpecializations.ApprovedIntake = affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId).Select(e => e.TotalIntake).FirstOrDefault().ToString();
                    //        }

                    //    }
                    //}
                    if (
                        affiliatedCourses.Where(e => e.SpecializationId == item.SpecializationId)
                            .Select(e => e.ispercentage)
                            .FirstOrDefault())
                    {
                        DegreewiseCollegeSpecializations.isPercentage = true;
                    }
                    else
                    {
                        DegreewiseCollegeSpecializations.isPercentage = false;
                    }
                    var LastYearApprovedIntake =
                        db.jntuh_college_intake_existing.Where(
                            ie =>
                                ie.collegeId == CollegeId && ie.specializationId == item.SpecializationId &&
                                ie.shiftId == item.ShiftId && ie.academicYearId == 14 &&
                                ie.approvedIntake != 0).Select(s => s).FirstOrDefault();
                    if (LastYearApprovedIntake != null)
                    {
                        DegreewiseCollegeSpecializations.ApprovedIntake = LastYearApprovedIntake.approvedIntake.ToString();
                    }
                    else
                    {
                        DegreewiseCollegeSpecializations.ApprovedIntake = "0";
                    }
                    DegreewiseCollegeSpecializationsList.Add(DegreewiseCollegeSpecializations);
                }
            }
            //Commented by Narayana Reddy reson get all cources in Counselling Report with 0 or - Print
            return DegreewiseCollegeSpecializationsList.OrderBy(e => e.Specialization).ToList();
            //return DegreewiseCollegeSpecializationsList.Where(i=>i.ApprovedIntake!="0").OrderBy(e => e.Specialization).ToList();
        }

        //private int GetIntake2(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        //{
        //    int intake = 0;

        //    //Degree B.Tech  
        //    if (DegreeId == 4)
        //    {
        //        //admitted
        //        if (flag == 1 && (academicYearId == 8 || academicYearId == 3 || academicYearId == 2))
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 9)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else   //approved
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //        }
        //    }
        //    else
        //    {
        //        //approved
        //        if (flag == 1 && academicYearId != 9)
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 9)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else //admitted
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //        }
        //    }
        //    return intake;
        //}


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
            //CollegeId = 8;
            GetIds();



            //  SubmitteCollegesId = db.jntuh_appeal_college_edit_status.Select(e => e.collegeId).Distinct().ToArray();
            var colleges = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e).OrderBy(e => e.collegeId).ToList();
            //Commented By Narayan reddy 07-05-2018 
            //var CollegeIdsaffiliation1 = new[] { 7, 8, 12, 17, 38, 41, 48, 56, 69, 70, 77, 81, 85, 86, 87, 108, 113, 125, 128, 129, 138, 145, 147, 148, 152, 153, 172, 175, 176, 178, 179, 182, 183, 184, 186, 188, 189, 195, 201, 211, 214, 218, 242, 244, 249, 264, 271, 287, 299, 329, 336, 360, 365, 366, 382, 393, 423, 435, 439, 441 };
            //var CollegeIdsaffiliation2 = new[] { 29, 43, 88, 100, 123, 163, 164, 165, 166, 185, 192, 196, 222, 223, 229, 250, 260, 261, 266, 282, 291, 300, 307, 309, 310, 316, 326, 327, 330, 335, 349, 350, 367, 373, 380 };
            //var CollegeIdsaffiliation3 = new[] { 26, 40, 72, 121, 141, 157, 193, 225, 243, 256, 273, 286, 308, 371, 414};
            //var CollegeIdsaffiliation4 = new[] { 4, 23, 32, 39, 68, 75, 80, 134, 137, 156, 168, 173, 180, 203, 207, 210, 227, 228, 238, 276, 293, 324, 352, 368, 374, 385, 416 };
            //var CollegeIdsaffiliation5 = new[] { 9, 11, 20, 22, 46, 79, 84, 103, 109, 111, 116, 122, 130, 144, 155, 158, 162, 187, 197, 198, 215, 254, 292, 334, 342, 399, 400, 429, 42, 140, 35, 91 };
            //var CollegeIdsaffiliation6 = new[] { 22, 50, 74, 171, 177, 241, 245, 282, 306, 364 };
            //var CollegeIdsaffiliation7 = new[] { 132,443};
            //MBA Colleges List
            //var CollegeIdsaffiliation8 = new[] { 4, 5, 11, 12, 20, 23, 26, 29, 35, 38, 69, 70, 72, 74, 88, 100, 108, 109, 111, 113, 116, 119, 128, 147, 148, 152, 153, 158, 162, 163, 165, 168, 171, 174, 179, 180, 182, 185, 188, 189, 192, 196, 211, 214, 222, 223, 242, 246, 250, 260, 264, 273, 276, 279, 292, 296, 299, 300, 306, 307, 309, 316, 325, 326, 329, 335, 342, 352, 355, 360, 365, 367, 374, 380, 385, 386, 399, 400, 411, 439, 441, 452 };
            //var CollegeIdsAffiliation9 = new[] { 7, 9, 39, 40, 42, 50, 56, 68, 75, 79, 80, 103, 121, 122, 125, 130, 134, 143, 144, 156, 157, 164, 175, 194, 195, 203, 218, 229, 238, 243, 254, 261, 266, 271, 286, 293, 304, 308, 310, 321, 330, 343, 350, 368, 394, 414, 423, 430, 435 };
            //var CollegeIdsAffiliation10 = new[] { 40, 116, 121, 153, 261, 279, 292, 321, 325, 326, 330, 385 };

            //var IntegratedCollegeIds = new[] { 399,108 };//9, 18, 39, 42, 75, 140, 180, 332, 364, 261,
            //M.Tech Colleges List
            //var CollegeIdsAffiliation11 = new[] { 4, 7, 11, 12, 17, 20, 22, 23, 26, 29, 32, 38, 40, 41, 46, 48, 56, 68, 69, 70, 72, 73, 74, 77, 80, 84, 85, 87, 88, 100, 102, 103, 108, 109, 111, 113, 125, 128, 129, 130, 132, 134, 137, 138, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 162, 163, 164, 165, 167, 168, 171, 175, 178, 181, 182, 183, 185, 187, 188, 189, 192, 193, 195, 196, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 228, 238, 241, 242, 243, 249, 250, 256, 260, 261, 271, 276, 282, 286, 287, 292, 299, 300, 304, 305, 308, 309, 310, 316, 322, 324, 326, 327, 335, 336, 349, 360, 365, 367, 368, 371, 373, 374, 375, 380, 382, 385, 393, 399, 400, 401, 414, 416, 423, 429, 435, 439, 6, 24, 27, 30, 34, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 146, 150, 159, 169, 202, 204, 206, 219, 234, 237, 239, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 315, 317, 318, 319, 320, 348, 353, 362, 370, 376, 379, 384, 389, 392, 395, 410, 427, 428, 442, 445, 454, 179, 18, 39, 42, 43, 75, 140, 180, 217, 223, 235, 266, 332, 364, 422, 9, 35, 50, 273, 369 };
            //var CollegeIdsAffiliation12 = new[] { 4, 9, 11, 12, 17, 20, 23, 26, 29, 32, 35, 38, 39, 40, 41, 46, 50, 56, 69, 70, 72, 74, 75, 80, 84, 85, 87, 88, 100, 103, 108, 109, 111, 113, 125, 128, 129, 130, 137, 140, 144, 145, 147, 148, 152, 153, 156, 157, 158, 162, 163, 164, 165, 168, 171, 179, 180, 182, 183, 185, 187, 188, 189, 192, 193, 195, 196, 198, 211, 214, 222, 223, 225, 238, 242, 243, 250, 260, 261, 266, 273, 276, 282, 286, 287, 292, 299, 300, 304, 305, 308, 309, 310, 316, 324, 326, 327, 335, 360, 364, 365, 367, 368, 374, 380, 385, 399, 400, 414, 423, 435, 439 };
            //M.pharmacy Colleges   117 out from list due to getting error
            //var CollegeIdsAffiliation13 = new[] { 6, 24, 27, 30, 34, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 97, 104, 105, 107, 110, 114, 118, 120, 127, 135, 136, 146, 150, 159, 169, 202, 204, 206, 219, 234, 237, 239, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 315, 317, 318, 319, 320, 348, 353, 362, 370, 376, 379, 384, 389, 392, 395, 410, 427, 428, 442, 445, 454, 18, 39, 42, 75, 140, 180, 235, 332, 364, 9 };

            //var CollegeIdsAffiliation14 = new[] {117,44,75};

            SubmitteCollegesId = colleges.Where(e => e.IsCollegeEditable == false && e.collegeId != 375).Select(e => e.collegeId).Distinct().ToArray();

            var appealCollegeIds = db.jntuh_college.Where(c => c.isActive == true && SubmitteCollegesId.Contains(c.id)).Select(c => c.id).ToList();//

            //var ids = new[] {125,140,161,416,422,424};
            //appealCollegeIds.AddRange(ids);

            // CollegeIds = appealCollegeIds.ToArray();
            //CollegeIds = new int[] { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 35, 38, 39, 40, 41, 42, 43, 46, 48, 50, 56, 59, 68, 69, 70, 72, 74, 75, 77, 79, 80, 81, 84, 85, 86, 87, 88, 91, 100, 103, 106, 108, 109, 111, 113, 115, 116, 121, 122, 123, 125, 128, 129, 130, 132, 134, 137, 138, 140, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 194, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 217, 218, 222, 223, 225, 227, 228, 229, 230, 235, 236, 238, 241, 242, 243, 244, 245, 249, 250, 254, 256, 260, 261, 264, 266, 271, 273, 276, 282, 286, 287, 291, 292, 293, 299, 300, 304, 306, 307, 308, 309, 310, 316, 321, 322, 324, 327, 329, 330, 332, 334, 335, 336, 342, 349, 350, 352, 360, 364, 365, 366, 367, 368, 369, 371, 373, 374, 380, 382, 385, 391, 393, 399, 400, 403, 414, 416, 420, 422, 423, 429, 435, 439, 441, 443, 455};
            //CollegeIds = new int[] {115,416};
            CollegeIds = appealCollegeIds.ToArray();



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

            if ((cmd == "Export B.Tech" || cmd == "Export B.Tech/B.Pharm" || cmd == "Export M.Tech" || cmd == "Export M.Tech/M.Pharm" || cmd == "Export MBA" || cmd == "Export MCA" || cmd == "Export MBA/MCA" || cmd == "Export Pharm.D/Pharm.D PB" || cmd == "Export MAM" || cmd == "Export MTM" || cmd == "Export MAM/MTM") && Count != 0 || cmd == "Export ALL" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_CounsellingReport.cshtml", CounsellingList);
            }
            if ((cmd == "Export B.Pharm" || cmd == "Export M.Pharm" || cmd == "Export Pharm.D" || cmd == "Export Pharm.D PB") && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_PharmacyCounsellingReport.cshtml", CounsellingList);
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

            var faculty = new List<CollegeFacultyLabs>();
            // var affiliation40percentids = new[] { 125, 140, 157, 161, 162, 416, 422, 424 };

            var integreatedIds = new[] { 9, 39, 42, 75, 140, 180, 332, 364, 235 };// 18,140

            var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };

            if (pharmacyids.Contains(collegeId))
            {
                faculty = PharmacyDeficienciesInFaculty(collegeId);
            }

            //else if (collegeId == 140)
            //{
            //    var integratedpharmacyfaculty = Affiliation4080PercentPharmacyDeficienciesInFaculty(collegeId);
            //    var integratedbtechfaculty = Affiliation4080PercentDeficienciesInFaculty(collegeId);
            //    faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            //}
            else if (integreatedIds.Contains(collegeId))
            {
                var integratedpharmacyfaculty = PharmacyDeficienciesInFaculty(collegeId);
                var integratedbtechfaculty = DeficienciesInFaculty(collegeId);
                faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            }

            //else if (affiliation40percentids.Contains(collegeId))
            //{
            //    faculty = Affiliation4080PercentDeficienciesInFaculty(collegeId);
            //}
            else
            {
                faculty = DeficienciesInFaculty(collegeId);
            }
            List<CollegeFacultyLabs> labs = DeficienciesInLabsnew(collegeId);
            //DeficienciesInLabs(collegeId);

            var collegeFacultyLabs = labs.FirstOrDefault(i => i.Degree == "B.Pharmacy");
            if (collegeFacultyLabs != null)
            {
                var bphramcylabs = collegeFacultyLabs.LabsDeficiency;
                if (bphramcylabs != "NIL")
                {
                    foreach (var c in labs.Where(i => i.Degree == "M.Pharmacy").ToList())
                    {
                        c.LabsDeficiency = "YES";
                    }
                }
            }

            foreach (var l in labs)
            {
                if (l.Degree == "B.Tech" && l.LabsDeficiency != "NIL")
                {
                    labs.Where(i => i.Department == l.Department && i.Degree == "M.Tech" && i.LabsDeficiency == "NIL").ToList().ForEach(c => c.LabsDeficiency = "YES");
                }

            }
            int[] FSpecIds = faculty.Select(F => F.SpecializationId).ToArray();
            int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();
            int[] PharmacySpecualizationIds = new int[] { 12, 13, 18, 19, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 167, 169, 170, 171, 172 };
            if (pharmacyids.Contains(collegeId))
            {

                foreach (int id in FSpecIds)
                {
                    if (!LSpecIds.Contains(id))
                    {
                        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                        lab.SpecializationId = id;
                        lab.Deficiency = "NIL";
                        lab.LabsDeficiency = "NIL";
                        labs.Add(lab);
                    }
                }
            }
            else if (integreatedIds.Contains(collegeId))
            {
                foreach (int id in FSpecIds)
                {
                    if (PharmacySpecualizationIds.Contains(id))
                    {
                        if (!LSpecIds.Contains(id))
                        {
                            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                            lab.SpecializationId = id;
                            lab.Deficiency = "NIL";
                            lab.LabsDeficiency = "NIL";
                            labs.Add(lab);
                        }
                    }
                }
            }
            //Commented by Narayana Reddy on 21-04-2019 because print all cources
            List<CollegeFacultyLabs> clearedCourses1 = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
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
                                            DollerCourseIntake = a.f.DollerCourseIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency,
                                            ShiftId = a.f.ShiftId,
                                            AvailablePhdFaculty = a.f.AvailablePhdFaculty,
                                            RequiredPhdFaculty = a.f.RequiredPhdFaculty,
                                            ispercentage = a.f.ispercentage,
                                            NTPLFaculty = a.f.NTPLFaculty,
                                            CSEPhDFaculty = a.f.CSEPhDFaculty
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

            List<CollegeFacultyLabs> clearedCourses = faculty.ToList();

            //List<CollegeFacultyLabs> clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })

            //                            .Select(a => new CollegeFacultyLabs
            //                            {
            //                                DegreeType = a.f.DegreeType,
            //                                DegreeDisplayOrder = a.f.DegreeDisplayOrder,
            //                                Degree = a.f.Degree,
            //                                Department = a.f.Department,
            //                                SpecializationId = a.f.SpecializationId,
            //                                Specialization = a.f.Specialization,
            //                                TotalIntake = a.f.TotalIntake,
            //                                Required = a.f.Required,
            //                                Available = a.f.Available,
            //                                Deficiency = a.f.Deficiency,
            //                                PhdDeficiency = a.f.PhdDeficiency,
            //                                LabsDeficiency = a.l.LabsDeficiency,
            //                                ShiftId = a.f.ShiftId,
            //                                AvailablePhdFaculty = a.f.AvailablePhdFaculty,
            //                                RequiredPhdFaculty = a.f.RequiredPhdFaculty,
            //                                ispercentage = a.f.ispercentage,
            //                            })
            //                            .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
            //                            .ToList();

            List<CollegeFacultyLabs> deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES" || a.l.LabsDeficiency != "NIL"))
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            DegreeType = a.f.DegreeType,
                                            DegreeDisplayOrder = a.f.DegreeDisplayOrder,
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            DollerCourseIntake = a.f.DollerCourseIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            ShiftId = a.f.ShiftId,
                                            LabsDeficiency = a.l.LabsDeficiency,
                                            AvailablePhdFaculty = a.f.AvailablePhdFaculty,
                                            RequiredPhdFaculty = a.f.RequiredPhdFaculty,
                                            NTPLFaculty = a.f.NTPLFaculty,
                                            CSEPhDFaculty = a.f.CSEPhDFaculty
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

            //if (collegeId == 319)
            //{
            //    if (DegreeIds.Contains(9) || DegreeIds.Contains(10))
            //    {
            //        clearedCourses.Clear();
            //    }
            //}


            int affiliatedCount = 0; int defiencyRows = 0;

            List<string> deficiencyDepartments = new List<string>();

            foreach (var course in deficiencyCourses)
            {
                if (!deficiencyDepartments.Contains(course.Department))
                {
                    deficiencyDepartments.Add(course.Department);
                }

                ////FIVE percent relaxation for faculty only
                //decimal percentage = 0;

                //if (course.LabsDeficiency == "NIL" && (course.Degree == "B.Pharmacy" || course.Degree == "M.Pharmacy" || course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB"))
                //{
                //    percentage = (course.Required * 5) / 100;
                //    percentage = percentage > 1 ? 1 : percentage;
                //    bpharmacydeficiecny = course.Deficiency != "YES";
                //    if ((course.Required - course.Available) <= 1 && percentage > 0)
                //    {
                //        bpharmacydeficiecny = true;
                //        deficiencyCourses.Where(i => i.Degree == "M.Pharmacy" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                //        deficiencyCourses.Where(i => i.Degree == "Pharm.D" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                //        deficiencyCourses.Where(i => i.Degree == "Pharm.D PB" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                //    }
                //}


                //if (course.LabsDeficiency == "NIL" && (course.Degree == "B.Tech" || course.Degree == "M.Tech" || course.Degree == "MBA" || course.Degree == "MCA"))
                //{
                //    percentage = (course.Required * 10) / 100;
                //    percentage = percentage > 2 ? 2 : percentage;
                //    bpharmacydeficiecny = true;
                //}

                //bool isCourseAffiliated = false;

                //var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

                //if (affiliation != null)
                //{
                //    if (affiliation.isAffiliated == true)
                //    {
                //        isCourseAffiliated = true;
                //    }
                //}

                //if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL" && bpharmacydeficiecny))
                //{
                //    if (course.TotalIntake != 0)
                //    {
                //        affiliatedCount++;
                //        affiliatedCourses.Add(course); //commented by suresh
                //        rowCount++;
                //    }
                //}
                //else if (isCourseAffiliated == false)
                //{
                //    defiencyRows++;
                //    assessmentCount++;
                //}


                defiencyRows++;
                assessmentCount++;
            }

            foreach (var course in clearedCourses)
            {
                //if (course.Deficiency == "YES" || course.PhdDeficiency == "YES" || course.ispercentage == true || course.LabsDeficiency == "NIL")
                //{
                //    course.TotalIntake = 0;
                //}
                string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                   .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList();//.Where(a => a.DegreeType == "UG")
                //Commented By Narayana || course.LabsDeficiency !="NIL"
                //if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                //{
                //    defiencyRows++;
                //    assessmentCount++;
                //}
                //else
                //{
                //if (course.TotalIntake != 0)
                //{
                affiliatedCount++; affiliatedCourses.Add(course);
                rowCount++;
                //}

                //}
            }

            return affiliatedCourses;
        }




        public List<CollegeFacultyLabs> CollegeCoursesNotClear(int collegeId)
        {
            List<CollegeFacultyLabs> notAffiliatedCourses = new List<CollegeFacultyLabs>();


            List<CollegeFacultyLabs> AffiliatedCourses = new List<CollegeFacultyLabs>(); //Added by suresh



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
                                        .Where(a => a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES" || a.l.LabsDeficiency != "NIL")
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
                //Commented on 18-06-2018 by Narayana Reddy
                //var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

                //if (affiliation != null)
                //{
                //    if (affiliation.isAffiliated == true)
                //    {
                //        isCourseAffiliated = true;
                //    }
                //}

                if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL") || isCourseAffiliated != false)
                {
                    if (course.TotalIntake != 0)
                    {
                        affiliatedCount++;
                        rowCount++;
                    }
                }
                else if (isCourseAffiliated == false)
                {
                    defiencyRows++; notAffiliatedCourses.Add(course);
                    assessmentCount++;
                }
            }

            foreach (var course in clearedCourses)
            {
                string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                   .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                //List<string> clearedDepartments = clearedCourses.Where(a => a.DegreeType == "UG").Select(a => a.Department).ToList();
                List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList();
                if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                {

                    defiencyRows++;
                    assessmentCount++;
                }
                else
                {
                    if (course.TotalIntake != 0)
                    {
                        AffiliatedCourses.Add(course);
                        affiliatedCount++; //affiliatedCourses.Add(course);
                        rowCount++;
                    }
                }
            }

            return notAffiliatedCourses;                          // notAffiliatedCourses; Commetted by suresh
        }



        public List<CollegeFacultyLabs> DeficienciesInFaculty(int? collegeID)
        {
            List<CollegeFacultyWithIntakeReport> orgfacultyCounts = collegeFaculty(collegeID).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCounts = orgfacultyCounts.Where(c => c.shiftId == 1).ToList();
            //This is New Code in 05-05-2018
            List<CollegeFacultyWithIntakeReport> facultyCountsmetechsecond = facultyCounts.Where(c => c.shiftId == 2).Select(e => e).ToList();
            foreach (var item in facultyCountsmetechsecond)
            {
                int id =
                    facultyCounts.Where(
                        s => s.specializationId == item.specializationId && s.shiftId == 1 && s.Degree == "M.Tech" && s.Proposedintake != 0)
                        .Select(s => s.shiftId)
                        .FirstOrDefault();
                if (id == 0)
                {
                    facultyCounts.Remove(item);
                }
            }
            //List<CollegeFacultyWithIntakeReport> facultyCountper = collegeFaculty(collegeID).Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCountper = orgfacultyCounts.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                {
                }
                else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                {

                }
                else
                {
                    List<CollegeFacultyWithIntakeReport> notshownmtech = facultyCounts.Where(
                        s => s.Department == itemmtech.Department && s.Degree == "M.Tech" && s.Proposedintake != 0)
                        .Select(s => s)
                        .ToList();
                    if (notshownmtech.Count() != 0)
                    {
                        // facultyCounts.Remove(itemmtech);
                        foreach (var removemtech in notshownmtech)
                        {
                            facultyCounts.Remove(removemtech);
                        }

                    }
                }

            }

            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.SIStatus == true)
                {
                    facultyCounts = facultyCounts.Where(e => e.Degree == "B.Tech").Select(e => e).ToList();
                }
            }




            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
            int[] OthersSpecIds = new int[] { 155, 156, 157, 158 };
            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech" && !departments.Contains(d.Department)).Select(d => d.Proposedintake).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 160);//120
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;

            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";
            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            var degrees = db.jntuh_degree.Select(t => t).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            int[] SpecializationIDS = db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
            string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
            foreach (var item in facultyCounts.Where(e => e.Proposedintake != 0).Select(e => e).ToList())//&& !admittedIntakeTwoormoreZeroDeptName1.Contains(e.Department) e.ispercentage == false
            {

                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                //if (item.Degree == "M.Tech")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
                //else if (item.Degree == "MCA")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
                //else if (item.Degree == "MBA")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
                //TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();
                //SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 1;
                if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                {

                    if (item.Degree == "M.Tech")
                    {
                        SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.Proposedintake, item.shiftId);
                    }
                    else if (item.Degree == "MCA")
                    {
                        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.Proposedintake);
                    }
                    else if (item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                    {
                        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.Proposedintake);
                    }


                }
                else if (item.Degree == "B.Tech")
                {
                    //SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                    SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
                }

                TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();


                //if (SpecializationIDS.Contains(item.specializationId))
                //{
                //    int SpecializationwisePGFaculty1 = facultyCounts.Where(S => S.specializationId == item.specializationId).Count();
                //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId == item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                //}

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }
                if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
                {
                    deptloop = 1;
                }
                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                string PhdminimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int ChangeSpecializationwisePHDFaculty = 0;
                int adjustedPHDFaculty = 0;
                int remainingFaculty1 = 0;
                int tFaculty = 0;
                int othersRequiredfaculty = 0;

                if (item.Department == "MBA" || item.Department == "MCA")
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
                else
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {

                    if (OthersSpecIds.Contains(item.specializationId))
                    {
                        double rid = (double)(firstYearRequired / 2);
                        rFaculty = (int)(Math.Ceiling(rid));
                        //othersRequiredfaculty = 1;

                    }
                    else
                    {
                        rFaculty = (int)firstYearRequired;
                    }
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "YES";
                        item.deficiency = false;
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;//tFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                        remainingFaculty = 0;
                    }


                    remainingPHDFaculty = item.phdFaculty;
                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)//remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA" && rFaculty <= adjustedFaculty
                    {


                        //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                        //adjustedPHDFaculty = item.SpecializationsphdFaculty;
                        //PhdminimumRequirementMet = "YES";
                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        {
                            if (item.shiftId == 1)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                }
                                else
                                {
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                }
                                //adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";
                            }
                        }
                        else
                        {

                            remainingPHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;

                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                PhdminimumRequirementMet = "YES";
                            else
                                PhdminimumRequirementMet = "NO";
                        }


                    }
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)//remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA" && rFaculty <= adjustedFaculty
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                    {

                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                        {
                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;//item.SpecializationsphdFaculty;
                            adjustedPHDFaculty = SpecializationwisePHDFaculty;//item.SpecializationsphdFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                        {
                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)//item.SpecializationsphdFaculty
                            {
                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                PhdminimumRequirementMet = "YES";
                            }
                            else//No condition
                            {

                                //SpecializationwisePHDFaculty = remainingPHDFaculty;
                                //adjustedPHDFaculty = remainingPHDFaculty;
                                //Commented by Narayana on 04-04-2018 Reopen on 07-05-2018
                                item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                                ChangeSpecializationwisePHDFaculty = SpecializationwisePHDFaculty;
                                SpecializationwisePHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "YES";
                                //remainingPHDFaculty = 0;
                                //PhdminimumRequirementMet = "NO";
                            }

                        }



                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                    {
                        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                        //Commented by Narayana on 04-04-2018
                        if (item.Proposedintake > 60)
                        {
                            item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                            ChangeSpecializationwisePHDFaculty = SpecializationwisePHDFaculty;
                            SpecializationwisePHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        //adjustedPHDFaculty = remainingPHDFaculty;
                        //PhdminimumRequirementMet = "NO";

                    }
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                    {
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                    {
                        PhdminimumRequirementMet = "YES";
                    }
                    //Dual Degree Checking
                    else if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                    {
                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        {
                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                PhdminimumRequirementMet = "YES";
                            else
                                PhdminimumRequirementMet = "NO";
                        }
                        else
                        {

                            remainingPHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;

                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                PhdminimumRequirementMet = "YES";
                            else
                                PhdminimumRequirementMet = "NO";
                        }

                    }
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty >= 0)
                    {
                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
                    {
                        PhdminimumRequirementMet = "NO";
                    }

                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "YES";
                        item.deficiency = false;
                        //Old Code Comment on 2018
                        //if (rFaculty <= item.specializationWiseFaculty)
                        //{
                        //    remainingFaculty = remainingFaculty - rFaculty;
                        //    adjustedFaculty = rFaculty;
                        //}

                        //else if (rFaculty >= item.specializationWiseFaculty)
                        //{
                        //    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                        //    adjustedFaculty = item.specializationWiseFaculty;
                        //}
                        if (item.shiftId == 1)
                        {
                            if (item.specializationWiseFaculty >= rFaculty)
                            {
                                if (rFaculty <= item.specializationWiseFaculty)
                                {
                                    remainingFaculty = remainingFaculty - rFaculty;
                                    adjustedFaculty = rFaculty;
                                }

                                else if (rFaculty >= item.specializationWiseFaculty)
                                {
                                    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                    adjustedFaculty = item.specializationWiseFaculty;
                                }
                            }
                            else
                            {
                                minimumRequirementMet = "NO";
                                item.deficiency = true;
                                remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                adjustedFaculty = item.specializationWiseFaculty;
                            }
                        }
                        else
                        {
                            if (item.shiftId == 2)
                            {
                                int firstshiftrequiredFaculty = (int)Math.Ceiling(facultyCounts.Where(e =>
                             e.Proposedintake != 0 && e.ispercentage == false &&
                             e.specializationId == item.specializationId && e.shiftId == 1)
                             .Select(e => e.requiredFaculty)
                             .FirstOrDefault());

                                item.specializationWiseFaculty = item.specializationWiseFaculty -
                                                                 firstshiftrequiredFaculty;
                                if (item.specializationWiseFaculty < 0)
                                {
                                    item.specializationWiseFaculty = 0;
                                }
                            }
                            if (item.specializationWiseFaculty >= rFaculty)
                            {
                                if (rFaculty <= item.specializationWiseFaculty)
                                {
                                    remainingFaculty = remainingFaculty - rFaculty;
                                    adjustedFaculty = rFaculty;
                                }

                                else if (rFaculty >= item.specializationWiseFaculty)
                                {
                                    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                    adjustedFaculty = item.specializationWiseFaculty;
                                }
                            }
                            else
                            {
                                minimumRequirementMet = "NO";
                                item.deficiency = true;
                                remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                adjustedFaculty = item.specializationWiseFaculty;
                            }
                        }
                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        item.deficiency = true;
                        //adjustedFaculty = remainingFaculty;
                        //facultyShortage = rFaculty - remainingFaculty;
                        //remainingFaculty = 0;
                        if (remainingFaculty >= item.specializationWiseFaculty)
                        {

                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                            adjustedFaculty = item.specializationWiseFaculty;
                        }
                        else
                        {
                            adjustedFaculty = remainingFaculty;
                            if (remainingFaculty == 0)
                            {
                                remainingPHDFaculty = 0;
                            }
                            remainingFaculty = 0;
                        }
                    }
                    //if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                    //{
                    //    //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                    //    //adjustedPHDFaculty = item.SpecializationsphdFaculty;
                    //    //PhdminimumRequirementMet = "YES";
                    //    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                    //    {
                    //        if (item.shiftId==1)
                    //        {
                    //            if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                    //            {
                    //                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                    //                {
                    //                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                    //                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                    //                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                    //                        PhdminimumRequirementMet = "YES";
                    //                    else
                    //                        PhdminimumRequirementMet = "NO";
                    //                }
                    //                else
                    //                {
                    //                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                    //                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                    //                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                    //                        PhdminimumRequirementMet = "YES";
                    //                    else
                    //                        PhdminimumRequirementMet = "NO";
                    //                }
                    //            }
                    //            else
                    //            {
                    //                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                    //                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                    //                PhdminimumRequirementMet = "NO";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (item.shiftId == 2)
                    //            {
                    //                int firstshiftintake = facultyCounts.Where(e =>
                    //                 e.Proposedintake != 0 && e.ispercentage == false &&                                   
                    //                 e.specializationId == item.specializationId && e.shiftId == 1)
                    //                 .Select(e => e.Proposedintake)
                    //                 .FirstOrDefault();
                    //                int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                    //                item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                    //                                                 firstshiftPHDFaculty;
                    //                if (item.SpecializationsphdFaculty<0)
                    //                {
                    //                    item.SpecializationsphdFaculty = 0;
                    //                }
                    //            }                    
                    //            if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                    //            {
                    //                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                    //                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                    //                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                    //                    PhdminimumRequirementMet = "YES";
                    //                else
                    //                    PhdminimumRequirementMet = "NO";
                    //            }
                    //            else
                    //            {
                    //                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                    //                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                    //                PhdminimumRequirementMet = "NO";
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        remainingPHDFaculty = remainingPHDFaculty;
                    //        adjustedPHDFaculty = remainingPHDFaculty;

                    //        if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                    //            PhdminimumRequirementMet = "YES";
                    //        else
                    //            PhdminimumRequirementMet = "NO";
                    //    }
                    //}
                    if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                    {
                        //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                        //adjustedPHDFaculty = remainingPHDFaculty;
                        //PhdminimumRequirementMet = "NO";
                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        {
                            if (item.shiftId == 1)
                            {
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                            else
                            {

                                if (item.shiftId == 2)
                                {
                                    int firstshiftintake = facultyCounts.Where(e =>
                                     e.Proposedintake != 0 && e.ispercentage == false &&
                                     e.specializationId == item.specializationId && e.shiftId == 1)
                                     .Select(e => e.Proposedintake)
                                     .FirstOrDefault();
                                    int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                    item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                     firstshiftPHDFaculty;
                                    if (item.SpecializationsphdFaculty < 0)
                                    {
                                        item.SpecializationsphdFaculty = 0;
                                    }
                                }
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                        }
                        else
                        {
                            if (item.shiftId == 2)
                            {
                                int firstshiftintake = facultyCounts.Where(e =>
                                    e.Proposedintake != 0 && e.ispercentage == false &&
                                    e.specializationId == item.specializationId && e.shiftId == 1)
                                    .Select(e => e.Proposedintake)
                                    .FirstOrDefault();
                                int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                 firstshiftPHDFaculty;
                                if (item.SpecializationsphdFaculty < 0)
                                {
                                    item.SpecializationsphdFaculty = 0;
                                }
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty -
                                                          item.SpecializationsphdFaculty;
                                    if (remainingPHDFaculty < 0)
                                    {
                                        adjustedPHDFaculty = 0;
                                    }
                                    else
                                    {
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    }

                                    PhdminimumRequirementMet = "NO";
                                }
                            }
                            else
                            {
                                if (remainingPHDFaculty == SpecializationwisePHDFaculty)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else
                                    {
                                        PhdminimumRequirementMet = "NO";
                                        remainingPHDFaculty = 0;
                                    }
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else
                                    {
                                        PhdminimumRequirementMet = "NO";
                                        remainingPHDFaculty = 0;
                                    }
                                }
                            }
                        }

                    }
                    //else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                    //{
                    //     //This code commented on 23-04-2019
                    //    //adjustedPHDFaculty = remainingPHDFaculty;
                    //    //remainingPHDFaculty = remainingPHDFaculty - 1;
                    //    //PhdminimumRequirementMet = "NO";
                    //    if (item.shiftId==2)
                    //    {
                    //        int firstshiftintake = facultyCounts.Where(e =>
                    //                   e.Proposedintake != 0 && e.ispercentage == false &&
                    //                   e.specializationId == item.specializationId && e.shiftId == 1)
                    //                   .Select(e => e.Proposedintake)
                    //                   .FirstOrDefault();
                    //        int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                    //        item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                    //                                         firstshiftPHDFaculty;
                    //        if (item.SpecializationsphdFaculty < 0)
                    //        {
                    //            item.SpecializationsphdFaculty = 0;
                    //        }
                    //        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                    //        {
                    //            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                    //            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                    //            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                    //                PhdminimumRequirementMet = "YES";
                    //            else
                    //                PhdminimumRequirementMet = "NO";
                    //        }
                    //        else
                    //        {
                    //            remainingPHDFaculty = remainingPHDFaculty -
                    //                                  item.SpecializationsphdFaculty;
                    //            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                    //            PhdminimumRequirementMet = "NO";
                    //        }
                    //    }
                    //}
                    else if (remainingPHDFaculty >= SpecializationwisePHDFaculty && remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                            item.SpecializationsphdFaculty > 0)
                    {
                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        {
                            if (item.shiftId == 1)
                            {
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                            else
                            {
                                if (item.shiftId == 2)
                                {
                                    int firstshiftintake = facultyCounts.Where(e =>
                                     e.Proposedintake != 0 && e.ispercentage == false &&
                                     e.specializationId == item.specializationId && e.shiftId == 1)
                                     .Select(e => e.Proposedintake)
                                     .FirstOrDefault();
                                    int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                    item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                     firstshiftPHDFaculty;
                                }
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                        }
                        else
                        {
                            if (item.shiftId == 2)
                            {
                                int firstshiftintake = facultyCounts.Where(e =>
                                    e.Proposedintake != 0 && e.ispercentage == false &&
                                    e.specializationId == item.specializationId && e.shiftId == 1)
                                    .Select(e => e.Proposedintake)
                                    .FirstOrDefault();
                                int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                 firstshiftPHDFaculty;
                                if (item.SpecializationsphdFaculty < 0)
                                {
                                    item.SpecializationsphdFaculty = 0;
                                }
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty -
                                                          item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;
                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";
                            }
                        }
                    }
                    else if (remainingPHDFaculty >= SpecializationwisePHDFaculty && remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                     item.SpecializationsphdFaculty > 0)
                    {
                        if (item.shiftId == 1)
                        {
                            if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                            {
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "NO";
                            }

                        }
                        else if (item.shiftId == 2)
                        {
                            int firstshiftintake = facultyCounts.Where(e =>
                                e.Proposedintake != 0 && e.ispercentage == false &&
                                e.specializationId == item.specializationId && e.shiftId == 1)
                                .Select(e => e.Proposedintake)
                                .FirstOrDefault();
                            int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                            item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                             firstshiftPHDFaculty;
                            if (item.SpecializationsphdFaculty < 0)
                            {
                                item.SpecializationsphdFaculty = 0;
                            }
                            if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";
                            }
                            else
                            {
                                remainingPHDFaculty = remainingPHDFaculty -
                                                      item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "NO";
                            }
                        }
                        else
                        {
                            adjustedPHDFaculty = remainingPHDFaculty;

                            remainingPHDFaculty = remainingPHDFaculty - 1;
                            PhdminimumRequirementMet = "NO";
                        }

                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                    {

                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                        {
                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                        {
                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                            PhdminimumRequirementMet = "YES";
                        }

                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                    {
                        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                        if (item.Proposedintake > 60)
                        {
                            item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                            ChangeSpecializationwisePHDFaculty = SpecializationwisePHDFaculty;
                            SpecializationwisePHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        //adjustedPHDFaculty = remainingPHDFaculty;
                        //PhdminimumRequirementMet = "NO";

                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                    {
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                    {
                        PhdminimumRequirementMet = "YES";
                    }

                    //Dual Degree
                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                    {
                        //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                        //adjustedPHDFaculty = item.SpecializationsphdFaculty;   

                        //PhdminimumRequirementMet = "YES";
                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                        {
                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                PhdminimumRequirementMet = "YES";
                            else
                                PhdminimumRequirementMet = "NO";
                        }
                        else
                        {

                            remainingPHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;

                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                PhdminimumRequirementMet = "YES";
                            else
                                PhdminimumRequirementMet = "NO";
                        }

                    }
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;

                        remainingPHDFaculty = remainingPHDFaculty - 1;
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("Dual Degree")))
                    {
                        PhdminimumRequirementMet = "YES";
                    }
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
                    {
                        PhdminimumRequirementMet = "NO";
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
                newFaculty.DollerCourseIntake = item.dollercourseintake;
                newFaculty.NTPLFaculty = item.NTPLFaculty;
                newFaculty.CSEPhDFaculty = item.CSEPhDFaculty;
                if (item.ishashcourses == true)
                {
                    if (item.Proposedintake > 60)
                    {
                        newFaculty.TotalIntake = 60;
                    }
                    else
                    {
                        newFaculty.TotalIntake = item.Proposedintake;
                    }
                }
                else
                {
                    newFaculty.TotalIntake = item.Proposedintake;
                }

                if (departments.Contains(item.Department))
                {
                    newFaculty.TotalIntake = totalBtechFirstYearIntake;
                    if (OthersSpecIds.Contains(item.specializationId))
                    {
                        newFaculty.Required = (int)Math.Ceiling(firstYearRequired / 2);
                        newFaculty.Available = adjustedFaculty;
                        HumantitiesminimamRequireMet += item.totalFaculty;
                    }
                    else
                    {
                        newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
                        newFaculty.Available = adjustedFaculty;
                        HumantitiesminimamRequireMet += item.totalFaculty;
                    }

                }
                else
                {
                    //newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                    newFaculty.Available = adjustedFaculty;
                }

                if (adjustedFaculty > 0)
                    adjustedFaculty = adjustedFaculty;
                else
                    adjustedFaculty = 0;

                if (adjustedPHDFaculty > 0)
                    adjustedPHDFaculty = adjustedPHDFaculty;
                else
                    adjustedPHDFaculty = 0;


                if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                {
                    if (rFaculty <= adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";

                    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        PhdminimumRequirementMet = "NO";
                    else
                        PhdminimumRequirementMet = "YES";

                }
                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                {
                    if (rFaculty == adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";


                    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                        PhdminimumRequirementMet = "NO";
                    else
                        PhdminimumRequirementMet = "YES";
                }
                else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                {
                    if (rFaculty <= adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";

                    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                        PhdminimumRequirementMet = "NO";
                    else
                        PhdminimumRequirementMet = "YES";

                }
                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                {
                    if (rFaculty == adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";


                    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                        PhdminimumRequirementMet = "NO";
                    else
                        PhdminimumRequirementMet = "YES";
                }

                //if (departments.Contains(item.Department))
                //{
                //    if (minimumRequirementMet != "NO")
                //    {
                //        HumantitiesminimamRequireMetStatus = "NO";
                //    }
                //}



                newFaculty.ShiftId = item.shiftId;
                //newFaculty.AvailablePhdFaculty = adjustedPHDFaculty;
                newFaculty.AvailablePhdFaculty = adjustedPHDFaculty;
                if (ChangeSpecializationwisePHDFaculty == 0)
                    newFaculty.RequiredPhdFaculty = SpecializationwisePHDFaculty;
                else
                    newFaculty.RequiredPhdFaculty = ChangeSpecializationwisePHDFaculty;

                if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                {
                    newFaculty.Deficiency = "NO";
                }
                else
                {
                    newFaculty.Deficiency = "YES";
                }
                newFaculty.ispercentage = item.ispercentage;
                //if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree != "MBA")
                //{
                //    newFaculty.PhdDeficiency = "NO";
                //}
                //else if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree == "MBA")
                //{
                //    if (item.totalIntake > 120)
                //    {
                //        if (adjustedPHDFaculty >= 2)
                //            newFaculty.PhdDeficiency = "NO";
                //        else
                //            newFaculty.PhdDeficiency = "YES";
                //    }
                //    else
                //    {
                //        if (adjustedPHDFaculty >= 1)
                //            newFaculty.PhdDeficiency = "NO";
                //        else
                //            newFaculty.PhdDeficiency = "YES";
                //    }

                //}
                //else if (degreeType.Equals("PG"))
                //{
                //    newFaculty.PhdDeficiency = "YES";
                //}
                //else
                //{
                //    newFaculty.PhdDeficiency = "-";
                //}

                lstFaculty.Add(newFaculty);
                deptloop++;
            }


            //if (Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 15))) <= HumantitiesminimamRequireMet && HumantitiesminimamRequireMetStatus != "NO")
            //{
            //    //minimumRequirementMet = "YES";
            //}
            //else
            //{
            //    //minimumRequirementMet = "NO";
            //    if (lstFaculty.Count() != 0)
            //        lstFaculty.ToList().ForEach(e => e.Deficiency = "Yes");
            //}



            //if (HumantitiesminimamRequireMetStatus != "YES")
            //{
            //    if (lstFaculty.Count()!=0)
            //    lstFaculty.ToList().ForEach(e => e.Deficiency = "Yes");
            //}




            return lstFaculty;
        }


        public int GetIntakeBasedOnPhd(int phdcount)
        {
            int intake = 0;
            if (phdcount == 0)
            {
                intake = 60;
            }
            else if (phdcount == 1)
            {
                intake = 120;
            }
            else if (phdcount == 2)
            {
                intake = 180;
            }
            else if (phdcount == 3)
            {
                intake = 240;
            }
            else if (phdcount == 4)
            {
                intake = 300;
            }
            else if (phdcount == 5)
            {
                intake = 360;
            }
            else if (phdcount == 6)
            {
                intake = 420;
            }
            return intake;
        }

        public List<CollegeFacultyLabs> DeficienciesInLabs(int? collegeID)
        {
            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            var labMaster = db.jntuh_lab_master.AsNoTracking().ToList();
            var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();
            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(i => i.CollegeID == collegeID).ToList();
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in deficiencies)
            {
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();//degreeType.Equals("PG") ? "No Equipement Uploaded" :

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

                        int[] labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();


                        if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count())
                        {
                            string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(labName))
                                defs.Add(year + "-" + semester + "-" + labName);
                            //else
                            //    defs.Add(null);
                        }
                    }
                });

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = item.degree;
                newFaculty.Department = item.department;
                newFaculty.Specialization = item.specializationName;
                newFaculty.SpecializationId = item.specializationid;
                // newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));
                newFaculty.LabsDeficiency = "NIL";
                lstFaculty.Add(newFaculty);
            }

            return lstFaculty;
        }

        public List<CollegeFacultyLabs> DeficienciesInLabsnew(int? collegeID)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == prAy && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
            List<int> NewspecializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == prAy && e.courseStatus != "Closure" && e.courseStatus == "New" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

            if (DegreeIds.Contains(4))
            {
                specializationIds.Add(39);
            }




            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == collegeID && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
                                                      .Select(l => new Lab
                                                      {
                                                          EquipmentID = l.id,
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
                                                      }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                if (specializationIds.Contains(33) || specializationIds.Contains(43))
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                          .Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
                                                          .Select(l => new Lab
                                                          {
                                                              EquipmentID = l.id,
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
                                                          }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
                }
                else
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                         .Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional") && l.Labcode != "PH105BS")
                                                         .Select(l => new Lab
                                                         {
                                                             EquipmentID = l.id,
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
                                                         }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
                }
            }



            int[] labequipmentIds = collegeLabMaster.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID) && l.isActive == true).Select(i => i.EquipmentID).ToArray();

            int[] DeficiencyEquipmentIds = collegeLabMaster.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).Select(e => e.EquipmentID).ToArray();


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
                lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                if (DeficiencyEquipmentIds.Contains(item.EquipmentID))
                {
                    lstlabs.deficiency = true;
                }
                else
                {
                    lstlabs.deficiency = null;
                    lstlabs.id = 0;
                }
                lstlaboratories.Add(lstlabs);
            }

            lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();

            lstlaboratories = lstlaboratories.GroupBy(l => new { l.deficiency, l.year, l.Semester, l.Labcode, l.specializationId }).Select(s => s.First()).ToList();

            var deficiencies = lstlaboratories.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId }).Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty }).ToList();

            var jntuh_college_laboratories = db.jntuh_college_laboratories.Where(i => i.CollegeID == collegeID && i.isActive == true).ToList();






            //foreach (var item in deficiencies)
            //{

            //    var labsWithDeficiency = lstlaboratories.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
            //       .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();


            //    List<string> defs = new List<string>();
            //    labsWithDeficiency.ForEach(l =>
            //    {

            //        string[] strLab = l.Split('-');

            //        int specializationid = Convert.ToInt32(strLab[3]);
            //        int year = Convert.ToInt32(strLab[0]);
            //        int semester = Convert.ToInt32(strLab[1]);
            //        string labCode = strLab[2].Replace("$", "-");

            //        var requiredLabs = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.EquipmentID).ToList();
            //        int requiredCount = requiredLabs.Count();
            //        int availableCount = jntuh_college_laboratories.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

            //        int[] labmasterids = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.EquipmentID).Distinct().ToArray();
            //        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();

            //        if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count())//&& labCode!="14LAB"
            //        {
            //            string labName = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
            //            if (!string.IsNullOrEmpty(labName))
            //                defs.Add(year + "-" + semester + "-" + labName);
            //            else
            //                defs.Add(null);
            //        }

            //    });







            //    CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
            //    newFaculty.Degree = item.degree;
            //    newFaculty.Department = item.department;
            //    newFaculty.Specialization = item.specializationName;
            //    newFaculty.SpecializationId = item.specializationid;
            //    if (NewspecializationIds.Contains(item.specializationid))
            //    {
            //        newFaculty.LabsDeficiency = "NIL";
            //    }
            //    else
            //    {
            //        newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));
            //    }


            //    lstFaculty.Add(newFaculty);
            //}


            return lstFaculty;
        }

        public class CollegeFacultyLabs
        {
            public int SNo { get; set; }
            public int DegreeDisplayOrder { get; set; }
            public string DegreeType { get; set; }
            public string Degree { get; set; }
            public int DegreeId { get; set; }
            public string Department { get; set; }
            public int DepartmentId { get; set; }
            public string Specialization { get; set; }
            public int SpecializationId { get; set; }
            public string Shift { get; set; }
            public int ShiftId { get; set; }
            public int TotalIntake { get; set; }
            public int DollerCourseIntake { get; set; }
            public int Required { get; set; }
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
            public int AvailablePhdFaculty { get; set; }
            public int RequiredPhdFaculty { get; set; }
            public bool ispercentage { get; set; }
            public int NTPLFaculty { get; set; }
            public int CSEPhDFaculty { get; set; }
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
            public int? AvailableFaculty { get; set; }
            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int approvedIntake5 { get; set; }
            public int totalIntake { get; set; }
            public int totalBtechFirstYearIntake { get; set; }
            public int dollercourseintake { get; set; }
            public int firstYearRequired { get; set; }
            public decimal requiredFaculty { get; set; }
            public decimal ArequiredFaculty { get; set; }
            public decimal SrequiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int NTPLFaculty { get; set; }
            public int CSEPhDFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int oldtotalFaculty { get; set; }
            public int newtotalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }
            public int A416TotalFaculty { get; set; }
            public int form16count { get; set; }
            public int aictecount { get; set; }
            public int adjustedFaculty { get; set; }
            public string minimumRequirementMet { get; set; }

            public int DegreeID { get; set; }
            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }
            public int NOSCM { get; set; }
            public string PHDminimumRequirementMet { get; set; }
            public int DeactivationReasionsCount { get; set; }
            public int FacultyAbsentCount { get; set; }
            public int InvalidPanCount { get; set; }
            public int incompletecerificatesCount { get; set; }
            public int adjointfacultycount { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }
            public string Note { get; set; }
            public string AffliationStatus { get; set; }
            public string collegeRandomCode { get; set; }
            public decimal BphramacyrequiredFaculty { get; set; }
            public decimal pharmadrequiredfaculty { get; set; }
            public decimal pharmadPBrequiredfaculty { get; set; }
            public int totalcollegefaculty { get; set; }
            public int SortId { get; set; }

            public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
            public int BtechAdjustedFaculty { get; set; }
            public int specializationWiseFacultyPHDFaculty { get; set; }

            public string PharmacyGroup1 { get; set; }


            public string PharmacySubGroup1 { get; set; }
            public int BPharmacySubGroup1Count { get; set; }
            public int BPharmacySubGroupRequired { get; set; }
            public string BPharmacySubGroupMet { get; set; }
            public int PharmacyspecializationWiseFaculty { get; set; }

            public string PharmacySpec1 { get; set; }
            public string PharmacySpec2 { get; set; }

            public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }

            public bool AffiliationStatus2 { get; set; }
            public bool AffiliationStatus3 { get; set; }
            public bool AffiliationStatus4 { get; set; }

            //Added this in 25-04-2017
            public int admittedIntake1 { get; set; }
            public int admittedIntake2 { get; set; }
            public int admittedIntake3 { get; set; }
            public int admittedIntake4 { get; set; }
            public int admittedIntake5 { get; set; }

            //Added 02-03-2019
            public int Proposedintake { get; set; }
            public int AICTESanctionIntake1 { get; set; }
            public int AICTESanctionIntake2 { get; set; }
            public int AICTESanctionIntake3 { get; set; }
            public int AICTESanctionIntake4 { get; set; }
            public int AICTESanctionIntake5 { get; set; }

            public int ExambranchIntake_R1 { get; set; }
            public int ExambranchIntake_R2 { get; set; }
            public int ExambranchIntake_R3 { get; set; }
            public int ExambranchIntake_R4 { get; set; }
            public int ExambranchIntake_R5 { get; set; }

            public int ExambranchIntake_L1 { get; set; }
            public int ExambranchIntake_L2 { get; set; }
            public int ExambranchIntake_L3 { get; set; }
            public int ExambranchIntake_L4 { get; set; }
            public int ExambranchIntake_L5 { get; set; }

            public int SanctionIntake1 { get; set; }
            public int SanctionIntake2 { get; set; }
            public int SanctionIntake3 { get; set; }
            public int SanctionIntake4 { get; set; }
            public int SanctionIntake5 { get; set; }
            public bool ispercentage { get; set; }
            public bool ishashcourses { get; set; }
            public bool isstarcourses { get; set; }

            public int totalAdmittedIntake { get; set; }
        }
        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }



        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);
            var jntuh_departments = db.jntuh_department.ToList();
            if (collegeId != null)
            {
                var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
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
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.AsNoTracking().Where(i => collegeIDs.Contains(i.collegeId)).ToList();
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
                    newIntake.courseStatus = item.courseStatus;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }

                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                var DeptNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     select new
                                                     {
                                                         DeptId = a.id,
                                                         DeptName = a.departmentName,
                                                         Specid = b.id
                                                     }).ToList();


                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.AsNoTracking().Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();
                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.AsNoTracking().ToList();

                // var registeredFaculty = db.jntuh_registered_faculty.AsNoTracking().Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                //  var registeredFaculty = db.jntuh_registered_faculty.AsNoTracking().Where(rf => strRegnos.Contains(rf.RegistrationNumber)).ToList();


                int[] CollegeIDS = new int[] { 7, 8, 12, 35, 50, 56, 69, 74, 77, 84, 85, 86, 88, 91, 111, 113, 128, 137, 141, 144, 145, 147, 151, 152, 162, 165, 166, 176, 185, 186, 193, 194, 211, 215, 222, 223, 225, 230, 238, 245, 249, 250, 261, 264, 276, 282, 288, 293, 299, 300, 306, 307, 327, 342, 352, 374, 382, 385, 414, 429, 435, 443, 4, 20, 70, 72, 81, 87, 116, 124, 130, 148, 156, 172, 177, 182, 187, 195, 197, 214, 218, 228, 241, 242, 247, 256, 287, 334, 336, 360, 365, 43, 266, 41, 79, 80, 103, 121, 129, 138, 155, 163, 164, 201, 227, 244, 254, 260, 269, 271, 286, 321, 324, 329, 338, 368, 369, 373, 400, 403, 455, 11, 22, 23, 26, 29, 32, 38, 40, 46, 68, 108, 109, 115, 123, 134, 153, 170, 171, 175, 178, 179, 180, 183, 184, 188, 189, 192, 196, 207, 210, 243, 291, 310, 316, 326, 330, 349, 367, 399, 420, 441, 100, 158, 168, 236, 259, 304, 309, 350, 408, 39, 75, 332, 364 };
                //int[] CollegeIDS = new int[] { 26, 40, 72, 121, 141, 157, 193, 225, 243, 256, 273, 286, 308, 371, 414};

                var registeredFaculty = new List<jntuh_registered_faculty>();

                //var BASFlagFacultyList =
                //    db.jntuh_registered_faculty.Where(e => e.BAS == "Yes")
                //        .Select(e => e.RegistrationNumber)
                //        .ToArray();


                //2017-18 Principal Consider as Faculty if Principal was Added in College_faculty Table.
                //commented by suresh
                //if (CollegeIDS.Contains((int)collegeId))
                //{
                registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //registeredFaculty =
                //    registeredFaculty.Where(r => !BASFlagFacultyList.Contains(r.RegistrationNumber))
                //        .Select(r => r)
                //        .ToList();
                //}
                //else
                //{
                //    registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                //    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //}

                //Reg nos related online facultyIds
                //var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.isApproved == null || rf.isApproved == true) && (rf.PANNumber != null || rf.AadhaarNumber != null))
                //                                 .Select(rf => new

                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();
                var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                // var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                        && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new



                ////Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                            && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.Xeroxcopyofcertificates == false)) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))

                //Flags Name Changed on 02-04-2018 -OriginalsVerifiedPHD Colunm Consider as -NoGuideSigninPHDThesis;OriginalsVerifiedUG Column Consider as -ComplaintPHDFaculty 
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                        {
                            FacultyId = rf.id,
                            RegistrationNumber = rf.RegistrationNumber.Trim(),
                            DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                            //HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e=>e.educationId!=8).Select(e => e.educationId).Max() : 0,
                            HighestDegreeID = rf.NotconsideredPHD == true ?
                                                                        rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                        rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                                                                        :
                                                                        rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                        rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                            IsApproved = rf.isApproved,
                            PanNumber = rf.PANNumber,
                            AadhaarNumber = rf.AadhaarNumber,
                            CsePhDFacultyFlag = rf.PhdDeskVerification,
                            NTPLFacultyreg = rf.NotIdentityfiedForanyProgram
                        }).ToList();
                // jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID != 0).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    // Department = rf.Department,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = rf.SpecializationId,
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rf.FacultyId).Count() > 0 ? true : false,
                    CsePhDFacultyFlag = rf.CsePhDFacultyFlag,
                    NTPLFaculty = rf.NTPLFacultyreg
                }).Where(e => e.Department != null).ToList();


                var form16Count = registeredFaculty.Where(i => i.NoForm16 == true).ToList();
                var aictecount = registeredFaculty.Where(i => i.NotQualifiedAsperAICTE == true).ToList();
                #region old code
                //foreach (var item in collegeIntakeExisting)
                //{
                //    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                //    int phdFaculty = 0;
                //    int pgFaculty = 0;
                //    int ugFaculty = 0;
                //    int SpecializationphdFaculty = 0; 
                //    int SpecializationpgFaculty = 0;
                //    intakedetails.collegeId = item.collegeId;
                //    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                //    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                //    intakedetails.Degree = item.Degree;
                //    intakedetails.Department = item.Department;
                //    intakedetails.Specialization = item.Specialization;
                //    intakedetails.specializationId = item.specializationId;
                //    intakedetails.DepartmentID = item.DepartmentID;
                //    intakedetails.shiftId = item.shiftId;

                //    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                //    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                //    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                //    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                //    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
                //    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                //    if (item.Degree == "B.Tech")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                //    }
                //    else if (item.Degree == "B.Pharmacy")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                //                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                //    }
                //    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                //                       Convert.ToDecimal(facultystudentRatio);

                //    }
                //    else if (item.Degree == "M.Pharmacy")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                //    }

                //    //else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
                //    //{
                //    //    intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                //    //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

                //    //}
                //    else if (item.Degree == "MCA")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                //    }
                //    else if (item.Degree == "Pharm.D")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                //                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                //                                    (intakedetails.approvedIntake5);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                //    }
                //    else if (item.Degree == "Pharm.D PB")
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                //    }
                //    else //MAM MTM Pharm.D Pharm.D PB
                //    {
                //        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                //        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                //    }

                //    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                //    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                //    //====================================
                //    // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

                //    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                //    if (strdegreetype == "UG")
                //    {
                //        if (item.Degree == "B.Pharmacy")
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                //        }
                //        //else
                //        //{
                //        //    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
                //        //}
                //        else if (item.Degree == "Pharm.D")
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                //        }
                //        else if (item.Degree == "Pharm.D PB")
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                //        }
                //        else
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                //        }
                //    }

                //    if (strdegreetype == "PG")
                //    {
                //        //if (item.Degree == "M.Pharmacy")
                //        //{
                //        //    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                //        //}
                //        //else
                //        //{
                //        //    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                //        //}
                //        if (item.Degree == "M.Pharmacy")
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                //                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                //        }
                //        else
                //        {
                //            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                //                        f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                //        }
                //    }



                //     if (strdegreetype == "Dual Degree")
                //    {
                //        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                //    }


                //    intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                //    if (intakedetails.id > 0)
                //    {
                //        int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                //        if (swf != null)
                //        {
                //            intakedetails.specializationWiseFaculty = (int)swf;
                //        }
                //        intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                //        intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                //    }

                //    //============================================

                //    int noPanOrAadhaarcount = 0;
                //    #region old code
                //    //if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
                //    //{
                //    //    ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
                //    //    pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
                //    //    phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
                //    //    noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //    //    intakedetails.Department = "Pharmacy";
                //    //}
                //    //else
                //    //{
                //    //    ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
                //    //    pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
                //    //    phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
                //    //    noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //    //}
                //    #endregion

                //    #region New Code
                //    if (item.Degree == "B.Pharmacy")
                //    {
                //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //        //intakedetails.Department = "Pharmacy";
                //    }
                //    if (item.Degree == "M.Pharmacy")
                //    {
                //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                //        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                //                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                //        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                //        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                //        //            (f.isApproved == null || f.isApproved == true)).Count();
                //        //intakedetails.Department = "Pharmacy";
                //    }
                //    else if (item.Degree == "Pharm.D")
                //    {
                //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //        intakedetails.Department = "Pharm.D";
                //    }
                //    else if (item.Degree == "Pharm.D PB")
                //    {
                //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //        intakedetails.Department = "Pharm.D PB";
                //    }
                //    else
                //    {
                //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                //        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                //        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                //        SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //        var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                //        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));

                //    }
                //    #endregion




                //    intakedetails.phdFaculty = phdFaculty;
                //    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                //    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                //    intakedetails.pgFaculty = pgFaculty;
                //    intakedetails.ugFaculty = ugFaculty;
                //    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                //    intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree ||
                //                i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                //    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                //    //=============//

                //    intakedetailsList.Add(intakedetails);
                //}
                #endregion

                int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                {
                    var intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;
                    intakedetails.ispercentage = false;
                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;

                    if (item.Specialization == "Industrial Pharmacy")
                    {

                    }
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;
                    //intakedetails.ishashcourses = false;
                    item.courseStatus = db.jntuh_college_intake_existing.Where(
                        e =>
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                            e.academicYearId == AY1 &&
                            e.collegeId == item.collegeId).Select(s => s.courseStatus).FirstOrDefault();
                    intakedetails.dollercourseintake = db.jntuh_college_intake_existing.Where(
                       e =>
                           e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                           e.academicYearId == AY2 &&
                           e.collegeId == item.collegeId).Select(s => s.approvedIntake).FirstOrDefault();
                    var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
                    if (status.Count > 0)
                    {
                        intakedetails.AffliationStatus = "A";
                    }
                    intakedetails.approvedIntake1 = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.Proposedintake = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake1 = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake2 = GetIntake1(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake3 = GetIntake1(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake4 = GetIntake1(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake5 = GetIntake1(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    intakedetails.AICTESanctionIntake1 = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake2 = GetIntake1(item.collegeId, AY2, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake3 = GetIntake1(item.collegeId, AY3, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake4 = GetIntake1(item.collegeId, AY4, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake5 = GetIntake1(item.collegeId, AY5, item.specializationId, item.shiftId, 2, item.degreeID);

                    //Getting exmationation Brach intake 
                    intakedetails.ExambranchIntake_R1 = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R2 = GetIntake1(item.collegeId, AY2, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R3 = GetIntake1(item.collegeId, AY3, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R4 = GetIntake1(item.collegeId, AY4, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R5 = GetIntake1(item.collegeId, AY5, item.specializationId, item.shiftId, 3, item.degreeID);

                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);


                    //if (item.degreeID == 4)
                    //{
                    intakedetails.SanctionIntake1 = GetIntake1(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake2 = GetIntake1(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake3 = GetIntake1(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake4 = GetIntake1(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake5 = GetIntake1(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    //}



                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                    if (item.Degree == "B.Tech")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                        //                            (intakedetails.approvedIntake4);
                        //New Code Highest Admitted Intake
                        //int SanctionIntakeHigest = Max(intakedetails.approvedIntake2, intakedetails.approvedIntake3, intakedetails.approvedIntake4);
                        //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                        //if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                        //{
                        //    //New Code 
                        //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                        //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                        //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                        //}
                        //else
                        //{
                        //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        //}

                        //intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                        //if (intakedetails.admittedIntake2 == 0)
                        //    intakedetails.admittedIntake2 = 60;
                        //intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                        //if (intakedetails.admittedIntake3 == 0)
                        //    intakedetails.admittedIntake3 = 60;
                        //intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);
                        //if (intakedetails.admittedIntake4 == 0)
                        //    intakedetails.admittedIntake4 = 60;

                        //Take Higest of 3 Years Of Admitated Intake
                        //approvedIntake1 means Proposed Intake of Present Year
                        #region This Code is Commented on 16-04-2019
                        //int SanctionIntakeHigest = Max(intakedetails.approvedIntake2, intakedetails.approvedIntake3, intakedetails.approvedIntake4);
                        //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                        //25% Calculation Based on Admitted Intake intakes on 418 commented on 21-04-2019
                        //int senondyearpercentage = 0;
                        //int thirdyearpercentage = 0;
                        //int fourthyearpercentage = 0;
                        //if (intakedetails.SanctionIntake2 != 0)
                        //{
                        //    senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake3 != 0)
                        //{
                        //    thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake4 != 0)
                        //{
                        //    fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                        //}

                        //if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                        //{
                        //    intakedetails.ispercentage = true;
                        //    if ((intakedetails.approvedIntake2 >= studentcount ||
                        //         intakedetails.approvedIntake3 >= studentcount ||
                        //         intakedetails.approvedIntake4 >= studentcount) && intakedetails.approvedIntake1 != 0)
                        //    {
                        //        intakedetails.ispercentage = false;
                        //        intakedetails.ishashcourses = true;
                        //    }

                        //}
                        #endregion
                        int senondyearpercentage = 0;
                        int thirdyearpercentage = 0;
                        int fourthyearpercentage = 0;
                        if (CollegeAffiliationStatus == "Yes")
                        {
                            intakedetails.ispercentage = false;
                        }
                        else
                        {
                            if (intakedetails.SanctionIntake2 != 0)
                            {
                                senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                            }
                            if (intakedetails.SanctionIntake3 != 0)
                            {
                                thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                            }
                            if (intakedetails.SanctionIntake4 != 0)
                            {
                                fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                            }

                            //if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                            //{
                            //    intakedetails.ispercentage = true;
                            //    if ((intakedetails.ExambranchIntake_R2 >= studentcount ||
                            //         intakedetails.ExambranchIntake_R3 >= studentcount ||
                            //         intakedetails.ExambranchIntake_R4 >= studentcount) && intakedetails.Proposedintake != 0)
                            //    {
                            //        intakedetails.ispercentage = false;
                            //        intakedetails.ishashcourses = true;
                            //        //Adding on 26-04-2019 If above 60 Proposed intake we Reduced to 60 Proposed Intake.
                            //        //This code commented on 25-05-2019
                            //        //if (intakedetails.Proposedintake>60)
                            //        //{
                            //        //    intakedetails.Proposedintake = 60;
                            //        //}
                            //    }
                            //}
                        }
                        if (intakedetails.ispercentage == false)
                        {
                            //if (item.courseStatus == "New")
                            //{
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //    intakedetails.admittedIntake3 = 0;
                            //    intakedetails.admittedIntake4 = 0;
                            //}
                            //else if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                            //{
                            //    //New Code 
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                            //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                            //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                            //}
                            //else
                            //{
                            //    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            //    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //}
                            //Getting from web.config AICTE Sanctioned-1 or JNTU Sanctioned-2 or Admitted Intake-3
                            int takecondition = Convert.ToInt32(ConfigurationManager.AppSettings["intakecondition"]);
                            if (item.courseStatus == "NewIncrease")
                            {
                                facultyRatio = 3;
                            }

                            else if (item.courseStatus == "TwoYears")
                            {
                                //This Condition is for which course have only last Two Years we take only last Two Years Intake Based Faculty Ration written by Narayana Reddy.                                
                                if (takecondition == 1)
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake =
                                    //        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) +
                                    //        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                    GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                                }
                                else if (takecondition == 2)
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                    //                                GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                                }
                                else
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                    //                                GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                                }
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                               Convert.ToDecimal(facultystudentRatio);
                            }
                            else
                            {
                                if (takecondition == 1)//AICTE
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                    //                        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                            GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                                }
                                else if (takecondition == 2)//JNTU
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                    //                              GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                    //                              GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                                  GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                                                  GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                                }
                                else//Admitted
                                {
                                    //if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    //}
                                    //else
                                    //{
                                    //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                    //                            GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                    //                            GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                                    //}
                                    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                                GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                                GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                                }
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                                    Convert.ToDecimal(facultystudentRatio);
                            }

                        }

                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        if (item.Degree == "M.Tech" && item.shiftId == 1)
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                        }
                        if (item.Degree == "M.Tech" && item.shiftId == 2)
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                            facultyRatio = facultyRatio / 2;
                        }
                        if (item.Degree == "MBA")
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }
                        //Old Code
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //               Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3);
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2); //+(intakedetails.AICTESanctionIntake3);

                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                    }
                    else //MAM MTM
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                        //                            (intakedetails.approvedIntake5);
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) +
                                                   (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4) +
                                                   (intakedetails.AICTESanctionIntake5);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                        }
                        else
                        {

                            //var regno = jntuh_registered_faculty.Where(f => f.Department == item.Department).Select(f => f.RegistrationNumber);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.CsePhDFacultyFlag != true);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                    }
                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                    }
                    //intakedetails.id =
                    //    jntuh_college_faculty_deficiency.Where(
                    //        fd =>
                    //            fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId &&
                    //            fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    //if (intakedetails.id > 0)
                    //{
                    //    int? swf =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.SpecializationWiseFaculty)
                    //            .FirstOrDefault();
                    //    if (swf != null)
                    //    {
                    //        intakedetails.specializationWiseFaculty = (int) swf;
                    //    }
                    //    intakedetails.deficiency =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Deficiency)
                    //            .FirstOrDefault();
                    //    intakedetails.shortage =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Shortage)
                    //            .FirstOrDefault();
                    //}

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);
                        //intakedetails.Department = "Pharmacy";
                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);

                        //if (item.Degree == "M.Tech")
                        //    phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department && f.SpecializationId == item.specializationId);
                        //else
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department);

                        //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                        //if (item.Department == "MBA")
                        //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                        // string REG=

                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);


                        // SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        //var ugFacultydept = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();
                        //var pgfacltydept = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();
                        //var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));

                    }
                    intakedetails.NTPLFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.NTPLFaculty == true);
                    intakedetails.CSEPhDFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.CsePhDFacultyFlag == true);
                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree ||
                                 i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }
                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                int btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
                        int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Count();
                        int phdFaculty = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == department).Count();

                        //int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        //if (facultydeficiencyId == 0)
                        //{
                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            Department = department,
                            Specialization = department,
                            ugFaculty = ugFaculty,
                            pgFaculty = pgFaculty,
                            phdFaculty = phdFaculty,
                            totalFaculty = ugFaculty + pgFaculty + phdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == deptid) : 0,
                            aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == deptid) : 0,
                            A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            Proposedintake = 1
                        });
                        //}
                        //else
                        //{
                        //    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        //    bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
                        //    int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
                        //    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
                        //}
                    }
                }
            }

            return intakedetailsList;
        }

        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.AsNoTracking().Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();


            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();


            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                          .Where(l => specializationIds.Contains(l.SpecializationID))
                                                          .Select(l => new Lab
                                                          {

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
            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                          .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
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

            }



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
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Deficiency).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Id).FirstOrDefault();
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

        //private int GetIntake1(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        //{
        //    int intake = 0;

        //    //Degree B.Tech  
        //    if (DegreeId == 4)
        //    {
        //        //admitted
        //        if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 2 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 3)//Exam Branch Intake Regular Intake
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 11)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else//approved
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //        }
        //    }
        //    else
        //    {
        //        //admitted
        //        if (flag == 1 && academicYearId != 11)
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 11)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else if (flag == 2) //AICTE
        //        {
        //            if (academicYearId == 11)
        //            {
        //                intake = 0;
        //            }
        //            else
        //            {
        //                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
        //            }
        //        }
        //        else //JNTU Approved
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //        }
        //    }
        //    return intake;
        //}

        private int GetIntake1(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;

            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 14 || academicYearId == 13 || academicYearId == 12 || academicYearId == 11 || academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 2 && (academicYearId == 14 || academicYearId == 13 || academicYearId == 12 || academicYearId == 11 || academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();

                }
                else if (flag == 3)//Exam Branch Intake Regular Intake
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntakeasperExambranch_R).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 15)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else//approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //admitted
                if (flag == 1 && academicYearId != 15)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 15)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 2) //AICTE
                {
                    if (academicYearId == 15)
                    {
                        intake = 0;
                    }
                    else
                    {
                        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
                    }
                }
                else //JNTU Approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            return intake;
        }

        //private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        //{
        //    int intake = 0;

        //    ////approved
        //    //if (flag == 1)
        //    //{
        //    //    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

        //    //}
        //    //else //admitted
        //    //{
        //    //    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //    //}

        //    if (flag == 1 && academicYearId != 8)
        //    {
        //        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

        //    }
        //    else if (flag == 1 && academicYearId == 8)
        //    {
        //        var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
        //        if (inta != null)
        //        {
        //            intake = (int)inta.proposedIntake;
        //        }

        //    }
        //    else //admitted
        //    {
        //        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //    }



        //    return intake;
        //}

        //private int GetIntake1(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        //{
        //    int intake = 0;

        //    //Degree B.Tech  
        //    if (DegreeId == 4)
        //    {
        //        //admitted
        //        if (flag == 1 && (academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 10)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else   //approved
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //        }
        //    }
        //    else
        //    {
        //        //approved
        //        if (flag == 1 && academicYearId != 10)
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

        //        }
        //        else if (flag == 1 && academicYearId == 10)
        //        {
        //            var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
        //            if (inta != null)
        //            {
        //                intake = Convert.ToInt32(inta.proposedIntake);
        //            }

        //        }
        //        else //admitted
        //        {
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //        }
        //    }
        //    return intake;
        //}

        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }

        private int GetBtechAdmittedIntake(int Intake)
        {
            int BtechAdmittedIntake = 0;
            if (Intake > 0 && Intake <= 60)
            {
                BtechAdmittedIntake = 60;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                BtechAdmittedIntake = 120;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                BtechAdmittedIntake = 180;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                BtechAdmittedIntake = 240;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                BtechAdmittedIntake = 300;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                BtechAdmittedIntake = 360;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                BtechAdmittedIntake = 420;
            }
            return BtechAdmittedIntake;
        }

        private int Max(int AdmittedIntake2, int AdmittedIntake3, int AdmittedIntake4)
        {
            return Math.Max(AdmittedIntake2, Math.Max(AdmittedIntake3, AdmittedIntake4));
        }

        private int IntakeWisePhdForBtech(int Intake)
        {
            int Phdcount = 0;
            if (Intake >= 0 && Intake <= 60)
            {
                Phdcount = 0;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 1;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 2;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 3;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 4;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 5;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 6;
            }
            return Phdcount;
        }

        private int IntakeWisePhdForMtech(int Intake, int shiftid)
        {
            int Phdcount = 0;
            if (shiftid == 1)
            {
                if (Intake >= 0 && Intake <= 30)
                {
                    Phdcount = 2;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 4;
                }
            }
            else
            {
                if (Intake >= 0 && Intake <= 30)
                {
                    Phdcount = 1;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 2;
                }
            }

            return Phdcount;
        }



        private int IntakeWisePhdForMBAandMCA(int Intake)
        {
            int Phdcount = 0;
            if (Intake >= 0 && Intake <= 60)
            {
                Phdcount = 1;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 2;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 3;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 4;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 5;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 6;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 7;
            }

            return Phdcount;
        }


        #region ForPharmacyCollegeFaculty

        //        public List<CollegeFacultyLabs> PharmacyDeficienciesInFaculty(int? collegeID)
        //        {
        //            string faculty = string.Empty;

        //            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //            faculty += "<tr>";
        //            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
        //            faculty += "</tr>";
        //            faculty += "</table>";

        //            List<CollegeFacultyWithIntakeReport> facultyCounts = PharmacyCollegeFaculty(collegeID).Where(c => c.shiftId == 1).OrderBy(i => i.SortId).ToList();//Where(c => c.shiftId == 1)

        //            #region Facultydata Starting
        //            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeID).ToList();
        //            string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //            var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeID).ToList();
        //            var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

        //            //education categoryIds UG,PG,PHD...........

        //            var jntuh_specializations = db.jntuh_specialization.ToList();
        //            var jntuh_departments = db.jntuh_department.ToList();
        //            int pharmacyDeptId = jntuh_departments.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
        //            var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
        //            var jntuh_education_category = db.jntuh_education_category.ToList();

        //            //var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
        //            //    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();


        //            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
        //            var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
        //            //Reg nos related online facultyIds
        //            var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
        //                                                    && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && rf.BASStatusOld == "Y"))
        //                                                    .Select(rf => new
        //                                                    {
        //                                                        RegistrationNumber = rf.RegistrationNumber,
        //                                                        Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
        //                                                        //Department=
        //                                                        HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                                        IsApproved = rf.isApproved,
        //                                                        PanNumber = rf.PANNumber,
        //                                                        AadhaarNumber = rf.AadhaarNumber,
        //                                                        PGSpecializationId = rf.PGSpecialization,
        //                                                        UGDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault(),
        //                                                        SpecializationId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.SpecializationId).FirstOrDefault(),
        //                                                        jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
        //                                                    }).ToList();
        //            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
        //            var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
        //            {
        //                RegistrationNumber = rf.RegistrationNumber,
        //                Department = rf.Department,
        //                //Department=rf.UGDepartmentId!=null?jntuh_departments.Where(D=>D.id==rf.UGDepartmentId).Select(D=>D.departmentName).FirstOrDefault():"",
        //                PGSpecializationId = rf.PGSpecializationId,
        //                UGDepartmentId = rf.UGDepartmentId,
        //                HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //                Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //                SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
        //                PanNumber = rf.PanNumber,
        //                AadhaarNumber = rf.AadhaarNumber,
        //                //registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
        //                registered_faculty_specialization = rf.SpecializationId != null ? jntuh_specializations.Where(S => S.id == rf.SpecializationId).Select(S => S.specializationName).FirstOrDefault() : rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : "",
        //            }).ToList();
        //            #endregion Facultydata Ending
        //            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
        //            if (collegeStatus != null)
        //            {
        //                if (collegeStatus.SIStatus == true)
        //                {
        //                    facultyCounts = facultyCounts.Where(e => e.Degree != "M.Pharmacy").Select(e => e).ToList();
        //                }
        //            }
        //            var count = facultyCounts.Count();
        //            var distDeptcount = 1;
        //            var deptloop = 1;
        //            var specloop = 1;
        //            decimal departmentWiseRequiredFaculty = 0;

        //            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

        //            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
        //            var degrees = db.jntuh_degree.ToList();
        //            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
        //            int remainingFaculty = 0;
        //            int remainingPHDFaculty = 0;
        //            var remainingphramdFaculty = 0;
        //            var distSpeccount = 0;
        //            var totalusedfaculty = 0;
        //            var remainingmpharmacyfaculty = 0;

        //            //if (PharmDRequiredFaculty == 0)
        //            //{
        //            //    var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(ViewBag.BpharmacyrequiredFaculty);
        //            //    remainingmpharmacyfaculty = facultycount;
        //            //}
        //            if (TotalcollegeFaculty > 0)
        //            {
        //                var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                remainingmpharmacyfaculty = facultycount;
        //                if (ViewBag.PharmDrequiredFaculty > 0)
        //                {
        //                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDRequiredFaculty);
        //                }
        //                if (ViewBag.PharmDPBrequiredFaculty > 0)
        //                {
        //                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDPBRequiredFaculty);
        //                }

        //                if (remainingmpharmacyfaculty < 0)
        //                {
        //                    remainingmpharmacyfaculty = 0;
        //                }
        //            }
        //            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
        //            faculty += "<tr>";
        //            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
        //            //faculty += "<th style='text-align: left; vertical-align: top;' >Status</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Pharmacy Specializations *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
        //            //faculty += "<th style='text-align: center; vertical-align: top;' >Adjusted faculty</th>";
        //            //faculty += "<th style='text-align: center; vertical-align: top;' >Not Qualified as per AICTE faculty</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No. of Ph.D faculty</th>";
        //            faculty += "</tr>";
        //            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
        //            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
        //            string str = "";
        //            int GroupCount1 = 0, GroupCount2 = 0, GroupCount3 = 0, GroupCount4 = 0;
        //            int Group1Assignedfaculty = 0, Group2Assignedfaculty = 0, Group3Assignedfaculty = 0, Group4Assignedfaculty = 0;
        //            foreach (var item in facultyCounts)
        //            {
        //                var pharmadsubgroupmet = "";
        //                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
        //                distSpeccount = facultyCounts.Where(d => d.Specialization == item.Specialization && d.Degree == item.Degree).Distinct().Count();
        //                int indexnow = facultyCounts.IndexOf(item);

        //                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
        //                {
        //                    deptloop = 1;
        //                }
        //                if (indexnow > 0 && facultyCounts[indexnow].Specialization != facultyCounts[indexnow - 1].Specialization)
        //                {
        //                    specloop = 1;
        //                }
        //                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

        //                string minimumRequirementMet = string.Empty;
        //                int facultyShortage = 0;
        //                int adjustedFaculty = 0;
        //                int adjustedPHDFaculty = 0;

        //                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//totalFaculty
        //                int PharmDcount = facultyCounts.Count(F => F.Degree == "Pharm.D");
        //                int rFaculty = 0;
        //                int rFaculty1 = 0;
        //                if (PharmDcount > 0)
        //                {
        //                    int facultyCount = facultyCounts.Where(F => F.Degree == "Pharm.D" && F.BPharmacySubGroupMet != null).Select(F => F.BPharmacySubGroup1Count).FirstOrDefault();
        //                    if ((int)Math.Ceiling(item.requiredFaculty) > 0)
        //                    {
        //                        rFaculty1 = (int)Math.Ceiling(item.requiredFaculty);
        //                    }

        //                }

        //                if (departments.Contains(item.Department))
        //                {
        //                    rFaculty = (int)firstYearRequired;
        //                    departmentWiseRequiredFaculty = (int)firstYearRequired;
        //                }

        //                var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();


        //                if (item.Degree == "Pharm.D")//&& @ViewBag.BpharmcyCondition == "No"
        //                {
        //                    tFaculty = item.totalcollegefaculty;
        //                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

        //                    var pharmadreqfaculty = (int)Math.Ceiling(item.pharmadrequiredfaculty);
        //                    //if (deptloop == 1)
        //                    //{
        //                    if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadreqfaculty) && (item.BPharmacySubGroup1Count >= (pharmadreqfaculty / 2)) && str == "")
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        adjustedFaculty = tFaculty - rFaculty;

        //                        if (adjustedFaculty > pharmadreqfaculty)
        //                        {
        //                            remainingphramdFaculty = adjustedFaculty - pharmadreqfaculty;
        //                            adjustedFaculty = pharmadreqfaculty;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (PharmacyandPharmDMeet == "Yes")
        //                            minimumRequirementMet = "NO";
        //                        else
        //                            minimumRequirementMet = "YES";
        //                        remainingphramdFaculty = tFaculty - rFaculty;
        //                    }
        //                    //}
        //                }

        //                if (item.Degree == "Pharm.D PB")//&& @ViewBag.BpharmcyCondition == "No" && @ViewBag.PharmaDCondition == "No"
        //                {
        //                    tFaculty = item.totalcollegefaculty;
        //                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

        //                    var pharmadpbreqfaculty = (int)Math.Ceiling(item.pharmadPBrequiredfaculty);
        //                    //if (deptloop == 1)
        //                    //{
        //                    if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadpbreqfaculty))
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        adjustedFaculty = tFaculty - rFaculty;

        //                        if (remainingphramdFaculty > pharmadpbreqfaculty)
        //                        {
        //                            adjustedFaculty = pharmadpbreqfaculty;
        //                            remainingphramdFaculty = remainingphramdFaculty - pharmadpbreqfaculty;
        //                        }
        //                        else
        //                        {
        //                            adjustedFaculty = remainingphramdFaculty;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (PharmacyandPharmDMeet == "Yes")
        //                            minimumRequirementMet = "NO";
        //                        else
        //                            minimumRequirementMet = "YES";
        //                    }
        //                    //}
        //                }



        //                if (item.Degree == "B.Pharmacy" && indexnow > 0 && item.PharmacySubGroup1 != "SubGroup6")
        //                {
        //                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
        //                    rFaculty = item.BPharmacySubGroupRequired;
        //                    if (deptloop == 1)
        //                    {
        //                        if (rFaculty <= tFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = tFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            facultyShortage = rFaculty - tFaculty;
        //                        }

        //                        remainingPHDFaculty = item.phdFaculty;

        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = remainingFaculty + (tFaculty - rFaculty);
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else if (tFaculty >= rFaculty && rFaculty != 0)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = rFaculty;
        //                            facultyShortage = rFaculty - remainingFaculty;
        //                            //remainingFaculty = 0;
        //                        }
        //                        else if (tFaculty < rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            remainingFaculty = remainingFaculty - tFaculty;
        //                        }
        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }

        //                }

        //                else if (item.Degree == "B.Pharmacy" && indexnow == 0 && item.PharmacySubGroup1 != "SubGroup6")
        //                {
        //                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
        //                    rFaculty = item.BPharmacySubGroupRequired;
        //                    if (deptloop == 1)
        //                    {
        //                        if (rFaculty <= tFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = tFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            facultyShortage = rFaculty - tFaculty;
        //                        }

        //                        remainingPHDFaculty = item.phdFaculty;

        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = remainingFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else if (tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = remainingFaculty;
        //                            facultyShortage = rFaculty - remainingFaculty;
        //                            remainingFaculty = 0;
        //                        }
        //                        else if (tFaculty < rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            //remainingFaculty = remainingFaculty - tFaculty;
        //                        }




        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }

        //                }

        //                ////New
        //                //var mpharmremainingfaculty = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                //if (item.Degree == "M.Pharmacy" && PharmDRequiredFaculty <= 0 && item.PharmacyspecializationWiseFaculty >= 1)
        //                //{
        //                //    tFaculty = TotalcollegeFaculty;
        //                //    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                //    //remainingFaculty = tFaculty - rFaculty;

        //                //    var mpharmacyreqfaculty = (int)Math.Ceiling(item.requiredFaculty);
        //                //    if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= mpharmacyreqfaculty))
        //                //    {
        //                //        minimumRequirementMet = "YES";
        //                //        adjustedFaculty = tFaculty - rFaculty;

        //                //        if (remainingFaculty > mpharmacyreqfaculty)
        //                //        {
        //                //            adjustedFaculty = mpharmacyreqfaculty;
        //                //            remainingFaculty = remainingFaculty - mpharmacyreqfaculty;
        //                //        }
        //                //        else
        //                //        {
        //                //            adjustedFaculty = remainingFaculty;
        //                //        }
        //                //    }
        //                //}

        ////New


        //                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& (PharmDRequiredFaculty > 0 || PharmDPBRequiredFaculty > 0)
        //                {
        //                    //tFaculty = (int)Math.Ceiling(Convert.ToDecimal(list[i].specializationWiseFaculty));PharmDRequiredFaculty
        //                    //if (remainingphramdFaculty > 0)
        //                    //{
        //                    //    tFaculty = remainingphramdFaculty;
        //                    //}
        //                    //else
        //                    //{
        //                    //    tFaculty = remainingFaculty;
        //                    //}

        //                    tFaculty = remainingmpharmacyfaculty;
        //                    rFaculty = (int)Math.Ceiling(item.requiredFaculty);


        //                    if (rFaculty <= tFaculty && remainingphramdFaculty > 0)
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        remainingphramdFaculty = remainingphramdFaculty - rFaculty;
        //                        adjustedFaculty = rFaculty;
        //                    }
        //                    else if (rFaculty <= tFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = rFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - rFaculty;
        //                    }

        //                    //else if (tFaculty <= rFaculty && remainingphramdFaculty > 0)
        //                    //{
        //                    //    remainingphramdFaculty = remainingphramdFaculty - rFaculty;
        //                    //    adjustedFaculty = tFaculty;
        //                    //}
        //                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty == 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty == 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                    {
        //                        adjustedPHDFaculty = 1;
        //                        remainingPHDFaculty = remainingPHDFaculty - 1;
        //                    }
        //                }



        //                if ((item.Degree == "B.Pharmacy") && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired)
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";//Yes
        //                    }
        //                }

        //                else if (item.Degree == "Pharm.D" && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(PharmDRequiredFaculty) && bpharmacycondition == "No")
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                        pharmadsubgroupmet = "No";
        //                        //minimumRequirementMet = "NO";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";
        //                        //minimumRequirementMet = "YES";
        //                    }
        //                }

        //                else if (item.Degree == "Pharm.D PB" && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(pharmadpbrequiredfaculty) && bpharmacycondition == "No" && pharmadsubgroupmet == "No")
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                        //minimumRequirementMet = "NO";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";
        //                        //minimumRequirementMet = "YES";
        //                    }
        //                }



        //                if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& @adjustedFaculty == rFaculty
        //                {
        //                    if (bpharmacycondition == "No" && item.phdFaculty >= 1)//&& pharmdcondition == "No" && pharmadpbcondition == "No"
        //                    {
        //                        int Group1facultyCount = facultyCounts.Count(F => F.Degree == "M.Pharmacy" && (F.specializationId == 120 || F.specializationId == 115));
        //                        int Group2facultyCount = facultyCounts.Count(F => F.Degree == "M.Pharmacy" && (F.specializationId == 117 || F.specializationId == 116 || F.specializationId == 123 || F.specializationId == 124));
        //                        int Group3facultyCount = facultyCounts.Count(F => F.Degree == "M.Pharmacy" && (F.specializationId == 122));
        //                        int Group4facultyCount = facultyCounts.Count(F => F.Degree == "M.Pharmacy" && (F.specializationId == 121 || F.specializationId == 117));


        //                        if (item.specializationId == 120 || item.specializationId == 115)
        //                        {
        //                            #region Group1 Conditions Start
        //                            int Pharmaceutics = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 120);
        //                            int IndustrialPharmacy = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 115);
        //                            int MinumGroupRequired = 0, TotalFaculty = 0, PharmacyRequiredFaculty = 0;
        //                            PharmacyRequiredFaculty = facultyCounts.Where(F => F.Degree == "B.Pharmacy" && F.PharmacySpec1 == "Group1 (Pharmaceutics , Industrial Pharmacy)").Select(F => F.BPharmacySubGroupRequired).FirstOrDefault();
        //                            if (Group1facultyCount == 1)
        //                            {
        //                                if (item.specializationId == 120 && GroupCount1 == 0)
        //                                {
        //                                    MinumGroupRequired = 5;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }


        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }

        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {

        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else if (item.specializationId == 115 && GroupCount1 == 0)
        //                                {
        //                                    MinumGroupRequired = 5;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {

        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                            }
        //                            else if (Group1facultyCount == 2)
        //                            {
        //                                if (item.specializationId == 120 && GroupCount1 == 0)
        //                                {
        //                                    MinumGroupRequired = 5;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {

        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else if (item.specializationId == 120 && GroupCount1 == 1)
        //                                {
        //                                    MinumGroupRequired = 6;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {

        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && Pharmaceutics >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else if (item.specializationId == 115 && GroupCount1 == 0)
        //                                {
        //                                    MinumGroupRequired = 5;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {

        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else if (item.specializationId == 115 && GroupCount1 == 1)
        //                                {
        //                                    MinumGroupRequired = 6;
        //                                    TotalFaculty = (Pharmaceutics + IndustrialPharmacy) - PharmacyRequiredFaculty;
        //                                    if (TotalFaculty > 0)
        //                                    {
        //                                        if (PharmacyRequiredFaculty > MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                {
        //                                                    GroupCount1++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }

        //                                        else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                            {
        //                                                if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                                {
        //                                                    Group1Assignedfaculty = Group1Assignedfaculty + Group1Assignedfaculty > 0 ? 1 : 2;
        //                                                    if (TotalFaculty >= Group1Assignedfaculty && IndustrialPharmacy >= 2)
        //                                                    {
        //                                                        GroupCount1++;
        //                                                        if (adjustedFaculty == 0)
        //                                                            adjustedFaculty = 2;
        //                                                        item.BPharmacySubGroupMet = "No Deficiency";
        //                                                        minimumRequirementMet = "NO";
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        minimumRequirementMet = "Yes";
        //                                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }

        //                            }
        //                            #endregion Group1 Conditions End

        //                        }
        //                        else if (item.specializationId == 117 || item.specializationId == 116 || item.specializationId == 123 || item.specializationId == 124)
        //                        {
        //                            #region Group2 Conditions Start
        //                            int PharmaceuticalChemistry = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 117);
        //                            int PAQA = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 116);
        //                            int QA = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 123);
        //                            int QAPRA = facultyCounts.Count(F => F.specializationId == 124);


        //                            int Pharmacognosy = facultyCounts.Count(F => F.specializationId == 117);
        //                            int PharmacyRequiredFaculty = facultyCounts.Where(F => F.Degree == "B.Pharmacy" && F.PharmacySpec1 == "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)").Select(F => F.BPharmacySubGroupRequired).FirstOrDefault();
        //                            int MinumGroupRequired = 0, TotalFaculty = 0;
        //                            if (item.specializationId == 117)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (PAQA + QA + QAPRA + PharmaceuticalChemistry) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                            if (TotalFaculty >= Group2Assignedfaculty && PharmaceuticalChemistry >= 2)
        //                                            {
        //                                                GroupCount2++;
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group2Assignedfaculty && PharmaceuticalChemistry >= 2)
        //                                                {
        //                                                    GroupCount2++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                            else if (item.specializationId == 116)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (PAQA + QA + QAPRA + PharmaceuticalChemistry) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                            if (TotalFaculty >= Group2Assignedfaculty && PAQA >= 2)
        //                                            {
        //                                                GroupCount2++;
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group2Assignedfaculty && PAQA >= 2)
        //                                                {
        //                                                    GroupCount2++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                            else if (item.specializationId == 123)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (PAQA + QA + QAPRA + PharmaceuticalChemistry) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                            if (TotalFaculty >= Group2Assignedfaculty && QA >= 2)
        //                                            {
        //                                                GroupCount2++;
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group2Assignedfaculty && QA >= 2)
        //                                                {
        //                                                    GroupCount2++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                            else if (item.specializationId == 124)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (PAQA + QA + QAPRA + PharmaceuticalChemistry) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                            if (TotalFaculty >= Group2Assignedfaculty && QAPRA >= 2)
        //                                            {
        //                                                GroupCount2++;
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                Group2Assignedfaculty = Group2Assignedfaculty + Group2Assignedfaculty > 0 ? 1 : 2;
        //                                                if (TotalFaculty >= Group2Assignedfaculty && QAPRA >= 2)
        //                                                {
        //                                                    GroupCount2++;
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }

        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }


        //                            #endregion Group2 Conditions End

        //                        }
        //                        else if (item.specializationId == 122)
        //                        {
        //                            #region Group3 Conditions Start
        //                            int MinumGroupRequired = 5;
        //                            int TotalFaculty = 0;
        //                            int PharmacyRequiredFaculty = facultyCounts.Where(F => F.Degree == "B.Pharmacy" && F.PharmacySpec1 == "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)").Select(F => F.BPharmacySubGroupRequired).FirstOrDefault();
        //                            int Pharmacology = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 122);
        //                            int PharmDPB = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 19);
        //                            int PharmD = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 18);
        //                            if (item.specializationId == 122)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (Pharmacology + PharmDPB + PharmD) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty > MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            if (TotalFaculty >= 2)
        //                                            {
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                if (TotalFaculty >= 2)
        //                                                {
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }

        //                            #endregion Group3 Conditions End


        //                        }
        //                        else if (item.specializationId == 121)
        //                        {
        //                            #region Group4 Conditions Start
        //                            int MinumGroupRequired = 5;
        //                            int TotalFaculty = 0;
        //                            int PharmacyRequiredFaculty = facultyCounts.Where(F => F.Degree == "B.Pharmacy" && F.PharmacySpec1 == "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)").Select(F => F.BPharmacySubGroupRequired).FirstOrDefault();
        //                            int Pharmacognosy = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 121);
        //                            int PharmaceuticalChemistry = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 117);
        //                            if (item.specializationId == 121)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = (Pharmacognosy) - PharmacyRequiredFaculty;
        //                                if (TotalFaculty > 0)
        //                                {
        //                                    if (PharmacyRequiredFaculty >= MinumGroupRequired)
        //                                    {
        //                                        if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                        {
        //                                            if (TotalFaculty >= 2)
        //                                            {
        //                                                if (adjustedFaculty == 0)
        //                                                    adjustedFaculty = 2;
        //                                                item.BPharmacySubGroupMet = "No Deficiency";
        //                                                minimumRequirementMet = "NO";
        //                                            }

        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                    {

        //                                        if ((PharmacyRequiredFaculty + TotalFaculty) >= MinumGroupRequired)
        //                                        {
        //                                            if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                            {
        //                                                if (TotalFaculty >= 2)
        //                                                {
        //                                                    if (adjustedFaculty == 0)
        //                                                        adjustedFaculty = 2;
        //                                                    item.BPharmacySubGroupMet = "No Deficiency";
        //                                                    minimumRequirementMet = "NO";
        //                                                }
        //                                                else
        //                                                {
        //                                                    minimumRequirementMet = "Yes";
        //                                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                minimumRequirementMet = "Yes";
        //                                                item.BPharmacySubGroupMet = "Deficiency";
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                            #endregion Group4 Conditions End


        //                        }


        //                        else if (item.specializationId == 114)
        //                        {

        //                            int HospitalClinicalPharmacy = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 114);
        //                            int MinumGroupRequired = 5;
        //                            int TotalFaculty = 0;

        //                            if (item.specializationId == 114)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = HospitalClinicalPharmacy;
        //                                if (TotalFaculty > 0)
        //                                {

        //                                    if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                    {
        //                                        if (TotalFaculty >= 5)
        //                                        {
        //                                            item.BPharmacySubGroupMet = "No Deficiency";
        //                                            minimumRequirementMet = "NO";
        //                                        }

        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }


        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                        }
        //                        else if (item.specializationId == 118)
        //                        {
        //                            int PharmaceuticalManagementRegulatoryAffaires = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 118);
        //                            int MinumGroupRequired = 5;
        //                            int TotalFaculty = 0;
        //                            if (item.specializationId == 118)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = PharmaceuticalManagementRegulatoryAffaires;
        //                                if (TotalFaculty > 0)
        //                                {

        //                                    if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                    {
        //                                        if (TotalFaculty >= 5)
        //                                        {
        //                                            item.BPharmacySubGroupMet = "No Deficiency";
        //                                            minimumRequirementMet = "NO";
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }

        //                        }
        //                        else if (item.specializationId == 119)
        //                        {
        //                            int PharmaceuticalTechnology = jntuh_registered_faculty.Count(F => F.PGSpecializationId == 119);
        //                            int MinumGroupRequired = 5;
        //                            int TotalFaculty = 0;
        //                            if (item.specializationId == 119)
        //                            {
        //                                MinumGroupRequired = 5;
        //                                TotalFaculty = PharmaceuticalTechnology;
        //                                if (TotalFaculty > 0)
        //                                {

        //                                    if (item.PharmacyspecializationWiseFaculty >= 1)
        //                                    {
        //                                        if (TotalFaculty >= 5)
        //                                        {
        //                                            item.BPharmacySubGroupMet = "No Deficiency";
        //                                            minimumRequirementMet = "NO";
        //                                        }
        //                                        else
        //                                        {
        //                                            minimumRequirementMet = "Yes";
        //                                            item.BPharmacySubGroupMet = "Deficiency";
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        minimumRequirementMet = "Yes";
        //                                        item.BPharmacySubGroupMet = "Deficiency";
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    minimumRequirementMet = "Yes";
        //                                    item.BPharmacySubGroupMet = "Deficiency";
        //                                }

        //                            }
        //                        }
        //                        //minimumRequirementMet = "NO";
        //                    }


        //                }
        //                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty < 1)
        //                {
        //                    item.BPharmacySubGroupMet = "Deficiency";
        //                    minimumRequirementMet = "YES";
        //                }
        //                //else if (item.Degree == "M.Pharmacy")
        //                //{
        //                //    item.BPharmacySubGroupMet = "Yes";
        //                //}


        //                faculty += "<tr>";
        //                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";

        //                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Department + "</td>";
        //                if (specloop == 1)
        //                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Degree + "</td>";
        //                if (specloop == 1)
        //                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Specialization + "</td>";


        //                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
        //                if (item.totalIntake > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (Math.Ceiling(item.requiredFaculty) > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 == null)
        //                {
        //                    if (TotalcollegeFaculty > Math.Ceiling(item.requiredFaculty))
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
        //                    }
        //                    else
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + TotalcollegeFaculty + " </td>";
        //                    }

        //                }
        //                else if (item.Degree == "M.Pharmacy" || item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
        //                {
        //                    totalusedfaculty = totalusedfaculty + adjustedFaculty;
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + " </td>";
        //                }
        //                else if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 != null)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
        //                }

        //                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.PharmacySubGroup1 + "</td>";
        //                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
        //                }
        //                //else if (item.Degree == "M.Pharmacy")
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
        //                //}
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }
        //                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
        //                }
        //                else if (item.Degree == "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (item.BPharmacySubGroupMet == null && item.Degree == "B.Pharmacy")
        //                {
        //                    if (bpharmacycondition == "No")
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy No Deficiency.</b></td>";
        //                    }
        //                    else
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy Deficiency Exists & Hence Other Degrees will not be considered. </b></td>";
        //                    }
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupMet + "</td>";
        //                }

        //                if (item.phdFaculty > 0 || item.totalIntake > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                //if (adjustedPHDFaculty > 0)
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //                //}
        //                //else if (item.approvedIntake1 > 0)
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
        //                //}
        //                //else
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //                //}

        //                faculty += "</tr>";

        //                if (minimumRequirementMet == "YES" && item.Degree == "B.Pharmacy")
        //                {
        //                    if (rFaculty <= adjustedFaculty)
        //                        minimumRequirementMet = "NO";
        //                    else
        //                        minimumRequirementMet = "YES";
        //                }

        //                else if (minimumRequirementMet == "NO" && item.Degree == "B.Pharmacy")
        //                {
        //                    if (rFaculty == adjustedFaculty)
        //                        minimumRequirementMet = "NO";
        //                    else
        //                        minimumRequirementMet = "YES";
        //                }

        //                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //                int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
        //                newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
        //                newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //                newFaculty.Degree = item.Degree;
        //                newFaculty.Department = item.Department;
        //                newFaculty.Specialization = item.Specialization;
        //                newFaculty.SpecializationId = item.specializationId;
        //                newFaculty.TotalIntake = item.approvedIntake1;

        //                if (departments.Contains(item.Department))
        //                {
        //                    //newFaculty.TotalIntake = totalBtechFirstYearIntake;
        //                    newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
        //                }
        //                else
        //                {
        //                    // newFaculty.TotalIntake = item.totalIntake;
        //                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
        //                }

        //                newFaculty.Available = adjustedFaculty;
        //                if (item.Degree != "B.Pharmacy" && Allgroupscount > 0 && item.phdFaculty < 0 && item.BPharmacySubGroupMet == "No Deficiency")
        //                {
        //                    newFaculty.Deficiency = "YES";
        //                }
        //                else if (item.Degree != "B.Pharmacy" && Allgroupscount == 0)
        //                {
        //                    newFaculty.Deficiency = minimumRequirementMet;
        //                }
        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    newFaculty.Required = (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                    newFaculty.Available = (int)Math.Ceiling(BpharmacyrequiredFaculty) - Allgroupscount;
        //                    if (DeficiencyInPharmacy == "Deficiency")
        //                        newFaculty.Deficiency = "YES";
        //                    else
        //                        newFaculty.Deficiency = "NO";
        //                    //newFaculty.Deficiency = Allgroupscount > 0 ? "YES" : "NO";
        //                }


        //                if (item.PharmacyspecializationWiseFaculty >= 1 && degreeType.Equals("PG") && item.approvedIntake1 > 0)
        //                {
        //                    newFaculty.AvailablePhdFaculty = item.PharmacyspecializationWiseFaculty;
        //                    newFaculty.PhdDeficiency = "NO";
        //                }
        //                else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
        //                {
        //                    newFaculty.AvailablePhdFaculty = item.PharmacyspecializationWiseFaculty;
        //                    newFaculty.PhdDeficiency = "YES";
        //                }
        //                else
        //                {
        //                    newFaculty.AvailablePhdFaculty = 0;
        //                    newFaculty.PhdDeficiency = "-";
        //                }
        //                if (item.Degree != "B.Pharmacy" && item.Degree != "Pharm.D" && item.Degree != "Pharm.D PB")
        //                {
        //                    if (specloop == 1)
        //                        lstFaculty.Add(newFaculty);
        //                }
        //                else
        //                {
        //                    if ((item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 == null) || (item.Degree == "Pharm.D" && item.PharmacySubGroup1 == null) || (item.Degree == "Pharm.D PB" && item.PharmacySubGroup1 == null))
        //                        lstFaculty.Add(newFaculty);
        //                }

        //                deptloop++;
        //                specloop++;
        //            }


        //            faculty += "</table>";

        //            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //            faculty += "<tr>";
        //            faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy";
        //            faculty += "<td align='left'>* I, II Year for M.Pharmacy";
        //            faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D";
        //            faculty += "<td align='left'>* IV, V Year for Pharm.D PB";
        //            faculty += "</tr>";
        //            faculty += "</table>";

        //            return lstFaculty;
        //        }



        //        public List<CollegeFacultyWithIntakeReport> PharmacyCollegeFaculty(int? collegeId)
        //        {
        //            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
        //            {
        //                collegeId = c.id,
        //                collegeName = c.collegeCode + "-" + c.collegeName
        //            }).OrderBy(c => c.collegeName).ToList();

        //            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

        //            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();



        //            var randomcode = "";
        //            if (collegeId != null)
        //            {
        //                randomcode = db.jntuh_college_randamcodes.FirstOrDefault(i => i.CollegeId == collegeId).RandamCode;
        //            }
        //            var pharmadTotalintake = 0;
        //            var pharmadPBTotalintake = 0;

        //            #region PharmacyCode
        //            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
        //            if (collegeId != null)
        //            {
        //                var jntuh_Bpharmacy_faculty_deficiency = db.jntuh_bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
        //                var jntuh_specialization = db.jntuh_specialization.ToList();

        //                int[] collegeIDs = null;
        //                int facultystudentRatio = 0;
        //                decimal facultyRatio = 0m;
        //                if (collegeId != 0)
        //                {
        //                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
        //                }
        //                else
        //                {
        //                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
        //                }
        //                var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
        //                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
        //                var jntuh_degree = db.jntuh_degree.ToList();

        //                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
        //                foreach (var item in intake)
        //                {
        //                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //                    newIntake.id = item.id;
        //                    newIntake.collegeId = item.collegeId;
        //                    newIntake.academicYearId = item.academicYearId;
        //                    newIntake.shiftId = item.shiftId;
        //                    newIntake.isActive = item.isActive;
        //                    newIntake.nbaFrom = item.nbaFrom;
        //                    newIntake.nbaTo = item.nbaTo;
        //                    newIntake.specializationId = item.specializationId;
        //                    newIntake.Specialization = item.jntuh_specialization.specializationName;
        //                    newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //                    newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //                    newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //                    newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //                    newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //                    newIntake.shiftId = item.shiftId;
        //                    newIntake.Shift = item.jntuh_shift.shiftName;
        //                    collegeIntakeExisting.Add(newIntake);
        //                }
        //                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

        //                //college Reg nos
        //                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

        //                //education categoryIds UG,PG,PHD...........
        //                var jntuh_specializations = db.jntuh_specialization.ToList();
        //                var jntuh_departments = db.jntuh_department.ToList();
        //                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
        //                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
        //                var jntuh_education_category = db.jntuh_education_category.ToList();

        //                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
        //                var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
        //                //Reg nos related online facultyIds
        //                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
        //                                                        && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && rf.BASStatusOld == "Y"))
        //                                                        .Select(rf => new
        //                                                        {
        //                                                            RegistrationNumber = rf.RegistrationNumber,
        //                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
        //                                                            //Department=
        //                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                                            IsApproved = rf.isApproved,
        //                                                            PanNumber = rf.PANNumber,
        //                                                            AadhaarNumber = rf.AadhaarNumber,
        //                                                            PGSpecializationId = rf.PGSpecialization,
        //                                                            UGDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault(),
        //                                                            SpecializationId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.SpecializationId).FirstOrDefault(),
        //                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
        //                                                        }).ToList();
        //                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
        //                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
        //                {
        //                    RegistrationNumber = rf.RegistrationNumber,
        //                    Department = rf.Department,
        //                    //Department=rf.UGDepartmentId!=null?jntuh_departments.Where(D=>D.id==rf.UGDepartmentId).Select(D=>D.departmentName).FirstOrDefault():"",
        //                    PGSpecializationId = rf.PGSpecializationId,
        //                    UGDepartmentId = rf.UGDepartmentId,
        //                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
        //                    PanNumber = rf.PanNumber,
        //                    AadhaarNumber = rf.AadhaarNumber,
        //                    //registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
        //                    registered_faculty_specialization = rf.SpecializationId != null ? jntuh_specializations.Where(S => S.id == rf.SpecializationId).Select(S => S.specializationName).FirstOrDefault() : rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : "",
        //                }).ToList();


        //                var bpharmacyintake = 0;
        //                decimal BpharcyrequiredFaculty = 0;
        //                decimal PharmDrequiredFaculty = 0;
        //                decimal PharmDPBrequiredFaculty = 0;
        //                var pharmacydeptids = new[] { 26, 27, 36, 39 };
        //                if (collegeId == 42)
        //                    jntuh_registered_faculty = jntuh_registered_faculty.Where(R => R.UGDepartmentId == 26 || R.UGDepartmentId == 27 || R.UGDepartmentId == 36 || R.UGDepartmentId == 39).ToList();
        //                collegeIntakeExisting = collegeIntakeExisting.Where(i => pharmacydeptids.Contains(i.DepartmentID)).ToList();
        //                foreach (var item in collegeIntakeExisting)
        //                {
        //                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
        //                    int phdFaculty = 0;
        //                    int pgFaculty = 0;
        //                    int ugFaculty = 0;

        //                    intakedetails.collegeId = item.collegeId;
        //                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                    intakedetails.collegeRandomCode = randomcode;
        //                    intakedetails.Degree = item.Degree;
        //                    intakedetails.Department = item.Department;
        //                    intakedetails.Specialization = item.Specialization;
        //                    intakedetails.specializationId = item.specializationId;
        //                    intakedetails.DepartmentID = item.DepartmentID;
        //                    intakedetails.shiftId = item.shiftId;
        //                    var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
        //                    if (status.Count > 0)
        //                    {
        //                        intakedetails.AffliationStatus = "A";
        //                    }
        //                    intakedetails.approvedIntake1 = GetIntake2(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
        //                    intakedetails.approvedIntake2 = GetIntake2(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
        //                    intakedetails.approvedIntake3 = GetIntake2(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
        //                    intakedetails.approvedIntake4 = GetIntake2(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
        //                    intakedetails.approvedIntake5 = GetIntake2(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);
        //                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

        //                    if (item.Degree == "B.Tech")
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
        //                                                    (intakedetails.approvedIntake4);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                                       Convert.ToDecimal(facultystudentRatio);
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
        //                    }
        //                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                                       Convert.ToDecimal(facultystudentRatio);
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

        //                    }
        //                    else if (item.Degree == "MCA")
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                                    (intakedetails.approvedIntake3);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                                       Convert.ToDecimal(facultystudentRatio);
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                    }
        //                    else if (item.Degree == "B.Pharmacy")
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;
        //                        // intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                        //var total = intakedetails.totalIntake > 400 ? 100 : 60;
        //                        //bpharmacyintake = total;
        //                        int PharmDCount = collegeIntakeExisting.Count(C => C.Degree == "Pharm.D");
        //                        bpharmacyintake = intakedetails.approvedIntake1 >= 100 ? 100 : 60;
        //                        if (PharmDCount > 0)
        //                            intakedetails.requiredFaculty = intakedetails.approvedIntake1 >= 100 ? 35 : 25;
        //                        else
        //                            intakedetails.requiredFaculty = intakedetails.approvedIntake1 >= 100 ? 25 : 15;
        //                        ApprovedIntake = intakedetails.approvedIntake1>=400?100:60;
        //                        specializationId = intakedetails.specializationId;
        //                    }
        //                    else if (item.Degree == "M.Pharmacy")
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

        //                        //intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                        intakedetails.requiredFaculty = 2;

        //                    }
        //                    else if (item.Degree == "Pharm.D")
        //                    {
        //                        intakedetails.totalIntake = pharmadTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                                    (intakedetails.approvedIntake5);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                        PharmaDApprovedIntake = intakedetails.approvedIntake1;
        //                        PharmaDspecializationId = intakedetails.specializationId;
        //                    }
        //                    else if (item.Degree == "Pharm.D PB")
        //                    {
        //                        intakedetails.totalIntake = pharmadPBTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                        PharmaDPBApprovedIntake = intakedetails.approvedIntake1;
        //                        PharmaDPBspecializationId = intakedetails.specializationId;
        //                    }
        //                    else //MAM MTM Pharm.D Pharm.D PB
        //                    {
        //                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                                    (intakedetails.approvedIntake5);
        //                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                    }


        //                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

        //                    //====================================
        //                    // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();



        //                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //                    if (strdegreetype == "UG")
        //                    {
        //                        if (item.Degree == "B.Pharmacy")
        //                        {
        //                            intakedetails.SortId = 1;
        //                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
        //                        }
        //                        else
        //                        {
        //                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
        //                        }
        //                    }

        //                    if (strdegreetype == "PG")
        //                    {
        //                        if (item.Degree == "M.Pharmacy")
        //                        {
        //                            intakedetails.SortId = 4;
        //                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //                            switch (item.specializationId)
        //                            {
        //                                case 114://Hospital & Clinical Pharmacy
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice/Pharm D";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP" || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization == "PHARMD".ToUpper() || f.registered_faculty_specialization == "PHARM D" || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));

        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == 114));
        //                                    //phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
        //                                    break;
        //                                case 116://Pharmaceutical Analysis & Quality Assurance
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharma Chemistry";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA" || f.registered_faculty_specialization == "PA RA" || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 118://Pharmaceutical Management & Regulatory Affaires
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PMRA/Regulatory Affairs/Pharmaceutics";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PMRA".ToUpper() || f.registered_faculty_specialization == "Regulatory Affairs".ToUpper() || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 120://Pharmaceutics
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));//|| f.registered_faculty_specialization == "Pharmaceutics".ToUpper();
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));// || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 122://Pharmacology
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP".ToUpper() || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 124://Quality Assurance & Pharma Regulatory Affairs
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
        //                                    //var s = jntuh_registered_faculty.Where(f => (f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                                    //             f.registered_faculty_specialization == "QA".ToUpper() ||
        //                                    //             f.registered_faculty_specialization == "PA RA".ToUpper() ||
        //                                    //             f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA"))).ToList();
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 115://Industrial Pharmacy
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 121://Pharmacognosy
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 117://Pharmaceutical Chemistry
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 119://Pharmaceutical Technology (2011-12)
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                case 123://Quality Assurance
        //                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
        //                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
        //                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
        //                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
        //                                    break;
        //                                default:
        //                                    intakedetails.PharmacySpec1 = "";
        //                                    intakedetails.PharmacyspecializationWiseFaculty = 0;
        //                                    phdFaculty = 0;
        //                                    break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //                        }
        //                    }

        //                    if (strdegreetype == "Dual Degree")
        //                    {
        //                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //                    }
        //                    intakedetails.id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

        //                    if (intakedetails.id > 0)
        //                    {
        //                        int? swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                        if (swf != null)
        //                        {
        //                            intakedetails.specializationWiseFaculty = (int)swf;
        //                        }
        //                        intakedetails.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                        intakedetails.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
        //                    }

        //                    //============================================

        //                    int noPanOrAadhaarcount = 0;

        //                    if (item.Degree == "B.Pharmacy")
        //                    {
        //                        intakedetails.SortId = 1;
        //                        BpharcyrequiredFaculty = Math.Round(intakedetails.requiredFaculty);
        //                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
        //                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
        //                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
        //                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                        intakedetails.Department = "Pharmacy";
        //                    }
        //                    else if (item.Degree == "M.Pharmacy")
        //                    {
        //                        intakedetails.SortId = 4;
        //                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                        //phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                        intakedetails.Department = "Pharmacy";
        //                    }
        //                    else if (item.Degree == "Pharm.D")
        //                    {
        //                        intakedetails.SortId = 2;
        //                        PharmDRequiredFaculty = PharmDrequiredFaculty = intakedetails.requiredFaculty;
        //                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
        //                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
        //                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                    }
        //                    else if (item.Degree == "Pharm.D PB")
        //                    {
        //                        intakedetails.SortId = 3;
        //                        PharmDPBRequiredFaculty = PharmDPBrequiredFaculty = intakedetails.requiredFaculty;
        //                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                    }
        //                    else
        //                    {
        //                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
        //                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
        //                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
        //                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                    }

        //                    intakedetails.phdFaculty = phdFaculty;
        //                    intakedetails.pgFaculty = pgFaculty;
        //                    intakedetails.ugFaculty = ugFaculty;
        //                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);

        //                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
        //                    //=============//


        //                    //intakedetails.PharmacySpecilaizationList = pharmacyspeclist;
        //                    intakedetailsList.Add(intakedetails);
        //                }
        //            #endregion

        //                var pharmdspeclist = new List<PharmacySpecilaizationList>
        //                {
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacy Practice",
        //                    //    Specialization = "Pharm.D"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharm D",
        //                    //    Specialization = "Pharm.D"
        //                    //}
        //                    new PharmacySpecilaizationList()
        //                    {
        //                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)",
        //                        Specialization = "Pharm.D"
        //                    }
        //                };
        //                var pharmdpbspeclist = new List<PharmacySpecilaizationList>
        //                {
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacy Practice",
        //                    //    Specialization = "Pharm.D PB"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharm D",
        //                    //    Specialization = "Pharm.D PB"
        //                    //}
        //                    new PharmacySpecilaizationList()
        //                    {
        //                       PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)",
        //                        Specialization = "Pharm.D PB"
        //                    }
        //                };

        //                var pharmacyspeclist = new List<PharmacySpecilaizationList>
        //                {
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmaceutics",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Industrial Pharmacy",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacy BioTechnology",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmaceutical Technology",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmaceutical Chemistry",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacy Analysis",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},

        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "PAQA",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacology",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    ////new PharmacySpecilaizationList()
        //                    ////{
        //                    ////    PharmacyspecName = "Pharma D",
        //                    ////    Specialization = "B.Pharmacy"
        //                    ////},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Pharmacognosy",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "English",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Mathematics",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Computers",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Computer Science",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Zoology",
        //                    //    Specialization = "B.Pharmacy"
        //                    //}


        //                     new PharmacySpecilaizationList()
        //                    {
        //                        PharmacyspecName = "Group1 (Pharmaceutics , Industrial Pharmacy)",//, Industrial Pharmacy, Pharmacy BioTechnology, Pharmaceutical Technology
        //                        Specialization = "B.Pharmacy"
        //                    },
        //                    new PharmacySpecilaizationList()
        //                    {
        //                        PharmacyspecName = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)",
        //                        Specialization = "B.Pharmacy"
        //                    },
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Group3 (Pharmacy Analysis, PAQA)",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                    new PharmacySpecilaizationList()
        //                    {
        //                        PharmacyspecName = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)",
        //                        Specialization = "B.Pharmacy"
        //                    },
        //                    new PharmacySpecilaizationList()
        //                    {
        //                        PharmacyspecName = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)",
        //                        Specialization = "B.Pharmacy"
        //                    },
        //                    //new PharmacySpecilaizationList()
        //                    //{
        //                    //    PharmacyspecName = "Group6 (English, Mathematics, Computers)",
        //                    //    Specialization = "B.Pharmacy"
        //                    //},
        //                };

        //                TotalcollegeFaculty = jntuh_registered_faculty.Count;

        //                #region All B.Pharmacy Specializations
        //                string PharmacyDeficiency = "";
        //                var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();
        //                var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
        //                var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
        //                string subgroupconditionsmet;
        //                string conditionbpharm = null;
        //                string conditionpharmd = null;
        //                string conditionphardpb = null;
        //                #region Old Commented Code Start
        //                //foreach (var list in pharmacyspeclist)
        //                //{
        //                //    int phd;
        //                //    int pg;
        //                //    int ug;
        //                //    var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                //    bpharmacylist.Specialization = list.Specialization;
        //                //    bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                //    bpharmacylist.collegeId = (int)collegeId;
        //                //    bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                //    bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                //    bpharmacylist.collegeRandomCode = randomcode;
        //                //    bpharmacylist.shiftId = 1;
        //                //    bpharmacylist.Degree = "B.Pharmacy";
        //                //    bpharmacylist.Department = "Pharmacy";
        //                //    bpharmacylist.PharmacyGroup1 = "Group1";

        //                //    bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    //bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
        //                //    //bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
        //                //    //bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
        //                //    //bpharmacylist.totalFaculty = ug + pg + phd;

        //                //    bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                //    bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
        //                //    bpharmacylist.SortId = 1;
        //                //    bpharmacylist.approvedIntake1 = ApprovedIntake;
        //                //    bpharmacylist.specializationId = specializationId;
        //                //    #region bpharmacyspecializationcount

        //                //    if (list.PharmacyspecName == "Pharmaceutics")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }

        //                //    else if (list.PharmacyspecName == "Industrial Pharmacy")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmacy BioTechnology")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                        f.registered_faculty_specialization == "Bio-Technology".ToUpper());

        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmaceutical Technology")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
        //                //            f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                //        bpharmacylist.requiredFaculty = 3;
        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.requiredFaculty = 2;
        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmacy Analysis")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }

        //                //    else if (list.PharmacyspecName == "PAQA")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                     f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                //            //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
        //                //            //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
        //                //                     f.registered_faculty_specialization == "QAPRA".ToUpper() ||
        //                //                     f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
        //                //                     f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
        //                //        bpharmacylist.requiredFaculty = 1;
        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmacology")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }

        //                //    else if (list.PharmacyspecName == "Pharma D")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                       f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                //                      f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                //                      f.registered_faculty_specialization == "Pharm.D".ToUpper());
        //                //        bpharmacylist.requiredFaculty = 2;
        //                //    }
        //                //    else if (list.PharmacyspecName == "Pharmacognosy")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                      f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
        //                //                      f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
        //                //        bpharmacylist.requiredFaculty = 2;
        //                //    }

        //                //    else if (list.PharmacyspecName == "English")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }
        //                //    else if (list.PharmacyspecName == "Mathematics")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }
        //                //    else if (list.PharmacyspecName == "Computers")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }
        //                //    else if (list.PharmacyspecName == "Computer Science")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //    }
        //                //    else if (list.PharmacyspecName == "Zoology")
        //                //    {
        //                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
        //                //    }
        //                //    #endregion





        //                //    if (list.PharmacyspecName == "Group1 (Pharmaceutics)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
        //                //    {
        //                //        group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
        //                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
        //                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
        //                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
        //                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
        //                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                //        bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
        //                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
        //                //        bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics)";
        //                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 4;
        //                //    }

        //                //    else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                //    {
        //                //        group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                //        bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
        //                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
        //                //        bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)";
        //                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 5;
        //                //    }
        //                //    //else if (list.PharmacyspecName == "Group3 (Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
        //                //    //{
        //                //    //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
        //                //    //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
        //                //    //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
        //                //    //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

        //                //    //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                //    //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                //    //    bpharmacylist.BPharmacySubGroupRequired = 1;
        //                //    //    bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacy Analysis, PAQA)";
        //                //    //}

        //                //    else if (list.PharmacyspecName == "Group3 (Pharmacology)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                //    {
        //                //        group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
        //                //        bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
        //                //        bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology)";
        //                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 5 : 4;
        //                //    }

        //                //    else if (list.PharmacyspecName == "Group4 (Pharmacognosy)" || list.PharmacyspecName == "Pharmacognosy")
        //                //    {
        //                //        group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
        //                //            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
        //                //            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));
        //                //        bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
        //                //        bpharmacylist.BPharmacySubGroupRequired = 3;
        //                //        bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy)";
        //                //        bpharmacylist.requiredFaculty = 3;
        //                //    }

        //                //    //else if (list.PharmacyspecName == "Group6 (English, Mathematics, Computers)" || list.PharmacyspecName == "English" || list.PharmacyspecName == "Mathematics" || list.PharmacyspecName == "Computers" || list.PharmacyspecName == "Computer Science")//|| list.PharmacyspecName == "Zoology"
        //                //    //{
        //                //    //    group6Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "English".ToUpper()) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Mathematics".ToUpper()) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER")) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER SCIENCE")) +
        //                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("CSE"));
        //                //    //    //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("ZOOLOGY"));
        //                //    //    bpharmacylist.BPharmacySubGroup1Count = group6Subcount;
        //                //    //    bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake == 100 ? 3 : 2;
        //                //    //    bpharmacylist.PharmacySubGroup1 = "Group6 (English, Mathematics, Computers)";
        //                //    //}



        //                //    var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                //    if (id > 0)
        //                //    {
        //                //        var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                //        if (swf != null)
        //                //        {
        //                //            bpharmacylist.specializationWiseFaculty = (int)swf;
        //                //        }
        //                //        bpharmacylist.id = id;
        //                //        bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                //        bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                //    }

        //                //    intakedetailsList.Add(bpharmacylist);
        //                //}

        //                ////for pharma D specializations
        //                //var pharmaD = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
        //                //if (pharmaD.Count > 0)
        //                //{
        //                //    foreach (var list in pharmdspeclist)
        //                //    {
        //                //        int phd;
        //                //        int pg;
        //                //        int ug;
        //                //        var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                //        bpharmacylist.Specialization = list.Specialization;
        //                //        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                //        bpharmacylist.collegeId = (int)collegeId;
        //                //        bpharmacylist.collegeRandomCode = randomcode;
        //                //        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                //        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                //        bpharmacylist.shiftId = 1;
        //                //        bpharmacylist.Degree = "Pharm.D";
        //                //        bpharmacylist.Department = "Pharm.D";
        //                //        bpharmacylist.PharmacyGroup1 = "Group1";
        //                //        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
        //                //        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
        //                //        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                //        bpharmacylist.totalFaculty = ug + pg + phd;
        //                //        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                //        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                //        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                //        bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                //        bpharmacylist.SortId = 2;
        //                //        bpharmacylist.approvedIntake1 = PharmaDApprovedIntake;
        //                //        bpharmacylist.specializationId = PharmaDspecializationId;
        //                //        #region pharmadSpecializationcount
        //                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
        //                //        {
        //                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                //        }
        //                //        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
        //                //        {
        //                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        }
        //                //        #endregion


        //                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                //        {
        //                //            pharmadgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper());
        //                //            bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
        //                //            bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
        //                //            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
        //                //            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                //        }


        //                //        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                //        if (id > 0)
        //                //        {
        //                //            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                //            if (swf != null)
        //                //            {
        //                //                bpharmacylist.specializationWiseFaculty = (int)swf;
        //                //            }
        //                //            bpharmacylist.id = id;
        //                //            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                //            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                //        }

        //                //        intakedetailsList.Add(bpharmacylist);
        //                //    }
        //                //}


        //                ////for pharma.D PB specializations
        //                //var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
        //                //if (pharmaDPB.Count > 0)
        //                //{
        //                //    foreach (var list in pharmdpbspeclist)
        //                //    {
        //                //        int phd;
        //                //        int pg;
        //                //        int ug;
        //                //        var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                //        bpharmacylist.Specialization = list.Specialization;
        //                //        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                //        bpharmacylist.collegeId = (int)collegeId;
        //                //        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                //        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                //        bpharmacylist.collegeRandomCode = randomcode;
        //                //        bpharmacylist.shiftId = 1;
        //                //        bpharmacylist.Degree = "Pharm.D PB";
        //                //        bpharmacylist.Department = "Pharm.D PB";
        //                //        bpharmacylist.PharmacyGroup1 = "Group1";
        //                //        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                //        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
        //                //        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                //        bpharmacylist.totalFaculty = ug + pg + phd;
        //                //        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                //        bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                //        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                //        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                //        bpharmacylist.SortId = 3;
        //                //        bpharmacylist.approvedIntake1 = PharmaDPBApprovedIntake;
        //                //        bpharmacylist.specializationId = PharmaDPBspecializationId;
        //                //        #region pharmadPbSpecializationcount
        //                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
        //                //        {
        //                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                //                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                //                          f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                //        }
        //                //        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
        //                //        {
        //                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                //        }
        //                //        #endregion


        //                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                //        {
        //                //            pharmadPBgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
        //                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
        //                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
        //                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
        //                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
        //                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
        //                //            bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
        //                //            bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
        //                //            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
        //                //            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                //        }


        //                //        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                //        if (id > 0)
        //                //        {
        //                //            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                //            if (swf != null)
        //                //            {
        //                //                bpharmacylist.specializationWiseFaculty = (int)swf;
        //                //            }
        //                //            bpharmacylist.id = id;
        //                //            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                //            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                //        }

        //                //        intakedetailsList.Add(bpharmacylist);
        //                //    }
        //                //}
        //                #endregion Old Commented Code End
        //                #region PharmD and PharmDPB
        //                int pharmaD = collegeIntakeExisting.Count(i => i.specializationId == 18);
        //                if (pharmaD > 0)
        //                {
        //                    List<CollegeFacultyWithIntakeReport> intakedetailsList1 = new List<CollegeFacultyWithIntakeReport>();
        //                    foreach (var list in pharmacyspeclist)
        //                    {
        //                        int phd;
        //                        int pg;
        //                        int ug;
        //                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                        bpharmacylist.Specialization = list.Specialization;
        //                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                        bpharmacylist.collegeId = (int)collegeId;
        //                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                        bpharmacylist.collegeRandomCode = randomcode;
        //                        bpharmacylist.shiftId = 1;
        //                        bpharmacylist.Degree = "B.Pharmacy";
        //                        bpharmacylist.Department = "Pharmacy";
        //                        bpharmacylist.PharmacyGroup1 = "Group1";

        //                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());

        //                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                        bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
        //                        bpharmacylist.SortId = 1;
        //                        #region bpharmacyspecializationcount

        //                        if (list.PharmacyspecName == "Pharmaceutics")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "Industrial Pharmacy")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacy BioTechnology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                            f.registered_faculty_specialization == "Bio-Technology".ToUpper());

        //                        }
        //                        else if (list.PharmacyspecName == "Pharmaceutical Technology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
        //                                f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                            bpharmacylist.requiredFaculty = 3;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacy Analysis")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "PAQA")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                         f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "QAPRA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
        //                                         f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
        //                            bpharmacylist.requiredFaculty = 1;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "Pharma D")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharm.D".ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacognosy")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }

        //                        else if (list.PharmacyspecName == "English")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Mathematics")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Computers")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Computer Science")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Zoology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                            bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
        //                        }

        //                        #endregion





        //                        if (list.PharmacyspecName == "Group1 (Pharmaceutics , Industrial Pharmacy)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
        //                        {
        //                            group1Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 120 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115 && f.RegistrationNumber != principalRegno);

        //                            bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 10 : 7;
        //                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics , Industrial Pharmacy)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group1Subcount)
        //                                PharmacyDeficiency = "Deficiency";
        //                        }

        //                        else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                        {
        //                          //  jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));

        //                            group2Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
        //                            bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 10 : 7;
        //                            bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group2Subcount)
        //                                PharmacyDeficiency = "Deficiency";


        //                        }


        //                        else if (list.PharmacyspecName == "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                        {
        //                            group3Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 122 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 19 && f.RegistrationNumber != principalRegno);

        //                            bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 12 : 9;
        //                            bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group3Subcount)
        //                                PharmacyDeficiency = "Deficiency";
        //                        }

        //                        else if (list.PharmacyspecName == "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)" || list.PharmacyspecName == "Pharmacognosy")
        //                        {

        //                            int PharmacognosySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber != principalRegno);
        //                            int PharmaceuticalChemistrySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno);
        //                            int PAQASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno);
        //                            int QASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno);
        //                            int QAPRASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
        //                            int Grop2Required = bpharmacyintake >= 100 ? 10 : 7;
        //                            int Grop4Required = bpharmacyintake >= 100 ? 4 : 3;
        //                            int Total1 = Grop2Required - (PAQASp + QAPRASp + QASp);
        //                            int Total = (PAQASp + QAPRASp + PharmaceuticalChemistrySp + QASp) - Grop2Required;
        //                            if (Total > 0)
        //                                group4Subcount = PharmacognosySp + (PharmaceuticalChemistrySp - (Total1 < 0 ? 0 : Total1));
        //                            else if (Total <= 0)
        //                                group4Subcount = PharmacognosySp;
        //                            else if (PharmacognosySp == Grop2Required)
        //                                group4Subcount = PharmacognosySp;
        //                            bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = Grop4Required;
        //                            bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group4Subcount)
        //                                PharmacyDeficiency = "Deficiency";
        //                        }

        //                        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                        if (id > 0)
        //                        {
        //                            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                            if (swf != null)
        //                            {
        //                                bpharmacylist.specializationWiseFaculty = (int)swf;
        //                            }
        //                            bpharmacylist.id = id;
        //                            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                        }

        //                        intakedetailsList1.Add(bpharmacylist);
        //                    }
        //                    if (PharmacyDeficiency != "Deficiency")
        //                    {
        //                        PharmacyandPharmDMeet = "Yes";
        //                        foreach (var item in intakedetailsList1)
        //                        {
        //                            intakedetailsList.Add(item);
        //                        }

        //                        #region Pharmd Start
        //                        var pharmaD2 = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
        //                        if (pharmaD2.Count > 0)
        //                        {
        //                            foreach (var list in pharmdspeclist)
        //                            {
        //                                int phd;
        //                                int pg;
        //                                int ug;
        //                                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                                bpharmacylist.Specialization = list.Specialization;
        //                                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                                bpharmacylist.collegeId = (int)collegeId;
        //                                bpharmacylist.collegeRandomCode = randomcode;
        //                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                                bpharmacylist.shiftId = 1;
        //                                bpharmacylist.Degree = "Pharm.D";
        //                                bpharmacylist.Department = "Pharm.D";
        //                                bpharmacylist.PharmacyGroup1 = "Group1";
        //                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
        //                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
        //                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                                bpharmacylist.totalFaculty = ug + pg + phd;
        //                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                                bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                                bpharmacylist.SortId = 2;
        //                                #region pharmadSpecializationcount
        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharm D")
        //                                {
        //                                    //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                    //               f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                    //              f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                    //              f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                                    //              f.registered_faculty_specialization == "Pharma D".ToUpper());

        //                                    // bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 || f.PGSpecializationId == 122 );




        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = 10;

        //                                }
        //                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                }
        //                                #endregion


        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                                {


        //                                    bpharmacylist.BPharmacySubGroup1Count = 5;
        //                                    bpharmacylist.BPharmacySubGroupRequired = 5;
        //                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
        //                                }


        //                                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                                if (id > 0)
        //                                {
        //                                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                                    if (swf != null)
        //                                    {
        //                                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                                    }
        //                                    bpharmacylist.id = id;
        //                                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                                }

        //                                intakedetailsList.Add(bpharmacylist);
        //                            }
        //                        }
        //                        #endregion Pharmd End

        //                        #region PharmDPB Strt
        //                        var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
        //                        if (pharmaDPB.Count > 0)
        //                        {
        //                            foreach (var list in pharmdpbspeclist)
        //                            {
        //                                int phd;
        //                                int pg;
        //                                int ug;
        //                                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                                bpharmacylist.Specialization = list.Specialization;
        //                                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                                bpharmacylist.collegeId = (int)collegeId;
        //                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                                bpharmacylist.collegeRandomCode = randomcode;
        //                                bpharmacylist.shiftId = 1;
        //                                bpharmacylist.Degree = "Pharm.D PB";
        //                                bpharmacylist.Department = "Pharm.D PB";
        //                                bpharmacylist.PharmacyGroup1 = "Group1";
        //                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
        //                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                                bpharmacylist.totalFaculty = ug + pg + phd;
        //                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                                bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                                bpharmacylist.SortId = 3;
        //                                #region pharmadPbSpecializationcount
        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology))" || list.PharmacyspecName == "Pharm D")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                                }
        //                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                }
        //                                #endregion


        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                                {

        //                                    bpharmacylist.BPharmacySubGroup1Count = 2;
        //                                    bpharmacylist.BPharmacySubGroupRequired = 2;
        //                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
        //                                }


        //                                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                                if (id > 0)
        //                                {
        //                                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                                    if (swf != null)
        //                                    {
        //                                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                                    }
        //                                    bpharmacylist.id = id;
        //                                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                                }

        //                                intakedetailsList.Add(bpharmacylist);
        //                            }
        //                        }
        //                        #endregion PharmDPB End

        //                    }
        //                    else if (PharmacyDeficiency == "Deficiency")
        //                    {
        //                        #region Pharmd Start
        //                        var pharmaD2 = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
        //                        if (pharmaD2.Count > 0)
        //                        {
        //                            foreach (var list in pharmdspeclist)
        //                            {
        //                                int phd;
        //                                int pg;
        //                                int ug;
        //                                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                                bpharmacylist.Specialization = list.Specialization;
        //                                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                                bpharmacylist.collegeId = (int)collegeId;
        //                                bpharmacylist.collegeRandomCode = randomcode;
        //                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                                bpharmacylist.shiftId = 1;
        //                                bpharmacylist.Degree = "Pharm.D";
        //                                bpharmacylist.Department = "Pharm.D";
        //                                bpharmacylist.PharmacyGroup1 = "Group1";
        //                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
        //                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
        //                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                                bpharmacylist.totalFaculty = ug + pg + phd;
        //                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                                bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                                bpharmacylist.SortId = 2;
        //                                #region pharmadSpecializationcount
        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharm D")
        //                                {

        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = 0;

        //                                }
        //                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                }
        //                                #endregion


        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                                {


        //                                    bpharmacylist.BPharmacySubGroup1Count = 0;
        //                                    bpharmacylist.BPharmacySubGroupRequired = 5;
        //                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
        //                                }


        //                                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                                if (id > 0)
        //                                {
        //                                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                                    if (swf != null)
        //                                    {
        //                                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                                    }
        //                                    bpharmacylist.id = id;
        //                                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                                }

        //                                intakedetailsList.Add(bpharmacylist);
        //                            }
        //                        }
        //                        #endregion Pharmd End

        //                        #region PharmDPB Strt
        //                        var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
        //                        if (pharmaDPB.Count > 0)
        //                        {
        //                            foreach (var list in pharmdpbspeclist)
        //                            {
        //                                int phd;
        //                                int pg;
        //                                int ug;
        //                                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                                bpharmacylist.Specialization = list.Specialization;
        //                                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                                bpharmacylist.collegeId = (int)collegeId;
        //                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                                bpharmacylist.collegeRandomCode = randomcode;
        //                                bpharmacylist.shiftId = 1;
        //                                bpharmacylist.Degree = "Pharm.D PB";
        //                                bpharmacylist.Department = "Pharm.D PB";
        //                                bpharmacylist.PharmacyGroup1 = "Group1";
        //                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
        //                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                                bpharmacylist.totalFaculty = ug + pg + phd;
        //                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                                bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                                bpharmacylist.SortId = 3;
        //                                #region pharmadPbSpecializationcount
        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology))" || list.PharmacyspecName == "Pharm D")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                                }
        //                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
        //                                {
        //                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                                }
        //                                #endregion


        //                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                                {

        //                                    bpharmacylist.BPharmacySubGroup1Count = 0;
        //                                    bpharmacylist.BPharmacySubGroupRequired = 2;
        //                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
        //                                }


        //                                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                                if (id > 0)
        //                                {
        //                                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                                    if (swf != null)
        //                                    {
        //                                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                                    }
        //                                    bpharmacylist.id = id;
        //                                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                                }

        //                                intakedetailsList.Add(bpharmacylist);
        //                            }
        //                        }
        //                        #endregion PharmDPB End

        //                    }
        //                }
        //                #endregion Pharmd And PharmDPB
        //                #region Pharmacy Only
        //                if (PharmacyDeficiency == "Deficiency" || pharmaD == 0)
        //                {
        //                    PharmacyandPharmDMeet = "No";
        //                    foreach (var list in pharmacyspeclist)
        //                    {
        //                        int phd;
        //                        int pg;
        //                        int ug;
        //                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                        bpharmacylist.Specialization = list.Specialization;
        //                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                        bpharmacylist.collegeId = (int)collegeId;
        //                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                        bpharmacylist.collegeRandomCode = randomcode;
        //                        bpharmacylist.shiftId = 1;
        //                        bpharmacylist.Degree = "B.Pharmacy";
        //                        bpharmacylist.Department = "Pharmacy";
        //                        bpharmacylist.PharmacyGroup1 = "Group1";

        //                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        //bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
        //                        //bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
        //                        //bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
        //                        //bpharmacylist.totalFaculty = ug + pg + phd;
        //                        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                        bpharmacylist.BphramacyrequiredFaculty = bpharmacyintake >= 100 ? 25 : 15;
        //                        bpharmacylist.SortId = 1;
        //                        #region bpharmacyspecializationcount

        //                        if (list.PharmacyspecName == "Pharmaceutics")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "Industrial Pharmacy")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacy BioTechnology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                            f.registered_faculty_specialization == "Bio-Technology".ToUpper());

        //                        }
        //                        else if (list.PharmacyspecName == "Pharmaceutical Technology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
        //                                f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                            bpharmacylist.requiredFaculty = 3;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacy Analysis")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "PAQA")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                         f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                                //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
        //                                //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
        //                                         f.registered_faculty_specialization == "QAPRA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
        //                                         f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
        //                            bpharmacylist.requiredFaculty = 1;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }

        //                        else if (list.PharmacyspecName == "Pharma D")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharm.D".ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }
        //                        else if (list.PharmacyspecName == "Pharmacognosy")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
        //                                          f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
        //                            bpharmacylist.requiredFaculty = 2;
        //                        }

        //                        else if (list.PharmacyspecName == "English")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Mathematics")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Computers")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Computer Science")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                        }
        //                        else if (list.PharmacyspecName == "Zoology")
        //                        {
        //                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                            bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
        //                        }
        //                        #endregion



        //                        BpharcyrequiredFaculty = bpharmacyintake >= 100 ? 25 : 15;

        //                        if (list.PharmacyspecName == "Group1 (Pharmaceutics , Industrial Pharmacy)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
        //                        {
        //                            //group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
        //                            group1Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 120 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115 && f.RegistrationNumber != principalRegno);
        //                            // jntuh_registered_faculty.Count(f => f.PGSpecializationId == 119)+
        //                            //jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115);

        //                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
        //                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
        //                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
        //                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
        //                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                            //int TotalIntake=0;
        //                            //if (bpharmacyintake == 0)
        //                            //    TotalIntake = intakedetails.approvedIntake1;
        //                            //else if (bpharmacyintake >= 0)
        //                            //    TotalIntake = bpharmacyintake;
        //                            bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 8 : 5;
        //                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics , Industrial Pharmacy)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group1Subcount)
        //                                DeficiencyInPharmacy = "Deficiency";
        //                        }

        //                        else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
        //                        {
        //                            //group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));

        //                            group2Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno) +
        //                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
        //                            bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 7 : 4;
        //                            bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group2Subcount)
        //                                DeficiencyInPharmacy = "Deficiency";

        //                        }
        //                        //else if (list.PharmacyspecName == "Group3 (Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
        //                        //{
        //                        //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
        //                        //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
        //                        //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
        //                        //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

        //                        //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                        //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                        //    bpharmacylist.BPharmacySubGroupRequired = 1;
        //                        //    bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacy Analysis, PAQA)";
        //                        //}

        //                        else if (list.PharmacyspecName == "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                        {
        //                            // group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
        //                            group3Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 122 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 && f.RegistrationNumber != principalRegno) +
        //                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 19 && f.RegistrationNumber != principalRegno);
        //                            // jntuh_registered_faculty.Count(f => f.PGSpecializationId == 114);

        //                            bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
        //                            bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group3Subcount)
        //                                DeficiencyInPharmacy = "Deficiency";
        //                        }

        //                        else if (list.PharmacyspecName == "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)" || list.PharmacyspecName == "Pharmacognosy")
        //                        {
        //                            //group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
        //                            //    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
        //                            //    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));

        //                            //group4Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber!=principalRegno);

        //                            int PharmacognosySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber != principalRegno);
        //                            int PharmaceuticalChemistrySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno);
        //                            int PAQASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno);
        //                            int QASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno);
        //                            int QAPRASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
        //                            int Group2Required = bpharmacyintake >= 100 ? 7 : 4;
        //                            int Group4Required = bpharmacyintake >= 100 ? 4 : 2;
        //                            int Total1 = Group2Required - (PAQASp + QAPRASp + QASp);
        //                            int Total = (PAQASp + QAPRASp + PharmaceuticalChemistrySp + QASp) - Group2Required;
        //                            if (Total > 0)
        //                                group4Subcount = PharmacognosySp + (PharmaceuticalChemistrySp - (Total1 < 0 ? 0 : Total1));
        //                            else if (Total <= 0)
        //                                group4Subcount = PharmacognosySp;
        //                            else if (PharmacognosySp == Group2Required)
        //                                group4Subcount = PharmacognosySp;
        //                            bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
        //                            bpharmacylist.BPharmacySubGroupRequired = Group4Required;
        //                            bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)";
        //                            if (bpharmacylist.BPharmacySubGroupRequired > group4Subcount)
        //                                DeficiencyInPharmacy = "Deficiency";
        //                        }

        //                        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                        if (id > 0)
        //                        {
        //                            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                            if (swf != null)
        //                            {
        //                                bpharmacylist.specializationWiseFaculty = (int)swf;
        //                            }
        //                            bpharmacylist.id = id;
        //                            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                        }

        //                        intakedetailsList.Add(bpharmacylist);
        //                    }
        //                }
        //                #endregion  Pharmacy Only

        //                if (BpharcyrequiredFaculty > 0)
        //                {

        //                    if (bpharmacyintake >= 100)
        //                    {
        //                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
        //                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                        BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                    }
        //                    else
        //                    {
        //                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
        //                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                        BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                    }
        //                    intakedetailsList.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharcyrequiredFaculty);


        //                    if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty)
        //                    {
        //                        if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                        {
        //                            subgroupconditionsmet = conditionbpharm = "No";
        //                        }
        //                        else
        //                        {
        //                            subgroupconditionsmet = conditionbpharm = "Yes";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        subgroupconditionsmet = conditionbpharm = "Yes";
        //                    }

        //                    ViewBag.BpharmcyCondition = conditionbpharm;
        //                    bpharmacycondition = conditionbpharm;
        //                    intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);


        //                }

        //                ViewBag.PharmDrequiredFaculty = PharmDrequiredFaculty;

        //                if (PharmDrequiredFaculty > 0)
        //                {
        //                    if (jntuh_registered_faculty.Count >= PharmDrequiredFaculty)
        //                    {
        //                        if (pharmadgroup1Subcount >= pharmadTotalintake / 30)
        //                        {
        //                            subgroupconditionsmet = conditionpharmd = "No";
        //                        }
        //                        else
        //                        {
        //                            subgroupconditionsmet = conditionpharmd = "Yes";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        subgroupconditionsmet = conditionpharmd = "Yes";
        //                    }

        //                    ViewBag.PharmaDCondition = conditionpharmd;
        //                    pharmdcondition = conditionpharmd;
        //                    if (conditionbpharm == "No")
        //                    {
        //                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
        //                    }
        //                    else
        //                    {
        //                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
        //                    }


        //                }

        //                ViewBag.PharmDPBrequiredFaculty = PharmDPBrequiredFaculty;
        //                pharmadpbrequiredfaculty = PharmDPBrequiredFaculty;
        //                if (PharmDPBrequiredFaculty > 0)
        //                {
        //                    if (jntuh_registered_faculty.Count >= PharmDPBrequiredFaculty)
        //                    {
        //                        if (pharmadPBgroup1Subcount >= pharmadPBTotalintake / 10)
        //                        {
        //                            subgroupconditionsmet = conditionphardpb = "No";
        //                        }
        //                        else
        //                        {
        //                            subgroupconditionsmet = conditionphardpb = "Yes";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        subgroupconditionsmet = conditionphardpb = "Yes";
        //                    }

        //                    ViewBag.PharmaDPBCondition = conditionphardpb;
        //                    pharmadpbcondition = conditionphardpb;
        //                    if (conditionbpharm == "No" && conditionpharmd == "No")
        //                    {
        //                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
        //                    }
        //                    else
        //                    {
        //                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
        //                    }

        //                }



        //                #endregion

        //                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //                #region Faculty Appeal Deficiency Status
        //                //var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
        //                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
        //                foreach (var item in intakedetailsList.Where(i => i.Degree == "B.Pharmacy").ToList())
        //                {
        //                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
        //                    if (deparment != null)
        //                    {
        //                        var facultydefcount = 0;//(int)Math.Ceiling(item.requiredFaculty) - item.totalFaculty

        //                        if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
        //                        {
        //                            if (pharmaD > 0)
        //                            {
        //                                if (group1Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group2Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group3Subcount >= (bpharmacyintake >= 100 ? 12 : 9) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 3))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                                {
        //                                    Allgroupscount = 0;
        //                                }
        //                                else
        //                                {
        //                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                                    if (group1Subcount < (bpharmacyintake >= 100 ? 10 : 7))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group1Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group2Subcount < (bpharmacyintake >= 100 ? 10 : 7))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group2Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group3Subcount < (bpharmacyintake >= 100 ? 12 : 9))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 12 : 9) - group3Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 3))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 4 : 3) - group4Subcount;
        //                                        count = count == 1 ? 0 : count;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (pharmaD > 0)
        //                                    if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                                    {
        //                                        Allgroupscount = 0;
        //                                    }
        //                                    else
        //                                    {
        //                                        //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                                        if (group1Subcount < (bpharmacyintake >= 100 ? 8 : 5))
        //                                        {
        //                                            var count = (bpharmacyintake >= 100 ? 8 : 5) - group1Subcount;
        //                                            Allgroupscount = Allgroupscount + count;
        //                                        }
        //                                        if (group2Subcount < (bpharmacyintake >= 100 ? 7 : 4))
        //                                        {
        //                                            var count = (bpharmacyintake >= 100 ? 7 : 4) - group2Subcount;
        //                                            Allgroupscount = Allgroupscount + count;
        //                                        }
        //                                        if (group3Subcount < (bpharmacyintake >= 100 ? 6 : 4))
        //                                        {
        //                                            var count = (bpharmacyintake >= 100 ? 6 : 4) - group3Subcount;
        //                                            Allgroupscount = Allgroupscount + count;
        //                                        }
        //                                        if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 2))
        //                                        {
        //                                            var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
        //                                            count = count == 1 ? 0 : count;
        //                                            Allgroupscount = Allgroupscount + count;
        //                                        }
        //                                    }
        //                            }

        //                            facultydefcount = Allgroupscount;
        //                        }

        //                        else if (jntuh_registered_faculty.Count < BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
        //                        {
        //                            if (pharmaD > 0)
        //                            {
        //                                if (group1Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group2Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group3Subcount >= (bpharmacyintake >= 100 ? 12 : 9) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 3))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                                {
        //                                    Allgroupscount = 0;
        //                                }
        //                                else
        //                                {
        //                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                                    if (group1Subcount < (bpharmacyintake >= 100 ? 10 : 7))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group1Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group2Subcount < (bpharmacyintake >= 100 ? 10 : 7))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group2Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group3Subcount < (bpharmacyintake >= 100 ? 12 : 9))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 12 : 9) - group3Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 3))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
        //                                        count = count == 1 ? 0 : count;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                                {
        //                                    Allgroupscount = 0;
        //                                }
        //                                else
        //                                {
        //                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                                    if (group1Subcount < (bpharmacyintake >= 100 ? 8 : 5))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 8 : 5) - group1Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group2Subcount < (bpharmacyintake >= 100 ? 7 : 4))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 7 : 4) - group2Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group3Subcount < (bpharmacyintake >= 100 ? 6 : 4))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 6 : 4) - group3Subcount;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 2))
        //                                    {
        //                                        var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
        //                                        count = count == 1 ? 0 : count;
        //                                        Allgroupscount = Allgroupscount + count;
        //                                    }
        //                                }
        //                            }


        //                            var lessfaculty = BpharcyrequiredFaculty - jntuh_registered_faculty.Count;

        //                            if (lessfaculty > Allgroupscount)
        //                            {
        //                                facultydefcount = Allgroupscount;//(int)lessfaculty + 
        //                            }
        //                            else if (Allgroupscount > lessfaculty)
        //                            {
        //                                facultydefcount = Allgroupscount;//+ (int)lessfaculty
        //                            }
        //                        }

        //                        if (item.Degree == "B.Pharmacy")
        //                        {
        //                            if (Allgroupscount > 0)
        //                            {
        //                                //item.deficiency = true; 
        //                            }
        //                            ViewBag.BpharmacyRequired = facultydefcount;
        //                        }

        //                        //if (item.PharmacyspecializationWiseFaculty < 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
        //                        //{
        //                        //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty) + 1;
        //                        //}
        //                        //if (item.PharmacyspecializationWiseFaculty >= 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
        //                        //{
        //                        //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty);
        //                        //}
        //                        //if (item.Department == "Pharm.D" || item.Department == "Pharm.D PB")
        //                        //{
        //                        //    facultydefcount = item.BPharmacySubGroupRequired - item.BPharmacySubGroup1Count;
        //                        //}

        //                    }
        //                }


        //                #endregion
        //            }
        //            return intakedetailsList;
        //        }
        public List<CollegeFacultyLabs> PharmacyDeficienciesInFaculty(int? collegeID)
        {
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            string facultyAdmittedIntakeZero = string.Empty;

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            int[] Specializationids = Specializations.Select(s => s.id).ToArray();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == AY1 && i.proposedIntake != 0 && Specializationids.Contains(i.specializationId) && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeIntakeExisting> CurrentyearcollegeIntakeExisting = new List<CollegeIntakeExisting>();

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
            CurrentyearcollegeIntakeExisting = collegeIntakeExisting.Where(a => a.shiftId == 1).OrderBy(a => a.Specialization).ToList();
            string cid = collegeID.ToString();



            string[] Collegefaculty = db.jntuh_college_faculty_registered.Where(CF => CF.collegeId == collegeID).Select(Cf => Cf.RegistrationNumber).ToArray();

            //string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => Collegefaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_facultyBAS = registeredFacultyNew.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid && (p.Deficiency == null || !BASRegNos.Contains(p.Deficiency))).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))
                                                    .Select(rf => new
                                                    {
                                                        RegistrationNumber = rf.RegistrationNumber,
                                                        HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                    }).Where(e => e.HighestDegreeID == 6).ToList();
            string[] PhdRegNO = jntuh_registered_faculty1.Select(e => e.RegistrationNumber).ToArray();

            //var collegeFacultycount = collegeFacultycount1.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
            //                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null)) && rf.InvalidAadhaar != "Yes").Select(r => r.RegistrationNumber).ToList();


            //var collegeFacultycount1 = db.jntuh_registered_faculty.Where(rf => Collegefaculty.Contains(rf.RegistrationNumber)).ToList();
            //var collegeFacultycount = collegeFacultycount1.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
            //                                             (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(r => r.RegistrationNumber).ToList();
            ////TotalcollegeFaculty = collegeFacultycount.Count();
            int? Required = 0;
            int? Avilable = 0;
            int? PhDAvilable = 0;
            //int? TotalRequired = 0;
            //int? TotalAvilable = 0;
            int? TotalIntake = 0;
            int? PraposedIntake = 0;
            int Sno = 1;
            string strgroup = "";
            string PharmacyStatus = "";
            string specialization = "";

            string PharmD = "";


            foreach (var item in CurrentyearcollegeIntakeExisting)
            {
                specialization = item.specializationId.ToString();
                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = item.Degree;
                newFaculty.DegreeId = item.degreeID;
                newFaculty.Department = item.Department;
                newFaculty.DepartmentId = item.DepartmentID;
                newFaculty.SpecializationId = item.specializationId;
                newFaculty.Specialization = item.Specialization;
                if (item.Specialization == "Pharm.D")
                {
                    newFaculty.TotalIntake = 30;
                    newFaculty.Required = 0;
                    newFaculty.Available = 0;
                }
                else if (item.Specialization == "Pharm.D PB")
                {
                    newFaculty.TotalIntake = 10;
                    newFaculty.Required = 0;
                    newFaculty.Available = 0;
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    newFaculty.TotalIntake = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.TotalIntake).FirstOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.ProposedIntake).FirstOrDefault() : 0;
                    newFaculty.Required = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).LastOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).LastOrDefault() : 0;
                    newFaculty.Available = FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) != null ? (int)FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) : 0;
                }
                else
                {
                    var grp1 = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "1").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "1").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() : 0;
                    var grp2 = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "2").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "2").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() : 0;
                    var grp3 = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "3").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "3").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() : 0;
                    var grp4 = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "4").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization && p.PharmacySpecialization == "4").Select(p => p.SpecializationWiseRequiredFaculty).LastOrDefault() : 0;
                    newFaculty.TotalIntake = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.TotalIntake).FirstOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.ProposedIntake).FirstOrDefault() : 0;
                    //newFaculty.Required = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).FirstOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).FirstOrDefault() : 0;
                    newFaculty.Required = grp1 + grp2 + grp3 + grp4;
                    newFaculty.Available = FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) != null ? (int)FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) : 0;
                }

                if (item.Degree != "M.Pharmacy")
                    newFaculty.RequiredPhdFaculty = 0;
                else
                    newFaculty.RequiredPhdFaculty = 1;
                if (item.Degree != "M.Pharmacy")
                    newFaculty.AvailablePhdFaculty = 0;
                else
                    newFaculty.AvailablePhdFaculty = (int)FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization) > 0 ? (int)FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization) : 0;

                if (newFaculty.Required <= newFaculty.Available)
                {
                    if (PharmacyStatus == "Deficiency")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            newFaculty.Deficiency = "YES";
                        }
                        else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                        {
                            newFaculty.Deficiency = "YES";
                        }
                    }
                    else
                        newFaculty.Deficiency = "NO";

                }
                else
                {
                    if (item.Degree == "B.Pharmacy")
                    {
                        PharmacyStatus = "Deficiency";
                    }
                    else if ((item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB") && (PharmacyStatus != "Deficiency"))
                    {
                        newFaculty.Deficiency = "NO";
                    }
                    newFaculty.Deficiency = "YES";
                }

                if (item.Degree == "M.Pharmacy")
                {
                    if (newFaculty.AvailablePhdFaculty > 0)
                        newFaculty.PhdDeficiency = "NO";
                    else
                        newFaculty.PhdDeficiency = "YES";
                }
                else
                {
                    newFaculty.PhdDeficiency = "NO";
                }
                newFaculty.ShiftId = 1;


                lstFaculty.Add(newFaculty);
            }

            //var CheckBPharmacyDeficiency = lstFaculty.Where(e => e.Degree == "B.Pharmacy" && e.Deficiency != "YES").Select(e => e).ToList();
            var CheckBPharmacyDeficiency = lstFaculty.Where(e => e.Degree == "B.Pharmacy").Select(e => e).ToList();
            if (CheckBPharmacyDeficiency.Count() != 0 && CheckBPharmacyDeficiency.Count() != null)
            {
                return lstFaculty.Where(e => DegreeIds.Contains(e.DegreeId)).Select(e => e).ToList();
            }
            else
            {
                return lstFaculty = new List<CollegeFacultyLabs>();
            }
            // return lstFaculty;
        }
        #endregion



        #region For Affiliation40-80 PercentB.Tech& B.Pharmacy Logic

        //public List<CollegeFacultyWithIntakeReport> Affiliation4080PercentcollegeFaculty(int? collegeId)
        //{

        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //    List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
        //    var jntuh_departments = db.jntuh_department.ToList();
        //    if (collegeId != null)
        //    {
        //        var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
        //        var jntuh_specialization = db.jntuh_specialization.ToList();

        //        int[] collegeIDs = null;
        //        int facultystudentRatio = 0;
        //        decimal facultyRatio = 0m;
        //        if (collegeId != 0)
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
        //        }
        //        else
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
        //        }
        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //        var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
        //        var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
        //        var jntuh_degree = db.jntuh_degree.ToList();

        //        int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //        int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();

        //        foreach (var item in intake)
        //        {
        //            CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //            newIntake.id = item.id;
        //            newIntake.collegeId = item.collegeId;
        //            newIntake.academicYearId = item.academicYearId;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.isActive = item.isActive;
        //            newIntake.nbaFrom = item.nbaFrom;
        //            newIntake.nbaTo = item.nbaTo;
        //            newIntake.specializationId = item.specializationId;
        //            newIntake.Specialization = item.jntuh_specialization.specializationName;
        //            newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //            newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //            newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //            newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //            newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.Shift = item.jntuh_shift.shiftName;
        //            collegeIntakeExisting.Add(newIntake);
        //        }
        //        collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

        //        //college Reg nos
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();  // && cf.createdBy != 63809
        //        string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //        //education categoryIds UG,PG,PHD...........

        //        int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

        //        var jntuh_education_category = db.jntuh_education_category.ToList();

        //        var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //        var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();


        //        int[] CollegeIDS = new int[] { 113, 172, 360, 162 };

        //        var registeredFaculty = new List<jntuh_reinspection_registered_faculty>();
        //        if (CollegeIDS.Contains((int)collegeId))
        //        {
        //            registeredFaculty = db.jntuh_reinspection_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
        //        }
        //        else
        //        {
        //            registeredFaculty = principalRegno != null ? db.jntuh_reinspection_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
        //            : db.jntuh_reinspection_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
        //        }


        //        //Reg nos related online facultyIds
        //        var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
        //                                                && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct")
        //                                         .Select(rf => new
        //                                         {
        //                                             RegistrationNumber = rf.RegistrationNumber.Trim(),
        //                                             Department = rf.jntuh_department.departmentName,
        //                                             HighestDegreeID = rf.jntuh_reinspection_registered_faculty_education.Count() != 0 ? rf.jntuh_reinspection_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                             IsApproved = rf.isApproved,
        //                                             PanNumber = rf.PANNumber,
        //                                             AadhaarNumber = rf.AadhaarNumber,
        //                                             NoForm16 = rf.NoForm16,
        //                                             TotalExperience = rf.TotalExperience
        //                                         }).ToList();
        //        jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
        //        var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
        //        {
        //            RegistrationNumber = rf.RegistrationNumber,
        //            Department = rf.Department,
        //            HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //            Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
        //            PanNumber = rf.PanNumber,
        //            AadhaarNumber = rf.AadhaarNumber
        //        }).Where(e => e.Department != null)
        //                                         .ToList();


        //        int[] StrPharmacy = new[] { 26, 27, 36, 39 };
        //        foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
        //        {
        //            CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
        //            int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

        //            intakedetails.collegeId = item.collegeId;
        //            intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
        //            intakedetails.Degree = item.Degree;
        //            intakedetails.Department = item.Department;
        //            intakedetails.Specialization = item.Specialization;
        //            intakedetails.specializationId = item.specializationId;
        //            intakedetails.DepartmentID = item.DepartmentID;
        //            intakedetails.shiftId = item.shiftId;
        //          // intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
        //          //  intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
        //          ///  intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
        //           // intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
        //           // intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
        //            facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());


        //            if (item.Degree == "B.Tech")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
        //                                            (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);
        //                //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
        //            }
        //            else if (item.Degree == "M.Tech" || item.Degree == "MBA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);

        //            }
        //            else if (item.Degree == "MCA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else if (item.Degree == "B.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }
        //            else if (item.Degree == "M.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

        //            }
        //            else if (item.Degree == "Pharm.D")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                            (intakedetails.approvedIntake5);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
        //            }
        //            else if (item.Degree == "Pharm.D PB")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
        //            }
        //            else //MAM MTM
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                            (intakedetails.approvedIntake5);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //            }

        //            intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //            intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

        //            //====================================
        //            // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

        //            string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //            if (strdegreetype == "UG")
        //            {
        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department).Count();//&& f.Recruitedfor == "UG"
        //                }
        //            }

        //            if (strdegreetype == "PG")
        //            {
        //                if (item.Degree == "M.Pharmacy")
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.SpecializationId == item.specializationId).Count();//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
        //                }
        //            }
        //            //intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

        //            //if (intakedetails.id > 0)
        //            //{
        //            //    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //            //    if (swf != null)
        //            //    {
        //            //        intakedetails.specializationWiseFaculty = (int)swf;
        //            //    }
        //            //    intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
        //            //    intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
        //            //}

        //            //============================================

        //            int noPanOrAadhaarcount = 0;

        //            if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
        //            {
        //                ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
        //                pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
        //                phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
        //                noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
        //                intakedetails.Department = "Pharmacy";
        //            }
        //            else
        //            {
        //                //ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
        //                //pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
        //                //phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
        //                //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();


        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
        //                if (item.Degree == "M.Tech")
        //                    phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department && f.SpecializationId == item.specializationId);
        //                else
        //                    phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

        //                var phd = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).ToList();
        //                SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
        //                var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
        //                SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));


        //            }

        //            intakedetails.phdFaculty = phdFaculty;
        //            intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
        //            intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
        //            intakedetails.pgFaculty = pgFaculty;
        //            intakedetails.ugFaculty = ugFaculty;
        //            intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
        //            intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
        //            //=============//

        //            intakedetailsList.Add(intakedetails);
        //        }

        //        intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //        string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
        //        int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
        //        if (btechdegreecount != 0)
        //        {
        //            foreach (var department in strOtherDepartments)
        //            {
        //                //int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
        //                //int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
        //                //int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
        //                //int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

        //                //int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                //if (facultydeficiencyId == 0)
        //                //{
        //                //    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
        //                //}
        //                //else
        //                //{
        //                //    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                //    bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
        //                //    int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
        //                //    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
        //                //}
        //                var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
        //                var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
        //                int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
        //                int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
        //                int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department);
        //                int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

        //                intakedetailsList.Add(new CollegeFacultyWithIntakeReport
        //                {
        //                    collegeId = (int)collegeId,
        //                    Degree = "B.Tech",
        //                    Department = department,
        //                    Specialization = department,
        //                    ugFaculty = ugFaculty,
        //                    pgFaculty = pgFaculty,
        //                    phdFaculty = phdFaculty,
        //                    totalFaculty = ugFaculty + pgFaculty + phdFaculty,
        //                    specializationId = speId,
        //                    shiftId = 1,

        //                    specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname)
        //                });
        //            }
        //        }
        //    }

        //    return intakedetailsList;


        //}


        //public List<CollegeFacultyLabs> Affiliation4080PercentDeficienciesInFaculty(int? collegeID)
        //{


        //    string faculty = string.Empty;
        //    int facultycount = 0;
        //    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    faculty += "<tr>";
        //    faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
        //    faculty += "</tr>";
        //    faculty += "</table>";

        //    List<CollegeFacultyWithIntakeReport> facultyCounts = Affiliation4080PercentcollegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

        //    var count = facultyCounts.Count();
        //    var distDeptcount = 1;
        //    var deptloop = 1;
        //    decimal departmentWiseRequiredFaculty = 0;

        //    string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

        //    int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
        //    var degrees = db.jntuh_degree.ToList();
        //    var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
        //    int remainingFaculty = 0;
        //    int remainingPHDFaculty = 0;
        //    int SpecializationwisePHDFaculty = 0;
        //    int SpecializationwisePGFaculty = 0;
        //    int TotalCount = 0;

        //    faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    faculty += "<tr>";
        //    faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >PG Specialization</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
        //    faculty += "</tr>";

        //    var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();

        //    List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
        //    int[] SpecializationIDS = db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
        //    int remainingFaculty2 = 0;
        //    foreach (var item in facultyCounts)
        //    {
        //        distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
        //        if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
        //        else if (item.Degree == "MCA")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
        //        else if (item.Degree == "MBA")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
        //        TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();
        //        SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;


        //        if (SpecializationIDS.Contains(item.specializationId))
        //        {
        //            int SpecializationwisePGFaculty1 = facultyCounts.Where(S => S.specializationId == item.specializationId).Count();
        //            SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId == item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

        //        }
        //        int indexnow = facultyCounts.IndexOf(item);


        //        if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
        //        {
        //            deptloop = 1;
        //        }

        //        departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

        //        string minimumRequirementMet = string.Empty;
        //        int facultyShortage = 0;
        //        int adjustedFaculty = 0;
        //        int adjustedPHDFaculty = 0;
        //        int tFaculty = 0;
        //        if (item.Department == "MBA" || item.Department == "MCA")
        //            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
        //        else
        //            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
        //        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

        //        if (departments.Contains(item.Department))
        //        {
        //            rFaculty = (int)firstYearRequired;
        //            departmentWiseRequiredFaculty = (int)firstYearRequired;
        //        }

        //        var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //        if (deptloop == 1)
        //        {
        //            if (rFaculty <= tFaculty)
        //            {
        //                minimumRequirementMet = "YES";
        //                remainingFaculty = tFaculty - rFaculty;
        //                adjustedFaculty = rFaculty;
        //            }
        //            else
        //            {
        //                minimumRequirementMet = "NO";
        //                adjustedFaculty = tFaculty;
        //                facultyShortage = rFaculty - tFaculty;
        //            }

        //            remainingPHDFaculty = item.phdFaculty;

        //            //if (remainingPHDFaculty > 2 && degreeType.Equals("PG"))
        //            //{
        //            //    adjustedPHDFaculty = 1;
        //            //    remainingPHDFaculty = remainingPHDFaculty - 1;
        //            //}
        //            if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA")
        //            {
        //                adjustedPHDFaculty = 1; //remainingPHDFaculty;
        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //                // facultyCounts.Where(i => i.Department == item.Department).ToList().ForEach(I => I.phdFaculty = remainingPHDFaculty);
        //            }
        //            else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA")
        //            {
        //                if (item.totalIntake > 120)
        //                    adjustedPHDFaculty = remainingPHDFaculty;
        //                else
        //                    adjustedPHDFaculty = 1;
        //            }
        //            else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && (rFaculty - adjustedFaculty == 1) && item.Degree == "MBA")
        //            {
        //                if (item.totalIntake > 120)
        //                    adjustedPHDFaculty = remainingPHDFaculty;
        //                else
        //                    adjustedPHDFaculty = 1;
        //            }
        //            else if (remainingPHDFaculty < 1 && (degreeType.Equals("PG")) && remainingPHDFaculty > 0)
        //            {
        //                adjustedPHDFaculty = remainingPHDFaculty;
        //                // remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }
        //        }
        //        else
        //        {
        //            if (rFaculty <= remainingFaculty)
        //            {
        //                minimumRequirementMet = "YES";
        //                //remainingFaculty = remainingFaculty - rFaculty;
        //                //adjustedFaculty = rFaculty;
        //                if (rFaculty <= item.specializationWiseFaculty)
        //                {
        //                    remainingFaculty = remainingFaculty - rFaculty;
        //                    adjustedFaculty = rFaculty;
        //                }

        //                else if (rFaculty >= item.specializationWiseFaculty)
        //                {
        //                    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
        //                    adjustedFaculty = item.specializationWiseFaculty;
        //                }
        //            }
        //            else
        //            {
        //                minimumRequirementMet = "NO";
        //                adjustedFaculty = remainingFaculty;
        //                facultyShortage = rFaculty - remainingFaculty;
        //                remainingFaculty = 0;
        //            }

        //            //if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //            //{
        //            //    adjustedPHDFaculty = 1;
        //            //    remainingPHDFaculty = remainingPHDFaculty - 1;
        //            //}
        //            remainingPHDFaculty = item.phdFaculty;
        //            if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA")
        //            {
        //                adjustedPHDFaculty = 1; //remainingPHDFaculty;
        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //                //facultyCounts.Where(i => i.Department == item.Department).ToList().ForEach(I => I.phdFaculty = remainingPHDFaculty);
        //            }
        //            else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA")
        //            {
        //                if (item.totalIntake > 120)
        //                    adjustedPHDFaculty = remainingPHDFaculty;
        //                else
        //                    adjustedPHDFaculty = 1;
        //            }
        //            else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && (rFaculty - adjustedFaculty == 1) && item.Degree == "MBA")
        //            {
        //                if (item.totalIntake > 120)
        //                    adjustedPHDFaculty = remainingPHDFaculty;
        //                else
        //                    adjustedPHDFaculty = 1;
        //            }
        //            else if (remainingPHDFaculty < 1 && (degreeType.Equals("PG")) && remainingPHDFaculty > 0)
        //            {

        //                adjustedPHDFaculty = remainingPHDFaculty;

        //                // remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }


        //        }

        //        faculty += "<tr>";
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

        //        if (departments.Contains(item.Department))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
        //        }
        //        else
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
        //        }
        //        facultycount = facultycount + item.specializationWiseFaculty;
        //        if (adjustedFaculty > 0)
        //            adjustedFaculty = adjustedFaculty;
        //        else
        //            adjustedFaculty = 0;
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";

        //        if (item.Degree == "M.Tech")
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
        //        else
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";



        //        if (minimumRequirementMet == "YES")
        //        {
        //            if (rFaculty <= adjustedFaculty)
        //                minimumRequirementMet = "NO";
        //            else
        //                minimumRequirementMet = "YES";
        //        }

        //        else if (minimumRequirementMet == "NO")
        //        {
        //            if (rFaculty == adjustedFaculty)
        //                minimumRequirementMet = "NO";
        //            else
        //                minimumRequirementMet = "YES";
        //        }
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

        //        if (adjustedPHDFaculty >= 2 && degreeType.Equals("PG"))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //        }
        //        else if (degreeType.Equals("PG"))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
        //        }
        //        else
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
        //        }

        //        faculty += "</tr>";

        //        CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //        int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
        //        newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
        //        newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //        newFaculty.Degree = item.Degree;
        //        newFaculty.Department = item.Department;
        //        newFaculty.Specialization = item.Specialization;
        //        newFaculty.SpecializationId = item.specializationId;
        //        newFaculty.TotalIntake = item.approvedIntake1;

        //        if (departments.Contains(item.Department))
        //        {
        //            //newFaculty.TotalIntake = totalBtechFirstYearIntake;
        //            newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
        //        }
        //        else
        //        {
        //            // newFaculty.TotalIntake = item.totalIntake;
        //            newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
        //        }

        //        newFaculty.Available = adjustedFaculty;
        //        if (item.Degree == "M.Tech")
        //        {
        //            newFaculty.Available = adjustedFaculty; //item.SpecializationspgFaculty;
        //        }
        //        newFaculty.Deficiency = minimumRequirementMet;

        //        if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree != "MBA")
        //        {
        //            newFaculty.PhdDeficiency = "NO";
        //        }
        //        else if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree == "MBA")
        //        {
        //            if (item.totalIntake > 120)
        //            {
        //                if (adjustedPHDFaculty >= 2)
        //                    newFaculty.PhdDeficiency = "NO";
        //                else
        //                    newFaculty.PhdDeficiency = "YES";
        //            }
        //            else
        //            {
        //                if (adjustedPHDFaculty >= 1)
        //                    newFaculty.PhdDeficiency = "NO";
        //                else
        //                    newFaculty.PhdDeficiency = "YES";
        //            }
        //        }
        //        else if (degreeType.Equals("PG"))
        //        {
        //            newFaculty.PhdDeficiency = "YES";
        //        }
        //        else
        //        {
        //            newFaculty.PhdDeficiency = "-";
        //        }

        //        lstFaculty.Add(newFaculty);
        //        deptloop++;
        //    }

        //    faculty += "</table>";

        //    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    faculty += "<tr>";
        //    faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
        //    faculty += "</tr>";
        //    faculty += "</table>";

        //    return lstFaculty;



        //}




        #region Affiliation4080PercentPharmacyCode

        //        public List<CollegeFacultyLabs> Affiliation4080PercentPharmacyDeficienciesInFaculty(int? collegeID)
        //        {
        //            string faculty = string.Empty;

        //            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //            faculty += "<tr>";
        //            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
        //            faculty += "</tr>";
        //            faculty += "</table>";

        //            List<CollegeFacultyWithIntakeReport> facultyCounts = Affiliation4080PercentPharmacyCollegeFaculty(collegeID).Where(c => c.shiftId == 1).OrderBy(i => i.SortId).ToList();//Where(c => c.shiftId == 1)

        //            var count = facultyCounts.Count();
        //            var distDeptcount = 1;
        //            var deptloop = 1;
        //            var specloop = 1;
        //            decimal departmentWiseRequiredFaculty = 0;

        //            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

        //            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
        //            var degrees = db.jntuh_degree.ToList();
        //            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
        //            int remainingFaculty = 0;
        //            int remainingPHDFaculty = 0;
        //            var remainingphramdFaculty = 0;
        //            var distSpeccount = 0;
        //            var totalusedfaculty = 0;
        //            var remainingmpharmacyfaculty = 0;
        //            //if (PharmDRequiredFaculty == 0)
        //            //{
        //            //    var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(ViewBag.BpharmacyrequiredFaculty);
        //            //    remainingmpharmacyfaculty = facultycount;
        //            //}
        //            if (TotalcollegeFaculty > 0)
        //            {
        //                var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                remainingmpharmacyfaculty = facultycount;
        //                if (PharmDRequiredFaculty > 0)
        //                {
        //                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDRequiredFaculty);
        //                }
        //                if (ViewBag.PharmDPBrequiredFaculty > 0)
        //                {
        //                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDPBRequiredFaculty);
        //                }

        //                if (remainingmpharmacyfaculty < 0)
        //                {
        //                    remainingmpharmacyfaculty = 0;
        //                }
        //            }
        //            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
        //            faculty += "<tr>";
        //            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
        //            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
        //            //faculty += "<th style='text-align: left; vertical-align: top;' >Status</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Pharmacy Specializations *</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
        //            //faculty += "<th style='text-align: center; vertical-align: top;' >Adjusted faculty</th>";
        //            //faculty += "<th style='text-align: center; vertical-align: top;' >Not Qualified as per AICTE faculty</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
        //            faculty += "<th style='text-align: center; vertical-align: top;' >No. of Ph.D faculty</th>";
        //            faculty += "</tr>";
        //            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
        //            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
        //            foreach (var item in facultyCounts)
        //            {
        //                var pharmadsubgroupmet = "";
        //                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
        //                distSpeccount = facultyCounts.Where(d => d.Specialization == item.Specialization && d.Degree == item.Degree).Distinct().Count();
        //                int indexnow = facultyCounts.IndexOf(item);

        //                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
        //                {
        //                    deptloop = 1;
        //                }
        //                if (indexnow > 0 && facultyCounts[indexnow].Specialization != facultyCounts[indexnow - 1].Specialization)
        //                {
        //                    specloop = 1;
        //                }
        //                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

        //                string minimumRequirementMet = string.Empty;
        //                int facultyShortage = 0;
        //                int adjustedFaculty = 0;
        //                int adjustedPHDFaculty = 0;

        //                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//totalFaculty
        //                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

        //                if (departments.Contains(item.Department))
        //                {
        //                    rFaculty = (int)firstYearRequired;
        //                    departmentWiseRequiredFaculty = (int)firstYearRequired;
        //                }

        //                var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();


        //                if (item.Degree == "Pharm.D")//&& @ViewBag.BpharmcyCondition == "No"
        //                {
        //                    tFaculty = item.totalcollegefaculty;
        //                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

        //                    var pharmadreqfaculty = (int)Math.Ceiling(item.pharmadrequiredfaculty);
        //                    if (deptloop == 1)
        //                    {
        //                        if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadreqfaculty))
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty - rFaculty;

        //                            if (adjustedFaculty > pharmadreqfaculty)
        //                            {
        //                                remainingphramdFaculty = adjustedFaculty - pharmadreqfaculty;
        //                                adjustedFaculty = pharmadreqfaculty;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingphramdFaculty = tFaculty - rFaculty;
        //                        }
        //                    }
        //                }

        //                if (item.Degree == "Pharm.D PB")//&& @ViewBag.BpharmcyCondition == "No" && @ViewBag.PharmaDCondition == "No"
        //                {
        //                    tFaculty = item.totalcollegefaculty;
        //                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

        //                    var pharmadpbreqfaculty = (int)Math.Ceiling(item.pharmadPBrequiredfaculty);
        //                    if (deptloop == 1)
        //                    {
        //                        if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadpbreqfaculty))
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty - rFaculty;

        //                            if (remainingphramdFaculty > pharmadpbreqfaculty)
        //                            {
        //                                adjustedFaculty = pharmadpbreqfaculty;
        //                                remainingphramdFaculty = remainingphramdFaculty - pharmadpbreqfaculty;
        //                            }
        //                            else
        //                            {
        //                                adjustedFaculty = remainingphramdFaculty;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "YES";
        //                        }
        //                    }
        //                }



        //                if (item.Degree == "B.Pharmacy" && indexnow > 0 && item.PharmacySubGroup1 != "SubGroup6")
        //                {
        //                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
        //                    rFaculty = item.BPharmacySubGroupRequired;
        //                    if (deptloop == 1)
        //                    {
        //                        if (rFaculty <= tFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = tFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            facultyShortage = rFaculty - tFaculty;
        //                        }

        //                        remainingPHDFaculty = item.phdFaculty;

        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = remainingFaculty + (tFaculty - rFaculty);
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else if (tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = rFaculty;
        //                            facultyShortage = rFaculty - remainingFaculty;
        //                            //remainingFaculty = 0;
        //                        }
        //                        else if (tFaculty < rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            remainingFaculty = remainingFaculty - tFaculty;
        //                        }
        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }

        //                }

        //                else if (item.Degree == "B.Pharmacy" && indexnow == 0 && item.PharmacySubGroup1 != "SubGroup6")
        //                {
        //                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
        //                    rFaculty = item.BPharmacySubGroupRequired;
        //                    if (deptloop == 1)
        //                    {
        //                        if (rFaculty <= tFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = tFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            facultyShortage = rFaculty - tFaculty;
        //                        }

        //                        remainingPHDFaculty = item.phdFaculty;

        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "YES";
        //                            remainingFaculty = remainingFaculty - rFaculty;
        //                            adjustedFaculty = rFaculty;
        //                        }
        //                        else if (tFaculty >= rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = remainingFaculty;
        //                            facultyShortage = rFaculty - remainingFaculty;
        //                            remainingFaculty = 0;
        //                        }
        //                        else if (tFaculty < rFaculty)
        //                        {
        //                            minimumRequirementMet = "NO";
        //                            adjustedFaculty = tFaculty;
        //                            //remainingFaculty = remainingFaculty - tFaculty;
        //                        }




        //                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                        {
        //                            adjustedPHDFaculty = 1;
        //                            remainingPHDFaculty = remainingPHDFaculty - 1;
        //                        }
        //                    }

        //                }

        //                ////New
        //                //var mpharmremainingfaculty = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                //if (item.Degree == "M.Pharmacy" && PharmDRequiredFaculty <= 0 && item.PharmacyspecializationWiseFaculty >= 1)
        //                //{
        //                //    tFaculty = TotalcollegeFaculty;
        //                //    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                //    //remainingFaculty = tFaculty - rFaculty;

        //                //    var mpharmacyreqfaculty = (int)Math.Ceiling(item.requiredFaculty);
        //                //    if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= mpharmacyreqfaculty))
        //                //    {
        //                //        minimumRequirementMet = "YES";
        //                //        adjustedFaculty = tFaculty - rFaculty;

        //                //        if (remainingFaculty > mpharmacyreqfaculty)
        //                //        {
        //                //            adjustedFaculty = mpharmacyreqfaculty;
        //                //            remainingFaculty = remainingFaculty - mpharmacyreqfaculty;
        //                //        }
        //                //        else
        //                //        {
        //                //            adjustedFaculty = remainingFaculty;
        //                //        }
        //                //    }
        //                //}

        ////New


        //                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& (PharmDRequiredFaculty > 0 || PharmDPBRequiredFaculty > 0)
        //                {
        //                    //tFaculty = (int)Math.Ceiling(Convert.ToDecimal(list[i].specializationWiseFaculty));PharmDRequiredFaculty
        //                    //if (remainingphramdFaculty > 0)
        //                    //{
        //                    //    tFaculty = remainingphramdFaculty;
        //                    //}
        //                    //else
        //                    //{
        //                    //    tFaculty = remainingFaculty;
        //                    //}

        //                    tFaculty = remainingmpharmacyfaculty;
        //                    rFaculty = (int)Math.Ceiling(item.requiredFaculty);


        //                    if (rFaculty <= tFaculty && remainingphramdFaculty > 0)
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        remainingphramdFaculty = remainingphramdFaculty - rFaculty;
        //                        adjustedFaculty = rFaculty;
        //                    }
        //                    else if (rFaculty <= tFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "NO";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = rFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - rFaculty;
        //                    }

        //                    //else if (tFaculty <= rFaculty && remainingphramdFaculty > 0)
        //                    //{
        //                    //    remainingphramdFaculty = remainingphramdFaculty - rFaculty;
        //                    //    adjustedFaculty = tFaculty;
        //                    //}
        //                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty == 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty == 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty > 0)
        //                    {
        //                        minimumRequirementMet = "YES";
        //                        remainingFaculty = remainingFaculty - rFaculty;
        //                        adjustedFaculty = tFaculty;
        //                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
        //                    }
        //                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //                    {
        //                        adjustedPHDFaculty = 1;
        //                        remainingPHDFaculty = remainingPHDFaculty - 1;
        //                    }
        //                }



        //                if ((item.Degree == "B.Pharmacy") && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired)
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";//Yes
        //                    }
        //                }

        //                else if (item.Degree == "Pharm.D" && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(PharmDRequiredFaculty) && bpharmacycondition == "No")
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                        pharmadsubgroupmet = "No";
        //                        //minimumRequirementMet = "NO";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";
        //                        //minimumRequirementMet = "YES";
        //                    }
        //                }

        //                else if (item.Degree == "Pharm.D PB" && item.PharmacyGroup1 == "Group1")
        //                {
        //                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(pharmadpbrequiredfaculty) && bpharmacycondition == "No" && pharmadsubgroupmet == "No")
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                        //minimumRequirementMet = "NO";
        //                    }
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";
        //                        //minimumRequirementMet = "YES";
        //                    }
        //                }



        //                if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& @adjustedFaculty == rFaculty
        //                {
        //                    if (bpharmacycondition == "No")//&& pharmdcondition == "No" && pharmadpbcondition == "No"
        //                    {
        //                        item.BPharmacySubGroupMet = "No Deficiency";
        //                        //minimumRequirementMet = "NO";
        //                    }
        //                    //else if (bpharmacycondition == "No")//&& pharmdcondition == "No" && pharmadpbrequiredfaculty == 0
        //                    //{
        //                    //    item.BPharmacySubGroupMet = "No";
        //                    //}
        //                    else
        //                    {
        //                        item.BPharmacySubGroupMet = "Deficiency";
        //                        //minimumRequirementMet = "YES";
        //                    }

        //                }
        //                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty < 1)
        //                {
        //                    item.BPharmacySubGroupMet = "Deficiency";
        //                    minimumRequirementMet = "YES";
        //                }
        //                //else if (item.Degree == "M.Pharmacy")
        //                //{
        //                //    item.BPharmacySubGroupMet = "Yes";
        //                //}


        //                faculty += "<tr>";
        //                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";

        //                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Department + "</td>";
        //                if (specloop == 1)
        //                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Degree + "</td>";
        //                if (specloop == 1)
        //                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Specialization + "</td>";


        //                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
        //                if (item.totalIntake > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (Math.Ceiling(item.requiredFaculty) > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 == null)
        //                {
        //                    if (TotalcollegeFaculty > Math.Ceiling(item.requiredFaculty))
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
        //                    }
        //                    else
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + TotalcollegeFaculty + " </td>";
        //                    }

        //                }
        //                else if (item.Degree == "M.Pharmacy" || item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
        //                {
        //                    totalusedfaculty = totalusedfaculty + adjustedFaculty;
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + " </td>";
        //                }
        //                else if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 != null)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
        //                }

        //                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.PharmacySubGroup1 + "</td>";
        //                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
        //                }
        //                //else if (item.Degree == "M.Pharmacy")
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
        //                //}
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }
        //                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
        //                }
        //                else if (item.Degree == "M.Pharmacy")
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                if (item.BPharmacySubGroupMet == null && item.Degree == "B.Pharmacy")
        //                {
        //                    if (bpharmacycondition == "No")
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy No Deficiency.</b></td>";
        //                    }
        //                    else
        //                    {
        //                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy Deficiency Exists & Hence Other Degrees will not be considered. </b></td>";
        //                    }
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupMet + "</td>";
        //                }

        //                if (item.phdFaculty > 0 || item.totalIntake > 0)
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";
        //                }
        //                else
        //                {
        //                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
        //                }

        //                //if (adjustedPHDFaculty > 0)
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //                //}
        //                //else if (item.approvedIntake1 > 0)
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
        //                //}
        //                //else
        //                //{
        //                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //                //}

        //                faculty += "</tr>";

        //                if (minimumRequirementMet == "YES" && item.Degree == "B.Pharmacy")
        //                {
        //                    if (rFaculty <= adjustedFaculty)
        //                        minimumRequirementMet = "NO";
        //                    else
        //                        minimumRequirementMet = "YES";
        //                }

        //                else if (minimumRequirementMet == "NO" && item.Degree == "B.Pharmacy")
        //                {
        //                    if (rFaculty == adjustedFaculty)
        //                        minimumRequirementMet = "NO";
        //                    else
        //                        minimumRequirementMet = "YES";
        //                }

        //                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //                int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
        //                newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
        //                newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //                newFaculty.Degree = item.Degree;
        //                newFaculty.Department = item.Department;
        //                newFaculty.Specialization = item.Specialization;
        //                newFaculty.SpecializationId = item.specializationId;
        //                newFaculty.TotalIntake = item.approvedIntake1;

        //                if (departments.Contains(item.Department))
        //                {
        //                    //newFaculty.TotalIntake = totalBtechFirstYearIntake;
        //                    newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
        //                }
        //                else
        //                {
        //                    // newFaculty.TotalIntake = item.totalIntake;
        //                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
        //                }

        //                newFaculty.Available = adjustedFaculty;
        //                if (item.Degree != "B.Pharmacy" && Allgroupscount > 0)
        //                {
        //                    newFaculty.Deficiency = "YES";
        //                }
        //                else if (item.Degree != "B.Pharmacy" && Allgroupscount == 0)
        //                {
        //                    newFaculty.Deficiency = minimumRequirementMet;
        //                }

        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    newFaculty.Required = (int)Math.Ceiling(BpharmacyrequiredFaculty);
        //                    newFaculty.Available = (int)Math.Ceiling(BpharmacyrequiredFaculty) - Allgroupscount;
        //                    newFaculty.Deficiency = Allgroupscount > 0 ? "YES" : "NO";
        //                }


        //                if (item.PharmacyspecializationWiseFaculty >= 1 && degreeType.Equals("PG") && item.approvedIntake1 > 0)
        //                {
        //                    newFaculty.PhdDeficiency = "NO";
        //                }
        //                else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
        //                {
        //                    newFaculty.PhdDeficiency = "YES";
        //                }
        //                else
        //                {
        //                    newFaculty.PhdDeficiency = "-";
        //                }
        //                if (specloop == 1)
        //                    lstFaculty.Add(newFaculty);

        //                deptloop++;
        //                specloop++;
        //            }


        //            faculty += "</table>";

        //            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //            faculty += "<tr>";
        //            faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy";
        //            faculty += "<td align='left'>* I, II Year for M.Pharmacy";
        //            faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D";
        //            faculty += "<td align='left'>* IV, V Year for Pharm.D PB";
        //            faculty += "</tr>";
        //            faculty += "</table>";

        //            return lstFaculty;
        //        }



        //public List<CollegeFacultyWithIntakeReport> Affiliation4080PercentPharmacyCollegeFaculty(int? collegeId)
        //{
        //    var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
        //    {
        //        collegeId = c.id,
        //        collegeName = c.collegeCode + "-" + c.collegeName
        //    }).OrderBy(c => c.collegeName).ToList();

        //    //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

        //    ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();



        //    var randomcode = "";
        //    if (collegeId != null)
        //    {
        //        randomcode = db.jntuh_college_randamcodes.FirstOrDefault(i => i.CollegeId == collegeId).RandamCode;
        //    }
        //    var pharmadTotalintake = 0;
        //    var pharmadPBTotalintake = 0;

        //    #region PharmacyCode
        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //    List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
        //    if (collegeId != null)
        //    {
        //        var jntuh_Bpharmacy_faculty_deficiency = db.jntuh_bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
        //        var jntuh_specialization = db.jntuh_specialization.ToList();

        //        int[] collegeIDs = null;
        //        int facultystudentRatio = 0;
        //        decimal facultyRatio = 0m;
        //        if (collegeId != 0)
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
        //        }
        //        else
        //        {
        //            collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
        //        }
        //        var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //        var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
        //        var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
        //        var jntuh_degree = db.jntuh_degree.ToList();

        //        int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //        int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
        //        foreach (var item in intake)
        //        {
        //            CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //            newIntake.id = item.id;
        //            newIntake.collegeId = item.collegeId;
        //            newIntake.academicYearId = item.academicYearId;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.isActive = item.isActive;
        //            newIntake.nbaFrom = item.nbaFrom;
        //            newIntake.nbaTo = item.nbaTo;
        //            newIntake.specializationId = item.specializationId;
        //            newIntake.Specialization = item.jntuh_specialization.specializationName;
        //            newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //            newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //            newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //            newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //            newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //            newIntake.shiftId = item.shiftId;
        //            newIntake.Shift = item.jntuh_shift.shiftName;
        //            collegeIntakeExisting.Add(newIntake);
        //        }
        //        collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

        //        //college Reg nos
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //        string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

        //        var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
        //        var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

        //        //education categoryIds UG,PG,PHD...........

        //        int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
        //        var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
        //        var jntuh_education_category = db.jntuh_education_category.ToList();

        //        var registeredFaculty = principalRegno != null ? db.jntuh_reinspection_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
        //            : db.jntuh_reinspection_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
        //        var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
        //        //Reg nos related online facultyIds
        //        var jntuh_registered_faculty1 = registeredFaculty.Where(rf => ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
        //                                                && (rf.NoSCM != true) && (rf.PANNumber != null) && (rf.PHDundertakingnotsubmitted != true)
        //                                                && (rf.Notin116 != true) && (rf.Blacklistfaculy != true))).Select(rf => new
        //                                                {
        //                                                    RegistrationNumber = rf.RegistrationNumber,
        //                                                    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
        //                                                    HighestDegreeID = rf.jntuh_reinspection_registered_faculty_education.Count() != 0 ? rf.jntuh_reinspection_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                                    IsApproved = rf.isApproved,
        //                                                    PanNumber = rf.PANNumber,
        //                                                    AadhaarNumber = rf.AadhaarNumber,
        //                                                    jntuh_registered_faculty_education = rf.jntuh_reinspection_registered_faculty_education
        //                                                }).ToList();
        //        jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
        //        var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
        //        {
        //            RegistrationNumber = rf.RegistrationNumber,
        //            Department = rf.Department,
        //            HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
        //            Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
        //            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
        //            PanNumber = rf.PanNumber,
        //            AadhaarNumber = rf.AadhaarNumber,
        //            registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
        //        }).ToList();


        //        var bpharmacyintake = 0;
        //        decimal BpharcyrequiredFaculty = 0;
        //        decimal PharmDrequiredFaculty = 0;
        //        decimal PharmDPBrequiredFaculty = 0;
        //        var pharmacydeptids = new[] { 26, 27, 36, 39 };
        //        collegeIntakeExisting = collegeIntakeExisting.Where(i => pharmacydeptids.Contains(i.DepartmentID)).ToList();
        //        foreach (var item in collegeIntakeExisting)
        //        {
        //            CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
        //            int phdFaculty = 0;
        //            int pgFaculty = 0;
        //            int ugFaculty = 0;

        //            intakedetails.collegeId = item.collegeId;
        //            intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
        //            intakedetails.collegeRandomCode = randomcode;
        //            intakedetails.Degree = item.Degree;
        //            intakedetails.Department = item.Department;
        //            intakedetails.Specialization = item.Specialization;
        //            intakedetails.specializationId = item.specializationId;
        //            intakedetails.DepartmentID = item.DepartmentID;
        //            intakedetails.shiftId = item.shiftId;
        //            var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
        //            if (status.Count > 0)
        //            {
        //                intakedetails.AffliationStatus = "A";
        //            }
        //          //  intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
        //           // intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
        //           // intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
        //          //  intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
        //          //  intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
        //            facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

        //            if (item.Degree == "B.Tech")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
        //                                            (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
        //            }
        //            else if (item.Degree == "M.Tech" || item.Degree == "MBA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

        //            }
        //            else if (item.Degree == "MCA")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
        //                               Convert.ToDecimal(facultystudentRatio);
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //            }
        //            else if (item.Degree == "B.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                var total = intakedetails.totalIntake > 400 ? 100 : 60;
        //                bpharmacyintake = total;
        //                ApprovedIntake = intakedetails.approvedIntake1;
        //                specializationId = intakedetails.specializationId;
        //            }
        //            else if (item.Degree == "M.Pharmacy")
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

        //            }
        //            else if (item.Degree == "Pharm.D")
        //            {
        //                intakedetails.totalIntake = pharmadTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                            (intakedetails.approvedIntake5);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                PharmaDApprovedIntake = intakedetails.approvedIntake1;
        //                PharmaDspecializationId = intakedetails.specializationId;
        //            }
        //            else if (item.Degree == "Pharm.D PB")
        //            {
        //                intakedetails.totalIntake = pharmadPBTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //                PharmaDPBApprovedIntake = intakedetails.approvedIntake1;
        //                PharmaDPBspecializationId = intakedetails.specializationId;
        //            }
        //            else //MAM MTM Pharm.D Pharm.D PB
        //            {
        //                intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
        //                                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
        //                                            (intakedetails.approvedIntake5);
        //                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
        //                intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
        //            }


        //            intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

        //            //====================================
        //            // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();



        //            string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //            if (strdegreetype == "UG")
        //            {
        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    intakedetails.SortId = 1;
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
        //                }
        //            }

        //            if (strdegreetype == "PG")
        //            {
        //                if (item.Degree == "M.Pharmacy")
        //                {
        //                    intakedetails.SortId = 4;
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //                    switch (item.specializationId)
        //                    {
        //                        case 114://Hospital & Clinical Pharmacy
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice/Pharm D";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP" || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization == "PHARMD".ToUpper() || f.registered_faculty_specialization == "PHARM D" || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
        //                            break;
        //                        case 116://Pharmaceutical Analysis & Quality Assurance
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharma Chemistry";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA" || f.registered_faculty_specialization == "PA RA" || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
        //                            break;
        //                        case 118://Pharmaceutical Management & Regulatory Affaires
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PMRA/Regulatory Affairs/Pharmaceutics";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PMRA".ToUpper() || f.registered_faculty_specialization == "Regulatory Affairs".ToUpper() || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
        //                            break;
        //                        case 120://Pharmaceutics
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
        //                            break;
        //                        case 122://Pharmacology
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP".ToUpper() || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
        //                            break;
        //                        case 124://Quality Assurance & Pharma Regulatory Affairs
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
        //                            var s = jntuh_registered_faculty.Where(f => (f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "QA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "PA RA".ToUpper() ||
        //                                         f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA"))).ToList();
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                            break;
        //                        case 115://Industrial Pharmacy
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
        //                            break;
        //                        case 121://Pharmacognosy
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
        //                            break;
        //                        case 117://Pharmaceutical Chemistry
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
        //                            break;
        //                        case 119://Pharmaceutical Technology (2011-12)
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
        //                            break;
        //                        case 123://Quality Assurance
        //                            intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
        //                            //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
        //                            intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
        //                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
        //                            break;
        //                        default:
        //                            intakedetails.PharmacySpec1 = "";
        //                            intakedetails.PharmacyspecializationWiseFaculty = 0;
        //                            phdFaculty = 0;
        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //                }
        //            }

        //            if (strdegreetype == "Dual Degree")
        //            {
        //                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
        //            }
        //            intakedetails.id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

        //            if (intakedetails.id > 0)
        //            {
        //                int? swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                if (swf != null)
        //                {
        //                    intakedetails.specializationWiseFaculty = (int)swf;
        //                }
        //                intakedetails.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                intakedetails.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
        //            }

        //            //============================================

        //            int noPanOrAadhaarcount = 0;

        //            if (item.Degree == "B.Pharmacy")
        //            {
        //                intakedetails.SortId = 1;
        //                BpharcyrequiredFaculty = Math.Round(intakedetails.requiredFaculty);
        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
        //                phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
        //                noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                intakedetails.Department = "Pharmacy";
        //            }
        //            else if (item.Degree == "M.Pharmacy")
        //            {
        //                intakedetails.SortId = 4;
        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                //phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
        //                noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //                intakedetails.Department = "Pharmacy";
        //            }
        //            else if (item.Degree == "Pharm.D")
        //            {
        //                intakedetails.SortId = 2;
        //                PharmDRequiredFaculty = PharmDrequiredFaculty = intakedetails.requiredFaculty;
        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
        //                phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //            }
        //            else if (item.Degree == "Pharm.D PB")
        //            {
        //                intakedetails.SortId = 3;
        //                PharmDPBRequiredFaculty = PharmDPBrequiredFaculty = intakedetails.requiredFaculty;
        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
        //                noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //            }
        //            else
        //            {
        //                ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
        //                pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
        //                phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
        //                noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
        //            }

        //            intakedetails.phdFaculty = phdFaculty;
        //            intakedetails.pgFaculty = pgFaculty;
        //            intakedetails.ugFaculty = ugFaculty;
        //            intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);

        //            intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
        //            //=============//


        //            //intakedetails.PharmacySpecilaizationList = pharmacyspeclist;
        //            intakedetailsList.Add(intakedetails);
        //        }
        //    #endregion

        //        var pharmdspeclist = new List<PharmacySpecilaizationList>
        //        {
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacy Practice",
        //            //    Specialization = "Pharm.D"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharm D",
        //            //    Specialization = "Pharm.D"
        //            //}
        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
        //                Specialization = "Pharm.D"
        //            }
        //        };
        //        var pharmdpbspeclist = new List<PharmacySpecilaizationList>
        //        {
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacy Practice",
        //            //    Specialization = "Pharm.D PB"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharm D",
        //            //    Specialization = "Pharm.D PB"
        //            //}
        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
        //                Specialization = "Pharm.D PB"
        //            }
        //        };

        //        var pharmacyspeclist = new List<PharmacySpecilaizationList>
        //        {
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmaceutics",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Industrial Pharmacy",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacy BioTechnology",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmaceutical Technology",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmaceutical Chemistry",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacy Analysis",
        //            //    Specialization = "B.Pharmacy"
        //            //},

        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "PAQA",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacology",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            ////new PharmacySpecilaizationList()
        //            ////{
        //            ////    PharmacyspecName = "Pharma D",
        //            ////    Specialization = "B.Pharmacy"
        //            ////},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Pharmacognosy",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "English",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Mathematics",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Computers",
        //            //    Specialization = "B.Pharmacy"
        //            //},new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Computer Science",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Zoology",
        //            //    Specialization = "B.Pharmacy"
        //            //}


        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group1 (Pharmaceutics)",//, Industrial Pharmacy, Pharmacy BioTechnology, Pharmaceutical Technology
        //                Specialization = "B.Pharmacy"
        //            },
        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)",
        //                Specialization = "B.Pharmacy"
        //            },
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Group3 (Pharmacy Analysis, PAQA)",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group3 (Pharmacology)",
        //                Specialization = "B.Pharmacy"
        //            },
        //            new PharmacySpecilaizationList()
        //            {
        //                PharmacyspecName = "Group4 (Pharmacognosy)",
        //                Specialization = "B.Pharmacy"
        //            },
        //            //new PharmacySpecilaizationList()
        //            //{
        //            //    PharmacyspecName = "Group6 (English, Mathematics, Computers)",
        //            //    Specialization = "B.Pharmacy"
        //            //},
        //        };


        //        TotalcollegeFaculty = jntuh_registered_faculty.Count;

        //        #region All B.Pharmacy Specializations

        //        var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();
        //        var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
        //        var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
        //        string subgroupconditionsmet;
        //        string conditionbpharm = null;
        //        string conditionpharmd = null;
        //        string conditionphardpb = null;
        //        foreach (var list in pharmacyspeclist)
        //        {
        //            int phd;
        //            int pg;
        //            int ug;
        //            var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //            bpharmacylist.Specialization = list.Specialization;
        //            bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //            bpharmacylist.collegeId = (int)collegeId;
        //            bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //            bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //            bpharmacylist.collegeRandomCode = randomcode;
        //            bpharmacylist.shiftId = 1;
        //            bpharmacylist.Degree = "B.Pharmacy";
        //            bpharmacylist.Department = "Pharmacy";
        //            bpharmacylist.PharmacyGroup1 = "Group1";

        //            bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            //bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
        //            //bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
        //            //bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
        //            //bpharmacylist.totalFaculty = ug + pg + phd;

        //            bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //            bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
        //            bpharmacylist.SortId = 1;
        //            bpharmacylist.approvedIntake1 = ApprovedIntake;
        //            bpharmacylist.specializationId = specializationId;
        //            #region bpharmacyspecializationcount

        //            if (list.PharmacyspecName == "Pharmaceutics")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }

        //            else if (list.PharmacyspecName == "Industrial Pharmacy")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }
        //            else if (list.PharmacyspecName == "Pharmacy BioTechnology")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                f.registered_faculty_specialization == "Bio-Technology".ToUpper());

        //            }
        //            else if (list.PharmacyspecName == "Pharmaceutical Technology")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
        //                    f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                bpharmacylist.requiredFaculty = 3;
        //            }
        //            else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.requiredFaculty = 2;
        //            }
        //            else if (list.PharmacyspecName == "Pharmacy Analysis")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }

        //            else if (list.PharmacyspecName == "PAQA")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                             f.registered_faculty_specialization == "PA & QA".ToUpper() ||
        //                    //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
        //                    //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
        //                             f.registered_faculty_specialization == "QAPRA".ToUpper() ||
        //                             f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
        //                             f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
        //                bpharmacylist.requiredFaculty = 1;
        //            }
        //            else if (list.PharmacyspecName == "Pharmacology")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }

        //            else if (list.PharmacyspecName == "Pharma D")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                               f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                              f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                              f.registered_faculty_specialization == "Pharm.D".ToUpper());
        //                bpharmacylist.requiredFaculty = 2;
        //            }
        //            else if (list.PharmacyspecName == "Pharmacognosy")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                              f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
        //                              f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
        //                bpharmacylist.requiredFaculty = 2;
        //            }

        //            else if (list.PharmacyspecName == "English")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }
        //            else if (list.PharmacyspecName == "Mathematics")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }
        //            else if (list.PharmacyspecName == "Computers")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }
        //            else if (list.PharmacyspecName == "Computer Science")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //            }
        //            else if (list.PharmacyspecName == "Zoology")
        //            {
        //                bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
        //            }
        //            #endregion





        //            if (list.PharmacyspecName == "Group1 (Pharmaceutics)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
        //            {
        //                group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
        //                //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
        //                //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
        //                //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
        //                //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
        //                //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
        //                bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
        //                bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
        //                bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics)";
        //                bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 4;
        //            }

        //            else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
        //            {
        //                group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //                bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
        //                bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
        //                bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)";
        //                bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 5;
        //            }
        //            //else if (list.PharmacyspecName == "Group3 (Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
        //            //{
        //            //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
        //            //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
        //            //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
        //            //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

        //            //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
        //            //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //            //    bpharmacylist.BPharmacySubGroupRequired = 1;
        //            //    bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacy Analysis, PAQA)";
        //            //}

        //            else if (list.PharmacyspecName == "Group3 (Pharmacology)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //            {
        //                group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
        //                bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
        //                bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
        //                bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology)";
        //                bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 5 : 4;
        //            }

        //            else if (list.PharmacyspecName == "Group4 (Pharmacognosy)" || list.PharmacyspecName == "Pharmacognosy")
        //            {
        //                group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
        //                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
        //                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));
        //                bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
        //                bpharmacylist.BPharmacySubGroupRequired = 3;
        //                bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy)";
        //                bpharmacylist.requiredFaculty = 3;
        //            }

        //            //else if (list.PharmacyspecName == "Group6 (English, Mathematics, Computers)" || list.PharmacyspecName == "English" || list.PharmacyspecName == "Mathematics" || list.PharmacyspecName == "Computers" || list.PharmacyspecName == "Computer Science")//|| list.PharmacyspecName == "Zoology"
        //            //{
        //            //    group6Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "English".ToUpper()) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Mathematics".ToUpper()) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER")) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER SCIENCE")) +
        //            //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("CSE"));
        //            //    //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("ZOOLOGY"));
        //            //    bpharmacylist.BPharmacySubGroup1Count = group6Subcount;
        //            //    bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake == 100 ? 3 : 2;
        //            //    bpharmacylist.PharmacySubGroup1 = "Group6 (English, Mathematics, Computers)";
        //            //}



        //            var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //            if (id > 0)
        //            {
        //                var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                if (swf != null)
        //                {
        //                    bpharmacylist.specializationWiseFaculty = (int)swf;
        //                }
        //                bpharmacylist.id = id;
        //                bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //            }

        //            intakedetailsList.Add(bpharmacylist);
        //        }

        //        //for pharma D specializations
        //        var pharmaD = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
        //        if (pharmaD.Count > 0)
        //        {
        //            foreach (var list in pharmdspeclist)
        //            {
        //                int phd;
        //                int pg;
        //                int ug;
        //                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                bpharmacylist.Specialization = list.Specialization;
        //                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                bpharmacylist.collegeId = (int)collegeId;
        //                bpharmacylist.collegeRandomCode = randomcode;
        //                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                bpharmacylist.shiftId = 1;
        //                bpharmacylist.Degree = "Pharm.D";
        //                bpharmacylist.Department = "Pharm.D";
        //                bpharmacylist.PharmacyGroup1 = "Group1";
        //                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
        //                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
        //                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
        //                bpharmacylist.totalFaculty = ug + pg + phd;
        //                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                bpharmacylist.SortId = 2;
        //                bpharmacylist.approvedIntake1 = PharmaDApprovedIntake;
        //                bpharmacylist.specializationId = PharmaDspecializationId;
        //                #region pharmadSpecializationcount
        //                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
        //                {
        //                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                }
        //                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
        //                {
        //                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                }
        //                #endregion


        //                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                {
        //                    pharmadgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper());
        //                    bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
        //                    bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
        //                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
        //                    bpharmacylist.requiredFaculty = Math.Ceiling(PharmDrequiredFaculty);
        //                }


        //                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                if (id > 0)
        //                {
        //                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                    if (swf != null)
        //                    {
        //                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                    }
        //                    bpharmacylist.id = id;
        //                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                }

        //                intakedetailsList.Add(bpharmacylist);
        //            }
        //        }


        //        //for pharma.D PB specializations
        //        var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
        //        if (pharmaDPB.Count > 0)
        //        {
        //            foreach (var list in pharmdpbspeclist)
        //            {
        //                int phd;
        //                int pg;
        //                int ug;
        //                var bpharmacylist = new CollegeFacultyWithIntakeReport();
        //                bpharmacylist.Specialization = list.Specialization;
        //                bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
        //                bpharmacylist.collegeId = (int)collegeId;
        //                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
        //                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
        //                bpharmacylist.collegeRandomCode = randomcode;
        //                bpharmacylist.shiftId = 1;
        //                bpharmacylist.Degree = "Pharm.D PB";
        //                bpharmacylist.Department = "Pharm.D PB";
        //                bpharmacylist.PharmacyGroup1 = "Group1";
        //                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
        //                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
        //                bpharmacylist.totalFaculty = ug + pg + phd;
        //                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
        //                bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
        //                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
        //                bpharmacylist.SortId = 3;
        //                bpharmacylist.approvedIntake1 = PharmaDPBApprovedIntake;
        //                bpharmacylist.specializationId = PharmaDPBspecializationId;
        //                #region pharmadPbSpecializationcount
        //                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
        //                {
        //                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
        //                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
        //                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
        //                }
        //                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
        //                {
        //                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
        //                }
        //                #endregion


        //                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
        //                {
        //                    pharmadPBgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
        //                                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
        //                                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
        //                                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
        //                                    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
        //                                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
        //                    bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
        //                    bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
        //                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
        //                    bpharmacylist.requiredFaculty = Math.Ceiling(PharmDPBrequiredFaculty);
        //                }


        //                var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
        //                if (id > 0)
        //                {
        //                    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
        //                    if (swf != null)
        //                    {
        //                        bpharmacylist.specializationWiseFaculty = (int)swf;
        //                    }
        //                    bpharmacylist.id = id;
        //                    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
        //                    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
        //                }

        //                intakedetailsList.Add(bpharmacylist);
        //            }
        //        }

        //        if (BpharcyrequiredFaculty > 0)
        //        {

        //            if (bpharmacyintake >= 100)
        //            {
        //                BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
        //                ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //            }
        //            else
        //            {
        //                BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
        //                ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //                BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
        //            }
        //            intakedetailsList.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharcyrequiredFaculty);


        //            if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty)
        //            {
        //                if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                {
        //                    subgroupconditionsmet = conditionbpharm = "No";
        //                }
        //                else
        //                {
        //                    subgroupconditionsmet = conditionbpharm = "Yes";
        //                }
        //            }
        //            else
        //            {
        //                subgroupconditionsmet = conditionbpharm = "Yes";
        //            }

        //            ViewBag.BpharmcyCondition = conditionbpharm;
        //            bpharmacycondition = conditionbpharm;
        //            intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);


        //        }

        //        if (PharmDrequiredFaculty > 0)
        //        {
        //            if (jntuh_registered_faculty.Count >= PharmDrequiredFaculty)
        //            {
        //                if (pharmadgroup1Subcount >= pharmadTotalintake / 30)
        //                {
        //                    subgroupconditionsmet = conditionpharmd = "No";
        //                }
        //                else
        //                {
        //                    subgroupconditionsmet = conditionpharmd = "Yes";
        //                }
        //            }
        //            else
        //            {
        //                subgroupconditionsmet = conditionpharmd = "Yes";
        //            }

        //            ViewBag.PharmaDCondition = conditionpharmd;
        //            pharmdcondition = conditionpharmd;
        //            if (conditionbpharm == "No")
        //            {
        //                intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
        //            }
        //            else
        //            {
        //                intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
        //            }


        //        }

        //        ViewBag.PharmDPBrequiredFaculty = PharmDPBrequiredFaculty;
        //        pharmadpbrequiredfaculty = PharmDPBrequiredFaculty;
        //        if (PharmDPBrequiredFaculty > 0)
        //        {
        //            if (jntuh_registered_faculty.Count >= PharmDPBrequiredFaculty)
        //            {
        //                if (pharmadPBgroup1Subcount >= pharmadPBTotalintake / 10)
        //                {
        //                    subgroupconditionsmet = conditionphardpb = "No";
        //                }
        //                else
        //                {
        //                    subgroupconditionsmet = conditionphardpb = "Yes";
        //                }
        //            }
        //            else
        //            {
        //                subgroupconditionsmet = conditionphardpb = "Yes";
        //            }

        //            ViewBag.PharmaDPBCondition = conditionphardpb;
        //            pharmadpbcondition = conditionphardpb;
        //            if (conditionbpharm == "No" && conditionpharmd == "No")
        //            {
        //                intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
        //            }
        //            else
        //            {
        //                intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
        //            }

        //        }



        //        #endregion

        //        intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

        //        #region Faculty Appeal Deficiency Status
        //        //var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
        //        var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
        //        foreach (var item in intakedetailsList.Where(i => i.Degree == "B.Pharmacy").ToList())
        //        {
        //            var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
        //            if (deparment != null)
        //            {
        //                var facultydefcount = 0;//(int)Math.Ceiling(item.requiredFaculty) - item.totalFaculty

        //                if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
        //                {
        //                    if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                    {
        //                        Allgroupscount = 0;
        //                    }
        //                    else
        //                    {
        //                        //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                        if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group4Subcount < 3)
        //                        {
        //                            var count = 3 - group4Subcount;
        //                            count = count == 1 ? 0 : count;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                    }
        //                    facultydefcount = Allgroupscount;
        //                }

        //                else if (jntuh_registered_faculty.Count < BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
        //                {
        //                    if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
        //                    {
        //                        Allgroupscount = 0;
        //                    }
        //                    else
        //                    {
        //                        //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
        //                        if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
        //                        {
        //                            var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                        if (group4Subcount < 3)
        //                        {
        //                            var count = 3 - group4Subcount;
        //                            count = count == 1 ? 0 : count;
        //                            Allgroupscount = Allgroupscount + count;
        //                        }
        //                    }

        //                    var lessfaculty = BpharcyrequiredFaculty - jntuh_registered_faculty.Count;

        //                    if (lessfaculty > Allgroupscount)
        //                    {
        //                        facultydefcount = Allgroupscount;//(int)lessfaculty + 
        //                    }
        //                    else if (Allgroupscount > lessfaculty)
        //                    {
        //                        facultydefcount = Allgroupscount;//+ (int)lessfaculty
        //                    }
        //                }

        //                if (item.Degree == "B.Pharmacy")
        //                {
        //                    if (Allgroupscount > 0)
        //                    {
        //                        //item.deficiency = true; 
        //                    }
        //                    ViewBag.BpharmacyRequired = facultydefcount;
        //                }

        //                //if (item.PharmacyspecializationWiseFaculty < 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
        //                //{
        //                //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty) + 1;
        //                //}
        //                //if (item.PharmacyspecializationWiseFaculty >= 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
        //                //{
        //                //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty);
        //                //}
        //                //if (item.Department == "Pharm.D" || item.Department == "Pharm.D PB")
        //                //{
        //                //    facultydefcount = item.BPharmacySubGroupRequired - item.BPharmacySubGroup1Count;
        //                //}

        //            }
        //        }


        //        #endregion
        //    }
        //    return intakedetailsList;
        //}

        #endregion


        #endregion
    }
}