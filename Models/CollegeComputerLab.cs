using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace UAAAS.Models
{
    public class CollegeComputerLab
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Printers Availability")]
        public bool printersAvailability { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Working Hours Of Computer Lab (timings)  From :")]
        [RegularExpression(@"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter correct Timinngs, ex: 08:30")]
        public string workingHoursofComputerLabFrom { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Working Hours Of Computer Lab (timings) To :")]
        [RegularExpression(@"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter correct Timinngs, ex: 08:30")]
        public string workingHoursofComputerLabTo { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Internet Accessibility (timings) From :")]
        [RegularExpression(@"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter correct Timinngs, ex: 08:30")]
        public string internetAccessibilityFrom { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Internet Accessibility (timings) To :")]
        [RegularExpression(@"^(?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Please enter correct Timinngs, ex: 08:30")]
        public string internetAccessibilityTo { get; set; }


        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}