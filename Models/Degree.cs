using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(DegreeAttribs))]
    public partial class jntuh_degree
    {
        //leave it empty
    }

    public class DegreeAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Must be under 25 characters")]
        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degreen Type")]
        public int degreeTypeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degreen Duration")]
        public decimal degreeDuration { get; set; }

        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Description")]
        public string degreeDescription { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_degree_type jntuh_degree_type { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_department> jntuh_department { get; set; }
    }
}