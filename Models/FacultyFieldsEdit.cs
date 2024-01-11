using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class FacultyFieldsEdit
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FatherORHusbandName { get; set; }
        public string MotherName { get; set; }
        public string Gender { get; set; }
        public string Dateofbirth { get; set; }
        public string Mobile { get; set; }
        public string AadhaarNumber { get; set; }
        public string PANNumber { get; set; }
        public string Photo { get; set; }
        public string AadhaarDocument { get; set; }
        public string PANDocument { get; set; }
        public string Form16 { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string DateofAppostringment { get; set; }
        public string TotalExperience { get; set; }
        public string TotalExperiencePresentCollege { get; set; }
        public string ProceedingsNumber { get; set; }
        public string ProceedingsDocument { get; set; }
        public string AICTEFacultyID { get; set; }
        public string GrossSalary { get; set; }
        public string CourseStudied { get; set; }
        public string BranchORSpecialization { get; set; }
        public string YearofPassing { get; set; }
        public string CGPA { get; set; }
        public string Division { get; set; }
        public string BoardORUniversity { get; set; }
        public string Place { get; set; }
        public string certificate { get; set; }
        public string MPhil { get; set; }
        public string PhD { get; set; }
        public string National { get; set; }
        public string stringernational { get; set; }
        public string citation { get; set; }
        public string Awards { get; set; }
        public string SSC { get; set; }
        public string UG { get; set; }
        public string PG { get; set; }

    }

    public class fields
    {
        public int Id { get; set; }
        public string fieldName { get; set; }
        public bool  isselect { get; set; }
    }
}