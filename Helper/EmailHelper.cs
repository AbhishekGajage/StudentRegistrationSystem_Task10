using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web;

/// <summary>
/// Sends templated HTML emails: registration confirmation and OTP verification
/// to the student, new-registration notifications to the admin, and application
/// approval/rejection notifications to the student.
///
/// Every send goes through SendMail(), which validates inputs up front and logs
/// every attempt (success or failure) to ~/App_Data/EmailLog.txt so delivery
/// problems can be diagnosed without needing a debugger attached. Exceptions are
/// logged then re-thrown -- callers keep deciding for themselves (as they already
/// did) whether an email failure should surface to the user or just be swallowed.
/// </summary>
public static class EmailHelper
{
    private const string InstituteName = "new institude";
    private static readonly object LogLock = new object();

    private static SendGridClient BuildSendGridClient()
    {
        string apiKey = ConfigurationManager.AppSettings["SendGridApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ConfigurationErrorsException(
                "SendGrid is not configured: SendGridApiKey is missing from Web.config's <appSettings>.");
        }

        return new SendGridClient(apiKey);
    }

    private static string LoadTemplate(string fileName)
    {
        string path = HttpContext.Current.Server.MapPath("~/EmailTemplates/" + fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "Email template '" + fileName + "' was not found at " + path + ".", fileName);
        }

        return File.ReadAllText(path);
    }

    /// <summary>Sends the "we received your registration" confirmation to the student --
    /// distinct from the OTP email sent earlier in the flow, and sent once the student
    /// record has actually been saved. Includes every field the student submitted, plus
    /// their profile photo embedded directly in the email (as an inline attachment, not
    /// a hosted-image link -- a link back to localhost would be dead for the recipient).</summary>
    public static void SendRegistrationConfirmation(string toEmail, string studentName, string studentId,
        string mobile, string gender, DateTime dob, string address,
        string country, string state, string district, string course, string semester,
        string photoVirtualPath, DateTime registeredAt)
    {
        string body = LoadTemplate("RegistrationConfirmationTemplate.html");
        body = body.Replace("{{InstituteName}}", InstituteName)
                    .Replace("{{StudentName}}", string.IsNullOrWhiteSpace(studentName) ? "Student" : studentName)
                    .Replace("{{StudentID}}", studentId)
                    .Replace("{{Email}}", toEmail)
                    .Replace("{{Mobile}}", mobile)
                    .Replace("{{Gender}}", string.IsNullOrWhiteSpace(gender) ? "-" : gender)
                    .Replace("{{DateOfBirth}}", dob.ToString("dd-MMM-yyyy"))
                    .Replace("{{Address}}", HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(address) ? "-" : address))
                    .Replace("{{Country}}", country)
                    .Replace("{{State}}", state)
                    .Replace("{{District}}", district)
                    .Replace("{{Course}}", string.IsNullOrWhiteSpace(course) ? "-" : course)
                    .Replace("{{Semester}}", string.IsNullOrWhiteSpace(semester) ? "-" : semester)
                    .Replace("{{RegistrationDateTime}}", registeredAt.ToString("dd-MMM-yyyy hh:mm tt"))
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

        // Resolve the virtual path (e.g. "~/Uploads/Students/STU_123.jpg") to a physical
        // path so it can be attached. If this fails for any reason, fall back to sending
        // without the embedded photo rather than losing the whole email over it.
        string physicalPhotoPath = null;
        if (!string.IsNullOrWhiteSpace(photoVirtualPath))
        {
            try
            {
                physicalPhotoPath = HttpContext.Current.Server.MapPath(photoVirtualPath);
            }
            catch
            {
                physicalPhotoPath = null;
            }
        }

        SendMail(toEmail, "Registration Received - " + InstituteName, body, physicalPhotoPath, "profilePhoto");
    }

    /// <summary>Sends the OTP verification email to the student.</summary>
    public static void SendOtpEmail(string toEmail, string studentName, string otpCode)
    {
        string body = LoadTemplate("OtpEmailTemplate.html");
        body = body.Replace("{{InstituteName}}", InstituteName)
                    .Replace("{{StudentName}}", string.IsNullOrWhiteSpace(studentName) ? "Student" : studentName)
                    .Replace("{{OtpCode}}", otpCode)
                    .Replace("{{ExpiryMinutes}}", ConfigurationManager.AppSettings["OtpExpiryMinutes"])
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

        SendMail(toEmail, "Your OTP Code for Student Registration", body);
    }

    /// <summary>Sends a "new student registered" notification to the admin.</summary>
    public static void SendAdminNotification(string studentId, string studentName, string email,
        string mobile, string country, string state, string district, DateTime registeredAt)
    {
        string body = LoadTemplate("AdminNotificationTemplate.html");
        body = body.Replace("{{InstituteName}}", InstituteName)
                    .Replace("{{StudentID}}", studentId)
                    .Replace("{{StudentName}}", studentName)
                    .Replace("{{Email}}", email)
                    .Replace("{{Mobile}}", mobile)
                    .Replace("{{Country}}", country)
                    .Replace("{{State}}", state)
                    .Replace("{{District}}", district)
                    .Replace("{{RegistrationDateTime}}", registeredAt.ToString("dd-MMM-yyyy hh:mm tt"));

        string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];
        SendMail(adminEmail, "New Student Registration: " + studentName, body);
    }

    /// <summary>Notifies the student their application was approved.</summary>
    public static void SendApprovalEmail(string toEmail, string studentName, string studentId)
    {
        string body = LoadTemplate("ApprovalEmailTemplate.html");
        body = body.Replace("{{InstituteName}}", InstituteName)
                    .Replace("{{StudentName}}", string.IsNullOrWhiteSpace(studentName) ? "Student" : studentName)
                    .Replace("{{StudentID}}", studentId)
                    .Replace("{{ApprovalDateTime}}", DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"))
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

        SendMail(toEmail, "Your Registration Has Been Approved", body);
    }

    /// <summary>Notifies the student their application was rejected, with the remark.</summary>
    public static void SendRejectionEmail(string toEmail, string studentName, string studentId, string remark)
    {
        string body = LoadTemplate("RejectionEmailTemplate.html");
        body = body.Replace("{{InstituteName}}", InstituteName)
                    .Replace("{{StudentName}}", string.IsNullOrWhiteSpace(studentName) ? "Student" : studentName)
                    .Replace("{{StudentID}}", studentId)
                    .Replace("{{RejectionRemark}}", HttpUtility.HtmlEncode(remark))
                    .Replace("{{RejectionDateTime}}", DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"))
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

        SendMail(toEmail, "Update on Your Registration Application", body);
    }

    /// <summary>Fails fast with a clear message if required inputs are missing/malformed,
    /// instead of letting a cryptic exception bubble up from deep inside System.Net.Mail.</summary>
    private static void ValidateEmailInputs(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email address is required.");
        }

        try
        {
            var check = new MailAddress(toEmail);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Recipient email address '" + toEmail + "' is not a valid email format.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Email subject is required.");
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new ArgumentException("Email body is required.");
        }
    }

    private static void SendMail(string toEmail, string subject, string htmlBody,
        string embeddedImagePath = null, string embeddedImageContentId = null)
    {
        ValidateEmailInputs(toEmail, subject, htmlBody);

        try
        {
            var from = new EmailAddress(
                ConfigurationManager.AppSettings["FromEmail"],
                ConfigurationManager.AppSettings["FromDisplayName"]);
            var to = new EmailAddress(toEmail);

            // Minimal plain-text fallback alongside the HTML part -- some spam filters
            // penalize HTML-only emails, and it's the courteous thing to include anyway.
            string plainTextFallback = "This email contains HTML content. Please view it in an email client that supports HTML.";

            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextFallback, htmlBody);

            bool hasEmbeddedImage = !string.IsNullOrWhiteSpace(embeddedImagePath) && File.Exists(embeddedImagePath);
            if (hasEmbeddedImage)
            {
                string ext = Path.GetExtension(embeddedImagePath).ToLowerInvariant();
                string contentType = ext == ".png" ? "image/png" : "image/jpeg";
                byte[] photoBytes = File.ReadAllBytes(embeddedImagePath);

                msg.AddAttachment(new SendGrid.Helpers.Mail.Attachment
                {
                    Content = Convert.ToBase64String(photoBytes),
                    Filename = Path.GetFileName(embeddedImagePath),
                    Type = contentType,
                    Disposition = "inline",
                    ContentId = embeddedImageContentId
                });
            }
            // No image to embed, or the file is missing on disk -- send the HTML as-is.
            // The template's <img src="cid:..."> simply won't resolve, which beats
            // failing the whole email over a missing photo file.

            SendGridClient client = BuildSendGridClient();

            // SendGrid's SDK is async-only; these callers are classic synchronous
            // ASP.NET event handlers, so this runs it to completion synchronously.
            // ConfigureAwait(false) avoids re-entering ASP.NET's request context on
            // the continuation, which is what would otherwise risk a deadlock here.
            Response response = client.SendEmailAsync(msg).ConfigureAwait(false).GetAwaiter().GetResult();

            int statusCode = (int)response.StatusCode;
            if (statusCode < 200 || statusCode >= 300)
            {
                string responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                throw new Exception("SendGrid API returned HTTP " + statusCode + ": " + responseBody);
            }

            LogEmailAttempt(toEmail, subject, true, null);
        }
        catch (Exception ex)
        {
            LogEmailAttempt(toEmail, subject, false, ex.Message);
            throw; // preserve existing behavior -- callers decide whether to surface or swallow this
        }
    }

    /// <summary>Appends one line per send attempt to ~/App_Data/EmailLog.txt.
    /// Logging failures must never be the reason an email operation crashes the app,
    /// so this method swallows its own exceptions rather than propagating them.</summary>
    private static void LogEmailAttempt(string toEmail, string subject, bool success, string errorMessage)
    {
        try
        {
            string logPath = HttpContext.Current.Server.MapPath("~/App_Data/EmailLog.txt");
            string logDir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            string line = string.Format(
                "{0:yyyy-MM-dd HH:mm:ss}\t{1}\t{2}\t{3}\t{4}",
                DateTime.Now,
                success ? "SUCCESS" : "FAILED",
                toEmail,
                subject,
                success ? "" : (errorMessage ?? "").Replace("\r", " ").Replace("\n", " "));

            lock (LogLock)
            {
                File.AppendAllText(logPath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Never let a logging failure mask or replace the original send result.
        }
    }
}
