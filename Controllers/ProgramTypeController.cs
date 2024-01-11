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
    public class ProgramTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /ProgramType/

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_program_type = db.jntuh_program_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_program_type.ToList());
        }

        //
        // GET: /ProgramType/Details/5

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_program_type jntuh_program_type = db.jntuh_program_type.Find(id);
            return View(jntuh_program_type);
        }

        //
        // GET: /ProgramType/Create

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /ProgramType/Create

        [HttpPost]
        public ActionResult Create(jntuh_program_type jntuh_program_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_program_type
                                 where s.programType == jntuh_program_type.programType
                                 select s.programType);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_program_type.Add(jntuh_program_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Program Type is already exists. Please enter a different Program Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.updatedBy);
            return View(jntuh_program_type);
        }
        
        //
        // GET: /ProgramType/Edit/5

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_program_type jntuh_program_type = db.jntuh_program_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.updatedBy);
            return View(jntuh_program_type);
        }

        //
        // POST: /ProgramType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_program_type jntuh_program_type)
        {
            if (ModelState.IsValid)
            {
                 var rowExists = (from s in db.jntuh_program_type
                                 where s.programType == jntuh_program_type.programType && s.id != jntuh_program_type.id
                                 select s.programType);
                 if (rowExists.Count() == 0)
                 {
                     db.Entry(jntuh_program_type).State = EntityState.Modified;
                     db.SaveChanges();
                     TempData["Success"] = "Updated successfully.";
                     return View(jntuh_program_type);
                 }
                 else
                 {
                     TempData["Error"] = "Program Type is already exists. Please enter a different Program Type.";
                     return View(jntuh_program_type);
                 }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_program_type.updatedBy);
            return View(jntuh_program_type);
        }

        //
        // GET: /ProgramType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_program_type jntuh_program_type = db.jntuh_program_type.Find(id);
            return View(jntuh_program_type);
        }

        //
        // POST: /ProgramType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_program_type jntuh_program_type = db.jntuh_program_type.Find(id);
            db.jntuh_program_type.Remove(jntuh_program_type);
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