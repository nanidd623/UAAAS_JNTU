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
    
    public partial class jntuh_ffc_committee
    {
        public int id { get; set; }
        public Nullable<int> scheduleID { get; set; }
        public Nullable<int> auditorID { get; set; }
        public Nullable<int> isConvenor { get; set; }
        public Nullable<int> memberOrder { get; set; }
    
        public virtual jntuh_ffc_auditor jntuh_ffc_auditor { get; set; }
        public virtual jntuh_ffc_schedule jntuh_ffc_schedule { get; set; }
    }
}
