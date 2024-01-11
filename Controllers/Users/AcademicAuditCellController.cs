using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AcademicAuditCellController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult MandateOfTheCell()
        {
            return View();
        }

        public ActionResult Director()
        {
            return View();
        }

        public ActionResult VisionMission()
        {
            return View();
        }

        public ActionResult Objectivies()
        {
            return View();
        }

        public ActionResult Goals()
        {
            return View();
        }

        public ActionResult TimelinesOfAffiliationActivities()
        {
            return View();
        }

        public ActionResult Coordinator()
        {
            return RedirectToAction("Index", "UnderConstruction");
            //return View();
        }

        public ActionResult Coordinatorone()
        {
            return RedirectToAction("Index", "UnderConstruction");
            //return View();
        }

        public ActionResult Coordinatortwo()
        {
            return RedirectToAction("Index", "UnderConstruction");
            //return View();
        }

        public ActionResult DownloadFile()
        {
            string path1 = Server.MapPath("~/Content/");
            byte[] fileBytes = System.IO.File.ReadAllBytes(path1 + "Dr. Prattipati Prasanna.pdf");
            string fileName = "Dr. Prattipati Prasanna.pdf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult deputydirectorfile()
        {
            string path1 = Server.MapPath("~/Content/");
            byte[] fileBytes = System.IO.File.ReadAllBytes(path1 + "Ravindra_Reddy.pdf");
            string fileName = "Ravindra_Reddy.pdf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult DownloadDirectorFile()
        {
            string path1 = Server.MapPath("~/Content/");
            byte[] fileBytes = System.IO.File.ReadAllBytes(path1 + "DRDuggiralaSrinivasaRaoBIO.pdf");
            string fileName = "DRDuggiralaSrinivasaRaoBIO.pdf";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        public ActionResult ViceChancellor()
        {
            return View();
        }

        public ActionResult AssistantDirector()
        {
            return View();
        }

        public ActionResult StaffMembers()
        {
            return View();
        }

        public ActionResult ProcedureforGrantofAffiliation()
        {
            return View();
        }

        public ActionResult Newsletters()
        {
            return View();
        }

        public ActionResult DeputyDirector()
        {
            return View();
        }

        public ActionResult AffCollegesStatus()
        {
            var approvedAdmittedColleges = db.jntuh_approvedadmitted_intake.AsNoTracking().ToList();
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int ActualYearAcademicYearID = jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
            var PresentAcademicYearId = jntuh_academic_year.Where(ay => ay.id == (ActualYearAcademicYearID + 1)).Select(ay => ay.id).FirstOrDefault();
            //var AcademicYear1 = jntuh_academic_year.Where(ay => ay.id == PresentAcademicYearId).Select(ay => ay.id).FirstOrDefault();
            var AcademicYear1 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 1)).Select(ay => ay).FirstOrDefault();
            var AcademicYear2 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 2)).Select(ay => ay).FirstOrDefault();
            var AcademicYear3 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 3)).Select(ay => ay).FirstOrDefault();
            var AcademicYear4 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 4)).Select(ay => ay).FirstOrDefault();
            var AcademicYear5 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 5)).Select(ay => ay).FirstOrDefault();
            ViewBag.AcademicYear1 = AcademicYear1.id; ViewBag.AcademicYear2 = AcademicYear2.id; ViewBag.AcademicYear3 = AcademicYear3.id; ViewBag.AcademicYear4 = AcademicYear4.id; ViewBag.AcademicYear5 = AcademicYear5.id;
            ViewBag.AY1 = AcademicYear1.academicYear; ViewBag.AY2 = AcademicYear2.academicYear; ViewBag.AY3 = AcademicYear3.academicYear; ViewBag.AY4 = AcademicYear4.academicYear; ViewBag.AY5 = AcademicYear5.academicYear;
            AffiliatedClgsStats affiliatedClgsStats = new AffiliatedClgsStats();
            affiliatedClgsStats.degreewiseTotalIntake1 = new List<DegreewiseTotalSeats>();
            affiliatedClgsStats.degreewiseLst = (from i in approvedAdmittedColleges
                                                 join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                                 join de in db.jntuh_department on s.departmentId equals de.id
                                                 join d in db.jntuh_degree on de.degreeId equals d.id
                                                 //join c in db.jntuh_college on i.collegeId equals c.id
                                                 where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear1.id)
                                                 orderby d.degreeDisplayOrder
                                                 group i by new
                                                 {
                                                     Id = d.id,
                                                     degree = d.degree,
                                                     degreeDisplayOrder = d.degreeDisplayOrder
                                                 } into g
                                                 select new DegreeWiseColl
                                                 {
                                                     Id = g.Key.Id,
                                                     degree = g.Key.degree,
                                                     degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                                 }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var degreewiseTotalIntake1 = (from i in approvedAdmittedColleges
                                          join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                          join de in db.jntuh_department on s.departmentId equals de.id
                                          join d in db.jntuh_degree on de.degreeId equals d.id
                                          //join c in db.jntuh_college on i.collegeId equals c.id
                                          where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear1.id)
                                          orderby d.degreeDisplayOrder
                                          group i by new
                                          {
                                              Id = d.id,
                                              degree = d.degree,
                                              degreeDisplayOrder = d.degreeDisplayOrder
                                          } into g
                                          select new DegreewiseTotalSeats
                                          {
                                              academicYearId = AcademicYear1.id,
                                              degreeId = g.Key.Id,
                                              degree = g.Key.degree,
                                              approvedIntake = g.Sum(a => a.ApprovedIntake),
                                              admittedIntake = g.Sum(a => a.AdmittedIntake),
                                              degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                          }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var degreewiseTotalIntake2 = (from i in approvedAdmittedColleges
                                          join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                          join de in db.jntuh_department on s.departmentId equals de.id
                                          join d in db.jntuh_degree on de.degreeId equals d.id
                                          //join c in db.jntuh_college on i.collegeId equals c.id
                                          where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear2.id)
                                          orderby d.degreeDisplayOrder
                                          group i by new
                                          {
                                              Id = d.id,
                                              degree = d.degree,
                                              degreeDisplayOrder = d.degreeDisplayOrder
                                          } into g
                                          select new DegreewiseTotalSeats
                                          {
                                              academicYearId = AcademicYear2.id,
                                              degreeId = g.Key.Id,
                                              degree = g.Key.degree,
                                              approvedIntake = g.Sum(a => a.ApprovedIntake),
                                              admittedIntake = g.Sum(a => a.AdmittedIntake),
                                              degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                          }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var degreewiseTotalIntake3 = (from i in approvedAdmittedColleges
                                          join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                          join de in db.jntuh_department on s.departmentId equals de.id
                                          join d in db.jntuh_degree on de.degreeId equals d.id
                                          //join c in db.jntuh_college on i.collegeId equals c.id
                                          where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear3.id)
                                          orderby d.degreeDisplayOrder
                                          group i by new
                                          {
                                              Id = d.id,
                                              degree = d.degree,
                                              degreeDisplayOrder = d.degreeDisplayOrder
                                          } into g
                                          select new DegreewiseTotalSeats
                                          {
                                              academicYearId = AcademicYear3.id,
                                              degreeId = g.Key.Id,
                                              degree = g.Key.degree,
                                              approvedIntake = g.Sum(a => a.ApprovedIntake),
                                              admittedIntake = g.Sum(a => a.AdmittedIntake),
                                              degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                          }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var degreewiseTotalIntake4 = (from i in approvedAdmittedColleges
                                          join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                          join de in db.jntuh_department on s.departmentId equals de.id
                                          join d in db.jntuh_degree on de.degreeId equals d.id
                                          //join c in db.jntuh_college on i.collegeId equals c.id
                                          where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear4.id)
                                          orderby d.degreeDisplayOrder
                                          group i by new
                                          {
                                              Id = d.id,
                                              degree = d.degree,
                                              degreeDisplayOrder = d.degreeDisplayOrder
                                          } into g
                                          select new DegreewiseTotalSeats
                                          {
                                              academicYearId = AcademicYear4.id,
                                              degreeId = g.Key.Id,
                                              degree = g.Key.degree,
                                              approvedIntake = g.Sum(a => a.ApprovedIntake),
                                              admittedIntake = g.Sum(a => a.AdmittedIntake),
                                              degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                          }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var degreewiseTotalIntake5 = (from i in approvedAdmittedColleges
                                          join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                          join de in db.jntuh_department on s.departmentId equals de.id
                                          join d in db.jntuh_degree on de.degreeId equals d.id
                                          //join c in db.jntuh_college on i.collegeId equals c.id
                                          where (i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear5.id)
                                          orderby d.degreeDisplayOrder
                                          group i by new
                                          {
                                              Id = d.id,
                                              degree = d.degree,
                                              degreeDisplayOrder = d.degreeDisplayOrder
                                          } into g
                                          select new DegreewiseTotalSeats
                                          {
                                              academicYearId = AcademicYear5.id,
                                              degreeId = g.Key.Id,
                                              degree = g.Key.degree,
                                              approvedIntake = g.Sum(a => a.ApprovedIntake),
                                              admittedIntake = g.Sum(a => a.AdmittedIntake),
                                              degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                          }).OrderBy(a => a.degreeDisplayOrder).ToList();

            foreach (var item in degreewiseTotalIntake1)
            {
                affiliatedClgsStats.degreewiseTotalIntake1.Add(new DegreewiseTotalSeats()
                {
                    academicYearId = item.academicYearId,
                    degree = item.degree,
                    degreeId = item.degreeId,
                    approvedIntake = item.approvedIntake,
                    admittedIntake = item.admittedIntake,
                    degreeDisplayOrder = item.degreeDisplayOrder
                });
            }
            foreach (var item2 in degreewiseTotalIntake2)
            {
                affiliatedClgsStats.degreewiseTotalIntake1.Add(new DegreewiseTotalSeats()
                {
                    academicYearId = item2.academicYearId,
                    degree = item2.degree,
                    degreeId = item2.degreeId,
                    approvedIntake = item2.approvedIntake,
                    admittedIntake = item2.admittedIntake,
                    degreeDisplayOrder = item2.degreeDisplayOrder
                });
            }
            foreach (var item3 in degreewiseTotalIntake3)
            {
                affiliatedClgsStats.degreewiseTotalIntake1.Add(new DegreewiseTotalSeats()
                {
                    academicYearId = item3.academicYearId,
                    degree = item3.degree,
                    degreeId = item3.degreeId,
                    approvedIntake = item3.approvedIntake,
                    admittedIntake = item3.admittedIntake,
                    degreeDisplayOrder = item3.degreeDisplayOrder
                });
            }
            foreach (var item4 in degreewiseTotalIntake4)
            {
                affiliatedClgsStats.degreewiseTotalIntake1.Add(new DegreewiseTotalSeats()
                {
                    academicYearId = item4.academicYearId,
                    degree = item4.degree,
                    degreeId = item4.degreeId,
                    approvedIntake = item4.approvedIntake,
                    admittedIntake = item4.admittedIntake,
                    degreeDisplayOrder = item4.degreeDisplayOrder
                });
            }
            foreach (var item5 in degreewiseTotalIntake5)
            {
                affiliatedClgsStats.degreewiseTotalIntake1.Add(new DegreewiseTotalSeats()
                {
                    academicYearId = item5.academicYearId,
                    degree = item5.degree,
                    degreeId = item5.degreeId,
                    approvedIntake = item5.approvedIntake,
                    admittedIntake = item5.admittedIntake,
                    degreeDisplayOrder = item5.degreeDisplayOrder
                });
            }
            affiliatedClgsStats.degreewiseTotalColleges = new List<DegreewiseAppClgs>();
            var approvedadmittedclgs1 = (from i in db.jntuh_approvedadmitted_intake
                                         join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on i.collegeId equals c.id
                                         where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear1.id)
                                         orderby d.degreeDisplayOrder
                                         select new DegreewiseTotalSeats
                                         {
                                             academicYearId = AcademicYear1.id,
                                             degreeId = d.id,
                                             degree = d.degree,
                                             collegeCode = c.collegeCode,
                                             collegeId = c.id,
                                             collegeType = c.formercollegename,
                                             degreeDisplayOrder = (int)d.degreeDisplayOrder
                                         }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var approvedadmittedclgs2 = (from i in db.jntuh_approvedadmitted_intake
                                         join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on i.collegeId equals c.id
                                         where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear2.id)
                                         orderby d.degreeDisplayOrder
                                         select new DegreewiseTotalSeats
                                         {
                                             academicYearId = AcademicYear2.id,
                                             degreeId = d.id,
                                             degree = d.degree,
                                             collegeCode = c.collegeCode,
                                             collegeId = c.id,
                                             collegeType = c.formercollegename,
                                             degreeDisplayOrder = (int)d.degreeDisplayOrder
                                         }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var approvedadmittedclgs3 = (from i in db.jntuh_approvedadmitted_intake
                                         join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on i.collegeId equals c.id
                                         where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear3.id)
                                         orderby d.degreeDisplayOrder
                                         select new DegreewiseTotalSeats
                                         {
                                             academicYearId = AcademicYear3.id,
                                             degreeId = d.id,
                                             degree = d.degree,
                                             collegeCode = c.collegeCode,
                                             collegeId = c.id,
                                             collegeType = c.formercollegename,
                                             degreeDisplayOrder = (int)d.degreeDisplayOrder
                                         }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var approvedadmittedclgs4 = (from i in db.jntuh_approvedadmitted_intake
                                         join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on i.collegeId equals c.id
                                         where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear4.id)
                                         orderby d.degreeDisplayOrder
                                         select new DegreewiseTotalSeats
                                         {
                                             academicYearId = AcademicYear4.id,
                                             degreeId = d.id,
                                             degree = d.degree,
                                             collegeCode = c.collegeCode,
                                             collegeId = c.id,
                                             collegeType = c.formercollegename,
                                             degreeDisplayOrder = (int)d.degreeDisplayOrder
                                         }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var approvedadmittedclgs5 = (from i in db.jntuh_approvedadmitted_intake
                                         join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on i.collegeId equals c.id
                                         where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == AcademicYear5.id)
                                         orderby d.degreeDisplayOrder
                                         select new DegreewiseTotalSeats
                                         {
                                             academicYearId = AcademicYear5.id,
                                             degreeId = d.id,
                                             degree = d.degree,
                                             collegeCode = c.collegeCode,
                                             collegeId = c.id,
                                             collegeType = c.formercollegename,
                                             degreeDisplayOrder = (int)d.degreeDisplayOrder
                                         }).OrderBy(a => a.degreeDisplayOrder).ToList();

            int[] courses = new int[] { 4, 5, 6 };
            var totClgsCount1 = 0; var totClgsCount2 = 0; var totClgsCount3 = 0; var totClgsCount4 = 0; var totClgsCount5 = 0;
            foreach (var item in courses)
            {
                var colgsbtechCount1 = approvedadmittedclgs1.Where(i => i.degreeId == item && i.collegeType == "Engineering").GroupBy(s => s.collegeId).ToList().Count;
                var colgsbpharCount1 = approvedadmittedclgs1.Where(i => i.degreeId == item && i.collegeType == "Pharmacy").GroupBy(s => s.collegeId).ToList().Count;
                var colgsstandaloneCount1 = approvedadmittedclgs1.Where(i => i.degreeId == item && i.collegeType == "Standalone MBA/MCA").GroupBy(s => s.collegeId).ToList().Count;

                var colgsbtechCount2 = approvedadmittedclgs2.Where(i => i.degreeId == item && i.collegeType == "Engineering").GroupBy(s => s.collegeId).ToList().Count;
                var colgsbpharCount2 = approvedadmittedclgs2.Where(i => i.degreeId == item && i.collegeType == "Pharmacy").GroupBy(s => s.collegeId).ToList().Count;
                var colgsstandaloneCount2 = approvedadmittedclgs2.Where(i => i.degreeId == item && i.collegeType == "Standalone MBA/MCA").GroupBy(s => s.collegeId).ToList().Count;

                var colgsbtechCount3 = approvedadmittedclgs3.Where(i => i.degreeId == item && i.collegeType == "Engineering").GroupBy(s => s.collegeId).ToList().Count;
                var colgsbpharCount3 = approvedadmittedclgs3.Where(i => i.degreeId == item && i.collegeType == "Pharmacy").GroupBy(s => s.collegeId).ToList().Count;
                var colgsstandaloneCount3 = approvedadmittedclgs3.Where(i => i.degreeId == item && i.collegeType == "Standalone MBA/MCA").GroupBy(s => s.collegeId).ToList().Count;

                var colgsbtechCount4 = approvedadmittedclgs4.Where(i => i.degreeId == item && i.collegeType == "Engineering").GroupBy(s => s.collegeId).ToList().Count;
                var colgsbpharCount4 = approvedadmittedclgs4.Where(i => i.degreeId == item && i.collegeType == "Pharmacy").GroupBy(s => s.collegeId).ToList().Count;
                var colgsstandaloneCount4 = approvedadmittedclgs4.Where(i => i.degreeId == item && i.collegeType == "Standalone MBA/MCA").GroupBy(s => s.collegeId).ToList().Count;

                var colgsbtechCount5 = approvedadmittedclgs5.Where(i => i.degreeId == item && i.collegeType == "Engineering").GroupBy(s => s.collegeId).ToList().Count;
                var colgsbpharCount5 = approvedadmittedclgs5.Where(i => i.degreeId == item && i.collegeType == "Pharmacy").GroupBy(s => s.collegeId).ToList().Count;
                var colgsstandaloneCount5 = approvedadmittedclgs5.Where(i => i.degreeId == item && i.collegeType == "Standalone MBA/MCA").GroupBy(s => s.collegeId).ToList().Count;

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear1.id,
                    courseName = item == 4 ? "Engineering" : (item == 5 ? "Pharmacy" : "Standalone MBA/MCA"),
                    collgesCount = item == 4 ? colgsbtechCount1 : (item == 5 ? colgsbpharCount1 : colgsstandaloneCount1),
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear2.id,
                    courseName = item == 4 ? "Engineering" : (item == 5 ? "Pharmacy" : "Standalone MBA/MCA"),
                    collgesCount = item == 4 ? colgsbtechCount2 : (item == 5 ? colgsbpharCount2 : colgsstandaloneCount2),
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear3.id,
                    courseName = item == 4 ? "Engineering" : (item == 5 ? "Pharmacy" : "Standalone MBA/MCA"),
                    collgesCount = item == 4 ? colgsbtechCount3 : (item == 5 ? colgsbpharCount3 : colgsstandaloneCount3),
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear4.id,
                    courseName = item == 4 ? "Engineering" : (item == 5 ? "Pharmacy" : "Standalone MBA/MCA"),
                    collgesCount = item == 4 ? colgsbtechCount4 : (item == 5 ? colgsbpharCount4 : colgsstandaloneCount4),
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear5.id,
                    courseName = item == 4 ? "Engineering" : (item == 5 ? "Pharmacy" : "Standalone MBA/MCA"),
                    collgesCount = item == 4 ? colgsbtechCount5 : (item == 5 ? colgsbpharCount5 : colgsstandaloneCount5),
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear1.id,
                    courseName = "Total",
                    totalCollges = totClgsCount1 = totClgsCount1 + (item == 4 ? colgsbtechCount1 : (item == 5 ? colgsbpharCount1 : colgsstandaloneCount1))
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear2.id,
                    courseName = "Total",
                    totalCollges = totClgsCount2 = totClgsCount2 + (item == 4 ? colgsbtechCount2 : (item == 5 ? colgsbpharCount2 : colgsstandaloneCount2))
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear3.id,
                    courseName = "Total",
                    totalCollges = totClgsCount3 = totClgsCount3 + (item == 4 ? colgsbtechCount3 : (item == 5 ? colgsbpharCount3 : colgsstandaloneCount3))
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear4.id,
                    courseName = "Total",
                    totalCollges = totClgsCount4 = totClgsCount4 + (item == 4 ? colgsbtechCount4 : (item == 5 ? colgsbpharCount4 : colgsstandaloneCount4))
                });

                affiliatedClgsStats.degreewiseTotalColleges.Add(new DegreewiseAppClgs()
                {
                    academicYearId = AcademicYear5.id,
                    courseName = "Total",
                    totalCollges = totClgsCount5 = totClgsCount5 + (item == 4 ? colgsbtechCount5 : (item == 5 ? colgsbpharCount5 : colgsstandaloneCount5))
                });
            }
            return View(affiliatedClgsStats);
        }
    }

    public class AffiliatedClgsStats
    {
        public string degree { get; set; }
        public IEnumerable<DegreeWiseColl> degreewiseLst { get; set; }
        public IList<DegreewiseTotalSeats> degreewiseTotalIntake1 { get; set; }
        public IList<DegreewiseAppClgs> degreewiseTotalColleges { get; set; }
    }
    public class DegreeWiseColl
    {
        public int Id { get; set; }
        public string degree { get; set; }
        public int degreeDisplayOrder { get; set; }
    }
    public class DegreewiseAppClgs
    {
        public int academicYearId { get; set; }
        public string courseName { get; set; }
        public int collgesCount { get; set; }
        public int totalCollges { get; set; }
    }
    public class DegreewiseTotalSeats
    {
        public string degree { get; set; }
        public int totalIntake { get; set; }
        public int? proposedIntake { get; set; }
        public int approvedIntake { get; set; }
        public int admittedIntake { get; set; }
        public int degreeDisplayOrder { get; set; }
        public int collegeId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public int? academicYearId { get; set; }
        public int? degreeId { get; set; }
        public int? departmentId { get; set; }
        public int? SpecealizationId { get; set; }
        public string CourseStatus { get; set; }
        public int ProposedCourses { get; set; }
        public string collegeType { get; set; }
    }
}
