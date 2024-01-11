using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class TempFacultyAnalysisController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult TempFacultyAnalysis()
        {
            List<CollegeFaculty> collegeFaculty = new List<Models.CollegeFaculty>();
            //collegeFaculty = (from t in db.temp_faculty_analysis_2014_15
            //                                join f in db.jntuh_college_faculty //on t.FirstName equals f.facultyFirstName
            //                                on new { t.FirstName, t.LastName, t.Surname, t.Father, t.PANnumber, t.Mobile } 
            //                                equals new { f.facultyFirstName, f.facultyLastName, f.facultySurname, f.facultyFatherName, f.facultyPANNumber, f.facultyMobile }
            //                                where (f.facultyTypeId == 1)
            //                                select new CollegeFaculty
            //                                {
            //                                    facultyFirstName=t.FirstName,
            //                                    facultyLastName = t.LastName,
            //                                    facultySurname = t.Surname,
            //                                    facultyFatherName = t.Father,
            //                                    photo=f.facultyPhoto
            //                                }).ToList();

            //collegeFaculty = (from t in db.temp_faculty_analysis_2014_15
            //                  from f in db.jntuh_college_faculty
            //                  where (t.FirstName == f.facultyFirstName &&
            //                        t.LastName == f.facultyLastName &&
            //                        t.Surname == f.facultySurname &&
            //                        t.Father == f.facultyFatherName &&
            //                         t.PANnumber == f.facultyPANNumber &&
            //                        t.Mobile == f.facultyMobile
            //                        )
            //                 // where (f.facultyTypeId == 1)
            //                  select new CollegeFaculty
            //                  {
            //                      facultyFirstName = t.FirstName,
            //                      facultyLastName = t.LastName,
            //                      facultySurname = t.Surname,
            //                      facultyFatherName = t.Father,
            //                      photo = f.facultyPhoto
            //                  }).ToList();

            collegeFaculty = (from t in db.temp_faculty_analysis_2014_15

                              join f in db.jntuh_college_faculty on t.FirstName equals f.facultyFirstName

                              join c in db.jntuh_college on f.collegeId equals c.id

                              where (//t.FirstName == f.facultyFirstName &&

                                  (string.IsNullOrEmpty(t.LastName) || t.LastName == f.facultyLastName)
                                  && (string.IsNullOrEmpty(t.Surname) || t.Surname == f.facultySurname)
                                  && (string.IsNullOrEmpty(t.Father) || t.Father == f.facultyFatherName)
                                  && (string.IsNullOrEmpty(t.Email) || t.Email == f.facultyEmail)
                                  && (string.IsNullOrEmpty(t.PANnumber) || t.PANnumber == f.facultyPANNumber)
                                  && c.collegeName == c.collegeName
                                  )                       

                              select new CollegeFaculty
                              {
                                  facultyFirstName = t.FirstName,
                                  facultyLastName = t.LastName,
                                  facultySurname = t.Surname,
                                  facultyFatherName = t.Father,
                                  photo = f.facultyPhoto.Replace("~/", "http://202.53.85.206:75/")
                              }).ToList();

            //Response.ClearContent();
            //Response.Buffer = true;
            //Response.AddHeader("content-disposition", "attachment; filename=Faculty Analysys Report.xls");
            //Response.ContentType = "application/vnd.ms-excel";          
            return PartialView("~/Views/Reports/_TempFacultyAnalysis.cshtml", collegeFaculty);
        }

    }
}
