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
    public class RoleController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Role/

        public ViewResult Index()
        {
            return View(db.my_aspnet_roles.ToList());
        }

        //
        // GET: /Role/Details/5

        public ViewResult Details(int id)
        {
            my_aspnet_roles my_aspnet_roles = db.my_aspnet_roles.Find(id);
            return View(my_aspnet_roles);
        }

        //
        // GET: /Role/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Role/Create

        [HttpPost]
        public ActionResult Create(my_aspnet_roles my_aspnet_roles)
        {
            if (ModelState.IsValid)
            {
                db.my_aspnet_roles.Add(my_aspnet_roles);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(my_aspnet_roles);
        }
        
        //
        // GET: /Role/Edit/5
 
        public ActionResult Edit(int id)
        {
            my_aspnet_roles my_aspnet_roles = db.my_aspnet_roles.Find(id);
            return View(my_aspnet_roles);
        }

        //
        // POST: /Role/Edit/5

        [HttpPost]
        public ActionResult Edit(my_aspnet_roles my_aspnet_roles)
        {
            if (ModelState.IsValid)
            {
                db.Entry(my_aspnet_roles).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(my_aspnet_roles);
        }

        //
        // GET: /Role/Delete/5
 
        public ActionResult Delete(int id)
        {
            my_aspnet_roles my_aspnet_roles = db.my_aspnet_roles.Find(id);
            return View(my_aspnet_roles);
        }

        //
        // POST: /Role/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            my_aspnet_roles my_aspnet_roles = db.my_aspnet_roles.Find(id);
            db.my_aspnet_roles.Remove(my_aspnet_roles);
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