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
    
    public partial class jntuh_college_studentregistration
    {
        public int Id { get; set; }
        public int AcademicYearid { get; set; }
        public int CollegeId { get; set; }
        public string HallTicketNo { get; set; }
        public string StudentName { get; set; }
        public int Gender { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string FatherName { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }
        public string AadhaarNumber { get; set; }
        public string AadhaarDocument { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    }
}