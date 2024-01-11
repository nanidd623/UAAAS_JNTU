using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegePrinters
    {
        public IEnumerable<CollegePrinterDetails> CollegePrinter { get; set; }
    }
    
    public class CollegePrinterDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeId { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available Computers")]
        public int availableComputers { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Available Printers")]
        public int availablePrinters { get; set; }

        public virtual jntuh_degree jntuh_degree { get; set; }
    }
}