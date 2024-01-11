using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ComplianceQuestionnaire
    {
        public int id { get; set; }
        public int complainceid { get; set; }
        public int collegeid { get; set; }
        public Nullable<int> academicyearid { get; set; }

        [Required(ErrorMessage = "Required")]
        public string complaincestatus { get; set; }
        public string complaincedescription { get; set; }
        public int complaincetype { get; set; }

        [Required(ErrorMessage = "Required")]
        public string remarks { get; set; }
        public string remarks1 { get; set; }
        public HttpPostedFileBase circulardoc { get; set; }
        public string supportingdocuments { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}