using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class college_staticdata_modifications
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int formno { get; set; }
        public string FormName{ get; set; }
        public int AcademicYearid { get; set; }
        public HttpPostedFileBase staticdatafile { get; set; }
        public string staticdatafilename { get; set; }
        public string justification { get; set; }
    }
}