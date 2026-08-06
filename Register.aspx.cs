using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Net.Http;
using System.Web.Script.Serialization;

public partial class Register : Page
{
    // Session keys
    private const string SESSION_EMAIL_VERIFIED = "EmailVerified";
    private const string SESSION_VERIFIED_EMAIL = "VerifiedEmailAddress";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindCountryDropdown();
            ResetVerificationState();
        }
        txtDob.Attributes["max"] = DateTime.Today.ToString("yyyy-MM-dd");
    }

    private void ResetVerificationState()
    {
        Session[SESSION_EMAIL_VERIFIED] = false;
        Session[SESSION_VERIFIED_EMAIL] = "";
        btnRegister.Enabled = false;
    }

    private void BindCountryDropdown()
    {
        DataTable dt = DBHelper.ExecuteQuery("SELECT CountryID, CountryName FROM Countries ORDER BY CountryName");
        ddlCountry.DataTextField = "CountryName";
        ddlCountry.DataValueField = "CountryID";
        ddlCountry.DataSource = dt;
        ddlCountry.DataBind();
        ddlCountry.Items.Insert(0, new ListItem("Select Country", ""));
    }

    /* =========================================================
       WebMethods used by cascading-dropdown.js via PageMethods.
       Must be public static and decorated with [WebMethod].
       ========================================================= */
    private static readonly HttpClient httpClient = new HttpClient();

    [WebMethod]
    public static List<DropdownItem> GetStates(string countryName)
    {
        var list = new List<DropdownItem>();
        if (string.IsNullOrWhiteSpace(countryName)) return list;

        try
        {
            var payload = new JavaScriptSerializer().Serialize(new { country = countryName });
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/states", content).Result;
            string json = response.Content.ReadAsStringAsync().Result;

            var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            if (result.ContainsKey("data") && result["data"] != null)
            {
                var data = (Dictionary<string, object>)result["data"];
                var states = (System.Collections.ArrayList)data["states"];
                foreach (Dictionary<string, object> s in states)
                {
                    string name = s["name"].ToString();
                    list.Add(new DropdownItem { Id = name, Name = name });
                }
            }
        }
        catch
        {
            //list.Add(new DropdownItem { Id = "error", Name = "ERROR: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message) });
        }

        return list;
    }

    [WebMethod]
    public static List<DropdownItem> GetDistricts(string countryName, string stateName)
    {
        var list = new List<DropdownItem>();
        if (string.IsNullOrWhiteSpace(countryName) || string.IsNullOrWhiteSpace(stateName)) return list;

        try
        {
            var payload = new JavaScriptSerializer().Serialize(new { country = countryName, state = stateName });
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync("https://countriesnow.space/api/v0.1/countries/state/cities", content).Result;
            string json = response.Content.ReadAsStringAsync().Result;

            var result = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            if (result.ContainsKey("data") && result["data"] != null)
            {
                var cities = (System.Collections.ArrayList)result["data"];
                foreach (var c in cities)
                {
                    string name = c.ToString();
                    list.Add(new DropdownItem { Id = name, Name = name });
                }
            }
        }
        catch { }

        return list;
    }

    /* =========================================================
       Task III -- Client-side duplicate-email check (live, on blur).
       Called via PageMethods from Register.aspx before the user even
       requests an OTP, so they get instant feedback.
       ========================================================= */
    [WebMethod]
    public static bool CheckEmailExists(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        object result = DBHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Students WHERE Email = @Email",
            new SqlParameter("@Email", email.Trim()));

        return Convert.ToInt32(result) > 0;
    }

    /* =========================================================
       OTP Flow
       ========================================================= */
    protected void btnSendOtp_Click(object sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim();

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
        {
            ShowOtpStatus("Please enter a valid email address before requesting an OTP.", false);
            return;
        }

        // Server-side guard as well, in case the client-side check was bypassed
        // (JS disabled, direct postback, etc.) -- don't even send an OTP for a
        // duplicate email.
        if (CheckEmailExists(email))
        {
            ShowOtpStatus("A student is already registered with this Email Address.", false);
            return;
        }

        string otp = OtpHelper.GenerateAndStoreOtp(email);

        try
        {
            EmailHelper.SendOtpEmail(email, txtFullName.Text.Trim(), otp);
            ShowOtpStatus("An OTP has been sent to " + email + ". It is valid for 5 minutes.", true);
            btnResendOtp.Enabled = true;
            btnSendOtp.Enabled = false;
        }
        catch (Exception ex)
        {
            ShowOtpStatus("Failed to send OTP email. Please check the SMTP configuration. (" + ex.Message + ")", false);
        }
    }

    protected void btnResendOtp_Click(object sender, EventArgs e)
    {
        btnSendOtp.Enabled = true;
        btnSendOtp_Click(sender, e);
    }

    protected void btnVerifyOtp_Click(object sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim();
        string enteredOtp = txtOtp.Text.Trim();

        if (string.IsNullOrWhiteSpace(enteredOtp))
        {
            ShowOtpStatus("Please enter the OTP sent to your email.", false);
            return;
        }

        bool isValid = OtpHelper.VerifyOtp(email, enteredOtp);

        if (isValid)
        {
            Session[SESSION_EMAIL_VERIFIED] = true;
            Session[SESSION_VERIFIED_EMAIL] = email;
            btnRegister.Enabled = true;
            ShowOtpStatus("Email verified successfully. You may now complete your registration.", true);
        }
        else
        {
            ShowOtpStatus("Invalid or expired OTP. Please try again or click Resend OTP.", false);
        }
    }

    private void ShowOtpStatus(string message, bool success)
    {
        lblOtpStatus.Text = message;
        lblOtpStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblOtpStatus.Visible = true;
    }

    /* =========================================================
       Final Registration Submit
       ========================================================= */
    protected void btnRegister_Click(object sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim();

        // Guard: email must have been verified via OTP in this session.
        bool verified = Session[SESSION_EMAIL_VERIFIED] != null && (bool)Session[SESSION_EMAIL_VERIFIED];
        string verifiedEmail = Session[SESSION_VERIFIED_EMAIL] as string;

        if (!verified || verifiedEmail != email)
        {
            ShowRegisterStatus("Please verify your email with OTP before submitting the form.", false);
            return;
        }

        if (!Page.IsValid) return;

        if (ddlState.SelectedValue == "" || ddlDistrict.SelectedValue == "" || ddlCountry.SelectedValue == "")
        {
            ShowRegisterStatus("Please select Country, State and District.", false);
            return;
        }

        string mobileToCheck = string.IsNullOrWhiteSpace(hdnFullMobile.Value) ? "" : hdnFullMobile.Value;

        // ---- Task III: Prevent Duplicate Registration ----
        // Checked as two separate queries so each gives its own precise message,
        // per spec ("A student is already registered with this Email Address.").
        // This re-checks Email even though CheckEmailExists ran earlier client-side
        // and again in btnSendOtp_Click -- someone else could have registered that
        // exact email in the few minutes since this student verified their OTP.
        object emailExists = DBHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Students WHERE Email = @Email",
            new SqlParameter("@Email", email));

        if (Convert.ToInt32(emailExists) > 0)
        {
            ShowRegisterStatus("A student is already registered with this Email Address.", false);
            return;
        }

        object mobileExists = DBHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Students WHERE MobileNumber = @Mobile",
            new SqlParameter("@Mobile", mobileToCheck));

        if (Convert.ToInt32(mobileExists) > 0)
        {
            ShowRegisterStatus("A student is already registered with this Mobile Number.", false);
            return;
        }

        // ---- Handle profile photo upload ----
        string photoPath = "~/Uploads/Students/default-avatar.png";
        if (fuProfilePhoto.HasFile)
        {
            string ext = Path.GetExtension(fuProfilePhoto.FileName).ToLower();
            string[] allowed = { ".jpg", ".jpeg", ".png" };

            if (Array.IndexOf(allowed, ext) < 0)
            {
                lblPhotoError.Text = "Only JPG, JPEG and PNG files are allowed.";
                return;
            }

            double maxSizeMb = double.Parse(ConfigurationManager.AppSettings["MaxUploadSizeMB"]);
            if (fuProfilePhoto.PostedFile.ContentLength > maxSizeMb * 1024 * 1024)
            {
                lblPhotoError.Text = "Photo must be smaller than " + maxSizeMb + " MB.";
                return;
            }

            string uploadFolder = ConfigurationManager.AppSettings["ProfilePhotoUploadPath"];
            string physicalFolder = Server.MapPath(uploadFolder);
            if (!Directory.Exists(physicalFolder)) Directory.CreateDirectory(physicalFolder);

            string uniqueFileName = "STU_" + DateTime.Now.Ticks + ext;
            string physicalPath = Path.Combine(physicalFolder, uniqueFileName);
            fuProfilePhoto.SaveAs(physicalPath);

            photoPath = uploadFolder.TrimEnd('/') + "/" + uniqueFileName;
        }

        // ---- Generate Student ID and insert record ----
        string studentId = DBHelper.GetNextStudentId();
        string fullMobile = string.IsNullOrWhiteSpace(hdnFullMobile.Value) ? "" : hdnFullMobile.Value;
        DateTime registrationTime = DateTime.Now;

        DBHelper.ExecuteNonQuery(
    @"INSERT INTO Students
        (StudentID, FullName, Email, MobileNumber, CountryID, StateName, DistrictName,
         Address, Gender, DateOfBirth, ProfilePhotoPath, Course, Semester,
         RegistrationDate, IsEmailVerified, ApprovalStatus, AccountStatus,
         CreatedDate, LastModifiedDate)
      VALUES
        (@StudentID, @FullName, @Email, @Mobile, @CountryID, @StateName, @DistrictName,
         @Address, @Gender, @Dob, @Photo, @Course, @Semester, @RegDate, 1,
         'Pending', 'Active', @RegDate, @RegDate)",
    new SqlParameter("@StudentID", studentId),
    new SqlParameter("@FullName", txtFullName.Text.Trim()),
    new SqlParameter("@Email", email),
    new SqlParameter("@Mobile", fullMobile),
    new SqlParameter("@CountryID", int.Parse(ddlCountry.SelectedValue)),
    new SqlParameter("@StateName", ddlState.SelectedItem.Text),
    new SqlParameter("@DistrictName", ddlDistrict.SelectedItem.Text),
    new SqlParameter("@Address", (object)txtAddress.Text.Trim() ?? DBNull.Value),
    new SqlParameter("@Gender", rblGender.SelectedValue),
    new SqlParameter("@Dob", DateTime.Parse(txtDob.Text)),
    new SqlParameter("@Photo", photoPath),
    new SqlParameter("@Course", ddlCourse.SelectedValue),
    new SqlParameter("@Semester", ddlSemester.SelectedValue),
    new SqlParameter("@RegDate", registrationTime));

        // ---- Confirm to the student ----
        // Independent try/catch from the admin notification below: either one
        // failing shouldn't stop the other from being attempted, and neither
        // should ever block the registration that has already been saved.
        try
        {
            EmailHelper.SendRegistrationConfirmation(
                email, txtFullName.Text.Trim(), studentId, fullMobile,
                rblGender.SelectedValue, DateTime.Parse(txtDob.Text), txtAddress.Text.Trim(),
                ddlCountry.SelectedItem.Text, ddlState.SelectedItem.Text, ddlDistrict.SelectedItem.Text,
                ddlCourse.SelectedValue, ddlSemester.SelectedValue,
                photoPath, registrationTime);
        }
        catch
        {
            // Registration should still succeed even if the confirmation email fails.
        }

        // ---- Notify admin ----
        try
        {
            EmailHelper.SendAdminNotification(
                studentId, txtFullName.Text.Trim(), email, fullMobile,
                ddlCountry.SelectedItem.Text, ddlState.SelectedItem.Text, ddlDistrict.SelectedItem.Text,
                registrationTime);
        }
        catch
        {
            // Registration should still succeed even if the admin notification email fails.
        }

        ShowRegisterStatus("Registration successful! Your Student ID is " + studentId +
            ". Your application is pending admin approval -- you'll receive an email once it's reviewed, and you can then log in.", true);
        ClearFormAfterSuccess();
    }

    private void ShowRegisterStatus(string message, bool success)
    {
        lblRegisterStatus.Text = message;
        lblRegisterStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblRegisterStatus.Visible = true;
    }

    private void ClearFormAfterSuccess()
    {
        txtFullName.Text = "";
        txtEmail.Text = "";
        txtOtp.Text = "";
        txtAddress.Text = "";
        txtDob.Text = "";
        ddlState.Items.Clear();
        ddlState.Items.Add(new ListItem("Select State", ""));
        ddlDistrict.Items.Clear();
        ddlDistrict.Items.Add(new ListItem("Select District", ""));
        ResetVerificationState();
        btnSendOtp.Enabled = true;
        btnResendOtp.Enabled = false;
    }

    protected void cvDob_ServerValidate(object source, ServerValidateEventArgs args)
    {
        DateTime dob;
        if (!DateTime.TryParse(args.Value, out dob))
        {
            args.IsValid = false;
            return;
        }
        args.IsValid = dob <= DateTime.Today && dob >= DateTime.Today.AddYears(-100);
    }

    protected void cvMobile_ServerValidate(object source, ServerValidateEventArgs args)
    {
        args.IsValid = !string.IsNullOrWhiteSpace(hdnFullMobile.Value);
    }

    private bool IsValidEmail(string email)
    {
        try { return new System.Net.Mail.MailAddress(email).Address == email; }
        catch { return false; }
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            RebindDynamicDropdownsForPostback();
        }
    }

    /// <summary>
    /// ddlState/ddlDistrict are populated client-side via PageMethods and never
    /// exist in the server's Items collection. On postback, ASP.NET matches the
    /// submitted value against Items BEFORE Page_Load runs, so without this the
    /// match always fails and SelectedValue comes back empty. This rebuilds the
    /// same options server-side (using the posted Country/State) early enough
    /// for that match to succeed.
    /// </summary>
    private void RebindDynamicDropdownsForPostback()
    {
        string postedCountryId = Request.Form["ddlCountry"];
        string postedStateName = Request.Form["ddlState"];

        int countryId;
        if (!string.IsNullOrEmpty(postedCountryId) && int.TryParse(postedCountryId, out countryId))
        {
            string countryName = DBHelper.ExecuteScalar(
                "SELECT CountryName FROM Countries WHERE CountryID = @CountryId",
                new SqlParameter("@CountryId", countryId))?.ToString();

            if (!string.IsNullOrEmpty(countryName))
            {
                var states = GetStates(countryName);

                ddlState.Items.Clear();
                ddlState.Items.Add(new ListItem("Select State", ""));
                foreach (var s in states)
                {
                    ddlState.Items.Add(new ListItem(s.Name, s.Id));
                }
                ddlState.Enabled = true;

                if (!string.IsNullOrEmpty(postedStateName))
                {
                    var districts = GetDistricts(countryName, postedStateName);

                    ddlDistrict.Items.Clear();
                    ddlDistrict.Items.Add(new ListItem("Select District", ""));
                    foreach (var d in districts)
                    {
                        ddlDistrict.Items.Add(new ListItem(d.Name, d.Id));
                    }
                    ddlDistrict.Enabled = true;
                }
            }
        }
    }
}

/// <summary>Simple DTO used to serialize dropdown options to JSON for PageMethods.</summary>
public class DropdownItem
{
    public string Id { get; set; }
    public string Name { get; set; }
}
