//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UAAAS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class jntuh_college_aictefaculty
    {
        public int Id { get; set; }
        public int AcademicyearId { get; set; }
        public int CollegeId { get; set; }
        public string FacultyId { get; set; }
        public string RegistrationNumber { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string AICTEFacultyType { get; set; }
        public string JobType { get; set; }
        public string ExactDesignation { get; set; }
        public Nullable<System.DateTime> DateOfJoiningTheInstitute { get; set; }
        public string PanNumber { get; set; }
        public string AadhaarNumber { get; set; }
        public string AppointmentType { get; set; }
        public string Doctorate { get; set; }
        public string MastersDegree { get; set; }
        public string BachelorsDegree { get; set; }
        public string OtherQualification { get; set; }
        public bool IsActive { get; set; }
        public string Programme { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Course { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    }
}
