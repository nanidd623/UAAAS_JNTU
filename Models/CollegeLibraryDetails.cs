using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeLibraryDetails
    {
        public IEnumerable<LibraryDetails> adminLand { get; set; }
    }

    public class LibraryDetails
    {
        public int id { get; set; }

        public int degreeId { get; set; }

        public string degree { get; set; }
        public string LibraryTitlesPath { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? totalTitles { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? totalVolumes { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? totalNationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? totalInternationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? totalEJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? newTitles { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? newVolumes { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? newNationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? newInternationalJournals { get; set; }

        [Required(ErrorMessage = "Required")]
        public int? newEJournals { get; set; }

        public string EJournalsSubscriptionNumber { get; set; }
        public HttpPostedFileBase uploadFile { get; set; }
        public string FilePath { get; set; }

        public int collegeId { get; set; }
        public virtual jntuh_degree jntuh_degree { get; set; }
    }
}