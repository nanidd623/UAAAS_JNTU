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
    
    public partial class jntuh_college_experiments
    {
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public Nullable<int> ExperimentId { get; set; }
        public string ExperimentName { get; set; }
        public Nullable<short> ExperimentStatus { get; set; }
        public Nullable<int> ExperimentNO { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> CreatedBY { get; set; }
        public Nullable<System.DateTime> Updatedon { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string LabName { get; set; }
    }
}
