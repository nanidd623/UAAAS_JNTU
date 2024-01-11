using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeAcademicPerformance
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        public int academicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

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
        public int yearInDegreeId1 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int appearedStudents1 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int passedStudents1 { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal passPercentage1 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int yearInDegreeId2 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int appearedStudents2 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int passedStudents2 { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal passPercentage2 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int yearInDegreeId3 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int appearedStudents3 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int passedStudents3 { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal passPercentage3 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int yearInDegreeId4 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int appearedStudents4 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int passedStudents4 { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal passPercentage4 { get; set; }

        public bool isActive { get; set; }

        [Display(Name = "Degree")]
        public string Degree { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Specialization")]
        public string Specialization { get; set; }

        [Display(Name = "Shift")]
        public string Shift { get; set; }

        public int? degreeDisplayOrder { get; set; }

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
        public virtual jntuh_year_in_degree jntuh_year_in_degree { get; set; }
    }
}