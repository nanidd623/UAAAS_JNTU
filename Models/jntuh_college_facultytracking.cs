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
    
    public partial class jntuh_college_facultytracking
    {
        public int Id { get; set; }
        public int academicYearId { get; set; }
        public int collegeId { get; set; }
        public Nullable<int> previousworkingcollegeid { get; set; }
        public string RegistrationNumber { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> SpecializationId { get; set; }
        public string aadhaarnumber { get; set; }
        public string aadhaardocument { get; set; }
        public Nullable<System.DateTime> FacultyJoinDate { get; set; }
        public string FacultyJoinDocument { get; set; }
        public Nullable<System.DateTime> relevingdate { get; set; }
        public string relevingdocumnt { get; set; }
        public string scmdocument { get; set; }
        public string payscale { get; set; }
        public string grosssalary { get; set; }
        public string designation { get; set; }
        public int ActionType { get; set; }
        public string FacultyType { get; set; }
        public string FacultyStatus { get; set; }
        public string Reasion { get; set; }
        public Nullable<bool> isActive { get; set; }
        public System.DateTime Createdon { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> Updatedon { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    }
}
