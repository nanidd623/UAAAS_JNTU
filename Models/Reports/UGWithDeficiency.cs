using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class DeficiencyCollege
    {
        public int CollegeId { get; set; }
        public List<CollegeInfo> AllColleges { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string LabsShortage { get; set; }
        public string PrincipalGrade { get; set; }
        public string Number { get; set; }
        public decimal Percentage { get; set; }
        public string LibraryShortage { get; set; }
        public List<string> ComputersShortage { get; set; }
        public List<string> UGObservations { get; set; }
        public List<string> FacultyShortage { get; set; }
        public string CollegeAddress { get; set; }
        public int Establishyear { get; set; }
        public string IsPermanentCollege { get; set; }
        public string OverallPoints { get; set; }
        public string NBAStatus { get; set; }
        public string NAACStatus { get; set; }
        public string Grade;
        public int InspectionPhaseId { get; set; }
        public IEnumerable<CollegeSpecialization> CollegeSpecializations { get; set; }

        public string[] degreeId { get; set; }
        public IEnumerable<Item> degree { get; set; }

        public string[] degreeOnlyId { get; set; }
        public IEnumerable<Item> degreeOnly { get; set; }

        public string[] columnId { get; set; }
        public IEnumerable<Item> columnList { get; set; }

        public string[] reportTypeId { get; set; }
        public IEnumerable<Item> reportType { get; set; }

        public string[] shortageTypeId { get; set; }
        public IEnumerable<Item> shortageType { get; set; }

        public string[] remarksTypeId { get; set; }
        public IEnumerable<Item> remarksType { get; set; }

        public string[] viewTypeId { get; set; }
        public IEnumerable<Item> viewType { get; set; }

    }

    public class CollegeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CollegeSpecialization
    {
        public int CollegeId { get; set; }
        public string Specialization { get; set; }
        public string Intake { get; set; }
        public int ExistingIntake { get; set; }
    }

    public class CollegeDegreeDetails
    {
        public int CollegeId { get; set; }
        public int DegreeId { get; set; }
        public string DegreeType { get; set; }
        public string DegreeName { get; set; }
        public int DegreeTypeId { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public int CourseAffiliationStatusCodeId { get; set; }
        public int ApprovedIntake { get; set; }
        public int ProposedIntake { get; set; }
        public int ShiftId { get; set; }
        public int? DegreeDisplayOrder { get; set; }
    }

    public class UGWithDeficiency
    {
        public int CollegeId { get; set; }
        public List<WithDeficiencyColleges> WithDeficiencyColleges { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string LabsShortage { get; set; }
        public string PrincipalGrade { get; set; }
        public string Number { get; set; }
        public decimal Percentage { get; set; }
        public string LibraryShortage { get; set; }
        public List<string> ComputersShortage { get; set; }
        public List<string> UGObservations { get; set; }
        public List<string> FacultyShortage { get; set; }
        public string CollegeAddress { get; set; }
        public string Establishyear { get; set; }
        public string IsPermanentCollege { get; set; }
        public string OverallPoints { get; set; }
        public string NBAStatus { get; set; }
        public string NAACStatus { get; set; }
        public string Grade;
        public IEnumerable<UGWithDeficiencyCollegeSpecializations> CollegeSpecializations { get; set; }
    }

    public class UGWithDeficiencyCollegeSpecializations
    {
        public int CollegeId { get; set; }
        public string Specialization { get; set; }
        public string Intake { get; set; }
        public int ExistingIntake { get; set; }

        //ramesh added on 15-Sep-2014
        public int AY13admittedIntake { get; set; }
    }

    public class UGWithDeficiencySpecializations
    {
        public int CollegeId { get; set; }
        public int DegreeId { get; set; }
        public string DegreeType { get; set; }
        public string DegreeName { get; set; }
        public int DegreeTypeId { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public int CourseAffiliationStatusCodeId { get; set; }
        public int ProposedIntake { get; set; }
        public int ShiftId { get; set; }
    }

    public class WithDeficiencyColleges
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}