using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    public class CollegeFacultyLeavesController : BaseController
    {
        //
        // GET: /CollegeFacultyLeaves/
        private uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index(string fid)
        {
            //return RedirectToAction("Index", "UnderConstruction");
            int fID = 0;
            if (fid!=null)
            {
                fID =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            
            var leavetyps =
                db.jntuh_leavetype.Where(l => l.isActive == true)
                    .Select(a => new {Id = a.id, LeaveType = a.leavetype})
                    .ToList();
            ViewBag.LeaveTyps =leavetyps;
            Facultyleave facultyleaves= new Facultyleave();
            facultyleaves.facultyId = fID;
            jntuh_registered_faculty registered_faculty = db.jntuh_registered_faculty.Where(r => r.id == fID).Select(s => s).FirstOrDefault();

            if (registered_faculty!=null)
            {
                facultyleaves.RegistrationNumber = registered_faculty.RegistrationNumber.Trim();
                facultyleaves.Firstname = registered_faculty.FirstName;
                facultyleaves.Middlename = registered_faculty.MiddleName;
                facultyleaves.Lastname = registered_faculty.LastName;
            }
            

            List<Facultyleave> llist=new List<Facultyleave>();
            List<jntuh_collegefaculty_leaves> collegefacultyleaves = db.jntuh_collegefaculty_leaves.Where(r => r.RegistrationNumber == facultyleaves.RegistrationNumber).ToList();
            if (collegefacultyleaves.Count!=0)
            {
                foreach (var item in collegefacultyleaves)
                {
                    Facultyleave facultyleave = new Facultyleave();
                    facultyleave.id = item.id;
                    facultyleave.isDelete = true;
                    facultyleave.CollegeId = item.collegeId;
                    facultyleave.Leavetypeid = item.leavetypeid;
                    facultyleave.Leavetype = db.jntuh_leavetype.Where(l => l.id == item.leavetypeid).Select(s=>s.leavetype).FirstOrDefault();
                    facultyleave.RegistrationNumber = item.RegistrationNumber.Trim();
                    facultyleave.SLeavefromdate = item.leavefromdate.ToString().Split(' ')[0];
                    facultyleave.SLeavetodate = item.leavetodate.ToString().Split(' ')[0];
                    int Toyear = Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[2]);
                    int Tomonth = Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[0]);
                    int Tomdate = (Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[1]));
                    DateTime DOA = new DateTime(Toyear, Tomonth, Tomdate);
                    DOA = DOA.AddDays(2);
                    string presentdate = DateTime.Now.ToShortDateString();
                    int Pyear = Convert.ToInt32(presentdate.Split('/')[2]);
                    int Pmonth = Convert.ToInt32(presentdate.Split('/')[0]);
                    int Pmdate = Convert.ToInt32(presentdate.Split('/')[1]);
                    DateTime PDL = new DateTime(Pyear, Pmonth, Pmdate);
                    
                    if (PDL > DOA)
                    {
                        facultyleave.isDelete = false;
                    }
                    facultyleave.Documentview = item.document;
                    facultyleave.noofdays = item.noofdays;
                    llist.Add(facultyleave);
                    
                }
            }
            facultyleaves.facultyleaveslist = llist;
            return View(facultyleaves);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Index(Facultyleave facultyleaves)
        {
            //int usercollegeId = 0;
            //return RedirectToAction("Index", "UnderConstruction");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string sfid = UAAAS.Models.Utilities.EncryptString(facultyleaves.facultyId.ToString(),
               WebConfigurationManager.AppSettings["CryptoKey"]);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (facultyleaves.RegistrationNumber!=null)
            {
                jntuh_registered_faculty jntuh_registered_faculty =
                    db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == facultyleaves.RegistrationNumber)
                        .Select(s => s)
                        .FirstOrDefault();
                if (jntuh_registered_faculty.Blacklistfaculy == true || jntuh_registered_faculty.AbsentforVerification == true)
                {
                    TempData["ERROR"] = "This Registration Number is in Blacklist.";
                    return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
                }
            }
            else
            {
                TempData["ERROR"] = "Registration Number can not be empty.";
                return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
            }
            if (facultyleaves != null)
            {
                var facultyleavedocumentspath = "~/Content/Upload/Faculty/LeaveDocuments";
                if (facultyleaves.Documentfile != null)
                {
                    if (!Directory.Exists(Server.MapPath(facultyleavedocumentspath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(facultyleavedocumentspath));
                    }

                    var ext = Path.GetExtension(facultyleaves.Documentfile.FileName);

                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string filename = facultyleaves.RegistrationNumber.Trim() + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") +"_CL";
                        facultyleaves.Documentfile.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(facultyleavedocumentspath), filename, ext));
                        facultyleaves.Documentview = string.Format("{0}{1}", filename, ext);
                    }
                }
                else
                {
                    //Experience.PresentfacultyJoiningDocument = Experience.PresentfacultyJoiningDocument;
                    TempData["ERROR"] = "File can not be empty.";
                    return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
                }

               jntuh_collegefaculty_leaves savedata=new jntuh_collegefaculty_leaves();
               savedata.collegeId = userCollegeID;
               savedata.academicYearId = ay0;
                savedata.leavetypeid = facultyleaves.Leavetypeid;
                savedata.RegistrationNumber =facultyleaves.RegistrationNumber.Trim();
                if (facultyleaves.SLeavefromdate!=null)
                    savedata.leavefromdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(facultyleaves.SLeavefromdate);
                if (facultyleaves.SLeavetodate!=null)
                    savedata.leavetodate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(facultyleaves.SLeavetodate);
                TimeSpan days = (savedata.leavetodate- savedata.leavefromdate);
                savedata.noofdays =((days).Days)+1;
                savedata.document = facultyleaves.Documentview;
                savedata.createdOn = DateTime.Now;
                savedata.createdBy = 1;
                savedata.updatedOn = null;
                savedata.updatedBy = null;
                if (savedata!=null)
                {
                 db.jntuh_collegefaculty_leaves.Add(savedata);
                db.SaveChanges();
                TempData["SUCCESS"] = "Leave applied Successfully.";
                return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
                }
               
            }
           
            return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
        }

        //Check Leaves from date and to date
        public ActionResult CheckLeavesDates(int facultyId, string facultyfromdate, string facultytodate)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);

            if (userdata == null)
            {
                return RedirectToAction("FacultyExperience", "NewOnlineRegistration");
            }

            var status = false;
            var message = string.Empty;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var facultyid = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(a => a.id).FirstOrDefault();

            if (facultyfromdate != null && facultytodate != null)
            {
                int Fromyear = Convert.ToInt32(facultyfromdate.Split('/')[2]);
                int Frommonth = Convert.ToInt32(facultyfromdate.Split('/')[1]);
                int Fromdate = Convert.ToInt32(facultyfromdate.Split('/')[0]);

                DateTime DOA = new DateTime(Fromyear, Frommonth, Fromdate);

                int Toyear = Convert.ToInt32(facultytodate.Split('/')[2]);
                int Tomonth = Convert.ToInt32(facultytodate.Split('/')[1]);
                int Todate = Convert.ToInt32(facultytodate.Split('/')[0]);

                DateTime DOR = new DateTime(Toyear, Tomonth, Todate);

                if (DOA > DOR)
                {
                    status = true;
                    message = "From Date should be less than to Date";
                    return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
                }
                var regno =
                    db.jntuh_registered_faculty.Where(r => r.id == facultyId)
                        .Select(s => s.RegistrationNumber)
                        .FirstOrDefault();
                //var Experience = db.jntuh_registered_faculty_experience.Where(s => s.facultyId == facultyid && s.facultyDateOfResignation != null).Select(e => e).ToList();

                var Leaves = db.jntuh_collegefaculty_leaves.Where(s => s.RegistrationNumber.Trim() == regno.Trim()).Select(e => e).ToList();

                if (Leaves.Count != 0)
                {
                    foreach (var item in Leaves)
                    {
                        //string Appointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.facultyDateOfAppointment.ToString());
                        //string Resignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.facultyDateOfResignation.ToString());

                        string From = item.leavefromdate.ToString().Split(' ')[0];
                        string To = item.leavetodate.ToString().Split(' ')[0];

                        int Appyear = Convert.ToInt32(From.Split('/')[2]);
                        int Appmonth = Convert.ToInt32(From.Split('/')[0]);
                        int Appdate = Convert.ToInt32(From.Split('/')[1]);

                        DateTime DateOfApp = new DateTime(Appyear, Appmonth, Appdate);

                        int Resignyear = Convert.ToInt32(To.Split('/')[2]);
                        int Resignmonth = Convert.ToInt32(To.Split('/')[0]);
                        int Resigndate = Convert.ToInt32(To.Split('/')[1]);

                        DateTime DateOfResign = new DateTime(Resignyear, Resignmonth, Resigndate);

                        if (DateOfApp >= DOA && DateOfResign <= DOR)
                        {
                            status = true;
                            //message = "You are already working in this peroid as per your claim";
                            message = "You have already applied leave in this peroid as per your claim";
                            break;
                        }
                        if (DateOfApp >= DOA && DateOfResign >= DOR)
                        {
                            if (DOA == DOR)
                            {
                                //status = true;
                                //message = "";
                                // break;
                            }
                            //else if (DOA <= DateOfApp && DOR <= DateOfApp)
                            //{
                            //    status = false;
                            //    message = "";
                            //    // break;
                            //}
                            else if (DOA <= DateOfApp && DOR <= DateOfApp)
                            {
                                status = false;
                                message = "";
                                // break;
                            }
                            else
                            {
                                status = true;
                                message = "You have already applied leave in this peroid as per your claim";
                                break;
                            }
                        }
                        if (DateOfApp <= DOA && DateOfResign <= DOR)
                        {
                            if (DOA == DOR)
                            {
                                //status = true;
                                //message = "";
                                //break;
                            }
                            //else if (DOA >= DateOfResign && DOR > DateOfResign)
                            //{
                            //    status = false;
                            //    message = "";
                            //    // break;
                            //}
                            else if (DOA > DateOfResign && DOR > DateOfResign)
                            {
                                status = false;
                                message = "";
                                // break;
                            }
                            else
                            {
                                status = true;
                                message = "You have already applied leave in this peroid as per your claim";
                                break;
                            }
                        }
                        if (DateOfApp <= DOA && DateOfResign >= DOR)
                        {
                            status = true;
                            message = "You have already applied leave in this peroid as per your claim";
                            break;
                        }
                    }
                    return Json(new { status = status, message = message }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = false, message = message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = true, message = "Please Select Date" }, JsonRequestBehavior.AllowGet);
        }
    
        //Delete Leave Data
        [Authorize(Roles = "Admin,College")]
        public ActionResult DeleteLeaveData(int id)
        {
            string sfid = string.Empty;
            //string sfid = UAAAS.Models.Utilities.EncryptString(facultyleaves.facultyId.ToString(),
            //   WebConfigurationManager.AppSettings["CryptoKey"]);
            if (id != 0 && id!=null)
            {
                jntuh_collegefaculty_leaves deleteleave =
                    db.jntuh_collegefaculty_leaves.Where(l => l.id == id).Select(s => s).FirstOrDefault();
                int facultyid =
                      db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == deleteleave.RegistrationNumber)
                          .Select(s => s.id)
                          .FirstOrDefault();
                sfid = UAAAS.Models.Utilities.EncryptString(facultyid.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);

                //Check Delete Date Condition
                Facultyleave facultyleave= new Facultyleave();
                facultyleave.SLeavefromdate = deleteleave.leavefromdate.ToString().Split(' ')[0];
                facultyleave.SLeavetodate = deleteleave.leavetodate.ToString().Split(' ')[0];
                int Toyear = Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[2]);
                int Tomonth = Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[0]);
                int Tomdate = (Convert.ToInt32(facultyleave.SLeavetodate.Split('/')[1]));
                DateTime DOA = new DateTime(Toyear, Tomonth, Tomdate);
                DOA = DOA.AddDays(2);
                string presentdate = DateTime.Now.ToShortDateString();
                int Pyear = Convert.ToInt32(presentdate.Split('/')[2]);
                int Pmonth = Convert.ToInt32(presentdate.Split('/')[0]);
                int Pmdate = Convert.ToInt32(presentdate.Split('/')[1]);
                DateTime PDL = new DateTime(Pyear, Pmonth, Pmdate);

                if (PDL > DOA)
                {
                    //facultyleave.isDelete = false;
                    TempData["ERROR"] = "you can not delete leave.";
                    return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
                }

                if (deleteleave!=null)
                {
                  
                    db.jntuh_collegefaculty_leaves.Remove(deleteleave);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Leave request deleted Successfully.";
                    return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
                }
                //string regno=db.jntuh_collegefaculty_leaves.Where(l=>l.)
            }
            return RedirectToAction("Index", "CollegeFacultyLeaves", new { fid = sfid });
        }
    }
}
