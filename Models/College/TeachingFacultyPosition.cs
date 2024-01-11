using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class TeachingFacultyPosition
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage="Required")]
        public int degreeId { get; set; }

        public string degreeName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int departmentId { get; set; }

        public string departmentName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int specializationId { get; set; }

        public string specializationName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int shiftId { get; set; }

        public string shiftName { get; set; }

        [Required(ErrorMessage = "Required")]
        public int intake { get; set; }

        [Required(ErrorMessage = "Required")]
        public int professors { get; set; }

        [Required(ErrorMessage = "Required")]
        public int assocProfessors { get; set; }

        [Required(ErrorMessage = "Required")]
        public int asstProfessors { get; set; }

        [Required(ErrorMessage = "Required")]
        public int ratified { get; set; }

        [Required(ErrorMessage = "Required")]
        public string facultyStudentRatio { get; set; }

        public int? degreeDisplayOrder { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
    }
}