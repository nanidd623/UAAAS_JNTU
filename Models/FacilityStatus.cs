using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(FacilityStatusAttribs))]
    public partial class jntuh_facility_status
    {
        //leave it empty
    }

    public class FacilityStatusAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(15, ErrorMessage = "Must be under 15 characters")]
        [Display(Name = "Facility Status")]
        public string facilityStatus { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual ICollection<jntuh_college_land> jntuh_college_land { get; set; }
        public virtual ICollection<jntuh_college_land> jntuh_college_land1 { get; set; }
    }
}