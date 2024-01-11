using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AllCollegesDeficiencyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private decimal sComputers = 0;
        private decimal sFaculty = 0;

        private int[] MbaMcaIds = null;
        private string[] MbaMcaDegrees = { "MCA", "MBA" };
        private int[] allCollegeDegreeIds = null;
        private int[] otherDegreeIds = null;

        private string ReportHeader = null;
        //int[] ActiveCollegeIds;
        //int[] SubmittedCollegeIds;
        private int nextAcademicYearId = 0;
        private int academicYearId = 0;
        private int presentActualYear = 0;
        int InspectionPhaseId = 0;
        private int PrincipalShortageCount = 0;
        private int LabShortageCount = 0;
        private int LibraryShortageCount = 0;
        private string StrLibraryShortage = null;
        private string StrLabsShortage = null;
        private string StrPrincipalShortage = null;
        private string StrComputersShortageDegrees = null;
        private string StrFacultyShortageDegrees = null;
        private int ComputersShortageCount = 0;
        private int FacultyShortageCount = 0;

        private string[] StrNill = {    "-", 
                                        "- ", 
                                        "_",
                                        "--", 
                                        "---",
                                        "ACCEPTS", 
                                        "ADEQUATE",
                                        "ARE PROVIDED",
                                        "AS PER NORMS",
                                        "AVAILABLE AS PER NORMS",
                                        "AVAILABLE", 
                                        "AVAILABLE.",
                                        "AVAILABLE AND ADEQUATE",
                                        "AVERAGE SATISFACTORY",
                                        "BOOKS ARE THERE NORMS. NO SHORTAGE",
                                        "DO", 
                                        "FACILITIES ARE AVAILABLE ARE SUFFICIENT", 
                                        "GOOD",
                                        "GOOD AMBIENCE,PG LABS ARE WELL ESTABLISHED.",
                                        "GOOD AMBIANCE,ENOUGH BOOKS AVAILABLE.",
                                        "LABS ARE WELL EQUIPPED EXPERIMENTS WERE CONDUCTED PROPERLY",
                                        "LABORATORIES ARE TO",
                                        "LIBRARY IS TO LOCATED AT ONE FACE.",
                                        "NEED TO BE IMPROVED.",
                                        "NEW EDITIONS ARE TO BE ADDED.",
                                        "NEW REPUTED INTERNATIONAL JOURNAL MAY BE SUBSCRIBED",
                                        "NIL", 
                                        "NILL",
                                        "NIl",
                                        "NO COMPLAINTS",
                                        "NO DEFICIENCIES ALL ARE SUFFICIENT", 
                                        "NO DEFICIENCY ( B. TECH AGRICULTURAL COURSE WAS DROPPED )",
                                        "NO DEFICIENCY FOUND", 
                                        "NO DEFICIENCY",
                                        "NO DEFICIENCY ",
                                        "NO DEFICIENCY.",
                                        "NO DEFFICIENCY.",
                                        "NO DEFICIENCY EXCEPT FOR ONLINE JOURNALS.",
                                        "NO MAJOR COMPLAINTS",
                                        "NO SHORTAGE",
                                        "NO SHORTAGE.",
                                        "NO SPECIFIC DEFICIENCIES",
                                        "NONE", 
                                        "NOT MAJOR DEFICIENCIES",
                                        "OK",
                                        "ONE LIBRARIAN ONLY BOOKS ARE SUFFICIENT IN LIBRARY SETTING CAPACITY IS ONLY 30",
                                        "ORDER IN PLACED FOR JOURNALS",
                                        "REQUIRED NO OF UG AND PG LABS AVAILABLE",
                                        "REQUIRED LABS ARE AVAILABLE AS PER NORMS",
                                        "REQUIRED NUMBER OF LABS AVAILABLE",
                                        "REQUIRED NO OF LABS AVAILABLE AS PER NORMS",
                                        "-SATISFACTORY",
                                        "SATISFACTORY",
                                        "SATISFACTORY.",
                                        "SATISFACTION",
                                        "SUFFICIENT AVAILABLE.", 
                                        "SUFFICIENT LABS AVAILABLE",
                                        "SUFFICIENT NO DEFICIENCY",
                                        "SUFFICIENT NO OF BOOKS AND JOURNALS ARE AVAILABLE AND STUDENTS HAVE E-JOURNALS ACCESS",
                                        "SUFFICIENT",
                                        "SUFFICIENT NUMBER OF BOOKS AND BOOKS AND JOURNALS AVAILABLE AS PER NORMS",
                                        "SUFFICIENT NUMBER OF BOOKS AND JOURNALS AVAILABLE AS PER NORMS",
                                        "SUFFICIENT BOOKS AND JOURNALS AVAILABLE",
                                        "SUFFICIENT BOOKS ARE AVAILABLE",
                                        "SUFFICIENT LIBRARY BOOKS AND JOURNALS AVAILABLE",
                                        "SUFFICIENT VOLUMES ARE PROVIDED",
                                        "SUFFICIENT BOOKS AND TITLES ARE AVAILABLE",
                                        "TO BE IMPROVED",
                                        "TITLE,VOLUMES,SITTING CAPACITY IS ADEQUATE",
                                        "THERMAL ENGINEERING AND HEAT TRANSFER LABS TO BE ESTABLISHED ORDERS TO BE PLACED DETAILS ENCLOSED",
                                        "UG & PG LABS ADEQUATE",
                                        "WELL MAINTAINED",
                                        "WELL EQUIPPED LABS"
                                   };

        int filterCollegeId = 0;
        string filterReportType = string.Empty;
        string filterShortageType = string.Empty;
        int[] filterMultipleDegrees = new int[] { };
        int filterOnlyDegree = 0;
        int[] filterColumns = new int[] { };
        string filterRemarksType = string.Empty;
        string filterViewType = string.Empty;

        //gets pre-requisite ids which can be useful in the respective methods
        private void GetIds()
        {
            //Get present academic Year row
            var presentAY = db.jntuh_academic_year.Where(year => year.isActive == true && year.isPresentAcademicYear == true).Select(year => year).FirstOrDefault();

            //Get Present actual year
            presentActualYear = presentAY.actualYear;

            //Get next academic year id by adding 1
            academicYearId = presentAY.id;

            //Get next academic year id by adding 1
            nextAcademicYearId = db.jntuh_academic_year.Where(year => year.actualYear == (presentActualYear + 1)).Select(year => year.id).FirstOrDefault();

            // get active inspection phase id
            InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).FirstOrDefault();

            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId })
                                          .Distinct().OrderByDescending(p => p.id).ToList();
        }

        //setup filter variables for GET/POST
        private void SetFilters(string method, int selectedCollegeId, int[] selectedDegrees, int[] onlyDegreeSelected, int[] reportTypeIDs, int[] shortageTypeIDs, int[] columnIDs, int[] remarkTypeIDs, int[] viewTypeIDs)
        {
            if (method.Equals("GET"))
            {
                //setup filters with default values
                filterCollegeId = 0;
            }
            else
            {
                //setup filters with selected values
                filterCollegeId = selectedCollegeId;
            }

            filterReportType = (from r in ReportType() where r.id == reportTypeIDs[0] select r.name).FirstOrDefault().ToUpper();
            filterShortageType = (from r in ShortageType() where r.id == shortageTypeIDs[0] select r.name).FirstOrDefault().ToUpper();
            filterMultipleDegrees = selectedDegrees;

            if (onlyDegreeSelected.Count() > 0)
            {
                filterOnlyDegree = onlyDegreeSelected[0];
            }

            filterColumns = columnIDs;
            filterRemarksType = (from r in RemarksType() where r.id == remarkTypeIDs[0] select r.name).FirstOrDefault().ToUpper();
            filterViewType = (from r in ViewType() where r.id == viewTypeIDs[0] select r.name).FirstOrDefault().ToUpper();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AllCollegesDeficiency()
        {
            DateTime dtStart = DateTime.Now;
            DeficiencyCollege deficiencyCollege = new DeficiencyCollege();

            //Get required Ids
            GetIds();

            //multiple degrees
            int[] selectedDegrees = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            deficiencyCollege.degree = Degrees(selectedDegrees);

            //olny one degree
            int[] onlyDegreeSelected = new int[] { };
            deficiencyCollege.degreeOnly = Degrees(onlyDegreeSelected);

            //report type
            int[] reportTypeIDs = new int[] { 1 };
            deficiencyCollege.reportType = ListItems(ReportType(), reportTypeIDs);

            //shortage type
            int[] shortageTypeIDs = new int[] { 1 };
            deficiencyCollege.shortageType = ListItems(ShortageType(), shortageTypeIDs);

            //Columns
            int[] columnIDs = new int[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            deficiencyCollege.columnList = ListItems(Columns(), columnIDs);

            //Remarks Type
            int[] remarkTypeIDs = new int[] { 2 };
            deficiencyCollege.remarksType = ListItems(RemarksType(), remarkTypeIDs);

            //View Type
            int[] viewTypeIDs = new int[] { 1 };
            deficiencyCollege.viewType = ListItems(ViewType(), viewTypeIDs);

            //setup filters with default values
            SetFilters("GET", deficiencyCollege.CollegeId, selectedDegrees, onlyDegreeSelected, reportTypeIDs, shortageTypeIDs, columnIDs, remarkTypeIDs, viewTypeIDs);

            //generate colleges deficiencies into a table
            //DeficiencyList(filterCollegeId, filterReportType, filterShortageType, filterRemarksType, deficiencyCollege.columnList.ToList());

            //bind colleges dropdown with active colleges
            deficiencyCollege.AllColleges = db.jntuh_college.Where(c => c.isActive == true)
                                                        .Select(c => new CollegeInfo
                                                        {
                                                            Id = c.id,
                                                            Name = c.collegeCode + " - " + c.collegeName
                                                        }).OrderBy(c => c.Name).ToList();

            DateTime dtEnd = DateTime.Now;
            TimeSpan span = (dtEnd - dtStart);
            ViewBag.Time = String.Format("{0} minutes, {1} seconds", span.Minutes, span.Seconds);
            //ViewBag.Time = String.Format("{0} days, {1} hours, {2} minutes, {3} seconds", span.Days, span.Hours, span.Minutes, span.Seconds);

            return View("~/Views/Reports/AllCollegesDeficiency.cshtml", deficiencyCollege);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AllCollegesDeficiency(DeficiencyCollege deficiencyCollege, string cmd)
        {
            DateTime dtStart = DateTime.Now;
            List<DeficiencyCollege> DeficiencyCollegeList = new List<DeficiencyCollege>();

            //multiple degrees
            int[] selectedDegrees = (from row in deficiencyCollege.degreeId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.degree = Degrees(selectedDegrees);

            //olny one degree
            int[] onlyDegreeSelected = (from row in deficiencyCollege.degreeOnlyId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.degreeOnly = Degrees(onlyDegreeSelected);

            //report type
            int[] reportTypeIDs = (from row in deficiencyCollege.reportTypeId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.reportType = ListItems(ReportType(), reportTypeIDs);

            //shortage type
            int[] shortageTypeIDs = (from row in deficiencyCollege.shortageTypeId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.shortageType = ListItems(ShortageType(), shortageTypeIDs);

            //Columns
            int[] columnIDs = (from row in deficiencyCollege.columnId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.columnList = ListItems(Columns(), columnIDs);

            //Remarks Type
            int[] remarkTypeIDs = (from row in deficiencyCollege.remarksTypeId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.remarksType = ListItems(RemarksType(), remarkTypeIDs);

            //View Type
            int[] viewTypeIDs = (from row in deficiencyCollege.viewTypeId where row != "" select Convert.ToInt32(row)).ToArray();
            deficiencyCollege.viewType = ListItems(ViewType(), viewTypeIDs);

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds();
            if (deficiencyCollege.InspectionPhaseId != 0)
            {
                InspectionPhaseId = deficiencyCollege.InspectionPhaseId;
            }

            //setup filters with selected values
            SetFilters("POST", deficiencyCollege.CollegeId, selectedDegrees, onlyDegreeSelected, reportTypeIDs, shortageTypeIDs, columnIDs, remarkTypeIDs, viewTypeIDs);

            DeficiencyCollegeList = DeficiencyList(filterCollegeId, filterReportType, filterShortageType, filterRemarksType, deficiencyCollege.columnList.ToList(), selectedDegrees, onlyDegreeSelected, filterViewType);

            int Count = DeficiencyCollegeList.Count();
            DateTime dtEnd = DateTime.Now;
            TimeSpan span = (dtEnd - dtStart);
            ViewBag.Time = String.Format("{0} minutes, {1} seconds", span.Minutes, span.Seconds);

            //export or show
            if (cmd == "Export" && Count != 0)
            {
                if (ReportHeader == null)
                {
                    ReportHeader = "All_Colleges_Deficiencies.xls";
                }

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AllCollegesDeficiency.cshtml", deficiencyCollege);
            }
            else
            {
                return View("~/Views/Reports/AllCollegesDeficiency.cshtml", deficiencyCollege);
            }
        }

        //generates college deficiencies of all types including remarks
        private List<DeficiencyCollege> DeficiencyList(int selectedCollegeId, string reportType, string shortageFilter, string remarksFilter, List<Item> columns, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            List<DeficiencyCollege> DeficiencyList = new List<DeficiencyCollege>();
            List<DeficiencyCollege> identifiedDeficiencyList = new List<DeficiencyCollege>();


            DeficiencyList = db.jntuh_college.Join(db.jntuh_college_establishment, c => c.id, e => e.collegeId, (c, e) => new { c.id, c.isActive, c.collegeCode, c.collegeName, e.instituteEstablishedYear, c.isPermant })
                               //.Join(db.jntuh_ffc_schedule, c => c.id, s => s.collegeID, (c, s) => new { c.id, c.isActive, c.isNew, c.collegeCode, c.collegeName, c.instituteEstablishedYear, c.isPermant, s.InspectionPhaseId })
                               .Where(college => college.isActive == true
                                   && (selectedCollegeId == 0 || college.id == selectedCollegeId)                   // if college id selected
                                   && (!reportType.Equals("PERMANENT COLLEGES") || college.isPermant == true)       // for permanent colleges
                                   )                               // && college.InspectionPhaseId == InspectionPhaseId)
                               .Select(college => new DeficiencyCollege
                               {
                                   CollegeId = college.id,
                                   CollegeCode = college.collegeCode,
                                   CollegeName = college.collegeName,
                                   Establishyear = college.instituteEstablishedYear
                               }).ToList();

            //if college is selected
            if (selectedCollegeId != 0)
            {
                ReportHeader = DeficiencyList.Select(c => c.CollegeCode).FirstOrDefault() + "_Deficiencies.xls";
            }

            bool isValid = true;

            for (int i = 0; i <= DeficiencyList.Count() - 1; i++)
            {
                isValid = true;
                var item = DeficiencyList[i];

                //if selected in multiplt degrees even though one degree selected
                if (selectedDegrees.Count() > 0 && onlyDegreeSelected.Count() == 0)
                {
                    allCollegeDegreeIds = CollegeDegreesIds(item.CollegeId, selectedDegrees, onlyDegreeSelected, viewFilter);

                    if (allCollegeDegreeIds.Count() == 0)
                    {
                        isValid = false;
                    }
                }
                //if selected in only degree
                else if (selectedDegrees.Count() == 0 && onlyDegreeSelected.Count() > 0)
                {
                    allCollegeDegreeIds = CollegeDegreesIds(item.CollegeId, selectedDegrees, onlyDegreeSelected, viewFilter);

                    if (allCollegeDegreeIds.Count() == 0)
                    {
                        isValid = false;
                    }
                }

                if (isValid == true)
                {
                    LabShortageCount = 0;
                    ComputersShortageCount = 0;
                    PrincipalShortageCount = 0;
                    FacultyShortageCount = 0;
                    LibraryShortageCount = 0;
                    StrLabsShortage = null;
                    StrPrincipalShortage = null;
                    StrLibraryShortage = null;
                    sComputers = 0;
                    sFaculty = 0;

                    item.CollegeAddress = GetCollegeAddress(item.CollegeId);

                    if (reportType.Equals("PERMANENT COLLEGES"))
                    {
                        item.IsPermanentCollege = "Permanent College";
                    }

                    //get shortages at the begining only to identify the college deficiencies
                    //then categorise the college into - "WITH DEFICICENCY / WITHOUT DEFICICENCY"

                    if (columns[1].selected.ToString().Equals("1"))
                    {
                        item.LabsShortage = GetLabsShortage(item.CollegeId);
                    }

                    if (columns[2].selected.ToString().Equals("1"))
                    {
                        item.ComputersShortage = GetComputersShortageList(item.CollegeId, shortageFilter, remarksFilter, selectedDegrees, onlyDegreeSelected, viewFilter);
                    }

                    if (columns[3].selected.ToString().Equals("1"))
                    {
                        item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
                    }

                    if (columns[4].selected.ToString().Equals("1"))
                    {
                        item.FacultyShortage = GetFacultyShortageList(item.CollegeId, shortageFilter, remarksFilter, selectedDegrees, onlyDegreeSelected, viewFilter);
                    }

                    if (columns[6].selected.ToString().Equals("1"))
                    {
                        item.LibraryShortage = GetLibraryShortage(item.CollegeId);
                    }

                    //
                    if (reportType.Equals("WITHOUT DEFICIENCY") && LabShortageCount == 0 && ComputersShortageCount == 0 && PrincipalShortageCount == 0 && FacultyShortageCount == 0 && LibraryShortageCount == 0)
                    {
                        SelectCollege(columns, remarksFilter, item, identifiedDeficiencyList, selectedDegrees, onlyDegreeSelected, viewFilter);
                    }
                    else if (reportType.Equals("WITH DEFICIENCY") && (LabShortageCount == 0 || ComputersShortageCount == 0 || PrincipalShortageCount == 0 || FacultyShortageCount == 0 || LibraryShortageCount == 0))
                    {
                        SelectCollege(columns, remarksFilter, item, identifiedDeficiencyList, selectedDegrees, onlyDegreeSelected, viewFilter);
                    }
                    else if (reportType.Equals("ALL COLLEGES") || reportType.Equals("PERMANENT COLLEGES"))
                    {
                        SelectCollege(columns, remarksFilter, item, identifiedDeficiencyList, selectedDegrees, onlyDegreeSelected, viewFilter);
                    }
                }
            }

            //set order by college name for all colleges
            identifiedDeficiencyList = identifiedDeficiencyList.OrderBy(c => c.CollegeName).ToList();

            //assign colleges data to viewbag
            ViewBag.CollegeSpecializations = identifiedDeficiencyList;

            //assign colleges count to viewbag
            ViewBag.Count = identifiedDeficiencyList.Count();

            //return data of all colleges
            return identifiedDeficiencyList;
        }

        private void SelectCollege(List<Item> columns, string remarksFilter, DeficiencyCollege item, List<DeficiencyCollege> identifiedDeficiencyList, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            if (columns[0].selected.ToString().Equals("1"))
            {
                item.CollegeSpecializations = GetCollegeSpecializations(item.CollegeId, selectedDegrees, onlyDegreeSelected, viewFilter);
            }

            if (columns[5].selected.ToString().Equals("1"))
            {
                //item.Number = GetFacultyNumber(item.CollegeId);
                //item.Percentage = GetFacultyPercentage(item.CollegeId);
            }

            if (columns[7].selected.ToString().Equals("1"))
            {
                item.OverallPoints = GetOverallPoints(item.CollegeId);
            }

            if (columns[8].selected.ToString().Equals("1"))
            {
                item.NBAStatus = GetNBAStatus(item.CollegeId);
            }

            if (columns[9].selected.ToString().Equals("1"))
            {
                item.NAACStatus = GetNAACStatus(item.CollegeId);
            }

            if (columns[10].selected.ToString().Equals("1"))
            {
                item.UGObservations = GetCollegeRemarks(item.CollegeId, remarksFilter);
            }

            if (columns[11].selected.ToString().Equals("1"))
            {
                //item.Grade = db.college_grade.Where(g => g.collegeId == item.CollegeId && g.inspectionPhaseId == InspectionPhaseId).Select(g => g.grade).FirstOrDefault();
            }

            identifiedDeficiencyList.Add(item);
        }

        private string GetCollegeAddress(int CollegeId)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string Address = string.Empty;
            string District = string.Empty;

            //get the college address row 
            jntuh_address CollegeAddress = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == CollegeId).FirstOrDefault();

            //add city
            if (!string.IsNullOrEmpty(CollegeAddress.townOrCity))
            {
                sb.Append(CollegeAddress.townOrCity + ", ");
            }

            //add mandal
            if (!string.IsNullOrEmpty(CollegeAddress.mandal))
            {
                sb.Append(CollegeAddress.mandal + ", ");
            }

            //get district name and add to address
            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district.Where(d => d.id == CollegeAddress.districtId).Select(d => d.districtName).FirstOrDefault();
                sb.Append(District + ", ");
            }

            //add pincode
            if (CollegeAddress.pincode != 0)
            {
                sb.Append(CollegeAddress.pincode.ToString());
            }

            Address = sb.ToString();

            return Address;
        }

        //Binding Specializations data 
        private List<CollegeSpecialization> GetCollegeSpecializations(int CollegeId, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            string NewSpecialization = string.Empty;
            string NewIntake = string.Empty;

            allCollegeDegreeIds = CollegeDegreesIds(CollegeId, selectedDegrees, onlyDegreeSelected, viewFilter);

            List<CollegeSpecialization> collegeSpecializations = new List<CollegeSpecialization>();

            //get college specialization with degree & department details
            List<CollegeDegreeDetails> collegeDegreeDetails = db.jntuh_college_intake_existing
                .Join(db.jntuh_specialization, p => p.specializationId, s => s.id, (p, s) => new { p.collegeId, p.academicYearId, p.isActive, p.specializationId, p.shiftId, p.approvedIntake, p.proposedIntake, s.specializationName, s.departmentId })
                .Join(db.jntuh_department, t => t.departmentId, d => d.id, (t, d) => new { t.collegeId, t.academicYearId, t.isActive, t.specializationId, t.shiftId, t.approvedIntake, t.proposedIntake, t.specializationName, t.departmentId, d.degreeId })
                .Join(db.jntuh_degree, t => t.degreeId, de => de.id, (t, de) => new { t.collegeId, t.academicYearId, t.isActive, t.specializationId, t.shiftId, t.approvedIntake, t.proposedIntake, t.specializationName, t.departmentId, t.degreeId, de.id, de.degree, de.degreeTypeId, de.degreeDisplayOrder })
                .Join(db.jntuh_degree_type, t => t.degreeTypeId, dt => dt.id, (t, dt) => new { t.collegeId, t.academicYearId, t.isActive, t.specializationId, t.shiftId, t.approvedIntake, t.proposedIntake, t.specializationName, t.departmentId, t.degreeId, t.id, t.degree, t.degreeTypeId, dt.degreeType, t.degreeDisplayOrder })
                .Where(t => t.collegeId == CollegeId && t.academicYearId == nextAcademicYearId && t.isActive == true
                                  && (allCollegeDegreeIds.Contains(t.degreeId))
                                  )
                .Select(t => new CollegeDegreeDetails
                {
                    CollegeId = t.collegeId,
                    SpecializationId = t.specializationId,
                    ShiftId = t.shiftId,
                    ProposedIntake = (int)t.proposedIntake,
                    ApprovedIntake = t.approvedIntake,
                    DegreeType = t.degreeType,
                    DegreeName = t.degree,
                    SpecializationName = t.specializationName,
                    DegreeId = t.id,
                    DegreeDisplayOrder = t.degreeDisplayOrder
                }).OrderBy(t => new { t.DegreeDisplayOrder, t.SpecializationName }).ToList();

            //get intake for each specialization based on degree & department
            for (int i = 0; i <= collegeDegreeDetails.Count() - 1; i++)
            {
                var item = collegeDegreeDetails[i];

                if (item.DegreeType != null)
                {
                    NewIntake = string.Empty;
                    NewSpecialization = string.Empty;

                    //get existing intake for the specialization
                    int existingIntake = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId && e.specializationId == item.SpecializationId && e.shiftId == item.ShiftId && e.academicYearId == academicYearId && e.isActive == true).Select(e => e.approvedIntake).FirstOrDefault();

                    //RAMESH : DOUBT - Verify later - 12/09/2014
                    //int existingIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == CollegeId && ex.specializationId == item.SpecializationId && ex.shiftId == item.ShiftId).Select(ex => ex.approvedIntake).FirstOrDefault();

                    //For PG Courses
                    if (item.DegreeType.ToUpper().Trim() == "PG")
                    {
                        //if degree is MBA then sum all specializations proposed intake to show as one specialization instead of showing multiple specializations
                        if (item.DegreeName.ToUpper().Trim().Equals("MBA"))
                        {
                            NewSpecialization = item.DegreeName;
                            NewIntake = GetMBAIntake(item.CollegeId, item.DegreeId);
                        }
                        else
                        {
                            //for M.Tech / M.Pharmacy specialization name should include shift also like Degreename-specializationname-shiftname
                            if (item.DegreeName.ToUpper().Trim().Equals("M.TECH") || item.DegreeName.ToUpper().Trim().Equals("M.PHARMACY"))
                            {
                                NewSpecialization = item.ShiftId == 1 ? (item.DegreeName + "-" + item.SpecializationName) : (item.DegreeName + "-" + item.SpecializationName + "-" + "(II Shift)");
                                NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake, existingIntake);
                            }
                            else
                            //for other than M.Tech / M.Pharmacy courses specialization name should be like specializationname-shiftname
                            //if (!item.DegreeName.ToUpper().Trim().Equals("M.TECH") && !item.DegreeName.ToUpper().Trim().Equals("M.PHARMACY"))
                            {
                                NewSpecialization = item.ShiftId == 1 ? (item.SpecializationName) : (item.SpecializationName + "-" + "(II Shift)");
                                NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake, existingIntake);
                            }
                        }
                    }
                    else//For UG Courses
                    {
                        NewSpecialization = item.ShiftId == 1 ? (item.SpecializationName) : (item.SpecializationName + "-" + "(II Shift)");
                        NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake, existingIntake);
                    }

                    //if intake and specialization exists then add the specialization to the list
                    if (!string.IsNullOrEmpty(NewIntake) && !string.IsNullOrEmpty(NewSpecialization))
                    {
                        CollegeSpecialization collegeSpecialization = new CollegeSpecialization();
                        collegeSpecialization.Specialization = NewSpecialization;
                        collegeSpecialization.Intake = NewIntake;
                        collegeSpecialization.ExistingIntake = existingIntake;

                        collegeSpecializations.Add(collegeSpecialization);
                    }
                }
            }

            return collegeSpecializations;
        }

        //calculate MBA degree intake by sum of all specializations in MBA degree
        private string GetMBAIntake(int collegeId, int degreeId)
        {
            int existingIntake = 0;
            int proposedIntake = 0;

            string intake = string.Empty;

            //get proposed college specializations based on degree
            var specializations = db.jntuh_college_intake_existing
                                       .Join(db.jntuh_specialization, p => p.specializationId, s => s.id, (p, s) => new { p.collegeId, p.academicYearId, p.isActive, p.specializationId, s.id, s.departmentId, p.shiftId, p.proposedIntake, p.approvedIntake })
                                       .Join(db.jntuh_department, t => t.departmentId, d => d.id, (t, d) => new { t.collegeId, t.academicYearId, t.isActive, t.specializationId, t.id, t.departmentId, d.degreeId, t.shiftId, t.proposedIntake, t.approvedIntake })
                                       .Where(ei => ei.collegeId == collegeId && ei.academicYearId == nextAcademicYearId && ei.isActive == true && ei.degreeId == degreeId)
                                       .Select(ei => ei).ToList();

            //get existing intake from intake table using proposed specilialization id and shift id
            foreach (var item in specializations)
            {
                //add proposed intake for all specializations under mba degree
                if (item.approvedIntake != 0)
                {
                    proposedIntake += item.approvedIntake;
                }
                else if (item.proposedIntake != 0)
                {
                    proposedIntake += (int)item.proposedIntake;
                }

                //add existing intake for all specializations under mba degree
                existingIntake += db.jntuh_college_intake_existing
                                    .Where(ei => ei.collegeId == collegeId && ei.specializationId == item.specializationId &&
                                                 ei.academicYearId == academicYearId && ei.shiftId == item.shiftId)
                                   .Select(ei => ei.approvedIntake)
                                   .FirstOrDefault();

            }

            // format the intake according to increate intake (+) / decrease intake (-) / closure (*) / new course (#) / same intake ($)
            if (proposedIntake != 0)
            {
                //if exising and proposed intake are same then no change
                if (existingIntake == proposedIntake)
                {
                    intake = proposedIntake.ToString() + "$";
                }
                //if course not exists then it is new course
                else if (existingIntake == 0)
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else //if proposed intake is greater or less than existing intake then show (+) / (-) symbols
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

        //get intake for coresponding degree
        private string GetIntake(int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake, int existingIntake)
        {
            string intake = string.Empty;
            
            if (proposedIntake != 0)
            {
                //same intake
                if (proposedIntake == existingIntake)
                {
                    intake = proposedIntake.ToString() + "$";
                }

                //increase intake
                if (proposedIntake > existingIntake)
                {
                    intake = proposedIntake.ToString() + "*";
                }

                //decrease intake
                if (proposedIntake < existingIntake)
                {
                    intake = proposedIntake.ToString() + "*";
                }

                //new course
                if (existingIntake == 0)
                {
                    intake = proposedIntake.ToString() + "#";
                }
            }
            else
            {
                //closure
                intake = proposedIntake.ToString() + "*";
            }

            return intake;
        }

        //Get college labs shortage
        private string GetLabsShortage(int collegeId)
        {
            //HARDCODED: observationTypeId=12 for labs in jntuh_committee_observations
            string collegeLabsShortage = db.jntuh_college_committee_observations
                                    .Where(c => c.collegeId == collegeId && c.observationTypeId == 12 && !StrNill.Contains(c.observations.ToUpper().Trim()))
                                    .Select(c => c.observations.ToUpper().Trim())
                                    .FirstOrDefault();

            //if collegeLabsShortage exists
            if (!string.IsNullOrEmpty(collegeLabsShortage))
            {
                LabShortageCount++;
                StrLabsShortage = collegeLabsShortage;
                collegeLabsShortage = "*";
            }
            else //if collegeLabsShortage not exists
            {
                collegeLabsShortage = "NIL";
            }

            return collegeLabsShortage;
        }

        //gets college computer shortage details
        private List<string> GetComputersShortageList(int collegeId, string shortageFilter, string remarksFilter, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            List<string> compuetrsShortageList = new List<string>();

            int requiredRatio = 0;
            decimal totalIntake = 0;
            decimal actualRatio = 0;
            decimal deficiency = 0;
            decimal totalComputers = 0;

            //get MBA & MCA ids
            MbaMcaIds = db.jntuh_degree.Where(d => d.isActive == true && MbaMcaDegrees.Contains(d.degree.Trim())).Select(d => d.id).ToArray();

            //get college degreeIds
            allCollegeDegreeIds = CollegeDegreesIds(collegeId, selectedDegrees, onlyDegreeSelected, viewFilter);

            //filter college degreeIds other than MBA & MCA
            otherDegreeIds = (from d in allCollegeDegreeIds where !MbaMcaIds.Contains(d) select d).ToArray();

            //filter college degreeIds for MBA & MCA
            int[] collegeMbaMcaIds = (from d in allCollegeDegreeIds where MbaMcaIds.Contains(d) select d).ToArray();

            string degreeName = string.Empty;

            if (otherDegreeIds.Count() > 0)
            {
                foreach (var item in otherDegreeIds)
                {
                    degreeName = string.Empty;

                    totalIntake = 0;
                    actualRatio = 0;
                    requiredRatio = 0;
                    totalComputers = 0;
                    deficiency = 0;

                    //get Computer:Student NORMS for the degreeid
                    requiredRatio = Convert.ToInt32(db.jntuh_computer_student_ratio_norms.Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms).FirstOrDefault().Split(':')[1]);

                    //get total intake
                    totalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();

                    //get total computers
                    totalComputers = db.jntuh_college_computer_student_ratio.Where(c => c.collegeId == collegeId && c.degreeId == item)
                                       .Select(c => c.availableComputers).FirstOrDefault();

                    //if both total intake & total computers exists
                    if (totalIntake > 0 && totalComputers > 0)
                    {
                        //calculate actual ratio
                        actualRatio = totalIntake / totalComputers;

                        //if both required & actual ratios exists
                        if (requiredRatio > 0 && actualRatio > 0)
                        {
                            degreeName = db.jntuh_degree.Where(d => d.id == item).Select(d => d.degree).FirstOrDefault();

                            // if required ratio is less than actual ratio --> "SHORTAGE"
                            if (requiredRatio < decimal.Round(actualRatio))
                            {
                                if (shortageFilter.Equals("SHOW RATIO & NIL"))
                                {
                                    //With Ratio only
                                    ComputersShortageCount++;
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                                }
                                else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                                {
                                    //With Ratio and Count
                                    if (totalIntake > totalComputers)
                                    {
                                        deficiency = (totalIntake - (totalComputers * requiredRatio)) / requiredRatio;
                                    }
                                    else
                                    {
                                        deficiency = 0;
                                    }

                                    StrComputersShortageDegrees += degreeName + ", ";
                                    ComputersShortageCount++;
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero)) + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";

                                }

                                if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                                {
                                    if (deficiency > 0)
                                        sComputers += decimal.Round(deficiency);
                                }
                            }
                            // if required ratio is greater than actual ratio --> "NO SHORTAGE"
                            else
                            {
                                if (shortageFilter.Equals("SHOW RATIO & NIL"))
                                {
                                    //With Ratio only
                                    degreeName = degreeName + " - NIL";
                                }
                                else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                                {
                                    //With Ratio and Count
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                                }
                            }
                        }
                    }
                    else
                    //if (TotalIntake != 0 && TotalComputers == 0)
                    //if (totalComputers == 0 || (totalIntake == 0 && totalComputers > 0))
                    {
                        if (shortageFilter.Equals("SHOW RATIO & NIL"))
                        {
                            //With Ratio only
                            degreeName = db.jntuh_degree.Where(d => d.id == item).Select(d => d.degree).FirstOrDefault();

                            ComputersShortageCount++;
                            degreeName = degreeName + " - No Computers";
                        }
                        else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                        {
                            //With Ratio and Count
                            degreeName = db.jntuh_degree.Where(d => d.id == item).Select(d => d.degree).FirstOrDefault();

                            if (totalIntake > totalComputers)
                            {
                                deficiency = (totalIntake - (totalComputers * requiredRatio)) / requiredRatio;
                            }
                            else
                            {
                                deficiency = 0;
                            }

                            ComputersShortageCount++;

                            StrComputersShortageDegrees += degreeName + ", ";

                            //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                            degreeName = degreeName + " - No Computers" + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                        }

                        if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                        {
                            if (deficiency > 0)
                                sComputers += decimal.Round(deficiency);
                        }
                    }

                    compuetrsShortageList.Add(degreeName);
                }
            }

            if (collegeMbaMcaIds.Count() > 0)
            {
                degreeName = string.Empty;

                totalIntake = 0;
                actualRatio = 0;
                requiredRatio = 0;
                totalComputers = 0;
                deficiency = 0;

                foreach (var item in MbaMcaIds)
                {
                    requiredRatio = Convert.ToInt32(db.jntuh_computer_student_ratio_norms
                                         .Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms)
                                         .FirstOrDefault().Split(':')[1]);

                    //get total intake
                    totalIntake += db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();

                    //get total computers
                    totalComputers += db.jntuh_college_computer_student_ratio.Where(c => c.collegeId == collegeId && c.degreeId == item)
                                       .Select(c => c.availableComputers).FirstOrDefault();
                }

                //if both total intake & total computer exists
                if (totalIntake > 0 && totalComputers > 0)
                {
                    //calculate actual ratio
                    actualRatio = totalIntake / totalComputers;

                    //if both required & actual ratios exists
                    if (requiredRatio > 0 && actualRatio > 0)
                    {
                        degreeName = "MBA/MCA";

                        // if required ratio is less than actual ratio --> "SHORTAGE"
                        if (requiredRatio < decimal.Round(actualRatio))
                        {
                            if (shortageFilter.Equals("SHOW RATIO & NIL"))
                            {
                                //With Ratio only
                                ComputersShortageCount++;
                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                            }
                            else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                            {
                                //With Ratio and Count
                                ComputersShortageCount++;
                                if (totalIntake > totalComputers)
                                {
                                    deficiency = (totalIntake - (totalComputers * requiredRatio)) / requiredRatio;
                                }
                                else
                                {
                                    deficiency = 0;
                                }

                                StrComputersShortageDegrees += degreeName + ", ";
                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero)) + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                            }

                            if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                            {
                                if (deficiency > 0)
                                    sComputers += decimal.Round(deficiency);
                            }
                        }
                        // if required ratio is greater than actual ratio --> "NO SHORTAGE"
                        else
                        {
                            if (shortageFilter.Equals("SHOW RATIO & NIL"))
                            {
                                //With Ratio only
                                degreeName = degreeName + " - NIL";
                            }
                            else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                            {
                                //With Ratio and Count
                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }
                }
                else
                //if (TotalIntake != 0 && TotalComputers == 0)
                //if (totalComputers == 0 || (totalIntake == 0 && totalComputers != 0))
                {
                    degreeName = "MBA/MCA";
                    if (shortageFilter.Equals("SHOW RATIO & NIL"))
                    {
                        //With Ratio only
                        ComputersShortageCount++;
                        degreeName = degreeName + " - No Computers";
                    }
                    else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                    {
                        //With Ratio and Count
                        if (totalIntake > totalComputers)
                        {
                            deficiency = (totalIntake - (totalComputers * requiredRatio)) / requiredRatio;
                        }
                        else
                        {
                            deficiency = 0;
                        }

                        ComputersShortageCount++;
                        StrComputersShortageDegrees += degreeName + ", ";

                        //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                        degreeName = degreeName + " - No Computers" + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                    }

                    if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                    {
                        if (deficiency > 0)
                            sComputers += decimal.Round(deficiency);
                    }
                }

                compuetrsShortageList.Add(degreeName);
            }

            return compuetrsShortageList;
        }

        //gets college faculty shortage details
        private List<string> GetFacultyShortageList(int collegeId, string shortageFilter, string remarksFilter, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            List<string> facultyShortageList = new List<string>();

            decimal actualRatio = 0;
            decimal facultyIntake = 0;
            decimal totalFaculty = 0;
            int requiredRatio = 0;
            decimal deficiency = 0;

            //get MBA & MCA ids
            MbaMcaIds = db.jntuh_degree.Where(d => d.isActive == true && MbaMcaDegrees.Contains(d.degree.Trim())).Select(d => d.id).ToArray();

            //get college degreeIds
            allCollegeDegreeIds = CollegeDegreesIds(collegeId, selectedDegrees, onlyDegreeSelected, viewFilter);

            //filter college degreeIds other than MBA & MCA
            otherDegreeIds = (from d in allCollegeDegreeIds where !MbaMcaIds.Contains(d) select d).ToArray();

            //filter college degreeIds for MBA & MCA
            int[] collegeMbaMcaIds = (from d in allCollegeDegreeIds where MbaMcaIds.Contains(d) select d).ToArray();

            string degreeName = string.Empty;

            if (otherDegreeIds.Count() > 0)
            {
                foreach (var item in otherDegreeIds)
                {
                    degreeName = string.Empty;
                    degreeName = db.jntuh_degree.Where(d => d.id == item).Select(d => d.degree).FirstOrDefault();

                    actualRatio = 0;
                    requiredRatio = 0;
                    facultyIntake = 0;
                    totalFaculty = 0;
                    deficiency = 0;

                    //get Faculty:Student NORMS for the degreeid
                    requiredRatio = Convert.ToInt32((db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true && f.degreeId == item)
                                                          .Select(f => f.Norms).FirstOrDefault()).Split(':')[1]);

                    //get total intake for the degree
                    facultyIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item)
                                      .Select(f => f.totalIntake).FirstOrDefault();

                    //get total faculty for the degree
                    totalFaculty = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item)
                                     .Select(f => f.totalFaculty).FirstOrDefault();

                    //if facultyintake & totalfaculty exists
                    if (facultyIntake > 0 && totalFaculty > 0)
                    {
                        //calculate actual faculty student ratio
                        actualRatio = facultyIntake / totalFaculty;

                        //if both requirednorms & actualratio exists
                        if (requiredRatio > 0 && actualRatio > 0)
                        {
                            // if required ratio is less than actual ratio --> "SHORTAGE"
                            if (requiredRatio < decimal.Round(actualRatio))
                            {
                                FacultyShortageCount++;
                                if (shortageFilter.Equals("SHOW RATIO & NIL"))
                                {
                                    //With Ratio only
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                                }
                                else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                                {
                                    //With Ratio and Count
                                    if (facultyIntake > totalFaculty)
                                    {
                                        deficiency = (facultyIntake - (totalFaculty * requiredRatio)) / requiredRatio;
                                    }
                                    else
                                    {
                                        deficiency = 0;
                                    }

                                    StrFacultyShortageDegrees += degreeName + ", ";
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero)) + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                                }

                                if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                                {
                                    if (deficiency > 0)
                                        sFaculty += decimal.Round(deficiency);
                                }
                            }
                            // if required ratio is greate than actual ratio --> "NO SHORTAGE"
                            else
                            {
                                if (shortageFilter.Equals("SHOW RATIO & NIL"))
                                {
                                    //With Ratio only
                                    degreeName = degreeName + " - NIL";
                                }
                                else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                                {
                                    //With Ratio and Count
                                    degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                                }
                            }
                        }
                    }
                    else
                    //if (FacultyIntake != 0 && TotalFaculty == 0)
                    //if (totalFaculty == 0)
                    {
                        FacultyShortageCount++;

                        if (shortageFilter.Equals("SHOW RATIO & NIL"))
                        {
                            //With Ratio only
                            degreeName = degreeName + " - No Faculty";
                        }
                        else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                        {
                            //With Ratio and Count
                            if (facultyIntake > totalFaculty)
                            {
                                deficiency = (facultyIntake - (totalFaculty * requiredRatio)) / requiredRatio;
                            }
                            else
                            {
                                deficiency = 0;
                            }

                            StrFacultyShortageDegrees += degreeName + ", ";
                            degreeName = degreeName + " - No Faculty" + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                        }

                        if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                        {
                            if (deficiency > 0)
                                sFaculty += decimal.Round(deficiency);
                        }
                    }

                    facultyShortageList.Add(degreeName);
                }
            }

            if (collegeMbaMcaIds.Count() > 0)
            {
                degreeName = string.Empty;
                degreeName = "MBA/MCA";

                facultyIntake = 0;
                totalFaculty = 0;
                actualRatio = 0;
                requiredRatio = 0;
                deficiency = 0;

                foreach (var item in MbaMcaIds)
                {
                    requiredRatio += Convert.ToInt32((db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true && f.degreeId == item)
                                                        .Select(f => f.Norms).FirstOrDefault()).Split(':')[1]);

                    facultyIntake += db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item)
                                       .Select(f => f.totalIntake).FirstOrDefault();

                    totalFaculty += db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item)
                                      .Select(f => f.totalFaculty).FirstOrDefault();
                }

                //if facultyintake & totalfaculty exists
                if (facultyIntake > 0 && totalFaculty > 0)
                {
                    //calculate actual ratio
                    actualRatio = facultyIntake / totalFaculty;

                    //if noth required & actual ratios exists
                    if (requiredRatio != 0 && actualRatio != 0)
                    {
                        // if required ratio is less than actual ratio --> "SHORTAGE"
                        if (requiredRatio < decimal.Round(actualRatio))
                        {
                            FacultyShortageCount++;

                            if (shortageFilter.Equals("SHOW RATIO & NIL"))
                            {
                                //With Ratio only
                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                            }
                            else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                            {
                                //With Ratio and Count
                                StrFacultyShortageDegrees += degreeName + ", ";
                                if (facultyIntake > totalFaculty)
                                {
                                    deficiency = (facultyIntake - (totalFaculty * requiredRatio)) / requiredRatio;
                                }
                                else
                                {
                                    deficiency = 0;
                                }

                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero)) + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                            }

                            if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                            {
                                if (deficiency > 0)
                                    sFaculty += decimal.Round(deficiency);
                            }
                        }
                        // if required ratio is greater than actual ratio --> "NO SHORTAGE"
                        else
                        {
                            if (shortageFilter.Equals("SHOW RATIO & NIL"))
                            {
                                //With Ratio only
                                degreeName = degreeName + " - NIL";
                            }
                            else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                            {
                                //With Ratio and Count
                                degreeName = degreeName + " - 1:" + Convert.ToString(Math.Round(actualRatio, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }
                }
                else
                //if (FacultyIntake != 0 && TotalFaculty == 0)
                //if (totalFaculty == 0)
                {
                    FacultyShortageCount++;
                    if (shortageFilter.Equals("SHOW RATIO & NIL"))
                    {
                        //With Ratio only
                        degreeName = degreeName + " - No Faculty";
                    }
                    else if (shortageFilter.Equals("SHOW RATIO & SHORTAGE COUNT"))
                    {
                        //With Ratio and Count
                        if (facultyIntake > totalFaculty)
                        {
                            deficiency = (facultyIntake - (totalFaculty * requiredRatio)) / requiredRatio;
                        }
                        else
                        {
                            deficiency = 0;
                        }

                        StrFacultyShortageDegrees += degreeName + ", ";
                        degreeName = degreeName + " - No Faculty" + " (" + Convert.ToString(decimal.Round(deficiency)) + ")";
                    }

                    if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                    {
                        if (deficiency > 0)
                            sFaculty += decimal.Round(deficiency);
                    }
                }

                facultyShortageList.Add(degreeName);
            }

            return facultyShortageList;
        }

        //gets college total faculty
        //private string GetFacultyNumber(int CollegeId)
        //{
        //    string FacultyNum;
        //    int facultyRatifiedSum = 0;
        //    int facultySum = 0;
        //    int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                .Where(f => f.collegeId == CollegeId)
        //                                .Select(f => f.ratified)
        //                                .ToArray();
        //    int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                .Where(f => f.collegeId == CollegeId)
        //                                .Select(f => f.totalFaculty)
        //                                .ToArray();
        //    if (facultyRatifiedCount.Count() != 0)
        //    {
        //        facultyRatifiedSum = facultyRatifiedCount.Sum();
        //    }
        //    if (facultyCount.Count() != 0)
        //    {
        //        facultySum = facultyCount.Sum();
        //    }

        //    if (facultyRatifiedSum != 0 && facultySum != 0)
        //    {
        //        FacultyNum = Convert.ToString(facultyRatifiedSum) + "/" + Convert.ToString(facultySum) + ".";
        //    }
        //    else if (facultyRatifiedSum != 0)
        //    {
        //        FacultyNum = Convert.ToString(facultyRatifiedSum) + "/" + "0" + ".";
        //    }
        //    else if (facultySum != 0)
        //    {
        //        FacultyNum = "0" + "/" + Convert.ToString(facultySum) + ".";
        //    }
        //    else
        //    {
        //        FacultyNum = "0" + "/" + "0" + ".";
        //    }
        //    return FacultyNum;
        //}

        //gets college faculty percentage
        //private decimal GetFacultyPercentage(int CollegeId)
        //{
        //    decimal Percentage = 0;
        //    decimal facultyRatifiedSum = 0;
        //    decimal facultySum = 0;
        //    int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                .Where(f => f.collegeId == CollegeId)
        //                                .Select(f => f.ratified)
        //                                .ToArray();
        //    int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                .Where(f => f.collegeId == CollegeId)
        //                                .Select(f => f.totalFaculty)
        //                                .ToArray();
        //    if (facultyRatifiedCount.Count() != 0)
        //    {
        //        facultyRatifiedSum = facultyRatifiedCount.Sum();
        //    }
        //    if (facultyCount.Count() != 0)
        //    {
        //        facultySum = facultyCount.Sum();
        //    }
        //    if (facultyRatifiedSum != 0 && facultySum != 0)
        //    {
        //        Percentage = (facultyRatifiedSum / facultySum) * 100;
        //        Percentage = Math.Round(Percentage, 2);
        //    }
        //    return Percentage;
        //}

        private string GetLibraryShortage(int collegeId)
        {
            var collegeLibraryShortage = db.jntuh_college_committee_observations
                                   .Where(c => c.collegeId == collegeId && c.observationTypeId == 13 && !StrNill.Contains(c.observations.ToUpper().Trim()))
                                   .Select(c => c.observations)
                                   .FirstOrDefault();

            if (collegeLibraryShortage != null)
            {
                LibraryShortageCount++;
                StrLibraryShortage = collegeLibraryShortage;
                collegeLibraryShortage = "*";
            }
            else
            {
                collegeLibraryShortage = "NIL";
            }

            return collegeLibraryShortage;
        }

        //get college overallpoints
        private string GetOverallPoints(int collegeId)
        {
            int overallpoints = 0;
            int apoints = 0;
            int ipoints = 0;

            apoints = db.jntuh_college_academic_performance_points.Where(a => a.collegeId == collegeId && a.pointsObtained != null).Select(a => (int?)a.pointsObtained).Sum() ?? 0;
            ipoints = db.jntuh_college_infrastructure_parameters.Where(a => a.collegeId == collegeId && a.pointsObtained != null).Select(a => (int?)a.pointsObtained).Sum() ?? 0;

            overallpoints = apoints + ipoints;

            return overallpoints.ToString();
        }

        //gets college NBA status
        private string GetNBAStatus(int collegeId)
        {
            string NBAStatus = string.Empty;

            //NBA affiliationTypeId == 8
            var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 8).Select(a => a).FirstOrDefault();

            if (Status != null)
            {
                NBAStatus = Status.affiliationStatus;
            }
            else
            {
                NBAStatus = "Not Yet Applied";
            }

            return NBAStatus;
        }

        //gets college NAAC status
        private string GetNAACStatus(int collegeId)
        {
            string NAACStatus = string.Empty;

            //NAAC affiliationTypeId == 2
            var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 2).Select(a => a).FirstOrDefault();

            if (Status != null)
            {
                NAACStatus = Status.affiliationStatus;
            }
            else
            {
                NAACStatus = "Not Yet Applied";
            }

            return NAACStatus;
        }

        //gets college principal ratification status
        private string GetPrincipalRatified(int collegeId)
        {
            string PrincipalGrade = string.Empty;

            var PrincipalDetails = db.jntuh_college_principal_director
                                     .Where(p => p.collegeId == collegeId && p.type == "PRINCIPAL")
                                     .Select(p => new
                                     {
                                         Id = p.id,
                                         Ratified = p.isRatified,
                                     }).FirstOrDefault();

            if (PrincipalDetails != null)
            {
                if (PrincipalDetails.Ratified == true)
                {
                    PrincipalGrade = "A";
                }
                else
                {
                    PrincipalShortageCount++;
                    StrPrincipalShortage = "Principal is not ratified";
                    PrincipalGrade = "B";
                }
            }
            else
            {
                PrincipalShortageCount++;
                StrPrincipalShortage = "No Principal";
                PrincipalGrade = "*";
            }

            return PrincipalGrade;
        }

        //gets college shortage remarks
        private List<string> GetCollegeRemarks(int collegeId, string remarksFilter)
        {
            List<string> remarks = new List<string>();
            if (LabShortageCount > 0 && StrLabsShortage != null)
            {
                remarks.Add(StrLabsShortage);
            }
            if (ComputersShortageCount > 0)
            {
                if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                {
                    remarks.Add("Shortage of computers(" + sComputers + ")");
                }
                else
                {
                    remarks.Add("Shortage of computers");
                }
            }
            if (PrincipalShortageCount > 0 && StrPrincipalShortage != null)
            {
                remarks.Add(StrPrincipalShortage);
            }
            if (FacultyShortageCount > 0)
            {
                if (remarksFilter.Equals("WITH SHORTAGE COUNT"))
                {
                    remarks.Add("Shortage of faculty(" + sFaculty + ")");
                }
                else
                {
                    remarks.Add("Shortage of faculty");
                }
            }
            if (LibraryShortageCount > 0 && StrLibraryShortage != null)
            {
                remarks.Add(StrLibraryShortage);
            }
            return remarks;
        }

        //Report Type
        private List<Item> ReportType()
        {
            List<Item> reportType = new List<Item>();

            reportType.Add(new Item { id = 1, name = "All Colleges", selected = 0 });
            reportType.Add(new Item { id = 2, name = "With Deficiency", selected = 0 });
            reportType.Add(new Item { id = 3, name = "Without Deficiency", selected = 0 });
            reportType.Add(new Item { id = 4, name = "Permanent Colleges", selected = 0 });

            return reportType.ToList();
        }

        //Shortage Type
        private List<Item> ShortageType()
        {
            List<Item> shortageType = new List<Item>();
            shortageType.Add(new Item { id = 1, name = "Show Ratio & NIL", selected = 0 });
            shortageType.Add(new Item { id = 2, name = "Show Ratio & Shortage Count", selected = 0 });

            return shortageType.ToList();
        }

        //Remarks Type
        private List<Item> RemarksType()
        {
            List<Item> remarksType = new List<Item>();
            remarksType.Add(new Item { id = 1, name = "With Shortage Count", selected = 0 });
            remarksType.Add(new Item { id = 2, name = "Without Shortage Count", selected = 0 });

            return remarksType.ToList();
        }

        //View Type
        private List<Item> ViewType()
        {
            List<Item> viewType = new List<Item>();
            viewType.Add(new Item { id = 1, name = "Only Selected Degrees", selected = 0 });
            viewType.Add(new Item { id = 2, name = "Show All Degrees", selected = 0 });

            return viewType.ToList();
        }

        //Columns
        private List<Item> Columns()
        {
            List<Item> columns = new List<Item>();
            //columns.Add(new Item { id = 1, name = "S.No", selected = 0 });
            //columns.Add(new Item { id = 2, name = "College Name", selected = 0 });
            //columns.Add(new Item { id = 3, name = "College Code", selected = 0 });
            columns.Add(new Item { id = 4, name = "Specializations", selected = 0 });
            columns.Add(new Item { id = 5, name = "Labs", selected = 0 });
            columns.Add(new Item { id = 6, name = "Computers", selected = 0 });
            columns.Add(new Item { id = 7, name = "Principal", selected = 0 });
            columns.Add(new Item { id = 8, name = "Faculty", selected = 0 });
            columns.Add(new Item { id = 9, name = "Ratified", selected = 0 });
            columns.Add(new Item { id = 10, name = "Library", selected = 0 });
            columns.Add(new Item { id = 11, name = "Overall Points", selected = 0 });
            columns.Add(new Item { id = 12, name = "NBA", selected = 0 });
            columns.Add(new Item { id = 13, name = "NAAC ", selected = 0 });
            columns.Add(new Item { id = 14, name = "Remarks", selected = 0 });
            columns.Add(new Item { id = 15, name = "Grade", selected = 0 });

            return columns.ToList();
        }

        //degrees with selection/without selection
        private List<Item> Degrees(int[] selectedItems)
        {
            List<Item> degrees = (from d in db.jntuh_degree
                                  where d.isActive == true
                                  orderby d.degreeDisplayOrder
                                  select new Item
                                  {
                                      id = d.id,
                                      name = d.degree,
                                      selected = selectedItems.Contains(d.id) ? 1 : 0
                                  }).ToList();

            return degrees;
        }

        //any list with selection/without selection
        private List<Item> ListItems(List<Item> list, int[] selectedItems)
        {
            List<Item> filteredList = (from d in list
                                       select new Item
                                       {
                                           id = d.id,
                                           name = d.name,
                                           selected = selectedItems.Contains(d.id) ? 1 : 0
                                       }).ToList();

            return filteredList;
        }

        private int[] CollegeDegreesIds(int collegeId, int[] selectedDegrees, int[] onlyDegreeSelected, string viewFilter)
        {
            var collegeDegrees = new int[] { };

            //if seleted degrees exists
            if (selectedDegrees.Count() > 0)
            {
                //get selected degrees in the college degrees table
                collegeDegrees = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where cd.isActive == true && cd.collegeId == collegeId && d.isActive == true
                                  && selectedDegrees.Contains(cd.degreeId)
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();

                //if college have selected degrees
                if (collegeDegrees.Count() > 0)
                {
                    //if view filter is "SHOW ALL DEGREES" then select all degrees of the college from college degree table
                    if (viewFilter.Equals("SHOW ALL DEGREES"))
                    {
                        collegeDegrees = (from cd in db.jntuh_college_degree
                                          join d in db.jntuh_degree on cd.degreeId equals d.id
                                          where cd.isActive == true && cd.collegeId == collegeId && d.isActive == true
                                          orderby d.degreeDisplayOrder
                                          select cd.degreeId).ToArray();
                    }
                }
                else
                {
                    collegeDegrees = new int[] { };
                }
            }
            //if only degrees exists
            else
            {
                collegeDegrees = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where cd.isActive == true && cd.collegeId == collegeId && d.isActive == true
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();

                if ((collegeDegrees.Count() == 1) && (collegeDegrees[0] == onlyDegreeSelected[0]))
                {

                }
                else
                {
                    collegeDegrees = new int[] { };
                }
            }
            return collegeDegrees;
        }
    }
}
