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
    
    public partial class jntuh_appeal_scmproceedingrequest_addfaculty
    {
        public int Id { get; set; }
        public int ScmProceedingId { get; set; }
        public string RegistrationNumber { get; set; }
        public Nullable<bool> OtresFacultyMovingStatus { get; set; }
        public int Createdby { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> Updatedby { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<bool> Isactive { get; set; }
        public Nullable<bool> IsApproved { get; set; }
        public Nullable<bool> SecondLevelVerification { get; set; }
        public Nullable<bool> ThirdLevelVerification { get; set; }
        public string PreviousCollegeId { get; set; }
        public Nullable<int> FacultyType { get; set; }
        public string DeactiviationReason { get; set; }
        public string SecondLevelDeactiviationReason { get; set; }
        public string ThirdLevelDeactiviationReason { get; set; }
        public Nullable<bool> isPayment { get; set; }
    }
}
