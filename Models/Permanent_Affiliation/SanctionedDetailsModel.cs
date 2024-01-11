using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class SanctionedDetailsModel
    {
        public int CollegeId { get; set; }

        public int IntakesId { get; set; }

        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int DegreeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Sanctioned Intake by JNTUH")]
        public int? JNTUHSanctioned { get; set; }

        [Display(Name = "Approved Intake by PCI")]
        public int? PCISanctioned { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approved Intake by AICTE")]
        public int? AICTESanctioned { get; set; }

        [Display(Name = "Year of Establishment of the Institution")]
        public int YearOfEstablishment { get; set; }
    }
}