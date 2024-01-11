using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(AcademicYearAttribs))]
    public partial class jntuh_academic_year
    {
        //leave it empty
    }

    public class AcademicYearAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(7, ErrorMessage = "Must be under 7 characters")]
        [Display(Name = "Academic Year")]
        public string academicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Actual Year")]
        public int actualYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Is Present Academic Year")]
        public bool isPresentAcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_college_intake_existing> jntuh_college_intake_existing { get; set; }
        public virtual ICollection<jntuh_inspection_phase> jntuh_inspection_phase { get; set; }
    }
}