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
    
    public partial class jntuh_college_aicteapprovedintake
    {
        public int Id { get; set; }
        public int collegeId { get; set; }
        public int academicyearId { get; set; }
        public int specializationId { get; set; }
        public int aicteintake { get; set; }
        public string aictecollegeid { get; set; }
        public bool declarationstatus { get; set; }
        public string remarks { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<short> isactive { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}
