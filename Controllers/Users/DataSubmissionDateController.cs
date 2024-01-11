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
    public class DataSubmissionDateController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index(string id)
        {
            if (id != null)
            {
                int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                List<jntuh_college_edit_status> jntuh_college_edit_status = db.jntuh_college_edit_status.Where(cs => cs.collegeId == userCollegeID).ToList();
                DataSubmissionDate DataSubmissionDate = new DataSubmissionDate();
                DataSubmissionDate.CollegeId = userCollegeID;
                jntuh_college jntuh_college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c).FirstOrDefault();
                if (jntuh_college != null)
                {
                    DataSubmissionDate.CollegeName = jntuh_college.collegeName;
                    DataSubmissionDate.CollegeCode = jntuh_college.collegeCode;
                }

                jntuh_college_edit_status jntuh_college_editstatus = db.jntuh_college_edit_status.Where(cs => cs.collegeId == userCollegeID).Select(cs => cs).FirstOrDefault();
                if (jntuh_college_editstatus != null)
                {
                    DataSubmissionDate.Id = jntuh_college_editstatus.id;
                    DataSubmissionDate.EditStatus = jntuh_college_editstatus.IsCollegeEditable;
                    string strFromDate = jntuh_college_editstatus.editFromDate.ToString();
                    if (strFromDate != null)
                    {
                        DataSubmissionDate.strFromDate = Utilities.MMDDYY2DDMMYY(strFromDate).ToString();
                    }

                    string strToDate = jntuh_college_editstatus.editToDate.ToString();
                    if (strToDate != null)
                    {
                        DataSubmissionDate.strToDate = Utilities.MMDDYY2DDMMYY(strToDate).ToString();
                    }
                    DataSubmissionDate.CreatedBy = (int)jntuh_college_editstatus.createdBy;
                    DataSubmissionDate.CreatedOn = jntuh_college_editstatus.createdOn;
                }

                jntuh_college_edit_remarks jntuh_college_edit_remarks = db.jntuh_college_edit_remarks.Where(cr => cr.collegeId == userCollegeID).Select(cr => cr).FirstOrDefault();
                if (jntuh_college_edit_remarks != null)
                {
                    DataSubmissionDate.CollegeEditRemarks = jntuh_college_edit_remarks.collegeEditRemarks;
                    DataSubmissionDate.IsCollegeRemarks = jntuh_college_edit_remarks.isCollegeRemarks;
                }
                return View(DataSubmissionDate);
            }
            return View();
        }
        [HttpPost]
        public ActionResult Index(DataSubmissionDate DataSubmissionData, string cmd)
        {
            int collegeid = DataSubmissionData.CollegeId;
            int CollegeEditRemarkId = db.jntuh_college_edit_remarks.Where(cr => cr.collegeId == collegeid).Select(cr => cr.id).FirstOrDefault();
            int CollegeEditStatusId = db.jntuh_college_edit_status.Where(cs => cs.collegeId == collegeid).Select(cs => cs.id).FirstOrDefault();
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            if (CollegeEditRemarkId == 0)
            {

                jntuh_college_edit_remarks jntuh_college_edit_remarks = new jntuh_college_edit_remarks();
                jntuh_college_edit_remarks.collegeId = collegeid;
                jntuh_college_edit_remarks.isCollegeRemarks = false;
                jntuh_college_edit_remarks.collegeEditRemarks = DataSubmissionData.CollegeEditRemarks;
                jntuh_college_edit_remarks.createdBy = createdBy;
                jntuh_college_edit_remarks.createdOn = DateTime.Now;
                db.jntuh_college_edit_remarks.Add(jntuh_college_edit_remarks);
                db.SaveChanges();
            }
            else
            {
                jntuh_college_edit_remarks jntuh_college_edit_remarks = new jntuh_college_edit_remarks();
                jntuh_college_edit_remarks.id = CollegeEditRemarkId;
                jntuh_college_edit_remarks.collegeId = collegeid;
                jntuh_college_edit_remarks.isCollegeRemarks = false;
                jntuh_college_edit_remarks.collegeEditRemarks = DataSubmissionData.CollegeEditRemarks;
                jntuh_college_edit_remarks.createdBy = db.jntuh_college_edit_remarks.AsNoTracking().Where(e => e.collegeId == CollegeEditRemarkId).Select(e => e.createdBy).FirstOrDefault();
                jntuh_college_edit_remarks.createdOn = db.jntuh_college_edit_remarks.AsNoTracking().Where(e => e.collegeId == CollegeEditRemarkId).Select(e => e.createdOn).FirstOrDefault();
                jntuh_college_edit_remarks.updatedBy = createdBy;
                jntuh_college_edit_remarks.updatedOn = DateTime.Now;
                db.Entry(jntuh_college_edit_remarks).State = EntityState.Modified;
                db.SaveChanges();
            }

            jntuh_college_edit_status jntuh_college_edit_status = new jntuh_college_edit_status();
            jntuh_college_edit_status.collegeId = collegeid;
            jntuh_college_edit_status.IsCollegeEditable = DataSubmissionData.EditStatus;
            jntuh_college_edit_status.editFromDate = Utilities.DDMMYY2MMDDYY(DataSubmissionData.strFromDate);
            // jntuh_college_edit_status.editToDate = Utilities.DDMMYY2MMDDYY(DataSubmissionData.strToDate);

            //This is for Day end date don't change any thing-Start            
            string seditToDate = DataSubmissionData.strToDate;
            string[] date = seditToDate.Split('/');
            string dd = date[0];
            string mm = date[1];
            string yyyy = date[2];
            string streditToDate = mm + "/" + dd + "/" + yyyy + " 23:59:59";
            jntuh_college_edit_status.editToDate = Convert.ToDateTime(streditToDate);
            //end
            if (cmd == "Edit Permission For Single College")
            {
                if (CollegeEditStatusId == 0)
                {
                    jntuh_college_edit_status.createdBy = createdBy;
                    jntuh_college_edit_status.createdOn = DateTime.Now;
                    db.jntuh_college_edit_status.Add(jntuh_college_edit_status);
                }
                else
                {
                    jntuh_college_edit_status.id = CollegeEditStatusId;
                    jntuh_college_edit_status.createdBy = db.jntuh_college_edit_status.AsNoTracking().Where(e => e.collegeId == CollegeEditStatusId).Select(e => e.createdBy).FirstOrDefault();
                    jntuh_college_edit_status.createdOn = db.jntuh_college_edit_status.AsNoTracking().Where(e => e.collegeId == CollegeEditStatusId).Select(e => e.createdOn).FirstOrDefault();
                    jntuh_college_edit_status.updatedBy = createdBy;
                    jntuh_college_edit_status.updatedOn = DateTime.Now;
                    db.Entry(jntuh_college_edit_status).State = EntityState.Modified;
                }

                db.SaveChanges();

                TempData["Success"] = "Permission saved successfully";

                //send email to college with editable dates when edit status is TRUE
                if (DataSubmissionData.EditStatus == true)
                {
                    string email = db.jntuh_address.AsNoTracking().Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeid).Select(a => a.email).FirstOrDefault();

                    if (email == null)
                    {
                        string code = db.jntuh_college.AsNoTracking().Where(c => c.id == collegeid).Select(c => c.collegeCode).FirstOrDefault();
                        //Commented on 18-06-2018 by Narayana Reddy
                        //email = db.all_college_emails.AsNoTracking().Where(e => e.Code == code).Select(e => e.Email).FirstOrDefault();
                    }

                    int userId = db.jntuh_college_users.Where(cu => cu.collegeID == collegeid).Select(cu => cu.userID).FirstOrDefault();
                    string username = db.my_aspnet_users.Find(userId).name;

                    IUserMailer mailer = new UserMailer();
                    mailer.SendEditDates(email, "supportaac@jntuh.ac.in", "AAC, JNTUH: Edit option enabled", username, DataSubmissionData.strFromDate, DataSubmissionData.strToDate).SendAsync();
                }
            }
            else
            {
                List<jntuh_college_edit_status> editStatsu = db.jntuh_college_edit_status.Select(e => e).ToList();
                foreach (var item in editStatsu)
                {
                    var collegeEditStatsu = db.jntuh_college_edit_status.Where(e => e.id == item.id).Select(e => e).FirstOrDefault();
                    collegeEditStatsu.IsCollegeEditable = DataSubmissionData.EditStatus;
                    collegeEditStatsu.editFromDate = Utilities.DDMMYY2MMDDYY(DataSubmissionData.strFromDate);
                    collegeEditStatsu.editToDate = Convert.ToDateTime(streditToDate);
                    collegeEditStatsu.updatedBy = createdBy;
                    collegeEditStatsu.updatedOn = DateTime.Now;
                    db.Entry(collegeEditStatsu).State = EntityState.Modified;
                    db.SaveChanges();

                    #region Sending Email
                    /* string email = db.jntuh_address.AsNoTracking().Where(a => a.addressTye == "COLLEGE" && a.collegeId == item.collegeId).Select(a => a.email).FirstOrDefault();
                    if (email == null)
                    {
                        string code = db.jntuh_college.AsNoTracking().Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        email = db.all_college_emails.AsNoTracking().Where(e => e.Code == code).Select(e => e.Email).FirstOrDefault();
                    }

                    int userId = db.jntuh_college_users.Where(cu => cu.collegeID == item.collegeId).Select(cu => cu.userID).FirstOrDefault();
                    string username = db.my_aspnet_users.Find(userId).name;

                    IUserMailer mailer = new UserMailer();
                    mailer.SendEditDates(email, "aac.do.not.reply@gmail.com", "AAC, JNTUH: Edit option enabled", username, DataSubmissionData.strFromDate, DataSubmissionData.strToDate).SendAsync();*/

                    #endregion
                }
                TempData["Success"] = "Permission saved successfully";
            }

            return View("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CollegeScreens(int? id)
        {
            CollegeAssignedScreens collegeAssignedScreens = new Models.CollegeAssignedScreens();
            
            collegeAssignedScreens.colleges = (from c in db.jntuh_college
                                               where (c.isActive == true)
                                               select new Colleges
                                               {
                                                   Id = c.id,
                                                   CollegeName = c.collegeCode + "-" + c.collegeName
                                               }).OrderBy(c => c.CollegeName).ToList();
            collegeAssignedScreens.colleges.Add(new Colleges() { Id = 0, CollegeName = 0 + "-" + "Select All" });
            collegeAssignedScreens.colleges = collegeAssignedScreens.colleges.OrderBy(c => c.Id).ToList();
            TempData["CollegeID"] = id;
           // collegeAssignedScreens.colleges.Add(new Colleges() { Id = 0, CollegeName =  "--Select--" });
            if (id != null && id!=0)
            {
                ViewBag.Edit = true;
                var estatus = db.jntuh_college_edit_status.Where(c => c.collegeId == id).OrderByDescending(d => d.createdOn).Select(c => c).FirstOrDefault();
                var eremarks = db.jntuh_college_edit_remarks.Where(c => c.collegeId == id).Select(c => c).OrderByDescending(d=>d.createdOn).FirstOrDefault();
                collegeAssignedScreens.CollegeId = (int)id;
                if (estatus != null)
                {
                    collegeAssignedScreens.FromDate = Utilities.MMDDYY2DDMMYY(estatus.editFromDate.ToString()).ToString();
                    collegeAssignedScreens.ToDate = Utilities.MMDDYY2DDMMYY(estatus.editToDate.ToString()).ToString();
                    if (eremarks != null)
                    {
                        collegeAssignedScreens.Remarks = eremarks.collegeEditRemarks;
                    }
                }
                collegeAssignedScreens.collegeScreens = (from s in db.jntuh_college_screens
                                                         where (s.IsActive == true)
                                                         select new CollegeScreens
                                                         {
                                                             Id = s.Id,
                                                             ScreenCode = s.ScreenCode,
                                                             ScreenName = s.ScreenName,
                                                             IsSelected = false
                                                         }).ToList();

                foreach (var item in collegeAssignedScreens.collegeScreens.Where(c => c.Id != 0).ToList())
                {
                    item.IsSelected = db.jntuh_college_screens_assigned.Where(c => c.ScreenId == item.Id && c.CollegeId == id).Select(c => c.IsEditable).FirstOrDefault();
                }

            }
            else if (id ==0)
            {
               // id = 457;

                int id1 = 0;
                var estatus1 = db.jntuh_college_edit_status.FirstOrDefault();
                id = 375;
                ViewBag.Edit = true;
                var estatus = db.jntuh_college_edit_status.Where(c => c.collegeId == id).OrderByDescending(d => d.createdOn).Select(c => c).FirstOrDefault();
                var eremarks = db.jntuh_college_edit_remarks.Where(c => c.collegeId == id).OrderByDescending(d => d.createdOn).Select(c => c).FirstOrDefault();
                collegeAssignedScreens.CollegeId = (int)id1;
                if (estatus != null)
                {
                    collegeAssignedScreens.FromDate = Utilities.MMDDYY2DDMMYY(estatus.editFromDate.ToString()).ToString();
                    collegeAssignedScreens.ToDate = Utilities.MMDDYY2DDMMYY(estatus.editToDate.ToString()).ToString();
                    if (eremarks != null)
                    {
                        collegeAssignedScreens.Remarks = eremarks.collegeEditRemarks;
                    }
                }
                collegeAssignedScreens.collegeScreens = (from s in db.jntuh_college_screens
                                                         where (s.IsActive == true)
                                                         select new CollegeScreens
                                                         {
                                                             Id = s.Id,
                                                             ScreenCode = s.ScreenCode,
                                                             ScreenName = s.ScreenName,
                                                             IsSelected = false
                                                         }).ToList();

                foreach (var item in collegeAssignedScreens.collegeScreens.Where(c => c.Id != 0).ToList())
                {
                    item.IsSelected = db.jntuh_college_screens_assigned.Where(c => c.ScreenId == item.Id && c.CollegeId == id).Select(c => c.IsEditable).FirstOrDefault();
                }

            }
            else
            {
                List<CollegeScreens> collegeScreens = new List<Models.CollegeScreens>();
                collegeAssignedScreens.collegeScreens = collegeScreens;
                
                ViewBag.Edit = false;
            }

            //collegeAssignedScreens
            return View(collegeAssignedScreens);
        }

        [HttpPost]
        public ActionResult CollegeScreens(CollegeAssignedScreens collegeAssignedScreens)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            string SelectedCollegeId = TempData["CollegeID"].ToString();


            // var collegeEditStatus = db.jntuh_college_edit_status.Where(es => es.collegeId == collegeAssignedScreens.CollegeId).Select(es => es).FirstOrDefault();

            var collegeEditStatus = db.jntuh_college_edit_status.Where(es => es.collegeId == collegeAssignedScreens.CollegeId).Select(es => es).FirstOrDefault();
            int[] EditCollegeId = db.jntuh_college_edit_status.Select(e => e.collegeId).ToArray();
            //int[] EditCollegeId =
            //{
            //    4, 38, 80, 84, 86, 100, 102, 103, 113, 122, 141, 147, 152, 153, 188, 211, 218,
            //    276, 287, 305, 308, 315, 324, 330, 352, 369, 376, 393, 395, 400, 403, 415, 416, 420, 55, 58, 107, 120,
            //    135, 139, 202, 206, 213, 219, 252, 253, 267, 290, 295, 302, 353, 389, 410, 5, 411, 42, 194, 332, 91, 174
            //};
            #region To insert into  collegeEditStatusLog
            if (SelectedCollegeId == "0")
            {

                #region  To insert into Multiple collegeEditStatusLog
                ////List<jntuh_college_edit_status> editCollegeLogId = db.jntuh_college_edit_status.Select(e => e).ToList();
                #region To insert into Multiple collegeEditStatusLog
                List<jntuh_college_edit_status_log> jntuh_college_edit_status_loglist = new List<jntuh_college_edit_status_log>();
                // if (collegeEditStatusLog.)
                foreach (var EditLogCollegeId in EditCollegeId)
                {
                    if (EditLogCollegeId != null)
                    {
                        var collegeEditStatusLog = db.jntuh_college_edit_status.Find(EditLogCollegeId);
                        jntuh_college_edit_status_log jntuh_college_edit_status_log = new jntuh_college_edit_status_log();
                        jntuh_college_edit_status_log.collegeId = collegeEditStatusLog.collegeId;
                        jntuh_college_edit_status_log.IsCollegeEditable = collegeEditStatusLog.IsCollegeEditable;
                        jntuh_college_edit_status_log.editFromDate = collegeEditStatusLog.editFromDate;
                        jntuh_college_edit_status_log.editToDate = collegeEditStatusLog.editToDate;
                        jntuh_college_edit_status_log.createdOn = collegeEditStatusLog.createdOn;
                        jntuh_college_edit_status_log.createdBy = collegeEditStatusLog.createdBy;
                        jntuh_college_edit_status_log.updatedOn = collegeEditStatusLog.updatedOn;
                        jntuh_college_edit_status_log.updatedBy = collegeEditStatusLog.updatedBy;
                        jntuh_college_edit_status_log.loggedOn = DateTime.Now;
                        jntuh_college_edit_status_log.loggedBy = userId;
                        jntuh_college_edit_status_loglist.Add(jntuh_college_edit_status_log);
                        //db.jntuh_college_edit_status_log.Add(jntuh_college_edit_status_log);                       
                    }                    
                #endregion

                }
                jntuh_college_edit_status_loglist.ForEach(d => db.jntuh_college_edit_status_log.Add(d));
                db.SaveChanges();
                #endregion
            }
            else
            {
                #region To insert into Sinngle  collegeEditStatusLog
                //var SingleCollegeEditStatusLog = db.jntuh_college_edit_status.Find(collegeEditStatus.id);
                if (collegeEditStatus != null)
                {
                    jntuh_college_edit_status_log jntuh_college_edit_status_log = new jntuh_college_edit_status_log();
                    jntuh_college_edit_status_log.collegeId = collegeEditStatus.collegeId;
                    jntuh_college_edit_status_log.IsCollegeEditable = collegeEditStatus.IsCollegeEditable;
                    jntuh_college_edit_status_log.editFromDate = collegeEditStatus.editFromDate;
                    jntuh_college_edit_status_log.editToDate = collegeEditStatus.editToDate;
                    jntuh_college_edit_status_log.createdOn = collegeEditStatus.createdOn;
                    jntuh_college_edit_status_log.createdBy = collegeEditStatus.createdBy;
                    jntuh_college_edit_status_log.updatedOn = collegeEditStatus.updatedOn;
                    jntuh_college_edit_status_log.updatedBy = collegeEditStatus.updatedBy;
                    jntuh_college_edit_status_log.loggedOn = DateTime.Now;
                    jntuh_college_edit_status_log.loggedBy = userId;
                    db.jntuh_college_edit_status_log.Add(jntuh_college_edit_status_log);
                    db.SaveChanges();
                }
                #endregion
            }

            #endregion

            int uccount = 0;
            #region To update into  collegeEditStatus
            //List<jntuh_college_edit_status> EditCollegeId = db.jntuh_college_edit_status.Select(e => e).ToList();
            if (SelectedCollegeId == "0")
            {
                #region To update into Multiple collegeEditStatus
                foreach (var EditedCollegeId in EditCollegeId)
                {
                    #region To update into collegeEditStatus
                    if (EditedCollegeId!= null)
                    {
                        var editdetails = db.jntuh_college_edit_status.Find(EditedCollegeId);

                        editdetails.editFromDate = Utilities.DDMMYY2MMDDYY(collegeAssignedScreens.FromDate);
                        //This is for Day end date don't change any thing-Start            
                        string seditToDate = collegeAssignedScreens.ToDate;
                        string[] date = seditToDate.Split('/');
                        string dd = date[0];
                        string mm = date[1];
                        string yyyy = date[2];
                        string streditToDate = mm + "/" + dd + "/" + yyyy + " 23:59:59";
                        editdetails.editToDate = Convert.ToDateTime(streditToDate);
                        editdetails.IsCollegeEditable = true;
                        editdetails.updatedBy = userId;

                        editdetails.updatedOn = DateTime.Now;
                        db.Entry(editdetails).State = EntityState.Modified;
                        db.SaveChanges();
                        uccount++;
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
                #region To update into Single collegeEditStatus
                if (collegeEditStatus != null)
                {
                    //var editdetails = db.jntuh_college_edit_status.Find(collegeEditStatus.id);

                    collegeEditStatus.editFromDate = Utilities.DDMMYY2MMDDYY(collegeAssignedScreens.FromDate);
                    //This is for Day end date don't change any thing-Start            
                    string seditToDate = collegeAssignedScreens.ToDate;
                    string[] date = seditToDate.Split('/');
                    string dd = date[0];
                    string mm = date[1];
                    string yyyy = date[2];
                    string streditToDate = mm + "/" + dd + "/" + yyyy + " 23:59:59";
                    collegeEditStatus.editToDate = Convert.ToDateTime(streditToDate);
                    collegeEditStatus.IsCollegeEditable = true;
                    collegeEditStatus.updatedBy = userId;

                    collegeEditStatus.updatedOn = DateTime.Now;
                    db.Entry(collegeEditStatus).State = EntityState.Modified;
                    db.SaveChanges();


                }
                #endregion
            }
            #endregion

            #region collegeEditStatusRemarks
            //List<jntuh_college_edit_status> EditCollegeRemarksID = db.jntuh_college_edit_status.Select(e => e).ToList();
            if (SelectedCollegeId == "0")
            {
                foreach (var EditRemarks in EditCollegeId)
                {
                    #region Multiple CollegeEditStatusRemarks
                    var college_edit_remarks = db.jntuh_college_edit_remarks.Where(c => c.collegeId == collegeAssignedScreens.CollegeId).Select(c => c).FirstOrDefault();
                    if (college_edit_remarks != null)
                    {
                        var remarks = db.jntuh_college_edit_remarks.Find(EditRemarks);
                        remarks.collegeEditRemarks = collegeAssignedScreens.Remarks;
                        remarks.updatedBy = userId;
                        remarks.updatedOn = DateTime.Now;
                        db.Entry(remarks).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        jntuh_college_edit_remarks jntuh_college_edit_remarks = new Models.jntuh_college_edit_remarks();
                        jntuh_college_edit_remarks.collegeId = EditRemarks;
                        jntuh_college_edit_remarks.collegeEditRemarks = collegeAssignedScreens.Remarks;
                        jntuh_college_edit_remarks.isCollegeRemarks = true;
                        jntuh_college_edit_remarks.createdBy = userId;
                        jntuh_college_edit_remarks.createdOn = DateTime.Now;
                        db.jntuh_college_edit_remarks.Add(jntuh_college_edit_remarks);
                        db.SaveChanges();
                    }
                    #endregion
                }
            }
            else
            {
                #region Single CollegeEditStatusRemarks
                var college_edit_remarks = db.jntuh_college_edit_remarks.Where(c => c.collegeId == collegeAssignedScreens.CollegeId).Select(c => c).FirstOrDefault();
                if (college_edit_remarks != null)
                {
                    var remarks = db.jntuh_college_edit_remarks.Find(college_edit_remarks.id);
                    remarks.collegeEditRemarks = collegeAssignedScreens.Remarks;
                    remarks.updatedBy = userId;
                    remarks.updatedOn = DateTime.Now;
                    db.Entry(remarks).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    jntuh_college_edit_remarks jntuh_college_edit_remarks = new Models.jntuh_college_edit_remarks();
                    jntuh_college_edit_remarks.collegeId = collegeAssignedScreens.CollegeId;
                    jntuh_college_edit_remarks.collegeEditRemarks = collegeAssignedScreens.Remarks;
                    jntuh_college_edit_remarks.isCollegeRemarks = true;
                    jntuh_college_edit_remarks.createdBy = userId;
                    jntuh_college_edit_remarks.createdOn = DateTime.Now;
                    db.jntuh_college_edit_remarks.Add(jntuh_college_edit_remarks);
                    db.SaveChanges();
                }
                #endregion
            }
            #endregion
            #region To insert  into  AssignedScreens Log
            //List<jntuh_college_edit_status> AssignedCoolegesLogID = db.jntuh_college_edit_status.Select(e => e).ToList();
            if (SelectedCollegeId == "0")
            {
                List<jntuh_college_screens_assigned_log> jntuh_college_screens_assigned_loglist= new List<jntuh_college_screens_assigned_log>();
                List<jntuh_college_screens_assigned> jntuh_college_screens_assigned =
                    db.jntuh_college_screens_assigned.AsNoTracking().ToList();
                foreach (var AssignedScreensLogID in EditCollegeId)
                {
                    #region To insert into Multiple  AssignedScreens Log
                    var AssignedScreens = jntuh_college_screens_assigned.Where(c => c.CollegeId == AssignedScreensLogID).ToList();
                    if (AssignedScreens.Count() > 0)
                    {
                        foreach (var item in AssignedScreens)
                        {

                            jntuh_college_screens_assigned_log jntuh_college_screens_assigned_log = new jntuh_college_screens_assigned_log();
                            jntuh_college_screens_assigned_log.CollegeId = AssignedScreensLogID;
                            jntuh_college_screens_assigned_log.ScreenId = item.ScreenId;
                            jntuh_college_screens_assigned_log.IsEditable = item.IsEditable;
                            jntuh_college_screens_assigned_log.CreatedOn = item.CreatedOn;
                            jntuh_college_screens_assigned_log.CreatedBy = item.CreatedBy;
                            jntuh_college_screens_assigned_log.UpdatedOn = item.UpdatedOn;
                            jntuh_college_screens_assigned_log.UpdatedBy = item.UpdatedBy;
                            jntuh_college_screens_assigned_log.LoggedOn = DateTime.Now;
                            jntuh_college_screens_assigned_log.LoggedBy = userId;
                            jntuh_college_screens_assigned_loglist.Add(jntuh_college_screens_assigned_log);
                            //db.jntuh_college_screens_assigned_log.Add(jntuh_college_screens_assigned_log);                           
                        }
                        //db.SaveChanges();
                        //ToRemove Data in jntuh_college_screens_assigned
                        //foreach (var item in AssignedScreens)
                        //{
                        //    db.jntuh_college_screens_assigned.Remove(item);
                        //    db.SaveChanges();
                        //}
                        
                    }
                    #endregion
                }
                jntuh_college_screens_assigned_loglist.ForEach(a => db.jntuh_college_screens_assigned_log.Add(a));
                db.SaveChanges();
                db.jntuh_college_screens_assigned.Where(d => EditCollegeId.Contains(d.CollegeId)).ToList().ForEach(d => db.jntuh_college_screens_assigned.Remove(d));
                db.SaveChanges();
            }
            else
            {
                #region To insert into Single  AssignedScreens Log
                var AssignedScreens = db.jntuh_college_screens_assigned.Where(c => c.CollegeId == collegeAssignedScreens.CollegeId).ToList();
                if (AssignedScreens.Count() > 0)
                {
                    foreach (var item in AssignedScreens)
                    {

                        jntuh_college_screens_assigned_log jntuh_college_screens_assigned_log = new jntuh_college_screens_assigned_log();
                        jntuh_college_screens_assigned_log.CollegeId = item.CollegeId;
                        jntuh_college_screens_assigned_log.ScreenId = item.ScreenId;
                        jntuh_college_screens_assigned_log.IsEditable = item.IsEditable;
                        jntuh_college_screens_assigned_log.CreatedOn = item.CreatedOn;
                        jntuh_college_screens_assigned_log.CreatedBy = item.CreatedBy;
                        jntuh_college_screens_assigned_log.UpdatedOn = item.UpdatedOn;
                        jntuh_college_screens_assigned_log.UpdatedBy = item.UpdatedBy;
                        jntuh_college_screens_assigned_log.LoggedOn = DateTime.Now;
                        jntuh_college_screens_assigned_log.LoggedBy = userId;
                        db.jntuh_college_screens_assigned_log.Add(jntuh_college_screens_assigned_log);
                        
                    }
                    db.SaveChanges();
                    //ToRemove Data in jntuh_college_screens_assigned
                    //foreach (var item in AssignedScreens)
                    //{
                    //    db.jntuh_college_screens_assigned.Remove(item);
                    //    db.SaveChanges();
                    //}
                    db.jntuh_college_screens_assigned.Where(d => d.CollegeId == collegeAssignedScreens.CollegeId).ToList().ForEach(d => db.jntuh_college_screens_assigned.Remove(d));
                    db.SaveChanges();
                }
                #endregion
            }
            #endregion

            #region To insert into  AssignedScreens
            //List<jntuh_college_edit_status> AssignedCoolegesID = db.jntuh_college_edit_status.Select(e => e).ToList();
            if (SelectedCollegeId == "0")
            {
                List<jntuh_college_screens_assigned> jntuh_college_screens_assignedlist = new List<jntuh_college_screens_assigned>();
                foreach (var AssignedScreensID in EditCollegeId)
                {
                    #region To insert into  AssignedScreens
                    var selectedScreens = collegeAssignedScreens.collegeScreens.Where(c => c.IsSelected == true).ToList();
                    //var selectedScreens = db.jntuh_college_screens_assigned.Where(c => c.CollegeId == AssignedScreensID.collegeId).ToList();
                    if (selectedScreens.Count() > 0)
                    {
                        foreach (var item in selectedScreens)
                        {
                            jntuh_college_screens_assigned jntuh_college_screens_assigned = new jntuh_college_screens_assigned();
                            jntuh_college_screens_assigned.CollegeId = AssignedScreensID;
                            jntuh_college_screens_assigned.ScreenId = item.Id;
                            jntuh_college_screens_assigned.IsEditable = true;
                            jntuh_college_screens_assigned.CreatedOn = DateTime.Now;
                            jntuh_college_screens_assigned.CreatedBy = userId;
                            jntuh_college_screens_assignedlist.Add(jntuh_college_screens_assigned);
                            //db.jntuh_college_screens_assigned.Add(jntuh_college_screens_assigned);                           
                        }
                        
                        TempData["Success"] = "Screens assigned.";

                        // Commented on Srinivas -- Sending all maila at a time coming time out error  --21/12/2015

                        ////send email to college with editable dates when edit status is TRUE
                        //string email = db.jntuh_address.AsNoTracking().Where(a => a.addressTye == "COLLEGE" && a.collegeId == AssignedScreensID.collegeId).Select(a => a.email).FirstOrDefault();

                        //if (email == null)
                        //{
                        //    string code = db.jntuh_college.AsNoTracking().Where(c => c.id == collegeAssignedScreens.CollegeId).Select(c => c.collegeCode).FirstOrDefault();
                        //    email = db.all_college_emails.AsNoTracking().Where(e => e.Code == code).Select(e => e.Email).FirstOrDefault();
                        //}

                        //int user_Id = db.jntuh_college_users.Where(cu => cu.collegeID == AssignedScreensID.collegeId).Select(cu => cu.userID).FirstOrDefault();
                        //string username = db.my_aspnet_users.Find(user_Id).name;
                        //if (!String.IsNullOrEmpty(email))
                        //{

                        //    IUserMailer mailer = new UserMailer();
                        //    mailer.SendEditDates(email, "aac.do.not.reply@gmail.com", "AAC, JNTUH: Edit option enabled", username, collegeAssignedScreens.FromDate, collegeAssignedScreens.ToDate).SendAsync();
                        //}


                    }
                    else
                    {
                        TempData["Error"] = "Screens not assigned. Please try again.";
                    }
                    #endregion
                }
                jntuh_college_screens_assignedlist.ForEach(d=>db.jntuh_college_screens_assigned.Add(d));
                db.SaveChanges();
                TempData["Success"] = "Screens assigned.";
            }
            else
            {
                #region To insert into  AssignedScreens
                var selectedScreens = collegeAssignedScreens.collegeScreens.Where(c => c.IsSelected == true).ToList();
                if (selectedScreens.Count() > 0)
                {
                    foreach (var item in selectedScreens)
                    {
                        jntuh_college_screens_assigned jntuh_college_screens_assigned = new jntuh_college_screens_assigned();
                        jntuh_college_screens_assigned.CollegeId = collegeAssignedScreens.CollegeId;
                        jntuh_college_screens_assigned.ScreenId = item.Id;
                        jntuh_college_screens_assigned.IsEditable = true;
                        jntuh_college_screens_assigned.CreatedOn = DateTime.Now;
                        jntuh_college_screens_assigned.CreatedBy = userId;
                        db.jntuh_college_screens_assigned.Add(jntuh_college_screens_assigned);
                       
                    }
                    db.SaveChanges();
                    TempData["Success"] = "Screens assigned.";

                    //send email to college with editable dates when edit status is TRUE
                    string email = db.jntuh_address.AsNoTracking().Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeAssignedScreens.CollegeId).Select(a => a.email).FirstOrDefault();

                    if (email == null)
                    {
                        string code = db.jntuh_college.AsNoTracking().Where(c => c.id == collegeAssignedScreens.CollegeId).Select(c => c.collegeCode).FirstOrDefault();
                        //Commented on 18-06-2018 by Narayana Reddy
                        //email = db.all_college_emails.AsNoTracking().Where(e => e.Code == code).Select(e => e.Email).FirstOrDefault();
                    }

                    int user_Id = db.jntuh_college_users.Where(cu => cu.collegeID == collegeAssignedScreens.CollegeId).Select(cu => cu.userID).FirstOrDefault();
                    string username = db.my_aspnet_users.Find(user_Id).name;

                    //IUserMailer mailer = new UserMailer();
                    //mailer.SendEditDates(email, "aac.do.not.reply@gmail.com", "AAC, JNTUH: Edit option enabled", username, collegeAssignedScreens.FromDate, collegeAssignedScreens.ToDate).SendAsync();

                }
                else
                {
                    TempData["Error"] = "Screens not assigned. Please try again.";
                }
                #endregion
            }
            #endregion



            return RedirectToAction("CollegeScreens", new { id = collegeAssignedScreens.CollegeId });
        }
     

        public ActionResult AddorEditCollegeScreen(int? id)
        {
            jntuh_college_screens jntuh_college_screens = new jntuh_college_screens();
            if (id != null)
            {
                jntuh_college_screens = db.jntuh_college_screens.Where(c => c.Id == id).FirstOrDefault();
            }
            else
            {
            }
            return PartialView("_AddorEditCollegeScreen", jntuh_college_screens);
        }

    }
}
