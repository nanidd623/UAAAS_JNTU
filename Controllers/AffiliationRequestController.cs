using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AffiliationRequestController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /AffiliationRequest/

        public ViewResult Index()
        {
            var jntuh_affiliation_requests = db.jntuh_affiliation_requests.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1).Where(j => j.isApproved == null).OrderByDescending(j => j.id);

            ViewBag.State = db.jntuh_state.ToList();
            ViewBag.District = db.jntuh_district.Where(d=>d.isActive==true).ToList();
            return View(jntuh_affiliation_requests.ToList());
        }

        //
        // GET: /AffiliationRequest/Details/5

        [HttpGet]
        public ViewResult Details(int id)
        {
            jntuh_affiliation_requests jntuh_affiliation_requests = db.jntuh_affiliation_requests.Find(id);

            ViewBag.State = db.jntuh_state.ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.ToList();
            ViewBag.CollegeStatus = db.jntuh_college_status.ToList();
            return View(jntuh_affiliation_requests);
        }

        [HttpPost]
        public ViewResult Details(jntuh_affiliation_requests jntuh_affiliation_requests)
        {
            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            jntuh_affiliation_requests.jntuh_state = db.jntuh_state.Find(jntuh_affiliation_requests.stateId);
            jntuh_affiliation_requests.jntuh_district = db.jntuh_district.Find(jntuh_affiliation_requests.districtId);
            jntuh_affiliation_requests.jntuh_college_status = db.jntuh_college_status.Find(jntuh_affiliation_requests.collegeStatusId);
            jntuh_affiliation_requests.jntuh_college_type = db.jntuh_college_type.Find(jntuh_affiliation_requests.collegeTypeId);
            jntuh_affiliation_requests.updatedBy = createdBy;
            jntuh_affiliation_requests.updatedOn = DateTime.Now;
            if (jntuh_affiliation_requests.comments == null)
            {
                jntuh_affiliation_requests.comments = string.Empty;
            }
            if (ModelState.IsValid)
            {
                jntuh_affiliation_requests.id = jntuh_affiliation_requests.id;
                db.Entry(jntuh_affiliation_requests).State = EntityState.Modified;
                db.SaveChanges();
            }
            jntuh_college jntuh_college = new jntuh_college();

            jntuh_college.collegeName = jntuh_affiliation_requests.collegeName;
            jntuh_college.collegeCode = "00";
            jntuh_college.collegeTypeID = jntuh_affiliation_requests.collegeTypeId;
            jntuh_college.collegeStatusID = jntuh_affiliation_requests.stateId;
            jntuh_college.societyName = jntuh_affiliation_requests.societyName == null ? string.Empty : jntuh_affiliation_requests.societyName;
            jntuh_college.isActive = true;
            jntuh_college.createdBy = createdBy;
            jntuh_college.createdOn = DateTime.Now;
            jntuh_college.collegeAffiliationTypeID = db.jntuh_college_affiliation_type.Select(at => at.id).FirstOrDefault();

            jntuh_college.jntuh_college_status = db.jntuh_college_status.Find(jntuh_affiliation_requests.collegeStatusId);
            jntuh_college.jntuh_college_type = db.jntuh_college_type.Find(jntuh_affiliation_requests.collegeTypeId);

            var rowExists = (from c in db.jntuh_college
                             join a in db.jntuh_address on c.id equals a.collegeId
                             where (a.email == jntuh_affiliation_requests.email || c.collegeName == jntuh_affiliation_requests.collegeName) && c.isActive == true
                             select c.collegeName);
            if (rowExists.Count() == 0 && jntuh_affiliation_requests.isApproved == true)
            {
                if (ModelState.IsValid)
                {
                    db.jntuh_college.Add(jntuh_college);
                    db.SaveChanges();
                }
            }
            else
            {
                TempData["Error"] = "Sorry, somebody has already requested affiliation with the same College Nameor or Email Address.";

            }
            int collegeID = jntuh_college.id;
            if (collegeID != 0)
            {
                //create college address object
                #region jntuh_address
                jntuh_address jntuh_address = new jntuh_address();
                jntuh_address.collegeId = collegeID;
                jntuh_address.addressTye = "COLLEGE";
                jntuh_address.address = jntuh_affiliation_requests.collegeAddress;
                jntuh_address.townOrCity = jntuh_affiliation_requests.townOrCity;
                jntuh_address.mandal = jntuh_affiliation_requests.mandal;
                jntuh_address.districtId = jntuh_affiliation_requests.districtId;
                jntuh_address.stateId = jntuh_affiliation_requests.stateId;
                jntuh_address.pincode = jntuh_affiliation_requests.pincode;
                jntuh_address.fax = jntuh_affiliation_requests.fax;
                jntuh_address.landline = jntuh_affiliation_requests.landline;
                jntuh_address.mobile = jntuh_affiliation_requests.mobile;
                jntuh_address.email = jntuh_affiliation_requests.email;

                jntuh_address.website = jntuh_affiliation_requests.website;
                jntuh_address.createdBy = createdBy;
                jntuh_address.createdOn = DateTime.Now;
                jntuh_address.updatedBy = createdBy;
                jntuh_address.updatedOn = DateTime.Now;

                db.jntuh_address.Add(jntuh_address);
                db.SaveChanges();
                #endregion


                //College EditStatus
                #region jntuh_college_edit_status
                jntuh_college_edit_status jntuh_college_edit_status = new jntuh_college_edit_status();
                jntuh_college_edit_status.collegeId = collegeID;
                jntuh_college_edit_status.IsCollegeEditable = true;
                jntuh_college_edit_status.editFromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                //jntuh_college_edit_status.editToDate = DateTime.Now.AddDays(2);

                //This is for Day end date don't change any thing-Start 7/18/2014 4:41:15 PM
                string seditToDate = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                string[] date = seditToDate.Split('/');
                string dd = date[0];
                string mm = date[1];
                string yyyy = date[2];
                string streditToDate = mm + "/" + dd + "/" + yyyy + " 23:59:59";

                jntuh_college_edit_status.editToDate =Convert.ToDateTime(streditToDate);

                jntuh_college_edit_status.createdBy = createdBy;
                jntuh_college_edit_status.createdOn = DateTime.Now;
                db.jntuh_college_edit_status.Add(jntuh_college_edit_status);
                db.SaveChanges();

                //send email to college with editable dates when edit status is TRUE
                var collegeeditstatus= db.jntuh_college_edit_status.Where(s=>s.collegeId==collegeID).Select(s=>s).FirstOrDefault();
                if (collegeeditstatus.IsCollegeEditable == true)
                {
                    string email = string.Empty;
                    string username = string.Empty;
                    email = db.jntuh_address.AsNoTracking().Where(a => a.addressTye == "COLLEGE" && a.collegeId == collegeID).Select(a => a.email).FirstOrDefault();
                    int userId = db.jntuh_college_users.Where(cu => cu.collegeID == collegeID).Select(cu => cu.userID).FirstOrDefault();
                    if (userId != 0)
                    {
                         username = db.my_aspnet_users.Find(userId).name;
                    }

                    IUserMailer mailer = new UserMailer();
                    if (email != string.Empty)
                    {
                        mailer.SendEditDates(email, "aac.do.not.reply@gmail.com", "AAC, JNTUH: Edit option enabled", username, collegeeditstatus.editFromDate.ToString(), collegeeditstatus.editToDate.ToString()).SendAsync();
                    }
                }


                #endregion


                TempData["Success"] = "Your Affiliation Request has approved successfully.";

                var affiliationRequests = db.jntuh_affiliation_requests.Include(j => j.my_aspnet_users).Include(j => j.my_aspnet_users1);

                ViewBag.State = db.jntuh_state.ToList();
                ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            }


            return View(jntuh_affiliation_requests);
        }

        //
        // GET: /AffiliationRequest/Create

        public ActionResult Create()
        {

            ViewBag.State = db.jntuh_state.ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.ToList();
            ViewBag.CollegeStatus = db.jntuh_college_status.ToList();
            return View();
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetDistrictList(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }
            var districtList = this.GetDistricts(Convert.ToInt32(id));

            var myData = districtList.Select(a => new SelectListItem()
            {
                Text = a.districtName,
                Value = a.id.ToString(),
            });

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        private IList<jntuh_district> GetDistricts(int id)
        {
            return db.jntuh_district.Where(d => d.stateId == id).ToList();
        }


        // POST: /AffiliationRequest/Create

        [HttpPost]
        public ActionResult Create(jntuh_affiliation_requests jntuh_affiliation_requests)
        {
            if (ModelState.IsValid)
            {

                //get only active list items
                ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                ViewBag.District = db.jntuh_district.Where(d => d.isActive == true && d.stateId == jntuh_affiliation_requests.stateId).ToList();
                ViewBag.CollegeType = db.jntuh_college_type.Where(t => t.isActive == true).ToList();
                ViewBag.CollegeStatus = db.jntuh_college_status.Where(s => s.isActive == true).ToList();


                var rowExists = (from r in db.jntuh_affiliation_requests
                                 where (r.email == jntuh_affiliation_requests.email || r.collegeName == jntuh_affiliation_requests.collegeName)
                                 select r.collegeName);
                if (rowExists.Count() == 0)
                {
                    if (jntuh_affiliation_requests.comments == null)
                    { jntuh_affiliation_requests.comments = string.Empty; }

                    jntuh_affiliation_requests.isActive = true;
                    jntuh_affiliation_requests.isApproved = null;
                    jntuh_affiliation_requests.createdBy = 1;
                    jntuh_affiliation_requests.createdOn = DateTime.Now;
                    db.jntuh_affiliation_requests.Add(jntuh_affiliation_requests);
                    //validate captcha
                    if (Session["Captcha"] == null || Session["Captcha"].ToString() != jntuh_affiliation_requests.Captcha)
                    {
                        //ModelState.AddModelError("Captcha", "Wrong value of sum, please try again.");
                        TempData["Error"] = "Image Text Should be matched with Image.";
                        return View(jntuh_affiliation_requests);
                    }
                    else
                    {

                        // ModelState.AddModelError("", "Captcha matched.");
                        db.SaveChanges();
                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.AffiliationRequestByUser(jntuh_affiliation_requests.email).SendAsync();
                        TempData["Success"] = "Your Affiliation Request has been sent successfully.";
                    }
                }
                else
                {
                    TempData["Error"] = "Sorry, somebody has already requested affiliation with the same College Name or Email Address.";
                }

            }

            //get only active list items
            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true && d.stateId == jntuh_affiliation_requests.stateId).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.Where(t => t.isActive == true).ToList();
            ViewBag.CollegeStatus = db.jntuh_college_status.Where(s => s.isActive == true).ToList();

            return View(jntuh_affiliation_requests);
        }
        public ActionResult CaptchaImage(string prefix, bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);

            //This is Numeric Captcha Image Start
            //    //generate new question
            //    int a = rand.Next(10, 99);
            //    int b = rand.Next(0, 9);
            //    var captcha = string.Format("{0} + {1} = ?", a, b);

            //    //store answer
            //    Session["Captcha" + prefix] = a + b;
            //End


            //This is AlphaNumeric Captcha Image Start Remaing code same for both captcha
            var captcha = RandomText.GenerateRandomText(6);
            Session["Captcha" + prefix] = captcha;
            ViewBag.captcha = captcha;
            //End

            //image stream
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }

                //add question
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }
            return img;
        }
        public static class RandomText
        {
            public static string GenerateRandomText(int textLength)
            {
                const string Chars = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijklmnpqrstuvwxyz123456789";
                var random = new Random();
                var result = new string(
                    Enumerable.Repeat(Chars, textLength)
                        .Select(s => s[random.Next(s.Length)])
                        .ToArray());
                return result;
            }
        }

        //
        // GET: /AffiliationRequest/Edit/5

        public ActionResult Edit(int id)
        {
            jntuh_affiliation_requests jntuh_affiliation_requests = db.jntuh_affiliation_requests.Find(id);
            ViewBag.IsApprove = jntuh_affiliation_requests.isApproved;
            ViewBag.IsActive = jntuh_affiliation_requests.isActive;

            ViewBag.State = db.jntuh_state.ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.ToList();
            ViewBag.CollegeStatus = db.jntuh_college_status.ToList();
            return View(jntuh_affiliation_requests);
        }

        //
        // POST: /AffiliationRequest/Edit/5

        [HttpPost]
        public ActionResult Edit(jntuh_affiliation_requests jntuh_affiliation_requests)
        {
            jntuh_affiliation_requests.isApproved = jntuh_affiliation_requests.isApproved;
            jntuh_affiliation_requests.societyName = jntuh_affiliation_requests.societyName;
            if (jntuh_affiliation_requests.comments == null)
            {
                jntuh_affiliation_requests.comments = string.Empty;
            }

            jntuh_affiliation_requests.jntuh_state = db.jntuh_state.Find(jntuh_affiliation_requests.stateId);
            jntuh_affiliation_requests.jntuh_district = db.jntuh_district.Find(jntuh_affiliation_requests.districtId);
            jntuh_affiliation_requests.jntuh_college_status = db.jntuh_college_status.Find(jntuh_affiliation_requests.collegeStatusId);
            jntuh_affiliation_requests.jntuh_college_type = db.jntuh_college_type.Find(jntuh_affiliation_requests.collegeTypeId);

            int createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);


            jntuh_affiliation_requests.updatedBy = createdBy;
            jntuh_affiliation_requests.updatedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Entry(jntuh_affiliation_requests).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.State = db.jntuh_state.ToList();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();
            ViewBag.CollegeType = db.jntuh_college_type.ToList();
            ViewBag.CollegeStatus = db.jntuh_college_status.ToList();

            return View(jntuh_affiliation_requests);
        }

        //
        // GET: /AffiliationRequest/Delete/5

        public ActionResult Delete(int id)
        {
            jntuh_affiliation_requests jntuh_affiliation_requests = db.jntuh_affiliation_requests.Find(id);
            return View(jntuh_affiliation_requests);
        }

        //
        // POST: /AffiliationRequest/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            jntuh_affiliation_requests jntuh_affiliation_requests = db.jntuh_affiliation_requests.Find(id);
            db.jntuh_affiliation_requests.Remove(jntuh_affiliation_requests);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}