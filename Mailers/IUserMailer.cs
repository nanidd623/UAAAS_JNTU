using Mvc.Mailer;
using System.Collections.Generic;

namespace UAAAS.Models
{
    public interface IUserMailer
    {
        MvcMailMessage Welcome(string Email, string View, string Subject, string Username);
        MvcMailMessage Welcome(string Email, string View, string Subject, string Username, string Passsword, string encUsername, string encPasssword);
        MvcMailMessage FacultyOnlineRegistration(string Email, string View, string Subject, string Username, string Password, string FacultyRegistrationNumber, string UniqueID, string facultyType, string PANNumber);
        MvcMailMessage GoodBye();
        MvcMailMessage AffiliationRequestByUser(string email);
        MvcMailMessage SendOrder(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments);
        MvcMailMessage SendOrderToPrincipal(string to, string cc, string bcc, string subject, string InspectionDate);
        MvcMailMessage SendEditDates(string To, string Cc, string Subject, string Username, string StartDate, string EndDate);

        MvcMailMessage SendAttachmentToAllColleges(string to, string cc, string bcc, string subject, string Message, List<System.Net.Mail.Attachment> attachments);
        MvcMailMessage SendMailToCollege(string To, string Cc, string bcc, string Subject, string Username, string Message, List<System.Net.Mail.Attachment> attachments);

        MvcMailMessage SendDeficiencyLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments);
        MvcMailMessage SendCommonDeficiencyLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments);
        MvcMailMessage SendProformaNotSubmittedLetter(string to, string cc, string bcc, string subject, List<System.Net.Mail.Attachment> attachments);
        MvcMailMessage ClusterEmails(string to, string cc, string bcc, string subject, string message);

        MvcMailMessage PaymentResponse(string Email,string View, string collegecode, string customerid, string txnrefno, string bankrefno, decimal txnamount, string txndate, string description, string challanno, string message, string subject);

        MvcMailMessage FacultyNotApprovedStatus(string Email, string view, string regno, string status,string subject);

        MvcMailMessage FacultyMobilityRequestMail(string Email, string View, string Subject, string TicketId, string Name, string RegistrationNumber ,string CollegeName);

        MvcMailMessage FacultyComplaintRequestMail(string Email, string View, string Subject, string TicketId, string Name, string RegistrationNumber, string CompalintDate);
    }
}