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
    
    public partial class jntuh_college_women_protection_cell_rti_antiragging_details
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicyear { get; set; }
        public int complainttype { get; set; }
        public string memberName { get; set; }
        public string registrationnumber { get; set; }
        public Nullable<int> memberDesignation { get; set; }
        public Nullable<int> actualDesignation { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public Nullable<bool> isactive { get; set; }
        public System.DateTime createdOn { get; set; }
        public int createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
    }
}