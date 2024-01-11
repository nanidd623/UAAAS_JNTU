using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ClusterScheduleEmails
    {
        public int id { get; set; }
        [Required(ErrorMessage="Required")]
        [Display(Name="Cluster")]
        public string cluster { get; set; }

        public string newcluster { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email Subject")]
        public string emailSubject { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email Body")]
        public string emailBody { get; set; }  
     
        public string emailTo { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "SMS Text")]
        public string smsText { get; set; }

        public string smsTo { get; set; }
        public System.DateTime createdOn { get; set; }
        public int createdBy { get; set; }      

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
    }
}