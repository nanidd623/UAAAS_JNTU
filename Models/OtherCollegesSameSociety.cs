using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class OtherCollegesSameSociety
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "College Name")]
        public string collegeName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Must be under 500 characters")]
        [Display(Name = "College Location")]
        public string collegeLocation { get; set; }       

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Affiliated University")]
        public int affiliatedUniversityId { get; set; }

        [Display(Name = "University Name (if other)")]
        public string otherUniversityName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status")]
        public bool isActive { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public string universityName { get; set; }
        public int rowNumber { get; set; }

        [RegularExpression(@"\d{4}", ErrorMessage = "Must be 4 digits, ex: 2014")]
        [Display(Name = "Year Of Establishment(YYYY)")]
        public Nullable<int> yearOfEstablishment { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_university jntuh_university { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}