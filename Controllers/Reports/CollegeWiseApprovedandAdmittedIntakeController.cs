using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeWiseApprovedandAdmittedIntakeController : BaseController
    {
        //
        // GET: /CollegeWiseApprovedandAdmittedIntake/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        public ActionResult CollegeWiseApprovedandAdmittedIntake()
        {

          
            var accademiyearList = (from a in db.jntuh_academic_year                                    
                                     where (a.isActive == true)
                                     select new
                                     {
                                         a.id,
                                         a.academicYear
                                     }).ToList();
            
            ViewBag.accademiyearList = accademiyearList.OrderByDescending(a=>a.academicYear).ToList();
            ViewBag.degreeList = (from d in db.jntuh_degree
                                  where (d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select new
                                  {
                                      d.id,
                                      d.degree
                                  }).ToList();
            int ActualYearAcademicYearID = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
            int DegreeId = db.jntuh_degree.Where(d => d.isActive == true && d.degree == "B.Tech").Select(d => d.id).FirstOrDefault();
            string AcademicYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.id == ActualYearAcademicYearID).Select(ay => ay.academicYear).FirstOrDefault();
            ViewBag.AcademicYear = AcademicYear;
            List<DegreewiseTotalIntake> degreewiseTotalIntake = DegreewiseTotalIntake(ActualYearAcademicYearID, DegreeId);
            return View("~/Views/Reports/CollegeWiseApprovedandAdmittedIntake.cshtml", degreewiseTotalIntake);
        }

        private List<DegreewiseTotalIntake> DegreewiseTotalIntake(int ActualYearAcademicYearID, int DegreeId)
        {
            List<DegreewiseTotalIntake> degreewiseTotalIntake = new List<DegreewiseTotalIntake>();
            degreewiseTotalIntake = (from i in db.jntuh_college_intake_existing
                                     join s in db.jntuh_specialization on i.specializationId equals s.id
                                     join de in db.jntuh_department on s.departmentId equals de.id
                                     join d in db.jntuh_degree on de.degreeId equals d.id
                                     join c in db.jntuh_college on i.collegeId equals c.id
                                     where (c.isActive == true && i.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.academicYearId == ActualYearAcademicYearID && d.id == DegreeId)
                                     orderby c.collegeName
                                     group i by new
                                     {
                                         collegeName = c.collegeName,
                                         collegecode = c.collegeCode,
                                         degree = d.degree,
                                         academicYearId=i.academicYearId,
                                         degreeid=d.id                                        
                                     } into g
                                     select new DegreewiseTotalIntake
                                     {
                                         collegeName = g.Key.collegeName,
                                         collegeCode = g.Key.collegecode,
                                         degree = g.Key.degree,
                                         academicYearId=g.Key.academicYearId,
                                         degreeId=g.Key.degreeid,
                                         totalIntake = g.Sum(a => a.approvedIntake),
                                         admittedIntake = g.Sum(a => a.admittedIntake)
                                     }).OrderBy(c => c.collegeName).ToList();

            return degreewiseTotalIntake;
        }       

        [HttpPost]
        public ActionResult CollegeWiseApprovedandAdmittedIntake(int ? ddlacademicYearId, int ? ddldegreeId, string cmd)
        {
            var accademiyearList = (from a in db.jntuh_academic_year
                                    where (a.isActive == true)
                                    select new
                                    {
                                        a.id,
                                        a.academicYear
                                    }).ToList();

            ViewBag.accademiyearList = accademiyearList.OrderByDescending(a => a.academicYear).ToList();
            ViewBag.degreeList = (from d in db.jntuh_degree
                                  where (d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select new
                                  {
                                      d.id,
                                      d.degree
                                  }).ToList();
              int ActualYearAcademicYearID = (int)ddlacademicYearId;
              int DegreeId = (int)ddldegreeId;
              string AcademicYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.id == ActualYearAcademicYearID).Select(ay => ay.academicYear).FirstOrDefault();
              ViewBag.AcademicYear = AcademicYear;
              List<DegreewiseTotalIntake> degreewiseTotalIntake = DegreewiseTotalIntake(ActualYearAcademicYearID, DegreeId);

              if (degreewiseTotalIntake.Count==0)
              {
                  ViewBag.Intake = "Proposed";
                  degreewiseTotalIntake = (from i in db.jntuh_college_intake_proposed
                                           join s in db.jntuh_specialization on i.specializationId equals s.id
                                           join de in db.jntuh_department on s.departmentId equals de.id
                                           join d in db.jntuh_degree on de.degreeId equals d.id
                                           join c in db.jntuh_college on i.collegeId equals c.id
                                           where (c.isActive == true && i.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.academicYearId == ActualYearAcademicYearID && d.id == DegreeId)
                                           orderby c.collegeName
                                           group i by new
                                           {
                                               collegeName = c.collegeName,
                                               collegecode = c.collegeCode,
                                               degree = d.degree,
                                               academicYearId = i.academicYearId,
                                               degreeid = d.id
                                           } into g
                                           select new DegreewiseTotalIntake
                                           {
                                               collegeName = g.Key.collegeName,
                                               collegeCode = g.Key.collegecode,
                                               degree = g.Key.degree,
                                               academicYearId = g.Key.academicYearId,
                                               degreeId = g.Key.degreeid,
                                               totalIntake = g.Sum(a => a.proposedIntake)                                              
                                           }).OrderBy(c => c.collegeName).ToList();
              }
             

            if (degreewiseTotalIntake.Count != 0 && cmd=="Export")
            {
                ViewBag.degreewiseTotalIntake = degreewiseTotalIntake;
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=College Wise Approved and AdmittedIntake-" + AcademicYear + ".xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_CollegeWiseApprovedandAdmittedIntake.cshtml");
            }
            return View("~/Views/Reports/CollegeWiseApprovedandAdmittedIntake.cshtml", degreewiseTotalIntake);
        }
    }
}
