using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(DistrictAttribs))]
    public partial class jntuh_district
    {
        //leave it empty
    }

    public class DistrictAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "State")]
        public int stateId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "District Name")]
        public string districtName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual ICollection<jntuh_address> jntuh_address { get; set; }
        //public virtual ICollection<jntuh_affiliation_requests> jntuh_affiliation_requests { get; set; }
        public virtual jntuh_state jntuh_state { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}