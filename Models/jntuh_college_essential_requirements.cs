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
    
    public partial class jntuh_college_essential_requirements
    {
        public int sno { get; set; }
        public int essentialid { get; set; }
        public int collegeid { get; set; }
        public int acadamicyearid { get; set; }
        public bool essentialstatus { get; set; }
        public string supportintdocument { get; set; }
        public string remarks { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}