using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using UAAAS.Models;
using System.Data.OleDb;
using System.Configuration;
using System.Data;
using System.Net.Mail;
namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AICTEFacultyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult Index(string id)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
         
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && id != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (!String.IsNullOrEmpty(id) && userCollegeID==0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
           //userCollegeID = 118;
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("Admin"))
            {
                ViewBag.IsEditable = true;
                var Aicte = db.jntuh_college_aictefaculty.Where(s => s.IsActive == true && s.CollegeId == userCollegeID).Select(e => e).ToList();
                ViewBag.AicteData = Aicte;
                return View();
             
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AIF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College") && isPageEditable==true)
            {
                return RedirectToAction("AicteFAcultyView");
            }
            var AicteNew = db.jntuh_college_aictefaculty.Where(s => s.IsActive == true && s.CollegeId == userCollegeID).Select(e => e).ToList();
            ViewBag.AicteData = AicteNew;
            return View();
        }

        [HttpGet]
        [Authorize(Roles="Admin,College,DataEntry,FacultyVerification")]
        public ActionResult AicteFAcultyView()
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            } 
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AIF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (status > 0 && Roles.IsUserInRole("College") && isPageEditable==true)
            {
                var Aicte = db.jntuh_college_aictefaculty.Where(s => s.IsActive == true && s.CollegeId == userCollegeID).Select(e => e).ToList();
                ViewBag.AicteData = Aicte;
                return View();              
            }
            else
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult AicteFaculty()
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);

            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;

            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                return View();
                //bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AICTE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                //if (isPageEditable)
                //{
                //    ViewBag.IsEditable = true;
                //}
                //else
                //{
                //    ViewBag.IsEditable = false;
                //}
            }
            else
            {
                ViewBag.IsEditable = false;
            }


            //List<SelectListItem> Facultytype = new List<SelectListItem>();
            //Facultytype.Add(new SelectListItem() { Text = "UG", Value = "1" });
            //Facultytype.Add(new SelectListItem() { Text = "PG", Value = "2" });
            //ViewBag.FacultyType = Facultytype;

            //List<SelectListItem> FacultyJobType = new List<SelectListItem>();
            //FacultyJobType.Add(new SelectListItem() { Text = "FT", Value = "1" });
            //FacultyJobType.Add(new SelectListItem() { Text = "PT", Value = "2" });
            //ViewBag.FacultyJobType = FacultyJobType;

          


            //List<SelectListItem> Doctorate = new List<SelectListItem>();
            //Doctorate.Add(new SelectListItem() { Text = "YES", Value = "1" });
            //Doctorate.Add(new SelectListItem() { Text = "No", Value = "2" });
            //ViewBag.doctorate = Doctorate;

           
            //ViewBag.designation = db.jntuh_designation.Where(s => s.isActive == true).Take(4).ToList();

            //var Aicte = db.jntuh_college_aictefaculty.Where(s => s.IsActive == true && s.CollegeId == userCollegeID).Select(e => e).ToList();
            //ViewBag.AicteData = Aicte;
            return RedirectToAction("Index");
           
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult AicteFaculty(AICTEFacultyClass AICTEFacultyClass, HttpPostedFileBase ExcelFile)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            int actualYear =
               db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true && a.isActive == true)
                   .Select(s => s.actualYear)
                   .FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear+1) && a.isActive == true)
                   .Select(s => s.id)
                   .FirstOrDefault();
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int UserCollegeId = db.jntuh_college_users.Where(u => u.userID == userid).Select(u => u.collegeID).FirstOrDefault();
            if (UserCollegeId == 375)
            {
                UserCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }         

            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;


            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();
            //ViewBag.designation = db.jntuh_designation.Where(s => s.isActive == true).Take(4).ToList();

            //if (ExcelFile != null)
            //{
            //    #region Excel File Code
            //    string filePath = string.Empty;


            //    dynamic Result;
            //    //  string appfilepath = Server.MapPath("~/Content/AicteFacultyExcelFiles/");
            //    // string appfilepath = 
            //    string appfilename = ExcelFile.FileName;
            //    filePath = appfilename;
            //    string extension = Path.GetExtension(ExcelFile.FileName);
            //    string conString = string.Empty;
            //    switch (extension)
            //    {
            //        case ".xls":
            //            conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
            //            break;
            //        case ".xlsx":
            //            conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
            //            break;
            //    }
            //    conString = string.Format(conString, filePath);
            //    using (OleDbConnection connExcel = new OleDbConnection(conString))
            //    {
            //        using (OleDbCommand cmdExcel = new OleDbCommand())
            //        {
            //            using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
            //            {
            //                DataTable dt = new DataTable();
            //                cmdExcel.Connection = connExcel;
            //                connExcel.Open();
            //                DataTable dtExcelSchema;

            //                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //                string sheetName = dtExcelSchema.Rows[1]["Table_Name"].ToString();


            //                connExcel.Close();
            //                connExcel.Open();
            //                cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
            //                odaExcel.SelectCommand = cmdExcel;
            //                odaExcel.Fill(dt);
            //                connExcel.Close();
            //                int i = 1;

            //                var query = from row in dt.AsEnumerable()
            //                            select row;

            //                foreach (DataRow row in dt.Rows)
            //                {


            //                    jntuh_college_aictefaculty jntuh_college_aictefaculty = new jntuh_college_aictefaculty();
            //                    jntuh_college_aictefaculty.CollegeId = UserCollegeId;
            //                    jntuh_college_aictefaculty.FacultyId = row["FacultyId"].ToString();
            //                    jntuh_college_aictefaculty.RegistrationNumber = row["RegistrationNumber"].ToString();
            //                    jntuh_college_aictefaculty.FirstName = row["FirstName"].ToString();
            //                    jntuh_college_aictefaculty.SurName = row["SurName"].ToString();
            //                    jntuh_college_aictefaculty.PanNumber = row["PANNumber"].ToString();
            //                    jntuh_college_aictefaculty.AadhaarNumber = row["AadhaarNumber"].ToString();
            //                    jntuh_college_aictefaculty.Programme = row["Programme"].ToString();
            //                    jntuh_college_aictefaculty.Course = row["Course"].ToString();
            //                    // data.CollegeId = row["CollegeCode"].ToString(); ;

            //                    jntuh_college_aictefaculty.DateOfJoiningTheInstitute = null;
            //                    jntuh_college_aictefaculty.AppointmentType = null;
            //                    jntuh_college_aictefaculty.Doctorate = null;
            //                    jntuh_college_aictefaculty.MastersDegree = null;
            //                    jntuh_college_aictefaculty.BachelorsDegree = null;
            //                    jntuh_college_aictefaculty.OtherQualification = "Test";


            //                    jntuh_college_aictefaculty.IsActive = true;
            //                    jntuh_college_aictefaculty.CreatedBy = userid;
            //                    jntuh_college_aictefaculty.CreatedOn = DateTime.Now;
            //                    jntuh_college_aictefaculty.UpdatedBy = null;
            //                    jntuh_college_aictefaculty.UpdatedOn = null;

            //                    db.jntuh_college_aictefaculty.Add(jntuh_college_aictefaculty);
            //                    db.SaveChanges();
            //                }

            //            }
            //        }
            //    }

            //    #endregion
            //    return RedirectToAction("AicteFaculty");
            //}
            if (ModelState.IsValid == true)
            {
                if (AICTEFacultyClass != null)
                {
                    jntuh_college_aictefaculty jntuh_college_aictefaculty = new jntuh_college_aictefaculty();

                    jntuh_college_aictefaculty.CollegeId = UserCollegeId;
                    jntuh_college_aictefaculty.AcademicyearId = ay0;
                    jntuh_college_aictefaculty.FacultyId = AICTEFacultyClass.FacultyId.Trim();
                    jntuh_college_aictefaculty.Programme = Programme.Where(s => s.Value == AICTEFacultyClass.Programme).Select(s => s.Text).FirstOrDefault();
                    jntuh_college_aictefaculty.Course = AICTEFacultyClass.Course;
                    jntuh_college_aictefaculty.FirstName = AICTEFacultyClass.FirstName.Trim();
                    jntuh_college_aictefaculty.SurName = AICTEFacultyClass.SurName.Trim();
                    jntuh_college_aictefaculty.RegistrationNumber = AICTEFacultyClass.RegistrationNumber == null ? " " : AICTEFacultyClass.RegistrationNumber.Trim();
                    jntuh_college_aictefaculty.PanNumber = AICTEFacultyClass.PanNumber.ToUpper().Trim();
                    jntuh_college_aictefaculty.AadhaarNumber = AICTEFacultyClass.AadhaarNumber.Trim();
                    jntuh_college_aictefaculty.AICTEFacultyType = null;
                    jntuh_college_aictefaculty.JobType = null;
                    jntuh_college_aictefaculty.Email = null;
                    jntuh_college_aictefaculty.Mobile = null;
                    jntuh_college_aictefaculty.ExactDesignation = null;


                    jntuh_college_aictefaculty.DateOfJoiningTheInstitute = null;
                    jntuh_college_aictefaculty.AppointmentType = null;
                    jntuh_college_aictefaculty.Doctorate = null;
                    jntuh_college_aictefaculty.MastersDegree = null;
                    jntuh_college_aictefaculty.BachelorsDegree = null;
                    jntuh_college_aictefaculty.OtherQualification = "Test";


                    jntuh_college_aictefaculty.IsActive = true;
                    jntuh_college_aictefaculty.CreatedBy = userid;
                    jntuh_college_aictefaculty.CreatedOn = DateTime.Now;
                    jntuh_college_aictefaculty.UpdatedBy = null;
                    jntuh_college_aictefaculty.UpdatedOn = null;
                    db.jntuh_college_aictefaculty.Add(jntuh_college_aictefaculty);
                    db.SaveChanges();

                    TempData["message"] = "Registration completed successfully.";
           
                    return RedirectToAction("AicteFAcultyView");
                }
                else
                {
                    TempData["Error"] = "No data Found";
                    return RedirectToAction("AicteFAcultyView");
                }
            }

            else
            {
                TempData["Error"] = "Fill the All Fields";
                return RedirectToAction("AicteFAcultyView");
              
            }
          
        }
       
        [HttpGet]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult EditAICTEFaculty(int Id)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);

            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            
            
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //userCollegeID = 118;
            //List<SelectListItem> Facultytype = new List<SelectListItem>();
            //Facultytype.Add(new SelectListItem() { Text = "UG", Value = "1" });
            //Facultytype.Add(new SelectListItem() { Text = "PG", Value = "2" });
            //ViewBag.FacultyType = Facultytype;

            //List<SelectListItem> FacultyJobType = new List<SelectListItem>();
            //FacultyJobType.Add(new SelectListItem() { Text = "FT", Value = "1" });
            //FacultyJobType.Add(new SelectListItem() { Text = "PT", Value = "2" });
            //ViewBag.FacultyJobType = FacultyJobType;

            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;


            //List<SelectListItem> Doctorate = new List<SelectListItem>();
            //Doctorate.Add(new SelectListItem() { Text = "YES", Value = "1" });
            //Doctorate.Add(new SelectListItem() { Text = "No", Value = "2" });
            //ViewBag.doctorate = Doctorate;

            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
               
                //bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AICTE") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                //if (isPageEditable)
                //{
                //    ViewBag.IsEditable = true;
                //}
                //else
                //{
                //    ViewBag.IsEditable = false;
                //}
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("Index");
            }


            if (Id != null)
            {
                AICTEFacultyClass AICTEFacultyClass = new AICTEFacultyClass();
                var jntuh_college_aictefaculty = db.jntuh_college_aictefaculty.Where(s => s.Id==Id).Select(e => e).FirstOrDefault();
                AICTEFacultyClass.Id = jntuh_college_aictefaculty.Id;
                AICTEFacultyClass.FacultyId = jntuh_college_aictefaculty.FacultyId.Trim();
                AICTEFacultyClass.Programme = jntuh_college_aictefaculty.Programme.Trim();
                AICTEFacultyClass.Course = jntuh_college_aictefaculty.Course.Trim();
               
                AICTEFacultyClass.FirstName = jntuh_college_aictefaculty.FirstName.Trim();
                AICTEFacultyClass.SurName = jntuh_college_aictefaculty.SurName.Trim();
              
                AICTEFacultyClass.RegistrationNumber = jntuh_college_aictefaculty.RegistrationNumber.Trim();
                AICTEFacultyClass.PanNumber = jntuh_college_aictefaculty.PanNumber.Trim();
                AICTEFacultyClass.AadhaarNumber = jntuh_college_aictefaculty.AadhaarNumber.Trim();
                AICTEFacultyClass.CollegeId = jntuh_college_aictefaculty.CollegeId;
                return View(AICTEFacultyClass);
            }
            else
            {
                TempData["Error"] = "No Data Found";
                return RedirectToAction("AicteFAcultyView");
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult EditAICTEFaculty(AICTEFacultyClass AICTEFacultyClass)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            int actualYear =
               db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true && a.isActive == true)
                   .Select(s => s.actualYear)
                   .FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1) && a.isActive == true)
                   .Select(s => s.id)
                   .FirstOrDefault();
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            List<SelectListItem> Facultytype = new List<SelectListItem>();
            Facultytype.Add(new SelectListItem() { Text = "UG", Value = "1" });
            Facultytype.Add(new SelectListItem() { Text = "PG", Value = "2" });
            ViewBag.FacultyType = Facultytype;

            List<SelectListItem> FacultyJobType = new List<SelectListItem>();
            FacultyJobType.Add(new SelectListItem() { Text = "FT", Value = "1" });
            FacultyJobType.Add(new SelectListItem() { Text = "PT", Value = "2" });
            ViewBag.FacultyJobType = FacultyJobType;

            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;


            List<SelectListItem> Doctorate = new List<SelectListItem>();
            Doctorate.Add(new SelectListItem() { Text = "YES", Value = "1" });
            Doctorate.Add(new SelectListItem() { Text = "No", Value = "2" });
            ViewBag.doctorate = Doctorate;

            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();

            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int UserCollegeId = db.jntuh_college_users.Where(u => u.userID == userid).Select(s => s.id).FirstOrDefault();

           // UserCollegeId = 118;
            if(ModelState.IsValid==true)
            {
                if(AICTEFacultyClass != null)
                {
                    jntuh_college_aictefaculty jntuh_college_aictefaculty = db.jntuh_college_aictefaculty.Where(s => s.Id == AICTEFacultyClass.Id).Select(e => e).FirstOrDefault();

                    if (jntuh_college_aictefaculty !=null)
                    {
                        //jntuh_college_aictefaculty.CollegeId = UserCollegeId;
                        jntuh_college_aictefaculty.FacultyId = AICTEFacultyClass.FacultyId.Trim();
                        jntuh_college_aictefaculty.Programme = Programme.Where(s => s.Value == AICTEFacultyClass.Programme).Select(s => s.Text).FirstOrDefault();
                        jntuh_college_aictefaculty.Course = AICTEFacultyClass.Course.Trim();
                        jntuh_college_aictefaculty.FirstName = AICTEFacultyClass.FirstName.Trim();
                        jntuh_college_aictefaculty.SurName = AICTEFacultyClass.SurName.Trim();
                        jntuh_college_aictefaculty.RegistrationNumber = AICTEFacultyClass.RegistrationNumber == null ? " " : AICTEFacultyClass.RegistrationNumber.Trim();
                        jntuh_college_aictefaculty.PanNumber = AICTEFacultyClass.PanNumber.ToUpper().Trim();
                        jntuh_college_aictefaculty.AadhaarNumber = AICTEFacultyClass.AadhaarNumber.Trim();
                        
                        jntuh_college_aictefaculty.IsActive = true;
                        jntuh_college_aictefaculty.CreatedBy = jntuh_college_aictefaculty.CreatedBy;
                        jntuh_college_aictefaculty.CreatedOn = jntuh_college_aictefaculty.CreatedOn;
                        jntuh_college_aictefaculty.UpdatedBy = userid;
                        jntuh_college_aictefaculty.UpdatedOn = DateTime.Now;

                        db.Entry(jntuh_college_aictefaculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Faculty Details Updated Successfully";
                        return RedirectToAction("AicteFAcultyView");
                    }
                    else
                    {
                        TempData["Error"] = "No Data Found";
                        return RedirectToAction("AicteFAcultyView");
                    }
                   
                   
                }
                else
                {
                    TempData["Error"] = "Faculty Data Is Missing";
                    return RedirectToAction("AicteFAcultyView");
                }
            }
            else
            {
                TempData["Error"] = "Enter The All Mandatory Fields";
                return RedirectToAction("AicteFAcultyView");
            }

        }

        [HttpGet]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult ViewAICTEFaculty(int Id)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);

            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            
            //var departments = db.jntuh_department.Where(s => s.isActive == true).ToList();
            if (Id != null)
            {
                AICTEFacultyClass AICTEFacultyClass = new AICTEFacultyClass();
                var jntuh_college_aictefaculty = db.jntuh_college_aictefaculty.Where(s => s.Id==Id).Select(e => e).FirstOrDefault();
                AICTEFacultyClass.FacultyId = jntuh_college_aictefaculty.FacultyId;
                AICTEFacultyClass.Programme = jntuh_college_aictefaculty.Programme;
                AICTEFacultyClass.Course = jntuh_college_aictefaculty.Course;
         
                AICTEFacultyClass.FirstName = jntuh_college_aictefaculty.FirstName;
                AICTEFacultyClass.SurName = jntuh_college_aictefaculty.SurName;
       
                AICTEFacultyClass.RegistrationNumber = jntuh_college_aictefaculty.RegistrationNumber;
                AICTEFacultyClass.PanNumber = jntuh_college_aictefaculty.PanNumber;
                AICTEFacultyClass.AadhaarNumber = jntuh_college_aictefaculty.AadhaarNumber;
                AICTEFacultyClass.CollegeId = jntuh_college_aictefaculty.CollegeId;
                return View(AICTEFacultyClass);
            }
            else
            {
                TempData["Error"] = "No Data Found";
                return RedirectToAction("AicteFAcultyView");
            }

        }

        [HttpGet]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult DeleteAicte(int aid)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            if (aid != 0)
            {
                var data = db.jntuh_college_aictefaculty.Where(s => s.Id == aid).Select(e => e).FirstOrDefault();
                if (data != null)
                {
                    db.jntuh_college_aictefaculty.Remove(data);
                    db.SaveChanges();
                    TempData["Message"] = "Faculty Data is Deleted Successfully";
                    return RedirectToAction("AicteFAcultyView");
                }
                else
                {
                    TempData["Message"] = "Faculty Data is Not Found";
                    return RedirectToAction("AicteFAcultyView");
                }
            }
            else
            {
                TempData["Message"] = "Faculty Data is Not Found";
                return RedirectToAction("AicteFAcultyView");
            }
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            //}

            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            //}
            //DateTime todayDate = DateTime.Now.Date;
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
            //                                                             editStatus.IsCollegeEditable == true &&
            //                                                             editStatus.editFromDate <= todayDate &&
            //                                                             editStatus.editToDate >= todayDate)
            //                                        .Select(editStatus => editStatus.id)
            //                                        .FirstOrDefault();
            //if (status > 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = true;
            //}
            //else
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("Index");
            //}
          
        }

        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification")]
        public ActionResult AicteFacultyAdminView(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            //var jntuh_department = db.jntuh_department.ToList();
            //List<AICTEFacultyClass> AICTEFacultyClass = new List<AICTEFacultyClass>();
            List<jntuh_college_aictefaculty> jntuh_college_aictefaculty= new List<jntuh_college_aictefaculty>();
            if (collegeid != null)
            {
                jntuh_college_aictefaculty = db.jntuh_college_aictefaculty.Where(s => s.CollegeId == collegeid).ToList();
            }
            return View(jntuh_college_aictefaculty);
        }

        //Aadhar Number Checking
        public JsonResult CheckAadharNumber(string AadhaarNumber)
        {
            var status = aadharcard.validateVerhoeff(AadhaarNumber.Trim());
            if (status)
            {
                return Json(true);
            }
            else
            {
                return Json("AadhaarNumber is not a validnumber.", JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult email()
        //{
        //    string MailBody = string.Empty;
         
        //    MailBody = "<div>";
        //    MailBody = "<div style='text-align:center;width:50%'>";
        //    MailBody += @"<img src=""http://jntuhaac.in//Content/Images/new_logo.jpg"" alt=""Logo"" style='width:65px;height:65px;'/>";
        //    MailBody += "<div style='text-align:center; color:brown;'>";
        //    MailBody += "<b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD<br>Directorate of Affiliations & Academic Audit <br>Kukatpally, Hyderabad – 500 085, Telangana, India</b></div></div>";
        //    MailBody += "<hr style='width:50%;float:left;'/><br/>";
        //    MailBody += "<p style='float:left;'>Dear Sir/Madam : </p><br/>";
        //    MailBody += "<br/><p>Your Paymnet Status ,</p>";
        //    MailBody += "<table border='1' style='background-color:papayawhip;padding:5px;width:50%;line-height:25px;border:2px solid rosybrown;color:black;'>";
        //    MailBody += "<tr>";
        //    MailBody += "<td style='width:36%;border:none;'>College code / College name </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>ZZ/Sree Dattha Institute of Engineering and Science </td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Customer Id </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>123</td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Transaction Ref.no </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>4567</td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Bank Refno </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>23423</td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Transaction Amount </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>100.00</td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Transaction Date </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>today</td></tr>";
        //    MailBody += "<tr><td style='width:36%;border:none;'>Payment Description </td><td style='text-align:center;border:none;'>:</td><td style='border:none;'>yes</td></tr>";
        //    MailBody += "</table>";
        //    MailBody += "</div>";
          
        //    MailBody += "<br/>";
        //    MailBody += "<p>Thanks & Regards</p><p>Director, AAC,</p><p>JNTUH, Hyderabad</p>";
           
         


        //    System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
        //    message.To.Add("patnala.shiva@syntizen.com");
        //    message.Subject = "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS";
        //    message.Body = MailBody;
        //    message.IsBodyHtml = true;
        //    //  message.Attachments.Add(new Attachment(filepath));
        //    //  message.Attachments.Add(new Attachment(filepathsecond));
        //    var smtp = new SmtpClient();
        //    smtp.Credentials = new NetworkCredential("supportaac@jntuh.ac.in", "uaaac@aac");
        //    smtp.Host = "smtp.gmail.com";
        //    smtp.Port = 587;
        //    smtp.EnableSsl = true;
        //    smtp.Send(message);

        //    return View();
        //}

    }
}
