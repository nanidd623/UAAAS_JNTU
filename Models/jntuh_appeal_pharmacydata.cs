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
    
    public partial class jntuh_appeal_pharmacydata
    {
        public int Sno { get; set; }
        public string CollegeCode { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public Nullable<int> TotalIntake { get; set; }
        public Nullable<int> ProposedIntake { get; set; }
        public Nullable<int> NoOfFacultyRequired { get; set; }
        public Nullable<int> NoOfFacultyAvilabli { get; set; }
        public string PharmacySpecialization { get; set; }
        public Nullable<int> SpecializationWiseRequiredFaculty { get; set; }
        public Nullable<int> SpecializationWiseAvilableFaculty { get; set; }
        public string Deficiency { get; set; }
        public Nullable<int> PhDFaculty { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
