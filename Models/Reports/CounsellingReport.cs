using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CounsellingReport
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }      
        public string Number { get; set; }      
        public string CollegeAddress { get; set; }
        public string Establishyear { get; set; }       
        public string IsPermanentCollege { get; set; }
        public string Grade { get; set; }
        public int InspectionPhaseId { get; set; }
        public IEnumerable<DegreewiseCollegeSpecializations> CollegeSpecializations { get; set; }
    }
    public class DegreewiseCollegeSpecializations
    {
        public int CollegeId { get; set; }
        public string Specialization { get; set; }
        public string Intake { get; set; }
        public string ApprovedIntake { get; set; }
        public string Provisional { get; set; }
        public int RequiredFaculty { get; set; }
        public int AvailableFaculty { get; set; }
        public int PHDAvailableFaculty { get; set; }
        public int PHDRequiredFaculty { get; set; }
        public string DepartmentName { get; set; }
        public bool isPercentage { get; set; }
        public int NTPLFaculty { get; set; }
        public int CSEPhDFaculty { get; set; }
        public int SortingOrder { get; set; }
    }
    public class Specializations
    {
        public int CollegeId { get; set; }
        public int DegreeId { get; set; }
        public int DepartmentId { get; set; }
        public int DegreeTypeId { get; set; }
        public int SpecializationId { get; set; }
        public int CourseAffiliationStatusCodeId { get; set; }
        public int ProposedIntake { get; set; }
        public int DollerCourseIntake { get; set; }
        public int ShiftId { get; set; }
        public int RequiredFaculty { get; set; }
        public int AvailableFaculty { get; set; }
    }
    
}