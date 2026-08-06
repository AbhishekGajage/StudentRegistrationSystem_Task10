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

public partial class BulkRegister : Page
{
    // ViewState key for the temp batch table
    private const string TempTableKey = "BulkRegister_TempTable";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindCountryDropdown();
            InitTempTable();
            BindGrid();
        }
        txtDob.Attributes["max"] = DateTime.Today.ToString("yyyy-MM-dd");
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        // ddlState/ddlDistrict are populated client-side via PageMethods and never
        // exist in the server's Items collection, so on postback ASP.NET's match
        // against Items fails and SelectedValue comes back empty. This rebuilds the
        // same options server-side (using the posted Country/State) early enough
        // for that match to succeed -- identical fix to the one on Register.aspx.
        if (IsPostBack)
        {
            RebindDynamicDropdownsForPostback();
        }
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

    /* =========================================================
       WebMethods used by cascading-dropdown.js via PageMethods.
       Must be public static and decorated with [WebMethod].
       PageMethods only expose methods declared on the current
       page's code-behind, so these mirror Register.aspx.cs's
       versions rather than calling into them directly.
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
            // Fail quiet -- dropdown just stays empty; Add Record validation catches it.
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
       Temp (pending) batch storage
       ========================================================= */
    private DataTable InitTempTable()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("FullName", typeof(string));
        dt.Columns.Add("Email", typeof(string));
        dt.Columns.Add("Mobile", typeof(string));
        dt.Columns.Add("Gender", typeof(string));
        dt.Columns.Add("Dob", typeof(DateTime));
        dt.Columns.Add("Address", typeof(string));
        dt.Columns.Add("CountryID", typeof(int));
        dt.Columns.Add("CountryName", typeof(string));
        dt.Columns.Add("State", typeof(string));
        dt.Columns.Add("District", typeof(string));
        dt.Columns.Add("Course", typeof(string));
        dt.Columns.Add("Semester", typeof(string));
        dt.Columns.Add("PhotoPath", typeof(string));
        ViewState[TempTableKey] = dt;
        return dt;
    }

    private DataTable GetTempTable()
    {
        DataTable dt = ViewState[TempTableKey] as DataTable;
        if (dt == null)
        {
            dt = InitTempTable();
        }
        return dt;
    }

    private void BindGrid()
    {
        DataTable dt = GetTempTable();
        gvTemp.DataSource = dt;
        gvTemp.DataBind();
        lblRecordCount.Text = dt.Rows.Count + " record(s) pending";
    }

    private void ShowAddError(string message)
    {
        lblAddError.Text = message;
        lblAddError.CssClass = "status-message status-error";
        lblAddError.Visible = true;
    }

    private void ShowStatus(string message, bool success)
    {
        lblStatus.Text = message;
        lblStatus.CssClass = "status-message " + (success ? "status-success" : "status-error");
        lblStatus.Visible = true;
    }

    // "Add Record" -> validates (mirrors Register.aspx's rules), does NOT touch the
    // database, and appends a row to the in-memory pending table.
    protected void btnAdd_Click(object sender, EventArgs e)
    {
        lblAddError.Visible = false;

        if (!Page.IsValid)
        {
            return;
        }

        // Country/State/District are validated here directly from Request.Form
        // rather than via RequiredFieldValidator/SelectedValue. ddlState/ddlDistrict
        // are populated client-side and are never reliably present in the server's
        // Items collection on postback -- the Page_Init rebind helps them *display*
        // correctly but SelectedValue itself still can't be trusted for logic
        // (same root cause and same fix as Dashboard.aspx).
        string postedCountryId = Request.Form["ddlCountry"];
        string postedStateName = Request.Form["ddlState"];
        string postedDistrictName = Request.Form["ddlDistrict"];

        int countryId;
        string countryName = null;
        if (!string.IsNullOrEmpty(postedCountryId) && int.TryParse(postedCountryId, out countryId))
        {
            countryName = DBHelper.ExecuteScalar(
                "SELECT CountryName FROM Countries WHERE CountryID = @CountryId",
                new SqlParameter("@CountryId", countryId))?.ToString();
        }

        if (string.IsNullOrEmpty(postedCountryId) || string.IsNullOrEmpty(countryName)
            || string.IsNullOrEmpty(postedStateName) || string.IsNullOrEmpty(postedDistrictName))
        {
            ShowAddError("Please select Country, State and District.");
            return;
        }

        string fullName = txtFullName.Text.Trim();
        string email = txtEmail.Text.Trim();

        DateTime dob;
        if (!DateTime.TryParse(txtDob.Text, out dob))
        {
            ShowAddError("Invalid Date of Birth.");
            return;
        }

        string mobile = string.IsNullOrWhiteSpace(hdnFullMobile.Value) ? "" : hdnFullMobile.Value;
        if (string.IsNullOrWhiteSpace(mobile))
        {
            ShowAddError("Mobile number is required.");
            return;
        }

        DataTable dt = GetTempTable();

        // Prevent duplicate entries within the temporary (not-yet-saved) list
        foreach (DataRow row in dt.Rows)
        {
            if (string.Equals(row["Email"].ToString(), email, StringComparison.OrdinalIgnoreCase))
            {
                ShowAddError("This email is already in your pending list: " + email);
                return;
            }
            if (string.Equals(row["Mobile"].ToString(), mobile, StringComparison.OrdinalIgnoreCase))
            {
                ShowAddError("This mobile number is already in your pending list.");
                return;
            }
        }

        // Server-side duplicate check against the database too, so staff find out
        // before Save All rather than mid-batch.
        object emailExists = DBHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Students WHERE Email = @Email",
            new SqlParameter("@Email", email));
        if (Convert.ToInt32(emailExists) > 0)
        {
            ShowAddError("A student is already registered with this Email Address.");
            return;
        }

        object mobileExists = DBHelper.ExecuteScalar(
            "SELECT COUNT(1) FROM Students WHERE MobileNumber = @Mobile",
            new SqlParameter("@Mobile", mobile));
        if (Convert.ToInt32(mobileExists) > 0)
        {
            ShowAddError("A student is already registered with this Mobile Number.");
            return;
        }

        // ---- Profile photo: saved immediately since FileUpload content doesn't
        // survive across postbacks the way ViewState-backed fields do. ----
        string photoPath = ResolveUrl("~/Uploads/Students/default-avatar.png");
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

            photoPath = ResolveUrl(uploadFolder.TrimEnd('/') + "/" + uniqueFileName);
        }
        lblPhotoError.Text = "";

        DataRow newRow = dt.NewRow();
        newRow["FullName"] = fullName;
        newRow["Email"] = email;
        newRow["Mobile"] = mobile;
        newRow["Gender"] = rblGender.SelectedValue;
        newRow["Dob"] = dob;
        newRow["Address"] = txtAddress.Text.Trim();
        newRow["CountryID"] = int.Parse(postedCountryId);
        newRow["CountryName"] = countryName;
        newRow["State"] = postedStateName;
        newRow["District"] = postedDistrictName;
        newRow["Course"] = ddlCourse.SelectedValue;
        newRow["Semester"] = ddlSemester.SelectedValue;
        newRow["PhotoPath"] = photoPath;
        dt.Rows.Add(newRow);

        ViewState[TempTableKey] = dt;
        BindGrid();

        // Clear the form for the next entry
        txtFullName.Text = "";
        txtEmail.Text = "";
        hdnFullMobile.Value = "";
        txtDob.Text = "";
        txtAddress.Text = "";
        rblGender.SelectedIndex = 0;
        ddlCourse.SelectedIndex = 0;
        ddlSemester.SelectedIndex = 0;
        ddlCountry.ClearSelection();
        ddlState.Items.Clear();
        ddlState.Items.Add(new ListItem("Select State", ""));
        ddlState.Enabled = false;
        ddlDistrict.Items.Clear();
        ddlDistrict.Items.Add(new ListItem("Select District", ""));
        ddlDistrict.Enabled = false;
    }

    // "Remove Selected Record" -> removes only the checked rows from the pending list
    protected void btnRemoveSelected_Click(object sender, EventArgs e)
    {
        DataTable dt = GetTempTable();

        for (int i = gvTemp.Rows.Count - 1; i >= 0; i--)
        {
            CheckBox chk = gvTemp.Rows[i].FindControl("chkSelect") as CheckBox;
            if (chk != null && chk.Checked)
            {
                string email = gvTemp.DataKeys[i].Value.ToString();
                DataRow[] matches = dt.Select("Email = '" + email.Replace("'", "''") + "'");
                foreach (DataRow match in matches)
                {
                    dt.Rows.Remove(match);
                }
            }
        }

        ViewState[TempTableKey] = dt;
        BindGrid();
    }

    // "Clear All Records" -> wipes the entire pending batch
    protected void btnClearAll_Click(object sender, EventArgs e)
    {
        InitTempTable();
        BindGrid();
    }

    // "Save All" -> inserts every pending record into SQL Server.
    // Re-checks for duplicate email/mobile against the DATABASE before each insert,
    // since another user could have registered the same details in the meantime.
    protected void btnSaveAll_Click(object sender, EventArgs e)
    {
        DataTable dt = GetTempTable();

        if (dt.Rows.Count == 0)
        {
            ShowAddError("There are no pending records to save.");
            return;
        }

        int savedCount = 0;

        try
        {
            foreach (DataRow row in dt.Rows)
            {
                string email = row["Email"].ToString();
                string mobile = row["Mobile"].ToString();

                object emailExists = DBHelper.ExecuteScalar(
                    "SELECT COUNT(1) FROM Students WHERE Email = @Email",
                    new SqlParameter("@Email", email));

                if (Convert.ToInt32(emailExists) > 0)
                {
                    ShowAddError("Save All stopped: a student is already registered with this email — "
                        + email + ". " + savedCount + " record(s) were saved before this one. "
                        + "Remove the duplicate row and click Save All again for the rest.");
                    RemoveSavedRows(dt, savedCount);
                    ViewState[TempTableKey] = dt;
                    BindGrid();
                    return;
                }

                object mobileExists = DBHelper.ExecuteScalar(
                    "SELECT COUNT(1) FROM Students WHERE MobileNumber = @Mobile",
                    new SqlParameter("@Mobile", mobile));

                if (Convert.ToInt32(mobileExists) > 0)
                {
                    ShowAddError("Save All stopped: a student is already registered with this mobile number — "
                        + mobile + ". " + savedCount + " record(s) were saved before this one. "
                        + "Remove the duplicate row and click Save All again for the rest.");
                    RemoveSavedRows(dt, savedCount);
                    ViewState[TempTableKey] = dt;
                    BindGrid();
                    return;
                }

                string studentId = DBHelper.GetNextStudentId();

                DBHelper.ExecuteNonQuery(
                    @"INSERT INTO Students
                        (StudentID, FullName, Email, MobileNumber, CountryID, StateName, DistrictName,
                         Address, Gender, DateOfBirth, ProfilePhotoPath, Course, Semester,
                         RegistrationDate, IsEmailVerified, ApprovalStatus, AccountStatus,
                         CreatedDate, LastModifiedDate)
                      VALUES
                        (@StudentID, @FullName, @Email, @Mobile, @CountryID, @StateName, @DistrictName,
                         @Address, @Gender, @Dob, @Photo, @Course, @Semester, @RegDate, 0,
                         'Pending', 'Active', @RegDate, @RegDate)",
                    new SqlParameter("@StudentID", studentId),
                    new SqlParameter("@FullName", row["FullName"]),
                    new SqlParameter("@Email", row["Email"]),
                    new SqlParameter("@Mobile", row["Mobile"]),
                    new SqlParameter("@CountryID", row["CountryID"]),
                    new SqlParameter("@StateName", row["State"]),
                    new SqlParameter("@DistrictName", row["District"]),
                    new SqlParameter("@Address", row["Address"]),
                    new SqlParameter("@Gender", row["Gender"]),
                    new SqlParameter("@Dob", row["Dob"]),
                    new SqlParameter("@Photo", row["PhotoPath"]),
                    new SqlParameter("@Course", row["Course"]),
                    new SqlParameter("@Semester", row["Semester"]),
                    new SqlParameter("@RegDate", DateTime.Now));

                savedCount++;
            }

            InitTempTable();
            BindGrid();

            ShowStatus("✅ " + savedCount + " student record(s) saved successfully.", true);
        }
        catch (Exception ex)
        {
            RemoveSavedRows(dt, savedCount);
            ViewState[TempTableKey] = dt;
            BindGrid();
            ShowAddError("Save All failed after " + savedCount + " record(s). Remaining records are still pending. Error: " + ex.Message);
        }
    }

    // Removes the first N rows (the ones already successfully inserted) from the pending
    // batch, so a partial failure doesn't force staff to re-enter records that saved fine.
    private void RemoveSavedRows(DataTable dt, int count)
    {
        for (int i = 0; i < count && dt.Rows.Count > 0; i++)
        {
            dt.Rows.RemoveAt(0);
        }
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
}

// Note: DropdownItem is already declared as a public top-level class in
// Register.aspx.cs within this project/namespace, so it is not redeclared here.
