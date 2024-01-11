using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models.Permanent_Affiliation
{
    public class FacultyInformationModel
    {
        public int FacultyInfoId { get; set; }

        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int DegreeID { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Staff")]
        public string Staff { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Students")]
        public string Students { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total No. of Faculty on Role")]
        public string FacultyOnRole { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Total No. of Faculty Terminated / Resigned")]
        public string FacultyTerminatedResigned { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Pay scale implemented")]
        public int PayscaleSelected { get; set; }

        [Display(Name = "Assistant Professor Pay scale")]
        public string AssiProfPayScale { get; set; }

        public string AssiProfPay { get; set; }

        [Display(Name = "Associate Professor Pay scale")]
        public string AssoProfPayScale { get; set; }

        public string AssoProfPay { get; set; }

        [Display(Name = "Professor Pay scale")]
        public string ProfPayScale { get; set; }

        public string ProfPay { get; set; }

        [Display(Name = "Retention Percentage")]
        public string RetentionPercentage { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Oppurtunities Provided to faculty")]
        public int Oppurtunities { get; set; }
    }
}