using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.OnlineRegistration
{
    [ErrorHandling]
    public class FacultyRequestsController : BaseController
    {
        //
        // GET: /FacultyRequests/
        private string serverURL;
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin,Faculty")]
        [HttpGet]
        public ActionResult FacultyMobilityRequest(string fid)
        {
            int fID = 0;
            if (fid != null)
            {
                fID =Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int? userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            MobilityRequest mrequest = new MobilityRequest();
            if (fID != 0)
            {
                jntuh_registered_faculty registredFaculty = db.jntuh_registered_faculty.Where(r => r.id == fID).Select(s => s).FirstOrDefault();
                if (registredFaculty != null)
                {
                    mrequest.FacultyRegistration = registredFaculty.RegistrationNumber.Trim();
                    mrequest.FacultyId = registredFaculty.id;
                    mrequest.FacultyName = registredFaculty.FirstName + " " + registredFaculty.MiddleName + " " + registredFaculty.LastName;
                    jntuh_college_faculty_registered collegeFacultyRegistered =
                        db.jntuh_college_faculty_registered.Where(
                            r => r.RegistrationNumber.Trim() == registredFaculty.RegistrationNumber.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    if (collegeFacultyRegistered != null)
                    {
                        mrequest.CollegeId = collegeFacultyRegistered.collegeId;
                        mrequest.CollegeName =db.jntuh_college.Where(c => c.id == mrequest.CollegeId).Select(s => s.collegeName).FirstOrDefault();
                        mrequest.CollegeCode =db.jntuh_college.Where(c => c.id == mrequest.CollegeId).Select(s => s.collegeCode).FirstOrDefault();
                    }
                    else
                    {
                        TempData["BtnHiding"] = "false";
                        return View(mrequest);
                    }
                }

                TempData["BtnHiding"] = "false";
                var MobilityRequest = db.jntuh_faculty_mobility_requests.Where(a => a.academicYearId == 11 && a.facultyId == fID).Select(a => a).FirstOrDefault();
                if (MobilityRequest != null)
                {
                    mrequest.TicketId = MobilityRequest.TicketId;
                    mrequest.SupportingDocumentView = MobilityRequest.supportingDocs;
                    TempData["BtnHiding"] = "true";
                }
            }
            return View(mrequest);
        }
        [Authorize(Roles = "Admin,Faculty")]
        [HttpPost]
        public ActionResult FacultyMobilityRequest(MobilityRequest mobility)
        {
            int fID = 0;
            int? userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualyear =
                db.jntuh_academic_year.Where(c => c.isActive == true && c.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(c => c.actualYear == (actualyear + 1)).Select(s => s.id).FirstOrDefault();
            string mobilitysupportdocument = "~/Content/Upload/Faculty/MobilitySupportDocuments";
            if (mobility != null)
            {
                jntuh_faculty_mobility_requests mobilityRequests =
                    db.jntuh_faculty_mobility_requests.Where(a => a.facultyId == mobility.FacultyId)
                        .Select(s => s)
                        .FirstOrDefault();
                if (mobility.SupportingDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(mobilitysupportdocument)))
                    {
                        Directory.CreateDirectory(Server.MapPath(mobilitysupportdocument));
                    }
                    var ext = Path.GetExtension(mobility.SupportingDocument.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        if (mobility.SupportingDocumentView == null)
                        {
                            string fileName = DateTime.Now.ToString("yyMMdd-HHmmss") + "JFMR";
                            mobility.SupportingDocument.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(mobilitysupportdocument), fileName, ext));
                            mobility.SupportingDocumentView = string.Format("{0}{1}", fileName, ext);
                        }
                        else
                        {
                            mobility.SupportingDocument.SaveAs(string.Format("{0}/{1}",
                                Server.MapPath(mobilitysupportdocument), mobility.SupportingDocumentView));
                        }
                    }
                }
                else
                {
                    return RedirectToAction("FacultyMobilityRequest", new { fid = UAAAS.Models.Utilities.EncryptString(mobility.FacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                }
                //Saving
                if (mobilityRequests == null)
                {
                    var RegFaculty = db.jntuh_registered_faculty.Where(a => a.id == mobility.FacultyId).Select(q => q).FirstOrDefault();
                    var FacultyName = RegFaculty.FirstName + " " + RegFaculty.MiddleName + " " + RegFaculty.LastName;
                    var ticketId = "JF" + DateTime.Now.ToString("yyMMdd") + "-MR" + DateTime.Now.ToString("HHmmss");

                    jntuh_faculty_mobility_requests Requests = new jntuh_faculty_mobility_requests();
                    Requests.academicYearId = ay0;
                    Requests.TicketId = ticketId;
                    Requests.collegeId = mobility.CollegeId;
                    Requests.facultyId = mobility.FacultyId;
                    Requests.supportingDocs = mobility.SupportingDocumentView;
                    Requests.isApproved = 0;
                    Requests.rejectReason = null;
                    Requests.isActive = true;
                    Requests.createdOn = DateTime.Now;
                    Requests.createdBy = userId;
                    Requests.updatedOn = null;
                    Requests.updatedBy = null;
                    db.jntuh_faculty_mobility_requests.Add(Requests);
                    db.SaveChanges();

                    TempData["SUCCESS"] = "Your Request is Processing...";
                    ViewBag.ticketId = ticketId;

                    string CollegeName = db.jntuh_college.Where(a => a.id == mobility.CollegeId).Select(a => a.collegeName + "(" + a.collegeCode + ")").FirstOrDefault().ToString();

                    //send email
                    IUserMailer mailer = new UserMailer();
                    mailer.FacultyMobilityRequestMail(RegFaculty.Email, "FacultyMobilityRequestMail",
                        "JNTUH Faculty Mobility Request Details", ticketId, FacultyName, RegFaculty.RegistrationNumber, CollegeName)
                        .SendAsync();
                }
                //else
                //{
                //    mobilityRequests.collegeId = mobility.CollegeId;
                //    mobilityRequests.facultyId = mobility.FacultyId;
                //    mobilityRequests.supportingDocs = mobility.SupportingDocumentView;
                //    mobilityRequests.isApproved = 0;
                //    mobilityRequests.rejectReason = null;
                //    mobilityRequests.isActive = true;
                //    mobilityRequests.updatedOn = DateTime.Now;
                //    mobilityRequests.updatedBy = userId;
                //    db.Entry(mobilityRequests).State = EntityState.Modified;
                //    db.SaveChanges();

                //    TempData["SUCCESS"] = "Your Request is Updating...";
                //}
            }
            return RedirectToAction("FacultyMobilityRequest", new { fid = UAAAS.Models.Utilities.EncryptString(mobility.FacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        //Faculty Edit Fields
        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult EditfieldsRequests(string fid)
        {
            int fID = 0;
            int? userId = 0;
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("Logon", "Account");
            else
                userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string RegistrationNumber = string.Empty;

            if (fid != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (userId != null && userId != 0)
            {
                RegistrationNumber = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.RegistrationNumber).FirstOrDefault();
                fID = db.jntuh_registered_faculty.Where(a => a.RegistrationNumber == RegistrationNumber).Select(a => a.id).FirstOrDefault();
            }
            else
            {
                TempData["SUCCESS"] = null;
                return RedirectToAction("Logon", "Account");
            }

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            var actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
            var presentyearId = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            //Code writen by siva for Faculty Edit Fields.
            var FacultyEditFieldsLinkId = db.jntuh_link_screens.Where(a => a.isActive == true && a.linkCode == "FFE").Select(a => a.id).FirstOrDefault();
            DateTime? facultyEditFieldsfromdate = DateTime.Now;
            DateTime? facultyEditFieldsclosedate = DateTime.Now; ;
            if (FacultyEditFieldsLinkId != 0)
            {
                facultyEditFieldsfromdate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.fromdate).FirstOrDefault();
                facultyEditFieldsclosedate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.todate).FirstOrDefault();
            }

            List<FacultyEditFieldnames> FacultyEditFieldnames = new List<FacultyEditFieldnames>();

            var notrequiredIds = new int[] { 39, 40, 41, 33, 34 };

            var EditRequests = db.jntuh_registered_faculty_edit_fields.Where(s => s.IsActive == true && !notrequiredIds.Contains(s.Id)).Select(m => new EditFields { FieldId = m.Id, Field = m.fieldName.Replace(" ", "").Replace("'", "").Replace("/", "Or").Replace("%", "percentage"), FieldDesc = m.fieldDescription, isSelect = false }).ToList();
            var EditEduRequests = db.jntuh_registered_faculty_edit_fields.Where(s => s.IsActive == true && notrequiredIds.Contains(s.Id)).Select(m => new EditEducationFields { FieldId = m.Id, Field = m.fieldName.Replace(" ", "").Replace("'", "").Replace("/", "Or").Replace("%", "percentage"), FieldDesc = m.fieldDescription, isSelect = false }).ToList();

            var faculty = db.jntuh_registered_faculty.Where(r => r.id == fID).Select(s => s).FirstOrDefault();
            FacultyEditFieldnames filedname = new FacultyEditFieldnames();
            filedname.facultyId = fID;
            filedname.EditCheckboxs = EditRequests;
            filedname.EditEducationCheckboxs = EditEduRequests;

            var RequestsData = db.jntuh_faculty_edit_requests.Where(s => s.academicYearId == presentyearId && s.facultyId == fID).Select(s => s).ToList();
            var EducationRequestsData = db.jntuh_faculty_education_edit_request.Where(s => s.academicyearid == presentyearId && s.facultyId == fID).Select(s => s).ToList();

            TempData["UpdateButton"] = "false";
            var currentDate = DateTime.Now;
            if (currentDate > facultyEditFieldsclosedate)
                return RedirectToAction("RequestsList", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
            else if (RequestsData.Count != 0 || EducationRequestsData.Count != 0)
            {
                TempData["UpdateButton"] = "true";
                return RedirectToAction("RequestsList", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
            }

            filedname.ticketId = RequestsData.Select(a => a.TicketId).FirstOrDefault() == null ? EducationRequestsData.Select(a => a.TicketId).FirstOrDefault() : RequestsData.Select(a => a.TicketId).FirstOrDefault();
            filedname.AllSuportdocumentView = RequestsData.Select(a => a.supportingDocs).FirstOrDefault() == null ? EducationRequestsData.Select(a => a.supportingDocs).FirstOrDefault() : RequestsData.Select(a => a.supportingDocs).FirstOrDefault();
            var PathFiledsIds = new int[] { 12, 13, 14, 15 };
            foreach (var item in filedname.EditCheckboxs)
            {
                item.Id = RequestsData.Where(s => s.fieldId == item.FieldId).Select(s => s.id).FirstOrDefault();
                item.isSelect = RequestsData.Where(s => s.fieldId == item.FieldId).Count() > 0 ? true : false;
                if (PathFiledsIds.Contains(item.FieldId))
                    item.SuportdocumentView = RequestsData.Where(s => s.fieldId == item.FieldId).Select(s => s.requestReason).FirstOrDefault();
                else
                    item.requestReason = RequestsData.Where(s => s.fieldId == item.FieldId).Select(s => s.requestReason).FirstOrDefault();

                switch (item.FieldId)
                {
                    case 5:
                        item.Fieldorginal = faculty.FatherOrHusbandName;
                        break;
                    case 6:
                        item.Fieldorginal = faculty.MotherName;
                        break;
                    case 7:
                        item.Fieldorginal = faculty.GenderId == 1 ? "Male" : "Female";
                        break;
                    case 8:
                        item.Fieldorginal = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                        break;
                    case 9:
                        item.Fieldorginal = faculty.Mobile.Trim();
                        break;
                    case 10:
                        if (faculty.AadhaarNumber != null)
                            item.Fieldorginal = faculty.AadhaarNumber.Trim();
                        break;
                    case 11:
                        item.Fieldorginal = faculty.PANNumber.Trim();
                        break;
                    case 12:
                        item.Fieldorginal = "../Content/Upload/Faculty/Photos/" + faculty.Photo.Trim();
                        break;
                    case 13:
                        item.Fieldorginal = "../Content/Upload/Faculty/AADHAARCARDS/" + faculty.AadhaarDocument.Trim();
                        break;
                    case 14:
                        item.Fieldorginal = "../Content/Upload/Faculty/PANCARDS/" + faculty.PANDocument.Trim();
                        break;
                    case 15:
                        item.Fieldorginal = "../Content/Upload/Faculty/INCOMETAX/" + faculty.IncometaxDocument.Trim();
                        break;
                    case 16:
                        item.Fieldorginal = faculty.DepartmentId != null ? db.jntuh_department.Where(s => s.id == faculty.DepartmentId).Select(a => a.departmentName).FirstOrDefault() : null;
                        break;
                    case 17:
                        item.Fieldorginal = faculty.DesignationId != null ? db.jntuh_designation.Where(s => s.id == faculty.DesignationId).Select(a => a.designation).FirstOrDefault() : null;
                        break;
                    case 18:
                        if (faculty.DateOfAppointment != null)
                            item.Fieldorginal = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                        break;
                    case 23:
                        if (faculty.AICTEFacultyId != null)
                            item.Fieldorginal = faculty.AICTEFacultyId;
                        break;
                    case 24:
                        if (faculty.grosssalary != null)
                            item.Fieldorginal = faculty.grosssalary;
                        break;
                }
            }

            foreach (var educ in filedname.EditEducationCheckboxs)
            {
                educ.Id = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(s => s.id).FirstOrDefault();
                educ.isSelect = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Count() > 0 ? true : false;
                educ.Coursestudied = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.courseStudied).FirstOrDefault();
                educ.Specialization = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.specialization).FirstOrDefault();
                educ.PassedYear = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.passedYear).FirstOrDefault();
                educ.MarkasPercentage = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.marksPercentage).FirstOrDefault();
                educ.Division = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.division).FirstOrDefault();
                educ.BoardorUniversity = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.boardOrUniversity).FirstOrDefault();
                educ.PlaceofEducation = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.placeOfEducation).FirstOrDefault();
                educ.EducationcertificateView = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(a => a.certificate).FirstOrDefault();
                educ.requestReason = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(s => s.reason).FirstOrDefault();
                educ.isApproved = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(s => s.isApproved).FirstOrDefault();
                educ.AllSupportFilesView = EducationRequestsData.Where(s => s.fieldId == educ.FieldId).Select(s => s.supportingDocs).FirstOrDefault();
            }

            List<DistinctDepartment> depts = GetDepartments();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();

            filedname.Depts = new List<string>() { "Test" };
            foreach (var item1 in depts)
            {
                filedname.Depts.Add(item1.departmentName);
            }
            filedname.Depts.Remove("Test");

            filedname.Design = new List<string>() { "Test" };
            foreach (var item2 in jntuh_designation.Where(c => c.isActive == true).Take(4).ToList())
            {
                filedname.Design.Add(item2.designation);
            }
            filedname.Design.Remove("Test");

            #region Auto Populate Condition.
            var specializations = db.jntuh_specialization.Where(s => s.isActive == true).Select(a => a).ToList();

            var ugspecializations = (from s in specializations
                                     join d in db.jntuh_department.ToList() on s.departmentId equals d.id
                                     join de in db.jntuh_degree.ToList() on d.degreeId equals de.id
                                     where (de.id == 4 || de.id == 5)
                                     select new
                                     {
                                         id = s.id,
                                         specializationname = s.specializationName
                                     }).ToList();

            filedname.ug_specializations = new List<string>() { "Test" };
            foreach (var item3 in ugspecializations)
            {
                filedname.ug_specializations.Add(item3.specializationname);
            }

            var pgspecializations = (from s in specializations
                                     join d in db.jntuh_department.ToList() on s.departmentId equals d.id
                                     join de in db.jntuh_degree.ToList() on d.degreeId equals de.id
                                     where (de.id != 4 && de.id != 5)
                                     select new
                                     {
                                         id = s.id,
                                         specializationname = s.specializationName
                                     }).ToList();

            filedname.pg_specializations = new List<string>() { "Test" };
            foreach (var item4 in pgspecializations)
            {
                filedname.pg_specializations.Add(item4.specializationname);
            }

            var Education = db.jntuh_registered_faculty_education.Select(e => new { educationid = e.educationId, coursestudied = e.courseStudied, universites = e.boardOrUniversity, places = e.placeOfEducation, specialization = e.specialization }).ToList();

            var RegisteredFacultyEducation_Courses = Education.Select(e => e.coursestudied.Trim()).Distinct().ToList();
            var RegisteredFacultyEducation_universities = Education.Where(z => z.universites != null).Select(e => e.universites.Trim()).Distinct().ToList();
            var RegisteredFacultyEducation_places = Education.Where(z => z.places != null).Select(e => e.places).Distinct().ToList();
            var RegisteredFacultyEducation_UGspecialization = Education.Where(a => a.educationid == 3 && a.specialization != null).Select(e => e.specialization.Trim()).Distinct().ToList();
            var RegisteredFacultyEducation_PGspecialization = Education.Where(a => a.educationid == 4 && a.specialization != null).Select(e => e.specialization.Trim()).Distinct().ToList();

            foreach (var UG in RegisteredFacultyEducation_UGspecialization)
            {
                filedname.ug_specializations.Add(UG.Trim());
            }
            filedname.ug_specializations.Remove("Test");
            foreach (var PG in RegisteredFacultyEducation_PGspecialization)
            {
                filedname.pg_specializations.Add(PG.Trim());
            }
            filedname.pg_specializations.Remove("Test");
            filedname.universitys = new List<string>() { "Rajiv Gandhi University", "Assam University", "Tezpur University", "University of Hyderabad, Hyderabad", "Maulana Azad National Urdu University", "English   and   Foreign   Languages   University", "Jamia Millia Islamia", "University of Delhi", "JawaharLal Nehru University", "Indira Gandhi National Open University", "South Asian University", "The  Indira  Gandhi  National  Tribal  University", "Dr. Harisingh Gour Vishwavidyalaya", "Mahatma  Gandhi  Antarrashtriya  Hindi  Vishwavidyalaya", "Mizoram University", "North Eastern Hill University", "Manipur University", "Central Agricultural University", "Nagaland University", "Pondicherry  University", "Sikkim University", "Tripura University", "Aligarh Muslim University", "Babasaheb  Bhimrao  Ambedkar  University", "Banaras Hindu University", "University of Allahabad", "Rajiv  Gandhi  National  Aviation  University", "Rani  Lakshmi  Bai  Central  Agricultural  University", "Visva Bharati, Shantiniketan", "Hemwati Nandan Bahuguna Garhwal University", "Central University of Tamil Nadu", "Indian Maritime University", "Central  University  of  Rajasthan", "Central University of Punjab", "Central University of Orissa", "Central University of Kerala", "Central University of Karnataka", "Central University of Jharkhand", "Central University of Kashmir, Transit Campus", "Central University of Jammu, Bagla (Rahya-Suchani)", "Central University of Himachal Pradesh", "Central University of Haryana", "Guru  Ghasidas  Vishwavidyalaya", "Central University of Bihar", "Nalanda University", "Mahatma  Gandhi  Central  University", "Central University of Gujarat", "Academy of Maritime Education and Training", "Amrita Vishwa Vidyapeetham", "Avinashilingam Institute for Home Science & Higher Education for Women", "B.L.D.E.", "B.S. Abdur Rahman Institute of Science and Technology", "Banasthai Vidyapith", "Bharath Institute of Higher Education & Research", "Bharati Vidyapeeth", "Bhatkhande Music Institute", "Birla Institute of Technology", "Birla Institute of Technology & Science", "Central Institute of Buddhist Studies(CIBS)", "Central Institute of Fisheries Education", "Central Institute of Higher Tibetan Studies", "Chennai Mathematical Institute", "Chettinad Academy of Research and Education (CARE)", "Chinmaya Vishwavidyapeeth", "Christ", "D.Y Patil Educational Society", "Datta Meghe Institute of Medical Sciences", "Dayalbagh Educational Institute", "Deccan College Postgraduate & Research Institute", "Dr. D.Y. Patil Vidyapeeth", "Dr. M.G.R. Educational and Research Institute", "Forest Research Institute", "Gandhi Institute of Technology and Management (GITAM)", "Gandhiigram Rural Institute", "Gokhale Institute of Politics & Economics", "Graphic Era", "Gujarat Vidyapith", "Gurukul Kangri vidyapeeth", "Hindustan Institute of Technology and Science (HITS)", "Homi Bhabha National Institute, Regd. Office", "ICFAI Foundation for Higher Education", "IIS", "Indain Institute of Foreigen Trade", "Indian Agricultural Research Institute", "Indian Association for the Cultivation of Science (IACS)", "Indian Institute of Information Technology and Management", "Indian Institute of Science", "Indian Institute of Space Science and Technology", "Indian Law Institute", "Indian School of Mines", "Indian Veterinary Research Institute", "Indira Gandhi Institute of Development Research", "Institute of Advanced Studies in Education", "Institute of Chemical Technology", "Institute of liver and Biliary Sciences", "Instituteof Armamrnt Technology", "International Institute for Population Sciences", "International Institute of Information Technology", "Jain", "Jain Vishva Bharati Institute", "Jamia Hamdard", "Janardan Rai Nagar Rajasthan Vidyapeeth", "Jawahar lal Nehru Centre for Advanced Scientific Research", "Jaypee Institute of Information Technology", "JSS Academy of Higher Education & Research", "K.L.E. Academy of Higher Education and Research", "Kalasalingam Academy of Research and Education", "Kalinga Institute of Industrial Technology", "Karpagam Academy of Higher Education", "Karunya Institute of Technology and Sciences", "Kerala Kalamandalam", "Koneru Lakshmaiah Education Foundation", "Krishna Institute of Medical Sciences", "Lakshmibai National Institute of Physical Education", "Lingaya's Vidyapeeth", "LNM Istitute of Information Technology", "Maharishi Markandeshwar (Deemed to be University)", "Manav Rachna International Institute of Research and Studies", "Manipal Academy of Higher Education", "Meenakshi Academy of Higher Education and Research", "MGM Institute of Health Sciences", "Narsee Monjee Institute of Management Studies", "National Brain Research Centre", "National Dairy Research Institute", "National Institute of Food Technology, Entrepreneurship & Management (NIFTEM)", "National Institute of Mental Health & Neuro Sciences", "National Museum Institute of History of Arts, Conservation and Musicology", "National Rail and Transportation Institute", "National University of Educational Planning & Administration", "Nava Nalanda Mahavihara", "Nehru Gram Bharati", "NITTE", "Noorul Islam Centre for Higher Education", "North Eastern Regional Institute of Science & Technology", "Padmashree Dr.D.Y. Patil Vidyapeeeth", "Pandit Dwarka Prasad Mishra Indian Institute of Information Technology", "Periyar Manaimmai Institute of Science & Technology (PMIST)", "Ponnaiyan Ramajayam Institute of Science & technology (PMIST)", "Pravara Institute of Medical Sciences", "Punjab Engineering College", "Rajiv Gandhi National Institute of Youth Development", "Ramakrishna Mission Vivekananda Educational and Research Institute", "Rashtriya Sanskrit Sansthana", "Rashtriya Sanskrit Vidyapeeth", "S.R.M Institute of Science and Technology", "Sam Higginbottom Institute of Agriculture, Technology & Sciences", "Sant Longowal Institute of Engineering and Technology", "Santosh", "Sathyabama Institute of Science and Technology", "Saveetha Institute of Medical and Technical Sciences", "Shanmugha Arts Science Technology & Research Academy (SASTRA)", "Shobhit Institute of Engineering & Technology", "Shri Lal Bahadur Shastri Rashtriya Sanskrit Vidyapith", "Siksha 'O' Anusandhan", "Sri Balaji Vidyapeeth (Deemed to be University)", "Sri Chandrasekharendra Saraswathi Vishwa Mahavidyalaya", "Sri Devraj Urs Academy of Higher Education and Research", "Sri Ramachandra Medical College and Research Institute", "Sri Sathya Sai Institute of Higher Learning", "Sri Siddhartha Academy of Higher Education", "St. Peterâ€™s Institute of Higher Education and Research", "Sumandeep Vidyapeeth", "Swami Vivekananda Yoga Anusandhana Samsthana", "Symbiosis International", "Tata Institute of Fundamental Research", "Tata Institute of Social Sciences", "TERI School of Advanced studies", "Thapar Institute of Engineering & Technology", "Tilak Maharashtra Vidyapeeth", "Vel Tech Rangarajan Dr. Sagunthala R & D Institute of Science and Technology", "Vellore Institute of Technology", "VELS Institute of Science Technology & Advanced Studies (VISTAS)", "Vignan's Foundation for Science, Technology and Research", "Vinayaka Missionâ€™s Research Foundation", "Yenepoya", "A.P.J. Abdul Kalam Technological University", "Acharaya N.G.Ranga Agricultural University", "Acharaya Nagarjuna University", "Adikavi Nannaya University", "Akkamahadevi women's University (Formerly known as Karnataka State Women's University)", "Alagappa University", "Aliah University", "Allahabad State University", "Ambedkar University Delhi (AUD)", "Anand Agricultural University", "Andhra University", "Anna University", "Annamalai University", "Arybhatta Knowledge University", "Assam Agricultural University", "Assam Rajiv Gandhi University of Co-operative Management", "Assam Science & Technology University", "Assam Womens University", "Atal Bihari Vajpayee Hindi Vishwavidyalaya", "Awadesh Pratap Singh University", "Ayush and Health Sciences University of Chhattisgarh", "Baba Farid University of Health Sciences", "Baba Ghulam Shah Badshah University", "Babasaheb Bhimrao Ambedkar Bihar University", "Banda University of Agriculture & Technology", "Bangalore University", "Bankura University", "Barkatullaah University", "Bastar Vishwavidyalaya", "Bengaluru Central University", "Bengaluru North University", "Berhampur University", "Bhagat Phool Singh Mahila Vishwavidyalaya", "Bhakta Kavi Narsinh Mehta University", "Bharathiar University", "Bharathidasan University", "Bhupender Narayan Mandal University", "Bidhan Chandra Krishi Vishwavidyalaya", "Bihar Agricultural University", "Biju Patnaik University of Technology", "Bilaspur Vishwavidyalaya", "Binod Bihari Mahto Koylanchal University", "Birsa Agricultural University", "Bodoland University", "Bundelkhand University", "Burdwan University", "Calcutta University", "Calicut University", "CEPT University", "Ch. Bansi Lal University", "Chanakya National Law University", "Chandr Shekhar Azad University of Agriculture & Technology", "Chatrapati Sahuji Maharaj Kanpur University", "Chaudhary Devi Lal University", "Chaudhary Ranbir Singh University", "Chaudhary Sarwan Kumar Himachal Pradesh Krishi Vishvavidyalaya", "Chhattisgarh Kamdhenu Vishwavidyalaya", "Chhattisgarh Swami Vivekanad Technical Universty", "Childrens University", "Choudary Charan Singh Haryana Agricultural Univeersity", "Choudary Charan Singh University", "Cluster University of Jammu", "Cluster University of Srinagar", "Cochin Unviersity of Science & Technology", "Cooch Behar Panchanan Barma University", "Cotton University", "Damodaram Sanjivayya National Law University", "Davangere University", "Deen Bandhu Chhotu Ram University of Sciences & Technology", "Deen Dayal Upadhyay Gorakhpur University", "Delhi Pharmaceutical Sciences & Research University", "Delhi Technological University", "Devi Ahilya Vishwavidyalaya", "Dharmashastra National Law University", "Dharmsinh Desai University", "Diamond Harbour Womens University", "Dibrugarh University", "Doon University", "Dr Shyama Prasad Mukherjee University", "Dr. A.P.J. Abdul Kalam Technical University", "Dr. B. R. Ambedkar University of Social Sciences", "Dr. B.R. Ambedkar University", "Dr. B.R.Ambedkar Open University", "Dr. B.R.Ambedkar University", "Dr. Babasaheb Ambedkar Marathwada University", "Dr. Babasaheb Ambedkar Open University", "Dr. Babasaheb AmbedkarTechnological University", "Dr. Bhimrao Ambedkar Law University", "Dr. N.T.R. University of Health Sciences", "Dr. Punjabrao Deshmukh Krishi Vidyapeeth", "Dr. Ram Manohar Lohia Awadh University", "Dr. Ram Manohar Lohiya National Law University", "Dr. Shakuntala Misra National Rehabilitation University", "Dr. Shyama Prasad University", "Dr. Y.S.Parmar University of Horticulture & Forestry", "Dr. Y.S.R. Horticultural Univerity", "Dravidian University", "Durg Vishwavidyalaya", "Fakir Mohan University", "G.B.Pant University of Agriculture & Technology", "Gangadhar Meher University", "Gauhati University", "Gautam Buddha University", "Goa University", "Gondwana University", "Govind Guru Tribal University", "Gujarat Agricultural University", "Gujarat Ayurveda University", "Gujarat Forensic Sciences University", "Gujarat National Law University", "Gujarat Technological University", "Gujarat University", "Gujarat University of Transplantation Sciences", "Gulbarga University", "Guru Angad Dev Veterinary & Animal Sciences University", "Guru Gobind Singh Indraprastha Vishwavidyalaya", "Guru Jambeshwar University of Science and Technology", "Guru Nanak Dev University", "Guru Ravidas Ayurved University", "Harcourt Butler Technical University", "Haridev Joshi University of Journalism and Mass Communication", "Haryana Vishwakarma Skill University", "Hemchandracharya North Gujarat University", "Hemwati Nandan Bahuguna Medical Education University", "Hidayatullah National Law University", "Himachal Pradesh National Law University", "Himachal Pradesh Technical University", "Himachal Pradesh University", "Indian Institute of Teacher Education", "Indira Gandhi Delhi Technical University for Women", "Indira Gandhi Krishi Vishwavidyalaya", "Indira Gandhi University", "Indira Kala Sangeet Vishwavidyalaya", "Indraprastha Institute of Information Technology", "Institute of Infrastructure Technology Research and Management", "International Institute of Information Technology (IIIT)", "Islamic University of Sciences & Technology University", "Jadavpur University", "Jagadguru Ramanandacharya Sanskrit University", "Jai Naraim Vyas University", "Jai Prakash vishwavidyalaya(university)", "Jammu University", "Jananayak Chandrashekhar University", "Jawaharlal Nehru Architecture and Fine Arts University", "Jawaharlal Nehru Krishi Vishwavidyalaya", "Jawaharlal Nehru Technological University", "Jharkhand Raksha Shakti University", "Jiwaji University", "Junagarh Agricultural University", "Kakatiya University", "Kaloji Narayan Rao University of Health Sciences", "Kalyani University", "Kamdhenu University", "Kameshwar Singh.Darbhanga Sanskrit Vishwavidyalaya", "Kannada University", "Kannur University", "Karanataka State Law University", "Karanataka State Open University", "Karanataka University", "Karanataka Veterinary, Animal & Fisheries Science University", "Karnataka Folklore University", "Karnataka Sanskrit University", "Karnataka State Rural Development and Panchayat Raj University", "Kashmir University", "Kavi Kulguru Kalidas Sanskrit Vishwavidyalaya", "Kazi Nazrul University", "Kerala Agricultural Unviersity", "Kerala University", "Kerala University of Fisheries & Ocean Studies", "Kerala University of Health Sciences", "Kerala Veterinary & Animal Sciences University", "Khallikote University", "Khwaja Moinuddin Chishti Urdu, Arabi-Farsi University", "King Georges Medical University", "Kolhan University", "Konkan Krishi Vidyapeeth", "Krantiguru Shyamji Krishna Verma Kachchh University", "Krishna Kanta Handique State Open University", "Krishna University", "Krishnakumarsinhji Bhavnagar University", "KSGH Music and Performing Arts University", "Kumar Bhaskar Varma Sanskrit & Ancient Studies University", "Kumaun University", "Kurukshetra University", "Kushabhau Thakre Patrakarita Avam Jansanchar Vishwavidyalaya", "Kuvempu University", "Lala Lajpat Rai University of Veterinary & Animal Sciences", "Lalit Narayan Mithila University", "Lucknow University", "M.J.P. Rohilkhand University", "M.P.Bhoj (open) University", "Madan Mohan Malaviya University of Technology", "Madaras University", "Madhya Pradesh Pashu Chikitsa Vigyan Vishwavidyalaya", "Madurai Kamraj University", "Magadh University", "Mahamaya Technical University", "Maharahtra University of Health Sciences", "Maharaja Bir Bikram University", "Maharaja Chhatrasal Bundelkhand Vishwavidyalaya", "Maharaja Ganga Singh University", "Maharaja Ranjit Singh Punjab Technical University", "Maharaja Sayajirao University of Baroda", "Maharaja Surajmal Brij University", "Maharana Partap Horticultural University", "Maharana Pratap University of Agriculture & Technology", "Maharashtra Animal & Fishery Sciences University", "Maharashtra National Law University", "Maharashtra National Law University,", "Maharishi Dayanand Saraswati University", "Maharishi Dayanand University", "Maharshi Panini Sanskrit Evam Vedic Vishwavidyalaya", "Mahatam Gandhi Kashi Vidyapeeth", "Mahatma Gandhi Chitrakoot Gramodaya Vishwavidyalaya", "Mahatma Gandhi University", "Mahatma Gandhi Unversity", "Mahatma Phule Krishi Vidyapeeth", "Makhanlal Chaturvedi National University of Journalism & Communication", "Mangalore University", "Manipur Technical University", "Manipur University of Culture", "Manonmaniam Sundarnar University", "Marathwada Agricultural University", "Maulana Abul Kalam Azad University of Technology", "Maulana Mazharul Haque Arabic & Persian University", "Mohan Lal Shukhadia University", "Mother Teresa Womens University", "Mumbai University", "Munger University", "Mysore University", "Nalanda Open University", "Narendra Deo University of Agriculture & Technology", "National Academy of Legal Studies & Research University", "National Law Institute University", "National law School of India University", "National Law Universituy", "National Law University", "National Law University and Judicial Academy", "National University of Advanced Legal Studies (NUALS)", "National University of Study & Research in Law", "Navsari Agriculture University", "Netaji Shubhash Open University", "Nilamber-Pitamber University", "Nizams Institute of Medical Sciences", "North Benagal University", "North Maharashtra University", "North Orissa University", "Odisha State Open University", "Orissa University Of Agriculture & Technology", "Osmania University", "Palamuru University", "Pandit S N Shukla University", "Patliputra University", "Patna University", "Periyar University", "Potti Sreeramulu Teugu Universtity", "Presidency University", "Professor Jayashankar Telangana State Agricultural University", "Pt. Bhagwat Dayal Sharma University of Health Sciences", "Pt. Sundarlal Sharma (Open) University", "Pt.Ravishankar Shukla University", "Punjab Agriculture University", "Punjab Technical University", "Punjab University", "Punjabi University", "Purnea University", "Rabindra Bharati University", "Raiganj University", "Raj Rishi Bhartrihari Matsya University", "Raja Mansingh Tomar Music & Arts University", "Rajasthan Ayurveda University", "Rajasthan ILD Skills University (RISU)", "Rajasthan Technical University", "Rajasthan University", "Rajasthan University of Health Sciences", "Rajasthan University of Veterinary & Animal Sciences", "Rajendra Agricultural University", "Rajiv Gandhi Prodoyogiki Vishwavidyalaya", "Rajiv Gandhi University of Health Science", "Rajiv Gandhi University of Knowledge Technology", "Rajmata Vijayaraje Scindia Krishi Vishwa Vidyalaya", "Raksha Shakti University", "Rama Devi Womens University", "Ranchi University", "Rani Channamma University", "Rani Durgavati Vishwavidyalaya", "Ravenshaw University", "Rayalaseema University", "Sambalpur University", "Sampurnanand Sanskrit Vishwavidyalaya", "Sanchi University of Buddhist-Indic Studies", "Sant Gadge Baba Amravati University", "Sardar Krushinagar Dantiwada Agricultural University", "Sardar Patel University", "Sardar Patel University of Police, Security & Criminal Justice", "Sardar Vallabh Bhai Patel University of Agriculture & Technology", "Sarguja University", "Satavahana University", "Saurashtra University", "Savitribai Phule Pune University", "Shekhawati University", "Sher-e-Kashmir University of Agricultural Science & Technology", "Shivaji University", "shree guru gobind singh tricentenary university", "Shree Sankaracharaya University of Sanskrit", "Shree Somnath Sanskrit University", "Shri Govind Guru University", "Shri Jagannath Sanskrit Vishwavidyalaya", "Shri Mata Vaishno Devi University", "Siddharth University", "Sidho-Kanho-Birsha University", "Sido Kanhu University", "Smt. Nathibai Damodar Thackersey Womens University", "Solapur University", "Sri Dev Suman Uttarakhand Vishwavidyalaya", "Sri Konda Laxman Telangana State Horticultural University", "Sri krishnadevaraya University", "Sri P V Narsimha Rao Telangana Veterinary University", "Sri Padmavati Mahila Vishwavidyalayam", "Sri Venkateswara Institute of Medical Sciences", "Sri Venkateswara University", "Sri Venkateswara Vedic University", "Sri Venkateswara Veterinary University", "Srimanta Sankaradeva University of Health Sciences", "State University of Performing and Visual Arts", "Swami Keshwanand Rajasthan Agriculture University", "Swami Ramanand Teerth Marathwada University", "Swarnim Gujarat Sports University", "T.M. Bhagalpur University", "Tamil Nadu Fisheries University", "Tamil Nadu Music and Fine Arts University", "Tamil Nadu Open University", "Tamil Nadu Teacher Education University", "Tamil University", "Tamilnadu Agricultural University", "Tamilnadu Dr. Ambedkar Law University", "Tamilnadu Dr. M.G.R.Medical University", "Tamilnadu National Law School", "Tamilnadu Physical Educaton and Sports University", "Tamilnadu Veterinary & Animal Sciences University", "Telangana University", "The Bengal Engineering & Science University", "The Rajiv Gandhi National University of Law", "The Rashtrasant Tukadoji Maharaj Nagpur University", "The Sanskrit College and University", "The West Bengal National University of Juridical Science", "The West Bengal University of Health Sciences", "Thiruvalluvar University", "Thunchath Ezhuthachan Malayalam University", "Tumkur University", "U.P. Pandit Deen Dayal Upadhyaya Pashu Chikitsa Vigyan Vishwavidhyalaya Evam Go-Anusandhan Sansthan", "U.P. Rajarshi Tandon Open University", "U.P.King Georges University of Dental Sciences", "University of Agricultural Sciences", "University of Horticultural Sciences", "University of Kota", "Univesity of Gour Banga", "Utkal University", "Utkal University of Culture", "Uttar Banga Krishi Vishwavidyalaya", "Uttar Pradesh University of Medical Sciences", "Uttarakhand Aawasiya Viswavidyalaya", "Uttarakhand Ayurved University", "Uttarakhand Open University", "Uttarakhand Sanskrit University", "Uttarakhand Technical University", "Vardhman Mahaveer Open University", "Veer Bahadur Singh Purvanchal University", "Veer Chandra Singh Garhwali Uttarakhand University of Horticulture & Forestry", "Veer Kunwar Singh University", "Veer Narmad South Gujarat University", "Veer Surendra Sai Institute of Medical Science and Research", "Veer Surendra Sai University of Technology", "Vesveswaraiah Technological University", "Vidya Sagar University", "Vijayanagara Sri Krishnadevaraya University", "Vikram Simhapuri University", "Vikram University", "Vinoba Bhave University", "West Bengal State University", "West Bengal University of Animal and Fishery Sciences", "West Bengal University of Teachers, Training, Education Planning and Administration", "Yashwant Rao Chavan Maharashtra Open University", "YMCA University of Science & Technology", "Yogi Vemana University" };
            filedname.places = new List<string>() { "Arunachal Pradesh", "Assam", "Telangana", "Delhi", "Madhya Pradesh", "Maharashtra", "Mizoram", "Meghalaya", "Manipur", "Nagaland", "Pondicherry", "Sikkim", "Tripura", "Uttar Pradesh", "West Bengal", "Uttarakhand", "Tamil Nadu", "Rajasthan", "Punjab", "Orissa", "Kerala", "Karnataka", "Jharkhand", "Jammu & Kashmir", "Himachal", "Pradesh", "Haryana", "Chhattisgarh", "Bihar", "Gujarat", "Jammu and Kashmir", "Andhra Pradesh", "Chandigarh", "Puducherry", "Himachal Pradesh", "Goa" };
            filedname.Courses = new List<string>() { "BE", "BTech", "ME", "MTech", "MBA", "MCA", "BCA", "PharmD", "BPharmacy", "MPharmacy", "BCom", "MCom", "BSC", "MSC", "M.A", "BA", "BZC", "MPhil", "PhD" };

            foreach (var course in RegisteredFacultyEducation_Courses)
            {
                filedname.Courses.Add(course.Trim());
            }

            foreach (var university in RegisteredFacultyEducation_universities)
            {
                filedname.universitys.Add(university.Trim());
            }

            foreach (var place in RegisteredFacultyEducation_places)
            {
                filedname.places.Add(place.Trim());
            }
            #endregion

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

            FacultyEditFieldnames.Add(filedname);
            return View(filedname);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult EditfieldsRequests(FacultyEditFieldnames facultyfields)
        {
            int fID = 0;
            int? userId = 0;
            string RegistrationNumber = string.Empty;
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string facultysupportfile = "~/Content/Upload/Faculty/FieldEditSupportDocuments";
            string facultysupportfileinPDf = "~/Content/Upload/Faculty/FieldEditSupportDocuments/AllSuppDocPDF";
            if (userId != null && userId != 0)
            {
                RegistrationNumber = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.RegistrationNumber).FirstOrDefault();
                fID = db.jntuh_registered_faculty.Where(a => a.RegistrationNumber == RegistrationNumber).Select(a => a.id).FirstOrDefault();
            }
            else
            {
                TempData["SUCCESS"] = null;
                return RedirectToAction("Logon", "Account");
            }
            //Writtenby Narayana Reddy
            var cheeckededitfiels = facultyfields.EditCheckboxs.Where(a => a.isSelect == true).Select(s => s).ToList();
            var CheckedEduEditFields = facultyfields.EditEducationCheckboxs.Where(a => a.isSelect == true).Select(a => a).ToList();
            try
            {
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var actualYear = jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
                var presentyearId = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

                //written by siva
                var FacultyrequestsList = db.jntuh_faculty_edit_requests.Where(a => a.academicYearId == presentyearId && a.facultyId == fID).Select(s => s).ToList();
                var FacultyEducationrequestsList = db.jntuh_faculty_education_edit_request.Where(a => a.academicyearid == presentyearId && a.facultyId == fID).Select(s => s).ToList();
                var allsuppDocsPath = string.Empty;

                if (facultyfields.AllSuportdocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultysupportfileinPDf)))
                        Directory.CreateDirectory(Server.MapPath(facultysupportfileinPDf));

                    var ext = Path.GetExtension(facultyfields.AllSuportdocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string fileName = "SuppDoc" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        facultyfields.AllSuportdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultysupportfileinPDf),
                            fileName, ext));
                        allsuppDocsPath = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else if (facultyfields.AllSuportdocumentView != null)
                    allsuppDocsPath = facultyfields.AllSuportdocumentView;
                else
                    return RedirectToAction("EditfieldsRequests", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });

                var PathFiledsIds = new int[] { 12, 13, 14, 15 };
                var filepathsIdsCount = cheeckededitfiels.Where(s => PathFiledsIds.Contains(s.FieldId)).Select(s => s.FieldId).Count();
                var filepaths = cheeckededitfiels.Where(s => PathFiledsIds.Contains(s.FieldId) && s.Suportdocument != null).Select(s => s.FieldId).Count() == 0 ?
                                cheeckededitfiels.Where(s => PathFiledsIds.Contains(s.FieldId) && s.SuportdocumentView != null).Select(s => s.FieldId).Count() : cheeckededitfiels.Where(s => PathFiledsIds.Contains(s.FieldId) && s.Suportdocument != null).Select(s => s.FieldId).Count();
                if (filepathsIdsCount != filepaths)
                    return RedirectToAction("EditfieldsRequests", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });

                var RegFacultyPanNumber = db.jntuh_registered_faculty.Where(a => a.id == fID).Select(q => q.PANNumber).FirstOrDefault();
                var ticketId = "JF" + DateTime.Now.ToString("yyMMdd") + "-ER" + DateTime.Now.ToString("HHmmss");

                if (FacultyrequestsList.Count != 0 || FacultyEducationrequestsList.Count != 0)
                {
                    ticketId = FacultyrequestsList.Select(a => a.TicketId).FirstOrDefault() != null ? FacultyrequestsList.Select(s => s.TicketId).FirstOrDefault() :
                        FacultyEducationrequestsList.Select(a => a.TicketId).FirstOrDefault() != null ? FacultyEducationrequestsList.Select(a => a.TicketId).FirstOrDefault() : ticketId;
                }

                foreach (var save in cheeckededitfiels)
                {
                    jntuh_faculty_edit_requests editrequests =
                       FacultyrequestsList.Where(a => a.facultyId == fID && a.fieldId == save.FieldId)
                           .Select(s => s)
                           .FirstOrDefault();

                    if (save.FieldId == 12 || save.FieldId == 13 || save.FieldId == 14 || save.FieldId == 15)
                    {
                        if (save.Suportdocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath(facultysupportfile)))
                                Directory.CreateDirectory(Server.MapPath(facultysupportfile));

                            var ext = Path.GetExtension(save.Suportdocument.FileName);
                            if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                            {
                                string fileName = save.FieldId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                                save.Suportdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultysupportfile),
                                    fileName, ext));
                                save.requestReason = string.Format("{0}{1}", fileName, ext);
                            }
                        }
                        else
                            save.requestReason = editrequests.supportingDocs;
                    }


                    if (editrequests == null)
                    {
                        jntuh_faculty_edit_requests neweditrequests = new jntuh_faculty_edit_requests();
                        neweditrequests.academicYearId = presentyearId;
                        neweditrequests.TicketId = ticketId;
                        neweditrequests.fieldId = save.FieldId;
                        neweditrequests.facultyId = fID;
                        neweditrequests.requestReason = save.requestReason;
                        neweditrequests.supportingDocs = allsuppDocsPath;
                        neweditrequests.isActive = true;
                        neweditrequests.isApproved = 0;
                        neweditrequests.rejectReason = null;
                        neweditrequests.createdOn = DateTime.Now;
                        neweditrequests.createdBy = userId;
                        neweditrequests.updatedOn = null;
                        neweditrequests.updatedBy = null;
                        db.jntuh_faculty_edit_requests.Add(neweditrequests);
                        db.SaveChanges();
                    }
                    else
                    {
                        editrequests.requestReason = save.requestReason;
                        editrequests.supportingDocs = allsuppDocsPath;
                        editrequests.updatedOn = DateTime.Now;
                        editrequests.updatedBy = userId;
                        db.Entry(editrequests).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                //written by siva.
                var DeleteRequest = facultyfields.EditCheckboxs.Where(a => a.isSelect == false).Select(s => s).ToList();
                foreach (var delete in DeleteRequest)
                {
                    jntuh_faculty_edit_requests request = FacultyrequestsList.Where(s => s.fieldId == delete.FieldId).Select(s => s).FirstOrDefault();
                    if (request != null)
                    {
                        if (request.fieldId == 12 || request.fieldId == 13 || request.fieldId == 14 || request.fieldId == 15)
                        {
                            if (request.requestReason != null && request.requestReason != "")
                            {
                                System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultysupportfile), request.requestReason));
                            }
                        }
                        db.jntuh_faculty_edit_requests.Remove(request);
                        db.SaveChanges();
                    }
                }


                foreach (var Edu in CheckedEduEditFields)
                {
                    jntuh_faculty_education_edit_request Edueditrequests =
                       FacultyEducationrequestsList.Where(a => a.facultyId == fID && a.fieldId == Edu.FieldId)
                           .Select(s => s)
                           .FirstOrDefault();
                    if (Edu.Educationcertificate != null)
                    {
                        if (!Directory.Exists(Server.MapPath(facultysupportfile)))
                            Directory.CreateDirectory(Server.MapPath(facultysupportfile));

                        var ext = Path.GetExtension(Edu.Educationcertificate.FileName);
                        if (ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG"))
                        {
                            string fileName = Edu.FieldId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                            Edu.Educationcertificate.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultysupportfile),
                                fileName, ext));
                            Edu.EducationcertificateView = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else
                        Edu.EducationcertificateView = Edueditrequests.certificate;

                    if (Edueditrequests == null)
                    {
                        jntuh_faculty_education_edit_request neweditrequests = new jntuh_faculty_education_edit_request();
                        neweditrequests.academicyearid = presentyearId;
                        neweditrequests.TicketId = ticketId;
                        neweditrequests.fieldId = Edu.FieldId;
                        neweditrequests.facultyId = fID;
                        neweditrequests.educationId = Edu.FieldId == 39 ? 1 : Edu.FieldId == 40 ? 3 : Edu.FieldId == 41 ? 4 : Edu.FieldId == 33 ? 5 : Edu.FieldId == 34 ? 6 : Edu.Educationid;
                        neweditrequests.courseStudied = String.IsNullOrEmpty(Edu.Coursestudied) ? Edu.Coursestudied : Edu.Coursestudied.Trim();
                        neweditrequests.specialization = String.IsNullOrEmpty(Edu.Specialization) ? Edu.Specialization : Edu.Specialization.Trim();
                        neweditrequests.passedYear = Edu.PassedYear == null ? 0 : (int)Edu.PassedYear;
                        neweditrequests.marksPercentage = Edu.MarkasPercentage == null ? 0 : (int)Edu.MarkasPercentage;
                        neweditrequests.division = Edu.Division == null ? 0 : (int)Edu.Division;
                        neweditrequests.boardOrUniversity = String.IsNullOrEmpty(Edu.BoardorUniversity) ? Edu.BoardorUniversity : Edu.BoardorUniversity.Trim();
                        neweditrequests.placeOfEducation = String.IsNullOrEmpty(Edu.PlaceofEducation) ? Edu.PlaceofEducation : Edu.PlaceofEducation.Trim();
                        neweditrequests.certificate = Edu.EducationcertificateView;
                        neweditrequests.supportingDocs = allsuppDocsPath;
                        neweditrequests.reason = Edu.requestReason;
                        neweditrequests.isApproved = 0;
                        neweditrequests.createdOn = DateTime.Now;
                        neweditrequests.createdBy = userId;
                        db.jntuh_faculty_education_edit_request.Add(neweditrequests);
                        db.SaveChanges();

                    }
                    else
                    {
                        Edueditrequests.courseStudied = String.IsNullOrEmpty(Edu.Coursestudied) ? Edu.Coursestudied : Edu.Coursestudied.Trim();
                        Edueditrequests.specialization = String.IsNullOrEmpty(Edu.Specialization) ? Edu.Specialization : Edu.Specialization.Trim();
                        Edueditrequests.passedYear = Edu.PassedYear == null ? 0 : (int)Edu.PassedYear;
                        Edueditrequests.marksPercentage = Edu.MarkasPercentage == null ? 0 : (int)Edu.MarkasPercentage;
                        Edueditrequests.division = Edu.Division == null ? 0 : (int)Edu.Division;
                        Edueditrequests.boardOrUniversity = String.IsNullOrEmpty(Edu.BoardorUniversity) ? Edu.BoardorUniversity : Edu.BoardorUniversity.Trim();
                        Edueditrequests.placeOfEducation = String.IsNullOrEmpty(Edu.PlaceofEducation) ? Edu.PlaceofEducation : Edu.PlaceofEducation.Trim();
                        Edueditrequests.certificate = Edu.EducationcertificateView;
                        Edueditrequests.supportingDocs = allsuppDocsPath;
                        Edueditrequests.reason = Edu.requestReason;
                        db.Entry(Edueditrequests).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                //written by siva.
                var EducationDeleteRequest = facultyfields.EditEducationCheckboxs.Where(a => a.isSelect == false).Select(s => s).ToList();
                foreach (var Edudelete in EducationDeleteRequest)
                {
                    jntuh_faculty_education_edit_request request = FacultyEducationrequestsList.Where(s => s.fieldId == Edudelete.FieldId).Select(s => s).FirstOrDefault();
                    if (request != null)
                    {
                        if (request.certificate != null && request.certificate != "")
                        {
                            System.IO.File.Delete(string.Format("{0}\\{1}", Server.MapPath(facultysupportfile), request.certificate));
                        }
                        db.jntuh_faculty_education_edit_request.Remove(request);
                        db.SaveChanges();
                    }
                }

                TempData["SUCCESS"] = "Requests are saved successfully.";
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";
            }
            return RedirectToAction("RequestsList", new { fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        public List<DistinctDepartment> GetDepartments()
        {
            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_departments = (from a in jntuh_department
                                     join b in jntuh_degree on a.degreeId equals b.id
                                     select new
                                     {
                                         id = a.id,
                                         departmentName = b.degree + "-" + a.departmentName
                                     }).ToList();

            string existingDepts = string.Empty;
            int[] notRequiredIds = { 25, 26, 27, 33, 34, 36, 37, 38, 39, 53, 54, 55, 56, 71, 72, 73, 74, 75, 76, 77, 78, 60 };
            foreach (var item in jntuh_departments.Where(s => !notRequiredIds.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            return depts;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult RequestsList(string fid)
        {
            int fID = 0;
            int? userId = 0;
            string RegistrationNumber = string.Empty;
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (fid != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (userId != null && userId != 0)
            {
                RegistrationNumber = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.RegistrationNumber).FirstOrDefault();
                fID = db.jntuh_registered_faculty.Where(a => a.RegistrationNumber == RegistrationNumber).Select(a => a.id).FirstOrDefault();
            }
            else
            {
                TempData["SUCCESS"] = null;
                return RedirectToAction("Logon", "Account");
            }

            //Code writen by siva for Faculty Edit Fields.
            var FacultyEditFieldsLinkId = db.jntuh_link_screens.Where(a => a.isActive == true && a.linkCode == "FFE").Select(a => a.id).FirstOrDefault();
            DateTime? facultyEditFieldsfromdate = DateTime.Now;
            DateTime? facultyEditFieldsclosedate = DateTime.Now; ;
            if (FacultyEditFieldsLinkId != 0)
            {
                facultyEditFieldsfromdate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.fromdate).FirstOrDefault();
                facultyEditFieldsclosedate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.todate).FirstOrDefault();
            }
            TempData["EditFieldsLink"] = "false";
            var currentDate = DateTime.Now;
            if (currentDate > facultyEditFieldsclosedate)
                TempData["EditFieldsLink"] = "false";
            else
                TempData["EditFieldsLink"] = "true";

            var RequestsData = db.jntuh_faculty_edit_requests.Where(s => s.facultyId == fID).Select(s => s).ToList();
            var EduRequestsData = db.jntuh_faculty_education_edit_request.Where(s => s.facultyId == fID).Select(s => s).ToList();
            var requests = new FacultyEditFieldnames();
            requests.facultyId = fID;

            TempData["AcknowledgementLink"] = "false";

            //requests.ticketId = RequestsData.Select(a => a.TicketId).FirstOrDefault() == null ? RequestsData.Select(a => a.TicketId).FirstOrDefault() : RequestsData.Select(a => a.TicketId).FirstOrDefault();
            if (RequestsData.Count != 0)
            {
                var requestsList = (from r in db.jntuh_registered_faculty_edit_fields.ToList()
                                    join req in RequestsData on r.Id equals req.fieldId
                                    select new EditFields
                                    {
                                        Id = req.facultyId,
                                        ticketId = req.TicketId,
                                        FieldId = req.fieldId,
                                        Field = r.fieldName,
                                        requestReason = req.requestReason,
                                        SuportdocumentView = req.supportingDocs,
                                        isApproved = req.isApproved,
                                        createddate = req.createdOn.ToString()
                                    }).ToList();
                requests.EditCheckboxs = requestsList;
                TempData["AcknowledgementLink"] = "true";
            }
            if (EduRequestsData.Count != 0)
            {
                var edurequestsList = (from r in db.jntuh_registered_faculty_edit_fields.ToList()
                                       join req in EduRequestsData on r.Id equals req.fieldId
                                       select new EditEducationFields
                                       {
                                           Id = req.facultyId,
                                           ticketId = req.TicketId,
                                           FieldId = r.Id,
                                           Field = r.fieldName,
                                           Coursestudied = req.courseStudied,
                                           Specialization = req.specialization,
                                           PassedYear = req.passedYear,
                                           MarkasPercentage = req.marksPercentage,
                                           Division = req.division,
                                           BoardorUniversity = req.boardOrUniversity,
                                           PlaceofEducation = req.placeOfEducation,
                                           EducationcertificateView = req.certificate,
                                           AllSupportFilesView = req.supportingDocs,
                                           requestReason = req.reason,
                                           isApproved = req.isApproved,
                                           createddate = req.createdOn.ToString()
                                       }).ToList();
                requests.EditEducationCheckboxs = edurequestsList;
                TempData["AcknowledgementLink"] = "true";
            }
            return View(requests);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult AllRequestsListForAdmin(int? FieldId)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var Edit_requests = db.jntuh_faculty_edit_requests.Where(a => a.isActive == true && a.isApproved == 0).Select(a => a).ToList();
            var Edit_Edu_requests = db.jntuh_faculty_education_edit_request.Where(s => s.isApproved == 0).Select(a => a).ToList();

            var Edit_requestsIds = Edit_requests.Select(q => q.fieldId).Distinct().ToList();
            var Edit_Edu_requestsIds = Edit_Edu_requests.Select(a => a.fieldId).Distinct().ToList();

            foreach (var field in Edit_Edu_requestsIds)
            {
                if (!Edit_requestsIds.Contains(field))
                    Edit_requestsIds.Add(field);
            }
            var edit_requests = db.jntuh_registered_faculty_edit_fields.Where(a => Edit_requestsIds.Contains(a.Id)).Select(s => new { Id = s.Id, Field = s.fieldDescription }).ToList();
            ViewBag.EditRequests = edit_requests;
            RequestsListBasedonRegNumbers Requests = new RequestsListBasedonRegNumbers();
            List<jntuh_faculty_edit_requests> RequestsList = new List<jntuh_faculty_edit_requests>();
            List<jntuh_faculty_education_edit_request> EduRequestsList = new List<jntuh_faculty_education_edit_request>();
            if (FieldId != null && FieldId != 0)
            {
                RequestsList = db.jntuh_faculty_edit_requests.Where(a => a.fieldId == FieldId && a.isActive == true && a.isApproved == 0).ToList();
                EduRequestsList = db.jntuh_faculty_education_edit_request.Where(a => a.fieldId == FieldId && a.isApproved == 0).ToList();
                Requests.FieldId = FieldId;
            }
            else
            {
                RequestsList = db.jntuh_faculty_edit_requests.Where(a => a.isActive == true).ToList();
                EduRequestsList = db.jntuh_faculty_education_edit_request.ToList();
            }

            var GroupbyEduRequestsList = EduRequestsList.GroupBy(a => new { a.facultyId }).Select(q => q.First()).ToList();
            var GroupbyRequestsList = RequestsList.GroupBy(z => new { z.facultyId }).Select(q => q.First()).ToList();

            var GroupbyRequestsListIds = GroupbyRequestsList.Select(a => a.facultyId).Distinct().ToList();
            var GroupbyEduRequestsListIds = GroupbyEduRequestsList.Select(a => a.facultyId).Distinct().ToList();

            foreach (var add in GroupbyEduRequestsListIds)
            {
                if (!GroupbyRequestsListIds.Contains(add))
                    GroupbyRequestsListIds.Add(add);
            }

            var RegIds = GroupbyRequestsListIds;
            var jntuh_registrated_faculty = db.jntuh_registered_faculty.Where(s => RegIds.Contains(s.id)).Select(s => s).ToList();
            var RegNos = jntuh_registrated_faculty.Select(s => s.RegistrationNumber).Distinct().ToList();
            var jntuh_colleges = db.jntuh_college.Where(a => a.isActive == true).ToList();
            var CollegeFaculty = db.jntuh_college_faculty_registered.Where(a => RegNos.Contains(a.RegistrationNumber)).Select(a => a).ToList();

            if (GroupbyRequestsListIds.Count != 0)
            {
                List<FacultyData> Req = new List<FacultyData>();
                foreach (var item in GroupbyRequestsListIds)
                {
                    FacultyData singlerequest = new FacultyData();
                    var RFaculty = jntuh_registrated_faculty.Where(a => a.id == item).Select(s => s).FirstOrDefault();
                    singlerequest.ticketId = String.IsNullOrEmpty(RequestsList.Where(s => s.facultyId == item).Select(a => a.TicketId).FirstOrDefault()) ? EduRequestsList.Where(s => s.facultyId == item).Select(a => a.TicketId).FirstOrDefault() : RequestsList.Where(s => s.facultyId == item).Select(a => a.TicketId).FirstOrDefault();
                    singlerequest.FacultyId = item;
                    singlerequest.RegistrationNumber = RFaculty.RegistrationNumber;
                    singlerequest.Name = RFaculty.FirstName + " " + RFaculty.MiddleName + " " + RFaculty.LastName;
                    singlerequest.Gender = RFaculty.GenderId == 1 ? "Male" : "Female";
                    singlerequest.CollegeId = CollegeFaculty.Where(q => q.RegistrationNumber == RFaculty.RegistrationNumber).Select(s => s.collegeId).FirstOrDefault();
                    singlerequest.CollegeName = jntuh_colleges.Where(a => a.id == singlerequest.CollegeId).Select(s => s.collegeName + "(" + s.collegeCode + ")").FirstOrDefault();
                    singlerequest.RequestsCount = RequestsList.Where(a => a.facultyId == item && a.isApproved == 0).Count();
                    singlerequest.EduRequestsCount = EduRequestsList.Where(a => a.facultyId == item && a.isApproved == 0).Count();
                    Req.Add(singlerequest);
                }
                Requests.FacultyList = Req;
            }
            return View(Requests);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyRequestsForAdmin(string Fid, string IntFacultyId)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            if (Fid != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(Fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (IntFacultyId != "" && IntFacultyId != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(IntFacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else
            {
                TempData["ERROR"] = "Something went wrong,Please Try Again";
                return RedirectToAction("AllRequestsListForAdmin");
            }
            List<AdminRequestsClass> Requests = new List<AdminRequestsClass>();
            var requests = db.jntuh_faculty_edit_requests.Where(a => a.isActive == true && a.facultyId == fID && a.isApproved == 0).ToList();

            if (requests.Count != 0)
            {
                var FieldsIds = requests.Select(s => s.fieldId).Distinct().ToList();
                var jntuh_registered_faculty_edit_fields = db.jntuh_registered_faculty_edit_fields.Where(s => FieldsIds.Contains(s.Id)).Select(a => a).ToList();
                var strRegNo = requests.Select(s => s.facultyId).Distinct().ToList();

                foreach (var item in requests)
                {
                    AdminRequestsClass singlerequest = new AdminRequestsClass();
                    singlerequest.FacultyId = item.facultyId;
                    singlerequest.Id = item.id;
                    singlerequest.FieldId = item.fieldId;
                    singlerequest.requestReason = item.requestReason;
                    singlerequest.SuportdocumentView = item.supportingDocs;
                    singlerequest.isApproved = item.isApproved;
                    singlerequest.rejectReason = item.rejectReason;
                    singlerequest.FieldDesc = jntuh_registered_faculty_edit_fields.Where(a => a.Id == item.fieldId).Select(a => a.fieldDescription).FirstOrDefault();
                    Requests.Add(singlerequest);
                }
            }

            TempData["EducationCount"] = "false";
            var edurequests = db.jntuh_faculty_education_edit_request.Where(a => a.facultyId == fID && a.isApproved == 0).ToList();
            if (edurequests.Count != 0 && requests.Count == 0)
            {
                TempData["EducationCount"] = "true";
                return RedirectToAction("FacultyEducationRequestsForAdmin", new { Fid = UAAAS.Models.Utilities.EncryptString(fID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                //AdminRequestsClass Edurequest = new AdminRequestsClass();
                //Edurequest.FacultyId = edurequests.Select(s => s.facultyId).FirstOrDefault();
                //Requests.Add(Edurequest);
            }
            else if (edurequests.Count != 0 && requests.Count != 0)
                TempData["EducationCount"] = "true";
            return View(Requests);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyEducationRequestsForAdmin(string Fid, string IntFacultyId)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;
            if (Fid != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(Fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (IntFacultyId != "" && IntFacultyId != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(IntFacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else
            {
                TempData["ERROR"] = "Something went wrong,Please Try Again";
                return RedirectToAction("AllRequestsListForAdmin");
            }
            var edurequests = db.jntuh_faculty_education_edit_request.Where(a => a.facultyId == fID && a.isApproved == 0).ToList();
            List<EditEducationFields> EduRequests = new List<EditEducationFields>();
            if (edurequests.Count != 0)
            {
                var EduFieldsIds = edurequests.Select(s => s.fieldId).Distinct().ToList();
                var jntuh_registered_faculty_edit_education_fields = db.jntuh_registered_faculty_edit_fields.Where(s => EduFieldsIds.Contains(s.Id)).Select(a => a).ToList();
                var strfids = edurequests.Select(s => s.facultyId).Distinct().ToList();

                foreach (var item2 in edurequests)
                {
                    EditEducationFields singlerequest = new EditEducationFields();
                    singlerequest.Id = item2.id;
                    singlerequest.facultyId = item2.facultyId;
                    singlerequest.FieldId = item2.fieldId;
                    singlerequest.Coursestudied = item2.courseStudied;
                    singlerequest.Specialization = item2.specialization;
                    singlerequest.PassedYear = item2.passedYear;
                    singlerequest.MarkasPercentage = item2.marksPercentage;
                    singlerequest.Division = item2.division;
                    singlerequest.BoardorUniversity = item2.boardOrUniversity;
                    singlerequest.PlaceofEducation = item2.placeOfEducation;
                    singlerequest.EducationcertificateView = item2.certificate;
                    singlerequest.AllSupportFilesView = item2.supportingDocs;
                    singlerequest.isApproved = item2.isApproved;
                    singlerequest.requestReason = item2.reason;
                    singlerequest.FieldDesc = jntuh_registered_faculty_edit_education_fields.Where(a => a.Id == item2.fieldId).Select(a => a.fieldDescription).FirstOrDefault();
                    EduRequests.Add(singlerequest);
                }
            }
            return View(EduRequests);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult AdminRequestApproval(int? id, string type)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string facultysupportfile = "~/Content/Upload/Faculty/FieldEditSupportDocuments/";
            if (id != null)
            {
                if (type == "Faculty")
                {
                    var FacultyRequest = db.jntuh_faculty_edit_requests.Where(q => q.id == id).Select(a => a).FirstOrDefault();
                    if (FacultyRequest != null)
                    {
                        FacultyRequest.isApproved = 1;
                        FacultyRequest.rejectReason = null;
                        FacultyRequest.deactivatedOn = DateTime.Now;
                        FacultyRequest.deactivatedBy = userId;
                        db.Entry(FacultyRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        //string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/Certificates1";
                        string NewDir = "C:/JNTUH/Certificates1/";
                        string PhotoPath = "D:/JNTUH/Prod/Content/Upload/Faculty/Photos/";
                        string AadhaarPath = "D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/";
                        string PANPath = "D:/JNTUH/Prod/Content/Upload/Faculty/PANCARDS/";
                        string IncomeTaxPath = "D:/JNTUH/Prod/Content/Upload/Faculty/INCOMETAX/";


                        var RegFaculty = db.jntuh_registered_faculty.Where(a => a.id == FacultyRequest.facultyId).Select(a => a).FirstOrDefault();
                        var Depts = GetDepartments();

                        string FileName = FacultyRequest.requestReason;
                        string fullpath = Path.Combine("D:/JNTUH/Content/Upload/Faculty/FieldEditSupportDocuments/", FileName);
                        var file = fullpath;

                        switch (FacultyRequest.fieldId)
                        {
                            case 5:
                                RegFaculty.FatherOrHusbandName = FacultyRequest.requestReason;
                                break;
                            case 6:
                                RegFaculty.MotherName = FacultyRequest.requestReason;
                                break;
                            case 7:
                                RegFaculty.GenderId = FacultyRequest.requestReason == "Male" ? 1 : 2;
                                break;
                            case 8:
                                string DOB = UAAAS.Models.Utilities.MMDDYY2DDMMYY(FacultyRequest.requestReason.ToString());
                                RegFaculty.DateOfBirth = Convert.ToDateTime(DOB);
                                break;
                            case 9:
                                RegFaculty.Mobile = FacultyRequest.requestReason;
                                break;
                            case 10:
                                RegFaculty.AadhaarNumber = FacultyRequest.requestReason;
                                break;
                            case 11:
                                RegFaculty.PANNumber = FacultyRequest.requestReason;
                                break;
                            case 12:
                                if (System.IO.File.Exists(fullpath))
                                {
                                    if (!System.IO.File.Exists(Path.Combine(PhotoPath, FileName)))
                                        System.IO.File.Copy(file, Path.Combine(PhotoPath, FileName));
                                }
                                RegFaculty.Photo = FileName.Trim();
                                break;
                            case 13:
                                if (System.IO.File.Exists(fullpath))
                                {
                                    if (!System.IO.File.Exists(Path.Combine(AadhaarPath, FileName)))
                                        System.IO.File.Copy(file, Path.Combine(AadhaarPath, FileName));
                                }
                                RegFaculty.AadhaarDocument = FileName.Trim();
                                break;
                            case 14:
                                if (System.IO.File.Exists(fullpath))
                                {
                                    if (!System.IO.File.Exists(Path.Combine(PANPath, FileName)))
                                        System.IO.File.Copy(file, Path.Combine(PANPath, FileName));
                                }
                                RegFaculty.PANDocument = FileName.Trim();
                                break;
                            case 15:
                                if (System.IO.File.Exists(fullpath))
                                {
                                    if (!System.IO.File.Exists(Path.Combine(IncomeTaxPath, FileName)))
                                        System.IO.File.Copy(file, Path.Combine(IncomeTaxPath, FileName));
                                }
                                RegFaculty.IncometaxDocument = FileName.Trim();
                                break;
                            case 16:
                                RegFaculty.DepartmentId = FacultyRequest.requestReason != null ? Depts.Where(s => s.departmentName == FacultyRequest.requestReason.Trim()).Select(a => a.id).FirstOrDefault() : 0;
                                break;
                            case 17:
                                RegFaculty.DesignationId = FacultyRequest.requestReason != null ? db.jntuh_designation.Where(s => s.designation == FacultyRequest.requestReason.Trim()).Select(a => a.id).FirstOrDefault() : 0;
                                break;
                            case 18:
                                var DOA = UAAAS.Models.Utilities.MMDDYY2DDMMYY(FacultyRequest.requestReason.ToString());
                                RegFaculty.DateOfAppointment = Convert.ToDateTime(DOA);
                                break;
                            case 23:
                                RegFaculty.AICTEFacultyId = FacultyRequest.requestReason;
                                break;
                            case 24:
                                RegFaculty.grosssalary = FacultyRequest.requestReason;
                                break;

                        }

                        db.Entry(RegFaculty).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["SUCCESS"] = "Faculty Request is Approved";
                        return RedirectToAction("FacultyRequestsForAdmin", new { Fid = UAAAS.Models.Utilities.EncryptString(FacultyRequest.facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                    {
                        TempData["ERROR"] = "Request Data is Not Found.";
                    }
                }
                else if (type == "Education")
                {
                    var FacultyEduRequest = db.jntuh_faculty_education_edit_request.Where(q => q.id == id).Select(a => a).FirstOrDefault();
                    if (FacultyEduRequest != null)
                    {
                        FacultyEduRequest.isApproved = 1;
                        FacultyEduRequest.deactivatedOn = DateTime.Now;
                        FacultyEduRequest.deactivatedBy = userId;
                        db.Entry(FacultyEduRequest).State = EntityState.Modified;
                        db.SaveChanges();

                        var RegFacultyEdu = db.jntuh_registered_faculty_education.Where(a => a.facultyId == FacultyEduRequest.facultyId).Select(a => a).ToList();
                        var EduObj = new jntuh_registered_faculty_education();


                        string certificatesPath = "D:/JNTUH/Prod/Content/Upload/Faculty/CERTIFICATES/";
                        string CertificateFileName = FacultyEduRequest.certificate;
                        string OldCopiedFilePath = Path.Combine(facultysupportfile, CertificateFileName);
                        string NewCopiedFilePath = Path.Combine(certificatesPath, CertificateFileName);
                        if (System.IO.File.Exists(OldCopiedFilePath))
                        {
                            var file = OldCopiedFilePath;
                            if (!System.IO.File.Exists(NewCopiedFilePath))
                                System.IO.File.Copy(file, NewCopiedFilePath);
                        }

                        if (FacultyEduRequest.fieldId == 39)
                        {
                            EduObj = RegFacultyEdu.Where(a => a.educationId == 1).Select(w => w).FirstOrDefault();
                            EduObj.educationId = 1;
                        }
                        else if (FacultyEduRequest.fieldId == 40)
                        {
                            EduObj = RegFacultyEdu.Where(a => a.educationId == 3).Select(w => w).FirstOrDefault();
                            EduObj.educationId = 3;
                        }
                        else if (FacultyEduRequest.fieldId == 41)
                        {
                            EduObj = RegFacultyEdu.Where(a => a.educationId == 4).Select(w => w).FirstOrDefault();
                            EduObj.educationId = 4;
                        }
                        else if (FacultyEduRequest.fieldId == 33)
                        {
                            EduObj = RegFacultyEdu.Where(a => a.educationId == 5).Select(w => w).FirstOrDefault();
                            EduObj.educationId = 5;
                        }
                        else if (FacultyEduRequest.fieldId == 34)
                        {
                            EduObj = RegFacultyEdu.Where(a => a.educationId == 6).Select(w => w).FirstOrDefault();
                            EduObj.educationId = 6;
                        }

                        EduObj.facultyId = FacultyEduRequest.facultyId;
                        EduObj.courseStudied = String.IsNullOrEmpty(FacultyEduRequest.courseStudied) ? FacultyEduRequest.courseStudied : FacultyEduRequest.courseStudied.Trim();
                        EduObj.specialization = String.IsNullOrEmpty(FacultyEduRequest.specialization) ? FacultyEduRequest.specialization : FacultyEduRequest.specialization.Trim();
                        EduObj.passedYear = FacultyEduRequest.passedYear == null ? 0 : (int)FacultyEduRequest.passedYear;
                        EduObj.marksPercentage = FacultyEduRequest.marksPercentage == null ? 0 : (decimal)FacultyEduRequest.marksPercentage;
                        EduObj.division = FacultyEduRequest.division == null ? 0 : (int)FacultyEduRequest.division;
                        EduObj.boardOrUniversity = String.IsNullOrEmpty(FacultyEduRequest.boardOrUniversity) ? FacultyEduRequest.boardOrUniversity : FacultyEduRequest.boardOrUniversity.Trim();
                        EduObj.placeOfEducation = String.IsNullOrEmpty(FacultyEduRequest.placeOfEducation) ? FacultyEduRequest.placeOfEducation : FacultyEduRequest.placeOfEducation.Trim();
                        EduObj.certificate = FacultyEduRequest.certificate;
                        EduObj.isActive = true;

                        if (RegFacultyEdu.Where(a => a.educationId == EduObj.educationId).Select(w => w).FirstOrDefault() == null)
                        {
                            EduObj.createdBy = userId;
                            EduObj.createdOn = DateTime.Now;
                            db.jntuh_registered_faculty_education.Add(EduObj);
                            db.SaveChanges();
                        }
                        else
                        {
                            EduObj.updatedBy = userId;
                            EduObj.updatedOn = DateTime.Now;
                            db.Entry(EduObj).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        TempData["SUCCESS"] = "Faculty Request is Approved";
                        return RedirectToAction("FacultyEducationRequestsForAdmin", new { Fid = UAAAS.Models.Utilities.EncryptString(FacultyEduRequest.facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                    {
                        TempData["ERROR"] = "Request Data is Not Found.";
                    }
                }
            }
            else
            {
                TempData["ERROR"] = "Something Went Wrong,Please Try Again.";
            }
            return RedirectToAction("AllRequestsListForAdmin");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult AdminRequestNotApproval(int? id, string type)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (id != null)
            {
                if (type == "Faculty")
                {
                    AdminRequestsClass GetRequest = new AdminRequestsClass();
                    var FacultyRequest = db.jntuh_faculty_edit_requests.Where(q => q.id == id).Select(a => a).FirstOrDefault();
                    if (FacultyRequest != null)
                    {
                        GetRequest.Id = FacultyRequest.id;
                        GetRequest.type = "Faculty";
                        GetRequest.FacultyId = FacultyRequest.facultyId;
                        GetRequest.FieldId = FacultyRequest.fieldId;
                        GetRequest.requestReason = FacultyRequest.requestReason;
                        GetRequest.isApproved = FacultyRequest.isApproved;
                        GetRequest.rejectReason = FacultyRequest.rejectReason;
                        return PartialView("~/Views/FacultyRequests/_AdminRequestNotApproval.cshtml", GetRequest);
                    }
                    else
                        TempData["ERROR"] = "Request Data is Not Found.";
                }
                else if (type == "Education")
                {
                    AdminRequestsClass GetEduRequest = new AdminRequestsClass();
                    var FacultyEduRequest = db.jntuh_faculty_education_edit_request.Where(q => q.id == id).Select(a => a).FirstOrDefault();
                    if (FacultyEduRequest != null)
                    {
                        GetEduRequest.Id = FacultyEduRequest.id;
                        GetEduRequest.type = "Education";
                        GetEduRequest.FacultyId = FacultyEduRequest.facultyId;
                        GetEduRequest.FieldId = FacultyEduRequest.fieldId;
                        GetEduRequest.requestReason = FacultyEduRequest.reason;
                        GetEduRequest.isApproved = FacultyEduRequest.isApproved;
                        GetEduRequest.rejectReason = FacultyEduRequest.reason;
                        return PartialView("~/Views/FacultyRequests/_AdminRequestNotApproval.cshtml", GetEduRequest);
                    }
                    else
                        TempData["ERROR"] = "Request Data is Not Found.";
                }
            }
            else
                TempData["ERROR"] = "Something Went Wrong,Please Try Again.";
            return RedirectToAction("AllRequestsListForAdmin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult AdminRequestNotApproval(AdminRequestsClass Request)
        {
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("LogOn", "Account");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (Request != null && Request.Id != 0 && Request.rejectReason != null)
            {
                if (Request.type == "Faculty")
                {
                    var FacultyRequest = db.jntuh_faculty_edit_requests.Where(q => q.id == Request.Id).Select(a => a).FirstOrDefault();
                    if (FacultyRequest != null)
                    {
                        FacultyRequest.isApproved = 2;
                        FacultyRequest.rejectReason = Request.rejectReason;
                        FacultyRequest.deactivatedOn = DateTime.Now;
                        FacultyRequest.deactivatedBy = userId;
                        db.Entry(FacultyRequest).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SUCCESS"] = "Faculty Request is Not Approved.";
                        return RedirectToAction("FacultyRequestsForAdmin", new { Fid = UAAAS.Models.Utilities.EncryptString(FacultyRequest.facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                        TempData["ERROR"] = "Request Data is Not Found.";
                }
                else if (Request.type == "Education")
                {
                    var FacultyRequest = db.jntuh_faculty_education_edit_request.Where(q => q.id == Request.Id).Select(a => a).FirstOrDefault();
                    if (FacultyRequest != null)
                    {
                        FacultyRequest.isApproved = 2;
                        FacultyRequest.reason = Request.rejectReason;
                        FacultyRequest.deactivatedOn = DateTime.Now;
                        FacultyRequest.deactivatedBy = userId;
                        db.Entry(FacultyRequest).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SUCCESS"] = "Education Request is Not Approved";
                        return RedirectToAction("FacultyRequestsForAdmin", new { Fid = UAAAS.Models.Utilities.EncryptString(FacultyRequest.facultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    else
                        TempData["ERROR"] = "Request Data is Not Found.";
                }
            }
            return RedirectToAction("AllRequestsListForAdmin");
        }

        //Print Faculty Edit Request in View
        public ActionResult FacultyRequestPDF(int preview, string strfacultyId)
        {
            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            if (!string.IsNullOrEmpty(strfacultyId))
            {
                int fid =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strfacultyId,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                // fid = 125662;
                var reg = db.jntuh_registered_faculty.Where(s => s.id == fid).Select(q => q.RegistrationNumber).FirstOrDefault();
                string pdfPath = string.Empty;
                if (preview == 0)
                {
                    pdfPath = SaveFacultyRequestPdf(preview, fid, reg);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                return File(pdfPath, "application/pdf", reg + "Edit_Request.pdf");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }

        public string SaveFacultyRequestPdf(int preview, int fid, string reg)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 50, 50);

            string path = Server.MapPath("~/Content/PDFReports/temp/FacultyRequestPrint");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/temp/FacultyRequestPrint")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/temp/FacultyRequestPrint"));
            }
            const int DelayOnRetry = 3000;
            try
            {
                if (preview == 0)
                {
                    fullPath = path + "/" + reg + "-Request" + ".pdf"; //
                    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                    ITextEvents iTextEvents = new ITextEvents();
                    iTextEvents.CollegeCode = reg;
                    iTextEvents.CollegeName = reg;
                    iTextEvents.formType = "Faculty Request";
                    pdfWriter.PageEvent = iTextEvents;
                }
            }
            catch (IOException e)
            {
                Thread.Sleep(DelayOnRetry);
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/FacultyRequest.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = contents.Replace("##ticketid##", "JF190330-ER152037");
            // contents = GetFacultyRequestData(fid, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
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
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;
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

        public string GetFacultyRequestData(int fid, string contents)
        {
            var facultydata = (from r in db.jntuh_registered_faculty
                               join f in db.jntuh_college_faculty_registered on r.RegistrationNumber equals f.RegistrationNumber into Regdata
                               from reg in Regdata.DefaultIfEmpty()
                               join ca in db.jntuh_college on reg.collegeId equals ca.id into rcdata
                               from rc in rcdata.DefaultIfEmpty()
                               join dep in db.jntuh_department on r.DepartmentId equals dep.id into rcdepdata
                               from rcd in rcdepdata.DefaultIfEmpty()
                               join des in db.jntuh_designation on r.DesignationId equals des.id into rcdepdesData
                               from rcdd in rcdepdesData.DefaultIfEmpty()
                               where r.id == fid
                               select new FacultyRegistration()
                               {
                                   id = r.id,
                                   CollegeId = reg.collegeId == null ? 0 : reg.collegeId,
                                   CollegeName = rc.collegeName == null ? "NotWorking" : rc.collegeName,
                                   Eid = r.UserId,
                                   Type = r.type,
                                   RegistrationNumber = r.RegistrationNumber,
                                   FirstName = r.FirstName,
                                   LastName = r.LastName,
                                   MiddleName = r.MiddleName,
                                   GenderId = r.GenderId,
                                   FatherOrhusbandName = r.FatherOrHusbandName,
                                   MotherName = r.MotherName,
                                   DateOfBirth = r.DateOfBirth,
                                   WorkingStatus = r.WorkingStatus,
                                   // OrganizationName = r.OrganizationName,
                                   DesignationId = r.DesignationId,
                                   designation = rcdd.designation,
                                   OtherDesignation = r.OtherDesignation,
                                   DepartmentId = r.DepartmentId,
                                   department = rcd.departmentName,
                                   OtherDepartment = r.OtherDesignation,
                                   GrossSalary = r.grosssalary,
                                   DateOfAppointment = r.DateOfAppointment,
                                   isFacultyRatifiedByJNTU = r.isFacultyRatifiedByJNTU,
                                   ProceedingsNo = r.ProceedingsNumber,
                                   SelectionCommitteeProcedings = r.ProceedingDocument,
                                   AICTEFacultyId = r.AICTEFacultyId,
                                   TotalExperience = r.TotalExperience,
                                   TotalExperiencePresentCollege = r.TotalExperiencePresentCollege,
                                   PANNumber = r.PANNumber,
                                   AadhaarNumber = r.AadhaarNumber != null ? r.AadhaarNumber.Substring(0, 8) + "XXXX" : "-",
                                   Mobile = r.Mobile,
                                   Email = r.Email,
                                   National = r.National,
                                   InterNational = r.InterNational,
                                   Awards = r.Awards,
                                   Citation = r.Citation,
                                   facultyPhoto = r.Photo,
                                   facultyPANCardDocument = r.PANDocument,
                                   facultyAadhaarCardDocument = r.AadhaarDocument,
                                   isActive = r.isActive,
                                   isApproved = r.isApproved,
                                   BlacklistFaculty = r.Blacklistfaculy,
                                   VerificationStatus = r.AbsentforVerification
                               }).ToList();

            var facultyeducationdata = (from educatgy in db.jntuh_education_category
                                        join edu in db.jntuh_registered_faculty_education on educatgy.id equals edu.educationId
                                        where
                                            edu.facultyId == fid &&
                                            (educatgy.id == 1 || educatgy.id == 3 || educatgy.id == 4 || educatgy.id == 5 || educatgy.id == 6)
                                        select new
                                        {
                                            id = edu.id,
                                            educationId = educatgy.id,
                                            facultyId = edu.facultyId,
                                            studiedEducation = edu.courseStudied,
                                            educationName = educatgy.educationCategoryName,
                                            specialization = edu.specialization,
                                            passedYear = edu.passedYear,
                                            marksPercentage = edu.marksPercentage,
                                            division = edu.division,
                                            boardOrUniversity = edu.boardOrUniversity,
                                            placeOfEducation = edu.placeOfEducation,
                                            certificte = edu.certificate,

                                        }).ToList();
            string contentdata = string.Empty;

            var Gender = "";
            var presentworking = "";
            if (facultydata[0].GenderId == 1)
            {
                Gender = "Male";
            }
            else
            {
                Gender = "FeMale";
            }

            if (facultydata[0].WorkingStatus == true)
            {
                presentworking = "Yes";
            }
            else
            {
                presentworking = "No";
            }

            string facultyphoto = "";

            string url = "";

            if (!string.IsNullOrEmpty(facultydata[0].facultyPhoto))
            {
                facultyphoto = "/Content/Upload/Faculty/PHOTOS/" + facultydata[0].facultyPhoto;
            }
            string imgpath = @"~" + facultyphoto.Replace("%20", " ");
            imgpath = System.Web.HttpContext.Current.Server.MapPath(imgpath);


            string facultyPAndoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].facultyPANCardDocument))
            {
                facultyPAndoc = "/Content/Upload/Faculty/PANCARDS/" + facultydata[0].facultyPANCardDocument;
            }
            string pandocpath = @"~" + facultyPAndoc.Replace("%20", " ");
            pandocpath = System.Web.HttpContext.Current.Server.MapPath(pandocpath);

            string facultyAadhaaardoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].facultyAadhaarCardDocument))
            {
                facultyAadhaaardoc = "/Content/Upload/Faculty/AADHAARCARDS/" + facultydata[0].facultyAadhaarCardDocument;
            }
            string aadhaardocpath = @"~" + facultyAadhaaardoc.Replace("%20", " ");
            aadhaardocpath = System.Web.HttpContext.Current.Server.MapPath(aadhaardocpath);

            string facultySCMdoc = "";
            if (!string.IsNullOrEmpty(facultydata[0].SelectionCommitteeProcedings))
            {
                facultySCMdoc = "/Content/Upload/Faculty/PROCEEDINGS/" + facultydata[0].SelectionCommitteeProcedings;
            }
            string scmdocpath = @"~" + facultySCMdoc.Replace("%20", " ");
            scmdocpath = System.Web.HttpContext.Current.Server.MapPath(scmdocpath);


            string Middlename = "--";
            if (!string.IsNullOrEmpty(facultydata[0].MiddleName))
            {
                Middlename = facultydata[0].MiddleName;
            }

            contentdata += "<div>";
            contentdata += "<p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>Jawaharlal Nerhu Technological University Hyderabad</p>";
            contentdata += "</div>";
            contentdata += "<div><p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>Directorate of Affiliations & Academic Audit </p></div>";
            contentdata += "<div><p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>Kukatpally, Hyderabad – 500 085, Telangana, India</p></div>";
            contentdata += "<br/>";
            if (facultydata[0].BlacklistFaculty == true)
                contentdata += "<p style='text-align:left;font-size:10px;'>You are Blacklisted due to possessing of ingenuine UG/PG/Ph.D. Certificates.</p><br/>";
            if (facultydata[0].VerificationStatus == true)
                contentdata += "<p style='text-align:left;font-size:10px;'>Your Candidature is made inactive due to your absence for physical verification.</p><br/>";
            contentdata += "<p style='text-align:left'><b><u>Faculty Registration Information :</b></u></p>";
            contentdata += "<br/><table border='0'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:10px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'
            contentdata += "<tr><td style='text-align:left' width='16%'>Registration ID&nbsp;&nbsp;:</td><td  style='text-align:left'  width='47%'>" +
            facultydata[0].RegistrationNumber + "</td>";



            if (imgpath != null)
            {
                string FacultyParsing = string.Empty;
                string strFacultyPhoto = string.Empty;
                var ServerPath = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + facultydata[0].facultyPhoto;

                // byte[] data;
                //WebClient client = new WebClient();
                //data = client.DownloadData(ServerPath);

                #region With-Out Html Parsing
                try
                {
                    if (!string.IsNullOrEmpty(imgpath))
                    {
                        FacultyParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(imgpath.Trim()) + "' align='center'  width='80' height='80' /></p>";
                        var ParseEliments = HTMLWorker.ParseToList(new StringReader(FacultyParsing), null);

                        if (imgpath.Contains("."))
                        {
                            strFacultyPhoto = "<img style='text-align:left'  alt=''src='" + HtmlEncoder.Encode(imgpath.Trim()) + "' height='80'  width='80' />";
                            contentdata += "<td rowspan=2  width='18%'><p align='center'>" + strFacultyPhoto + "</p></td>";
                        }
                        else
                        {
                            contentdata += "<td rowspan=2  width='18%'><p align='center'>&nbsp;</p></td>";
                        }
                    }
                    else
                    {
                        contentdata += "<td rowspan=2  width='18%'><p align='center'>&nbsp;</p></td>";
                    }
                }
                catch (Exception ex)
                {
                    contentdata += "<td rowspan=2  width='18%'><p align='center'>&nbsp;</p></td>";

                }
                #endregion
            }
            else
            {
                contentdata += "<td style='text-align:left'  rowspan=2  width='18%'>----</td>";
            }

            if (url != null)
            {
                contentdata += "<td rowspan=2  width='18%'><img style='text-align:left'  src='" + url +
                       "' height='80' Width='80'/></td>";
            }
            else
            {
                contentdata += "<td style='text-align:left'   rowspan=2 width='18%'>----</td>";
            }

            contentdata += "<tr>";
            contentdata += "<td style='text-align:left' width='16%'>FirstName&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<br/>MiddleName&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<br/>LastName&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</td><td style='text-align:left' width='47%'>" + facultydata[0].FirstName + "<br/>"
                + facultydata[0].MiddleName + "<br/>" + facultydata[0].LastName + "</td>";

            contentdata += "</tr>";
            contentdata += "</table>";


            contentdata += "<p style='text-align:left'><b><u>Faculty Basic Information :</b></u></p>";
            contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'   width='100%' class='auto' style='font-size:10px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'

            contentdata += "<tr><td style='text-align:left'>Father's/Husband's Name : </td><td style='text-align:left'>" +
                     facultydata[0].FatherOrhusbandName + "</td>";
            contentdata += "<td style='text-align:left'>Mother Name : </td><td style='text-align:left'>" + facultydata[0].MotherName +
                           "</td></tr>";

            contentdata += "<tr><td style='text-align:left'>Gender : </td><td style='text-align:left'>" + Gender + "</td>";
            contentdata += "<td style='text-align:left'>Date of Birth  : </td><td style='text-align:left'>" +
                           UAAAS.Models.Utilities.MMDDYY2DDMMYY(facultydata[0].DateOfBirth.ToString())
                           + "</td></tr>";
            contentdata += "<tr><td style='text-align:left'>Mobile No : </td><td style='text-align:left'>" +
                          facultydata[0].Mobile + "</td><td style='text-align:left'  width='13%'>Email :</td><td  style='text-align:left'  width='45%'>" +
                facultydata[0].Email + "</td></tr>";



            contentdata += "<tr><td style='text-align:left'>PAN Number :</td><td style='text-align:left'>"
                + facultydata[0].PANNumber + "</td><td style='text-align:left'>Aadhaar Number : </td><td style='text-align:left'>"
                + facultydata[0].AadhaarNumber +
                         "</td></tr>";

            contentdata += "<tr>";
            if (pandocpath != null)
            {
                contentdata += "<td style='text-align:left'>PAN  Document : </td><td style='text-align:left'>Yes</td>";
            }
            else
            {
                contentdata += "<td style='text-align:left'>PAN  Document : </td><td style='text-align:left'>No</td>";
            }

            if (aadhaardocpath != null)
            {
                contentdata += "<td style='text-align:left'>Aadhaaar Document  : </td><td style='text-align:left'>Yes</td>";
            }
            else
            {
                contentdata += "<td style='text-align:left'>Aadhaaar Document  : </td><td style='text-align:left'>No</td>";
            }
            contentdata += "</tr></table>";

            contentdata += "<p style='text-align:left'><b><u>Faculty College Information :</b></u></p>";

            if (facultydata[0].CollegeName != null || facultydata[0].CollegeName != "")
            {
                contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:10px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'

                contentdata += "<tr style='text-align:left' colspan=3 width='50%'><td>Name of the College presently working in</td><td style='text-align:left' colspan=3 width='50%'>" + facultydata[0].CollegeName + "</td></tr>";
                contentdata += "</tr></table>";
            }
            else
            {
                contentdata += "<div style='text-align:center'>No CollegeDetails Are Found</div>";
            }


            contentdata += "<table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:10px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'
            contentdata += "<tr><td style='text-align:left'>Department  : </td><td style='text-align:left'>" + facultydata[0].department +
                         "</td>";
            contentdata += "<td style='text-align:left'>Designation  : </td><td style='text-align:left'>" + facultydata[0].designation +
                           "</td></tr>";



            contentdata += "<tr><td style='text-align:left'>Experience in the present Institution (years)</td><td style='text-align:left'>" +
                facultydata[0].TotalExperiencePresentCollege + "</td><td style='text-align:left'>Total Experience  : </td><td style='text-align:left'>" +
                           facultydata[0].TotalExperience +
                           "</td></tr>";


            contentdata += "<tr><td style='text-align:left'>AICTE Faculty Id :</td><td style='text-align:left'>" + facultydata[0].AICTEFacultyId +
                           "</td><td style='text-align:left'>Gross Salary Last Drawn:</td><td style='text-align:left'>" +
                           facultydata[0].GrossSalary + "</td></tr>";

            contentdata += "<tr><td style='text-align:left'>Date of Appointment  : </td><td style='text-align:left'>" +
                         UAAAS.Models.Utilities.MMDDYY2DDMMYY(facultydata[0].DateOfAppointment.ToString()) +
                         "</td><td></td><td></td></tr>";

            contentdata += "</table>";

            contentdata += "<p style='text-align:left'><b><u>Faculty Education Information :</b></u></p>";
            if (facultyeducationdata.Count != 0 && facultyeducationdata.Count != null)
            {

                contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'
                contentdata += "<tr>";
                contentdata += "<td width='15%' style='text-align:left'>Education Name</td>";
                contentdata += "<td width='15%' style='text-align:left'>Course Studied</td>";
                contentdata += "<td width='10%' style='text-align:left'>Branch /Specialization</td>";
                contentdata += "<td width='10%' style='text-align:left'>Year of Passing</td>";
                contentdata += "<td width='10%' style='text-align:left'>% of Marks /CGPA</td>";
                contentdata += "<td width='10%' style='text-align:left'>Division</td>";
                contentdata += "<td width='12%' style='text-align:left'>Board /University</td>";
                contentdata += "<td width='8%' style='text-align:left'>Place</td>";
                contentdata += "<td width='10%' style='text-align:left'>Ceritificate</td>";
                contentdata += "</tr>";
                foreach (var item in facultyeducationdata)
                {
                    string facultycertificatedoc = "";
                    if (!string.IsNullOrEmpty(item.certificte))
                    {
                        facultycertificatedoc = "/Content/Upload/Faculty/CERTIFICATES/" + item.certificte;
                    }
                    string certificatedocpath = @"~" + facultycertificatedoc.Replace("%20", " ");
                    certificatedocpath = System.Web.HttpContext.Current.Server.MapPath(certificatedocpath);
                    string CerificateStatus = item.certificte == null ? "No" : "Yes";
                    contentdata += "<tr>";
                    contentdata += "<td width='15%' style='text-align:left'>" + item.educationName + "</td>";
                    contentdata += "<td width='15%' style='text-align:left'>" + item.studiedEducation + "</td>";
                    contentdata += "<td width='10%' style='text-align:left'>" + item.specialization + "</td>";
                    contentdata += "<td width='10%' style='text-align:left'>" + item.passedYear + "</td>";
                    contentdata += "<td width='10%' style='text-align:left'>" + item.marksPercentage + "</td>";
                    contentdata += "<td width='10%' style='text-align:left'>" + item.division + "</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + item.boardOrUniversity + "</td>";
                    contentdata += "<td width='8%' style='text-align:left'>" + item.placeOfEducation + "</td>";
                    contentdata += "<td width='10%' style='text-align:left'>" + CerificateStatus + "</td>";
                    contentdata += "</tr>";
                    //contentdata += "<td><img src='" + certificatedocpath + "' height='60px' width='40px'/></td></tr>";
                }
                contentdata += "</table><br/>";
            }
            else
            {
                contentdata += "<div style='text-align:center'>No EducationDetails Are Found</div>";
            }

            contents = contents.Replace("##COLLEGE_RANDOMCODE##", contentdata);
            return contents;
        }

        //Print Faculty Mobility Request in View
        public ActionResult FacultyMobiltyPDF(int preview, string strfacultyId)
        {
            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            if (!string.IsNullOrEmpty(strfacultyId))
            {
                int fid =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strfacultyId,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                // fid = 125662;
                var reg = db.jntuh_registered_faculty.Where(s => s.id == fid).Select(q => q.RegistrationNumber).FirstOrDefault();
                string pdfPath = string.Empty;
                if (preview == 0)
                {
                    pdfPath = SaveFacultyMobilityPdf(preview, fid, reg);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                return File(pdfPath, "application/pdf", reg + "Mobility_Request.pdf");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }

        }

        public string SaveFacultyMobilityPdf(int preview, int fid, string reg)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 50, 50);

            string path = Server.MapPath("~/Content/PDFReports/temp/FacultyMobilityPrint");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/temp/FacultyMobilityPrint")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/temp/FacultyMobilityPrint"));
            }
            const int DelayOnRetry = 3000;
            try
            {
                if (preview == 0)
                {
                    fullPath = path + "/" + reg + "-MobilityRequest" + ".pdf"; //
                    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                    ITextEvents iTextEvents = new ITextEvents();
                    iTextEvents.CollegeCode = reg;
                    iTextEvents.CollegeName = reg;
                    iTextEvents.formType = "Faculty Mobility Request";
                    pdfWriter.PageEvent = iTextEvents;
                }
            }
            catch (IOException e)
            {
                Thread.Sleep(DelayOnRetry);
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/FacultyMobility.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = contents.Replace("##ticketid##", "JFMR-190330-152037");
            // contents = GetFacultyRequestData(fid, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
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
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;
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

        [HttpGet]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult Faculty_suppDocs(string fid)
        {
            int fID = 0;
            int? userId = 0;
            if (Membership.GetUser(User.Identity.Name) == null)
                return RedirectToAction("Logon", "Account");
            else
                userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string RegistrationNumber = string.Empty;

            if (fid != null)
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (userId != null && userId != 0)
            {
                RegistrationNumber = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.RegistrationNumber).FirstOrDefault();
                fID = db.jntuh_registered_faculty.Where(a => a.RegistrationNumber == RegistrationNumber).Select(a => a.id).FirstOrDefault();
            }
            else
            {
                TempData["SUCCESS"] = null;
                return RedirectToAction("Logon", "Account");
            }

            //Code writen by siva for Faculty Edit Fields.
            var FacultyEditFieldsLinkId = db.jntuh_link_screens.Where(a => a.isActive == true && a.linkCode == "FSDU").Select(a => a.id).FirstOrDefault();
            DateTime? facultyEditFieldsfromdate = DateTime.Now;
            DateTime? facultyEditFieldsclosedate = DateTime.Now;
            if (FacultyEditFieldsLinkId != 0)
            {
                facultyEditFieldsfromdate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.fromdate).FirstOrDefault();
                facultyEditFieldsclosedate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultyEditFieldsLinkId && a.isActive == true).Select(s => s.todate).FirstOrDefault();
            }
            TempData["EditSupportingDocsLink"] = "false";
            var currentDate = DateTime.Now;
            if (currentDate > facultyEditFieldsclosedate)
                TempData["EditSupportingDocsLink"] = "true";


            var CertificateTypes = db.jntuh_complaints.Where(a => a.isActive == true && a.roleId == 7 && a.typeId == 3).Select(q => new { CertificateId = q.id, CertificateType = q.complaintType }).ToList();
            ViewBag.CertificateTypes = CertificateTypes;

           // var DegreeTypes = db.jntuh_degree.Where(a => a.isActive == true).Select(q => new { DegreeId = q.id, DegreeType = q.degree }).ToList();
           // ViewBag.DegreeTypes = DegreeTypes;

            FacultySuppDocsClass suppDocs = new FacultySuppDocsClass();
            suppDocs.FacultyId = fID;
            suppDocs.PlaceOfEducation = "";
            var supporting_documents =
                   db.jntuh_faculty_supporting_documents.Where(a => a.facultyId == suppDocs.FacultyId).Select(s => s).ToList();

            List<FacultySuppDocsClass> DocumentsList = new List<FacultySuppDocsClass>();
            var Jntuh__faculty_degree = db.jntuh_faculty_degree.Where(a => a.isActive == true).ToList();
            var jntuh_certificates_type = db.jntuh_complaints.Where(a => a.isActive == true && a.roleId == 7 && a.typeId == 3).Select(z => z).ToList();
            foreach (var item in supporting_documents)
            {
                FacultySuppDocsClass Docs = new FacultySuppDocsClass();
                Docs.academicyearId = item.academicyearId;
                Docs.CertificateTypeId = item.certificateTypeId;
                Docs.CertificateType = jntuh_certificates_type.Where(a=>a.id == item.certificateTypeId).Select(z=>z.complaintType).FirstOrDefault();
                Docs.DegreeId = item.facultydegreeId;
                Docs.Degree = Jntuh__faculty_degree.Where(q => q.id == item.facultydegreeId).Select(z => z.degreeName).FirstOrDefault();
                Docs.Specialization = item.specialization;
                Docs.AwardedUniversity = item.awardedUniversity;
                Docs.AwardedYear = item.awardedYear;
                Docs.PlaceOfEducation = item.place;
                Docs.facultyCertificate = item.certificate;
                Docs.facultyCertificateIssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.issusedDate.ToString());
                Docs.isApproved = item.isApproved;
                Docs.remarks = item.remarks;
                DocumentsList.Add(Docs);
            }
            ViewBag.DocumentsList = DocumentsList;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;


            #region Auto Populate Condition.
            var specializations = db.jntuh_specialization.Where(s => s.isActive == true).Select(a => a).ToList();

            List<string> specializationsData = new List<string>() { "Test" };
            foreach (var item1 in specializations)
            {
                if (!specializationsData.Contains(item1.specializationName))
                    specializationsData.Add(item1.specializationName);
            }

            suppDocs.ugspecializations = specializationsData;

            suppDocs.ugspecializations.Remove("Test");

            var Courses = db.jntuh_faculty_degree.Where(z => z.isActive == true).Select(q => new { DegreeId = q.id, DegreeType = q.degreeName }).ToList();
            ViewBag.DegreeTypes = Courses;

            var Universitys = db.jntuh_university.Where(a => a.isActive == true).Select(q => new { UId = q.universityName, University = q.universityName }).ToList();
            ViewBag.universitys = Universitys.GroupBy(n => new { n.University }).Select(a => a.First()).OrderBy(q=>q.University).ToList(); ;

            //var Places = db.jntuh_university.Where(a => a.isActive == true).Select(q => new { SId = q.state, State = q.state }).ToList();
            //ViewBag.places = Places.GroupBy(n => new { n.State}).Select(a=>a.First()).OrderBy(q=>q.State).ToList();
            #endregion

            return View(suppDocs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult Faculty_suppDocs(FacultySuppDocsClass suppDocs)
        {
            int fID = 0;
            int? userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualyear =
                db.jntuh_academic_year.Where(c => c.isActive == true && c.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(c => c.actualYear == (actualyear + 1)).Select(s => s.id).FirstOrDefault();
            string supportdocument = "~/Content/Upload/Faculty/SupportDocuments";

            //Code writen by siva for Faculty Edit Fields.
            var FacultySuppDocsLinkId = db.jntuh_link_screens.Where(a => a.isActive == true && a.linkCode == "FSDU").Select(a => a.id).FirstOrDefault();
            DateTime? facultySuppDocsclosedate = DateTime.Now; ;
            if (FacultySuppDocsLinkId != 0)
            {
                facultySuppDocsclosedate = db.jntuh_college_links_assigned.Where(a => a.linkId == FacultySuppDocsLinkId && a.isActive == true).Select(s => s.todate).FirstOrDefault();
            }
            
            var currentDate = DateTime.Now;
            if (currentDate > facultySuppDocsclosedate)
                return RedirectToAction("Faculty_suppDocs", new { fid = UAAAS.Models.Utilities.EncryptString(suppDocs.FacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });

            if (suppDocs != null)
            {
                jntuh_faculty_supporting_documents supporting_documents =
                    db.jntuh_faculty_supporting_documents.Where(a => a.facultyId == suppDocs.FacultyId && a.certificateTypeId == suppDocs.CertificateTypeId && a.facultydegreeId == suppDocs.DegreeId)
                        .Select(s => s)
                        .FirstOrDefault();
                
                if (suppDocs.facultyCertificateIssuedDate != null)
                    suppDocs.facultyCertificateIssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(suppDocs.facultyCertificateIssuedDate);

                if (supporting_documents == null)
                {
                    if (suppDocs.Certificate != null)
                    {
                        if (!Directory.Exists(Server.MapPath(supportdocument)))
                        {
                            Directory.CreateDirectory(Server.MapPath(supportdocument));
                        }
                        var ext = Path.GetExtension(suppDocs.Certificate.FileName);

                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            var Faculty = db.jntuh_registered_faculty.Where(a => a.id == suppDocs.FacultyId).Select(a => a).FirstOrDefault();
                            string fileName = suppDocs.CertificateTypeId + "-" + suppDocs.DegreeId + "-" + DateTime.Now.ToString("yyMMdd-HHmmss") + "-" +
                                        Faculty.FirstName.Substring(0, 1) + "-" + Faculty.LastName.Substring(0, 1);
                            suppDocs.Certificate.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(supportdocument), fileName, ext));
                            suppDocs.facultyCertificate = string.Format("{0}{1}", fileName, ext);
                        }
                    }
                    else if (string.IsNullOrEmpty(suppDocs.facultyCertificate))
                    {
                        suppDocs.facultyCertificate = suppDocs.facultyCertificate;
                    }
                    else
                    {
                        return RedirectToAction("Faculty_suppDocs", new { fid = UAAAS.Models.Utilities.EncryptString(suppDocs.FacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }

                    jntuh_faculty_supporting_documents Docs = new jntuh_faculty_supporting_documents();
                    Docs.academicyearId = ay0;
                    Docs.facultyId = suppDocs.FacultyId;
                    Docs.certificateTypeId = suppDocs.CertificateTypeId;
                    Docs.facultydegreeId = suppDocs.DegreeId;
                   // Docs.degreeId = 4;
                    Docs.specialization = String.IsNullOrEmpty(suppDocs.Specialization) ? suppDocs.Specialization : suppDocs.Specialization.Trim();
                    Docs.awardedYear = suppDocs.AwardedYear == null ? 0 : (int)suppDocs.AwardedYear;
                    Docs.awardedUniversity = String.IsNullOrEmpty(suppDocs.AwardedUniversity) ? suppDocs.AwardedUniversity : suppDocs.AwardedUniversity.Trim();
                    Docs.place = String.IsNullOrEmpty(suppDocs.PlaceOfEducation) ? suppDocs.PlaceOfEducation : suppDocs.PlaceOfEducation.Trim();
                    Docs.certificate = suppDocs.facultyCertificate;
                    Docs.issusedDate = Convert.ToDateTime(suppDocs.facultyCertificateIssuedDate);
                    Docs.isApproved = 0;
                    Docs.remarks = null;
                    Docs.isActive = true;
                    Docs.createdOn = DateTime.Now;
                    Docs.createdBy = userId;
                    db.jntuh_faculty_supporting_documents.Add(Docs);
                    db.SaveChanges();

                    TempData["SUCCESS"] = "Supporting documents are Uploaded.";
                }
                else
                {
                    TempData["ERROR"] = "Document is Already Exists";
                }
                //else
                //{
                //    supporting_documents.certificateTypeId = suppDocs.CertificateTypeId;
                //    supporting_documents.degreeId = suppDocs.DegreeId;
                //    supporting_documents.certificate = suppDocs.facultyCertificate;
                //    supporting_documents.issusedDate = Convert.ToDateTime(suppDocs.facultyCertificateIssuedDate);
                //    supporting_documents.isApproved = 0;
                //    supporting_documents.updatedOn = DateTime.Now;
                //    supporting_documents.updatedBy = userId;
                //    db.Entry(supporting_documents).State = EntityState.Modified;
                //    db.SaveChanges();

                //    TempData["SUCCESS"] = "Record is Updated Sucessfully.";
                //}
            }
            return RedirectToAction("Faculty_suppDocs", new { fid = UAAAS.Models.Utilities.EncryptString(suppDocs.FacultyId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        public JsonResult GetState(string UniversityName)
        {
            var statename = string.Empty;
            if(!String.IsNullOrEmpty(UniversityName))
            {
                statename = db.jntuh_university.Where(a => a.universityName == UniversityName.Trim()).Select(a => a.state).FirstOrDefault();
                return Json(new { data = statename } ,JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { data = statename }, JsonRequestBehavior.AllowGet);
        }

    }
}
