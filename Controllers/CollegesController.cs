using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.SqlServer.Server;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int[] CollegeIds;
        //private int[] CollegeIds163;

        //
        // GET: /Colleges/
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            //int[] somecolleges = new int[] { 201,176,4,8, 5, 6, 7, 9, 11, 12, 20, 23, 24, 26, 27, 29, 30, 32, 33,125,77,75,88,41,198,180,108,134,};
            List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            //List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true && somecolleges.Contains(e.id)).Select(e => e).ToList();
            //db.jntuh_college.Where(i => collegeIds.Contains(i.id) || collegeIds1.Contains(i.id)).Select(i => i).ToList();
            //List<jntuh_college> colleges = db.jntuh_college.Select(i => i).ToList();
            return View(colleges);
        }

        public ActionResult CollegesDocuments()
        {
            int[] collegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(s => s.collegeId).ToArray();
            List<CollegesSupportDocuments> CollegesSupportDocumentsList= new List<CollegesSupportDocuments>();
            List<jntuh_college> colleges = db.jntuh_college.Where(e => collegeIds.Contains(e.id)).Select(e => e).ToList();
            List<jntuh_college_enclosures> jntuh_college_enclosures =
                db.jntuh_college_enclosures.Where(a => a.enclosureId == 17).Select(s => s).ToList();

            foreach (var college in colleges)
            {
                CollegesSupportDocuments documents=new CollegesSupportDocuments();
                documents.CollegeCode = college.collegeCode;
                documents.CollegeName = college.collegeName;
                documents.EOAdocument = jntuh_college_enclosures.Where(a=>a.collegeID==college.id).Select(s=>s.path).FirstOrDefault();
                CollegesSupportDocumentsList.Add(documents);
            }
            return View(CollegesSupportDocumentsList);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ReInspectionIndex()
        {
            var collegeIds = new[] { 48, 62, 125, 140, 143, 157, 161, 162, 252, 292, 305, 370, 371, 401, 415, 416, 422, 424, 447, 20, 33, 83, 107, 338 };
            var colleges = db.jntuh_college.Where(i => collegeIds.Contains(i.id)).Select(i => i).ToList();
            return View(colleges);
        }



        [Authorize(Roles = "Admin")]
        public ActionResult SIandAICTEFlagsforColleges()
        {
            int[] collegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).Distinct().ToArray();

            List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true && e.isClosed == false && collegeIds.Contains(e.id)).Select(e => e).OrderBy(e => e.collegeCode).ToList();
            
            return View(colleges);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult SIStatus(int? id)
        {
            CollegeStatus College = new CollegeStatus();
            if (id != null)
            {
                var collegeStatusflags =db.jntuh_collegestatus.Where(e => e.CollegeId == id).Select(e => e).FirstOrDefault();
                if (collegeStatusflags != null)
                {
                    College = db.jntuh_college.Where(C => C.id == id).Select(C => new CollegeStatus
                    {
                        CollegeId = C.id,
                        CollegeCode = C.collegeCode,
                        CollegeName = C.collegeName,
                        CollegeStatusId = collegeStatusflags.Id,
                        SIflag = collegeStatusflags.SIStatus,
                        AICTEFlag = collegeStatusflags.AICTEStatus,
                        Isactive = collegeStatusflags.CollegeStatus
                    }).FirstOrDefault();
                }
                else
                {
                    College = db.jntuh_college.Where(C => C.id == id).Select(C => new CollegeStatus
                    {
                        CollegeId = C.id,
                        CollegeCode = C.collegeCode,
                        CollegeName = C.collegeName
                    }).FirstOrDefault();
                }
            }

            return PartialView("_AICTEandSIFlags", College);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult SIStatus(CollegeStatus collegemodel)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (collegemodel != null)
            {
               
                //if (collegemodel.CollegeStatusId != 0)
                //{
               var  jntuhCollegeStatus1 = db.jntuh_collegestatus.Where(e => e.Id == collegemodel.CollegeStatusId).Select(e => e).FirstOrDefault();
               // }
                if (jntuhCollegeStatus1 != null)
                {
                    jntuhCollegeStatus1.CollegeCode = collegemodel.CollegeCode;
                    jntuhCollegeStatus1.CollegeId = collegemodel.CollegeId;
                  
                    jntuhCollegeStatus1.CollegeStatus = collegemodel.Isactive;
                    jntuhCollegeStatus1.SIStatus = collegemodel.SIflag;
                    jntuhCollegeStatus1.AICTEStatus = collegemodel.AICTEFlag;
                    jntuhCollegeStatus1.UpDatedBy = userID;
                    jntuhCollegeStatus1.UpdatedON = DateTime.Now;
                    db.Entry(jntuhCollegeStatus1).State=EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Record Updated SuccessFully...";
                }
                else
                {
                    var jntuhCollegeStatus = new jntuh_collegestatus();
                    jntuhCollegeStatus.CollegeCode = collegemodel.CollegeCode;
                    jntuhCollegeStatus.CollegeId = collegemodel.CollegeId;
                    jntuhCollegeStatus.CollegeStatus = collegemodel.Isactive;
                    jntuhCollegeStatus.SIStatus = collegemodel.SIflag;
                    jntuhCollegeStatus.AICTEStatus = collegemodel.AICTEFlag;
                    jntuhCollegeStatus.CreatedBy = userID;
                    jntuhCollegeStatus.CreatedON = DateTime.Now;
                    db.jntuh_collegestatus.Add(jntuhCollegeStatus);
                    db.SaveChanges();
                    TempData["Success"] = "Record Inserted SuccessFully...";
                }

            }
            else
            {
                TempData["Error"] = "Operation Failed..";
            }

            return RedirectToAction("SIandAICTEFlagsforColleges");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AffiliationIndex()
        {
            int[] collegeIds = db.jntuh_appeal_college_edit_status.Select(e => e.collegeId).Distinct().ToArray();

            int[] collegeIds1 = {42,273 };

            List<jntuh_college> colleges = db.jntuh_college.Where(i => collegeIds.Contains(i.id) || collegeIds1.Contains(i.id)).Select(i => i).ToList();
            return View(colleges);

        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllCollegesDeficiency()
        {
            int[] collegeIds = new int[] { 1, 2, 10, 18, 28, 59, 73, 92, 93, 102, 208, 280, 315, 346, 370, 402, 417, 456, 48, 125, 132, 140, 143, 161, 217, 252, 292, 305, 401, 415, 416, 422, 424, 14, 322, 371, 14, 62, 167, 257, 446,447, 141, 157, 162,388,338,255 };
            List<jntuh_college> colleges = db.jntuh_college.Where(e => e.isActive == true && collegeIds.Contains(e.id)).Select(e => e).ToList();
            return View(colleges);
        }

        //
        // GET: /Colleges/
        [HttpGet]
        public ActionResult Public()
        {
            PublicColleges colleges = GetCollegeList(0, 0);
            return View("~/Views/Admin/PublicColleges.cshtml", colleges);
        }

        [HttpPost]
        public ActionResult Public(PublicColleges colleges)
        {
            colleges = GetCollegeList(colleges.DistrictId, colleges.DegreeId);
            return View("~/Views/Admin/PublicColleges.cshtml", colleges);
        }

        private PublicColleges GetCollegeList(int DistrictId, int DegreeId)
        {
            int?[] CollegeIds163 = db.college_clusters
                             .Where(c => c.clusterName == "163 COLLEGES")
                             .Select(c => c.collegeId)
                             .ToArray();

            PublicColleges colleges = new PublicColleges();
            colleges.DistrictList = db.jntuh_district
                                      .Where(d => d.isActive == true)
                                      .OrderBy(d => d.districtName)
                                      .ToList();
            colleges.DegreeList = db.jntuh_degree
                                  .Where(d => d.isActive == true)
                                  .OrderBy(d => d.degreeDisplayOrder)
                                  .ToList();
            if (DistrictId == 0 && DegreeId == 0)
            {
                CollegeIds = db.jntuh_college
                             .Where(c => c.isActive == true && CollegeIds163.Contains(c.id))
                             .Select(c => c.id)
                             .ToArray();
            }
            else if (DistrictId != 0 && DegreeId == 0)
            {
                CollegeIds = db.jntuh_address
                               .Where(a => a.districtId == DistrictId &&
                                           a.addressTye == "COLLEGE" && CollegeIds163.Contains(a.id))
                               .Select(a => a.collegeId)
                               .Distinct()
                               .ToArray();
            }
            else if (DistrictId == 0 && DegreeId != 0)
            {
                CollegeIds = db.jntuh_college_degree
                               .Where(d => d.degreeId == DegreeId &&
                                           d.isActive == true && CollegeIds163.Contains(d.collegeId))
                               .Select(a => a.collegeId)
                               .Distinct()
                               .ToArray();
            }
            else if (DistrictId != 0 && DegreeId != 0)
            {
                CollegeIds = (from a in db.jntuh_address
                              join d in db.jntuh_college_degree on a.collegeId equals d.collegeId
                              where a.addressTye == "COLLEGE" &&
                                    a.districtId == DistrictId &&
                                    d.degreeId == DegreeId && CollegeIds163.Contains(a.collegeId)
                              select a.collegeId)
                             .ToArray();
            }
            colleges.PublicCollegeList = db.jntuh_college
                                                .Where(c => c.isActive == true && CollegeIds.Contains(c.id) && CollegeIds163.Contains(c.id))
                                                .OrderBy(c => c.collegeName.Trim())
                                                .Select(c => new PublicCollegeList
                                                {
                                                    id = c.id,
                                                    CollegeCode = c.collegeCode,
                                                    CollegeName = c.collegeName
                                                }).ToList();
            return colleges;
        }


        //Colleges status update
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateCollegeStatus(string id)
        {
            UpdateCollegeStatus updateCollegeStatus = new UpdateCollegeStatus();
            int collegeId = 0;
            if (id != null)
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var college = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            updateCollegeStatus.collegeId = collegeId;
            updateCollegeStatus.collegeCode = college.collegeCode;
            updateCollegeStatus.collegeName = college.collegeName;
            updateCollegeStatus.Active = college.isActive;

            //List<CollegeType> collegeType = new List<CollegeType>(){
            // new  CollegeType{id=1,collegeType="New"},
            // new CollegeType{id=2,collegeType="Closed"},
            // new CollegeType{id=3,collegeType="Reopen"},
            // new CollegeType{id=4,collegeType="Delete"},
            // new CollegeType{id=5,collegeType="Permanent"}
            //};
            //ViewBag.collegeType = collegeType.ToList();
            return PartialView(updateCollegeStatus);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateCollegeStatus(UpdateCollegeStatus updateCollegeStatus)
        {
            if (updateCollegeStatus != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var ClgStatus = updateCollegeStatus.Active;
                var jcollege = db.jntuh_college.Find(updateCollegeStatus.collegeId);
                jcollege.isActive = updateCollegeStatus.Active;
                jcollege.updatedBy = userID;
                jcollege.updatedOn = DateTime.Now;
                db.Entry(jcollege).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AffiliactionLettersFor40Percent()
        {
            // 40 % 80 % CollegeID's
            int[] collegeIds = new int[] { 1, 2, 10, 18, 28, 59, 73, 92, 93, 102, 208, 280, 315, 346, 370, 402, 417, 456, 48, 125, 132, 140, 143, 161, 217, 252, 292, 305, 401, 415, 416, 422, 424, 14, 322, 371, 14, 62, 167, 257, 446,447,141,157,162,388,338,255 };
            // int[] collegeIds = db.jntuh_appeal_college_edit_status.Select(e => e.collegeId).Distinct().ToArray();
            List<jntuh_college> colleges = db.jntuh_college.Where(i => collegeIds.Contains(i.id)).Select(i => i).ToList();

            return View(colleges);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AffiliactionLettersPharmacyUndertaking()
        {
            int[] collegeIds = new int[] { 9, 44, 60, 90, 117, 135, 136, 150, 206, 252, 253, 283, 284, 302, 348, 370, 384, 389, 395 };
            List<jntuh_college> colleges = db.jntuh_college.Where(i => collegeIds.Contains(i.id)).Select(i => i).ToList();

            return View(colleges);
        }

       

        public class CollegeType
        {
            public int id { get; set; }
            public string collegeType { get; set; }
        }
    }
    public class CollegeStatus
    {
        public int CollegeStatusId { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public bool SIflag { get; set; }
        public bool AICTEFlag { get; set; }
        public bool Isactive { get; set; }
    }
}
