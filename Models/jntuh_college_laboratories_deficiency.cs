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
    
    public partial class jntuh_college_laboratories_deficiency
    {
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public int SpecializationId { get; set; }
        public int Year { get; set; }
        public int Semister { get; set; }
        public string LabCode { get; set; }
        public bool Deficiency { get; set; }
        public Nullable<bool> DeficiencyStatus { get; set; }
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
    
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
    }
}
