using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FeedbackController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        //
        // GET: /Feedback/Suggestions
        public ActionResult Suggestions()
        {
            FeedBack feedBackDetails = new FeedBack();

            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;
            
            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Suggestions";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //
        // POST: /Feedback/Suggestions
        [HttpPost]
        public ActionResult Suggestions(FeedBack feedBackDetails)
        {
            if (ModelState.IsValid)
            {
                feedBackDetails.feedbackType = "Suggestion";
                FeedBackDetails(feedBackDetails);
            }
            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;

            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Suggestions";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //
        // GET: /Feedback/Complaints
        public ActionResult Complaints()
        {
            FeedBack feedBackDetails = new FeedBack();

            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;

            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Complaints";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //
        // POST: /Feedback/Complaints
        [HttpPost]
        public ActionResult Complaints(FeedBack feedBackDetails)
        {
            if (ModelState.IsValid)
            {
                feedBackDetails.feedbackType = "Complaint";
                FeedBackDetails(feedBackDetails);
            }
            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;

            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Complaints";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //
        // GET: /Feedback/Opinions
        public ActionResult Opinions()
        {
            FeedBack feedBackDetails = new FeedBack();

            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;

            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Opinions";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //
        // POST: /Feedback/Opinions
        [HttpPost]
        public ActionResult Opinions(FeedBack feedBackDetails)
        {
            if (ModelState.IsValid)
            {
                feedBackDetails.feedbackType = "Opinion";
                FeedBackDetails(feedBackDetails);
            }
            //get all colleges where status is true
            List<jntuh_college> collegeDetails = db.jntuh_college.Where(college => college.isActive == true).ToList();
            jntuh_college newCollege = new jntuh_college();
            newCollege.collegeName = "Other";
            newCollege.collegeCode = "Other";
            collegeDetails.Add(newCollege);
            ViewBag.Colleges = collegeDetails;

            //get all stack holders where status is true
            ViewBag.StackHolders = db.jntuh_stackholder_type.Where(stackholder => stackholder.isActive == true).ToList();
            ViewBag.FeedBackType = "Opinions";
            return View("~/Views/Users/FeedBack.cshtml", feedBackDetails);
        }

        //Saving Feed back Details
        private void FeedBackDetails(FeedBack feedBackDetails)
        {
            jntuh_feedback feedBackInformation = new jntuh_feedback();
            feedBackInformation.college = feedBackDetails.college;
            feedBackInformation.stackholderId = feedBackDetails.stackholderId;
            feedBackInformation.feedbackType = feedBackDetails.feedbackType;
            feedBackInformation.subject = feedBackDetails.subject;
            feedBackInformation.fullName = feedBackDetails.fullName;
            feedBackInformation.phoneNumber = feedBackDetails.phoneNumber;
            feedBackInformation.emailAddress = feedBackDetails.emailAddress;
            feedBackInformation.comments = feedBackDetails.comments;
            feedBackInformation.createdOn = DateTime.Now;
            db.jntuh_feedback.Add(feedBackInformation);
            db.SaveChanges();
            TempData["Success"] = "FeedBack Sent successfully.";
        }
    }
}
