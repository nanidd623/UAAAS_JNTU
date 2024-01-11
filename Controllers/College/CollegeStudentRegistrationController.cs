using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using UAAAS.Models;


namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeStudentRegistrationController : BaseController
    {
        //
        // GET: /CollegeStudentRegistration/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        //Specilaziton List Approved BY JNTU
        [Authorize(Roles = "College,Admin")]
        public ActionResult CollegeCources()
        {
            return RedirectToAction("College", "Dashboard");
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            var collegeCode = db.jntuh_college.Where(u => u.id == userCollegeId).Select(u => u.collegeCode).FirstOrDefault();
            //int userCollegeID = 375;
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeId == null)
            {
                return RedirectToAction("Logon", "Account");
            }
            List<jntuh_college_intake_existing> jntuh_college_intake_existing =
                db.jntuh_college_intake_existing.AsNoTracking().ToList();

            List<jntuh_approvedadmitted_intake> jntuh_approvedadmitted_intake =
              db.jntuh_approvedadmitted_intake.AsNoTracking().ToList();
            List<jntuh_college_studentregistration> jntuh_college_studentregistration =
                db.jntuh_college_studentregistration.Where(s => s.CollegeId == userCollegeId)
                    .Select(s => s)
                    .ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegeApprovedSpecializations> CollegeApprovedSpecializationslist = new List<CollegeApprovedSpecializations>();
            CollegeApprovedSpecializationslist = (from t in jntuh_approvedadmitted_intake
                                                  join cf in db.jntuh_specialization on t.SpecializationId equals cf.id
                                                  join dd in db.jntuh_department on cf.departmentId equals dd.id
                                                  join deg in db.jntuh_degree on dd.degreeId equals deg.id
                                                  join type in db.jntuh_degree_type on deg.degreeTypeId equals type.id
                                                  where cf.isActive == true && deg.id != 4 && deg.id != 5 && dd.isActive == true && deg.isActive == true && t.collegeId == userCollegeId
                                                  && t.AcademicYearId == prAy
                                                  //where t.collegeId == userCollegeID && t.academicYearId == 9 && t.shiftId==1
                                                  select new CollegeApprovedSpecializations()
                                                  {
                                                      DegreeTypeId = deg.id,
                                                      DepartmentId = dd.id,
                                                      DegreeName = deg.degree,
                                                      DepartmentName = dd.departmentName,
                                                      specid = cf.id,
                                                      shiftId = t.ShiftId,
                                                      Intake = t.ApprovedIntake,
                                                      TotalStudents = jntuh_college_studentregistration.Where(s => s.CollegeId == userCollegeId && s.SpecializationId == cf.id && s.ShiftId == t.ShiftId).Count(),
                                                      Specializationname = cf.specializationName

                                                  }).ToList();
            return View(CollegeApprovedSpecializationslist);
        }

        //Student Add
        //public ActionResult AddStudent()
        //{
        //    return View();
        //}
        [Authorize(Roles = "College,Admin")]
        public ActionResult StudentRegistration(string sid, int shiftids)
        {
            return RedirectToAction("College", "Dashboard");
            int id = 0;
            if (sid != null && shiftids != 0)
            {
                id =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(sid.ToString(),
                        System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                return RedirectToAction("CollegeCources");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            StudentRegistration StudentRegistration = new StudentRegistration();
            StudentRegistration.SpecializationId = id;

            StudentRegistration.PresentAcademicYear = db.jntuh_academic_year.Where(a => a.id == prAy).Select(s => s.academicYear).FirstOrDefault();
            // StudentRegistration.SpecializationName =
            db.jntuh_specialization.Where(s => s.id == id).Select(s => s.specializationName).FirstOrDefault();
            StudentRegistration.DepartmentId = db.jntuh_specialization.Where(s => s.id == id).Select(s => s.departmentId).FirstOrDefault();
            StudentRegistration.DegreeId = db.jntuh_department.Where(s => s.id == StudentRegistration.DepartmentId).Select(s => s.degreeId).FirstOrDefault();

            var specname = db.jntuh_specialization.Where(s => s.id == id).Select(s => s.specializationName).FirstOrDefault();
            var DegName = db.jntuh_degree.Where(s => s.id == StudentRegistration.DegreeId).Select(s => s.degree).FirstOrDefault();
            if (shiftids == 1)
            {
                StudentRegistration.SpecializationName = DegName + "-" + specname;
                StudentRegistration.ShiftId = shiftids;
            }
            else
            {
                StudentRegistration.SpecializationName = DegName + "-" + specname + "-2";
                StudentRegistration.ShiftId = shiftids;
            }
            List<jntuh_college_intake_existing> jntuh_college_intake_existing =
                db.jntuh_college_intake_existing.AsNoTracking().ToList();

            List<jntuh_approvedadmitted_intake> jntuh_approvedadmitted_intake =
               db.jntuh_approvedadmitted_intake.AsNoTracking().ToList();
            StudentRegistration.GenderId = -1;
            //List<Departments> Dept = new List<Departments>();
            //Dept = (from t in jntuh_approvedadmitted_intake
            //        join cf in db.jntuh_specialization on t.SpecializationId equals cf.id
            //        join dd in db.jntuh_department on cf.departmentId equals dd.id
            //        join deg in db.jntuh_degree on dd.degreeId equals deg.id
            //        join type in db.jntuh_degree_type on deg.degreeTypeId equals type.id
            //        where cf.isActive == true && dd.isActive == true && deg.isActive == true
            //        //where t.collegeId == userCollegeID && t.academicYearId == 9 && t.shiftId==1
            //        select new Departments()
            //        {
            //            DegreeTypeId = type.id,
            //            DepartmentId = dd.id,
            //            DepartmentName = dd.departmentName + "-" + cf.specializationName,
            //            specid = cf.id,
            //            Specializationname = cf.specializationName

            //        }).ToList();
            return PartialView(StudentRegistration);
        }

        [Authorize(Roles = "College,Admin")]
        [HttpPost]
        public ActionResult StudentRegistration(StudentRegistration studreg)
        {
            return RedirectToAction("College", "Dashboard");
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            var collegeCode = db.jntuh_college.Where(u => u.id == userCollegeId).Select(u => u.collegeCode).FirstOrDefault();
            List<jntuh_specialization> jntuh_specialization =
                db.jntuh_specialization.Where(s => s.id == studreg.SpecializationId).ToList();
            var aadhaarnumber =
                db.jntuh_college_studentregistration.Where(a => a.AadhaarNumber.Trim() == studreg.AadhaarNumber.Trim())
                    .Select(s => s.AadhaarNumber)
                    .FirstOrDefault();
            var HallTicketNumber =
               db.jntuh_college_studentregistration.Where(
                   f => f.HallTicketNo.Trim() == studreg.HallTicketNumber.Trim())
                   .Select(e => e)
                   .Count();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (HallTicketNumber != 0)
            {
                TempData["Error"] = "HallTicketNumber already Exist";
                return RedirectToAction("CollegeCources");
            }
            if (aadhaarnumber != null)
            {
                TempData["Error"] = "Student already Exist";
                return RedirectToAction("CollegeCources");
            }
            if (userCollegeId != null && studreg != null)
            {
                string aadhaarCardsPath = "~/Content/Upload/Student/AADHAARCARDS";
                if (studreg.AadhaarDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                    }

                    var ext = Path.GetExtension(studreg.AadhaarDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = studreg.AadhaarNumber.Trim() + "-" +
                        collegeCode.Trim();
                        //string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                        //studreg.Name.Substring(0, 1) + "-" + studreg.Name.Substring(0, 2);
                        studreg.AadhaarDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        studreg.AadhaarDocumentview = string.Format("{0}{1}", fileName, ext);
                    }
                }
                jntuh_college_studentregistration studentregistration = new jntuh_college_studentregistration();
                studentregistration.HallTicketNo = studreg.HallTicketNumber.Trim();
                studentregistration.AcademicYearid = prAy;
                studentregistration.StudentName = studreg.Name.Trim();
                studentregistration.Gender = Convert.ToInt32(studreg.GenderId);
                studentregistration.FatherName = studreg.FatherOrhusbandName.Trim();
                studentregistration.Mobile = studreg.Mobile.Trim();
                studentregistration.Email = studreg.Email.Trim();
                studentregistration.AadhaarNumber = studreg.AadhaarNumber.Trim();
                studentregistration.AadhaarDocument = studreg.AadhaarDocumentview.Trim();
                studentregistration.CollegeId = userCollegeId;
                studentregistration.ShiftId = studreg.ShiftId;
                studentregistration.SpecializationId = (int)studreg.SpecializationId;
                studentregistration.DepartmentId = db.jntuh_specialization.Where(s => s.id == studreg.SpecializationId).Select(d => d.departmentId).FirstOrDefault();
                studentregistration.DateOfBirth = UAAAS.Models.Utilities.DDMMYY2MMDDYY(studreg.studentDateOfBirth);
                studentregistration.CreatedBy = userId;
                studentregistration.CreatedOn = DateTime.Now;
                studentregistration.IsActive = true;
                studentregistration.Status = true;
                db.jntuh_college_studentregistration.Add(studentregistration);
                db.SaveChanges();
                TempData["Success"] = "Student Added Successfully..";
            }
            else
            {
                TempData["Error"] = "Try again";
                return RedirectToAction("CollegeCources");
            }
            return RedirectToAction("CollegeCources");
        }

        /// <summary>
        /// Student Registration Aadhaar Number Checking
        /// </summary>
        /// <param name="AadhaarNumber"></param>
        /// <returns></returns>
        public JsonResult CheckAadharNumber(string AadhaarNumber)
        {
            var status = aadharcard.validateVerhoeff(AadhaarNumber);
            // string Regno = TempData["regno"].ToString()_
            var jntuh_college_studentregistration =
                db.jntuh_college_studentregistration.Where(
                    f => f.AadhaarNumber == AadhaarNumber)
                    .Select(e => e)
                    .Count();
            if (status)
            {

                if (jntuh_college_studentregistration == 0)
                {
                    return Json(true);

                }
                else
                {
                    return Json("AadhaarNumber already Exists", JsonRequestBehavior.AllowGet);
                }



            }
            else
            {
                return Json("AadhaarNumber is not a validnumber", JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Student HallticketNumber Checking
        /// </summary>
        /// <param name="AadhaarNumber"></param>
        /// <returns></returns>
        public JsonResult CheckHallticketNumber(string HallTicketNumber)
        {

            var jntuh_college_studentregistration =
               db.jntuh_college_studentregistration.Where(
                   f => f.HallTicketNo == HallTicketNumber)
                   .Select(e => e)
                   .Count();
            if (jntuh_college_studentregistration == 0)
            {
                return Json(true);

            }
            else
            {
                return Json("HallTicketNumber already exists", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult EditHallticketNumber(string UpdateHallTicketNo, string AadhaarNumber)
        {
            var newaadhar = db.jntuh_college_studentregistration.Where(f => f.HallTicketNo == UpdateHallTicketNo).Select(e => e).Count();

            var jntuh_college_studentregistration = db.jntuh_college_studentregistration.Where(f => f.HallTicketNo == UpdateHallTicketNo && f.AadhaarNumber == AadhaarNumber).Select(e => e).Count();

            if (newaadhar == 0)
            {
                return Json(true);
            }
            else
            {

                if (jntuh_college_studentregistration == 1)
                {

                    return Json(true);
                }
                else
                {
                    return Json("HallTicketNumber already exists", JsonRequestBehavior.AllowGet);
                }


            }





        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult ViewStudents(string sspecid, int shiftids)
        {
            return RedirectToAction("College", "Dashboard");
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            var collegeCode = db.jntuh_college.Where(u => u.id == userCollegeId).Select(u => u.collegeCode).FirstOrDefault();
            int specid = 0;
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (sspecid != null)
            {
                specid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(sspecid.ToString(),
                        System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                return RedirectToAction("CollegeCources");
            }
            if (userCollegeId == null)
            {
                return RedirectToAction("CollegeCources");
            }

            int[] studentspeids = { 13, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 167, 169, 170, 171, 172 };
            List<jntuh_college_studentregistration> jntuh_college_studentregistration =
                db.jntuh_college_studentregistration.Where(
                    s => s.SpecializationId == specid && s.CollegeId == userCollegeId && s.ShiftId == shiftids).Select(s => s).ToList();
            List<jntuh_specialization> jntuh_specialization =
                db.jntuh_specialization.AsNoTracking().Select(s => s).ToList();
            List<jntuh_department> jntuh_department =
                db.jntuh_department.AsNoTracking().Select(s => s).ToList();
            List<StudentRegistration> StudentRegistrationview = new List<StudentRegistration>();

            foreach (var item in jntuh_college_studentregistration)
            {
                StudentRegistration studReg = new StudentRegistration();
                studReg.Id = item.Id;
                studReg.ShiftId = Convert.ToInt32(item.ShiftId);
                studReg.HallTicketNumber = item.HallTicketNo;
                studReg.Name = item.StudentName;
                studReg.FatherOrhusbandName = item.FatherName;
                studReg.GenderId = item.Gender;
                studReg.AadhaarNumber = item.AadhaarNumber;
                studReg.CollegeId = item.CollegeId;
                studReg.SpecializationName = jntuh_specialization.Where(s => s.id == item.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                studReg.department = jntuh_department.Where(s => s.id == item.DepartmentId).Select(s => s.departmentName).FirstOrDefault();
                studReg.AadhaarDocumentview = item.AadhaarDocument;
                //if (studentspeids.Contains(item.SpecializationId))
                //{
                //    studReg.Isedit = true;
                //}
                //else
                //{
                //    studReg.Isedit = false;
                //}

                StudentRegistrationview.Add(studReg);
            }

            return View(StudentRegistrationview);
        }
        [Authorize(Roles = "College,Admin")]
        public ActionResult EditStudent(string sid)
        {
            return RedirectToAction("College", "Dashboard");
            int id = 0;
            if (sid != null)
            {
                id = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(sid.ToString(),
                        System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            jntuh_college_studentregistration stu = db.jntuh_college_studentregistration.Where(e => e.Id == id).Select(e => e).FirstOrDefault();
            StudentRegistration StudentRegistration = new StudentRegistration();
            StudentRegistration.Id = stu.Id;
            StudentRegistration.Name = stu.StudentName;
            StudentRegistration.UpdateHallTicketNo = stu.HallTicketNo;
            StudentRegistration.AadhaarNumber = stu.AadhaarNumber;

            return PartialView(StudentRegistration);
        }
        [Authorize(Roles = "College,Admin")]
        [HttpPost]
        public ActionResult EditStudent(StudentRegistration studreg)
        {
            return RedirectToAction("College", "Dashboard");

            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_college_studentregistration reg = db.jntuh_college_studentregistration.Where(e => e.Id == studreg.Id).FirstOrDefault();
            reg.Id = studreg.Id;
            reg.HallTicketNo = studreg.UpdateHallTicketNo.ToUpper().Trim();
            reg.UpdatedOn = DateTime.Now;
            reg.UpdatedBy = userId;
            db.Entry(reg).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Student Hallticket number updated Successfully..";

            return RedirectToAction("ViewStudents", "CollegeStudentRegistration", new { @sspecid = UAAAS.Models.Utilities.EncryptString(reg.SpecializationId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]), @shiftids = reg.ShiftId });

        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult DeleteStudentNumber(int Id)
        {
            return RedirectToAction("College", "Dashboard");
            string SpecId = string.Empty;
            int shiftid = 0;
            if (Id != 0)
            {
                jntuh_college_studentregistration deletestudent =
                    db.jntuh_college_studentregistration.Where(d => d.Id == Id)
                        .Select(s => s)
                        .FirstOrDefault();
                SpecId = UAAAS.Models.Utilities.EncryptString(deletestudent.SpecializationId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
                shiftid = Convert.ToInt32(deletestudent.ShiftId);
                if (deletestudent != null)
                {
                    db.jntuh_college_studentregistration.Remove(deletestudent);
                    db.SaveChanges();
                    TempData["Success"] = "Student Deleted Successfully..";
                }
                else
                {
                    TempData["Error"] = "Data Not found";
                }
            }
            return RedirectToAction("ViewStudents", new { sspecid = SpecId, shiftids = shiftid });
        }

        public ActionResult CollegePGStudents(int? collegeid)
        {
            return RedirectToAction("College", "Dashboard");
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.collegeid = collegeid;
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).ToList();
            List<StudentRegistration> StudentRegistrationview = new List<StudentRegistration>();
            if (collegeid != null && collegeid != 0)
            {
                StudentRegistrationview = (from sreg in db.jntuh_college_studentregistration
                                           join spec in db.jntuh_specialization on sreg.SpecializationId equals spec.id
                                           join dept in db.jntuh_department on spec.departmentId equals dept.id
                                           join deg in db.jntuh_degree on dept.degreeId equals deg.id
                                           where sreg.CollegeId == collegeid
                                           select new StudentRegistration
                                           {
                                               Name = sreg.StudentName,
                                               HallTicketNumber = sreg.HallTicketNo,
                                               GenderId = sreg.Gender,
                                               FatherOrhusbandName = sreg.FatherName,
                                               department = dept.departmentName,
                                               SpecializationName = spec.specializationName,
                                               DegreeName = deg.degree,
                                               AadhaarNumber = sreg.AadhaarNumber,
                                               AadhaarDocumentview = sreg.AadhaarDocument

                                           }).ToList();
            }
            return View(StudentRegistrationview.OrderBy(s => s.department));
        }
    }

    public class CollegeApprovedSpecializations
    {
        public int specid { get; set; }
        public int shiftId { get; set; }
        public string Specializationname { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int DegId { get; set; }
        public string DegreeName { get; set; }
        public int? Intake { get; set; }
        public int TotalStudents { get; set; }
        public int DegreeTypeId { get; set; }
    }

    public class Departments
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int specid { get; set; }
        public string Specializationname { get; set; }
        public int PGdeptid { get; set; }
        public string PGDeptname { get; set; }
        public int Totaldeptid { get; set; }
        public string Totaldeptname { get; set; }

        public int DegreeTypeId { get; set; }
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
    }
}
