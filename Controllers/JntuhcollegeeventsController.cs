using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Data;



namespace UAAAS.Controllers
{
    public class JntuhcollegeeventsController : Controller
    {
        //
        // GET: /Jntuhcollegeevents/
        uaaasDBContext db = new uaaasDBContext();

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index()
        {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var collegeEvents =
                db.jntuh_college_events.Where(i => i.collegeid == userCollegeID && i.academicyearid == ay0).ToList();
            ViewBag.collegeEvents = collegeEvents;
            //if (collegeEvents.Count > 0)
            //{
            //    return RedirectToAction("jntuheventview");
            //}
            var eventname = (from obj in db.jntuh_eventmaster select obj).ToList();
            ViewBag.eventtype = eventname;
            return View("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult SaveEvent(Jntuheventtype listobj)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();

            var jntuheventebodypath = "~/Content/Upload/College/Event";
            if (!Directory.Exists(Server.MapPath(jntuheventebodypath)))
            {
                Directory.CreateDirectory(Server.MapPath(jntuheventebodypath));
            }
            var ext = System.IO.Path.GetExtension(listobj.SupportingDocument.FileName);
            if (ext.ToUpper().Equals(".PDF"))
            {
                string filename = jntuhCollege.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_C";
                listobj.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
                    Server.MapPath(jntuheventebodypath), filename, ext));
                listobj.SupportingDocumentfile = string.Format("{0}{1}", filename, ext);
            }
            jntuh_college_events newevente = new jntuh_college_events();
            newevente.academicyearid = ay0;
            newevente.collegeid = userCollegeID;
            newevente.eventid = listobj.eventid;
            newevente.fromdate = Convert.ToDateTime(UAAAS.Models.Utilities.DDMMYY2MMDDYY(listobj.fromdate));
            newevente.todate = Convert.ToDateTime(UAAAS.Models.Utilities.DDMMYY2MMDDYY(listobj.todate));
            //if (!string.IsNullOrEmpty(listobj.fromdate))
            //{
            //    newevente.fromdate = Convert.ToDateTime(listobj.fromdate);
            //}

            //if (!string.IsNullOrEmpty(listobj.todate))
            //{
            //    newevente.todate = Convert.ToDateTime(listobj.todate);
            //}
            newevente.remarks = listobj.remarks;
            newevente.supportingdocument = listobj.SupportingDocumentfile;
            newevente.createdby = userID;
            newevente.createdon = DateTime.Now;
            db.jntuh_college_events.Add(newevente);
            db.SaveChanges();
            TempData["Success"] = "Data Saved Successfully.";
            return RedirectToAction("jntuheventview");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult jntuheventview()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<Jntuheventtype> objeventtype = new List<Jntuheventtype>();
            if (userCollegeID > 0)
            {
                var query = (from objevent in db.jntuh_college_events
                             join objmaster in db.jntuh_eventmaster on objevent.eventid equals objmaster.id
                             where objevent.collegeid == userCollegeID
                             select new { objevent.id, objevent.eventid, objevent.fromdate, objevent.todate, objevent.supportingdocument, objevent.remarks, objmaster.eventtype });
                foreach (var items in query)
                {
                    objeventtype.Add(new Jntuheventtype
                    {
                        id = items.id,
                        eventtype = items.eventtype,
                        eventid = items.eventid,
                        // DateTime date = Convert.ToDateTime(items.fromdate);
                        //    fromdate = date.ToString("dd/MM/yyyy").Split(' ')[0];
                        //  fromdate= Convert.ToDateTime(items.fromdate),
                        fromdate = items.fromdate.ToShortDateString(),
                        todate = items.todate.ToShortDateString(),
                        supportingdocuments = items.supportingdocument,
                        remarks = items.remarks,
                    });
                }
            }
            
