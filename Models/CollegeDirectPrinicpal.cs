using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegeDirectPrinicpal
    {
        public int sno { get; set; }
        public int activityid { get; set; }
        public string activityDesc { get; set; }
        public int collegeid { get; set; }
        public int academicyear { get; set; }
        [Required(ErrorMessage = "Activity Status Required")]
        public bool activitystatus { get; set; }
        public HttpPostedFileBase activitydoc { get; set; }
        public string activitydocpath { get; set; }
        public string supportingdocuments { get; set; }
        [RegularExpression("([0-9]+)", ErrorMessage = "Invalid Number")] 
        public string noofphdscholars { get; set; }
        public string remarks { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
        public List<CollegeDirectPrinicpalExperience> principalExperiences { get; set; }
    }

    public class CollegeDirectPrinicpalExperience
    {
        public int activityid { get; set; }
        public string encactivityId { get; set; }
        public int collegeid { get; set; }
        public string enccollegeId { get; set; }
        public int designationid { get; set; }
        public string designation { get; set; }
        public bool activitystatus { get; set; }
        [Remote("CheckPrincipalRegistrationNumber", "DirectRecruitmentPrincipalDirector", HttpMethod = "POST", ErrorMessage = "*")]
        public string registrationnumber { get; set; }
        public string presentlyWorking { get; set; }
        public string workingorganisationName { get; set; }
        public string facultyName { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public string displayExperience { get; set; }
        public HttpPostedFileBase supportingdoc1 { get; set; }
        public string supportingdoc1path { get; set; }
        public HttpPostedFileBase supportingdoc2 { get; set; }
        public string supportingdoc2path { get; set; }
        public HttpPostedFileBase supportingdoc3 { get; set; }
        public string supportingdoc3path { get; set; }
        public string remarks { get; set; }
        public bool isactive { get; set; }
        public System.DateTime createdon { get; set; }
        public int createdby { get; set; }
        public Nullable<System.DateTime> updatedon { get; set; }
        public Nullable<int> updatedby { get; set; }
    }
}