﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class Society
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string SocietyName { get; set; }
        public string SocietyAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsCollegeEditable { get; set; }
        public string SelectedCollegeType { get; set; }
        public int DistrictId { get; set; }
        public int CollegeTypeId { get; set; }
    }
}