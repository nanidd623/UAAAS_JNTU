using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class CourtCasesModel
    {

        public int CollegeId { get; set; }
        public int CourtCaseId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "WP / SL / other No.")]
        public string WporSlorOtherNo { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year of filing")]
        public int YearofFilling { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Prayer of the Petitioner")]
        public string PrayerofthePetitioner { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "If JNTUH is one of the respondents, mention the position")]
        public int? Respondents { get; set; }

        public string WpPetitionerFilepath { get; set; }

        [Display(Name = "Petitioner Affidavit")]
        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase WpPetitionerFile { get; set; }

        public string OrderCopypath { get; set; }

        [Display(Name = "Order / Judgement Copy")]
        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase OrderCopy { get; set; }

        public string InternOrderCopyPath { get; set; }

        [Display(Name="Interim Order Copy (If any)")]
        public HttpPostedFileBase InternOrderCopy { get; set; }
    }
}