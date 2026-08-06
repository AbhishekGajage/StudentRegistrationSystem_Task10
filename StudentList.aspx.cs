using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class StudentList : Page
{
    private const string SortExprKey = "StudentList_SortExpr";
    private const string SortDirKey = "StudentList_SortDir";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ViewState[SortExprKey] = "RegistrationDate";
            ViewState[SortDirKey] = "DESC";
            BindStudentGrid();
        }
    }

    // Builds and runs the filtered/sorted query. All filter values are optional —
    // an empty filter means "don't restrict on this field".
    private DataTable GetFilteredStudents()
    {
        string name = txtSearchName.Text.Trim();
        string email = txtSearchEmail.Text.Trim();
        string mobile = txtSearchMobile.Text.Trim();
        string gender = ddlFilterGender.SelectedValue;

        string sortExpr = ViewState[SortExprKey] as string ?? "RegistrationDate";
        string sortDir = ViewState[SortDirKey] as string ?? "DESC";

        // Whitelist sortable columns to avoid building ORDER BY from raw user/GridView input.
        switch (sortExpr)
        {
            case "FullName":
            case "RegistrationDate":
            case "Email":
                break;
            default:
                sortExpr = "RegistrationDate";
                break;
        }
        sortDir = (sortDir == "ASC") ? "ASC" : "DESC";

        StringBuilder sql = new StringBuilder(
            @"SELECT s.StudentID, s.FullName, s.Email, s.MobileNumber, s.Gender,
                     c.CountryName, s.StateName, s.DistrictName,
                     s.Course, s.Semester, s.RegistrationDate, s.ProfilePhotoPath
              FROM Students s
              INNER JOIN Countries c ON s.CountryID = c.CountryID
              WHERE 1 = 1 ");

        if (!string.IsNullOrEmpty(name)) sql.Append(" AND s.FullName LIKE @Name ");
        if (!string.IsNullOrEmpty(email)) sql.Append(" AND s.Email LIKE @Email ");
        if (!string.IsNullOrEmpty(mobile)) sql.Append(" AND s.MobileNumber LIKE @Mobile ");
        if (!string.IsNullOrEmpty(gender)) sql.Append(" AND s.Gender = @Gender ");

        sql.Append(" ORDER BY s.").Append(sortExpr).Append(" ").Append(sortDir);

        var parameters = new System.Collections.Generic.List<SqlParameter>();
        if (!string.IsNullOrEmpty(name)) parameters.Add(new SqlParameter("@Name", "%" + name + "%"));
        if (!string.IsNullOrEmpty(email)) parameters.Add(new SqlParameter("@Email", "%" + email + "%"));
        if (!string.IsNullOrEmpty(mobile)) parameters.Add(new SqlParameter("@Mobile", "%" + mobile + "%"));
        if (!string.IsNullOrEmpty(gender)) parameters.Add(new SqlParameter("@Gender", gender));

        return DBHelper.ExecuteQuery(sql.ToString(), parameters.ToArray());
    }

    private void BindStudentGrid()
    {
        DataTable dt = GetFilteredStudents();
        gvStudents.DataSource = dt;
        gvStudents.DataBind();
        lblTotalCount.Text = dt.Rows.Count + " student record(s) found";
        UpdateSortStatusMessage();
    }

    // Shows a plain-language banner ("Sorted by Name in ascending order.") above
    // the grid so users don't have to infer the active sort from a tiny header
    // arrow alone.
    private void UpdateSortStatusMessage()
    {
        string sortExpr = ViewState[SortExprKey] as string ?? "RegistrationDate";
        string sortDir = ViewState[SortDirKey] as string ?? "DESC";

        string columnLabel;
        switch (sortExpr)
        {
            case "FullName": columnLabel = "Name"; break;
            case "Email": columnLabel = "Email"; break;
            case "RegistrationDate": columnLabel = "Registration Date"; break;
            default: columnLabel = sortExpr; break;
        }

        string directionLabel = (sortDir == "ASC") ? "ascending" : "descending";

        lblSortStatus.Text = "Sorted by <strong>" + columnLabel + "</strong> in " + directionLabel + " order.";
        lblSortStatus.Visible = true;
    }

    // Appends a small ▲/▼ next to whichever sortable header is currently active,
    // so the direction is visible at a glance without reading the banner text.
    protected void gvStudents_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.Header) return;

        string sortExpr = ViewState[SortExprKey] as string ?? "RegistrationDate";
        string sortDir = ViewState[SortDirKey] as string ?? "DESC";

        // Column index matches declaration order in the markup: 0=Photo, 1=StudentID,
        // 2=FullName, 3=Email, ... 11=RegistrationDate.
        AddSortArrow(e.Row.Cells[2], "FullName", sortExpr, sortDir);
        AddSortArrow(e.Row.Cells[3], "Email", sortExpr, sortDir);
        AddSortArrow(e.Row.Cells[11], "RegistrationDate", sortExpr, sortDir);
    }

    private void AddSortArrow(TableCell cell, string columnSortExpr, string currentExpr, string currentDir)
    {
        if (columnSortExpr != currentExpr) return;

        string arrow = (currentDir == "ASC") ? "▲" : "▼";
        cell.Controls.Add(new LiteralControl("<span class='sort-arrow'>" + arrow + "</span>"));
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindStudentGrid();
    }

    protected void btnResetFilters_Click(object sender, EventArgs e)
    {
        txtSearchName.Text = "";
        txtSearchEmail.Text = "";
        txtSearchMobile.Text = "";
        ddlFilterGender.SelectedIndex = 0;
        ViewState[SortExprKey] = "RegistrationDate";
        ViewState[SortDirKey] = "DESC";
        BindStudentGrid();
    }

    protected void btnRefresh_Click(object sender, EventArgs e)
    {
        BindStudentGrid();
    }

    // Clicking a sortable column header toggles ASC/DESC if it's already the active sort,
    // otherwise switches to that column ascending.
    protected void gvStudents_Sorting(object sender, GridViewSortEventArgs e)
    {
        string currentExpr = ViewState[SortExprKey] as string;
        string currentDir = ViewState[SortDirKey] as string;

        if (currentExpr == e.SortExpression)
        {
            ViewState[SortDirKey] = (currentDir == "ASC") ? "DESC" : "ASC";
        }
        else
        {
            ViewState[SortExprKey] = e.SortExpression;
            ViewState[SortDirKey] = "ASC";
        }

        BindStudentGrid();
    }

    /// <summary>
    /// Exports the currently filtered/sorted student list to an Excel-compatible .xls file.
    /// Uses a lightweight HTML-table-with-Excel-mimetype approach so no
    /// external NuGet package (EPPlus/ClosedXML) is required to run this
    /// out of the box. Swap in EPPlus for true .xlsx if you have it installed.
    /// </summary>
    protected void btnExportExcel_Click(object sender, EventArgs e)
    {
        DataTable dt = GetFilteredStudents();

        Response.Clear();
        Response.Buffer = true;
        Response.AddHeader("content-disposition",
            "attachment;filename=StudentRecords_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
        Response.Charset = "";
        Response.ContentType = "application/vnd.ms-excel";

        StringBuilder sb = new StringBuilder();

        sb.Append("<html><head><style>");
        sb.Append("table { border-collapse: collapse; font-family: Arial, sans-serif; font-size: 11pt; }");
        sb.Append("th { background:#2b5fd9; color:#fff; padding:6px 10px; border:1px solid #999; text-align:left; }");
        sb.Append("td { padding:5px 10px; border:1px solid #ccc; }");
        sb.Append("</style></head><body>");
        sb.Append("<table>");

        sb.Append("<tr>");
        string[] headers = {
            "Student ID", "Full Name", "Email", "Mobile Number", "Gender", "Country",
            "State", "District", "Course", "Semester", "Registration Date"
        };
        foreach (string h in headers) sb.Append("<th>" + h + "</th>");
        sb.Append("</tr>");

        foreach (DataRow row in dt.Rows)
        {
            sb.Append("<tr>");
            sb.Append("<td>" + row["StudentID"] + "</td>");
            sb.Append("<td>" + row["FullName"] + "</td>");
            sb.Append("<td>" + row["Email"] + "</td>");
            sb.Append("<td>" + row["MobileNumber"] + "</td>");
            sb.Append("<td>" + row["Gender"] + "</td>");
            sb.Append("<td>" + row["CountryName"] + "</td>");
            sb.Append("<td>" + row["StateName"] + "</td>");
            sb.Append("<td>" + row["DistrictName"] + "</td>");
            sb.Append("<td>" + row["Course"] + "</td>");
            sb.Append("<td>" + row["Semester"] + "</td>");
            sb.Append("<td>" + Convert.ToDateTime(row["RegistrationDate"]).ToString("dd-MMM-yyyy hh:mm tt") + "</td>");
            sb.Append("</tr>");
        }

        sb.Append("</table></body></html>");

        Response.Output.Write(sb.ToString());
        Response.Flush();
        Response.End();
    }
}
