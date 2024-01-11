using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class InspectionPhase
    {
    }
    [MetadataType(typeof(InspectionPhaseAttribs))]
    public partial class jntuh_inspection_phase
    {
        //leave it empty
    }

    public class InspectionPhaseAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(25, ErrorMessage = "Must be under 25 characters")]
        [Display(Name = "Inspection Phase")]
        public string inspectionPhase { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int academicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}