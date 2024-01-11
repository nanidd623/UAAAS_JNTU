using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FacultyDeactivationReason
    {
        public int id { get; set; }
        public string reason { get; set; }
        public bool selected { get; set; }
    }
}