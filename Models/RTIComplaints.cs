using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class RTIComplaints
    {
        public int id { get; set; }
        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint Received")]
        public string complaintReceived { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint given by Registration No.")]
        public string complaintGivenByRegNum { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint given by Name")]
        public string complaintGivenByName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint on Registration No.")]
        public string complaintOnRegNum { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint on Name")]
        public string complaintOnName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Complaint Description")]
        public string complaintDescription { get; set; }

        [Display(Name = "Complaint Supporting Document")]
        public HttpPostedFileBase complaintSupportingDoc { get; set; }

        public string complaintSupportingDocPath { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Action Taken")]
        public string actionsTaken { get; set; }

        [Display(Name = "Action Taken Supporting Document")]
        public HttpPostedFileBase actionTakenSupportingDoc { get; set; }

        public string actionTakenSupportingDocPath { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
    }
}