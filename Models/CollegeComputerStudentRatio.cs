using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeComputerStudentRatio
    {
        public IEnumerable<ComputerStudentRatioDetails> computerStudentRatio { get; set; }
        
    }
    public class ComputerStudentRatioDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }

        public int academicYearId { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Display(Name = "Degree")]
        public int totalIntake { get; set; }
        
        public int specializationId { get; set; }
      
        public string MacAddressList { get; set; }
        public string PreviousMacAddresspath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available computers")]
        public int availableComputers { get; set; }        
        public virtual jntuh_degree jntuh_degree { get; set; }
    }

}