using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

public partial class AdminLogin : Page
{
    // Bound into the markup via <%= %> to redraw the form with feedback,
    // since there's no server-side ViewState/control tree driving this page.
    public string ErrorMessage { get; private set; }
    public string PostedUsername { get; private set; } = string.Empty;
    public bool UsernameError { get; private set; }
    public bool PasswordError { get; private set; }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Already logged in? Skip straight to the dashboard.
        if (Session["AdminID"] != null)
        {
            Response.Redirect("AdminDashboard.aspx");
            return;
        }

        // The login form submits a real HTTP GET with action=login. Anything else
        // (a plain visit to the page) just renders the empty form.
        if (Request.HttpMethod == "GET" && Request.QueryString["action"] == "login")
        {
            AttemptLogin();
        }
    }

    private void AttemptLogin()
    {
        string username = (Request.QueryString["username"] ?? string.Empty).Trim();
        string password = Request.QueryString["password"] ?? string.Empty;

        PostedUsername = username;

        // ---- Server-side validation BEFORE authentication ----
        UsernameError = string.IsNullOrEmpty(username);
        PasswordError = string.IsNullOrEmpty(password);

        if (UsernameError || PasswordError)
        {
            ErrorMessage = "Please fill in both Username and Password.";
            return;
        }

        bool authenticated = false;

        // ---- Exception handling: a DB/hashing failure must never crash the page ----
        try
        {
            string hashedPassword = AdminAuthHelper.HashPassword(password);

            DataTable dt = DBHelper.ExecuteQuery(
                "SELECT AdminID, Username, FullName FROM Admins WHERE Username = @Username AND PasswordHash = @PasswordHash",
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", hashedPassword));

            if (dt.Rows.Count == 0)
            {
                ErrorMessage = "Invalid username or password.";
            }
            else
            {
                DataRow row = dt.Rows[0];
                Session["AdminID"] = row["AdminID"];
                Session["AdminUsername"] = row["Username"].ToString();
                Session["AdminFullName"] = row["FullName"].ToString();
                authenticated = true;
            }
        }
        catch (SqlException)
        {
            // DB unreachable / query failed -- fail gracefully, no stack trace to the user.
            ErrorMessage = "We couldn't reach the server right now. Please try again in a moment.";
        }
        catch (Exception)
        {
            // Catch-all safety net so nothing unexpected ever surfaces to the browser.
            ErrorMessage = "Something went wrong while logging you in. Please try again.";
        }

        // Redirect happens outside the try/catch: Response.Redirect internally raises
        // a ThreadAbortException, which a broad catch(Exception) above would otherwise
        // swallow and turn into a false "something went wrong" error message.
        if (authenticated)
        {
            Response.Redirect("AdminDashboard.aspx");
        }
    }
}
