using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class MinorityModel
    {
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status From Date")]
        public string StatusFromDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Status To Date")]
        public string StatusToDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Supporting Document")]
        public string StatusFile { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College Status")]
        public string CollegeStatus { get; set; }
    }
}