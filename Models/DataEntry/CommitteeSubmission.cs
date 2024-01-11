using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CommitteeSubmission
    {
        public int id { get; set; }    
        [Required(ErrorMessage = "Required")]
        [Display(Name = "College")]
        public int collegeId { get; set; }
        [Display(Name = "Submitted Date")]
        public Nullable<System.DateTime> submittedDate { get; set; }
        [Display(Name = "Remarks")]
        public string  remarks { get; set; }
        [Display(Name = "Status")]
        public bool isActive { get; set; }      
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> createdon { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Submitted Date")]
        public string  strsubmittedDate { get; set; }
      //  public virtual jntuh_committee_submission jntuh_committee_submission { get; set; }
        public Nullable<int> InspectionPhaseId { get; set; }
    }
}