using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class OperationalFunds
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Bank Name")]
        public string bankName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Bank Branch")]
        public string bankBranch { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Bank Address")]
        [StringLength(500, ErrorMessage = "Must be under 250 characters")]
        public string bankAddress { get; set; }

        [Display(Name = "Cash Balance")]
        public decimal cashBalance { get; set; }

        [Display(Name = "FDR")]
        public Nullable<decimal> FDR { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "FDR Receipt")]
        public HttpPostedFileBase  FDRReceipt{ get; set; }
        public string FDRReceiptview { get; set; }

        public Nullable<decimal> total { get; set; }

        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }

        [Display(Name = "Account Number")]
        [StringLength(500, ErrorMessage = "Must be under 20 characters")]
        public string bankAccountNumber { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}