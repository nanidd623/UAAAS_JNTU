using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeLegalSystemSoftwareAndLegalApplicationSoftware
    {
        public IEnumerable<CollegeLegalSoftwarDetails> CollegeLegalSoftware { get; set; }
    }
    public class CollegeLegalSoftwarDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available System Software")]
        public int availableSystemSoftware { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available Application Software")]
        public int availableApplicationSoftware { get; set; }

        public virtual jntuh_degree jntuh_degree { get; set; }
    }
}