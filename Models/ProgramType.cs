using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(ProgramTypeAttribs))]
    public partial class jntuh_program_type
    {
        //leave it empty
    }

    public class ProgramTypeAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Must be under 25 characters")]
        [Display(Name = "Program Type")]
        public string programType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual ICollection<jntuh_area_requirement> jntuh_area_requirement { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}