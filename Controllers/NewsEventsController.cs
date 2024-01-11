using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class NewsEventsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            var jntuh_newsevents = db.jntuh_newsevents.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1).OrderByDescending(n => n.id);
            return View(jntuh_newsevents.ToList());
        }

        public ViewResult Public()
        {
            //var jntuh_newsevents = db.jntuh_newsevents.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1).Where(n => n.isActive == true && n.isNews == true).OrderByDescending(n => n.id);
            var jntuh_newsevents = db.jntuh_newsevents.Where(n => n.isActive == true && n.isNews == true).OrderByDescending(n => n.id);
            return View(jntuh_newsevents.ToList());
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {
            jntuh_newsevents jntuh_newsevents = db.jntuh_newsevents.Find(id);
            return View(jntuh_newsevents);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name");
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create(jntuh_newsevents jntuh_newsevents)
        {
            if (jntuh_newsevents.uploadFile != null)
            {
                int Id = db.jntuh_newsevents.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                Id = Id + 1;
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/News")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/News"));
                }
                var ext = Path.GetExtension(jntuh_newsevents.uploadFile.FileName);
                var fileName = jntuh_newsevents.uploadFile.FileName;
                if (ext.ToUpper().Equals(".PDF"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }

                if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }

                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }
            }
            if (ModelState.IsValid)
            {
                if (jntuh_newsevents.startDate > jntuh_newsevents.endDate)
                {
                    TempData["Error"] = "Start Date Should not be greater than End Date";
                }
                else
                {
                    var rowExists = (from s in db.jntuh_newsevents
                                     where s.title == jntuh_newsevents.title
                                     select s.title);
                    if (rowExists.Count() == 0)
                    {
                        jntuh_newsevents.newsOrder = 1;
                        db.jntuh_newsevents.Add(jntuh_newsevents);
                        db.SaveChanges();
                        TempData["Success"] = "Added successfully.";
                    }
                    else
                    {
                        TempData["Error"] = "News Title is already exists. Please enter a different Title.";
                    }
                }
            }

            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.updatedBy);
            return View(jntuh_newsevents);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {
            jntuh_newsevents jntuh_newsevents = db.jntuh_newsevents.Find(id);
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.updatedBy);
            if (jntuh_newsevents.navigateURL != null)
            {
                if (jntuh_newsevents.navigateURL.Length >= 20)
                {
                    if (jntuh_newsevents.navigateURL.Substring(0, 20) == "/Content/Upload/News/")
                    {
                        jntuh_newsevents.navigateURL = string.Empty;
                        ViewBag.uploadFile = true;
                    }
                }
            }
            return View(jntuh_newsevents);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(jntuh_newsevents jntuh_newsevents)
        {
            if (jntuh_newsevents.uploadFile == null && jntuh_newsevents.navigateURL == null)
            {
                jntuh_newsevents.navigateURL = db.jntuh_newsevents.Where(news => news.id == jntuh_newsevents.id)
                                                                  .Select(news => news.navigateURL)
                                                                  .FirstOrDefault();

            }
            if (jntuh_newsevents.uploadFile != null)
            {
                int Id = db.jntuh_newsevents.Count() > 0 ? db.jntuh_newsevents.Select(d => d.id).Max() : 0;
                Id = Id + 1;

                if (!Directory.Exists(Server.MapPath("~/Content/Upload/News")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/News"));
                }

                var ext = Path.GetExtension(jntuh_newsevents.uploadFile.FileName);
                var fileName = jntuh_newsevents.uploadFile.FileName;

                if (ext.ToUpper().Equals(".PDF"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }

                if (ext.ToUpper().Equals(".DOC") || ext.ToUpper().Equals(".DOCX"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }

                if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                {
                    jntuh_newsevents.uploadFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/News"), Id, fileName));
                    jntuh_newsevents.navigateURL = string.Format("{0}/{1}{2}", "/Content/Upload/News", Id, fileName);
                }
            }

            if (ModelState.IsValid)
            {
                if (jntuh_newsevents.startDate > jntuh_newsevents.endDate)
                {
                    TempData["Error"] = "Start Date Should not be greater than End Date";
                }
                else
                {
                    var rowExists = (from s in db.jntuh_newsevents
                                     where s.title == jntuh_newsevents.title && s.id != jntuh_newsevents.id
                                     select s.title);
                    if (rowExists.Count() == 0)
                    {
                        db.Entry(jntuh_newsevents).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Updated successfully.";
                        return View(jntuh_newsevents);
                    }
                    else
                    {
                        TempData["Error"] = "News Title is already exists. Please enter a different Title.";
                        return View(jntuh_newsevents);
                    }
                }
            }
            ViewBag.createdBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.createdBy);
            ViewBag.updatedBy = new SelectList(db.my_aspnet_users, "id", "name", jntuh_newsevents.updatedBy);
            return View(jntuh_newsevents);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Delete(int id)
        {
            jntuh_newsevents jntuh_newsevents = db.jntuh_newsevents.Find(id);
            return View(jntuh_newsevents);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_newsevents jntuh_newsevents = db.jntuh_newsevents.Find(id);
            db.jntuh_newsevents.Remove(jntuh_newsevents);
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