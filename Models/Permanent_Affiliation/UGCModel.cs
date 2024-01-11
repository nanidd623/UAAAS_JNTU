using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class UGCModel
    {
        public int affiliationTypeId { get; set; }

        public int affiliationId { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int AcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Autonomous from date")]
        public string AffiliationFromDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Autonomous to date")]
        public string AffiliationToDate { get; set; }

        public string affiliationfilepath { get; set; }

        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase affiliationfile { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Duration")]
        public string Duration { get; set; }

    }
}