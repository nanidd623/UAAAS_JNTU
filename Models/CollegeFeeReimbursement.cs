using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeFeeReimbursement
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicYearId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Degree")]
        public int degreeID { get; set; }

        [Display(Name = "Degree")]
        public string degree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Department ")]
        public int departmentID { get; set; }

        [Display(Name = "Department")]
        public string department { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Specialization")]
        public int specializationId { get; set; }

        [Display(Name = "Specialization")]
        public string specialization { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Shift")]
        public int shiftId { get; set; }

        [Display(Name = "Shift")]
        public string shift { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Year in Degree")]
        public int yearInDegreeId { get; set; }

        [Display(Name = "Year in Degree")]
        public string yearInDegree { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Without Re-imbursement Seats")]
        public int seatsWithoutReimbursement { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Fee, ex: 9999999999.99")]
        [Display(Name = "Total Fee(Rs.)")]
        public decimal feeWithoutReimbursement { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "With Re-imbursement Seats")]
        public int seatsWithReimbursement { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Area In Acres, ex: 9999999999.99")]
        [Display(Name = "Total Fee(Rs.)")]
        public decimal feeWithReimbursement { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "NRI Seats")]
        public int NRISeats { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Area In Acres, ex: 9999999999.99")]
        [Display(Name = "Total Fee(Rs.)")]
        public decimal totalNRIFee { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "PIO Seats")]
        public int PIOSeats { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Area In Acres, ex: 9999999999.99")]
        [Display(Name = "Total Fee(Rs.)")]
        public decimal totalPIOFee { get; set; }

        public int? degreeDisplayOrder { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_academic_year jntuh_academic_year { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_specialization jntuh_specialization { get; set; }
        public virtual jntuh_shift jntuh_shift { get; set; }
        public virtual jntuh_year_in_degree jntuh_year_in_degree { get; set; }
    }
}