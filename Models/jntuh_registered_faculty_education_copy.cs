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
    
    public partial class jntuh_registered_faculty_education_copy
    {
        public int id { get; set; }
        public int facultyId { get; set; }
        public int educationId { get; set; }
        public string courseStudied { get; set; }
        public string specialization { get; set; }
        public int passedYear { get; set; }
        public Nullable<decimal> marksPercentage { get; set; }
        public Nullable<int> division { get; set; }
        public string boardOrUniversity { get; set; }
        public string placeOfEducation { get; set; }
        public string certificate { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
    }
}
