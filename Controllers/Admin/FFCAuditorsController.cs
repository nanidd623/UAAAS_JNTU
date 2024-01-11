using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.Data;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FFCAuditorsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /FFCAuditors/

        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult FFCAuditorsCreate(int? id)
        {
            if (id != null)
            {
                ViewBag.IsUpdate = true;
                ViewBag.Campus = db.jntuh_ffc_auditor_campus.Where(ac => ac.isActive == true).Select(ac => ac).OrderBy(ac => ac.Name).ToList();
                //List<SelectListItem> Campus = new List<SelectListItem>();
                //Campus.Add(new SelectListItem { Text = "JNTUH", Value = "JNTUH" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEH", Value = "JNTUH CEH" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEJ Jagityal", Value = "JNTUH CEJ Jagityal" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEM Manthani", Value = "JNTUH CEM Manthani" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CES Sulthanpur", Value = "JNTUH CES Sulthanpur" });

                if (id > 0)
                {
                    FFCAuditors FFCAuditors = new FFCAuditors();
                    jntuh_ffc_auditor item = db.jntuh_ffc_auditor.Where(i => i.id == id).Select(i => i).FirstOrDefault();
                    FFCAuditors Auditors = new FFCAuditors();

                    Auditors.id = item.id;
                    Auditors.auditorName = item.auditorName;
                    Auditors.auditorDepartmentID = (int)item.auditorDepartmentID;
                    Auditors.auditorDesignationID = (int)item.auditorDesignationID;
                    Auditors.Department = db.jntuh_department.Where(d => d.id == item.auditorDepartmentID && d.isActive == true).Select(d => d.departmentName).FirstOrDefault().ToString();
                    Auditors.Designation = db.jntuh_designation.Where(d => d.id == item.auditorDesignationID && d.isActive == true).Select(d => d.designation).FirstOrDefault().ToString();
                    Auditors.auditorPreferredDesignation = item.auditorPreferredDesignation;
                    Auditors.auditorEmail1 = item.auditorEmail1;
                    Auditors.auditorEmail2 = item.auditorEmail2;
                    Auditors.auditorMobile1 = item.auditorMobile1;
                    Auditors.auditorMobile2 = item.auditorMobile2;
                    Auditors.auditorPlace = item.auditorPlace;
                    Auditors.createdBy = (int)item.createdBy;
                    Auditors.createdOn = item.createdOn;
                    Auditors.isActive = item.isActive;
                    ViewBag.Campus = db.jntuh_ffc_auditor_campus.Where(ac => ac.isActive == true).Select(ac => ac).OrderBy(ac => ac.Name).ToList();
                    return PartialView("~/Views/Admin/FFCAuditorsCreate.cshtml", Auditors);
                }
                else
                {
                    return PartialView("~/Views/Admin/FFCAuditorsCreate.cshtml");
                }
            }
            else
            {
                FFCAuditors FFCAuditors = new FFCAuditors();
                //List<SelectListItem> Campus = new List<SelectListItem>();
                //Campus.Add(new SelectListItem { Text = "JNTUH", Value = "JNTUH" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEH", Value = "JNTUH CEH" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEJ Jagityal", Value = "JNTUH CEJ Jagityal" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CEM Manthani", Value = "JNTUH CEM Manthani" });
                //Campus.Add(new SelectListItem { Text = "JNTUH CES Sulthanpur", Value = "JNTUH CES Sulthanpur" });



                ViewBag.Campus = db.jntuh_ffc_auditor_campus.Where(ac=>ac.isActive==true).Select(ac=>ac).OrderBy(ac=>ac.Name).ToList();
                return PartialView("~/Views/Admin/FFCAuditorsCreate.cshtml", FFCAuditors);
            }

        }
        [Authorize(Roles = "Admin")]
        public ActionResult FFCAuditorsList()
        {
            List<FFCAuditors> FFCAuditors = new List<FFCAuditors>();
            List<jntuh_ffc_auditor> jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(i => i).ToList();
            foreach (var item in jntuh_ffc_auditor)
            {
                FFCAuditors Auditors = new FFCAuditors();
                Auditors.id = item.id;
                Auditors.auditorName = item.auditorName.Replace(".", ". ");
                Auditors.auditorDepartmentID = (int)item.auditorDepartmentID;
                Auditors.Department = db.jntuh_department.Where(d => d.id == item.auditorDepartmentID && d.isActive == true).Select(d => d.departmentName).FirstOrDefault().ToString();
                Auditors.Designation = db.jntuh_designation.Where(d => d.id == item.auditorDesignationID && d.isActive == true).Select(d => d.designation).FirstOrDefault().ToString();
                Auditors.auditorDesignationID = (int)item.auditorDesignationID;
                Auditors.auditorEmail1 = item.auditorEmail1;
                Auditors.auditorMobile1 = item.auditorMobile1;
                Auditors.isActive = item.isActive;
                FFCAuditors.Add(Auditors);
            }
            return View("~/Views/Admin/FFCAuditorsList.cshtml", FFCAuditors);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult FFCAuditorsCreate(FFCAuditors FFCAuditors, string cmd)
        {

            if (cmd == "Add")
            {
                jntuh_ffc_auditor jntuh_ffc_auditor = new Models.jntuh_ffc_auditor();
                int Createdby = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                FFCAuditors.createdBy = Createdby;
                FFCAuditors.createdOn = DateTime.Now;
                FFCAuditors.isActive = true;
                if (ModelState.IsValid == true)
                {
                    jntuh_ffc_auditor.auditorName = FFCAuditors.auditorName;
                    jntuh_ffc_auditor.auditorDepartmentID = FFCAuditors.auditorDepartmentID;
                    jntuh_ffc_auditor.auditorDesignationID = FFCAuditors.auditorDesignationID;
                    jntuh_ffc_auditor.auditorEmail1 = FFCAuditors.auditorEmail1;
                    jntuh_ffc_auditor.auditorEmail2 = FFCAuditors.auditorEmail2;
                    jntuh_ffc_auditor.auditorPreferredDesignation = FFCAuditors.auditorPreferredDesignation;
                    jntuh_ffc_auditor.auditorMobile1 = FFCAuditors.auditorMobile1;
                    jntuh_ffc_auditor.auditorMobile2 = FFCAuditors.auditorMobile2;
                    jntuh_ffc_auditor.auditorPlace = FFCAuditors.auditorPlace;
                    jntuh_ffc_auditor.createdBy = Createdby;
                    jntuh_ffc_auditor.createdOn = DateTime.Now;
                    jntuh_ffc_auditor.isActive = true;
                    db.jntuh_ffc_auditor.Add(jntuh_ffc_auditor);
                    db.SaveChanges();
                    TempData["Success"] = "Auditors Details  Added successfully.";
                }
            }
            else
            {
                jntuh_ffc_auditor jntuh_ffc_auditor = new Models.jntuh_ffc_auditor();
                int Createdby = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                if (ModelState.IsValid == true)
                {
                    jntuh_ffc_auditor.id = FFCAuditors.id;
                    jntuh_ffc_auditor.auditorName = FFCAuditors.auditorName;
                    jntuh_ffc_auditor.auditorDepartmentID = FFCAuditors.auditorDepartmentID;
                    jntuh_ffc_auditor.auditorDesignationID = FFCAuditors.auditorDesignationID;
                    jntuh_ffc_auditor.auditorEmail1 = FFCAuditors.auditorEmail1;
                    jntuh_ffc_auditor.auditorEmail2 = FFCAuditors.auditorEmail2;
                    jntuh_ffc_auditor.auditorPreferredDesignation = FFCAuditors.auditorPreferredDesignation;
                    jntuh_ffc_auditor.auditorMobile1 = FFCAuditors.auditorMobile1;
                    jntuh_ffc_auditor.auditorMobile2 = FFCAuditors.auditorMobile2;
                    jntuh_ffc_auditor.auditorPlace = FFCAuditors.auditorPlace;
                    jntuh_ffc_auditor.createdBy = FFCAuditors.createdBy;
                    jntuh_ffc_auditor.createdOn = FFCAuditors.createdOn;
                    jntuh_ffc_auditor.updatedBy = Createdby;
                    jntuh_ffc_auditor.updatedOn = DateTime.Now;
                    jntuh_ffc_auditor.isActive = FFCAuditors.isActive;
                    db.Entry(jntuh_ffc_auditor).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Auditors Details  Updated successfully.";
                }

            }
            return RedirectToAction("FFCAuditorsList");
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
            List<jntuh_department> depts = new List<jntuh_department>();
            string existingDepts = string.Empty;

            foreach (var item in db.jntuh_department.Where(s => s.isActive == true).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new jntuh_department { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            //return db.jntuh_department.Where(d => d.isActive == true).OrderBy(d => d.departmentName).ToList();
            return depts;
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDesignation()
        {
            var DesignationList = this.Designations();
            var DesignationData = DesignationList.Select(a => new SelectListItem()
            {
                Text = a.designation,
                Value = a.id.ToString(),
            });
            return Json(DesignationData, JsonRequestBehavior.AllowGet);
        }
        private List<jntuh_designation> Designations()
        {
            return db.jntuh_designation.Where(d => d.isActive == true).ToList();
        }

    }
}
