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
    public class LabsNewController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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


            List<jntuh_college_laboratories> laboratories = db.jntuh_college_laboratories.Where(l => l.CollegeID == userCollegeID).ToList();
            laboratories = laboratories.OrderBy(ei => ei.jntuh_lab_master.jntuh_degree.degreeDisplayOrder).ThenBy(ei => ei.jntuh_lab_master.jntuh_department.departmentName).ThenBy(ei => ei.jntuh_lab_master.jntuh_specialization.specializationName).ThenBy(ei => ei.jntuh_lab_master.Year).ToList();
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
            return View(laboratories);
        }

        private List<jntuh_department> Departments(int id)
        {
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id).OrderBy(d => d.departmentName).ToList();
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

        private List<jntuh_lab_master> LabMaster(int degreeId, int departmentId, int specializationId, int year, int semester)
        {
            return db.jntuh_lab_master.Where(d => d.isActive == true && d.DegreeID == degreeId 
                                            && d.DepartmentID == departmentId 
                                            &&  d.SpecializationID == specializationId 
                                            &&  d.Year == year  
                                            && d.Semester == semester)                                            
                                            .OrderBy(d=>d.LabName).ToList();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LabmasterDetails(string degreeId, string departmentId, string specializationId, string year, string semester)
        {
            if (degreeId == string.Empty && departmentId == string.Empty && specializationId == string.Empty && year == string.Empty && semester == string.Empty)
            {
                degreeId = "0"; departmentId = "0"; specializationId = "0"; year = "0"; semester = "0";
            }
            var LabMasterList = this.LabMaster(Convert.ToInt32(degreeId), Convert.ToInt32(departmentId), Convert.ToInt32(specializationId), Convert.ToInt32(year), Convert.ToInt32(semester));

            var LabmasterData = LabMasterList.Select(a => new SelectListItem()
            {
                Text = a.LabName,
                Value = a.LabName.ToString(),
            });
            return Json(LabmasterData.Distinct().ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
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
            int[] DepartmentIDs = db.jntuh_lab_master.Where(l => l.isActive == true).Select(l => l.DepartmentID).Distinct().ToArray();
            int[] SpecializationIDs = db.jntuh_lab_master.Where(l => l.isActive == true).Select(l => l.SpecializationID).Distinct().ToArray();
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true && DepartmentIDs.Contains(d.id));
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true && SpecializationIDs.Contains(s.id));         
            ViewBag.Year = db.jntuh_year_in_degree.Where(c => c.isActive == true);
            List<SelectListItem> Semester = new List<SelectListItem>();
            for (int i = 1; i <= 2; i++)
            {
                Semester.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.Semester = Semester;
            int[] EqupIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == userCollegeID).Select(l => l.EquipmentID).ToArray();
            ViewBag.Labnames = db.jntuh_lab_master.Where(s => s.isActive == true && !EqupIDs.Contains(s.id)).Select(s => new { EqupmentId = s.id, Labname = s.LabName }).ToList();

            Lab laboratories = new Lab();
            laboratories.collegeId = userCollegeID;

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,

                                    }).FirstOrDefault();

                    return PartialView("_LaboratoriesData", laboratories);
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    laboratories.collegeId = userCollegeID;
                    //laboratories.EquipmentNo = 1;                   
                    return PartialView("_LaboratoriesData", laboratories);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    laboratories = (from m in db.jntuh_lab_master
                                    join labs in db.jntuh_college_laboratories on m.id equals labs.EquipmentID
                                    where (labs.CollegeID == userCollegeID && labs.id == id)
                                    select new Lab
                                    {
                                        id = labs.id,
                                        collegeId = userCollegeID,
                                        EquipmentID = labs.EquipmentID,
                                        EquipmentName = m.EquipmentName,
                                        LabEquipmentName = labs.EquipmentName,
                                        EquipmentNo = labs.EquipmentNo,
                                        Make = labs.Make,
                                        Model = labs.Model,
                                        EquipmentUniqueID = labs.EquipmentUniqueID,
                                        AvailableUnits = labs.AvailableUnits,
                                        AvailableArea = labs.AvailableArea,
                                        RoomNumber = labs.RoomNumber,
                                        createdBy = labs.createdBy,
                                        createdOn = labs.createdOn,
                                        IsActive = true,

                                        degreeId = m.DegreeID,
                                        departmentId = m.DepartmentID,
                                        specializationId = m.SpecializationID,
                                        degree = m.jntuh_degree.degree,
                                        department = m.jntuh_department.departmentName,
                                        specializationName = m.jntuh_specialization.specializationName,
                                        year = m.Year,
                                        Semester = m.Semester,
                                        Labcode = m.Labcode,
                                        LabName = m.LabName,

                                    }).FirstOrDefault();
                }
                else
                {
                    ViewBag.IsUpdate = false;
                    laboratories.collegeId = userCollegeID;
                    //laboratories.EquipmentNo = 1;   
                }
                return View("LaboratoriesData", laboratories);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(Lab laboratories, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = laboratories.collegeId;
            }
            if (ModelState.IsValid)
            {
                jntuh_college_laboratories jntuh_college_laboratories = new jntuh_college_laboratories();
                jntuh_college_laboratories.CollegeID = userCollegeID;
                jntuh_college_laboratories.EquipmentID = laboratories.EquipmentID;
                jntuh_college_laboratories.Make = laboratories.Make;
                jntuh_college_laboratories.Model = laboratories.Model;
                jntuh_college_laboratories.EquipmentUniqueID = laboratories.EquipmentUniqueID;
                jntuh_college_laboratories.EquipmentName = laboratories.EquipmentName;
                jntuh_college_laboratories.AvailableUnits = laboratories.AvailableUnits;
                jntuh_college_laboratories.AvailableArea = laboratories.AvailableArea;
                jntuh_college_laboratories.RoomNumber = laboratories.RoomNumber;
                jntuh_college_laboratories.EquipmentNo = laboratories.EquipmentNo;
                jntuh_college_laboratories.isActive = true;

                if (cmd == "Save")
                {
                    jntuh_college_laboratories.createdBy = userID;
                    jntuh_college_laboratories.createdOn = DateTime.Now;
                    db.jntuh_college_laboratories.Add(jntuh_college_laboratories);
                    db.SaveChanges();
                    TempData["Success"] = "Lab Added Successfully.";
                }
                else
                {
                    jntuh_college_laboratories.id = (int)laboratories.id;
                    jntuh_college_laboratories.createdBy = laboratories.createdBy;
                    jntuh_college_laboratories.createdOn = laboratories.createdOn;
                    jntuh_college_laboratories.updatedBy = userID;
                    jntuh_college_laboratories.updatedOn = DateTime.Now;
                    jntuh_college_laboratories.isActive = true;
                    db.Entry(jntuh_college_laboratories).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Lab Updated Successfully.";
                }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_laboratories lab = db.jntuh_college_laboratories.Where(oc => oc.id == id).FirstOrDefault();
            int userCollegeId = lab.CollegeID;
            if (lab != null)
            {
                try
                {
                    db.jntuh_college_laboratories.Remove(lab);
                    db.SaveChanges();
                    TempData["Success"] = "Laboratories is Deleted successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

    }
}
