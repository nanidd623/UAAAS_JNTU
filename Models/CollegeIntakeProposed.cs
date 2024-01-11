using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeIntakeProposed
    {
        public int id { get; set; }

        public int academicYearId { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

        [Display(Name = "Degree")]
        public string Degree { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department ")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Shift")]
        public int shiftId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Program Status")]
        public int courseAffiliationStatusCodeId { get; set; }

        [Display(Name = "Sanctioned Intake for the A.Y.")]
        [RegularExpression(@"([0-9]+)", ErrorMessage = "Must be a Number.")]
        public int ExistingIntake { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Proposed Intake for the A.Y.")]
        [RegularExpression(@"([0-9]+)", ErrorMessage = "Must be a Number.")]
        public int proposedIntake { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [Display(Name = "Shift")]
        public string Shift { get; set; }

        [Display(Name = "Program Status")]
        public string CourseAffiliationStatusCode { get; set; }

        public string AcademicYear { get; set; }

        public int? degreeDisplayOrder { get; set; }

        [Required(ErrorMessage = "Required")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual jntuh_course_affiliation_status jntuh_course_affiliation_status { get; set; }


    }

}