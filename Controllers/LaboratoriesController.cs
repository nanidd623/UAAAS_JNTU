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
    public class LaboratoriesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            
            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID).ToList();

            List<Laboratories> laboratories = new List<Laboratories>();

            foreach (var item in labs)
            {
                Laboratories newLab = new Laboratories();
                newLab.id = item.id;
                newLab.collegeId = item.collegeId;
                newLab.labName = item.labName;
                newLab.IsShared =(bool)item.isShared;
                newLab.totalExperiments = item.totalExperiments;
                newLab.labFloorArea = item.labFloorArea;
                newLab.specializationId = item.specializationId;
                newLab.shiftId = item.shiftId;
                newLab.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newLab.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newLab.department = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newLab.degreeId = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newLab.degree = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degree).FirstOrDefault();
                newLab.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newLab.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newLab.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newLab.yearInDegreeId = item.yearInDegreeId;
                laboratories.Add(newLab);
            }

            laboratories = laboratories.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.year).ToList();
            ViewBag.Laboratories = laboratories;
            ViewBag.Count = laboratories.Count();
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
            if (laboratories.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && laboratories.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "Laboratories");
            }          
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "DataEntry")]
        public ActionResult DataEntryIndex(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }


            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID).ToList();

            List<Laboratories> laboratories = new List<Laboratories>();

            foreach (var item in labs)
            {
                Laboratories newLab = new Laboratories();
                newLab.id = item.id;
                newLab.collegeId = item.collegeId;
                newLab.labName = item.labName;
                newLab.totalExperiments = item.totalExperiments;
                newLab.labFloorArea = item.labFloorArea;
                newLab.specializationId = item.specializationId;
                newLab.shiftId = item.shiftId;
                newLab.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newLab.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newLab.department = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newLab.degreeId = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newLab.degree = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degree).FirstOrDefault();
                newLab.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newLab.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newLab.yearInDegreeId = item.yearInDegreeId;
                newLab.createdBy = item.createdBy;
                newLab.createdOn = item.createdOn;

                laboratories.Add(newLab);
            }

            laboratories = laboratories.OrderBy(l => l.degree).ToList();

            ViewBag.Laboratories = laboratories;
            ViewBag.Count = laboratories.Count();
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
            if (laboratories.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && laboratories.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "Laboratories");
            }
            ViewBag.count = laboratories.Count();
            return View(laboratories);
        }

        [HttpPost]
        [Authorize(Roles = "DataEntry")]
        public ActionResult DataEntryIndex(string collegeId, List<Laboratories> labslst)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (labslst.Count > 0)
            {
                foreach (var item in labslst)
                {

                    jntuh_college_lab uLab = new jntuh_college_lab();
                    uLab.id = item.id;
                    uLab.collegeId = userCollegeID;
                    uLab.specializationId = item.specializationId;
                    uLab.shiftId = item.shiftId;
                    uLab.yearInDegreeId = item.yearInDegreeId;
                    uLab.labName = item.labName;
                    uLab.totalExperiments = item.totalExperiments;
                    uLab.labFloorArea = item.labFloorArea;
                    uLab.createdBy = item.createdBy;
                    uLab.createdOn = item.createdOn;
                    uLab.updatedBy = userID;
                    uLab.updatedOn = DateTime.Now;
                    db.Entry(uLab).State = EntityState.Modified;
                    db.SaveChanges();
                }
                TempData["Success"] = "Laboratories is Updated successfully.";

            }
            return RedirectToAction("DataEntryIndex", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
           
            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID).ToList();

            List<Laboratories> laboratories = new List<Laboratories>();

            foreach (var item in labs)
            {
                Laboratories newLab = new Laboratories();
                newLab.id = item.id;
                newLab.collegeId = item.collegeId;
                newLab.labName = item.labName;
                newLab.IsShared =(bool)item.isShared;
                newLab.totalExperiments = item.totalExperiments;
                newLab.labFloorArea = item.labFloorArea;
                newLab.specializationId = item.specializationId;
                newLab.shiftId = item.shiftId;
                newLab.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newLab.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newLab.department = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newLab.degreeId = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newLab.degree = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degree).FirstOrDefault();
                newLab.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newLab.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newLab.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newLab.yearInDegreeId = item.yearInDegreeId;
                laboratories.Add(newLab);
            }
            laboratories = laboratories.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.year).ToList();
            ViewBag.Laboratories = laboratories;
            ViewBag.Count = laboratories.Count();
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

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
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
                    else if (id != null)
                    {
                        userCollegeID = db.jntuh_college_lab.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
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
            ViewBag.Year = db.jntuh_year_in_degree.Where(c => c.isActive == true);

            Laboratories laboratories = new Laboratories();
            laboratories.collegeId = userCollegeID;
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                   

                    List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID && l.id == id).ToList();

                    foreach (var item in labs)
                    {
                        laboratories.id = item.id;
                        laboratories.collegeId = item.collegeId;
                        laboratories.labName = item.labName;
                        laboratories.IsShared =(bool)item.isShared;
                        laboratories.totalExperiments = item.totalExperiments;
                        laboratories.labFloorArea = item.labFloorArea;
                        laboratories.specializationId = item.specializationId;
                        laboratories.shiftId = item.shiftId;
                        laboratories.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                        laboratories.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                        laboratories.department = db.jntuh_department.Where(d => d.id == laboratories.departmentId).Select(d => d.departmentName).FirstOrDefault();
                        laboratories.degreeId = db.jntuh_department.Where(d => d.id == laboratories.departmentId).Select(d => d.degreeId).FirstOrDefault();
                        laboratories.degree = db.jntuh_degree.Where(d => d.id == laboratories.degreeId).Select(d => d.degree).FirstOrDefault();
                        laboratories.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                        laboratories.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                        laboratories.yearInDegreeId = item.yearInDegreeId;
                    }
                    return PartialView("_LaboratoriesData", laboratories);
                }
                else
                {
                    laboratories.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    return PartialView("_LaboratoriesData", laboratories);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    Laboratories uLaboratories = db.jntuh_college_lab.Where(oc => oc.id == id).Select(a =>
                                               new Laboratories
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   specializationId = a.specializationId,
                                                   shiftId = a.shiftId,
                                                   yearInDegreeId = a.yearInDegreeId,
                                                   labName = a.labName,
                                                   IsShared=(bool)a.isShared,
                                                   labFloorArea = a.labFloorArea,
                                                   totalExperiments = a.totalExperiments,
                                                   createdBy = a.createdBy,
                                                   createdOn = a.createdOn,
                                                   updatedBy = a.updatedBy,
                                                   updatedOn = a.updatedOn,
                                                   jntuh_college = a.jntuh_college,
                                                   my_aspnet_users = a.my_aspnet_users,
                                                   my_aspnet_users1 = a.my_aspnet_users1
                                               }).FirstOrDefault();
                    return View("LaboratoriesData", uLaboratories);
                }
                {
                    Laboratories uLaboratories = new Laboratories();
                    uLaboratories.collegeId = userCollegeID;
                    ViewBag.IsUpdate = false;
                    return View("LaboratoriesData");
                }
            }





        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(Laboratories laboratories, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = laboratories.collegeId;
            }
            ViewBag.Degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
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
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Year = db.jntuh_year_in_degree.Where(c => c.isActive == true);
            if (ModelState.IsValid)
            {
                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_lab jntuh_college_lab = new jntuh_college_lab();
                        jntuh_college_lab.id = laboratories.id;
                        jntuh_college_lab.collegeId = userCollegeID;
                        jntuh_college_lab.specializationId = laboratories.specializationId;
                        jntuh_college_lab.shiftId = laboratories.shiftId;
                        jntuh_college_lab.yearInDegreeId = laboratories.yearInDegreeId;
                        jntuh_college_lab.labName = laboratories.labName;
                        jntuh_college_lab.isShared = laboratories.IsShared;
                        jntuh_college_lab.totalExperiments = laboratories.totalExperiments;
                        jntuh_college_lab.labFloorArea = laboratories.labFloorArea;
                        jntuh_college_lab.createdBy = userID;
                        jntuh_college_lab.createdOn = DateTime.Now;

                        db.jntuh_college_lab.Add(jntuh_college_lab);
                        db.SaveChanges();
                        TempData["Success"] = "Laboratories is Added successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_lab uLab = new jntuh_college_lab();

                        if (uLab != null)
                        {
                            uLab.id = laboratories.id;
                            uLab.collegeId = userCollegeID;
                            uLab.specializationId = laboratories.specializationId;
                            uLab.shiftId = laboratories.shiftId;
                            uLab.yearInDegreeId = laboratories.yearInDegreeId;
                            uLab.labName = laboratories.labName;
                            uLab.isShared = laboratories.IsShared;
                            uLab.totalExperiments = laboratories.totalExperiments;
                            uLab.labFloorArea = laboratories.labFloorArea;
                            uLab.createdBy = laboratories.createdBy;
                            uLab.createdOn = laboratories.createdOn;
                            uLab.updatedBy = userID;
                            uLab.updatedOn = DateTime.Now;
                            db.Entry(uLab).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "Laboratories is Updated successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_LaboratoriesData", laboratories);
            }
            else
            {
                return View("LaboratoriesData", laboratories);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_lab lab = db.jntuh_college_lab.Where(oc => oc.id == id).FirstOrDefault();           
            if (lab != null)
            {
                try
                {
                    db.jntuh_college_lab.Remove(lab);
                    db.SaveChanges();
                    TempData["Success"] = "Laboratories is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(lab.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_lab.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID = db.jntuh_college_lab.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
          
            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID && l.id == id).ToList();

            Laboratories laboratories = new Laboratories();
            foreach (var item in labs)
            {
                laboratories.id = item.id;
                laboratories.collegeId = item.collegeId;
                laboratories.labName = item.labName;
                laboratories.IsShared =(bool)item.isShared;
                laboratories.totalExperiments = item.totalExperiments;
                laboratories.labFloorArea = item.labFloorArea;
                laboratories.specializationId = item.specializationId;
                laboratories.shiftId = item.shiftId;
                laboratories.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                laboratories.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                laboratories.department = db.jntuh_department.Where(d => d.id == laboratories.departmentId).Select(d => d.departmentName).FirstOrDefault();
                laboratories.degreeId = db.jntuh_department.Where(d => d.id == laboratories.departmentId).Select(d => d.degreeId).FirstOrDefault();
                laboratories.degree = db.jntuh_degree.Where(d => d.id == laboratories.degreeId).Select(d => d.degree).FirstOrDefault();
                laboratories.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                laboratories.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                laboratories.yearInDegreeId = item.yearInDegreeId;
            }
            if (laboratories != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LaboratoriesDetails", laboratories);
                }
                else
                {
                    return View("LaboratoriesDetails", laboratories);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == userCollegeID).ToList();

            List<Laboratories> laboratories = new List<Laboratories>();

            foreach (var item in labs)
            {
                Laboratories newLab = new Laboratories();
                newLab.id = item.id;
                newLab.collegeId = item.collegeId;
                newLab.labName = item.labName;
                newLab.IsShared =(bool)item.isShared;
                newLab.totalExperiments = item.totalExperiments;
                newLab.labFloorArea = item.labFloorArea;
                newLab.specializationId = item.specializationId;
                newLab.shiftId = item.shiftId;
                newLab.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newLab.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newLab.department = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newLab.degreeId = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newLab.degree = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degree).FirstOrDefault();
                newLab.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newLab.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newLab.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newLab.yearInDegreeId = item.yearInDegreeId;
                laboratories.Add(newLab);
            }
            laboratories = laboratories.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ThenBy(ei=>ei.year).ToList();
            ViewBag.Laboratories = laboratories;
            ViewBag.Count = laboratories.Count();
            return View();
        }
    }
}
