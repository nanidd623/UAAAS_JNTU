using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(DepartmentAttribs))]
    public partial class jntuh_department
    {
        //leave it empty
    }

    public class DepartmentAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Department Name")]
        public string departmentName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }

        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Description")]
        public string departmentDescription { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual ICollection<jntuh_college_principal_director> jntuh_college_principal_director { get; set; }
        public virtual jntuh_degree jntuh_degree { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_specialization> jntuh_specialization { get; set; }
    }
}