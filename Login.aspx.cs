using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;

public partial class Login : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Already logged in? Skip straight to dashboard.
        if (!IsPostBack && Session["StudentID"] != null)
        {
            Response.Redirect("Dashboard.aspx");
        }
    }

    protected void lnkRefreshCaptcha_Click(object sender, EventArgs e)
    {
        // Simply re-render; the <asp:Image> ImageUrl points at the handler,
        // which generates a brand-new code and image on every request.
        imgCaptcha.ImageUrl = "~/CaptchaHandler.ashx?ts=" + DateTime.Now.Ticks;
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string email = txtEmail.Text.Trim();
        string enteredPassword = txtPassword.Text.Trim();
        string enteredCaptcha = txtCaptcha.Text.Trim();

        // ---- CAPTCHA check first ----
        string expectedCaptcha = Session["CaptchaCode"] as string;
        if (string.IsNullOrEmpty(expectedCaptcha) ||
            !string.Equals(expectedCaptcha, enteredCaptcha, StringComparison.Ordinal))
        {
            ShowStatus("Incorrect CAPTCHA code. Please try again.", false);
            RefreshCaptchaImage();
            return;
        }

        // CAPTCHA is single-use -- clear it so it can't be replayed.
        Session["CaptchaCode"] = null;

        bool readyToRedirect = false;

        // ---- Exception handling: DB/network failures must never crash the page ----
        try
        {
            // ---- Look up the student ----
            DataTable dt = DBHelper.ExecuteQuery(
                @"SELECT StudentID, FullName, Email, MobileNumber, LastLoginDate,
                         ApprovalStatus, AccountStatus, RejectionRemark
                  FROM Students WHERE Email = @Email",
                new SqlParameter("@Email", email));

            if (dt.Rows.Count == 0)
            {
                ShowStatus("No account found with that email address.", false);
                RefreshCaptchaImage();
                return;
            }

            DataRow row = dt.Rows[0];
            string storedMobile = row["MobileNumber"].ToString();

            // Stored mobile numbers may include a country code (e.g. +919876543210).
            // Compare against just the last 10 digits, matching what the student types.
            string storedDigitsOnly = Regex.Replace(storedMobile, "[^0-9]", "");
            string last10 = storedDigitsOnly.Length >= 10
                ? storedDigitsOnly.Substring(storedDigitsOnly.Length - 10)
                : storedDigitsOnly;

            if (!string.Equals(last10, enteredPassword, StringComparison.Ordinal))
            {
                ShowStatus("Incorrect password.", false);
                RefreshCaptchaImage();
                return;
            }

            // ---- Task 7: approval / account status gates ----
            // Credentials are correct at this point -- now check whether this
            // student is actually allowed to log in yet.
            string approvalStatus = row["ApprovalStatus"].ToString();
            string accountStatus = row["AccountStatus"].ToString();

            if (approvalStatus == "Pending")
            {
                ShowStatus("Your registration is still pending admin approval. Please check back later.", false);
                RefreshCaptchaImage();
                return;
            }

            if (approvalStatus == "Rejected")
            {
                string remark = row["RejectionRemark"] == DBNull.Value ? "" : row["RejectionRemark"].ToString();
                ShowStatus("Your registration was rejected." + (string.IsNullOrEmpty(remark) ? "" : " Reason: " + remark), false);
                RefreshCaptchaImage();
                return;
            }

            if (accountStatus == "Inactive")
            {
                ShowStatus("Your account has been deactivated. Please contact the administrator.", false);
                RefreshCaptchaImage();
                return;
            }

            // ---- Success: start the session ----
            string studentId = row["StudentID"].ToString();

            Session["StudentID"] = studentId;
            Session["StudentEmail"] = row["Email"].ToString();
            Session["StudentName"] = row["FullName"].ToString();

            // Carry the PREVIOUS LastLoginDate forward for display on the dashboard
            // ("Last Login" should show the prior session, not the one just starting).
            bool hadPriorLogin = row["LastLoginDate"] != DBNull.Value;
            Session["HasPriorLogin"] = hadPriorLogin;
            if (hadPriorLogin)
            {
                Session["PreviousLastLogin"] = row["LastLoginDate"];
            }

            // Now stamp this login as the new LastLoginDate for next time.
            DBHelper.ExecuteNonQuery(
                "UPDATE Students SET LastLoginDate = @Now WHERE StudentID = @StudentID",
                new SqlParameter("@Now", DateTime.Now),
                new SqlParameter("@StudentID", studentId));

            readyToRedirect = true;
        }
        catch (SqlException)
        {
            // DB unreachable / query failed -- fail gracefully, no stack trace to the user.
            ShowStatus("We couldn't reach the server right now. Please try again in a moment.", false);
            RefreshCaptchaImage();
        }
        catch (Exception)
        {
            // Catch-all safety net so nothing unexpected ever surfaces to the browser.
            ShowStatus("Something went wrong while logging you in. Please try again.", false);
            RefreshCaptchaImage();
        }

        // Redirect happens outside the try/catch: Response.Redirect internally raises
        // a ThreadAbortException, which a broad catch(Exception) above would otherwise
        // swallow and turn into a false "something went wrong" error message.
        if (readyToRedirect)
        {
            Response.Redirect("Dashboard.aspx");
        }
    }

    private void RefreshCaptchaImage()
    {
        imgCaptcha.ImageUrl = "~/CaptchaHandler.ashx?ts=" + DateTime.Now.Ticks;
    }

    private void ShowStatus(string message, bool success)
    {
        string icon = success ? "✅ " : "⚠️ ";
        lblLoginStatus.Text = icon + message;
        lblLoginStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblLoginStatus.Visible = true;
    }
}
