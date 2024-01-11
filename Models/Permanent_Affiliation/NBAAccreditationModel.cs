using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class NBAAccreditationModel
    {
        public long Sno { get; set; }
        public int CollegeId { get; set; }

        [Required(ErrorMessage="Required")]
        [Display(Name = "Academic Year")]
        public int AcademicYear { get; set; }

        [Display(Name = "Degree")]
        [Required(ErrorMessage = "Required")]
        public int DegreeId { get; set; }

        [Display(Name = "Department")]
        [Required(ErrorMessage = "Required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name="NBA From Date")]
        public string NbaFrom { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "NBA To Date")]
        public string NbaTo { get; set; }

        public string NbaApprovalLetterPath { get; set; }

        [Display(Name = "NBA Approval Letter")]
        [Required(ErrorMessage = "Required")]
        public HttpPostedFileBase NbaApprovalLetter { get; set; }
    }
}