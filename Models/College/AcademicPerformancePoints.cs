using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class AcademicPerformancePoints
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        public int typeId1 { get; set; }
        public int typeId2 { get; set; }
        public int typeId3 { get; set; }
        public int typeId4 { get; set; }
        public int typeId5 { get; set; }
        public int typeId6 { get; set; }
        public int typeId7 { get; set; }
        public int typeId8 { get; set; }
        public int typeId9 { get; set; }
        public int typeId10 { get; set; }

        public int allotedPoints1 { get; set; }
        public int allotedPoints2 { get; set; }
        public int allotedPoints3 { get; set; }
        public int allotedPoints4 { get; set; }
        public int allotedPoints5 { get; set; }
        public int allotedPoints6 { get; set; }
        public int allotedPoints7 { get; set; }
        public int allotedPoints8 { get; set; }
        public int allotedPoints9 { get; set; }
        public int allotedPoints10 { get; set; }

        [Required(ErrorMessage="Required")]
        public int obtainedPoints1 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints2 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints3 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints4 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints5 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints6 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints7 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints8 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints9 { get; set; }
        [Required(ErrorMessage = "Required")]
        public int obtainedPoints10 { get; set; }

        public int totalAllotedPoints { get; set; }

        public int totalObtainedPoints { get; set; }

    }
}