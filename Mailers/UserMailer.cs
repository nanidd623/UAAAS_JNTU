using Mvc.Mailer;
using System.Collections.Generic;

namespace UAAAS.Models
{
    public class UserMailer : MailerBase, IUserMailer
    {
        public UserMailer()
        {
            MasterName = "_Layout";
        }

        public virtual MvcMailMessage Welcome(string Email, string View, string Subject, string Username)
        {
            ViewBag.Username = Username;
            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = View;
                x.To.Add(Email);
            });
        }

        public virtual MvcMailMessage Welcome(string Email, string View, string Subject, string Username, string Password, string encUsername, string encPassword)
        {
            ViewBag.Username = Username;
            ViewBag.Password = Password;
            ViewBag.EncryptUsername = encUsername;
            ViewBag.EncryptPassword = encPassword;
            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = View;
                x.To.Add(Email);
            });
        }

        public virtual MvcMailMessage FacultyOnlineRegistration(string Email, string View, string Subject, string Username, string Password, string FacultyRegistrationNumber, string UniqueID, string facultyType, string PANNumber)
        {
            ViewBag.Username = Username;
            ViewBag.Password = Password;
            ViewBag.FacultyRegistrationNumber = FacultyRegistrationNumber;
            ViewBag.UniqueID = UniqueID;
            ViewBag.Type = facultyType;
            ViewBag.PANNumber = !string.IsNullOrEmpty(PANNumber) ? PANNumber : "";

            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = View;
                x.To.Add(Email);
            });
        }

        public virtual MvcMailMessage FacultyMobilityRequestMail(string Email, string View, string Subject, string TicketId, string Name, string RegistrationNumber, string CollegeName)
        {
            ViewBag.Name = Name;
            ViewBag.ticketId = TicketId;
            ViewBag.RegistrationNumber = RegistrationNumber;
            ViewBag.CollegeName = CollegeName;
            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = View;
                x.To.Add(Email);
            });
        }

        public virtual MvcMailMessage GoodBye()
        {
            //ViewBag.Data = someObject;
            return Populate(x =>
            {
                x.Subject = "GoodBye";
                x.ViewName = "GoodBye";
                x.To.Add("some-email@example.com");
            });
        }

        public virtual MvcMailMessage AffiliationRequestByUser(string email)
        {
            //ViewBag.Data = someObject;
            return Populate(x =>
            {
                x.Subject = "Request for JNTUH Affiliation";
                x.ViewName = "AffiliationRequestByUser";
                x.To.Add(email);
            });
        }

        public virtual MvcMailMessage SendOrder(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments)
        {
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "OrderToCommittee";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
        }

        public virtual MvcMailMessage SendOrderToPrincipal(string to, string cc, string bcc, string subject, string InspectionDate)
        {
            ViewBag.InspectionDate = InspectionDate;

            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "OrderToPrincipal";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

            });
        }

        public virtual MvcMailMessage SendEditDates(string To, string Cc, string Subject, string Username, string StartDate, string EndDate)
        {
            ViewBag.Username = Username;
            ViewBag.Start = StartDate;
            ViewBag.End = EndDate;
            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = "SendEditDates";
                x.To.Add(To);
                x.CC.Add(Cc);
            });
        }

        public virtual MvcMailMessage SendAttachmentToAllColleges(string to, string cc, string bcc, string subject, string Message, List<System.Net.Mail.Attachment> attachments)
        {

            ViewBag.Message = subject;
            ViewBag.MailMessage = Message;
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "AllCollegeMails";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
        }
        public virtual MvcMailMessage SendMailToCollege(string To, string Cc, string Bc, string Subject, string Username, string Message, List<System.Net.Mail.Attachment> attachments)
        {
            ViewBag.Message = Subject;
            ViewBag.MailMessage = Message;
            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = "AllCollegeMails";
                x.To.Add(To);

                if (!string.IsNullOrWhiteSpace(Cc))
                {
                    x.CC.Add(Cc);
                }

                if (!string.IsNullOrWhiteSpace(Bc))
                {
                    x.CC.Add(Bc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
            //ViewBag.Username = Username;
            //ViewBag.Message = Message;            
            //return Populate(x =>
            //{
            //    x.Subject = Subject;
            //    x.ViewName = "SendMailToCollege";
            //    x.To.Add(To);
            //    x.CC.Add(Cc);
            //    if (!string.IsNullOrWhiteSpace(Cc))
            //    {
            //        x.CC.Add(Cc);
            //    }

            //    if (!string.IsNullOrWhiteSpace(Bc))
            //    {
            //        x.CC.Add(Bc);
            //    }

            //    foreach (var attachment in attachments)
            //    {
            //        if (attachment != null)
            //        {
            //            x.Attachments.Add(attachment);
            //        }
            //    }
            //});
        }

        public virtual MvcMailMessage SendDeficiencyLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments)
        {
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "DeficiencyLetter";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
        }

        public virtual MvcMailMessage SendCommonDeficiencyLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments)
        {
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "CommonDeficiencyLetter";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
        }

        public virtual MvcMailMessage SendProformaNotSubmittedLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments)
        {
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "SendProformaNotSubmittedLetter";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        x.Attachments.Add(attachment);
                    }
                }

            });
        }

        public virtual MvcMailMessage ClusterEmails(string to, string cc, string bcc, string subject, string message)
        {
            ViewBag.subject = subject;
            ViewBag.message = message;
            return Populate(x =>
            {
                x.Subject = subject;
                x.ViewName = "ClusterEmails";
                x.To.Add(to);

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    x.CC.Add(cc);
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    x.CC.Add(bcc);
                }

            });
        }

        public virtual MvcMailMessage PaymentResponse(string Email, string View, string collegecode, string customerid, string txnrefno, string bankrefno, decimal txnamount, string txndate, string description, string challanno, string message, string subject)
        {
            ViewBag.subject = subject;
            ViewBag.message = message;
            ViewBag.collegecode = collegecode;
            ViewBag.customerid = customerid;
            ViewBag.txnrefno = txnrefno;
            ViewBag.bankrefno = bankrefno;
            ViewBag.txnamount = txnamount;
            ViewBag.txndate = txndate;
            ViewBag.description = description;
            ViewBag.challanno = challanno;
            return Populate(x =>
            {
                x.Subject = subject;
                x.To.Add(Email);
                x.ViewName = View;

                x.Body = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine + "Your Paymnet Status ," + System.Environment.NewLine + "College code / College name :- " + collegecode + System.Environment.NewLine + "Customer Id :- " + customerid + System.Environment.NewLine + "Transaction Ref.no :- " + txnrefno + System.Environment.NewLine
                     + "Bank Refno :- " + bankrefno + System.Environment.NewLine + "Transaction Amount :- " + txnamount + System.Environment.NewLine + "Transaction Date :- " + txndate + System.Environment.NewLine + "Payment Description :-" + description + System.Environment.NewLine + System.Environment.NewLine
                     + "Regards" + System.Environment.NewLine + "DIRECTOR, UAAC" + System.Environment.NewLine + "JNTUH.";
            });
        }

        public virtual MvcMailMessage FacultyNotApprovedStatus(string Email, string view, string regno, string status,string subject)
        {
            ViewBag.regno = regno;
            ViewBag.status = status;
            ViewBag.subject = subject;

            return Populate(x =>
            {
                x.Subject = subject;
                x.To.Add(Email);
                x.ViewName = view;

                x.Body = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine + "Your Registration Number is :- " + regno + System.Environment.NewLine + "Your Faculty Registration is not confirmed because of :- " + status +"."+ System.Environment.NewLine +
                         System.Environment.NewLine + "You may edit by resubmitting all the valid documents." + System.Environment.NewLine + System.Environment.NewLine 
                         + "Regards" + System.Environment.NewLine + "DIRECTOR, UAAC" + System.Environment.NewLine + "JNTUH.";
            });
        }

        public virtual MvcMailMessage FacultyComplaintRequestMail(string Email, string View, string Subject, string TicketId, string Name, string RegistrationNumber, string CompalintDate)
        {
            ViewBag.Username = Name;
            ViewBag.referenceID = TicketId;

            return Populate(x =>
            {
                x.Subject = Subject;
                x.ViewName = View;
                x.To.Add(Email);
            });
        }
    }
}