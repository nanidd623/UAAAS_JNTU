using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Providers.Entities;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FFCAuditorsCampusController : BaseController
    {

        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            List<jntuh_ffc_auditor_campus> jntuh_ffc_auditor_campus = db.jntuh_ffc_auditor_campus.OrderBy(ac=>ac.Name).Select(ac => ac).ToList();
            return View(jntuh_ffc_auditor_campus);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ActionResult AddorEdit(int? id)
        {
            FFCAuditorsCampus fFCAuditorsCampus = new FFCAuditorsCampus();
            if (id != null)
            {
                jntuh_ffc_auditor_campus jntuh_ffc_auditor_campus = db.jntuh_ffc_auditor_campus.Where(ac => ac.id == id).Select(ac => ac).FirstOrDefault();
                fFCAuditorsCampus.id = jntuh_ffc_auditor_campus.id;
                fFCAuditorsCampus.Name = jntuh_ffc_auditor_campus.Name;
                fFCAuditorsCampus.Description = jntuh_ffc_auditor_campus.Description;
                fFCAuditorsCampus.isActive = jntuh_ffc_auditor_campus.isActive;
                fFCAuditorsCampus.createdBy = jntuh_ffc_auditor_campus.createdBy;
                fFCAuditorsCampus.createdOn = jntuh_ffc_auditor_campus.createdOn;
                ViewBag.IsUpdate = true;
            }
            else
            {
                ViewBag.IsUpdate = false;
            }
            return PartialView(fFCAuditorsCampus);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ActionResult AddorEdit(FFCAuditorsCampus fFCAuditorsCampus, string cmd)
        {            
            jntuh_ffc_auditor_campus jntuh_ffc_auditor_campus = new jntuh_ffc_auditor_campus();
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ModelState.IsValid)
            {
                if (cmd == "Update")
                {
                    var rowExists = (from s in db.jntuh_ffc_auditor_campus where s.Name == fFCAuditorsCampus.Name && s.id != fFCAuditorsCampus.id select s.Name);
                      if (rowExists.Count() == 0)
                      {
                          jntuh_ffc_auditor_campus.id = fFCAuditorsCampus.id;
                          jntuh_ffc_auditor_campus.Name = fFCAuditorsCampus.Name;
                          jntuh_ffc_auditor_campus.Description = fFCAuditorsCampus.Description;
                          jntuh_ffc_auditor_campus.isActive = fFCAuditorsCampus.isActive;
                          jntuh_ffc_auditor_campus.createdBy = fFCAuditorsCampus.createdBy;
                          jntuh_ffc_auditor_campus.createdOn = fFCAuditorsCampus.createdOn;
                          jntuh_ffc_auditor_campus.updatedBy = userid;
                          jntuh_ffc_auditor_campus.updatedOn = DateTime.Now;
                          db.Entry(jntuh_ffc_auditor_campus).State = EntityState.Modified;
                          db.SaveChanges();
                          TempData["Success"] = "Campus updated successfully";
                      }
                      else
                      {
                          TempData["Error"] = "Campus alredy exists";
                      }
                }
                else
                {    
                    var rowExists = (from s in db.jntuh_ffc_auditor_campus where s.Name == fFCAuditorsCampus.Name select s.Name);
                    if (rowExists.Count() == 0)
                    {
                        jntuh_ffc_auditor_campus.Name = fFCAuditorsCampus.Name;
                        jntuh_ffc_auditor_campus.Description = fFCAuditorsCampus.Description;
                        jntuh_ffc_auditor_campus.isActive = true;
                        jntuh_ffc_auditor_campus.createdBy = userid;
                        jntuh_ffc_auditor_campus.createdOn = DateTime.Now;
                        db.jntuh_ffc_auditor_campus.Add(jntuh_ffc_auditor_campus);
                        db.SaveChanges();
                        TempData["Success"] = "Campus saved successfully";
                    }
                    else
                    {
                        TempData["Error"]="Campus alredy exists";
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ActionResult Details(int id)
        {
            jntuh_ffc_auditor_campus jntuh_ffc_auditor_campus = db.jntuh_ffc_auditor_campus.Find(id);
            return PartialView(jntuh_ffc_auditor_campus);
        }


    }
}
