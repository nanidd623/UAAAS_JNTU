using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;
using WebGrease.Css.Extensions;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeComplaintsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeComplaints/
        #region College Complaints Taken by Role id-4 written by Narayana Reddy
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_college_complaints = db.jntuh_college_complaints.Where(c => c.roleId == 4).Select(s => s).ToList();
            List<ComplaintsColleges> ComplaintsCollegeslist = new List<ComplaintsColleges>();
            foreach (var item in colleges)
            {
                ComplaintsColleges Complaints = new ComplaintsColleges();
                Complaints.id = item.id;
                Complaints.collegecode = item.collegeCode;
                Complaints.collegename = item.collegeName;
                Complaints.complaintcount = jntuh_college_complaints.Where(l => l.college_faculty_Id == Complaints.id).Select(s => s).Count();
                Complaints.nocomplaintpathcount = jntuh_college_complaints.Where(l => l.college_faculty_Id == Complaints.id && l.complaintFile == null).Select(s => s).Count();
                Complaints.isActive = item.isActive;
                ComplaintsCollegeslist.Add(Complaints);
            }
            return View(ComplaintsCollegeslist);
        }


        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeComplaint(int collegeid, int? complaintid)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            Complaints complaintsnew = new Complaints();
            if (collegeid != null && collegeid != 0)
            {
                List<jntuh_complaints> jntucomplaints =
               db.jntuh_complaints.Where(r => r.roleId == 4 && r.isActive == true).Select(s => s).ToList();
                List<jntuh_complaints_givenby> jntuh_complaintsgivenby =
                    db.jntuh_complaints_givenby.Where(r => r.roleId == 4 && r.isActive == true).Select(s => s).ToList();

                ViewBag.Complaints = jntucomplaints;
                ViewBag.Complaintsgivenby = jntuh_complaintsgivenby;
                List<jntuh_college_complaints> collegecomplaints =
                    db.jntuh_college_complaints.Where(s => s.college_faculty_Id == collegeid && s.roleId == 4).Select(s => s).ToList();
                List<ComplaintsList> totalcomplaints = new List<ComplaintsList>();
                jntuh_college jntuhcollege =
                    db.jntuh_college.Where(c => c.id == collegeid && c.isActive == true).Select(s => s).FirstOrDefault();
                if (complaintid != 0 && complaintid != null)
                {
                    List<jntuh_subcomplaints> jntusubcomplaints =
              db.jntuh_subcomplaints.Where(r => r.isActive == true && r.complaintId == complaintid && r.roleid == 4).Select(s => s).ToList();
                    foreach (var item in jntusubcomplaints)
                    {
                        ComplaintsList Complaints = new ComplaintsList();
                        Complaints.complaintid = Convert.ToInt32(item.complaintId);
                        Complaints.subcomplaint = false;
                        Complaints.subcomplaintid = item.id;
                        //Complaints.nooftimes = collegecomplaints.Where(a => a.complaintId == item.id).Select(s => s).Count();
                        Complaints.subcomplaintname = item.subcomplaintType;
                        totalcomplaints.Add(Complaints);
                    }
                }

                if (jntuhcollege != null)
                {
                    complaintsnew.collegeId = collegeid;
                    complaintsnew.collegeCode = jntuhcollege.collegeCode;
                    complaintsnew.collegeName = jntuhcollege.collegeName;
                }
                complaintsnew.complaintsdata = totalcomplaints;
                //return View(totalcomplaints);
                return View("CollegeComplaint", complaintsnew);
            }
            return RedirectToAction("Index");

            //Complaints complaintsnew = new Complaints();
            //if (collegeid != null && collegeid != 0)
            //{
            //    List<jntuh_complaints> jntucomplaints =
            //   db.jntuh_complaints.Where(r => r.isActive == true).Select(s => s).ToList();
            //    List<jntuh_college_complaints> collegecomplaints =
            //        db.jntuh_college_complaints.Where(s => s.collegeId == collegeid).Select(s => s).ToList();
            //    List<ComplaintsList> totalcomplaints = new List<ComplaintsList>();
            //    jntuh_college jntuhcollege =
            //        db.jntuh_college.Where(c => c.id == collegeid && c.isActive == true).Select(s => s).FirstOrDefault();
            //    foreach (var item in jntucomplaints)
            //    {
            //        ComplaintsList Complaints = new ComplaintsList();
            //        Complaints.complaintid = item.id;
            //        Complaints.complaint = false;
            //        Complaints.nooftimes = collegecomplaints.Where(a => a.complaintId == item.id).Select(s => s).Count();
            //        Complaints.complaintname = item.complaintType;
            //        totalcomplaints.Add(Complaints);
            //    }
            //    if (jntuhcollege != null)
            //    {
            //        complaintsnew.collegeId = collegeid;
            //        complaintsnew.collegeCode = jntuhcollege.collegeCode;
            //        complaintsnew.collegeName = jntuhcollege.collegeName;
            //    }
            //    complaintsnew.complaintsdata = totalcomplaints;
            //    //return View(totalcomplaints);
            //    return PartialView("CollegeComplaint", complaintsnew);
            //}
            //return RedirectToAction("Index");

        }
        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeComplaint(Complaints Complaintsnew)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (Complaintsnew != null)
            {
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                string complaintFilepath = "~/Content/Upload/Complaints/ComplaintFile";
                string replyFilepath = "~/Content/Upload/Complaints/ReplyFile";

                if (Complaintsnew.complaintid != 0)
                {
                    List<jntuh_complaints> jntuh_complaints =
                        db.jntuh_complaints.Where(c => c.isActive == true).Select(s => s).ToList();
                    jntuh_college_complaints collegeComplaints = new jntuh_college_complaints();
                    collegeComplaints.roleId = 4;
                    collegeComplaints.college_faculty_Id = Complaintsnew.collegeId;
                    collegeComplaints.academicyearId = ay0;
                    collegeComplaints.complaintId = Complaintsnew.complaintid;
                    Complaintsnew.complaintname =
                        jntuh_complaints.Where(c => c.id == Complaintsnew.complaintid)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    if (Complaintsnew.complaintname == "Others")
                    {
                        collegeComplaints.otherComplaint = Complaintsnew.otherscomplaint.Trim();
                    }
                    collegeComplaints.subcomplaintId = 0;
                    collegeComplaints.replayStatus = Complaintsnew.replaystatus;
                    if (Complaintsnew.complaintdate != null)
                        collegeComplaints.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.complaintdate);
                    if (Complaintsnew.replaystatusdate != null)
                        collegeComplaints.replayDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.replaystatusdate);

                    if (Complaintsnew.givenById != 0)
                    {
                        Complaintsnew.givenBy = db.jntuh_complaints_givenby.Where(g => g.id == Complaintsnew.givenById && g.roleId == 4).Select(s => s.givenbyName).FirstOrDefault();
                        if (Complaintsnew.OthergivenBy != null)
                        {
                            Complaintsnew.OthergivenBy = Regex.Replace(Complaintsnew.OthergivenBy, @"\r|\n|\t", "");
                            Complaintsnew.OthergivenBy = Complaintsnew.OthergivenBy.Trim();
                            collegeComplaints.givenBy = Complaintsnew.givenBy + "-" + Complaintsnew.OthergivenBy;
                        }
                        else
                        {
                            collegeComplaints.givenBy = Complaintsnew.givenBy;
                        }
                    }

                    //Complaint File Save
                    if (Complaintsnew.complaintFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                        }

                        var ext = Path.GetExtension(Complaintsnew.complaintFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = Complaintsnew.collegeCode + "-" + Complaintsnew.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                            Complaintsnew.complaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                            Complaintsnew.CollegecomplaintFile = string.Format("{0}{1}", filename, ext);
                        }
                    }
                    //Complaint File Save
                    if (Complaintsnew.replyFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(replyFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(replyFilepath));
                        }

                        var ext = Path.GetExtension(Complaintsnew.replyFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = Complaintsnew.collegeCode + "-" + Complaintsnew.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_RF";
                            Complaintsnew.replyFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(replyFilepath), filename, ext));
                            Complaintsnew.CollegereplyFile = string.Format("{0}{1}", filename, ext);
                        }
                    }
                    collegeComplaints.complaintFile = Complaintsnew.CollegecomplaintFile;
                    collegeComplaints.replayFile = Complaintsnew.CollegereplyFile;
                    collegeComplaints.nooftimes = 1;
                    collegeComplaints.remarks = Complaintsnew.remarks;
                    collegeComplaints.isActive = true;
                    collegeComplaints.createdBy = userID;
                    collegeComplaints.createdOn = DateTime.Now;
                    db.jntuh_college_complaints.Add(collegeComplaints);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.collegeName;
                    if (Complaintsnew.complaintsdata != null)
                    {
                        if (Complaintsnew.complaintsdata.Where(a => a.subcomplaint == true).Select(s => s.subcomplaint).FirstOrDefault() == true)
                        {
                            List<jntuh_subcomplaints> jntuh_subcomplaints =
                                db.jntuh_subcomplaints.Where(s => s.isActive == true && s.roleid == 4).Select(s => s).ToList();
                            foreach (var item in Complaintsnew.complaintsdata.Where(a => a.subcomplaint == true).Select(s => s).ToList())
                            {
                                jntuh_college_complaints subcollegeComplaints = new jntuh_college_complaints();
                                subcollegeComplaints.college_faculty_Id = Complaintsnew.collegeId;
                                subcollegeComplaints.academicyearId = ay0;
                                subcollegeComplaints.complaintId = Complaintsnew.complaintid;
                                subcollegeComplaints.subcomplaintId = item.subcomplaintid;
                                item.subcomplaintname = jntuh_subcomplaints.Where(c => c.id == item.subcomplaintid).Select(s => s.subcomplaintType).FirstOrDefault();
                                if (item.subcomplaintname == "Others")
                                {
                                    subcollegeComplaints.otherComplaint = item.othersubcomplaint;
                                }
                                subcollegeComplaints.replayStatus = Complaintsnew.replaystatus;
                                if (Complaintsnew.complaintdate != null)
                                    subcollegeComplaints.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.complaintdate);
                                if (Complaintsnew.replaystatusdate != null)
                                    subcollegeComplaints.replayDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.replaystatusdate);
                                subcollegeComplaints.givenBy = Complaintsnew.givenBy;
                                subcollegeComplaints.nooftimes = 1;
                                subcollegeComplaints.remarks = Complaintsnew.remarks;
                                subcollegeComplaints.isActive = true;
                                subcollegeComplaints.createdBy = userID;
                                subcollegeComplaints.createdOn = DateTime.Now;
                                subcollegeComplaints.updatedBy = null;
                                subcollegeComplaints.updatedOn = null;
                                db.jntuh_college_complaints.Add(subcollegeComplaints);
                                db.SaveChanges();
                                TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.collegeName;
                            }
                        }
                    }
                    //TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.collegeName;
                }
                //if (Complaintsnew.complaintsdata.Where(a => a.subcomplaint == true).Select(s => s.subcomplaint).FirstOrDefault() == true)
                //{
                //    foreach (var item in Complaintsnew.complaintsdata.Where(a => a.subcomplaint == true).Select(s => s).ToList())
                //    {
                //        jntuh_college_complaints collegeComplaints= new jntuh_college_complaints();
                //        collegeComplaints.collegeId = Complaintsnew.collegeId;
                //        collegeComplaints.complaintId = item.complaintid;
                //        collegeComplaints.nooftimes = 1;
                //        collegeComplaints.remarks =item.remarks ;
                //        collegeComplaints.isActive = true;
                //        collegeComplaints.createdBy =1;
                //        collegeComplaints.createdOn = DateTime.Now;
                //        db.jntuh_college_complaints.Add(collegeComplaints);
                //        db.SaveChanges();
                //        TempData["SUCCESS"] ="Complaint Added to "+Complaintsnew.collegeName;
                //    }
                //}
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult ViewCollegeComplaint(int? collegeid)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (collegeid != 0)
            {
                var college =
                       db.jntuh_college.Where(c => c.id == collegeid).Select(s => s).FirstOrDefault();
                if (college != null)
                {
                    ViewBag.CollegeName = college.collegeName;
                    ViewBag.CollegeCode = college.collegeCode;
                }
                List<Complaints> Complaintslist = new List<Complaints>();
                List<jntuh_college_complaints> collegeComplaints =
                    db.jntuh_college_complaints.Where(c => c.college_faculty_Id == collegeid && c.subcomplaintId == 0 && c.roleId == 4).Select(s => s).ToList();
                foreach (var citem in collegeComplaints)
                {
                    Complaints newcomplaint = new Complaints();
                    newcomplaint.Id = citem.id;
                    newcomplaint.collegeId = citem.college_faculty_Id;


                    newcomplaint.complaintid = Convert.ToInt32(citem.complaintId);
                    newcomplaint.complaintname =
                        db.jntuh_complaints.Where(c => c.id == newcomplaint.complaintid)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    if (newcomplaint.complaintname == "Others")
                    {
                        newcomplaint.complaintname = newcomplaint.complaintname + "-" + citem.otherComplaint.Trim();
                    }
                    newcomplaint.dcomplaintdate = citem.complaintDate;
                    newcomplaint.complaintdate = citem.complaintDate.ToString().Split(' ')[0];
                    newcomplaint.replaystatusdate = citem.replayDate.ToString().Split(' ')[0];
                    newcomplaint.replaystatus = (bool)citem.replayStatus;
                    newcomplaint.givenBy = citem.givenBy;
                    newcomplaint.CollegecomplaintFile = citem.complaintFile;
                    newcomplaint.CollegereplyFile = citem.replayFile;
                    newcomplaint.actionstatusName = citem.complaintOn;
                    Complaintslist.Add(newcomplaint);
                }
                return View("ViewCollegeComplaint", Complaintslist);
            }
            return RedirectToAction("Index");
        }

        //Delete College Complaint
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult DeleteCollegeComplaint(int id, int collegeid, int complaintId, DateTime dateofcomplaint)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (id != 0 && complaintId != 0 && collegeid != 0)
            {
                jntuh_college_complaints deletecollegecompalints =
                 db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.id == id && c.subcomplaintId == 0 && c.complaintDate == dateofcomplaint && c.roleId == 4)
                     .Select(s => s)
                     .FirstOrDefault();
                List<jntuh_college_complaints> deletesubcollegecompalints =
              db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.id == deletecollegecompalints.college_faculty_Id && c.subcomplaintId != 0 && c.complaintDate == dateofcomplaint && c.roleId == 4)
                  .Select(s => s)
                  .ToList();
                if (deletesubcollegecompalints.Count != 0)
                {
                    foreach (var deletecomplaint in deletesubcollegecompalints)
                    {
                        db.jntuh_college_complaints.Remove(deletecomplaint);
                        db.SaveChanges();
                    }
                }
                db.jntuh_college_complaints.Remove(deletecollegecompalints);
                db.SaveChanges();
                TempData["SUCCESS"] = "Complaint Deleted successfully...";
            }
            return RedirectToAction("ViewCollegeComplaint", "CollegeComplaints", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult ViewCollegeSubComplaints(int collegeid, int complaintId, DateTime dateofcomplaint)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college_complaints> collegecompalints =
                db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.college_faculty_Id == collegeid && c.subcomplaintId != 0 && c.complaintDate == dateofcomplaint && c.roleId == 4)
                    .Select(s => s)
                    .ToList();
            List<ComplaintsList> ComplaintsList = new List<ComplaintsList>();
            foreach (var item in collegecompalints)
            {
                ComplaintsList subcomplaint = new ComplaintsList();
                subcomplaint.subcomplaintid = Convert.ToInt32(item.subcomplaintId);
                subcomplaint.subcomplaintname =
                    db.jntuh_subcomplaints.Where(s => s.id == subcomplaint.subcomplaintid)
                        .Select(s => s.subcomplaintType)
                        .FirstOrDefault();
                if (subcomplaint.subcomplaintname == "Others")
                {
                    subcomplaint.subcomplaintname = subcomplaint.subcomplaintname + "-" + item.otherComplaint.Trim();
                }
                subcomplaint.subcomplaintdate = item.complaintDate.ToString().Split(' ')[0];
                ComplaintsList.Add(subcomplaint);
            }
            return PartialView("ViewCollegeSubComplaints", ComplaintsList);
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeReplayStatus(int id, int complaintId, DateTime dateofcomplaint)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_college_complaints collegecompalints =
                db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.id == id && c.subcomplaintId == 0 && c.complaintDate == dateofcomplaint && c.roleId == 4)
                    .Select(s => s)
                    .FirstOrDefault();
            jntuh_college college =
                db.jntuh_college.Where(c => c.id == collegecompalints.college_faculty_Id).Select(s => s).FirstOrDefault();
            Complaints Complaints = new Complaints();
            Complaints.Id = collegecompalints.id;
            Complaints.collegeId = collegecompalints.college_faculty_Id;
            Complaints.roleId = collegecompalints.roleId;
            Complaints.academicYearid = collegecompalints.academicyearId;
            Complaints.complaintid = Convert.ToInt32(collegecompalints.complaintId);
            Complaints.complaintname = db.jntuh_complaints.Where(r => r.id == Complaints.complaintid && r.roleId == 4).Select(s => s.complaintType).FirstOrDefault();
            if (Complaints.complaintname == "Others")
            {
                Complaints.complaintname = Complaints.complaintname + "-" + collegecompalints.otherComplaint;
            }
            Complaints.complaintdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegecompalints.complaintDate.ToString().Split(' ')[0]);
            Complaints.dcomplaintdate = collegecompalints.complaintDate;
            Complaints.givenBy = collegecompalints.givenBy;
            Complaints.replaystatus = (bool)collegecompalints.replayStatus;
            if (collegecompalints.replayDate != null)
                Complaints.replaystatusdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegecompalints.replayDate.ToString().Split(' ')[0]);

            //Added by siva
            Complaints.EditCollegecomplaintFile = collegecompalints.complaintFile;
            Complaints.CollegereplyFile = collegecompalints.replayFile;

            Complaints.collegeCode = college.collegeCode;
            Complaints.collegeName = college.collegeName;
            List<jntuh_college_complaints> subcollegecompalints =
               db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.roleId == 4 && c.id == Complaints.collegeId && c.subcomplaintId != 0 && c.complaintDate == dateofcomplaint)
                   .Select(s => s)
                   .ToList();
            List<ComplaintsList> newcomplaintslist = new List<ComplaintsList>();
            if (subcollegecompalints.Count != 0)
            {
                foreach (var subitem in subcollegecompalints)
                {
                    ComplaintsList newcomplaints = new ComplaintsList();
                    newcomplaints.subcomplaintid = newcomplaints.subcomplaintid;
                    newcomplaints.complaintid = newcomplaints.complaintid;
                    newcomplaints.subcomplaintname = db.jntuh_subcomplaints.Where(s => s.id == newcomplaints.subcomplaintid && s.roleid == 4).Select(s => s.subcomplaintType).FirstOrDefault();
                    newcomplaints.subcomplaint = true;
                    newcomplaintslist.Add(newcomplaints);
                }

            }
            Complaints.complaintsdata = newcomplaintslist;
            //Complaints.Id = collegecompalints.id;

            return PartialView("CollegeReplayStatus", Complaints);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeReplayStatus(Complaints Complaintsnew)
        {
            string complaintFilepath = "~/Content/Upload/Complaints/ComplaintFile";
            string replyFilepath = "~/Content/Upload/Complaints/ReplyFile";
            //if (Complaintsnew)
            //{

            //}
            int collegeID = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college_complaints> collegecompalints =
                db.jntuh_college_complaints.Where(c => c.complaintId == Complaintsnew.complaintid && c.college_faculty_Id == Complaintsnew.collegeId && c.roleId == 4 && c.complaintDate == Complaintsnew.dcomplaintdate)
                    .Select(s => s)
                    .ToList();
            foreach (var item in collegecompalints)
            {
                item.replayStatus = Complaintsnew.replaystatus;
                collegeID = item.college_faculty_Id;

                item.givenBy = Complaintsnew.givenBy;
                if (Complaintsnew.complaintdate != null)
                    item.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.complaintdate);
                if (Complaintsnew.replaystatusdate != null)
                    item.replayDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.replaystatusdate);
                item.updatedBy = userID;
                item.updatedOn = DateTime.Now;

                //Complaint File Save
                if (Complaintsnew.EditcomplaintFile != null)
                {
                    if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                    }

                    var ext = Path.GetExtension(Complaintsnew.EditcomplaintFile.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string filename = Complaintsnew.collegeCode + "-" + Complaintsnew.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                        Complaintsnew.EditcomplaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                        item.complaintFile = string.Format("{0}{1}", filename, ext);
                    }
                }

                //Complaint File Save
                if (Complaintsnew.replyFile != null)
                {
                    if (!Directory.Exists(Server.MapPath(replyFilepath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(replyFilepath));
                    }

                    var ext = Path.GetExtension(Complaintsnew.replyFile.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string filename = Complaintsnew.collegeCode + "-" + Complaintsnew.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_RF";
                        Complaintsnew.replyFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(replyFilepath), filename, ext));
                        item.replayFile = string.Format("{0}{1}", filename, ext);
                    }
                }


                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.collegeName;
            }




            //jntuh_college college =
            //    db.jntuh_college.Where(c => c.id == collegecompalints.collegeId).Select(s => s).FirstOrDefault();
            //Complaints Complaints = new Complaints();
            //Complaints.Id = collegecompalints.id;
            //Complaints.collegeId = collegecompalints.collegeId;
            //Complaints.complaintid = Convert.ToInt32(collegecompalints.complaintId);
            //Complaints.complaintname = db.jntuh_complaints.Where(r => r.id == Complaints.complaintid).Select(s => s.complaintType).FirstOrDefault();
            //Complaints.complaintdate = collegecompalints.complaintDate.ToString().Split(' ')[0];
            //Complaints.dcomplaintdate = collegecompalints.complaintDate;
            //Complaints.givenBy = collegecompalints.givenBy;
            //Complaints.collegeCode = college.collegeCode;
            //Complaints.collegeName = college.collegeName;
            //List<jntuh_college_complaints> subcollegecompalints =
            //   db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.id == Complaints.collegeId && c.subcomplaintId != 0 && c.complaintDate == dateofcomplaint)
            //       .Select(s => s)
            //       .ToList();
            //List<ComplaintsList> newcomplaintslist = new List<ComplaintsList>();
            //if (subcollegecompalints.Count != 0)
            //{
            //    foreach (var subitem in subcollegecompalints)
            //    {
            //        ComplaintsList newcomplaints = new ComplaintsList();
            //        newcomplaints.subcomplaintid = newcomplaints.subcomplaintid;
            //        newcomplaints.complaintid = newcomplaints.complaintid;
            //        newcomplaints.subcomplaintname = db.jntuh_subcomplaints.Where(s => s.id == newcomplaints.subcomplaintid).Select(s => s.subcomplaintType).FirstOrDefault();
            //        newcomplaints.subcomplaint = true;
            //        newcomplaintslist.Add(newcomplaints);
            //    }

            //}
            //Complaints.complaintsdata = newcomplaintslist;
            //Complaints.Id = collegecompalints.id;
            return RedirectToAction("ViewCollegeComplaint", "CollegeComplaints", new { collegeid = collegeID });
            //return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeActionStatus(int id, int complaintId, DateTime dateofcomplaint)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_college_complaints collegecompalints =
                db.jntuh_college_complaints.Where(c => c.complaintId == complaintId && c.id == id && c.subcomplaintId == 0 && c.complaintDate == dateofcomplaint && c.roleId == 4)
                    .Select(s => s)
                    .FirstOrDefault();
            jntuh_college college =
                db.jntuh_college.Where(c => c.id == collegecompalints.college_faculty_Id).Select(s => s).FirstOrDefault();
            Complaints Complaints = new Complaints();
            Complaints.Id = collegecompalints.id;
            Complaints.collegeId = collegecompalints.college_faculty_Id;
            Complaints.roleId = collegecompalints.roleId;
            Complaints.academicYearid = collegecompalints.academicyearId;
            Complaints.complaintid = Convert.ToInt32(collegecompalints.complaintId);
            Complaints.complaintname = db.jntuh_complaints.Where(r => r.id == Complaints.complaintid && r.roleId == 4).Select(s => s.complaintType).FirstOrDefault();
            if (Complaints.complaintname == "Others")
            {
                Complaints.complaintname = Complaints.complaintname + "-" + collegecompalints.otherComplaint;
            }
            Complaints.complaintdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegecompalints.complaintDate.ToString().Split(' ')[0]);
            Complaints.dcomplaintdate = collegecompalints.complaintDate;
            Complaints.givenBy = collegecompalints.givenBy;
            Complaints.replaystatus = (bool)collegecompalints.replayStatus;
            if (collegecompalints.replayDate != null)
                Complaints.replaystatusdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegecompalints.replayDate.ToString().Split(' ')[0]);

            //Added by siva
            Complaints.EditCollegecomplaintFile = collegecompalints.complaintFile;
            Complaints.CollegereplyFile = collegecompalints.replayFile;

            Complaints.collegeCode = college.collegeCode;
            Complaints.collegeName = college.collegeName;
            Complaints.pendingstatus = collegecompalints.complaintOn == "Pending" ? true : false;
            Complaints.closedstatus = collegecompalints.complaintOn == "Closed" ? true : false;
            Complaints.actionstatusName = collegecompalints.complaintOn;
            return PartialView("CollegeActionStatus", Complaints);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult CollegeActionStatus(Complaints Complaintsnew)
        {
            int collegeID = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college_complaints> collegecompalints =
                db.jntuh_college_complaints.Where(c => c.complaintId == Complaintsnew.complaintid && c.college_faculty_Id == Complaintsnew.collegeId && c.roleId == 4 && c.complaintDate == Complaintsnew.dcomplaintdate)
                    .Select(s => s)
                    .ToList();
            foreach (var item in collegecompalints)
            {
                collegeID = item.college_faculty_Id;
                item.complaintOn = Complaintsnew.pendingstatus ? "Pending" : "Closed";
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Complaint Action Status Changed to " + item.complaintOn;
            }

            return RedirectToAction("ViewCollegeComplaint", "CollegeComplaints", new { collegeid = collegeID });
        }
        #endregion

        #region BAS Complaints Screnns

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult AbsIndex()
        {
            DateTime todaydate = DateTime.Now.Date;
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            List<jntuh_college_bas_complaints> bascomplaints = db.jntuh_college_bas_complaints.Select(s => s).ToList();
            ViewBag.Totalcomplaints = bascomplaints.Count();
            ViewBag.Todaycomplaints = bascomplaints.Where(c => c.createdon >= todaydate).Select(s => s).Count();
            return View(colleges);
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult BasCollegeComplaints(int collegeid)
        {
            Bascomplaints complaintsnew = new Bascomplaints();
            if (collegeid != 0)
            {
                List<SelectListItem> complaintype = new List<SelectListItem>();
                SelectListItem item1 = new SelectListItem() { Text = "College", Value = "1" };
                SelectListItem item2 = new SelectListItem() { Text = "faculty", Value = "2" };
                SelectListItem item3 = new SelectListItem() { Text = "Student", Value = "3" };
                complaintype.Add(item1);
                complaintype.Add(item2);
                complaintype.Add(item3);

                ViewBag.Complaints = complaintype;
                jntuh_college jntuhcollege =
                    db.jntuh_college.Where(c => c.id == collegeid && c.isActive == true).Select(s => s).FirstOrDefault();

                if (jntuhcollege != null)
                {
                    complaintsnew.collegeid = collegeid;
                    complaintsnew.collegeCode = jntuhcollege.collegeCode;
                    complaintsnew.collegeName = jntuhcollege.collegeName;
                }

                return View("BasCollegeComplaints", complaintsnew);
            }


            return RedirectToAction("AbsIndex");
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult BasCollegeComplaints(Bascomplaints objbas)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            jntuh_college_bas_complaints complaints = new jntuh_college_bas_complaints();
            string complaintpath = "~/Content/Upload/complaints/bascomplaints";
            if (objbas.complaintsupportingdoc == null || objbas.complaintdate == null || objbas.collegeid == 0)
            {
                TempData["ERROR"] = "Mandatory fields missing ! try again ";
                return RedirectToAction("AbsIndex");
            }
            if (objbas.complaintsupportingdoc != null)
            {
                if (!Directory.Exists(Server.MapPath(complaintpath)))
                {
                    Directory.CreateDirectory(Server.MapPath(complaintpath));

                }
                var ext = Path.GetExtension(objbas.complaintsupportingdoc.FileName);

                if (ext.ToUpper().Equals(".PDF"))
                {
                    string fileName = objbas.collegeCode.Trim() + "-" +
                    DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");

                    objbas.complaintsupportingdoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintpath),
                        fileName, ext));
                    objbas.complaintdocview = string.Format("{0}{1}", fileName, ext);
                }
                complaints.collegeid = objbas.collegeid;
                complaints.academicyearId = ay0;
                complaints.complainttype = Convert.ToInt32(objbas.complainttype);
                if (objbas.regnum != null)
                {
                    objbas.regnum = Regex.Replace(objbas.regnum, @"\r|\n|\t", "");
                    complaints.registrationnumber = objbas.regnum.Trim();
                }
                complaints.complaint = Regex.Replace(objbas.complaint, @"\r|\n|\t", "");
                complaints.complaint = complaints.complaint.Trim();
                complaints.supportdoc = objbas.complaintdocview;
                complaints.complaintid = objbas.complaintid;
                if (objbas.complaintdate != null)
                    complaints.complaintdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(objbas.complaintdate);
                complaints.replaystatus = objbas.replaystatus;
                if (objbas.replaydate != null)
                    complaints.replaydate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(objbas.replaydate);
                complaints.cretaedby = userID;
                complaints.createdon = DateTime.Now;
                complaints.isactive = true;
                db.jntuh_college_bas_complaints.Add(complaints);
                db.SaveChanges();
                TempData["SUCCESS"] = "BAS Complaint added Successfully";
                return RedirectToAction("AbsIndex");
            }
            return View();
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult BasViewCollegeComplaint(int collegeid)
        {
            if (collegeid != 0)
            {
                List<Bascomplaints> bascomplaintslist = new List<Bascomplaints>();

                List<jntuh_college_bas_complaints> collegecompalints = db.jntuh_college_bas_complaints.Where(c => c.collegeid == collegeid && c.isactive == true).Select(s => s).ToList();
                return View("BasViewCollegeComplaint", collegecompalints);


            }
            return RedirectToAction("AbsIndex");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult EditBasComplaint(int id)
        {
            if (id != 0)
            {

                jntuh_college_bas_complaints collegecompalints = db.jntuh_college_bas_complaints.Where(c => c.id == id && c.isactive == true).Select(s => s).FirstOrDefault();

                jntuh_college college =
             db.jntuh_college.Where(c => c.id == collegecompalints.collegeid).Select(s => s).FirstOrDefault();

                Bascomplaints complaint = new Bascomplaints();
                complaint.collegeid = collegecompalints.collegeid;
                complaint.collegeid = collegecompalints.academicyearId;
                complaint.Id = collegecompalints.id;
                complaint.replay = collegecompalints.Replay;
                complaint.complaintid = collegecompalints.complaintid;
                complaint.complaint = collegecompalints.complaint;
                complaint.collegeName = college.collegeName;
                complaint.collegeCode = college.collegeCode;
                complaint.Createdon = collegecompalints.createdon;
                if (collegecompalints.complaintdate != null)
                {
                    complaint.complaintdate = collegecompalints.complaintdate.ToString().Split(' ')[0];
                    complaint.dcomplaintdate = collegecompalints.complaintdate;
                }


                if (collegecompalints.replaystatus == true)
                {
                    complaint.replaystatus = true;
                }
                else
                {
                    complaint.replaystatus = false;
                }

                if (collegecompalints.replaydate != null)
                {
                    complaint.replaydate = collegecompalints.replaydate.ToString().Split(' ')[0];
                }
                complaint.complaintdocview = collegecompalints.supportdoc;
                return PartialView("EditBasComplaint", complaint);

            }
            return RedirectToAction("AbsIndex");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult EditBasComplaint(Bascomplaints bascomplaint)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            try
            {
                jntuh_college_bas_complaints complaints = new jntuh_college_bas_complaints();
                complaints.id = bascomplaint.Id;
                complaints.collegeid = bascomplaint.collegeid;
                complaints.academicyearId = ay0;
                if (bascomplaint.regnum != null)
                {
                    bascomplaint.regnum = Regex.Replace(bascomplaint.regnum, @"\r|\n|\t", "");
                    complaints.registrationnumber = bascomplaint.regnum.Trim();
                }
                complaints.complaint = Regex.Replace(bascomplaint.complaint, @"\r|\n|\t", "");
                complaints.complaint = complaints.complaint.Trim();
                complaints.complaint = complaints.complaint;
                complaints.supportdoc = bascomplaint.complaintdocview;
                complaints.complaintid = bascomplaint.complaintid;

                if (bascomplaint.dcomplaintdate != null)
                {
                    complaints.complaintdate = bascomplaint.dcomplaintdate;
                }
                complaints.replaystatus = bascomplaint.replaystatus;

                if (bascomplaint.replaydate != null)
                {
                    complaints.replaydate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(bascomplaint.replaydate);
                }
                if (bascomplaint.Createdon != null)
                {
                    complaints.createdon = bascomplaint.Createdon;
                }
                if (bascomplaint.complaint != null)
                    complaints.Replay = Regex.Replace(bascomplaint.complaint.Trim(), @"\r|\n|\t", "");

                complaints.updatedon = DateTime.Now;
                complaints.updatedby = userID;
                complaints.isactive = true;
                db.Entry(complaints).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Complaint Update successfully.";
                return RedirectToAction("BasViewCollegeComplaint", new { collegeid = bascomplaint.collegeid });
            }
            catch (Exception)
            {

                throw;
            }

        }

        //Delete College BAS Complaint
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteBasComplaint(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = 0;
            if (id != 0)
            {
                jntuh_college_bas_complaints deletebascompalints =
                 db.jntuh_college_bas_complaints.Where(c => c.id == id)
                     .Select(s => s)
                     .FirstOrDefault();
                if (deletebascompalints != null)
                {
                    collegeId = deletebascompalints.collegeid;
                    db.jntuh_college_bas_complaints.Remove(deletebascompalints);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Complaint Deleted successfully...";
                }
                else
                {
                    TempData["ERROR"] = "No data found.";
                }
            }
            return RedirectToAction("BasViewCollegeComplaint", new { collegeid = collegeId });
        }
        #endregion
    }

    public class Complaints
    {
        public int Id { get; set; }
        public int collegeId { get; set; }
        public int roleId { get; set; }
        public int academicYearid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string remarks { get; set; }
        [Required(ErrorMessage = "*")]
        public int complaintid { get; set; }
        public Nullable<System.DateTime> dcomplaintdate { get; set; }
        [Required(ErrorMessage = "*")]
        public string complaintdate { get; set; }
        public string complaintname { get; set; }
        public string otherscomplaint { get; set; }
        public bool replaystatus { get; set; }
        public string replaystatusdate { get; set; }
        public string givenBy { get; set; }
        public int givenById { get; set; }
        public string OthergivenBy { get; set; }
        public List<ComplaintsList> complaintsdata { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "complaintFile")]
        public HttpPostedFileBase complaintFile { get; set; }

        public string CollegecomplaintFile { get; set; }

        [Display(Name = "EditcomplaintFile")]
        public HttpPostedFileBase EditcomplaintFile { get; set; }

        public string EditCollegecomplaintFile { get; set; }

        //[Required(ErrorMessage = "*")]
        [Display(Name = "replayFile")]
        public HttpPostedFileBase replyFile { get; set; }

        public string CollegereplyFile { get; set; }
        public bool pendingstatus { get; set; }
        public bool closedstatus { get; set; }
        public string actionstatusName { get; set; }
    }

    public class ComplaintsList
    {
        public int complaintid { get; set; }
        public int subcomplaintid { get; set; }
        public bool subcomplaint { get; set; }
        public int nooftimes { get; set; }
        public string subcomplaintname { get; set; }
        public string subcomplaintdate { get; set; }
        public bool subreplaystatus { get; set; }
        public string subreplaystatusdate { get; set; }
        public string othersubcomplaint { get; set; }
        public string remarks { get; set; }
    }

    public class ComplaintsColleges
    {
        public int id { get; set; }
        public string collegecode { get; set; }
        public string collegename { get; set; }
        public int complaintcount { get; set; }
        public int nocomplaintpathcount { get; set; }
        public bool isActive { get; set; }
    }
    public class Bascomplaints
    {
        public int Id { get; set; }
        public int collegeid { get; set; }
        public int Academicyearid { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string regnum { get; set; }
        [Required(ErrorMessage = "*")]
        public string name { get; set; }
        //[MaxLength(500)]
        [Required(ErrorMessage = "*")]
        public string complaint { get; set; }
        public HttpPostedFileBase complaintsupportingdoc { get; set; }
        public string complaintdocview { get; set; }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Inward Number")]
        public string complaintid { get; set; }
        public Nullable<System.DateTime> dcomplaintdate { get; set; }
        [Required(ErrorMessage = "*")]
        public string complaintdate { get; set; }
        public bool replaystatus { get; set; }
        public string replaydate { get; set; }
        public bool complaintstatus { get; set; }
        [Required(ErrorMessage = "*")]
        public string complainttype { get; set; }
        //[MaxLength(80)]
        public string replay { get; set; }

        public Nullable<System.DateTime> Createdon { get; set; }
    }
}
