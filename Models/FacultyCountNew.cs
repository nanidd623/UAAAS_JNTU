using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FacultyCountNew
    {
        public int? CollegeId { get; set; }
        public string COllegecode { get; set; }
        public string CollegeName { get; set; }
        public string Monthname { get; set; }
        public int PortalFaculty { get; set; }
        public int UploadedCount { get; set; }
        public int BiometricCount { get; set; }
        public int RequiredCount { get; set; }
        public int RequiredPHDCount { get; set; }
        public int PHDPresentCount { get; set; }
        public int BASDifferenceCount { get; set; }
        public int RequiredDifferenceCount { get; set; }
        public double RequiredCountPercentage { get; set; }
        public double PHDCountPercentage { get; set; }
    }
}