using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Governingmeeting
    {
        public int Id { get; set; }
        public int Collegeid { get; set; }
        public int Academicyearid { get; set; }
        [Required(ErrorMessage = "Date can't be Empty")]
        public string Dateofmetting { get; set; }
        [Required(ErrorMessage = "File can't be Empty")]
        public HttpPostedFileBase Minutescopy { get; set; }
        public string Remarks { get; set; }
        public string Minutescopyfile { get; set; }
    }
}