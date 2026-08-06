using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Dashboard : Page
{
    private static readonly HttpClient httpClient = new HttpClient();

    protected void Page_Init(object sender, EventArgs e)
    {
        if (IsPostBack)
        {
            RebindDynamicDropdownsForPostback();
        }
    }

    /// <summary>
    /// ddlEditState/ddlEditDistrict are populated client-side via PageMethods and
    /// never exist in the server's Items collection. On postback, ASP.NET matches
    /// the submitted value against Items BEFORE Page_Load runs, so without this the
    /// match always fails and SelectedValue comes back empty. This rebuilds the same
    /// options server-side (using the posted Country/State) early enough for that
    /// match to succeed -- same fix as Register.aspx.cs.
    /// </summary>
    private void RebindDynamicDropdownsForPostback()
    {
        string postedCountryName = Request.Form["ddlEditCountry"];
        string postedStateName = Request.Form["ddlEditState"];

        if (string.IsNullOrEmpty(postedCountryName)) return;

        var states = FetchStatesFromApi(postedCountryName);

        ddlEditState.Items.Clear();
        ddlEditState.Items.Add(new ListItem("Select State", ""));
        foreach (var s in states)
        {
            ddlEditState.Items.Add(new ListItem(s.Name, s.Id));
        }

        if (!string.IsNullOrEmpty(postedStateName))
        {
            var districts = FetchDistrictsFromApi(postedCountryName, postedStateName);

            ddlEditDistrict.Items.Clear();
            ddlEditDistrict.Items.Add(new ListItem("Select District", ""));
            foreach (var d in districts)
            {
                ddlEditDistrict.Items.Add(new ListItem(d.Name, d.Id));
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent direct access without logging in.
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            BindEditCountryDropdown();
            LoadProfile();
        }
    }

    /* =========================================================
       Load + display profile (view mode)
       ========================================================= */
    private void LoadProfile()
    {
        string studentId = Session["StudentID"].ToString();

        DataTable dt = DBHelper.ExecuteQuery(
            @"SELECT StudentID, FullName, Email, MobileNumber, CountryID, StateName, DistrictName,
                     Address, Gender, DateOfBirth, ProfilePhotoPath, RegistrationDate, LastLoginDate
              FROM Students WHERE StudentID = @StudentID",
            new SqlParameter("@StudentID", studentId));

        if (dt.Rows.Count == 0)
        {
            // Account no longer exists -- force re-login.
            Session.Clear();
            Response.Redirect("Login.aspx");
            return;
        }

        DataRow row = dt.Rows[0];

        string fullName = row["FullName"].ToString();
        string countryName = GetCountryNameById(row["CountryID"]);
        string stateName = row["StateName"] == DBNull.Value ? "" : row["StateName"].ToString();
        string districtName = row["DistrictName"] == DBNull.Value ? "" : row["DistrictName"].ToString();
        string photoPath = row["ProfilePhotoPath"] == DBNull.Value ? "~/Uploads/Students/default-avatar.png" : row["ProfilePhotoPath"].ToString();
        string dob = row["DateOfBirth"] == DBNull.Value ? "" : Convert.ToDateTime(row["DateOfBirth"]).ToString("dd MMM yyyy");
        string regDate = row["RegistrationDate"] == DBNull.Value ? "-" : Convert.ToDateTime(row["RegistrationDate"]).ToString("dd MMM yyyy, hh:mm tt");

        // Login.aspx already overwrote Students.LastLoginDate to "now" before redirecting here,
        // so prefer the PREVIOUS value it stashed in Session for this display over the DB column.
        string lastLogin;
        if (Session["HasPriorLogin"] != null)
        {
            // Fresh from Login.aspx this request cycle -- trust the flag it set.
            lastLogin = (bool)Session["HasPriorLogin"]
                ? Convert.ToDateTime(Session["PreviousLastLogin"]).ToString("dd MMM yyyy, hh:mm tt")
                : "This is your first login";
        }
        else if (row["LastLoginDate"] != DBNull.Value)
        {
            // Dashboard loaded outside the login flow (e.g. browser refresh later) --
            // fall back to the DB value, which reflects this session's login time.
            lastLogin = Convert.ToDateTime(row["LastLoginDate"]).ToString("dd MMM yyyy, hh:mm tt");
        }
        else
        {
            lastLogin = "This is your first login";
        }

        litWelcomeName.Text = System.Web.HttpUtility.HtmlEncode(fullName);
        litFullName.Text = System.Web.HttpUtility.HtmlEncode(fullName);
        litStudentId.Text = row["StudentID"].ToString();
        photoPreview.Src = ResolvePhotoUrl(photoPath);

        litViewStudentId.Text = row["StudentID"].ToString();
        litViewFullName.Text = System.Web.HttpUtility.HtmlEncode(fullName);
        litViewEmail.Text = System.Web.HttpUtility.HtmlEncode(row["Email"].ToString());
        litViewMobile.Text = System.Web.HttpUtility.HtmlEncode(row["MobileNumber"].ToString());
        litViewCountry.Text = System.Web.HttpUtility.HtmlEncode(countryName);
        litViewState.Text = System.Web.HttpUtility.HtmlEncode(string.IsNullOrEmpty(stateName) ? "-" : stateName);
        litViewDistrict.Text = System.Web.HttpUtility.HtmlEncode(string.IsNullOrEmpty(districtName) ? "-" : districtName);
        litViewGender.Text = System.Web.HttpUtility.HtmlEncode(row["Gender"].ToString());
        litViewDob.Text = dob;
        litViewRegDate.Text = regDate;
        litViewLastLogin.Text = lastLogin;
        litViewAddress.Text = System.Web.HttpUtility.HtmlEncode(row["Address"] == DBNull.Value ? "-" : row["Address"].ToString());
    }

    private string ResolvePhotoUrl(string storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return ResolveUrl("~/Uploads/Students/default-avatar.png");
        return storedPath.StartsWith("~") ? ResolveUrl(storedPath) : storedPath;
    }

    private string GetCountryNameById(object countryIdObj)
    {
        if (countryIdObj == null || countryIdObj == DBNull.Value) return "-";
        object result = DBHelper.ExecuteScalar(
            "SELECT CountryName FROM Countries WHERE CountryID = @CountryId",
            new SqlParameter("@CountryId", countryIdObj));
        return result == null ? "-" : result.ToString();
    }

    /* =========================================================
       Edit mode toggle
       ========================================================= */
    protected void btnEdit_Click(object sender, EventArgs e)
    {
        EnterEditMode();
    }

    /// <summary>
    /// "Change Password" reuses Edit Profile, since this system authenticates
    /// with the Mobile Number (not a separate password field/column). Updating
    /// Mobile Number via Save Changes IS the password change. We just surface
    /// a note and focus the Mobile field (handled client-side via #changepw).
    /// </summary>
    protected void btnChangePassword_Click(object sender, EventArgs e)
    {
        EnterEditMode();
        lblEditModeNote.Visible = true;
    }

    private void EnterEditMode()
    {
        string studentId = Session["StudentID"].ToString();

        DataTable dt = DBHelper.ExecuteQuery(
            @"SELECT FullName, Email, MobileNumber, CountryID, StateName, DistrictName, Address, ProfilePhotoPath
              FROM Students WHERE StudentID = @StudentID",
            new SqlParameter("@StudentID", studentId));

        if (dt.Rows.Count == 0) return;
        DataRow row = dt.Rows[0];

        txtEditStudentId.Text = studentId;
        txtEditEmail.Text = row["Email"].ToString();
        txtEditFullName.Text = row["FullName"].ToString();
        txtEditMobileDisplay.Text = row["MobileNumber"].ToString();
        hdnEditFullMobile.Value = row["MobileNumber"].ToString();
        txtEditAddress.Text = row["Address"] == DBNull.Value ? "" : row["Address"].ToString();
        editPhotoPreview.Src = ResolvePhotoUrl(row["ProfilePhotoPath"] == DBNull.Value ? "" : row["ProfilePhotoPath"].ToString());

        // Pre-select the current country.
        string currentCountryName = GetCountryNameById(row["CountryID"]);
        if (ddlEditCountry.Items.FindByText(currentCountryName) != null)
        {
            ddlEditCountry.ClearSelection();
            ddlEditCountry.Items.FindByText(currentCountryName).Selected = true;
        }

        // Pre-populate State/District from the live API so the current
        // values show up as selected, and the rest are ready for editing.
        string currentStateName = row["StateName"] == DBNull.Value ? "" : row["StateName"].ToString();
        string currentDistrictName = row["DistrictName"] == DBNull.Value ? "" : row["DistrictName"].ToString();

        ddlEditState.Items.Clear();
        ddlEditState.Items.Add(new ListItem("Select State", ""));
        ddlEditDistrict.Items.Clear();
        ddlEditDistrict.Items.Add(new ListItem("Select District", ""));

        if (!string.IsNullOrEmpty(currentCountryName))
        {
            var states = FetchStatesFromApi(currentCountryName);
            foreach (var s in states)
            {
                ddlEditState.Items.Add(new ListItem(s.Name, s.Id));
            }
            if (!string.IsNullOrEmpty(currentStateName) && ddlEditState.Items.FindByValue(currentStateName) != null)
            {
                ddlEditState.ClearSelection();
                ddlEditState.Items.FindByValue(currentStateName).Selected = true;

                var districts = FetchDistrictsFromApi(currentCountryName, currentStateName);
                foreach (var d in districts)
                {
                    ddlEditDistrict.Items.Add(new ListItem(d.Name, d.Id));
                }
                if (!string.IsNullOrEmpty(currentDistrictName) && ddlEditDistrict.Items.FindByValue(currentDistrictName) != null)
                {
                    ddlEditDistrict.ClearSelection();
                    ddlEditDistrict.Items.FindByValue(currentDistrictName).Selected = true;
                }
            }
        }

        pnlView.Visible = false;
        pnlEdit.Visible = true;
    }

    protected void btnCancelEdit_Click(object sender, EventArgs e)
    {
        pnlEdit.Visible = false;
        pnlView.Visible = true;
        lblEditModeNote.Visible = false;
        LoadProfile();
    }

    /* =========================================================
       Save profile changes
       ========================================================= */
    protected void btnSaveProfile_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string studentId = Session["StudentID"].ToString();

        // Read State/District directly from the posted form values, since
        // those options were added by JavaScript and won't necessarily be
        // present in the server-rendered Items collection.
        string postedCountryText = Request.Form[ddlEditCountry.UniqueID];
        string postedStateName = Request.Form[ddlEditState.UniqueID];
        string postedDistrictName = Request.Form[ddlEditDistrict.UniqueID];

        if (string.IsNullOrWhiteSpace(postedCountryText) ||
            string.IsNullOrWhiteSpace(postedStateName) ||
            string.IsNullOrWhiteSpace(postedDistrictName))
        {
            ShowUpdateStatus("Please select Country, State and District.", false);
            return;
        }

        object countryIdObj = DBHelper.ExecuteScalar(
            "SELECT CountryID FROM Countries WHERE CountryName = @CountryName",
            new SqlParameter("@CountryName", postedCountryText));

        if (countryIdObj == null)
        {
            ShowUpdateStatus("Invalid country selected.", false);
            return;
        }

        // ---- Handle profile photo upload (optional -- only if a new file was chosen) ----
        string newPhotoPath = null;
        if (fuEditProfilePhoto.HasFile)
        {
            string ext = Path.GetExtension(fuEditProfilePhoto.FileName).ToLower();
            string[] allowed = { ".jpg", ".jpeg", ".png" };

            if (Array.IndexOf(allowed, ext) < 0)
            {
                lblEditPhotoError.Text = "Only JPG, JPEG and PNG files are allowed.";
                return;
            }

            double maxSizeMb = double.Parse(ConfigurationManager.AppSettings["MaxUploadSizeMB"]);
            if (fuEditProfilePhoto.PostedFile.ContentLength > maxSizeMb * 1024 * 1024)
            {
                lblEditPhotoError.Text = "Photo must be smaller than " + maxSizeMb + " MB.";
                return;
            }

            string uploadFolder = ConfigurationManager.AppSettings["ProfilePhotoUploadPath"];
            string physicalFolder = Server.MapPath(uploadFolder);
            if (!Directory.Exists(physicalFolder)) Directory.CreateDirectory(physicalFolder);

            string uniqueFileName = "STU_" + DateTime.Now.Ticks + ext;
            string physicalPath = Path.Combine(physicalFolder, uniqueFileName);
            fuEditProfilePhoto.SaveAs(physicalPath);

            newPhotoPath = uploadFolder.TrimEnd('/') + "/" + uniqueFileName;
        }

        string sql = newPhotoPath == null
            ? @"UPDATE Students SET FullName=@FullName, MobileNumber=@Mobile, CountryID=@CountryID,
                   StateName=@StateName, DistrictName=@DistrictName, Address=@Address
               WHERE StudentID=@StudentID"
            : @"UPDATE Students SET FullName=@FullName, MobileNumber=@Mobile, CountryID=@CountryID,
                   StateName=@StateName, DistrictName=@DistrictName, Address=@Address, ProfilePhotoPath=@Photo
               WHERE StudentID=@StudentID";

        string mobileToSave = string.IsNullOrWhiteSpace(hdnEditFullMobile.Value)
            ? txtEditMobileDisplay.Text.Trim()
            : hdnEditFullMobile.Value;

        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@FullName", txtEditFullName.Text.Trim()),
            new SqlParameter("@Mobile", mobileToSave),
            new SqlParameter("@CountryID", countryIdObj),
            new SqlParameter("@StateName", postedStateName),
            new SqlParameter("@DistrictName", postedDistrictName),
            new SqlParameter("@Address", (object)txtEditAddress.Text.Trim() ?? DBNull.Value),
            new SqlParameter("@StudentID", studentId)
        };
        if (newPhotoPath != null)
        {
            parameters.Add(new SqlParameter("@Photo", newPhotoPath));
        }

        DBHelper.ExecuteNonQuery(sql, parameters.ToArray());

        Session["StudentName"] = txtEditFullName.Text.Trim();

        pnlEdit.Visible = false;
        pnlView.Visible = true;
        lblEditModeNote.Visible = false;
        LoadProfile();
        ShowUpdateStatus("Profile updated successfully.", true);
    }

    private void ShowUpdateStatus(string message, bool success)
    {
        string icon = success ? "✅ " : "⚠️ ";
        lblUpdateStatus.Text = icon + message;
        lblUpdateStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblUpdateStatus.Visible = true;
    }

    protected void cvEditMobile_ServerValidate(object source, ServerValidateEventArgs args)
    {
        args.IsValid = !string.IsNullOrWhiteSpace(hdnEditFullMobile.Value)
            || !string.IsNullOrWhiteSpace(txtEditMobileDisplay.Text);
    }

    /* =========================================================
       Logout
       ========================================================= */
    protected void btnLogout_Click(object sender, EventArgs e)
    {
        Session.Clear();
        Session.Abandon();
        Response.Redirect("Login.aspx");
    }

    /* =========================================================
       Country dropdown (local DB -- same list used on Register.aspx)
       ========================================================= */
    private void BindEditCountryDropdown()
    {
        DataTable dt = DBHelper.ExecuteQuery("SELECT CountryID, CountryName FROM Countries ORDER BY CountryName");
        ddlEditCountry.DataTextField = "CountryName";
        ddlEditCountry.DataValueField = "CountryName";
        ddlEditCountry.DataSource = dt;
        ddlEditCountry.DataBind();
        ddlEditCountry.Items.Insert(0, new ListItem("Select Country", ""));
    }

    /* =========================================================
       WebMethods used by cascading-dropdown.js via PageMethods.
       Same CountriesNow-backed approach as Register.aspx.cs.
       ========================================================= */
    [WebMethod]
    public static List<DropdownItem> GetStates(string countryName)
    {
        return FetchStatesFromApi(countryName);
    }

    [WebMethod]
    public static List<DropdownItem> GetDistricts(string countryName, string stateName)
    {
        return FetchDistrictsFromApi(countryName, stateName);
    }

    private static List<DropdownItem> FetchStatesFromApi(string countryName)
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
        catch { /* API unreachable or bad response -- list stays empty */ }

        return list;
    }

    private static List<DropdownItem> FetchDistrictsFromApi(string countryName, string stateName)
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
        catch { /* API unreachable or bad response -- list stays empty */ }

        return list;
    }
}
