using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeExaminationBranchstaffDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "Name of the Staff")]
        public string staffName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Maximum 100 characters")]
        [Display(Name = "Designation")]
        public string staffDesignation { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Teaching/Non-Teaching")]
        public bool isTeachingStaff { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}