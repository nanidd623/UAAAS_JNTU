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
    public class UniversityController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /University/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_university = db.jntuh_university.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_university.ToList());
        }

        //
        // GET: /University/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_university jntuh_university = db.jntuh_university.Find(id);
            return View(jntuh_university);
        }

        //
        // GET: /University/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /University/Create

        [HttpPost]
        public ActionResult Create(jntuh_university jntuh_university)
        {
            if (ModelState.IsValid)
            {
                var rowWxists = (from s in db.jntuh_university
                                 where s.universityName == jntuh_university.universityName
                                 select s.universityName);
                if (rowWxists.Count() == 0)
                {
                    db.jntuh_university.Add(jntuh_university);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "University Name is already exists. Please enter a different University Name.";
                }  
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.updatedBy);
            return View(jntuh_university);
        }
        
        //
        // GET: /University/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_university jntuh_university = db.jntuh_university.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.updatedBy);
            return View(jntuh_university);
        }

        //
        // POST: /University/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_university jntuh_university)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_university
                                 where s.universityName == jntuh_university.universityName && s.id != jntuh_university.id
                                 select s.universityName);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_university).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_university);
                }
                else
                {
                    TempData["Error"] = "University Name is already exists. Please enter a different University Name.";
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_university.updatedBy);
            return View(jntuh_university);
        }

        //
        // GET: /University/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_university jntuh_university = db.jntuh_university.Find(id);
            return View(jntuh_university);
        }

        //
        // POST: /University/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_university jntuh_university = db.jntuh_university.Find(id);
            db.jntuh_university.Remove(jntuh_university);
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