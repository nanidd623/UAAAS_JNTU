using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(CollegeAffiliationTypeAttribs))]
    public partial class jntuh_college_affiliation_type
    {
        //leave it empty
    }

    public class CollegeAffiliationTypeAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "College Affiliation Type")]
        public string collegeAffiliationType { get; set; }

        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "Description")]
        public string collegeAffiliationTypeDescription { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name="Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_college> jntuh_college { get; set; }
    }
}