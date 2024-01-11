using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class OtherCourse
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "Course Name")]
        public string courseName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Affiliated University")]
        public int affiliatedUniversityId { get; set; }

        [Display(Name = "University Name (if other)")]
        public string otherUniversityName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public string universityName { get; set; }
        public int rowNumber { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_university jntuh_university { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}