using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class PendingSubmission
    {
        public int id { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int collegeId { get; set; }
    }
}