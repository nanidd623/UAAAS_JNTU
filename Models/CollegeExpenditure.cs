using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeExpenditure
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        [Required(ErrorMessage = "Required")]
        //[RegularExpression(@"^[0-9]\d{0,9}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct expenditure, ex: 9999999999.99")]
        public int expenditureTypeID { get; set; }
        public string expenditure { get; set; }
        [Required(ErrorMessage = "Required")]
        public decimal expenditureAmount { get; set; }
        [Required(ErrorMessage = "Required")]
        public bool expenditureStatus { get; set; }
        [Required(ErrorMessage = "Required")]
        public bool payScaleStatus { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_college_expenditure_type jntuh_college_expenditure_type { get; set; }
    }
}