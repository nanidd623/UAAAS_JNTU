﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DeficiencyReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private decimal sComputers = 0;
        private decimal sFaculty = 0;
        private int lastYearAdmitted = 0;

        private int academicYearId = 0;
        private int AcademicYear = 0;
        private int nextAcademicYearId = 0;
        private int LibrarycommitteobservationId = 0;
        private int[] CollegeIds;
        private int ClosureCourseId = 0;
        private int LabcommitteobservationId = 0;
        private int AcademicYearId1 = 0;
        private int AcademicYearId2 = 0;
        private int AcademicYearId3 = 0;
        private int AcademicYearId4 = 0;
        private int ComputersShortageCount = 0;
        private int FacultyShortageCount = 0;
        private int LabShortageCount = 0;
        private int LibraryShortageCount = 0;
        private int[] SubmitteCollegesId;
        private string ReportHeader = null;
        private int PrincipalShortageCount = 0;
        private string StrPrincipalShortage = null;
        private string StrLabsShortage = null;
        private string StrLibraryShortage = null;
        private int[] PermanentCollegesId;
        private int[] MBAAndMCAID;
        private int[] EmptyDegreeRatioId;
        //private string[] EmptyDegrees = { "MAM", "MTM", "Pharm.D", "Pharm.D PB" };
        private string[] EmptyDegrees = { };
        private string[] MBAAndMCAIDDegrees = { "MCA", "MBA" };
        private string StrComputersShortageDegrees = null;
        private string StrFacultyShortageDegrees = null;
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

        private string[] tmpCollegeCodes = { "H6","D9","H5","R0","7R","B8","R1","24","WJ","C4","J4","W9","N3","RH","Q9","S1","R2","N8","D4","K8","R9","K9","64","88","91","P6",
                                          "07","H1","87","62","84","7W","D0","BD","M2","RJ","31","VE","UK","E1","83","U0","5D","86","E4","X3","14","8R","TP","D2","G7","82" };

        private void GetIds()
        {
            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
            EmptyDegreeRatioId = db.jntuh_degree
                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
                                 .Select(d => d.id).ToArray();

            //Get MBA and MCA Ids
            MBAAndMCAID = db.jntuh_degree
                            .Where(d => d.isActive == true && MBAAndMCAIDDegrees.Contains(d.degree.Trim()))
                            .Select(d => d.id).ToArray();

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
            AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear + 1)).Select(a => a.id).FirstOrDefault();
            AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 1)).Select(a => a.id).FirstOrDefault();
            AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 2)).Select(a => a.id).FirstOrDefault();
            AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 3)).Select(a => a.id).FirstOrDefault();

            //Get all data entry submitted colleges 
            int[] tmpCollegeIds = db.jntuh_college.Where(c => tmpCollegeCodes.Contains(c.collegeCode))
                         .OrderBy(c => c.collegeName.ToUpper().Trim())
                         .Select(c => c.id)
                         .ToArray();

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            DateTime now = Convert.ToDateTime("08/16/2014 15:00:00");
            //Get Data entry Submitted colleges
            SubmitteCollegesId = db.jntuh_dataentry_allotment
                //.Where(d => d.isVerified == true)
                                   .Where(d => d.isCompleted == true && d.InspectionPhaseId == InspectionPhaseId) // && d.updatedOn >= now
                                   .Select(d => d.collegeID)
                                   .ToArray();
            //Get all data entry submitted colleges 
            CollegeIds = db.jntuh_college
                         .Where(c =>
                                     tmpCollegeIds.Contains(c.id)
                                     )
                         .OrderBy(c => c.collegeName.ToUpper().Trim())
                         .Select(c => c.id)
                         .ToArray();
            //get all permanent colleges 
            PermanentCollegesId = db.jntuh_college
                                    .Where(c => c.isActive == true &&
                                                SubmitteCollegesId.Contains(c.id) &&
                                                c.isPermant == true  &&
                                                c.isClosed == false)
                                    .OrderBy(c => c.collegeName.ToUpper().Trim())
                                    .Select(c => c.id)
                                    .ToArray();
            //Get lab Committe observationId
            LibrarycommitteobservationId = db.jntuh_committee_observations
                                             .Where(c => c.isActive == true &&
                                                    c.observationType == "Library")
                                             .Select(c => c.id)
                                             .FirstOrDefault();
            //Get lab Committe observationId
            LabcommitteobservationId = db.jntuh_committee_observations
                                             .Where(c => c.isActive == true &&
                                                    c.observationType == "Labs")
                                             .Select(c => c.id)
                                             .FirstOrDefault();
        }

        //private List<UGWithDeficiency> DeficiencyList(int? CollegeId, string cmd)
        //{
        //    int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

        //    int Count = 0;
        //    List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();
        //    if ((cmd == "All" || cmd == "Search" || cmd == "Export" || cmd == "PermanentCollege" || cmd == "WithOutDeficiency" || cmd == "WithDeficiency") && CollegeId != null)
        //    {
        //        Count++;
        //        UGWithDeficiencyList = db.jntuh_college
        //                                 .Where(college => college.isActive == true &&
        //                                                   college.id == CollegeId)
        //                                 .Select(college => new UGWithDeficiency
        //                                 {
        //                                     CollegeId = college.id,
        //                                     CollegeCode = college.collegeCode,
        //                                     CollegeName = college.collegeName.Trim(),
        //                                 }).ToList();
        //        //set file name as collegeCode
        //        if (UGWithDeficiencyList != null)
        //        {
        //            ReportHeader = UGWithDeficiencyList.Select(c => c.CollegeCode).FirstOrDefault() + "_Deficiencies.xls";
        //        }
        //    }
        //    //if user selects permanent colleges Then get collegeIds of permanent colleges
        //    else if (cmd == "PermanentCollege")
        //    {
        //        ReportHeader = "Permanent_Colleges_Deficiencies.xls";
        //        CollegeIds = PermanentCollegesId.ToArray();
        //    }
        //    //if user selects without deficiency colleges Then get collegeIds of without deficiency colleges
        //    else if (cmd == "WithOutDeficiency")
        //    {
        //        ReportHeader = "Colleges_Without_Deficiencies.xls";
        //    }
        //    else if (cmd == "All")
        //    {
        //        ReportHeader = "All_Colleges_Deficiencies.xls";
        //    }
        //    else//if user selects with deficiency colleges Then get collegeIds of with deficiency colleges
        //    {
        //        ReportHeader = "Colleges_With_Deficiencies.xls";
        //    }

        //    //if user all colleges data of either all permanent or with deficiency or without deficiency colleges then get data of that colleges 
        //    if (CollegeId == null && Count == 0)
        //    {
        //        if (cmd == "All")
        //        {
        //            //Get all data entry submitted colleges 
        //            //college_undertaking
        //            int?[] undertakingIDS = db.college_undertaking.Select(u => u.collegeId).ToArray();
        //            int?[] gradeIDS = db.college_grade.Select(u => u.collegeId).ToArray();

        //            CollegeIds = db.jntuh_college.Join(db.jntuh_college_degree, c => c.id, cd => cd.collegeId, (c, cd) => new { c.isActive, c.isNew, c.id, c.collegeName, cdisActive = cd.isActive, cd.degreeId })
        //                //db.jntuh_college
        //                         .Where(c => c.isActive == true && c.isNew != true && //c.cdisActive == true &&
        //                                     SubmitteCollegesId.Contains(c.id) //&& (c.degreeId == 3)//|| c.degreeId == 6) //&& gradeIDS.Contains(c.id)
        //                                     )
        //                         .OrderBy(c => c.collegeName.ToUpper().Trim())
        //                         .Select(c => c.id)
        //                         .ToArray();
        //        }

        //        UGWithDeficiencyList = db.jntuh_college
        //            //.Join(db.jntuh_college_intake_proposed, c => c.id, d => d.collegeId, (c, d) => new { c.isActive, c.isNew, c.id, c.collegeCode, c.collegeName, d.specializationId })
        //            //.Join(db.jntuh_specialization, c => c.specializationId, s => s.id, (c, s) => new { c.isActive, c.isNew, c.id, c.collegeCode, c.collegeName, c.specializationId, s.departmentId })
        //            //.Join(db.jntuh_department, c => c.departmentId, d => d.id, (c, d) => new { c.isActive, c.isNew, c.id, c.collegeCode, c.collegeName, c.specializationId, c.departmentId, d.degreeId })
        //                                 .Where(college => college.isActive == true && college.isNew != true && //college.degreeId == 2 &&
        //                                     CollegeIds.Contains(college.id))
        //                                 .Select(college => new UGWithDeficiency
        //                                 {
        //                                     CollegeId = college.id,
        //                                     CollegeCode = college.collegeCode,
        //                                     CollegeName = college.collegeName.Trim()
        //                                 })
        //                                 .ToList();
        //    }
        //    List<UGWithDeficiency> DeficiencyList = new List<UGWithDeficiency>();

        //    //for with out deficiency report (get labs,library,computers,faculty,principal,ratified faculty  deficiencies and specialization details, college establishment details)
        //    if (cmd == "WithOutDeficiency")
        //    {
        //        foreach (var item in UGWithDeficiencyList)
        //        {
        //            LabShortageCount = 0;
        //            ComputersShortageCount = 0;
        //            PrincipalShortageCount = 0;
        //            FacultyShortageCount = 0;
        //            LibraryShortageCount = 0;
        //            StrLabsShortage = null;
        //            StrPrincipalShortage = null;
        //            StrLibraryShortage = null;
        //            item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
        //            if (PrincipalShortageCount == 0)
        //            {
        //                item.LabsShortage = GetLabsShortage(item.CollegeId);
        //                if (LabShortageCount == 0)
        //                {
        //                    item.LibraryShortage = GetLibraryShortage(item.CollegeId);
        //                    if (LibraryShortageCount == 0)
        //                    {
        //                        item.FacultyShortage = GetUGFacultyShortageList(item.CollegeId);
        //                        if (FacultyShortageCount == 0)
        //                        {
        //                            item.ComputersShortage = GetComputersShortageList(item.CollegeId);
        //                            if (ComputersShortageCount == 0)
        //                            {
        //                                //Binding collegeCode CollegeName and specializationDetails Data to table
        //                                item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
        //                                item.Number = GetFacultyNumber(item.CollegeId);
        //                                item.Percentage = GetFacultyPercentage(item.CollegeId);
        //                                item.CollegeAddress = CollegeAddress(item.CollegeId);
        //                                item.UGObservations = GetUGObservationsList(item.CollegeId);
        //                                item.Establishyear = CollegeEstablishYear(item.CollegeId);
        //                                item.IsPermanentCollege = string.Empty;
        //                                DeficiencyList.Add(item);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //for permanent colleges deficiency report (get labs,library,computers,faculty,principal,ratified faculty  deficiencies and specialization details, college establishment details)
        //    else if (cmd == "PermanentCollege")
        //    {
        //        foreach (var item in UGWithDeficiencyList)
        //        {
        //            LabShortageCount = 0;
        //            ComputersShortageCount = 0;
        //            PrincipalShortageCount = 0;
        //            FacultyShortageCount = 0;
        //            LibraryShortageCount = 0;
        //            StrLabsShortage = null;
        //            StrPrincipalShortage = null;
        //            StrLibraryShortage = null;
        //            item.LabsShortage = GetLabsShortage(item.CollegeId);
        //            item.ComputersShortage = GetComputersShortageList(item.CollegeId);
        //            item.LibraryShortage = GetLibraryShortage(item.CollegeId);
        //            item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
        //            item.FacultyShortage = GetUGFacultyShortageList(item.CollegeId);
        //            item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
        //            item.Number = GetFacultyNumber(item.CollegeId);
        //            item.Percentage = GetFacultyPercentage(item.CollegeId);
        //            item.CollegeAddress = CollegeAddress(item.CollegeId);
        //            item.UGObservations = GetUGObservationsList(item.CollegeId);
        //            item.Establishyear = CollegeEstablishYear(item.CollegeId);
        //            item.IsPermanentCollege = "Permanent College";
        //            DeficiencyList.Add(item);
        //        }
        //    }
        //    else if (cmd == "All")
        //    {
        //        foreach (var item in UGWithDeficiencyList)
        //        {
        //            lastYearAdmitted = 0;
        //            LabShortageCount = 0;
        //            ComputersShortageCount = 0;
        //            PrincipalShortageCount = 0;
        //            FacultyShortageCount = 0;
        //            LibraryShortageCount = 0;
        //            sComputers = 0;
        //            sFaculty = 0;
        //            StrLabsShortage = null;
        //            StrPrincipalShortage = null;
        //            StrLibraryShortage = null;
        //            item.LabsShortage = GetLabsShortage(item.CollegeId);
        //            item.ComputersShortage = GetComputersShortageList(item.CollegeId);
        //            item.LibraryShortage = GetLibraryShortage(item.CollegeId);
        //            item.OverallPoints = GetOverallPoints(item.CollegeId);
        //            item.NBAStatus = GetNBAStatus(item.CollegeId);
        //            item.NAACStatus = GetNAACStatus(item.CollegeId);
        //            item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
        //            item.FacultyShortage = GetUGFacultyShortageList(item.CollegeId);
        //            item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
        //            item.Number = GetFacultyNumber(item.CollegeId);
        //            item.Percentage = GetFacultyPercentage(item.CollegeId);
        //            item.CollegeAddress = CollegeAddress(item.CollegeId);
        //            item.UGObservations = GetUGObservationsList(item.CollegeId);
        //            item.Establishyear = CollegeEstablishYear(item.CollegeId);
        //            //item.Grade = db.college_grade.Where(g => g.collegeId == item.CollegeId && g.inspectionPhaseId == InspectionPhaseId).Select(g => g.grade).FirstOrDefault();

        //            //if (PermanentCollegesId.Contains(item.CollegeId))
        //            //{ item.IsPermanentCollege = "Permanent College"; }
        //            //else
        //            //{ item.IsPermanentCollege = ""; }

        //            item.IsPermanentCollege = "";

        //            if (item.CollegeSpecializations.Count() > 0)
        //            {
        //                //int id = db.college_undertaking.Where(u => u.collegeId == item.CollegeId && u.updatedBy == 450).Select(u => u.id).FirstOrDefault();

        //                //if (id != 0)
        //                //{       

        //                if (lastYearAdmitted == 0)
        //                {
        //                    DeficiencyList.Add(item);
        //                }
        //                //}
        //            }

        //            //DeficiencyList.Add(item);
        //        }
        //    }
        //    else//for with deficiency colleges report (get labs,library,computers,faculty,principal,ratified faculty  deficiencies and specialization details, college establishment details)
        //    {
        //        foreach (var item in UGWithDeficiencyList)
        //        {
        //            LabShortageCount = 0;
        //            ComputersShortageCount = 0;
        //            PrincipalShortageCount = 0;
        //            FacultyShortageCount = 0;
        //            LibraryShortageCount = 0;
        //            StrLabsShortage = null;
        //            StrPrincipalShortage = null;
        //            StrLibraryShortage = null;
        //            item.LabsShortage = GetLabsShortage(item.CollegeId);
        //            item.ComputersShortage = GetComputersShortageList(item.CollegeId);
        //            item.LibraryShortage = GetLibraryShortage(item.CollegeId);
        //            item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
        //            item.FacultyShortage = GetUGFacultyShortageList(item.CollegeId);
        //            if (LabShortageCount == 0 && ComputersShortageCount == 0 && PrincipalShortageCount == 0 && FacultyShortageCount == 0 && LibraryShortageCount == 0)
        //            {

        //            }
        //            else
        //            {
        //                //Binding collegeCode CollegeName and specializationDetails Data to table
        //                item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
        //                item.Number = GetFacultyNumber(item.CollegeId);
        //                item.Percentage = GetFacultyPercentage(item.CollegeId);
        //                item.CollegeAddress = CollegeAddress(item.CollegeId);
        //                item.UGObservations = GetUGObservationsList(item.CollegeId);
        //                item.Establishyear = CollegeEstablishYear(item.CollegeId);
        //                item.IsPermanentCollege = string.Empty;
        //                DeficiencyList.Add(item);
        //            }
        //        }
        //    }

        //    //set order by college name for all colleges
        //    DeficiencyList = DeficiencyList.OrderBy(c => c.CollegeName).ToList();

        //    //assign colleges data to viewbag
        //    ViewBag.CollegeSpecializations = DeficiencyList;

        //    //assign colleges count to viewbag
        //    ViewBag.Count = DeficiencyList.Count();

        //    //return data of all colleges
        //    return DeficiencyList;
        //}

        //[Authorize(Roles = "Admin")]
        //public ActionResult WithDeficiency()
        //{
        //    UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();

        //    //Get all Ids(college,academicYear,Library,Lab)
        //    GetIds();

        //    //Binding all deficiency colleges data to table
        //    List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "WithDeficiency");

        //    //Binding With out Deficiency colleges to Dropdown
        //    UGWithDeficiency.WithDeficiencyColleges = (from d in UGWithDeficiencies
        //                                               join c in db.jntuh_college on d.CollegeId equals c.id
        //                                               where c.isActive == true
        //                                               select new WithDeficiencyColleges
        //                                               {
        //                                                   Id = c.id,
        //                                                   Name = c.collegeCode + " - " + c.collegeName
        //                                               }).ToList();

        //    //showing with deficiency report buttons and hids without deficiency buttons and permanent collegs buttons
        //    ViewBag.WithDeficiency = true;
        //    return View("~/Views/Reports/DeficiencyReport.cshtml", UGWithDeficiency);
        //}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult WithDeficiency(UGWithDeficiency collegeSpecializationsDetails, string cmd)
        {
            List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();

            //assign collegeId 
            int CollegeId = collegeSpecializationsDetails.CollegeId;

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds();

            //showing with deficiency report buttons and hids without deficiency buttons and permanent collegs buttons
            ViewBag.WithDeficiency = true;

            //if user selects collegeId Then get data of that college
            if (CollegeId != 0 && (cmd == "Search" || cmd == "Export"))
            {
                //UGWithDeficiencyList = DeficiencyList(CollegeId, "WithDeficiency");
            }
            else//if user doesn't selects any collegeId Then get all deficiency colleges data 
            {
                //UGWithDeficiencyList = DeficiencyList(null, "WithDeficiency");
            }
            int Count = UGWithDeficiencyList.Count();

            //if user select export to excel then exporting data to excel
            if (cmd == "Export" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_DeficiencyReport.cshtml");
            }
            else//Binding data to table
            {
                return View("~/Views/Reports/DeficiencyReport.cshtml", collegeSpecializationsDetails);
            }
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult WithOutDeficiency()
        //{
        //    UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();

        //    //Get all Ids(college,academicYear,Library,Lab)
        //    GetIds();

        //    //Binding all with out deficiency colleges data to table
        //    List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "WithOutDeficiency");

        //    //Binding With out Deficiency colleges to Dropdown
        //    UGWithDeficiency.WithDeficiencyColleges = (from d in UGWithDeficiencies
        //                                               join c in db.jntuh_college on d.CollegeId equals c.id
        //                                               where c.isActive == true
        //                                               orderby c.collegeName.Trim()
        //                                               select new WithDeficiencyColleges
        //                                               {
        //                                                   Id = c.id,
        //                                                   Name = c.collegeCode + " - " + c.collegeName
        //                                               }).ToList();

        //    //showing with out deficiency report buttons and hids with deficiency buttons and permanent collegs buttons
        //    ViewBag.WithOutDeficiency = true;

        //    return View("~/Views/Reports/DeficiencyReport.cshtml", UGWithDeficiency);
        //}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult WithOutDeficiency(UGWithDeficiency collegeSpecializationsDetails, string cmd)
        {
            List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();

            //assign collegeId
            int CollegeId = collegeSpecializationsDetails.CollegeId;

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds();

            //if user selects collegeId Then get data of that college
            if (CollegeId != 0 && (cmd == "Search" || cmd == "Export"))
            {
                //UGWithDeficiencyList = DeficiencyList(CollegeId, "WithOutDeficiency");
            }
            else//if user doesn't selects any collegeId Then get all deficiency colleges data 
            {
                //UGWithDeficiencyList = DeficiencyList(null, "WithOutDeficiency");
            }

            int Count = UGWithDeficiencyList.Count();

            //if user select export to excel then exporting data to excel
            if ((cmd == "Export") && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_DeficiencyReport.cshtml");
            }
            else//Binding data to table
            {
                ViewBag.WithOutDeficiency = true;
                return View("~/Views/Reports/DeficiencyReport.cshtml", collegeSpecializationsDetails);
            }
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult PermanentCollegesDeficiencyReport()
        //{
        //    UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();

        //    //Get all Ids(college,academicYear,Library,Lab)
        //    GetIds();

        //    //Binding all permanent deficiency colleges data to table
        //    List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "PermanentCollege");

        //    //showing permanent collegs buttons and hids with deficiency buttons and with out deficiency report buttons 
        //    ViewBag.PermanentColleges = true;

        //    //Binding permant colleges data to dropdown
        //    UGWithDeficiency.WithDeficiencyColleges = (from d in UGWithDeficiencies
        //                                               join c in db.jntuh_college on d.CollegeId equals c.id
        //                                               where c.isActive == true
        //                                               orderby c.collegeName.Trim()
        //                                               select new WithDeficiencyColleges
        //                                               {
        //                                                   Id = c.id,
        //                                                   Name = c.collegeCode + " - " + c.collegeName
        //                                               }).ToList();
        //    return View("~/Views/Reports/DeficiencyReport.cshtml", UGWithDeficiency);
        //}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult PermanentCollegesDeficiencyReport(UGWithDeficiency collegeSpecializationsDetails, string cmd)
        {
            List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();

            //assign collegeId
            int CollegeId = collegeSpecializationsDetails.CollegeId;

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds();

            //if user selects collegeId Then get data of that college
            if (CollegeId != 0 && (cmd == "Search" || cmd == "Export"))
            {
                //UGWithDeficiencyList = DeficiencyList(CollegeId, "PermanentCollege");
            }
            else//if user doesn't selects any collegeId Then get all deficiency colleges data 
            {
                //UGWithDeficiencyList = DeficiencyList(null, "PermanentCollege");
            }

            int Count = UGWithDeficiencyList.Count();

            //if user select export to excel then exporting data to excel
            if ((cmd == "Export") && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_DeficiencyReport.cshtml");
            }
            else//Binding data to table
            {
                ViewBag.PermanentColleges = true;
                return View("~/Views/Reports/DeficiencyReport.cshtml", collegeSpecializationsDetails);
            }
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult AllColleges()
        //{
        //    UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();

        //    //Get all Ids(college,academicYear,Library,Lab)
        //    GetIds();

        //    //Binding all permanent deficiency colleges data to table
        //    List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "All");

        //    //showing permanent collegs buttons and hids with deficiency buttons and with out deficiency report buttons 
        //    ViewBag.AllColleges = true;

        //    //Binding permant colleges data to dropdown
        //    UGWithDeficiency.WithDeficiencyColleges = (from d in UGWithDeficiencies
        //                                               join c in db.jntuh_college on d.CollegeId equals c.id
        //                                               where c.isActive == true
        //                                               orderby c.collegeName.Trim()
        //                                               select new WithDeficiencyColleges
        //                                               {
        //                                                   Id = c.id,
        //                                                   Name = c.collegeCode + " - " + c.collegeName
        //                                               }).ToList();
        //    return View("~/Views/Reports/DeficiencyReport.cshtml", UGWithDeficiency);
        //}

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AllColleges(UGWithDeficiency collegeSpecializationsDetails, string cmd)
        {
            List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();

            //assign collegeId
            int CollegeId = collegeSpecializationsDetails.CollegeId;

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds();

            //if user selects collegeId Then get data of that college
            if (CollegeId != 0 && (cmd == "Search" || cmd == "Export"))
            {
                //UGWithDeficiencyList = DeficiencyList(CollegeId, "All");
            }
            else//if user doesn't selects any collegeId Then get all deficiency colleges data 
            {
                //UGWithDeficiencyList = DeficiencyList(null, "All");
            }

            int Count = UGWithDeficiencyList.Count();

            //if user select export to excel then exporting data to excel
            if ((cmd == "Export") && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_DeficiencyReport.cshtml");
            }
            else//Binding data to table
            {
                ViewBag.AllColleges = true;
                return View("~/Views/Reports/DeficiencyReport.cshtml", collegeSpecializationsDetails);
            }
        }

        //Binding Specializations data 
        private List<UGWithDeficiencyCollegeSpecializations> GetUGWithDeficiencyCollegeSpecializations(int CollegeId)
        {
            string adminMBA = string.Empty;
            string NewSpecialization = string.Empty;
            string NewIntake = string.Empty;

            List<UGWithDeficiencyCollegeSpecializations> UGWithDeficiencyCollegeSpecializations = new List<UGWithDeficiencyCollegeSpecializations>();

            List<UGWithDeficiencySpecializations> SpecializationList = (from p in db.jntuh_college_intake_proposed
                                                                        join s in db.jntuh_specialization on p.specializationId equals s.id
                                                                        join d in db.jntuh_department on s.departmentId equals d.id
                                                                        join de in db.jntuh_degree on d.degreeId equals de.id
                                                                        join dt in db.jntuh_degree_type on de.degreeTypeId equals dt.id
                                                                        where (p.collegeId == CollegeId //&& (d.degreeId == 3)// || d.degreeId == 6)
                                                                        &&
                                                                               p.academicYearId == nextAcademicYearId &&
                                                                            // p.courseAffiliationStatusCodeId != ClosureCourseId &&
                                                                               p.isActive == true)
                                                                        //orderby p.specializationId
                                                                        orderby de.degreeDisplayOrder
                                                                        select new UGWithDeficiencySpecializations
                                                                        {
                                                                            CollegeId = CollegeId,
                                                                            SpecializationId = p.specializationId,
                                                                            ShiftId = p.shiftId,
                                                                            ProposedIntake = p.proposedIntake,
                                                                            CourseAffiliationStatusCodeId = p.courseAffiliationStatusCodeId,
                                                                            DegreeType = dt.degreeType,
                                                                            DegreeName = de.degree,
                                                                            SpecializationName = s.specializationName,
                                                                            DegreeId = de.id
                                                                        }).ToList();
            foreach (var item in SpecializationList)
            {
                UGWithDeficiencyCollegeSpecializations UGWithDeficiencySpecializations = new UGWithDeficiencyCollegeSpecializations();
                if (item.DegreeType != null)
                {
                    NewIntake = string.Empty;
                    NewSpecialization = string.Empty;
                    if (item.DegreeType.ToUpper().Trim() == "PG")
                    {
                        //if degree is MBA then show degreeName as mba and add all mba specializations proposed intake 
                        if (item.DegreeName.ToUpper().Trim() == "MBA" && adminMBA != "MBA")
                        {
                            adminMBA = "MBA";
                            NewSpecialization = item.DegreeName;
                            //display proposed intake bold based on existing intake
                            NewIntake = GetMBAIntake(item.CollegeId, item.DegreeId);
                        }
                        //if degree is mtech or mpharmacy then display specialization name as degreename-specializationname-shiftname
                        if (item.DegreeName.ToUpper().Trim() == "M.TECH" || item.DegreeName.ToUpper().Trim() == "M.PHARMACY")
                        {
                            //display proposed intake bold based on CourseAffiliationStatusCodeId
                            NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            //if specialization consists of shift id then dispaly shift anme
                            NewSpecialization = item.ShiftId == 1 ? item.DegreeName + "-" + item.SpecializationName : item.DegreeName + "-" + item.SpecializationName + "-" + "(II Shift)";
                        }
                        //if degree is not equal to mtech or mpharmacy then display specialization name as specializationname-shiftname
                        if (item.DegreeName.ToUpper().Trim() != "M.TECH" && item.DegreeName.ToUpper().Trim() != "M.PHARMACY" && item.DegreeName.ToUpper().Trim() != "MBA")
                        {
                            NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? item.SpecializationName : item.SpecializationName + "-" + "(II Shift)";
                        }
                    }
                    else//for Ug degrees
                    {
                        NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                        NewSpecialization = item.ShiftId == 1 ? item.SpecializationName : item.SpecializationName + "-" + "(II Shift)";
                    }
                    //if intake and specialization is exists then add to list
                    if (NewIntake != string.Empty && NewSpecialization != string.Empty)
                    {
                        UGWithDeficiencySpecializations.Specialization = NewSpecialization;
                        UGWithDeficiencySpecializations.Intake = NewIntake;
                        int existingIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == CollegeId && ex.specializationId == item.SpecializationId && ex.shiftId == item.ShiftId).Select(ex => ex.approvedIntake).FirstOrDefault();
                        UGWithDeficiencySpecializations.ExistingIntake = existingIntake;
                        int AY13admittedIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == CollegeId && ex.specializationId == item.SpecializationId && ex.shiftId == item.ShiftId).Select(ex => ex.admittedIntake).FirstOrDefault();
                        lastYearAdmitted = AY13admittedIntake;
                        UGWithDeficiencySpecializations.AY13admittedIntake = AY13admittedIntake;
                        UGWithDeficiencyCollegeSpecializations.Add(UGWithDeficiencySpecializations);
                    }
                }
            }
            return UGWithDeficiencyCollegeSpecializations;
        }

        //Binding MBA degree intake
        private string GetMBAIntake(int collegeId, int degreeId)
        {
            int existingIntake = 0;
            int proposedIntake = 0;
            string intake = string.Empty;
            //get all specializations for corresponding degree
            int[] specializationId = (from department in db.jntuh_department
                                      join specialization in db.jntuh_specialization on department.id equals specialization.departmentId
                                      where department.degreeId == degreeId
                                      select specialization.id).ToArray();
            foreach (var item in specializationId)
            {
                //get shifts
                int[] shift = db.jntuh_shift.Where(s => s.isActive == true).Select(s => s.id).ToArray();
                foreach (var shiftId in shift)
                {
                    //get proposed intake id 
                    int id = db.jntuh_college_intake_proposed.Where(ei => ei.collegeId == collegeId &&
                                                                          ei.specializationId == item &&
                                                                          ei.academicYearId == nextAcademicYearId &&
                                                                          ei.shiftId == shiftId &&
                                                                          ei.isActive == true
                        //&& ei.courseAffiliationStatusCodeId != ClosureCourseId
                                                                          )
                                                             .Select(ei => ei.id)
                                                             .FirstOrDefault();
                    //if proposed intake id is exists then get existing intake 
                    if (id > 0)
                    {
                        //add proposed intake for all specializations under mba degree
                        proposedIntake += db.jntuh_college_intake_proposed
                                            .Where(ei => ei.id == id)
                                            .Select(ei => ei.proposedIntake).FirstOrDefault();
                        //add existing intake for all specializations under mba degree
                        existingIntake += db.jntuh_college_intake_existing
                                            .Where(ei => ei.collegeId == collegeId &&
                                                         ei.specializationId == item &&
                                                         ei.academicYearId == academicYearId &&
                                                         ei.shiftId == shiftId)
                                           .Select(ei => ei.approvedIntake)
                                           .FirstOrDefault();
                    }

                }
            }
            if (proposedIntake != 0)
            {
                //if exising and proposed intake is same then dispaly this specialization and intake normal based on $ symbol inside view
                if (existingIntake == proposedIntake)
                {
                    intake = proposedIntake.ToString() + "$";
                }
                //if exising is zero then dispaly this specialization and intake bold letters based on # symbol inside view
                else if (existingIntake == 0)
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else//if proposed intake is greater or less than of existing intake then dispaly this specialization normal and intake bold letters based on * symbol inside view
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
        private string GetIntake(int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake)
        {
            string intake = string.Empty;
            string courseAffiliationStatusCode = string.Empty;
            courseAffiliationStatusCode = (db.jntuh_course_affiliation_status.Where(status => status.id == courseAffiliationStatusCodeId).Select(status => status.courseAffiliationStatusCode).FirstOrDefault()).ToUpper().Trim();
            if (proposedIntake != 0)
            {
                //if courseAffiliationStatusCode is 'S' then dispaly this specialization and intake normal based on $ symbol inside view
                if (courseAffiliationStatusCode == "S")
                {
                    intake = proposedIntake.ToString() + "$";
                }
                //if courseAffiliationStatusCode is 'N' then dispaly this specialization and intake bold based on # symbol inside view
                else if (courseAffiliationStatusCode == "N")
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else//dispaly this specialization normal and intake bold letters based on * symbol inside view
                {
                    intake = proposedIntake.ToString() + "*";

                    //int existingIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == collegeId && ex.specializationId == specializationId && ex.shiftId == shiftId).Select(ex => ex.approvedIntake).FirstOrDefault();

                    ////if [existing intake > proposed intake] then show [existing intake - decrease in proposed intake]
                    //if (existingIntake > proposedIntake)
                    //{
                    //    intake = existingIntake.ToString() + "-<span style='font-weight: bold'>" + (existingIntake - proposedIntake).ToString() + "</span>" + "*";
                    //}

                    ////if [existing intake < proposed intake] then show [existing intake + increase in proposed intake]
                    //if (existingIntake < proposedIntake)
                    //{
                    //    intake = existingIntake.ToString() + "+<span style='font-weight: bold'>" + (proposedIntake - existingIntake).ToString() + "</span>" + "*";
                    //}

                }
            }
            else
            {
                intake = proposedIntake.ToString() + "*";
            }
            return intake;
        }

        //Get labs shortage
        private string GetLabsShortage(int collegeId)
        {
            int ObservationId = 0;
            string collegeLabsShortage = null;
            //get ObservationId of corresponding collegeId
            ObservationId = db.jntuh_college_committee_observations
                                    .Where(c => c.collegeId == collegeId &&
                                                c.observationTypeId == LabcommitteobservationId)
                                    .Select(c => c.id)
                                    .FirstOrDefault();
            //if ObservationId is not equal to zero then display 
            if (ObservationId != 0)
            {
                collegeLabsShortage = db.jntuh_college_committee_observations.Where(o => o.id == ObservationId && !StrNill.Contains(o.observations.ToUpper().Trim()))
                    .Select(o => o.observations).FirstOrDefault();
                //if observations is match with any string in the StrNill array then display it as NIL                      
                if (collegeLabsShortage == null)
                {
                    collegeLabsShortage = "NIL";
                }
                else//if observations is does not match with any string in the StrNill array then display it * and write remarks as observations     
                {
                    LabShortageCount++;
                    StrLabsShortage = collegeLabsShortage;
                    collegeLabsShortage = "*";
                }
            }
            else//if ObservationId is not equal to zero then display NIL
            {
                collegeLabsShortage = "NIL";
            }
            return collegeLabsShortage;
        }

        private int GetDegreeWiseIntake(int degreeId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            //int[] specializations = specializationsId.Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId && e.academicYearId == AcademicYearId1).Select(e => e.shiftId).ToArray();
                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                }
                if (duration >= 5)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                }
                if (duration == 4)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                }
                if (duration == 3)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                }
                if (duration == 2)
                {
                    totalIntake += totalIntake1 + totalIntake2;
                }
                if (duration == 1)
                {
                    totalIntake += totalIntake1;
                }
            }

            return totalIntake;
        }

        private List<string> GetComputersShortageList(int collegeId)
        {
            List<string> UGCompuetrsShortageList = new List<string>();
            string Existingratio = string.Empty;
            int ActualRatioValue = 0;
            decimal TotalIntake = 0;
            decimal CalculatedRatio = 0;
            decimal Deficiency = 0;
            decimal TotalComputers = 0;
            //get degreeIds based on collegeId
            int[] AllDegreeIds = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();
            //get degreeId doesnot contains mba,mca,mam,mtm,pharm d,pharmdpb
            int[] DegreeIds = (from d in AllDegreeIds
                               where !MBAAndMCAID.Contains(d) && !EmptyDegreeRatioId.Contains(d)
                               select d).ToArray();
            //get mba,mca, degreeids
            int[] MBAAndMCAIds = (from d in AllDegreeIds
                                  where MBAAndMCAID.Contains(d)
                                  select d).ToArray();
            //get mam,mtm,pharm d,pharmdpb degreeIds
            //int[] EmptyRatioDegreeIds = (from d in AllDegreeIds
            //                             where EmptyDegreeRatioId.Contains(d)
            //                             select d).ToArray();
            string DegreeName = string.Empty;
            if (DegreeIds.Count() > 0)
            {
                foreach (var item in DegreeIds)
                {
                    DegreeName = string.Empty;
                    TotalIntake = 0;
                    CalculatedRatio = 0;
                    TotalComputers = 0;
                    Existingratio = (db.jntuh_computer_student_ratio_norms
                                         .Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms)
                                         .FirstOrDefault()).Split(':')[1];
                    ActualRatioValue = Convert.ToInt32(Existingratio);
                    //TotalIntake = GetIntakeForComputers(item, collegeId);
                    TotalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
                    //Commented on 15-Sep-2014 : Ramesh
                    //TotalIntake = GetDegreeWiseIntake(item, collegeId);
                    TotalComputers = db.jntuh_college_computer_student_ratio
                                       .Where(c => c.collegeId == collegeId &&
                                                   c.degreeId == item)
                                       .Select(c => c.availableComputers)
                                       .FirstOrDefault();
                    if (TotalIntake != 0 && TotalComputers != 0)
                    {
                        CalculatedRatio = TotalIntake / TotalComputers;
                        if (ActualRatioValue != 0 && CalculatedRatio != 0)
                        {
                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                    .Select(d => d.degree)
                                                    .FirstOrDefault();

                            if (ActualRatioValue < decimal.Round(CalculatedRatio))
                            {
                                //With out Ratio
                                ComputersShortageCount++;
                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));

                                ////With Ratio
                                //Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                                //StrComputersShortageDegrees += DegreeName + ", ";
                                //ComputersShortageCount++;
                                //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                                //if (Deficiency > 0)
                                //    sComputers += decimal.Round(Deficiency);
                            }
                            else
                            {
                                //With out Ratio
                                DegreeName = DegreeName + "- NIL";

                                ////With Ratio
                                //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }

                    //if (TotalIntake != 0 && TotalComputers == 0)
                    if (TotalComputers == 0 || (TotalIntake == 0 && TotalComputers != 0))
                    {
                        //With out Ratio
                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                   .Select(d => d.degree).FirstOrDefault();
                        ComputersShortageCount++;
                        DegreeName = DegreeName + "- No Computers";

                        ////With Ratio
                        //DegreeName = db.jntuh_degree.Where(d => d.id == item)
                        //                           .Select(d => d.degree).FirstOrDefault();
                        //Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                        //ComputersShortageCount++;
                        //StrComputersShortageDegrees += DegreeName + ", ";
                        ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                        //DegreeName = DegreeName + "- No Computers" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                        //if (Deficiency > 0)
                        //    sComputers += decimal.Round(Deficiency);
                    }

                    UGCompuetrsShortageList.Add(DegreeName);
                }
            }

            if (MBAAndMCAIds.Count() > 0)
            {
                DegreeName = string.Empty;
                TotalIntake = 0;
                CalculatedRatio = 0;
                TotalComputers = 0;
                ActualRatioValue = 0;
                foreach (var item in MBAAndMCAIds)
                {
                    Existingratio = (db.jntuh_computer_student_ratio_norms
                                         .Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms)
                                         .FirstOrDefault()).Split(':')[1];
                    ActualRatioValue += Convert.ToInt32(Existingratio);
                    //TotalIntake += GetIntakeForComputers(item, collegeId);
                    TotalIntake += db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
                    //Commented on 15-Sep-2014 : Ramesh
                    //TotalIntake += GetDegreeWiseIntake(item, collegeId);
                    TotalComputers += db.jntuh_college_computer_student_ratio
                                       .Where(c => c.collegeId == collegeId &&
                                                   c.degreeId == item)
                                       .Select(c => c.availableComputers)
                                       .FirstOrDefault();
                }

                if (TotalIntake != 0 && TotalComputers != 0)
                {
                    CalculatedRatio = TotalIntake / TotalComputers;
                    if (ActualRatioValue != 0 && CalculatedRatio != 0)
                    {
                        DegreeName = "MBA/MCA";
                        if (ActualRatioValue < decimal.Round(CalculatedRatio))
                        {
                            //With out Ratio
                            ComputersShortageCount++;
                            DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));

                            ////With  Ratio
                            //ComputersShortageCount++;
                            //Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                            //StrComputersShortageDegrees += DegreeName + ", ";
                            //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                            //if (Deficiency > 0)
                            //    sComputers += decimal.Round(Deficiency);
                        }
                        else
                        {
                            //With out Ratio
                            DegreeName = DegreeName + "- NIL";

                            ////With Ratio
                            //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));
                        }
                    }
                }

                //if (TotalIntake != 0 && TotalComputers == 0)
                if (TotalComputers == 0 || (TotalIntake == 0 && TotalComputers != 0))
                {
                    DegreeName = "MBA/MCA";
                    //With out Ratio
                    ComputersShortageCount++;
                    DegreeName = DegreeName + "- No Computers";

                    ////With Ratio
                    //Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                    //ComputersShortageCount++;
                    //StrComputersShortageDegrees += DegreeName + ", ";
                    ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                    //DegreeName = DegreeName + "- No Computers" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                    //if (Deficiency > 0)
                    //    sComputers += decimal.Round(Deficiency);
                }

                UGCompuetrsShortageList.Add(DegreeName);
            }

            return UGCompuetrsShortageList;
        }

        private int GetIntakeForComputers(int degreeId, int collegeId)
        {
            int totalIntake1 = 0;
            int totalIntake2 = 0;
            int totalIntake3 = 0;
            int totalIntake4 = 0;
            int totalIntake5 = 0;
            int totalIntake = 0;
            int[] shiftIds;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId && ProposedIntakeExisting.isActive == true)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                totalIntake1 = 0;
                totalIntake2 = 0;
                totalIntake3 = 0;
                totalIntake4 = 0;
                totalIntake5 = 0;
                shiftIds = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId && e.academicYearId == AcademicYearId1 && e.isActive == true).Select(e => e.shiftId).ToArray();
                foreach (var shift in shiftIds)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shift && e.isActive == true).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == academicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shift).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shift).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shift).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shift).Select(a => a.approvedIntake).FirstOrDefault();
                }
                if (duration >= 5)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                }
                if (duration == 4)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                }
                if (duration == 3)
                {
                    totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                }
                if (duration == 2)
                {
                    totalIntake += totalIntake1 + totalIntake2;
                }
                if (duration == 1)
                {
                    totalIntake += totalIntake1;
                }
            }
            return totalIntake;
        }

        private string GetPrincipalRatified(int collegeId)
        {
            string PrincipalGrade = string.Empty;
            var PrincipalDetails = db.jntuh_college_principal_director
                                     .Where(p => p.collegeId == collegeId && p.type == "PRINCIPAL")
                                     .Select(p => new
                                     {
                                         Id = p.id,
                                         Ratified = p.isRatified,
                                     })
                                     .FirstOrDefault();
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

        private List<string> GetUGFacultyShortageList(int CollegeId)
        {
            string DegreeName = string.Empty;
            decimal Caluculatedvalue = 0;
            decimal FacultyIntake = 0;
            decimal TotalFaculty = 0;
            int ActualRatioValue = 0;
            decimal Deficiency;
            List<string> UGFacultyShortageList = new List<string>();
            int[] AllDegreeIds = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where (cd.isActive == true && cd.collegeId == CollegeId && d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();
            //get degreeId doesnot contains mba,mca,mam,mtm,pharm d,pharmdpb
            int[] DegreeIds = (from d in AllDegreeIds
                               where !MBAAndMCAID.Contains(d) && !EmptyDegreeRatioId.Contains(d)
                               select d).ToArray();
            //get mba,mca, degreeids
            int[] MBAAndMCAIds = (from d in AllDegreeIds
                                  where MBAAndMCAID.Contains(d)
                                  select d).ToArray();
            //get mam,mtm,pharm d,pharmdpb degreeIds
            //int[] EmptyRatioDegreeIds = (from d in AllDegreeIds
            //                             where EmptyDegreeRatioId.Contains(d)
            //                             select d).ToArray();

            if (DegreeIds.Count() > 0)
            {
                foreach (var item in DegreeIds)
                {
                    DegreeName = string.Empty;
                    Caluculatedvalue = 0;
                    ActualRatioValue = Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
                                   .Where(f => f.isActive == true && f.degreeId == item)
                                   .Select(f => f.Norms)
                                  .FirstOrDefault()).Split(':')[1]);


                    FacultyIntake = db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalIntake)
                                           .FirstOrDefault();

                    //Commented on 15-Sep-2014 : Ramesh
                    //FacultyIntake = GetDegreeWiseIntake(item, CollegeId);

                    TotalFaculty = db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalFaculty)
                                           .FirstOrDefault();
                    if (FacultyIntake != 0 && TotalFaculty != 0)
                    {
                        Caluculatedvalue = FacultyIntake / TotalFaculty;
                        if (ActualRatioValue != 0 && Caluculatedvalue != 0)
                        {
                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                        .Select(d => d.degree).FirstOrDefault();
                            if (ActualRatioValue < decimal.Round(Caluculatedvalue))
                            {
                                //with out Ratio
                                FacultyShortageCount++;
                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));

                                ////With Ratio
                                //Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                                //FacultyShortageCount++;
                                //StrFacultyShortageDegrees += DegreeName + ", ";
                                ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                                //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                                //if (Deficiency > 0)
                                //    sFaculty += decimal.Round(Deficiency);
                            }
                            else
                            {
                                //With out Ratio
                                DegreeName = DegreeName + "- NIL";

                                ////With Ratio
                                //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }

                    //if (FacultyIntake != 0 && TotalFaculty == 0)
                    if (TotalFaculty == 0)
                    {
                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                    .Select(d => d.degree).FirstOrDefault();

                        //With out Ratio
                        FacultyShortageCount++;
                        DegreeName = DegreeName + "- No Faculty";

                        ////With  Ratio
                        //Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                        //FacultyShortageCount++;
                        //StrFacultyShortageDegrees += DegreeName + ", ";
                        ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                        //DegreeName = DegreeName + "- No Faculty" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                        //if (Deficiency > 0)
                        //    sFaculty += decimal.Round(Deficiency);
                    }

                    UGFacultyShortageList.Add(DegreeName);
                }
            }

            if (MBAAndMCAIds.Count() > 0)
            {
                DegreeName = string.Empty;
                FacultyIntake = 0;
                TotalFaculty = 0;
                Caluculatedvalue = 0;
                ActualRatioValue = 0;
                foreach (var item in MBAAndMCAIds)
                {
                    ActualRatioValue += Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
                                   .Where(f => f.isActive == true && f.degreeId == item)
                                   .Select(f => f.Norms)
                                  .FirstOrDefault()).Split(':')[1]);
                    FacultyIntake += db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalIntake)
                                           .FirstOrDefault();

                    //Commented on 10-Sep-2014 : Ramesh
                    //FacultyIntake += GetDegreeWiseIntake(item, CollegeId);

                    TotalFaculty += db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalFaculty)
                                           .FirstOrDefault();
                }
                if (FacultyIntake != 0 && TotalFaculty != 0)
                {
                    Caluculatedvalue = FacultyIntake / TotalFaculty;
                    if (ActualRatioValue != 0 && Caluculatedvalue != 0)
                    {
                        DegreeName = "MBA/MCA";
                        if (ActualRatioValue < decimal.Round(Caluculatedvalue))
                        {
                            //With out Ratio
                            FacultyShortageCount++;
                            DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));

                            ////With Ratio
                            //FacultyShortageCount++;
                            //StrFacultyShortageDegrees += DegreeName + ", ";
                            //Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                            ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                            //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                            //if (Deficiency > 0)
                            //    sFaculty += decimal.Round(Deficiency);
                        }
                        else
                        {
                            //With out Ratio
                            DegreeName = DegreeName + "-NIL";

                            ////With Ratio
                            //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
                        }
                    }
                }

                //if (FacultyIntake != 0 && TotalFaculty == 0)
                if (TotalFaculty == 0)
                {
                    DegreeName = "MBA/MCA";

                    //With out Ratio
                    FacultyShortageCount++;
                    DegreeName = DegreeName + "- No Faculty";

                    ////With Ratio
                    //Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                    //FacultyShortageCount++;
                    //StrFacultyShortageDegrees += DegreeName + ", ";
                    ////Modified Date:01/07/2014, Repalace * inplace of Deficiency
                    //DegreeName = DegreeName + "- No Faculty" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                    //if (Deficiency > 0)
                    //    sFaculty += decimal.Round(Deficiency);

                }

                UGFacultyShortageList.Add(DegreeName);
            }

            return UGFacultyShortageList;
        }

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
            int ObservationId = 0;
            string collegeLibraryShortage = null;
            ObservationId = db.jntuh_college_committee_observations
                                   .Where(c => c.collegeId == collegeId &&
                                               c.observationTypeId == LibrarycommitteobservationId)
                                   .Select(c => c.id)
                                   .FirstOrDefault();
            if (ObservationId != 0)
            {
                collegeLibraryShortage = db.jntuh_college_committee_observations
                                           .Where(o => o.id == ObservationId &&
                                                       !StrNill.Contains(o.observations.ToUpper().Trim()))
                                           .Select(o => o.observations).FirstOrDefault();
                if (collegeLibraryShortage == null)
                {
                    collegeLibraryShortage = "NIL";
                }
                else
                {
                    LibraryShortageCount++;
                    StrLibraryShortage = collegeLibraryShortage;
                    collegeLibraryShortage = "*";
                }
            }
            else
            {
                collegeLibraryShortage = "NIL";
            }
            return collegeLibraryShortage;
        }

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

        private string GetNBAStatus(int collegeId)
        {
            string NBAStatus = "";
            if (db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 8) != null)
            {
                var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 8).Select(a => a).FirstOrDefault();
                if (Status != null)
                {
                    NBAStatus = Status.affiliationStatus;

                    //    NBAStatus = "YES";

                }
                else
                {
                    NBAStatus = "Not Yet Applied";
                }
            }
            return NBAStatus;
        }

        private string GetNAACStatus(int collegeId)
        {
            string NAACStatus = "";
            var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 2).Select(a => a).FirstOrDefault();
            if (Status != null)
            {
                NAACStatus = Status.affiliationStatus;

                //NAACStatus = "YES";
            }
            else
            {
                NAACStatus = "Not Yet Applied";
            }
            return NAACStatus;
        }

        private List<string> GetUGObservationsList(int CollegeId)
        {
            List<string> UGObservations = new List<string>();
            if (LabShortageCount > 0 && StrLabsShortage != null)
            {
                UGObservations.Add(StrLabsShortage);
            }
            if (ComputersShortageCount > 0)
            {
                UGObservations.Add("Shortage of computers");

                //if (sComputers > 0)
                //UGObservations.Add("Shortage of computers (" + sComputers + ")");
            }
            if (PrincipalShortageCount > 0 && StrPrincipalShortage != null)
            {
                UGObservations.Add(StrPrincipalShortage);
            }
            if (FacultyShortageCount > 0)
            {
                //if (sFaculty > 0)
                UGObservations.Add("Shortage of faculty");
            }
            if (LibraryShortageCount > 0 && StrLibraryShortage != null)
            {
                UGObservations.Add(StrLibraryShortage);
            }
            return UGObservations;
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
                Address += CollegeAddress.townOrCity + ", ";
            }
            if (CollegeAddress.mandal != null || CollegeAddress.mandal != string.Empty)
            {
                Address += CollegeAddress.mandal + ", ";
            }
            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district
                             .Where(d => d.id == CollegeAddress.districtId)
                             .Select(d => d.districtName)
                             .FirstOrDefault();
                Address += District + ", ";
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

    }
}