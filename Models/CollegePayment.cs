using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegePayment
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        
        public System.DateTime paymentDate { get; set; }
        [Required(ErrorMessage = "Required")]
        public string date { get; set; }
        [Required(ErrorMessage = "Required")]
        public int paymentType { get; set; }
        [Required(ErrorMessage = "Required")]
        public string paymentNumber { get; set; }
        [Required(ErrorMessage = "Required")]
        public int paymentStatus { get; set; }
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Invalid, ex: 17350.50")]
        public decimal paymentAmount { get; set; }
        [Required(ErrorMessage = "Required")]
        public string paymentBranch { get; set; }
        [Required(ErrorMessage = "Required")]
        public string paymentLocation { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}