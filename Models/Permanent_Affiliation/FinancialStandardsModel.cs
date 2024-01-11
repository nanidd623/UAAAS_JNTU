using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class FinancialStandardsModel
    {
        public int FinStandId { get; set; }

        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int AcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int Degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of Students")]
        public string NumberOfStudents { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Sanctioned Amount (In Lakhs.)")]
        public string TotalAmount { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Supporting Document")]
        public HttpPostedFileBase ScholarshipDocument { get; set; }

        public string ScholarshipDocumentPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Audited Document")]
        public HttpPostedFileBase AuditDocument { get; set; }

        public string AuditDocumentPath { get; set; }
    }
}