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
    
    public partial class jntuh_desirable_requirement_type
    {
        public jntuh_desirable_requirement_type()
        {
            this.jntuh_college_desirable_requirement = new HashSet<jntuh_college_desirable_requirement>();
            this.jntuh_college_hostel_maintenance = new HashSet<jntuh_college_hostel_maintenance>();
        }
    
        public int id { get; set; }
        public string requirementType { get; set; }
        public bool isHostelRequirement { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual ICollection<jntuh_college_desirable_requirement> jntuh_college_desirable_requirement { get; set; }
        public virtual ICollection<jntuh_college_hostel_maintenance> jntuh_college_hostel_maintenance { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}
