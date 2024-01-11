using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(CourseAffiliationStatusAttribs))]
    public partial class jntuh_course_affiliation_status
    {
        //leave it empty
    }

    public class CourseAffiliationStatusAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(1, ErrorMessage = "Must be under 1 characters")]
        [Display(Name = "Course Affiliation Status Code")]
        public string courseAffiliationStatusCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(100, ErrorMessage = "Must be under 100 characters")]
        [Display(Name = "Description")]
        public string courseAffiliationStatusDescription { get; set; }

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