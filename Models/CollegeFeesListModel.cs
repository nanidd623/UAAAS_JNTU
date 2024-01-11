using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeFeesListModel
    {
        public int PaymentOfFeeId { get; set; }

        public int CollegeId { get;set; }

        public string CollegeCode { get; set; }

        public string CollegeName { get; set; }

        [Required(ErrorMessage="Required")]
        [Display(Name="Affiliation Fee")]
        public string AffiliationFee { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Common Service Fee")]
        public string CommonServiceFee { get; set; }

        public string TotalFee { get; set; }

        public int academicYearId { get; set; }

        public string year { get; set; }

        public string GstAmount { get; set; }
    }

    public class CollegesFeesListExport
    {
        public string CollegeCode { get; set; }

        public string CollegeName { get; set; }

        public string AcademicYear { get; set; }

        public string AffiliationFee { get; set; }

        public string CommonServiceFee { get; set; }

        public string GstAmount { get; set; }

        public string TotalFee { get; set; }
    }
}