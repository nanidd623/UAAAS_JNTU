using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class ScmDashboard
    {
        public string Fscmfromdate { get; set; }
        public string Fscmtodate { get; set; }

        public int Frequestcollegecount { get; set; }
        public int Frequestsubmitcollegecount { get; set; }
        public int Frequestnotsubmitcollegecount { get; set; }
        public int Prequestcollegecount { get; set; }
        public int Prequestsubmitcollegecount { get; set; }

        public int submitfacultycount { get; set; }
        public int notsubmitfacultycount { get; set; }

        public int submitprincipalcount { get; set; }
        public int notsubmitprincipalcount { get; set; }
    }
}