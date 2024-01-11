using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class TempFacultyAnalysis_2014_15Controller : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult TempFacultyAnalysis(string cmd)
        {
            List<TempFacultyAnalysis_2014_15> collegeFaculty = new List<Models.TempFacultyAnalysis_2014_15>();           

            //Don not  delete this Query 

            //collegeFaculty = (from t in db.temp_faculty_analysis_2014_15
            //                  join f in db.jntuh_college_faculty on t.FirstName equals f.facultyFirstName
            //                  join c in db.jntuh_college on f.collegeId equals c.id
            //                  where (

            //                      (string.IsNullOrEmpty(t.LastName) || t.LastName == f.facultyLastName)
            //                      && (string.IsNullOrEmpty(t.Surname) || t.Surname == f.facultySurname)
            //                      && (string.IsNullOrEmpty(t.Father) || t.Father == f.facultyFatherName)
            //                      && (string.IsNullOrEmpty(t.Email) || t.Email == f.facultyEmail)
            //                      && (string.IsNullOrEmpty(t.PANnumber) || t.PANnumber == f.facultyPANNumber)
            //                      && c.collegeName == c.collegeName
            //                      )

            //                  select new TempFacultyAnalysis_2014_15
            //                  {
            //                      Code = c.collegeCode,
            //                      CollegeName = c.collegeName,
            //                      FirstName = t.FirstName,
            //                      LastName = t.LastName,
            //                      Surname = t.Surname,
            //                      Father = t.Father,                               
            //                      DateOfAppointment = f.facultyDateOfAppointment,
            //                      DateOfBirth = f.facultyDateOfBirth,                               
            //                      photo = f.facultyPhoto.Replace("~/", "http://112.133.193.228:75/")
            //                  }).ToList();


            int[] fids = { 66339, 74297, 29574, 40973, 65799, 65812, 66283 };
            collegeFaculty = (from t in db.facultyanalysis_2014_15
                              join co in db.facultyanalysis_2014_15_count on t.Code equals co.Code
                              join c in db.jntuh_college on t.Code equals c.collegeCode                            
                              join f in db.jntuh_college_faculty on c.id equals f.collegeId into cf
                              from rt in cf.DefaultIfEmpty()
                              where(t.Facultyinothercolleges!="SAME COLLEGE" && t.Facultyinothercolleges!=string.Empty 
                              && rt.facultyFirstName == t.FirstName && rt.facultySurname == t.Surname && rt.facultyFatherName == t.Father 
                              && !fids.Contains(rt.id)
                              )
                              select new TempFacultyAnalysis_2014_15
                              {
                                  Code = c.collegeCode,
                                  CollegeName = c.collegeName,
                                  FirstName = t.FirstName,
                                  LastName = t.LastName,
                                  Surname = t.Surname,
                                  Father = t.Father,
                                  DateOfAppointment = rt.facultyDateOfAppointment,
                                  DateOfBirth = rt.facultyDateOfBirth,
                                  facultyinothercollege=t.Facultyinothercolleges,
                                  count = (int)co.Count,
                                  photo = rt.facultyPhoto.Replace("~/", "http://112.133.193.228:75/")
                              }).ToList();

            if (cmd == "NameWise")
            {
                collegeFaculty = collegeFaculty.OrderBy(f => f.FirstName).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2014_15.cshtml", collegeFaculty);   
            }
            if (cmd == "CountWise")
            {
                collegeFaculty = collegeFaculty.OrderByDescending(f => f.count).ThenBy(f => f.Code).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2014_15.cshtml", collegeFaculty);   
            }
            if (cmd == "CounGreater5")
            {
                collegeFaculty = collegeFaculty.Where(f => f.count >= 5).OrderByDescending(f => f.count).ThenBy(f => f.Code).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2014_15.cshtml", collegeFaculty);   
            }

            if (cmd == "Excel")
            {
                ViewBag.Excel = true;
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Faculty Analysys AY-2014-15.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_TempFacultyAnalysisExcel_2014_15.cshtml", collegeFaculty);
               
            }
            else
            {
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2014_15.cshtml", collegeFaculty);               
            }
        }

    }
}
