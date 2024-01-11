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
    public class Specialization1Controller : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Specialization/
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_specialization = db.jntuh_specialization.Include(j => j.jntuh_department).Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);
            /*List<jntuh_specialization> jntuh_specialization = (from s in db.jntuh_specialization
                                                               join dept in db.jntuh_department on s.departmentId equals dept.id
                                                               join d in db.jntuh_degree on dept.degreeId equals d.id
                                                               where s.isActive == true && dept.isActive == true && d.isActive == true
                                                               orderby s.id descending
                                                               select new jntuh_specialization
                                                               {
                                                                   degreeId=dept.degreeId,
                                                                   degree=d.degree,
                                                                   departmentId=s.departmentId,
                                                                   departmentName=dept.departmentName,
                                                                   id=s.id,
                                                                   specializationName=s.specializationName,
                                                                   specializationDescription=s.specializationDescription,
                                                                   isActive=s.isActive                                                                

                                                               }).ToList();*/
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();
            return View(jntuh_specialization.ToList());
        }

        //
        // GET: /Specialization/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_specialization jntuh_specialization = db.jntuh_specialization.Find(id);
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();

            ViewBag.Degree = (from s in db.jntuh_specialization
                              join dept in db.jntuh_department on s.departmentId equals dept.id
                              join d in db.jntuh_degree on dept.degreeId equals d.id
                              where s.isActive == true && dept.isActive == true && d.isActive == true
                              select new
                              {
                                  d.degree
                              }).FirstOrDefault();

            return View(jntuh_specialization);
        }

        //
        // GET: /Specialization/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {

            jntuh_specialization jntuh_specialization = new Models.jntuh_specialization();
            ViewBag.Degree = (from d in db.jntuh_degree
                                  where d.isActive == true
                                  select d).ToList();
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();
            //ViewBag.departmentId = new SelectList(db.jntuh_department, "id", "departmentName");
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View(jntuh_specialization);
        } 

        //
        // POST: /Specialization/Create

        [HttpPost]
        public ActionResult Create(jntuh_specialization jntuh_specialization)
        {
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_specialization
                                where s.specializationName == jntuh_specialization.specializationName
                                select s.specializationName);
                if (rowExist.Count() == 0)
                {
                    db.jntuh_specialization.Add(jntuh_specialization);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Specialization Name is already exists. Please enter a different Specialization Name.";
                }
            }
            ViewBag.Degree = (from d in db.jntuh_degree
                              where d.isActive == true
                              select d).ToList();
            //ViewBag.departmentId = new SelectList(db.jntuh_department, "id", "departmentName", jntuh_specialization.departmentId);
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.updatedBy);
            return View(jntuh_specialization);
        }
        
        //
        // GET: /Specialization/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
           // jntuh_specialization jntuh_specialization = db.jntuh_specialization.Find(id);

            jntuh_specialization jntuh_specialization = (from s in db.jntuh_specialization
                                                         join dept in db.jntuh_department on s.departmentId equals dept.id
                                                         join d in db.jntuh_degree on dept.degreeId equals d.id
                                                         where s.id==id && s.isActive == true && dept.isActive == true && d.isActive == true
                                                         orderby s.id descending
                                                         select new jntuh_specialization
                                                         {
                                                             degreeId = dept.degreeId,
                                                             degree = d.degree,
                                                             departmentId = s.departmentId,
                                                             departmentName = dept.departmentName,
                                                             id = s.id,
                                                             specializationName = s.specializationName,
                                                             specializationDescription = s.specializationDescription,
                                                             isActive = s.isActive,
                                                             createdBy=s.createdBy,
                                                             createdOn=s.createdOn

                                                         }).FirstOrDefault();



            ViewBag.Degree = (from d in db.jntuh_degree
                              where d.isActive == true
                              select d).ToList();
            //ViewBag.departmentId = new SelectList(db.jntuh_department, "id", "departmentName", jntuh_specialization.departmentId);
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.updatedBy);
            return View(jntuh_specialization);
        }

        //
        // POST: /Specialization/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_specialization jntuh_specialization)
        {
            ViewBag.Degree = (from d in db.jntuh_degree
                              where d.isActive == true
                              select d).ToList();
            //ViewBag.departmentId = new SelectList(db.jntuh_department, "id", "departmentName", jntuh_specialization.departmentId);
            //ViewBag.Department = db.jntuh_department.ToList();
            ViewBag.Department = (from s in db.jntuh_department
                                  where s.isActive == true
                                  select s).ToList();
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_specialization.updatedBy);
            if (ModelState.IsValid)
            {
                 var rowExist = (from s in db.jntuh_specialization
                                where s.specializationName == jntuh_specialization.specializationName && s.id !=jntuh_specialization.id
                                select s.specializationName);
                 if (rowExist.Count() == 0)
                 {
                     db.Entry(jntuh_specialization).State = EntityState.Modified;
                     db.SaveChanges();
                     TempData["Success"] = "Updated successfully.";
                     return View(jntuh_specialization);
                 }
                 else
                 {
                     TempData["Error"] = "Specialization Name is already exists. Please enter a different Specialization Name.";
                     return View(jntuh_specialization);
                 }
            }
            
            return View(jntuh_specialization);
        }

        //
        // GET: /Specialization/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_specialization jntuh_specialization = db.jntuh_specialization.Find(id);
            return View(jntuh_specialization);
        }

        //
        // POST: /Specialization/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_specialization jntuh_specialization = db.jntuh_specialization.Find(id);
            db.jntuh_specialization.Remove(jntuh_specialization);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDepartments(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var DepartmentList = this.Departments(Convert.ToInt32(id));

            var DepartmentsData = DepartmentList.Select(a => new SelectListItem()
            {
                Text = a.departmentName,
                Value = a.id.ToString(),
            });
            return Json(DepartmentsData, JsonRequestBehavior.AllowGet);
        }
        private List<jntuh_department> Departments(int id)
        {
            return db.jntuh_department.Where(d => d.isActive == true && d.degreeId == id).OrderBy(d => d.departmentName).ToList();
        }
    }
}