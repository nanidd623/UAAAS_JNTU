using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class AuditorReport
    {
        public string name { get; set; }

        public string prefferedDesignation { get; set; }

        public string auditorPlace { get; set; }

        public string collegeName { get; set; }

        public string collegeCode { get; set; }

        public string inspectionDate { get; set; }

        public string alternateInspectionDate { get; set; }
    }
}