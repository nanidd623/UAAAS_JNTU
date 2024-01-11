using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class LandRegistrationDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        public string landRegistraionDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,2}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Acres, ex: 999.99")]
        [Display(Name = "Area in Acres")]
        public decimal landAreaInAcres { get; set; }

        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Document Number")]
        public string landDocumentNumber { get; set; }

        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Survey Number")]
        public string landSurveyNumber { get; set; }

        [StringLength(50, ErrorMessage = "Must be under 50 characters")]
        [Display(Name = "Location/Village")]
        public string landLocation { get; set; }

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