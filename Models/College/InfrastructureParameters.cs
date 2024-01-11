using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class InfrastructureParameters
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        public int InfrastructureId1 { get; set; }

        public int InfrastructureId2 { get; set; }

        public int InfrastructureId3 { get; set; }

        public int InfrastructureId4 { get; set; }

        public int InfrastructureId5 { get; set; }

        public int InfrastructureId6 { get; set; }

        public int InfrastructureId7 { get; set; }

        public int InfrastructureId8 { get; set; }

        public int InfrastructureId9 { get; set; }

        public int InfrastructureId10 { get; set; }

        public int allottedPoints1 { get; set; }

        public int allottedPoints2 { get; set; }

        public int allottedPoints3 { get; set; }

        public int allottedPoints4 { get; set; }

        public int allottedPoints5 { get; set; }

        public int allottedPoints6 { get; set; }

        public int allottedPoints7 { get; set; }

        public int allottedPoints8 { get; set; }

        public int allottedPoints9 { get; set; }

        public int allottedPoints10 { get; set; }

        [Required(ErrorMessage="Required")]
        public int Infrastructure1 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure2 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure3 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure4 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure5 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure6 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure7 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure8 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure9 { get; set; }

        [Required(ErrorMessage = "Required")]
        public int Infrastructure10 { get; set; }

        public int totalObtainedPoints { get ; set; }

        public int totalAllotedPoints { get; set; }
    }
}