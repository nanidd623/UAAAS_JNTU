using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class FacultyStudentRatio
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Display(Name="Degree")]
        [Required(ErrorMessage="Required")]
        public int degreeId { get; set; }

        [Display(Name = "Total Student Strength")]
        [Required(ErrorMessage = "Required")]
        public int totalIntake { get; set; }

        [Display(Name = "Total Faculty")]
        [Required(ErrorMessage = "Required")]
        public int totalFaculty { get; set; }

        public string degree { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_degree jntuh_degree { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}