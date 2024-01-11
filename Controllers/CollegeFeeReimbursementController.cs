using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeFeeReimbursementController : BaseController
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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            ViewBag.AcademeicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                      a.isPresentAcademicYear == true)
                                                          .Select(a => a.academicYear).FirstOrDefault();            

            List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeFeeReimbursement> collegeFeeReimbursement = new List<CollegeFeeReimbursement>();

            foreach (var item in reimbursement)
            {
                CollegeFeeReimbursement newReimbursement = new CollegeFeeReimbursement();
                newReimbursement.id = item.id;
                newReimbursement.collegeId = item.collegeId;
                newReimbursement.academicYearId = item.academicYearId;
                newReimbursement.specializationId = item.specializationId;
                newReimbursement.shiftId = item.shiftId;
                newReimbursement.yearInDegreeId = item.yearInDegreeId;
                newReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                newReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                newReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                newReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                newReimbursement.NRISeats = item.NRISeats;
                newReimbursement.totalNRIFee = item.totalNRIFee;
                newReimbursement.PIOSeats = item.PIOSeats;
                newReimbursement.totalPIOFee = item.totalPIOFee;
                newReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newReimbursement.department = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newReimbursement.degreeID = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newReimbursement.degree = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                newReimbursement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeFeeReimbursement.Add(newReimbursement);
            }
            collegeFeeReimbursement = collegeFeeReimbursement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.yearInDegree).ToList();
            ViewBag.CollegeFeeReimbursement = collegeFeeReimbursement;
            ViewBag.Count = collegeFeeReimbursement.Count();
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
            if (collegeFeeReimbursement.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegeFeeReimbursement.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeFeeReimbursement");
            }           
            return View();
        }

        private List<jntuh_department> Departments(int id)
        {
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id).OrderBy(d => d.departmentName).ToList();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
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
            ViewBag.AcademeicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                      a.isPresentAcademicYear == true)
                                                          .Select(a => a.academicYear).FirstOrDefault();
           
            List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeFeeReimbursement> collegeFeeReimbursement = new List<CollegeFeeReimbursement>();

            foreach (var item in reimbursement)
            {
                CollegeFeeReimbursement newReimbursement = new CollegeFeeReimbursement();
                newReimbursement.id = item.id;
                newReimbursement.collegeId = item.collegeId;
                newReimbursement.academicYearId = item.academicYearId;
                newReimbursement.specializationId = item.specializationId;
                newReimbursement.shiftId = item.shiftId;
                newReimbursement.yearInDegreeId = item.yearInDegreeId;
                newReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                newReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                newReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                newReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                newReimbursement.NRISeats = item.NRISeats;
                newReimbursement.totalNRIFee = item.totalNRIFee;
                newReimbursement.PIOSeats = item.PIOSeats;
                newReimbursement.totalPIOFee = item.totalPIOFee;
                newReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newReimbursement.department = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newReimbursement.degreeID = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newReimbursement.degree = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                newReimbursement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeFeeReimbursement.Add(newReimbursement);
            }
            collegeFeeReimbursement = collegeFeeReimbursement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.yearInDegree).ToList();
            ViewBag.CollegeFeeReimbursement = collegeFeeReimbursement;
            ViewBag.Count = collegeFeeReimbursement.Count();
            return View();
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

        private List<jntuh_specialization> Specializations(int id)
        {
            return db.jntuh_specialization.Where(s => s.isActive == true && s.departmentId == id).OrderBy(s => s.specializationName).ToList();
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

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id,string collegeId)
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
                    else if(id!=null)
                    {
                        userCollegeID = db.jntuh_college_fee_reimbursement.Where(f => f.id == id).Select(f => f.collegeId).FirstOrDefault();
                    }
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeFeeReimbursement collegeFeeReimbursement = new CollegeFeeReimbursement();
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).ToList();
                    ViewBag.Degree = degrees;
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.YearInDegree = db.jntuh_year_in_degree.Where(y => y.isActive == true);                   

                    List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

                    foreach (var item in reimbursement)
                    {
                        collegeFeeReimbursement.id = item.id;
                        collegeFeeReimbursement.collegeId = item.collegeId;
                        collegeFeeReimbursement.academicYearId = item.academicYearId;
                        collegeFeeReimbursement.specializationId = item.specializationId;
                        collegeFeeReimbursement.shiftId = item.shiftId;
                        collegeFeeReimbursement.yearInDegreeId = item.yearInDegreeId;
                        collegeFeeReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                        collegeFeeReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                        collegeFeeReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                        collegeFeeReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                        collegeFeeReimbursement.NRISeats = item.NRISeats;
                        collegeFeeReimbursement.totalNRIFee = item.totalNRIFee;
                        collegeFeeReimbursement.PIOSeats = item.PIOSeats;
                        collegeFeeReimbursement.totalPIOFee = item.totalPIOFee;
                        collegeFeeReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                        collegeFeeReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeFeeReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        collegeFeeReimbursement.department = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                        collegeFeeReimbursement.degreeID = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                        collegeFeeReimbursement.degree = db.jntuh_degree.Where(d => d.id == collegeFeeReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                        collegeFeeReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    }

                    return PartialView("_Create", collegeFeeReimbursement);

                }
                else
                {
                    CollegeFeeReimbursement collegeFeeReimbursement = new CollegeFeeReimbursement();
                    collegeFeeReimbursement.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                  (collegeDegree, degree) => new
                                                                  {
                                                                      collegeDegree.degreeId,
                                                                      collegeDegree.collegeId,
                                                                      collegeDegree.isActive,
                                                                      degree.degree
                                                                  })
                                                              .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                              .Select(collegeDegree => new
                                                              {
                                                                  collegeDegree.degreeId,
                                                                  collegeDegree.degree
                                                              }).ToList();
                    ViewBag.Degree = degrees;
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.YearInDegree = db.jntuh_year_in_degree.Where(y => y.isActive == true);
                    ViewBag.AcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id);
                    return PartialView("_Create", collegeFeeReimbursement);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegeFeeReimbursement collegeFeeReimbursement = new CollegeFeeReimbursement();
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                 (collegeDegree, degree) => new
                                                                 {
                                                                     collegeDegree.degreeId,
                                                                     collegeDegree.collegeId,
                                                                     collegeDegree.isActive,
                                                                     degree.degree
                                                                 })
                                                             .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree
                                                             }).ToList();
                    ViewBag.Degree = degrees;
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.YearInDegree = db.jntuh_year_in_degree.Where(y => y.isActive == true);
                   

                    List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

                    foreach (var item in reimbursement)
                    {
                        collegeFeeReimbursement.id = item.id;
                        collegeFeeReimbursement.collegeId = item.collegeId;
                        collegeFeeReimbursement.academicYearId = item.academicYearId;
                        collegeFeeReimbursement.specializationId = item.specializationId;
                        collegeFeeReimbursement.shiftId = item.shiftId;
                        collegeFeeReimbursement.yearInDegreeId = item.yearInDegreeId;
                        collegeFeeReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                        collegeFeeReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                        collegeFeeReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                        collegeFeeReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                        collegeFeeReimbursement.NRISeats = item.NRISeats;
                        collegeFeeReimbursement.totalNRIFee = item.totalNRIFee;
                        collegeFeeReimbursement.PIOSeats = item.PIOSeats;
                        collegeFeeReimbursement.totalPIOFee = item.totalPIOFee;
                        collegeFeeReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                        collegeFeeReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeFeeReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        collegeFeeReimbursement.department = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                        collegeFeeReimbursement.degreeID = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                        collegeFeeReimbursement.degree = db.jntuh_degree.Where(d => d.id == collegeFeeReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                        collegeFeeReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    }

                    return View("Create", collegeFeeReimbursement);
                }
                else
                {
                    CollegeFeeReimbursement collegeFeeReimbursement = new CollegeFeeReimbursement();
                    collegeFeeReimbursement.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
                                                                  (collegeDegree, degree) => new
                                                                  {
                                                                      collegeDegree.degreeId,
                                                                      collegeDegree.collegeId,
                                                                      collegeDegree.isActive,
                                                                      degree.degree
                                                                  })
                                                              .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                                                              .Select(collegeDegree => new
                                                              {
                                                                  collegeDegree.degreeId,
                                                                  collegeDegree.degree
                                                              }).ToList();
                    ViewBag.Degree = degrees;
                    ViewBag.Count = degrees.Count();
                    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
                    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
                    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
                    ViewBag.YearInDegree = db.jntuh_year_in_degree.Where(y => y.isActive == true);
                    ViewBag.AcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id);
                    return View("Create", collegeFeeReimbursement);
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(CollegeFeeReimbursement collegeFeeReimbursement, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeFeeReimbursement.collegeId;
            }
            if (ModelState.IsValid)
            {
                if (cmd == "Add")
                {
                    var id = db.jntuh_college_fee_reimbursement.Where(s => s.collegeId == userCollegeID &&
                                                                s.specializationId == collegeFeeReimbursement.specializationId &&
                                                                s.shiftId == collegeFeeReimbursement.shiftId && s.yearInDegreeId == collegeFeeReimbursement.yearInDegreeId).Select(s => s.id).FirstOrDefault();

                    if (id > 0)
                    {
                        TempData["FeeError"] = "Specialization, Shift and Year In Degree is already exists . Please enter a different Specialization, Shift and Year In Degree";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_fee_reimbursement jntuh_college_fee_reimbursement = new jntuh_college_fee_reimbursement();
                        jntuh_college_fee_reimbursement.id = collegeFeeReimbursement.id;
                        jntuh_college_fee_reimbursement.collegeId = userCollegeID;
                        jntuh_college_fee_reimbursement.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                        jntuh_college_fee_reimbursement.specializationId = collegeFeeReimbursement.specializationId;
                        jntuh_college_fee_reimbursement.shiftId = collegeFeeReimbursement.shiftId;
                        jntuh_college_fee_reimbursement.yearInDegreeId = collegeFeeReimbursement.yearInDegreeId;
                        jntuh_college_fee_reimbursement.seatsWithoutReimbursement = collegeFeeReimbursement.seatsWithoutReimbursement;
                        jntuh_college_fee_reimbursement.feeWithoutReimbursement = collegeFeeReimbursement.feeWithoutReimbursement;
                        jntuh_college_fee_reimbursement.seatsWithReimbursement = collegeFeeReimbursement.seatsWithReimbursement;
                        jntuh_college_fee_reimbursement.feeWithReimbursement = collegeFeeReimbursement.feeWithReimbursement;
                        jntuh_college_fee_reimbursement.NRISeats = collegeFeeReimbursement.NRISeats;
                        jntuh_college_fee_reimbursement.totalNRIFee = collegeFeeReimbursement.totalNRIFee;
                        jntuh_college_fee_reimbursement.PIOSeats = collegeFeeReimbursement.PIOSeats;
                        jntuh_college_fee_reimbursement.totalPIOFee = collegeFeeReimbursement.totalPIOFee;
                        jntuh_college_fee_reimbursement.createdBy = userID;
                        jntuh_college_fee_reimbursement.createdOn = DateTime.Now;
                        db.jntuh_college_fee_reimbursement.Add(jntuh_college_fee_reimbursement);
                        db.SaveChanges();
                        TempData["FeeSuccess"] = "College Fee Reimbursement Details Added successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                }
                else
                {
                    var IdUpdate = db.jntuh_college_fee_reimbursement.Where(s => s.collegeId == userCollegeID &&
                                                                s.specializationId == collegeFeeReimbursement.specializationId &&
                                                                s.shiftId == collegeFeeReimbursement.shiftId &&
                                                                s.id != collegeFeeReimbursement.id && s.yearInDegreeId == collegeFeeReimbursement.yearInDegreeId).Select(s => s.id).FirstOrDefault();

                    if (IdUpdate > 0)
                    {
                        TempData["FeeError"] = "Specialization, Shift and Year In Degree is already exists . Please enter a different Specialization, Shift and Year In Degree";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_fee_reimbursement jntuh_college_fee_reimbursement = new jntuh_college_fee_reimbursement();
                        jntuh_college_fee_reimbursement.id = collegeFeeReimbursement.id;
                        jntuh_college_fee_reimbursement.collegeId = userCollegeID;
                        jntuh_college_fee_reimbursement.academicYearId = jntuh_college_fee_reimbursement.academicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                        jntuh_college_fee_reimbursement.specializationId = collegeFeeReimbursement.specializationId;
                        jntuh_college_fee_reimbursement.shiftId = collegeFeeReimbursement.shiftId;
                        jntuh_college_fee_reimbursement.yearInDegreeId = collegeFeeReimbursement.yearInDegreeId;
                        jntuh_college_fee_reimbursement.seatsWithoutReimbursement = collegeFeeReimbursement.seatsWithoutReimbursement;
                        jntuh_college_fee_reimbursement.feeWithoutReimbursement = collegeFeeReimbursement.feeWithoutReimbursement;
                        jntuh_college_fee_reimbursement.seatsWithReimbursement = collegeFeeReimbursement.seatsWithReimbursement;
                        jntuh_college_fee_reimbursement.feeWithReimbursement = collegeFeeReimbursement.feeWithReimbursement;
                        jntuh_college_fee_reimbursement.NRISeats = collegeFeeReimbursement.NRISeats;
                        jntuh_college_fee_reimbursement.totalNRIFee = collegeFeeReimbursement.totalNRIFee;
                        jntuh_college_fee_reimbursement.PIOSeats = collegeFeeReimbursement.PIOSeats;
                        jntuh_college_fee_reimbursement.totalPIOFee = collegeFeeReimbursement.totalPIOFee;
                        jntuh_college_fee_reimbursement.createdBy = collegeFeeReimbursement.createdBy;
                        jntuh_college_fee_reimbursement.createdOn = collegeFeeReimbursement.createdOn;
                        jntuh_college_fee_reimbursement.updatedBy = userID;
                        jntuh_college_fee_reimbursement.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_fee_reimbursement).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["FeeSuccess"] = "College Fee Reimbursement Details Updated successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_fee_reimbursement.Where(f => f.id == id).Select(f=>f.collegeId).FirstOrDefault();
            }
            jntuh_college_fee_reimbursement jntuh_college_fee_reimbursement = db.jntuh_college_fee_reimbursement.Where(f => f.id == id).FirstOrDefault();
            if (jntuh_college_fee_reimbursement != null)
            {
                db.jntuh_college_fee_reimbursement.Remove(jntuh_college_fee_reimbursement);
                db.SaveChanges();
                TempData["FeeSuccess"] = "College Fee Reimbursement Details Deleted successfully.";
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_fee_reimbursement.Where(f => f.id == id).Select(f => f.collegeId).FirstOrDefault();
            }
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_fee_reimbursement.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }    

            List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID && p.id == id).ToList();

            CollegeFeeReimbursement collegeFeeReimbursement = new CollegeFeeReimbursement();
            foreach (var item in reimbursement)
            {
                collegeFeeReimbursement.id = item.id;
                collegeFeeReimbursement.collegeId = item.collegeId;
                collegeFeeReimbursement.academicYearId = item.academicYearId;
                collegeFeeReimbursement.specializationId = item.specializationId;
                collegeFeeReimbursement.shiftId = item.shiftId;
                collegeFeeReimbursement.yearInDegreeId = item.yearInDegreeId;
                collegeFeeReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                collegeFeeReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                collegeFeeReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                collegeFeeReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                collegeFeeReimbursement.NRISeats = item.NRISeats;
                collegeFeeReimbursement.totalNRIFee = item.totalNRIFee;
                collegeFeeReimbursement.PIOSeats = item.PIOSeats;
                collegeFeeReimbursement.totalPIOFee = item.totalPIOFee;
                collegeFeeReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                collegeFeeReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                collegeFeeReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                collegeFeeReimbursement.department = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                collegeFeeReimbursement.degreeID = db.jntuh_department.Where(d => d.id == collegeFeeReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                collegeFeeReimbursement.degree = db.jntuh_degree.Where(d => d.id == collegeFeeReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                collegeFeeReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
            }

            if (collegeFeeReimbursement != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_Details", collegeFeeReimbursement);
                }
                else
                {
                    return View("Details", collegeFeeReimbursement);
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            ViewBag.AcademeicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
                                                                      a.isPresentAcademicYear == true)
                                                          .Select(a => a.academicYear).FirstOrDefault();
            List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == userCollegeID).ToList();

            List<CollegeFeeReimbursement> collegeFeeReimbursement = new List<CollegeFeeReimbursement>();

            foreach (var item in reimbursement)
            {
                CollegeFeeReimbursement newReimbursement = new CollegeFeeReimbursement();
                newReimbursement.id = item.id;
                newReimbursement.collegeId = item.collegeId;
                newReimbursement.academicYearId = item.academicYearId;
                newReimbursement.specializationId = item.specializationId;
                newReimbursement.shiftId = item.shiftId;
                newReimbursement.yearInDegreeId = item.yearInDegreeId;
                newReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
                newReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
                newReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
                newReimbursement.feeWithReimbursement = item.feeWithReimbursement;
                newReimbursement.NRISeats = item.NRISeats;
                newReimbursement.totalNRIFee = item.totalNRIFee;
                newReimbursement.PIOSeats = item.PIOSeats;
                newReimbursement.totalPIOFee = item.totalPIOFee;
                newReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newReimbursement.department = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newReimbursement.degreeID = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newReimbursement.degree = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
                newReimbursement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeFeeReimbursement.Add(newReimbursement);
            }
            collegeFeeReimbursement = collegeFeeReimbursement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.yearInDegree).ToList();
            ViewBag.CollegeFeeReimbursement = collegeFeeReimbursement;
            ViewBag.Count = collegeFeeReimbursement.Count();
            return View();
        }
    }
}
