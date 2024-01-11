using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UAAAS.Controllers.Reports;

namespace UAAAS.Models
{
    public class PharmacyReportsClass
    {
        public int Sno { get; set; }
        public int Collegeid { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public int ShiftId { get; set; }
        public int? TotalIntake { get; set; }
        public int? ProposedIntake { get; set; }
        public int? CoursesCount { get; set; }
        public int? NoOfFacultyRequired { get; set; }
        public int? NoOfAvilableFaculty { get; set; }
        public string PharmacySpecialization { get; set; }
        public int? SpecializationwiseRequiredFaculty { get; set; }
        public int? SpecializationwiseAvilableFaculty { get; set; }

        public int? Group1RequiredFaculty { get; set; }
        public int? Group1AvilableFaculty { get; set; }

        public int? Group2RequiredFaculty { get; set; }
        public int? Group2AvilableFaculty { get; set; }

        public int? Group3RequiredFaculty { get; set; }
        public int? Group3AvilableFaculty { get; set; }

        public int? Group4RequiredFaculty { get; set; }
        public int? Group4AvilableFaculty { get; set; }

        public string Deficiency { get; set; }
        public int? RequiredPHdFaculty { get; set; }
        public int? PHdFaculty { get; set; }
        public int? TotalPHdFaculty { get; set; }
        public DateTime? createdon { get; set; }
        public bool IsActive { get; set; }
        public string GroupId { get; set; }

        public string address { get; set; }
        public int districtId { get; set; }
        public string district { get; set; }

        public int? JntuhApprovedIntake { get; set; }
    }

    public class PharmacyIntakeFaculty
    {
        public int? collegeId { get; set; }
        public int? DegreeId { get; set; }
        public string Department { get; set; }
        public string GroupId { get; set; }
        public int? SpecializationId { get; set; }
        public string Specialization { get; set; }
        public int? TotalIntake { get; set; }
        public int? ProposedIntake { get; set; }
        public int? TotalRequiredFaculty { get; set; }
        public int? SpecializationWiseFaculty { get; set; }
        public int? FacultyCount { get; set; }
    }

    public class CollegeIntakeReport
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
        public int DegreeId { get; set; }
        public int? degreeDisplayOrder { get; set; }
        public string collegeRandomCode { get; set; }
        public int ProposedIntake { get; set; }
        public int approvedIntake1 { get; set; }
        public int approvedIntake2 { get; set; }
        public int approvedIntake3 { get; set; }
        public int approvedIntake4 { get; set; }
        public int approvedIntake5 { get; set; }

        //Added this in 25-04-2017
        public int admittedIntake1 { get; set; }
        public int admittedIntake2 { get; set; }
        public int admittedIntake3 { get; set; }
        public int admittedIntake4 { get; set; }
        public int admittedIntake5 { get; set; }

        public int SanctionIntake1 { get; set; }
        public int SanctionIntake2 { get; set; }
        public int SanctionIntake3 { get; set; }
        public int SanctionIntake4 { get; set; }
        public int SanctionIntake5 { get; set; }

        public int totalAdmittedIntake { get; set; }
        
        public bool AffiliationStatus2 { get; set; }
        public bool AffiliationStatus3 { get; set; }
        public bool AffiliationStatus4 { get; set; }

        public int division1 { get; set; }
        public int division2 { get; set; }
        public int division3 { get; set; }

        public int totalIntake { get; set; }
        public decimal requiredFaculty { get; set; }
        public int phdFaculty { get; set; }
        public int SpecializationsphdFaculty { get; set; }
        public int SpecializationspgFaculty { get; set; }
        public int pgFaculty { get; set; }
        public int ugFaculty { get; set; }
        public int totalFaculty { get; set; }
        public int specializationWiseFaculty { get; set; }
        public int PharmacyspecializationWiseFaculty { get; set; }
        public int facultyWithoutPANAndAadhaar { get; set; }
        public int newlyAddedFaculty { get; set; }

        public bool isActive { get; set; }
        public DateTime? nbaFrom { get; set; }
        public DateTime? nbaTo { get; set; }

        public bool? deficiency { get; set; }
        public bool? PHDdeficiency { get; set; }
        public bool? PHDBtechdeficiency { get; set; }
        public int shortage { get; set; }
        public IList<Lab> LabsListDefs { get; set; }
        // public List<AnonymousLabclass> LabsListDefs1 { get; set; }
        //public List<AnonymousMBAMACclass> MBAMACDetails { get; set; }
        public bool deficiencystatus { get; set; }
        public string RegistrationNumber { get; set; }
        //=====18-06-2015=====//
        public int FalseNameFaculty { get; set; }
        public int FalsePhotoFaculty { get; set; }
        public int FalsePANNumberFaculty { get; set; }
        public int FalseAadhaarNumberFaculty { get; set; }
        public int CertificatesIncompleteFaculty { get; set; }
        public int AbsentFaculty { get; set; }
        public int AvailableFaculty { get; set; }
        public int AvailablePHDFaculty { get; set; }

        //For collegeintake

        public List<CollegeIntakeExisting> CollegeIntakeExistings { get; set; }

        public string AffliationStatus { get; set; }
        public decimal BphramacyrequiredFaculty { get; set; }
        public decimal pharmadrequiredfaculty { get; set; }
        public decimal pharmadPBrequiredfaculty { get; set; }
        public int totalcollegefaculty { get; set; }
        public int SortId { get; set; }

        public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
        public int BtechAdjustedFaculty { get; set; }
        public int specializationWiseFacultyPHDFaculty { get; set; }
        public IList<PhysicalLabMaster> PhysicalLabs { get; set; }
    }
}