using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class MiscellaneousParameters
    {
        public int id { get; set; }

        public int typeId { get; set; }

        public string type { get; set; }

        [Required(ErrorMessage = "Required")]
        public string isSelected { get; set; }
        public int collegeId { get; set; }
    }
}