            ViewBag.objeventtypeDetails = objeventtype;
            return View("jntuheventview");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult _JntuhcollegeEdit(int? id, Jntuheventtype listobjtest)
        {





            //if (Membership.GetUser() == null)
            //{
            //    return RedirectToAction("LogOn", "Account");
            //}
            //DateTime todayDate = DateTime.Now;
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID =
            //    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            //}
            //var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            //int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();

            //var jntuheventebodypath = "~/Content/Upload/College/GoverningBody";
            //if (!Directory.Exists(Server.MapPath(jntuheventebodypath)))
            //{
            //    Directory.CreateDirectory(Server.MapPath(jntuheventebodypath));
            //}
            //var ext = System.IO.Path.GetExtension(listobj.SupportingDocument.FileName);
            //if (ext.ToUpper().Equals(".PDF"))
            //{
            //    string filename = jntuhCollege.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_C";
            //    listobj.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
            //        Server.MapPath(jntuheventebodypath), filename, ext));
            //    listobj.SupportingDocumentfile = string.Format("{0}{1}", filename, ext);
            //}



            Jntuheventtype listobj = new Jntuheventtype();
            var eventtypetest = (from obj in db.jntuh_college_events where obj.id == id select obj).FirstOrDefault();
            if (eventtypetest != null)
            {

                listobj.id = eventtypetest.id;
                listobj.remarks = eventtypetest.remarks;
                listobj.SupportingDocumentfile = eventtypetest.supportingdocument;
                listobj.eventid = eventtypetest.eventid;
                listobj.fromdate = eventtypetest.fromdate.ToString("dd/MM/yyyy").Split(' ')[0];
                listobj.todate = eventtypetest.todate.ToString("dd/MM/yyyy").Split(' ')[0];
            }
            var eventname = (from obj in db.jntuh_eventmaster select obj).ToList();
            ViewBag.eventtype = eventname;

            return PartialView("_JntuhcollegeEdit", listobj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult EditEvent(Jntuheventtype listobj, int? id)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var jntuhCollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();

            jntuh_college_events newevente = new jntuh_college_events();
            if (listobj.SupportingDocument != null)
            {
                var jntuheventebodypath = "~/Content/Upload/College/Event";
                if (!Directory.Exists(Server.MapPath(jntuheventebodypath)))
                {
                    Directory.CreateDirectory(Server.MapPath(jntuheventebodypath));
                }
                var ext = System.IO.Path.GetExtension(listobj.SupportingDocument.FileName);
                if (ext.ToUpper().Equals(".PDF"))
                {
                    string filename = jntuhCollege.collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_C";
                    listobj.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
                        Server.MapPath(jntuheventebodypath), filename, ext));
                    listobj.SupportingDocumentfile = string.Format("{0}{1}", filename, ext);

                }

                newevente.supportingdocument = listobj.SupportingDocumentfile;
            }
            else
            {
                newevente.supportingdocument = listobj.SupportingDocumentfile;
            }
            newevente.academicyearid = ay0;
            newevente.collegeid = userCollegeID;
            newevente.eventid = listobj.eventid;
            newevente.fromdate = Convert.ToDateTime(UAAAS.Models.Utilities.DDMMYY2MMDDYY(listobj.fromdate));
            newevente.todate = Convert.ToDateTime(UAAAS.Models.Utilities.DDMMYY2MMDDYY(listobj.todate));
            //if (!string.IsNullOrEmpty(listobj.fromdate))
            //{
            //    newevente.fromdate = Convert.ToDateTime(listobj.fromdate);
            //}

            //if (!string.IsNullOrEmpty(listobj.todate))
            //{
            //    newevente.todate = Convert.ToDateTime(listobj.todate);
            //}
            newevente.id = listobj.id;
            newevente.remarks = listobj.remarks;
            newevente.createdby = userID;
            newevente.createdon = DateTime.Now;
            newevente.updatedby = userID;
            newevente.updatedon = DateTime.Now;
            db.Entry(newevente).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Data Updated Successfully.";
            return RedirectToAction("jntuheventview");
        }
    }
}
