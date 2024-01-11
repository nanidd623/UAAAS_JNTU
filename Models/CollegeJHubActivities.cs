using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeJHubActivities
    {
        public int sno { get; set; }
        public int activityid { get; set; }
        public string activityDesc { get; set; }
        public int collegeid { get; set; }
        public int academicyear { get; set; }
        [Required(ErrorMessage = "Activity Status Required")]
        public bool activitystatus { get; set; }
        public HttpPostedFileBase activitydoc { get; set; }
        public string activitydocpath { get; set; }
        public string supportingdocuments { get; set; }
        [StringLength(2500, MinimumLength = 1, ErrorMessage = "Must be 1-2500 characters long.")]
        public string remarks { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}