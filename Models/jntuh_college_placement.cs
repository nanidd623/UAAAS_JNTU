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
    
    public partial class jntuh_college_placement
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicYearId { get; set; }
        public int specializationId { get; set; }
        public Nullable<int> totalStudentsAppeared { get; set; }
        public Nullable<int> totalStudentsPassed { get; set; }
        public Nullable<int> totalStudentsDestincion { get; set; }
        public Nullable<int> totalStudentsfirstclass { get; set; }
        public Nullable<int> detainedforattendance { get; set; }
        public Nullable<int> detainedforcredits { get; set; }
        public Nullable<int> totalStudentsPlaced { get; set; }
        public Nullable<int> above10lpa { get; set; }
        public Nullable<int> above5to10lpa { get; set; }
        public Nullable<int> above3to5lpa { get; set; }
        public Nullable<int> below3lpa { get; set; }
        public Nullable<int> highereducation { get; set; }
        public Nullable<int> publicsector { get; set; }
        public Nullable<int> entrepreneurs { get; set; }
        public Nullable<decimal> placementpercentage { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
    }
}
