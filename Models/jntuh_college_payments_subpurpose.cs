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
    
    public partial class jntuh_college_payments_subpurpose
    {
        public jntuh_college_payments_subpurpose()
        {
            this.jntuh_college_dd_payments = new HashSet<jntuh_college_dd_payments>();
        }
    
        public int Id { get; set; }
        public string Sub_PurposeType { get; set; }
        public Nullable<int> FeeTypeId { get; set; }
        public Nullable<int> Sub_PurposeId { get; set; }
        public string Sub_PurposeDescription { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    
        public virtual ICollection<jntuh_college_dd_payments> jntuh_college_dd_payments { get; set; }
        public virtual jntuh_college_paymentoffee_type jntuh_college_paymentoffee_type { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}