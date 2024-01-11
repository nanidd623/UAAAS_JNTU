using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegePGCourseController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }

            List<jntuh_college_pgcourses> pgCourses = db.jntuh_college_pgcourses.Where(p => p.collegeId == userCollegeID && p.isActive == true).ToList();

            ViewBag.NotUpload = false;
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            //var isEditable = db.college_circulars.Where(c => c.collegeId == userCollegeID).Select(c => c.isEditable).FirstOrDefault();

            //ViewBag.IsEditable = isEditable;


            ViewBag.Count = pgCourses.Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (pgCourses.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.collegeView = true;

            }
            if (status == 0 && pgCourses.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegePGCourse");
            }
            return View(pgCourses);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Index");
            }
            List<jntuh_college_pgcourses> pgCourses = db.jntuh_college_pgcourses.Where(p => p.collegeId == userCollegeID && p.isActive == true).ToList();
            ViewBag.Count = pgCourses.Count();
            return View("View", pgCourses);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            // int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));

            // var isEditable = db.college_circulars.Where(c => c.collegeId == userCollegeID).Select(c => c.isEditable).FirstOrDefault();

            // if (isEditable == false)
            // {
            //     return RedirectToAction("Index", "CollegePGCourse");
            //}


            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_intake_proposed.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            CollegePGCourse collegePGCourse = new CollegePGCourse();
            if (id == null)
            {
                if (collegeId != null)
                {

                    collegePGCourse.collegeId = userCollegeID;
                    collegePGCourse.jntuh_college = db.jntuh_college.Where(c => c.isActive == true && c.id == userCollegeID).Select(c => c).FirstOrDefault();
                    // "B.Tech" || d.degree=="B.Pharmacy
                    int degreeid1 = db.jntuh_degree.Where(d => d.isActive == true && d.degree == "B.Tech").Select(d => d.id).FirstOrDefault();
                    int degreeid2 = db.jntuh_degree.Where(d => d.isActive == true && d.degree == "B.Pharmacy").Select(d => d.id).FirstOrDefault();
                    ViewBag.Degree = db.jntuh_degree.Where(d => d.isActive == true && (d.id != degreeid1 && d.id != degreeid2)).Select(d => d).OrderBy(d => d.degreeDisplayOrder).ToList();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true).Select(d => d).ToList();
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(s => s).ToList();
                    ViewBag.Shifts = db.jntuh_shift.Where(sh => sh.isActive == true).Select(sh => sh).ToList();
                }
                ViewBag.IsUpdate = false;
                return View(collegePGCourse);
            }
            else
            {
                ViewBag.IsUpdate = true;
                return View(collegePGCourse);
            }

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var DepartmentList = this.Departments(Convert.ToInt32(id));

            var DepartmentsData = DepartmentList.Select(a => new SelectListItem()
            {
                Text = a.departmentName,
                Value = a.id.ToString(),
            });
            return Json(DepartmentsData, JsonRequestBehavior.AllowGet);
        }

        private List<jntuh_department> Departments(int id)
        {
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id).OrderBy(d => d.departmentName).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSpecialization(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var SecializationList = this.Specializations(Convert.ToInt32(id));
            var Specializationdata = SecializationList.Select(s => new SelectListItem()
            {
                Text = s.specializationName,
                Value = s.id.ToString(),
            });
            return Json(Specializationdata, JsonRequestBehavior.AllowGet);
        }

        private List<jntuh_specialization> Specializations(int id)
        {
            return db.jntuh_specialization.Where(s => s.isActive == true && s.departmentId == id).OrderBy(s => s.specializationName).ToList();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegePGCourse collegePGCourse, FormCollection fc)
        {
            if (ModelState.IsValid)
            {
                if (collegePGCourse.specializationId != null)
                {
                    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    jntuh_college_pgcourses pgcourse = new jntuh_college_pgcourses();

                    pgcourse.collegeId = collegePGCourse.collegeId;
                    pgcourse.specializationId = collegePGCourse.specializationId;
                    pgcourse.shiftId = collegePGCourse.shiftId;
                    pgcourse.intake = collegePGCourse.intake;
                    pgcourse.departmentId = collegePGCourse.departmentId;
                    pgcourse.professors = collegePGCourse.professors;
                    pgcourse.associateProfessors = collegePGCourse.associateProfessors;
                    pgcourse.assistantProfessors = collegePGCourse.assistantProfessors;
                    pgcourse.UGFacultyStudentRatio = collegePGCourse.UGFacultyStudentRatio;
                    pgcourse.isActive = true;
                    pgcourse.createdOn = DateTime.Now;
                    pgcourse.createdBy = userID;
                    pgcourse.Type = collegePGCourse.type;

                    db.jntuh_college_pgcourses.Add(pgcourse);
                    db.SaveChanges();
                    int courseid = pgcourse.id;

                    string tabledata = fc["griddata"];
                    jntuh_college_pgcourse_faculty pgcoursefaculty = new jntuh_college_pgcourse_faculty();
                    if (tabledata != string.Empty)
                    {
                        string[] strTemp = tabledata.Split('$');

                        foreach (var a in strTemp)
                        {
                            string[] strrows = a.Split('~');
                            if (strrows.Count() > 0)
                            {
                                if (strrows[0].ToString() != string.Empty)
                                {
                                    string fname = strrows[0].ToString();
                                    string desg = strrows[1].ToString();
                                    string ug = strrows[2].ToString();
                                    string pg = strrows[3].ToString();
                                    string phd = strrows[4].ToString();
                                    string ugs = strrows[5].ToString();
                                    string pgs = strrows[6].ToString();
                                    string phds = strrows[7].ToString();
                                    pgcoursefaculty.courseId = courseid;
                                    pgcoursefaculty.facultyName = fname;
                                    pgcoursefaculty.designation = desg;
                                    pgcoursefaculty.UG = ug;
                                    pgcoursefaculty.PG = pg;
                                    pgcoursefaculty.Phd = phd;
                                    pgcoursefaculty.UGSpecialization = ugs;
                                    pgcoursefaculty.PGSpecialization = pgs;
                                    pgcoursefaculty.PhdSpecialization = phds;
                                    pgcoursefaculty.isActive = true;
                                    pgcoursefaculty.createdBy = userID;
                                    pgcoursefaculty.createdOn = DateTime.Now;
                                    db.jntuh_college_pgcourse_faculty.Add(pgcoursefaculty);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    TempData["Error"] = "Name of the Faculty is mandatory";
                                    return View();
                                }
                            }
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Name of the PG Programme is mandatory";
                    return View();
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(collegePGCourse.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int cpgid, string id, string collegeId)
        {
            var PGCourseDetails = db.jntuh_college_pgcourses.Where(p => p.isActive == true && p.id == cpgid).Select(p => p).FirstOrDefault();
            CollegePGCourse collegePGCourse = new CollegePGCourse();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (collegeId != null)
            {
                ViewBag.Editcollege = true;
            }
            if (PGCourseDetails != null)
            {
                collegePGCourse.id = PGCourseDetails.id;
                ViewBag.collegeId = Utilities.EncryptString(PGCourseDetails.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
                collegePGCourse.collegeId = PGCourseDetails.collegeId;
                collegePGCourse.jntuh_college = db.jntuh_college.Where(c => c.isActive == true && c.id == PGCourseDetails.collegeId).Select(c => c).FirstOrDefault();
                var degreeDetails = (from s in db.jntuh_specialization
                                     join de in db.jntuh_department on s.departmentId equals de.id
                                     join d in db.jntuh_degree on de.degreeId equals d.id
                                     where (s.isActive == true && de.isActive == true && d.isActive == true && s.id == PGCourseDetails.specializationId && de.id == PGCourseDetails.departmentId)
                                     select new
                                     {
                                         d.id,
                                         d.degree
                                     }).FirstOrDefault();
                collegePGCourse.degree = degreeDetails.degree;
                collegePGCourse.departmentId = PGCourseDetails.departmentId;
                collegePGCourse.jntuh_department = db.jntuh_department.Where(d => d.isActive == true && d.id == PGCourseDetails.departmentId).Select(d => d).FirstOrDefault();
                collegePGCourse.specializationId = PGCourseDetails.specializationId;
                collegePGCourse.jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true && s.id == PGCourseDetails.specializationId).Select(s => s).FirstOrDefault();
                collegePGCourse.shiftId = PGCourseDetails.shiftId;
                collegePGCourse.intake = PGCourseDetails.intake;
                collegePGCourse.professors = PGCourseDetails.professors;
                collegePGCourse.associateProfessors = PGCourseDetails.associateProfessors;
                collegePGCourse.assistantProfessors = PGCourseDetails.assistantProfessors;
                collegePGCourse.UGFacultyStudentRatio = PGCourseDetails.UGFacultyStudentRatio;
                collegePGCourse.isActive = true;
                collegePGCourse.createdOn = DateTime.Now;
                collegePGCourse.createdBy = userID;
                collegePGCourse.type = PGCourseDetails.Type;
                collegePGCourse.PGFaculty = PGCourseFacultyList(PGCourseDetails.id);
                return View(collegePGCourse);
            }
            else
            {
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(collegePGCourse.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }


        }

        private List<CollegePGCourseFaculty> PGCourseFacultyList(int id)
        {
            List<CollegePGCourseFaculty> collegePGCourseFaculty = new List<CollegePGCourseFaculty>();

            var PGCourseFacultyList = db.jntuh_college_pgcourse_faculty.Where(pf => pf.isActive == true && pf.courseId == id).ToList();
            if (PGCourseFacultyList.Count() > 0)
            {
                foreach (var item in PGCourseFacultyList)
                {
                    CollegePGCourseFaculty pgcoursefaculty = new CollegePGCourseFaculty();
                    pgcoursefaculty.courseId = item.courseId;
                    pgcoursefaculty.facultyName = item.facultyName;
                    pgcoursefaculty.designation = item.designation;
                    pgcoursefaculty.UG = item.UG;
                    pgcoursefaculty.PG = item.PG;
                    pgcoursefaculty.Phd = item.Phd;
                    pgcoursefaculty.UGSpecialization = item.UGSpecialization;
                    pgcoursefaculty.PGSpecialization = item.PGSpecialization;
                    pgcoursefaculty.PhdSpecialization = item.PhdSpecialization;
                    collegePGCourseFaculty.Add(pgcoursefaculty);
                }
            }
            return collegePGCourseFaculty;
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var row = db.jntuh_college_pgcourses.Find(id);
            if (row != null)
            {
                List<jntuh_college_pgcourse_faculty> faculty = db.jntuh_college_pgcourse_faculty.Where(f => f.courseId == id).Select(f => f).ToList();
                if (faculty.Count() > 0)
                {
                    foreach (var item in faculty)
                    {
                        item.isActive = false;
                        item.updatedOn = DateTime.Now;
                        item.updatedBy = userID;
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
                row.isActive = false;
                row.updatedOn = DateTime.Now;
                row.updatedBy = userID;
                db.Entry(row).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(row.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord123(int? id, string collegeId)
        {
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegePGCourse collegePGCourse = db.jntuh_college_pgcourses.Where(p => p.id == id).Select(p =>
                                              new CollegePGCourse
                                              {
                                                  id = p.id,
                                                  collegeId = p.collegeId,
                                                  departmentId = p.departmentId,
                                                  specializationId = p.specializationId,
                                                  shiftId = p.shiftId,
                                                  intake = p.intake,
                                                  professors = p.professors,
                                                  associateProfessors = p.associateProfessors,
                                                  assistantProfessors = p.assistantProfessors,
                                                  UGFacultyStudentRatio = p.UGFacultyStudentRatio,
                                                  isActive = p.isActive,
                                                  createdBy = p.createdBy,
                                                  createdOn = p.createdOn,
                                                  updatedBy = p.updatedBy,
                                                  updatedOn = p.updatedOn,
                                                  jntuh_college = p.jntuh_college,
                                                  //jntuh_college_pgcourse_faculty = p.jntuh_college_pgcourse_faculty,
                                                  my_aspnet_users = p.my_aspnet_users,
                                                  my_aspnet_users1 = p.my_aspnet_users1,
                                                  jntuh_specialization = p.jntuh_specialization,
                                                  jntuh_shift = p.jntuh_shift,
                                                  jntuh_department = p.jntuh_department,
                                                  PGFaculty = db.jntuh_college_pgcourse_faculty
                                                                .Where(f => f.courseId == p.id)
                                                                .Select(f => new CollegePGCourseFaculty
                                                                {
                                                                    id = f.id,
                                                                    courseId = f.courseId,
                                                                    facultyName = f.facultyName,
                                                                    designation = f.designation,
                                                                    UG = f.UG,
                                                                    PG = f.PG,
                                                                    Phd = f.Phd,
                                                                    UGSpecialization = f.UGSpecialization,
                                                                    PGSpecialization = f.PGSpecialization,
                                                                    PhdSpecialization = f.PhdSpecialization,
                                                                    isActive = f.isActive,
                                                                    createdBy = f.createdBy,
                                                                    createdOn = f.createdOn,
                                                                    updatedBy = f.updatedBy,
                                                                    updatedOn = f.updatedOn,
                                                                    //jntuh_college_pgcourses = f.jntuh_college_pgcourses,
                                                                    my_aspnet_users = f.my_aspnet_users,
                                                                    my_aspnet_users1 = f.my_aspnet_users1
                                                                }).ToList()
                                              }).FirstOrDefault();

                    return PartialView("_Create", collegePGCourse);
                }
                else
                {
                    CollegePGCourse collegePGCourse = new CollegePGCourse();

                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegePGCourse.collegeId = userCollegeID;
                        collegePGCourse.jntuh_college = db.jntuh_college.Find(userCollegeID);

                        List<SelectListItem> lstSpecialization = new List<SelectListItem>();

                        List<SelectListItem> lstDepartment = new List<SelectListItem>();

                        foreach (var item in db.jntuh_department)
                        {
                            string txt = item.departmentName;
                            string val = item.id.ToString();

                            int exists = 0;
                            foreach (var list in lstDepartment)
                            {
                                if (list.Text == txt)
                                { exists = 1; }
                            }

                            if (exists == 0)
                                lstDepartment.Add(new SelectListItem { Text = txt, Value = val });
                        }

                        ViewBag.Department = lstDepartment.OrderBy(l => l.Text);

                        List<SelectListItem> lstShifts = new List<SelectListItem>();

                        foreach (var item in db.jntuh_shift)
                        {
                            string txt = item.shiftName;
                            string val = item.id.ToString();

                            int exists = 0;
                            foreach (var list in lstShifts)
                            {
                                if (list.Text == txt)
                                { exists = 1; }
                            }

                            if (exists == 0)
                                lstShifts.Add(new SelectListItem { Text = txt, Value = val });
                        }

                        ViewBag.Shifts = lstShifts.OrderBy(l => l.Text);

                        foreach (var item in db.jntuh_specialization)
                        {
                            int degreeid = db.jntuh_department.Where(de => de.id == item.departmentId).Select(de => de.degreeId).FirstOrDefault();
                            string degree = db.jntuh_degree.Where(d => d.id == degreeid).Select(d => d.degree).FirstOrDefault();
                            string txt = degree + " - " + item.specializationName;
                            string val = item.id.ToString();

                            int exists = 0;
                            foreach (var list in lstSpecialization)
                            {
                                if (list.Text == txt)
                                { exists = 1; }
                            }

                            if (exists == 0)
                                lstSpecialization.Add(new SelectListItem { Text = txt, Value = val });
                        }

                        ViewBag.Specialization = lstSpecialization.OrderBy(l => l.Text);
                    }

                    ViewBag.IsUpdate = false;
                    return PartialView("_Create", collegePGCourse);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegePayment collegePayment = db.jntuh_college_payment.Where(p => p.id == id).Select(p =>
                                              new CollegePayment
                                              {
                                                  id = p.id,
                                                  collegeId = p.collegeId,
                                                  paymentDate = p.paymentDate,
                                                  paymentType = p.paymentType,
                                                  paymentNumber = p.paymentNumber,
                                                  paymentStatus = p.paymentStatus,
                                                  paymentAmount = p.paymentAmount,
                                                  paymentBranch = p.paymentBranch,
                                                  paymentLocation = p.paymentLocation,
                                                  createdBy = p.createdBy,
                                                  createdOn = p.createdOn,
                                                  updatedBy = p.updatedBy,
                                                  updatedOn = p.updatedOn
                                              }).FirstOrDefault();
                    collegePayment.date = Utilities.MMDDYY2DDMMYY(collegePayment.paymentDate.ToString());
                    return View("Create", collegePayment);
                }
                else
                {
                    CollegePGCourse collegePGCourse = new CollegePGCourse();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegePGCourse.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("Create", collegePGCourse);
                }
            }
        }

    }
}
