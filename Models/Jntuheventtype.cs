using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Jntuheventtype
    {

        public int id { get; set; }  
        public int Academicyearid { get; set; }
        public int Collegeid { get; set; }
        [Required(ErrorMessage = "*")]
        public int eventid { get; set; }
        public string eventtype  { get; set; }
        [Required(ErrorMessage = "*")]
        public string fromdate { get; set; }
        [Required(ErrorMessage = "*")]
        public string todate { get; set; }
        public string remarks { get; set; }
        
        public HttpPostedFileBase SupportingDocument { get; set; }
        public string SupportingDocumentfile { get; set; }
        public string supportingdocuments  { get; set; }

        

        public bool Isactive { get; set; }
    }
}