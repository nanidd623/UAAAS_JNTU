using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Laboratories 
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Shift")]
        public int shiftId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year")]
        public int yearInDegreeId { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Name of the Laboratory")]
        public string labName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "No. of Experiments")]
        public int totalExperiments { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Floor Area(in Sqm)")]
        public decimal labFloorArea { get; set; }

        public string specializationName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int departmentId { get; set; }

        public string department { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }
        public string degree { get; set; }
        public string shiftName { get; set; }
        public string year { get; set; }
        public int? degreeDisplayOrder { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Is lab space / equipment shared")]
        public bool IsShared { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual jntuh_year_in_degree jntuh_year_in_degree { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_degree jntuh_degree { get; set; }
    }
}