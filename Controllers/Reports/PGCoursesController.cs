using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class PGCoursesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        public class Colleges
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        private string[] Strphd = {
                                       "---------",
                                       "--------",
                                       "-------",
                                       "------",
                                       "-----",
                                       "----",
                                       "---",
                                       "--",
                                       "-",
                                       " "
                                    };



        [Authorize(Roles = "Admin")]
        public ActionResult PGCourseSubmittedandNotSubmittedColleges(string strtype)
        {
            string ReportHeader = string.Empty;
            List<CollegePGCourses> collegePGCoursesList = new List<CollegePGCourses>();
            collegePGCoursesList = (from c in db.jntuh_college
                                    join cp in db.jntuh_college_pgcourses on c.id equals cp.collegeId into cpg
                                    from cpgc in cpg.Where(cp => cp.isActive == true).DefaultIfEmpty()
                                    where (c.isActive == true)
                                    group c by new { c.id, c.collegeCode, c.collegeName, cpgc.collegeId } into g
                                    select new CollegePGCourses
                                    {
                                        collegeid = g.Key.collegeId,
                                        collegeCode = g.Key.collegeCode,
                                        collegeName = g.Key.collegeName
                                    }).ToList();

            if (strtype == "Submitted")
            {
                ReportHeader = "PG Courses Submitted Colleges.xls";
                collegePGCoursesList = collegePGCoursesList.Where(c => c.collegeid != null).OrderBy(c => c.collegeName).ToList();
            }
            else
            {
                ReportHeader = "PG Courses Not Submitted Colleges.xls";
                collegePGCoursesList = collegePGCoursesList.Where(c => c.collegeid == null).OrderBy(c => c.collegeName).ToList();
            }

            ViewBag.collegePGCoursesList = collegePGCoursesList.ToList();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Reports/_PGCourseSubmittedandNotSubmittedColleges.cshtml");

        }

        //new


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult PGCourseSubmittedandCollegesNew()
        {
            ViewBag.CollegesList = (from c in db.jntuh_college
                                    where c.isActive == true
                                    select new Colleges
                                    {
                                        id = c.id,
                                        name = c.collegeCode + " - " + c.collegeName
                                    }).ToList();

            List<PGCourseSpecializationandFacultyList> collegePGCourseList = new List<PGCourseSpecializationandFacultyList>();
            string degreetype = string.Empty;
            degreetype = "MTM";
            ViewBag.course = "MTM";
            var pgcoursecollegeIDs = db.jntuh_college_pgcourses.Where(cp => cp.isActive == true).Select(cp => cp.collegeId).Distinct().ToArray();

            int[] degree_id;

            if (degreetype != string.Empty)
            {
                degree_id = db.jntuh_degree.Where(d => d.isActive == true && d.degree == degreetype).Select(d => d.id).ToArray();
            }
            else
            {
                degree_id = db.jntuh_degree.Where(d => d.isActive == true && (d.degreeTypeId == 2 || d.degreeTypeId == 3)).Select(d => d.id).ToArray();
            }

            var specifiedDegreeColleges = db.jntuh_college_degree.Where(cd => cd.isActive == true && degree_id.Contains(cd.degreeId)).Select(cd => cd.collegeId).Distinct().ToArray();

            int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && pgcoursecollegeIDs.Contains(c.id)).Select(c => c.id).ToArray();
            //int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id==209).Select(c => c.id).ToArray();

            foreach (var cID in collegeIDs)
            {

                PGCourseSpecializationandFacultyList PGCourses = new PGCourseSpecializationandFacultyList();
                PGCourses.PGCourse = PGCourseList(cID, degreetype);
                var collegeDetails = db.jntuh_college.Where(c => c.isActive == true && c.id == cID).Select(c => c).FirstOrDefault();
                PGCourses.collegeCode = collegeDetails.collegeCode;
                PGCourses.collegeName = collegeDetails.collegeName;
                PGCourses.instituteEstablishedYear = db.jntuh_college_establishment.Where(e => e.collegeId == cID).Select(e => e.instituteEstablishedYear).FirstOrDefault();
                collegePGCourseList.Add(PGCourses);
            }
            return View("~/Views/Reports/PGCourseSubmittedandColleges.cshtml", collegePGCourseList.OrderBy(c => c.collegeName).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult PGCourseSubmittedandCollegesNew(string cmd, FormCollection fc)
        {
            int[] collegeIDs;
            int collegeid = 0;
            string strcollegeid = fc["collegeId"];
            if (strcollegeid != string.Empty)
            {
                collegeid = Convert.ToInt32(strcollegeid);
            }
            var pgcoursecollegeIDs = db.jntuh_college_pgcourses.Where(cp => cp.isActive == true).Select(cp => cp.collegeId).Distinct().ToArray();

            ViewBag.CollegesList = (from c in db.jntuh_college
                                    where c.isActive == true
                                    select new Colleges
                                    {
                                        id = c.id,
                                        name = c.collegeCode + " - " + c.collegeName
                                    }).ToList();

            List<PGCourseSpecializationandFacultyList> collegePGCourseList = new List<PGCourseSpecializationandFacultyList>();

            string degreetype = string.Empty;
            string ReportHeader = string.Empty;

            if (cmd == "M.Tech" || cmd == "Export M.Tech")
            {
                ReportHeader = "M.Tech Colleges Report.xls";
                degreetype = "M.Tech";

            }
            else if (cmd == "M.Pharm" || cmd == "Export M.Pharm")
            {
                ReportHeader = "M.Pharm Colleges Report.xls";
                degreetype = "M.Pharmacy";
            }
            else if (cmd == "MBA" || cmd == "Export MBA")
            {
                ReportHeader = "MBA Colleges Report.xls";
                degreetype = "MBA";
            }
            else if (cmd == "MCA" || cmd == "Export MCA")
            {
                ReportHeader = "MCA Colleges Report.xls";
                degreetype = "MCA";
            }
            else if (cmd == "Pharm.D" || cmd == "Export Pharm.D")
            {
                ReportHeader = "Pharm.D Colleges Report.xls";
                degreetype = "Pharm.D";
            }
            else if (cmd == "Pharm.D PB" || cmd == "Export Pharm.D PB")
            {
                ReportHeader = "Pharm.D PB Colleges Report.xls";
                degreetype = "Pharm.D PB";
            }
            else if (cmd == "MAM" || cmd == "Export MAM")
            {
                ReportHeader = "MAM Colleges Report.xls";
                degreetype = "MAM";
            }
            else if (cmd == "MTM" || cmd == "Export MTM")
            {
                ReportHeader = "MTM Colleges Report.xls";
                degreetype = "MTM";
            }
            else if (cmd == "ALL" || cmd == "Export")
            {
                ReportHeader = "ALL Courses Report.xls";
                degreetype = "";
            }
            else
            {
                ReportHeader = "M.Tech Colleges Report.xls";
                degreetype = "M.Tech";
            }

            int[] degree_id;

            if (degreetype != string.Empty)
            {
                degree_id = db.jntuh_degree.Where(d => d.isActive == true && d.degree == degreetype).Select(d => d.id).ToArray();
            }
            else
            {
                degree_id = db.jntuh_degree.Where(d => d.isActive == true && (d.degreeTypeId == 2 || d.degreeTypeId == 3)).Select(d => d.id).ToArray();
            }

            var specifiedDegreeColleges = db.jntuh_college_degree.Where(cd => cd.isActive == true && degree_id.Contains(cd.degreeId)).Select(cd => cd.collegeId).Distinct().ToArray();

            if (collegeid > 0)
            {
                collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeid && pgcoursecollegeIDs.Contains(c.id)).Select(c => c.id).ToArray();
            }
            else
            {
                collegeIDs = db.jntuh_college.Where(c => c.isActive == true && pgcoursecollegeIDs.Contains(c.id)).Select(c => c.id).ToArray();
                //collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id==209).Select(c => c.id).ToArray();
            }

            foreach (var cID in collegeIDs)
            {

                PGCourseSpecializationandFacultyList PGCourses = new PGCourseSpecializationandFacultyList();
                PGCourses.PGCourse = PGCourseList(cID, degreetype);
                var collegeDetails = db.jntuh_college.Where(c => c.isActive == true && c.id == cID).Select(c => c).FirstOrDefault();
                PGCourses.collegeCode = collegeDetails.collegeCode;
                PGCourses.collegeName = collegeDetails.collegeName;
                PGCourses.instituteEstablishedYear = db.jntuh_college_establishment.Where(e => e.collegeId == cID).Select(e => e.instituteEstablishedYear).FirstOrDefault();
                collegePGCourseList.Add(PGCourses);
            }

            ViewBag.course = degreetype;

            if (cmd == "Export M.Tech" || cmd == "Export M.Pharm" || cmd == "Export MBA" || cmd == "Export MCA" || cmd == "Export Pharm.D" || cmd == "Export Pharm.D PB" || cmd == "Export MAM" || cmd == "Export MTM" || cmd == "Export")
            {
                ViewBag.collegePGCourseList = collegePGCourseList;
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_PGCourseSubmittedandColleges.cshtml", collegePGCourseList.OrderBy(c => c.collegeName).ToList());
            }
            return View("~/Views/Reports/PGCourseSubmittedandColleges.cshtml", collegePGCourseList.OrderBy(c => c.collegeName).ToList());
        }

        private List<CollegePGCourse> PGCourseList(int id, string degreetype)
        {
            List<CollegePGCourse> CollegePGCourseList = new List<CollegePGCourse>();
            var CollegePGList1 = db.jntuh_college_pgcourses.Where(c => c.isActive == true && c.collegeId == id).ToList();

            var CollegePGList = (from cp in db.jntuh_college_pgcourses
                                 join s in db.jntuh_specialization on cp.specializationId equals s.id
                                 join de in db.jntuh_department on s.departmentId equals de.id
                                 join d in db.jntuh_degree on de.degreeId equals d.id
                                 where (cp.isActive == true && s.isActive == true && de.isActive == true & d.isActive == true && cp.collegeId == id)
                                 select new
                                 {
                                     cp.id,
                                     cp.collegeId,
                                     cp.specializationId,
                                     cp.departmentId,
                                     cp.intake,
                                     cp.professors,
                                     cp.associateProfessors,
                                     cp.assistantProfessors,
                                     cp.shiftId,
                                     cp.UGFacultyStudentRatio,
                                     d.degree
                                 }).ToList();

            if (degreetype != string.Empty)
            {
                CollegePGList = CollegePGList.Where(d => d.degree == degreetype).ToList();
            }

            if (CollegePGList.Count() > 0)
            {
                foreach (var item in CollegePGList)
                {
                    CollegePGCourse collegePGCourse = new CollegePGCourse();
                    collegePGCourse.jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true && s.id == item.specializationId).Select(s => s).FirstOrDefault();
                    collegePGCourse.jntuh_department = db.jntuh_department.Where(d => d.isActive == true && d.id == item.departmentId).Select(d => d).FirstOrDefault();
                    var degreeDetails = (from s in db.jntuh_specialization
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         where (s.isActive == true && de.isActive == true && d.isActive == true && s.id == item.specializationId && de.id == item.departmentId)
                                         select new
                                         {
                                             d.id,
                                             d.degree
                                         }).FirstOrDefault();
                    collegePGCourse.degree = degreeDetails.degree;
                    collegePGCourse.shiftId = item.shiftId;
                    collegePGCourse.intake = item.intake;
                    collegePGCourse.professors = item.professors;
                    collegePGCourse.associateProfessors = item.associateProfessors;
                    collegePGCourse.assistantProfessors = item.assistantProfessors;

                    collegePGCourse.PGFaculty = (from pf in db.jntuh_college_pgcourse_faculty
                                                 where (pf.isActive == true && pf.courseId == item.id
                                                 && !Strphd.Contains(pf.Phd)
                                                 && !Strphd.Contains(pf.PhdSpecialization)
                                                 && !Strphd.Contains(pf.PGSpecialization))
                                                 select new CollegePGCourseFaculty
                                                 {
                                                     UG = pf.UG,
                                                     PG = pf.PG,
                                                     Phd = pf.Phd,
                                                     UGSpecialization = pf.UGSpecialization,
                                                     PGSpecialization = pf.PGSpecialization,
                                                     PhdSpecialization = pf.PhdSpecialization
                                                 }).ToList();
                    CollegePGCourseList.Add(collegePGCourse);
                }
            }
            return CollegePGCourseList;
        }




    }
}
