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
    
    public partial class jntuh_registered_faculty_subjectstaught
    {
        public int Id { get; set; }
        public int facultyId { get; set; }
        public string facultysubjectsTaught { get; set; }
        public Nullable<int> NoOfTimesSubjectTaught { get; set; }
        public string remarks { get; set; }
        public Nullable<bool> IsAcitve { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    
        public virtual jntuh_registered_faculty jntuh_registered_faculty { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}