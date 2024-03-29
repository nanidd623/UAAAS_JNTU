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
    
    public partial class jntuh_college_principal_director
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public string type { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string surname { get; set; }
        public string photo { get; set; }
        public int qualificationId { get; set; }
        public System.DateTime dateOfAppointment { get; set; }
        public Nullable<System.DateTime> dateOfResignation { get; set; }
        public System.DateTime dateOfBirth { get; set; }
        public string fax { get; set; }
        public string landline { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public Nullable<int> departmentId { get; set; }
        public Nullable<int> phdId { get; set; }
        public string phdFromUniversity { get; set; }
        public Nullable<int> phdYear { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public bool isRatified { get; set; }
        public Nullable<System.DateTime> ratificationPeriodFrom { get; set; }
        public Nullable<System.DateTime> ratificationPeriodTo { get; set; }
        public string programType { get; set; }
    
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_qualification jntuh_qualification { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_phd_subject jntuh_phd_subject { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}
