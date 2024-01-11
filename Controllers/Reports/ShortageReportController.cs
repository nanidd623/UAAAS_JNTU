using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class ShortageReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int ComputersShortageCount = 0;
        private int FacultyShortageCount = 0;
        private string[] EmptyDegrees = { "MAM", "MTM", "Pharm.D", "Pharm.D PB" };
        private string[] MBAAndMCAIDDegrees = { "MCA", "MBA" };
        private int[] EmptyDegreeRatioId;
        int[] dataentryVerifiedCollegeIds;

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



        public ActionResult Index()
        {
            return View();
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult ShortageReport(string exportType)
        //{
        //    int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

        //    ShortageReport shortageReport = new Models.ShortageReport();
        //    dataentryVerifiedCollegeIds = db.jntuh_dataentry_allotment.Where(da => da.isCompleted == true && da.isActive == true && da.InspectionPhaseId==InspectionPhaseId).Select(da => da.collegeID).ToArray();
        //    string fileName = string.Empty;
        //    if (exportType == "Shortage of Labs")
        //    {
        //        int labsShortageId = db.jntuh_committee_observations.Where(co => co.isActive == true && co.observationType == "Labs").Select(co => co.id).FirstOrDefault();
        //        var shortageCollegeList = (from co in db.jntuh_college_committee_observations
        //                                   join c in db.jntuh_college on co.collegeId equals c.id
        //                                   join a in db.jntuh_address on c.id equals a.collegeId
        //                                   join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
        //                                   where (c.isActive == true && da.isCompleted == true && da.isActive == true && a.addressTye == "College" && co.observationTypeId == labsShortageId && !StrNill.Contains(co.observations) && da.InspectionPhaseId == InspectionPhaseId)
        //                                   select new ShortageReport
        //                                   {
        //                                       collegeId = c.id,
        //                                       collegeCode = c.collegeCode,
        //                                       collegeName = c.collegeName,
        //                                       observations = co.observations,
        //                                       address=a.address,
        //                                       mobile=a.mobile
        //                                   }).OrderBy(cl => cl.collegeName).ToList();
        //        ViewBag.shortageCollegeList = shortageCollegeList;
        //        fileName = exportType + ".xls";
        //    }
        //    if (exportType == "Shortage of Books")
        //    {
        //        int labsShortageId = db.jntuh_committee_observations.Where(co => co.isActive == true && co.observationType == "Library").Select(co => co.id).FirstOrDefault();
        //        var shortageCollegeList = (from co in db.jntuh_college_committee_observations
        //                                   join c in db.jntuh_college on co.collegeId equals c.id
        //                                   join a in db.jntuh_address on c.id equals a.collegeId
        //                                   join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
        //                                   where (c.isActive == true && da.isCompleted == true && da.isActive == true && a.addressTye == "College" && co.observationTypeId == labsShortageId && !StrNill.Contains(co.observations) && da.InspectionPhaseId == InspectionPhaseId)
        //                                   select new ShortageReport
        //                                   {
        //                                       collegeId = c.id,
        //                                       collegeCode = c.collegeCode,
        //                                       collegeName = c.collegeName,
        //                                       observations = co.observations,
        //                                       address = a.address,
        //                                       mobile = a.mobile
        //                                   }).OrderBy(cl => cl.collegeName).ToList();
        //        ViewBag.shortageCollegeList = shortageCollegeList;
        //        fileName = exportType + ".xls"; ;
        //    }
        //    if (exportType == "Principal is not ratified")
        //    {             

        //        var shortageCollegeList = (from pd in db.jntuh_college_principal_director
        //                                   join c in db.jntuh_college on pd.collegeId equals c.id
        //                                   join a in db.jntuh_address on c.id equals a.collegeId
        //                                   join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
        //                                   where (c.isActive == true && da.isCompleted == true && da.isActive == true && a.addressTye == "College" && pd.type == "PRINCIPAL" && pd.isRatified == false && da.InspectionPhaseId == InspectionPhaseId)
        //                                   select new ShortageReport
        //                                   {
        //                                       collegeId = c.id,
        //                                       collegeCode = c.collegeCode,
        //                                       collegeName = c.collegeName,
        //                                       observations = "Principal is not ratified",
        //                                       address = a.address,
        //                                       mobile = a.mobile
        //                                   }).OrderBy(cl => cl.collegeName).ToList();
        //        ViewBag.shortageCollegeList = shortageCollegeList;
        //        fileName = exportType + ".xls"; ;
        //    }
        //    if (exportType == "No Principal")
        //    {
        //        int[] collegeIds = db.jntuh_college_principal_director.Where(p => p.type == "PRINCIPAL").Select(p => p.collegeId).ToArray();
        //        var shortageCollegeList = (from c in db.jntuh_college
        //                                   join a in db.jntuh_address on c.id equals a.collegeId
        //                                   join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
        //                                   where (c.isActive == true && da.isCompleted == true && da.isActive == true && a.addressTye == "College" && !collegeIds.Contains(c.id) && da.InspectionPhaseId == InspectionPhaseId)
        //                                   select new ShortageReport
        //                                   {
        //                                       collegeId = c.id,
        //                                       collegeCode = c.collegeCode,
        //                                       collegeName = c.collegeName,
        //                                       observations = "No Principal",
        //                                       address = a.address,
        //                                       mobile = a.mobile
        //                                   }).OrderBy(cl => cl.collegeName).ToList();
        //        ViewBag.shortageCollegeList = shortageCollegeList;
        //        fileName = exportType + ".xls"; ;
        //    }
        //    if (exportType == "Shortage of Computers")
        //    {
                
        //        string Existingratio = string.Empty;
        //        int ActualRatioValue = 0;
        //        decimal TotalIntake = 0;
        //        decimal CalculatedRatio = 0;
        //        // decimal Deficiency = 0;
        //        decimal TotalComputers = 0;
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<ComputerShortageReport> ComputerShortageReportList = new List<ComputerShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            ComputerShortageReport computerShortageReport = new ComputerShortageReport();
        //            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
        //            EmptyDegreeRatioId = db.jntuh_degree
        //                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
        //                                 .Select(d => d.id).ToArray();

        //            //get degreeIds based on collegeId
        //            int[] DegreeIds = (from cd in db.jntuh_college_degree
        //                               join d in db.jntuh_degree on cd.degreeId equals d.id
        //                               where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true && !EmptyDegreeRatioId.Contains(d.id))
        //                               orderby d.degreeDisplayOrder
        //                               select cd.degreeId).ToArray();
        //            string DegreeName = string.Empty;
        //            if (DegreeIds.Count() > 0)
        //            {
        //                foreach (var item in DegreeIds)
        //                {
        //                    DegreeName = string.Empty;
        //                    TotalIntake = 0;
        //                    CalculatedRatio = 0;
        //                    TotalComputers = 0;
        //                    Existingratio = (db.jntuh_computer_student_ratio_norms
        //                                         .Where(c => c.isActive == true && c.degreeId == item)
        //                                         .Select(c => c.Norms)
        //                                         .FirstOrDefault()).Split(':')[1];
        //                    ActualRatioValue = Convert.ToInt32(Existingratio);
        //                    TotalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
        //                    TotalComputers = db.jntuh_college_computer_student_ratio
        //                                       .Where(c => c.collegeId == collegeId &&
        //                                                   c.degreeId == item)
        //                                       .Select(c => c.availableComputers)
        //                                       .FirstOrDefault();
        //                    if (TotalIntake != 0 && TotalComputers != 0)
        //                    {
        //                        CalculatedRatio = TotalIntake / TotalComputers;
        //                        if (ActualRatioValue != 0 && CalculatedRatio != 0)
        //                        {
        //                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree)
        //                                                    .FirstOrDefault();
        //                            if (ActualRatioValue < decimal.Round(CalculatedRatio))
        //                            {
        //                                ComputersShortageCount++;
        //                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));
                                       
        //                            }
        //                            else
        //                            {
        //                                //DegreeName = DegreeName + "- NIL";
        //                                DegreeName = "NIL";
        //                            }
        //                        }
        //                    }
        //                    if (TotalIntake != 0 && TotalComputers == 0)
        //                    {
        //                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree).FirstOrDefault();
        //                        ComputersShortageCount++;
        //                        //DegreeName = DegreeName + "- No Computers";
        //                        DegreeName ="No Computers";
        //                    }


        //                    var collegeAddressDetails = (from c in db.jntuh_college
        //                                                 join a in db.jntuh_address on c.id equals a.collegeId
        //                                                 where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                                 select new
        //                                                 {
        //                                                     c.collegeCode,
        //                                                     c.collegeName,
        //                                                     a.address,
        //                                                     a.mobile
        //                                                 }).FirstOrDefault();

        //                    computerShortageReport.collegeId = collegeId;
        //                    computerShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //                    computerShortageReport.collegeName = collegeAddressDetails.collegeName;
        //                    computerShortageReport.address = collegeAddressDetails.address;
        //                    computerShortageReport.mobile = collegeAddressDetails.mobile;
        //                    computerShortageReport.DegreeName = DegreeName;
        //                    computerShortageReport.ComputersShortageCount = ComputersShortageCount;
        //                    computerShortageReport.observations = "Shortage of Computers";
        //                    ComputerShortageReportList.Add(computerShortageReport);
        //                }
        //            }  

        //        }
        //         //&& cs.DegreeName != "No Computers"
        //        ComputerShortageReportList = ComputerShortageReportList.Where(cs => cs.DegreeName != "NIL").Distinct().OrderBy(cs=>cs.collegeName).ToList();
        //        ViewBag.shortageCollegeList = ComputerShortageReportList;
        //        fileName = exportType + ".xls"; 
        //    }
        //    if (exportType == "No Computers")
        //    {
        //        string Existingratio = string.Empty;
        //        int ActualRatioValue = 0;
        //        decimal TotalIntake = 0;
        //        decimal CalculatedRatio = 0;
        //        // decimal Deficiency = 0;
        //        decimal TotalComputers = 0;
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<ComputerShortageReport> ComputerShortageReportList = new List<ComputerShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            ComputerShortageReport computerShortageReport = new ComputerShortageReport();
        //            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
        //            EmptyDegreeRatioId = db.jntuh_degree
        //                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
        //                                 .Select(d => d.id).ToArray();

        //            //get degreeIds based on collegeId
        //            int[] DegreeIds = (from cd in db.jntuh_college_degree
        //                               join d in db.jntuh_degree on cd.degreeId equals d.id
        //                               where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true && !EmptyDegreeRatioId.Contains(d.id))
        //                               orderby d.degreeDisplayOrder
        //                               select cd.degreeId).ToArray();
        //            string DegreeName = string.Empty;
        //            if (DegreeIds.Count() > 0)
        //            {
        //                foreach (var item in DegreeIds)
        //                {
        //                    DegreeName = string.Empty;
        //                    TotalIntake = 0;
        //                    CalculatedRatio = 0;
        //                    TotalComputers = 0;
        //                    Existingratio = (db.jntuh_computer_student_ratio_norms
        //                                         .Where(c => c.isActive == true && c.degreeId == item)
        //                                         .Select(c => c.Norms)
        //                                         .FirstOrDefault()).Split(':')[1];
        //                    ActualRatioValue = Convert.ToInt32(Existingratio);
        //                    TotalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
        //                    TotalComputers = db.jntuh_college_computer_student_ratio
        //                                       .Where(c => c.collegeId == collegeId &&
        //                                                   c.degreeId == item)
        //                                       .Select(c => c.availableComputers)
        //                                       .FirstOrDefault();
        //                    if (TotalIntake != 0 && TotalComputers != 0)
        //                    {
        //                        CalculatedRatio = TotalIntake / TotalComputers;
        //                        if (ActualRatioValue != 0 && CalculatedRatio != 0)
        //                        {
        //                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree)
        //                                                    .FirstOrDefault();
        //                            if (ActualRatioValue < decimal.Round(CalculatedRatio))
        //                            {
        //                                ComputersShortageCount++;
        //                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));

        //                            }
        //                            else
        //                            {
        //                                //DegreeName = DegreeName + "- NIL";
        //                                DegreeName = "NIL";
        //                            }
        //                        }
        //                    }
        //                    if (TotalIntake != 0 && TotalComputers == 0)
        //                    {
        //                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree).FirstOrDefault();
        //                        ComputersShortageCount++;
        //                        //DegreeName = DegreeName + "- No Computers";
        //                        DegreeName = "No Computers";
        //                    }

        //                    var collegeAddressDetails = (from c in db.jntuh_college
        //                                                 join a in db.jntuh_address on c.id equals a.collegeId
        //                                                 where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                                 select new
        //                                                 {
        //                                                     c.collegeCode,
        //                                                     c.collegeName,
        //                                                     a.address,
        //                                                     a.mobile
        //                                                 }).FirstOrDefault();

        //                    computerShortageReport.collegeId = collegeId;
        //                    computerShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //                    computerShortageReport.collegeName = collegeAddressDetails.collegeName;
        //                    computerShortageReport.address = collegeAddressDetails.address;
        //                    computerShortageReport.mobile = collegeAddressDetails.mobile;
        //                    computerShortageReport.DegreeName = DegreeName;
        //                    computerShortageReport.ComputersShortageCount = ComputersShortageCount;
        //                    computerShortageReport.observations = DegreeName;
        //                    ComputerShortageReportList.Add(computerShortageReport);
        //                }
        //            }

        //        }
        //        ComputerShortageReportList = ComputerShortageReportList.Where(cs =>cs.DegreeName == "No Computers").Distinct().OrderBy(cs => cs.collegeName).ToList();
        //        ViewBag.shortageCollegeList = ComputerShortageReportList;
        //        fileName = exportType + ".xls"; 
        //    }
        //    if (exportType == "Shortage of Faculty")
        //    {
        //        string DegreeName = string.Empty;
        //        decimal Caluculatedvalue = 0;
        //        decimal FacultyIntake = 0;
        //        decimal TotalFaculty = 0;
        //        int ActualRatioValue = 0;
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();
        //            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
        //            EmptyDegreeRatioId = db.jntuh_degree
        //                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
        //                                 .Select(d => d.id).ToArray();

        //            //get degreeIds based on collegeId
        //            int[] DegreeIds = (from cd in db.jntuh_college_degree
        //                               join d in db.jntuh_degree on cd.degreeId equals d.id
        //                               where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true && !EmptyDegreeRatioId.Contains(d.id))
        //                               orderby d.degreeDisplayOrder
        //                               select cd.degreeId).ToArray();
                 
        //            if (DegreeIds.Count() > 0)
        //            {
        //                foreach (var item in DegreeIds)
        //                {
        //                    DegreeName = string.Empty;
        //                    Caluculatedvalue = 0;
        //                    ActualRatioValue = Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
        //                                   .Where(f => f.isActive == true && f.degreeId == item)
        //                                   .Select(f => f.Norms)
        //                                  .FirstOrDefault()).Split(':')[1]);
        //                    FacultyIntake = db.jntuh_college_faculty_student_ratio
        //                                           .Where(f => f.collegeId == collegeId &&
        //                                                       f.degreeId == item)
        //                                           .Select(f => f.totalIntake)
        //                                           .FirstOrDefault();
        //                    TotalFaculty = db.jntuh_college_faculty_student_ratio
        //                                           .Where(f => f.collegeId == collegeId &&
        //                                                       f.degreeId == item)
        //                                           .Select(f => f.totalFaculty)
        //                                           .FirstOrDefault();
        //                    if (FacultyIntake != 0 && TotalFaculty != 0)
        //                    {
        //                        Caluculatedvalue = FacultyIntake / TotalFaculty;
        //                        if (ActualRatioValue != 0 && Caluculatedvalue != 0)
        //                        {
        //                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                        .Select(d => d.degree).FirstOrDefault();
        //                            if (ActualRatioValue < decimal.Round(Caluculatedvalue))
        //                            {
        //                                FacultyShortageCount++;
        //                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
        //                            }
        //                            else
        //                            {
        //                                //DegreeName = DegreeName + "- NIL";
        //                                DegreeName = "NIL";
        //                            }
        //                        }
        //                    }
        //                    if (FacultyIntake != 0 && TotalFaculty == 0)
        //                    {
        //                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree).FirstOrDefault();
        //                        FacultyShortageCount++;
        //                        //DegreeName = DegreeName + "- No Faculty";
        //                        DegreeName = "No Faculty";
        //                    }

        //                    var collegeAddressDetails = (from c in db.jntuh_college
        //                                                 join a in db.jntuh_address on c.id equals a.collegeId
        //                                                 where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                                 select new
        //                                                 {
        //                                                     c.collegeCode,
        //                                                     c.collegeName,
        //                                                     a.address,
        //                                                     a.mobile
        //                                                 }).FirstOrDefault();

        //                    facultyShortageReport.collegeId = collegeId;
        //                    facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //                    facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //                    facultyShortageReport.address = collegeAddressDetails.address;
        //                    facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //                    facultyShortageReport.DegreeName = DegreeName;
        //                    facultyShortageReport.FacultyShortageCount = FacultyShortageCount;
        //                    facultyShortageReport.observations = "Shortage of Faculty";
        //                    FacultyShortageReportList.Add(facultyShortageReport);
        //                }
        //            }
        //        }
        //         //&& cs.DegreeName != "No Faculty"
        //        FacultyShortageReportList = FacultyShortageReportList.Where(cs => cs.DegreeName!="NIL").Distinct().OrderBy(cs => cs.collegeName).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList;
        //        fileName = exportType + ".xls"; 
             
        //    }
        //    if (exportType == "No Faculty")
        //    {
        //        string DegreeName = string.Empty;
        //        decimal Caluculatedvalue = 0;
        //        decimal FacultyIntake = 0;
        //        decimal TotalFaculty = 0;
        //        int ActualRatioValue = 0;
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();
        //            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
        //            EmptyDegreeRatioId = db.jntuh_degree
        //                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
        //                                 .Select(d => d.id).ToArray();

        //            //get degreeIds based on collegeId
        //            int[] DegreeIds = (from cd in db.jntuh_college_degree
        //                               join d in db.jntuh_degree on cd.degreeId equals d.id
        //                               where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true && !EmptyDegreeRatioId.Contains(d.id))
        //                               orderby d.degreeDisplayOrder
        //                               select cd.degreeId).ToArray();

        //            if (DegreeIds.Count() > 0)
        //            {
        //                foreach (var item in DegreeIds)
        //                {
        //                    DegreeName = string.Empty;
        //                    Caluculatedvalue = 0;
        //                    ActualRatioValue = Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
        //                                   .Where(f => f.isActive == true && f.degreeId == item)
        //                                   .Select(f => f.Norms)
        //                                  .FirstOrDefault()).Split(':')[1]);
        //                    FacultyIntake = db.jntuh_college_faculty_student_ratio
        //                                           .Where(f => f.collegeId == collegeId &&
        //                                                       f.degreeId == item)
        //                                           .Select(f => f.totalIntake)
        //                                           .FirstOrDefault();
        //                    TotalFaculty = db.jntuh_college_faculty_student_ratio
        //                                           .Where(f => f.collegeId == collegeId &&
        //                                                       f.degreeId == item)
        //                                           .Select(f => f.totalFaculty)
        //                                           .FirstOrDefault();
        //                    if (FacultyIntake != 0 && TotalFaculty != 0)
        //                    {
        //                        Caluculatedvalue = FacultyIntake / TotalFaculty;
        //                        if (ActualRatioValue != 0 && Caluculatedvalue != 0)
        //                        {
        //                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                        .Select(d => d.degree).FirstOrDefault();
        //                            if (ActualRatioValue < decimal.Round(Caluculatedvalue))
        //                            {
        //                                FacultyShortageCount++;
        //                                DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
        //                            }
        //                            else
        //                            {
        //                                //DegreeName = DegreeName + "- NIL";
        //                                DegreeName = "NIL";
        //                            }
        //                        }
        //                    }
        //                    if (FacultyIntake != 0 && TotalFaculty == 0)
        //                    {
        //                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
        //                                                    .Select(d => d.degree).FirstOrDefault();
        //                        FacultyShortageCount++;
        //                        //DegreeName = DegreeName + "- No Faculty";
        //                        DegreeName = "No Faculty";
        //                    }
        //                    var collegeAddressDetails = (from c in db.jntuh_college
        //                                                 join a in db.jntuh_address on c.id equals a.collegeId
        //                                                 where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                                 select new
        //                                                 {
        //                                                     c.collegeCode,
        //                                                     c.collegeName,
        //                                                     a.address,
        //                                                     a.mobile
        //                                                 }).FirstOrDefault();

        //                    facultyShortageReport.collegeId = collegeId;
        //                    facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //                    facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //                    facultyShortageReport.address = collegeAddressDetails.address;
        //                    facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //                    facultyShortageReport.DegreeName = DegreeName;
        //                    facultyShortageReport.FacultyShortageCount = FacultyShortageCount;
        //                    facultyShortageReport.observations = DegreeName;
        //                    FacultyShortageReportList.Add(facultyShortageReport);
        //                }
        //            }
        //        }
        //        FacultyShortageReportList = FacultyShortageReportList.Where(cs =>  cs.DegreeName == "No Faculty").Distinct().OrderBy(cs => cs.collegeName).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList;
        //        fileName = exportType + ".xls"; 
        //    }
        //    if (exportType == "Shortage of Ratified Faculty 0-25%")
        //    {
        //        ViewBag.exportType = "RatifiedFaculty";
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();           


        //            decimal Percentage = 0;
        //            decimal facultyRatifiedSum = 0;
        //            decimal facultySum = 0;
        //            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.ratified)
        //                                        .ToArray();
        //            int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.totalFaculty)
        //                                        .ToArray();
        //            if (facultyRatifiedCount.Count() != 0)
        //            {
        //                facultyRatifiedSum = facultyRatifiedCount.Sum();
        //            }
        //            if (facultyCount.Count() != 0)
        //            {
        //                facultySum = facultyCount.Sum();
        //            }
        //            if (facultyRatifiedSum != 0 && facultySum != 0)
        //            {
        //                Percentage = (facultyRatifiedSum / facultySum) * 100;
        //                Percentage = Math.Round(Percentage, 2);
        //            }

        //            var collegeAddressDetails = (from c in db.jntuh_college
        //                                         join a in db.jntuh_address on c.id equals a.collegeId
        //                                         where (c.isActive == true && a.addressTye == "College" && c.id==collegeId)
        //                                         select new
        //                                         {
        //                                             c.collegeCode,
        //                                             c.collegeName,
        //                                             a.address,
        //                                             a.mobile
        //                                         }).FirstOrDefault();

        //            facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //            facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //            facultyShortageReport.address = collegeAddressDetails.address;
        //            facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //            facultyShortageReport.ratifiedFacultyNo = facultyRatifiedSum + "/" + facultySum;
        //            facultyShortageReport.ratifiedFacultyPercentage = Percentage;
        //            decimal diff = 100 - Percentage;
        //            facultyShortageReport.observations = "Shortage of Ratified Faculty is " + diff;
        //            FacultyShortageReportList.Add(facultyShortageReport);                  

        //        }

        //        FacultyShortageReportList = FacultyShortageReportList.Where(fs => fs.ratifiedFacultyPercentage >= 76 && fs.ratifiedFacultyPercentage <=100).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList.OrderBy(cs => cs.collegeName);
        //        fileName = exportType + ".xls"; 

        //    }
        //    if (exportType == "Shortage of Ratified Faculty 26-50%")
        //    {
        //        ViewBag.exportType = "RatifiedFaculty";
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();


        //            decimal Percentage = 0;
        //            decimal facultyRatifiedSum = 0;
        //            decimal facultySum = 0;
        //            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.ratified)
        //                                        .ToArray();
        //            int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.totalFaculty)
        //                                        .ToArray();
        //            if (facultyRatifiedCount.Count() != 0)
        //            {
        //                facultyRatifiedSum = facultyRatifiedCount.Sum();
        //            }
        //            if (facultyCount.Count() != 0)
        //            {
        //                facultySum = facultyCount.Sum();
        //            }
        //            if (facultyRatifiedSum != 0 && facultySum != 0)
        //            {
        //                Percentage = (facultyRatifiedSum / facultySum) * 100;
        //                Percentage = Math.Round(Percentage, 2);
        //            }

        //            var collegeAddressDetails = (from c in db.jntuh_college
        //                                         join a in db.jntuh_address on c.id equals a.collegeId
        //                                         where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                         select new
        //                                         {
        //                                             c.collegeCode,
        //                                             c.collegeName,
        //                                             a.address,
        //                                             a.mobile
        //                                         }).FirstOrDefault();

        //            facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //            facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //            facultyShortageReport.address = collegeAddressDetails.address;
        //            facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //            facultyShortageReport.ratifiedFacultyNo = facultyRatifiedSum + "/" + facultySum;
        //            facultyShortageReport.ratifiedFacultyPercentage = Percentage;
        //            decimal diff = 100 - Percentage;
        //            facultyShortageReport.observations = "Shortage of Ratified Faculty is " + diff;
        //            FacultyShortageReportList.Add(facultyShortageReport);

        //        }

        //        FacultyShortageReportList = FacultyShortageReportList.Where(fs => fs.ratifiedFacultyPercentage >= 51 && fs.ratifiedFacultyPercentage <= 75).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList.OrderBy(cs => cs.collegeName); ;
        //        fileName = exportType + ".xls"; 
        //    }
        //    if (exportType == "Shortage of Ratified Faculty 51-75%")
        //    {
        //        ViewBag.exportType = "RatifiedFaculty";
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();


        //            decimal Percentage = 0;
        //            decimal facultyRatifiedSum = 0;
        //            decimal facultySum = 0;
        //            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.ratified)
        //                                        .ToArray();
        //            int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.totalFaculty)
        //                                        .ToArray();
        //            if (facultyRatifiedCount.Count() != 0)
        //            {
        //                facultyRatifiedSum = facultyRatifiedCount.Sum();
        //            }
        //            if (facultyCount.Count() != 0)
        //            {
        //                facultySum = facultyCount.Sum();
        //            }
        //            if (facultyRatifiedSum != 0 && facultySum != 0)
        //            {
        //                Percentage = (facultyRatifiedSum / facultySum) * 100;
        //                Percentage = Math.Round(Percentage, 2);
        //            }

        //            var collegeAddressDetails = (from c in db.jntuh_college
        //                                         join a in db.jntuh_address on c.id equals a.collegeId
        //                                         where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                         select new
        //                                         {
        //                                             c.collegeCode,
        //                                             c.collegeName,
        //                                             a.address,
        //                                             a.mobile
        //                                         }).FirstOrDefault();

        //            facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //            facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //            facultyShortageReport.address = collegeAddressDetails.address;
        //            facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //            facultyShortageReport.ratifiedFacultyNo = facultyRatifiedSum + "/" + facultySum;
        //            facultyShortageReport.ratifiedFacultyPercentage = Percentage;
        //            decimal diff = 100 - Percentage;
        //            facultyShortageReport.observations = "Shortage of Ratified Faculty is " + diff;
        //            FacultyShortageReportList.Add(facultyShortageReport);

        //        }

        //        FacultyShortageReportList = FacultyShortageReportList.Where(fs => fs.ratifiedFacultyPercentage >= 26 && fs.ratifiedFacultyPercentage <= 50).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList.OrderBy(cs => cs.collegeName); ;
        //        fileName = exportType + ".xls"; 
        //    }
        //    if (exportType == "Shortage of Ratified Faculty 76-100%")
        //    {
        //        ViewBag.exportType = "RatifiedFaculty";
        //        int[] collegeIds = db.jntuh_college.Where(c => c.isActive == true && dataentryVerifiedCollegeIds.Contains(c.id)).Select(c => c.id).ToArray();
        //        List<FacultyShortageReport> FacultyShortageReportList = new List<FacultyShortageReport>();
        //        foreach (var collegeId in collegeIds)
        //        {
        //            FacultyShortageReport facultyShortageReport = new FacultyShortageReport();


        //            decimal Percentage = 0;
        //            decimal facultyRatifiedSum = 0;
        //            decimal facultySum = 0;
        //            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.ratified)
        //                                        .ToArray();
        //            int[] facultyCount = db.jntuh_college_faculty_student_ratio
        //                                        .Where(f => f.collegeId == collegeId)
        //                                        .Select(f => f.totalFaculty)
        //                                        .ToArray();
        //            if (facultyRatifiedCount.Count() != 0)
        //            {
        //                facultyRatifiedSum = facultyRatifiedCount.Sum();
        //            }
        //            if (facultyCount.Count() != 0)
        //            {
        //                facultySum = facultyCount.Sum();
        //            }
        //            if (facultyRatifiedSum != 0 && facultySum != 0)
        //            {
        //                Percentage = (facultyRatifiedSum / facultySum) * 100;
        //                Percentage = Math.Round(Percentage, 2);
        //            }

        //            var collegeAddressDetails = (from c in db.jntuh_college
        //                                         join a in db.jntuh_address on c.id equals a.collegeId
        //                                         where (c.isActive == true && a.addressTye == "College" && c.id == collegeId)
        //                                         select new
        //                                         {
        //                                             c.collegeCode,
        //                                             c.collegeName,
        //                                             a.address,
        //                                             a.mobile
        //                                         }).FirstOrDefault();

        //            facultyShortageReport.collegeCode = collegeAddressDetails.collegeCode;
        //            facultyShortageReport.collegeName = collegeAddressDetails.collegeName;
        //            facultyShortageReport.address = collegeAddressDetails.address;
        //            facultyShortageReport.mobile = collegeAddressDetails.mobile;
        //            facultyShortageReport.ratifiedFacultyNo = facultyRatifiedSum + "/" + facultySum;
        //            facultyShortageReport.ratifiedFacultyPercentage = Percentage;
        //            decimal diff = 100 - Percentage;
        //            facultyShortageReport.observations = "Shortage of Ratified Faculty is " + diff;
        //            FacultyShortageReportList.Add(facultyShortageReport);

        //        }

        //        FacultyShortageReportList = FacultyShortageReportList.Where(fs => fs.ratifiedFacultyPercentage >= 0 && fs.ratifiedFacultyPercentage <= 25).ToList();
        //        ViewBag.shortageCollegeList = FacultyShortageReportList.OrderBy(cs => cs.collegeName); 
        //        fileName = exportType + ".xls"; 
        //    } 

        //    Response.ClearContent();
        //    Response.Buffer = true;
        //    Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
        //    Response.ContentType = "application/vnd.ms-excel";
        //    return PartialView("~/Views/Reports/_ShortageReports.cshtml");

        //}

        [Authorize(Roles = "Admin")]
        public ActionResult YearWiseEstablishedColleges(string exportType)
        {
             int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            string fileName=string.Empty;
            if (exportType == "Year Wise Established Colleges")
            {
                fileName= exportType+".xls";
                List<YearWiseEstablishedColleges> yearWiseEstablishedCollegesList = new List<YearWiseEstablishedColleges>();
                var EstablishYearList= (from ce in db.jntuh_college_establishment
                                          join c in db.jntuh_college on ce.collegeId equals c.id
                                          join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
                                          where (c.isActive == true && ce.instituteEstablishedYear!=0 && da.isCompleted==true && da.isActive==true && da.InspectionPhaseId==InspectionPhaseId)
                                          select new
                                          {
                                              ce.instituteEstablishedYear
                                          }).Distinct().ToList();

                foreach (var item in EstablishYearList)
                {
                  
                        YearWiseEstablishedColleges yearWiseEstablishedColleges = new YearWiseEstablishedColleges();

                        var list = (from c in db.jntuh_college
                                    join ct in db.jntuh_college_type on c.collegeTypeID equals ct.id
                                    join ce in db.jntuh_college_establishment on c.id equals ce.collegeId
                                    join da in db.jntuh_dataentry_allotment on c.id equals da.collegeID
                                    where (c.isActive == true && ct.isActive == true && ce.instituteEstablishedYear == item.instituteEstablishedYear && da.isCompleted == true && da.isActive == true && da.InspectionPhaseId == InspectionPhaseId)
                                    select new YearWiseEstablishedColleges
                                    {
                                        year = ce.instituteEstablishedYear,
                                        collegeType = ct.collegeType
                                    }).ToList();

                        yearWiseEstablishedColleges.year = item.instituteEstablishedYear;
                        yearWiseEstablishedColleges.EngineeringCount = list.Where(e => e.collegeType == "Engineering").Count();
                        yearWiseEstablishedColleges.PharmacyCount = list.Where(e => e.collegeType == "Pharmacy").Count();
                        yearWiseEstablishedColleges.StandaloneCount = list.Where(e => e.collegeType == "Standalone").Count();
                        yearWiseEstablishedColleges.IntegratedCampusCount = list.Where(e => e.collegeType == "Integrated Campus").Count();
                        yearWiseEstablishedColleges.TechnicalCampusCount = list.Where(e => e.collegeType == "Technical Campus").Count();
                        yearWiseEstablishedColleges.totalCount = yearWiseEstablishedColleges.EngineeringCount + yearWiseEstablishedColleges.PharmacyCount + yearWiseEstablishedColleges.StandaloneCount + yearWiseEstablishedColleges.IntegratedCampusCount + yearWiseEstablishedColleges.TechnicalCampusCount;

                        yearWiseEstablishedCollegesList.Add(yearWiseEstablishedColleges);
                   
                }
                yearWiseEstablishedCollegesList=yearWiseEstablishedCollegesList.OrderBy(e=>e.year).ToList();
                ViewBag.yearWiseEstablishedCollegesList = yearWiseEstablishedCollegesList;
               
            }
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_YearWiseEstablishedColleges.cshtml");

        }


    }
}
