using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CampusHostel
    {
        public IEnumerable<HostelRequirements> adminLand { get; set; }
    }

    public class HostelRequirements
    {

        public int id { get; set; }

        public Nullable<int> requirementId { get; set; }

        public string requirementType { get; set; }

        [Required(ErrorMessage = "Required")]
        public string isSelected { get; set; }
        public int collegeId { get; set; }
    }
}