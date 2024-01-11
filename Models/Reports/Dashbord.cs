using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace UAAAS.Models
{
    public class Dashbord
    {
        public int CollegeId { get; set; }
        public bool IsEditable { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string EamcetCode { get; set; }
        public string IcetCode { get; set; }
        public string Landline { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Fax { get; set; }
        public string CollegeType { get; set; }
        public string CourseInformation { get; set; }
        public string PrincipalName { get; set; }
        public string PrincipalLandLine { get; set; }
        public string PrincipalMobile { get; set; }
        public string PrincipalEmail { get; set; }
        public string PrincipalFax { get; set; }
        public string DirectorName { get; set; }
        public string DirectorLandLine { get; set; }
        public string DirectorMobile { get; set; }
        public string DirectorEmail { get; set; }
        public string DirectorFax { get; set; }
        public int TotalFaculty { get; set; }
        public int TeachingFaculty { get; set; }
        public int NonTeachingFaculty { get; set; }
        public int TechnicalFaculty { get; set; }
        public int RatifiedFaculty { get; set; }
        public List<CollegeIntakeDetails> CollegeIntakeDetails { get; set; }
        public List<CollegeAcademicPerformanceDetails> CollegeAcademicPerformanceDetails { get; set; }
        public List<CollegePlacementsDetails> CollegePlacementsDetails { get; set; }
        public List<CollegeLibraryBooksDetails> CollegeLibraryDetails { get; set; }
        public List<CollegeComputersDetails> CollegeComputersDetails { get; set; }
    }  
    public class CollegeIntakeDetails
    {
        public int DegreeId{set;get;}
        public string DegreeName{get;set;}
        public int AdmittedIntake { get; set; }
        public int ApprovedIntake { get; set; }
    }
    public class CollegeAcademicPerformanceDetails
    {
        public int SpecializationId { set; get; }
        public int ShiftId { get; set; }
        public int DegreeId { set; get; }
        public int YearIndegreeId { get; set; }
        public string DegreeName { get; set; }
        public decimal AppearedStudents1 { get; set; }
        public decimal PassedStudents1 { get; set; }
        public decimal Percentage1 { get; set; }
        public decimal AppearedStudents2 { get; set; }
        public decimal PassedStudents2 { get; set; }
        public decimal Percentage2 { get; set; }
        public decimal AppearedStudents3 { get; set; }
        public decimal PassedStudents3 { get; set; }
        public decimal Percentage3 { get; set; }
        public decimal AppearedStudents4 { get; set; }
        public decimal PassedStudents4 { get; set; }
        public decimal Percentage4 { get; set; }
    }
    public class CollegePlacementsDetails
    {
        public int SpecializationId { set; get; }
        public int DegreeId { set; get; }
        public string DegreeName { get; set; }
        public decimal? TotalStudentsPassed1 { get; set; }
        public decimal? TotalStudentsPlaced1 { get; set; }
        public decimal? Percentage1 { get; set; }
        public decimal? TotalStudentsPassed2 { get; set; }
        public decimal? TotalStudentsPlaced2 { get; set; }
        public decimal? Percentage2 { get; set; }
        public decimal? TotalStudentsPassed3 { get; set; }
        public decimal? TotalStudentsPlaced3 { get; set; }
        public decimal? Percentage3 { get; set; }
        public decimal? TotalStudentsPassed4 { get; set; }
        public decimal? TotalStudentsPlaced4 { get; set; }
        public decimal? Percentage4 { get; set; }
    }
    public class CollegeLibraryBooksDetails
    {
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public int TotalTitles { get; set; }
        public int TotalVolumes { get; set; }
        public int TotalNationalJournals { get; set; }
        public int TotalInterNationalJournals { get; set; }
        public int TotalEJournals { get; set; }
    }
    public class CollegeComputersDetails
    {
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
        public int TotalComputers { get; set; }
    }
    public class AdminDashbord
    {
        //This is for Colleges Count
        public int totalColleges { get; set; }
        public int activeColleges { get; set; }
        public int inActiveColleges { get; set; }
        public int permanentColleges { get; set; }
        public int newColleges { get; set; }
        public int closerColleges { get; set; }
        public int emailtoColleges { get; set; }
        public int AutonomousColleges { get; set; }

        //This is for colleges
        public int collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string eamcetcode { get; set; }
        public string icetcode { get; set; }

        public string address { get; set; }
        public string townorCity { get; set; }
        public int? pincode { get; set; }
        public string landline { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public string addressType { get; set; }
        public bool isActive { get; set; }
        public bool isPermanent { get; set; }
        public bool isClosed { get; set; }
        public bool isNew { get; set; }

        //DataEntry
        public int committeeSubmittedCollegesCount { get; set; }
        public int dataentryAssignedCollegesCount { get; set; }
        public int dataentryCompletedCollegesCount { get; set; }
        public int dataentryVerifiedCollegesCount { get; set; }
        public int CommitteePendingSubmittedCollegesCount { get; set; }

        //Total Intake degree wise
        public string degree { get; set; }
        public IEnumerable<DegreewiseTotalIntake> degreewiseTotalIntake1 { get; set; }
        public IEnumerable<DegreewiseTotalIntake> degreewiseTotalIntake2 { get; set; }
        public IEnumerable<DegreewiseTotalIntake> degreewiseTotalIntake3 { get; set; }
        public IEnumerable<DegreewiseTotalIntake> Praposedintakedownload { get; set; }



        //Scheduking Report
        public DateTime ? orderDate { get; set; }
        public DateTime ? inspectiondate { get; set; }
        public DateTime ? alternateInspectionDate { get; set; }


        //Schedule count
        public int schedulecount { get; set; }
        public int pendingSchedulecount { get; set; }
        public int orderscount { get; set; }
        public int pendingOrderscount { get; set; }

        //Users Count
        public int adminTotalUsersCount { get; set; }
        public int adminActiveUsersCount { get; set; }
        public int adminInActiveUsersCount { get; set; }

        public int committeeTotalUsersCount { get; set; }
        public int committeeActiveUsersCount { get; set; }
        public int committeeInActiveUsersCount { get; set; }

        public int collegeTotalUsersCount { get; set; }
        public int collegeActiveUsersCount { get; set; }
        public int collegeInActiveUsersCount { get; set; }        

        public int dataentryTotalUsersCount { get; set; }
        public int dataentryActiveUsersCount { get; set; }
        public int dataentryInActiveUsersCount { get; set; }
        
        //College submissionCount

        public int submissionCount { get; set; }
        public int submissionPendingCount { get; set; }
        public int editstatusCount { get; set; }
        

        
            
    }
    public class DegreewiseTotalIntake
    {
        public string degree { get; set; }
        public int totalIntake { get; set; }
        public int? proposedIntake { get; set; }
        public int approvedIntake { get; set; }
        public int admittedIntake { get; set; }
        public int degreeDisplayOrder { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int? academicYearId { get; set; }
        public int? degreeId { get; set; }
        public int? departmentId { get; set; }
        public int? SpecealizationId { get; set; }
        public string CourseStatus { get; set; }
        public int ProposedCourses { get; set; }

        //College Address
        public string address { get; set; }
        public string district { get; set; }
    }

    //collegedocuments
    public class CollegePhotos
    {
        public int id { get; set; }
        public string scannedCopy { get; set; }
        public string documentName { get; set; }
       
    }
    //Shortage Report
    public class ShortageReport
    {
        public int collegeId { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        public string observations { get; set; }
        public string address { get; set; }
        public string mobile { get; set; }
    }
    public class ComputerShortageReport
    {
        public int collegeId { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        public string DegreeName { get; set; }
        public int ComputersShortageCount { get; set; }
        public string observations { get; set; }
        public string address { get; set; }
        public string mobile { get; set; }
    }
    public class FacultyShortageReport
    {
        public int collegeId { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        public string DegreeName { get; set; }
        public int FacultyShortageCount { get; set; }
        public string observations { get; set; }
        public string address { get; set; }
        public string mobile { get; set; }
        public string ratifiedFacultyNo { get; set; }
        public decimal ratifiedFacultyPercentage { get; set; }
       
    }
    public class YearWiseEstablishedColleges
    {
        public int year { get; set; }
        public string collegeType { get; set; }
        public int EngineeringCount { get; set; }
        public int PharmacyCount { get; set; }
        public int StandaloneCount { get; set; }
        public int IntegratedCampusCount { get; set; }
        public int TechnicalCampusCount { get; set; }
        public int totalCount { get; set; }
    }

    public class CollegePGCourses
    {
        public int ? collegeid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
    }


}