using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegeSpecializations
    {
        public int collegeId { get; set; }

        public string collegeCode { get; set; }

        public string collegeName { get; set; }

        public int districtId { get; set; }

        public int district { get; set; }

        public List<AdminSpecialization> collegeSpecializations { get; set; }
    }

    public class AdminSpecialization
    {
        public int collegeId {get; set; }

        public string specialization { get; set; }

        public string intake { get; set; }
    }

    public class AdminCollegeSpecialization
    {
        public int collegeId { get; set; }

        public int degreeId { get; set; }

        public int degreeTypeId { get; set; }

        public int specializationId { get; set; }

        public int courseAffiliationStatusCodeId { get; set; }

        public int proposedIntake { get; set; }

        public int shiftId { get; set; }
    }
}