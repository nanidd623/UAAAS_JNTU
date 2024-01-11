using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(SpecializationAttribs))]
    public partial class jntuh_specialization
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }
        public string degree { get; set; }
        public string departmentName { get; set; }        
    }

    public class SpecializationAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Specialization Name")]
        public string specializationName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department Name")]
        public int departmentId { get; set; }

        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Description")]
        public string specializationDescription { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual ICollection<jntuh_college_intake_existing> jntuh_college_intake_existing { get; set; }
        public virtual jntuh_department jntuh_department { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }        
    }
}