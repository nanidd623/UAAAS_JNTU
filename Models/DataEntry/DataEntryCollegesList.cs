using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class DataEntryCollegesList
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "User")]
        public int userId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "College")]
        public int collegeId { get; set; }
        [Display(Name="Status")]
        public bool isActive { get; set; }
        [Display(Name = "Work Status")]
        public bool isCompleted { get; set; }
        public Nullable <int> createdBy { get; set; }
        public Nullable <System.DateTime> createdon { get; set; }
        public string collegeName { get; set; }
        public string userName { get; set; }
        
       // public virtual jntuh_dataentry_allotment jntuh_dataentry_allotment { get; set; }
    }
}