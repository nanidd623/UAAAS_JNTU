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
    public class ShiftController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Shift/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_shift = db.jntuh_shift.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_shift.ToList());
        }

        //
        // GET: /Shift/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_shift jntuh_shift = db.jntuh_shift.Find(id);
            return View(jntuh_shift);
        }

        //
        // GET: /Shift/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /Shift/Create

        [HttpPost]
        public ActionResult Create(jntuh_shift jntuh_shift)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_shift
                                 where s.shiftName == jntuh_shift.shiftName
                                 select s.shiftName);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_shift.Add(jntuh_shift);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Shift Name is already exists. Please enter a different Shift Name.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.updatedBy);
            return View(jntuh_shift);
        }
        
        //
        // GET: /Shift/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_shift jntuh_shift = db.jntuh_shift.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.updatedBy);
            return View(jntuh_shift);
        }

        //
        // POST: /Shift/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_shift jntuh_shift)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_shift
                                 where s.shiftName == jntuh_shift.shiftName && s.id != jntuh_shift.id
                                 select s.shiftName);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_shift).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_shift);
                }
                else
                {
                    TempData["Error"] = "Shift Name is already exists. Please enter a different Shift Name.";
                    return View(jntuh_shift);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_shift.updatedBy);
            return View(jntuh_shift);
        }

        //
        // GET: /Shift/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_shift jntuh_shift = db.jntuh_shift.Find(id);
            return View(jntuh_shift);
        }

        //
        // POST: /Shift/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_shift jntuh_shift = db.jntuh_shift.Find(id);
            db.jntuh_shift.Remove(jntuh_shift);
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