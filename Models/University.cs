using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    [MetadataType(typeof(UniversityAttribs))]
    public partial class jntuh_university
    {
        //leave it empty
    }

    public class UniversityAttribs
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(250, ErrorMessage = "Must be under 250 characters")]
        [Display(Name = "University Name")]
        public string universityName { get; set; }

        //[Required(ErrorMessage = "Required")]
        //[StringLength(10, ErrorMessage = "Must be under 10 characters")]
        //[Display(Name = "University Short Code")]
        //public string universityShortCode { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual ICollection<jntuh_college_other_university_courses> jntuh_college_other_university_courses { get; set; }
        public virtual ICollection<jntuh_society_other_colleges> jntuh_society_other_colleges { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}