using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace UAAAS.Models
{
    public class CollegeInternetBandwidth
    {
        public IEnumerable<InternetBandwidthDetails> internetBandwidth { get; set; }

    }
    public class InternetBandwidthDetails
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int degreeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,2}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Internet Speed, ex: 999.99")]
        [Display(Name = "Available Internet Speed")]
        public decimal availableInternetSpeed { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Display(Name = "Degree")]
        public int totalIntake { get; set; }

        public int specializationId { get; set; }

        public virtual jntuh_degree jntuh_degree { get; set; }

        public HttpPostedFileBase bandwidthdoc { get; set; }
        public string bandwidthdocpath { get; set; }
    }
}