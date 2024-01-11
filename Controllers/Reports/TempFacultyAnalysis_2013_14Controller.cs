using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class TempFacultyAnalysis_2013_14Controller : BaseController
    {

        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult TempFacultyAnalysis(string cmd)
        {
            List<TempFacultyAnalysis_2013_14> collegeFaculty = new List<Models.TempFacultyAnalysis_2013_14>();

            //Don not  delete this Query 

            //collegeFaculty = (from t in db.temp_faculty_analysis_2013_14
            //                  join f in db.jntuh_college_faculty_2013_14 on t.Name equals f.Name
            //                  where (
            //                          (string.IsNullOrEmpty(t.Department) || t.Department == f.Department)
            //                          && (string.IsNullOrEmpty(t.Designation) || t.Designation == f.Designation)
            //                          && (string.IsNullOrEmpty(t.Qualification) || t.Qualification == f.Qualification)
            //                          && f.CollegeName == t.CollegeName
            //                      )
            //                   select new TempFacultyAnalysis_2013_14
            //                  {
            //                      CollegeCode = t.Code,
            //                      CollegeName=t.CollegeName,
            //                      Name = t.Name,
            //                      Qualification = t.Qualification,
            //                      Department = t.Department,                               
            //                      Designation = t.Designation,
            //                      FileName = f.FileName
                                 
            //                  }).ToList();
          int[] snos={163,164};
            collegeFaculty = (from t in db.facultyanalysis_2013_14
                              join co in db.facultyanalysis_2013_14_count  on t.Code equals co.Code
                              join f in db.jntuh_college_faculty_2013_14 on t.CollegeName equals f.CollegeName into cf
                              from rt in cf.Where(tf => tf.Name == t.Name && t.Department == tf.Department && t.Designation == tf.Designation && t.Qualification == tf.Qualification).DefaultIfEmpty()
                              where (t.Facultyinothercolleges != "SAME COLLEGE" && t.Facultyinothercolleges != string.Empty && !snos.Contains(t.SNo))
                              select new TempFacultyAnalysis_2013_14
                              {
                                  CollegeCode = t.Code,
                                  CollegeName = t.CollegeName,
                                  Name = t.Name,
                                  Qualification = t.Qualification,
                                  Department = t.Department,
                                  Designation = t.Designation,
                                  FileName = rt.FileName,
                                  count=(int)co.Count,
                                  facultyinothercollege=t.Facultyinothercolleges
                              }).ToList();

            if (cmd == "NameWise")
            {
                collegeFaculty = collegeFaculty.OrderBy(f => f.Name).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2013_14.cshtml", collegeFaculty);
            }
            if (cmd == "CountWise")
            {
                collegeFaculty = collegeFaculty.OrderByDescending(f => f.count).ThenBy(f=>f.CollegeCode).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2013_14.cshtml", collegeFaculty);
            }
            if (cmd == "CounGreater5")
            {
                collegeFaculty = collegeFaculty.Where(f => f.count >= 5).OrderByDescending(f => f.count).ThenBy(f => f.CollegeCode).ToList();
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2013_14.cshtml", collegeFaculty);
            }
            if (cmd == "Excel")
            {
                ViewBag.Excel = true;
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Faculty Analysys AY-2013-14.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_TempFacultyAnalysisExcel_2013_14.cshtml", collegeFaculty);
            }
            else
            {
                return PartialView("~/Views/Reports/_TempFacultyAnalysis_2013_14.cshtml", collegeFaculty);
            }
        }
    }
}
