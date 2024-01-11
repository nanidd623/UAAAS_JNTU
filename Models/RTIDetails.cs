using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class RTIDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Designation in the committee")]
        public int memberDesignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Name")]
        public string memberName { get; set; }

        public string designationName { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Actual Designation")]
        public Nullable<int> actualDesignationId { get; set; }
        public string actualDesignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Registration Number")]
        public string registrationNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Must be 6-100 characters long.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Invalid email")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(10, ErrorMessage = "Maximum 10 characters")]
        [RegularExpression(@"\d{10}", ErrorMessage = "Invalid mobile")]
        [Display(Name = "Mobile No")]
        public string Mobile { get; set; }


        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}