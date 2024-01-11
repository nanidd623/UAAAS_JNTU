using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class BookandJournalsModel
    {
        public int CollegeId { get; set; }

        public int BookandJournalId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int DegreeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of Books")]
        public string NumberOfBooks { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Amount Spent")]
        public string AmountSpent { get; set; }

        public string SupportingDocumentPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase SupportingDocument { get; set; }

        [Display(Name = "Type")]
        public string EssentialType { get; set; }

        public int ActivityId { get; set; }

        public bool ActivitySelected { get; set; }

        public string ActivityDocumentPath { get; set; }

        public HttpPostedFileBase ActivityDocument { get; set; }

        public string Remarks { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of Computers Added")]
        public string NumberOfComputers { get; set; }
    }
}