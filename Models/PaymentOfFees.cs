using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class PaymentOfFees
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int FeeTypeID { get; set; }

        public string FeeType { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal paidAmount { get; set; }

        [Required(ErrorMessage = "Required")]
        public string duesAmount { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_college_paymentoffee_type jntuh_college_paymentoffee_type { get; set; }
    }
}