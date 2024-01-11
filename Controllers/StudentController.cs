using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Controllers.College;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class StudentController : BaseController
    {
        //
        private uaaasDBContext db = new uaaasDBContext();
        // GET: /Student/

        public ActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //[Authorize(Roles = "Admin, College")]
        //public ActionResult List(string id)
        //{
        //    return RedirectToAction("College", "Dashboard");
        //    int deptid = 0;

        //    if (id != null)
        //    {
        //        deptid = Convert.ToInt32(id);
        //        // deptid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
        //    }


        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    int UserCollegId = db.jntuh_college_users.Where(e => e.userID == userID).Select(e => e.collegeID).FirstOrDefault();
        //    if (UserCollegId == 375)
        //    {
        //        UserCollegId = 6;
        //    }
        //    List<Jntuh_college_student_list> stulist = new List<Jntuh_college_student_list>();
        //    int[] DegreeIds = { 1, 2, 3, 6 };

        //    var data = (from jp in db.jntuh_approvedadmitted_intake.ToList()
        //                join js in db.jntuh_specialization.ToList()
        //                    on jp.SpecializationId equals js.id
        //                join jd in db.jntuh_department.ToList()
        //                on js.departmentId equals jd.id
        //                where jp.collegeId == UserCollegId && jp.AcademicYearId == 9 && DegreeIds.Contains(jd.degreeId)

        //                select new { DepartmentName = jd.departmentName, DeptID = jd.id }).ToList();

        //    var daeptdata = data.Distinct();
        //    ViewBag.Departments = daeptdata;
        //    if (!string.IsNullOrEmpty(id))
        //    {

        //        var studata = db.jntuh_college_student.Where(e => e.DepartmentId == deptid && e.collegeid == UserCollegId)
        //               .Select(e => new { studentname = e.studentname, Hallticketnumber = e.hallticketnum, sid = e.sid }).ToList();

        //        var pdata = db.jntuh_pgstudent_projectdetatails.Where(e => e.CollegeId == UserCollegId).ToList();
        //        foreach (var item in studata)
        //        {
        //            Jntuh_college_student_list stu = new Jntuh_college_student_list();
        //            stu.sid = item.sid;
        //            stu.studentname = item.studentname;
        //            stu.HallticketNum = item.Hallticketnumber;
        //            stu.pid = pdata.Where(a => a.HallticketNumber == stu.HallticketNum).Select(e => e.ID).FirstOrDefault();
        //            stulist.Add(stu);
        //        }


        //    }


        //    return View(stulist);


        //}


        //[HttpGet]
        //[Authorize(Roles = "Admin, College")]
        //public ActionResult ProjectDetailsCreate(int sid)
        //{
        //    jntuh_college_student student = db.jntuh_college_student.Where(e => e.sid == sid).FirstOrDefault();

        //    jntuh_pgstudent_projectdetatails_class project = new jntuh_pgstudent_projectdetatails_class();
        //    project.StudentsName = student.studentname;
        //    project.HallticketNumber = student.hallticketnum;

        //    int[] designationsIds = { 1, 2, 3, 4 };

        //    ViewBag.designation = (from d in db.jntuh_designation.ToList()
        //                           where d.isActive == true && designationsIds.Contains(d.id)
        //                           select new { DesignationName = d.designation, DesignationId = d.id }).ToList();

        //    return View(project);
        //}
        //[HttpPost]
        //[Authorize(Roles = "Admin, College")]
        //public ActionResult ProjectDetailsCreate(jntuh_pgstudent_projectdetatails_class proj)
        //{
        //    try
        //    {
        //        var RegisteredEmail = User.Identity.Name;
        //        if (RegisteredEmail != null)
        //        {
        //            FormsAuthentication.SetAuthCookie(RegisteredEmail, true);
        //        }
        //        else
        //        {
        //            TempData["SUCCESS"] = null;
        //            return RedirectToAction("Logon", "Account");
        //        }

        //        int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);


        //        int UserCollegId = db.jntuh_college_users.Where(e => e.userID == userID).Select(e => e.collegeID).FirstOrDefault();

        //        if (UserCollegId == 375)
        //        {
        //            UserCollegId = 6;
        //        }
        //        jntuh_pgstudent_projectdetatails pgstu = new jntuh_pgstudent_projectdetatails();
        //        pgstu.StudentsName = proj.StudentsName;
        //        pgstu.HallticketNumber = proj.HallticketNumber;
        //        pgstu.ProjectEnroll = proj.ProjectEnroll;
        //        pgstu.CollegeId = UserCollegId;
        //        pgstu.CreatedBy = userID;
        //        pgstu.IsActive = true;
        //        pgstu.CreatedOn = System.DateTime.Now;
        //        if (proj.ProjectEnroll == true)
        //        {
        //            pgstu.ProjectTitle = proj.ProjectTitle;
        //            pgstu.ProjectDescription = proj.ProjectDescription;
        //            pgstu.ProjectType = proj.ProjectType;
        //            pgstu.InternalGuideRegNumber = proj.InternalGuideRegNumber;
        //            pgstu.InternalGuideName = proj.InternalGuideName;
        //            pgstu.InternalguideEmail = proj.InternalguideEmail;
        //            pgstu.InternalguideDepartment = proj.InternalguideDepartment;



        //            if (proj.ProjectType == 2)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    pgstu.ExternalGuideName = proj.ExternalGuideName;
        //                    pgstu.ExternalEmail = proj.ExternalEmail;
        //                    pgstu.ExternalExperience = proj.ExternalExperience;
        //                    pgstu.ExternalQualification = proj.ExternalQualification;
        //                    pgstu.ExternalGuideDesignation = proj.ExternalGuideDesignation;
        //                    pgstu.ExternalOrgName = proj.ExternalOrgName;
        //                    pgstu.ExternalOrgAddress = proj.ExternalOrgAddress;
        //                    db.jntuh_pgstudent_projectdetatails.Add(pgstu);
        //                    db.SaveChanges();
        //                    TempData["SUCCESS"] = "successful registered your project!with External Guide";
        //                }
        //            }
        //            if (proj.ProjectType == 1)
        //            {


        //                db.jntuh_pgstudent_projectdetatails.Add(pgstu);
        //                db.SaveChanges();
        //                TempData["SUCCESS"] = "successful registered your project !with Internal Guide ";
        //            }
        //        }
        //        else
        //        {
        //            pgstu.reason = proj.Reason;
        //            db.jntuh_pgstudent_projectdetatails.Add(pgstu);
        //            db.SaveChanges();
        //            TempData["SUCCESS"] = "successful registered!Student Not  Enrolled For Project ";
        //        }

        //    }
        //    catch (Exception Ex)
        //    {

        //        TempData["ERROR"] = "Try Again some error occured!oops ";

        //    }

        //    return RedirectToAction("List");


        //}
        [HttpPost]
        [Authorize(Roles = "Admin, College")]
        public JsonResult facultyDetails(string RegNum)
        {

            string Details = "";

            if (!string.IsNullOrEmpty(RegNum))
            {
                var data = (from jf in db.jntuh_registered_faculty
                            join
                                jr in db.jntuh_college_faculty_registered
                                on jf.RegistrationNumber equals jr.RegistrationNumber
                            join jd in db.jntuh_department on
                            jr.DepartmentId equals jd.id
                            where jr.RegistrationNumber == RegNum

                            select new
                            {
                                Fistname = "" + jf.FirstName,
                                midddlename = "" + jf.MiddleName,
                                lastname = "" + jf.LastName,
                                InternalguideEmail = jf.Email,
                                InternalguideDepartmentNAME = jd.departmentName,
                                InternalguideDepartment = jd.id
                            }).FirstOrDefault();
                string InternalGuideName = data.Fistname + " " + data.midddlename + " " + data.lastname;

                Details = InternalGuideName + "," + data.InternalguideEmail + "," + data.InternalguideDepartmentNAME + "," + data.InternalguideDepartment;

            }
            return Json(new { Details }, "application/json", JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckRegCollege(string InternalGuideRegNumber)
        {
            bool isRegNotIn = false;
            try
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                int UserCollegId = db.jntuh_college_users.Where(e => e.userID == userID).Select(e => e.collegeID).FirstOrDefault();
                if (UserCollegId == 375)
                {
                    UserCollegId = 5;
                }

                var reg = db.jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == InternalGuideRegNumber && e.collegeId == UserCollegId).Select(e => e.RegistrationNumber).FirstOrDefault();

                if (reg != null)
                {
                    isRegNotIn = true;
                }

                return Json(isRegNotIn, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(isRegNotIn, JsonRequestBehavior.AllowGet);

            }
            return Json(isRegNotIn, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult CollegeUgStudents(int? id, int? Value)
        {
           return RedirectToAction("College", "Dashboard");
           List<int?> sepid = new List<int?>();

            if (id != null)
            {
                var ids = db.jntuh_specialization.Where(e=>e.departmentId ==id).Select(e=>e.id).ToList();
                foreach (var item in ids)
	            {	            
                    sepid.Add(item);
	            }
               
                // deptid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }


            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int UserCollegId = db.jntuh_college_users.Where(e => e.userID == userID).Select(e => e.collegeID).FirstOrDefault();
            if (UserCollegId == 375)
            {
                UserCollegId = Convert.ToInt32(WebConfigurationManager.AppSettings["appCollegeId"]);
            }
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == UserCollegId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            if (CollegeAffiliationStatus=="Yes")
            {
                return RedirectToAction("ComingSoon","Labs");
            }
            var specializationids = db.jntuh_ugstudents.Where(e => e.collegeid == UserCollegId).Select(e => e.specializationid).ToList().Distinct();


            

            var deptlist = (from d in db.jntuh_department
                            join s in db.jntuh_specialization
                                on d.id equals s.departmentId
                            where specializationids.Contains(s.id)
                            select new { departmentName = d.departmentName, Id = d.id }).ToList();
            ViewBag.Departments = deptlist.Distinct().ToList();
           
            List<SelectListItem> selectyears=new List<SelectListItem>();
            selectyears.Add(new SelectListItem { Value = "10", Text = "First Year" });
            selectyears.Add(new SelectListItem { Value = "9", Text = "Second Year" });
            selectyears.Add(new SelectListItem { Value = "8", Text = "Third Year" });
            selectyears.Add(new SelectListItem { Value = "3", Text = "Fourth Year" });
            ViewBag.Years = selectyears.ToList();
            List<ugstudents> uglist = new List<ugstudents>();

            if (id != null && Value != null)
            {

                uglist = db.jntuh_ugstudents.Where(e => e.collegeid == UserCollegId && sepid.Contains(e.specializationid) && e.academicyearid == Value).Select(e => new ugstudents { studentName = e.estudentname, HallTicketNumber = e.hallticketno, Id = e.id, MobileNumber = e.mobileno, Email = e.emailid, departmentid = e.specializationid, AadhaarNumber = e.aadhaarno }).OrderBy(e => e.HallTicketNumber).ToList();
            }

            return View(uglist);
        }
        [HttpGet]
        [Authorize(Roles = "College,Admin")]
        public ActionResult CreateUgStudent(string Sid)
        {
            return RedirectToAction("College", "Dashboard");
            int stuid = 0;
            //if (userCollegeId == 375)
            //{
            //    userCollegeId = Convert.ToInt32(WebConfigurationManager.AppSettings["appCollegeId"]);
            //}
            if (Sid != null)
            {
                stuid =
                  Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(Sid.ToString(),
                      System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                return RedirectToAction("CollegeUgStudents");
            }

            ugstudents objstu = new ugstudents();
            var data = db.jntuh_ugstudents.Where(e => e.id == stuid).FirstOrDefault();

            objstu.Id = data.id;
            objstu.studentName = data.estudentname;
            objstu.HallTicketNumber= data.hallticketno;
            objstu.MobileNumber = data.mobileno;
            objstu.Email = data.emailid;
            objstu.AadhaarNumber = data.aadhaarno;
            objstu.NameAsAadhaar = data.studentname;
            objstu.aadhardocumentview = data.aadhaardoc;
            objstu.DOBasAadhaar = UAAAS.Models.Utilities.MMDDYY2DDMMYY(data.dob.ToString());



            //ugstudents objstu = db.jntuh_ugstudents.Where(e => e.id == stuid).Select(e => new ugstudents { Id = e.id, studentName = e.estudentname, HallTicketNumber = e.hallticketno, MobileNumber = e.mobileno, Email = e.emailid, departmentid = e.departmentid, AadhaarNumber = e.aadhaarno, NameAsAadhaar = e.studentname, aadhardocumentview = e.aadhaardoc, DOBasAadhaar = "" }).FirstOrDefault();

            return PartialView(objstu);
        }
        [HttpPost]
        [Authorize(Roles = "College,Admin")]
        public ActionResult CreateUgStudent(ugstudents stuobj)
        {
            return RedirectToAction("College", "Dashboard");
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            var collegeCode = db.jntuh_college.Where(u => u.id == userCollegeId).Select(u => u.collegeCode).FirstOrDefault();
            //AAdhaar Number Checking
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(WebConfigurationManager.AppSettings["appCollegeId"]);
            }
            var status = aadharcard.validateVerhoeff(stuobj.AadhaarNumber);
            var jntuh_college_studentregistration =
                db.jntuh_ugstudents.Where(
                    f => f.aadhaarno == stuobj.AadhaarNumber)
                    .Select(e => e)
                    .Count();
            jntuh_ugstudents ugstu = db.jntuh_ugstudents.Where(e => e.id == stuobj.Id).Select(e => e).FirstOrDefault();
            int depiid = db.jntuh_specialization.Where(e => e.id == ugstu.specializationid).Select(e => e.departmentId).FirstOrDefault();
            var jntuh_aadhar_samereg = db.jntuh_ugstudents.Where(f => f.aadhaarno == stuobj.AadhaarNumber && f.hallticketno == stuobj.HallTicketNumber).Select(e => e).Count();
            if (status)
            {

                if (jntuh_college_studentregistration == 0)
                {
                  
                }
                else
                {
                    if (jntuh_aadhar_samereg == 1)
                    {
                       
                    }
                    else
                    {
                        //return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                        TempData["ERROR"] = "AadhaarNumber already Exists.";
                        return RedirectToAction("CollegeUgStudents", new { id = depiid,Value = ugstu.academicyearid});
                    }
                }

            }
            else
            {
                //return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
                TempData["ERROR"] = "AadhaarNumber is not a validnumber.";
                return RedirectToAction("CollegeUgStudents", new { id = depiid , Value = ugstu.academicyearid });
            }
            string aadhaarCardsPath = "~/Content/Upload/Student/UG/AADHAARCARDS";
            if (stuobj.aadhardocument != null)
            {
                if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))

                {
                      Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));

                }
                  
                    var ext = Path.GetExtension(stuobj.aadhardocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = stuobj.AadhaarNumber.Trim() + "-" +
                        collegeCode.Trim();
                        //string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        //studreg.Name.Substring(0, 1) + "-" + studreg.Name.Substring(0, 2);
                        stuobj.aadhardocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        stuobj.aadhardocumentview = string.Format("{0}{1}", fileName, ext);
                    }             

            }
            //stuobj.aadhardocumentview = null;
            ugstu.studentname = stuobj.NameAsAadhaar;
            ugstu.mobileno = stuobj.MobileNumber;
            ugstu.emailid = stuobj.Email;
            ugstu.aadhaarno = stuobj.AadhaarNumber;
            ugstu.aadhaardoc = stuobj.aadhardocumentview.Trim();
            ugstu.dob = UAAAS.Models.Utilities.DDMMYY2MMDDYY(stuobj.DOBasAadhaar);
            ugstu.updatedon = DateTime.Now;
            ugstu.updatedby = userId;
            db.Entry(ugstu).State = EntityState.Modified;
            db.SaveChanges();
            TempData["SUCCESS"] = "Sudent Regstration successfully";
            return RedirectToAction("CollegeUgStudents", new { id = depiid, Value = ugstu.academicyearid });            
           
        }
        [HttpPost]
        public JsonResult CheckAadharNumber(string HallTicketNumber, string AadhaarNumber)
        {
            var status = aadharcard.validateVerhoeff(AadhaarNumber);
            // string Regno = TempData["regno"].ToString()_
            var jntuh_college_studentregistration =
                db.jntuh_ugstudents.Where(
                    f => f.aadhaarno == AadhaarNumber)
                    .Select(e => e)
                    .Count();

            var jntuh_aadhar_samereg = db.jntuh_ugstudents.Where(f => f.aadhaarno == AadhaarNumber && f.hallticketno == HallTicketNumber).Select(e => e).Count();
            if (status)
            {

                if (jntuh_college_studentregistration == 0)
                {
                    return Json(true);

                }
                else
                {
                    if (jntuh_aadhar_samereg == 1)
                    {
                        return Json(true);
                    }
                    else
                    {
                        return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                    }
                    
                }



            }
            else
            {
                return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
