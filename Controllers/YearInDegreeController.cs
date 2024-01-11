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
    public class YearInDegreeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /YearInDegree/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_year_in_degree = db.jntuh_year_in_degree.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_year_in_degree.ToList());
        }

        //
        // GET: /YearInDegree/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_year_in_degree jntuh_year_in_degree = db.jntuh_year_in_degree.Find(id);
            return View(jntuh_year_in_degree);
        }

        //
        // GET: /YearInDegree/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /YearInDegree/Create

        [HttpPost]
        public ActionResult Create(jntuh_year_in_degree jntuh_year_in_degree)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_year_in_degree
                                 where s.yearInDegree == jntuh_year_in_degree.yearInDegree
                                 select s.yearInDegree);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_year_in_degree.Add(jntuh_year_in_degree);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Year In Degree is already exists. Please enter a different Year In Degree.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.updatedBy);
            return View(jntuh_year_in_degree);
        }
        
        //
        // GET: /YearInDegree/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_year_in_degree jntuh_year_in_degree = db.jntuh_year_in_degree.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.updatedBy);
            return View(jntuh_year_in_degree);
        }

        //
        // POST: /YearInDegree/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_year_in_degree jntuh_year_in_degree)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_year_in_degree
                                 where s.yearInDegree == jntuh_year_in_degree.yearInDegree && s.id != jntuh_year_in_degree.id
                                 select s.yearInDegree);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_year_in_degree).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_year_in_degree);
                }
                else
                {
                    TempData["Error"] = "Year In Degree is already exists. Please enter a different Year In Degree.";
                    return View(jntuh_year_in_degree);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_year_in_degree.updatedBy);
            return View(jntuh_year_in_degree);
        }

        //
        // GET: /YearInDegree/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_year_in_degree jntuh_year_in_degree = db.jntuh_year_in_degree.Find(id);
            return View(jntuh_year_in_degree);
        }

        //
        // POST: /YearInDegree/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_year_in_degree jntuh_year_in_degree = db.jntuh_year_in_degree.Find(id);
            db.jntuh_year_in_degree.Remove(jntuh_year_in_degree);
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