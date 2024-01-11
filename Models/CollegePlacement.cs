using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegePlacement
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department ")]
        public int departmentID { get; set; }

        [Display(Name = "Department")]
        public string department { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Display(Name = "Specialization")]
        public string specialization { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAppeared6 { get; set; }

        //[Display(Name = "Total Students Passed for the A.Y.")]
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPassed6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDistinction6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsFirstClass6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetained6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPlaced6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsAbove10L6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents5to10L6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudents3to5L6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsBelow3L6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsHighEdu6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsPubSec6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsEnt6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<decimal> totalStudentsPlacePer6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> totalStudentsDetCred6 { get; set; }

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
    }

    public class RemedialTeaching
    {
        public int CollegeId { get; set; }
        public int ActivityId { get; set; }
        public bool ActivitySelected { get; set; }
        public string ActivityDocumentPath { get; set; }
        public HttpPostedFileBase ActivityDocument { get; set; }
        public string Remarks { get; set; }
    }
}