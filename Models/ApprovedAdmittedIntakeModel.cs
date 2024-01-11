using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ApprovedAdmittedIntakeModel
    {
        [Required(ErrorMessage = "Required")]
        public string[] AcademicYears { get; set; }

        [Required(ErrorMessage = "Required")]
        public string[] Colleges { get; set; }
    }
}