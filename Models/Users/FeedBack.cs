using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class FeedBack
    {
        public int id { get; set; }

        [Required(ErrorMessage="Required")]
        [Display(Name = "College Name")]
        [StringLength(10, ErrorMessage = "Must be 10 Charecters")]
        public string college { get; set; }

        [Required(ErrorMessage="Required")]
        [Display(Name = "Stack Holder Type")]
        public int stackholderId { get; set; }

        public string feedbackType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Subject")]
        [StringLength(250, ErrorMessage = "Must be 250 Charecters")]
        public string subject { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Full Name")]
        [StringLength(250, ErrorMessage = "Must be 250 Charecters")]
        public string fullName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Phone Number")]
        [StringLength(25, ErrorMessage = "Must be 25 Charecters")]
        public string phoneNumber { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email Address")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        [StringLength(250, ErrorMessage = "Must be 250 Charecters")]
        public string emailAddress { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Comments")]
        [StringLength(2000, ErrorMessage = "Must be 2000 Charecters")]
        public string comments { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }

       // public virtual jntuh_stackholder_type jntuh_stackholder_type { get; set; }
    }
}