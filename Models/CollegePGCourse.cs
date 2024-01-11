using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegePGCourse
    {
        public int id { get; set; }
        public Nullable<int> collegeId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> degreeId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> departmentId { get; set; }
        [Required(ErrorMessage="Required")]
        public Nullable<int> specializationId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> shiftId { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> intake { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> professors { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> associateProfessors { get; set; }
        [Required(ErrorMessage = "Required")]
        public Nullable<int> assistantProfessors { get; set; }
        [Required(ErrorMessage = "Required")]
        [StringLength(10)]
        public string UGFacultyStudentRatio { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public string collegename { get; set; }

        public string type { get; set; }
        public string  degree { get; set; }
        public int instituteEstablishedYear { get; set; }

        public List<CollegePGCourseFaculty> PGFaculty { get; set; }
      

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual ICollection<jntuh_college_pgcourse_faculty> jntuh_college_pgcourse_faculty { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }       

        public virtual jntuh_degree jntuh_degree { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
    }

    public class CollegePGCourseFaculty
    {
        public int id { get; set; }       
        public Nullable<int> courseId { get; set; }
         [Required(ErrorMessage = "Required")]
        public string facultyName { get; set; }
         [Required(ErrorMessage = "Required")]
        public string designation { get; set; }
         [Required(ErrorMessage = "Required")]
        public string UG { get; set; }
        public string PG { get; set; }
        public string Phd { get; set; }
        public string UGSpecialization { get; set; }
        public string PGSpecialization { get; set; }
        public string PhdSpecialization { get; set; }
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college_pgcourses jntuh_college_pgcourses { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }

    public class PGCourseSpecializationandFacultyList
    {
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int instituteEstablishedYear { get; set; }
        public List<CollegePGCourse> PGCourse { get; set; }
        public List<CollegePGCourseFaculty> PGFaculty { get; set; }

    }
}