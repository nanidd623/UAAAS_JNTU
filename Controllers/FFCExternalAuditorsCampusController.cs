using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FFCExternalAuditorsCampusController : BaseController 
    {
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            List<jntuh_ffc_external_auditor_groups> jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.OrderBy(ac => ac.Group).ThenBy(ac => ac.University).Select(ac => ac).ToList();
            return View(jntuh_ffc_external_auditor_groups);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ActionResult AddorEdit(int? id)
        {
            FFCExternalAuditorsCampus fFCExternalAuditorsCampus = new FFCExternalAuditorsCampus();
            if (id != null)
            {
                jntuh_ffc_external_auditor_groups jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Where(ac => ac.id == id).Select(ac => ac).FirstOrDefault();
                fFCExternalAuditorsCampus.id = jntuh_ffc_external_auditor_groups.id;
                fFCExternalAuditorsCampus.University = jntuh_ffc_external_auditor_groups.University;
                fFCExternalAuditorsCampus.Group = jntuh_ffc_external_auditor_groups.Group;
                fFCExternalAuditorsCampus.isActive = jntuh_ffc_external_auditor_groups.isActive;
                fFCExternalAuditorsCampus.createdBy = jntuh_ffc_external_auditor_groups.createdBy;
                fFCExternalAuditorsCampus.createdOn = jntuh_ffc_external_auditor_groups.createdOn;
                fFCExternalAuditorsCampus.isEditable = jntuh_ffc_external_auditor_groups.isEditable;
                ViewBag.IsUpdate = true;
            }
            else
            {
                ViewBag.IsUpdate = false;
            }
            return PartialView(fFCExternalAuditorsCampus);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ActionResult AddorEdit(FFCExternalAuditorsCampus fFCExternalAuditorsCampus, string cmd)
        {
            jntuh_ffc_external_auditor_groups jntuh_ffc_external_auditor_groups = new jntuh_ffc_external_auditor_groups();
            jntuh_ffc_auditor_campus jntuh_ffc_auditor_campus = new Models.jntuh_ffc_auditor_campus();

            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ModelState.IsValid)
            {
                if (cmd == "Update")
                {
                    var rowExists = (from s in db.jntuh_ffc_external_auditor_groups where s.University == fFCExternalAuditorsCampus.University && s.id != fFCExternalAuditorsCampus.id select s.University);
                    if (rowExists.Count() == 0)
                    {
                        #region jntuh_ffc_external_auditor_groups		
                        jntuh_ffc_external_auditor_groups.id = fFCExternalAuditorsCampus.id;
                        jntuh_ffc_external_auditor_groups.University = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_external_auditor_groups.Group = fFCExternalAuditorsCampus.Group;
                        jntuh_ffc_external_auditor_groups.isActive = fFCExternalAuditorsCampus.isActive;
                        jntuh_ffc_external_auditor_groups.createdBy = fFCExternalAuditorsCampus.createdBy;
                        jntuh_ffc_external_auditor_groups.createdOn = fFCExternalAuditorsCampus.createdOn;
                        jntuh_ffc_external_auditor_groups.updatedBy = userid;
                        jntuh_ffc_external_auditor_groups.updatedOn = DateTime.Now;
                        jntuh_ffc_external_auditor_groups.isEditable = fFCExternalAuditorsCampus.isEditable;
                        db.Entry(jntuh_ffc_external_auditor_groups).State = EntityState.Modified;
                        db.SaveChanges(); 
	                    #endregion

                        #region jntuh_ffc_auditor_campus
                        int campusId = db.jntuh_ffc_auditor_campus.Where(c => c.Name == fFCExternalAuditorsCampus.University).Select(c => c.id).FirstOrDefault();
                        jntuh_ffc_auditor_campus.id = campusId;
                        jntuh_ffc_auditor_campus.Name = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_auditor_campus.Description = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_auditor_campus.isActive = fFCExternalAuditorsCampus.isActive;
                        jntuh_ffc_auditor_campus.createdBy = fFCExternalAuditorsCampus.createdBy;
                        jntuh_ffc_auditor_campus.createdOn = fFCExternalAuditorsCampus.createdOn;
                        jntuh_ffc_auditor_campus.updatedBy = userid;
                        jntuh_ffc_auditor_campus.updatedOn = DateTime.Now;
                        db.Entry(jntuh_ffc_auditor_campus).State = EntityState.Modified;
                        db.SaveChanges(); 
                        #endregion
                        TempData["Success"] = "Data updated successfully";
                    }
                    else
                    {
                        TempData["Error"] = "University alredy exists";
                    }
                }
                else
                {
                    var rowExists = (from s in db.jntuh_ffc_external_auditor_groups where s.University == fFCExternalAuditorsCampus.University select s.University);
                    if (rowExists.Count() == 0)
                    {
                        #region jntuh_ffc_external_auditor_groups
                        jntuh_ffc_external_auditor_groups.University = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_external_auditor_groups.Group = fFCExternalAuditorsCampus.Group;
                        jntuh_ffc_external_auditor_groups.isActive = true;
                        jntuh_ffc_external_auditor_groups.createdBy = userid;
                        jntuh_ffc_external_auditor_groups.createdOn = DateTime.Now;
                        jntuh_ffc_external_auditor_groups.isEditable = true;
                        db.jntuh_ffc_external_auditor_groups.Add(jntuh_ffc_external_auditor_groups);
                        db.SaveChanges(); 
                        #endregion

                        #region jntuh_ffc_auditor_campus
                        jntuh_ffc_auditor_campus.Name = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_auditor_campus.Description = fFCExternalAuditorsCampus.University;
                        jntuh_ffc_auditor_campus.isActive = true;
                        jntuh_ffc_auditor_campus.createdBy = userid;
                        jntuh_ffc_auditor_campus.createdOn = DateTime.Now;
                        db.jntuh_ffc_auditor_campus.Add(jntuh_ffc_auditor_campus);
                        db.SaveChanges(); 
                        #endregion

                        TempData["Success"] = "Data saved successfully";
                    }
                    else
                    {
                        TempData["Error"] = "University alredy exists";
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ActionResult Details(int id)
        {
            jntuh_ffc_external_auditor_groups jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Find(id);
            return PartialView(jntuh_ffc_external_auditor_groups);
        }

    }
}
