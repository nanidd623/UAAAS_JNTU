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
    
    public partial class jntuh_college_computer_lab
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public bool printersAvailability { get; set; }
        public System.TimeSpan labWorkingHoursFrom { get; set; }
        public System.TimeSpan labWorkingHoursTo { get; set; }
        public System.TimeSpan internetAccessibilityFrom { get; set; }
        public System.TimeSpan internetAccessibilityTo { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}
