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
    
    public partial class jntuh_university
    {
        public jntuh_university()
        {
            this.jntuh_college_other_university_courses = new HashSet<jntuh_college_other_university_courses>();
            this.jntuh_society_other_colleges = new HashSet<jntuh_society_other_colleges>();
            this.jntuh_society_other_locations_colleges = new HashSet<jntuh_society_other_locations_colleges>();
        }
    
        public int id { get; set; }
        public string universityName { get; set; }
        public int universityType { get; set; }
        public string address { get; set; }
        public string state { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual ICollection<jntuh_college_other_university_courses> jntuh_college_other_university_courses { get; set; }
        public virtual ICollection<jntuh_society_other_colleges> jntuh_society_other_colleges { get; set; }
        public virtual ICollection<jntuh_society_other_locations_colleges> jntuh_society_other_locations_colleges { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}
