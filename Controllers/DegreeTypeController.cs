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
    public class DegreeTypeController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /DegreeType/
         [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_degree_type = db.jntuh_degree_type.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            return View(jntuh_degree_type.ToList());
        }

        //
        // GET: /DegreeType/Details/5
         [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_degree_type jntuh_degree_type = db.jntuh_degree_type.Find(id);
            return View(jntuh_degree_type);
        }

        //
        // GET: /DegreeType/Create
         [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /DegreeType/Create

        [HttpPost]
        public ActionResult Create(jntuh_degree_type jntuh_degree_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist=(from s in db.jntuh_degree_type
                              where s.degreeType==jntuh_degree_type.degreeType
                              select s.degreeType);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_degree_type.Add(jntuh_degree_type);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Degree Type is already exists. Please enter a different Degree Type.";
                }
                 
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.updatedBy);
            return View(jntuh_degree_type);
        }
        
        //
        // GET: /DegreeType/Edit/5
         [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_degree_type jntuh_degree_type = db.jntuh_degree_type.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.updatedBy);
            return View(jntuh_degree_type);
        }

        //
        // POST: /DegreeType/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_degree_type jntuh_degree_type)
        {
            if (ModelState.IsValid)
            {
                var rowExist=(from s in db.jntuh_degree_type
                              where s.degreeType==jntuh_degree_type.degreeType && s.id != jntuh_degree_type.id
                              select s.degreeType);
                if (rowExist.Count() == 0)
                {
                    db.Entry(jntuh_degree_type).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_degree_type);
                }
                else
                {
                    TempData["Error"] = "Degree Type is already exists. Please enter a different Degree Type.";
                    return View(jntuh_degree_type);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_degree_type.updatedBy);
            return View(jntuh_degree_type);
        }

        //
        // GET: /DegreeType/Delete/5
         [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_degree_type jntuh_degree_type = db.jntuh_degree_type.Find(id);
            return View(jntuh_degree_type);
        }

        //
        // POST: /DegreeType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_degree_type jntuh_degree_type = db.jntuh_degree_type.Find(id);
            db.jntuh_degree_type.Remove(jntuh_degree_type);
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