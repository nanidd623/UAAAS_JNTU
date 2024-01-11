using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegeEssentialReq
    {
        public int sno { get; set; }
        public int essentialid { get; set; }
        public string essentialDesc { get; set; }
        public int collegeid { get; set; }
        public int academicyear { get; set; }
        [Required(ErrorMessage = "Essential Status Required")]
        public bool essentialstatus { get; set; }
        public HttpPostedFileBase essentialdoc { get; set; }
        public string essentialdocpath { get; set; }
        public string supportingdocuments { get; set; }
        public string remarks { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}