using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DesirableRequirementTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /DesirableRequirementType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_desirable_requirement_type = db.jntuh_desirable_requirement_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_desirable_requirement_type.ToList());
        }

        //
        // GET: /DesirableRequirementType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_desirable_requirement_type jntuh_desirable_requirement_type = db.jntuh_desirable_requirement_type.Find(id);
            return View(jntuh_desirable_requirement_type);
        }

        //
        // GET: /DesirableRequirementType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /DesirableRequirementType/Create

        [HttpPost]
        public ActionResult Create(jntuh_desirable_requirement_type jntuh_desirable_requirement_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_desirable_requirement_type
                                 where (s.requirementType == jntuh_desirable_requirement_type.requirementType && s.isHostelRequirement == jntuh_desirable_requirement_type.isHostelRequirement)
                                 select s.requirementType);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_desirable_requirement_type.Add(jntuh_desirable_requirement_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Requirement Type is already exists. Please enter a different Requirement Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.updatedBy);
            return View(jntuh_desirable_requirement_type);
        }
        
        //
        // GET: /DesirableRequirementType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_desirable_requirement_type jntuh_desirable_requirement_type = db.jntuh_desirable_requirement_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.updatedBy);
            return View(jntuh_desirable_requirement_type);
        }

        //
        // POST: /DesirableRequirementType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_desirable_requirement_type jntuh_desirable_requirement_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_desirable_requirement_type
                                 where s.requirementType == jntuh_desirable_requirement_type.requirementType && s.isHostelRequirement == jntuh_desirable_requirement_type.isHostelRequirement && s.id != jntuh_desirable_requirement_type.id
                                 select s.requirementType);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_desirable_requirement_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_desirable_requirement_type);
                }
                else
                {
                    TempData["Error"] = "Requirement Type is already exists. Please enter a different Requirement Type.";
                    return View(jntuh_desirable_requirement_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_desirable_requirement_type.updatedBy);
            return View(jntuh_desirable_requirement_type);
        }

        //
        // GET: /DesirableRequirementType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_desirable_requirement_type jntuh_desirable_requirement_type = db.jntuh_desirable_requirement_type.Find(id);
            return View(jntuh_desirable_requirement_type);
        }

        //
        // POST: /DesirableRequirementType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_desirable_requirement_type jntuh_desirable_requirement_type = db.jntuh_desirable_requirement_type.Find(id);
            db.jntuh_desirable_requirement_type.Remove(jntuh_desirable_requirement_type);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}