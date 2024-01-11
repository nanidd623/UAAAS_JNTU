using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(DesignationAttribs))]
    public partial class jntuh_designation
    {
        //leave it empty
    }

    public class DesignationAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(15, ErrorMessage = "Must be under 15 characters")]
        [Display(Name = "Designation")]
        public string designation { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "Description")]
        public string designationDescription { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}