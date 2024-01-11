using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using UAAAS.Controllers.College;
using UAAAS.Controllers.Reports;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{

  
    public class CollegeSCMProceedingsRequestForAdminController : BaseController
    {

     
        //Asking SCM Faculty Details Print 
        private uaaasDBContext db = new uaaasDBContext();
       SCMReportsController scmreport=new SCMReportsController();
        private string serverURL;

         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpGet]
        public ActionResult CollegeScmProceedingsRequest(int? CollegeId)
        {
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true ).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            ScmProceedingsRequest scmProceedings = new ScmProceedingsRequest();
            scmProceedings.IsEdit = false;
            string clgCode;
            if (CollegeId != null)
            {

              //  var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
               // var userCollegeId =db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
                var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == CollegeId);
                var specs = new List<DistinctSpecializations>();
                var depts = new List<DistinctDepartments>();
                var degrees = db.jntuh_degree.AsNoTracking().ToList();
                var specializations = db.jntuh_specialization.AsNoTracking().ToList();
                var departments = db.jntuh_department.AsNoTracking().ToList();
                //int[] collegespecs = new int[];
                List<int> collegespecs = new List<int>();
                collegespecs.AddRange(
                    db.jntuh_college_intake_existing.Where(i => i.collegeId == CollegeId)
                        .Select(i => i.specializationId)
                        .Distinct()
                        .ToArray());

                //int[] degreeIds=(from a in db.jntuh_specialization join b in db.jntuh_department on a.departmentId equals b.id
                //               join c in db.jntuh_degree on b.degreeId equals c.id where collegespecs.Contains(a.id) select c.id).Distinct().ToArray();
                //if (degreeIds.Contains(4))
                //{
                //   var humanitesSpeci = new[] {37,48,42,31};
                //   collegespecs.AddRange(humanitesSpeci);
                //}



                foreach (var s in collegespecs)
                {
                    var specid = specializations.FirstOrDefault(i => i.id == s);

                    if (specid != null)
                    {
                        var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                        var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                        if (degree != null)
                            specs.Add(new DistinctSpecializations
                            {
                                SpecializationId = specid.id,
                                SpecializationName = degree.degree + " - " + specid.specializationName,
                                DepartmentId = specid.departmentId
                            });
                    }
                }


                //  if(specs.Contains())

                ViewBag.departments = specs.OrderBy(i => i.DepartmentId);

                var collegescmrequestslist =
                    db.jntuh_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == CollegeId).ToList();

                var proceedingsRequests = new List<ScmProceedingsRequest>();
                scmProceedings.ScmProceedingsRequestslist = new List<ScmProceedingsRequest>();
                foreach (var s in collegescmrequestslist)
                {

                    //string cretedDate = string.Empty;
                    //if (!string.IsNullOrEmpty(s.CreatedOn.ToString(CultureInfo.InvariantCulture)))
                    //{
                    //    cretedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(cretedDate);
                    //}
                    var specid = specializations.FirstOrDefault(i => i.id == s.SpecializationId);

                    if (specid != null && firstOrDefault != null)
                    {
                        var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                        var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                        if (degree != null)
                            // if (s.RequiredProfessor != null)
                            proceedingsRequests.Add(new ScmProceedingsRequest
                            {
                                CollegeName = firstOrDefault.collegeCode + " - " + firstOrDefault.collegeName,
                                CollegeCode = firstOrDefault.collegeCode,
                                ProfessorVacancies = s.ProfessorsCount.ToString(),
                                AssociateProfessorVacancies = s.AssociateProfessorsCount.ToString(),
                                AssistantProfessorVacancies = s.AssistantProfessorsCount.ToString(),
                                SpecializationName = degree.degree + " - " + specid.specializationName,
                                SpecializationId = specid.id,
                                CollegeId = firstOrDefault.id,
                                DepartmentId = specid.departmentId,
                                ScmNotificationpath = s.SCMNotification,
                                Id = s.ID,
                                CreatedDate = s.CreatedOn,
                                RequiredProfessorVacancies = s.RequiredProfessor.ToString(),
                                RequiredAssistantProfessorVacancies = s.RequiredAssistantProfessor.ToString(),
                                RequiredAssociateProfessorVacancies = s.RequiredAssociateProfessor.ToString(),
                                Checked = false
                            });
                    }
                }
                // ViewBag.collegescmrequestslist = proceedingsRequests;
                scmProceedings.ScmProceedingsRequestslist.AddRange(proceedingsRequests.OrderByDescending(e=>e.CreatedDate).Select(e=>e).ToList());
                ViewBag.collegescmrequestslist = scmProceedings.ScmProceedingsRequestslist;
                // scmProceedings.ScmProceedingsRequestslist.AddRange(proceedingsRequests);
                scmProceedings.IsEdit = true;
            }
            return View(scmProceedings);
            
        }

         [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        [HttpPost]
        public ActionResult CollegeScmProceedingsRequest(ScmProceedingsRequest scmrequest)
        {
           // var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
          //  var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();


            if (ModelState.IsValid)
            {
                var fileName = string.Empty;
                var filepath = string.Empty;
                var collegescmrequests = new jntuh_scmproceedingsrequests();
                const string scmnotificationpath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/ScmNotificationDocuments";
                //if (scmrequest.ScmNotificationSupportDoc != null)
               // {
                    //if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                    //{
                    //    Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                    //}

                    //var ext = Path.GetExtension(scmrequest.ScmNotificationSupportDoc.FileName);
                    //if (ext != null && ext.ToUpper().Equals(".PDF"))
                    //{
                    //    var scmfileName =db.jntuh_college.Where(c => c.id == scmrequest.CollegeId).Select(c => c.id).FirstOrDefault() + "_" + "ScmNotofication" + "_" +
                    //        DateTime.Now.ToString("yyyMMddHHmmss");
                    //    scmrequest.ScmNotificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}",Server.MapPath(scmnotificationpath), scmfileName, ext));
                    //    collegescmrequests.SCMNotification = scmfileName + ext;
                    //}
                    IUserMailer mailer = new UserMailer();
                    collegescmrequests.CollegeId = scmrequest.CollegeId;
                    collegescmrequests.SpecializationId = scmrequest.SpecializationId;
                    var specialization =db.jntuh_specialization.AsNoTracking().FirstOrDefault(i => i.id == scmrequest.SpecializationId);
                    var department =db.jntuh_department.AsNoTracking().FirstOrDefault(i => i.id == specialization.departmentId);
                    collegescmrequests.DEpartmentId = specialization != null ? specialization.departmentId : 0;
                    collegescmrequests.DegreeId = department != null ? department.degreeId : 0;
                    collegescmrequests.ProfessorsCount = Convert.ToInt16(scmrequest.ProfessorVacancies);
                    collegescmrequests.AssociateProfessorsCount = Convert.ToInt16(scmrequest.AssociateProfessorVacancies);
                    collegescmrequests.AssistantProfessorsCount = Convert.ToInt16(scmrequest.AssistantProfessorVacancies);
                    collegescmrequests.RequiredProfessor = Convert.ToInt16(scmrequest.RequiredProfessorVacancies);
                    collegescmrequests.RequiredAssistantProfessor = Convert.ToInt16(scmrequest.RequiredAssistantProfessorVacancies);
                    collegescmrequests.RequiredAssociateProfessor = Convert.ToInt16(scmrequest.RequiredAssociateProfessorVacancies);
                    if (scmrequest.NotificationDate != null)
                        collegescmrequests.Notificationdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmrequest.NotificationDate);
                    collegescmrequests.Remarks = scmrequest.Remarks;
                    collegescmrequests.SCMNotification ="Admin";
                    collegescmrequests.CreatedBy = 1;
                    collegescmrequests.CreatedOn = DateTime.Now;
                    collegescmrequests.ISActive = true;
                    db.jntuh_scmproceedingsrequests.Add(collegescmrequests);
                    try
                    {
                        db.SaveChanges();

                        //var attachments = new List<Attachment>();
                        //if (scmrequest.ScmNotificationSupportDoc != null)
                        //{

                        //    fileName = Path.GetFileName(scmrequest.ScmNotificationSupportDoc.FileName);
                        //    filepath = Path.Combine(Server.MapPath("~/Content/Attachments/SCMAttachments"), fileName);
                        //    scmrequest.ScmNotificationSupportDoc.SaveAs(filepath);
                        //    attachments.Add(new Attachment(filepath));
                        //    mailer.SendAttachmentToAllColleges("nageswararao.d623@gmail.com", "", "",
                        //        "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                        //    TempData["Success"] = "Your request has been proccessed successfully..";
                        //}
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                    validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                    }
                //}
                //else
                //{
                //    TempData["Error"] = "Please Fill All Mandatory Fields..";
                //}
            }


            return RedirectToAction("CollegeScmProceedingsRequest", new { CollegeId = scmrequest.CollegeId });
        }


         
        [HttpGet]
        public ActionResult AddRegistrationNumber(int id)
        {
            ScmProceedingsRequestAddReg scmDetails = new ScmProceedingsRequestAddReg();
            scmDetails = (from a in db.jntuh_scmproceedingsrequests
                          join b in db.jntuh_college on a.CollegeId equals b.id into abdata
                          from ab in abdata.DefaultIfEmpty()
                          join c in db.jntuh_specialization on a.SpecializationId equals c.id into abcdata
                          from abc in abcdata.DefaultIfEmpty()
                          join d in db.jntuh_department on a.DEpartmentId equals d.id into abcddata
                          from abcd in abcdata.DefaultIfEmpty()
                          join e in db.jntuh_degree on a.DegreeId equals e.id into abcdedata
                          from abcde in abcdedata.DefaultIfEmpty()
                          where a.ID == id
                          select new ScmProceedingsRequestAddReg
                          {
                              CollegeCode = ab.collegeCode,
                              CollegeName = ab.collegeName,
                              CollegeId = ab.id,
                              SpecializationId = abc.id,
                              SpecializationName = abc.specializationName,
                              DepartmentId = abcd.id,
                              DepartmentName = abc.jntuh_department.departmentName,
                              DegreeId = abcde.id,
                              DegreeName = abcde.degree,
                              Professors = (int)a.ProfessorsCount,
                              AssociateProfessors = (int)a.AssociateProfessorsCount,
                              AssistantProfessors = (int)a.AssistantProfessorsCount,
                              Id = a.ID
                          }).FirstOrDefault();

            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            jntuh_college.Add(new jntuh_college() { id = 0, collegeCode = "Not Working" });
            ViewBag.Colleges = jntuh_college.Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeId).ToList();


            ViewBag.Designations = db.jntuh_designation.Where(e => e.isActive == true && (e.id == 1 || e.id == 2 || e.id == 3)).Select(e => new { Id = e.id, Name = e.designation }).OrderBy(e => e.Id).ToList();


            return PartialView("_AddRegistrationNumber", scmDetails);
        }


        
        [HttpPost]
        public ActionResult AddRegistrationNumber(ScmProceedingsRequestAddReg reg)
        {
            TempData["Error"] = null;
           // int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //  int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (reg != null)
                {
                    jntuh_scmproceedingrequest_addfaculty addfaculty = new jntuh_scmproceedingrequest_addfaculty();
                    addfaculty.ScmProceedingId = reg.Id;
                    addfaculty.RegistrationNumber = reg.RegistrationNo;
                    addfaculty.FacultyType = reg.FacultyId;
                    addfaculty.PreviousCollegeId = reg.PreviousCollegeId.ToString();
                    addfaculty.Createdby = 1;
                    addfaculty.CreatedOn = DateTime.Now;
                    addfaculty.Isactive = true;
                    db.jntuh_scmproceedingrequest_addfaculty.Add(addfaculty);
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Add Successfully";
                    return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequestForAdmin", new { CollegeId =reg.CollegeId});
                }
            }
            return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequestForAdmin");
        }

        [HttpPost]
        public JsonResult CheckRegistrationNumber(string RegistrationNo)
        {
            string CheckingReg = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber.Trim() == RegistrationNo.Trim()).Select(f => f.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(CheckingReg))
            {
                if (CheckingReg.Trim() == RegistrationNo.Trim())
                    return Json(true);
                else
                    return Json("This Registration Number doesn't Exist", JsonRequestBehavior.AllowGet);
            }
            else
                return Json("This Registration Number doesn't Exist", JsonRequestBehavior.AllowGet);

        }

        public ActionResult ViewFaculty(int scmid)
        {
            if (scmid != 0)
            {
                List<ScmProceedingsRequestAddReg> addFacultyDetails = new List<ScmProceedingsRequestAddReg>();

                addFacultyDetails = (from a in db.jntuh_scmproceedingsrequests
                                     join b in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId into abdata
                                     from ab in abdata.DefaultIfEmpty()
                                     join c in db.jntuh_registered_faculty on ab.RegistrationNumber.Trim() equals c.RegistrationNumber.Trim() into abcdata
                                     from abc in abcdata.DefaultIfEmpty()
                                     join d in db.jntuh_specialization on a.SpecializationId equals d.id into abcddata
                                     from abcd in abcddata.DefaultIfEmpty()
                                     join e in db.jntuh_degree on a.DegreeId equals e.id into abcdedata
                                     from abcde in abcdedata.DefaultIfEmpty()
                                     where ab.ScmProceedingId == scmid
                                     select new ScmProceedingsRequestAddReg
                                     {
                                         Id = ab.Id,
                                         SpecializationId = a.SpecializationId,
                                         SpecializationName = abcde.degree + "-" + abcd.specializationName,
                                         Regno = abc.RegistrationNumber,
                                         RegName = abc.FirstName + " " + abc.LastName,
                                         ScmId = ab.ScmProceedingId,
                                         FacultyId = abc.id

                                     }).ToList();
                return View(addFacultyDetails);
            }
            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        
       [HttpGet]
        public ActionResult AddAuditors(int SCMId, int DeptId)
       {
           if (SCMId != 0 && DeptId != 0)
           {
               //
               var aditors = (from Adts in db.jntuh_ffc_auditor
                 join Desg in db.jntuh_designation on Adts.auditorDesignationID equals Desg.id
                              where Adts.auditorDepartmentID == DeptId
                   select new AditorsDetails()
                   {
                       SCMRequestId = SCMId,
                       AditorId = Adts.id,
                       AditorName = Adts.auditorName,
                       Designation = Desg.designation,
                       DesignationId = Desg.id,
                       DepartmentId = Adts.auditorDepartmentID??0,
                       Checke = false
                   }).Distinct().ToList();
               

               return PartialView("_AddAuditors",aditors);
           }
           return RedirectToAction("SCMRequestsList", "CollegeSCMProceedingsRequest");
       }

       [HttpPost]
       public ActionResult AddAuditors(List<AditorsDetails> aditorsdata)
       {
          int  userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
           if (aditorsdata.Count() > 0)
           {
               bool status=false;
               var attachments = new List<Attachment>();
               IUserMailer mailer = new UserMailer();
               var details = aditorsdata.Where(e => e.Checke == true).Select(e => e).ToList();
               if (details.Count() > 0)
               {
                   var aditorsIds = details.Select(e => e.AditorId).ToArray();
                   int SCMId = (int)details[0].SCMRequestId;
                   var scmdata =db.jntuh_scmproceedingsrequests.Where(e => e.ID == SCMId).Select(e => e).FirstOrDefault();
                   if (scmdata != null)
                   {
                       var filepath = SaveSCMReportPdfDeptWise(0,scmdata.CollegeId, scmdata.SpecializationId);
                       filepath = filepath.Replace("/", "\\");
                       attachments.Add(new Attachment(filepath));

                       List<jntuh_ffc_auditor> aditorslist = db.jntuh_ffc_auditor.Where(e => e.isActive == true && aditorsIds.Contains(e.id)).Select(e => e).ToList();

                       foreach (var item in aditorslist)
                       {
                           if (item != null)
                           {
                               mailer.SendAttachmentToAllColleges(item.auditorEmail1, "", "",
                                  "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                               status = UAAAS.Models.Utilities.SendSMS(item.auditorMobile1, "Your Nominee Details Send to Your mail");
                               jntuh_auditor_assigned auditors=new jntuh_auditor_assigned();
                               auditors.AuditorId = item.id;
                               auditors.ScmId = scmdata.ID;
                               auditors.IsActive = true;
                               auditors.CreatedBy = userID;
                               auditors.CreatedOn = DateTime.Now;
                               db.jntuh_auditor_assigned.Add(auditors);
                           }
                       }
                       db.SaveChanges();
                      if(status==true)
                       TempData["Success"] = "Data Saved Successfully.....!";
                      else
                          TempData["Error"] = "SMS Sending Failed......";
                       return RedirectToAction("CollegeScmProceedingsRequestView", "CollegeSCMProceedingsRequest", new { id = UAAAS.Models.Utilities.EncryptString(scmdata.CollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                   }
               }
           }
         return RedirectToAction("SCMRequestsList", "CollegeSCMProceedingsRequest");
       }


        #region SCM Download Only
       public string SaveSCMReportPdfDeptWise(int preview, int collegeId, int specializationId)
       {
           string fullPath = string.Empty;
           var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
           var speci = db.jntuh_specialization.Where(e => e.id == specializationId).Select(e => e).FirstOrDefault();
           var Dept = "";
           if (speci != null)
           {
               Dept = speci.jntuh_department.jntuh_degree.degree + "-" + speci.specializationName;
           }
           //Set page size as A4
           Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
           string path = Server.MapPath("~/Content/PDFReports/SCMReportDownload");
           if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMReportDownload")))
           {
               Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMReportDownload"));
           }

           if (preview == 0)
           {
               fullPath = path + "/" + collegedata.collegeCode + "- SCM Report Download -" + Dept + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
               PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
               ITextEvents iTextEvents = new ITextEvents();
               iTextEvents.CollegeCode = collegedata.collegeCode;
               iTextEvents.CollegeName = collegedata.collegeName;
               iTextEvents.formType = "Scm Report Download" + Dept;
               pdfWriter.PageEvent = iTextEvents;
           }
           //Open PDF Document to write data
           pdfDoc.Open();

           //Assign Html content in a string to write in PDF
           string contents = string.Empty;

           StreamReader sr;

           //Read file from server path
           sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMReportDownloadDeptWise.html"));
           //store content in the variable
           contents = sr.ReadToEnd();
           sr.Close();

           //  contents = contents.Replace("##SERVERURL##", serverURL);


           contents = GetScmReportDataDeptWise(contents, collegeId, specializationId);

           //Read string contents using stream reader and convert html to parsed conent
           var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
           //Get each array values from parsed elements and add to the PDF document
           bool pageRotated = false;
           int count = 0;
           foreach (var htmlElement in parsedHtmlElements)
           {
               count++;
               if (count == 100)
               {

               }
               if (htmlElement.Equals("<textarea>"))
               {
                   pdfDoc.NewPage();
               }

               if (htmlElement.Chunks.Count >= 3)
               {
                   if (htmlElement.Chunks.Count == 4)
                   {
                       pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                       pdfDoc.SetMargins(60, 50, 60, 60);
                       pageRotated = true;
                   }
                   else
                   {
                       if (pageRotated == true)
                       {
                           pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                           pdfDoc.SetMargins(60, 50, 60, 60);
                           pageRotated = false;
                       }
                   }

                   pdfDoc.NewPage();

               }
               else
               {
                   pdfDoc.Add(htmlElement as IElement);
               }
           }

           //Close your PDF
           pdfDoc.Close();
           if (pdfDoc.IsOpen())
           {
               pdfDoc.Close();
           }

           string returnPath = string.Empty;
           returnPath = fullPath;
           return returnPath;
       }


       public string GetScmReportDataDeptWise(string contents, int collegeId, int specializationId)
       {
           string contentdata = string.Empty;

           var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking().ToList();
           var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
           var collegedata = jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
           var scmregdata = (from scmreq in db.jntuh_scmproceedingsrequests
                             join scmaddfaculty in db.jntuh_scmproceedingrequest_addfaculty on scmreq.ID equals scmaddfaculty.ScmProceedingId
                             join regfaculty in db.jntuh_registered_faculty on scmaddfaculty.RegistrationNumber equals regfaculty.RegistrationNumber
                             join speci in db.jntuh_specialization on scmreq.SpecializationId equals speci.id
                             join deg in db.jntuh_degree on scmreq.DegreeId equals deg.id
                             join dept in db.jntuh_department on scmreq.DEpartmentId equals dept.id
                             where scmreq.CollegeId == collegeId && scmreq.SpecializationId == specializationId
                             select new
                             {
                                 FacultyName = regfaculty.FirstName + " " + regfaculty.LastName,
                                 RegNo = regfaculty.RegistrationNumber,
                                 PANNo = regfaculty.PANNumber,
                                 Experience = regfaculty.TotalExperience,
                                 facultyId = (int?)regfaculty.id,
                                 Branch = deg.degree + "-" + speci.specializationName,
                                 PreviousCollege = scmaddfaculty.PreviousCollegeId,
                                 Remarks=scmaddfaculty.DeactiviationReason
                             }).ToList();


           scmregdata = scmregdata.Where(e => e.RegNo != null && e.facultyId != null).Select(e => e).ToList();




           if (collegedata != null)
           {
               contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
               contentdata += "<tr><td style='text-align:left' width='90%'><b>College Name : </b>" + collegedata.collegeName + "</td>";
               contentdata += "<td style='text-align:left' width='20%'><b>Code : </b>" + collegedata.collegeCode + "</td></tr>";
               contentdata += "</table>";
           }
           contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
           contentdata += "<tr><td style='text-align:left' width='70%'><b>Proceeding No : </b></td>";
           contentdata += "<td style='text-align:left' width='30%'><b>Date : </b></td></tr>";
           contentdata += "</table>";

           contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
           contentdata += "<tr><td style='text-align:center'><b>Affiliated to</b></td></tr>";
           contentdata += "<tr><td style='text-align:center'><b>(JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD KUKATPALLY, HYDERABAD)</b></td></tr>";
           contentdata += "</table>";

           contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
           contentdata += "<tr><td style='text-align:left'>Minutes of the Selection Committee Meeting held on--------------------- at ------------------------</td></tr></table>";


           contentdata += "<br/><table border='0'cellspacing='0' cellpadding='4' width='100%'>";
           contentdata += "<tr><td style='text-align:left' width='30%'><b>Post</b></td><td  width='5%'>:</td><td  style='text-align:left'></td></tr>";
           if (scmregdata.Count() != 0)
           {
               contentdata += "<tr><td style='text-align:left' width='30%'><b>Department</b></td><td width='5%'>:</td><td style='text-align:left'>" + scmregdata[0].Branch + "</td></tr>";
           }
           else
           {
               contentdata += "<tr><td style='text-align:left' width='30%'><b>Department</b></td><td width='5%'>:</td><td style='text-align:left'></td></tr>";
           }
           contentdata += "<tr><td style='text-align:left' width='30%'><b>Scale of Pay</b></td><td width='5%'>:</td><td style='text-align:left'></td></tr>";
           contentdata += "</table><br/>";

           contentdata += "<table border='1'cellspacing='0' cellpadding='4' width='100%'>";
           contentdata += "<tr><td style='text-align:left' width='8%'><b>Sno</b></td>";
           contentdata += "<td style='text-align:left' width='22%'><b>Faculty Name</b></td>";
           contentdata += "<td style='text-align:left' width='20%'><b>Reg. Number</b></td>";
           contentdata += "<td style='text-align:left' width='10%'><b>PAN Number</b></td>";
           contentdata += "<td style='text-align:left' width='10%'><b>UG Branch</b></td>";
           contentdata += "<td style='text-align:left' width='10%'><b>PG Branch</b></td>";
           contentdata += "<td style='text-align:left' width='10%'><b>Ph.D</b></td>";
           //contentdata += "<td style='text-align:left' width='10%'><b>Experience</b></td>";
           //contentdata += "<td style='text-align:left' width='10%'><b>Previous College</b></td>";
           contentdata += "<td style='text-align:left' width='10%'><b>Remarks</b></td>";
           contentdata += "</tr>";
           if (scmregdata.Count() != 0)
           {
               for (int i = 0; i < scmregdata.Count(); i++)
               {

                   string ugBranch = string.Empty;
                   string pgBranch = string.Empty;
                   string phdBranch = string.Empty;


                   if (scmregdata[i].facultyId != null)
                   {
                       var ugdata = jntuh_registered_faculty_education.Where(e => e.educationId == 3 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                       if (ugdata != null)
                           ugBranch = ugdata;

                       var pgdata = jntuh_registered_faculty_education.Where(e => e.educationId == 4 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                       if (pgdata != null)
                           pgBranch = pgdata;

                       var phddata = jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                       if (phddata != null)
                           phdBranch = phddata;
                   }






                   contentdata += "<tr>";
                   contentdata += "<td style='text-align:left' width='8%'>" + (i + 1) + "</td>";
                   contentdata += "<td style='text-align:left' width='22%'>" + scmregdata[i].FacultyName + "</td>";
                   contentdata += "<td style='text-align:left' width='20%'>" + scmregdata[i].RegNo + "</td>";
                   contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].PANNo + "</td>";
                   contentdata += "<td style='text-align:left' width='10%'>" + ugBranch + "</td>";
                   contentdata += "<td style='text-align:left' width='10%'>" + pgBranch + "</td>";
                   contentdata += "<td style='text-align:left' width='10%'>" + phdBranch + "</td>";
                   //contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].Experience + "</td>";
                   //if (scmregdata[i].PreviousCollege != null)
                   //    contentdata += "<td style='text-align:left' width='10%'>" + jntuh_college.Where(e => e.id == scmregdata[i].PreviousCollege).Select(e => e.collegeName).FirstOrDefault() + "</td>";
                   //else
                   //    contentdata += "<td style='text-align:left' width='10%'></td>";
                   contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].Remarks + "</td>";
                   contentdata += "</tr>";
               }
           }
           contentdata += "</table><br/>";


           //contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
           //contentdata += "<tr><td style='text-align:left' width='10%'><b>Sno</b></td>";
           //contentdata += "<td style='text-align:left' width='30%' colspan='2'><b>Role</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b>Name</b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b>Signature</b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>1</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>Chairperson</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>2</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>Principal</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>3</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 1</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>4</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 2</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>5</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>Expert 1</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>6</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>Expert 2</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

           //contentdata += "<tr><td style='text-align:left' width='10%'><b>7</b></td>";
           //contentdata += "<td style='text-align:left' width='25%'><b>SC/ST/OBC/Women/<br/>Differently Abled if any</b></td>";
           //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
           //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
           //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";
           //contentdata += "</table>";

           contents = contents.Replace("##SCMREPORT##", contentdata);
           return contents;
       }
        #endregion
    }

    public class AditorsDetails
    {
        public int SCMRequestId { get; set; }
        public int AditorId { get; set; }
        public string AditorName { get; set; }
        public string Designation { get; set; }
        public int DesignationId { get; set; }
        public string Department { get; set; }
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public bool Checke  { get; set; }

    }

}
