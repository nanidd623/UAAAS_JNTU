using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SpecializationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Index()
        {
            List<Specialization> specialization = (from s in db.jntuh_specialization
                                                               join dept in db.jntuh_department on s.departmentId equals dept.id
                                                               join d in db.jntuh_degree on dept.degreeId equals d.id
                                                               where  dept.isActive == true && d.isActive == true
                                                               orderby s.id descending
                                                                select new Specialization
                                                               {
                                                                   degreeId=dept.degreeId,
                                                                   degree=d.degree,
                                                                   departmentId=s.departmentId,
                                                                   departmentName=dept.departmentName,
                                                                   id=s.id,
                                                                   specializationName=s.specializationName,
                                                                   specializationDescription=s.specializationDescription,
                                                                   isActive=s.isActive                                                             

                                                               }).ToList();

            return View(specialization);
        }

       
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ViewResult Details(int id)
        {            
            Specialization specialization = (from s in db.jntuh_specialization
                                                         join dept in db.jntuh_department on s.departmentId equals dept.id
                                                         join d in db.jntuh_degree on dept.degreeId equals d.id
                                                         where s.id == id  && dept.isActive == true && d.isActive == true
                                                         orderby s.id descending
                                                         select new Specialization
                                                         {
                                                             degreeId = dept.degreeId,
                                                             degree = d.degree,
                                                             departmentId = s.departmentId,
                                                             departmentName = dept.departmentName,
                                                             id = s.id,
                                                             specializationName = s.specializationName,
                                                             specializationDescription = s.specializationDescription,
                                                             isActive = s.isActive,
                                                             createdBy = s.createdBy,
                                                             createdOn = s.createdOn
                                                         }).FirstOrDefault();


            return View(specialization);
        }

       
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Create()
        {

            Specialization specialization = new Specialization();
            ViewBag.Degree = (from d in db.jntuh_degree where d.isActive == true select d).ToList();           
            return View(specialization);
        }    

        [HttpPost]
        public ActionResult Create(Specialization specialization)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            ViewBag.Degree = (from d in db.jntuh_degree where d.isActive == true  select d).ToList();   
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_specialization
                                where s.specializationName == specialization.specializationName && s.departmentId == specialization.departmentId
                                select s.specializationName);
                jntuh_specialization jntuh_specialization = new jntuh_specialization();
                if (rowExist.Count() == 0)
                {                    
                    jntuh_specialization.departmentId = specialization.departmentId;
                    jntuh_specialization.specializationName = specialization.specializationName;
                    jntuh_specialization.specializationDescription = specialization.specializationDescription;                    
                    jntuh_specialization.isActive=specialization.isActive;
                    jntuh_specialization.createdBy = userID;
                    jntuh_specialization.createdOn=DateTime.Now;
                    db.jntuh_specialization.Add(jntuh_specialization);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Error"] = "Specialization Name is already exists. Please enter a different Specialization Name.";
                    return View(specialization);
                }
            }             
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Edit(int id)
        {

            ViewBag.Degree = (from d in db.jntuh_degree where d.isActive == true select d).ToList();
            Specialization specialization = (from s in db.jntuh_specialization
                                                         join dept in db.jntuh_department on s.departmentId equals dept.id
                                                         join d in db.jntuh_degree on dept.degreeId equals d.id
                                                         where s.id == id  && dept.isActive == true && d.isActive == true
                                                         orderby s.id descending
                                                         select new Specialization
                                                         {
                                                             degreeId = dept.degreeId,
                                                             degree = d.degree,
                                                             departmentId = s.departmentId,
                                                             departmentName = dept.departmentName,
                                                             id = s.id,
                                                             specializationName = s.specializationName,
                                                             specializationDescription = s.specializationDescription,
                                                             isActive = s.isActive,
                                                             createdBy = s.createdBy,
                                                             createdOn = s.createdOn

                                                         }).FirstOrDefault();


            return View(specialization);
        }     
        [HttpPost]
        public ActionResult Edit(Specialization specialization)
        {
            ViewBag.Degree = (from d in db.jntuh_degree where d.isActive == true select d).ToList();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ModelState.IsValid)
            {
                var rowExist = (from s in db.jntuh_specialization
                                where s.specializationName == specialization.specializationName && s.id != specialization.id && s.departmentId==specialization.departmentId
                                select s.specializationName);
                
                if (rowExist.Count() == 0)
                {
                    jntuh_specialization jntuhspecialization = new jntuh_specialization();
                    jntuhspecialization.id = specialization.id;
                    jntuhspecialization.departmentId = specialization.departmentId;
                    jntuhspecialization.specializationName = specialization.specializationName;
                    jntuhspecialization.specializationDescription = specialization.specializationDescription;
                    jntuhspecialization.isActive = specialization.isActive;
                    jntuhspecialization.createdBy = specialization.createdBy;
                    jntuhspecialization.createdOn = specialization.createdOn;
                    jntuhspecialization.updatedBy = userID;
                    jntuhspecialization.updatedOn = DateTime.Now;
                    db.Entry(jntuhspecialization).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Updated successfully.";                    
                }
                else
                {
                    TempData["Error"] = "Specialization Name is already exists. Please enter a different Specialization Name.";
                    return View(specialization);
                }
            }

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
