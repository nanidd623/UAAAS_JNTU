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
    
    public partial class jntuh_college_status
    {
        public jntuh_college_status()
        {
            this.jntuh_affiliation_requests = new HashSet<jntuh_affiliation_requests>();
            this.jntuh_college = new HashSet<jntuh_college>();
            this.jntuh_college_minoritystatus = new HashSet<jntuh_college_minoritystatus>();
        }
    
        public int id { get; set; }
        public string collegeStatus { get; set; }
        public string statusDescription { get; set; }
        public Nullable<int> mainstatusId { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual ICollection<jntuh_affiliation_requests> jntuh_affiliation_requests { get; set; }
        public virtual ICollection<jntuh_college> jntuh_college { get; set; }
        public virtual ICollection<jntuh_college_minoritystatus> jntuh_college_minoritystatus { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}
