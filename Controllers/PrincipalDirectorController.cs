using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Controllers.College;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class PrincipalDirectorController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private uaaasDBContext db1 = new uaaasDBContext();
        private uaaasDBContext db2 = new uaaasDBContext();

        public class DistinctDepartment
        {
            public int id { get; set; }
            public string departmentName { get; set; }
        }

        // GET: /PrincipalDirector/
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index()
        {
            return View();
        }

        // GET: /CollegeInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            //return RedirectToAction("College", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                if (collegeId != null)
                {
                    if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int principalID = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type.Equals("PRINCIPAL")).Select(e => e.id).FirstOrDefault();
            int directorID = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if ((userCollegeID > 0 && (principalID > 0) || directorID > 0) && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PrincipalDirector");
            }
            if ((userCollegeID > 0 && (principalID > 0) || directorID > 0) && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Edit", "PrincipalDirector", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (status == 0 && (principalID == 0 || directorID == 0) && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PrincipalDirector");
            }

            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;

            foreach (var item in db.jntuh_department.Where(s => s.isActive == true).OrderBy(s => s.departmentName))
            {
                if (item.departmentName == "MBA")
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
                else
                {
                    if (!existingDepts.Contains(item.departmentName))
                    {
                        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                        existingDepts = existingDepts + "," + item.departmentName;
                    }
                }
            }

            ViewBag.department = depts;
            ViewBag.phd = db.jntuh_phd_subject.Where(s => s.isActive == true).OrderBy(s => s.id).ToList();

            PrincipalDirector Item1 = new PrincipalDirector();
            CollgeDirector Item2 = new CollgeDirector();
            Item1.collegeId = userCollegeID;
            Item2.collegeId = userCollegeID;
            return View(model: new Tuple<PrincipalDirector, CollgeDirector>(Item1, Item2));
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(PrincipalDirector Item1, CollgeDirector Item2)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Item1.collegeId;
                if (userCollegeID == 0)
                {
                    userCollegeID = Item2.collegeId;
                }
            }
            //SavePrincialDirectorInformation(Item1, Item2);
            return View("View");
        }

        private void SavePrincialInformation(PrincipalDirector Item1)
        {
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS/CollegePrincipalAadhaar";
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Item1.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            bool Principal = false;
            var resultMessagePrincipal = string.Empty;

            jntuh_college_principal_registered principal = new jntuh_college_principal_registered();

            string strCollegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;

            jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == Item1.RegistrationNumber).Select(e => e).FirstOrDefault();
            jntuh_registered_faculty jntuh_registered_faculty_flag = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == Item1.RegistrationNumber && (f.Blacklistfaculy == true || f.AbsentforVerification == true)).Select(e => e).FirstOrDefault();
            var collegefacultyregistered_CollegeId = db.jntuh_college_faculty_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Select(e => e.collegeId).SingleOrDefault();
            jntuh_college_principal_registered collegePrincipalregistered = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();

            if (jntuh_registered_faculty == null)
            {
                resultMessagePrincipal = "NotValid";
                TempData["ERROR"] = "Invalid Registration Number";
            }
            else if (jntuh_registered_faculty_flag != null)
            {
                if (jntuh_registered_faculty_flag.Blacklistfaculy == true && jntuh_registered_faculty_flag.AbsentforVerification == true)
                {
                    TempData["ERROR"] = "Faculty is Blacklisted due to possessing of ingenuine UG/PG/Ph.D. Certificates. And Faculty is made inactive due to your absence for physical verification.";
                }
                else
                {
                    if (jntuh_registered_faculty_flag.Blacklistfaculy == true)
                        TempData["ERROR"] = "Faculty is Blacklisted due to possessing of ingenuine UG/PG/Ph.D. Certificates.";
                    else if (jntuh_registered_faculty_flag.AbsentforVerification == true)
                        TempData["ERROR"] = "Faculty is made inactive due to your absence for physical verification.";
                }
            }
            else
            {
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var actualYear = jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(a => a.actualYear).FirstOrDefault();
                var academicYearId = jntuh_academic_year.Where(q => q.actualYear == (actualYear + 1)).Select(x => x.id).FirstOrDefault();

                if (collegefacultyregistered_CollegeId != null && collegefacultyregistered_CollegeId != 0)
                {
                    if (collegefacultyregistered_CollegeId == userCollegeID)
                    {
                        if (collegePrincipalregistered == null || collegePrincipalregistered.collegeId == 0)
                        {
                            if (db.jntuh_college_principal_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                            {
                                TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                            }
                            else
                            {
                                JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                                string status = AadharNumberCheck.Data.ToString();
                                if (status == "True")
                                {
                                    principal.collegeId = userCollegeID;
                                    principal.RegistrationNumber = Item1.RegistrationNumber;
                                    principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                    if (Item1.PrincipalAadharPhotoDocument != null)
                                    {
                                        if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                        {
                                            Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                        }

                                        var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                        {
                                            if (Item1.PrincipalAadharDocument == null)
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                                    fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                            else
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                                    Server.MapPath(aadhaarCardsPath),
                                                    fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                        }
                                    }
                                    else if (Item1.PrincipalAadharDocument != null)
                                    {
                                        principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                    }
                                    principal.createdBy = userID;
                                    principal.createdOn = DateTime.Now;
                                    db.jntuh_college_principal_registered.Add(principal);
                                    resultMessagePrincipal = "Save";
                                    db.SaveChanges();
                                    TempData["Success"] = "Principal Details are Added successfully.";

                                    //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                    //objART.academicYearId = academicYearId;
                                    //objART.collegeId = userCollegeID;
                                    //objART.RegistrationNumber = principal.RegistrationNumber;
                                    //objART.DepartmentId = 0;
                                    //objART.SpecializationId = 0;
                                    //objART.ActionType = 1;
                                    //objART.FacultyType = "Principal";
                                    //objART.FacultyStatus = "Y";
                                    //objART.Reasion = "Principal Insert";
                                    //objART.FacultyJoinDate = DateTime.Now;
                                    //objART.Createdon = DateTime.Now;
                                    //objART.CreatedBy = userID;
                                    //objART.Updatedon = null;
                                    //objART.UpdatedBy = null;
                                    //db.jntuh_college_facultytracking.Add(objART);
                                    //db.SaveChanges();
                                }
                                else
                                {
                                    TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                                }
                            }
                        }
                        else if (collegePrincipalregistered.collegeId == Item1.collegeId)
                        {
                            if (db.jntuh_college_principal_registered.Where(p => p.collegeId != userCollegeID && p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                            {
                                TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                            }
                            else
                            {
                                JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                                string status = AadharNumberCheck.Data.ToString();
                                if (status == "True")
                                {
                                    principal.id = Item1.id;
                                    principal.collegeId = userCollegeID;
                                    principal.RegistrationNumber = Item1.RegistrationNumber;
                                    principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                    if (Item1.PrincipalAadharPhotoDocument != null)
                                    {
                                        if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                        {
                                            Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                        }

                                        var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                        {
                                            if (Item1.PrincipalAadharDocument == null)
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                            else
                                            {
                                                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                                Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                                Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                                principal.AadhaarDocument = Item1.AadharDocumentView;
                                            }
                                        }
                                    }
                                    else if (Item1.PrincipalAadharDocument != null)
                                    {
                                        principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                    }

                                    if (collegePrincipalregistered.RegistrationNumber == Item1.RegistrationNumber)
                                    {
                                        principal.createdBy = Item1.createdBy;
                                        principal.createdOn = Item1.createdOn;
                                        principal.updatedBy = userID;
                                        principal.updatedOn = DateTime.Now;
                                    }
                                    else
                                    {
                                        principal.createdBy = userID;
                                        principal.createdOn = DateTime.Now;
                                    }

                                    db.Entry(principal).State = EntityState.Modified;
                                    resultMessagePrincipal = "Update";
                                    db.SaveChanges();
                                    TempData["Success"] = "Principal Details are Updated successfully.";

                                    //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                    //objART.academicYearId = academicYearId;
                                    //objART.collegeId = userCollegeID;
                                    //objART.RegistrationNumber = principal.RegistrationNumber.Trim().ToString();
                                    //objART.DepartmentId = 0;
                                    //objART.SpecializationId = 0;
                                    //objART.ActionType = 3;
                                    //objART.FacultyType = "Principal";
                                    //objART.FacultyStatus = "Y";
                                    //objART.Reasion = "Principal Updated";
                                    //objART.FacultyJoinDate = principal.createdOn;
                                    //objART.Createdon = DateTime.Now;
                                    //objART.CreatedBy = userID;
                                    //objART.Updatedon = null;
                                    //objART.UpdatedBy = null;
                                    //db.jntuh_college_facultytracking.Add(objART);
                                    //db.SaveChanges();
                                }
                                else
                                {
                                    TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["ERROR"] = "Faculty is already working in other JNTUH affiliated college.";
                    }
                }
                else
                {
                    if (collegePrincipalregistered == null || collegePrincipalregistered.collegeId == 0)
                    {
                        if (db.jntuh_college_principal_registered.Where(p => p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                        {
                            TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                        }
                        else
                        {
                            JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                            string status = AadharNumberCheck.Data.ToString();
                            if (status == "True")
                            {
                                principal.collegeId = userCollegeID;
                                principal.RegistrationNumber = Item1.RegistrationNumber;
                                principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                if (Item1.PrincipalAadharPhotoDocument != null)
                                {
                                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                    {
                                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                    }

                                    var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                    {
                                        if (Item1.PrincipalAadharDocument == null)
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                                                fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                        else
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}",
                                                Server.MapPath(aadhaarCardsPath),
                                                fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                    }
                                }
                                else if (Item1.PrincipalAadharDocument != null)
                                {
                                    principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                }
                                principal.createdBy = userID;
                                principal.createdOn = DateTime.Now;
                                db.jntuh_college_principal_registered.Add(principal);
                                resultMessagePrincipal = "Save";
                                db.SaveChanges();
                                TempData["Success"] = "Principal Details are Added successfully.";

                                //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                //objART.academicYearId = academicYearId;
                                //objART.collegeId = userCollegeID;
                                //objART.RegistrationNumber = principal.RegistrationNumber;
                                //objART.DepartmentId = 0;
                                //objART.SpecializationId = 0;
                                //objART.ActionType = 1;
                                //objART.FacultyType = "Principal";
                                //objART.FacultyStatus = "Y";
                                //objART.Reasion = "Principal Insert";
                                //objART.FacultyJoinDate = DateTime.Now;
                                //objART.Createdon = DateTime.Now;
                                //objART.CreatedBy = userID;
                                //objART.Updatedon = null;
                                //objART.UpdatedBy = null;
                                //db.jntuh_college_facultytracking.Add(objART);
                                //db.SaveChanges();
                            }
                            else
                            {
                                TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                            }
                        }
                    }
                    else if (collegePrincipalregistered.collegeId == Item1.collegeId)
                    {
                        if (db.jntuh_college_principal_registered.Where(p => p.collegeId != userCollegeID && p.RegistrationNumber == Item1.RegistrationNumber).Count() != 0)
                        {
                            TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                        }
                        else
                        {
                            JsonResult AadharNumberCheck = CheckAadharNumber(Item1.PrincipalAadhaarNumber, Item1.RegistrationNumber);
                            string status = AadharNumberCheck.Data.ToString();
                            if (status == "True")
                            {
                                principal.id = Item1.id;
                                principal.collegeId = userCollegeID;
                                principal.RegistrationNumber = Item1.RegistrationNumber;
                                principal.AadhaarNumber = Item1.PrincipalAadhaarNumber;
                                if (Item1.PrincipalAadharPhotoDocument != null)
                                {
                                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                                    {
                                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
                                    }
                                    var ext = Path.GetExtension(Item1.PrincipalAadharPhotoDocument.FileName);
                                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                    {
                                        if (Item1.PrincipalAadharDocument == null)
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                        else
                                        {
                                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                                            Item1.PrincipalAadharPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath), fileName, ext));
                                            Item1.AadharDocumentView = string.Format("{0}{1}", fileName, ext);
                                            principal.AadhaarDocument = Item1.AadharDocumentView;
                                        }
                                    }
                                }
                                else if (Item1.PrincipalAadharDocument != null)
                                {
                                    principal.AadhaarDocument = Item1.AadharDocumentView = Item1.PrincipalAadharDocument;
                                }

                                if (collegePrincipalregistered.RegistrationNumber == Item1.RegistrationNumber)
                                {
                                    principal.createdBy = Item1.createdBy;
                                    principal.createdOn = Item1.createdOn;
                                    principal.updatedBy = userID;
                                    principal.updatedOn = DateTime.Now;
                                }
                                else
                                {
                                    principal.createdBy = userID;
                                    principal.createdOn = DateTime.Now;
                                }
                                db.Entry(principal).State = EntityState.Modified;
                                resultMessagePrincipal = "Update";
                                db.SaveChanges();
                                TempData["Success"] = "Principal Details are Updated successfully.";

                                //jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                                //objART.academicYearId = academicYearId;
                                //objART.collegeId = userCollegeID;
                                //objART.RegistrationNumber = principal.RegistrationNumber.Trim().ToString();
                                //objART.DepartmentId = 0;
                                //objART.SpecializationId = 0;
                                //objART.ActionType = 3;
                                //objART.FacultyType = "Principal";
                                //objART.FacultyStatus = "Y";
                                //objART.Reasion = "Principal Updated";
                                //objART.FacultyJoinDate = principal.createdOn;
                                //objART.Createdon = DateTime.Now;
                                //objART.CreatedBy = userID;
                                //objART.Updatedon = null;
                                //objART.UpdatedBy = null;
                                //db.jntuh_college_facultytracking.Add(objART);
                                //db.SaveChanges();
                            }
                            else
                            {
                                TempData["ERROR"] = "Aadhaar Number is already Exists with another Faculty";
                            }

                        }
                    }
                    else
                    {
                        TempData["ERROR"] = "Principal is already working in other JNTUH affiliated college.";
                    }
                }

                //Principal Experiance Saving on 15-02-2020
                jntuh_registered_faculty_experience updatefacultyexperiance =
                db.jntuh_registered_faculty_experience.Where(e => e.Id == Item1.ExperienceId)
                    .Select(s => s)
                    .FirstOrDefault();
                string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";
                if (updatefacultyexperiance != null)
                {
                    if (Item1.PrincipalAppointmentDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalAppointmentDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalAppointmentDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                                fileName, ext));
                            Item1.AppointmentDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.facultyJoiningOrder = Item1.AppointmentDocumentView;
                        }
                    }
                    if (Item1.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            Item1.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.facultyRelievingLetter = Item1.RelivingDocumentView;
                        }
                    }
                    //Optional
                    if (Item1.PrincipalSelectionCommitteeDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalSelectionCommitteeDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalSelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                                fileName, ext));
                            Item1.SelectionCommitteeDocumentView = string.Format("{0}{1}", fileName, ext);
                            updatefacultyexperiance.FacultySCMDocument = Item1.SelectionCommitteeDocumentView;
                        }
                    }
                    if (Item1.Previouscollegeid == 375)
                    {
                        updatefacultyexperiance.collegeId = Item1.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = Item1.Otherscollegename;
                    }
                    else
                    {
                        updatefacultyexperiance.collegeId = Item1.Previouscollegeid;
                        updatefacultyexperiance.OtherCollege = null;
                    }

                    updatefacultyexperiance.facultyDesignationId = 4;
                    updatefacultyexperiance.OtherDesignation = "Principal";
                    if (Item1.PrincipalSalary != null)
                        updatefacultyexperiance.facultySalary = Item1.PrincipalSalary;


                    if (!String.IsNullOrEmpty(Item1.dateOfAppointment))
                        updatefacultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    if (!String.IsNullOrEmpty(Item1.PrincipaldateOfResignation))
                        updatefacultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    updatefacultyexperiance.updatedBy = userID;
                    updatefacultyexperiance.updatedOn = DateTime.Now;
                    db.Entry(updatefacultyexperiance).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();

                    facultyexperiance.facultyId = jntuh_registered_faculty.id;
                    facultyexperiance.createdBycollegeId = userCollegeID;
                    if (Item1.PrincipalAppointmentDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyappointmentletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyappointmentletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalAppointmentDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalAppointmentDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyappointmentletters),
                                fileName, ext));
                            Item1.AppointmentDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyJoiningOrder = Item1.AppointmentDocumentView;
                        }
                    }

                    if (Item1.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            Item1.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyRelievingLetter = Item1.RelivingDocumentView;
                        }
                    }

                    //Optional
                    if (Item1.PrincipalSelectionCommitteeDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                        }

                        var ext = Path.GetExtension(Item1.PrincipalSelectionCommitteeDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            jntuh_registered_faculty.FirstName.Substring(0, 1) + "-" + jntuh_registered_faculty.LastName.Substring(0, 1);
                            Item1.PrincipalSelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                                fileName, ext));
                            Item1.SelectionCommitteeDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.FacultySCMDocument = Item1.SelectionCommitteeDocumentView;
                        }
                    }

                    if (Item1.Previouscollegeid == 375)
                    {
                        facultyexperiance.collegeId = Item1.Previouscollegeid;
                        facultyexperiance.OtherCollege = Item1.Otherscollegename;
                    }
                    else
                    {
                        facultyexperiance.collegeId = Item1.Previouscollegeid;
                    }
                    facultyexperiance.facultyDesignationId = 4;
                    facultyexperiance.OtherDesignation = "Principal";

                    if (Item1.PrincipalSalary != null)
                        facultyexperiance.facultySalary = Item1.PrincipalSalary;
                    if (!String.IsNullOrEmpty(Item1.dateOfAppointment))
                        facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    if (!String.IsNullOrEmpty(Item1.PrincipaldateOfResignation))
                        facultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);

                    facultyexperiance.createdBy = userID;
                    facultyexperiance.createdOn = DateTime.Now;
                    facultyexperiance.updatedBy = null;
                    facultyexperiance.updatedOn = null;
                    db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                    db.SaveChanges();
                }
                if (resultMessagePrincipal == "Save")
                {
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    objART.academicYearId = academicYearId;
                    objART.collegeId = userCollegeID;
                    objART.RegistrationNumber = principal.RegistrationNumber;
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 1;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Insert";
                    objART.previousworkingcollegeid = Item1.Previouscollegeid;
                    objART.scmdocument = Item1.SelectionCommitteeDocumentView;
                    objART.FacultyJoinDate = Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    objART.FacultyJoinDocument = Item1.AppointmentDocumentView;
                    objART.relevingdate = Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    objART.relevingdocumnt = Item1.RelivingDocumentView;
                    objART.aadhaarnumber = Item1.PrincipalAadhaarNumber.Trim();
                    objART.aadhaardocument = Item1.AadharDocumentView;
                    objART.payscale = Item1.PrincipalSalary;
                    objART.designation = null; // Others
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                }
                else if (resultMessagePrincipal == "Update")
                {
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    objART.academicYearId = academicYearId;
                    objART.collegeId = userCollegeID;
                    objART.RegistrationNumber = principal.RegistrationNumber.Trim();
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 3;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Updated";
                    objART.previousworkingcollegeid = Item1.Previouscollegeid;
                    objART.scmdocument = Item1.SelectionCommitteeDocumentView;
                    objART.FacultyJoinDate = Utilities.DDMMYY2MMDDYY(Item1.dateOfAppointment);
                    objART.FacultyJoinDocument = Item1.AppointmentDocumentView;
                    objART.relevingdate = Utilities.DDMMYY2MMDDYY(Item1.PrincipaldateOfResignation);
                    objART.relevingdocumnt = Item1.RelivingDocumentView;
                    objART.aadhaarnumber = Item1.PrincipalAadhaarNumber.Trim();
                    objART.aadhaardocument = Item1.AadharDocumentView;
                    objART.payscale = Item1.PrincipalSalary;
                    objART.designation = null; // Others
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                }
            }
        }

        private void SaveDirectorInformation(CollgeDirector Item2)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
                userCollegeID = Item2.collegeId;
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var resultMessage = string.Empty;
            string strCollegeCode = db.jntuh_college.Find(userCollegeID).collegeCode;
            jntuh_college_principal_director director = new jntuh_college_principal_director();
            if (Item2.firstName != null && Item2.surname != null)
            {
                director.collegeId = userCollegeID;
                director.type = "DIRECTOR";
                director.firstName = Item2.firstName;
                director.lastName = Item2.lastName == null ? string.Empty : Item2.lastName;
                director.surname = Item2.surname;
                director.qualificationId = Item2.qualificationId;
                director.dateOfAppointment = Utilities.DDMMYY2MMDDYY(Item2.dateOfAppointment);
                director.dateOfResignation = Item2.dateOfResignation;
                director.dateOfBirth = Utilities.DDMMYY2MMDDYY(Item2.dateOfBirth);
                director.fax = Item2.fax;
                director.landline = Item2.landline;
                director.mobile = Item2.mobile;
                director.email = Item2.email;
                director.departmentId = Item2.departmentId;
                director.phdId = Item2.phdId;
                director.phdFromUniversity = Item2.phdFromUniversity;
                director.phdYear = Item2.phdYear;

                string photoPath = "~/Content/Upload/PrincipalDirectorPhotos";
                if (Item2.DirectorPhotoDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(photoPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(photoPath));
                    }

                    var ext = Path.GetExtension(Item2.DirectorPhotoDocument.FileName);

                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = "D-" + strCollegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        Item2.DirectorPhotoDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                        director.photo = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (Item2.DirectorPhoto != null)
                {
                    director.photo = Item2.DirectorPhoto;
                }

                int directorID = db.jntuh_college_principal_director.Where(p => p.collegeId == userCollegeID && p.type == "DIRECTOR").Select(p => p.id).FirstOrDefault();

                if (directorID == 0)
                {
                    resultMessage = "Save";
                    director.createdBy = userID;
                    director.createdOn = DateTime.Now;
                    db.jntuh_college_principal_director.Add(director);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Director Details are Added successfully.";

                }
                else
                {
                    director.id = directorID;
                    director.createdBy = db.jntuh_college_principal_director.Where(p => p.id == directorID && p.type == "DIRECTOR").Select(p => p.createdBy).FirstOrDefault();
                    director.createdOn = db.jntuh_college_principal_director.Where(p => p.id == directorID && p.type == "DIRECTOR").Select(p => p.createdOn).FirstOrDefault();
                    director.updatedBy = userID;
                    director.updatedOn = DateTime.Now;
                    db.Entry(director).State = EntityState.Modified;
                    db.SaveChanges();
                    resultMessage = "Update";
                    TempData["SUCCESS"] = "Director Details are Updated successfully.";
                }
            }
            else
            {
                TempData["ERROR"] = "Director Details are Missing.Please Try";
            }
        }



        #region After Principal Changes Adding Appoinment Letter By Narayana Reddy on 15-02-2020

        //New Code Added by Narayana Reddy on 14-02-2020 for Principal
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AddPrincipal()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();


            ViewBag.facId = facultyId;
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;

            return PartialView("AddPrincipal", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult AddPrincipal(PrincipalDirector princial)
        {
            SavePrincialInformation(princial);
            return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult PrincipalEdit(string fid, string collegeId)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuhprincipal = db.jntuh_college_principal_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuhprincipal == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "Faculty");
                }
                faculty.PrincipalId = regfaculty.id;
                faculty.RegistrationNumber = jntuhprincipal.RegistrationNumber.Trim();
                faculty.id = jntuhprincipal.id;
                faculty.collegeId = userCollegeID;
                faculty.firstName = regfaculty.FirstName;
                faculty.lastName = regfaculty.MiddleName;
                faculty.surname = regfaculty.LastName;
                faculty.PrincipalAadhaarNumber = jntuhprincipal.AadhaarNumber;
                faculty.PrincipalAadharDocument = jntuhprincipal.AadhaarDocument;
            }

            ViewBag.facId = facultyId;


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();



            //Colleges and Designation Drop Dow Codding written by Narayana Reddy on 11-02-2020
            List<SelectListItem> colleges = new List<SelectListItem>();
            var colleges_list = db.jntuh_college.Where(c => c.isActive == true && c.id != 375).Select(c => new { CollegeId = c.id, CollegeName = c.collegeName + " (" + c.collegeCode + ")" }).OrderBy(c => c.CollegeName).ToList();
            colleges = colleges_list.Select(s => new SelectListItem { Value = s.CollegeId.ToString(), Text = s.CollegeName }).ToList();
            colleges.Add(new SelectListItem { Value = "375", Text = "Others" });
            ViewBag.Institutions = colleges;
            var Designation = db.jntuh_designation.Where(e => e.isActive == true).Select(a => new { Id = a.id, designation = a.designation }).Take(4).ToList();
            ViewBag.Designation = Designation;


            jntuh_registered_faculty_experience facultyexperiance =
                db.jntuh_registered_faculty_experience.Where(
                    r => r.createdBycollegeId == userCollegeID && r.facultyId == regfaculty.id && r.OtherDesignation == "Principal")
                    .Select(s => s).ToList()
                    .LastOrDefault();
            if (facultyexperiance != null)
            {
                faculty.ExperienceId = facultyexperiance.Id;
                faculty.PrincipalId = facultyexperiance.facultyId;
                faculty.Previouscollegeid = (int)facultyexperiance.collegeId;
                faculty.Otherscollegename = facultyexperiance.OtherCollege;
                if (facultyexperiance.facultyDateOfAppointment != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfAppointment);
                    faculty.dateOfAppointment = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                if (facultyexperiance.facultyDateOfResignation != null)
                {
                    DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfResignation);
                    faculty.PrincipaldateOfResignation = date.ToString("dd/MM/yyyy").Split(' ')[0];
                }
                faculty.PrincipalSalary = facultyexperiance.facultySalary;
                //faculty.dateOfAppointment = facultyexperiance.facultyDateOfAppointment.ToString();
                //faculty.dateOfResignation = facultyexperiance.facultyDateOfResignation.ToString();
                faculty.AppointmentDocumentView = facultyexperiance.facultyJoiningOrder;
                faculty.RelivingDocumentView = facultyexperiance.facultyRelievingLetter;
                faculty.SelectionCommitteeDocumentView = facultyexperiance.FacultySCMDocument;
                //if (faculty.ViewRelivingDocument == null)
                //{
                //    faculty.Principalfresherexperiance = "Fresher";
                //}
                //else
                //{
                //    faculty.Principalfresherexperiance = "Experienced";
                //}               

            }
            return PartialView("PrincipalEdit", faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpPost]
        public ActionResult PrincipalEdit(PrincipalDirector princial)
        {
            SavePrincialInformation(princial);
            return View("View");
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (collegeId != null)
                {
                    if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PrincipalDirector");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PrincipalDirector");
                }
            }

            List<SelectListItem> MemberType = new List<SelectListItem>();
            MemberType.Add(new SelectListItem() { Text = "Principal", Value = "1" });
            MemberType.Add(new SelectListItem() { Text = "Director", Value = "2" });
            ViewBag.MemberType = MemberType.Select(a => new
            {
                typeId = a.Value,
                type = a.Text
            });

            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            PrincipalDirector principal = new PrincipalDirector();
            jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();

            if (_principal != null)
            {
                jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).SingleOrDefault();
                principal.id = _principal.id;
                principal.collegeId = userCollegeID;
                principal.RegistrationNumber = _principal.RegistrationNumber;
                principal.firstName = prinicipaldetails.FirstName;
                principal.lastName = prinicipaldetails.MiddleName;
                principal.surname = prinicipaldetails.LastName;
                principal.PrincipalAadhaarNumber = _principal.AadhaarNumber;
                principal.PrincipalAadharDocument = _principal.AadhaarDocument;
                principal.departmentName = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                principal.dateOfAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();

                principal.dateOfBirth = prinicipaldetails.DateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfBirth.ToString()).ToString();
                principal.fax = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.fax).FirstOrDefault();
                principal.landline = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.landline).FirstOrDefault();
                principal.mobile = prinicipaldetails.Mobile;
                principal.email = prinicipaldetails.Email;
                principal.PrincipalPhoto = prinicipaldetails.Photo;
                principal.createdOn = _principal.createdOn;
                principal.createdBy = _principal.createdBy;
            }

            CollgeDirector director = new CollgeDirector();
            jntuh_college_principal_director _director = db.jntuh_college_principal_director.Where(a => a.collegeId == userCollegeID && a.type == "DIRECTOR").Select(d => d).FirstOrDefault();
            if (_director != null)
            {
                director.id = _director.id;
                director.collegeId = userCollegeID;
                director.firstName = _director.firstName;
                director.lastName = _director.lastName;
                director.surname = _director.surname;
                director.qualificationId = _director.qualificationId;
                director.dateOfAppointment = _director.dateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfAppointment.ToString()).ToString();
                director.dateOfBirth = _director.dateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfBirth.ToString()).ToString();
                director.fax = _director.fax;
                director.landline = _director.landline;
                director.mobile = _director.mobile;
                director.email = _director.email;
                director.DirectorPhoto = _director.photo;
                director.phdId = _director.phdId;
                director.phd = db.jntuh_phd_subject.Where(q => q.id == _director.phdId).Select(a => a.phdSubjectName).FirstOrDefault();
                director.phdFromUniversity = _director.phdFromUniversity;
                director.phdYear = Convert.ToInt32(_director.phdYear);
                director.departmentId = _director.departmentId;
                director.department = departments.Where(d => d.id == _director.departmentId).Select(z => z.departmentName).FirstOrDefault();
                //TempData["DirectorEdit"] = true;
            }

            List<int> collegespecs = new List<int>();

            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();

            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);
            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);
                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { DepartmentId = specid.departmentId, DepartmentName = deptment.departmentDescription, SpecializationId = specid.id });
                }
            }
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
            ViewBag.phd = db.jntuh_phd_subject.Where(s => s.isActive == true).ToList();
            return View("Edit", model: new Tuple<PrincipalDirector, CollgeDirector>(principal, director));
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(PrincipalDirector Item1, CollgeDirector Item2, int? typeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Item1.collegeId;
                if (userCollegeID == 0)
                {
                    userCollegeID = Item2.collegeId;
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            Item1.collegeId = userCollegeID;
            Item2.collegeId = userCollegeID;
            Item2.phdYear = Item2.phdYear == null ? 0 : (int)Item2.phdYear;
            //if (typeId != null && typeId != 0)
            //{
            //    if (typeId == 1)
            //        SavePrincialInformation(Item1);
            //    else if (typeId == 2)
            //        
            //}
            SaveDirectorInformation(Item2);
            return RedirectToAction("View");
        }
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Delete(string fid, string collegeId)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int facultyId = 0;
            TempData["path"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            if (!string.IsNullOrEmpty(fid))
            {
                //  facultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Changed by Naushad Khan
                facultyId = Convert.ToInt32(fid);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            PrincipalDirector faculty = new PrincipalDirector();
            jntuh_registered_faculty regfaculty = new jntuh_registered_faculty();
            if (facultyId != 0)
            {
                regfaculty = db.jntuh_registered_faculty.Find(facultyId);
                var jntuhprincipal = db.jntuh_college_principal_registered.Where(e => e.RegistrationNumber.Trim() == regfaculty.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (jntuhprincipal == null)
                {
                    TempData["Error"] = "Faculty Data not found.";
                    return RedirectToAction("Teaching", "Faculty");
                }
                faculty.PrincipalId = regfaculty.id;
                faculty.RegistrationNumber = jntuhprincipal.RegistrationNumber.Trim();
                faculty.id = jntuhprincipal.id;
                faculty.PrincipalId = regfaculty.id;
                faculty.collegeId = userCollegeID;
                faculty.firstName = regfaculty.FirstName;
                faculty.lastName = regfaculty.MiddleName;
                faculty.surname = regfaculty.LastName;
                faculty.PrincipalAadhaarNumber = jntuhprincipal.AadhaarNumber;
                faculty.PrincipalAadharDocument = jntuhprincipal.AadhaarDocument;
            }

            ViewBag.facId = facultyId;


            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            return PartialView("Delete", faculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult Delete(PrincipalDirector principal)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentAyId = db.jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (principal.PrincipalId != 0)
            {
                var Principal = db.jntuh_college_principal_registered.Where(a => a.collegeId == userCollegeID).Select(a => a).FirstOrDefault();
                if (Principal != null)
                {
                    var regfacultydata = db.jntuh_registered_faculty.Find(principal.PrincipalId);
                    jntuh_registered_faculty_experience facultyexperiance = new jntuh_registered_faculty_experience();
                    //
                    string facultyappointmentletters = "~/Content/Upload/College/Faculty/AppointmentLetters";
                    string facultyrelevingletters = "~/Content/Upload/College/Faculty/RelevingLetters";
                    string facultypreviouscollegescms = "~/Content/Upload/College/Faculty/PreviousSCMs";

                    facultyexperiance.facultyId = principal.PrincipalId;
                    facultyexperiance.createdBycollegeId = userCollegeID;


                    if (principal.PrincipalRelivingDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultyrelevingletters)))
                        {
                            Directory.CreateDirectory(Server.MapPath(facultyrelevingletters));
                        }

                        var ext = Path.GetExtension(principal.PrincipalRelivingDocument.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                            regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                            principal.PrincipalRelivingDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyrelevingletters),
                                fileName, ext));
                            principal.RelivingDocumentView = string.Format("{0}{1}", fileName, ext);
                            facultyexperiance.facultyRelievingLetter = principal.RelivingDocumentView;
                        }
                    }

                    ////Optional
                    //if (faculty.SelectionCommitteeDocument != null)
                    //{
                    //    if (!Directory.Exists(Server.MapPath(facultypreviouscollegescms)))
                    //    {
                    //        Directory.CreateDirectory(Server.MapPath(facultypreviouscollegescms));
                    //    }

                    //    var ext = Path.GetExtension(faculty.SelectionCommitteeDocument.FileName);

                    //    if (ext.ToUpper().Equals(".PDF"))
                    //    {
                    //        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                    //        regfacultydata.FirstName.Substring(0, 1) + "-" + regfacultydata.LastName.Substring(0, 1);
                    //        faculty.SelectionCommitteeDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultypreviouscollegescms),
                    //            fileName, ext));
                    //        faculty.ViewSelectionCommitteeDocument = string.Format("{0}{1}", fileName, ext);
                    //        facultyexperiance.FacultySCMDocument = faculty.ViewSelectionCommitteeDocument;
                    //    }
                    //}
                    //if (faculty.Previouscollegeid == 0)
                    //{
                    //    facultyexperiance.collegeId = userCollegeID;
                    //}
                    //else
                    //{
                    //    if (faculty.Previouscollegeid == 375)
                    //    {
                    //        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    //        facultyexperiance.OtherCollege = faculty.Otherscollegename;
                    //    }
                    //    else
                    //    {
                    //        facultyexperiance.collegeId = faculty.Previouscollegeid;
                    //    }
                    //}

                    facultyexperiance.collegeId = userCollegeID;
                    facultyexperiance.facultyDesignationId = 4;
                    facultyexperiance.OtherDesignation = "Principal";

                    if (!String.IsNullOrEmpty(principal.dateOfAppointment))
                        facultyexperiance.facultyDateOfAppointment = Models.Utilities.DDMMYY2MMDDYY(principal.dateOfAppointment);
                    if (!String.IsNullOrEmpty(principal.PrincipaldateOfResignation))
                        facultyexperiance.facultyDateOfResignation =
                        Models.Utilities.DDMMYY2MMDDYY(principal.PrincipaldateOfResignation);

                    facultyexperiance.createdBy = userID;
                    facultyexperiance.createdOn = DateTime.Now;
                    facultyexperiance.updatedBy = null;
                    facultyexperiance.updatedOn = null;
                    db.jntuh_registered_faculty_experience.Add(facultyexperiance);
                    db.SaveChanges();

                    var jntuhPrinicalTrackingData =
                        db.jntuh_college_facultytracking.AsNoTracking()
                            .Where(
                                i =>
                                    i.RegistrationNumber == Principal.RegistrationNumber &&
                                    i.collegeId == Principal.collegeId && i.isActive == true && i.ActionType != 2 && i.FacultyType == "Principal")
                            .Select(e => e)
                            .OrderByDescending(i => i.Id)
                            .FirstOrDefault();


                    db.jntuh_college_principal_registered.Remove(Principal);
                    db.SaveChanges();
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    //jntuh_attendence_registrationnumberstracking objART = new jntuh_attendence_registrationnumberstracking();
                    objART.academicYearId = presentAyId;
                    objART.collegeId = Principal.collegeId;
                    objART.RegistrationNumber = Principal.RegistrationNumber;
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 2;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Deleted by College Successfully.";
                    // objART.FacultyStatus = "Y";
                    //  objART.Reasion = "Faculty Deleted by College Successfully.";
                    objART.FacultyJoinDate = Principal.createdOn;
                    if (jntuhPrinicalTrackingData != null)
                    {
                        objART.previousworkingcollegeid = jntuhPrinicalTrackingData.previousworkingcollegeid;
                        objART.scmdocument = jntuhPrinicalTrackingData.scmdocument;
                        objART.FacultyJoinDate = jntuhPrinicalTrackingData.FacultyJoinDate;
                        objART.FacultyJoinDocument = jntuhPrinicalTrackingData.FacultyJoinDocument;
                        objART.aadhaarnumber = jntuhPrinicalTrackingData.aadhaarnumber;
                        objART.aadhaardocument = jntuhPrinicalTrackingData.aadhaardocument;
                        objART.payscale = jntuhPrinicalTrackingData.payscale;
                        objART.designation = jntuhPrinicalTrackingData.designation;
                    }
                    objART.relevingdate = facultyexperiance.facultyDateOfResignation;
                    objART.relevingdocumnt = facultyexperiance.facultyRelievingLetter;
                    objART.isActive = true;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Principal is Deleted Successfully.";
                }
            }

            return RedirectToAction("View");
        }


        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult DeleteDirector()
        {
            //return RedirectToAction("Index", "UnderConstruction");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID != 0)
            {
                var college_Director =
                    db.jntuh_college_principal_director.Where(r => r.collegeId == userCollegeID)
                        .Select(s => s)
                        .FirstOrDefault();
                if (college_Director != null)
                {
                    db.jntuh_college_principal_director.Remove(college_Director);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Principal is Deleted Successfully.";

                }
                else
                {
                    TempData["ERROR"] = "No Data found";
                }
            }

            return RedirectToAction("View");
        }

        #endregion

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}        
            //return RedirectToAction("College", "Dashboard");
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }

            int userID = Convert.ToInt32(userdata.ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID <= 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PD") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            PrincipalDirector principal = new PrincipalDirector();
            CollgeDirector director = new CollgeDirector();

            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();

            jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();


            if (_principal != null)
            {
                jntuh_registered_faculty prinicipaldetails = db.jntuh_registered_faculty.Where(p => p.RegistrationNumber == _principal.RegistrationNumber).Select(p => p).FirstOrDefault();
                var facultyexperiance =
                db.jntuh_registered_faculty_experience.Where(
                    r => r.createdBycollegeId == userCollegeID && r.facultyId == prinicipaldetails.id && r.OtherDesignation == "Principal")
                    .Select(s => s).ToList()
                    .LastOrDefault();
                if (prinicipaldetails != null)
                {
                    principal.id = _principal.id;
                    principal.PrincipalId = prinicipaldetails.id;
                    principal.collegeId = _principal.collegeId;
                    principal.RegistrationNumber = _principal.RegistrationNumber;
                    principal.firstName = prinicipaldetails.FirstName;
                    principal.lastName = prinicipaldetails.MiddleName;
                    principal.surname = prinicipaldetails.LastName;
                    var scm_prncipal = db.jntuh_scmupload.Where(d => d.RegistrationNumber == prinicipaldetails.RegistrationNumber && d.Designation == "Principal").Select(d => d).OrderByDescending(de => de.CreatedOn).FirstOrDefault();
                    if (scm_prncipal != null)
                    {
                        principal.SCM = scm_prncipal.SCMDocument;
                    }
                    principal.departmentName = db.jntuh_department.Where(d => d.id == prinicipaldetails.DepartmentId).Select(d => d.departmentName).SingleOrDefault();
                    if (facultyexperiance != null && facultyexperiance.facultyDateOfAppointment != null)
                    {
                        DateTime date = Convert.ToDateTime(facultyexperiance.facultyDateOfAppointment);
                        principal.dateOfAppointment = date.ToString("dd/MM/yyyy").Split(' ')[0];
                    }
                    else
                    {
                        principal.dateOfAppointment = prinicipaldetails.DateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfAppointment.ToString()).ToString();
                    }
                    principal.dateOfBirth = prinicipaldetails.DateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(prinicipaldetails.DateOfBirth.ToString()).ToString();
                    principal.fax = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.fax).FirstOrDefault();
                    principal.landline = db.jntuh_address.Where(c => c.collegeId == userCollegeID).Select(c => c.landline).FirstOrDefault();
                    principal.mobile = prinicipaldetails.Mobile;
                    principal.email = prinicipaldetails.Email;
                    principal.PrincipalPhoto = prinicipaldetails.Photo;
                }
            }

            jntuh_college_principal_director _director = db.jntuh_college_principal_director.Where(a => a.collegeId == userCollegeID && a.type == "DIRECTOR").Select(d => d).FirstOrDefault();
            if (_director != null)
            {
                director.id = _director.id;
                director.firstName = _director.firstName;
                director.lastName = _director.lastName;
                director.surname = _director.surname;
                director.qualificationId = _director.qualificationId;
                //director.qualification = db.jntuh_qualification.Where(s => s.id == _director.qualificationId).Select(a => a.qualification).FirstOrDefault();
                director.dateOfAppointment = _director.dateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfAppointment.ToString()).ToString();
                director.dateOfBirth = _director.dateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfBirth.ToString()).ToString();
                director.fax = _director.fax;
                director.landline = _director.landline;
                director.mobile = _director.mobile;
                director.email = _director.email;
                director.DirectorPhoto = _director.photo;
                director.phdId = _director.phdId;
                director.phd = db.jntuh_phd_subject.Where(q => q.id == _director.phdId).Select(a => a.phdSubjectName).FirstOrDefault();
                director.phdFromUniversity = _director.phdFromUniversity;
                director.phdYear = Convert.ToInt32(_director.phdYear);
                director.departmentId = _director.departmentId;
                director.department = departments.Where(d => d.id == _director.departmentId).Select(z => z.departmentName).FirstOrDefault();
            }

            List<int> collegespecs = new List<int>();

            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();

            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);
            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);
                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = deptment.departmentDescription, DepartmentId = specid.departmentId });
                }
            }
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
            return View("View", model: new Tuple<PrincipalDirector, CollgeDirector>(principal, director));
        }

        //public ActionResult UserView(string id)
        //{
        //    int userCollegeID = userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
        //    int principalID = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type.Equals("PRINCIPAL")).Select(e => e.id).FirstOrDefault();
        //    int directorID = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();
        //    PrincipalDirector principal = new PrincipalDirector();
        //    CollgeDirector director = new CollgeDirector();
        //    jntuh_college_principal_registered _principal = db.jntuh_college_principal_registered.AsNoTracking().Where(p => p.collegeId == userCollegeID).Select(p => p).FirstOrDefault();
        //    if (_principal != null)
        //    {
        //        principal.id = _principal.id;
        //        principal.collegeId = _principal.collegeId;
        //        principal.RegistrationNumber = _principal.RegistrationNumber;

        //    }
        //    jntuh_college_principal_director _director = db.jntuh_college_principal_director.Find(directorID);
        //    if (_director != null)
        //    {
        //        director.id = _director.id;
        //        director.firstName = _director.firstName;
        //        director.lastName = _director.lastName;
        //        director.surname = _director.surname;
        //        director.qualificationId = _director.qualificationId;
        //        director.dateOfAppointment = _director.dateOfAppointment.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfAppointment.ToString()).ToString();
        //        director.dateOfBirth = _director.dateOfBirth.ToString() == null ? string.Empty : Utilities.MMDDYY2DDMMYY(_director.dateOfBirth.ToString()).ToString();
        //        director.fax = _director.fax;
        //        director.landline = _director.landline;
        //        director.mobile = _director.mobile;
        //        director.email = _director.email;
        //        director.DirectorPhoto = _director.photo;

        //        director.phdId = _director.phdId;
        //        director.phdFromUniversity = _director.phdFromUniversity;
        //        director.phdYear = Convert.ToInt32(_director.phdYear);
        //        director.departmentId = _director.departmentId;
        //    }
        //    ViewBag.Phdsubject = db.jntuh_phd_subject.Where(p => p.id == principal.phdId).Select(p => p.phdSubjectName).FirstOrDefault();
        //    ViewBag.DepartmentName = db.jntuh_department.Where(d => d.id == principal.departmentId).Select(d => d.departmentName).FirstOrDefault();
        //    ViewBag.directorPhdsubject = db.jntuh_phd_subject.Where(p => p.id == director.phdId).Select(p => p.phdSubjectName).FirstOrDefault();
        //    ViewBag.directorDepartmentName = db.jntuh_department.Where(d => d.id == director.departmentId).Select(d => d.departmentName).FirstOrDefault();
        //    if (_principal == null && _director == null)
        //    {
        //        ViewBag.NoRecords = true;
        //    }
        //    return View("UserView", model: new Tuple<PrincipalDirector, CollgeDirector>(principal, director));
        //}

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PrincipalVerfication()
        {
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            //   #region Teaching Faculty Logic Wrong Comare to College Teaching faculty Commented By Srinivas
            //   ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();

            var jntuh_department = db.jntuh_department.ToList();

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();


            List<jntuh_college_principal_registered> jntuh_college_principal_registered = db.jntuh_college_principal_registered.Join(db.jntuh_college_edit_status, p => p.collegeId, c => c.collegeId, (p, c) => new { p = p, c = c }).Where(e => e.c.IsCollegeEditable == false && e.c.academicyearId == prAy).Select(e => e.p).ToList();
            string[] strRegNoS = jntuh_college_principal_registered.Select(i => i.RegistrationNumber).ToArray();


            List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.isActive != false).ToList();
            int colid = 0;
            foreach (var a in jntuh_registered_faculty)
            {
                string Reason = String.Empty;
                var faculty = new FacultyRegistration();
                faculty.id = a.id;
                faculty.Type = a.type;
                faculty.RegistrationNumber = a.RegistrationNumber;
                faculty.CollegeId = colid = jntuh_college_principal_registered.Where(i => i.RegistrationNumber.TrimEnd() == a.RegistrationNumber).Select(i => i.collegeId).FirstOrDefault();
                faculty.CollegeName = jntuh_college.Where(i => i.id == colid).Select(i => i.collegeName).FirstOrDefault();
                faculty.UniqueID = a.UniqueID;
                faculty.FirstName = a.FirstName;
                faculty.MiddleName = a.MiddleName;
                faculty.LastName = a.LastName;
                faculty.GenderId = a.GenderId;
                faculty.Email = a.Email;
                faculty.facultyPhoto = a.Photo;
                faculty.Mobile = a.Mobile;
                faculty.PANNumber = a.PANNumber;
                faculty.AadhaarNumber = a.AadhaarNumber;
                faculty.isActive = a.isActive;
                faculty.isApproved = a.isApproved;
                faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                faculty.DeactivationReason = a.DeactivationReason;
                faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                faculty.updatedOn = a.updatedOn;
                faculty.createdOn = jntuh_college_principal_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                faculty.Absent = a.Absent ?? false;
                faculty.ModifiedPANNo = a.ModifiedPANNumber;
                faculty.InvalidPANNo = a.InvalidPANNumber != null && (bool)a.InvalidPANNumber;
                faculty.NORelevantPG = a.NoRelevantPG;
                faculty.NORelevantUG = a.NoRelevantUG;
                faculty.NORelevantPHD = a.NORelevantPHD;
                faculty.NoSCM = a.NoSCM != null && (bool)a.NoSCM;
                faculty.NOForm16 = a.NoForm16 != null && (bool)a.NoForm16;
                faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null && (bool)a.NotQualifiedAsperAICTE;
                faculty.InCompleteCeritificates = a.IncompleteCertificates != null && (bool)a.IncompleteCertificates;
                faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                //faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege ?? false;
                faculty.NoSCM17Flag = a.NoSCM17 ?? false;
                faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates ?? false;
                faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram ?? false;
                faculty.Basstatus = a.BAS;
                faculty.InvalidAadhaar = a.InvalidAadhaar;
                faculty.DeactivationReason = a.DeactivationReason;
                faculty.SCMDocumentView =
                    db.jntuh_scmupload.Where(
                        p => p.RegistrationNumber.Trim() == faculty.RegistrationNumber && p.CollegeId == faculty.CollegeId && p.DepartmentId == 0 && p.SpecializationId == 0)
                        .Select(s => s.SCMDocument)
                        .FirstOrDefault();
                faculty.PHDView = db.jntuh_faculty_phddetails.Where(i => i.Facultyid == faculty.id).Count();
                var appealprincipal =
                    db.jntuh_appeal_principal_registered.Where(
                        p => p.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim()).FirstOrDefault();
                //if (appealprincipal != null)
                //{
                //    if (appealprincipal.NOtificationReport != null)
                //    {
                //        faculty.AppealSCMDocsumentView = appealprincipal.SelectionCommiteMinutes;
                //        faculty.SCMDocumentView = faculty.AppealSCMDocumentView;
                //        faculty.appealPrincipalSupportdocument = appealprincipal.PHDUndertakingDocument;
                //    }
                //    else
                //    {
                //        faculty.appealPrincipalSupportdocument = appealprincipal.PHDUndertakingDocument;
                //    }
                //}



                //if (faculty.Absent == true)
                //{
                //    Reason = "Absent" + ",";
                //}

                //if (faculty.NOTQualifiedAsPerAICTE == true)
                //{
                //    Reason += "Not Qualified as AICTE" + ",";
                //}
                //if (faculty.FalsePAN == true)
                //{
                //    Reason += "False PAN" + ",";
                //}
                //if (faculty.InCompleteCeritificates == true)
                //{
                //    Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
                //}
                //if (faculty.InvalidPANNo == true)
                //{
                //    Reason += "No PAN" + ",";
                //}
                //if (faculty.NoSCM == true)
                //{
                //    Reason += "No SCM/Ratification" + ",";
                //}

                //if (Reason != "")
                //{
                //    Reason = Reason.Substring(0, Reason.Length - 1);
                //}


                //faculty.DeactivationNew = Reason;
                teachingFaculty.Add(faculty);
            }
            #region Appeal Principal Coding comentby Narayana Reddy on 29-03-2019
            //List<jntuh_appeal_principal_registered> pjntuh_college_principal_registered = db.jntuh_appeal_principal_registered.Join(db.jntuh_college_edit_status, p => p.collegeId, c => c.collegeId, (p, c) => new { p = p, c = c }).Where(e => e.c.IsCollegeEditable == false && e.p.NOtificationReport != null).Select(e => e.p).ToList();
            //string[] pstrRegNoS = pjntuh_college_principal_registered.Select(i => i.RegistrationNumber).ToArray();


            //List<jntuh_registered_faculty> pjntuh_registered_faculty = new List<jntuh_registered_faculty>();
            //pjntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => pstrRegNoS.Contains(rf.RegistrationNumber) && rf.Blacklistfaculy != true && rf.isActive != false).ToList();

            //foreach (var a in pjntuh_registered_faculty)
            //{
            //    string Reason = String.Empty;
            //    var faculty = new FacultyRegistration();
            //    faculty.id = a.id;
            //    faculty.Type = a.type;
            //    faculty.RegistrationNumber = a.RegistrationNumber;
            //    faculty.CollegeId = colid = pjntuh_college_principal_registered.Where(i => i.RegistrationNumber.TrimEnd() == a.RegistrationNumber).Select(i => i.collegeId).FirstOrDefault();
            //    faculty.CollegeName = jntuh_college.Where(i => i.id == colid).Select(i => i.collegeName).FirstOrDefault();
            //    faculty.UniqueID = a.UniqueID;
            //    faculty.FirstName = a.FirstName;
            //    faculty.MiddleName = a.MiddleName;
            //    faculty.LastName = a.LastName;
            //    faculty.GenderId = a.GenderId;
            //    faculty.Email = a.Email;
            //    faculty.facultyPhoto = a.Photo;
            //    faculty.Mobile = a.Mobile;
            //    faculty.PANNumber = a.PANNumber;
            //    faculty.AadhaarNumber = a.AadhaarNumber;
            //    faculty.isActive = a.isActive;
            //    faculty.isApproved = a.isApproved;
            //    faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
            //    faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
            //    faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
            //    faculty.DeactivationReason = a.DeactivationReason;
            //    faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
            //    faculty.updatedOn = a.updatedOn;
            //    faculty.createdOn = pjntuh_college_principal_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
            //    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
            //    faculty.Absent = a.Absent ?? false;
            //    faculty.ModifiedPANNo = a.ModifiedPANNumber;
            //    faculty.InvalidPANNo = a.InvalidPANNumber != null && (bool)a.InvalidPANNumber;
            //    faculty.NORelevantPG = a.NoRelevantPG;
            //    faculty.NORelevantUG = a.NoRelevantUG;
            //    faculty.NORelevantPHD = a.NORelevantPHD;
            //    faculty.NoSCM = a.NoSCM != null && (bool)a.NoSCM;
            //    faculty.NOForm16 = a.NoForm16 != null && (bool)a.NoForm16;
            //    faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null && (bool)a.NotQualifiedAsperAICTE;
            //    faculty.InCompleteCeritificates = a.IncompleteCertificates != null && (bool)a.IncompleteCertificates;
            //    faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
            //    faculty.MultipleReginSamecoll = a.MultipleRegInSameCollege ?? false;
            //    faculty.NoSCM17Flag = a.NoSCM17 ?? false;
            //    faculty.XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates ?? false;
            //    faculty.NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram ?? false;
            //    faculty.Basstatus = a.BASStatus;
            //    faculty.BasstatusOld = a.BASStatusOld;
            //    faculty.DeactivationReason = a.DeactivationReason;
            //    faculty.SCMDocumentView =
            //        db.jntuh_scmupload.Where(
            //            p => p.RegistrationNumber.Trim() == faculty.RegistrationNumber && p.CollegeId == faculty.CollegeId && p.DepartmentId == 0 && p.SpecializationId == 0)
            //            .Select(s => s.SCMDocument)
            //            .FirstOrDefault();
            //    var appealprincipal =
            //        db.jntuh_appeal_principal_registered.Where(
            //            p => p.RegistrationNumber.Trim() == faculty.RegistrationNumber.Trim()).FirstOrDefault();
            //    if (appealprincipal != null)
            //    {
            //        if (appealprincipal.NOtificationReport != null)
            //        {
            //            faculty.AppealSCMDocumentView = appealprincipal.SelectionCommiteMinutes;
            //            faculty.SCMDocumentView = faculty.AppealSCMDocumentView;
            //            faculty.appealPrincipalSupportdocument = appealprincipal.PHDUndertakingDocument;
            //        }
            //        else
            //        {
            //            faculty.appealPrincipalSupportdocument = appealprincipal.PHDUndertakingDocument;
            //        }
            //    }
            //    teachingFaculty.Add(faculty);
            //}
            #endregion
            //var data = jntuh_registered_faculty.Select(a =>
            //{
            //    return new FacultyRegistration
            //    {
            //        id = a.id,
            //        Type = a.type,
            //        RegistrationNumber = a.RegistrationNumber,
            //        CollegeId =colid =jntuh_college_principal_registered.Where(i => i.RegistrationNumber == a.RegistrationNumber).Select(i => i.collegeId).FirstOrDefault(),
            //        CollegeName =jntuh_college.Where(i => i.id == colid).Select(i => i.collegeName).FirstOrDefault(),
            //        UniqueID = a.UniqueID,
            //        FirstName = a.FirstName,
            //        MiddleName = a.MiddleName,
            //        LastName = a.LastName,
            //        GenderId = a.GenderId,
            //        Email = a.Email,
            //        facultyPhoto = a.Photo,
            //        Mobile = a.Mobile,
            //        PANNumber = a.PANNumber,
            //        AadhaarNumber = a.AadhaarNumber,
            //        isActive = a.isActive,
            //        isApproved = a.isApproved,
            //        department =jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault(),
            //        SamePANNumberCount =jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count(),
            //        SameAadhaarNumberCount =jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count(),
            //        DeactivationReason = a.DeactivationReason,
            //        FacultyVerificationStatus = a.FacultyVerificationStatus,
            //        updatedOn = a.updatedOn,
            //        createdOn =jntuh_college_principal_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault(),
            //        jntuh_registered_faculty_education = a.jntuh_registered_faculty_education,
            //        Absent = a.Absent != null && (bool) a.Absent,
            //        ModifiedPANNo = a.ModifiedPANNumber,
            //        InvalidPANNo = a.InvalidPANNumber != null && (bool)a.InvalidPANNumber,
            //        NORelevantPG = a.NoRelevantPG,
            //        NoSCM = a.NoSCM!=null && (bool)a.NoSCM,
            //        NOForm16 = a.NoForm16 != null && (bool)a.NoForm16,
            //        NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null && (bool)a.NotQualifiedAsperAICTE,
            //        InCompleteCeritificates = a.IncompleteCertificates != null && (bool)a.IncompleteCertificates
            //    };
            //}).ToList();

            //teachingFaculty.AddRange(data);
            teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
            return View(teachingFaculty);
        }

        public class GetFacultyBASDetails
        {
            public string RegistarationNumber { get; set; }
            public string BasJoiningDate { get; set; }
            public int? JulyTotalDays { get; set; }
            public int? AugustTotalDays { get; set; }
            public int? SeptemberTotalDays { get; set; }
            public int? OctoberTotalDays { get; set; }
            public int? NovemberTotalDays { get; set; }
            public int? DecemberTotalDays { get; set; }
            public int? JanuaryTotalDays { get; set; }
            public int? FebruaryTotalDays { get; set; }
            public int? MarchTotalDays { get; set; }
            public int? AprilTotalDays { get; set; }
            public int? JulyPresentDays { get; set; }
            public int? AugustPresentDays { get; set; }
            public int? SeptemberPresentDays { get; set; }
            public int? OctoberPresentDays { get; set; }
            public int? NovemberPresentDays { get; set; }
            public int? DecemberPresentDays { get; set; }
            public int? JanuaryPresentDays { get; set; }
            public int? FebruaryPresentDays { get; set; }
            public int? MarchPresentDays { get; set; }
            public int? AprilPresentDays { get; set; }
            public int? TotalWorkingDays { get; set; }
            public int? TotalPresentDays { get; set; }
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult GetFacultyBASDetailsView(string RegistarationNumber)
        {
            int lastYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int presentyear = db.jntuh_academic_year.Where(a => a.actualYear == (lastYear + 1)).Select(a => a.actualYear).FirstOrDefault();
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeID = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == RegistarationNumber).Select(e => e.collegeId).FirstOrDefault();
            if (RegistarationNumber != null)
            {
                var FacultyBASData = db.jntuh_college_basreport.Where(e => e.RegistrationNumber == RegistarationNumber && (e.year == lastYear || e.year == presentyear) && e.isActive == true).Select(e => e).ToList();
                if (FacultyBASData.Count() != 0 && FacultyBASData != null)
                {
                    GetFacultyBASDetails Faculty = new GetFacultyBASDetails();
                    Faculty.RegistarationNumber = RegistarationNumber;
                    string date = FacultyBASData.Select(e => e.joiningDate).LastOrDefault().ToString();

                    Faculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    Faculty.TotalWorkingDays = FacultyBASData.Select(e => e.totalworkingDays).Sum();
                    Faculty.TotalPresentDays = FacultyBASData.Select(e => e.NoofPresentDays).Sum();

                    foreach (var item in FacultyBASData)
                    {
                        if (item.month == "July" && item.year == lastYear)
                        {
                            Faculty.JulyTotalDays = item.totalworkingDays;
                            Faculty.JulyPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "August" && item.year == lastYear)
                        {
                            Faculty.AugustTotalDays = item.totalworkingDays;
                            Faculty.AugustPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "September" && item.year == lastYear)
                        {
                            Faculty.SeptemberTotalDays = item.totalworkingDays;
                            Faculty.SeptemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "October" && item.year == lastYear)
                        {
                            Faculty.OctoberTotalDays = item.totalworkingDays;
                            Faculty.OctoberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "November" && item.year == lastYear)
                        {
                            Faculty.NovemberTotalDays = item.totalworkingDays;
                            Faculty.NovemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "December" && item.year == lastYear)
                        {
                            Faculty.DecemberTotalDays = item.totalworkingDays;
                            Faculty.DecemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "January" && item.year == presentyear)
                        {
                            Faculty.JanuaryTotalDays = item.totalworkingDays;
                            Faculty.JanuaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "February" && item.year == presentyear)
                        {
                            Faculty.FebruaryTotalDays = item.totalworkingDays;
                            Faculty.FebruaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "March" && item.year == presentyear)
                        {
                            Faculty.MarchTotalDays = item.totalworkingDays;
                            Faculty.MarchPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "April" && item.year == presentyear)
                        {
                            Faculty.AprilTotalDays = item.totalworkingDays;
                            Faculty.AprilPresentDays = item.NoofPresentDays;
                        }
                    }


                    return PartialView("~/Views/PrincipalDirector/_GetFacultyBASDetails.cshtml", Faculty);
                }
                else
                {
                    return RedirectToAction("PrincipalVerfication");
                }
            }
            else
            {
                return RedirectToAction("PrincipalVerfication");
            }
            // return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PrincipalVerificationDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                //regFaculty.GenderId = null;
                //regFaculty.isFacultyRatifiedByJNTU = null;
                //fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                //jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                //regFaculty.id = fID;
                //regFaculty.Type = faculty.type;
                //regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                //regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                //regFaculty.Email = faculty.Email;
                //regFaculty.UniqueID = faculty.UniqueID;
                //regFaculty.FirstName = faculty.FirstName;
                //regFaculty.MiddleName = faculty.MiddleName;
                //regFaculty.LastName = faculty.LastName;
                //regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                //regFaculty.MotherName = faculty.MotherName;
                //regFaculty.GenderId = faculty.GenderId;
                //if (faculty.DateOfBirth != null)
                //{
                //    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                //}
                //regFaculty.Mobile = faculty.Mobile;
                //regFaculty.facultyPhoto = faculty.Photo;
                //regFaculty.PANNumber = faculty.PANNumber;
                //regFaculty.facultyPANCardDocument = faculty.PANDocument;
                //regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                //regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                //regFaculty.WorkingStatus = faculty.WorkingStatus;
                //regFaculty.TotalExperience = faculty.TotalExperience;
                //regFaculty.OrganizationName = faculty.OrganizationName;
                //if (faculty.collegeId != null)
                //{
                //    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                //}
                //regFaculty.CollegeId = faculty.collegeId;
                //if (faculty.DepartmentId != null)
                //{
                //    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                //}
                //regFaculty.DepartmentId = faculty.DepartmentId;
                //regFaculty.OtherDepartment = faculty.OtherDepartment;

                //if (faculty.DesignationId != null)
                //{
                //    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                //}
                //regFaculty.DesignationId = faculty.DesignationId;
                //regFaculty.OtherDesignation = faculty.OtherDesignation;

                //if (faculty.DateOfAppointment != null)
                //{
                //    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                //}
                //regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                //regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                //if (faculty.DateOfRatification != null)
                //{
                //    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                //}
                //regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                //regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                //regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                //regFaculty.GrossSalary = faculty.grosssalary;
                //regFaculty.National = faculty.National;
                //regFaculty.InterNational = faculty.InterNational;
                //regFaculty.Citation = faculty.Citation;
                //regFaculty.Awards = faculty.Awards;
                //regFaculty.isActive = faculty.isActive;
                //regFaculty.isApproved = faculty.isApproved;
                //regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                //regFaculty.isView = true;
                //regFaculty.DeactivationReason = faculty.DeactivationReason;
                //regFaculty.FacultyVerificationStatus = faculty.FacultyVerificationStatus;

                //regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                //                                            .Select(e => new RegisteredFacultyEducation
                //                                            {
                //                                                educationId = e.id,
                //                                                educationName = e.educationCategoryName,
                //                                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                //                                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                //                                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                //                                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                //                                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                //                                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                //                                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                //                                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                //                                            }).ToList();
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName =
                    db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                regFaculty.CollegeId =
                    db.jntuh_college_faculty_registered.Where(
                        f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                        .Select(s => s.collegeId)
                        .FirstOrDefault();
                if (regFaculty.CollegeId == 0 || regFaculty.CollegeId == null)
                {
                    regFaculty.CollegeId =
                              db.jntuh_college_principal_registered.Where(
                                  f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                                  .Select(s => s.collegeId)
                                  .FirstOrDefault();
                }
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth =
                        UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                }
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.OrganizationName = faculty.OrganizationName;
                if (regFaculty.CollegeId != 0)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeCode + " - " + db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                }
                //regFaculty.CollegeId = faculty.collegeId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.OtherDepartment = faculty.OtherDepartment;

                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.DateOfAppointment != null)
                {
                    regFaculty.facultyDateOfAppointment =
                        UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification =
                        UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                }
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                regFaculty.DeactivedOn = faculty.DeactivatedOn;
                regFaculty.Deactivedby = faculty.DeactivatedBy;
                DateTime verificationstartdate = new DateTime(2019, 03, 25);
                ViewBag.Isdone = true;
                if (verificationstartdate > regFaculty.DeactivedOn || regFaculty.DeactivedOn == null)
                {
                    ViewBag.Isdone = false;
                }
                #region Faculty Education Data Getting

                // var jntuh_education_category = db.jntuh_education_category.Where(e => e.isActive == true).ToList();
                var registeredFacultyEducation = db.jntuh_registered_faculty_education.Where(e => e.facultyId == fID).ToList();

                if (registeredFacultyEducation.Count != 0)
                {
                    foreach (var item in registeredFacultyEducation)
                    {
                        if (item.educationId == 1)
                        {
                            regFaculty.SSC_educationId = 1;
                            regFaculty.SSC_studiedEducation = item.courseStudied;
                            regFaculty.SSC_specialization = item.specialization;
                            regFaculty.SSC_passedYear = item.passedYear;
                            regFaculty.SSC_percentage = item.marksPercentage;
                            regFaculty.SSC_division = item.division == null ? 0 : item.division;
                            regFaculty.SSC_university = item.boardOrUniversity;
                            regFaculty.SSC_place = item.placeOfEducation;
                            regFaculty.SSC_facultyCertificate = item.certificate;
                        }
                        else if (item.educationId == 3)
                        {
                            regFaculty.UG_educationId = 3;
                            regFaculty.UG_studiedEducation = item.courseStudied;
                            regFaculty.UG_specialization = item.specialization;
                            regFaculty.UG_passedYear = item.passedYear;
                            regFaculty.UG_percentage = item.marksPercentage;
                            regFaculty.UG_division = item.division == null ? 0 : item.division;
                            regFaculty.UG_university = item.boardOrUniversity;
                            regFaculty.UG_place = item.placeOfEducation;
                            regFaculty.UG_facultyCertificate = item.certificate;
                        }
                        else if (item.educationId == 4)
                        {
                            regFaculty.PG_educationId = 4;
                            regFaculty.PG_studiedEducation = item.courseStudied;
                            regFaculty.PG_specialization = item.specialization;
                            regFaculty.PG_passedYear = item.passedYear;
                            regFaculty.PG_percentage = item.marksPercentage;
                            regFaculty.PG_division = item.division == null ? 0 : item.division;
                            regFaculty.PG_university = item.boardOrUniversity;
                            regFaculty.PG_place = item.placeOfEducation;
                            regFaculty.PG_facultyCertificate = item.certificate;
                        }
                        else if (item.educationId == 5)
                        {
                            regFaculty.MPhil_educationId = 5;
                            regFaculty.MPhil_studiedEducation = item.courseStudied;
                            regFaculty.MPhil_specialization = item.specialization;
                            regFaculty.MPhil_passedYear = item.passedYear;
                            regFaculty.MPhil_percentage = item.marksPercentage;
                            regFaculty.MPhil_division = item.division == null ? 0 : item.division;
                            regFaculty.MPhil_university = item.boardOrUniversity;
                            regFaculty.MPhil_place = item.placeOfEducation;
                            regFaculty.MPhil_facultyCertificate = item.certificate;
                        }
                        else if (item.educationId == 6)
                        {
                            regFaculty.PhD_educationId = 6;
                            regFaculty.PhD_studiedEducation = item.courseStudied;
                            regFaculty.PhD_specialization = item.specialization;
                            regFaculty.PhD_passedYear = item.passedYear;
                            regFaculty.PhD_percentage = item.marksPercentage;
                            regFaculty.PhD_division = item.division == null ? 0 : item.division;
                            regFaculty.PhD_university = item.boardOrUniversity;
                            regFaculty.PhD_place = item.placeOfEducation;
                            regFaculty.PhD_facultyCertificate = item.certificate;
                        }
                        else if (item.educationId == 8)
                        {
                            regFaculty.Others_educationId = 8;
                            regFaculty.faculty_AllCertificates = item.certificate;
                        }
                    }
                }
                #endregion

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }

            return View(regFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult OldNotApprovedInformation(string fid, string Command, bool pan, string collegeid)
        {

            var notapproved = db.jntuh_faculty_deactivation_reason.ToList();
            ViewBag.notapproved = notapproved;
            TempData["collegeid"] = collegeid;
            TempData["fid"] = fid;
            return PartialView("OldNotApprovedInformation", notapproved);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult Approve1(string fid, int? collegeId, bool pan, string[] remarks, string others)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
            {
                int fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));

                jntuh_registered_faculty jntuh_registered_faculty = db.jntuh_registered_faculty.Find(fID);
                jntuh_registered_faculty.updatedBy = userId;
                jntuh_registered_faculty.updatedOn = DateTime.Now;
                jntuh_registered_faculty.DeactivatedBy = userId;
                jntuh_registered_faculty.DeactivatedOn = DateTime.Now;
                var regno = jntuh_registered_faculty.RegistrationNumber;
                var principaldetails =
                    db.jntuh_college_principal_registered.Where(i => i.RegistrationNumber == regno)
                        .Select(i => i)
                        .FirstOrDefault();
                if (remarks != null)
                {
                    jntuh_registered_faculty.DeactivationReason = string.Join(",", remarks) + (!string.IsNullOrEmpty(others) ? "," + others : null);
                }
                else
                {
                    jntuh_registered_faculty.DeactivationReason = others;
                }


                db.Entry(jntuh_registered_faculty).State = EntityState.Modified;
                db.SaveChanges();

                if (principaldetails != null)
                {
                    principaldetails.isActive = true;
                    db.SaveChanges();
                }
            }

            return RedirectToAction("PrincipalVerfication");
        }




        #region Principal Verification Methods

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult PrincipalVerificationEdit(string fid, string collegeid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                ViewBag.collegeid = collegeid;
                ViewBag.fid = fid;
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                }
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.OrganizationName = faculty.OrganizationName;
                regFaculty.Absent = faculty.Absent ?? false;
                regFaculty.NOTQualifiedAsPerAICTE = faculty.NotQualifiedAsperAICTE ?? false;
                regFaculty.OriginalCertificatesnotshownFlag = faculty.OriginalCertificatesNotShown ?? false;
                regFaculty.InCompleteCeritificates = faculty.IncompleteCertificates ?? false;
                //regFaculty.MultipleReginSamecoll = faculty.MultipleRegInSameCollege ?? false;
                regFaculty.XeroxcopyofcertificatesFlag = faculty.Xeroxcopyofcertificates ?? false;
                regFaculty.NOForm16 = faculty.NoForm16 ?? false;
                regFaculty.NoSCM17Flag = faculty.NoSCM17 ?? false;
                regFaculty.NotIdentityFiedForAnyProgramFlag = faculty.NotIdentityfiedForanyProgram ?? false;
                regFaculty.NORelevantUG = faculty.NoRelevantUG;
                regFaculty.NORelevantPG = faculty.NoRelevantPG;
                regFaculty.NORelevantPHD = faculty.NORelevantPHD;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                if (faculty.NoRelevantUG != "No")
                {
                    regFaculty.NOrelevantUgFlag = true;
                }
                if (faculty.NoRelevantPG != "No")
                {
                    regFaculty.NOrelevantPgFlag = true;
                }
                if (faculty.NORelevantPHD != "No")
                {
                    regFaculty.NOrelevantPhdFlag = true;
                }



                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.OtherDepartment = faculty.OtherDepartment;

                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.DateOfAppointment != null)
                {
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                }
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;


                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                                                            .Select(e => new RegisteredFacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                                                            }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var departments = db.jntuh_department.ToList();
                var specializatons = db.jntuh_specialization.ToList();
                int[] ugids = departments.Where(i => i.degreeId == 4 || i.degreeId == 5).Select(i => i.id).ToArray();
                int[] pgids = departments.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
                List<DistinctDepartment> depts = new List<DistinctDepartment>();
                string existingDepts = string.Empty;
                int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
                foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
                {
                    if (!existingDepts.Split(',').Contains(item.departmentName))
                    {
                        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                        existingDepts = existingDepts + "," + item.departmentName;
                    }
                }

                ViewBag.department = depts;

                var ugcoures = specializatons.Where(i => ugids.Contains(i.departmentId)).ToList();
                var pgcoures = specializatons.Where(i => pgids.Contains(i.departmentId)).ToList();
                //var phdcoures = db.jntuh_specialization.Where(i => phdids.Contains(i.departmentId)).Select(i => i.specializationName).ToList();

                ViewBag.ugcourses = ugcoures;
                ViewBag.pgcourses = pgcoures;
                //ViewBag.phdcourses = phdcoures;
                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }
            return PartialView("_PrincipleVerificationEdit", regFaculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult PrincipalVerificationEditPost(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == faculty.RegistrationNumber);
            if (facultydetails != null)
            {
                if (faculty.Absent == true)
                    facultydetails.Absent = faculty.Absent;

                //if (faculty.NOTQualifiedAsPerAICTE == true)
                //facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;

                //if (faculty.InCompleteCeritificates == true)
                //    facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;

                //if (faculty.MultipleReginSamecoll == true)
                //    facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;

                //if (faculty.XeroxcopyofcertificatesFlag == true)
                //    facultydetails.Xeroxcopyofcertificates = faculty.XeroxcopyofcertificatesFlag;

                //if (faculty.NOForm16 == true)
                //    facultydetails.NoForm16 = faculty.NOForm16;

                //if (faculty.NotIdentityFiedForAnyProgramFlag == true)
                //    facultydetails.NotIdentityfiedForanyProgram = faculty.NotIdentityFiedForAnyProgramFlag;

                //if (faculty.NOrelevantUgFlag == true)
                //    facultydetails.NoRelevantUG ="Yes";

                //if (faculty.NOrelevantPgFlag == true)
                //    facultydetails.NoRelevantPG = "Yes";

                //if (faculty.NOrelevantPhdFlag == true)
                //    facultydetails.NORelevantPHD = "Yes";

                //if (faculty.NoSCM17Flag == true)
                //    facultydetails.NoSCM17 = faculty.NoSCM17Flag;
                if (!string.IsNullOrEmpty(faculty.DeactivationReason))
                    facultydetails.DeactivationReason = faculty.DeactivationReason;
                facultydetails.FacultyVerificationStatus = true;
                facultydetails.isApproved = false;
                //facultydetails.DeactivatedBy = userID;
                //facultydetails.DeactivatedOn = DateTime.Now;
                db.Entry(facultydetails).State = EntityState.Modified;

                db.SaveChanges();
            }
            TempData["Success"] = "Principal Data Update Successfully..";
            return RedirectToAction("PrincipalVerfication", "PrincipalDirector");
            // return RedirectToAction("PrincipalVerificationDetails", "PrincipalDirector", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"])});
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PrincipalVerificationApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                //facultydetails.Absent = false;
                //facultydetails.InvalidPANNumber = false;
                //facultydetails.NoRelevantUG = null;
                //facultydetails.NoRelevantPG = null;
                //facultydetails.NORelevantPHD = null;
                //facultydetails.NoSCM = false;
                //facultydetails.NoForm16 = false;
                //facultydetails.NotQualifiedAsperAICTE = false;
                //facultydetails.MultipleRegInSameCollege = false;
                //facultydetails.MultipleRegInDiffCollege = false;
                //facultydetails.SamePANUsedByMultipleFaculty = false;
                //facultydetails.PhotoCopyofPAN = false;
                //facultydetails.AppliedPAN = false;
                //facultydetails.LostPAN = false;
                //facultydetails.OriginalsVerifiedUG = false;
                //facultydetails.OriginalsVerifiedPG = false;
                //facultydetails.OriginalsVerifiedPHD = false;
                //facultydetails.FacultyVerificationStatus = true
                //facultydetails.IncompleteCertificates = false;
                facultydetails.DeactivationReason = null;
                facultydetails.isApproved = true;
                facultydetails.Absent = false;
                //facultydetails.DeactivatedBy = userID;
                //facultydetails.DeactivatedOn = DateTime.Now;
                db.Entry(facultydetails).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Principal Data Successfully Approved..";
            }
            return RedirectToAction("PrincipalVerfication", "PrincipalDirector");
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PrincipalVerificationReactivate(string fid, string collegeid)
        {
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.Absent = false;
                facultydetails.InvalidPANNumber = false;
                facultydetails.NoRelevantUG = null;
                facultydetails.NoRelevantPG = null;
                facultydetails.NORelevantPHD = null;
                facultydetails.NoSCM = false;
                facultydetails.NoForm16 = false;
                facultydetails.NotQualifiedAsperAICTE = false;
                //facultydetails.MultipleRegInSameCollege = false;
                //facultydetails.MultipleRegInDiffCollege = false;
                //facultydetails.SamePANUsedByMultipleFaculty = false;
                //facultydetails.PhotoCopyofPAN = false;
                //facultydetails.AppliedPAN = false;
                //facultydetails.LostPAN = false;
                facultydetails.OriginalsVerifiedUG = false;
                facultydetails.OriginalsVerifiedPG = false;
                facultydetails.OriginalsVerifiedPHD = false;
                facultydetails.FacultyVerificationStatus = false;
                facultydetails.IncompleteCertificates = false;
                db.SaveChanges();
                TempData["Success"] = "Principal Data Successfully Reactivated..";
            }
            return RedirectToAction("PrincipalVerfication", "PrincipalDirector");
        }
        #endregion

        #region Minority Principal Verification

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult MinorityPrincipalVerfication()
        {

            var jntuh_college = db.jntuh_college.ToList();
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
            var CollegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToArray();
            List<jntuh_college_principal_registered> jntuh_college_principal_registered = db.jntuh_college_principal_registered.Join(db.jntuh_college, p => p.collegeId, c => c.id, (p, c) => new { p = p, c = c }).Where(e => e.c.collegeStatusID == 1 && e.c.isActive == true && CollegeIds.Contains(e.c.id)).Select(e => e.p).ToList();
            string[] strRegNoS = jntuh_college_principal_registered.Select(i => i.RegistrationNumber).ToArray();
            List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => strRegNoS.Contains(rf.RegistrationNumber))  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                         .ToList();
            int colid = 0;
            foreach (var a in jntuh_registered_faculty)
            {
                string Reason = String.Empty;
                var faculty = new FacultyRegistration();
                faculty.id = a.id;
                faculty.Type = a.type;
                faculty.RegistrationNumber = a.RegistrationNumber;
                faculty.CollegeId = colid = jntuh_college_principal_registered.Where(i => i.RegistrationNumber.TrimEnd() == a.RegistrationNumber).Select(i => i.collegeId).FirstOrDefault();
                faculty.CollegeName = jntuh_college.Where(i => i.id == colid).Select(i => i.collegeName).FirstOrDefault();
                faculty.UniqueID = a.UniqueID;
                faculty.FirstName = a.FirstName;
                faculty.MiddleName = a.MiddleName;
                faculty.LastName = a.LastName;
                faculty.GenderId = a.GenderId;
                faculty.Email = a.Email;
                faculty.facultyPhoto = a.Photo;
                faculty.Mobile = a.Mobile;
                faculty.PANNumber = a.PANNumber;
                faculty.AadhaarNumber = a.AadhaarNumber;
                faculty.isActive = a.isActive;
                faculty.isApproved = a.isApproved;
                faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                faculty.DeactivationReason = a.DeactivationReason;
                faculty.FacultyVerificationStatus = a.FacultyVerificationStatus;
                faculty.updatedOn = a.updatedOn;
                faculty.createdOn = jntuh_college_principal_registered.Where(f => f.RegistrationNumber == a.RegistrationNumber).Select(f => f.createdOn).FirstOrDefault();
                faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                faculty.Absent = a.Absent != null && (bool)a.Absent;
                faculty.ModifiedPANNo = a.ModifiedPANNumber;
                faculty.InvalidPANNo = a.InvalidPANNumber != null && (bool)a.InvalidPANNumber;
                faculty.NORelevantPG = a.NoRelevantPG;
                faculty.NoSCM = a.NoSCM != null && (bool)a.NoSCM;
                faculty.NOForm16 = a.NoForm16 != null && (bool)a.NoForm16;
                faculty.NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null && (bool)a.NotQualifiedAsperAICTE;
                faculty.InCompleteCeritificates = a.IncompleteCertificates != null && (bool)a.IncompleteCertificates;
                faculty.FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false;
                if (faculty.Absent == true)
                {
                    Reason = "Absent" + ",";
                }

                if (faculty.NOTQualifiedAsPerAICTE == true)
                {
                    Reason += "Not Qualified as AICTE" + ",";
                }
                if (faculty.FalsePAN == true)
                {
                    Reason += "False PAN" + ",";
                }
                if (faculty.InCompleteCeritificates == true)
                {
                    Reason += "Incomplete Certificates(UG/PG/PHD/SCM)" + ",";
                }
                if (faculty.InvalidPANNo == true)
                {
                    Reason += "No PAN" + ",";
                }
                if (faculty.NoSCM == true)
                {
                    Reason += "No SCM/Ratification" + ",";
                }

                if (Reason != "")
                {
                    Reason = Reason.Substring(0, Reason.Length - 1);
                }


                faculty.DeactivationNew = Reason;
                teachingFaculty.Add(faculty);
            }

            teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.updatedOn).ThenBy(f => f.department).ToList();
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult MinorityPrincipalVerificationDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                }
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.OrganizationName = faculty.OrganizationName;
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.OtherDepartment = faculty.OtherDepartment;

                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.DateOfAppointment != null)
                {
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                }
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;
                regFaculty.FacultyVerificationStatus = faculty.FacultyVerificationStatus;

                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                                                            .Select(e => new RegisteredFacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                                                            }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);



                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }

            return View(regFaculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult MinorityPrincipalVerificationEdit(string fid, string collegeid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (fid != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyID = fID;
                ViewBag.collegeid = collegeid;
                ViewBag.fid = fid;
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);

                regFaculty.id = fID;
                regFaculty.Type = faculty.type;
                regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                regFaculty.UserName = db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                regFaculty.Email = faculty.Email;
                regFaculty.UniqueID = faculty.UniqueID;
                regFaculty.FirstName = faculty.FirstName;
                regFaculty.MiddleName = faculty.MiddleName;
                regFaculty.LastName = faculty.LastName;
                regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                regFaculty.MotherName = faculty.MotherName;
                regFaculty.GenderId = faculty.GenderId;
                if (faculty.DateOfBirth != null)
                {
                    regFaculty.facultyDateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                }
                regFaculty.Mobile = faculty.Mobile;
                regFaculty.facultyPhoto = faculty.Photo;
                regFaculty.PANNumber = faculty.PANNumber;
                regFaculty.facultyPANCardDocument = faculty.PANDocument;
                regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                regFaculty.WorkingStatus = faculty.WorkingStatus;
                regFaculty.TotalExperience = faculty.TotalExperience;
                regFaculty.OrganizationName = faculty.OrganizationName;
                if (faculty.collegeId != null)
                {
                    regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                }
                regFaculty.CollegeId = faculty.collegeId;
                if (faculty.DepartmentId != null)
                {
                    regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                }
                regFaculty.DepartmentId = faculty.DepartmentId;
                regFaculty.OtherDepartment = faculty.OtherDepartment;

                if (faculty.DesignationId != null)
                {
                    regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                }
                regFaculty.DesignationId = faculty.DesignationId;
                regFaculty.OtherDesignation = faculty.OtherDesignation;

                if (faculty.DateOfAppointment != null)
                {
                    regFaculty.facultyDateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                }
                regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                if (faculty.DateOfRatification != null)
                {
                    regFaculty.facultyDateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                }
                regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                regFaculty.GrossSalary = faculty.grosssalary;
                regFaculty.National = faculty.National;
                regFaculty.InterNational = faculty.InterNational;
                regFaculty.Citation = faculty.Citation;
                regFaculty.Awards = faculty.Awards;
                regFaculty.isActive = faculty.isActive;
                regFaculty.isApproved = faculty.isApproved;
                regFaculty.isView = true;
                regFaculty.DeactivationReason = faculty.DeactivationReason;


                regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                                                            .Select(e => new RegisteredFacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                                                            }).ToList();

                foreach (var item in regFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }

                string registrationNumber = db.jntuh_registered_faculty.Where(of => of.id == fID).Select(of => of.RegistrationNumber).FirstOrDefault();
                int facultyId = db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber).Select(of => of.id).FirstOrDefault();
                //int[] verificationOfficers = db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId).Select(v => v.VerificationOfficer).Distinct().ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var departments = db.jntuh_department.ToList();
                var specializatons = db.jntuh_specialization.ToList();
                int[] ugids = departments.Where(i => i.degreeId == 4 || i.degreeId == 5).Select(i => i.id).ToArray();
                int[] pgids = departments.Where(i => i.degreeId != 4 && i.degreeId != 5).Select(i => i.id).ToArray();
                List<DistinctDepartment> depts = new List<DistinctDepartment>();
                string existingDepts = string.Empty;
                int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56 };
                foreach (var item in db.jntuh_department.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
                {
                    if (!existingDepts.Split(',').Contains(item.departmentName))
                    {
                        depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                        existingDepts = existingDepts + "," + item.departmentName;
                    }
                }

                ViewBag.department = depts;

                var ugcoures = specializatons.Where(i => ugids.Contains(i.departmentId)).ToList();
                var pgcoures = specializatons.Where(i => pgids.Contains(i.departmentId)).ToList();
                //var phdcoures = db.jntuh_specialization.Where(i => phdids.Contains(i.departmentId)).Select(i => i.specializationName).ToList();

                ViewBag.ugcourses = ugcoures;
                ViewBag.pgcourses = pgcoures;
                //ViewBag.phdcourses = phdcoures;
                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
            }
            return PartialView("_MinorityPrincipleVerificationEdit", regFaculty);
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult MinorityPrincipalVerificationEditPost(FacultyRegistration faculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var facultydetails = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == faculty.RegistrationNumber).FirstOrDefault();
            if (facultydetails != null)
            {
                facultydetails.Absent = faculty.Absent;
                if (faculty.ModifiedPANNo != null)
                {
                    facultydetails.PANNumber = faculty.ModifiedPANNo;
                    facultydetails.ModifiedPANNumber = faculty.ModifiedPANNo;
                }
                if (faculty.MOdifiedDateofAppointment1 != null)
                {
                    facultydetails.DateOfAppointment = Convert.ToDateTime(faculty.MOdifiedDateofAppointment1);
                }
                if (faculty.DepartmentId != null)
                {
                    facultydetails.DepartmentId = faculty.DepartmentId;
                }
                facultydetails.InvalidPANNumber = faculty.InvalidPANNo;
                facultydetails.NoRelevantUG = faculty.NORelevantUG;
                facultydetails.NoRelevantPG = faculty.NORelevantPG;
                facultydetails.NORelevantPHD = faculty.NORelevantPHD;
                facultydetails.NoSCM = faculty.NoSCM;
                facultydetails.NoForm16 = faculty.NOForm16;
                facultydetails.NotQualifiedAsperAICTE = faculty.NOTQualifiedAsPerAICTE;
                //facultydetails.MultipleRegInSameCollege = faculty.MultipleReginSamecoll;
                //facultydetails.MultipleRegInDiffCollege = faculty.MultipleReginDiffcoll;
                //facultydetails.SamePANUsedByMultipleFaculty = faculty.SamePANUsedByMultipleFaculty;
                //facultydetails.PhotoCopyofPAN = faculty.PhotocopyofPAN;
                //facultydetails.AppliedPAN = faculty.AppliedPAN;
                //facultydetails.LostPAN = faculty.LostPAN;
                facultydetails.OriginalsVerifiedUG = faculty.OriginalsVerifiedUG;
                facultydetails.OriginalsVerifiedPG = faculty.OriginalsVerifiedPG;
                facultydetails.OriginalsVerifiedPHD = faculty.OriginalsVerifiedPHD;
                facultydetails.IncompleteCertificates = faculty.InCompleteCeritificates;
                facultydetails.FacultyVerificationStatus = true;
                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;
                facultydetails.FalsePAN = faculty.FalsePAN;
                db.SaveChanges();
            }
            return RedirectToAction("MinorityPrincipalVerfication", "PrincipalDirector");
            // return RedirectToAction("PrincipalVerificationDetails", "PrincipalDirector", new { fid = UAAAS.Models.Utilities.EncryptString(faculty.id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"])});
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult MinorityPrincipalVerificationApprove(string fid, string collegeid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var facultydetails = db.jntuh_registered_faculty.Find(fId);
            if (facultydetails != null)
            {
                facultydetails.Absent = false;
                facultydetails.InvalidPANNumber = false;
                facultydetails.NoRelevantUG = null;
                facultydetails.NoRelevantPG = null;
                facultydetails.NORelevantPHD = null;
                facultydetails.NoSCM = false;
                facultydetails.NoForm16 = false;
                facultydetails.NotQualifiedAsperAICTE = false;
                //facultydetails.MultipleRegInSameCollege = false;
                //facultydetails.MultipleRegInDiffCollege = false;
                //facultydetails = false;
                //facultydetails.PhotoCopyofPAN = false;
                //facultydetails.AppliedPAN = false;
                //facultydetails.LostPAN = false;
                facultydetails.OriginalsVerifiedUG = false;
                facultydetails.OriginalsVerifiedPG = false;
                facultydetails.OriginalsVerifiedPHD = false;
                facultydetails.FacultyVerificationStatus = true;
                facultydetails.IncompleteCertificates = false;
                facultydetails.DeactivatedBy = userID;
                facultydetails.DeactivatedOn = DateTime.Now;
                db.SaveChanges();
                TempData["Success"] = "Principal Data Successfully Approved..";
            }
            return RedirectToAction("MinorityPrincipalVerfication", "PrincipalDirector");
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FacultyRegistrationNumber"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,College,DataEntry")]
        public JsonResult CheckRegistrationNumber(string RegistrationNumber)
        {
            var isRegistrationNumber = db.jntuh_registered_faculty.Where(F => F.RegistrationNumber.Trim() == RegistrationNumber.Trim()).FirstOrDefault();
            if (isRegistrationNumber != null)
            {
                if (isRegistrationNumber.Blacklistfaculy == true)
                {
                    return Json("Registration Number is in Blacklist.", JsonRequestBehavior.AllowGet);
                }
                else if (isRegistrationNumber.AbsentforVerification == true)
                {
                    return Json("Registration Number is in Inactive.", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(true);
            }
            else
                return Json("Invalid Registration Number.", JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult CheckAadharNumber(string PrincipalAadhaarNumber, string RegistrationNumber)
        {
            var status = aadharcard.validateVerhoeff(PrincipalAadhaarNumber);
            var jntuh_college_faculty_registered =
                db.jntuh_college_faculty_registered.Where(
                    f => f.AadhaarNumber == PrincipalAadhaarNumber && f.RegistrationNumber != RegistrationNumber)
                    .Select(e => e)
                    .Count();
            var jntuh_college_principal_registered =
              db.jntuh_college_principal_registered.Where(
                  f => f.AadhaarNumber == PrincipalAadhaarNumber && f.RegistrationNumber != RegistrationNumber)
                  .Select(e => e)
                  .Count();

            if (status)
            {
                if (jntuh_college_faculty_registered == 0 && jntuh_college_principal_registered == 0)
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

        public ActionResult ViewFacultyphdDetails(string fid)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("Logon", "Account");
            int? userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            if (fid != null)
            {
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            else
            {
                fID =
                    db.jntuh_registered_faculty.Where(r => r.UserId == userId).Select(s => s.id).FirstOrDefault();
            }
            if (fID == 113698)
            {
                fID = Convert.ToInt32(ConfigurationManager.AppSettings["testRegnumberfid"]);
            }
            ViewBag.fid = fid;
            var registredfaculty = db.jntuh_registered_faculty.Where(r => r.id == fID).Select(s => s).FirstOrDefault();
            if (registredfaculty == null)
            {
                TempData["ERROR"] = "Faculty Details not found.";
            }
            FacultyphdDetails phddetails = new FacultyphdDetails();
            List<SelectListItem> phdtypeslist = new List<SelectListItem>();
            phdtypeslist.Add(new SelectListItem { Text = "Full Time/ Regular", Value = "1" });
            phdtypeslist.Add(new SelectListItem { Text = "Part Time/ External", Value = "2" });
            ViewBag.phdtype = phdtypeslist;

            List<SelectListItem> phdnotificationissuedlist = new List<SelectListItem>();
            phdnotificationissuedlist.Add(new SelectListItem { Text = "Online", Value = "1" });
            phdnotificationissuedlist.Add(new SelectListItem { Text = "Offline", Value = "2" });
            ViewBag.phdnotificationissuedlist = phdnotificationissuedlist;

            List<SelectListItem> phdofferingDepartmentsList = new List<SelectListItem>();
            var departments = db.jntuh_department.Where(r => r.degreeId == 4).Select(s => s).ToList();
            foreach (var item in departments)
            {
                phdofferingDepartmentsList.Add(new SelectListItem { Text = item.departmentDescription.ToString(), Value = item.id.ToString() });
            }
            //phdofferingDepartmentsList.Add(new SelectListItem {Text = "Others",Value = "0"});
            phdofferingDepartmentsList = phdofferingDepartmentsList.OrderByDescending(c => c.Value).ToList();
            ViewBag.phdofferingDepartmentsList = phdofferingDepartmentsList;
            phddetails.PhdDepts = phdofferingDepartmentsList.Select(s => s.Text).ToList();
            List<SelectListItem> phdofferingSpecList = new List<SelectListItem>();
            var specialization = db.jntuh_specialization.Select(s => s).ToList();
            foreach (var item in specialization)
            {
                phdofferingSpecList.Add(new SelectListItem { Text = item.specializationDescription.ToString(), Value = item.id.ToString() });
            }
            //phdofferingSpecList.Add(new SelectListItem { Text = "Others", Value = "0" });
            phdofferingSpecList = phdofferingSpecList.OrderByDescending(c => c.Value).ToList();
            ViewBag.phdofferingSpecList = phdofferingSpecList;
            phddetails.PhdSpecs = phdofferingSpecList.Select(s => s.Text).ToList();
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1970; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;
            phddetails.Universitys = new List<string>() { "JawaharLal Nehru Technologial University Hyderabad", "JawaharLal Nehru Technologial University Kakinada", "JawaharLal Nehru Technologial University Anathapur", "Rajiv Gandhi University", "Assam University", "Tezpur University", "University of Hyderabad, Hyderabad", "Maulana Azad National Urdu University", "English   and   Foreign   Languages   University", "Jamia Millia Islamia", "University of Delhi", "JawaharLal Nehru University", "Indira Gandhi National Open University", "South Asian University", "The  Indira  Gandhi  National  Tribal  University", "Dr. Harisingh Gour Vishwavidyalaya", "Mahatma  Gandhi  Antarrashtriya  Hindi  Vishwavidyalaya", "Mizoram University", "North Eastern Hill University", "Manipur University", "Central Agricultural University", "Nagaland University", "Pondicherry  University", "Sikkim University", "Tripura University", "Aligarh Muslim University", "Babasaheb  Bhimrao  Ambedkar  University", "Banaras Hindu University", "University of Allahabad", "Rajiv  Gandhi  National  Aviation  University", "Rani  Lakshmi  Bai  Central  Agricultural  University", "Visva Bharati, Shantiniketan", "Hemwati Nandan Bahuguna Garhwal University", "Central University of Tamil Nadu", "Indian Maritime University", "Central  University  of  Rajasthan", "Central University of Punjab", "Central University of Orissa", "Central University of Kerala", "Central University of Karnataka", "Central University of Jharkhand", "Central University of Kashmir, Transit Campus", "Central University of Jammu, Bagla (Rahya-Suchani)", "Central University of Himachal Pradesh", "Central University of Haryana", "Guru  Ghasidas  Vishwavidyalaya", "Central University of Bihar", "Nalanda University", "Mahatma  Gandhi  Central  University", "Central University of Gujarat", "Academy of Maritime Education and Training", "Amrita Vishwa Vidyapeetham", "Avinashilingam Institute for Home Science & Higher Education for Women", "B.L.D.E.", "B.S. Abdur Rahman Institute of Science and Technology", "Banasthai Vidyapith", "Bharath Institute of Higher Education & Research", "Bharati Vidyapeeth", "Bhatkhande Music Institute", "Birla Institute of Technology", "Birla Institute of Technology & Science", "Central Institute of Buddhist Studies(CIBS)", "Central Institute of Fisheries Education", "Central Institute of Higher Tibetan Studies", "Chennai Mathematical Institute", "Chettinad Academy of Research and Education (CARE)", "Chinmaya Vishwavidyapeeth", "Christ", "D.Y Patil Educational Society", "Datta Meghe Institute of Medical Sciences", "Dayalbagh Educational Institute", "Deccan College Postgraduate & Research Institute", "Dr. D.Y. Patil Vidyapeeth", "Dr. M.G.R. Educational and Research Institute", "Forest Research Institute", "Gandhi Institute of Technology and Management (GITAM)", "Gandhiigram Rural Institute", "Gokhale Institute of Politics & Economics", "Graphic Era", "Gujarat Vidyapith", "Gurukul Kangri vidyapeeth", "Hindustan Institute of Technology and Science (HITS)", "Homi Bhabha National Institute, Regd. Office", "ICFAI Foundation for Higher Education", "IIS", "Indain Institute of Foreigen Trade", "Indian Agricultural Research Institute", "Indian Association for the Cultivation of Science (IACS)", "Indian Institute of Information Technology and Management", "Indian Institute of Science", "Indian Institute of Space Science and Technology", "Indian Law Institute", "Indian School of Mines", "Indian Veterinary Research Institute", "Indira Gandhi Institute of Development Research", "Institute of Advanced Studies in Education", "Institute of Chemical Technology", "Institute of liver and Biliary Sciences", "Instituteof Armamrnt Technology", "International Institute for Population Sciences", "International Institute of Information Technology", "Jain", "Jain Vishva Bharati Institute", "Jamia Hamdard", "Janardan Rai Nagar Rajasthan Vidyapeeth", "Jawahar lal Nehru Centre for Advanced Scientific Research", "Jaypee Institute of Information Technology", "JSS Academy of Higher Education & Research", "K.L.E. Academy of Higher Education and Research", "Kalasalingam Academy of Research and Education", "Kalinga Institute of Industrial Technology", "Karpagam Academy of Higher Education", "Karunya Institute of Technology and Sciences", "Kerala Kalamandalam", "Koneru Lakshmaiah Education Foundation", "Krishna Institute of Medical Sciences", "Lakshmibai National Institute of Physical Education", "Lingaya's Vidyapeeth", "LNM Istitute of Information Technology", "Maharishi Markandeshwar (Deemed to be University)", "Manav Rachna International Institute of Research and Studies", "Manipal Academy of Higher Education", "Meenakshi Academy of Higher Education and Research", "MGM Institute of Health Sciences", "Narsee Monjee Institute of Management Studies", "National Brain Research Centre", "National Dairy Research Institute", "National Institute of Food Technology, Entrepreneurship & Management (NIFTEM)", "National Institute of Mental Health & Neuro Sciences", "National Museum Institute of History of Arts, Conservation and Musicology", "National Rail and Transportation Institute", "National University of Educational Planning & Administration", "Nava Nalanda Mahavihara", "Nehru Gram Bharati", "NITTE", "Noorul Islam Centre for Higher Education", "North Eastern Regional Institute of Science & Technology", "Padmashree Dr.D.Y. Patil Vidyapeeeth", "Pandit Dwarka Prasad Mishra Indian Institute of Information Technology", "Periyar Manaimmai Institute of Science & Technology (PMIST)", "Ponnaiyan Ramajayam Institute of Science & technology (PMIST)", "Pravara Institute of Medical Sciences", "Punjab Engineering College", "Rajiv Gandhi National Institute of Youth Development", "Ramakrishna Mission Vivekananda Educational and Research Institute", "Rashtriya Sanskrit Sansthana", "Rashtriya Sanskrit Vidyapeeth", "S.R.M Institute of Science and Technology", "Sam Higginbottom Institute of Agriculture, Technology & Sciences", "Sant Longowal Institute of Engineering and Technology", "Santosh", "Sathyabama Institute of Science and Technology", "Saveetha Institute of Medical and Technical Sciences", "Shanmugha Arts Science Technology & Research Academy (SASTRA)", "Shobhit Institute of Engineering & Technology", "Shri Lal Bahadur Shastri Rashtriya Sanskrit Vidyapith", "Siksha 'O' Anusandhan", "Sri Balaji Vidyapeeth (Deemed to be University)", "Sri Chandrasekharendra Saraswathi Vishwa Mahavidyalaya", "Sri Devraj Urs Academy of Higher Education and Research", "Sri Ramachandra Medical College and Research Institute", "Sri Sathya Sai Institute of Higher Learning", "Sri Siddhartha Academy of Higher Education", "St. Peterâ€™s Institute of Higher Education and Research", "Sumandeep Vidyapeeth", "Swami Vivekananda Yoga Anusandhana Samsthana", "Symbiosis International", "Tata Institute of Fundamental Research", "Tata Institute of Social Sciences", "TERI School of Advanced studies", "Thapar Institute of Engineering & Technology", "Tilak Maharashtra Vidyapeeth", "Vel Tech Rangarajan Dr. Sagunthala R & D Institute of Science and Technology", "Vellore Institute of Technology", "VELS Institute of Science Technology & Advanced Studies (VISTAS)", "Vignan's Foundation for Science, Technology and Research", "Vinayaka Missionâ€™s Research Foundation", "Yenepoya", "A.P.J. Abdul Kalam Technological University", "Acharaya N.G.Ranga Agricultural University", "Acharaya Nagarjuna University", "Adikavi Nannaya University", "Akkamahadevi women's University (Formerly known as Karnataka State Women's University)", "Alagappa University", "Aliah University", "Allahabad State University", "Ambedkar University Delhi (AUD)", "Anand Agricultural University", "Andhra University", "Anna University", "Annamalai University", "Arybhatta Knowledge University", "Assam Agricultural University", "Assam Rajiv Gandhi University of Co-operative Management", "Assam Science & Technology University", "Assam Womens University", "Atal Bihari Vajpayee Hindi Vishwavidyalaya", "Awadesh Pratap Singh University", "Ayush and Health Sciences University of Chhattisgarh", "Baba Farid University of Health Sciences", "Baba Ghulam Shah Badshah University", "Babasaheb Bhimrao Ambedkar Bihar University", "Banda University of Agriculture & Technology", "Bangalore University", "Bankura University", "Barkatullaah University", "Bastar Vishwavidyalaya", "Bengaluru Central University", "Bengaluru North University", "Berhampur University", "Bhagat Phool Singh Mahila Vishwavidyalaya", "Bhakta Kavi Narsinh Mehta University", "Bharathiar University", "Bharathidasan University", "Bhupender Narayan Mandal University", "Bidhan Chandra Krishi Vishwavidyalaya", "Bihar Agricultural University", "Biju Patnaik University of Technology", "Bilaspur Vishwavidyalaya", "Binod Bihari Mahto Koylanchal University", "Birsa Agricultural University", "Bodoland University", "Bundelkhand University", "Burdwan University", "Calcutta University", "Calicut University", "CEPT University", "Ch. Bansi Lal University", "Chanakya National Law University", "Chandr Shekhar Azad University of Agriculture & Technology", "Chatrapati Sahuji Maharaj Kanpur University", "Chaudhary Devi Lal University", "Chaudhary Ranbir Singh University", "Chaudhary Sarwan Kumar Himachal Pradesh Krishi Vishvavidyalaya", "Chhattisgarh Kamdhenu Vishwavidyalaya", "Chhattisgarh Swami Vivekanad Technical Universty", "Childrens University", "Choudary Charan Singh Haryana Agricultural Univeersity", "Choudary Charan Singh University", "Cluster University of Jammu", "Cluster University of Srinagar", "Cochin Unviersity of Science & Technology", "Cooch Behar Panchanan Barma University", "Cotton University", "Damodaram Sanjivayya National Law University", "Davangere University", "Deen Bandhu Chhotu Ram University of Sciences & Technology", "Deen Dayal Upadhyay Gorakhpur University", "Delhi Pharmaceutical Sciences & Research University", "Delhi Technological University", "Devi Ahilya Vishwavidyalaya", "Dharmashastra National Law University", "Dharmsinh Desai University", "Diamond Harbour Womens University", "Dibrugarh University", "Doon University", "Dr Shyama Prasad Mukherjee University", "Dr. A.P.J. Abdul Kalam Technical University", "Dr. B. R. Ambedkar University of Social Sciences", "Dr. B.R. Ambedkar University", "Dr. B.R.Ambedkar Open University", "Dr. B.R.Ambedkar University", "Dr. Babasaheb Ambedkar Marathwada University", "Dr. Babasaheb Ambedkar Open University", "Dr. Babasaheb AmbedkarTechnological University", "Dr. Bhimrao Ambedkar Law University", "Dr. N.T.R. University of Health Sciences", "Dr. Punjabrao Deshmukh Krishi Vidyapeeth", "Dr. Ram Manohar Lohia Awadh University", "Dr. Ram Manohar Lohiya National Law University", "Dr. Shakuntala Misra National Rehabilitation University", "Dr. Shyama Prasad University", "Dr. Y.S.Parmar University of Horticulture & Forestry", "Dr. Y.S.R. Horticultural Univerity", "Dravidian University", "Durg Vishwavidyalaya", "Fakir Mohan University", "G.B.Pant University of Agriculture & Technology", "Gangadhar Meher University", "Gauhati University", "Gautam Buddha University", "Goa University", "Gondwana University", "Govind Guru Tribal University", "Gujarat Agricultural University", "Gujarat Ayurveda University", "Gujarat Forensic Sciences University", "Gujarat National Law University", "Gujarat Technological University", "Gujarat University", "Gujarat University of Transplantation Sciences", "Gulbarga University", "Guru Angad Dev Veterinary & Animal Sciences University", "Guru Gobind Singh Indraprastha Vishwavidyalaya", "Guru Jambeshwar University of Science and Technology", "Guru Nanak Dev University", "Guru Ravidas Ayurved University", "Harcourt Butler Technical University", "Haridev Joshi University of Journalism and Mass Communication", "Haryana Vishwakarma Skill University", "Hemchandracharya North Gujarat University", "Hemwati Nandan Bahuguna Medical Education University", "Hidayatullah National Law University", "Himachal Pradesh National Law University", "Himachal Pradesh Technical University", "Himachal Pradesh University", "Indian Institute of Teacher Education", "Indira Gandhi Delhi Technical University for Women", "Indira Gandhi Krishi Vishwavidyalaya", "Indira Gandhi University", "Indira Kala Sangeet Vishwavidyalaya", "Indraprastha Institute of Information Technology", "Institute of Infrastructure Technology Research and Management", "International Institute of Information Technology (IIIT)", "Islamic University of Sciences & Technology University", "Jadavpur University", "Jagadguru Ramanandacharya Sanskrit University", "Jai Naraim Vyas University", "Jai Prakash vishwavidyalaya(university)", "Jammu University", "Jananayak Chandrashekhar University", "Jawaharlal Nehru Architecture and Fine Arts University", "Jawaharlal Nehru Krishi Vishwavidyalaya", "Jawaharlal Nehru Technological University", "Jharkhand Raksha Shakti University", "Jiwaji University", "Junagarh Agricultural University", "Kakatiya University", "Kaloji Narayan Rao University of Health Sciences", "Kalyani University", "Kamdhenu University", "Kameshwar Singh.Darbhanga Sanskrit Vishwavidyalaya", "Kannada University", "Kannur University", "Karanataka State Law University", "Karanataka State Open University", "Karanataka University", "Karanataka Veterinary, Animal & Fisheries Science University", "Karnataka Folklore University", "Karnataka Sanskrit University", "Karnataka State Rural Development and Panchayat Raj University", "Kashmir University", "Kavi Kulguru Kalidas Sanskrit Vishwavidyalaya", "Kazi Nazrul University", "Kerala Agricultural Unviersity", "Kerala University", "Kerala University of Fisheries & Ocean Studies", "Kerala University of Health Sciences", "Kerala Veterinary & Animal Sciences University", "Khallikote University", "Khwaja Moinuddin Chishti Urdu, Arabi-Farsi University", "King Georges Medical University", "Kolhan University", "Konkan Krishi Vidyapeeth", "Krantiguru Shyamji Krishna Verma Kachchh University", "Krishna Kanta Handique State Open University", "Krishna University", "Krishnakumarsinhji Bhavnagar University", "KSGH Music and Performing Arts University", "Kumar Bhaskar Varma Sanskrit & Ancient Studies University", "Kumaun University", "Kurukshetra University", "Kushabhau Thakre Patrakarita Avam Jansanchar Vishwavidyalaya", "Kuvempu University", "Lala Lajpat Rai University of Veterinary & Animal Sciences", "Lalit Narayan Mithila University", "Lucknow University", "M.J.P. Rohilkhand University", "M.P.Bhoj (open) University", "Madan Mohan Malaviya University of Technology", "Madaras University", "Madhya Pradesh Pashu Chikitsa Vigyan Vishwavidyalaya", "Madurai Kamraj University", "Magadh University", "Mahamaya Technical University", "Maharahtra University of Health Sciences", "Maharaja Bir Bikram University", "Maharaja Chhatrasal Bundelkhand Vishwavidyalaya", "Maharaja Ganga Singh University", "Maharaja Ranjit Singh Punjab Technical University", "Maharaja Sayajirao University of Baroda", "Maharaja Surajmal Brij University", "Maharana Partap Horticultural University", "Maharana Pratap University of Agriculture & Technology", "Maharashtra Animal & Fishery Sciences University", "Maharashtra National Law University", "Maharashtra National Law University,", "Maharishi Dayanand Saraswati University", "Maharishi Dayanand University", "Maharshi Panini Sanskrit Evam Vedic Vishwavidyalaya", "Mahatam Gandhi Kashi Vidyapeeth", "Mahatma Gandhi Chitrakoot Gramodaya Vishwavidyalaya", "Mahatma Gandhi University", "Mahatma Gandhi Unversity", "Mahatma Phule Krishi Vidyapeeth", "Makhanlal Chaturvedi National University of Journalism & Communication", "Mangalore University", "Manipur Technical University", "Manipur University of Culture", "Manonmaniam Sundarnar University", "Marathwada Agricultural University", "Maulana Abul Kalam Azad University of Technology", "Maulana Mazharul Haque Arabic & Persian University", "Mohan Lal Shukhadia University", "Mother Teresa Womens University", "Mumbai University", "Munger University", "Mysore University", "Nalanda Open University", "Narendra Deo University of Agriculture & Technology", "National Academy of Legal Studies & Research University", "National Law Institute University", "National law School of India University", "National Law Universituy", "National Law University", "National Law University and Judicial Academy", "National University of Advanced Legal Studies (NUALS)", "National University of Study & Research in Law", "Navsari Agriculture University", "Netaji Shubhash Open University", "Nilamber-Pitamber University", "Nizams Institute of Medical Sciences", "North Benagal University", "North Maharashtra University", "North Orissa University", "Odisha State Open University", "Orissa University Of Agriculture & Technology", "Osmania University", "Palamuru University", "Pandit S N Shukla University", "Patliputra University", "Patna University", "Periyar University", "Potti Sreeramulu Teugu Universtity", "Presidency University", "Professor Jayashankar Telangana State Agricultural University", "Pt. Bhagwat Dayal Sharma University of Health Sciences", "Pt. Sundarlal Sharma (Open) University", "Pt.Ravishankar Shukla University", "Punjab Agriculture University", "Punjab Technical University", "Punjab University", "Punjabi University", "Purnea University", "Rabindra Bharati University", "Raiganj University", "Raj Rishi Bhartrihari Matsya University", "Raja Mansingh Tomar Music & Arts University", "Rajasthan Ayurveda University", "Rajasthan ILD Skills University (RISU)", "Rajasthan Technical University", "Rajasthan University", "Rajasthan University of Health Sciences", "Rajasthan University of Veterinary & Animal Sciences", "Rajendra Agricultural University", "Rajiv Gandhi Prodoyogiki Vishwavidyalaya", "Rajiv Gandhi University of Health Science", "Rajiv Gandhi University of Knowledge Technology", "Rajmata Vijayaraje Scindia Krishi Vishwa Vidyalaya", "Raksha Shakti University", "Rama Devi Womens University", "Ranchi University", "Rani Channamma University", "Rani Durgavati Vishwavidyalaya", "Ravenshaw University", "Rayalaseema University", "Sambalpur University", "Sampurnanand Sanskrit Vishwavidyalaya", "Sanchi University of Buddhist-Indic Studies", "Sant Gadge Baba Amravati University", "Sardar Krushinagar Dantiwada Agricultural University", "Sardar Patel University", "Sardar Patel University of Police, Security & Criminal Justice", "Sardar Vallabh Bhai Patel University of Agriculture & Technology", "Sarguja University", "Satavahana University", "Saurashtra University", "Savitribai Phule Pune University", "Shekhawati University", "Sher-e-Kashmir University of Agricultural Science & Technology", "Shivaji University", "shree guru gobind singh tricentenary university", "Shree Sankaracharaya University of Sanskrit", "Shree Somnath Sanskrit University", "Shri Govind Guru University", "Shri Jagannath Sanskrit Vishwavidyalaya", "Shri Mata Vaishno Devi University", "Siddharth University", "Sidho-Kanho-Birsha University", "Sido Kanhu University", "Smt. Nathibai Damodar Thackersey Womens University", "Solapur University", "Sri Dev Suman Uttarakhand Vishwavidyalaya", "Sri Konda Laxman Telangana State Horticultural University", "Sri krishnadevaraya University", "Sri P V Narsimha Rao Telangana Veterinary University", "Sri Padmavati Mahila Vishwavidyalayam", "Sri Venkateswara Institute of Medical Sciences", "Sri Venkateswara University", "Sri Venkateswara Vedic University", "Sri Venkateswara Veterinary University", "Srimanta Sankaradeva University of Health Sciences", "State University of Performing and Visual Arts", "Swami Keshwanand Rajasthan Agriculture University", "Swami Ramanand Teerth Marathwada University", "Swarnim Gujarat Sports University", "T.M. Bhagalpur University", "Tamil Nadu Fisheries University", "Tamil Nadu Music and Fine Arts University", "Tamil Nadu Open University", "Tamil Nadu Teacher Education University", "Tamil University", "Tamilnadu Agricultural University", "Tamilnadu Dr. Ambedkar Law University", "Tamilnadu Dr. M.G.R.Medical University", "Tamilnadu National Law School", "Tamilnadu Physical Educaton and Sports University", "Tamilnadu Veterinary & Animal Sciences University", "Telangana University", "The Bengal Engineering & Science University", "The Rajiv Gandhi National University of Law", "The Rashtrasant Tukadoji Maharaj Nagpur University", "The Sanskrit College and University", "The West Bengal National University of Juridical Science", "The West Bengal University of Health Sciences", "Thiruvalluvar University", "Thunchath Ezhuthachan Malayalam University", "Tumkur University", "U.P. Pandit Deen Dayal Upadhyaya Pashu Chikitsa Vigyan Vishwavidhyalaya Evam Go-Anusandhan Sansthan", "U.P. Rajarshi Tandon Open University", "U.P.King Georges University of Dental Sciences", "University of Agricultural Sciences", "University of Horticultural Sciences", "University of Kota", "Univesity of Gour Banga", "Utkal University", "Utkal University of Culture", "Uttar Banga Krishi Vishwavidyalaya", "Uttar Pradesh University of Medical Sciences", "Uttarakhand Aawasiya Viswavidyalaya", "Uttarakhand Ayurved University", "Uttarakhand Open University", "Uttarakhand Sanskrit University", "Uttarakhand Technical University", "Vardhman Mahaveer Open University", "Veer Bahadur Singh Purvanchal University", "Veer Chandra Singh Garhwali Uttarakhand University of Horticulture & Forestry", "Veer Kunwar Singh University", "Veer Narmad South Gujarat University", "Veer Surendra Sai Institute of Medical Science and Research", "Veer Surendra Sai University of Technology", "Vesveswaraiah Technological University", "Vidya Sagar University", "Vijayanagara Sri Krishnadevaraya University", "Vikram Simhapuri University", "Vikram University", "Vinoba Bhave University", "West Bengal State University", "West Bengal University of Animal and Fishery Sciences", "West Bengal University of Teachers, Training, Education Planning and Administration", "Yashwant Rao Chavan Maharashtra Open University", "YMCA University of Science & Technology", "Yogi Vemana University" };
            // phddetails.Universitys = new List<string>() { "Rajiv Gandhi University", "Assam University", "Tezpur University", "University of Hyderabad, Hyderabad", "Maulana Azad National Urdu University", "English   and   Foreign   Languages   University", "Jamia Millia Islamia", "University of Delhi", "JawaharLal Nehru University", "Indira Gandhi National Open University", "South Asian University", "The  Indira  Gandhi  National  Tribal  University", "Dr. Harisingh Gour Vishwavidyalaya", "Mahatma  Gandhi  Antarrashtriya  Hindi  Vishwavidyalaya", "Mizoram University", "North Eastern Hill University", "Manipur University", "Central Agricultural University", "Nagaland University", "Pondicherry  University", "Sikkim University", "Tripura University", "Aligarh Muslim University", "Babasaheb  Bhimrao  Ambedkar  University", "Banaras Hindu University", "University of Allahabad", "Rajiv  Gandhi  National  Aviation  University", "Rani  Lakshmi  Bai  Central  Agricultural  University", "Visva Bharati, Shantiniketan", "Hemwati Nandan Bahuguna Garhwal University", "Central University of Tamil Nadu", "Indian Maritime University", "Central  University  of  Rajasthan", "Central University of Punjab", "Central University of Orissa", "Central University of Kerala", "Central University of Karnataka", "Central University of Jharkhand", "Central University of Kashmir, Transit Campus", "Central University of Jammu, Bagla (Rahya-Suchani)", "Central University of Himachal Pradesh", "Central University of Haryana", "Guru  Ghasidas  Vishwavidyalaya", "Central University of Bihar", "Nalanda University", "Mahatma  Gandhi  Central  University", "Central University of Gujarat", "Academy of Maritime Education and Training", "Amrita Vishwa Vidyapeetham", "Avinashilingam Institute for Home Science & Higher Education for Women", "B.L.D.E.", "B.S. Abdur Rahman Institute of Science and Technology", "Banasthai Vidyapith", "Bharath Institute of Higher Education & Research", "Bharati Vidyapeeth", "Bhatkhande Music Institute", "Birla Institute of Technology", "Birla Institute of Technology & Science", "Central Institute of Buddhist Studies(CIBS)", "Central Institute of Fisheries Education", "Central Institute of Higher Tibetan Studies", "Chennai Mathematical Institute", "Chettinad Academy of Research and Education (CARE)", "Chinmaya Vishwavidyapeeth", "Christ", "D.Y Patil Educational Society", "Datta Meghe Institute of Medical Sciences", "Dayalbagh Educational Institute", "Deccan College Postgraduate & Research Institute", "Dr. D.Y. Patil Vidyapeeth", "Dr. M.G.R. Educational and Research Institute", "Forest Research Institute", "Gandhi Institute of Technology and Management (GITAM)", "Gandhiigram Rural Institute", "Gokhale Institute of Politics & Economics", "Graphic Era", "Gujarat Vidyapith", "Gurukul Kangri vidyapeeth", "Hindustan Institute of Technology and Science (HITS)", "Homi Bhabha National Institute, Regd. Office", "ICFAI Foundation for Higher Education", "IIS", "Indain Institute of Foreigen Trade", "Indian Agricultural Research Institute", "Indian Association for the Cultivation of Science (IACS)", "Indian Institute of Information Technology and Management", "Indian Institute of Science", "Indian Institute of Space Science and Technology", "Indian Law Institute", "Indian School of Mines", "Indian Veterinary Research Institute", "Indira Gandhi Institute of Development Research", "Institute of Advanced Studies in Education", "Institute of Chemical Technology", "Institute of liver and Biliary Sciences", "Instituteof Armamrnt Technology", "International Institute for Population Sciences", "International Institute of Information Technology", "Jain", "Jain Vishva Bharati Institute", "Jamia Hamdard", "Janardan Rai Nagar Rajasthan Vidyapeeth", "Jawahar lal Nehru Centre for Advanced Scientific Research", "Jaypee Institute of Information Technology", "JSS Academy of Higher Education & Research", "K.L.E. Academy of Higher Education and Research", "Kalasalingam Academy of Research and Education", "Kalinga Institute of Industrial Technology", "Karpagam Academy of Higher Education", "Karunya Institute of Technology and Sciences", "Kerala Kalamandalam", "Koneru Lakshmaiah Education Foundation", "Krishna Institute of Medical Sciences", "Lakshmibai National Institute of Physical Education", "Lingaya's Vidyapeeth", "LNM Istitute of Information Technology", "Maharishi Markandeshwar (Deemed to be University)", "Manav Rachna International Institute of Research and Studies", "Manipal Academy of Higher Education", "Meenakshi Academy of Higher Education and Research", "MGM Institute of Health Sciences", "Narsee Monjee Institute of Management Studies", "National Brain Research Centre", "National Dairy Research Institute", "National Institute of Food Technology, Entrepreneurship & Management (NIFTEM)", "National Institute of Mental Health & Neuro Sciences", "National Museum Institute of History of Arts, Conservation and Musicology", "National Rail and Transportation Institute", "National University of Educational Planning & Administration", "Nava Nalanda Mahavihara", "Nehru Gram Bharati", "NITTE", "Noorul Islam Centre for Higher Education", "North Eastern Regional Institute of Science & Technology", "Padmashree Dr.D.Y. Patil Vidyapeeeth", "Pandit Dwarka Prasad Mishra Indian Institute of Information Technology", "Periyar Manaimmai Institute of Science & Technology (PMIST)", "Ponnaiyan Ramajayam Institute of Science & technology (PMIST)", "Pravara Institute of Medical Sciences", "Punjab Engineering College", "Rajiv Gandhi National Institute of Youth Development", "Ramakrishna Mission Vivekananda Educational and Research Institute", "Rashtriya Sanskrit Sansthana", "Rashtriya Sanskrit Vidyapeeth", "S.R.M Institute of Science and Technology", "Sam Higginbottom Institute of Agriculture, Technology & Sciences", "Sant Longowal Institute of Engineering and Technology", "Santosh", "Sathyabama Institute of Science and Technology", "Saveetha Institute of Medical and Technical Sciences", "Shanmugha Arts Science Technology & Research Academy (SASTRA)", "Shobhit Institute of Engineering & Technology", "Shri Lal Bahadur Shastri Rashtriya Sanskrit Vidyapith", "Siksha 'O' Anusandhan", "Sri Balaji Vidyapeeth (Deemed to be University)", "Sri Chandrasekharendra Saraswathi Vishwa Mahavidyalaya", "Sri Devraj Urs Academy of Higher Education and Research", "Sri Ramachandra Medical College and Research Institute", "Sri Sathya Sai Institute of Higher Learning", "Sri Siddhartha Academy of Higher Education", "St. Peterâ€™s Institute of Higher Education and Research", "Sumandeep Vidyapeeth", "Swami Vivekananda Yoga Anusandhana Samsthana", "Symbiosis International", "Tata Institute of Fundamental Research", "Tata Institute of Social Sciences", "TERI School of Advanced studies", "Thapar Institute of Engineering & Technology", "Tilak Maharashtra Vidyapeeth", "Vel Tech Rangarajan Dr. Sagunthala R & D Institute of Science and Technology", "Vellore Institute of Technology", "VELS Institute of Science Technology & Advanced Studies (VISTAS)", "Vignan's Foundation for Science, Technology and Research", "Vinayaka Missionâ€™s Research Foundation", "Yenepoya", "A.P.J. Abdul Kalam Technological University", "Acharaya N.G.Ranga Agricultural University", "Acharaya Nagarjuna University", "Adikavi Nannaya University", "Akkamahadevi women's University (Formerly known as Karnataka State Women's University)", "Alagappa University", "Aliah University", "Allahabad State University", "Ambedkar University Delhi (AUD)", "Anand Agricultural University", "Andhra University", "Anna University", "Annamalai University", "Arybhatta Knowledge University", "Assam Agricultural University", "Assam Rajiv Gandhi University of Co-operative Management", "Assam Science & Technology University", "Assam Womens University", "Atal Bihari Vajpayee Hindi Vishwavidyalaya", "Awadesh Pratap Singh University", "Ayush and Health Sciences University of Chhattisgarh", "Baba Farid University of Health Sciences", "Baba Ghulam Shah Badshah University", "Babasaheb Bhimrao Ambedkar Bihar University", "Banda University of Agriculture & Technology", "Bangalore University", "Bankura University", "Barkatullaah University", "Bastar Vishwavidyalaya", "Bengaluru Central University", "Bengaluru North University", "Berhampur University", "Bhagat Phool Singh Mahila Vishwavidyalaya", "Bhakta Kavi Narsinh Mehta University", "Bharathiar University", "Bharathidasan University", "Bhupender Narayan Mandal University", "Bidhan Chandra Krishi Vishwavidyalaya", "Bihar Agricultural University", "Biju Patnaik University of Technology", "Bilaspur Vishwavidyalaya", "Binod Bihari Mahto Koylanchal University", "Birsa Agricultural University", "Bodoland University", "Bundelkhand University", "Burdwan University", "Calcutta University", "Calicut University", "CEPT University", "Ch. Bansi Lal University", "Chanakya National Law University", "Chandr Shekhar Azad University of Agriculture & Technology", "Chatrapati Sahuji Maharaj Kanpur University", "Chaudhary Devi Lal University", "Chaudhary Ranbir Singh University", "Chaudhary Sarwan Kumar Himachal Pradesh Krishi Vishvavidyalaya", "Chhattisgarh Kamdhenu Vishwavidyalaya", "Chhattisgarh Swami Vivekanad Technical Universty", "Childrens University", "Choudary Charan Singh Haryana Agricultural Univeersity", "Choudary Charan Singh University", "Cluster University of Jammu", "Cluster University of Srinagar", "Cochin Unviersity of Science & Technology", "Cooch Behar Panchanan Barma University", "Cotton University", "Damodaram Sanjivayya National Law University", "Davangere University", "Deen Bandhu Chhotu Ram University of Sciences & Technology", "Deen Dayal Upadhyay Gorakhpur University", "Delhi Pharmaceutical Sciences & Research University", "Delhi Technological University", "Devi Ahilya Vishwavidyalaya", "Dharmashastra National Law University", "Dharmsinh Desai University", "Diamond Harbour Womens University", "Dibrugarh University", "Doon University", "Dr Shyama Prasad Mukherjee University", "Dr. A.P.J. Abdul Kalam Technical University", "Dr. B. R. Ambedkar University of Social Sciences", "Dr. B.R. Ambedkar University", "Dr. B.R.Ambedkar Open University", "Dr. B.R.Ambedkar University", "Dr. Babasaheb Ambedkar Marathwada University", "Dr. Babasaheb Ambedkar Open University", "Dr. Babasaheb AmbedkarTechnological University", "Dr. Bhimrao Ambedkar Law University", "Dr. N.T.R. University of Health Sciences", "Dr. Punjabrao Deshmukh Krishi Vidyapeeth", "Dr. Ram Manohar Lohia Awadh University", "Dr. Ram Manohar Lohiya National Law University", "Dr. Shakuntala Misra National Rehabilitation University", "Dr. Shyama Prasad University", "Dr. Y.S.Parmar University of Horticulture & Forestry", "Dr. Y.S.R. Horticultural Univerity", "Dravidian University", "Durg Vishwavidyalaya", "Fakir Mohan University", "G.B.Pant University of Agriculture & Technology", "Gangadhar Meher University", "Gauhati University", "Gautam Buddha University", "Goa University", "Gondwana University", "Govind Guru Tribal University", "Gujarat Agricultural University", "Gujarat Ayurveda University", "Gujarat Forensic Sciences University", "Gujarat National Law University", "Gujarat Technological University", "Gujarat University", "Gujarat University of Transplantation Sciences", "Gulbarga University", "Guru Angad Dev Veterinary & Animal Sciences University", "Guru Gobind Singh Indraprastha Vishwavidyalaya", "Guru Jambeshwar University of Science and Technology", "Guru Nanak Dev University", "Guru Ravidas Ayurved University", "Harcourt Butler Technical University", "Haridev Joshi University of Journalism and Mass Communication", "Haryana Vishwakarma Skill University", "Hemchandracharya North Gujarat University", "Hemwati Nandan Bahuguna Medical Education University", "Hidayatullah National Law University", "Himachal Pradesh National Law University", "Himachal Pradesh Technical University", "Himachal Pradesh University", "Indian Institute of Teacher Education", "Indira Gandhi Delhi Technical University for Women", "Indira Gandhi Krishi Vishwavidyalaya", "Indira Gandhi University", "Indira Kala Sangeet Vishwavidyalaya", "Indraprastha Institute of Information Technology", "Institute of Infrastructure Technology Research and Management", "International Institute of Information Technology (IIIT)", "Islamic University of Sciences & Technology University", "Jadavpur University", "Jagadguru Ramanandacharya Sanskrit University", "Jai Naraim Vyas University", "Jai Prakash vishwavidyalaya(university)", "Jammu University", "Jananayak Chandrashekhar University", "Jawaharlal Nehru Architecture and Fine Arts University", "Jawaharlal Nehru Krishi Vishwavidyalaya", "Jawaharlal Nehru Technological University", "Jharkhand Raksha Shakti University", "Jiwaji University", "Junagarh Agricultural University", "Kakatiya University", "Kaloji Narayan Rao University of Health Sciences", "Kalyani University", "Kamdhenu University", "Kameshwar Singh.Darbhanga Sanskrit Vishwavidyalaya", "Kannada University", "Kannur University", "Karanataka State Law University", "Karanataka State Open University", "Karanataka University", "Karanataka Veterinary, Animal & Fisheries Science University", "Karnataka Folklore University", "Karnataka Sanskrit University", "Karnataka State Rural Development and Panchayat Raj University", "Kashmir University", "Kavi Kulguru Kalidas Sanskrit Vishwavidyalaya", "Kazi Nazrul University", "Kerala Agricultural Unviersity", "Kerala University", "Kerala University of Fisheries & Ocean Studies", "Kerala University of Health Sciences", "Kerala Veterinary & Animal Sciences University", "Khallikote University", "Khwaja Moinuddin Chishti Urdu, Arabi-Farsi University", "King Georges Medical University", "Kolhan University", "Konkan Krishi Vidyapeeth", "Krantiguru Shyamji Krishna Verma Kachchh University", "Krishna Kanta Handique State Open University", "Krishna University", "Krishnakumarsinhji Bhavnagar University", "KSGH Music and Performing Arts University", "Kumar Bhaskar Varma Sanskrit & Ancient Studies University", "Kumaun University", "Kurukshetra University", "Kushabhau Thakre Patrakarita Avam Jansanchar Vishwavidyalaya", "Kuvempu University", "Lala Lajpat Rai University of Veterinary & Animal Sciences", "Lalit Narayan Mithila University", "Lucknow University", "M.J.P. Rohilkhand University", "M.P.Bhoj (open) University", "Madan Mohan Malaviya University of Technology", "Madaras University", "Madhya Pradesh Pashu Chikitsa Vigyan Vishwavidyalaya", "Madurai Kamraj University", "Magadh University", "Mahamaya Technical University", "Maharahtra University of Health Sciences", "Maharaja Bir Bikram University", "Maharaja Chhatrasal Bundelkhand Vishwavidyalaya", "Maharaja Ganga Singh University", "Maharaja Ranjit Singh Punjab Technical University", "Maharaja Sayajirao University of Baroda", "Maharaja Surajmal Brij University", "Maharana Partap Horticultural University", "Maharana Pratap University of Agriculture & Technology", "Maharashtra Animal & Fishery Sciences University", "Maharashtra National Law University", "Maharashtra National Law University,", "Maharishi Dayanand Saraswati University", "Maharishi Dayanand University", "Maharshi Panini Sanskrit Evam Vedic Vishwavidyalaya", "Mahatam Gandhi Kashi Vidyapeeth", "Mahatma Gandhi Chitrakoot Gramodaya Vishwavidyalaya", "Mahatma Gandhi University", "Mahatma Gandhi Unversity", "Mahatma Phule Krishi Vidyapeeth", "Makhanlal Chaturvedi National University of Journalism & Communication", "Mangalore University", "Manipur Technical University", "Manipur University of Culture", "Manonmaniam Sundarnar University", "Marathwada Agricultural University", "Maulana Abul Kalam Azad University of Technology", "Maulana Mazharul Haque Arabic & Persian University", "Mohan Lal Shukhadia University", "Mother Teresa Womens University", "Mumbai University", "Munger University", "Mysore University", "Nalanda Open University", "Narendra Deo University of Agriculture & Technology", "National Academy of Legal Studies & Research University", "National Law Institute University", "National law School of India University", "National Law Universituy", "National Law University", "National Law University and Judicial Academy", "National University of Advanced Legal Studies (NUALS)", "National University of Study & Research in Law", "Navsari Agriculture University", "Netaji Shubhash Open University", "Nilamber-Pitamber University", "Nizams Institute of Medical Sciences", "North Benagal University", "North Maharashtra University", "North Orissa University", "Odisha State Open University", "Orissa University Of Agriculture & Technology", "Osmania University", "Palamuru University", "Pandit S N Shukla University", "Patliputra University", "Patna University", "Periyar University", "Potti Sreeramulu Teugu Universtity", "Presidency University", "Professor Jayashankar Telangana State Agricultural University", "Pt. Bhagwat Dayal Sharma University of Health Sciences", "Pt. Sundarlal Sharma (Open) University", "Pt.Ravishankar Shukla University", "Punjab Agriculture University", "Punjab Technical University", "Punjab University", "Punjabi University", "Purnea University", "Rabindra Bharati University", "Raiganj University", "Raj Rishi Bhartrihari Matsya University", "Raja Mansingh Tomar Music & Arts University", "Rajasthan Ayurveda University", "Rajasthan ILD Skills University (RISU)", "Rajasthan Technical University", "Rajasthan University", "Rajasthan University of Health Sciences", "Rajasthan University of Veterinary & Animal Sciences", "Rajendra Agricultural University", "Rajiv Gandhi Prodoyogiki Vishwavidyalaya", "Rajiv Gandhi University of Health Science", "Rajiv Gandhi University of Knowledge Technology", "Rajmata Vijayaraje Scindia Krishi Vishwa Vidyalaya", "Raksha Shakti University", "Rama Devi Womens University", "Ranchi University", "Rani Channamma University", "Rani Durgavati Vishwavidyalaya", "Ravenshaw University", "Rayalaseema University", "Sambalpur University", "Sampurnanand Sanskrit Vishwavidyalaya", "Sanchi University of Buddhist-Indic Studies", "Sant Gadge Baba Amravati University", "Sardar Krushinagar Dantiwada Agricultural University", "Sardar Patel University", "Sardar Patel University of Police, Security & Criminal Justice", "Sardar Vallabh Bhai Patel University of Agriculture & Technology", "Sarguja University", "Satavahana University", "Saurashtra University", "Savitribai Phule Pune University", "Shekhawati University", "Sher-e-Kashmir University of Agricultural Science & Technology", "Shivaji University", "shree guru gobind singh tricentenary university", "Shree Sankaracharaya University of Sanskrit", "Shree Somnath Sanskrit University", "Shri Govind Guru University", "Shri Jagannath Sanskrit Vishwavidyalaya", "Shri Mata Vaishno Devi University", "Siddharth University", "Sidho-Kanho-Birsha University", "Sido Kanhu University", "Smt. Nathibai Damodar Thackersey Womens University", "Solapur University", "Sri Dev Suman Uttarakhand Vishwavidyalaya", "Sri Konda Laxman Telangana State Horticultural University", "Sri krishnadevaraya University", "Sri P V Narsimha Rao Telangana Veterinary University", "Sri Padmavati Mahila Vishwavidyalayam", "Sri Venkateswara Institute of Medical Sciences", "Sri Venkateswara University", "Sri Venkateswara Vedic University", "Sri Venkateswara Veterinary University", "Srimanta Sankaradeva University of Health Sciences", "State University of Performing and Visual Arts", "Swami Keshwanand Rajasthan Agriculture University", "Swami Ramanand Teerth Marathwada University", "Swarnim Gujarat Sports University", "T.M. Bhagalpur University", "Tamil Nadu Fisheries University", "Tamil Nadu Music and Fine Arts University", "Tamil Nadu Open University", "Tamil Nadu Teacher Education University", "Tamil University", "Tamilnadu Agricultural University", "Tamilnadu Dr. Ambedkar Law University", "Tamilnadu Dr. M.G.R.Medical University", "Tamilnadu National Law School", "Tamilnadu Physical Educaton and Sports University", "Tamilnadu Veterinary & Animal Sciences University", "Telangana University", "The Bengal Engineering & Science University", "The Rajiv Gandhi National University of Law", "The Rashtrasant Tukadoji Maharaj Nagpur University", "The Sanskrit College and University", "The West Bengal National University of Juridical Science", "The West Bengal University of Health Sciences", "Thiruvalluvar University", "Thunchath Ezhuthachan Malayalam University", "Tumkur University", "U.P. Pandit Deen Dayal Upadhyaya Pashu Chikitsa Vigyan Vishwavidhyalaya Evam Go-Anusandhan Sansthan", "U.P. Rajarshi Tandon Open University", "U.P.King Georges University of Dental Sciences", "University of Agricultural Sciences", "University of Horticultural Sciences", "University of Kota", "Univesity of Gour Banga", "Utkal University", "Utkal University of Culture", "Uttar Banga Krishi Vishwavidyalaya", "Uttar Pradesh University of Medical Sciences", "Uttarakhand Aawasiya Viswavidyalaya", "Uttarakhand Ayurved University", "Uttarakhand Open University", "Uttarakhand Sanskrit University", "Uttarakhand Technical University", "Vardhman Mahaveer Open University", "Veer Bahadur Singh Purvanchal University", "Veer Chandra Singh Garhwali Uttarakhand University of Horticulture & Forestry", "Veer Kunwar Singh University", "Veer Narmad South Gujarat University", "Veer Surendra Sai Institute of Medical Science and Research", "Veer Surendra Sai University of Technology", "Vesveswaraiah Technological University", "Vidya Sagar University", "Vijayanagara Sri Krishnadevaraya University", "Vikram Simhapuri University", "Vikram University", "Vinoba Bhave University", "West Bengal State University", "West Bengal University of Animal and Fishery Sciences", "West Bengal University of Teachers, Training, Education Planning and Administration", "Yashwant Rao Chavan Maharashtra Open University", "YMCA University of Science & Technology", "Yogi Vemana University" };
            phddetails.Places = new List<string>() { "Arunachal Pradesh", "Assam", "Telangana", "Delhi", "Madhya Pradesh", "Maharashtra", "Mizoram", "Meghalaya", "Manipur", "Nagaland", "Pondicherry", "Sikkim", "Tripura", "Uttar Pradesh", "West Bengal", "Uttarakhand", "Tamil Nadu", "Rajasthan", "Punjab", "Orissa", "Kerala", "Karnataka", "Jharkhand", "Jammu & Kashmir", "Himachal", "Pradesh", "Haryana", "Chhattisgarh", "Bihar", "Gujarat", "Jammu and Kashmir", "Andhra Pradesh", "Chandigarh", "Puducherry", "Himachal Pradesh", "Goa" };
            List<SelectListItem> universitytypelist = new List<SelectListItem>();
            universitytypelist.Add(new SelectListItem { Text = "Central", Value = "1" });
            universitytypelist.Add(new SelectListItem { Text = "State", Value = "2" });
            universitytypelist.Add(new SelectListItem { Text = "Private", Value = "3" });
            universitytypelist.Add(new SelectListItem { Text = "Deemed to be University", Value = "4" });
            universitytypelist.Add(new SelectListItem { Text = "International University", Value = "5" });
            ViewBag.universitytype = universitytypelist;
            if (registredfaculty != null)
            {
                phddetails.RegistrationNumber = registredfaculty.RegistrationNumber.Trim();
                phddetails.FacultyId = registredfaculty.id;
                phddetails.FirstName = registredfaculty.FirstName != null ? registredfaculty.FirstName.Trim() : registredfaculty.FirstName;
                phddetails.MiddleName = registredfaculty.MiddleName != null ? registredfaculty.MiddleName.Trim() : registredfaculty.MiddleName;
                phddetails.LastName = registredfaculty.LastName != null ? registredfaculty.LastName.Trim() : registredfaculty.LastName;
            }
            var collegefaculty =
                db.jntuh_college_faculty_registered.Where(
                    r => r.RegistrationNumber == registredfaculty.RegistrationNumber.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
            if (collegefaculty != null)
            {
                var college =
                    db.jntuh_college.Where(c => c.id == collegefaculty.collegeId).Select(s => s).FirstOrDefault();
                phddetails.CollegeCode = college.collegeCode.Trim();
                phddetails.CollegeName = college.collegeName.Trim();
            }
            var phdexsistdata =
                db.jntuh_faculty_phddetails.Where(p => p.Facultyid == registredfaculty.id)
                    .Select(s => s)
                    .FirstOrDefault();
            ViewBag.isPhdSubmit = false;
            if (phdexsistdata != null)
            {
                ViewBag.isPhdSubmit = phdexsistdata.IsSubmitted != true ? true : false;
                phddetails.PhdType = Convert.ToInt32(phdexsistdata.Phdtype);
                phddetails.PhdTypeid = phdtypeslist.Where(i => i.Value == phddetails.PhdType.ToString()).First().Text;
                phddetails.PhdAdmissionYear = phdexsistdata.AdmissionYear != null ? (int)phdexsistdata.AdmissionYear : 0;
                phddetails.PhdawardYear = Convert.ToInt32(phdexsistdata.Phdawardyear);
                phddetails.University = phdexsistdata.University;
                phddetails.PlaceofUniversity = phdexsistdata.Placeofuniversity;
                phddetails.PhdTitle = phdexsistdata.Phdtitle;
                phddetails.UniversityType = Convert.ToInt32(phdexsistdata.Universitytype);
                if (phdexsistdata.Universitytype != null && phdexsistdata.Universitytype > 0)
                {
                    phddetails.UniversityTypeTxt = universitytypelist.Where(i => i.Value == phdexsistdata.Universitytype.ToString()).First().Text;
                }
                phddetails.SupervisorName1 = phdexsistdata.Supervisorname1;
                phddetails.SupervisorName2 = phdexsistdata.Supervisorname2;
                phddetails.SupervisorName3 = phdexsistdata.Supervisorname3;
                phddetails.AdmissionLetterpath = phdexsistdata.Admissionletter;
                phddetails.PrephdLetterpath = phdexsistdata.Prephdletter;
                phddetails.GenuinenessLetterpath = phdexsistdata.Genuinenessletter;
                phddetails.CollegeAuthenticationLetterpath = phdexsistdata.Collegeauthaticationletter;
                phddetails.ThesiscoverPagepath = phdexsistdata.Thesiscoverpage;
                phddetails.OtherLetterpath = phdexsistdata.Otherletter;

                phddetails.InterNationalJrnls = phdexsistdata.IntNationalJrnls;
                phddetails.NationalJrnls = phdexsistdata.NationalJrnls;
                phddetails.InterNationalCnfrs = phdexsistdata.IntNationalCnfrns;
                phddetails.NationalCnfrs = phdexsistdata.NationalCnfrns;

                phddetails.InterNationalJrnlspath = phdexsistdata.IntNationalJrnlsDocLetter;
                phddetails.NationalJrnlspath = phdexsistdata.NationalJrnlsDocLetter;
                phddetails.InterNationalCnfrspath = phdexsistdata.IntNationalCnfrnsLetter;
                phddetails.NationalCnfrspath = phdexsistdata.NationalCnfrnsLetter;

                if (phdexsistdata.Vivadate != null)
                    phddetails.DateofVivaTxt = UAAAS.Models.Utilities.MMDDYY2DDMMYY(phdexsistdata.Vivadate.ToString()); ;
                if (phdexsistdata.Pressnotification != null)
                    phddetails.PressnotDateTxt = UAAAS.Models.Utilities.MMDDYY2DDMMYY(phdexsistdata.Pressnotification.ToString()); ;

                //New Fields
                phddetails.PhdnotificationIssued = phdexsistdata.Notificationissued;
                if (phdexsistdata.Notificationissued > 0)
                {
                    phddetails.PhdnotificationIssuedTxt = phdnotificationissuedlist.Where(i => i.Value == phdexsistdata.Notificationissued.ToString()).First().Text;
                }
                phddetails.RegistrationNumberOrHallticketNo = phdexsistdata.Hallticketnumber;
                phddetails.HowmanyreviewsRRMattended = phdexsistdata.Reviewsattended;
                phddetails.HowmanypapersPublished = phdexsistdata.Paperspublished;
                phddetails.Externalexamineratthetimeofdefense = phdexsistdata.Timeofdefense;
                //phddetails.Exactdateoffinalviva = phdexsistdata.Vivadate;
                phddetails.BOSChairpersonatthetimeofThesisSubmission = phdexsistdata.Boschairperson;
                //phddetails.PressnotificationofyourPhDDegree = phdexsistdata.Pressnotification;
                phddetails.HaveyouattendConvocation = phdexsistdata.Convocationattended != null ? (bool)phdexsistdata.Convocationattended : false;
                phddetails.HaveyouattendConvocationTxt = phdexsistdata.Convocationattended == true ? "Yes" : "No";
                phddetails.PhdofferingDepartment = phdexsistdata.Department;
                phddetails.PhdSpecialization = phdexsistdata.Specialization;
                phddetails.NoCformthecollegepath = phdexsistdata.Nocletter;
                phddetails.UniversityWebsite = phdexsistdata.Website;
                phddetails.UniversityAddress = phdexsistdata.Address;
                phddetails.HowmanypapersPublishedduringPhdWork = phdexsistdata.PaperspublishedDPW;
                phddetails.Phdodpath = phdexsistdata.Phdod;
                //phddetails.Phdod = phdexsistdata.Thesiscoverpage;
            }
            return View(phddetails);
        }
    }
}
