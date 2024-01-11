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
    public class DepartmentController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Department/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_department = db.jntuh_department.Include(j => j.jntuh_degree).Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            return View(jntuh_department.ToList());
        }

        //
        // GET: /Department/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_department jntuh_department = db.jntuh_department.Find(id);
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            return View(jntuh_department);
        }

        //
        // GET: /Department/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            //ViewBag.degreeId = new SelectList(db.jntuh_degree, "id", "degree");
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /Department/Create

        [HttpPost]
        public ActionResult Create(jntuh_department jntuh_department)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_department
                                 where s.departmentName == jntuh_department.departmentName && s.degreeId == jntuh_department.degreeId                                         
                                 select s.departmentName);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_department.Add(jntuh_department);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Department Name is already exists. Please enter a different Department Name.";
                }
            }
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            //ViewBag.degreeId = new SelectList(db.jntuh_degree, "id", "degree", jntuh_department.degreeId);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.updatedBy);
            return View(jntuh_department);
        }
        
        //
        // GET: /Department/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            jntuh_department jntuh_department = db.jntuh_department.Find(id);
            //ViewBag.degreeId = new SelectList(db.jntuh_degree, "id", "degree", jntuh_department.degreeId);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.updatedBy);
            return View(jntuh_department);
        }

        //
        // POST: /Department/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_department jntuh_department)
        {
            ViewBag.Degree = (from s in db.jntuh_degree
                              where s.isActive == true
                              select s).ToList();
            //ViewBag.degreeId = new SelectList(db.jntuh_degree, "id", "degree", jntuh_department.degreeId);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_department.updatedBy);
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_department
                                where s.departmentName == jntuh_department.departmentName && s.id != jntuh_department.id && s.degreeId == jntuh_department.degreeId
                                 select s.departmentName);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_department).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_department);
                }
                else
                {
                    TempData["Error"] = "Department Name is already exists. Please enter a different Department Name.";
                    return View(jntuh_department);
                }
            }
            
            return View(jntuh_department);
        }

        //
        // GET: /Department/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_department jntuh_department = db.jntuh_department.Find(id);
            return View(jntuh_department);
        }

        //
        // POST: /Department/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_department jntuh_department = db.jntuh_department.Find(id);
            db.jntuh_department.Remove(jntuh_department);
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