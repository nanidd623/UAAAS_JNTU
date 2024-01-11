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
    public class DegreeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Degree/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_degree = db.jntuh_degree.Include(j => j.jntuh_degree_type).Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            return View(jntuh_degree.ToList());
        }

        //
        // GET: /Degree/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            jntuh_degree jntuh_degree = db.jntuh_degree.Find(id);
            return View(jntuh_degree);
        }

        //
        // GET: /Degree/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            //ViewBag.degreeTypeId = new SelectList(db.jntuh_degree_type, "id", "degreeType");
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /Degree/Create

        [HttpPost]
        public ActionResult Create(jntuh_degree jntuh_degree)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_degree
                                 where s.degree == jntuh_degree.degree 
                                 select s.degree);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_degree.Add(jntuh_degree);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Degree is already exists. Please enter a different Degree.";
                }
            }

            //ViewBag.degreeTypeId = new SelectList(db.jntuh_degree_type, "id", "degreeType", jntuh_degree.degreeTypeId);
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.updatedBy);
            return View(jntuh_degree);
        }
        
        //
        // GET: /Degree/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_degree jntuh_degree = db.jntuh_degree.Find(id);
            //ViewBag.degreeTypeId = new SelectList(db.jntuh_degree_type, "id", "degreeType", jntuh_degree.degreeTypeId);
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.updatedBy);
            return View(jntuh_degree);
        }

        //
        // POST: /Degree/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_degree jntuh_degree)
        {
            ViewBag.DegreeType = (from s in db.jntuh_degree_type
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree.updatedBy);

            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_degree
                                 where s.degree == jntuh_degree.degree && s.id != jntuh_degree.id
                                 select s.degree);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_degree).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_degree);
                }
                else
                {
                    TempData["Error"] = "Degree is already exists. Please enter a different Degree.";                   
                    return View(jntuh_degree);
                }
            }
            //ViewBag.degreeTypeId = new SelectList(db.jntuh_degree_type, "id", "degreeType", jntuh_degree.degreeTypeId);
            
            return View(jntuh_degree);
        }

        //
        // GET: /Degree/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_degree jntuh_degree = db.jntuh_degree.Find(id);
            return View(jntuh_degree);
        }

        //
        // POST: /Degree/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_degree jntuh_degree = db.jntuh_degree.Find(id);
            db.jntuh_degree.Remove(jntuh_degree);
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