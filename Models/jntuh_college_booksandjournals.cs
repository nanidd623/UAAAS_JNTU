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
    
    public partial class jntuh_college_booksandjournals
    {
        public int id { get; set; }
        public int collegeid { get; set; }
        public int academicyearId { get; set; }
        public int degreeid { get; set; }
        public int numberofbooks { get; set; }
        public decimal amountspent { get; set; }
        public string supporingdocument { get; set; }
        public string remarks { get; set; }
        public Nullable<int> noofcomputers { get; set; }
        public Nullable<int> essentialtype { get; set; }
        public Nullable<bool> isactive { get; set; }
        public int createdby { get; set; }
        public System.DateTime createdon { get; set; }
        public Nullable<int> updatedby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
    
        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
    }
}