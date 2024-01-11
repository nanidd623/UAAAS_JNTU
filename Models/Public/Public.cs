using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Public
    {

    }

    public class CollegeFilter
    {
        public int? District { get; set; }
        public int? Degree { get; set; }
        public int? Department { get; set; }
        public int? Specialization { get; set; }

        List<CollegeDetails> details { get; set; }
    }

    public class CollegeDetails
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string TownOrCity { get; set; }
        public string Mandal { get; set; }
        public string District { get; set; }
        public string Pincode { get; set; }
        public string EstablishedYear { get; set; }
        public string Website { get; set; }
        public List<CollegeDocuments> Photos { get; set; }
        public string EamcetCode { get; set; }
        public string IcetCode { get; set; }
        public string PgcetCode { get; set; }
        public string Mobile { get; set; }
        public string LandLine { get; set; }
        public string Email { get; set; }

    }

    public class BatchWisePerformance
    {
        public string Batch { get; set; }
        public string Duration { get; set; }
        public int? Enrolled { get; set; }
        public int? Passed { get; set; }
        public decimal? Percentage { get; set; }
    }

    public class PrincipalDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurName { get; set; }
        public string Department { get; set; }
        public string DateofAppointment { get; set; }
        public string Photo { get; set; }
        public string LastUpdatedDate { get; set; }
    }
}