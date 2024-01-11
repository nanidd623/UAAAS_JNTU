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
    public class SportsTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /SportsType/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_sports_type = db.jntuh_sports_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_sports_type.ToList());
        }

        //
        // GET: /SportsType/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_sports_type jntuh_sports_type = db.jntuh_sports_type.Find(id);
            return View(jntuh_sports_type);
        }

        //
        // GET: /SportsType/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /SportsType/Create

        [HttpPost]
        public ActionResult Create(jntuh_sports_type jntuh_sports_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_sports_type
                                 where s.sportType == jntuh_sports_type.sportType
                                 select s.sportType);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_sports_type.Add(jntuh_sports_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Sports Type is already exists. Please enter a different Sports Type.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.updatedBy);
            return View(jntuh_sports_type);
        }
        
        //
        // GET: /SportsType/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_sports_type jntuh_sports_type = db.jntuh_sports_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.updatedBy);
            return View(jntuh_sports_type);
        }

        //
        // POST: /SportsType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_sports_type jntuh_sports_type)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_sports_type
                                 where s.sportType == jntuh_sports_type.sportType && s.id != jntuh_sports_type.id
                                 select s.sportType);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_sports_type).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_sports_type);
                }
                else
                {
                    TempData["Error"] = "Sports Type is already exists. Please enter a different Sports Type.";
                    return View(jntuh_sports_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_sports_type.updatedBy);
            return View(jntuh_sports_type);
        }

        //
        // GET: /SportsType/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_sports_type jntuh_sports_type = db.jntuh_sports_type.Find(id);
            return View(jntuh_sports_type);
        }

        //
        // POST: /SportsType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_sports_type jntuh_sports_type = db.jntuh_sports_type.Find(id);
            db.jntuh_sports_type.Remove(jntuh_sports_type);
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