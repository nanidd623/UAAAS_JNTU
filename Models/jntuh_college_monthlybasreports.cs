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
    
    public partial class jntuh_college_monthlybasreports
    {
        public int id { get; set; }
        public int collegeid { get; set; }
        public int academicyearid { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public string path { get; set; }
        public Nullable<bool> islatest { get; set; }
        public Nullable<bool> isactive { get; set; }
        public Nullable<System.DateTime> createdon { get; set; }
        public Nullable<int> createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
    }
}
