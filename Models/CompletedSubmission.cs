using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CompletedSubmission
    {
        public int id { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int collegeId { get; set; }
        public string district { get; set; }
        public Nullable<System.DateTime> submittedDate { get; set; }
        public string submitdate { get; set; }
        public string Collegetype { get; set; }
    }
}