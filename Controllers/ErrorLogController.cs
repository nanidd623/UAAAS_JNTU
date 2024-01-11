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
    public class ErrorLogController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /ErrorLog/
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet]
        public ViewResult Index()
        {
            College college = new College();
            ViewBag.colleges = (from c in db.jntuh_college
                                where (c.isActive == true)
                                select new College
                                {
                                    id=c.id,
                                    name=c.collegeCode+"-"+c.collegeName
                                }).OrderBy(c=>c.name).ToList();

           // var jntuh_error_log1 = db.jntuh_error_log.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1).Take(10);           
            DateTime dt1 = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 12:00:00"));
            DateTime dt2 = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 23:59:00"));

            //DateTime dt1 = DateTime.Now;
            //DateTime dt2 = DateTime.Now;
            
            var jntuh_error_log = db.jntuh_error_log.Where(j => j.createdOn >= dt1 && j.createdOn <= dt2).OrderByDescending(j=>j.id);
            //var jntuh_error_log = db.jntuh_error_log.Where(j =>j.createdOn.Value.Date >= dt1.Date && j.createdOn.Value.Date <= dt2.Date).OrderByDescending(j => j.id);
            return View(jntuh_error_log.ToList());
        }
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost]
        public ViewResult Index(FormCollection fc)
        {
            College college = new College();
            ViewBag.colleges = (from c in db.jntuh_college
                                where (c.isActive == true)
                                select new College
                                {
                                    id = c.id,
                                    name = c.collegeCode + "-" + c.collegeName
                                }).OrderBy(c => c.name).ToList();

            ViewBag.Post = true;
            string sdt1 = fc["txtFromDate"].ToString();
            string sdt2 = fc["txtToDate"].ToString();
            DateTime dt1;
            DateTime dt2;
            if (sdt1 != string.Empty && sdt2 != string.Empty)
            {
                ViewBag.dt1 = sdt1;
                ViewBag.dt2 = sdt2;
                string[] a1 = sdt1.Split('/');
                sdt1 = a1[1] + "/" + a1[0] + "/" + a1[2];

                string[] a2 = sdt2.Split('/');
                sdt2 = a2[1] + "/" + a2[0] + "/" + a2[2];
                 dt1 = Convert.ToDateTime(sdt1 + " " + "12:00:00");
                 dt2 = Convert.ToDateTime(sdt2 + " " + "23:59:00");
            }
            else
            {
                 dt1 = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 12:00:00"));
                 dt2 = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 23:59:00"));
            }
            string collegeid = fc["ddlcollegeId"].ToString();
            int userId = 0;
            if (collegeid != string.Empty)
            {
                int cid=Convert.ToInt32(collegeid);
                userId = db.jntuh_college_users.Where(u => u.collegeID == cid).Select(u=>u.userID).FirstOrDefault();
            }          
           
            var jntuh_error_log = db.jntuh_error_log.Where(j => j.createdOn >= dt1 && j.createdOn <= dt2).OrderByDescending(j => j.id);
            if (userId > 0)
            {
                jntuh_error_log = db.jntuh_error_log.Where(j => j.createdOn >= dt1 && j.createdOn <= dt2 && j.createdBy == userId).OrderByDescending(j => j.id);
            }
            else
            {
                jntuh_error_log = db.jntuh_error_log.Where(j => j.createdOn >= dt1 && j.createdOn <= dt2).OrderByDescending(j => j.id);
            }
            return View(jntuh_error_log.ToList());
        }

        public class College
        {
            public int id { get; set; }
            public string name { get; set; }
        }
        //
        // GET: /ErrorLog/Details/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_error_log jntuh_error_log = db.jntuh_error_log.Find(id);
            int ?userid=db.jntuh_error_log.Where(el=>el.id==id).Select(el=>el.createdBy).FirstOrDefault();
            ViewBag.CollegeId = db.jntuh_college_users.Where(cu => cu.userID == userid).Select(cu => cu.collegeID).FirstOrDefault();
            return View(jntuh_error_log);
        }

        //
        // GET: /ErrorLog/Create
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        } 

        //
        // POST: /ErrorLog/Create

        [HttpPost]
        public ActionResult Create(jntuh_error_log jntuh_error_log)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_error_log
                                 where s.exception == jntuh_error_log.exception
                                 select s.exception);
                if (rowExists.Count() == 0)
                {
                    db.jntuh_error_log.Add(jntuh_error_log);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Exception is already exists. Please enter a different Exception.";
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.updatedBy);
            return View(jntuh_error_log);
        }
        
        //
        // GET: /ErrorLog/Edit/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_error_log jntuh_error_log = db.jntuh_error_log.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.updatedBy);
            return View(jntuh_error_log);
        }

        //
        // POST: /ErrorLog/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_error_log jntuh_error_log)
        {
            if (ModelState.IsValid)
            {
                var rowExists = (from s in db.jntuh_error_log
                                 where s.exception == jntuh_error_log.exception && s.id != jntuh_error_log.id
                                 select s.exception);
                if (rowExists.Count() == 0)
                {
                    db.Entry(jntuh_error_log).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";
                    return View(jntuh_error_log);
                }
                else
                {
                    TempData["Error"] = "Exception is already exists. Please enter a different Exception.";
                    return View(jntuh_error_log);
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_error_log.updatedBy);
            return View(jntuh_error_log);
        }

        //
        // GET: /ErrorLog/Delete/5
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_error_log jntuh_error_log = db.jntuh_error_log.Find(id);
            return View(jntuh_error_log);
        }

        //
        // POST: /ErrorLog/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            jntuh_error_log jntuh_error_log = db.jntuh_error_log.Find(id);
            db.jntuh_error_log.Remove(jntuh_error_log);
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