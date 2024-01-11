using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class UploadSanctionedDetails
    {
        public int SupportingDocsId { get; set; }

        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int DegreeId { get; set; }

        public string JNTUHSanctionedDocumentPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Affiliation Letter issued by JNTUH")]
        public HttpPostedFileBase JNTUHSanctionedDocument { get; set; }

        public string PCISanctionedDocumentPath { get; set; }

        [Display(Name = "Documents indicating PCI Approval (EC Minutes / Decision Letter)")]
        public HttpPostedFileBase PCISanctionedDocument { get; set; }

        public string AICTESanctionedDocumentPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "EOA issued by AICTE")]
        public HttpPostedFileBase AICTESanctionedDocument { get; set; }
    }
}