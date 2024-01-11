using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class DeficiencyReport
    {
        public int id { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string address { get; set; }
        public string intake { get; set; }

    }
    public class CollegeAddressDetails
    {
        public int id { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string address { get; set; }
    }

   

  

}