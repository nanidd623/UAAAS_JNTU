using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeOverallFacultyStudentRatio
    {
        public int id { get; set; }
        public int academicYearId { get; set; }
        public int collegeId { get; set; }

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
        [Display(Name = "A.Y. ")]
        public int approvedIntake1 { get; set; }      

        [Required(ErrorMessage = "Required")]
        [Display(Name = "A.Y. ")]
        public int approvedIntake2 { get; set; }    

        [Required(ErrorMessage = "Required")]
        [Display(Name = "A.Y. ")]
        public int approvedIntake3 { get; set; }  

        [Required(ErrorMessage = "Required")]
        [Display(Name = "A.Y. ")]
        public int approvedIntake4 { get; set; }   

        [Required(ErrorMessage = "Required")]
        [Display(Name = "A.Y. ")]
        public int approvedIntake5 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "A.Y. ")]
        public int approvedIntake6 { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Faculty")]
        public int totalFaculty { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total Ratified Faculty")]
        public int ratifiedFaculty { get; set; }

        [Display(Name = "Status")]
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
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}