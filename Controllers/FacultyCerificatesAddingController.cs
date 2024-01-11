using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    //Written By Siva
    [ErrorHandling]
    public class FacultyCerificatesAddingController : BaseController
    {
        //
        // GET: /FacultyCerificatesAdding/
        //Written By Siva
        uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult GetFacultyRegistrationDetails()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index(string RegistrationNumber)
        {
            Faculty Faculty = new Faculty();
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            if (RegistrationNumber != null)
            {
                var RegisteredFaculty = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                if (RegisteredFaculty == null)
                {
                    TempData["Error"] = "Invalid Registration Number";
                    return RedirectToAction("GetFacultyRegistrationDetails");
                }

                var RegisteredFacultyEducation = db.jntuh_registered_faculty_education.Where(e => e.facultyId == RegisteredFaculty.id).Select(e => e).ToList();
                Faculty.RegistrationNumber = RegisteredFaculty.RegistrationNumber;
                Faculty.FullName = RegisteredFaculty.FirstName + " " + RegisteredFaculty.MiddleName + "" + RegisteredFaculty.LastName;
                Faculty.AadharNumber = RegisteredFaculty.AadhaarNumber;
                Faculty.AadharDocument = RegisteredFaculty.AadhaarDocument;
                Faculty.PANNumber = RegisteredFaculty.PANNumber;
                Faculty.PANDocument = RegisteredFaculty.PANDocument;

                if (RegisteredFacultyEducation.Count == 0)
                {
                    TempData["Error"] = "Education Details are NotFound";
                    return View(Faculty);
                }

                int i = 0;
                int FacultyLogId = 0;
                FacultyLogId = RegisteredFaculty.id;
                foreach (var item in RegisteredFacultyEducation.OrderBy(e => e.educationId))
                {
                    FacultyEducationClass edu = new FacultyEducationClass();
                    var couse = item.courseStudied;
                    Faculty.educationId = item.educationId;
                    if (i == 0)
                    {
                        Faculty.Education = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6)).Select(e => new FacultyEducationClass
                        {
                            hallticketno = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.hallticketnumber).FirstOrDefault(),
                            CouseStudied = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.courseStudied).FirstOrDefault(),
                            Specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.specialization).FirstOrDefault(),
                            PassedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.passedYear).FirstOrDefault(),
                            MarksPercentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.marksPercentage).FirstOrDefault(),
                            division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.division).FirstOrDefault(),
                            BoardOfUnivercity = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                            PlaceOfEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == FacultyLogId).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                        }).ToList();
                    }

                    Faculty.CreatedBy = item.createdBy;
                    Faculty.CreatedOn = item.createdOn;
                    Faculty.UpdatedBy = item.updatedBy;
                    Faculty.UpdatedOn = item.updatedOn;

                    if (Faculty.educationId == 1)
                        Faculty.FacultySSCcertificate = item.certificate;
                    else if (Faculty.educationId == 3)
                        Faculty.FacultyUGcertificate = item.certificate;
                    else if (Faculty.educationId == 4)
                        Faculty.FacultyPGcertificate = item.certificate;
                    else if (Faculty.educationId == 5)
                        Faculty.FacultyMphilcertificate = item.certificate;
                    else if (Faculty.educationId == 6)
                        Faculty.FacultyPhDcertificate = item.certificate;

                    i++;
                }
                return View("~/Views/FacultyCerificatesAdding/UpdateFacultyCeritificates.cshtml", Faculty);
            }
            else
            {
                TempData["Error"] = "Invalid Registration Number";
                return RedirectToAction("GetFacultyRegistrationDetails");
            }
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult SaveFacultyCerificates(Faculty Faculty)
        //{
        //    int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";
        //    string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
        //    string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";

        //    if (Faculty.RegistrationNumber != null)
        //    {
        //        var RegisteredFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber.Trim() == Faculty.RegistrationNumber.Trim());
        //        if (RegisteredFaculty == null)
        //        {
        //            TempData["Error"] = "Invalid RegistrationNumber";
        //            return RedirectToAction("Index");
        //        }

        //        jntuh_registered_faculty RegFaculty = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e).FirstOrDefault();
        //        RegFaculty.AadhaarNumber = Faculty.AadharNumber;

        //        if (Faculty.EditAadharDocument != null)
        //        {

        //            if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
        //            {
        //                Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));
        //            }

        //            var ext = Path.GetExtension(Faculty.EditAadharDocument.FileName);

        //            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //            {
        //                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                 RegFaculty.FirstName.Substring(0, 1) + "-" + RegFaculty.LastName.Substring(0, 1);
        //                Faculty.EditAadharDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
        //                    fileName, ext));
        //                RegFaculty.AadhaarDocument = string.Format("{0}{1}", fileName, ext);
        //            }
        //        }

        //        RegFaculty.PANNumber = Faculty.PANNumber;

        //        if (Faculty.EditPANDocument != null)
        //        {

        //            if (!Directory.Exists(Server.MapPath(panCardsPath)))
        //            {
        //                Directory.CreateDirectory(Server.MapPath(panCardsPath));
        //            }

        //            var ext = Path.GetExtension(Faculty.EditPANDocument.FileName);

        //            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //            {
        //                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                  RegFaculty.FirstName.Substring(0, 1) + "-" + RegFaculty.LastName.Substring(0, 1);
        //                Faculty.EditPANDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath),
        //                    fileName, ext));
        //                RegFaculty.PANDocument = string.Format("{0}{1}", fileName, ext);
        //            }
        //        }

        //        db.Entry(RegFaculty).State = EntityState.Modified;
        //        db.SaveChanges();

        //        for (int i = 0; i < 5; i++)
        //        {
        //            jntuh_registered_faculty_education FacultyEducation = new jntuh_registered_faculty_education();
        //            FacultyEducation.facultyId = RegisteredFaculty.id;
        //            FacultyEducation.hallticketnumber = Faculty.Education[i].hallticketno;
        //            FacultyEducation.courseStudied = Faculty.Education[i].CouseStudied;
        //            FacultyEducation.specialization = Faculty.Education[i].Specialization;
        //            FacultyEducation.passedYear = Faculty.Education[i].PassedYear;
        //            FacultyEducation.marksPercentage = Faculty.Education[i].MarksPercentage;
        //            FacultyEducation.division = Faculty.Education[i].division;
        //            FacultyEducation.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
        //            FacultyEducation.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

        //            if (i == 0)
        //            {
        //                if (Faculty.SSCcertificate != null)
        //                {
                            
        //                    FacultyEducation.educationId = 1;
        //                    FacultyEducation.courseStudied = "SSC";
        //                    FacultyEducation.specialization = "SSC";

        //                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
        //                    {
        //                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
        //                    }

        //                    var ext = Path.GetExtension(Faculty.SSCcertificate.FileName);

        //                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //                    {
        //                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                          RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
        //                                          RegisteredFaculty.LastName.Substring(0, 1) + "_" + FacultyEducation.courseStudied;
        //                        Faculty.SSCcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
        //                            fileName, ext));
        //                        FacultyEducation.certificate = string.Format("{0}{1}", fileName, ext);
        //                    }
        //                    FacultyEducation.isActive = true;
        //                    FacultyEducation.createdOn = DateTime.Now;
        //                    FacultyEducation.createdBy = UserId;
        //                    FacultyEducation.updatedOn = null;
        //                    FacultyEducation.updatedBy = null;
        //                    db.jntuh_registered_faculty_education.Add(FacultyEducation);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (i == 1)
        //            {

        //                if (Faculty.UGcertificate != null)
        //                {
        //                    FacultyEducation.educationId = 3;

        //                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
        //                    {
        //                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
        //                    }

        //                    var ext = Path.GetExtension(Faculty.UGcertificate.FileName);

        //                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //                    {
        //                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                          RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
        //                                          RegisteredFaculty.LastName.Substring(0, 1) + "_" + FacultyEducation.courseStudied;
        //                        Faculty.UGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
        //                            fileName, ext));
        //                        FacultyEducation.certificate = string.Format("{0}{1}", fileName, ext);
        //                    }
        //                    FacultyEducation.isActive = true;
        //                    FacultyEducation.createdOn = DateTime.Now;
        //                    FacultyEducation.createdBy = UserId;
        //                    FacultyEducation.updatedOn = null;
        //                    FacultyEducation.updatedBy = null;
        //                    db.jntuh_registered_faculty_education.Add(FacultyEducation);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (i == 2)
        //            {

        //                if (Faculty.PGcertificate != null)
        //                {
        //                    FacultyEducation.educationId = 4;
        //                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
        //                    {
        //                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
        //                    }

        //                    var ext = Path.GetExtension(Faculty.PGcertificate.FileName);

        //                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //                    {
        //                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                          RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
        //                                          RegisteredFaculty.LastName.Substring(0, 1) + "_" + FacultyEducation.courseStudied;
        //                        Faculty.PGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
        //                            fileName, ext));
        //                        FacultyEducation.certificate = string.Format("{0}{1}", fileName, ext);
        //                    }
        //                    FacultyEducation.isActive = true;
        //                    FacultyEducation.createdOn = DateTime.Now;
        //                    FacultyEducation.createdBy = UserId;
        //                    FacultyEducation.updatedOn = null;
        //                    FacultyEducation.updatedBy = null;
        //                    db.jntuh_registered_faculty_education.Add(FacultyEducation);
        //                    db.SaveChanges();
        //                }

        //            }
        //            else if (i == 3)
        //            {

        //                if (Faculty.MPhilcertificate != null)
        //                {
        //                    FacultyEducation.educationId = 5;
        //                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
        //                    {
        //                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
        //                    }

        //                    var ext = Path.GetExtension(Faculty.MPhilcertificate.FileName);

        //                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //                    {
        //                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                          RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
        //                                          RegisteredFaculty.LastName.Substring(0, 1) + "_" + FacultyEducation.courseStudied;
        //                        Faculty.MPhilcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
        //                            fileName, ext));
        //                        FacultyEducation.certificate = string.Format("{0}{1}", fileName, ext);
        //                    }
        //                    FacultyEducation.isActive = true;
        //                    FacultyEducation.createdOn = DateTime.Now;
        //                    FacultyEducation.createdBy = UserId;
        //                    FacultyEducation.updatedOn = null;
        //                    FacultyEducation.updatedBy = null;
        //                    db.jntuh_registered_faculty_education.Add(FacultyEducation);
        //                    db.SaveChanges();
        //                }

        //            }
        //            else if (i == 4)
        //            {

        //                if (Faculty.PhDcertificate != null)
        //                {
        //                    FacultyEducation.educationId = 6;
        //                    if (!Directory.Exists(Server.MapPath(certificatesPath)))
        //                    {
        //                        Directory.CreateDirectory(Server.MapPath(certificatesPath));
        //                    }

        //                    var ext = Path.GetExtension(Faculty.PhDcertificate.FileName);

        //                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
        //                    {
        //                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
        //                                          RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
        //                                          RegisteredFaculty.LastName.Substring(0, 1) + "_" + FacultyEducation.courseStudied;
        //                        Faculty.PhDcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
        //                            fileName, ext));
        //                        FacultyEducation.certificate = string.Format("{0}{1}", fileName, ext);
        //                    }
        //                    FacultyEducation.isActive = true;
        //                    FacultyEducation.createdOn = DateTime.Now;
        //                    FacultyEducation.createdBy = UserId;
        //                    FacultyEducation.updatedOn = null;
        //                    FacultyEducation.updatedBy = null;
        //                    db.jntuh_registered_faculty_education.Add(FacultyEducation);
        //                    db.SaveChanges();
        //                }

        //            }


        //        }

        //        TempData["Success"] = Faculty.RegistrationNumber + " - Faculty Education Details are Saved Successfully";
        //        return RedirectToAction("Index", new { RegistrationNumber = Faculty.RegistrationNumber });
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Invallid RegistrationNumber";
        //        return RedirectToAction("Index");
        //    }
        //}

        [Authorize(Roles = "Admin")]
        public ActionResult EditFacultyCertificates(Faculty Faculty)
        {
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string certificatesPath = "~/Content/Upload/Faculty/CERTIFICATES";
            string panCardsPath = "~/Content/Upload/Faculty/PANCARDS";
            string aadhaarCardsPath = "~/Content/Upload/Faculty/AADHAARCARDS";

            if (Faculty.RegistrationNumber != null)
            {
                var RegisteredFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber.Trim() == Faculty.RegistrationNumber.Trim());
                if (RegisteredFaculty == null)
                {
                    TempData["Error"] = "Invalid RegistrationNumber";
                    return RedirectToAction("Index");
                }

                //Aadhaar Number and Document Saving....
                RegisteredFaculty.AadhaarNumber = Faculty.AadharNumber;
                if (Faculty.EditAadharDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(aadhaarCardsPath)))
                        Directory.CreateDirectory(Server.MapPath(aadhaarCardsPath));

                    var ext = Path.GetExtension(Faculty.EditAadharDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            RegisteredFaculty.FirstName.Substring(0, 1) + "-" + RegisteredFaculty.LastName.Substring(0, 1);
                        Faculty.EditAadharDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(aadhaarCardsPath),
                            fileName, ext));
                        RegisteredFaculty.AadhaarDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }

                //Pan Number and Document Saving....
                RegisteredFaculty.PANNumber = Faculty.PANNumber;
                if (Faculty.EditPANDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(panCardsPath)))
                        Directory.CreateDirectory(Server.MapPath(panCardsPath));

                    var ext = Path.GetExtension(Faculty.EditPANDocument.FileName);
                    if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                    {
                        string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                            RegisteredFaculty.FirstName.Substring(0, 1) + "-" + RegisteredFaculty.LastName.Substring(0, 1);
                        Faculty.EditPANDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(panCardsPath),
                            fileName, ext));
                        RegisteredFaculty.PANDocument = string.Format("{0}{1}", fileName, ext);
                    }
                }

                db.Entry(RegisteredFaculty).State = EntityState.Modified;
                db.SaveChanges();

                //Education Details Saving.....
                var registeredFacultyEducation = db.jntuh_registered_faculty_education.Where(e => e.facultyId == RegisteredFaculty.id).Select(e => e).ToList();
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        jntuh_registered_faculty_education edu = registeredFacultyEducation.Where(e => e.educationId == 1).Select(e => e).FirstOrDefault();
                        if (edu != null)
                        {
                            if (Faculty.SSCcertificate != null)
                            {
                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.SSCcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + edu.courseStudied.Trim();
                                    Faculty.SSCcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    edu.certificate = string.Format("{0}{1}", fileName, ext);
                                }
                                db.Entry(edu).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            if (Faculty.SSCcertificate != null)
                            {
                                education.facultyId = RegisteredFaculty.id;
                                education.educationId = 1;
                                education.hallticketnumber = Faculty.Education[i].hallticketno;
                                education.courseStudied = Faculty.Education[i].CouseStudied;
                                education.specialization = Faculty.Education[i].Specialization;
                                education.passedYear = Faculty.Education[i].PassedYear;
                                education.marksPercentage = Faculty.Education[i].MarksPercentage;
                                education.division = Faculty.Education[i].division;
                                education.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
                                education.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.SSCcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + education.courseStudied.Trim();
                                    Faculty.SSCcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    education.certificate = string.Format("{0}{1}", fileName, ext);
                                }

                                education.isActive = true;
                                education.createdOn = DateTime.Now;
                                education.createdBy = UserId;
                                education.updatedOn = null;
                                education.updatedBy = null;

                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (i == 1)
                    {
                        jntuh_registered_faculty_education edu = registeredFacultyEducation.Where(e => e.educationId == 3).Select(e => e).FirstOrDefault();
                        if (edu != null)
                        {
                            if (Faculty.UGcertificate != null)
                            {
                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.UGcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + edu.courseStudied.Trim();
                                    Faculty.UGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    edu.certificate = string.Format("{0}{1}", fileName, ext);
                                }
                                edu.updatedBy = UserId;
                                edu.updatedOn = DateTime.Now;
                                db.Entry(edu).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            if (Faculty.UGcertificate != null)
                            {
                                education.facultyId = RegisteredFaculty.id;
                                education.educationId = 3;
                                education.hallticketnumber = Faculty.Education[i].hallticketno;
                                education.courseStudied = Faculty.Education[i].CouseStudied;
                                education.specialization = Faculty.Education[i].Specialization;
                                education.passedYear = Faculty.Education[i].PassedYear;
                                education.marksPercentage = Faculty.Education[i].MarksPercentage;
                                education.division = Faculty.Education[i].division;
                                education.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
                                education.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.UGcertificate.FileName);

                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + education.courseStudied.Trim();
                                    Faculty.UGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    education.certificate = string.Format("{0}{1}", fileName, ext);
                                }

                                education.isActive = true;
                                education.createdOn = DateTime.Now;
                                education.createdBy = UserId;
                                education.updatedOn = null;
                                education.updatedBy = null;

                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (i == 2)
                    {
                        jntuh_registered_faculty_education edu = registeredFacultyEducation.Where(e => e.educationId == 4).Select(e => e).FirstOrDefault();
                        if (edu != null)
                        {
                            if (Faculty.PGcertificate != null)
                            {
                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.PGcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + edu.courseStudied.Trim();
                                    Faculty.PGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    edu.certificate = string.Format("{0}{1}", fileName, ext);
                                }
                                edu.updatedBy = UserId;
                                edu.updatedOn = DateTime.Now;
                                db.Entry(edu).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            if (Faculty.PGcertificate != null)
                            {
                                education.educationId = 4;
                                education.facultyId = RegisteredFaculty.id;
                                education.hallticketnumber = Faculty.Education[i].hallticketno;
                                education.courseStudied = Faculty.Education[i].CouseStudied;
                                education.specialization = Faculty.Education[i].Specialization;
                                education.passedYear = Faculty.Education[i].PassedYear;
                                education.marksPercentage = Faculty.Education[i].MarksPercentage;
                                education.division = Faculty.Education[i].division;
                                education.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
                                education.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.PGcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + education.courseStudied.Trim();
                                    Faculty.PGcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    education.certificate = string.Format("{0}{1}", fileName, ext);
                                }

                                education.isActive = true;
                                education.createdOn = DateTime.Now;
                                education.createdBy = UserId;
                                education.updatedOn = null;
                                education.updatedBy = null;
                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (i == 3)
                    {
                        jntuh_registered_faculty_education edu = registeredFacultyEducation.Where(e => e.educationId == 5).Select(e => e).FirstOrDefault();
                        if (edu != null)
                        {
                            if (Faculty.MPhilcertificate != null)
                            {
                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.MPhilcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + edu.courseStudied.Trim();
                                    Faculty.MPhilcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    edu.certificate = string.Format("{0}{1}", fileName, ext);
                                }
                                edu.updatedBy = UserId;
                                edu.updatedOn = DateTime.Now;
                                db.Entry(edu).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            if (Faculty.MPhilcertificate != null)
                            {
                                education.educationId = 5;
                                education.hallticketnumber = Faculty.Education[i].hallticketno;
                                education.facultyId = RegisteredFaculty.id;
                                education.courseStudied = Faculty.Education[i].CouseStudied;
                                education.specialization = Faculty.Education[i].Specialization;
                                education.passedYear = Faculty.Education[i].PassedYear;
                                education.marksPercentage = Faculty.Education[i].MarksPercentage;
                                education.division = Faculty.Education[i].division;
                                education.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
                                education.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.MPhilcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + education.courseStudied.Trim();
                                    Faculty.MPhilcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    education.certificate = string.Format("{0}{1}", fileName, ext);
                                }

                                education.isActive = true;
                                education.createdOn = DateTime.Now;
                                education.createdBy = UserId;
                                education.updatedOn = null;
                                education.updatedBy = null;
                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (i == 4)
                    {
                        jntuh_registered_faculty_education edu = registeredFacultyEducation.Where(e => e.educationId == 6).Select(e => e).FirstOrDefault();
                        if (edu != null)
                        {
                            if (Faculty.PhDcertificate != null)
                            {
                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.PhDcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + edu.courseStudied.Trim();
                                    Faculty.PhDcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    edu.certificate = string.Format("{0}{1}", fileName, ext);
                                }
                                edu.updatedBy = UserId;
                                edu.updatedOn = DateTime.Now;
                                db.Entry(edu).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            if (Faculty.PhDcertificate != null)
                            {
                                education.educationId = 6;
                                education.facultyId = RegisteredFaculty.id;
                                education.hallticketnumber = Faculty.Education[i].hallticketno;
                                education.courseStudied = Faculty.Education[i].CouseStudied;
                                education.specialization = Faculty.Education[i].Specialization;
                                education.passedYear = Faculty.Education[i].PassedYear;
                                education.marksPercentage = Faculty.Education[i].MarksPercentage;
                                education.division = Faculty.Education[i].division;
                                education.boardOrUniversity = Faculty.Education[i].BoardOfUnivercity;
                                education.placeOfEducation = Faculty.Education[i].PlaceOfEducation;

                                if (!Directory.Exists(Server.MapPath(certificatesPath)))
                                    Directory.CreateDirectory(Server.MapPath(certificatesPath));

                                var ext = Path.GetExtension(Faculty.PhDcertificate.FileName);
                                if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                                {
                                    string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" +
                                                      RegisteredFaculty.FirstName.Substring(0, 1) + "-" +
                                                      RegisteredFaculty.LastName.Substring(0, 1) + "_" + education.courseStudied.Trim();
                                    Faculty.PhDcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(certificatesPath),
                                        fileName, ext));
                                    education.certificate = string.Format("{0}{1}", fileName, ext);
                                }

                                education.isActive = true;
                                education.createdOn = DateTime.Now;
                                education.createdBy = UserId;
                                education.updatedOn = null;
                                education.updatedBy = null;
                                db.jntuh_registered_faculty_education.Add(education);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                TempData["Success"] = Faculty.RegistrationNumber + " - Faculty Education Details are Updated Successfully";
                return RedirectToAction("Index", new { RegistrationNumber = Faculty.RegistrationNumber });
            }
            else
            {
                TempData["Error"] = "Invallid RegistrationNumber";
                return RedirectToAction("Index");
            }
        }

        public class Faculty
        {
            [Display(Name = "Faculty Registration ID")]
            public string RegistrationNumber { get; set; }
            public string FullName { get; set; }
            public int educationId { get; set; }
            public HttpPostedFileBase SSCcertificate { get; set; }
            public string FacultySSCcertificate { get; set; }
            public HttpPostedFileBase UGcertificate { get; set; }
            public string FacultyUGcertificate { get; set; }
            public HttpPostedFileBase PGcertificate { get; set; }
            public string FacultyPGcertificate { get; set; }
            public HttpPostedFileBase MPhilcertificate { get; set; }
            public string FacultyMphilcertificate { get; set; }
            public HttpPostedFileBase PhDcertificate { get; set; }
            public string FacultyPhDcertificate { get; set; }
            public bool isActive { get; set; }
            public DateTime? CreatedOn { get; set; }
            public int? CreatedBy { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public int? UpdatedBy { get; set; }

            [StringLength(16, ErrorMessage = "Must be 12 characters")]
            //[RegularExpression(@"\d{12}", ErrorMessage = "Invalid AADHAAR number")]
            [Display(Name = "Aadhaar Number")]
            public string AadharNumber { get; set; }

            [Display(Name = "Aadhaar Card Document")]
            public string AadharDocument { get; set; }

            public HttpPostedFileBase EditAadharDocument { get; set; }

            // [Required(ErrorMessage = "*")]
            // [RegularExpression(@"[A-Z]{3}[P][A-Z]{1}\d{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number")]
            [Display(Name = "PAN Number")]
            public string PANNumber { get; set; }

            [Display(Name = "PAN Card Document")]
            public string PANDocument { get; set; }

            public HttpPostedFileBase EditPANDocument { get; set; }

            public List<FacultyEducationClass> Education { get; set; }

        }

        public class FacultyEducationClass
        {
            public string hallticketno { get; set; }
            public string CouseStudied { get; set; }
            public string Specialization { get; set; }
            public int PassedYear { get; set; }
            public decimal? MarksPercentage { get; set; }
            public int? division { get; set; }
            public string BoardOfUnivercity { get; set; }
            public string PlaceOfEducation { get; set; }

        }
    }
}
